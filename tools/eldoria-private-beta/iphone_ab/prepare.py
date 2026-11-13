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
PERF = "485d7b1c7980b73fa6abac9dfad4a99ccc382ab8"
MINIMAL = "e445a712b16f0bfc8196bdcb734492e780ef082c"
MINIMAL_PATH = "tools/eldoria-private-beta/native/iphone-hotfix/RoyalGoldV3ContextHud.beta10-minimal.cs.txt"
MINIMAL_TIMER_BLOB = "144b1d6becec3055aac6a69cce07007cad1849a6"
PREFIX = "tools/eldoria-private-beta/native/ux/"
TIMER = "RoyalGoldV3ContextHud.cs.txt"
BETA10_TIMER_BLOB = "fe5b1de631d43109ac95ce471ea31feaa93dc8a6"
BETA11_TIMER_BLOB = "0d9cb29fa9198d8a87011cc554b6247bb63a9e30"
PERF_TIMER_BLOB = "031595af0f1ca5f44dc18d58183b3143e8c53676"


def git_bytes(revision, path):
    # The archived 19 overlays use PREFIX, while the minimal owner hotfix
    # lives in the canonical beta workstream as an independent sidecar.
    source_path = path if path.startswith("tools/") else PREFIX + path
    command = ["git", "show", revision + ":" + source_path]
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
    """Validate the already optimized immutable timer source, without editing it."""
    text = data.decode("utf-8")
    required = (
        "private bool TryPlaceConstructionTimer(",
        "TryProjectConstructionSilhouette(",
        "private readonly Vector3[] objectiveWorldCorners = new Vector3[4];",
        "GetWorldCorners(objectiveWorldCorners)",
        "if (!timer.HasLayout ||",
    )
    for marker in required:
        if marker not in text:
            raise ValueError("Optimized timer missing required safe path: " + marker)
    hot_path = text[text.index("private bool TryPlaceConstructionTimer("):]
    if "new Vector3[4]" in hot_path:
        raise ValueError("Frame-local objective corner heap allocation reintroduced")
    if "CanProject(target, camera, safe, true, out projectedRoof)" in hot_path:
        raise ValueError("Old double silhouette projection reintroduced")
    return data


def minimal_owner_anchor(data):
    text = data.decode("utf-8")
    # This is the beta10 player-verified version with only timer world anchoring
    # changed. No extra canvas, change to save-state, or new scene/clock.
    required = (
        'private static bool IsCityConstruction(string id)',
        'var target = FindAnchor(id == "upgrade-barracks" ? "barracks" : id);',
        'bool roofVisible = CanProject(target, camera, safe, true, out roof);',
        'if (!roofVisible)',
        'if (Mathf.Abs(x - xAnchor) > 28f)',
        'timer.Root.anchoredPosition = center;',
    )
    for marker in required:
        if marker not in text:
            raise ValueError("Minimal beta10 hotfix missing: " + marker)
    if "TryPlaceConstructionTimer" in text or "TryProjectConstructionSilhouette" in text:
        raise ValueError("Mobile11 timer algorithm was accidentally substituted")
    if "new Vector3[4]" in text:
        raise ValueError("Minimal owner timer introduces a frame-local corner array")
    return data


