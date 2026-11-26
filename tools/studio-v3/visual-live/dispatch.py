"""Authenticated one-shot visual benchmark dispatch. No model-generated code runs.
Read-only artifact preparation is separately exercised by the offline workflow.
The only paid entry point is the immutable authorized push checked below.
"""
import base64,datetime as dt,hashlib,json,os,pathlib,re,stat,subprocess,sys,zipfile
from urllib.parse import urlencode
import session as s
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
OWNER="chatgpt-studio-v3-visual-live-20261126"
WORKSTREAM="eldoria-studio-v3-autonomous-execution-20261126"
BRANCH=s.BRANCH.removeprefix("refs/heads/")
ROOT="tools/studio-v3/visual-live/"
PLAN=ROOT+"authorized-plan.json"
BUDGET=ROOT+"authorized-budget.json"
REQUEST="tools/studio-v3/live-requests/senda-visual-paired-20261126.json"
MESSAGE="Authorize one isolated V3 Senda visual comparison within USD 2.00"
CAL_RUN=45892128146
CAL_SHA="884bfd5f5cbb9541620046fb88261ab3f945108b"
V2="1657b2caa6b1a955319538ce3a8b2906398599b3"
CAL_RECORD=ROOT+"evidence/calibration-verified-20261126.json"
ARTIFACTS={
 15152775209:("studio-v3-visual-blind-review","8d9d090c870a2fae235be245393479592166c3ea137c0f003a363617e9b9976e","blind-review"),
 15152810075:("studio-v3-visual-receipts","368b07e3634f88574c6cebd2bc4e729208b6f3fe2ab4227d6160197a577652a4","")
}
DAY=lambda:dt.datetime.now(dt.timezone.utc).date()
P=pathlib.Path
need=s.need
def run(*cmd,limit=5000000):
    p=subprocess.run(cmd,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE,timeout=90)
    need(len(p.stdout)<=limit,"Trusted command output too large")
    return p.stdout
def git(*args):return run("git",*args).decode().strip()
def gh(path):return json.loads(run("gh","api","--method","GET",path))
def load(p):return s.unique_json(P(p).read_text(encoding="utf-8"))
def certified_calibration():
    r=gh("repos/"+REPO+"/actions/runs/"+str(CAL_RUN))
    need(r["head_sha"]==CAL_SHA and r["status"]=="completed" and r["conclusion"]=="success" and r["name"]=="Studio V3 visual benchmark calibration","Calibration run not certified")
    items=gh("repos/"+REPO+"/actions/runs/"+str(CAL_RUN)+"/artifacts?per_page=100")["artifacts"]
    for aid,(name,digest,_) in ARTIFACTS.items():
        a=[x for x in items if x["id"]==aid]
        need(len(a)==1 and a[0]["name"]==name and a[0]["digest"]=="sha256:"+digest and a[0]["expired"] is False,"Calibration artifact missing or changed")
    return load(CAL_RECORD)
def extract_zip(raw,root):
    with zipfile.ZipFile(raw) as z:
        entries=z.infolist()
        need(len(entries)<=60 and sum(x.file_size for x in entries)<=50*1024*1024,"Unexpected archive size")
        seen=set()
        for x in entries:
            rel=pathlib.PurePosixPath(x.filename)
            need(not rel.is_absolute() and all(re.fullmatch(r"[A-Za-z0-9_.-]+",p) and p not in (".","..") for p in rel.parts),"Unsafe archive path")
            need(x.filename not in seen and not stat.S_ISLNK(x.external_attr>>16),"Duplicate or linked archive file")
            seen.add(x.filename)
            target=root.joinpath(*rel.parts)
            if x.is_dir():target.mkdir(parents=True,exist_ok=True);continue
            target.parent.mkdir(parents=True,exist_ok=True)
            with target.open("xb") as f:f.write(z.read(x))
def verify_evidence(root,record):
    root=P(root);blind=root/"blind-review";role=root/"role-inputs"
    report=load(root/"report.json");need(report==record["proof"],"Report differs from certified run")
    manifest=load(blind/"frames-manifest.json");need(len(manifest)==33,"Missing exact 33 frames")
    groups={};seen=set()
    for row in manifest:
        vp,phase,alias=(row[k] for k in ("viewport","phase","variant"))
        need(vp in ("phone","compact","horizontal") and phase in ("intro","gates","battle","victory","defeat") and alias in ("baseline","variant-72","variant-19"),"Unknown fixture")
        name=vp+"-"+phase+"-"+alias+".png"
        need(name not in seen and s.sha((blind/name).read_bytes())==row["sha256"],"Capture bytes mismatch")
        seen.add(name);groups.setdefault((vp,phase),[]).append(row["state_digest"])
    need(len(groups)==11 and all(len(v)==3 and len(set(v))==1 for v in groups.values()),"Fixture states differ")
    actual={"primary_sha256":s.sha((role/"production-baseline.jpg").read_bytes()),"secondary_sha256":s.sha((role/"production-reference.jpg").read_bytes()),"reference_text_sha256":s.sha((blind/"reference-art-direction.md").read_bytes()+(blind/"reference-shared-standard.md").read_bytes())}
    need(actual==record["inputs"]["production_evidence"] and actual==load(role/"role-input-proof.json")["production_evidence"],"Production evidence not exact")
    return actual
