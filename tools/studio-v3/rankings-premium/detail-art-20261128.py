"""Actual six ranking detail screens on the selected art. Hosted browser QA; never production."""
import base64,datetime as dt,json,os,re,shutil,subprocess,sys,zipfile
from decimal import Decimal as D
from pathlib import Path as P
from urllib.parse import urlencode
from PIL import Image,ImageDraw
from playwright.sync_api import sync_playwright
import run as v
HERE=P(__file__).resolve().parent
ID="pr198-detail-art-20261128"
OWNER="chatgpt-eldoria-rankings-art-continuation-20261128"
SRC="519c9c2d2a713c64841d77057bd32b7dd2ac8252"
ART=15279774063
DIGEST="3e66b4e4fcd1ea3fad8396034ebe81ffd6251943c0ef6ac83d937ab68b5943ad"
REQUEST="tools/studio-v3/rankings-premium/detail-launch-20261128.json"
MESSAGE="Launch owner requested PR198 six detail views and bounded art refinement"
KINDS=["TotalPower","Kingdom","Army","Vigilants","Hunters","Explorers"]
EXISTING=["Overview","Rankings","Trophies","Discoveries","Heraldry","Monuments"]
def gate():
    v.need(os.getenv("GITHUB_REPOSITORY")==v.REPO and os.getenv("GITHUB_REF")=="refs/heads/"+v.BRANCH and os.getenv("GITHUB_EVENT_NAME")=="push" and os.getenv("GITHUB_RUN_ATTEMPT")=="1","one shot")
    e=v.read(os.environ["GITHUB_EVENT_PATH"]);h=os.environ["GITHUB_SHA"];before=e["before"]
    v.need(not e.get("forced") and not e.get("deleted") and e["after"]==h and v.git("rev-parse","HEAD")==h and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE,"launch")
    v.need(v.git("diff","--name-status",before,h)=="A\t"+REQUEST,"only grant")
    v.need(not v.gh("repos/"+v.REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1})),"already launched")
    v.need(v.gh("repos/"+v.REPO+"/git/ref/heads/"+v.BRANCH)["object"]["sha"]==h,"branch moved")
    pr=v.gh("repos/"+v.REPO+"/pulls/198");v.need(pr["state"]=="open" and pr["head"]["sha"]==SRC,"PR moved")
    m=v.gh("repos/"+v.REPO+"/contents/pipeline/active-workstreams.json?ref=main");doc=json.loads(base64.b64decode(v.gh("repos/"+v.REPO+"/git/blobs/"+m["sha"])["content"]))
    w=next(x for x in doc["active"] if x["id"]==v.ID);g=w["ranking_mission"]["detail_art_20261128"]
    v.need(w["owner"]==OWNER and w["ownership_revision"]==17 and not w["resources"] and w["ownership_state"]=="claimed","claim")
    v.need(v.read(REQUEST)==g and g["id"]==ID and g["source_sha"]==SRC and g["source_artifact"]==ART,"grant")
    v.need(D(g["prior_worst_case_bound_usd"])==D("4.91061740") and D(g["new_reservation_usd"])==D(".07") and D(g["maximum_aggregate_usd"])==D("4.98061740")<=D(w["ranking_mission"]["cap_usd"])==D("5.00"),"cap")
    v.need(dt.datetime.now(dt.timezone.utc).date()<dt.date.fromisoformat(g["expiry"]),"expired")
def capture(root,proof):
    proof.mkdir(parents=True,exist_ok=False); results=[]
    with sync_playwright() as p:
        b=p.chromium.launch(headless=True,args=["--no-sandbox"])
        for name,w,h in [("phone",390,844),("compact",320,720),("landscape",844,390)]:
            page=b.new_page(viewport={"width":w,"height":h},device_scale_factor=1);errors=[];page.on("pageerror",lambda e:errors.append(str(e)))
            page.route("**/*",lambda r:r.continue_() if r.request.url.startswith("file:") else r.abort())
            page.goto((root/"index.html").resolve().as_uri());page.wait_for_function("window.qa")
            for s in EXISTING:
                page.locator('[data-section="'+s+'"]').click()
                q=page.evaluate("""() => {const s=qa.get(),rows=s.profile.sections.find(x=>x.id===s.section).rows,dom=Array.from(document.querySelectorAll('.row'));return rows.length===dom.length&&rows.every((x,i)=>dom[i].dataset.key===x.key&&dom[i].querySelector('.title').textContent===x.title&&dom[i].querySelector('.detail').textContent===x.detail)}""")
                v.need(q,"canonical projection "+s)
                v.need(page.evaluate('document.querySelector("#app").scrollWidth<=innerWidth+1'),"overflow "+s)
                page.screenshot(path=str(proof/(name+"-"+s+".png")))
            page.locator('[data-section="Rankings"]').click()
            v.need(page.locator('button.row[data-key^="ranking:"]').count()==6,"six links")
            for k in KINDS:
                btn=page.locator('button[data-key="ranking:'+k+'"]');btn.click()
                v.need(page.locator('.rank-detail[data-ranking="'+k+'"][data-fixture="false"]').count()==1,"category")
                v.need(page.locator(".rank-player,.rank-champion").count()==0,"no fake offline")
                page.locator(".rank-toggle").click()
                v.need(page.locator(".rank-champion").count()==3 and page.locator(".rank-player").count()==5,"fixture rows")
                v.need("SOLO QA" in page.locator(".rank-qa").inner_text(),"fixture labeling")
                v.need(page.evaluate('document.querySelector("#app").scrollWidth<=innerWidth+1'),"overflow "+k)
                v.need(page.locator(".rank-own").is_visible(),"own position")
                page.screenshot(path=str(proof/(name+"-"+k+".png")))
                page.locator(".rank-back").click();v.need(page.locator('button.row[data-key^="ranking:"]').count()==6,"return")
                results.append({"viewport":name,"category":k,"offline_empty":True,"fixture_rows":8,"return":True})
            page.select_option("#level","1");v.need(page.locator('[data-section="Rankings"]').count()==0,"B1 locked")
            v.need(not errors,"browser error");page.close()
        b.close()
    v.dump(proof/"report.json",results)
    return results
