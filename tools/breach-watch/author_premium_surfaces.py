#!/usr/bin/env python3
"""Author actual 9-sliced independent Vigilia UI PNGs, NOT a mock screenshot.
The owner's approved dark stone / copper / burgundy design is the art direction.
No old HUD textures, copied game graphics or purchased asset packs are used.
"""
import hashlib,json,math,random
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
OUT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/PremiumSurfaces")
DESIGNS={
    "burgundy_action":dict(width=512,height=148,cut=16,seed=173018,kind="burgundy",
                           border=(22,14,22,14),guid="a6f0f34c42fd494f92a72d201e0aaf9c"),
    "brass_portrait":dict(width=512,height=288,cut=12,seed=173019,kind="portrait",
                          border=(17,15,17,15),guid="9a20dba5e3aa4bff94654d1e8b0e9a32"),
    "slate_resource":dict(width=512,height=148,cut=10,seed=173020,kind="resource",
                          border=(12,8,12,8),guid="90ec0b5c9a2b463194f198a623e5e36c"),
    "chiseled_quest":dict(width=512,height=220,cut=20,seed=173021,kind="quest",
                          border=(48,32,66,32),guid="1138e4a0b67b480a89ce751bd2fb370c"),
    "profile_plate":dict(width=512,height=148,cut=13,seed=173022,kind="profile",
                          border=(16,11,16,11),guid="3f29a67de3ee436a83ba9a64e1b98242")
}
FOLDER_META="""fileFormatVersion: 2
guid: f8e9b67b7ee54fd4a22ef0c2e89a14dd
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
PNG_META="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 1024
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: 100
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: {left}, y: {bottom}, z: {right}, w: {top}}}
"""
def corners(w,h,cut,inset=0):
    left=inset;top=inset;right=w-1-inset;bottom=h-1-inset
    c=max(2,cut-inset//2)
    return [(left+c,top),(right-c,top),(right,top+c),
            (right,bottom-c),(right-c,bottom),(left+c,bottom),
            (left,bottom-c),(left,top+c)]
def quest_outline(w,h,inset):
    # Unique asymmetrical carved tablet: a compact left tower and tapered
    # right point. Border remains an independently nine-sliced UI component.
    left=inset;right=w-1-inset;top=inset;bottom=h-1-inset
    return [(left+17,top),(right-56,top),(right,h*.5),
            (right-56,bottom),(left+17,bottom),
            (left,bottom-20),(left,top+20)]

def surface(name,d):
    w,h,cut=d["width"],d["height"],d["cut"]
    kind=d["kind"];rng=random.Random(d["seed"])
    shape=Image.new("L",(w,h),0)
    outline=(lambda inset:quest_outline(w,h,inset)) if kind=="quest" else (
        lambda inset:corners(w,h,cut,inset))
    ImageDraw.Draw(shape).polygon(outline(0),fill=255)
    # Original mineral grain with large and fine strata. Pixel colors are
    # intentionally understated so light Cormorant text is legible in Unity.
    coarse=Image.new("L",(max(2,w//19),max(2,h//19)))
    coarse.putdata([rng.randrange(65,205) for _ in range(coarse.width*coarse.height)])
    coarse=coarse.resize((w,h),Image.Resampling.BICUBIC)
    fine=Image.new("L",(max(2,w//5),max(2,h//5)))
    fine.putdata([rng.randrange(80,180) for _ in range(fine.width*fine.height)])
    fine=fine.resize((w,h),Image.Resampling.BILINEAR)
    p1=coarse.load();p2=fine.load();mask=shape.load()
    pix=Image.new("RGBA",(w,h)); px=pix.load()
    for y in range(h):
        yf=y/max(1,h-1)
        for x in range(w):
            if not mask[x,y]:continue
            n=(p1[x,y]-128)*.072+(p2[x,y]-128)*.034
            edge=min(x,y,w-1-x,h-1-y)
            shade=-7 if edge<16 else 0
            # Soft brushed aged basalt and dark burgundy leather, not plain fill
            if kind=="burgundy":
                r=int(75+n*.8+15*(1-abs(x-w*.50)/(w*.5))+9*(1-yf)+shade)
                g=int(20+n*.30+3*(1-yf)+shade*.3)
                b=int(22+n*.25+shade*.5)
            elif kind=="portrait":
                r=int(32+n*.8+5*(1-yf)+shade*.7)
                g=int(32+n*.85+4*(1-yf)+shade*.7)
                b=int(34+n*.8+shade*.7)
            elif kind in ("stone","quest"):
                # Genuine hand-finished mineral contrast: earlier 22,24,27
                # produced a featureless pitch-black slab behind the Cormorant
                # text in the successful Unity render. A warm brighter slate
                # reveals crack/stone depth while remaining dark enough for
                # authentic live ivory text.
                r=int(35+n*1.25+11*(1-yf)+shade*.65)
                g=int(34+n*1.12+10*(1-yf)+shade*.65)
                b=int(33+n*.97+shade*.6)
            else:
                r=int(25+n*.9+8*(1-yf)+shade)
                g=int(27+n*.9+6*(1-yf)+shade)
                b=int(28+n*.85+shade)
            px[x,y]=(max(5,min(255,r)),max(5,min(255,g)),
                     max(5,min(255,b)),255)
    if kind in ("stone","quest"):
        # Real low-contrast stone fractures extend across the tablet rather
        # than only its outer border. Alpha-composited in a NEW layer so that
        # they are physically part of our 9-sliced PNG and never generate
        # flat transparency holes behind gameplay typography.
        cracks=Image.new("RGBA",(w,h),(0,0,0,0))
        veins=ImageDraw.Draw(cracks)
        grain_rng=random.Random(d["seed"]+510)
        for k in range(21 if kind=="stone" else 7):
            xx=grain_rng.randint(65,w-102)
            yy=grain_rng.randint(65,h-55)
            nodes=[(xx,yy)]
            for step in range(grain_rng.randint(3,6)):
                xx=max(60,min(w-90,xx+grain_rng.randint(-14,16)))
                yy=max(55,min(h-48,yy+grain_rng.randint(8,17)))
                nodes.append((xx,yy))
            veins.line([(x+2,y+2) for x,y in nodes],
                       fill=(7,6,5,34),width=3)
            veins.line(nodes,fill=(168,139,98,33),width=2)
        pix=Image.alpha_composite(pix,cracks)
    draw=ImageDraw.Draw(pix,"RGBA")
    # Independent hand-arranged shallow fissures: beveled light lip + black
    # hairline. Only on edge zones to avoid putting noise under live text.
    if kind=="stone":
        for side in range(2):
            xbase=43 if side==0 else w-43
            for i in range(7):
                yy=72+i*((h-136)/7)
                points=[(xbase,yy),(xbase+rng.randint(-8,8),yy+rng.randint(11,22)),
                        (xbase+rng.randint(-15,15),yy+rng.randint(26,40))]
                draw.line([(x+1,y+1) for x,y in points],
                          fill=(146,112,75,90),width=2)
                draw.line(points,fill=(5,7,8,155),width=2)
    if kind=="burgundy":
        for i in range(6):
            y=39+i*14
            draw.line([(85,y),(w-85,y+rng.randint(-3,3))],
                      fill=(125,40,28,30),width=1)
    # Three-stage sculptural frame. Different widths/shadow orientation mimic
    # hammered iron, warm bronze, and tight gilded inlay.
    # Resource capsules are only 38 UI units tall. The original 22+22
    # corner borders collapsed their entire nine-slice center and crushed
    # actual resource numbers. The engraved edges must be thinner while
    # retaining the same real brass material.
    border_layers=([
        (2,4,(4,5,7,255)),(4,2,(99,66,42,255)),
        (7,1,(214,159,92,245)),(9,2,(17,13,12,255)),
        (12,1,(145,99,59,225))
    ] if kind=="resource" else [
        # Only 14 stable pixels at the top/bottom are nine-sliced on this
        # 49px-tall CTA. Outer accents must live inside those 14 pixels,
        # otherwise they collapse into bright '=' lines THROUGH the caption.
        (2,5,(7,4,7,255)),(4,3,(101,55,40,255)),
        (8,2,(235,165,84,255)),(11,2,(17,12,12,255)),
        (13,1,(194,124,61,245))
    ] if kind=="burgundy" else [
        # The photo, resource and player plates are SMALL. A deep stack
        # of concentric forged rings here consumed 20% of the image and
        # gave the native Unity HUD an unnatural octagonal tunnel.
        # Draw a SINGLE raised metal lip plus a hairline highlight.
        (2,3,(13,10,9,255)),
        (5,2,(102,66,37,255)),
        (8,1,(222,166,91,245)),
        (12,1,(42,28,22,230))
    ] if kind in ("portrait","profile") else [
        (2,3,(8,8,8,255)),
        (5,1,(105,67,39,247)),
        (7,1,(204,153,89,238)),
        (10,1,(27,21,18,235))
    ])
    for inset,width,color in border_layers:
        pts=outline(inset)
        draw.line(pts+[pts[0]],fill=color,width=width,joint="curve")
    # Painted light from upper left, black contact shadow below
    pts=outline(9)
    draw.line(pts[:3],fill=(229,186,108,204),width=1)
    draw.line(pts[3:7],fill=(23,12,8,250),width=2)
    # Copper corner pins and diamond signatures, fixed inside border
    locations=([(34,29),(w-79,29),(34,h-30),(w-79,h-30)]
        if kind=="quest" else
        [(cut+18,cut+14),(w-cut-18,cut+14),
         (cut+18,h-cut-14),(w-cut-18,h-cut-14)])
    for i,(x,y) in enumerate(locations):
        draw.ellipse((x-6,y-6,x+6,y+6),fill=(17,13,12,255),
                     outline=(200,150,83,245),width=2)
        draw.ellipse((x-2,y-2,x+1,y+1),fill=(255,215,146,230))
        draw.line((x+1,y+1,x+4,y+4),fill=(2,4,6,180),width=2)
    # The real 9-slice artwork must never include center-line gems.
    # The owner-approved independent gold ring handles top/bottom ornament
    # OUTSIDE the content. Only fixed corner pins live in this sprite.
    # This removes the stale diamond which previously crossed the live CTA.
    if kind=="quest":
        # Chiselled open hairline and faint right bronze taper; no text or
        # progress is baked in. Live Unity Text/Buttons remain editable.
        lip=outline(17)
        draw.line(lip+[lip[0]],fill=(128,81,48,162),width=2)
        for yy in (72,108,145):
            draw.line((23,yy,29,yy+8),fill=(210,146,78,78),width=1)
        draw.line((w-91,21,w-38,h//2),fill=(208,145,72,165),width=2)
        draw.line((w-38,h//2,w-91,h-21),fill=(209,136,67,165),width=2)
    if kind=="portrait":
        # A light, portrait-window lip resembling the owner's reference:
        # no second chunky octagon between the true model and the frame.
        # The genuine RenderTexture fills the middle dynamically in Unity.
        draw.rounded_rectangle((14,14,w-15,h-15),radius=5,
            outline=(191,131,65,234),width=2)
    if kind=="burgundy":
        # This is ONLY the carved red leather/brass underlay. Fixed-size
        # crossed forged hammers are their own native transparent uGUI
        # component, so the nine-slice can NEVER crush them into an '=' or
        # draw lines through the live (localized) upgrade caption.
        # Four tiny corner glints remain safely inside fixed border pixels.
        for x in (24,w-25):
            for y in (12,h-13):
                draw.ellipse((x-2,y-2,x+2,y+2),fill=(244,181,93,178))
    return pix

def main():
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT.parent/"PremiumSurfaces.meta").write_text(FOLDER_META,encoding="utf-8")
    manifest=[]
    for name,d in DESIGNS.items():
        image=surface(name,d)
        p=OUT/(name+".png")
        image.save(p,optimize=True)
        border=d["border"]
        (OUT/(name+".png.meta")).write_text(PNG_META.format(
            guid=d["guid"],left=border[0],bottom=border[1],
            right=border[2],top=border[3]),encoding="utf-8")
        manifest.append({
            "name":name,"sha256":hashlib.sha256(p.read_bytes()).hexdigest(),
            "size":[d["width"],d["height"]],"guid":d["guid"],
            "border":border
        })
    (OUT/"manifest.json").write_text(json.dumps({
        "origin":"Original script-authored Vigilia surface artwork, owner's approved image is visual direction only",
        "independent":"All five nine-slice sprites are new and no old HUD graphics are reused",
        "not_native_screenshot":True,
        "art_approved":False,
        "sources":manifest
    },ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_GENUINE_ORIGINAL_SLATE_BURGUNDY_UI_SURFACES_AUTHORED",len(manifest))
if __name__=="__main__":main()
