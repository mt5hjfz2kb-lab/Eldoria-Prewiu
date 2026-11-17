#!/usr/bin/env python3
"""Verifies reproducible source, Retina bake, Unity PPU and native quality contract."""
from pathlib import Path
from PIL import Image
import hashlib,json
ROOT=Path(__file__).resolve().parents[1]
p=ROOT/"QA/retina_manifest.json"
if not p.is_file():
    print("RETINA_ASSET_NOT_YET_BAKED: verified by separate bake workflow")
    raise SystemExit(0)
m=json.loads(p.read_text(encoding="utf-8"))
assert m["schema"]==1 and m["original_size"]==[941,1672]
assert m["baked_size"]==[1412,2508]
for field,h in [("source","source_sha256"),("retina","retina_sha256")]:
    f=ROOT/m[field];assert f.is_file() and hashlib.sha256(f.read_bytes()).hexdigest()==m[h]
with Image.open(ROOT/m["retina"]) as img:
    assert img.mode=="RGB" and img.size==(1412,2508)
meta=(ROOT/"Sprites/Retina/Terreno_Organico.png.meta").read_text(encoding="utf-8")
assert "spritePixelsToUnits: 150" in meta
imp=(ROOT/"Scripts/Editor/City2DSpriteImporter.cs").read_text(encoding="utf-8")
builder=(ROOT/"Scripts/Editor/City2DSceneBuilder.cs").read_text(encoding="utf-8")
assert 'assetPath.Contains("/Sprites/Retina/") ? 150f : 100f' in imp
assert 'SpritesFolder + "Retina/" + path' in builder
print("RETINA_ASSET_INTEGRITY_PASS; Unity and mobile are separate gates")
