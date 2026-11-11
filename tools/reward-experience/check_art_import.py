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



def alpha_matte_stats(blob):
    """Decode actual 8-bit, noninterlaced PNG alpha with all five row filters.

    No Pillow dependency. This is SOURCE evidence, not perceived-art approval.
    """
    w, h, depth, color_type, _, _, interlace = struct.unpack_from(">IIBBBBB", blob, 16)
    if depth != 8 or color_type not in (4, 6) or interlace:
        raise ValueError("Unsupported alpha decoding format")
    bpp = 4 if color_type == 6 else 2
    stride = w * bpp
    ptr = 8
    compressed = bytearray()
    while ptr + 12 <= len(blob):
        length = struct.unpack_from(">I", blob, ptr)[0]
        kind = blob[ptr+4:ptr+8]
        if kind == b"IDAT":
            compressed.extend(blob[ptr+8:ptr+8+length])
        ptr += length + 12
        if kind == b"IEND":
            break
    raw = zlib.decompress(compressed)
    if len(raw) != h * (stride + 1):
        raise ValueError("PNG scanline size mismatch")
    prev = bytearray(stride)
    total_clear = border_clear = border_total = 0
    corners = [None, None, None, None]
    center = None
    for y in range(h):
        off = y * (stride + 1)
        filter_kind = raw[off]
        if filter_kind > 4:
            raise ValueError("Invalid PNG row filter " + str(filter_kind))
        src = raw[off+1:off+1+stride]
        cur = bytearray(stride)
        for j, val in enumerate(src):
            a = cur[j-bpp] if j >= bpp else 0
            above = prev[j]
            c = prev[j-bpp] if j >= bpp else 0
            if filter_kind == 1:
                predictor = a
            elif filter_kind == 2:
                predictor = above
            elif filter_kind == 3:
                predictor = (a + above) // 2
            elif filter_kind == 4:
                p = a + above - c
                da, db, dc = abs(p-a), abs(p-above), abs(p-c)
                predictor = a if da <= db and da <= dc else above if db <= dc else c
            else:
                predictor = 0
            cur[j] = (val + predictor) & 255
        for x in range(w):
            a = cur[x*bpp+bpp-1]
            clear = a <= 16
            total_clear += clear
            if y == 0 or y == h-1 or x == 0 or x == w-1:
                border_total += 1
                border_clear += clear
            if (x, y) == (0, 0): corners[0] = a
            if (x, y) == (w-1, 0): corners[1] = a
            if (x, y) == (0, h-1): corners[2] = a
            if (x, y) == (w-1, h-1): corners[3] = a
            if (x, y) == (w//2, h//2): center = a
        prev = cur
    return {
        "transparentFraction": round(total_clear/(w*h), 4),
        "transparentBorderFraction": round(border_clear/max(1, border_total), 4),
        "cornerAlpha": corners,
        "centerAlpha": center,
    }

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
            if name in {"badge_new.png", "badge_claimable.png",
                        "tutorial_pointing_hand.png", "tutorial_target_halo.png"}:
                matte = alpha_matte_stats(blob)
                log["actualAlphaMatte"] = matte
                if matte["transparentBorderFraction"] < 0.55:
                    warnings.append(f"{name}: BADGE_OR_POINTER_MATTE_REVIEW: "
                                    f"edge transparency={matte['transparentBorderFraction']}; "
                                    "possible square-background overlay")
                if max(matte["cornerAlpha"]) > 64:
                    warnings.append(f"{name}: corner has opaque pixels; "
                                    "inspect original Unity icon against approved HUD")
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
    for warning in warnings:
        print("ISSUE186_ART_ALPHA_WARNING:", warning)
    print(f"ISSUE186_SOURCE_IMPORT_{report['status']}: checked={len(output)} "
          f"errors={len(errors)} report={REPORT}")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(audit())
