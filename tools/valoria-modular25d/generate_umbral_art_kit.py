#!/usr/bin/env python3
"""Original, reproducible Eldoria 9-slice material art for UMBRAL II.

Authoring source, not a conceptual city image: five independent RGBA surfaces
for Unity uGUI. No sourced/copied art from WOS, Kingshot or Top Heroes.
Only original geometry, palette, texture grain and bevels drawn by this script.
"""
from PIL import Image,ImageDraw
from pathlib import Path
import hashlib,json,re

ROOT=Path(__file__).resolve().parents[2]
UI=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI"
OUT=UI/"UmbralII"
PROOF=ROOT/"docs/valoria-modular-25d/UMBRAL_II_MATERIALS_SOURCE.json"
SCALE=3

def hexcolor(v):
    return tuple(int(v[i:i+2],16) for i in (1,3,5))

def blend(a,b,t):
    return tuple(int(a[i]*(1-t)+b[i]*t) for i in range(3))

def paint(name,w,h,variant):
    W,H=w*SCALE,h*SCALE
    pic=Image.new("RGBA",(W,H))
    d=ImageDraw.Draw(pic)
    bronze=hexcolor("#B88E53")
    gold=hexcolor("#EFCC83")
    navy=hexcolor("#102633")
    if variant=="parchment":
        rad=22*SCALE
        d.rounded_rectangle((3*SCALE,3*SCALE,W-4*SCALE,H-4*SCALE),
            radius=rad,fill=navy+(255,),outline=bronze+(255,),width=4*SCALE)
        d.rounded_rectangle((7*SCALE,7*SCALE,W-8*SCALE,H-8*SCALE),
            radius=rad-3*SCALE,outline=gold+(255,),width=3*SCALE)
        d.rounded_rectangle((13*SCALE,13*SCALE,W-14*SCALE,H-14*SCALE),
            radius=rad-8*SCALE,fill=hexcolor("#AB834F")+(255,),
            outline=navy+(255,),width=3*SCALE)
        d.rounded_rectangle((20*SCALE,20*SCALE,W-21*SCALE,H-21*SCALE),
            radius=8*SCALE,fill=hexcolor("#EBD8B6")+(255,))
        pix=pic.load()
        for y in range(22*SCALE,H-22*SCALE):
            for x in range(22*SCALE,W-22*SCALE):
                if pix[x,y][3]<200: continue
                base=blend(hexcolor("#F4E6C9"),hexcolor("#E5D0AB"),y/(H-1))
                grain=(x*31+y*41+x*y//27)%11-5
                pix[x,y]=tuple(max(0,min(255,v+grain)) for v in base)+(255,)
        for x in (10*SCALE,W-11*SCALE):
            for y in (10*SCALE,H-11*SCALE):
                r=8*SCALE
                d.polygon(((x-r,y),(x,y-r),(x+r,y),(x,y+r)),fill=gold+(255,))
                d.ellipse((x-2*SCALE,y-2*SCALE,x+2*SCALE,y+2*SCALE),
                          fill=navy+(255,))
        for x in (26*SCALE,W-27*SCALE):
            for y in (26*SCALE,H-27*SCALE):
                d.ellipse((x-2*SCALE,y-2*SCALE,x+2*SCALE,y+2*SCALE),
                          fill=bronze+(255,))
    else:
        if variant=="resource":
            # High-contrast navy face. Gold must not run through resource
            # numbers on a 43px phone capsule.
            top,bottom,trim=("#344E5A","#18303D","#A78A57")
            rad=22*SCALE
        elif variant=="dock":
            top,bottom,trim=("#385263","#152D3F","#C5A269")
            rad=23*SCALE
        elif variant=="green":
            top,bottom,trim=("#55A47B","#1F7759","#E5C68A")
            rad=24*SCALE
        else:
            top,bottom,trim=("#7A4452","#452E42","#CCA06A")
            rad=23*SCALE
        top,bottom,trim=map(hexcolor,(top,bottom,trim))
        d.rounded_rectangle((5*SCALE,6*SCALE,W-6*SCALE,H-6*SCALE),
            radius=rad+SCALE,fill=navy+(220,),outline=hexcolor("#091E27")+(255,),width=3*SCALE)
        d.rounded_rectangle((4*SCALE,2*SCALE,W-5*SCALE,H-9*SCALE),
            radius=rad,fill=trim+(255,))
        d.rounded_rectangle((9*SCALE,7*SCALE,W-10*SCALE,H-14*SCALE),
            radius=rad-4*SCALE,fill=navy+(255,))
        mask=Image.new("L",(W,H))
        ImageDraw.Draw(mask).rounded_rectangle((12*SCALE,11*SCALE,W-13*SCALE,H-17*SCALE),
            radius=rad-6*SCALE,fill=255)
        gradient=Image.new("RGBA",(W,H))
        gpx=gradient.load()
        for y in range(H):
            row=blend(top,bottom,y/(H-1))
            for x in range(W):
                grain=((x*17+y*29)%13-6)*.3
                gpx[x,y]=tuple(int(max(0,min(255,v+grain))) for v in row)+(255,)
        pic.alpha_composite(Image.composite(gradient,Image.new("RGBA",(W,H)),mask))
        d=ImageDraw.Draw(pic)
        # Use an uninterrupted center field for gameplay typography.
        # The previous gilded arc was enlarged across the text by Unity's
        # sliced-image scaling and made header/resources/objectives unreadable.
        # Confine ornaments to edge-only bands outside the safe text center.
        d.rounded_rectangle((9*SCALE,8*SCALE,W-10*SCALE,H-14*SCALE),
            radius=rad-4*SCALE,outline=hexcolor("#6E8F91")+(155,)
                                    if variant=="green" else hexcolor("#6A8082")+(126,),
                                    width=1*SCALE)
        d.line(((23*SCALE,(h-14)*SCALE),((w-24)*SCALE,(h-14)*SCALE)),
            fill=gold+(105,),width=1*SCALE)
        if variant in ("resource","dock"):
            for x,y in ((19,19),(w-20,19)):
                d.ellipse(((x-2)*SCALE,(y-2)*SCALE,(x+2)*SCALE,(y+2)*SCALE),
                          fill=gold+(255,),outline=bronze+(255,),width=SCALE)
        if variant=="green":
            d.line(((30*SCALE,(h-19)*SCALE),((w-30)*SCALE,(h-19)*SCALE)),
                fill=hexcolor("#0E5146")+(255,),width=3*SCALE)
    result=pic.resize((w,h),Image.Resampling.LANCZOS)
    p=OUT/(name+".png")
    result.save(p,optimize=True)
    return p

def main():
    OUT.mkdir(parents=True,exist_ok=True)
    template=(UI/"slate_resource.png.meta").read_text(encoding="utf8")
    specs=[
        ("parchment_frame",256,256,"parchment",22),
        ("resource_capsule",256,96,"resource",16),
        ("dock_medallion",256,118,"dock",18),
        ("green_action",256,96,"green",18),
        ("burgundy_objective",256,96,"burgundy",18)]
    manifest=[]
    for name,w,h,variant,border in specs:
        p=paint(name,w,h,variant)
        original=p.read_bytes()
        # The new files must be actual Unity Sprite 9-slices, not JPGs or
        # screenshots with baked labels. Preserve exact pixel sizes and border.
        assert original[:8]==bytes.fromhex("89504e470d0a1a0a")
        with Image.open(p) as img:
            img.load()
            assert img.mode=="RGBA" and img.size==(w,h)
            assert img.getpixel((0,0))[3]==0
        guid=hashlib.md5(("Eldoria-UMBRAL-II-original-"+name).encode()).hexdigest()
        meta=re.sub(r"^guid: [a-f0-9]{32}$","guid: "+guid,template,flags=re.M)
        meta=re.sub(r"spriteBorder: .*$",
            "spriteBorder: {x: %d, y: %d, z: %d, w: %d}"%(border,border,border,border),
            meta,flags=re.M)
        assert "TextureImporter:" in meta and "textureType: 8" in meta
        mp=Path(str(p)+".meta")
        mp.write_text(meta,encoding="utf8")
        digest=hashlib.sha256(original).hexdigest()
        manifest.append({"name":name,"size":[w,h],"border":border,"guid":guid,
            "sha256":digest,"bytes":len(original),"artist_source":"original deterministic Pillow 2.5D UI ornament",
            "visual_approved":False,"unity_tested":False})
        print("UMBRAL_AUTHORING_SRC_VERIFIED",name,digest)
    PROOF.parent.mkdir(parents=True,exist_ok=True)
    PROOF.write_text(json.dumps({"schema":1,"style":"Valoria medieval UI, commercial clarity",
        "output":"Resources/ValoriaModular25D/UI/UmbralII",
        "gameplay_changed":False,"claims_unity_qa":False,"source":"generate_umbral_art_kit.py",
        "revision":"Umbral II commercial readability pass - edge-only decoration",
        "sprites":manifest},indent=2,ensure_ascii=False)+"\n",encoding="utf8")
    print("UMBRAL_II_ORIGINAL_MATERIALS_READY_FOR_UNITY_IMPORT")

if __name__=="__main__":main()
