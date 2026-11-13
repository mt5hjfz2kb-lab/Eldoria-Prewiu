#!/usr/bin/env python3
"""Arrange unchanged native screenshots; never synthesize or retouch game pixels."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

VIEWS = {
    "nick": "BreachWatchNativeEvidence/FirstEntry/nickname-390x844.png",
    "vigilantes": "BreachWatchDestinationEvidence/real-unity-vigilants-390x844.png",
    "exploradores": "BreachWatchDestinationEvidence/real-unity-scouts-390x844.png",
}


def verify(root, expected):
    source = (root / "BreachWatchNativeEvidence/FirstEntry/source.txt").read_text().strip()
    report = json.loads((root / "BreachWatchDestinationEvidence/BreachWatchRetentionEvidence.json").read_text())
    if source != expected or report["source"] != expected:
        raise ValueError("Native capture source mismatch")
    if report["artworkApproved"] or not report["probeOnly"] or not report["saveUnchanged"]:
        raise ValueError("Native comparison must not claim automatic artistic approval")
    recorded = {s["file"]: s["sha256"] for s in report["screens"]}
    for key, relative in VIEWS.items():
        p = root / relative
        with Image.open(p) as im:
            if im.size != (390, 844):
                raise ValueError(f"Unequal native viewport: {p}")
        if key != "nick" and hashlib.sha256(p.read_bytes()).hexdigest() != recorded[p.name]:
            raise ValueError(f"Native screenshot digest mismatch: {p}")


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--before", type=Path, required=True)
    ap.add_argument("--after", type=Path, required=True)
    ap.add_argument("--before-sha", required=True)
    ap.add_argument("--after-sha", required=True)
    ap.add_argument("--output", type=Path, required=True)
    args = ap.parse_args()
    verify(args.before, args.before_sha)
    verify(args.after, args.after_sha)
    args.output.mkdir(parents=True, exist_ok=True)
    font = ImageFont.truetype("DejaVuSans.ttf", 17)
    small = ImageFont.truetype("DejaVuSans.ttf", 12)
    manifest = {"before_source": args.before_sha, "after_source": args.after_sha,
                "image_operation": "Unscaled native pixels, side-by-side with separate labels",
                "artistic_approval": False, "comparisons": []}
    for key, relative in VIEWS.items():
        canvas = Image.new("RGB", (804, 916), "#141418")
        draw = ImageDraw.Draw(canvas)
        inputs = []
        for index, (root, sha, label) in enumerate([
                (args.before, args.before_sha, "ANTES"), (args.after, args.after_sha, "VIGILIA · RECUPERACIÓN")]):
            p = root / relative
            x = 8 + index * 398
            draw.text((x + 8, 10), label, font=font, fill="#ead9b7")
            draw.text((x + 8, 35), "Unity 390×844 · " + sha[:12], font=small, fill="#bdb6ac")
            with Image.open(p) as im:
                canvas.paste(im.convert("RGB"), (x, 58))
            inputs.append({"file": relative, "source": sha,
                           "sha256": hashlib.sha256(p.read_bytes()).hexdigest()})
        out = args.output / (key + "_AB_Unity_390x844.png")
        canvas.save(out)
        manifest["comparisons"].append({"output": out.name, "inputs": inputs,
                                        "sha256": hashlib.sha256(out.read_bytes()).hexdigest()})
    (args.output / "native-comparison-provenance.json").write_text(
        json.dumps(manifest, indent=2, ensure_ascii=False) + "\n")
    print(json.dumps(manifest, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
