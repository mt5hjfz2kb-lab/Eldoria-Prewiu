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
    "basalt_context":dict(width=512,height=736,cut=28,seed=173017,kind="stone",
                          border=(34,38,34,38),guid="74b0ad4795bd4ef1a98d624325997f55"),
    "burgundy_action":dict(width=512,height=148,cut=25,seed=173018,kind="burgundy",
                           border=(35,24,35,24),guid="a6f0f34c42fd494f92a72d201e0aaf9c"),
    "brass_portrait":dict(width=512,height=288,cut=20,seed=173019,kind="portrait",
                          border=(29,27,29,27),guid="9a20dba5e3aa4bff94654d1e8b0e9a32"),
    "slate_resource":dict(width=512,height=148,cut=23,seed=173020,kind="resource",
                          border=(24,22,24,22),guid="90ec0b5c9a2b463194f198a623e5e36c")
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
def surface(name,d):
    w,h,cut=d["width"],d["height"],d["cut"]
    kind=d["kind"];rng=random.Random(d["seed"])
    shape=Image.new("L",(w,h),0)
    ImageDraw.Draw(shape).polygon(corners(w,h,cut),fill=255)
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
                r=int(27+n*.8+5*(1-yf)+shade)
                g=int(28+n*.85+4*(1-yf)+shade)
                b=int(30+n*.8+shade)
            else:
                r=int(22+n*.9+8*(1-yf)+shade)
                g=int(24+n*.9+6*(1-yf)+shade)
                b=int(27+n*.85+shade)
            px[x,y]=(max(5,min(255,r)),max(5,min(255,g)),
                     max(5,min(255,b)),255)
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
    for inset,width,color in [
        (2,7,(4,5,7,255)),(5,4,(101,68,44,255)),
        (9,2,(200,147,83,245)),(14,3,(19,15,15,255)),
        (20,2,(130,86,51,255)),(23,1,(226,184,99,220))]:
        pts=corners(w,h,cut,inset)
        draw.line(pts+[pts[0]],fill=color,width=width,joint="curve")
    # Painted light from upper left, black contact shadow below
    pts=corners(w,h,cut,9)
    draw.line(pts[:3],fill=(255,217,131,225),width=2)
    draw.line(pts[3:7],fill=(23,12,8,255),width=3)
    # Copper corner pins and diamond signatures, fixed inside border
    locations=[(cut+18,cut+14),(w-cut-18,cut+14),
               (cut+18,h-cut-14),(w-cut-18,h-cut-14)]
    for i,(x,y) in enumerate(locations):
        draw.ellipse((x-6,y-6,x+6,y+6),fill=(17,13,12,255),
                     outline=(200,150,83,245),width=2)
        draw.ellipse((x-2,y-2,x+1,y+1),fill=(255,215,146,230))
        draw.line((x+1,y+1,x+4,y+4),fill=(2,4,6,180),width=2)
    # Center diamonds inside stretched UI panels obstruct real live labels.
    # The approved style has a discrete seal BELOW the building panel, not
    # a massive jewel through the title or the tappable action text.
    for yy in ([h-16] if kind=="stone" else []):
        center=w//2
        pts=[(center,yy-12),(center+10,yy),(center,yy+12),(center-10,yy)]
        draw.polygon(pts,fill=(143,86,49,255))
        draw.line(pts+[pts[0]],fill=(253,187,90,255),width=2)
        draw.line((center-4,yy,center+4,yy),fill=(252,225,151,220),width=2)
    if kind=="portrait":
        # A single restrained gold inner lip, not triple geometric borders.
        for yoff,color in [(33,(34,25,20,255)),(37,(212,148,71,255))]:
            draw.rounded_rectangle((yoff, yoff, w-1-yoff, h-1-yoff),
                        radius=7,outline=color,width=3)
    if kind=="burgundy":
        # Small metallic cross-hammer emblem on left; the button label is
        # rendered as real live Unity text, never baked into this artwork.
        ox,oy=58,h//2
        for sign in [-1,1]:
            draw.line((ox-15,oy+16*sign,ox+15,oy-16*sign),
                      fill=(11,8,7,255),width=11)
            draw.line((ox-15,oy+16*sign,ox+15,oy-16*sign),
                      fill=(186,121,62,255),width=6)
        draw.ellipse((ox-5,oy-5,ox+5,oy+5),
                     fill=(219,167,91,255),outline=(22,13,9,255),width=1)
        draw.line((100,27,w-58,27),fill=(235,119,59,130),width=3)
        draw.line((100,h-27,w-58,h-27),fill=(52,11,13,180),width=3)
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
        "independent":"All three nine-slice sprites are new and no old HUD graphics are reused",
        "not_native_screenshot":True,
        "art_approved":False,
        "sources":manifest
    },ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_GENUINE_ORIGINAL_SLATE_BURGUNDY_UI_SURFACES_AUTHORED",len(manifest))
if __name__=="__main__":main()
