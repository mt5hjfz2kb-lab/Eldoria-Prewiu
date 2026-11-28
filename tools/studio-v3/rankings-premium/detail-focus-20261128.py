"""Zero-API visual polish on the existing actual QA artifact, not native or live ranking proof."""
import json,re,shutil,subprocess,sys,zipfile
from pathlib import Path as P
from PIL import Image,ImageDraw
from playwright.sync_api import sync_playwright
import run as v
import importlib.util
HERE=P(__file__).resolve().parent
spec=importlib.util.spec_from_file_location("detail_art",HERE/"detail-art-20261128.py")
d=importlib.util.module_from_spec(spec);spec.loader.exec_module(d)
ART=15281870582
DIGEST="8329da729610ac2d0bf3eb4e4f77027bc9199bc47973cb628a700840bd722234"
def main(out):
    out.mkdir()
    a=next(x for x in v.gh("repos/"+v.REPO+"/actions/runs/46190600883/artifacts")["artifacts"] if x["id"]==ART)
    v.need(not a["expired"] and a["digest"]=="sha256:"+DIGEST,"artifact identity")
    f=out/"source.zip"
    with f.open("xb") as st:subprocess.run(["gh","api","repos/"+v.REPO+"/actions/artifacts/"+str(ART)+"/zip"],stdout=st,stderr=subprocess.PIPE,check=True,timeout=120)
    v.need(v.sha(f.read_bytes())==DIGEST and f.stat().st_size<70*1024*1024,"artifact bytes")
    root=out/"focus";root.mkdir()
    with zipfile.ZipFile(f) as z:
        summary=json.loads(z.read("summary.json"));v.need(summary["source_sha"]==d.SRC and summary["browser_flows"]==18,"source fixture")
        for n in z.namelist():
            if n=="refined/index.html" or re.fullmatch(r"refined/assets/[a-zA-Z0-9_-]+\.(png|jpg|jpeg)",n):
                p=root/n.removeprefix("refined/");p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(n))
    f.unlink()
    html=(root/"index.html").read_text();css=(HERE/"detail-focus-20261128.css").read_text()
    v.need(html.count("</style>")==1 and "</style>" not in css,"patch")
    # Keep all canonical rows and explicit QA labels unchanged.
    (root/"index.html").write_text(html.replace("</style>",css+"</style>",1))
    captures=out/"captures";rows=d.capture(root,captures)
    checks=[]
    with sync_playwright() as p:
        b=p.chromium.launch(headless=True,args=["--no-sandbox"])
        for name,w,h in [("phone",390,844),("compact",320,720),("landscape",844,390)]:
            page=b.new_page(viewport={"width":w,"height":h},device_scale_factor=1)
            page.route("**/*",lambda r:r.continue_() if r.request.url.startswith("file:") else r.abort())
            page.goto((root/"index.html").resolve().as_uri());page.locator('[data-section="Rankings"]').click()
            for k in d.KINDS:
                page.locator('button[data-key="ranking:'+k+'"]').click()
                v.need(not page.locator("#nav").is_visible(),"detail must be separate")
                v.need(page.locator(".rank-player,.rank-champion").count()==0,"offline must be empty")
                page.locator(".rank-toggle").click()
                own=page.locator(".rank-own").bounding_box();main=page.locator("main").bounding_box()
                fits=own["y"]>=main["y"]-.5 and own["y"]+own["height"]<=main["y"]+main["height"]+.5
                if name!="landscape":v.need(fits,"own position below phone fold "+k)
                page.locator(".rank-back").click()
                v.need(page.locator("#nav").is_visible() and page.locator('button.row[data-key^="ranking:"]').count()==6,"back to hub")
                checks.append({"view":name,"category":k,"own_visible":fits})
            page.close()
        b.close()
    d.sheet(captures,out/"details.jpg",d.KINDS);d.sheet(captures,out/"existing.jpg",d.EXISTING)
    v.dump(out/"summary.json",{"source_artifact":ART,"fixture_source":d.SRC,"flows":len(rows),"focus_checks":checks,"native":False,"production":False,"api_calls":0,"independent_art_score":None})
    print("PR198_DETAIL_FOCUS_PASS",len(rows),len(checks),flush=True)
if __name__=="__main__":main(P(sys.argv[1]))
