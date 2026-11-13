#!/usr/bin/env python3
"""Prepare an immutable, owner-only same-binary lazy Region 1 Mac A/B site.

Runs only after genuine Unity PlayMode, native I-V, terrain parity and WebGL
are SUCCESS. Reuses beta14's fixed shell; no textures, models or WASM changed.
This script neither publishes nor grants testers access.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path
from types import SimpleNamespace
import prepare_structural_site as structural

NATIVE_STATUS = "INTEGRATION_PASS_ART_REVIEW_REQUIRED"
WEB_STATUS = "WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED"
FROZEN_SOURCE = "948ffa9837168a0b68dab80be76b66350422b23a"
BETA14_SOURCE = "63fb2095412a3066e18f3a8a31c2a03f40ef4944"
BETA14_REVISION = "native325-mobile14-city-only-owner-qa"
LAZY_HOST_MODE = "FULL_REGION1_LAZY_OPTIN_OVER_EXACT_FROZEN_BETA10_UX19"

def require(condition, reason):
    if not condition:
        raise ValueError(reason)

def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))

def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()

def exact_build_dir(root, web, native_run, source_head, native_attempt=1, proof_path=None):
    """Select the recorded run, then verify every compiled file without altering siblings."""
    require(str(native_run).isdigit() and str(native_attempt).isdigit(), "Invalid native run/attempt")
    require(web.get("source_sha") == source_head and web.get("status") == WEB_STATUS,
            "Build selection source/status mismatch")
    root = Path(root).resolve()
    expected_name = f"pr179-lazy-full-webgl-{native_run}-{native_attempt}"
    candidates = []
    for index in root.rglob("index.html"):
        candidate = index.parent
        require(not index.is_symlink(), "Symlink index is forbidden")
        if index.is_file() and (candidate / "Build").is_dir():
            require(candidate.resolve().is_relative_to(root), "Build escaped artifact root")
            candidates.append(candidate)
    exact = [p for p in candidates if p.name == expected_name]
    if len(candidates) == 1 and candidates[0] == root:
        selected = root  # Correct future producer uploads its contents directly.
    else:
        require(len(exact) == 1, "Missing or ambiguous exact native run/attempt build")
        selected = exact[0]
    expected = []
    prefix = "Builds/ValoriaWebPlaytest/"
    for raw in web.get("files", []):
        require(isinstance(raw, str), "Invalid compiled file entry")
        normalized = raw.replace("\\", "/")
        require(normalized.startswith(prefix), "Unexpected compiled manifest prefix")
        rel = normalized[len(prefix):]
        require(rel and all(p not in ("", ".", "..") for p in rel.split("/")), "Unsafe compiled manifest path")
        expected.append(rel)
    require(expected and len(expected) == len(set(expected)), "Empty or duplicate compiled manifest")
    actual = []
    for path in selected.rglob("*"):
        require(not path.is_symlink(), "Symlink in compiled build is forbidden")
        if path.is_file():
            actual.append(path.relative_to(selected).as_posix())
    require(set(actual) == set(expected), "Compiled file membership mismatch")
    total = sum((selected / rel).stat().st_size for rel in actual)
    require(type(web.get("bytes")) is int and total == web["bytes"], "Compiled total byte count mismatch")
    result = dict(source_sha=source_head, native_run=str(native_run), native_attempt=str(native_attempt),
                  selected_root=str(selected.relative_to(root)),
                  ignored_siblings=[str(p.relative_to(root)) for p in candidates if p != selected],
                  bytes=total, files=[dict(path=rel, bytes=(selected/rel).stat().st_size,
                      sha256=digest(selected/rel)) for rel in sorted(actual)])
    if proof_path is not None:
        Path(proof_path).write_text(json.dumps(result, indent=2))
    return selected

def sole_dir(root, marker, required=()):
    root = Path(root)
    found = [
        p.parent for p in root.rglob(marker)
        if p.is_file() and all((p.parent / rel).exists() for rel in required)
    ]
    require(len(found) == 1, f"Expected exactly one verified {marker} under {root}, got {len(found)}")
    return found[0]

def verify_source_proofs(proof, playmode, source_head):
    proof, playmode = Path(proof), Path(playmode)
    play = read_json(playmode / "lazy-world-native-result.json")
    require(
        play.get("source_head") == source_head
        and play.get("unity_tests_passed") is True
        and play.get("restored") is True
        and play.get("frozen_ux_files") == 19
        and play.get("hot_source_base") == FROZEN_SOURCE,
        "Real full-world PlayMode and exact hot Unity restoration NOT certified"
    )
    restore = read_json(proof / "warm-cache-restoration.json")
    require(
        restore.get("restored") is True
        and not restore.get("conflicts")
        and not restore.get("status_changes")
        and not restore.get("existing_input_changes"),
        "Full-world native source or authored assets were NOT restored"
    )
    overlay = read_json(proof / "source-overlay.json")
    require(
        overlay.get("overlay_commit") == source_head
        and sum(item.get("mode") == LAZY_HOST_MODE for item in overlay.get("files", [])) == 1,
        "Native build did not compose exactly one real lazy host with frozen UX19"
    )
    frozen = read_json(proof / "ux-source-overlay.json")
    paths = [x["path"] for x in frozen.get("files", [])]
    require(
        len(paths) == 19 and len(set(paths)) == 19
        and any(path.endswith("/RoyalContracts.cs") for path in paths)
        and any(path.endswith("/ValoriaIntegratedWorld.cs") for path in paths),
        "Required 19 pretester UX dependencies are not proven"
    )
    parity = read_json(proof / "comparison.json")
    require(
        parity.get("status") == "NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION"
        and len(parity.get("passes", [])) >= 2,
        "Real region physics/route parity is not proved"
    )
    native = read_json(proof / "native-journey" / "integration-result.json")
    require(native.get("status") == NATIVE_STATUS, "Real native I-V journey has not passed")
    web = read_json(proof / "web-build.json")
    require(
        web.get("status") == WEB_STATUS and web.get("source_sha") == source_head,
        "Compiled WebGL does not belong to exact reviewed PR179 HEAD"
    )
    return {"playmode": play, "native": native, "web": web, "parity": parity}

def verify_beta14_shell(shell):
    shell = Path(shell)
    origin = read_json(shell / "release-identity.json")
    require(
        origin.get("source_sha") == BETA14_SOURCE
        and origin.get("revision") == BETA14_REVISION
        and origin.get("release_for_external_testers") is False,
        "Shell not taken from the frozen owner-confirmed beta14 city-only artifact"
    )
    for name in ("index.html", "style.css", "city.png", "play.js"):
        require((shell / name).is_file(), f"Required frozen wrapper is missing: {name}")
    return {name: digest(shell / name) for name in ("index.html", "style.css", "city.png", "play.js")}

def package_owner(a):
    require(re.fullmatch(r"[0-9a-f]{40}", a.source_head) is not None, "Invalid pinned PR HEAD")
    require(a.revision == "native325-mobile15-full-lazy-owner-qa", "Unexpected owner-only candidate label")
    require(str(a.native_run).isdigit(), "Invalid native run id")
    evidence = sole_dir(a.native_artifact, "comparison.json", (
        "warm-cache-restoration.json", "source-overlay.json", "ux-source-overlay.json",
        "web-build.json", "native-journey/integration-result.json"))
    playmode = sole_dir(a.playmode_artifact, "lazy-world-native-result.json")
    shell = sole_dir(a.beta14_shell_artifact, "release-identity.json", (
        "index.html", "style.css", "city.png", "play.js"))
    proofs = verify_source_proofs(evidence, playmode, a.source_head)
    shell_hashes = verify_beta14_shell(shell)
    out = Path(a.output)
    work = Path(a.work)
    require(not out.exists() and not work.exists(), "Owner QA artifact or scratch would be overwritten")
    work.mkdir(parents=True)
    build = exact_build_dir(a.webgl_artifact, proofs["web"], a.native_run,
                            a.source_head, getattr(a, "native_attempt", 1),
                            work / "exact-build-selection.json")
    decoded = work / "original"
    structural.extract(build, decoded)
    original_sha = digest(decoded / "data.unity3d")
    extraction = read_json(decoded / "extraction.json")
    require(original_sha == extraction["bundle_sha256"], "Original native data bundle is not byte-exact")
    # NO texture dedup/compression transform: reconstruct the delivery chunks
    # directly from the original genuine WebGL build's embedded data.unity3d.
    texture_proof = work / "unchanged-bundle.json"
    texture_proof.write_text(json.dumps({
        "source_sha256": original_sha, "candidate_sha256": original_sha,
        "transformation": "none", "original_bundle_unchanged": True
    }, indent=2))
    structural.package(SimpleNamespace(
        build=str(build), wrapper=str(shell), output=str(out),
        data=str(decoded / "data.bin"), bundle=str(decoded / "data.unity3d"),
        texture_proof=str(texture_proof),
        native_proof=str(evidence / "comparison.json"),
        web_proof=str(evidence / "web-build.json"),
        overlay_commit=a.source_head, revision=a.revision, native_run=str(a.native_run)))
    identity_path = out / "release-identity.json"
    identity = read_json(identity_path)
    require(
        identity.get("source_sha") == a.source_head
        and identity.get("candidate_bundle_sha256") == original_sha,
        "Packaged candidate no longer matches exact compiled native data"
    )
    identity.update(
        source_composition="One frozen 19-UX baseline plus reviewed additive full real Region1 lazy host; exact native WebGL bytes, no texture transform",
        texture_payload_modified=False,
        original_full_world_bundle_sha256=original_sha,
        full_region1_compiled=True,
        real_lazy_unity_playmode_passed=True,
        real_native_i_v_journey_passed=True,
        mac_same_binary_ab_verified=False,
        physical_iphone_30min_verified=False,
        physical_android_30min_verified=False,
        world_entry_physical_verified=False,
        runtime_verified=False,
        release_for_external_testers=False,
        owner_only=True,
        recommended_owner_opt_in="eldoria_p0_lazy_world=1",
        published=False
    )
    identity_path.write_text(json.dumps(identity, indent=2, ensure_ascii=False))
    proof = {
        "source_head": a.source_head, "native_run": str(a.native_run),
        "same_compiled_binary_for_both_mac_ab_arms": True,
        "world_mode_differs_only_by_url_query": "eldoria_p0_lazy_world=1",
        "unchanged_original_bundle_sha256": original_sha,
        "frozen_beta14_shell_sha256": shell_hashes,
        "unity_playmode_verified": True,
        "native_journey_verified": True,
        "mac_ab_pending": True, "physical_iphone_pass": False, "physical_android_pass": False,
        "testers_authorized": False, "published": False
    }
    (out / "owner-only-lazy-proof.json").write_text(json.dumps(proof, indent=2))
    print("OWNER_ONLY_FULL_WORLD_LAZY_SOURCE_COMPILED_NOT_MAC_OR_IPHONE_CERTIFIED", a.source_head)

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    for key in ("native-artifact", "playmode-artifact", "webgl-artifact",
                "beta14-shell-artifact", "output", "work",
                "source-head", "native-run", "revision"):
        parser.add_argument("--" + key, required=True)
    package_owner(parser.parse_args())
