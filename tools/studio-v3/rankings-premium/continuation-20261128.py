"""Bounded hosted PR198 art continuation. Real artists, existing original art, actual browser QA."""
import base64, datetime as dt, json, os, re, subprocess, sys, zipfile
from decimal import Decimal as D
from pathlib import Path as P
from urllib.parse import urlencode
from PIL import Image, ImageDraw
HERE=P(__file__).resolve().parent
sys.path.insert(0,str(HERE));sys.path.insert(0,str(HERE/"pivot"))
import run as v
import layout
ID="pr198-six-view-refinement-20261128"
OWNER="chatgpt-eldoria-rankings-art-continuation-20261128"
REQUEST="tools/studio-v3/rankings-premium/continuation-launch-20261128.json"
MESSAGE="Launch bounded PR198 six-view refinement 20261128"
SLOTS={"director":D(".15"),"integrator":D(".30"),"audit":D(".15")}
SECTIONS=layout.SECTIONS
PAGES=layout.PAGES
BASE=(46025786624,15212037696,"d21b490559988c1a43a6d995fc50be3ba1cd75ae6e180e66630a9d98ad0ac460")
ART=(46024809880,15210344144,"52774d0cf55f0a98208edfebea274e72be5e507c461018affd8cfbc21200886e")
def need(x,msg):
    if not x: raise ValueError(msg)