def prepare(root):
    root=P(root);root.mkdir(parents=False,exist_ok=False)
    record=certified_calibration()
    for aid,(_,digest,subdir) in ARTIFACTS.items():
        f=root/("artifact-"+str(aid)+".zip")
        with f.open("xb") as out:subprocess.run(["gh","api","--method","GET","repos/"+REPO+"/actions/artifacts/"+str(aid)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
        need(f.stat().st_size<=25*1024*1024 and s.sha(f.read_bytes())==digest,"Artifact ZIP bytes mismatch")
        extract_zip(f,root/subdir);f.unlink()
    e=verify_evidence(root,record)
    print("STUDIO_V3_VISUAL_CALIBRATION_INPUT "+json.dumps({"run":CAL_RUN,"frames":33,"matched":11,"evidence":e}),flush=True)
def load_plan():
    raw=P(PLAN).read_bytes();g=s.unique_json(raw.decode());b=load(BUDGET)
    need(g["calibration_run"]==CAL_RUN and g["production_evidence"]==load(CAL_RECORD)["inputs"]["production_evidence"],"Grant differs from calibrated evidence")
    return raw,g,b
def proof_for(raw):
    return {"branch":s.BRANCH,"run_attempt":1,"grant_sha256":s.sha(raw),"verified_unique_dispatch":True,"owner_authorized":True,"verified_calibration_run":CAL_RUN}
def validate_parent():
    raw,g,b=load_plan()
    s.authorize(raw,b,proof_for(raw),DAY())
    return raw,g,b
def preflight(cal):
    need(os.environ.get("GITHUB_REPOSITORY")==REPO and os.environ.get("GITHUB_REF")==s.BRANCH and os.environ.get("GITHUB_RUN_ATTEMPT")=="1" and os.environ.get("GITHUB_EVENT_NAME")=="push","Not first authorized push")
    event=load(os.environ["GITHUB_EVENT_PATH"]);head=os.environ["GITHUB_SHA"];before=event["before"]
    need(event["ref"]==s.BRANCH and event["after"]==head and event.get("forced") is False and event.get("deleted") is False,"Unexpected push event")
    need(git("rev-parse","HEAD")==head and git("rev-parse","HEAD^")==before and git("log","-1","--format=%s")==MESSAGE,"Wrong authorized commit")
    need(git("diff","--name-status",before,head)=="A\t"+REQUEST,"Only immutable new authorization permitted")
    raw,g,b=validate_parent()
    need(run("git","show",before+":"+PLAN)==raw and P(REQUEST).read_bytes()==raw and run("git","show",before+":"+BUDGET)==P(BUDGET).read_bytes(),"Request/budget bytes changed")
    offline=gh("repos/"+REPO+"/actions/workflows/studio-v3-offline.yml/runs?"+urlencode({"head_sha":before,"status":"success","per_page":20}))["workflow_runs"]
    certified=[x for x in offline if x["head_sha"]==before and x["head_branch"]==BRANCH and x["name"]=="Studio V3 offline execution proof" and x["conclusion"]=="success" and x["status"]=="completed"]
    need(len(certified)==1,"Missing exact preparation CI success")
    need(git("diff","--name-only",V2,"HEAD","--","tools/openai-development-studio")=="","Pinned V2 client changed")
    branch=gh("repos/"+REPO+"/git/ref/heads/"+BRANCH)
    need(branch["object"]["sha"]==head,"Branch changed; opportunity revoked")
    history=gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1}))
    need(isinstance(history,list) and not history,"Grant already historically dispatched")
    current=gh("repos/"+REPO+"/actions/runs/"+os.environ["GITHUB_RUN_ID"])
    need(current["head_sha"]==head and current["event"]=="push" and current["run_attempt"]==1 and current["name"]=="Studio V3 Senda visual live comparison","Wrong live run")
    meta=gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    blob=gh("repos/"+REPO+"/git/blobs/"+meta["sha"])
    registry=json.loads(base64.b64decode(blob["content"]))
    active=[x for x in registry["active"] if x["id"]==WORKSTREAM]
    need(len(active)==1,"Unique active scope required")
    w=active[0]
    need(w["owner"]==OWNER and w["ownership_revision"]==7 and w["ownership_state"]=="claimed" and w["branch"]==BRANCH and w["visual_benchmark"]["max_new_api_spend_usd"]=="2.00" and w["visual_benchmark"]["calibration_run"]==CAL_RUN,"Canonical ownership or grant changed")
    record=certified_calibration();need(verify_evidence(cal,record)==g["production_evidence"],"Calibrated input mismatch")
    p=proof_for(raw);s.authorize(raw,b,p,DAY())
    return raw,g,b,p,{"run_id":int(os.environ["GITHUB_RUN_ID"]),"head":head,"parent":before,"offline_run":certified[0]["id"],"registry_blob":meta["sha"],"calibration_run":CAL_RUN,"grant_sha256":s.sha(raw),"budget_sha256":s.sha(P(BUDGET).read_bytes())}
