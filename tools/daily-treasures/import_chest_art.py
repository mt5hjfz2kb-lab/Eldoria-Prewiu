#!/usr/bin/env python3
"""Import *original* Valoria chest sprites without recreating or approximating art.

Usage (on a workspace which actually contains the owner's approved package):
  python tools/daily-treasures/import_chest_art.py --archive /path/to/eldoria_tesoros_valoria_art_fase_final_20261108.zip --repo-root . --dry-run
  python tools/daily-treasures/import_chest_art.py --archive /path/to/...zip --repo-root . --install

Writes only the four files for two actual PNG Sprite assets. It does not run
Unity, accept alternative art hashes, activate gameplay, or grant rewards.
"""
import argparse
import hashlib
import os
from pathlib import Path
import struct
import tempfile
import zipfile

EXPECTED = {
    "treasure_closed_512.png": (
        "313c17d35dcb6f44129a143fabc0448e5a20bc3fb7d00f700394bb73b2d0a0dd",
        "a7d9dd6ab71540fc97dd1c3ca5018f11",
    ),
    "treasure_opened_512.png": (
        "a75afb45b6183d0a8d7e2841c82ef13b812d08fb7620f90c849e679f24429ccb",
        "b8e5609432d94fcf936a700cf0a4b112",
    ),
}
DESTINATION = Path("Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt")

def inspect_png(blob: bytes) -> None:
    if not blob.startswith(b"\x89PNG\r\n\x1a\n") or len(blob) < 65:
        raise ValueError("Original sprite is not a complete PNG")
    width, height = struct.unpack(">II", blob[16:24])
    if (width, height) != (512, 512):
        raise ValueError(f"Wrong sprite size: {width}x{height}")
    # Original optimized PNGs are indexed with alpha and tRNS.
    color_type = blob[25]
    if color_type not in (3, 6):
        raise ValueError(f"Sprite has no supported alpha-capable color mode: {color_type}")
    if color_type == 3 and b"tRNS" not in blob:
        raise ValueError("Indexed PNG has no transparency mask")

def prepare(archive: Path):
    output = {}
    with zipfile.ZipFile(archive, "r") as pack:
        if pack.testzip() is not None:
            raise ValueError("Artwork archive has a corrupt zip member")
        for name, (checksum, guid) in EXPECTED.items():
            png = pack.read("import/" + name)
            if hashlib.sha256(png).hexdigest() != checksum:
                raise ValueError(f"Unexpected or degraded artwork: {name}")
            inspect_png(png)
            meta = pack.read("import/" + name + ".meta")
            text = meta.decode("utf-8")
            if "guid: " + guid + "\n" not in text:
                raise ValueError(f"Wrong Unity GUID: {name}")
            if "textureType: 8" not in text or "spriteMode: 1" not in text:
                raise ValueError(f"Not imported as an independent Sprite: {name}")
            if "alphaIsTransparency: 1" not in text:
                raise ValueError(f"Sprite alpha import not enabled: {name}")
            output[name] = png
            output[name + ".meta"] = meta
    return output

def install(root: Path, payload: dict, dry_run: bool) -> None:
    dest = (root / DESTINATION).resolve()
    if not (root / "Unity/Assets/Eldoria/Scripts/Domain/GameState.cs").is_file():
        raise ValueError("The destination is not the Eldoria Unity repository root")
    if not (root / "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs").is_file():
        raise ValueError("DailyTreasurePanel is missing; do not touch other branches")
    if not (root / "Unity/Assets/Eldoria/DailyTreasures/Resources.meta").is_file():
        raise ValueError("Daily Treasure resource folder meta is not versioned")
    changes = []
    for name, blob in payload.items():
        target = dest / name
        if target.is_file():
            if target.read_bytes() == blob:
                continue
            raise ValueError(f"Existing art differs. Refusing overwrite: {target}")
        changes.append((target, blob))
    for target, blob in changes:
        print(("PLAN" if dry_run else "ADD"), target.relative_to(root),
              len(blob), hashlib.sha256(blob).hexdigest())
    if dry_run:
        print("DRY_RUN. No files modified.")
        return
    dest.mkdir(parents=True, exist_ok=True)
    # Validate everything first; use per-file atomic renames on installation.
    for target, blob in changes:
        with tempfile.NamedTemporaryFile(mode="wb", dir=dest, prefix=".asset-",
                                         delete=False) as temp:
            temp.write(blob)
            tmp = Path(temp.name)
        os.replace(tmp, target)
    # No approval without compiling/inspecting the actual Unity game.
    print("SPRITES_IMPORTED. Unity native render and art review STILL REQUIRED.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--repo-root", type=Path, default=Path("."))
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--install", action="store_true")
    mode.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()
    install(args.repo_root.resolve(), prepare(args.archive), args.dry_run)
