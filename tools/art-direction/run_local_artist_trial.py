#!/usr/bin/env python3
"""Run the established zero-paid-credit Valoria Qwen -> BlenderMCP -> GLB trial locally.

Explicit opt-in: --run. Never alters the frozen source or main branch.
Example:
  python tools/art-direction/run_local_artist_trial.py --capture C:/captures/cliff.png --addon C:/tools/blender-mcp/addon.py --run
"""
import argparse
import hashlib
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
PIN = "0abf13870d21725c30acf3f3e3a8c86385470073"
SOURCE_PATH = "art-source/valoria/production/asset-family-v2/quality-v3/cliff_rocks/cliff_rocks.blend"
TARGET_PATH = "references/VALORIA_FAMILY_OWNER_TARGET_20261011.jpg"
AUTHOR = ROOT / "tools/art-direction/build_cliff_talus_transition_v1.py"
PINNED_ADDON_HASH = "eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5"


def git(*args):
    return subprocess.run(["git", *args], cwd=ROOT, check=True,
                          capture_output=True).stdout


def ensure_pinned_file(relative):
    destination = ROOT / relative
    data = git("show", f"{PIN}:{relative}")
    if destination.exists():
        if destination.read_bytes() != data:
            raise RuntimeError(f"Refusing to replace existing nonmatching frozen input: {destination}")
        return destination
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(data)
    return destination


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--capture", type=Path, required=True, help="Genuine Unity PNG, not mock imagery")
    parser.add_argument("--addon", type=Path, required=True, help="Pinned BlenderMCP addon.py")
    parser.add_argument("--blender", default="blender", help="Blender executable or full path")
    parser.add_argument("--run", action="store_true", help="Explicitly allow inference and variant generation")
    args = parser.parse_args()
    branch = git("branch", "--show-current").decode().strip()
    if branch != "art/local-ai-professional-artist-role-20261012":
        raise SystemExit(f"Wrong branch: {branch}. Do not work on main or the Unity branch.")
    if not args.capture.is_file() or args.capture.stat().st_size < 20000:
        raise SystemExit("Missing/invalid real Unity capture")
    if not args.addon.is_file() or hashlib.sha256(args.addon.read_bytes()).hexdigest() != PINNED_ADDON_HASH:
        raise SystemExit("Missing/unverified BlenderMCP addon; require pinned SHA256")
    binary = shutil.which(args.blender) or (str(args.blender) if Path(args.blender).is_file() else None)
    if not binary:
        raise SystemExit("Blender not found; provide --blender full/path/to/blender[.exe]")
    if not AUTHOR.is_file():
        raise SystemExit(f"Artist authoring script missing: {AUTHOR}")
    if not args.run:
        print("PREFLIGHT PASS. Add --run to stage pinned inputs and execute Qwen + BlenderMCP.")
        return 0
    source = ensure_pinned_file(SOURCE_PATH)
    target = ensure_pinned_file(TARGET_PATH)
    env = dict(os.environ, VALORIA_UNITY_CLIFF_SCREENSHOT=str(args.capture.resolve()),
               ELDORIA_BLENDER_MCP_ADDON=str(args.addon.resolve()))
    print(f"PINNED_INPUTS source={source} target={target}", flush=True)
    subprocess.run([sys.executable, str(AUTHOR), "--infer"], cwd=ROOT, env=env, check=True)
    subprocess.run([binary, "-b", "--python-exit-code", "1", "-t", "4",
                    "--python", str(AUTHOR)], cwd=ROOT, env=env, check=True)
    print("TECHNICAL VARIANT GENERATED. Inspect before/after PNG and GLB in "
          "art-source/valoria/variants/cliff_talus_transition_v1/. Unity visual approval remains separate.")


if __name__ == "__main__":
    raise SystemExit(main())