def build_pair():
    base = json.loads(git_bytes(BETA10, "manifest.json"))
    newer = json.loads(git_bytes(BETA11, "manifest.json"))
    perf_manifest = json.loads(git_bytes(PERF, "manifest.json"))
    original = base.get("files", [])
    anchor_entries = newer.get("files", [])
    if len(original) != 19 or len(anchor_entries) != 19:
        raise ValueError("Expected exactly nineteen UX overlays in each frozen source")
    base_map = {x["overlay"]: x for x in original}
    anchor_map = {x["overlay"]: x for x in anchor_entries}
    if len(base_map) != 19 or set(base_map) != set(anchor_map):
        raise ValueError("Missing/duplicated overlay or changed source surface")
    if base["base_runtime"] != newer["base_runtime"] or base["base_runtime"] != perf_manifest["base_runtime"]:
        raise ValueError("Source runtime baseline changed; not a one-file A/B")
    perf_entries = {x["overlay"]: x for x in perf_manifest["files"]}
    if TIMER not in perf_entries or perf_entries[TIMER]["path"] != base_map[TIMER]["path"] or perf_entries[TIMER]["baseline_sha256"] != base_map[TIMER]["baseline_sha256"]:
        raise ValueError("Optimized timer target/runtime baseline diverged")
    perf_timer = git_bytes(PERF, TIMER)
    if gh_blob(perf_timer) != PERF_TIMER_BLOB or digest(perf_timer) != perf_entries[TIMER]["candidate_sha256"]:
        raise ValueError("Optimized timer differs from exact reviewed source")
    perf_timer = optimized_anchor(perf_timer)
    minimal_timer = git_bytes(MINIMAL, MINIMAL_PATH)
    if gh_blob(minimal_timer) != MINIMAL_TIMER_BLOB:
        raise ValueError("Minimal timer differs from the immutable reviewed sidecar")
    minimal_timer = minimal_owner_anchor(minimal_timer)
    baseline, experiment, minimal = {}, {}, {}
    out_original, out_experimental, out_minimal = [], [], []
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
        experiment[name] = perf_timer if name == TIMER else baseline_data
        minimal[name] = minimal_timer if name == TIMER else baseline_data
        if baseline[name] != experiment[name]:
            changed.append(name)
        out_original.append(dict(entry))
        out_experimental.append({**entry, "candidate_sha256": digest(experiment[name])})
        out_minimal.append({**entry, "candidate_sha256": digest(minimal[name])})
    if changed != [TIMER]:
        raise ValueError("A/B does not isolate the construction clock")
    if gh_blob(baseline[TIMER]) != BETA10_TIMER_BLOB:
        raise ValueError("Historical playable timer bytes do not match beta10")
    if gh_blob(git_bytes(BETA11, TIMER)) != BETA11_TIMER_BLOB:
        raise ValueError("Anchored timer does not match beta11")
    if digest(baseline[TIMER]) == digest(experiment[TIMER]):
        raise ValueError("A/B differential absent")
    if digest(baseline[TIMER]) == digest(minimal[TIMER]) or minimal[TIMER] == experiment[TIMER]:
        raise ValueError("Minimal beta10 branch not an independent one-file clock experiment")
    assert all(minimal[name] == baseline[name] for name in baseline if name != TIMER)
    info = {
        "schema": 1,
        "mode": "SOURCE_ONLY_NOT_PUBLISHED",
        "control_build": BETA10,
        "intermediate_timer_source": BETA11,
        "experimental_timer_source": PERF,
        "target": "recovered beta10 + isolated optimized world-anchored timer: one silhouette pass, reusable UI scratch, conditional layout",
        "only_changed_overlay": TIMER,
        "overlays_verified_identical_other_than_timer": 18,
        "base_runtime": base["base_runtime"],
        "control_timer_sha256": digest(baseline[TIMER]),
        "experimental_timer_sha256": digest(experiment[TIMER]),
        "minimal_timer_source": MINIMAL,
        "minimal_timer_sha256": digest(minimal[TIMER]),
        "variants": ["beta10-control", "anchored-gc0", "beta10-minimal-anchor"],
        "one_file_differential_each": True,
        "native_compiled": False,
        "browser_qa": False,
        "physical_iphone_30min": False,
        "external_distribution_approved": False,
        "legacy_pages_beta10_must_remain_unchanged": True,
    }
    manifests = {
        "beta10-control": {**base, "files": out_original},
        "anchored-gc0": {**base, "files": out_experimental},
        "beta10-minimal-anchor": {**base, "files": out_minimal}
    }
    return info, manifests, {"beta10-control": baseline, "anchored-gc0": experiment,
                             "beta10-minimal-anchor": minimal}


def stage(directory, info, manifests, variants):
    directory = Path(directory).resolve()
    if directory.exists() and any(directory.iterdir()):
        raise ValueError("Refusing to overwrite populated A/B output directory: " + str(directory))
    for variant in ("beta10-control", "anchored-gc0", "beta10-minimal-anchor"):
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
