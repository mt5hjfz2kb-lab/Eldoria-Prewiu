#!/usr/bin/env python3
"""One authored navigation/action family for Vigilia's first playable HUD.

Derives ONLY the five exact license-pinned PremiumMedallions from the owner's
chosen Vigilia concept. Unlike the old floaty sprites, each icon is composited
into its navigation tile OFFLINE: Unity paints ONE image per button plus the
genuine live (translatable) text. No UI screenshots, fake labels, or gameplay data.
"""
from PIL import Image, ImageDraw, ImageEnhance
from pathlib import Path
import math, random, json, hashlib

ROOT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt")
SRC=ROOT/"PremiumMedallions"
DEST=ROOT/"UnifiedButtons"
DEST.mkdir(parents=True, exist_ok=True)
(DEST.parent/"UnifiedButtons.meta").write_text(
    "fileFormatVersion: 2\n"
    "guid: a607e8b753e946a690d1ce417323c0d9\n"
    "folderAsset: yes\n"
    "DefaultImporter:\n  externalObjects: {}\n"
    "  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
    encoding="utf-8")

def octagon(w,h,r):
    return [(r,0),(w-r,0),(w,r),(w,h-r),
            (w-r,h),(r,h),(0,h-r),(0,r)]

def metal(w,h,mode="night",corner=18):
    image=Image.new("RGBA",(w,h),(0,0,0,0))
    px=image.load()
    rnd=random.Random(w*31+h)
    for y in range(h):
        fy=y/max(1,h-1)
        for x in range(w):
            edge=min(x/w,(w-1-x)/w,y/h,(h-1-y)/h)
            vign=.80+.20*min(1,edge*9)
            noise=rnd.randrange(-10,11)*.20
            if mode=="active":c=(22+10*fy,54+11*fy,72+9*fy)
            elif mode=="gold":c=(188-23*fy,139-38*fy,71-15*fy)
            else:c=(16+6*fy,33+7*fy,49+8*fy)
            px[x,y]=tuple(max(0,min(255,int(v*(1-.24*fy)*vign+noise))) for v in c)+(255,)
    mask=Image.new("L",(w,h),0)
    ImageDraw.Draw(mask).polygon(octagon(w,h,corner),fill=255)
    image.putalpha(mask)
    d=ImageDraw.Draw(image,"RGBA")
    for offset,rgba,thick in [
        (2,(70,42,24,230),3),(5,(219,174,87,255),3),
        (8,(112,77,41,255),2),(11,(37,44,50,255),2)]:
        corners=[(a+offset,b+offset) for a,b in
                 octagon(w-2*offset,h-2*offset,max(4,corner-offset))]
        d.line(corners+[corners[0]],fill=rgba,width=thick,joint="curve")
    d.line([(corner+15,7),(w-corner-15,7)],
           fill=(255,224,157,155),width=2)
    d.line([(corner+15,h-7),(w-corner-15,h-7)],
           fill=(9,14,22,210),width=2)
    for x in (corner//2+10,w-corner//2-12):
        d.ellipse((x-2,h//2-2,x+2,h//2+2),
                  fill=(32,27,16,255),outline=(216,176,92,250),width=2)
    return image

def symbol(name):
    # The previous tiles composited the whole MEDALLION circle onto a
    # rectangular card; technically one sprite, but still visually looked
    # like a sticker. Extract the engraved gold GLYPH from the approved
    # source's brighter center and feather only its matte. This is a real
    # owner-source transformation: no new icon, stock asset or AI overlay.
    im=Image.open(SRC/(name+".png")).convert("RGBA")
    px=im.load()
    a=Image.new("L",im.size,0)
    ap=a.load()
    cx=im.width*.5
    cy=im.height*.53125
    for y in range(im.height):
        for x in range(im.width):
            r,g,b,alpha=px[x,y]
            luma=r*.24+g*.67+b*.09
            dist=math.hypot((x-cx)/(im.width*.270833),
                            (y-cy)/(im.height*.296875))
            central=max(0.,min(1.,(1.00-dist)/.12))
            glyph=max(0.,min(1.,(luma-75.)/78.))
            ap[x,y]=int(alpha*glyph*central)
    im.putalpha(a)
    # Tight source-content crop: a clean engraved tower/sack/raven should
    # occupy the UPPER TWO THIRDS of each native tile without touching
    # the gold top bevel or live caption divider, not become a 7px emblem because
    # the surrounding (now transparent) old medallion occupied 90% of the
    # raster. All six game icons remain genuine extracted owner glyphs.
    return im.crop((48,45,144,155)).resize((98,98),Image.Resampling.LANCZOS)

def save(image,name,border=(0,0,0,0)):
    dest=DEST/(name+".png")
    image.save(dest,optimize=True)
    guid=hashlib.sha256(("ELDORIA-VIGILIA-ART-FAMILY-20261109:"+str(dest)).encode()).hexdigest()[:32]
    x,y,z,w=border
    (dest.with_suffix(".png.meta")).write_text(
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "TextureImporter:\n"
        "  serializedVersion: 12\n"
        "  externalObjects: {}\n"
        "  mipmaps:\n    enableMipMap: 0\n"
        "  sRGBTexture: 1\n  isReadable: 0\n"
        "  textureFormat: 1\n  maxTextureSize: 1024\n"
        "  textureSettings:\n    serializedVersion: 2\n"
        "    filterMode: 1\n    aniso: 1\n"
        "    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n"
        "  nPOTScale: 0\n  alphaIsTransparency: 1\n"
        "  alphaSource: 1\n  textureType: 8\n  spriteMode: 1\n"
        "  spriteMeshType: 0\n  spritePixelsToUnits: 100\n"
        "  spriteAlignment: 0\n"
        "  spritePivot: {x: 0.5, y: 0.5}\n"
        f"  spriteBorder: {{x: {x}, y: {y}, z: {z}, w: {w}}}\n",
        encoding="utf-8")
    return {"name":name,"sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),
            "bytes":dest.stat().st_size,"guid":guid}

manifest=[]
dock=metal(1024,175,corner=28)
dr=ImageDraw.Draw(dock,"RGBA")
dr.line([(50,152),(974,152)],fill=(25,102,124,110),width=3)
dr.line([(50,23),(974,23)],fill=(224,177,95,100),width=2)
for x in (33,991):
    dr.polygon([(x,65),(x+7,85),(x,105),(x-7,85)],
               fill=(159,126,68,180))
manifest.append(save(dock,"nav_dock",(40,28,40,28)))
for name in ("castle","provisions","swords","raven","helm"):
    original=symbol(name)
    for selected in (False,True):
        face=metal(240,168,"active" if selected else "night",19)
        d=ImageDraw.Draw(face,"RGBA")
        d.line([(30,115),(210,115)],fill=(223,186,112,230),width=2)
        d.line([(31,118),(209,118)],fill=(11,16,27,220),width=2)
        if selected:
            d.line([(26,12),(214,12)],fill=(94,196,215,190),width=3)
            d.line([(28,151),(212,151)],fill=(91,183,207,230),width=4)
        icon=ImageEnhance.Brightness(original).enhance(1.2) if selected else original
        face.alpha_composite(icon,(71,10))
        manifest.append(save(face,"nav_"+name+("_active" if selected else "")))
cta=metal(660,140,"gold",34)
d=ImageDraw.Draw(cta,"RGBA")
# The real source gold face IS the button, not a ruled parchment
# with another plate painted over it. Limit engraving to one thin
# inset highlight and ONE pair of crossed hammer handles on its left.
# The dynamic Cormorant inscription remains completely unobstructed.
# No interior horizontal rules: the previous 9-sliced CTA made those
# source-engraved stripes run directly THROUGH the live upgrade caption.
# The original border is enough. Typography must occupy uncluttered gold.
for tilt in (-1,1):
    d.line((73-12*tilt,45,73+12*tilt,95),fill=(61,42,22,225),width=8)
    d.line((73-12*tilt,45,73+12*tilt,95),fill=(235,201,132,240),width=4)
d.line([(130,32),(130,108)],fill=(84,54,24,105),width=2)
manifest.append(save(cta,"action_gold",(40,27,40,27)))
(DEST/"manifest.json").write_text(json.dumps({
  "source":"five approved Vigilia PremiumMedallions, all icon glyphs integrated into ONE raster per action",
  "license":"source art provenance tracked by original Vigilia PremiumMedallions manifest",
  "no_baked_gameplay_text":True,"files":manifest
},ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("VIGILIA_UNIFIED_BUTTON_ART_AUTHORED",len(manifest))