def gate():
    need(os.getenv("GITHUB_REPOSITORY")==v.REPO and os.getenv("GITHUB_REF")=="refs/heads/"+v.BRANCH and os.getenv("GITHUB_EVENT_NAME")=="push" and os.getenv("GITHUB_RUN_ATTEMPT")=="1","Only first authorized push")
    ev=v.read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=ev["before"]
    need(not ev.get("forced") and not ev.get("deleted") and ev["after"]==h and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"Wrong launch")
    need(v.git("diff","--name-status",before,h)=="A\t"+REQUEST,"Only new launch file permitted")
    need(not v.gh("repos/"+v.REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1})),"Already launched")
    need(v.gh("repos/"+v.REPO+"/git/ref/heads/"+v.BRANCH)["object"]["sha"]==h,"Pilot moved")
    pr=v.gh("repos/"+v.REPO+"/pulls/198");need(pr["state"]=="open" and pr["head"]["sha"]==v.SOURCE,"PR198 changed")
    meta=v.gh("repos/"+v.REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    doc=json.loads(base64.b64decode(v.gh("repos/"+v.REPO+"/git/blobs/"+meta["sha"])["content"]))
    w=next(x for x in doc["active"] if x["id"]==v.ID);g=w["ranking_mission"]["continuation_20261128"]
    need(w["owner"]==OWNER and w["ownership_revision"]==17 and w["ownership_state"]=="claimed" and not w["resources"],"Studio lease changed")
    need(v.read(REQUEST)==g and g["id"]==ID and g["source_sha"]==v.SOURCE,"Grant mismatch")
    need(g["prior_worst_case_bound_usd"]==w["ranking_mission"]["atelier"]["total_mission_bound_usd"]=="4.16061740","Previous conservative bound changed")
    need(sum(SLOTS.values())==D(g["new_reservation_usd"])==D(".60") and D(g["maximum_aggregate_usd"])==D("4.16061740")+sum(SLOTS.values())<=D(w["ranking_mission"]["cap_usd"])==D("5.00"),"Cap")
    need(dt.datetime.now(dt.timezone.utc).date()<dt.date.fromisoformat(g["expiry"]),"Grant expired")
    print("PR198_CONTINUATION_AUTHORIZED",ID,"USD .60 existing cap",flush=True)
def artifact(spec,dest,names):
    run,aid,digest=spec
    a=next(x for x in v.gh("repos/"+v.REPO+"/actions/runs/"+str(run)+"/artifacts")["artifacts"] if x["id"]==aid)
    need(a["digest"]=="sha256:"+digest and not a["expired"],"Artifact identity")
    dest.mkdir(parents=True,exist_ok=True);f=dest/"source.zip"
    with f.open("xb") as out:subprocess.run(["gh","api","repos/"+v.REPO+"/actions/artifacts/"+str(aid)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
    need(f.stat().st_size<=70*1024*1024 and v.sha(f.read_bytes())==digest,"Artifact bytes mismatch")
    with zipfile.ZipFile(f) as z:
        for n,relative in names:
            p=dest/relative;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    f.unlink()
def sheet(folder,dest,label):
    out=Image.new("RGB",(390*3,868*2),"#15131a");draw=ImageDraw.Draw(out)
    for i,k in enumerate(SECTIONS):
        im=Image.open(folder/("phone-"+k+".png")).convert("RGB")
        need(im.size==(390,844),"Unexpected viewport")
        x=(i%3)*390;y=(i//3)*868
        out.paste(im,(x,y+24));draw.text((x+8,y+5),label+" / "+k,fill="white")
    out.save(dest,quality=87,optimize=True)
def prepare(source,projection,out):
    gate();out.mkdir()
    data=v.read(projection);need(data["sourceSha"]==v.SOURCE and data["fixture"] is True and not any(p["publicRankings"] for p in data["profiles"]),"Canonical projection")
    v.dump(out/"projection.json",data)
    artifact(BASE,out/"prior",[("candidate-C/theme.json","theme.json")]+[("candidate-C/captures/phone-"+k+".png","captures/phone-"+k+".png") for k in SECTIONS])
    artifact(ART,out/"art",[("art-C/"+n+".png",n+".png") for n in ["hall","relic","atlas-1","atlas-2","atlas-3"]])
    theme=v.read(out/"prior/theme.json");need(theme["pages"]["Overview"]["art"]=="profile-hero","Unexpected profile C")
    theme["pages"]["Overview"]["art"]="hall";layout.validate_pages(theme["pages"]);layout.validate_css(theme["css"])
    v.dump(out/"prior/theme-adapted.json",theme)
    sheet(out/"prior/captures",out/"baseline.jpg","PRIOR")
    # Replay all existing views without paid calls, before exposing the key.
    rows=layout.compose(data,source,out/"art",theme["pages"],theme["css"],out/"offline-replay")
    need(len(rows)>=18,"Baseline replay incomplete")
    print("PR198_CONTINUATION_NO_SPEND_REPLAY_OK",len(rows),flush=True)
def live(source,projection,out):
    gate();need((out/"offline-replay/theme.json").exists(),"Missing no-spend replay")
    data=v.read(projection);need(data==v.read(out/"projection.json"),"Projection changed")
    theme=v.read(out/"prior/theme-adapted.json")
    key=os.environ.get("OPENAI_API_KEY");need(bool(key),"Missing key")
    v.SLOTS=SLOTS;ledger=v.Ledger(out)
    def ask(slot,model,instructions,common,images=(),maxout=2500):
        p=out/(slot+"-input.txt");p.write_text(common.encode("utf-8")[:22000].decode("utf-8",errors="ignore"),encoding="utf-8")
        payload,manifest=v.studio.make_payload("visual" if images else "programming",model,p,images)
        payload["instructions"]=instructions+" Return exactly one JSON object; no markdown."
        payload["max_output_tokens"]=maxout
        for c in payload["input"][0]["content"]:
            if c["type"]=="input_image":c["detail"]="high"
        row=ledger.reserve(slot);print("STUDIO_CONTINUATION_ROLE_START",slot,flush=True)
        raw=v.studio.request_api(payload,key);ledger.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
        rep=v.studio.report("visual" if images else "programming",payload,manifest,raw)
        v.dump(out/(slot+"-receipt.json"),rep)
        obj=v.json_result(rep["response"]);v.dump(out/(slot+"-output.json"),obj)
        print("STUDIO_CONTINUATION_ROLE_COMPLETE",slot,row["estimated_usd"],flush=True);return obj
    common="PR198 exact source "+v.SOURCE+". Actual QA fixture, no invented ranks/rewards/players. Reuse original hall C and existing distinct atlases; preserve live text, real labels, locks, row ordering and navigation. Aim for premium noble Valoria, not six identical cards. Prior C 7.35, others 6–6.5. No new systems. Native and commercial-reference validation pending.\n"+(source/"contracts/ART_DIRECTION.md").read_text()[:3500]+"\n"+(source/"contracts/SHARED_STANDARD.md").read_text()[:2500]+"\nActual B5 data: "+json.dumps(data["profiles"][-1],ensure_ascii=False)[:8500]
    d=ask("director","gpt-6.1-terra","Independent senior commercial fantasy game art director. Inspect the six actual prior captures. Design six differentiated compositions and a discovery-detail view using the existing imagery, with strong identity/typography, materiality and phone legibility; explain concrete corrections. Return {concept:string,pages:{Overview:string,Rankings:string,Trophies:string,Discoveries:string,Heraldry:string,Monuments:string,DiscoveryDetail:string}}. Do not pretend a new image was made.",common,(out/"baseline.jpg",),3500)
    need(set(d)=={"concept","pages"} and set(d["pages"])==set(PAGES) and all(isinstance(x,str) and 20<=len(x)<=1800 for x in d["pages"].values()),"Director contract")
    # Actual declarative art implementation, not merely a design brief.
    ins="You are a senior UI artist/technical integrator. Author new actual declarative templates and scoped CSS for the seven real views. Return {pages:{each of Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments,DiscoveryDetail:{outer,lead,row,art}},css:string}. Each outer HTML contains {{heading}},{{art}},{{lead}},{{rows}} exactly once, lead before rows; detail additionally {{back}}. Lead/row each contains {{icon}},{{title}},{{detail}},{{state}} exactly once. Tags div,section,article,span,aside,figure,header only; classes x-lowercase only; no visible hardcoded text. art hall or relic. CSS is an APPEND patch to existing styles, 700–7500 chars total; each rule selector begins .page-Overview or another actual page ID, no @ rules, urls, comments, scripts, hidden rows, generated labels, or unscoped selectors. Use existing art only. Real labels >=11px. Honor 320/390 phone and landscape; do not invent buttons or ranks. Distinct hierarchy and material language, not recolor. Preserve all actual values."
    # Prior page templates and compact CSS tail help avoid fighting the existing C layout.
    brief=common+"\nDirector: "+json.dumps(d,ensure_ascii=False)+"\nExisting pages: "+json.dumps(theme["pages"],ensure_ascii=False)+"\nCurrent CSS excerpt: "+theme["css"][-5000:]
    result=ask("integrator","gpt-6.2-sol",ins,brief,(out/"baseline.jpg",),7000)
    need(set(result)=={"pages","css"},"Integrator contract")
    pages=layout.validate_pages(result["pages"]);patch=layout.validate_css(result["css"])
    need("@" not in patch and 700<=len(patch)<=7500,"Only bounded scoped CSS")
    # The trusted renderer validates tokens, markup and CSS; additionally reject
    # global selectors and any CSS attempts to hide canonical data.
    need(not re.search(r"display\s*:\s*none|visibility\s*:\s*hidden",patch,re.I),"Hidden data CSS")
    for selectors in re.findall(r"([^{}]+)\{",patch):
        # Split only top-level selector commas (e.g. commas in :is are allowed).
        depth=0;parts=[];start=0
        for i,ch in enumerate(selectors):
            if ch in "([":depth+=1
            elif ch in ")]":depth-=1
            elif ch=="," and depth==0:parts.append(selectors[start:i]);start=i+1
        parts.append(selectors[start:])
        need(all(any(s.strip().startswith(".page-"+k) for k in PAGES) for s in parts),"Unscoped selector")
    rows=layout.compose(data,source,out/"art",pages,theme["css"]+"\n"+patch,out/"candidate")
    need(len(rows)>=18,"Candidate frames incomplete")
    sheet(out/"candidate/captures",out/"candidate.jpg","CANDIDATE")
    a=ask("audit","gpt-6.1-terra","Independent visual auditor. Compare both actual 390×844 six-view contact sheets, prior and candidate. Score each screen baseline and candidate 0–10 by resources 20%, integration 20%, coherence 15%, composition 15%, materials 10%, mobile readability 10%, polish 10%. Return {baseline:{six section IDs:number},candidate:{six section IDs:number},findings:{six section IDs:string},decision:'revise'|'candidate'}. No inflated marks, no native or production approval. Candidate only if every score >=8 and no regression.",common,(out/"baseline.jpg",out/"candidate.jpg"),3000)
    need(set(a)=={"baseline","candidate","findings","decision"} and all(set(a[k])==set(SECTIONS) for k in ("baseline","candidate","findings")) and all(type(n) in (int,float) and 0<=n<=10 for k in ("baseline","candidate") for n in a[k].values()),"Audit contract")
    passed=a["decision"]=="candidate" and min(a["candidate"].values())>=8 and all(a["candidate"][k]>=a["baseline"][k] for k in SECTIONS)
    s={"mission":ID,"source_sha":v.SOURCE,"artist_roles":len(ledger.rows),"estimated_usd":str(sum(D(r["estimated_usd"]) for r in ledger.rows)),"billing_verified":False,"frames":len(rows),"audit":a,"browser_quality_gate":passed,"native_unity":False,"production_promoted":False,"art_approved":False}
    v.dump(out/"summary.json",s);print("PR198_CONTINUATION_RESULT",json.dumps(s,ensure_ascii=False),flush=True)
if __name__=="__main__":
    mode,src,proj,dest=sys.argv[1:5]
    if mode=="--prepare":prepare(P(src),P(proj),P(dest))
    elif mode=="--live":live(P(src),P(proj),P(dest))
    else:raise ValueError("Explicit mode required")
