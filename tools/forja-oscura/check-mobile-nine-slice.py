#!/usr/bin/env python3
"""Fail a Forja import when 9-slice borders consume the entire mobile button.
No Unity installation required; tests actual Sprite importer .meta files.
The importer's C# settings must agree with the serialized .meta values.
"""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[2]
assets=root/"Unity/Assets/Eldoria/VisualKit"
surfaces=assets/"Resources/ForjaOscura/Surfaces"
code=(assets/"Editor/EldoriaVisualKitTextureImporter.cs").read_text()
def expected(name):
    if name.startswith(("button_primary_","button_secondary_")): return (56,15,56,15,44)
    if name.startswith("nav_tile_"): return (48,22,48,22,65)
    if name in ("panel_dark","panel_light"): return (56,20,56,20,72)
    if name=="panel_quest": return (56,22,56,22,80)
    if name=="resource_chip": return (36,14,36,14,48)
    if name.startswith("tab_"): return (36,10,36,10,36)
    if name=="badge_level": return (32,8,32,8,28)
    if name.startswith("progress_"): return (20,3,20,3,12)
    if name=="toast_background": return (40,12,40,12,44)
    if name.startswith("medallion_"): return (0,0,0,0,0)
    raise AssertionError("Unknown Forja surface type: "+name)
passed=0
files=sorted(surfaces.glob("*.png"))
assert len(files)==21, len(files)
for png in files:
    l,b,r,t,min_height=expected(png.stem)
    meta=png.with_name(png.name+".meta")
    assert meta.is_file(),png
    text=meta.read_text()
    m=re.search(r"spriteBorder: \{x: (\d+), y: (\d+), z: (\d+), w: (\d+)\}",text)
    assert m, ("Missing border",meta)
    actual=tuple(map(int,m.groups()))
    assert actual==(l,b,r,t), (png.name,actual,(l,b,r,t))
    assert "spriteMeshType: 0" in text,(png.name,"requires FullRect mesh")
    if min_height: assert b+t<=int(min_height*.8), (png.name,"mobile center squeezed")
    if l+b+r+t: assert f"new Vector4({l}, {b}, {r}, {t})" in code,(png.name,"importer stale")
    passed+=1
print("FORJA_MOBILE_SLICED_SPRITE_IMPORT_PASS",passed)
