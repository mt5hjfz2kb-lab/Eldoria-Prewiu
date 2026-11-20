#!/usr/bin/env python3
"""Original Eldoria six-icon art atlas -> exact game Sprite-ready RGBA PNG bytes.

The tiny source is original, independently authored artwork made for this
Eldoria experiment, not a crop from Whiteout Survival or another game.
Only lossless colour-type normalization follows the colour-reduced source.
"""
from pathlib import Path
import base64,hashlib,io,json
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"tools/valoria-modular25d/original_umbral_icons48_png.b64"
DEST=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/UmbralII"
PROOF=ROOT/"docs/valoria-modular-25d/UMBRAL_II_GAMEPLAY_ICONS_SOURCE.json"
SHA="0c656ddb672784f6bea83252cf778e83f5985e93fb02e4cc42bc1b3913838695"
GUID=hashlib.md5(b"Eldoria-UmbralII-original-six-gameplay-icon-atlas-binary").hexdigest()

def main():
    raw=base64.b64decode(SRC.read_text(encoding="ascii"),validate=False)
    assert hashlib.sha256(raw).hexdigest()==SHA,"Original icon source mismatch"
    assert len(raw)==2993 and raw[:8]==bytes.fromhex("89504e470d0a1a0a")
    with Image.open(io.BytesIO(raw)) as image:
        image.load()
        assert image.size==(144,96) and image.mode=="P"
        rgba=image.convert("RGBA")
        exact_original_pixel_sha=hashlib.sha256(rgba.tobytes()).hexdigest()
        assert rgba.getpixel((0,0))[3]==0,"Icon atlas lost original transparency"
        dst=io.BytesIO()
        rgba.save(dst,format="PNG",optimize=True)
    out=dst.getvalue()
    with Image.open(io.BytesIO(out)) as image:
        image.load()
        assert image.size==(144,96) and image.mode=="RGBA"
        assert hashlib.sha256(image.tobytes()).hexdigest()==exact_original_pixel_sha
    DEST.mkdir(parents=True,exist_ok=True)
    file=DEST/"umbral_gameplay_icons_png.bytes"
    file.write_bytes(out)
    meta=DEST/"umbral_gameplay_icons_png.bytes.meta"
    meta.write_text("fileFormatVersion: 2\n"+
        "guid: "+GUID+"\n"+
        "TextScriptImporter:\n"+
        "  externalObjects: {}\n"+
        "  userData:\n  assetBundleName:\n  assetBundleVariant:\n",encoding="utf-8")
    icons=["wood","stone","food","hammer","city","task"]
    PROOF.parent.mkdir(parents=True,exist_ok=True)
    PROOF.write_text(json.dumps({
        "schema":1,
        "artist_source":"Original art atlas authored for this Eldoria experiment; no third-party game art",
        "source_file":SRC.relative_to(ROOT).as_posix(),
        "source_palette_sha256":SHA,
        "original_pixels_rgba_sha256":exact_original_pixel_sha,
        "packed_file":file.relative_to(ROOT).as_posix(),
        "packed_rgba_sha256":hashlib.sha256(out).hexdigest(),
        "packed_bytes":len(out),"dimensions":[144,96],"cell_size":48,
        "cells_row_major_top_down":icons,"textasset_guid":GUID,
        "visual_art_approved":False,"native_unity_import_verified":False,
        "physical_iphone_tested":False
    },ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("ORIGINAL_6_ICON_ATLAS_SOURCE_PIXELS_AND_TEXTASSET_IDENTITY_PASS")

if __name__=="__main__":main()
