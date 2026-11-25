"""Safe reusable request gate for GitHub-hosted Eldoria Studio missions.
Only a newly added, immutable, explicitly authorized request can spend.
Read-only preparation validates native evidence; the API key is absent here.
"""
import argparse,datetime as dt,hashlib,json,os,pathlib,re,subprocess,sys,tempfile,unittest
from decimal import Decimal
from urllib.parse import quote
import budget_guard
import mission
import mission_packet
import studio
import routing

REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
PREFIX="tools/openai-development-studio/"
BRANCH="refs/heads/pilot/eldoria-openai-development-studio-20261125"
SHA=re.compile(r"[0-9a-f]{40}")
DIGEST=re.compile(r"[0-9a-f]{64}")
IDENT=re.compile(r"[a-z0-9][a-z0-9-]{2,79}")
SAFE_NAME=re.compile(r"[A-Za-z0-9_.-]{1,150}")
SOURCE_PREFIXES=("Unity/Assets/Experimentos/ValoriaModular25D/",
                 "Unity/Assets/Eldoria/")
def require(ok,message):
    if not ok:raise ValueError(message)
def run(*args,limit=2_000_000):
    p=subprocess.run(args,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE,timeout=180)
    require(len(p.stdout)<=limit,"Command result exceeded bounded size")
    return p.stdout
def git(*args):return run("git",*args).decode().strip()
def gh(path):return json.loads(run("gh","api","--method","GET",path))
def sha256(data):return hashlib.sha256(data).hexdigest()
def safe_relative(name):
    require(isinstance(name,str) and bool(name) and "\\" not in name and
            not name.startswith("/") and all(SAFE_NAME.fullmatch(x) and x not in (".","..") for x in name.split("/")),
            "Invalid local artifact filename")
    return name
def validate_request(d):
    require(d.get("schema")==1,"Unknown request schema")
    require(isinstance(d.get("id"),str) and IDENT.fullmatch(d["id"]),"Invalid request ID")
    require(d.get("mode") in ("review","art_candidate"),"Invalid mode")
    require(d.get("live") is True or d.get("live") is False,"Explicit live flag required")
    require(d.get("approved_public_game_evidence") is True,"Only approved public game evidence")
    for k in ("source_sha","evidence_head_sha"):
        require(isinstance(d.get(k),str) and SHA.fullmatch(d[k]),"Exact SHA required: "+k)
    require(isinstance(d.get("source_run"),int) and d["source_run"]>0,"Exact evidence run ID required")
    require(isinstance(d.get("artifact_name"),str) and SAFE_NAME.fullmatch(d["artifact_name"]),"Invalid artifact name")
    path=safe_relative(d["source_path"])
    require(path.startswith(SOURCE_PREFIXES) and path.endswith((".cs",".shader",".hlsl",".md")),"Source must be a bounded public Unity file")
    require(DIGEST.fullmatch(d["source_sha256"]),"Exact source content digest required")
    require(isinstance(d.get("frames"),list) and 1<=len(d["frames"])<=2,"One or two frame names required")
    for f in d["frames"]:
        require(safe_relative(f).endswith((".png",".jpg",".jpeg")),"Frame is not an image")
    require(len(set(d["frames"]))==len(d["frames"]),"Duplicate frame")
    require(safe_relative(d.get("manifest","")) in ("native-evidence.json","evidence.json"),"Unsupported native manifest")
    require(isinstance(d.get("target_id"),str) and re.fullmatch("[A-Z0-9_-]{3,48}",d["target_id"]),"Invalid target")
    for k in ("objective","scope","acceptance"):
        require(isinstance(d.get(k),str) and 1<=len(d[k])<=2000,"Invalid "+k)
    if d["mode"]=="art_candidate":
        require(isinstance(d.get("asset_brief"),str) and 1<=len(d["asset_brief"])<=1200,"Explicit asset brief required")
    slots=[r["role"] for r in routing.plan(d["mode"],d.get("routing_profile","standard"))]
    return sum((mission.RESERVES[s] for s in slots),Decimal(0))
