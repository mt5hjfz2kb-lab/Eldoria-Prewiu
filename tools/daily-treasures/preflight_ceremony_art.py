#!/usr/bin/env python3
"""Pre-Unity art admission check for Eldoria's four DISTINCT non-daily ceremonies.

This rejects transparent-layer, provenance, and duplicate-stage failures.
A PASS is NOT proof of visual quality, gameplay, Unity, or ART_APPROVED.
Run --selftest to verify fail-closed behavior without artwork.
"""
import argparse
import hashlib
import json
from pathlib import Path
import tempfile
from PIL import Image, ImageDraw

EVENTS = {
    "lyra": ("HeroesApprovedDestination", "canonical_hero"),
    "explorers": ("EldoriaRetention/scouts-crest", "canonical_discovery"),
    "vigilants": ("EldoriaRetention/vigilants-compass", "canonical_insignia"),
    "bastion": ("Valoria_City_World_UI.unity", "actual_city"),
}
FORBIDDEN_PATH_TERMS = (
    "chest", "coffer", "treasure", "gem", "loot", "coin",
    "reward", "future_bastion", "invented_lyra",
)

def require(test, message):
    if not test:
        raise ValueError(message)

def check_layer(directory, item):
    name = item.get("path", "")
    require(isinstance(name, str) and name.endswith(".png"), "Not a PNG layer")
    candidate = Path(name)
    require(not candidate.is_absolute() and ".." not in candidate.parts,
            "Unsafe layer path: " + name)
    path = (directory / candidate).resolve()
    require(path.is_relative_to(directory.resolve()) and path.is_file(),
            "Missing asset: " + name)
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    require(item.get("sha256") == digest, "Incorrect source hash: " + name)
    require(path.stat().st_size <= 20_000_000, "Huge source asset: " + name)
    with Image.open(path) as picture:
        require(picture.format == "PNG" and picture.mode in ("RGBA", "LA"),
                "Flattened image / screenshot prohibited: " + name)
        w, h = picture.size
        require(w >= 1024 and h >= 512,
                "Insufficient illustration resolution: " + name)
        alpha = picture.getchannel("A")
        min_a, max_a = alpha.getextrema()
        require(min_a <= 8 and max_a >= 220,
                "Scene requires transparent compositing aperture: " + name)
        histogram = alpha.histogram()
        transparent = sum(histogram[:16]) / (w * h)
        visible = sum(histogram[160:]) / (w * h)
        require(.08 <= transparent <= .94 and visible >= .04,
                "Flat screenshot or empty sprite: " + name)
        thumb = picture.convert("RGB").resize((64, 36))
        values = [sum(thumb.getpixel((x, y))) / 3
                  for y in range(36) for x in range(64)]
        require(max(values) - min(values) > 32,
                "Unillustrated flat source plate: " + name)
    return {"filename": name, "sha256": digest, "size": [w, h],
            "transparent_ratio": round(transparent, 4)}

def audit(manifest_path):
    require(manifest_path.is_file(),
            "NO SOURCE ART MANIFEST: production art is NOT ready")
    payload = json.loads(manifest_path.read_text(encoding="utf-8"))
    require(payload.get("schema") == 1, "Unknown art manifest schema")
    require(payload.get("art_approved") is False and
            payload.get("unity_native_verified") is False,
            "Pre-Unity art cannot approve itself")
    groups = payload.get("ceremonies", {})
    require(set(groups) == set(EVENTS),
            "Four DISTINCT non-daily scenes required; daily chest is separate")
    focal_hashes = set()
    results = {}
    for scene, (source, slot) in EVENTS.items():
        spec = groups[scene]
        require(spec.get("identity_source") == source and
                spec.get("identity_slot") == slot,
                "Official character/world identity missing: " + scene)
        require(spec.get("baked_text") is False and
                spec.get("contains_fake_rewards") is False and
                spec.get("renders_real_identity") is False,
                "Invented narrative or baked UI not allowed: " + scene)
        layers = spec.get("layers", [])
        require(len(layers) >= 2, "Stage and ornament layers required: " + scene)
        roles = set()
        results[scene] = []
        for layer in layers:
            role = layer.get("role")
            require(role in ("stage", "ornament", "lighting"),
                    "Invalid art layer role: " + scene)
            roles.add(role)
            name = layer.get("path", "").lower()
            require(not any(word in name for word in FORBIDDEN_PATH_TERMS),
                    "Story scene contains chest/invented loot artwork: " + name)
            result = check_layer(manifest_path.parent, layer)
            if role == "stage":
                require(result["sha256"] not in focal_hashes,
                        "Duplicate focal art reused across scenes")
                focal_hashes.add(result["sha256"])
            results[scene].append({"role": role, **result})
        require({"stage", "ornament"} <= roles,
                "Missing distinct composed artwork layers: " + scene)
    return {"result": "SOURCE_ART_PREFLIGHT_PASS_NOT_ART_APPROVAL",
            "scenes": results, "art_approved": False}

def selftest():
    with tempfile.TemporaryDirectory() as folder:
        base = Path(folder)
        groups = {}
        for scene_idx, (scene, (source, slot)) in enumerate(EVENTS.items()):
            layers = []
            for role_idx, role in enumerate(("stage", "ornament")):
                folder_scene = base / scene
                folder_scene.mkdir(exist_ok=True)
                name = scene + "/" + role + ".png"
                path = base / name
                img = Image.new("RGBA", (1024, 576), (0, 0, 0, 0))
                draw = ImageDraw.Draw(img)
                tone = 130 + scene_idx * 13 + role_idx * 3
                draw.rectangle((70, 40, 950, 530),
                               fill=(tone, 90, 40, 240))
                draw.ellipse((250, 100, 800, 500), fill=(230, tone, 50, 225))
                img.save(path)
                layers.append({"role": role, "path": name,
                               "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
            groups[scene] = {"identity_source": source, "identity_slot": slot,
                             "baked_text": False, "contains_fake_rewards": False,
                             "renders_real_identity": False, "layers": layers}
        manifest = base / "ceremony-art-production.json"
        original = {"schema": 1, "ceremonies": groups,
                    "art_approved": False, "unity_native_verified": False}
        manifest.write_text(json.dumps(original), encoding="utf-8")
        require(audit(manifest)["result"].startswith("SOURCE_ART_PREFLIGHT_PASS"),
                "Positive source fixture failed")
        fake = json.loads(json.dumps(original))
        fake["ceremonies"]["vigilants"]["layers"][0] = dict(
            fake["ceremonies"]["lyra"]["layers"][0])
        manifest.write_text(json.dumps(fake), encoding="utf-8")
        try:
            audit(manifest)
        except ValueError as exc:
            require("Duplicate focal" in str(exc),
                    "Unexpected failure in duplicate scene regression: " + str(exc))
        else:
            raise AssertionError("Duplicate art was accepted")
        fake = json.loads(json.dumps(original))
        fake["ceremonies"]["bastion"]["contains_fake_rewards"] = True
        manifest.write_text(json.dumps(fake), encoding="utf-8")
        try:
            audit(manifest)
        except ValueError:
            pass
        else:
            raise AssertionError("Invented reward scene was accepted")
    print("PASS: isolated stage layers, actual identity, duplicate and fake-loot guards")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", type=Path)
    parser.add_argument("--selftest", action="store_true")
    args = parser.parse_args()
    if args.selftest:
        selftest()
    else:
        require(args.manifest is not None, "--manifest is required")
        print(json.dumps(audit(args.manifest), indent=2, ensure_ascii=False))