def paths(root,kind):
    root=P(root);r=root/"role-inputs";bl=root/"blind-review"
    names=("production-baseline.jpg","production-reference.jpg") if kind=="production" else ("judge-phone.jpg","judge-small-horizontal.jpg")
    return [r/n for n in names],[bl/"reference-art-direction.md",bl/"reference-shared-standard.md"]
def transport(cal,out):
    raw,g,b,p,proof=preflight(cal);out=P(out)
    key=os.environ.get("OPENAI_API_KEY");need(bool(key and key.strip()),"Missing authorized API key")
    with (out/"dispatch-proof.json").open("x") as f:json.dump(proof,f,indent=2)
    sess=s.Session(raw,b,p,DAY(),out/"ledger")
    judge_root=None
    for line in sys.stdin:
        try:
            need(len(line)<=100000,"Transport request oversized")
            m=s.unique_json(line)
            if m.get("action")=="register":
                need(s.exact(m,["action","round","evidence"]) and type(m["round"]) is int and m["round"] in (1,2),"Invalid judge registration")
                root=out/("render-round-"+str(m["round"]))/"evidence"
                report=load(root/"report.json")
                need(all(not x["issues"] and x["all_controls"] for x in report["variants"][1:]),"Technical candidate gate not passed")
                need(load(root/"role-inputs/role-input-proof.json")["judge_evidence"]==m["evidence"],"Judge evidence mismatch")
                sess.register_judge_evidence(m["evidence"],report);judge_root=root
                answer={"ok":True}
            else:
                need(s.exact(m,["action","slot","req"]) and m["action"]=="call" and m["slot"] in s.SLOTS,"Unknown command or slot")
                slot=m["slot"];req=m["req"];role=s.SLOTS[slot][0]
                need(role!="judge" or judge_root is not None,"Judge not registered")
                images,refs=paths(judge_root if role=="judge" else cal,"judge" if role=="judge" else "production")
                # Persist requests and actual successful responses independently of Node validation.
                with (out/("request-"+slot+".json")).open("x",encoding="utf-8") as f:json.dump(req,f,ensure_ascii=False,indent=2)
                response=sess.call(slot,req,images,refs,key)
                with (out/("response-"+slot+".json")).open("x",encoding="utf-8") as f:json.dump(response,f,ensure_ascii=False,indent=2)
                answer={"ok":True,"origin":"real_api","receipt_ref":g["id"]+":"+slot,"response":response}
            print(json.dumps(answer,ensure_ascii=False,separators=(",",":")),flush=True)
        except Exception as exc:
            # Never echo request/response bodies, signed URLs or secrets.
            print(json.dumps({"ok":False,"error":type(exc).__name__}),flush=True)
            return
if __name__=="__main__":
    mode=sys.argv[1]
    if mode=="--prepare" and len(sys.argv)==3:prepare(sys.argv[2])
    elif mode=="--validate-plan" and len(sys.argv)==2:
        raw,g,b=validate_parent();print("STUDIO_V3_VISUAL_PLAN "+json.dumps({"grant_sha256":s.sha(raw),"cap":g["max_total_usd"],"evidence":g["production_evidence"]}))
    elif mode=="--preflight" and len(sys.argv)==3:
        *_,proof=preflight(sys.argv[2]);print("STUDIO_V3_VISUAL_LIVE_PREFLIGHT_PASS "+json.dumps(proof))
    elif mode=="--transport" and len(sys.argv)==4:transport(sys.argv[2],sys.argv[3])
    else:raise SystemExit("Explicit bounded visual dispatch mode required")
