"""Stage editor-only observation in a disposable checkout and run canonical Unity fixtures."""
import argparse
import contextlib
import fnmatch
import hashlib
import json
import os
from pathlib import Path
import shutil
import struct
import subprocess
import sys
import zlib

BREACH = "Assets/Eldoria/BreachWatch/Editor/"
RECORD = "global::Eldoria.MigrationQA.ValoriaMigrationCapture.Record"
SUITES = {
    "hud": {
        "path": BREACH + "BreachWatchNativeEvidence.cs",
        "method": "Eldoria.BreachWatch.Editor.BreachWatchNativeEvidence.CaptureBatch",
        "hooks": [
            ('File.WriteAllBytes(Path.Combine(folder,"nickname-390x844.png"),pixels.EncodeToPNG());',
             'Path.Combine(folder,"nickname-390x844.png"),camera,390,844'),
            ('File.WriteAllBytes(Path.Combine(folder,"story-visible-"+storyPages+".png"),pixels.EncodeToPNG());',
             'Path.Combine(folder,"story-visible-"+storyPages+".png"),camera,390,844'),
            ('File.WriteAllBytes(file,image.EncodeToPNG());', 'file,camera,w,h'),
        ],
        "expected": ["nickname-390x844", "story-visible-*"] + [
            "real-unity-" + state + "-" + size for state in ("city", "bastion")
            for size in ("1600x900", "900x1600", "844x390", "390x844")
        ],
    },
    "destinations": {
        "path": BREACH + "BreachWatchDestinationNativeEvidence.cs",
        "method": "Eldoria.BreachWatch.Editor.BreachWatchDestinationNativeEvidence.CaptureBatch",
        "hooks": [('File.WriteAllBytes(full,texture.EncodeToPNG());','full,camera,width,height')],
        "expected": ["real-unity-" + state + "-390x844" for state in (
            "nav-advanced", "training", "march", "march-information", "heroes",
            "hero-aldric-resumen", "hero-aldric-habilidades", "hero-aldric-talentos",
            "hero-aldric-equipamiento", "hero-lyra-resumen", "mail", "mail-with-report-row",
            "mail-report-document", "vigilants", "scouts", "treasures"
        )],
    },
    "retention": {
        "path": BREACH + "BreachWatchRetentionInteractionEvidence.cs",
        "method": "Eldoria.BreachWatch.Editor.BreachWatchRetentionInteractionEvidence.Capture",
        "hooks": [('File.WriteAllBytes(Path.Combine(Output,file+".png"),pixels.EncodeToPNG());',
                   'Path.Combine(Output,file+".png"),camera,390,844')],
        "expected": ["vigilants-before-real-battle", "vigilants-real-victory", "vigilants-real-defeat",
                     "scouts-real-underway", "scouts-real-ready", "scouts-three-real-discoveries",
                     "scouts-six-real-reports-four-families"],
    },
    "bag": {
        "path": "Assets/Eldoria/Issue186Art/Editor/Issue186BagCombinedNativeEvidence.cs",
        "method": "Eldoria.Issue186Art.Editor.Issue186BagCombinedNativeEvidence.Capture",
        "hooks": [('File.WriteAllBytes(Path.Combine(Output,file),bytes);',
                   'Path.Combine(Output,file),camera,width,height')],
        "expected": ["real-vigilia-bag-" + state + "-*" for state in (
            "actual-integrated-city", "new", "mail-read-unclaimed", "mail-claimed-no-credit", "used-reloaded"
        )],
    },
}


CITY_STATES = {
    "hud": {"real-unity-city-390x844": 1, "real-unity-bastion-390x844": 1},
    "destinations": {"real-unity-nav-advanced-390x844": 2},
    "bag": {"real-vigilia-bag-actual-integrated-city-390x844": 3},
}


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def git(root, *args):
    return subprocess.check_output(["git", "-C", str(root), *args], text=True).strip()


def patched(original, suite):
    data = original
    for anchor, args in suite["hooks"]:
        token = anchor.encode()
        if data.count(token) != 1:
            raise ValueError("Expected one unmodified native PNG writer: " + anchor)
        data = data.replace(token, token + (" " + RECORD + "(" + args + ");").encode())
    return data


def json_write(path, value):
    temporary = path.with_suffix(".tmp")
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")
    os.replace(temporary, path)


