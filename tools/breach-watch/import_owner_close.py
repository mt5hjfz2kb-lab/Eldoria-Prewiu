#!/usr/bin/env python3
"""Import one exact owner-approved circular close control for Vigilia.
The 64px source is isolated from the user's chosen original 864x1536 UI,
with no player text or world state; Unity renders it as a single Sprite.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image
SRC=Path("tools/breach-watch/source/owner-close-button.webp.b64")
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls")
SHA="12df8c23f94ac7610cbb2e90849e6ed88009bfd20618cf91aba9dc407f099f3c"
GUID="8e6ca219e4f54716a8210db545179f0c"
META="""fileFormatVersion: 2
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
    data=base64.b64decode(SRC.read_text().strip(),validate=True)
    assert hashlib.sha256(data).hexdigest()==SHA
    image=Image.open(io.BytesIO(data)).convert("RGBA")
    assert image.size==(64,64)
    assert image.getpixel((0,0))[3]<30
    DEST.mkdir(parents=True,exist_ok=True)
    dst=DEST/"close_medallion.png"
    image.save(dst,optimize=True)
    (DEST/"close_medallion.png.meta").write_text(META.format(guid=GUID))
    proof={
        "name":"close_medallion","size":[64,64],
        "source_sha256":SHA,
        "png_sha256":hashlib.sha256(dst.read_bytes()).hexdigest(),
        "guid":GUID,"source":"Owner-approved 864x1536 first-screen circle / one isolated close X",
        "transparent_hit_region_required":True,
        "not_native_screenshot":True}
    (DEST/"close_medallion_provenance.json").write_text(
        json.dumps(proof,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("VIGILIA_OWNER_ROUND_CLOSE_PROVENANCE_PASS",proof["png_sha256"])
if __name__=="__main__":
    main()
