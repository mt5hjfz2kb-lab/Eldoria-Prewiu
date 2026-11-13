#!/usr/bin/env python3
"""Reproduce three individual owner-approved utility icons in Vigilia.

Only the ALREADY UPLOADED, OWNER-APPROVED 864x1536 game UI concept is the
source of these separately cropped medallions. No complete UI image,
static gameplay numbers, old RoyalHUD art, or fake gameplay frame is used.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image

SOURCE=Path("tools/breach-watch/source/owner-utility-40.webp.b64")
SOURCE_SHA="01fcf2fe0aa7526fda850cdb6690807801235536e2151a037380fa7712365bb6"
OUT=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerUtilities")
NAMES=[
 ("bell","4da9483b75da4303ac4b1d4c963d3615"),
 ("settings","64de6b1fad4e428ab66e6bb5a535413e"),
 ("breach","7be55da4b5cd4ba3924cdf3fb073237e")
]
FOLDER_META="""fileFormatVersion: 2
guid: 3b26705f944c4e28ae3f431fe1a088da
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
  maxTextureSize: 128
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
    data=base64.b64decode(SOURCE.read_text().strip(),validate=True)
    digest=hashlib.sha256(data).hexdigest()
    if digest!=SOURCE_SHA:
        raise RuntimeError(f"The approved utility art source changed: {digest}")
    atlas=Image.open(io.BytesIO(data)).convert("RGBA")
    if atlas.size!=(120,40):
        raise RuntimeError(f"Unexpected independent atlas geometry: {atlas.size}")
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT.parent/"OwnerUtilities.meta").write_text(FOLDER_META,encoding="utf-8")
    entries=[]
    for i,(name,guid) in enumerate(NAMES):
        crop=atlas.crop((i*40,0,(i+1)*40,40))
        if not crop.getchannel("A").getbbox():
            raise RuntimeError("Unusable transparent source "+name)
        target=crop.resize((80,80),Image.Resampling.LANCZOS)
        dest=OUT/(name+".png")
        target.save(dest,optimize=True)
        (OUT/(name+".png.meta")).write_text(PNG_META.format(guid=guid),encoding="utf-8")
        entries.append({"name":name,"guid":guid,"size":[80,80],
                        "sha256":hashlib.sha256(dest.read_bytes()).hexdigest()})
    (OUT/"manifest.json").write_text(json.dumps({
        "source":str(SOURCE),"source_sha256":SOURCE_SHA,
        "origin":"Owner-approved vertical Vigilia screenshot in this conversation",
        "copyright":"User-provided generated image, independent cropped medallions",
        "runtime_scope":"Only the new Vigilia canvas; all three remain non-functional until their systems exist",
        "not_native_screenshot":True,"art_approved_in_unity":False,
        "sprites":entries
    },ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_3_OWNER_APPROVED_GOLD_UTILITY_MEDALLIONS_EXTRACTED")

if __name__=="__main__":main()
