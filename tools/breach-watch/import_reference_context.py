#!/usr/bin/env python3
"""Preserve the owner's selected rocky contextual frame as one new empty
nine-slice panel, from the selected Vigilia concept (NOT the old UI).

All static false prices, characters, models and texts were removed BEFORE
the source WebP was pinned. The real Unity data, 3D preview and input layer are
always separate live child components. This script doesn't compose a fake
screen or publish a game.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image

SOURCE=Path("tools/breach-watch/source/context-vigilia-256.webp.b64")
FOLDER=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerReference")
SOURCE_SHA="7a0d2377c969d75677e9493a776a9615f984eb72590f6a9d33d18fdca5120d49"
GUID="b86646e246a44877b9d17f036b799cb6"
META="""fileFormatVersion: 2
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
  spriteBorder: {{x: 39, y: 34, z: 34, w: 57}}
"""
def main():
    source=base64.b64decode(SOURCE.read_text().strip(),validate=True)
    if hashlib.sha256(source).hexdigest()!=SOURCE_SHA:
        raise RuntimeError("Source image does not match approved contextual frame")
    im=Image.open(io.BytesIO(source)).convert("RGBA")
    assert im.size==(256,398),im.size
    assert im.getpixel((0,0))[3]==0,"Screenshot exterior must remain transparent"
    assert im.getpixel((128,180))[3]>=250,"No live content may be baked into the real panel"
    FOLDER.mkdir(parents=True,exist_ok=True)
    path=FOLDER/"carved_context.png"
    im.save(path,optimize=True)
    (FOLDER/"carved_context.png.meta").write_text(META.format(guid=GUID),encoding="utf-8")
    mfile=FOLDER/"source-manifest.json"
    m=json.loads(mfile.read_text())
    m["context_frame"]={
        "name":"carved_context","sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
        "source_sha256":SOURCE_SHA,"guid":GUID,
        "dimensions":[256,398],
        "border":{"left":39,"bottom":34,"right":34,"top":57},
        "source":"owner-approved assistant-generated original UI concept; inner labels/figures removed",
        "not_native_screenshot":True
    }
    mfile.write_text(json.dumps(m,ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_ORIGINAL_INDEPENDENT_NINE_SLICE_CONTEXT_PASS",m["context_frame"]["sha256"])
if __name__=="__main__":main()
