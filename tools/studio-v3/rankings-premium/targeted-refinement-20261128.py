"""Keep the earlier C composition; deterministic material/scale refinement and one independent audit."""
import base64,datetime as dt,json,os,re,shutil,subprocess,sys,zipfile
from decimal import Decimal as D
from pathlib import Path as P
from urllib.parse import urlencode
from PIL import Image,ImageDraw
HERE=P(__file__).resolve().parent
import run as v
from render import capture,SECTIONS
ID="pr198-targeted-material-and-scale-20261128"
OWNER="chatgpt-eldoria-rankings-art-continuation-20261128"
REQUEST="tools/studio-v3/rankings-premium/targeted-launch-20261128.json"
MESSAGE="Launch targeted PR198 material and scale audit 20261128"
ART=15279980769
DIGEST="9a2f639421096a6b40d2bcc2a557c52a5740b94e2d38e600a74f34a17b50c8be"
def gate():
    v.need(os.getenv("GITHUB_REPOSITORY")==v.REPO and os.getenv("GITHUB_REF")=="refs/heads/"+v.BRANCH and os.getenv("GITHUB_EVENT_NAME")=="push" and os.getenv("GITHUB_RUN_ATTEMPT")=="1","one-shot push")
    ev=v.read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=ev["before"]
    v.need(not ev.get("forced") and not ev.get("deleted") and ev["after"]==h and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"launch")
    v.need(v.git("diff","--name-status",before,h)=="A\t"+REQUEST,"only grant file")
    v.need(not v.gh("repos/"+v.REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1})),"already launched")
    v.need(v.gh("repos/"+v.REPO+"/git/ref/heads/"+v.BRANCH)["object"]["sha"]==h,"pilot moved")
    pr=v.gh("repos/"+v.REPO+"/pulls/198");v.need(pr["state"]=="open" and pr["head"]["sha"]==v.SOURCE,"PR moved")
    m=v.gh("repos/"+v.REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    reg=json.loads(base64.b64decode(v.gh("repos/"+v.REPO+"/git/blobs/"+m["sha"])["content"]))
    w=next(x for x in reg["active"] if x["id"]==v.ID);g=w["ranking_mission"]["targeted_refinement_20261128"]
    v.need(w["owner"]==OWNER and w["ownership_revision"]==17 and not w["resources"] and w["ownership_state"]=="claimed","claim moved")
    v.need(v.read(REQUEST)==g and g["source_sha"]==v.SOURCE and g["source_artifact"]==ART and g["id"]==ID,"grant")
    v.need(D(g["prior_worst_case_bound_usd"])==D("4.76061740") and D(g["new_reservation_usd"])==D(".15") and D(g["maximum_aggregate_usd"])==D("4.91061740")<=D(w["ranking_mission"]["cap_usd"])==D("5.00"),"cap")
    v.need(dt.datetime.now(dt.timezone.utc).date()<dt.date.fromisoformat(g["expiry"]),"expired")
    print("PR198_TARGETED_AUTHORIZED",flush=True)
def sheet(root,path):
    im=Image.new("RGB",(1170,1736),"#15131a");d=ImageDraw.Draw(im)
    for i,k in enumerate(SECTIONS):
        f=Image.open(root/"captures"/("phone-"+k+".png")).convert("RGB")
        v.need(f.size==(390,844),"wrong viewport")
        x,y=(i%3)*390,(i//3)*868;im.paste(f,(x,y+24));d.text((x+6,y+4),k,fill="white")
    im.save(path,quality=88,optimize=True)
def prepare(out):
    gate();out.mkdir()
    a=next(x for x in v.gh("repos/"+v.REPO+"/actions/runs/46185809727/artifacts")["artifacts"] if x["id"]==ART)
    v.need(not a["expired"] and a["digest"]=="sha256:"+DIGEST,"artifact identity")
    f=out/"source.zip"
    with f.open("xb") as stream:subprocess.run(["gh","api","repos/"+v.REPO+"/actions/artifacts/"+str(ART)+"/zip"],stdout=stream,stderr=subprocess.PIPE,check=True,timeout=120)
    v.need(v.sha(f.read_bytes())==DIGEST and f.stat().st_size<70*1024*1024,"artifact bytes")
    with zipfile.ZipFile(f) as z:
        data=json.loads(z.read("projection.json"));s=json.loads(z.read("summary.json"))
        v.need(data["sourceSha"]==v.SOURCE and data["fixture"] and not any(p["publicRankings"] for p in data["profiles"]) and s["browser_quality_gate"] is False,"wrong baseline")
        v.dump(out/"projection.json",data)
        for n in z.namelist():
            if n=="offline-replay/index.html" or re.fullmatch(r"offline-replay/assets/[a-zA-Z0-9_-]+\.(png|jpg|jpeg)",n):
                target=out/"baseline"/n.removeprefix("offline-replay/");target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
    f.unlink()
    base=out/"baseline";candidate=out/"candidate";shutil.copytree(base,candidate)
    patch=(HERE/"targeted-refinement-20261128.css").read_text()
    html=(base/"index.html").read_text()
    v.need(html.count("</style>")==1 and "</style>" not in patch and "<" not in patch,"patch format")
    (candidate/"index.html").write_text(html.replace("</style>",patch+"</style>",1))
    b=capture(base);c=capture(candidate)
    v.need(len(b)==len(c)==21,"Incomplete renders")
    sheet(base,out/"baseline.jpg");sheet(candidate,out/"candidate.jpg")
    v.dump(out/"prepared.json",{"source_sha":v.SOURCE,"source_artifact":ART,"patch_sha256":v.sha(patch.encode()),"baseline_frames":len(b),"candidate_frames":len(c),"data_parity":True,"native":False})
    print("PR198_TARGETED_RENDER_COMPLETE",flush=True)
def audit(out):
    gate();p=v.read(out/"prepared.json");v.need(p["source_sha"]==v.SOURCE and p["data_parity"],"render gate")
    v.SLOTS={"audit":D(".15")};l=v.Ledger(out)
    ev=out/"audit-input.txt";ev.write_text("Compare baseline then candidate, actual six-view browser renders on exact PR198 QA data. Prior C composition is preserved and candidate changes only scale, material and legibility. No native approval, no fabricated rankings. Score each screen without rewarding mere recolor.")
    payload,manifest=v.studio.make_payload("visual","gpt-6.1-terra",ev,(out/"baseline.jpg",out/"candidate.jpg"))
    payload["instructions"]="Independent mobile fantasy game art auditor. Evaluate actual six views. Return strict JSON {baseline:{Overview:number,Rankings:number,Trophies:number,Discoveries:number,Heraldry:number,Monuments:number},candidate:{same keys},findings:{same keys:string},decision:'revise'|'candidate'}. Weight resource quality 20%, integration 20%, coherence 15%, composition 15%, materials 10%, mobile legibility 10%, polish 10%. Note actual improvement or regressions, don't invent native proof. Only candidate if all >=8 and no regression."
    payload["max_output_tokens"]=3000
    for c in payload["input"][0]["content"]:
        if c["type"]=="input_image":c["detail"]="high"
    row=l.reserve("audit");raw=v.studio.request_api(payload,os.environ["OPENAI_API_KEY"]);l.finish(row,raw.get("usage"),raw.get("model") or "gpt-6.1-terra",raw.get("service_tier") or "standard")
    rep=v.studio.report("visual",payload,manifest,raw);v.dump(out/"audit-receipt.json",rep);a=v.json_result(rep["response"])
    v.need(set(a)=={"baseline","candidate","findings","decision"} and all(set(a[k])==set(SECTIONS) for k in ("baseline","candidate","findings")) and all(type(n) in (int,float) and 0<=n<=10 for k in ("baseline","candidate") for n in a[k].values()),"audit shape")
    good=[k for k in SECTIONS if a["candidate"][k]>a["baseline"][k]]
    result={"mission":ID,"audit":a,"improved_sections":good,"browser_quality_gate":a["decision"]=="candidate" and min(a["candidate"].values())>=8 and all(a["candidate"][k]>=a["baseline"][k] for k in SECTIONS),"estimated_usd":row["estimated_usd"],"billing_verified":False,"native_unity":False,"art_approved":False,"production_promoted":False}
    v.dump(out/"summary.json",result);print("PR198_TARGETED_RESULT",json.dumps(result,ensure_ascii=False),flush=True)
if __name__=="__main__":
    mode,out=sys.argv[1:];out=P(out)
    if mode=="--prepare":prepare(out)
    elif mode=="--audit":audit(out)
    else:raise ValueError("explicit mode")
