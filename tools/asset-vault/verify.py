#!/usr/bin/env python3
"""Audit Eldoria asset-vault entries against the current Git checkout. Read-only.

Usage:
  python3 tools/asset-vault/verify.py
  python3 tools/asset-vault/verify.py --strict --output /tmp/asset-audit.json

Never deletes, moves, exports, regenerates or auto-promotes an asset.
Historical branch refs are link-only unless the exact file exists in this checkout.
"""
import argparse
import hashlib
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "pipeline/asset-vault/master-catalog-v1.json"
EXTENSIONS = {".blend", ".glb", ".gltf", ".fbx", ".obj", ".stl"}

def digest(path):
    sha = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(4 * 1024 * 1024), b""):
            sha.update(block)
    return sha.hexdigest()

def tracked_3d():
    try:
        names = subprocess.check_output(
            ["git", "-C", str(ROOT), "ls-files", "-z"], stderr=subprocess.DEVNULL
        ).decode("utf-8", errors="replace").split("\0")
    except (OSError, subprocess.CalledProcessError):
        return [], "git ls-files unavailable"
    return sorted(x for x in names if Path(x).suffix.lower() in EXTENSIONS), None

def run():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--strict", action="store_true", help="Exit 2 on missing/hash-drift of an available canonical GLB")
    parser.add_argument("--output", type=Path, help="Write same JSON to an explicit report file")
    a = parser.parse_args()

    source = json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    entries = source.get("records", [])
    ids = [x.get("id") for x in entries]
    if len(ids) != len(set(ids)):
        raise ValueError("Duplicate asset IDs in master catalog")
    canonical = [x for x in entries if x["id"].startswith("canon-")]
    actual, scan_warning = tracked_3d()
    known = {r["path"] for r in entries if r.get("path")}
    for entry in entries:
        if entry.get("editable"):
            known.add(entry["editable"])
        for collection in ("sources", "integration"):
            for variant in entry.get(collection, {}).values():
                if variant.get("path"):
                    known.add(variant["path"])
    checks = []
    for x in canonical:
        path = ROOT / x["path"]
        item = {"id": x["id"], "path": x["path"], "exists": path.is_file(),
                "meta_exists": (ROOT / (x["path"] + ".meta")).is_file(),
                "sha256_result": "unavailable", "license_checked": False}
        if item["exists"]:
            item["sha256_result"] = "matches-recorded-audit" if digest(path) == x["sha256_recorded"] else "DIFFERS-from-2026-10-02-audit"
        checks.append(item)

    extra = [p for p in actual if p not in known]
    result = {
        "schema_version": 1,
        "scope": "read_only_current_git_checkout_not_all_historical_branches",
        "catalog_entries": len(entries),
        "canonical_entries": len(canonical),
        "canonical_present": sum(i["exists"] for i in checks),
        "canonical_meta_present": sum(i["meta_exists"] for i in checks),
        "canonical_hash_matches": sum(i["sha256_result"] == "matches-recorded-audit" for i in checks),
        "tracked_3d_in_this_checkout": len(actual),
        "other_tracked_3d_not_listed_in_curated_v1": len(extra),
        "unreviewed_paths_sample": extra[:50],
        "scan_warning": scan_warning,
        "canonical_checks": checks,
        "historical_note": "Archived branch sources are links, not copied binaries; this scan does not prove all 201 historical branches have been exhaustively inventoried.",
        "status_note": "A file/hash match does not equal ART_APPROVED, performance, provenance or current-camera acceptance."
    }
    payload = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if a.output:
        a.output.parent.mkdir(parents=True, exist_ok=True)
        a.output.write_text(payload, encoding="utf-8")
    sys.stdout.write(payload)
    problems = [x for x in checks if not x["exists"] or not x["meta_exists"] or x["sha256_result"] != "matches-recorded-audit"]
    return 2 if a.strict and problems else 0

if __name__ == "__main__":
    sys.exit(run())
