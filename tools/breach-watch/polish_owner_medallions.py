#!/usr/bin/env python3
"""Reproducible owner-approved dark metal medallion finishing pass.

Uses ONLY the five sprites extracted from the explicitly selected Vigilia
concept, never the RoyalHUD/Forja HUD, game art, a composited fake screenshot,
or a copyrighted outside UI. Does not alter original archived art.
"""
import hashlib
import json
import uuid
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter,ImageEnhance

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
        rgb=ImageEnhance.Contrast(image.convert("RGB")).enhance(1.14)
        rgb=ImageEnhance.Color(rgb).enhance(1.10)
        rgb=ImageEnhance.Brightness(rgb).enhance(1.13)
        data=[]
        for r,g,b in rgb.getdata():
            # Lift warm carved relief without brightening dark stone and
            # making the entire medallion look like a glowing flat disc.
            if r>g*1.06 and g>b*.98 and r>60:
                r=min(255,round(r*1.18+7))
                g=min(255,round(g*1.13+5))
                b=min(255,round(b*1.05+2))
            data.append((r,g,b))
        toned=Image.new("RGB",(192,192))
        toned.putdata(data)
        toned=Image.merge("RGBA",(*toned.split(),alpha))
        toned=toned.filter(ImageFilter.UnsharpMask(radius=1.2,percent=150,threshold=3))
        ambient=Image.new("RGBA",(192,192))
        glow=Image.new("RGBA",(192,192))
        gx=ImageDraw.Draw(glow)
        gx.ellipse((8,8,183,183),outline=(181,91,38,125),width=8)
        glow=glow.filter(ImageFilter.GaussianBlur(6))
        ambient.alpha_composite(glow)
        ambient.alpha_composite(toned)
        draw=ImageDraw.Draw(ambient)
        draw.arc((11,11,181,181),198,336,fill=(249,174,79,220),width=3)
        draw.arc((12,12,180,180),16,96,fill=(132,80,45,145),width=2)
        # Four tiny metal chisel nicks read as crafted ornament, not neon.
        for x,y in ((96,9),(183,96),(96,183),(9,96)):
            draw.ellipse((x-1,y-1,x+1,y+1),fill=(247,201,114,210))
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
        "authoring":"16-bit style tone and warm metal relief; non-destructive selected concept asset derivative",
        "not_native_capture":True,
        "not_owner_art_approval":True,
        "sprites":records
    },ensure_ascii=False,indent=2))
    print("BREACHWATCH_PREMIUM_OWNER_REFERENCE_MEDALLIONS_GENERATED",len(records))
if __name__=="__main__":main()
