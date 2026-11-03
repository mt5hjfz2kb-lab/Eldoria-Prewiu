#!/usr/bin/env python3
"""Eldoria B11: rebuild a clean environmental backplate, not six duplicates.
Source-preserving/zero-credit offline production. Never replaces approved art.
The six original painterly cutouts remain separate, live Unity objects.
"""
from __future__ import annotations
import hashlib
import json
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageChops, ImageEnhance, ImageFilter

ROOT = Path("Unity/Assets/Eldoria/Resources/EldoriaRetention")
ORIGINAL = ROOT / "vigilants-painterly-v2.png"
B10 = ROOT / "DepthB10"
OUTPUT = ROOT / "DepthB11"
EVIDENCE = Path("docs/evidence/vigilants-b11-source")
SHA = "9503ca15c74e0e41d3268f7af9e4c5a0c24aabf4e4ecbae2a0d64e3585796149"
EXPECTED = ["left_castle", "bridge", "right_castle", "left_rock", "right_cliff", "near_cliff"]
META = """fileFormatVersion: 2
guid: 776494a73bb14331ad5435af8e2e6111
TextureImporter:
  serializedVersion: 12
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  alphaIsTransparency: 0
  alphaSource: 0
  textureType: 0
  spriteMode: 0
"""

def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def main():
    source_bytes = ORIGINAL.read_bytes()
    assert sha(source_bytes) == SHA, "Refuse source change: approved source differs"
    source = Image.open(ORIGINAL).convert("RGB")
    w, h = source.size
    assert (w, h) == (1536, 1024)
    catalog = json.loads((B10 / "layer_index.json").read_text(encoding="utf-8"))
    assert catalog["source_sha256"] == SHA
    assert [x["name"] for x in catalog["layers"]] == EXPECTED

    # Build precise source deletion matte from the six real-alpha B10 cutouts.
    # Shadow + edge safety margin is intentional: when layers move there must
    # never be a second castle/bridge silhouette printed behind them.
    combined = Image.new("L", (w, h), 0)
    originals = {}
    for entry in catalog["layers"]:
        name = entry["name"]
        data = (B10 / (name + ".png")).read_bytes()
        assert sha(data) == entry["sha256"], name + " no longer authentic"
        cut = Image.open(B10 / (name + ".png")).convert("RGBA")
        x, y, bw, bh = entry["pixel_bbox"]
        assert cut.size == (bw, bh)
        alpha = cut.getchannel("A")
        assert alpha.getextrema()[1] > 150
        footprint = Image.new("L", (w, h), 0)
        footprint.paste(alpha, (x, y))
        originals[name] = (cut, x, y)
        combined = ImageChops.lighter(combined, footprint)

    deletion = combined.point(lambda p: 255 if p > 37 else 0)
    deletion = deletion.filter(ImageFilter.MaxFilter(27))
    # At low resolution, Telea fills underlying distant landscape from
    # surrounding real image colors. No AI generation, no invented photography.
    size = (w // 4, h // 4)
    small_rgb = cv2.resize(np.array(source)[:, :, ::-1], size, interpolation=cv2.INTER_AREA)
    small_mask = cv2.resize(np.array(deletion), size, interpolation=cv2.INTER_NEAREST)
    restored_small = cv2.inpaint(small_rgb, small_mask, 13, cv2.INPAINT_TELEA)
    restored = Image.fromarray(restored_small[:, :, ::-1], mode="RGB").resize(
        (w, h), Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(3.5))
    # Preserve every unrelated source pixel, feather only the removed surface.
    feather = deletion.filter(ImageFilter.GaussianBlur(18))
    clean = Image.composite(restored, source, feather)

    # Physical depth cue: distant air slightly cooler and lower contrast.
    # This is deliberately *below* all real cutouts; source pixels never
    # masquerade as two castles. Do not paint a gradient on top of the stage.
    clean = ImageEnhance.Color(clean).enhance(.86)
    clean = ImageEnhance.Contrast(clean).enhance(.93)
    cold = Image.new("RGB", (w, h), (115, 144, 166))
    clean = Image.blend(clean, cold, .075)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    backplate = OUTPUT / "clean_backdrop.png"
    clean.save(backplate, optimize=True)
    (OUTPUT / "clean_backdrop.png.meta").write_text(META, encoding="utf-8")

    # Source-only composition proof. This is NOT a Unity screenshot and not
    # artistic acceptance; the native head will still produce real A/B PNG.
    demo = clean.convert("RGBA")
    depth_tones = {
        "left_castle": (.94, .98, 1.03),
        "bridge": (1.03, 1.02, .96),
        "right_castle": (1.065, 1.035, .965),
        "left_rock": (1.025, 1.015, .99),
        "right_cliff": (1.045, 1.018, .97),
        "near_cliff": (1.06, 1.03, .955),
    }
    offsets = {
        "left_castle": (-4, 1), "bridge": (-2, 2),
        "right_castle": (7, 2), "left_rock": (-4, -3),
        "right_cliff": (6, -4), "near_cliff": (0, 0),
    }
    for name in EXPECTED:
        cut, x, y = originals[name]
        c = np.array(cut).copy()
        tone = depth_tones[name]
        c[:, :, 0:3] = np.clip(c[:, :, 0:3].astype(np.float32) *
                                np.array(tone, dtype=np.float32), 0, 255).astype(np.uint8)
        colored = Image.fromarray(c, mode="RGBA")
        dx, dy = offsets[name]
        # Clip own contact edge; don't shift stage objects or their hit zones.
        layer = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        layer.paste(colored, (x + dx, y + dy), colored)
        demo.alpha_composite(layer)
    preview = EVIDENCE / "B11_source_only_preview_NOT_UNITY.png"
    demo.convert("RGB").save(preview, optimize=True)
    deletion.save(EVIDENCE / "removed_duplicate_source_regions.png", optimize=True)
    source.save(EVIDENCE / "approved_original_for_comparison.png", optimize=True)

    stats = {
        "version": "B11_SOURCE_ONLY", "approval": False,
        "native_unity_capture": False, "original_sha256": SHA,
        "original_preserved": sha(ORIGINAL.read_bytes()) == SHA,
        "clean_backdrop_sha256": sha(backplate.read_bytes()),
        "clean_backdrop_path": str(backplate),
        "removed_regions_coverage": float((np.array(deletion) > 127).mean()),
        "distinct_real_layers": EXPECTED,
        "no_duplicate_complete_painting_in_new_backplate": True,
        "no_paid_credit": True,
        "source_preparation_only": "Native Unity stage+scene+route+interaction still mandatory"
    }
    (OUTPUT / "source_manifest.json").write_text(json.dumps(stats, indent=2) + "\n", encoding="utf-8")
    assert .15 < stats["removed_regions_coverage"] < .90
    print("ELDORIA_B11_CLEAN_BACKPLATE_SOURCE_PASS", stats["removed_regions_coverage"])

if __name__ == "__main__":
    main()
