"""Replay the three actually paid profile designs without API calls. Scope validation parses CSS tokens rather than splitting nested :is commas."""
from pathlib import Path as P
import sys,os,re,zipfile,json,hashlib,subprocess,shutil
from decimal import Decimal as D
from PIL import Image,ImageDraw
import tinycss2,cssselect2
from playwright.sync_api import sync_playwright
sys.path.insert(0,str(P(__file__).resolve().parent))
sys.path.insert(0,str(P(__file__).resolve().parent/"pivot"))
import run as v
import layout
def need(x,m):
    if not x:raise ValueError(m)
def fetch(run,artifact,digest,dest):
    repo="repos/"+v.REPO+"/actions/"
    r=v.gh(repo+"runs/"+str(run));need(r["status"]=="completed","Run still active")
    a=next(a for a in v.gh(repo+"runs/"+str(run)+"/artifacts")["artifacts"] if a["id"]==artifact)
    need(a["digest"]=="sha256:"+digest and not a["expired"],"Archive metadata mismatch")
    with dest.open("xb") as out:subprocess.run(["gh","api",repo+"artifacts/"+str(artifact)+"/zip"],stdout=out,stderr=subprocess.PIPE,check=True,timeout=120)
    need(dest.stat().st_size<60*1024*1024 and v.sha(dest.read_bytes())==digest,"Archive bytes mismatch")
def scoped(css):
    layout.validate_css(css)
    rules=tinycss2.parse_stylesheet(css,skip_comments=True,skip_whitespace=True)
    need(rules and all(r.type=="qualified-rule" for r in rules),"Only plain scoped CSS")
    for r in rules:
        groups=[[]]
        for t in r.prelude:
            if t.type=="literal" and t.value==",":groups.append([])
            else:groups[-1].append(t)
        for g in groups:
            s=tinycss2.serialize(g).strip()
            need(re.match(r"^\.page-Overview(?:[\s.#:\[>+~]|$)",s),"Outside Overview: "+s)
            # Forbid sibling escape from the scope root; actual agents use only descendants.
            need(not re.match(r"^\.page-Overview\s*[+~]",s),"Sibling escape")
            cssselect2.compile_selector_list(s)
        need(all(d.type in ("declaration","whitespace","comment") for d in tinycss2.parse_declaration_list(r.content)),"Bad declarations")
    return len(rules)