def authorization(d,raw,previous,current,day,attempt):
    cap=validate_request(d)
    require(attempt==1,"Reruns never receive authorization")
    if not d["live"]:return False
    require(previous==current,"Request commit must not modify budget")
    budget_guard.authorize(previous,day,d["id"],d["id"],attempt,cap)
    rows=[x for x in previous["reservations"] if x["id"]==d["id"]]
    require(len(rows)==1 and rows[0].get("request_sha256")==sha256(raw),"Reservation not bound to exact request bytes")
    return True
def gate(event,attempt,head,ref):
    require(ref==BRANCH and attempt==1,"Only first attempt on Studio branch")
    before=event.get("before","")
    require(SHA.fullmatch(before) and SHA.fullmatch(head) and event.get("after")==head,"Expected direct push event")
    require(event.get("forced") is False and event.get("deleted") is False,"No forced or deleted ref")
    require(git("rev-parse","HEAD")==head and git("rev-parse","HEAD^")==before,"Expected single direct child commit")
    diff=git("diff","--name-status",before,head).splitlines()
    require(len(diff)==1,"Request commit must add one file only")
    parts=diff[0].split("\t")
    require(len(parts)==2 and parts[0]=="A" and re.fullmatch(re.escape(PREFIX)+"requests/[a-z0-9][a-z0-9-]{2,79}\\.json",parts[1]),"Only new request file permitted")
    path=parts[1]
    raw=pathlib.Path(path).read_bytes()
    require(0<len(raw)<=16000,"Request too large")
    d=json.loads(raw)
    require(path==PREFIX+"requests/"+d["id"]+".json","Request ID/path mismatch")
    # Historical deletion/re-add is not a new spending authorization.
    history=gh("repos/"+REPO+"/commits?sha="+before+"&path="+quote(path,safe="")+"&per_page=1")
    require(isinstance(history,list) and not history,"Request path has historical use")
    # A later push while preparation is running revokes this event's opportunity.
    live_head=gh("repos/"+REPO+"/git/ref/heads/"+BRANCH.removeprefix("refs/heads/"))
    require(live_head["object"]["sha"]==head,"Branch moved before request validation")
    old=json.loads(run("git","show",before+":"+PREFIX+"budget-cycle.json"))
    new=json.loads(pathlib.Path(PREFIX+"budget-cycle.json").read_text())
    live=authorization(d,raw,old,new,dt.datetime.now(dt.timezone.utc).date(),attempt)
    return d,live
def materialize(d,out):
    require(not out.exists(),"Never overwrite mission packet")
    out.mkdir(parents=True)
    # Re-open exact public source via GitHub's immutable contents endpoint.
    source=gh("repos/"+REPO+"/contents/"+quote(d["source_path"],safe="/")+"?ref="+d["source_sha"])
    require(source["type"]=="file" and source["encoding"]=="base64" and 0<source["size"]<=12000,"Invalid public source")
    import base64
    data=base64.b64decode(source["content"])
    require(sha256(data)==d["source_sha256"],"Source SHA256 mismatch")
    require(not any(s in data for s in (b"sk-proj-",b"ghp_",b"github_pat_",b"-----BEGIN PRIVATE KEY-----")),"Credential marker in source")
    path=out/"source.txt";path.write_bytes(data)
    r=gh("repos/"+REPO+"/actions/runs/"+str(d["source_run"]))
    require(r["head_sha"]==d["evidence_head_sha"] and r["status"]=="completed" and r["conclusion"]=="success","Native evidence run not certified")
    a=gh("repos/"+REPO+"/actions/runs/"+str(d["source_run"])+"/artifacts?per_page=100")
    matches=[x for x in a["artifacts"] if x["name"]==d["artifact_name"] and not x["expired"]]
    require(len(matches)==1 and 0<matches[0]["size_in_bytes"]<=150*1024*1024,"Missing or oversized evidence artifact")
    with tempfile.TemporaryDirectory() as tmp:
        root=pathlib.Path(tmp).resolve()
        run("gh","run","download",str(d["source_run"]),"--repo",REPO,"--name",d["artifact_name"],"--dir",str(root))
        manifest=list(root.rglob(d["manifest"]))
        require(len(manifest)==1 and root in manifest[0].resolve().parents,"Missing native manifest")
        m=json.loads(studio.read_limited(manifest[0],2*1024*1024))
        require(m.get("sourceSha")==d["source_sha"] and m.get("engine")=="Unity" and m.get("nativeWorldRender") is True,"Native source provenance mismatch")
        refs=[]
        for i,name in enumerate(d["frames"]):
            f=manifest[0].parent/safe_relative(name)
            require(root in f.resolve().parents and f.is_file(),"Frame outside artifact or absent")
            records=[x for x in m["frames"] if x["file"]==name]
            require(len(records)==1 and records[0].get("sha256"),"Frame absent from native evidence")
            raw=studio.read_limited(f,studio.MAX_IMAGE_BYTES)
            require(sha256(raw)==records[0]["sha256"],"Native frame hash mismatch")
            dest=out/("frame-"+str(i)+f.suffix.lower());dest.write_bytes(raw)
            refs.append({"path":dest.name,"sha256":sha256(raw)})
    packet={k:d[k] for k in ("schema","target_id","source_sha","source_run","objective","scope","acceptance","mode")}
    if "asset_brief" in d:packet["asset_brief"]=d["asset_brief"]
    packet["routing_profile"]=d.get("routing_profile","standard")
    packet.update(source={"path":path.name,"sha256":sha256(data)},images=refs)
    p=out/"packet.json";p.write_text(json.dumps(packet,ensure_ascii=False,indent=2),encoding="utf-8")
    mission_packet.execute(p,None,False)
    return p

