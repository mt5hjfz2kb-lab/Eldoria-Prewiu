"""Atelier: specialist-authored compositions, original art and real input evidence.
Only trusted bounded interpreters execute; no generated code runs on the PC."""
import os,sys,pathlib,json,zipfile,hashlib,subprocess,base64,re,datetime as dt
from decimal import Decimal as D
from urllib.parse import urlencode
from PIL import Image,ImageDraw
from concurrent.futures import ThreadPoolExecutor
P=pathlib.Path;HERE=P(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent));import run as v
SOURCE=v.SOURCE;REPO=v.REPO;OWNER=v.OWNER;BRANCH=v.BRANCH
SECTIONS=["Overview","Rankings","Trophies","Discoveries","Heraldry","Monuments","DiscoveryDetail"]
ASSETS=["hero-profile","hero-rankings","hero-trophies","hero-discoveries","hero-heraldry","hero-monuments","collection-ashes","collection-fissure","collection-frontier","medal-sheet","insignia-sheet","monument-sheet","frame-sheet"]
SLOTS={"director":D(".20"),"layout":D(".25"),"asset-review":D(".15"),**{k:D(".20") for k in ASSETS}}
MESSAGE="Launch PR198 atelier with certified zero-spend recovery and supported director"
REQ="tools/studio-v3/rankings-premium/atelier/recovery-launch.json"
def need(x,m):
 if not x:raise ValueError(m)
def gate():
 reg=v.gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main");reg=json.loads(base64.b64decode(v.gh("repos/"+REPO+"/git/blobs/"+reg["sha"])["content"]))
 w=next(x for x in reg["active"] if x["id"]==v.ID)
 need(w["owner"]==OWNER and w["ownership_revision"]==13 and w["ranking_mission"]["atelier"]["prior_estimated_usd"]=="0.61061740","Lease changed")
 need(sum(SLOTS.values())==D("3.20"),"Slot mismatch")
 need(D(".61061740")+sum(SLOTS.values())+D(".35")<=D(5),"Mission cap")
 ev=v.read(os.environ["GITHUB_EVENT_PATH"]);head=os.environ["GITHUB_SHA"];before=ev["before"]
 need(os.environ.get("GITHUB_REPOSITORY")==REPO and os.environ.get("GITHUB_REF")=="refs/heads/"+BRANCH and os.environ.get("GITHUB_RUN_ATTEMPT")=="1" and os.environ.get("GITHUB_EVENT_NAME")=="push","Wrong event")
 need(v.git("rev-parse","HEAD")==head and v.git("rev-parse","HEAD^")==before and v.git("log","-1","--format=%s")==MESSAGE and v.git("diff","--name-status",before,head)=="A\t"+REQ,"Exact one-shot grant only")
 need(P(REQ).read_bytes()==(HERE/"grant.json").read_bytes(),"Grant mismatch")
 need(not v.gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQ,"per_page":1})),"Previously launched")
 need(v.gh("repos/"+REPO+"/git/ref/heads/"+BRANCH)["object"]["sha"]==head and v.gh("repos/"+REPO+"/pulls/198")["head"]["sha"]==SOURCE,"Source changed")
 need(dt.datetime.now(dt.timezone.utc).date()<dt.date(2026,12,2),"Expired")
 ci=v.gh("repos/"+REPO+"/actions/workflows/studio-v3-rankings-atelier.yml/runs?"+urlencode({"head_sha":before,"status":"success","per_page":20}))["workflow_runs"]
 need(any(r["head_sha"]==before and r["conclusion"]=="success" for r in ci),"No calibration proof")
 # The first launch failed in make_payload before reservation or network.
 # Verify immutable artifact and empty ledger before consuming this same grant.
 import tempfile
 with tempfile.TemporaryDirectory() as td:
  path=P(td)/"failed.zip"
  archive(45970152249,15183979348,"14aba4880014a3a10c4b7f906e8d16d730d3eec2b674081524a5b5f91587c2dc",path)
  with zipfile.ZipFile(path) as z:
   need(json.loads(z.read("ledger.json"))["entries"]==[] and not any(n.endswith("-receipt.json") for n in z.namelist()),"Previous API calls or uncertain cost")
 print("ATELIER_GATE_PASS",head,flush=True)
