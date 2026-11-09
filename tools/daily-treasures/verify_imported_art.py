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

# All three visual frames are real copyrighted-project-origin binary art,
# not mere references to files that do not exist on the branch.
import json
import struct
manifest=json.loads((target/"source-manifest.json").read_text(encoding="utf-8"))
assert manifest["status"]=="IMPORTED_ORIGINAL_ART_NOT_YET_NATIVE_UNITY_APPROVED"
frames=manifest["ui_frames"]
assert len(frames)==3
for item in frames:
    name=item["file"]
    png=target/name
    meta=target/(name+".meta")
    assert png.is_file() and meta.is_file(), f"Missing real nine-slice frame: {name}"
    data=png.read_bytes()
    assert data.startswith(b"\x89PNG\r\n\x1a\n")
    width,height=struct.unpack(">II",data[16:24])
    assert (width,height)==(item["width"],item["height"]), f"Bad 9-slice image dimensions: {name}"
    assert hashlib.sha256(data).hexdigest()==item["sha256"], f"Artwork SHA mismatch: {name}"
    assert data[25] in (3,6) and (data[25]!=3 or b"tRNS" in data)
    text=meta.read_text(encoding="utf-8")
    assert f"guid: {item['guid']}\n" in text
    border=item["nine_slice"]
    assert f"spriteBorder: {{x: {border}, y: {border}, z: {border}, w: {border}}}" in text
    assert "spriteMode: 1" in text and "textureType: 8" in text
    assert item["guid"] not in seen
    seen.add(item["guid"])
    print(f"PASS nine-slice frame {name}: SHA256, alpha, border={border}, GUID={item['guid']}")
assert len(seen)==5
print("PASS FIVE EXACT VALORIA ART SPRITES, UNAPPROVED UNTIL NATIVE UNITY IMAGE REVIEW")

# The visible reward symbols are original illustrated resources, never emoji,
# unlicensed stock art or misleading representations of unavailable loot.
icons=manifest["resource_icons"]
assert len(icons)==4
for item in icons:
    name=item["file"]
    png=target/name
    meta=target/(name+".meta")
    assert png.is_file() and meta.is_file(), f"Missing genuine reward illustration: {name}"
    data=png.read_bytes()
    assert data[:8]==bytes([137,80,78,71,13,10,26,10])
    width,height=struct.unpack(">II",data[16:24])
    assert (width,height)==(128,128), f"Wrong reward icon dimensions: {name}"
    assert hashlib.sha256(data).hexdigest()==item["sha256"], f"Reward icon SHA256 mismatch: {name}"
    assert data[25] in (3,6) and (data[25]!=3 or b"tRNS" in data)
    text=meta.read_text(encoding="utf-8")
    assert f"guid: {item['guid']}" in text.splitlines()
    for required in ("textureType: 8", "spriteMode: 1", "alphaIsTransparency: 1"):
        assert required in text, f"Reward icon lacks Unity sprite setup: {name} -> {required}"
    assert item["guid"] not in seen
    seen.add(item["guid"])
    print(f"PASS painted {name} original art, alpha, SHA256 and Unity GUID")
assert len(seen)==9
print("PASS NINE ORIGINAL VALORIA 2D PNG ASSETS (native quality approval is separate)")

# Four new source-independent icons remove baked text, clipped screenshots,
# low resolution and circular HUD backgrounds in the old 128px fallbacks.
clean=manifest["clean_resource_icons"]
assert len(clean)==4
for item in clean:
    name=item["file"]
    data=(target/name).read_bytes()
    info=(target/(name+".meta")).read_text(encoding="utf-8")
    assert data[:8]==bytes([137,80,78,71,13,10,26,10])
    assert struct.unpack(">II",data[16:24])==(256,256), name
    assert hashlib.sha256(data).hexdigest()==item["sha256"], name
    assert data[25] in (3,6) and (data[25]!=3 or b"tRNS" in data)
    assert f"guid: {item['guid']}" in info.splitlines()
    assert "textureType: 8" in info and "spriteMode: 1" in info
    assert "alphaIsTransparency: 1" in info and "enableMipMap: 0" in info
    assert "spritePixelsToUnits: 256" in info
    assert item["guid"] not in seen
    seen.add(item["guid"])
    print(f"PASS 256px premium candidate {name}: original PNG, SHA256 and Unity importer")
assert len(seen)==13
print("PASS 13 actual Valoria 2D PNG files; clean iconic art STILL NEEDS native artistic review")
