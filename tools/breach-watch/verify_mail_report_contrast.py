#!/usr/bin/env python3
"""Reject light-on-light text in the REAL Unity Vigilia report document.

Only the genuine new-build 390x844 screenshot is accepted.  Contrast PASS
is a minimum mobile accessibility proof, never artistic approval.
"""
import sys
from pathlib import Path
from PIL import Image

NAME = "real-unity-mail-report-document-390x844.png"


def ink_fraction(image, rect, threshold=165):
    crop = image.crop(rect).convert("RGB")
    pixels = list(crop.get_flattened_data())
    return sum((r + g + b) / 3 < threshold for r, g, b in pixels) / len(pixels)


def verify(folder):
    matches = list(Path(folder).rglob(NAME))
    if len(matches) != 1:
        raise AssertionError(
            f"Expected exactly one native phone report; found {len(matches)}"
        )
    with Image.open(matches[0]) as source:
        image = source.convert("RGB")
        if image.size != (390, 844):
            raise AssertionError(f"Wrong native dimensions: {image.size}")

        # In 43076731132 the real Unity report showed 0.000% dark pixels
        # in both boxes: the report's cream parchment inherited ivory ink.
        # These areas exclude frame edges, red buttons and decorative seals.
        zones = {
            "section_heading": (61, 425, 233, 454),
            "report_body": (60, 500, 268, 581),
        }
        scores = {name: ink_fraction(image, rect)
                  for name, rect in zones.items()}
        if any(score < 0.012 for score in scores.values()):
            raise AssertionError(
                "VIGILIA_NATIVE_REPORT_INVISIBLE_INK: " +
                ", ".join(f"{key}={val:.3%}" for key, val in scores.items())
            )
        print("VIGILIA_NATIVE_REPORT_DARK_INK_ON_PARCHMENT_PASS", scores)
        print("ART_APPROVED=false; contrast is necessary but not sufficient")


if __name__ == "__main__":
    verify(sys.argv[1] if len(sys.argv) > 1 else "current-native")
