"""Eldoria: deterministic 9-slice gold frame for real hero card controls.
Decorative only. Does not encode hero identity, gameplay values or navigation.
"""
from PIL import Image, ImageDraw, ImageFilter
from pathlib import Path
import math, random, hashlib
S=2;W=512;H=672
sz=(W*S,H*S)
def P(x):return int(round(x*S))
def coords(r):return tuple(P(v) for v in r)
gold=(206,145,61,255); bright=(253,220,124,235); dark=(56,37,23,230)
# Transparent central viewport. Faint charcoal wall shadows at edges make portraits fit physically.
im=Image.new('RGBA',sz,(0,0,0,0)); d=ImageDraw.Draw(im,'RGBA')
# Stone depth shadows; keep center transparent
for i in range(55,0,-1):
    a=int(78*(1-i/56)**1.65)
    r=coords((i,i,W-i,H-i))
    d.rounded_rectangle(r,radius=P(21),outline=(4,11,22,a),width=P(2))
# Layered metallic rim, top/bottom with warm highlights
for inset, color, w in [(0,(28,22,23,246),12),(7,(65,43,31,238),8),(12,(156,94,42,238),5),(16,(241,190,89,251),3),(20,(89,53,27,200),5),(25,(253,213,126,170),2)]:
    d.rounded_rectangle(coords((inset,inset,W-inset,H-inset)), radius=P(22), outline=color,width=P(w))
# Ornamental corner brackets; visual volume from engravings rather than simple gold lines.
for fx in [False, True]:
 for fy in [False, True]:
  def mappt(x,y):return (W-x if fx else x,H-y if fy else y)
  def ln(points,fill,width):d.line([coords((u,v)) for u,v in [mappt(*p) for p in points]],fill=fill,width=P(width),joint='curve')
  # forged gold corner sweep with curved central arc, dark shadow inset
  for inset,thick,col in [(0,12,(45,27,20,230)),(3,6,(187,113,42,230)),(6,3,(255,207,112,220))]:
   q=[(27+inset,109),(27+inset,66),(38+inset,42),(58+inset,27),(110,27+inset)]
   ln(q,col,thick)
  # three silver/bronze inset leaf engravings meeting at eye of card
  ln([(45,79),(61,63),(79,45),(111,45)],(180,114,49,175),4)
  ln([(45,79),(55,89),(80,102)],(209,156,73,205),3)
  ln([(80,102),(68,88),(73,73)],(251,198,110,170),2)
  for xx,yy,r in [(36,36,7),(57,56,4),(91,48,3),(48,91,3)]:
   x,y=mappt(xx,yy)
   d.ellipse(coords((x-r,y-r,x+r,y+r)),fill=(254,203,100,190),outline=(88,52,31,220),width=P(2))
  # engraved metal droplets, sparkling strokes
  ln([(42,36),(65,36),(85,47)],(241,198,105,180),2)
  ln([(36,42),(36,65),(47,85)],(241,198,105,180),2)
# Mid-side brass rivets and tiny jewel notches, individually visible at real screen sizes.
for y in [H*.25,H*.50,H*.75]:
 for x in [18,W-18]:
  d.ellipse(coords((x-7,y-7,x+7,y+7)),fill=(34,24,21,255),outline=(224,169,79,230),width=P(3))
  d.ellipse(coords((x-3,y-4,x+3,y+2)),fill=(108,157,168,206))
for x in [W*.24,W*.5,W*.76]:
 for y in [18,H-18]:
  d.ellipse(coords((x-6,y-6,x+6,y+6)),fill=(35,32,28,255),outline=(206,147,64,215),width=P(2))
  d.ellipse(coords((x-2,y-2,x+2,y+2)),fill=(244,196,113,235))
# Add slow gradient warm light along upper corners (decorative only).
glow=Image.new('RGBA',sz,(0,0,0,0)); g=ImageDraw.Draw(glow,'RGBA')
for x in [80,W-80]:
 for y in [62,H-62]:
  g.ellipse(coords((x-27,y-27,x+27,y+27)),fill=(238,162,63,95))
glow=glow.filter(ImageFilter.GaussianBlur(P(22)))
im=Image.alpha_composite(glow,im)
im.resize((W,H),Image.Resampling.LANCZOS).save('/mnt/data/eldoria-new-art/HeroCardNineSliceGold.png',optimize=True)
a=Path('/mnt/data/eldoria-new-art/HeroCardNineSliceGold.png').read_bytes()
print('file_bytes',len(a),'sha256',hashlib.sha256(a).hexdigest())