def archive(run,aid,digest,dest,limit=50):
 a=next(x for x in v.gh("repos/"+REPO+"/actions/runs/"+str(run)+"/artifacts")["artifacts"] if x["id"]==aid)
 need(a["digest"]=="sha256:"+digest and not a["expired"],"Wrong artifact")
 with dest.open("xb") as f:subprocess.run(["gh","api","repos/"+REPO+"/actions/artifacts/"+str(aid)+"/zip"],stdout=f,stderr=subprocess.PIPE,check=True,timeout=150)
 need(dest.stat().st_size<=limit*1024*1024 and v.sha(dest.read_bytes())==digest,"ZIP bytes mismatch")
def sheet(paths,dest,cols=3,width=390):
 imgs=[Image.open(p).convert("RGB") for p in paths];height=max(round(i.height*width/i.width) for i in imgs);out=Image.new("RGB",(cols*width,((len(imgs)+cols-1)//cols)*(height+25)),"#17131e");draw=ImageDraw.Draw(out)
 for i,(p,im) in enumerate(zip(paths,imgs)):
  im.thumbnail((width,height));x=(i%cols)*width;y=(i//cols)*(height+25);out.paste(im,(x,y+23));draw.text((x+6,y+3),p.stem,fill="#f1e9d4")
 out.save(dest,quality=90)
def prepare(source,out):
 out.mkdir();base=out/"baseline";base.mkdir();assets=out/"assets";assets.mkdir()
 p=out/"native.zip";archive(45968651560,15183288699,"0b00ca40004f2f8e2a61f14e2b0f1b4bc46658c0a56393e83e754986936637bc",p)
 with zipfile.ZipFile(p) as z:
  proof=json.loads(z.read("unity-proof.json"));need(proof["sourceSha"]==SOURCE and len(proof["shots"])==21,"Native proof")
  for shot in proof["shots"]:
   data=z.read(shot["path"]);need(v.sha(data)==shot["sha256"],"Baseline image changed")
   (base/shot["path"]).write_bytes(data)
  (assets/"medallion.png").write_bytes(z.read("blender/medallion.png"))
  (out/"baseline-proof.json").write_text(json.dumps(proof,indent=2))
 p.unlink();p=out/"input.zip";archive(45967270833,15184075850,"2a4e9525616af0dea53dea4c3bdc1fae9d6447b0913bf140ffe26ec431e05430",p)
 with zipfile.ZipFile(p) as z:
  r=json.loads(z.read("role-proof.json"));ledger=json.loads(z.read("ledger.json"))
  need(r["cumulative_estimated_usd"]=="0.61061740" and all(e["state"]=="completed" and e["estimated_usd"] is not None for e in ledger["entries"]),"Prior cost unknown")
  for n in z.namelist():
   if re.fullmatch(r"assets/[A-Za-z0-9_-]+\.(png|jpg)",n) or re.fullmatch(r"domain/[A-Za-z0-9_-]+\.cs",n) or n in ("Cormorant.ttf","SourceSans.ttf","native.ps1","unity_mcp_probe.py","NativeCapture.cs"):
    dest=out/n;dest.parent.mkdir(exist_ok=True,parents=True);dest.write_bytes(z.read(n))
 p.unlink()
 sheet([base/("phone-"+s+".png") for s in SECTIONS[:3]],out/"baseline-1.jpg")
 sheet([base/("phone-"+s+".png") for s in SECTIONS[3:]],out/"baseline-2.jpg",4,320)
def valid_plan(x):
 need(set(x)=={"concept","palette","pages","briefs"} and isinstance(x["concept"],str) and 20<len(x["concept"])<=2000,"Director structure")
 need(set(x["palette"])=={"ink","surface","gold","paper","muted","action"} and all(re.fullmatch("#[0-9a-fA-F]{6}",c) for c in x["palette"].values()),"Palette")
 need(set(x["briefs"])==set(ASSETS) and all(isinstance(s,str) and 40<=len(s)<=1800 for s in x["briefs"].values()),"Asset briefs")
 need(set(x["pages"])==set(SECTIONS) and all(isinstance(s,str) and 30<=len(s)<=1200 for s in x["pages"].values()),"Page design")
 return x
def valid_layout(x):
 need(set(x)=={"screens"} and len(x["screens"])==7,"Layout scope")
 need([s["section"] for s in x["screens"]]==SECTIONS,"Section order")
 for s in x["screens"]:
  need(set(s)=={"section","heroHeight","rowHeight","columns","iconSize","titleSize","artSide"},"Screen keys")
  for k,a,b in [("heroHeight",96,190),("rowHeight",62,165),("columns",1,2),("iconSize",36,100),("titleSize",24,36)]:
   need(type(s[k]) is int and a<=s[k]<=b,"Dimension "+k)
  need(s["artSide"] in ("left","right"),"Art side")
  if s["section"] in ("Rankings","Discoveries","DiscoveryDetail"):need(s["columns"]==1,"Full-width row")
 return x
# The atlases are decorative individual resources; no static text or baked screen.
SUBJECTS={
"hero-profile":"A magnificent Valoria fortress overlooking layered mountain ranges, sunlit warm gold and lavender mist, panoramic architectural focal point",
"hero-rankings":"Royal archive interior, monumental stone ledger dais and golden heraldic scale, shafts of daylight, no text",
"hero-trophies":"Chamber of prestigious earned relics, illuminated individual gold and silver cups and shields on pedestals",
"hero-discoveries":"Explorer's archive, open map, compass, weathered scrolls and distant mountain frontier, golden natural light",
"hero-heraldry":"Grand noble hall with hanging dark purple and cream heraldic fabric and aged gold metalwork",
"hero-monuments":"Sunlit Valoria stone courtyard with sculptural pedestals and architectural arches, monumental scale, no recognizable specific locked monument",
"collection-ashes":"Charred map and a small ember-lit ruined stone arch, archaeological still life",
"collection-fissure":"Geological fracture with amethyst crystal veins, field journal and luminous minerals, not a monumental building",
"collection-frontier":"Weathered frontier map with brass compass and watchtower on a misty ridge",
"medal-sheet":"Precisely nine equally spaced isolated collectible medals on transparent background, three by three, row-major: simple bronze sunrise fortress, silver frontier tower, gold expanding castle, ornate founding crown, silver watch shield, gilded sentinel lantern, hunter tusk, explorer compass, gilded discovery scroll. Distinct silhouettes and materials. No text",
"insignia-sheet":"Precisely nine isolated medieval heraldic insignias on transparent background in three by three equal cells, row-major: pioneer compass, watchman lantern, guardian shield, knight sword, warden fortress key, chronicler book, champion laurel, marshal crossed banners, legendary amethyst crown. Distinct silhouettes, generous padding, no text",
"monument-sheet":"Precisely nine isolated miniature sculptural game assets on transparent background in three by three equal cells: founder statue, stone fountain, antler hunting trophy, amethyst fissure memorial, ash brazier, folded purple banner, cream embroidered banner, gilded chronicle book, generic sealed unmarked dark medallion. Generous padding, no lettering",
"frame-sheet":"Exactly four horizontal ornamental game UI material samples in 2 by 2 grid on transparent background: aged engraved gold narrow corner frame on deep blue-black enamel, carved warm bronze panel with restrained corners, ivory parchment tablet with antique edging, narrow decorative gold divider. Each separate rectangle in own cell, clean empty center; no labels or full UI"
}
def main(source,out,live):
 source=P(source);out=P(out);prepare(source,out)
 # Prior verified source projection and exact UI rules; no source game edits.
 common=(HERE.parent/"BRIEF.md").read_text()+"\n"+(source/"contracts/ART_DIRECTION.md").read_text()+"\n"+(source/"contracts/SHARED_STANDARD.md").read_text()+"\n"+(source/"domain/ProfilePresentation.cs").read_text()[:8500]
 if not live:
  need(len(SUBJECTS)==len(ASSETS) and set(SUBJECTS)==set(ASSETS),"Asset contract")
  print("ATELIER_FIXTURE_PASS",flush=True);return
 gate();key=os.environ.get("OPENAI_API_KEY");need(bool(key),"No key")
 v.SLOTS=SLOTS;l=v.Ledger(out)
 def ask(slot,model,instructions,content,images=(),maxout=4500):
  file=out/(slot+"-input.txt");file.write_text(content.encode("utf-8")[:22000].decode("utf-8",errors="ignore"),encoding="utf-8")
  pl,m=v.studio.make_payload("visual" if images else "programming",model,file,images);pl["instructions"]=instructions+" Return strict JSON only.";pl["max_output_tokens"]=maxout
  for c in pl["input"][0]["content"]:
   if c["type"]=="input_image":c["detail"]="high"
  row=l.reserve(slot);print("ATELIER_SLOT",slot,flush=True);raw=v.studio.request_api(pl,key);l.finish(row,raw.get("usage"),raw.get("model") or model,raw.get("service_tier") or "standard")
  rep=v.studio.report("visual" if images else "programming",pl,m,raw);v.dump(out/(slot+"-receipt.json"),rep);res=v.json_result(rep["response"]);v.dump(out/(slot+"-output.json"),res);return res
 d=valid_plan(ask("director","gpt-6.2-sol","You are the senior commercial mobile game art director. The previous native captures are inadequate: weak contrast, generic identical rectangles, tiny numbers, repeated medals, wasted dark hero space. Design an ambitious new Valoria heritage gallery, with rich painterly original artwork, strong hierarchy, readable labels, differentiated sections, tactile warm metals and amethyst used with restraint, not dark purple everywhere. Preserve all domain facts/locks; no fictional rankings, players, rewards or new mechanics. No full-screen image or text in artwork. Provide JSON exactly {concept,palette:{ink,surface,gold,paper,muted,action},pages:{Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments,DiscoveryDetail},briefs:{each of the 13 named assets}}. Palette hex. Each page is a precise composition and each asset a distinct high-quality art brief; each brief 40..1800 chars. Maintain subjects/atlas cell order. Use the rejected actual Unity frames as evidence.",common+"\nExact art slots and required subjects: "+json.dumps(SUBJECTS,ensure_ascii=False),(out/"baseline-1.jpg",out/"baseline-2.jpg"),6500))
 for slot in ASSETS:
  # Each call is reserved and durably recorded before sending. No retry on uncertain costs.
  row=l.reserve(slot);print("ATELIER_SLOT",slot,flush=True)
  prompt="Original isolated modular art asset for commercial Valoria mobile fantasy strategy UI. Painterly highly detailed, nuanced natural materials, strong focal hierarchy, restrained ornamental craftsmanship, rich warm light and deep spatial depth. NO text, numbers, lettering, GUI mockup, buttons or invented game mechanics. Required subject: "+SUBJECTS[slot]+". Art director guidance: "+d["briefs"][slot]
  pl=v.image.make_payload(prompt);pl["quality"]="high"
  if slot.endswith("-sheet"):pl["background"]="transparent"
  raw=v.image.generate(pl,key);l.finish(row,raw.get("usage"),v.image.MODEL)
  bits,meta=v.image.unpack(raw);f=out/"assets"/(slot+".png");f.write_bytes(bits)
  with Image.open(f) as im:im.verify()
  meta.update(estimated_usd=row["estimated_usd"],quality="high",prompt=pl["prompt"]);v.dump(out/(slot+"-receipt.json"),meta)
  # Keep original asset; derive individual separate sprites for native Unity.
  if slot.endswith("-sheet"):
   keys={"medal-sheet":["medal-b2","medal-b3","medal-b4","medal-b5","medal-v1","medal-v5","medal-hunt","medal-scout","medal-discovery"],"insignia-sheet":["insignia-pioneer","insignia-watchman","insignia-guardian","insignia-knight","insignia-warden","insignia-chronicler","insignia-champion","insignia-marshal","insignia-legend"],"monument-sheet":["statue-founder","statue-guard","statue-hunt","statue-fissure","statue-ash","banner-purple","banner-cream","folio","sealed"],"frame-sheet":["frame-royal","frame-bronze","frame-parchment","frame-divider"]}[slot]
   n=2 if slot=="frame-sheet" else 3
   with Image.open(f) as im:
    im=im.convert("RGBA");w,h=im.size
    for i,k in enumerate(keys):
     x,y=i%n,i//n;im.crop((round(x*w/n),round(y*h/n),round((x+1)*w/n),round((y+1)*h/n))).save(out/"assets"/(k+".png"))
  else:
   with Image.open(f) as im:
    im.convert("RGB").save(out/"assets"/(slot+".jpg"),quality=94)
 sheet([out/"assets"/(s+".png") for s in ASSETS[:6]],out/"art-heroes.jpg",3,320)
 sheet([out/"assets"/(s+".png") for s in ASSETS[9:]],out/"art-objects.jpg",4,280)
 a=ask("asset-review","gpt-6.1-terra","Independent art specialist: inspect original painted asset sheets. Identify real strengths and defects, check atlas subjects distinct, no text or invented UI, style consistent with Valoria. Return JSON {findings:[...],reject_assets:[exact asset IDs],recommendations:[...]}. Do not claim approval of screens.",json.dumps(d,ensure_ascii=False),(out/"art-heroes.jpg",out/"art-objects.jpg"))
 need(set(a)=={"findings","reject_assets","recommendations"} and isinstance(a["reject_assets"],list) and set(a["reject_assets"])<=set(ASSETS),"Asset QA schema")
 layout=valid_layout(ask("layout","gpt-6.2-sol","Senior Unity technical artist: determine native screen composition dimensions based on real assets and art direction, not repeated identical lists. JSON exactly {screens:[7 objects in order Overview,Rankings,Trophies,Discoveries,Heraldry,Monuments,DiscoveryDetail]}. Each object exactly section,heroHeight 96..190, rowHeight 62..165, columns 1|2 (Rankings, Discoveries and detail 1), iconSize 36..100,titleSize 24..36,artSide left|right. Overview prominent identity and numeric hierarchy, trophy grids, heraldry differentiated insignias, monuments sculptural showcase, three discovery dossier cards. Favor generous legible text and reduce unused hero space. All data remains native live C#.",json.dumps({"director":d,"asset_review":a},ensure_ascii=False),(out/"art-heroes.jpg",out/"art-objects.jpg")))
 for s in layout["screens"]:s["hero"]="hero-"+{"Overview":"profile","Rankings":"rankings","Trophies":"trophies","Discoveries":"discoveries","Heraldry":"heraldry","Monuments":"monuments","DiscoveryDetail":"discoveries"}[s["section"]]
 layout.update(concept=d["concept"],metal=[.64,.35,.10],enamel=[.035,.022,.065],gem=[.38,.08,.61],points=12,bevel=.055)
 v.dump(out/"native-design.json",layout);v.dump(out/"palette.json",d["palette"])
 cost=sum(D(e["estimated_usd"]) for e in l.rows)
 proof={"source_sha":SOURCE,"live":True,"api_calls":len(l.rows),"estimated_usd":str(cost),"cumulative_estimated_usd":str(D(".61061740")+cost),"asset_count":len(ASSETS),"asset_review":a,"billing_verified":False,"art_approved":False}
 v.dump(out/"atelier-proof.json",proof);print("ATELIER_PRODUCTION_PROOF "+json.dumps(proof,ensure_ascii=False),flush=True)
if __name__=="__main__":
 if sys.argv[1:]==["--gate"]:gate()
 else:
  need(len(sys.argv)==4 and sys.argv[1] in ("--fixture","--live"),"Mode");main(sys.argv[2],sys.argv[3],sys.argv[1]=="--live")
