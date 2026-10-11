#!/usr/bin/env python3
"""Stage Eldoria's already-proven art studio on demand without modifying tracked files.

Not a new agent. This is a reversible cross-branch access bridge: --check inventories
the existing capabilities and --stage copies exact sources to an isolated directory.
"""
import argparse
import json
import pathlib
import shutil
import subprocess
import sys

SOURCE_REF = "origin/art/studio-reuse-handoff-20261011"
ROOTS = ("tools/art-studio-recovery/", "tools/art-studio-reuse/")
STUDIO_FILES = (
    "tools/art-studio-recovery/REPRODUCE.md",
    "tools/art-studio-recovery/review_with_local_vision.py",
    "tools/art-studio-recovery/review_v2_advisory.py",
    "tools/art-studio-recovery/mcp_repair_contacts.py",
    "tools/art-studio-recovery/decision_policy.py",
    "tools/art-studio-recovery/produce_rock_study.py",
    "tools/art-studio-recovery/extract_cc0.py",
    "tools/art-studio-reuse/prepare-task.mjs",
    "tools/art-studio-reuse/README.md",
)
def git(*args):
    return subprocess.run(["git", *args], check=True, capture_output=True).stdout

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--stage", type=pathlib.Path,
                        help="Stage all existing studio source into an isolated, untracked directory.")
    args = parser.parse_args()
    if not args.check and args.stage is None:
        parser.error("provide --check or --stage")
    try:
        git("rev-parse", "--show-toplevel")
        sha = git("rev-parse", SOURCE_REF).decode().strip()
        paths = git("ls-tree", "-r", "--name-only", SOURCE_REF).decode().splitlines()
        studio_paths = [p for p in paths if p.startswith(ROOTS)]
        missing = [p for p in STUDIO_FILES if p not in studio_paths]
        if missing:
            raise RuntimeError("Missing required upstream sources: " + ", ".join(missing))
        staged = 0
        if args.stage:
            dest = args.stage.resolve()
            repo = pathlib.Path(git("rev-parse", "--show-toplevel").decode().strip())
            if dest == repo or repo in dest.parents:
                raise RuntimeError("Stage outside the repository to avoid accidental commits and overwrites")
            if dest.exists() and any(dest.iterdir()):
                raise RuntimeError("Destination must be empty; existing studio content is never overwritten")
            for relative in studio_paths:
                output = dest / relative
                output.parent.mkdir(parents=True, exist_ok=True)
                output.write_bytes(git("show", f"{SOURCE_REF}:{relative}"))
                staged += 1
        print(json.dumps({
            "source_ref": SOURCE_REF,
            "source_sha": sha,
            "available_files": len(studio_paths),
            "staged_files": staged,
            "capabilities": {
                "blender_mcp_contact_repair": "source_available; runtime unverified; rock-specific",
                "ollama_qwen_visual_review": "source_available; local inference runtime unverified",
                "cc0_donor_extract": "source_available; verify license and donor for gatehouse",
                "production_scripts": "source_available; rock-specific recipes require adaptation",
                "unity": "not tested by bridge",
            },
            "note": "Only reusable source access verified, NOT art quality or live model/MCP use.",
        }, indent=2))
    except (subprocess.CalledProcessError, RuntimeError, OSError) as exc:
        print(f"ELDORIA_ART_STUDIO_BRIDGE_FAIL: {exc}", file=sys.stderr)
        return 1
    return 0
if __name__ == "__main__":
    sys.exit(main())
