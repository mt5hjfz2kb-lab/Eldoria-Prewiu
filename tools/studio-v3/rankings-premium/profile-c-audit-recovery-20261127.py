"""Independent one-shot audit of C; immutable evidence, no image generation or game edits."""
import sys,os,json,zipfile,hashlib,urllib.request,io,subprocess
from pathlib import Path as P
from decimal import Decimal as D
from PIL import Image,ImageDraw
sys.path.insert(0,str(P(__file__).resolve().parent))
import run as v
def need(ok,msg):
    if not ok:raise ValueError(msg)
OUT=P(sys.argv[2]); ROOT=P(os.environ["RUNNER_TEMP"])
PATH=".github/workflows/studio-v3-profile-c-audit-recovery-20261127.yml"
MSG="Authorize profile C audit recovery after local input limit"
def fetch(run,artifact,digest,dest):
    prefix="repos/"+v.REPO+"/actions/"
    r=v.gh(prefix+"runs/"+str(run));need(r["conclusion"]=="success","Invalid source run")
    a=next(a for a in v.gh(prefix+"runs/"+str(run)+"/artifacts")["artifacts"] if a["id"]==artifact)
    need(a["digest"]=="sha256:"+digest and not a["expired"],"Archive metadata mismatch")
    with dest.open("xb") as out:subprocess.run(["gh","api",prefix+"artifacts/"+str(artifact)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
    need(dest.stat().st_size<70*1024*1024 and v.sha(dest.read_bytes())==digest,"Archive bytes mismatch")
def gate():
    need(os.getenv("GITHUB_REPOSITORY")==v.REPO and os.getenv("GITHUB_REF")=="refs/heads/"+v.BRANCH and os.getenv("GITHUB_EVENT_NAME")=="push" and os.getenv("GITHUB_RUN_ATTEMPT")=="1","Not one-shot")
    e=v.read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=e["before"]
    need(not e.get("forced") and e["after"]==h and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MSG and v.git("diff","--name-status",before,h)=="A\t"+PATH,"Invalid launch")
    from urllib.parse import urlencode
    need(not v.gh("repos/"+v.REPO+"/commits?"+urlencode({"sha":before,"path":PATH,"per_page":1})),"Already used")
    need(v.gh("repos/"+v.REPO+"/pulls/198")["head"]["sha"]==v.SOURCE,"Source changed")
    reg=v.gh("repos/"+v.REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    import base64
    doc=json.loads(base64.b64decode(v.gh("repos/"+v.REPO+"/git/blobs/"+reg["sha"])["content"]))
    w=next(x for x in doc["active"] if x["id"]==v.ID)
    need(w["owner"]==v.OWNER and w["ownership_revision"]==14 and w["ownership_state"]=="claimed","Studio ownership moved")
def sheet(items,path):
    ims=[]
    for label,b in items:
        with Image.open(io.BytesIO(b)) as im:
            im=im.convert("RGB");im.thumbnail((550,850));ims.append((label,im))
    canvas=Image.new("RGB",(sum(i.width for _,i in ims),max(i.height for _,i in ims)+32),"#18151b");d=ImageDraw.Draw(canvas);x=0
    for label,i in ims:
        d.text((x+8,8),label,fill="white");canvas.paste(i,(x,32));x+=i.width
    canvas.save(path,quality=93)
def prepare():
    gate();OUT.mkdir()
    a=ROOT/"audit-replay.zip";b=ROOT/"audit-native.zip"
    fetch(46025786624,15212037696,"d21b490559988c1a43a6d995fc50be3ba1cd75ae6e180e66630a9d98ad0ac460",a)
    fetch(45968651560,15183288699,"0b00ca40004f2f8e2a61f14e2b0f1b4bc46658c0a56393e83e754986936637bc",b)
    with zipfile.ZipFile(a) as z,zipfile.ZipFile(b) as n:
        r=json.loads(z.read("report.json"));need(r["source_sha"]==v.SOURCE and r["candidates"]["C"]["passed"] and r["candidates"]["C"]["other_views_pixel_identical"] and D(r["prior_estimated_usd"])==D("0.2782585"),"Evidence mismatch")
        sheet([("BROWSER BASELINE",z.read("baseline/captures/phone-Overview.png")),("CANDIDATE C",z.read("candidate-C/captures/phone-Overview.png")),("C SCROLLED",z.read("candidate-C/captures/phone-Overview-bottom.png"))],OUT/"matched.jpg")
        sheet([("PR198 PRIOR UNITY PROFILE",n.read("phone-Overview.png")),("PR198 PRIOR UNITY TROPHIES",n.read("phone-Trophies.png"))],OUT/"native.jpg")
    refs=[("Whiteout Survival chief profile","https://res.ldrescdn.com/rms/ldplayer/process/img/37d9127ebd0c406ab7fe4796aa0788a21755649793.jpg?x-oss-process=image%2Fformat%2Cwebp%2Fquality%2CQ_100%2Fresize%2Cw_1024"),("Call of Dragons lord profile","https://st.gamevui.vn/images/image/2023/11/01/chon-settings.jpg")]
    imgs=[];meta=[]
    for label,url in refs:
        with urllib.request.urlopen(urllib.request.Request(url,headers={"User-Agent":"Mozilla/5.0"}),timeout=30) as f:raw=f.read(8*1024*1024+1)
        need(len(raw)<8*1024*1024,"Reference too large")
        with Image.open(io.BytesIO(raw)) as im:need(min(im.size)>=180,"Reference too small")
        imgs.append((label,raw));meta.append({"name":label,"url":url,"sha256":v.sha(raw)})
    sheet(imgs,OUT/"commercial.jpg");v.dump(OUT/"references.json",meta)
    print("PROFILE_AUDIT_INPUTS_VERIFIED",flush=True)
def audit():
    gate();need(all((OUT/x).is_file() for x in ("matched.jpg","native.jpg","commercial.jpg","references.json")),"Missing inputs")
    old=v.gh("repos/"+v.REPO+"/actions/runs/46026558463");need(old["conclusion"]=="failure","Earlier audit not closed")
    with Image.open(OUT/"native.jpg") as a, Image.open(OUT/"commercial.jpg") as b:
        a=a.convert("RGB");b=b.convert("RGB")
        c=Image.new("RGB",(a.width+b.width,max(a.height,b.height)),"#18151b");c.paste(a,(0,0));c.paste(b,(a.width,0));c.save(OUT/"reference-montage.jpg",quality=92)
    v.SLOTS={"audit":D(".75")}
    l=v.Ledger(OUT);row=l.reserve("audit")
    p=OUT/"brief.txt";p.write_text("Audit independent of authors, only visual presentation. First montage: matched browser baseline and C, C scrolled. Second montage: left real prior Unity PR198 captures, different implementation, not C; right historical third-party commercial screenshots, not latest live builds; compare design principles only, account for differing gameplay and screen aspect, never demand new mechanics. Grade baseline and C on resources, integration, coherence, composition, materials, readability, polish 0-10; weights 20/20/15/15/10/10/10. Evaluate visible issues specifically; ignore QA header/footer as test scaffolding but evaluate actual nav. No native certification or approval. Distinguish whether C is at least 8 and competitive, and propose one high-impact correction if not. JSON object with scores {baseline:{resources,integration,coherence,composition,materials,readability,polish},candidate:{same keys}}, findings [strings], commercial_comparison string, next_action string. Spanish. No fabricated features or evidence.",encoding="utf-8")
    payload,manifest=v.studio.make_payload("visual","gpt-6.1-terra",p,tuple(OUT/x for x in ("matched.jpg","reference-montage.jpg")))
    for c in payload["input"][0]["content"]:
        if c["type"]=="input_image":c["detail"]="high"
    payload["instructions"]="Act as a critical independent game art director. Return only valid JSON, no markdown."
    payload["max_output_tokens"]=3000
    raw=v.studio.request_api(payload,os.environ["OPENAI_API_KEY"]);l.finish(row,raw.get("usage"),raw.get("model") or "gpt-6.1-terra",raw.get("service_tier") or "standard")
    rep=v.studio.report("visual",payload,manifest,raw);v.dump(OUT/"audit-receipt.json",rep);a=v.json_result(rep["response"])
    weights={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
    need(set(a["scores"])=={"baseline","candidate"},"Missing scores")
    for x,s in a["scores"].items():need(set(s)==set(weights) and all(type(n) in (int,float) and 0<=n<=10 for n in s.values()),"Invalid scores")
    scores={x:round(sum(weights[k]*s[k] for k in weights)/100,3) for x,s in a["scores"].items()}
    result={"weighted_scores":scores,"audit":a,"estimated_audit_usd":row["estimated_usd"],"total_estimated_usd":str(D("0.2782585")+D(row["estimated_usd"])),"native_candidate":False,"art_approved":False,"browser_target_met":scores["candidate"]>=8}
    v.dump(OUT/"summary.json",result);print("PROFILE_C_AUDIT_RESULT "+json.dumps(result,ensure_ascii=False),flush=True)
if sys.argv[1]=="prepare":prepare()
elif sys.argv[1]=="audit":audit()
else:raise ValueError("mode")
