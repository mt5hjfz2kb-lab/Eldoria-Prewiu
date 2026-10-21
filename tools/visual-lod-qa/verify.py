#!/usr/bin/env python3
"""Exact provenance / structural checks for zero-credit Blender mobile LOD QA.
Reuses tools/art-rd/audit_glb.py, NEVER reports ART_APPROVED/MOBILE_PASS.
"""
import argparse
import hashlib
import json
import pathlib
import struct
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools" / "art-rd"))
from audit_glb import audit  # noqa: E402

IDS = {"boar", "focus"}
PNG_SIGNATURE = bytes.fromhex("89504e470d0a1a0a")


def sha256(path):
    h = hashlib.sha256()
    with pathlib.Path(path).open("rb") as src:
        for block in iter(lambda: src.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def valid_png(path, expected=640):
    raw = pathlib.Path(path).read_bytes()
    if len(raw) < 5000 or raw[:8] != PNG_SIGNATURE:
        raise ValueError(f"Not a genuine nonempty PNG: {path}")
    width, height = struct.unpack(">II", raw[16:24])
    if (width, height) != (expected, expected):
        raise ValueError(f"Wrong LOD screenshot resolution: {path}")
    return {"name": pathlib.Path(path).name, "sha256": sha256(path), "bytes": len(raw)}


def structural_report(glb):
    row = audit(str(glb))
    if row["errors"]:
        raise ValueError(f"Invalid GLB {glb}: {row['errors']}")
    if any("missing" in w.lower() and ("normal" in w.lower() or "uv0" in w.lower()) for w in row["warnings"]):
        raise ValueError(f"UV/normal lost: {glb}")
    if row["meshes"] < 1 or row["triangles_estimated"] < 100 or row["materials"] < 1 or row["images"] < 1:
        raise ValueError(f"Unusable geometry or missing PBR maps: {glb}")
    if row["external_images"]:
        raise ValueError(f"LOD has non-embedded texture dependencies: {glb}")
    return row


def validate(name, profile_path, lod_dir, renders):
    if name not in IDS:
        raise ValueError("Unsupported QA asset")
    config = json.loads(pathlib.Path(profile_path).read_text("utf8"))
    if config.get("schema_version") != 1 or config.get("no_api_spend") is not True or config.get("art_approved") is not False:
        raise ValueError("QA profile must explicitly refuse spending and approval")
    item = config["assets"][name]
    source_path = ROOT / item["source"]
    if not source_path.is_file() or sha256(source_path) != item["sha256"]:
        raise ValueError("Source path/SHA mismatch (stop without regenerating)")
    source = structural_report(source_path)
    if source["triangles_estimated"] != item["source_triangles"]:
        raise ValueError("Source polygon count drift")
    if source["has_skins"] or source["has_animations"]:
        raise ValueError("A supposedly static source unexpectedly carries rig/animation")
    ratios = item["lod_ratios"]
    ceilings = item["lod_max_triangles"]
    if len(ratios) != 3 or len(ceilings) != 3 or ratios[0] != 1.0 or not (1 > ratios[1] > ratios[2] > 0):
        raise ValueError("Invalid LOD profile")
    report_path = pathlib.Path(lod_dir) / "blender-lods.json"
    blender = json.loads(report_path.read_text("utf8"))
    if blender.get("tripo_credits") != 0 or len(blender.get("lods", [])) != 3:
        raise ValueError("Blender LOD proof missing explicit zero-credit result")
    actual = []
    last_tris = source["triangles_estimated"] + 1
    for i in range(3):
        p = pathlib.Path(lod_dir) / f"lod{i}.glb"
        row = structural_report(p)
        tris = row["triangles_estimated"]
        claimed = blender["lods"][i]
        if claimed["lod"] != i or claimed["metrics"]["triangles"] != tris:
            raise ValueError(f"Blender report differs from inspected GLB LOD{i}")
        if abs(float(claimed["ratio_requested"]) - ratios[i]) > 1e-8:
            raise ValueError(f"Blender ratio drift for LOD{i}")
        if not (100 <= tris <= ceilings[i] and tris < last_tris):
            raise ValueError(f"LOD{i} triangle budget/order fail: {tris}")
        if row["images"] < source["images"]:
            raise ValueError(f"LOD{i} lost source PBR textures")
        if row["has_skins"] or row["has_animations"]:
            raise ValueError(f"LOD{i} must not fabricate rig/animation")
        actual.append({
            "lod": i, "triangles": tris, "triangles_saved_percent": round(100 * (1 - tris/source["triangles_estimated"]), 2),
            "bytes": p.stat().st_size, "sha256": sha256(p),
            "meshes": row["meshes"], "materials": row["materials"], "embedded_images": row["images"]
        })
        last_tris = tris
    previews = []
    for i in range(3):
        for view in ["isometric", "strategic"]:
            previews.append(valid_png(pathlib.Path(renders) / f"lod{i}-{view}.png"))
    render_evidence = json.loads((pathlib.Path(renders) / "render-evidence.json").read_text("utf8"))
    if render_evidence.get("visual_verdict") != "REVIEW_REQUIRED" or render_evidence.get("tripo_credits_used") != 0:
        raise ValueError("False visual-approval metadata")
    if render_evidence.get("source_sha256") != item["sha256"] or len(render_evidence.get("captures", [])) != 6:
        raise ValueError("Mismatched camera/source evidence")
    if {row["sha256"] for row in render_evidence["captures"]} != {x["sha256"] for x in previews}:
        raise ValueError("Preview SHA mismatch")
    return {
        "asset": name, "model": item["label"], "original_glb_sha256": item["sha256"],
        "original_triangles": source["triangles_estimated"], "source_bytes": source_path.stat().st_size,
        "lod_candidates": actual, "captures": previews,
        "same_locked_blender_camera": True,
        "new_credits_spent": 0,
        "qwen_art_authoring": "NOT_EXECUTED",
        "technical_qa": "PASS",
        "unity_integrated_qa": "NOT_EXECUTED",
        "art_approved": False, "mobile_device_profiled": False,
        "release_eligible": False,
        "next_gate": "Independent visual comparison and actual Unity strategic-camera render; Android FPS/RAM before any replacement."
    }


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--asset", choices=sorted(IDS), required=True)
    p.add_argument("--profiles", required=True)
    p.add_argument("--lod-dir", required=True)
    p.add_argument("--renders", required=True)
    p.add_argument("--out", required=True)
    a = p.parse_args()
    result = validate(a.asset, a.profiles, a.lod_dir, a.renders)
    target = pathlib.Path(a.out)
    if target.exists():
        raise FileExistsError("Never overwrite existing LOD evidence")
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", "utf8")
    print("ELDORIA_TRIPO_LOD_ZERO_CREDIT_TECH_PASS", a.asset, [x["triangles"] for x in result["lod_candidates"]])


if __name__ == "__main__":
    main()
