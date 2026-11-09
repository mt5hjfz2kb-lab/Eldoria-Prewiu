#!/usr/bin/env python3
"""A/B for authentic Unity BreachWatch native evidence, NOT artistic approval.

Both inputs are directories extracted from GitHub Actions artifacts. Never
substitute an illustration or generated concept for real Unity screenshots.
Requires Pillow; output is a JPG for simple phone viewing and JSON diagnostics.
"""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw

SIZES = ((1600, 900), (900, 1600), (844, 390), (390, 844))
SCENES = ("city", "bastion")


def compare(first: Path, second: Path, out: Path, threshold=12):
    out.mkdir(parents=True, exist_ok=True)
    portraits = []
    records = []
    for scene in SCENES:
        for width, height in SIZES:
            filename = f"real-unity-{scene}-{width}x{height}.png"
            with Image.open(first / filename) as base, Image.open(second / filename) as candidate:
                a, b = base.convert("RGB"), candidate.convert("RGB")
            if a.size != (width, height) or b.size != a.size:
                raise ValueError(f"Invalid genuine Unity dimensions: {filename}")
            red, green, blue = ImageChops.difference(a, b).split()
            binary = [channel.point(lambda value: 255 if value > threshold else 0)
                      for channel in (red, green, blue)]
            mask = ImageChops.lighter(ImageChops.lighter(binary[0], binary[1]), binary[2])
            changed = mask.histogram()[255]
            bounds = mask.getbbox()
            records.append({
                "file": filename,
                "changed_pixels": changed,
                "changed_pct": round(changed * 100 / (width * height), 4),
                "bbox": list(bounds) if bounds else None,
            })
            if scene != "bastion":
                continue
            crop = ((int(width * .55), int(height * .47), width, int(height * .91))
                    if width < height else
                    (int(width * .62), int(height * .12), width, int(height * .89)))
            original, revised = a.crop(crop), b.crop(crop)
            display_width = 390
            display_height = round(original.height * display_width / original.width)
            card = Image.new("RGB", (display_width * 2 + 36, display_height + 84), (28, 26, 24))
            card.paste(original.resize((display_width, display_height), Image.Resampling.LANCZOS), (18, 54))
            card.paste(revised.resize((display_width, display_height), Image.Resampling.LANCZOS), (18 + display_width, 54))
            draw = ImageDraw.Draw(card)
            draw.text((18, 16), f"{width}x{height}  REAL UNITY BEFORE / AFTER", fill=(236, 218, 185))
            draw.text((18, 37), f"Changed: {records[-1]['changed_pct']:.2f}% of screenshot (not art approval)",
                      fill=(195, 183, 159))
            portraits.append(card)
    sheet = Image.new("RGB", (portraits[0].width + 36,
                             sum(p.height for p in portraits) + (len(portraits) + 1) * 18), (21, 20, 19))
    y = 18
    for card in portraits:
        sheet.paste(card, (18, y))
        y += card.height + 18
    sheet.save(out / "vigilia_native_art_ab.jpg", quality=91, subsampling=0)
    (out / "vigilia_native_art_diff.json").write_text(json.dumps({
        "baseline": str(first), "candidate": str(second), "threshold": threshold,
        "art_approved": False, "records": records,
    }, indent=2), encoding="utf-8")
    return records


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("baseline", type=Path)
    parser.add_argument("candidate", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--threshold", type=int, default=12)
    args = parser.parse_args()
    for item in compare(args.baseline, args.candidate, args.output, args.threshold):
        print(f"{item['file']}: {item['changed_pct']:.4f}% changed; bbox={item['bbox']}")
