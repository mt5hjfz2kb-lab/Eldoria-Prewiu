import sys,pathlib,json,zipfile,hashlib,shutil,collections
from decimal import Decimal as D
from PIL import Image,ImageDraw
P=pathlib.Path;HERE=P(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent));import run as v
from produce import archive,ASSETS,valid_layout
SRC=v.SOURCE
def need(x,m):
 if not x:raise ValueError(m)
def sha(x):return hashlib.sha256(x).hexdigest()
def trim_alpha(p):
 im=Image.open(p).convert("RGBA");a=im.getchannel("A");b=a.point(lambda x:255 if x>32 else 0).getbbox()
 if not b:raise ValueError("empty alpha "+str(p))
 x,y,X,Y=b;pad=8
 im.crop((max(0,x-pad),max(0,y-pad),min(im.width,X+pad),min(im.height,Y+pad))).save(p)
def remove_white(p):
 # Only exterior-connected near-white pixels; preserve pale highlights within the relic.
 im=Image.open(p).convert("RGBA");im.thumbnail((768,768));w,h=im.size;px=im.load();visited=bytearray(w*h);q=collections.deque()
 def add(x,y):
  n=y*w+x
  if visited[n]:return
  visited[n]=1;r,g,b,a=px[x,y]
  if min(r,g,b)>=228 and max(r,g,b)-min(r,g,b)<=32:q.append((x,y))
 for x in range(w):add(x,0);add(x,h-1)
 for y in range(h):add(0,y);add(w-1,y)
 count=0
 while q:
  x,y=q.popleft();px[x,y]=(0,0,0,0);count+=1
  for X,Y in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
   if 0<=X<w and 0<=Y<h:add(X,Y)
 need(count>w*h*.04 and count<w*h*.75,"unexpected white background mask")
 im.save(p);trim_alpha(p);return count
def main(out):
 out.mkdir();p=out/"verified.zip";archive(45970517186,15184847422,"5e89318bf3805fc57c55d917682c82682ffb41f286ddfc7e78440e4ed4ec1c93",p,60)
 with zipfile.ZipFile(p) as z:
  proof=json.loads(z.read("atelier-proof.json"));ledger=json.loads(z.read("ledger.json"))
  need(proof["source_sha"]==SRC and proof["asset_count"]==13 and len(ledger["entries"])==16 and all(e["state"]=="completed" for e in ledger["entries"]),"proof")
  need(sum(D(e["estimated_usd"]) for e in ledger["entries"])==D(".77454275") and proof["cumulative_estimated_usd"]=="1.38516015","cost")
  for k in ASSETS:
   receipt=json.loads(z.read(k+"-receipt.json"))
   need(sha(z.read("assets/"+k+".png"))==receipt["image_sha256"],"image receipt "+k)
  for n in z.namelist():
   import re
   if re.fullmatch(r"assets/[A-Za-z0-9_-]+\.(png|jpg)",n) or re.fullmatch(r"domain/[A-Za-z0-9_-]+\.cs",n) or n in ("Cormorant.ttf","SourceSans.ttf","native-design.json","palette.json","atelier-proof.json","ledger.json","baseline-proof.json","baseline-1.jpg","baseline-2.jpg","director-output.json","asset-review-output.json"):
    dst=out/n;dst.parent.mkdir(exist_ok=True,parents=True);dst.write_bytes(z.read(n))
 p.unlink()
 assets=out/"assets"
 # Reject the frame atlas and anonymous laurel seal. Use only distinct approved cells.
 for k in ["frame-sheet","frame-royal","frame-bronze","frame-parchment","frame-divider","monument-sheet","sealed"]:
  f=assets/(k+".png")
  if f.exists():f.unlink()
 # Correct rejected hero crops: archive book and balance only, interior hall, anonymous upper arcade.
 boxes={"hero-rankings":(45,70,980,555),"hero-heraldry":(125,240,900,775),"hero-monuments":(0,0,1024,490)}
 for k,box in boxes.items():
  f=assets/(k+".png");im=Image.open(f).convert("RGB").crop(box);im.save(f)
  im.save(assets/(k+".jpg"),quality=95)
 # Preserve isolated objects as true alpha sprites instead of white-backed screenshots.
 masked={k:remove_white(assets/(k+".png")) for k in ["collection-ashes","collection-fissure"]}
 for p in assets.glob("*.png"):
  if p.stem.startswith(("medal-","insignia-","statue-","banner-")) and not p.stem.endswith("-sheet"):
   if Image.open(p).mode=="RGBA":trim_alpha(p)
 # Avoid Unity asset basename conflicts between PNG/JPEG sources.
 for p in assets.glob("*.jpg"):
  if p.with_suffix(".png").exists():p.unlink()
 for k in ["medal-sheet","insignia-sheet"]:
  (assets/(k+".png")).unlink()
 for p in assets.glob("*"):
  with Image.open(p) as im:im.verify()
 # Trusted native code is supplied from current pinned commit, not the model output.
 for n in ("NativeProfile.cs","native.ps1","NativeCapture.cs","unity_mcp_probe.py"):
  shutil.copy2(HERE/n,out/n)
 evidence={"source_sha":SRC,"production_run":45970517186,"prior_cost_usd":"1.38516015","asset_receipts_verified":13,"corrected_hero_crops":boxes,"white_background_removed_pixels":masked,"rejected_frames_omitted":True,"generic_locked_symbol":"unknown","art_approved":False}
 (out/"native-input-proof.json").write_text(json.dumps(evidence,indent=2))
 print("ATELIER_NATIVE_INPUT_PROOF "+json.dumps(evidence),flush=True)
if __name__=="__main__":main(P(sys.argv[1]))
