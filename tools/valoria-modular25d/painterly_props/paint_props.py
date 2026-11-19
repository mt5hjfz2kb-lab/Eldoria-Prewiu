#!/usr/bin/env python3
"""Original RGBA foliage, pebbles and meadow tufts for Valoria Modular25D.

Brush-authored deterministic sprite source: same warm northwest illumination,
olive/stone colour family and transparent ground pivots as the settlement.
No copied game art and no full-scene background.
"""
from pathlib import Path
import hashlib, json, math, random, sys
from PIL import Image, ImageDraw, ImageFilter

OUT=Path(sys.argv[1] if len(sys.argv)>1 else "/tmp/eldoria-painted-props")
OUT.mkdir(parents=True,exist_ok=True)
S=3

def col(v,a=255):
    h=v.lstrip("#")
    return tuple(int(h[i:i+2],16) for i in (0,2,4))+(a,)
def coords(points):return [(round(x*S),round(y*S)) for x,y in points]
def bxy(points):return tuple(round(v*S) for v in points)
def layer(w,h):return Image.new("RGBA",(w*S,h*S),(0,0,0,0))
def finish(image,name):
    result=image.resize((image.width//S,image.height//S),Image.Resampling.LANCZOS)
    result.save(OUT/name,optimize=True)
def shadow(image,box,tint=(21,37,28,72),blur=4):
    back=Image.new("RGBA",image.size,(0,0,0,0))
    ImageDraw.Draw(back).ellipse(bxy(box),fill=tint)
    image.alpha_composite(back.filter(ImageFilter.GaussianBlur(blur*S)))
def stipple(image,mask,rng,n,palette):
    noise=Image.new("RGBA",image.size,(0,0,0,0))
    d=ImageDraw.Draw(noise)
    for _ in range(n):
        x=rng.randint(0,image.width//S-1);y=rng.randint(0,image.height//S-1)
        r=rng.randint(1,2)
        d.ellipse(bxy((x-r,y-r*.4,x+r*.65,y+r*.8)),fill=rng.choice(palette))
    noise.putalpha(Image.composite(noise.getchannel("A"),Image.new("L",mask.size,0),mask))
    image.alpha_composite(noise)

def rock():
    rng=random.Random(607)
    im=layer(140,109)
    shadow(im,(23,73,128,93),(20,30,21,95),3)
    silhouette=[(14,72),(15,52),(37,30),(70,23),(102,28),(125,47),(128,66),(107,82),(65,87),(35,82)]
    mask=Image.new("L",im.size,0)
    ImageDraw.Draw(mask).polygon(coords(silhouette),fill=255)
    d=ImageDraw.Draw(im)
    layers=[
      (silhouette,"5c6962"),
      ([(14,52),(37,30),(70,23),(102,28),(125,47),(90,57),(55,62),(23,62)],"a4a89c"),
      ([(14,72),(14,52),(23,62),(55,62),(65,87),(35,82)],"858d81"),
      ([(55,62),(90,57),(125,47),(128,66),(107,82),(65,87)],"697872"),
      ([(14,52),(37,30),(70,23),(56,36),(23,62)],"b8b5a3"),
      ([(90,57),(102,28),(125,47)],"9da296")
    ]
    for polygon,colour in layers:d.polygon(coords(polygon),fill=col(colour))
    d.line(coords([(37,30),(56,36),(90,57)]),fill=col("e1d8bd",200),width=2*S,joint="curve")
    d.line(coords([(23,62),(55,62),(65,87)]),fill=col("46584e",135),width=3*S)
    d.line(coords([(90,57),(107,82)]),fill=col("3d4f49",140),width=2*S)
    for _ in range(8):
        x=rng.choice([23,35,52,85,111])+rng.randint(-4,4)
        y=rng.choice([45,52,60,76]);r=rng.randint(4,10)
        d.ellipse(bxy((x-r,y-r*.32,x+r,y+r*.25)),
          fill=col(rng.choice(["536b48","6d794d","829057"]),120))
    stipple(im,mask,rng,1800,[(236,220,183,32),(26,40,33,28),(56,78,62,35),(110,115,85,38)])
    for x in (23,39,111,122):
        for _ in range(3):
            dx=rng.uniform(-3,3);dy=rng.uniform(0,7)
            d.line(coords([(x,80),(x+dx,80-dy)]),fill=col("688054",190),width=S)
    finish(im,"stone.png")

BUSH=["45623f","4e6a43","587548","678052","748850","80925c","98a46b"]
def bush(variant):
    rng=random.Random(103+variant*63)
    im=layer(148,112);shadow(im,(26,82,124,102),(19,36,25,72),4)
    d=ImageDraw.Draw(im)
    d.line(coords([(73,88),(73,63)]),fill=col("574734",230),width=5*S)
    groups=[]
    for j in range(24):
        angle=2*math.pi*j/24+rng.uniform(-.15,.15)
        radius=.35+.65*math.sqrt(rng.random())
        x=74+math.cos(angle)*48*radius
        y=62+math.sin(angle)*23*radius-rng.uniform(0,9)
        groups.append((x,y,rng.uniform(14,23)))
    for x,y,rr in groups:
        d.ellipse(bxy((x-rr*1.1,y-rr*.68,x+rr*1.1,y+rr*.69)),
          fill=col(rng.choice(BUSH[:4]),235))
        d.ellipse(bxy((x-rr*.85,y-rr*.76,x+rr*.75,y+rr*.11)),
          fill=col(rng.choice(BUSH[3:]),130))
        for _ in range(14):
            theta=rng.random()*2*math.pi
            r=rr*math.sqrt(rng.random())*.78
            cx=x+math.cos(theta)*r;cy=y+math.sin(theta)*r*.65
            ww=rng.uniform(2,5.2);hh=rng.uniform(1,3)
            light=col(rng.choice(BUSH[4:]),rng.randint(130,220))
            dark=col(rng.choice(BUSH[:3]),110)
            d.polygon(coords([(cx-ww,cy),(cx+.3,cy-hh),(cx+ww,cy),(cx,cy+hh)]),
             fill=light if cy<y else dark)
    for _ in range(30):
        x=rng.uniform(30,116);y=rng.uniform(66,84)
        d.line(coords([(x,y+4),(x+rng.uniform(-4,4),y-rng.uniform(2,9))]),
          fill=col(rng.choice(["637d4d","82935c","9aa063"]),rng.randint(120,235)),width=S)
    finish(im,"bush_"+str(variant)+".png")

def tuft(variant):
    rng=random.Random(402+variant*43)
    im=layer(102,76);shadow(im,(24,52,79,70),(29,44,27,48),4)
    d=ImageDraw.Draw(im)
    for _ in range(100):
        x=rng.uniform(15,86);y=rng.uniform(45,65)
        ht=rng.uniform(4,17);lean=rng.uniform(-7,7)
        d.line(coords([(x,y),(x+lean,y-ht)]),
          fill=col(rng.choice(["3f623f","59794b","6d8850","849452","9fa66a"]),
            rng.randint(100,230)),width=rng.choice([1,2])*S)
    for _ in range(7 if variant==0 else 12):
        x=rng.uniform(24,84);y=rng.uniform(30,54)
        d.line(coords([(x,y+8),(x,y)]),fill=col("526442",180),width=S)
        colour=col(rng.choice(["ddd3a7","d9c88d","c7bb87","d0bc71"]),rng.randint(160,240))
        for j in range(4):
            theta=j*math.pi*.5
            dx,dy=math.cos(theta)*2.3,math.sin(theta)*2.3
            d.ellipse(bxy((x+dx-1.3,y+dy-1,x+dx+1.3,y+dy+1)),fill=colour)
        d.ellipse(bxy((x-.9,y-.9,x+.9,y+.9)),fill=col("a88d42",210))
    finish(im,"tuft_"+str(variant)+".png")

for function in [rock,lambda:bush(0),lambda:bush(1),lambda:bush(2),
                 lambda:tuft(0),lambda:tuft(1)]:function()
# All previous GUIDs/pivots are retained when these existing images are
# overwritten. New tuft sprites need their own stable importer GUID.
manifest=OUT/"sprite_manifest.json"
if manifest.exists():
    data=json.loads(manifest.read_text(encoding="utf-8"))
    entries={item["name"]:item for item in data.get("sprites",[])}
    for path in sorted(OUT.glob("*.png")):
        from PIL import Image
        name=path.stem;im=Image.open(path)
        entries[name]={
            "name":name,"size":list(im.size),
            "foot_pivot": entries.get(name,{}).get("foot_pivot",[.5,.18]),
            "sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
            "bytes":path.stat().st_size,
            "source":"original deterministic painted 2.5D foliage"
        }
    data["sprites"]=[entries[name] for name in sorted(entries)]
    data["paintedGroundProps"]=True
    manifest.write_text(json.dumps(data,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
print("PAINTERLY_GROUND_PROPS_PASS "+str([(p.name,p.stat().st_size) for p in OUT.glob("*.png")]))
