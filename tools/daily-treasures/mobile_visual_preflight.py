#!/usr/bin/env python3
"""Visual readability at actual 32/48/64 pixel scale, before Windows/Unity.

Produces a small source-art contact sheet directly from the exact original
versioned PNGs. Numeric checks protect silhouette, material details and
contrast; passing does NOT assert subjective artistic excellence, animation,
or a native Unity render.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import hashlib
import json
from statistics import median

ROOT=Path(__file__).resolve().parents[2]
ASSETS=ROOT/"Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt"
OUT=ROOT/"tools/daily-treasures/mobile-source-art-contact.jpg"
BG=(25,24,28,255)
SIZES=(32,48,64)
NAMES={
    "MADERA":"resource_wood_clean_256.png",
    "PIEDRA":"resource_stone_clean_256.png",
    "COMIDA":"resource_food_clean_256.png",
    "ACELERADOR":"resource_hourglass_clean_256.png"
}

def need(test,description):
    if not test:raise AssertionError(description)

def verify(label,icon):
    need(icon.size==(256,256),label+" source dimension changed")
    for size in SIZES:
        tiny=icon.resize((size,size),Image.Resampling.LANCZOS)
        alphas=list(tiny.getchannel("A").getdata())
        strong=[i for i,a in enumerate(alphas) if a>96]
        need(len(strong)>size*size*.105,
             label+" reward completely vanishes at "+str(size)+" pixels")
        bbox=tiny.getchannel("A").point(
            lambda a:255 if a>96 else 0).getbbox()
        need(bbox is not None,label+" has no opaque silhouette")
        w,h=bbox[2]-bbox[0],bbox[3]-bbox[1]
        # The tall hourglass has a deliberately narrower silhouette than
        # wood, stone and food. Legibility depends on height and internal
        # material detail, not on stretching it into a fat rectangle.
        minimum_width=.46 if label=="ACELERADOR" else .50
        need(w>size*minimum_width and h>size*.48,
             label+" source art is too tiny for mobile "+str(bbox))
        bg=Image.new("RGBA",(size,size),BG)
        bg.alpha_composite(tiny)
        foreground=list(bg.convert("RGB").getdata())
        luma=[int(.2126*r+.7152*g+.0722*b)
              for i,(r,g,b) in enumerate(foreground) if alphas[i]>96]
        need(median(luma)>=52,
             label+" item has insufficient contrast at "+str(size)+" pixels")
        edge=list(bg.convert("RGB").convert("L")
                  .filter(ImageFilter.FIND_EDGES).getdata())
        detail=sum(edge[i]>28 for i in strong)
        need(detail>size*1.6,
             label+" loses all detailed material features at "+str(size)+" pixels")
        print("PASS",label,size,"px:",len(strong),
              "foreground pixels, box",bbox,
              "median luma",round(median(luma)),
              "detailed edges",detail)

def make_sheet(art):
    canvas=Image.new("RGB",(1040,575),(16,16,20))
    d=ImageDraw.Draw(canvas)
    d.rounded_rectangle((10,10,1029,564),radius=16,
                         outline=(155,100,49),width=2)
    d.text((35,24),"ELDORIA / TESOROS DE VALORIA — ART SOURCE MOBILE PREFLIGHT",
           fill=(230,192,132))
    d.text((35,48),
           "ORIGINAL ART · 32/48/64 PX · NOT A UNITY IMAGE · NOT ART APPROVED",
           fill=(161,151,140))
    for row,(label,file) in enumerate(NAMES.items()):
        icon=art[file]
        y=82+row*110
        d.rounded_rectangle((31,y,1006,y+98),radius=11,
                            fill=(30,28,29),outline=(115,78,43))
        d.text((47,y+11),label,fill=(226,190,136))
        for col,size in enumerate(SIZES):
            x=342+col*193
            d.text((x+14,y+8),str(size)+" px 1:1",fill=(196,178,151))
            tiny=icon.resize((size,size),Image.Resampling.LANCZOS)
            back=Image.new("RGBA",(71,69),(38,32,32,255))
            back.alpha_composite(tiny,((71-size)//2,(69-size)//2))
            canvas.paste(back.convert("RGB"),(x,y+26))
        large=icon.resize((90,90),Image.Resampling.LANCZOS)
        back=Image.new("RGBA",(94,94),(30,28,29,255))
        back.alpha_composite(large,(2,2))
        canvas.paste(back.convert("RGB"),(880,y+2))
    d.text((35,543),
           "Source QA: pixel silhouette, dark-stone contrast and edge detail. "
           "Real Unity/UI/UX approval remains pending.",
           fill=(162,148,127))
    OUT.parent.mkdir(parents=True,exist_ok=True)
    canvas.save(OUT,quality=92,subsampling=0)
    print("SOURCE_ART_CONTACT",OUT.relative_to(ROOT),
          hashlib.sha256(OUT.read_bytes()).hexdigest())

def main():
    manifest=json.loads((ASSETS/"source-manifest.json").read_text())
    expected={a["file"]:a["sha256"] for a in manifest["clean_resource_icons"]}
    need(set(expected)==set(NAMES.values()),"Mismatch: four clean resource identities")
    art={}
    for label,file in NAMES.items():
        source=ASSETS/file
        data=source.read_bytes()
        need(hashlib.sha256(data).hexdigest()==expected[file],
             "Source original art changed: "+file)
        icon=Image.open(source).convert("RGBA")
        verify(label,icon)
        art[file]=icon
    make_sheet(art)
    print("PASS all four 32/48/64px original resource icons. "
          "NOT a Unity image or human art approval")

if __name__=="__main__":main()