def run(out):
    out=P(out);out.mkdir()
    scratch=P(os.environ["RUNNER_TEMP"])
    oldzip=scratch/"profile-old.zip";newzip=scratch/"profile-paid.zip"
    fetch(45960681379,15181211611,"49a16a47523a7d7895fb0428cf21a527027eb2529ba0815618f4c5e1c3079fdd",oldzip)
    fetch(46024809880,15210344144,"52774d0cf55f0a98208edfebea274e72be5e507c461018affd8cfbc21200886e",newzip)
    base=out/"baseline";base.mkdir()
    with zipfile.ZipFile(oldzip) as z:
        for n in z.namelist():
            if n.startswith("round-2/assets/") or n in ("round-2/index.html","round-2/theme.json"):
                p=base/n.removeprefix("round-2/");need(p.resolve().is_relative_to(base.resolve()),"Unsafe path");p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    orig=(base/"index.html").read_text();theme=v.read(base/"theme.json")
    report={}
    with zipfile.ZipFile(newzip) as z:
        data=json.loads(z.read("projection.json"));led=json.loads(z.read("ledger.json"));need(all(r["state"]=="completed" for r in led["entries"]),"Uncertain cost")
        cost=sum(D(r["estimated_usd"]) for r in led["entries"]);need(cost==D("0.2782585"),"Cost mismatch")
        need(data["sourceSha"]==v.SOURCE and data["fixture"] is True and json.loads(orig.split("const data=",1)[1].split(",pages=",1)[0])==data,"Projection differs")
        v.dump(out/"paid-ledger.json",led)
        for x in "ABC":
            obj=json.loads(z.read("design-"+x+"-output.json"));css=obj["css"];n=scoped(css)
            pages=dict(theme["pages"]);pages["Overview"]=obj["page"];layout.validate_pages(pages)
            p=out/("candidate-"+x);p.mkdir();shutil.copytree(base/"assets",p/"assets")
            art=p/"assets/profile-hero.png";art.write_bytes(z.read("art-"+x+"/hall.png"))
            with Image.open(art) as im:im.convert("RGB").save(p/"assets/profile-hero.jpg",quality=94)
            # Separate image name ensures non-Overview screens are byte-for-byte unchanged.
            pages["Overview"]=dict(obj["page"],art="profile-hero")
            old=orig.split(",pages=",1)[1].split(";let profile=",1)[0];need(json.loads(old)==theme["pages"],"Baseline templates mismatch")
            script=orig.replace(",pages="+old+";let profile=",",pages="+json.dumps(pages,ensure_ascii=False).replace("<","\\u003c")+";let profile=",1).replace("</style>",css+"</style>",1)
            (p/"index.html").write_text(script,encoding="utf-8");v.dump(p/"theme.json",{"pages":pages,"css":theme["css"]+"\n"+css})
            report[x]={"scoped_rules":n,"art_sha256":v.sha(art.read_bytes()),"agent_css_unchanged":True,"html_sha256":v.sha(script.encode())}
    def qa(label,p):
        shots=p/"captures";shots.mkdir();checks=[]
        with sync_playwright() as pw:
            b=pw.chromium.launch(headless=True,args=["--no-sandbox"])
            for mode,w,h in [("phone",390,844),("compact",320,720),("landscape",844,390)]:
                page=b.new_page(viewport={"width":w,"height":h},device_scale_factor=1)
                errors=[];page.on("pageerror",lambda e:errors.append(str(e)))
                page.route("**/*",lambda r:r.continue_() if r.request.url.startswith("file:") else r.abort())
                page.goto((p/"index.html").resolve().as_uri());page.wait_for_function("window.qa")
                for k in layout.SECTIONS:
                    page.locator('[data-section="'+k+'"]').click();page.wait_for_timeout(70)
                    q=page.evaluate("""()=>{const s=qa.get(), rows=s.profile.sections.find(x=>x.id===s.section).rows, dom=Array.from(document.querySelectorAll('.row'));return {exact:rows.length===dom.length&&rows.every((x,i)=>dom[i].dataset.key===x.key&&dom[i].querySelector('.title').textContent===x.title&&dom[i].querySelector('.detail').textContent===x.detail),horizontal:document.querySelector('#app').scrollWidth<=innerWidth+1,clipped:dom.filter(e=>e.scrollWidth>e.clientWidth+1).length,buttons:Array.from(document.querySelectorAll('#nav button')).every(e=>e.getBoundingClientRect().width>=44),publicRankings:s.profile.publicRankings,minText:Math.min(...Array.from(document.querySelectorAll('.title,.detail')).map(e=>parseFloat(getComputedStyle(e).fontSize))),imageOK:Array.from(document.images).every(e=>e.complete&&e.naturalWidth>0)}}""")
                    checks.append({"mode":mode,"section":k,**q})
                    page.screenshot(path=str(shots/(mode+"-"+k+".png")))
                    if k=="Overview":
                        page.evaluate("()=>{const m=document.querySelector('main');m.scrollTop=m.scrollHeight}")
                        page.screenshot(path=str(shots/(mode+"-Overview-bottom.png")))
                page.locator('[data-section="Discoveries"]').click();page.locator(".row.action").first.click()
                need(page.evaluate("""()=>{let s=qa.get(),rows=s.profile.details[s.detail],dom=Array.from(document.querySelectorAll('.row'));return rows.length===dom.length&&rows.every((x,i)=>dom[i].dataset.key===x.key&&dom[i].querySelector('.title').textContent===x.title&&dom[i].querySelector('.detail').textContent===x.detail)}"""),"Detail regression")
                page.locator(".back").click()
                for level,count in [("1",1),("2",3),("3",4),("4",5)]:
                    page.select_option("#level",level);need(page.locator("#nav button").count()==count,"Unlock regression")
                need(not errors,"Browser errors: "+str(errors));page.close()
            b.close()
        passed=all(t["exact"] and t["horizontal"] and not t["clipped"] and t["buttons"] and not t["publicRankings"] and t["minText"]>=11 and t["imageOK"] for t in checks)
        report.setdefault(label,{}).update(passed=passed,checks=checks)
    for x,p in [("BASELINE",base)]+[(x,out/("candidate-"+x)) for x in "ABC"]:qa(x,p)
    # Non-profile matched captures must be bit-identical; compare actual pixel bytes, not PNG metadata.
    for x in "ABC":
        others=[p for p in (base/"captures").glob("*.png") if "Overview" not in p.name]
        same=all(Image.open(p).tobytes()==Image.open(out/("candidate-"+x)/"captures"/p.name).tobytes() for p in others)
        report[x]["other_views_pixel_identical"]=same
        report[x]["passed"]=report[x]["passed"] and same
    for suffix in ("","-bottom"):
        sheet=Image.new("RGB",(390*4,876),"#17121c");draw=ImageDraw.Draw(sheet)
        for i,(label,p) in enumerate([("BASELINE",base)]+[(x,out/("candidate-"+x)) for x in "ABC"]):
            sheet.paste(Image.open(p/"captures"/("phone-Overview"+suffix+".png")).convert("RGB"),(390*i,32));draw.text((390*i+12,10),label,fill="white")
        sheet.save(out/("comparison"+suffix+".png"))
    result={"source_sha":v.SOURCE,"previous_run":46024809880,"prior_estimated_usd":str(cost),"new_paid_calls":0,"native_unity":False,"art_approved":False,"candidates":report}
    v.dump(out/"report.json",result);print("PROFILE_REPLAY_RESULT",json.dumps({"prior_estimated_usd":str(cost),"new_paid_calls":0,"candidates":{k:{n:v for n,v in x.items() if n!="checks"} for k,x in report.items()}},ensure_ascii=False),flush=True)
if __name__=="__main__":run(sys.argv[1])
