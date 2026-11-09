#!/usr/bin/env python3
"""Stage a source-only, exact beta10 vs one-file anchored timer A/B.

Never publishes, launches Unity, changes a save, or releases testers. All
sources are fetched from immutable Git commits; no moving branch/URL aliases.
The ONLY differential in A/B is RoyalGoldV3ContextHud.cs (the timer). The
experimental variant also removes one transient per-frame Vector3[4] allocation.

Example (from a checkout with pinned commit objects fetched):
  python tools/eldoria-private-beta/iphone_ab/prepare.py --check
  python tools/eldoria-private-beta/iphone_ab/prepare.py --out /tmp/eldoria-timer-ab
"""
import argparse
import hashlib
import json
import subprocess
from pathlib import Path

BETA10 = "be49d62e3daf490f41dba0bf8123a8e2eee64480"
BETA11 = "8dc875430241a49b4d417782dfeab2d4c7a4f315"
PREFIX = "tools/eldoria-private-beta/native/ux/"
TIMER = "RoyalGoldV3ContextHud.cs.txt"
BETA10_TIMER_BLOB = "fe5b1de631d43109ac95ce471ea31feaa93dc8a6"
BETA11_TIMER_BLOB = "0d9cb29fa9198d8a87011cc554b6247bb63a9e30"


def git_bytes(revision, path):
    command = ["git", "show", revision + ":" + PREFIX + path]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if result.returncode:
        raise RuntimeError("Pinned Git source unavailable: " + revision + "/" + path +
                           ". Fetch that exact commit; do not substitute HEAD. " +
                           result.stderr.decode("utf-8", "replace")[:240])
    return result.stdout


def digest(data):
    return hashlib.sha256(data).hexdigest()


def gh_blob(data):
    """SHA-1 of canonical Git blob bytes, NOT SHA-256 of the document."""
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def optimized_anchor(data):
    text = data.decode("utf-8")
    old = "        private float nextRead;"
    new = (old + "\n        // Reused on Unity's main thread: no per-frame heap array while the\n"
           "        // objectives manuscript and construction timer are visible.\n"
           "        private readonly Vector3[] objectiveWorldCorners = new Vector3[4];")
    before = ("                var corners = new Vector3[4];\n"
              "                chronicle.ChronicleObjectiveRect.GetWorldCorners(corners);")
    after = "                chronicle.ChronicleObjectiveRect.GetWorldCorners(objectiveWorldCorners);"
    loop_before = "                foreach (var corner in corners)"
    loop_after = "                foreach (var corner in objectiveWorldCorners)"
    for needle in (old, before, loop_before):
        if text.count(needle) != 1:
            raise ValueError("Anchored timer source drift, expected exactly once: " + needle)
    text = text.replace(old, new).replace(before, after).replace(loop_before, loop_after)
    if "private bool TryPlaceConstructionTimer(" not in text:
        raise ValueError("Missing anchored construction timer")
    if "if (!TryPlaceConstructionTimer(id, camera" not in text:
        raise ValueError("Timer not bound to building projection")
    if "new Vector3[4]" in text[text.index("private bool TryPlaceConstructionTimer("):]:
        raise ValueError("A frame-local corner allocation remains in timer projection")
    return text.encode("utf-8")


