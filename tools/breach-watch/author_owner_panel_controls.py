#!/usr/bin/env python3
"""Separate source-approved close/tower ornaments, never a baked game panel."""
from PIL import Image,ImageDraw,ImageFilter
from pathlib import Path
import hashlib,json
SOURCE=Path("tools/breach-watch/source/owner_gold_reference.webp")
SOURCE_SHA="79e2264961b3ea4462f9fc3dc65f513e2f4e76f59f4f6b2e3d27a5d06013a5a1"
OUT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls")
FONT_META="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
  sRGBTexture: 1
  isReadable: 0
  maxTextureSize: 256
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: 100
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
"""
SPECS=[
  ("close",(205,6,247,48),"2e781b402ca8420aa9c33a622f431681"),
  ("tower",(21,15,65,59),"7be4a6b8fc6c432caab805a77e59fd0d")
]
def main():
    assert hashlib.sha256(SOURCE.read_bytes()).hexdigest()==SOURCE_SHA
    original=Image.open(SOURCE).convert("RGBA")
    assert original.size==(256,427)
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT.parent/"OwnerControls.meta").write_text(
        "fileFormatVersion: 2\nguid: a5dd51b5c4cd4ee9a4f02146d099b82f\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n")
    records=[]
    for name,box,guid in SPECS:
        art=original.crop(box)
        if name=="close":
            mask=Image.new("L",art.size,0)
            ImageDraw.Draw(mask).ellipse((3,3,art.width-4,art.height-4),fill=255)
            mask=mask.filter(ImageFilter.GaussianBlur(.5))
        else:
            # The tower is a non-circular freestanding golden seal.
            mask=Image.new("L",art.size,0)
            p=art.load();m=mask.load()
            for y in range(art.height):
                for x in range(art.width):
                    r,g,b,_=p[x,y]
                    tint=min(r-g,g-b)
                    light=max(0,min(255,(g-60)*6))
                    m[x,y]=int(light*max(0,min(1,tint/13)))
            mask=mask.filter(ImageFilter.GaussianBlur(.45))
        art.putalpha(mask)
        art=art.resize((96,96),Image.Resampling.LANCZOS)
        file=OUT/(name+".png")
        art.save(file,optimize=True)
        (OUT/(name+".png.meta")).write_text(FONT_META.format(guid=guid))
        records.append({"name":name,"guid":guid,
            "sha256":hashlib.sha256(file.read_bytes()).hexdigest(),
            "dimensions":[96,96]})
    (OUT/"manifest.json").write_text(json.dumps({
        "source_sha256":SOURCE_SHA,"owner_reference":True,
        "no_gameplay_text_or_full_reference_image":True,
        "new_canvas_only":True,"sprites":records
    },indent=2))
    print("VIGILIA_OWNER_EXACT_SEPARATE_BUTTON_ART_CREATED",len(records))
if __name__=="__main__":main()
