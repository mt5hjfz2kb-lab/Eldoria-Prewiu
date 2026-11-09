#!/usr/bin/env python3
"""Reproducible owner-approved dark metal medallion finishing pass.

Uses ONLY the five sprites extracted from the explicitly selected Vigilia
concept, never the RoyalHUD/Forja HUD, game art, a composited fake screenshot,
or a copyrighted outside UI. Does not alter original archived art.
"""
import hashlib
import json
import math
import uuid
from pathlib import Path
from PIL import Image,ImageFilter,ImageEnhance

SRC=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerReference")
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/PremiumMedallions")
FOLDER_META="""fileFormatVersion: 2
guid: 2e276fa9688144ddb76f2cfb8a6405c3
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
IMPORTER="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 512
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
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
"""
def main():
    manifest=json.loads((SRC/"source-manifest.json").read_text())
    assert manifest["sha256"]=="dff461d5e4c7d3de7ec6ccb641f7e372aee4f324310ca0b25ea630dfcb157437"
    assert sorted(x["name"] for x in manifest["sprites"])==[
        "castle","helm","provisions","raven","swords"]
    DEST.mkdir(parents=True,exist_ok=True)
    (DEST.parent/"PremiumMedallions.meta").write_text(FOLDER_META)
    records=[]
    for entry in manifest["sprites"]:
        name=entry["name"]
        source=SRC/(name+".png")
        assert hashlib.sha256(source.read_bytes()).hexdigest()==entry["sha256"]
        image=Image.open(source).convert("RGBA")
        assert image.size==(160,160)
        image=image.resize((192,192),Image.Resampling.LANCZOS)
        alpha=image.getchannel("A")
        rgb=ImageEnhance.Contrast(image.convert("RGB")).enhance(1.06)
        rgb=ImageEnhance.Color(rgb).enhance(1.04)
        rgb=ImageEnhance.Brightness(rgb).enhance(1.07)
        data=[]
        for r,g,b in rgb.getdata():
            # Lift warm carved relief without brightening dark stone and
            # making the entire medallion look like a glowing flat disc.
            if r>g*1.06 and g>b*.98 and r>60:
                r=min(255,round(r*1.08+3))
                g=min(255,round(g*1.06+2))
                b=min(255,round(b*1.03+1))
            data.append((r,g,b))
        toned=Image.new("RGB",(192,192))
        toned.putdata(data)
        toned=Image.merge("RGBA",(*toned.split(),alpha))
        # Premium aged-copper coloration applied INSIDE the ONE original
        # medallion texture, never as a second sprite, ring or button layer.
        # Preserve actual swords/raven/helmet in their original silver/stone:
        # only the outer sculpted metal catches reflected gold.
        pixels=toned.load()
        for y in range(toned.height):
            for x in range(toned.width):
                red,green,blue,opacity=pixels[x,y]
                radius=math.hypot(x-95.5,y-95.5)/96
                highlight=max(red,green,blue)
                if opacity<16 or radius<.66 or radius>1.09 or highlight<42:
                    continue
                amount=max(0,min(1,(radius-.66)/.14))
                amount*=max(0,min(1,(highlight-42)/95))*.82
                bronze=(min(255,highlight*1.10),
                        min(255,highlight*.76),
                        min(255,highlight*.43))
                pixels[x,y]=tuple(round(v*(1-amount)+c*amount)
                    for v,c in zip((red,green,blue),bronze))+(opacity,)
        # The owner's illustration already contains the entire carved rim.
        # Previous iterations drew a second glowing circumference, two arcs
        # and four studs *over* that rim; at real phone size they appeared as
        # ghost buttons behind the five approved icons. Eliminate every
        # extra decorative layer instead of superimposing another fix.
        ambient=toned.filter(ImageFilter.UnsharpMask(
            radius=.75,percent=78,threshold=4))
        dest=DEST/(name+".png")
        ambient.save(dest,optimize=True)
        guid=uuid.uuid5(uuid.NAMESPACE_DNS,"eldoria.vigilia.premium."+name).hex
        (DEST/(name+".png.meta")).write_text(IMPORTER.format(guid=guid))
        records.append({
            "name":name,"original_sha256":entry["sha256"],
            "result_sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),
            "guid":guid,"size":[192,192]
        })
    (DEST/"manifest.json").write_text(json.dumps({
        "source":"OwnerReference/source-manifest.json",
        "scope":"new independent Vigilia uGUI only",
        "authoring":"One original sculpted icon per control; its existing metal rim is gently color-graded to copper within the same sprite, no second circle/glow/studs/vector symbol.",
        "not_native_capture":True,
        "not_owner_art_approval":True,
        "sprites":records
    },ensure_ascii=False,indent=2))
    print("BREACHWATCH_PREMIUM_OWNER_REFERENCE_MEDALLIONS_GENERATED",len(records))
if __name__=="__main__":main()
