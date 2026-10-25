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

def tracked_versions():
    # Read Git tree SHA identifiers, not model bytes. Detect both new models
    # and changed existing .glb/.blend/.fbx at the very same path.
    entries = []
    for raw in git("ls-tree", "-r", "-z", "HEAD").decode("utf-8", "replace").split("\0"):
        if not raw or "\t" not in raw:
            continue
        meta, path = raw.split("\t", 1)
        if Path(path).suffix.lower() not in EXTS:
            continue
        columns = meta.split()
        if len(columns) >= 3 and columns[1] == "blob":
            entries.append((path, columns[2]))
    return sorted(entries)

def tracked():
    return [path for path, oid in tracked_versions()]


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
    all_assets = tracked_versions()
    all_paths = [path for path, oid in all_assets]
    known = known_paths(catalog)
    baseline_file = ROOT / "pipeline/asset-vault/current-full-index-v1.json"
    baseline = (json.loads(baseline_file.read_text(encoding="utf-8-sig"))
                if baseline_file.is_file() else {})
    original_oids = {item["path"]: item.get("git_blob_sha1")
                     for item in baseline.get("records", [])}
    candidates = []
    for path, oid in all_assets:
        old_oid = original_oids.get(path)
        if old_oid == oid:
            continue
        if not old_oid and path in known:
            continue
        item = {
            "path": path,
            "git_blob_sha1": oid,
            "previous_git_blob_sha1": old_oid,
            "change_type": "MODIFIED_EXISTING_SOURCE" if old_oid else "NEW_3D_FILE",
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
        "new_file_count": sum(x["change_type"] == "NEW_3D_FILE" for x in candidates),
        "modified_file_count": sum(x["change_type"] == "MODIFIED_EXISTING_SOURCE" for x in candidates),
        "candidates": candidates,
        "rule": "Discovery is automatic; reuse approval requires evidence and human/art review. Never copy or replace binaries.",
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    summary = [
        "# Eldoria asset vault: automatic discovery",
        "",
        f"Tracked 3D files: **{len(all_paths)}**. Known in curated catalog: **{result['known_catalog_paths']}**.",
        f"New/changed 3D candidates: **{len(candidates)}**.",
        "",
        "This is read-only. Unreviewed candidates are NOT art-approved.",
        "",
    ]
    for entry in candidates[:80]:
        summary.append(f"- `{entry['path']}` — {entry['change_type']}: {entry['category']}")
    if len(candidates) > 80:
        summary.append(f"- ...and {len(candidates) - 80} more (full JSON in artifact)")
    if args.summary:
        args.summary.parent.mkdir(parents=True, exist_ok=True)
        args.summary.write_text("\n".join(summary) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "candidates"}, ensure_ascii=False))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