class Tests(unittest.TestCase):
    def sample(self):
        return {"schema":1,"id":"test-case","live":True,"approved_public_game_evidence":True,"mode":"review","target_id":"MILL_YARD",
                "source_sha":"a"*40,"evidence_head_sha":"b"*40,"source_run":123,"artifact_name":"native",
                "source_path":"Unity/Assets/Experimentos/ValoriaModular25D/Resources/Ground.shader","source_sha256":"c"*64,
                "manifest":"native-evidence.json","frames":["b5-A-mill.png","b5-B-mill.png"],
                "objective":"Compare","scope":"Read only","acceptance":"No fake claims"}
    def reserve(self,d,raw):
        return {"cycle":{"starts":"2026-11-25","ends_exclusive":"2026-12-17","pilot_cap_usd":"5.00","automatic_reset":False},
                "reservations":[{"id":d["id"],"run_id":d["id"],"status":"authorized_once","reserve_usd":"0.70","request_sha256":sha256(raw)}]}
    def test_review(self):self.assertEqual(validate_request(self.sample()),Decimal("0.70"))
    def test_dry(self):
        d=self.sample();d["live"]=False
        self.assertFalse(authorization(d,b"",{}, {},dt.date(2026,11,25),1))
    def test_bound(self):
        d=self.sample();raw=json.dumps(d).encode();budget=self.reserve(d,raw)
        self.assertTrue(authorization(d,raw,budget,budget,dt.date(2026,11,25),1))
        with self.assertRaises(ValueError):authorization(d,raw+b" ",budget,budget,dt.date(2026,11,25),1)
        with self.assertRaises(ValueError):authorization(d,raw,budget,budget,dt.date(2026,11,25),2)
    def test_paths(self):
        for path in ("../secret","Unity/Assets/Eldoria/../../secret.cs","/a.cs","Unity/Assets/Eldoria/x\\y.cs"):
            d=self.sample();d["source_path"]=path
            with self.assertRaises(ValueError):validate_request(d)
    def test_approval(self):
        d=self.sample();d["approved_public_game_evidence"]=False
        with self.assertRaises(ValueError):validate_request(d)
    def test_closed(self):
        d=self.sample();raw=json.dumps(d).encode();budget=self.reserve(d,raw);budget["reservations"][0]["status"]="executed"
        with self.assertRaises(ValueError):authorization(d,raw,budget,budget,dt.date(2026,11,25),1)

if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]:unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=argparse.ArgumentParser();p.add_argument("--out",required=True);a=p.parse_args()
        ev=json.loads(pathlib.Path(os.environ["GITHUB_EVENT_PATH"]).read_text())
        d,live=gate(ev,int(os.environ["GITHUB_RUN_ATTEMPT"]),os.environ["GITHUB_SHA"],os.environ["GITHUB_REF"])
        materialize(d,pathlib.Path(a.out))
        with open(os.environ["GITHUB_OUTPUT"],"a") as f:
            f.write("live="+str(live).lower()+"\nrequest_id="+d["id"]+"\n")
        print("STUDIO_REQUEST_GATE_PASS",d["id"],"live="+str(live).lower())
