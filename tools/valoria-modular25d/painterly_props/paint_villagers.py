#!/usr/bin/env python3
"""Painted small medieval workers in two gait poses (not 3D grey mannequins).

Purpose: the previous Blender-generated figures read as stone blocks next to
image-authored buildings. This vector-brush source produces a reproducible,
painterly miniature at the same warm key/cool shadow and with an actual foot
pivot; the two frames differ in step, arm position and cloth movement.
"""
from PIL import Image, ImageDraw, ImageFilter
from pathlib import Path
import math, random, json, hashlib, re, sys
root=Path(sys.argv[1])
root.mkdir(parents=True,exist_ok=True)
scale=4
W,H=114,155
ink=(37,40,38,255)
def c(r,g,b,a=255):return (r,g,b,a)
def p(x,y):return round(x*scale),round(y*scale)
def box(b):return tuple(round(x*scale) for x in b)
def poly(draw,pts,color):draw.polygon([p(*pt) for pt in pts],fill=color)
def line(draw,pts,color,w):draw.line([p(*pt) for pt in pts],fill=color,width=int(w*scale),joint="curve")
def paint(step):
    i=Image.new("RGBA",(W*scale,H*scale),(0,0,0,0))
    d=ImageDraw.Draw(i)
    # The footprint is below the character, not a separate green disc.
    sh=Image.new("RGBA",i.size,(0,0,0,0));ds=ImageDraw.Draw(sh)
    ds.ellipse(box((29,135,92,149)),fill=c(25,37,26,64))
    i.alpha_composite(sh.filter(ImageFilter.GaussianBlur(4*scale)))
    d=ImageDraw.Draw(i)
    left=-8 if step==0 else 3
    right=5 if step==0 else -5
    # Leather boots and cloth legs: natural stagger, no T-pose or cubes.
    poly(d,[(42,107),(55,110),(53+left,137),(40+left,140),(36+left,133)],c(62,50,43))
    poly(d,[(59,107),(71,108),(69+right,137),(57+right,140),(54+right,128)],c(83,62,45))
    poly(d,[(40+left,129),(53+left,129),(57+left,142),(49+left,145),(31+left,143)],c(54,43,38))
    poly(d,[(57+right,129),(70+right,130),(75+right,142),(63+right,144),(52+right,143)],c(64,47,39))
    line(d,[(41+left,131),(42+left,142),(54+left,142)],c(131,90,58),1)
    line(d,[(60+right,130),(61+right,142),(71+right,141)],c(143,96,57),1)
    # Upper torso: muted navy-gray leather surcoat, wool shirt sleeves.
    poly(d,[(41,55),(76,53),(83,82),(73,117),(38,114),(32,82)],c(60,67,69))
    poly(d,[(45,57),(56,53),(48,113),(38,110),(35,85)],c(99,99,91))
    poly(d,[(60,55),(76,53),(78,91),(69,116),(59,112)],c(51,59,61))
    poly(d,[(30,63),(41,56),(46,72),(37,98),(29,96),(22,82)],c(185,164,127))
    poly(d,[(76,56),(86,68),(91,87),(80,97),(72,80)],c(162,145,120))
    # Left gloved hand at slightly different angle in gait.
    dx=-3 if step==0 else 3
    d.ellipse(box((24+dx,94,34+dx,106)),fill=c(143,105,71))
    d.ellipse(box((79-dx,91,90-dx,103)),fill=c(133,102,73))
    # Leather apron panel, belt and stitched diagonals.
    poly(d,[(45,69),(62,65),(71,88),(65,110),(43,112),(37,90)],c(63,74,78))
    line(d,[(37,87),(69,85)],c(96,67,42),7)
    line(d,[(43,83),(66,88)],c(174,128,72),2)
    line(d,[(39,102),(51,110),(63,105)],c(144,119,91,160),1)
    poly(d,[(39,64),(42,61),(75,106),(72,108)],c(102,71,53))
    line(d,[(42,61),(74,106)],c(156,111,69),2)
    # Satchel has leather flap and a readable buckle even at mobile scale.
    poly(d,[(71,91),(89,89),(90,113),(78,119),(68,108)],c(78,52,38))
    poly(d,[(71,93),(89,90),(86,100),(71,102)],c(146,94,54))
    line(d,[(78,96),(80,100)],c(216,172,89),1.4)
    # Adult face with shadowed right cheek, nose, hair, short beard.
    d.ellipse(box((45,25,74,61)),fill=c(189,139,100))
    poly(d,[(45,35),(60,27),(74,32),(73,56),(63,64),(49,54)],c(206,161,120))
    poly(d,[(62,28),(73,32),(73,55),(64,62),(61,52)],c(166,118,89))
    poly(d,[(47,44),(51,50),(54,53),(62,51),(68,54),(70,61),(63,65),(50,61)],c(83,63,49))
    d.ellipse(box((52,39,54,42)),fill=ink)
    d.ellipse(box((65,38,67,41)),fill=ink)
    line(d,[(61,41),(60,49),(65,50)],c(141,100,72),1)
    line(d,[(52,53),(58,52),(63,53)],c(109,70,50),1)
    # Soft felt workman's cap with rounded crown (not a square helmet).
    d.ellipse(box((41,16,78,39)),fill=c(55,58,61))
    poly(d,[(40,31),(77,31),(81,38),(66,42),(40,39)],c(46,53,55))
    line(d,[(44,28),(59,20),(72,26)],c(104,107,91,210),1.5)
    # Non-uniform painted highlights reinforce an original 2D illustration,
    # rather than a fake high-poly 3D screen capture.
    rng=random.Random(9328+step)
    for _ in range(130):
        x=rng.randint(24,89);y=rng.randint(30,115)
        color=i.getpixel(p(x,y))
        if color[3]<230:continue
        jitter=rng.choice([-25,-18,12,18])
        rgba=tuple(max(0,min(255,k+jitter)) for k in color[:3])+(rng.randint(55,110),)
        d.line([p(x,y),p(x+rng.choice([-1,0,1]),y+1)],fill=rgba,width=scale)
    i=i.resize((W,H),Image.Resampling.LANCZOS)
    return i

manifest=root/"sprite_manifest.json"
j=json.loads(manifest.read_text(encoding="utf8"))
entries={r["name"]:r for r in j.get("sprites",[])}
for frame in range(2):
    name=f"villager_walk_{frame}"
    im=paint(frame)
    dest=root/(name+".png")
    dest.write_bytes(b"")
    im.save(dest,optimize=True)
    a=entries[name]
    a["size"]=[W,H]
    a["bytes"]=dest.stat().st_size
    a["sha256"]=hashlib.sha256(dest.read_bytes()).hexdigest()
    a["foot_pivot"]=[.5,.075]
    a["source"]="original deterministic 2D illustrated Eldoria villagers; two gait frames; not Unity"
    meta=root/(name+".png.meta")
    source=meta.read_text(encoding="utf8")
    source,n1=re.subn(r"spritePixelsToUnits: \S+","spritePixelsToUnits: 130",source)
    source,n2=re.subn(r"spritePivot: \{x: [^}]+\}","spritePivot: {x: 0.500000, y: 0.075000}",source)
    if n1!=1 or n2!=1:raise RuntimeError("Cannot safely update original villager import")
    meta.write_text(source,encoding="utf8")
    print("PAINTED_INHABITANT",name,a["sha256"])
j["sprites"]=[entries[x] for x in sorted(entries)]
manifest.write_text(json.dumps(j,indent=2,ensure_ascii=False)+"\n",encoding="utf8")
