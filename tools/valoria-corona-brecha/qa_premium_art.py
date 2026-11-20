#!/usr/bin/env python3
"""Static-only original art QA, not a Unity PlayMode or graphical approval."""
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageFont
import json,hashlib,re,sys
ROOT=Path(__file__).resolve().parents[2]
folder=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrechaPremium"
manifest=json.loads((ROOT/"docs/valoria-modular-25d/CORONA_BRECHA_PREMIUM_SPRITES_PROVENANCE.json").read_text())
assert manifest["schema"]==2 and not manifest["unityRuntimeVerified"] and not manifest["artApproved"]
items=manifest["sprites"]
assert len(items)==15 and len({x["name"] for x in items})==15
assert len({x["guid"] for x in items})==15
out=ROOT/"artifacts/corona-premium";out.mkdir(parents=True,exist_ok=True)
width=1200;cellw=300;cellh=335
im=Image.new("RGB",(width,120+cellh*4),"#1D162C")
d=ImageDraw.Draw(im)
try:
 title=ImageFont.truetype("DejaVuSans-Bold.ttf",23)
 body=ImageFont.truetype("DejaVuSans.ttf",16)
except OSError:
 title=body=ImageFont.load_default()
d.text((24,20),"CORONA DE LA BRECHA · SPRITES PREMIUM ORIGINALES",fill="#F7D399",font=title)
d.text((24,55),"Cotejo de píxeles de repositorio; NO es captura Unity ni certificación de arte",fill="#C9B3D7",font=body)
for n,x in enumerate(items):
 p=folder/(x["name"]+".png")
 assert p.is_file(),p
 raw=p.read_bytes()
 assert hashlib.sha256(raw).hexdigest()==x["sha256"],"sha256:"+str(p)
 assert hashlib.sha1(("blob "+str(len(raw))+"\\0").encode().replace(b"\\0",b"\x00")+raw).hexdigest()==x["blob"],"Git blob hash:"+str(p)
 assert len(raw)==x["bytes"]
 with Image.open(p) as orig:
  orig.load()
  assert orig.format=="PNG" and orig.mode=="RGBA",(p,orig.mode,orig.format)
  assert orig.size==(x["width"],x["height"]),(p,orig.size)
  assert orig.getpixel((0,0))[3]<50,"top-left transparency:"+str(p)
  assert orig.getbbox() is not None,"asset is empty:"+str(p)
  if n<12:pass
  thumbnail=ImageOps.contain(orig,(cellw-28,cellh-80),Image.Resampling.LANCZOS)
 meta=(folder/(x["name"]+".png.meta")).read_text()
 assert "guid: "+x["guid"] in meta
 assert "spritePixelsToUnits: 200" in meta and "textureType: 8" in meta
 assert "spriteBorder: {x: "+str(x["border"]) in meta
 assert x["border"]*2<min(x["width"],x["height"]),"invalid 9slice border"
 col=n%4;row=n//4;sx=col*cellw;sy=110+row*cellh
 d.rounded_rectangle((sx+8,sy,sx+cellw-8,sy+cellh-9),radius=13,fill="#30243F",outline="#846C97",width=2)
 mask=thumbnail.getchannel("A")
 im.paste(thumbnail,(sx+(cellw-thumbnail.width)//2,sy+19+(cellh-83-thumbnail.height)//2),mask)
 d.text((sx+24,sy+cellh-45),x["name"],font=body,fill="#F6DFC0")
 print("STATIC_CORONA_PREMIUM_ASSET_PASS",x["name"],x["sha256"])
im.save(out/"corona-premium-15-source-evidence.png")
report={"status":"STATIC_SOURCE_ONLY_PASS_NOT_UNITY_QA","sprites_verified":len(items),"original_painted_bytes_verified":True,"gameplay_changed":False,"art_approved":False,"physical_iphone_tested":False,"source_git_sha":None}
(out/"evidence.json").write_text(json.dumps(report,indent=2)+"\n")
print("CORONA_PREMIUM_15_ASSETS_SOURCE_INTEGRITY_PASS; UNITY_NOT_TESTED")
