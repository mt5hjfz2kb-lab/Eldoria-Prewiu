#!/usr/bin/env python3
"""No-Unity proof that the actual versioned texture bytes match their
independently approved source identities and import as transparent Sprites.
This is an ASSET INTEGRITY test, never an artistic or gameplay approval.
"""
import hashlib
import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MOD = Path(__file__).resolve().parent / "import_chest_art.py"
spec = importlib.util.spec_from_file_location("valoria_chest_importer", MOD)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
target = ROOT / mod.DESTINATION
assert target.is_dir(), "Imported Sprites missing"
seen=set()
for name, (digest, guid) in mod.EXPECTED.items():
    png=target/name
    meta=target/(name+".meta")
    assert png.is_file() and meta.is_file(), f"Missing game asset: {name}"
    data=png.read_bytes()
    mod.inspect_png(data)
    actual=hashlib.sha256(data).hexdigest()
    assert actual==digest, f"Imported {name} does not match approved sprite SHA256"
    text=meta.read_text(encoding="utf-8")
    assert f"guid: {guid}\n" in text, f"Incorrect GUID for {name}"
    for part in ("textureType: 8", "spriteMode: 1", "alphaIsTransparency: 1", "enableMipMap: 0"):
        assert part in text, f"{name} wrong PNG importer setting: {part}"
    assert guid not in seen
    seen.add(guid)
    print(f"PASS {name} SHA256={digest} alpha+Unity Sprite config OK")
assert len(seen)==2
print("PASS TWO EXACT UNALTERED VALORIA ART SPRITES (not native Unity approval)")
