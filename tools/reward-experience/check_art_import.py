#!/usr/bin/env python3
"""Issue #186: fail-closed SHA/GUID/PNG import preflight, no Unity or image libraries."""
import hashlib
import json
import re
import struct
import sys
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / "Unity/Assets/Eldoria/Issue186Art"
PNG_DIR = BASE / "Resources/Issue186ResourceArt"
MANIFEST = BASE / "issue186_manifest.json"
REPORT = ROOT / "issue186_art_import_preflight.json"


def png_info(blob):
    if not blob.startswith(b"\x89PNG\r\n\x1a\n"):
        raise ValueError("Invalid PNG signature")
    cursor = 8
    width = height = color_type = depth = None
    saw_end = False
    has_idat = False
    while cursor + 12 <= len(blob):
        length = struct.unpack_from(">I", blob, cursor)[0]
        chunk = blob[cursor+4:cursor+8]
        end = cursor + 12 + length
        if end > len(blob):
            raise ValueError("Truncated PNG chunk")
        content = blob[cursor+8:end-4]
        checksum = struct.unpack_from(">I", blob, end-4)[0]
        if zlib.crc32(chunk + content) & 0xffffffff != checksum:
            raise ValueError("Invalid PNG chunk CRC: " + repr(chunk))
        if chunk == b"IHDR":
            if width is not None or length != 13:
                raise ValueError("Duplicate or malformed PNG IHDR")
            width, height, depth, color_type, compress, filt, interlace = (
                struct.unpack(">IIBBBBB", content)
            )
            if not width or not height or compress or filt or interlace:
                raise ValueError("Unsupported PNG IHDR or interlacing")
        if chunk == b"IDAT":
            has_idat = True
        if chunk == b"IEND":
            saw_end = True
            if end != len(blob):
                raise ValueError("Trailing data after PNG IEND")
            break
        cursor = end
    if not saw_end or not has_idat:
        raise ValueError("Missing compressed image or IEND")
    if color_type not in (4, 6) or depth != 8:
        raise ValueError("Art must be 8-bit PNG with actual alpha channel")
    return width, height


def audit():
    errors, warnings, output, ids = [], [], [], set()
    if not MANIFEST.is_file():
        raise SystemExit("Missing art manifest: " + str(MANIFEST))
    obj = json.loads(MANIFEST.read_text(encoding="utf-8"))
    rows = obj.get("artifacts")
    if not isinstance(rows, list) or len(rows) != 16 or obj.get("assetCount") != 16:
        raise SystemExit("Manifest must contain exactly sixteen source assets")
    files = set()
    kinds = {"wood", "stone", "food"}
    tiers = {"100", "1k", "10k", "100k"}
    expected = {f"pack_{kind}_{tier}.png" for kind in kinds for tier in tiers}
    expected |= {"badge_new.png", "badge_claimable.png",
                 "tutorial_pointing_hand.png", "tutorial_target_halo.png"}
    for row in rows:
        name = row.get("file", "")
        if name not in expected or name in files:
            errors.append("Unknown/duplicate filename: " + repr(name))
            continue
        files.add(name)
        png = PNG_DIR / name
        meta = PNG_DIR / (name + ".meta")
        log = {"file": name, "expectedSha256": row.get("sha256", ""),
               "expectedSize": row.get("bytes", 0),
               "expectedWidth": row.get("width", 0),
               "expectedHeight": row.get("height", 0)}
        if not png.is_file() or not meta.is_file():
            errors.append("PNG or .meta missing: " + name)
            output.append(log)
            continue
        blob = png.read_bytes()
        log["actualSha256"] = hashlib.sha256(blob).hexdigest()
        log["actualSize"] = len(blob)
        try:
            width, height = png_info(blob)
            log["actualWidth"], log["actualHeight"] = width, height
        except Exception as exc:
            errors.append(f"{name}: {exc}")
        if log["actualSha256"] != row.get("sha256"):
            errors.append(f"{name}: expected SHA {row.get('sha256')} actual {log['actualSha256']}")
        if len(blob) != row.get("bytes"):
            errors.append(f"{name}: expected bytes {row.get('bytes')} actual {len(blob)}")
        if (log.get("actualWidth"), log.get("actualHeight")) != (row.get("width"), row.get("height")):
            errors.append(f"{name}: expected dimensions {row.get('width')}x{row.get('height')} "
                          f"actual {log.get('actualWidth')}x{log.get('actualHeight')}")
        md = meta.read_text(encoding="utf-8")
        match = re.search(r"^guid:\s*([a-f0-9]{32})\s*$", md, re.MULTILINE)
        guid = match.group(1) if match else None
        log["actualGuid"] = guid
        if not guid or guid != row.get("guid") or guid in ids:
            errors.append(f"{name}: Unity GUID invalid, duplicated or changed")
        if guid:
            ids.add(guid)
        for req in ("textureType: 8", "spriteMode: 1", "alphaIsTransparency: 1",
                    "enableMipMap: 0"):
            if req not in md:
                errors.append(f"{name}: importer missing {req}")
        if row.get("key") != "Issue186ResourceArt/" + name[:-4]:
            errors.append(f"{name}: Resources.Load key mismatches name")
        output.append(log)
    if files != expected:
        errors.append("Missing resources: " + ",".join(sorted(expected - files)))
    actual_pngs = {p.name for p in PNG_DIR.glob("*.png")}
    if actual_pngs != expected:
        errors.append("Unexpected/missing imported PNG files: " + repr(sorted(actual_pngs ^ expected)))
    report = {"status": "FAIL" if errors else "PASS", "checked": len(output),
              "shaAndMetaActual": output, "errors": errors, "warnings": warnings,
              "note": "Source/import preflight only. Not actual Unity, mobile, or art approval."}
    REPORT.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    for error in errors:
        print("ISSUE186_IMPORT_ERROR:", error)
    print(f"ISSUE186_SOURCE_IMPORT_{report['status']}: checked={len(output)} "
          f"errors={len(errors)} report={REPORT}")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(audit())
