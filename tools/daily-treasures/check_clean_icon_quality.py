#!/usr/bin/env python3
"""Verify visual hygiene of the premium isolated 256px Valoria resource icons.

This catches regressions to icons cropped from a *screenshot*: baked circular
backgrounds, cut-off silhouettes, edge-clipped numbers and tiny unreadable
objects. It does not claim to certify subjective artistic quality in Unity.
"""
import json,hashlib
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
TARGET=ROOT/"Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt"
manifest=json.loads((TARGET/"source-manifest.json").read_text(encoding="utf-8"))
icons=manifest["clean_resource_icons"]
assert len(icons)==4
assert {x["file"] for x in icons}=={
    "resource_wood_clean_256.png","resource_stone_clean_256.png",
    "resource_food_clean_256.png","resource_hourglass_clean_256.png"}
values=set()
for a in icons:
    path=TARGET/a["file"]
    blob=path.read_bytes()
    assert hashlib.sha256(blob).hexdigest()==a["sha256"], a["file"]
    im=Image.open(path).convert("RGBA")
    assert im.size==(256,256),a["file"]
    data=list(im.getdata())
    opacity=[p[3] for p in data]
    # The 8px outer margin is free of opaque pixels; no screenshot panels.
    for y in range(256):
        for x in range(256):
            if x<8 or x>=248 or y<8 or y>=248:
                assert opacity[y*256+x]<=55, a["file"]
    strong=[(i%256,i//256) for i,v in enumerate(opacity) if v>95]
    assert len(strong)>9000, f"Icon occupies too little screen space: {a['file']}"
    assert .12<len(strong)/65536<.40, (
        f"Icon is either a giant flat HUD screenshot or a tiny unreadable item: {a['file']}")
    minx=min(x for x,y in strong);maxx=max(x for x,y in strong)
    miny=min(y for x,y in strong);maxy=max(y for x,y in strong)
    assert minx>20 and maxx<235 and miny>22 and maxy<234, (
        f"Foreground clips against the source texture: {a['file']}")
    for x,y in [(0,0),(0,255),(255,0),(255,255)]:
        assert opacity[y*256+x]<=8, a["file"]
    opaque=[(r,g,b) for r,g,b,a8 in data if a8>175]
    assert max(max(p[k] for p in opaque)-min(p[k] for p in opaque)
               for k in range(3))>75, a["file"]
    values.add(a["sha256"])
    print("PASS painted standalone resource",
          a["file"],"opaque pixels",len(strong),
          "foreground bounds", (minx,miny,maxx,maxy))
assert len(values)==4
print("PASS 4 original alpha-clean resources, no baked HUD geometry or cropped silhouettes; ART_APPROVED remains false")
