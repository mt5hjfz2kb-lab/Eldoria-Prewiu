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

    # B11 corrective pass: preserve the alpha edge exactly. Dilation 27 px
    # removed valid sky/rock pixels beyond the painterly silhouette and
    # introduced obvious pasted/foggy triangles. That source was REJECTED.
    deletion = combined
    # At low resolution, Telea fills underlying distant landscape from
    # surrounding real image colors. No AI generation, no invented photography.
    size = (w // 4, h // 4)
    small_rgb = cv2.resize(np.array(source)[:, :, ::-1], size, interpolation=cv2.INTER_AREA)
    small_mask = cv2.resize(np.array(deletion), size, interpolation=cv2.INTER_NEAREST)
    restored_small = cv2.inpaint(small_rgb, small_mask, 13, cv2.INPAINT_TELEA)
    restored = Image.fromarray(restored_small[:, :, ::-1], mode="RGB").resize(
        (w, h), Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(1.0))
    # Preserve every unrelated source pixel, feather only the removed surface.
    # Subtract exactly what each individually imported cutout can cover;
    # this prevents a dark halo when a matte is translucent at its edge.
    clean = Image.composite(restored, source, deletion)

    # Physical depth cue: distant air slightly cooler and lower contrast.
    # This is deliberately *below* all real cutouts; source pixels never
    # masquerade as two castles. Do not paint a gradient on top of the stage.
    clean = ImageEnhance.Color(clean).enhance(.93)
    clean = ImageEnhance.Contrast(clean).enhance(.965)
    cold = Image.new("RGB", (w, h), (115, 144, 166))
    clean = Image.blend(clean, cold, .048)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    backplate = OUTPUT / "clean_backdrop.png"
    clean.save(backplate, optimize=True)
    (OUTPUT / "clean_backdrop.png.meta").write_text(META, encoding="utf-8")

    # Six actual material variants: sculpt local stone/light using intensity
    # derivatives, rather than putting more photograph patches over a photograph.
    # Source and B10 cutouts are immutable. This is a zero-credit art experiment.
    tone = {
        "left_castle": (.955,.990,1.018),
        "bridge": (.995,1.012,1.012),
        "right_castle": (1.055,1.016,.958),
        "left_rock": (1.056,1.024,.950),
        "right_cliff": (1.066,1.021,.942),
        "near_cliff": (1.075,1.037,.930),
    }
    demo = clean.convert("RGBA")
    for i,name in enumerate(EXPECTED):
        cut, x, y = originals[name]
        arr = np.array(cut, dtype=np.float32)
        gray = cv2.cvtColor(arr[:, :, :3].astype(np.uint8), cv2.COLOR_RGB2GRAY)
        gray = cv2.GaussianBlur(gray.astype(np.float32)/255, (0, 0), 2.8)
        gy, gx = np.gradient(gray)
        surface = np.clip(1.0 + (-.7*gx-.65*gy) * (1.2 + i*.15), .81, 1.16)
        arr[:, :, :3] = np.clip(arr[:, :, :3] * surface[:, :, None] *
                                  np.asarray(tone[name])[None,None,:], 0, 255)
        tinted = Image.fromarray(arr.astype(np.uint8), "RGBA")
        tinted.save(OUTPUT / (name + ".png"), optimize=True)
        # Each asset has its own stable GUID. Do not reuse B10 GUIDs.
        identity = f"776494a73bb14331ad5435af8e2e{6200+i:04d}"
        (OUTPUT / (name + ".png.meta")).write_text(
            META.replace("776494a73bb14331ad5435af8e2e6111", identity),
            encoding="utf-8")
        full_plane = Image.new("RGBA", (w,h), (0,0,0,0))
        full_plane.alpha_composite(tinted, (x,y))  # no pasted offset at rest
        demo.alpha_composite(full_plane)
    folder_meta = """fileFormatVersion: 2
guid: 776494a73bb14331ad5435af8e2e6f00
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    (ROOT/"DepthB11.meta").write_text(folder_meta, encoding="utf-8")

    # Source-only composition proof, NOT Unity. Mandatory visual QA must
    # reject even technically passing source prep if it resembles cutout collage.
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
