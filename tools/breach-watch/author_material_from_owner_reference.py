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
    w,h=512,736
    sample=source.crop((63,66,191,338)).resize((w,h),Image.Resampling.BICUBIC)
    result=Image.new("RGBA",(w,h))
    src=sample.load();dst=result.load()
    for y in range(h):
        fy=y/(h-1)
        for x in range(w):
            r,g,b=src[x,y]
            # The archived empty frame still has two BAKED separator
            # strokes at y≈403 and 660 after scaling. Those would create
            # ghost lines behind the live dynamic divider. Replace the
            # strokes WITHIN this one material via local texture blending;
            # no extra image and no data are drawn over the live HUD.
            for low,high in ((388,424),(646,676)):
                if low<=y<=high:
                    t=(y-low)/(high-low)
                    top=src[x,low-6]
                    bottom=src[x,min(h-1,high+6)]
                    r,g,b=tuple(round((1-t)*top[i]+t*bottom[i])
                        for i in range(3))
                    break
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
