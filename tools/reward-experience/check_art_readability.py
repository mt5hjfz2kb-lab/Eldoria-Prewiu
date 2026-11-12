#!/usr/bin/env python3
"""Source-side 48px resource icon differentiation gate, zero external packages.

This is a *necessary* pre-Unity quality proxy, not artistic approval.
It detects identical/silhouette-collapsed tiers at real bag-icon sizes,
but cannot judge aesthetic finish, text legibility or integration.
"""
import json
import math
import struct
import sys
import zlib
from pathlib import Path

DIR = (Path(__file__).resolve().parents[2] /
       "Unity/Assets/Eldoria/Issue186Art/Resources/Issue186ResourceArt")
REPORT = Path(__file__).resolve().parents[2] / "issue186_48px_readability.json"
KINDS = ("wood", "stone", "food")
TIERS = ("100", "1k", "10k", "100k")
BASE_RGB = (29, 24, 24)


def png_pixels(filename):
    data = (DIR / filename).read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("Not a PNG")
    offset, idat, w, h, bpp = 8, bytearray(), None, None, None
    while offset < len(data):
        n = struct.unpack_from(">I", data, offset)[0]
        tag = data[offset + 4:offset + 8]
        payload = data[offset + 8:offset + 8 + n]
        offset += n + 12
        if tag == b"IHDR":
            w, h, depth, color, _, _, interlace = struct.unpack(">IIBBBBB", payload)
            if depth != 8 or color not in (4, 6) or interlace:
                raise ValueError("Requires 8-bit RGBA/LA non-interlaced source")
            bpp = 4 if color == 6 else 2
        elif tag == b"IDAT":
            idat.extend(payload)
        elif tag == b"IEND":
            break
    if not w or not h or not idat:
        raise ValueError("Missing image data")
    uncompressed = zlib.decompress(idat)
    stride = w * bpp
    if len(uncompressed) != h * (stride + 1):
        raise ValueError("Invalid PNG scanline size")
    rows = []
    prev = bytearray(stride)
    for y in range(h):
        start = y * (stride + 1)
        algorithm = uncompressed[start]
        if algorithm > 4:
            raise ValueError("Unsupported PNG row filter")
        source = uncompressed[start + 1:start + stride + 1]
        row = bytearray(stride)
        for x, byte in enumerate(source):
            left = row[x - bpp] if x >= bpp else 0
            top = prev[x]
            top_left = prev[x - bpp] if x >= bpp else 0
            if algorithm == 1:
                predictor = left
            elif algorithm == 2:
                predictor = top
            elif algorithm == 3:
                predictor = (left + top) // 2
            elif algorithm == 4:
                p = left + top - top_left
                a, b, c = abs(p - left), abs(p - top), abs(p - top_left)
                predictor = left if a <= b and a <= c else top if b <= c else top_left
            else:
                predictor = 0
            row[x] = (byte + predictor) & 255
        rows.append(row)
        prev = row
    return w, h, bpp, rows


def preview48(filename):
    w, h, bpp, rows = png_pixels(filename)
    result = bytearray()
    # Exclude the golden frame / empty quantity plaque, which is intentionally
    # identical across denominations. Measure the *resource object* only.
    for sy in range(48):
        y = min(h - 1, round(h * (.065 + sy / 47 * .72)))
        row = rows[y]
        for sx in range(48):
            x = min(w - 1, round(w * (.105 + sx / 47 * .79)))
            at = x * bpp
            if bpp == 4:
                r, g, b, alpha = row[at:at+4]
            else:
                v, alpha = row[at:at+2]
                r = g = b = v
            for value, under in zip((r, g, b), BASE_RGB):
                result.append((value * alpha + under * (255 - alpha) + 127) // 255)
    return bytes(result)


def mean_color_difference(a, b):
    return round(sum(abs(x - y) for x, y in zip(a, b)) / len(a), 2)


def luminance_spread(img):
    values = [int(img[i] * .2126 + img[i+1] * .7152 + img[i+2] * .0722)
              for i in range(0, len(img), 3)]
    avg = sum(values) / len(values)
    return round(math.sqrt(sum((v-avg)**2 for v in values)/len(values)), 2)


def main():
    errors, differences, spreads, thumbnails = [], [], {}, {}
    for kind in KINDS:
        for tier in TIERS:
            name = f"pack_{kind}_{tier}.png"
            thumb = preview48(name)
            spread = luminance_spread(thumb)
            spreads[name] = spread
            thumbnails[name] = thumb
            if spread < 18:
                errors.append(f"{name}: flat image at 48px (spread {spread})")
        for left, right in zip(TIERS, TIERS[1:]):
            a, b = f"pack_{kind}_{left}.png", f"pack_{kind}_{right}.png"
            diff = mean_color_difference(thumbnails[a], thumbnails[b])
            differences.append({"a": a, "b": b, "meanRgbDelta48": diff})
            # Frames do not count, only actual resource artwork. A duplicate
            # or almost-identical tier must never be silently shipped.
            if diff < 12:
                errors.append(f"{a} and {b}: tiers visually indistinct at 48px ({diff})")
    for tier in TIERS:
        pairs = (("wood", "stone"), ("wood", "food"), ("stone", "food"))
        for k1, k2 in pairs:
            a, b = f"pack_{k1}_{tier}.png", f"pack_{k2}_{tier}.png"
            diff = mean_color_difference(thumbnails[a], thumbnails[b])
            differences.append({"a": a, "b": b, "meanRgbDelta48": diff})
            if diff < 12:
                errors.append(f"{a} and {b}: resource types indistinct at 48px ({diff})")
    report = {
        "status": "FAIL" if errors else "PASS",
        "assets": len(thumbnails),
        "previewSize": 48,
        "objectOnlyNoFrame": True,
        "spreads": spreads, "differences": differences,
        "errors": errors,
        "limitation": "Pixel contrast/differentiation only. Unity-native screenshots and independent artistic review are still required."
    }
    REPORT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    for e in errors:
        print("ISSUE186_ART_READABILITY_ERROR", e)
    print(f"ISSUE186_48PX_{report['status']}: 12 original sources, "
          f"{len(differences)} object-only comparisons, errors={len(errors)}")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
