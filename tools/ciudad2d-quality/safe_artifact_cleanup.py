#!/usr/bin/env python3
"""One-shot, provenance-gated deletion of *redundant* City2D GitHub Actions artifacts.

This script never removes a repository file, a raw source sprite, a SHA manifest,
a local Unity project or an artifact from another workstream. Default: dry run.
"""
import hashlib
import json
import os
from pathlib import Path
from urllib import error, request

REPOSITORY = "mt5hjfz2kb-lab/Eldoria-Prewiu"
BRANCH = "art/ciudad-2d-hitos-1-5-20261116"
ROOT = Path(__file__).resolve().parents[2]
QA = ROOT / "Unity/Assets/Experimentos/Ciudad2D/QA"
REQUEST = Path(__file__).with_name("artifact_cleanup_request.json")
# Explicitly reviewed by SHA-protected retained QA/NativeV5 and QA/NativeV6.
APPROVED = {
    14335228213: (44096910663, "ArtQualityV5_NATIVE_EVIDENCE.json", 700_000_000),
    14337998938: (44105205248, "ArtQualityV6_NATIVE_EVIDENCE.json", 900_000_000),
}
BACKUP_ID = 14339683231
BACKUP_NAME = "ciudad2d-quality-v6-review-44109813247"


def sha256(path):
    with path.open("rb") as handle:
        digest = hashlib.sha256()
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
        return digest.hexdigest()


def github(endpoint, method="GET"):
    token = os.environ.get("GH_TOKEN")
    if not token:
        raise RuntimeError("GitHub token missing")
    url = f"https://api.github.com/repos/{REPOSITORY}/{endpoint}"
    req = request.Request(url, method=method, headers={
        "Authorization": "Bearer " + token,
        "Accept": "application/vnd.github+json",
        "X-GitHub-Api-Version": "2022-11-28",
        "User-Agent": "Eldoria-City2D-provenance-retention",
    })
    try:
        with request.urlopen(req, timeout=30) as response:
            content = response.read()
            return json.loads(content) if content else None
    except error.HTTPError as exc:
        if exc.code == 404:
            return None
        raise


def main():
    if os.environ.get("GITHUB_REPOSITORY") != REPOSITORY:
        raise RuntimeError("Refuse unrelated repository")
    if os.environ.get("GITHUB_REF_NAME") != BRANCH:
        raise RuntimeError("Refuse unrelated branch")

    spec = json.loads(REQUEST.read_text(encoding="utf-8"))
    assert spec["schema"] == 1 and spec["branch"] == BRANCH
    targets = spec.get("delete_artifact_ids")
    if not isinstance(targets, list) or not targets or len(targets) != len(set(targets)):
        raise RuntimeError("Missing or duplicate explicit artifact IDs")
    if any(type(item) is not int or item not in APPROVED for item in targets):
        raise RuntimeError("An unreviewed artifact ID was requested")
    dry_run = spec.get("dry_run")
    if type(dry_run) is not bool:
        raise RuntimeError("dry_run must be an explicit boolean")

    # First validate *all* kept native PNGs and the independently archived
    # full-image SHA manifest. Do not trust CI-green alone.
    verified = {}
    for artifact_id in targets:
        run_id, manifest_name, minimum_bytes = APPROVED[artifact_id]
        manifest = json.loads((QA / manifest_name).read_text(encoding="utf-8"))
        assert manifest["source_run_id"] == run_id
        assert manifest["source_artifact_id"] == artifact_id
        assert manifest["verified_original_frame_count"] == 60
        assert len(manifest["all_original_capture_sha256"]) == 60
        assert manifest["source_zip_sha256"]
        assert manifest["premium_art_approved"] is False
        preserved = manifest["archived_native_png_files"]
        assert len(preserved) == 10
        for img in preserved:
            file = ROOT / img["file"]
            if not file.is_file() or file.stat().st_size < 8000:
                raise RuntimeError("Missing protected native QA file " + str(file))
            if sha256(file) != img["sha256"]:
                raise RuntimeError("Native QA SHA mismatch " + str(file))
        meta = github(f"actions/artifacts/{artifact_id}")
        if meta is None:
            print("CITY2D_ALREADY_REMOVED", artifact_id)
            verified[artifact_id] = 0
            continue
        if (meta.get("name") != f"ciudad2d-real-unity-{run_id}"
                or meta.get("size_in_bytes", 0) < minimum_bytes):
            raise RuntimeError("Artifact is not the original reviewed large Unity bundle")
        if meta.get("expired"):
            raise RuntimeError("Artifact expired unexpectedly")
        verified[artifact_id] = meta["size_in_bytes"]
        print("CITY2D_VERIFIED_10_NATIVE_SHA", manifest_name,
              "originalFrames", len(manifest["all_original_capture_sha256"]))

    # The latest 40 MiB portable review artifact must also survive untouched.
    backup = github(f"actions/artifacts/{BACKUP_ID}")
    if (not backup or backup.get("name") != BACKUP_NAME
            or backup.get("expired") or backup.get("size_in_bytes", 0) < 40_000_000):
        raise RuntimeError("Latest curated V6 evidence artifact missing; no cleanup permitted")

    if dry_run:
        print("CITY2D_CLEANUP_DRY_RUN_PASS", verified,
              "bytes_claimed", sum(verified.values()))
        return

    # Delete only the exact historical bundles above. The current branch,
    # source sprites, Git commits, curated backup and run logs are untouched.
    for artifact_id in targets:
        if verified[artifact_id] == 0:
            continue
        github(f"actions/artifacts/{artifact_id}", method="DELETE")
        if github(f"actions/artifacts/{artifact_id}") is not None:
            raise RuntimeError("Artifact still exists after DELETE: " + str(artifact_id))
        print("CITY2D_REDUNDANT_ARTIFACT_DELETED", artifact_id,
              "bytes", verified[artifact_id])
    print("CITY2D_CLEANUP_SUCCESS_RELEASED_BYTES", sum(verified.values()))


if __name__ == "__main__":
    main()
