#!/usr/bin/env python3
"""Offline pixel integrity and polygon footprint audit. NOT a Unity rendering test."""
from __future__ import annotations
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

PILOT = Path(__file__).resolve().parents[1]
REPO = Path(__file__).resolve().parents[5]
DATA = PILOT / "Data/foreground_contours_v1.json"
TERRAIN = REPO / "Unity/Assets/Experimentos/Ciudad2D/Sprites/Terreno_Organico.png"
OUT = PILOT / "QA/generated-offline"

def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def check() -> dict:
    assert DATA.is_file() and TERRAIN.is_file(), "Missing immutable source terrain or polygons"
    layout = json.loads(DATA.read_text(encoding="utf-8"))
    assert layout["coordinate_width"] == 941 and layout["coordinate_height"] == 1672
    with Image.open(TERRAIN) as im:
        base = im.convert("RGBA")
    width, height = base.size
    assert width >= 941 and height >= 1672, (width,height)
    assert base.getchannel("A").getextrema() == (255,255), "Original terrain not fully opaque: require alpha-aware compositor"
    combined = Image.new("L", base.size, 0)
    records = []
    opaque_pixels = 0
    per_region_alpha = Image.new("L", base.size, 0)
    union_draw = ImageDraw.Draw(combined)
    for region in layout["regions"]:
        points = [(round(p["x"] * width / 941), round(p["y"] * height / 1672))
                  for p in region["contour"]]
        rmask = Image.new("L", base.size, 0)
        ImageDraw.Draw(rmask).polygon(points, fill=255)
        count = sum(1 for p in rmask.getdata() if p)
        assert count > 3000, ("tiny region",region["id"],count)
        assert count < base.width * base.height * .19, ("too broad region",region["id"],count)
        opaque_pixels += count
        union_draw.polygon(points, fill=255)
        records.append({"id":region["id"],"mask_pixels":count,
                        "foot_y":region["foot_y"],
                        "coverage_pct":round(count * 100 / (width * height), 2)})
    mask = combined
    overlay = base.copy()
    overlay.putalpha(mask)
    reconstruction = Image.alpha_composite(base, overlay)
    bbox = ImageChops.difference(reconstruction,base).getbbox()
    assert bbox is None, "A foreground polygon unexpectedly changes static terrain pixels"
    coverage = sum(1 for p in mask.getdata() if p)
    assert coverage <= width * height * .40, "Excessive overlapping wall coverage"
    OUT.mkdir(parents=True, exist_ok=True)
    # This is a diagram of source polygons, NOT a new illustration or true Unity screenshot.
    diagnostic = base.convert("RGB")
    draw = ImageDraw.Draw(diagnostic)
    for region in layout["regions"]:
        pts=[(round(p["x"] * width / 941),round(p["y"] * height / 1672))
             for p in region["contour"]]
        draw.line(pts+[pts[0]], fill=(235,155,60), width=max(2,width//460))
    diagnostic.thumbnail((760,1520),Image.Resampling.LANCZOS)
    preview = OUT / "B_gate_regions_OFFLINE_NOT_UNITY.jpg"
    diagnostic.save(preview, "JPEG", quality=86, optimize=True)
    report = {
        "test":"SOURCE_PIXEL_REGION_PROOF_ONLY",
        "unity_render":False,
        "art_approved":False,
        "source_terrain_sha256":sha(TERRAIN.read_bytes()),
        "dimensions":[width,height],
        "original_alpha_opaque":True,
        "static_roundtrip_identical":True,
        "five_convex_source_regions":len(records)==5,
        "region_coverage_percent":round(100*coverage/(width*height),2),
        "regions":records,
        "preview":str(preview.relative_to(REPO)),
        "limits":"No guarantee of correct wall contour, shader parity, occlusion or WebGL performance"
    }
    (OUT / "offline-evidence.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    print(json.dumps(report,indent=2))
    return report

if __name__ == "__main__":
    check()
