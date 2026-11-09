#!/usr/bin/env python3
"""Owner-reference mission panel: verify actual PNG bytes and native binding. Source QA only."""
from pathlib import Path
import hashlib
from PIL import Image
root=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls")
p=root/"mission_plate.png"
b=p.read_bytes()
assert hashlib.sha256(b).hexdigest()=="067568edfe8d59a0087e733131257e3aafe537df30ab8248e728069f7a0d5e2a"
with Image.open(p) as original:
    assert original.size==(303,100) and original.mode in ("P","RGBA")
    rgba=original.convert("RGBA")
    assert rgba.getpixel((0,0))[3]==0
    # Quantized PNG preserves near-opaque center as alpha 254 in the palette.
    assert rgba.getpixel((165,55))[3]>=250
    assert rgba.getpixel((45,50))[3]>=250
meta=(root/"mission_plate.png.meta").read_text()
assert "guid: 35ad1478e20a4c4493e6e918bcd21e05" in meta
assert "spritePixelsToUnits: 150" in meta
assert "spriteBorder: {x: 75, y: 12, z: 40, w: 16}" in meta
hud=Path("Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs").read_text()
assert 'VigiliaArt/OwnerControls/mission_plate' in hud
assert 'tablet.sprite.name=="mission_plate"' in hud
assert "chapterStone.ApplyIndependentArt(angledPlate,true);" in hud
assert "chapterStone.preserveAspect=false;" in hud
assert 'Glyph(objective,"Chapter stone rune"' not in hud
assert "realChapter.ToUpperInvariant()" in hud
print("OWNER_MISSION_SOURCE_PLATE_PASS source-derived live panel; Unity visual approval not claimed")
