#!/usr/bin/env python3
"""Build a faithful high-DPI shared velvet ribbon sprite for the Corona HUD.
Only source here is original editable SVG, no copied art or game screenshot.
"""
from pathlib import Path
import hashlib,json
import cairosvg
from PIL import Image
R=Path(__file__).resolve().parents[2]
SRC=R/"tools/valoria-corona-brecha/source/top_velvet_ribbon.svg"
DEST=R/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrecha"
def main():
    DEST.mkdir(parents=True,exist_ok=True)
    output=DEST/"top_velvet_ribbon.png"
    data=SRC.read_bytes()
    assert b'<svg' in data and b'</svg>' in data
    cairosvg.svg2png(bytestring=data,write_to=str(output),
                    output_width=1512,output_height=184)
    with Image.open(output) as image:
        image.load()
        assert image.mode=="RGBA" and image.size==(1512,184)
        assert image.getpixel((0,0))[3]<30
        assert image.getbbox() is not None
        image.save(output,optimize=True)
    meta=("fileFormatVersion: 2\n"
          "guid: 76bc0b73d0724f17924e85483987fb26\n"
          "TextureImporter:\n"
          "  serializedVersion: 12\n"
          "  externalObjects: {}\n"
          "  mipmaps:\n"
          "    enableMipMap: 0\n"
          "  sRGBTexture: 1\n"
          "  isReadable: 0\n"
          "  textureFormat: 1\n"
          "  maxTextureSize: 2048\n"
          "  textureSettings:\n"
          "    serializedVersion: 2\n"
          "    filterMode: 1\n"
          "    aniso: 1\n"
          "    mipBias: 0\n"
          "    wrapU: 1\n"
          "    wrapV: 1\n"
          "    wrapW: 1\n"
          "  nPOTScale: 0\n"
          "  alphaIsTransparency: 1\n"
          "  alphaSource: 1\n"
          "  textureType: 8\n"
          "  spriteMode: 1\n"
          "  spriteMeshType: 0\n"
          "  spritePixelsToUnits: 200\n"
          "  spriteAlignment: 0\n"
          "  spritePivot: {x: 0.5, y: 0.5}\n"
          "  spriteBorder: {x: 0, y: 0, z: 0, w: 0}\n")
    (DEST/"top_velvet_ribbon.png.meta").write_text(meta)
    manifest={"source":"original top_velvet_ribbon.svg",
              "source_sha256":hashlib.sha256(data).hexdigest(),
              "png_sha256":hashlib.sha256(output.read_bytes()).hexdigest(),
              "guid":"76bc0b73d0724f17924e85483987fb26",
              "width":1512,"height":184,
              "preserved_canonical_gameplay":True,
              "art_approved":False,"unity_tested":False}
    target=R/"docs/valoria-modular-25d/CORONA_VELVET_RIBBON_PROVENANCE.json"
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(json.dumps(manifest,indent=2)+"\n")
    print("CORONA_ORIGINAL_VELVET_RIBBON_SOURCE_AND_SHA_PASS")
if __name__=="__main__":main()
