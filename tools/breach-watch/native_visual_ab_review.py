#!/usr/bin/env python3
"""Read-only, reproducible owner-reference versus TRUE Unity capture review.

This is an artist's evidence pack, NEVER an automatic art-approval score.
Requires Pillow (pip install Pillow) and the UNMODIFIED Actions artifact ZIP.
Runs on Linux/macOS; consumes no shared Windows/Unity lease.

Example:
  python tools/breach-watch/native_visual_ab_review.py \
    --reference "75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg" \
    --native-zip github-actions-artifact-13887684195.zip \
    --output review-pr173
"""
import argparse
import hashlib
import io
import json
from pathlib import Path
import zipfile

from PIL import Image, ImageDraw, ImageOps


PORTRAIT = (
    ("city", "Bastión I / ciudad"),
    ("bastion", "Bastión I / ficha"),
    ("training", "Entrenamiento"),
    ("march", "Marchas"),
    ("heroes", "Sala de héroes"),
    ("hero-aldric-resumen", "Aldric"),
    ("mail", "Buzón vacío"),
    ("mail-report-document", "Buzón / informe"),
    ("vigilants", "Vigilantes"),
    ("scouts", "Exploradores"),
)
LANDSCAPE = (
    ("city", "Ciudad"),
    ("bastion", "Bastión / ficha"),
    ("training", "Entrenamiento"),
    ("march", "Marchas"),
    ("heroes", "Héroes"),
    ("mail", "Buzón"),
    ("vigilants", "Vigilantes"),
    ("scouts", "Exploradores"),
)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def find_one(archive, basename):
    matches = [n for n in archive.namelist() if
               n.endswith("/" + basename) or n == basename]
    if len(matches) != 1:
        raise ValueError(
            f"Expected exactly one native file {basename}, found {len(matches)}")
    data = archive.read(matches[0])
    return data


def resized(im, size):
    result = ImageOps.contain(im.convert("RGB"), size, Image.Resampling.LANCZOS)
    panel = Image.new("RGB", size, (24, 22, 22))
    panel.paste(result, ((size[0] - result.width) // 2,
                         (size[1] - result.height) // 2))
    return panel


def sheet(items, cols, cell, out):
    cw, ch = cell
    rows = (len(items) + cols - 1) // cols
    img = Image.new("RGB", (cw * cols, (ch + 43) * rows), (18, 17, 17))
    draw = ImageDraw.Draw(img)
    for i, (title, source) in enumerate(items):
        x = (i % cols) * cw
        y = (i // cols) * (ch + 43)
        draw.text((x + 12, y + 11), title, fill=(246, 223, 181))
        img.paste(resized(source, (cw, ch)), (x, y + 43))
    img.save(out, quality=93, subsampling=0)


def load_verified(archive, name, expected_hash):
    raw = find_one(archive, name)
    if expected_hash and digest(raw) != expected_hash:
        raise ValueError(f"SHA256 mismatch for genuine Unity image {name}")
    image = Image.open(io.BytesIO(raw))
    image.load()
    return image


def run(args):
    out = Path(args.output)
    out.mkdir(parents=True, exist_ok=True)
    reference_bytes = Path(args.reference).read_bytes()
    reference = Image.open(io.BytesIO(reference_bytes))
    reference.load()
    if reference.size != (864, 1536):
        raise ValueError(
            f"Owner approved 9:16 evidence must be 864x1536, got {reference.size}")
    with zipfile.ZipFile(args.native_zip) as archive:
        report = json.loads(find_one(archive, "BreachWatchNativeEvidence.json"))
        if (report.get("status") !=
                "UNITY_NATIVE_INDEPENDENT_CAPTURED_ART_REVIEW_PENDING" or
                report.get("screenshotCount") != 8 or
                report.get("oldCanvasVisible") != 0 or
                report.get("activeForeignScreenSpaceCanvases") != 0 or
                report.get("artworkApproved") is not False or
                report.get("saveUnchanged") is not True):
            raise ValueError("Native art-revision contract did not pass")
        source_sha = report["source"]
        if len(source_sha) != 40:
            raise ValueError("Real source commit SHA is required")
        index = {}
        evidence_names = (
            "BreachWatchNativeEvidence.json",
            "BreachWatchDestinationEvidence.json",
            "BreachWatchHeroProfilesEvidence.json",
            "BreachWatchPopulatedMailEvidence.json",
            "BreachWatchRetentionEvidence.json",
        )
        screen_count = 0
        for filename in evidence_names:
            details = json.loads(find_one(archive, filename))
            if details["source"] != source_sha:
                raise ValueError("Evidence mixes different Unity source commits")
            if details.get("artworkApproved") is not False:
                raise ValueError("Art was silently marked as owner approved")
            screenshots = details.get("shots", details.get("screens", []))
            if details["screenshotCount"] != len(screenshots):
                raise ValueError("Native screenshots missing from manifest")
            for shot in screenshots:
                name = shot["file"]
                if name in index:
                    raise ValueError("Duplicate native screenshot identity")
                data = find_one(archive, name)
                if digest(data) != shot["sha256"]:
                    raise ValueError(f"Original Unity PNG checksum mismatch: {name}")
                index[name] = shot
                screen_count += 1

        def open_capture(dest, orientation):
            name = f"real-unity-{dest}-{orientation}.png"
            shot = index.get(name)
            if not shot:
                raise ValueError(f"Missing validated Unity screen: {name}")
            return load_verified(archive, name, shot["sha256"])

        bastion = open_capture("bastion", "900x1600")
        sheet([
            ("CONCEPTO APROBADO / 864x1536", reference),
            ("UNITY NATIVO / 900x1600", bastion),
        ], 2, (540, 960), out / "reference-versus-unity.jpg")
        sheet([(label + " / 390x844",
                open_capture(dest, "390x844"))
               for dest, label in PORTRAIT],
              5, (320, 691), out / "real-native-portrait-destinations.jpg")
        sheet([(label + " / 844x390",
                open_capture(dest, "844x390"))
               for dest, label in LANDSCAPE],
              2, (633, 293), out / "real-native-landscape-destinations.jpg")

    manifest = {
        "reviewType": "OWNER_REFERENCE_VERSUS_REAL_UNITY_ART_PROOF",
        "nativeSourceSha": source_sha,
        "nativeRunId": args.run_id,
        "approvedReferenceOriginalSha256": digest(reference_bytes),
        "approvedReferenceWidth": 864,
        "approvedReferenceHeight": 1536,
        "unityBastionWidth": 900,
        "unityBastionHeight": 1600,
        "verifiedScreenshotCount": screen_count,
        "nativeOldCanvasHidden": True,
        "nativeSaveUnchanged": True,
        "nativeEvidenceHashesValidated": True,
        "ownerArtApproved": False,
        "webglTestedHere": False,
        "physicalIphoneTestedHere": False,
        "score": None,
        "interpretation": ("Direct visual judgement required; this report never "
                           "claims a numeric score or approval"),
        "output": [
            "reference-versus-unity.jpg",
            "real-native-portrait-destinations.jpg",
            "real-native-landscape-destinations.jpg",
        ],
    }
    (out / "visual-ab-manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8")
    print("VIGILIA_REFERENCE_NATIVE_REVIEW_READY", screen_count,
          source_sha, "NO_ART_APPROVAL")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference", required=True)
    parser.add_argument("--native-zip", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--run-id", type=int, default=0)
    run(parser.parse_args())
