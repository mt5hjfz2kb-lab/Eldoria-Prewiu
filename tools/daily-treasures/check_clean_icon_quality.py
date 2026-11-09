#!/usr/bin/env python3
"""Verify visual hygiene of the premium isolated 256px Valoria resource icons.

This catches regressions to icons cropped from a *screenshot*: baked circular
backgrounds, cut-off silhouettes, edge-clipped numbers and tiny unreadable
objects. It does not claim to certify subjective artistic quality in Unity.
"""
import json,hashlib
from pathlib import Path
from PIL import Image
import numpy as np

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
    data=np.asarray(im,dtype=np.uint8)
    opacity=data[:,:,3]
    # The 8px outer margin is free of opaque pixels; no screenshot panels.
    assert np.count_nonzero(opacity[:8,:]>55)==0, a["file"]
    assert np.count_nonzero(opacity[-8:,:]>55)==0, a["file"]
    assert np.count_nonzero(opacity[:,:8]>55)==0, a["file"]
    assert np.count_nonzero(opacity[:,-8:]>55)==0, a["file"]
    ys,xs=np.nonzero(opacity>95)
    assert len(xs)>9000, f"Icon occupies too little screen space: {a['file']}"
    assert 0.12<len(xs)/opacity.size<0.40, (
        f"Icon is either a giant flat HUD screenshot or a tiny unreadable item: {a['file']}")
    assert min(xs)>20 and max(xs)<235 and min(ys)>22 and max(ys)<234, (
        f"Foreground clips against the source texture: {a['file']}")
    # Transparent corners, no opaque square cards burned into the illustration.
    for x,y in [(0,0),(0,255),(255,0),(255,255)]:
        assert opacity[y,x]<=8, a["file"]
    opaqueRGB=data[opacity>175,:3]
    assert opaqueRGB.ptp(axis=0).max()>75 if hasattr(opaqueRGB,'ptp') else (
        np.ptp(opaqueRGB,axis=0).max()>75), a["file"]
    values.add(a["sha256"])
    print("PASS painted standalone resource",
          a["file"],"opaque pixels",len(xs),
          "foreground bounds", (int(xs.min()),int(ys.min()),int(xs.max()),int(ys.max())))
assert len(values)==4
print("PASS 4 original alpha-clean resources, no baked HUD geometry or cropped silhouettes; ART_APPROVED remains false")