def sheet(proof,out,keys):
    im=Image.new("RGB",(1170,870*2),"#17141d");d=ImageDraw.Draw(im)
    for i,k in enumerate(keys):
        x,y=(i%3)*390,(i//3)*870
        im.paste(Image.open(proof/("phone-"+k+".png")).convert("RGB"),(x,y+26));d.text((x+6,y+6),k,fill="white")
    im.save(out,quality=88,optimize=True)
def patch(html,css,js):
    def once(a,b):
        nonlocal html
        v.need(html.count(a)==1,"source anchor "+a[:50]);html=html.replace(a,b,1)
    once("</style>",css+"</style>")
    once('const canOpen=section==="Discoveries"&&detail===null&&Object.hasOwn(profile.details,r.key);','const canOpen=detail===null&&((section==="Discoveries"&&Object.hasOwn(profile.details,r.key))||(section==="Rankings"&&r.key.startsWith("ranking:")));')
    once('for(const b of main.querySelectorAll(".row.action"))b.onclick=()=>{detail=b.dataset.key;render()};','for(const b of main.querySelectorAll(".row.action"))b.onclick=()=>{if(section==="Rankings"&&b.dataset.key.startsWith("ranking:")){rankScroll=main.scrollTop;showRankDetail(b.dataset.key.slice(8));return;}detail=b.dataset.key;render()};')
    anchor='document.querySelector("#level").onchange=';once(anchor,js+"\n"+anchor)
    return html
def prepare(out):
    gate();out.mkdir()
    a=next(x for x in v.gh("repos/"+v.REPO+"/actions/runs/46187304641/artifacts")["artifacts"] if x["id"]==ART)
    v.need(a["digest"]=="sha256:"+DIGEST and not a["expired"],"artifact")
    f=out/"source.zip"
    with f.open("xb") as st:subprocess.run(["gh","api","repos/"+v.REPO+"/actions/artifacts/"+str(ART)+"/zip"],stdout=st,stderr=subprocess.PIPE,check=True,timeout=120)
    v.need(v.sha(f.read_bytes())==DIGEST and f.stat().st_size<70*1024*1024,"artifact bytes")
    baseline=out/"baseline";baseline.mkdir()
    with zipfile.ZipFile(f) as z:
        s=json.loads(z.read("summary.json"));v.need(s["browser_data_parity"] and len(s["pixel_equality"])==21,"selected evidence")
        for n in z.namelist():
            if n=="selected/index.html" or re.fullmatch(r"selected/assets/[a-zA-Z0-9_-]+\.(png|jpg|jpeg)",n):
                p=baseline/n.removeprefix("selected/");p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
            elif re.fullmatch(r"selected/captures/phone-[A-Za-z]+\.png",n):
                p=out/"prior"/P(n).name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    f.unlink()
    candidate=out/"candidate";shutil.copytree(baseline,candidate)
    html=patch((baseline/"index.html").read_text(),(HERE/"detail-art-20261128.css").read_text(),(HERE/"detail-art-20261128.js").read_text())
    (candidate/"index.html").write_text(html)
    results=capture(candidate,out/"candidate-captures");sheet(out/"candidate-captures",out/"candidate-details.jpg",KINDS);sheet(out/"candidate-captures",out/"candidate-existing.jpg",EXISTING);sheet(out/"prior",out/"prior-existing.jpg",EXISTING)
    print("PR198_DETAIL_BROWSER_QA",len(results),flush=True)
def run_art(out):
    gate();v.SLOTS={"artist":D(".04"),"audit":D(".03")};l=v.Ledger(out)
    def ask(slot,images,instructions,maxout):
        ev=out/(slot+"-input.txt");ev.write_text("PR198 isolated browser preview. Six rankings, all sample players explicitly QA-only. Existing original Valoria resources reused. Preserve data, category navigation, scroll, real offline state and QA labels. Existing hub approved; focus material, legibility and ornament, not feature redesign.")
        payload,m=v.studio.make_payload("visual","gpt-6.1-terra",ev,images)
        payload["instructions"]=instructions+" Return strict JSON only."
        payload["max_output_tokens"]=maxout
        # limit the request to the reserved budget with low detail; never retry a paid call
        row=l.reserve(slot);raw=v.studio.request_api(payload,os.environ["OPENAI_API_KEY"]);l.finish(row,raw.get("usage"),raw.get("model") or "gpt-6.1-terra",raw.get("service_tier") or "standard")
        rep=v.studio.report("visual",payload,m,raw);v.dump(out/(slot+"-receipt.json"),rep);a=v.json_result(rep["response"]);v.dump(out/(slot+"-result.json"),a);return a
    a=ask("artist",(out/"candidate-details.jpg",out/"candidate-existing.jpg"),"Senior game UI artist. Improve these real browser frames, especially ranking podium, material and compact Heraldry/Monuments. Return {css:string,reason:string}. CSS patch max 2000 chars, one rule per line, every selector starts .rank-detail or .page-Heraldry or .page-Monuments. No @, URLs, pseudo content, hidden elements, changed navigation, fake data. Only color, background, border, box-shadow, font-size, font-weight, letter-spacing, padding, gap, filter. Keep readable. Avoid increasing vertical density.",1400)
    v.need(set(a)=={"css","reason"} and isinstance(a["css"],str) and len(a["css"])<=2000,"artist shape")
    rules=[];allowed={"color","background","background-color","border","border-color","border-width","border-style","border-radius","box-shadow","font-size","font-weight","letter-spacing","padding","gap","filter"}
    for line in a["css"].splitlines():
        if not line.strip():continue
        m=re.fullmatch(r"([^{}]+)\{([^{}]+)\}",line.strip());v.need(m is not None,"artist css grammar")
        sel,decl=m.groups();v.need(not any(x in sel+decl for x in ["@","<","\\","/*","url(","!important"]) and "::" not in sel,"css unsafe")
        v.need(all(re.match(r"^\.(rank-detail|page-Heraldry|page-Monuments)(?![-\w])",s.strip()) for s in sel.split(",")),"scope")
        for prop in decl.split(";"):
            if prop.strip():v.need(prop.split(":",1)[0].strip() in allowed and ":" in prop,"style property")
        rules.append(line)
    refined=out/"refined";shutil.copytree(out/"candidate",refined);html=(refined/"index.html").read_text();(refined/"index.html").write_text(html.replace("</style>","\n".join(rules)+"</style>",1))
    results=capture(refined,out/"refined-captures");sheet(out/"refined-captures",out/"refined-details.jpg",KINDS);sheet(out/"refined-captures",out/"refined-existing.jpg",EXISTING)
    # Compare paired image contact sheets, with explicit labels and smaller image size.
    def pair(a,b,dst):
        im=Image.new("RGB",(1170,1770),"#18151d");d=ImageDraw.Draw(im)
        for i,p in enumerate([a,b]):
            x=i*585;pic=Image.open(p).convert("RGB").resize((585,870),Image.Resampling.LANCZOS);im.paste(pic,(x,25));d.text((x+8,8),["BASE","ARTIST"][i],fill="white")
        im=im.crop((0,0,1170,900));im.save(dst,quality=88)
    pair(out/"candidate-details.jpg",out/"refined-details.jpg",out/"compare-details.jpg")
    pair(out/"prior-existing.jpg",out/"refined-existing.jpg",out/"compare-existing.jpg")
    audit=ask("audit",(out/"compare-details.jpg",out/"compare-existing.jpg"),"Independent art auditor. Inspect actual baseline and artist candidate six details and six existing screens. Return {details:{TotalPower:number,Kingdom:number,Army:number,Vigilants:number,Hunters:number,Explorers:number},existing:{Overview:number,Rankings:number,Trophies:number,Discoveries:number,Heraldry:number,Monuments:number},regressions:[string],findings:string,decision:'candidate'|'revise'}. Score 0–10 for commercial quality, no inflated marks. Note small comparison image limitation. Reject real regressions; no claim of Unity validation.",1000)
    v.need(set(audit)=={"details","existing","regressions","findings","decision"} and set(audit["details"])==set(KINDS) and set(audit["existing"])==set(EXISTING) and all(type(n) in (float,int) and 0<=n<=10 for k in ("details","existing") for n in audit[k].values()),"audit")
    v.dump(out/"summary.json",{"source_sha":SRC,"browser_flows":len(results),"audit":audit,"all_8":min(list(audit["details"].values())+list(audit["existing"].values()))>=8 and not audit["regressions"],"estimated_usd":str(sum(D(r["estimated_usd"]) for r in l.rows)),"production":False,"native":False})
    print("PR198_DETAIL_ART_RESULT",json.dumps(v.read(out/"summary.json"),ensure_ascii=False),flush=True)
if __name__=="__main__":
    mode,out=sys.argv[1:];out=P(out)
    if mode=="--prepare":prepare(out)
    elif mode=="--art":run_art(out)
    else:raise ValueError("explicit mode")
