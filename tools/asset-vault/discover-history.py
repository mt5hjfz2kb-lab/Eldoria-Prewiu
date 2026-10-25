#!/usr/bin/env python3
"""Enumerate real 3D files in pinned archived commits available in local Git objects.

Read-only, no fetch, no cherry-pick, no checkout, no merge, no deletion.
A shallow clone will report incomplete coverage instead of claiming completion.
"""
import argparse
import json
import subprocess
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "pipeline/historical-branches-20261018.json"
EXTENSIONS = (".blend", ".glb", ".gltf", ".fbx", ".obj", ".stl")

def git(*args):
    p = subprocess.run(["git", "-C", str(ROOT), *args],
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
    return p.returncode, p.stdout.decode("utf-8", "replace")

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, help="Optional report destination")
    a = parser.parse_args()
    refs = json.loads(SOURCE.read_text(encoding="utf-8-sig"))["branches"]
    by_path = defaultdict(list)
    missing = []
    examined = 0
    for ref in refs:
        sha, name = ref["sha"], ref["name"]
        code, listing = git("ls-tree", "-r", "--name-only", sha)
        if code:
            missing.append({"branch": name, "sha": sha, "reason": "Commit object unavailable in local clone"})
            continue
        examined += 1
        for path in listing.splitlines():
            if path.lower().endswith(EXTENSIONS):
                by_path[path].append({"branch": name, "sha": sha})
    payload = {
        "schema_version": 1,
        "mode": "read_only_historical_git_tree_scan",
        "declared_refs": len(refs),
        "complete_refs": examined,
        "unavailable_refs": len(missing),
        "complete_201_tree_audit": examined == len(refs),
        "unique_3d_paths": len(by_path),
        "candidate_paths": [{"path": p, "refs": r} for p, r in sorted(by_path.items())],
        "missing_objects": missing,
        "warning": "File presence does not prove approved design, license, animation, mesh quality or recoverability after refs are deleted. No binary files have been copied."
    }
    report = json.dumps(payload, ensure_ascii=False, indent=2) + "\n"
    if a.output:
        a.output.parent.mkdir(parents=True, exist_ok=True)
        a.output.write_text(report, encoding="utf-8")
    else:
        print(report)
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
