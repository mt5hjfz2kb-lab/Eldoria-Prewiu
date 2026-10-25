#!/usr/bin/env python3
"""Read-only 3D candidate inventory, for every PR/main commit. No asset promotion."""
import argparse
import json
import subprocess
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EXTS = {".blend", ".glb", ".gltf", ".fbx", ".obj", ".stl"}
CATALOG = ROOT / "pipeline/asset-vault/master-catalog-v1.json"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args], stderr=subprocess.PIPE)

def tracked():
    # Names only: avoid git ls-tree -l in a partial clone, because obtaining
    # missing blob sizes can trigger lazy download of huge Blender/GLB files.
    return sorted(p for p in git("ls-tree", "-r", "--name-only", "-z", "HEAD")
                  .decode("utf-8", "replace").split("\0")
                  if p and Path(p).suffix.lower() in EXTS)


def known_paths(catalog):
    known = set()
    for item in catalog["records"]:
        for k in ("path", "editable"):
            if item.get(k):
                known.add(item[k])
        for collection in ("sources", "integration"):
            for sub in item.get(collection, {}).values():
                if isinstance(sub, dict) and sub.get("path"):
                    known.add(sub["path"])
    # Historical backlog already enumerated in the permanent full-current index.
    # Its entries remain unapproved but should not flood every future PR as "new".
    baseline = ROOT / "pipeline/asset-vault/current-full-index-v1.json"
    if baseline.is_file():
        index = json.loads(baseline.read_text(encoding="utf-8-sig"))
        known.update(x["path"] for x in index.get("records",[]) if x.get("path"))
    return known

def classify(path):
    p = path.lower()
    if "/arttests/" in p or "/experimental" in p or "/variants/" in p:
        return "EXPERIMENTAL_NEEDS_REVIEW"
    if p.endswith(".blend"):
        return "EDITABLE_SOURCE_NEEDS_REVIEW"
    if "creature" in p or "spawnling" in p or "boar" in p:
        return "CREATURE_NEEDS_REVIEW"
    return "ASSET_NEEDS_REVIEW"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=Path("/tmp/eldoria-asset-candidates.json"))
    parser.add_argument("--summary", type=Path)
    args = parser.parse_args()
    catalog = json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    all_paths = tracked()
    known = known_paths(catalog)
    candidates = []
    for path in all_paths:
        if path in known:
            continue
        item = {
            "path": path,
            "category": classify(path),
            "ref": "CURRENT_CHECKOUT",
            "size_bytes": None,
            "status": "DISCOVERED_UNREVIEWED",
            "art_approved": False,
            "provenance_reviewed": False,
            "license_reviewed": False,
            "quality_reviewed": False,
            "automatic_promotion_allowed": False,
        }
        candidates.append(item)
    result = {
        "schema_version": 1,
        "kind": "automatic_candidate_discovery",
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "source_commit": git("rev-parse", "HEAD").decode().strip(),
        "total_tracked_3d": len(all_paths),
        "known_catalog_paths": len(set(all_paths) & known),
        "new_unreviewed": len(candidates),
        "candidates": candidates,
        "rule": "Discovery is automatic; reuse approval requires evidence and human/art review. Never copy or replace binaries.",
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    summary = [
        "# Eldoria asset vault: automatic discovery",
        "",
        f"Tracked 3D files: **{len(all_paths)}**. Known in curated catalog: **{result['known_catalog_paths']}**.",
        f"Unreviewed candidates: **{len(candidates)}**.",
        "",
        "This is read-only. Unreviewed candidates are NOT art-approved.",
        "",
    ]
    for entry in candidates[:80]:
        summary.append(f"- `{entry['path']}` — {entry['category']}")
    if len(candidates) > 80:
        summary.append(f"- ...and {len(candidates) - 80} more (full JSON in artifact)")
    if args.summary:
        args.summary.parent.mkdir(parents=True, exist_ok=True)
        args.summary.write_text("\n".join(summary) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "candidates"}, ensure_ascii=False))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