def png_size(data):
    if not data.startswith(b"\x89PNG\r\n\x1a\n"):
        raise ValueError("Invalid PNG signature.")
    offset, dimensions, has_pixels = 8, None, False
    while offset + 12 <= len(data):
        size = struct.unpack_from(">I", data, offset)[0]
        chunk = data[offset + 4:offset + 8]
        end = offset + 8 + size
        if end + 4 > len(data) or zlib.crc32(data[offset + 4:end]) != struct.unpack_from(">I", data, end)[0]:
            raise ValueError("Invalid PNG chunk.")
        if dimensions is None:
            if chunk != b"IHDR" or size != 13:
                raise ValueError("Missing PNG header.")
            dimensions = struct.unpack_from(">II", data, offset + 8)
        if chunk == b"IDAT" and size:
            has_pixels = True
        if chunk == b"IEND" and size == 0 and has_pixels and end + 4 == len(data):
            return dimensions
        offset = end + 4
    raise ValueError("Incomplete PNG image stream.")


def validate(folder, suite_name, source, unity_version):
    all_states = set()
    frames = []
    for sample_path in sorted(folder.glob("*.sample.json")):
        sample = json.loads(sample_path.read_text(encoding="utf-8-sig"))
        state = sample["state"]
        if sample["suite"] != suite_name or sample["sourceCommit"] != source or state in all_states:
            raise ValueError("Duplicate or incorrectly attributed sample: " + sample_path.name)
        if sample_path.name != state + ".sample.json" or sample["inventory"] != state + ".inventory.json" or sample["screenshot"] != state + ".png":
            raise ValueError("Unexpected sample filenames: " + sample_path.name)
        image = (folder / sample["screenshot"]).read_bytes()
        if png_size(image) != (sample["width"], sample["height"]) or sha256(image) != sample["screenshotSha256"]:
            raise ValueError("Screenshot size or content mismatch: " + state)
        inventory_bytes = (folder / sample["inventory"]).read_bytes()
        report = json.loads(inventory_bytes.decode("utf-8-sig"))
        if (report["sourceCommit"], report["state"], report["frame"], report["scene"]) != (source, state, sample["frame"], sample["scene"]):
            raise ValueError("Observations differ in source, frame, scene or state: " + state)
        if sample["unity"] != unity_version or report["unity"] != unity_version:
            raise ValueError("Wrong Unity editor version for the project: " + state)
        if not report["controls"] or not report["cameras"] or report["scene"] != "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity":
            raise ValueError("Missing scene, controls or cameras in " + state)
        frame = {"state": state, "inventorySha256": sha256(inventory_bytes),
                 "screenshotSha256": sha256(image), "width": sample["width"], "height": sample["height"]}
        if state in CITY_STATES.get(suite_name, {}):
            city_bytes = (folder / (state + ".city-focus.json")).read_bytes()
            city = json.loads(city_bytes.decode("utf-8-sig"))
            expected = (source, state, sample["frame"], sample["scene"], sample["width"],
                        sample["height"], CITY_STATES[suite_name][state], False, True)
            actual = tuple(city[k] for k in ("sourceCommit", "sample", "frame", "scene", "targetWidth",
                                             "targetHeight", "bastion", "world", "ready"))
            if actual != expected or {b["id"] for b in city["buildings"]} != {"bastion", "sawmill", "barracks", "granary", "stoneworks"}:
                raise ValueError("City focus evidence differs in level, geometry, frame, buildings or source: " + state)
            frame["cityFocusSha256"] = sha256(city_bytes)
            frame["bastion"] = city["bastion"]
        all_states.add(state)
        frames.append(frame)
    missing = [pattern for pattern in SUITES[suite_name]["expected"] if not fnmatch.filter(all_states, pattern)]
    return {"samples": frames, "missingExpected": missing}


