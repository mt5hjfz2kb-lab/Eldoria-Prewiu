"""Package an unpublished integrated candidate; never reuse an older engine.

The stable shell is pinned separately. Native source/manifest checks are required;
browser, artistic and physical-device approval remain independent release gates.
"""
import argparse
import gzip
import hashlib
import json
import re
import shutil
from pathlib import Path

from prepare_lazy_owner_site import exact_build_dir, read_json, require, digest
from prepare_structural_site import extract, only


def package(a):
    require(re.fullmatch(r"[0-9a-f]{40}", a.source_head), "Invalid source SHA")
    require(re.fullmatch(r"integrated-mobile[0-9]+-owner-qa", a.revision), "Invalid candidate revision")
    proof, ui, shell = map(Path, (a.build_proof, a.ui_proof, a.stable_shell))
    journey = read_json(proof / "integration-result.json")
    require(journey.get("source") == a.source_head and
            journey.get("status") == "INTEGRATION_PASS_ART_REVIEW_REQUIRED",
            "Exact native journey has not passed")
    require({"continuous native I-V authored objectives complete",
             "continuous I-V final saved state exact"}.issubset(journey.get("checks", [])),
            "Complete native campaign/save proof missing")
    destinations = read_json(ui / "BreachWatchDestinationEvidence.json")
    treasures = read_json(ui / "BreachWatchIntegratedTreasureEvidence.json")
    for report, status in ((destinations, "VIGILIA_DESTINATIONS_CAPTURED_ART_REVIEW_PENDING"),
                           (treasures, "COMBINED_TREASURE_ENTRY_CALENDAR_TWO_TOUCHES_PASS")):
        require(report.get("source") == a.source_head and report.get("status") == status
                and report.get("saveUnchanged") is True, "Exact integrated UI proof missing")
        screens = report.get("screens", [])
        require(len(screens) == report.get("screenshotCount") and
                len({shot["file"] for shot in screens}) == len(screens),
                "Missing or duplicate screenshot evidence")
        for shot in screens:
            path = ui / shot["file"]
            require(path.resolve().is_relative_to(ui.resolve()) and not path.is_symlink(),
                    "Unsafe screenshot path")
            require(digest(path).lower() == shot["sha256"].lower(), "Screenshot bytes changed")
    require(destinations.get("screenshotCount", 0) >= 16 and treasures.get("screenshotCount") == 4,
            "Incomplete native destination views")
    require(digest(shell / "release-identity.json") == a.stable_identity_sha256,
            "Stable wrapper identity changed")
    web = read_json(proof / "web-build.json")
    out, work = Path(a.output), Path(a.work)
    require(not out.exists() and not work.exists(), "Refuse replacing an existing candidate")
    work.mkdir(parents=True)
    build = exact_build_dir(a.webgl_artifact, web, a.native_run, a.source_head,
                            proof_path=work / "exact-build-selection.json")
    decoded = work / "original"
    extract(build, decoded)
    profile_proof = None
    data = decoded / "data.bin"
    bundle = decoded / "data.unity3d"
    parent = getattr(a, "reference_parent", None)
    reference_site = getattr(a, "reference_site", None)
    require(bool(parent) == bool(reference_site), "Both stable texture profile inputs are required")
    if parent:
        # Reuse only byte-exact pre-existing texture transformations used by
        # stable mobile18; never choose new compression or alter gameplay.
        from types import SimpleNamespace
        from prepare_lazy_profile_owner_site import extract_reference
        import deduplicate_exact_textures as dedup
        import reuse_mobile6_profile as profile
        from mobile_site import pack_data
        reference = work / "mobile6.unity3d"
        extract_reference(reference_site, reference)
        exact = work / "deduplicated"
        dedup.apply(bundle, exact, digest(bundle))
        exact_proof = read_json(exact / "integrity.json")
        transformed = work / "profile"
        profile.run(SimpleNamespace(parent=str(parent), reference=str(reference),
            fresh=str(exact / "data.unity3d"), expected_fresh_sha256=exact_proof["candidate_sha256"],
            output=str(transformed)))
        profile_proof = read_json(transformed / "integrity.json")
        require(profile_proof.get("exact_serialized_matches_only") is True and
                profile_proof["all_objects_verified"] == exact_proof["all_objects_verified"],
                "Stable texture profile verification chain broken")
        bundle = transformed / "data.unity3d"
        data = work / "profiled-data.bin"
        pack_data(decoded / "data.bin", bundle, data)
    out.mkdir(parents=True)
    if profile_proof:
        shutil.copyfile(work / "deduplicated/integrity.json", out / "exact-dedup-integrity.json")
        shutil.copyfile(work / "profile/integrity.json", out / "stable-texture-profile-integrity.json")
    shell_hashes = {}
    for name in ("index.html", "style.css", "city.png", "play.js"):
        require(not (shell / name).is_symlink(), "Symlink wrapper input")
        shell_hashes[name] = digest(shell / name)
        shutil.copyfile(shell / name, out / name)
    script = (out / "play.js").read_text()
    script, count = re.subn(r'window\.__eldoriaDiagnosticRevision="[^"]*";',
                           'window.__eldoriaDiagnosticRevision='+json.dumps(a.revision)+';', script, count=1)
    require(count == 1, "Missing shell revision marker")
    version = re.search(r"mobile([0-9]+)", a.revision).group(1)
    script, count = re.subn(r'button\.textContent\s*=\s*"Diagnóstico[^"]*";',
                           'button.textContent = "Diagnóstico · beta '+version+'";', script, count=1)
    require(count == 1, "Missing diagnostic button")
    script_name = "play-"+hashlib.sha256(script.encode()).hexdigest()[:16]+".js"
    (out / "play.js").write_text(script)
    (out / script_name).write_text(script)
    html, count = re.subn(r'src="play(?:-[a-f0-9]+)?\.js(?:\?[^"]*)?"',
                         'src="'+script_name+'"', (out / "index.html").read_text())
    require(count == 1, "Ambiguous wrapper entry point")
    (out / "index.html").write_text(html)
    for directory in ("StreamingAssets", "TemplateData"):
        if (build / directory).is_dir():
            shutil.copytree(build / directory, out / directory)
    icon = shell / "TemplateData/webmemd-icon.png"
    if icon.exists() and not (out / "TemplateData/webmemd-icon.png").exists():
        (out / "TemplateData").mkdir(exist_ok=True)
        shutil.copyfile(icon, out / "TemplateData/webmemd-icon.png")
    (out / "Build").mkdir()
    manifest = dict(revision=a.revision, chunks={}, decoded_sizes={}, decoded_parts={},
                    delivery_adapter="github-pages-exact-gzip-integrated-v1")
    for key, pattern in (("loader", "*.loader.js"), ("framework", "*.framework.js.unityweb"),
                         ("code", "*.wasm.unityweb")):
        source = only(build, pattern)
        relative = "Build/"+source.name
        shutil.copyfile(source, out / relative)
        manifest[key] = relative+"?v="+digest(source)[:16]
        if key != "loader":
            raw = gzip.decompress(source.read_bytes())
            manifest["decoded_sizes"][manifest[key]] = len(raw)
            manifest["decoded_parts"][manifest[key]] = [dict(url=relative, encoding="gzip",
                decoded_bytes=len(raw), decoded_sha256=hashlib.sha256(raw).hexdigest())]
    manifest["data"] = "Build/"+only(build, "*.data.unityweb").name+"?revision="+a.revision
    manifest["decoded_sizes"][manifest["data"]] = data.stat().st_size
    parts = []
    (out / "payload-decoded").mkdir()
    # Chunk verified data. With a profile, only the independently verified
    # existing texture transformations differ; engine files remain exact.
    with data.open("rb") as stream:
        while block := stream.read(16 * 1024 * 1024):
            relative = "payload-decoded/"+a.revision+"-"+str(len(parts))+".gz"
            (out / relative).write_bytes(gzip.compress(block, compresslevel=6, mtime=0))
            parts.append(dict(url=relative, encoding="gzip", decoded_bytes=len(block),
                              decoded_sha256=hashlib.sha256(block).hexdigest()))
    manifest["decoded_parts"][manifest["data"]] = parts
    reconstructed = hashlib.sha256()
    for part in parts:
        reconstructed.update(gzip.decompress((out / part["url"]).read_bytes()))
    require(reconstructed.hexdigest() == digest(data), "Delivery chunks changed engine data")
    (out / "build-manifest.json").write_text(json.dumps(manifest, indent=2))
    identity = dict(revision=a.revision, source_sha=a.source_head, native_run=a.native_run,
        source_composition="Exact integrated Git source; no historical source overlays",
        stable_shell_sha256=shell_hashes, native_journey_verified=True,
        native_destination_captures_verified=True, candidate_bundle_sha256=digest(bundle),
        original_data_sha256=digest(decoded / "data.bin"), delivered_data_sha256=digest(data),
        texture_payload_modified=profile_proof is not None,
        preexisting_mobile6_profile_reused=profile_proof is not None,
        no_new_texture_compression_decisions=True,
        artwork_approved=False, physical_mobile_tested=False, runtime_verified=False,
        release_for_external_testers=False, owner_only=True, published=False)
    (out / "release-identity.json").write_text(json.dumps(identity, indent=2))
    print("INTEGRATED_CANDIDATE_PACKAGED_UNPUBLISHED_BROWSER_AND_DEVICE_UNVERIFIED")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("source-head", "revision", "build-proof", "ui-proof", "stable-shell",
                 "stable-identity-sha256", "webgl-artifact", "native-run", "output", "work"):
        parser.add_argument("--"+name, required=True)
    parser.add_argument("--reference-parent")
    parser.add_argument("--reference-site")
    package(parser.parse_args())
