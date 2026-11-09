#!/usr/bin/env python3
"""ONE blank stone fill derived from the user's approved empty carved art.
No static prices, gameplay text, model or duplicated frame is baked in.
"""
import hashlib,json
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
ROOT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt")
SOURCE=ROOT/"OwnerReference/carved_context.png"
OUT=ROOT/"PremiumSurfaces/panel_fill.png"
GUID="4acf13e685a74fd9b930b46e7fb580b3"
META="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
  sRGBTexture: 1
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  maxTextureSize: 1024
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spritePixelsToUnits: 100
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
"""
def main():
    manifest=json.loads((ROOT/"OwnerReference/source-manifest.json").read_text())
    sha=manifest["context_frame"]["sha256"]
    assert hashlib.sha256(SOURCE.read_bytes()).hexdigest()==sha
    source=Image.open(SOURCE).convert("RGB")
    assert source.size==(256,398)
    # The approved empty reference material still contains two *baked*
    # decorative horizontal separators (original rows 214-216 and 309-310).
    # They are not intended as game-data rows in our new dynamic panel.
    # Repair the SOURCE mineral image, not another layer drawn on top:
    # interpolate nearby stone and retain a little authentic micro-grain.
    fixed=source.copy()
    for low,high in ((211,219),(304,318)):
        for y in range(low,high):
            t=(y-low)/(high-low-1)
            for x in range(source.width):
                top=source.getpixel((x,low-1))
                bottom=source.getpixel((x,high+1))
                micro=source.getpixel((x,y-25))
                baseline=tuple(sum(source.getpixel((x,yy))[i]
                                   for yy in range(low-30,low-15))/15.0
                               for i in range(3))
                final=tuple(max(0,min(255,round(
                    top[i]*(1-t)+bottom[i]*t+
                    (micro[i]-baseline[i])*.16))) for i in range(3))
                fixed.putpixel((x,y),final)
    w,h=512,736
    sample=fixed.crop((63,66,191,338)).resize((w,h),Image.Resampling.BICUBIC)
    result=Image.new("RGBA",(w,h))
    src=sample.load();dst=result.load()
    for y in range(h):
        fy=y/(h-1)
        for x in range(w):
            r,g,b=src[x,y]
            grain=(r+g+b)/3-22
            light=5*(1-fy)-3*((x-w/2)/(w/2))**2
            color=(41+grain*.72+light,37+grain*.62+light*.78,
                   31+grain*.49+light*.55)
            dst[x,y]=tuple(max(13,min(112,round(q))) for q in color)+(255,)
    mask=Image.new("L",(w,h),0);d=ImageDraw.Draw(mask)
    pad,cut=6,18
    d.polygon([(pad+cut,pad),(w-pad-cut,pad),(w-pad,pad+cut),
               (w-pad,h-pad-cut),(w-pad-cut,h-pad),(pad+cut,h-pad),
               (pad,h-pad-cut),(pad,pad+cut)],fill=255)
    result.putalpha(mask.filter(ImageFilter.GaussianBlur(.7)))
    # Detect the exact regression that previously produced horizontal
    # bars across the premium Bastion card in genuine Unity screenshots.
    central=[sum(result.getpixel((x,y))[:3])
             for y in range(50,h-50) for x in range(100,401)]
    row_luma=[sum(central[y*301:(y+1)*301])/(301*3)
              for y in range(h-100)]
    sharpest=max(abs(row_luma[y]-row_luma[y-1])
                 for y in range(1,len(row_luma)))
    assert sharpest<1.40,("Baked horizontal separator ghost detected",
                         round(sharpest,3))
    OUT.parent.mkdir(parents=True,exist_ok=True)
    result.save(OUT,optimize=True)
    OUT.with_suffix(".png.meta").write_text(META.format(guid=GUID))
    proof={"guid":GUID,"source_sha256":sha,"sha256":hashlib.sha256(OUT.read_bytes()).hexdigest(),
           "size":[w,h],"source":"user-approved blank context interior",
           "no_baked_player_content":True,"art_approved":False}
    (OUT.parent/"panel_fill-provenance.json").write_text(
        json.dumps(proof,indent=2,ensure_ascii=False))
    print("VIGILIA_APPROVED_MINERAL_FILL_PASS",proof["sha256"])
if __name__=="__main__":main()
