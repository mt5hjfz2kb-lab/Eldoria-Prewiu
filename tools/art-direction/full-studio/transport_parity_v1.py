"""Run SAME original Qwen plan on SAME frozen source in direct and BlenderMCP.

This is an explicitly invoked, zero-spend, technical comparison. It creates
two fresh isolated outputs, requires the pinned local BlenderMCP addon, and
never modifies or approves an artistic source. Not a Qwen generator.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import time

from artist_capabilities_v1 import strict_preflight_plan


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def run_manifest(path):
    """Extract verifiable geometric evidence, not aesthetic scores."""
    root = Path(path)
    proof = json.loads((root / "execution-proof.json").read_text(encoding="utf-8"))
    catalog = json.loads((root / "reusable-asset-catalog.json").read_text(encoding="utf-8"))
    components = catalog.get("components", [])
    geom = sorted((str(x.get("name")), str(x.get("geometry_sha256")))
                  for x in components)
    before = root / "before-blender.png"
    after = root / "after-blender.png"
    for p in (before, after, root / "artist-asset.glb", root / "artist-asset.blend"):
        if not p.is_file():
            raise RuntimeError("MISSING_GENUINE_TRANSPORT_EVIDENCE: " + str(p))
    return {
        "transport": proof.get("transport"),
        "blender_execution": proof.get("blender_execution"),
        "matched_review_camera": proof.get("matched_review_camera"),
        "action_count": proof.get("action_count"),
        "execution": [{k: a.get(k) for k in
                       ("op", "geometry_changed", "spatial_transform_changed",
                        "originals_changed")}
                      for a in proof.get("execution", [])],
        "source_component_geometry": geom,
        "before_sha256": digest(before),
        "after_sha256": digest(after),
        "output_blend_sha256": digest(root / "artist-asset.blend"),
        "output_glb_sha256": digest(root / "artist-asset.glb"),
        "art_approved": False,
        "unity_tested": False,
    }


def compare_manifests(direct, mcp):
    """Parity is geometric/operational; JPEG/PNG byte equality is NOT required."""
    if direct.get("transport") != "direct" or mcp.get("transport") != "blendermcp":
        raise ValueError("TRANSPORT_LABELED_WRONG")
    return {
        "both_ran_blender": bool(direct.get("blender_execution")
                               and mcp.get("blender_execution")),
        "action_counts_match": direct.get("action_count") == mcp.get("action_count"),
        "same_action_effects": direct.get("execution") == mcp.get("execution"),
        "same_component_geometry": (
            direct.get("source_component_geometry") ==
            mcp.get("source_component_geometry")),
        "both_matched_camera": bool(direct.get("matched_review_camera")
                                    and mcp.get("matched_review_camera")),
        "pixel_identical_required": False,
        "art_approved": False,
    }


def main():
    parser = argparse.ArgumentParser()
    for key in ("source", "original-sha256", "plan", "reference",
                "review", "blender", "mcp-addon", "mcp-sha256", "out"):
        parser.add_argument("--" + key, required=True)
    parser.add_argument("--texture-root", default=".")
    args = parser.parse_args()

    src, plan, ref, review, addon, out = map(Path, (
        args.source, args.plan, args.reference, args.review, args.mcp_addon, args.out))
    for p in (src, plan, ref, review, addon):
        if not p.is_file():
            raise FileNotFoundError("PARITY_INPUT_MISSING: " + str(p))
    if digest(src) != args.original_sha256.lower():
        raise ValueError("ORIGINAL_SOURCE_HASH_MISMATCH")
    if digest(addon) != args.mcp_sha256.lower():
        raise ValueError("PINNED_MCP_ADDON_HASH_MISMATCH")
    document = json.loads(plan.read_text(encoding="utf-8"))
    preflight = strict_preflight_plan(document)
    if out.exists() and any(out.iterdir()):
        raise ValueError("FRESH_PARITY_OUTPUT_REQUIRED")
    out.mkdir(parents=True, exist_ok=True)

    # Imports of the actual studio happen ONLY after all cheap checks.
    from artist_loop import run_blender
    results = {}
    for name in ("direct", "blendermcp"):
        before_source = digest(src)
        started = time.monotonic()
        run_blender(str(args.blender), plan, out / name, src,
                    Path(args.texture_root), review, None, name,
                    addon if name == "blendermcp" else None,
                    args.mcp_sha256 if name == "blendermcp" else None)
        if digest(src) != before_source:
            raise RuntimeError("FROZEN_SOURCE_MUTATED")
        results[name] = {"seconds": round(time.monotonic() - started, 3),
                         **run_manifest(out / name)}
        (out / "transport-parity-progress.json").write_text(
            json.dumps(results, indent=2), encoding="utf-8")
    comparison = compare_manifests(results["direct"], results["blendermcp"])
    report = {
        "scope": "TECHNICAL_PARITY_OF_IDENTICAL_QWEN_PLAN_ONLY",
        "original_sha256": digest(src), "plan_sha256": digest(plan),
        "reference_sha256": digest(ref), "review_sha256": digest(review),
        "addon_sha256": digest(addon), "preflight": preflight,
        "runs": results, "comparison": comparison,
        "conclusion": ("TECHNICAL_GEOMETRY_PARITY" if
                       all(comparison[k] for k in ("both_ran_blender",
                          "action_counts_match", "same_action_effects",
                          "same_component_geometry", "both_matched_camera"))
                       else "TECHNICAL_PARITY_DISAGREEMENT"),
        "independent_art_review_performed": False,
        "art_approved": False, "calibration_validated": False,
    }
    (out / "transport-parity-result.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({"conclusion": report["conclusion"],
                      "comparisons": comparison}))
    if report["conclusion"] != "TECHNICAL_GEOMETRY_PARITY":
        raise SystemExit(2)


if __name__ == "__main__":
    main()
