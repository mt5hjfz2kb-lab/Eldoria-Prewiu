#!/usr/bin/env python3
"""Deterministically extract five navigation sprites from the OWNER-APPROVED
Vigilia de la Brecha concept, never the rejected legacy HUD skin.

The source image is an immutable small WebP atlas whose digest is pinned.
Result sprites are isolated transparent, separately selectable uGUI Images.
The approved concept is artist direction; runtime fidelity requires native Unity.
"""
import base64
import hashlib
import io
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

SRC=Path("tools/breach-watch/source/reference-80.webp.b64")
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerReference")
SOURCE_SHA="dff461d5e4c7d3de7ec6ccb641f7e372aee4f324310ca0b25ea630dfcb157437"
SPRITES=[
    ("helm","cb29e0bfbdd34f46a55bf778e3ff24a1"),
    ("swords","dd19e4e159ac435292c0790afbf39c02"),
    ("castle","e7770fb3850641e88306998af35b1479"),
    ("provisions","3702a0b165384ceebae5abf9d5fcf245"),
    ("raven","af9bef2d126d4443afcc613b9d3734cb"),
]
FOLDER_META="""fileFormatVersion: 2
guid: 93dbff66f63c47b6ae2fc9c1ce018fc4
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
  maxTextureSize: 256
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
    raw=base64.b64decode(SRC.read_text().strip(),validate=True)
    assert hashlib.sha256(raw).hexdigest()==SOURCE_SHA,"Approved concept source differs"
    atlas=Image.open(io.BytesIO(raw)).convert("RGB")
    assert atlas.size==(400,80),"Five medallions must have identical boundaries"
    DEST.mkdir(parents=True,exist_ok=True)
    (DEST.parent/"OwnerReference.meta").write_text(FOLDER_META,encoding="utf-8")
    manifest=[]
    for i,(name,guid) in enumerate(SPRITES):
        square=atlas.crop((i*80,0,(i+1)*80,80)).resize((160,160),Image.Resampling.LANCZOS)
        # Source screenshot has square background, remove only the outside
        # of the circular framed artifact while preserving actual ironwork.
        matte=Image.new("L",(160,160),0)
        d=ImageDraw.Draw(matte)
        d.ellipse((6,6,153,153),fill=255)
        matte=matte.filter(ImageFilter.GaussianBlur(.7))
        rgba=square.convert("RGBA")
        rgba.putalpha(matte)
        out=DEST/(name+".png")
        rgba.save(out,optimize=True)
        (DEST/(name+".png.meta")).write_text(PNG_META.format(guid=guid),encoding="utf-8")
        manifest.append({"name":name,"guid":guid,"size":[160,160],
            "sha256":hashlib.sha256(out.read_bytes()).hexdigest()})
    (DEST/"source-manifest.json").write_text(json.dumps({
        "source_file":str(SRC),"sha256":SOURCE_SHA,"origin":"owner-approved assistant-generated Vigilia de la Brecha concept in this conversation",
        "description":"New independent navigation artwork (not legacy UI, not a Unity game screenshot)",
        "sprites":manifest,"art_approved":False
    },indent=2,ensure_ascii=False),encoding="utf-8")
    print("BREACHWATCH_OWNER_CONCEPT_NAV_ATLAS_EXTRACTED",len(manifest),SOURCE_SHA)

if __name__=="__main__":main()