def build_pair():
    base = json.loads(git_bytes(BETA10, "manifest.json"))
    newer = json.loads(git_bytes(BETA11, "manifest.json"))
    original = base.get("files", [])
    anchor_entries = newer.get("files", [])
    if len(original) != 19 or len(anchor_entries) != 19:
        raise ValueError("Expected exactly nineteen UX overlays in each frozen source")
    base_map = {x["overlay"]: x for x in original}
    anchor_map = {x["overlay"]: x for x in anchor_entries}
    if len(base_map) != 19 or set(base_map) != set(anchor_map):
        raise ValueError("Missing/duplicated overlay or changed source surface")
    if base["base_runtime"] != newer["base_runtime"]:
        raise ValueError("Source runtime baseline changed; not a one-file A/B")
    baseline, experiment = {}, {}
    out_original, out_experimental = [], []
    changed = []
    for entry in original:
        name = entry["overlay"]
        baseline_data = git_bytes(BETA10, name)
        newer_data = git_bytes(BETA11, name)
        if digest(baseline_data) != entry["candidate_sha256"]:
            raise ValueError("beta10 candidate hash mismatch " + name)
        if digest(newer_data) != anchor_map[name]["candidate_sha256"]:
            raise ValueError("beta11 candidate hash mismatch " + name)
        if entry["path"] != anchor_map[name]["path"] or (
            entry["baseline_sha256"] != anchor_map[name]["baseline_sha256"]
        ):
            raise ValueError("Source target or pre-overlay baseline drift " + name)
        if name != TIMER and baseline_data != newer_data:
            raise ValueError("Unsafe A/B, unrelated changes in " + name)
        baseline[name] = baseline_data
        experiment[name] = optimized_anchor(newer_data) if name == TIMER else baseline_data
        if baseline[name] != experiment[name]:
            changed.append(name)
        out_original.append(dict(entry))
        out_experimental.append({**entry, "candidate_sha256": digest(experiment[name])})
    if changed != [TIMER]:
        raise ValueError("A/B does not isolate the construction clock")
    if gh_blob(baseline[TIMER]) != BETA10_TIMER_BLOB:
        raise ValueError("Historical playable timer bytes do not match beta10")
    if gh_blob(git_bytes(BETA11, TIMER)) != BETA11_TIMER_BLOB:
        raise ValueError("Anchored timer does not match beta11")
    if digest(baseline[TIMER]) == digest(experiment[TIMER]):
        raise ValueError("A/B differential absent")
    info = {
        "schema": 1,
        "mode": "SOURCE_ONLY_NOT_PUBLISHED",
        "control_build": BETA10,
        "experimental_timer_source": BETA11,
        "target": "recovered beta10 + construction timer world-anchor, GC-stable variant",
        "only_changed_overlay": TIMER,
        "overlays_verified_identical_other_than_timer": 18,
        "base_runtime": base["base_runtime"],
        "control_timer_sha256": digest(baseline[TIMER]),
        "experimental_timer_sha256": digest(experiment[TIMER]),
        "native_compiled": False,
        "browser_qa": False,
        "physical_iphone_30min": False,
        "external_distribution_approved": False,
        "legacy_pages_beta10_must_remain_unchanged": True,
    }
    manifests = {
        "beta10-control": {**base, "files": out_original},
        "anchored-gc0": {**base, "files": out_experimental}
    }
    return info, manifests, {"beta10-control": baseline, "anchored-gc0": experiment}


def stage(directory, info, manifests, variants):
    directory = Path(directory).resolve()
    if directory.exists() and any(directory.iterdir()):
        raise ValueError("Refusing to overwrite populated A/B output directory: " + str(directory))
    for variant in ("beta10-control", "anchored-gc0"):
        root = directory / variant
        root.mkdir(parents=True, exist_ok=True)
        for name, data in variants[variant].items():
            (root / name).write_bytes(data)
        (root / "manifest.json").write_text(
            json.dumps(manifests[variant], indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    (directory / "ab-evidence.json").write_text(
        json.dumps(info, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    # Verify generated snapshots and manifests at rest.
    for variant, manifest in manifests.items():
        for entry in manifest["files"]:
            if digest((directory / variant / entry["overlay"]).read_bytes()) != entry["candidate_sha256"]:
                raise ValueError("Staging write/hash mismatch in " + variant + "/" + entry["overlay"])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--check", action="store_true", help="Validate pinned sources, no writes")
    group.add_argument("--out", type=Path, help="Write staged independent source trees")
    args = parser.parse_args()
    info, manifests, variants = build_pair()
    if args.out is not None:
        stage(args.out, info, manifests, variants)
    print(json.dumps(info, ensure_ascii=False, sort_keys=True))


if __name__ == "__main__":
    main()