@contextlib.contextmanager
def stage(project, probe, fixture_hashes):
    # Unity default editor assembly. No changes to scene, runtime scripts, package manifest or game data.
    directory = project / "Assets" / "Editor" / "ValoriaMigrationCaptureQA"
    if directory.exists() or directory.with_suffix(".meta").exists():
        raise ValueError("Capture staging directory already exists: " + str(directory))
    directory.parent.mkdir(exist_ok=True)
    directory.mkdir()
    originals = []
    try:
        shutil.copyfile(probe, directory / "ValoriaLiveInventory.cs")
        driver = Path(__file__).with_name("ValoriaMigrationCapture.cs")
        shutil.copyfile(driver, directory / driver.name)
        city_probe = Path(__file__).with_name("EldoriaCityBaselineSnapshot.cs")
        shutil.copyfile(city_probe, directory / city_probe.name)
        for key, suite in SUITES.items():
            path = project / suite["path"]
            original = path.read_bytes()
            changed = patched(original, suite)
            fixture_hashes[key] = {"sourceSha256": sha256(original), "instrumentedSha256": sha256(changed)}
            originals.append((path, original, changed))
            path.write_bytes(changed)
        yield
    finally:
        conflicts = []
        for path, original, changed in originals:
            if path.read_bytes() == changed:
                path.write_bytes(original)
            elif path.read_bytes() != original:
                conflicts.append(str(path))
        shutil.rmtree(directory)
        directory.with_suffix(".meta").unlink(missing_ok=True)
        if conflicts:
            raise RuntimeError("Unexpected concurrent change in disposable worktree: " + ", ".join(conflicts))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for arg in ("checkout", "unity", "output", "probe"):
        parser.add_argument("--" + arg, required=True, type=Path)
    args = parser.parse_args()
    root, unity, output, probe = [getattr(args, key).resolve() for key in ("checkout", "unity", "output", "probe")]
    if root == output or root in output.parents:
        parser.error("Use a fresh output directory outside the checkout.")
    if not (unity.is_file() and probe.is_file()):
        parser.error("Unity executable and editor-only inventory probe must exist.")
    if git(root, "rev-parse", "--show-toplevel").replace("\\", "/").lower() != root.as_posix().lower():
        parser.error("Checkout must be the repository root.")
    if git(root, "status", "--porcelain", "--untracked-files=no"):
        parser.error("Capture requires a clean tracked checkout at the revision being tested.")
    if git(root, "ls-files", "--others", "--", "Unity/Assets", "Unity/Packages", "Unity/ProjectSettings"):
        parser.error("Untracked input under Unity/Assets, Packages or ProjectSettings in disposable checkout.")
    source = git(root, "rev-parse", "HEAD")
    project = root / "Unity"
    version = (project / "ProjectSettings" / "ProjectVersion.txt").read_text(encoding="utf-8")
    output.mkdir(parents=True, exist_ok=False)
    unity_version = next(line.split(":", 1)[1].strip() for line in version.splitlines() if line.startswith("m_EditorVersion:"))
    manifest = {"sourceCommit": source, "projectVersion": unity_version,
                "driverSha256": sha256(Path(__file__).with_name("ValoriaMigrationCapture.cs").read_bytes()),
                "cityProbeSha256": sha256(Path(__file__).with_name("EldoriaCityBaselineSnapshot.cs").read_bytes()),
                "probeSha256": sha256(probe.read_bytes()), "launcherSha256": sha256(Path(__file__).read_bytes()),
                "fixtures": {}, "runs": {}, "status": "INCOMPLETE"}
    try:
        with stage(project, probe, manifest["fixtures"]):
            for key, suite in SUITES.items():
                env = dict(os.environ, VALORIA_GAMEPLAY_PROBE="1", ELDORIA_NEW_UI_ONLY="1",
                           ELDORIA_QA_SOURCE_SHA=source, GITHUB_SHA=source, ISSUE186_VIGILIA_SOURCE=source,
                           VALORIA_MIGRATION_OUTPUT=str(output), VALORIA_MIGRATION_SUITE=key,
                           EARLY_RETENTION_EVIDENCE=str(output / "retention-fixture"))
                log = output / (key + ".log")
                cmd = [str(unity), "-batchmode", "-projectPath", str(project), "-executeMethod",
                       suite["method"], "-logFile", str(log)]
                launched = subprocess.run(cmd, env=env, cwd=root)
                result = {"exit": launched.returncode, "method": suite["method"]}
                manifest["runs"][key] = result
                try:
                    result.update(validate(output / key, key, source, unity_version))
                except (OSError, ValueError, KeyError, struct.error) as error:
                    result["validationError"] = str(error)
                json_write(output / "manifest.json", manifest)
        if all(r["exit"] == 0 and not r.get("validationError") and not r["missingExpected"]
               for r in manifest["runs"].values()) and set(manifest["runs"]) == set(SUITES):
            manifest["status"] = "CAPTURED"
    except Exception as error:
        manifest["error"] = str(error)
        raise
    finally:
        json_write(output / "manifest.json", manifest)
    return 0 if manifest["status"] == "CAPTURED" else 1


if __name__ == "__main__":
    sys.exit(main())
