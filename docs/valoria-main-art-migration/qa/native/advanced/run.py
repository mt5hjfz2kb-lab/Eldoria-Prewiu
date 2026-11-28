"""Capture advanced fixture states with Main's production Vigilia canvas and PNG writer."""
import importlib.util
import json
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location("valoria_native_runner", HERE.parent / "run.py")
native = importlib.util.module_from_spec(spec)
spec.loader.exec_module(native)
SUITE = "advanced"
LABELS = ("city-level-4", "city-level-5", "bastion-before-priority", "objective-expanded",
          "world-preset-2", "world-preset-1", "world-preset-0", "three-active-marches")
SIZES = ("390x844", "844x390")
fixture = dict(native.SUITES["destinations"])
fixture["expected"] = [f"real-unity-{label}-{size}" for label in LABELS for size in SIZES]
native.SUITES = {SUITE: fixture}
native.CITY_STATES = {SUITE: {f"real-unity-city-level-{level}-{size}": level
                              for level in (4, 5) for size in SIZES}}
patch_writer = native.patched


def once(data, old, new):
    if data.count(old) != 1:
        raise ValueError("The original Unity fixture has changed at " + old[:75].decode())
    return data.replace(old, new)


def instrument(data, suite):
    data = patch_writer(data, suite)
    insertion = b"        static void CaptureRealDestinations()"
    marker = b"                    CaptureRealDestinations();"
    newline = b"\r\n" if b"\r\n" in data else b"\n"
    extra = (HERE / "AdvancedFrames.cs.inc").read_bytes().replace(b"\r\n", b"\n").replace(b"\n", newline)
    data = once(data, insertion, extra + newline + insertion)
    start = data.index(marker)
    end = data.index(b"EditorApplication.Exit(0);", start) + len(b"EditorApplication.Exit(0);")
    data = once(data, data[start:end], b"                    host.StartCoroutine(AdvancedGuard());")
    writer = (native.RECORD + "(full,camera,width,height);").encode()
    city = b"""
                if (name.StartsWith("city-level-",StringComparison.Ordinal))
                    global::EldoriaCityBaselineSnapshot.Capture(Path.Combine(
                        Environment.GetEnvironmentVariable("VALORIA_MIGRATION_OUTPUT"),"advanced"),
                        Path.GetFileNameWithoutExtension(full));
"""
    return once(data, writer, writer + city)


validate_frames = native.validate


def validate(folder, suite_name, source, unity_version):
    result = validate_frames(folder, suite_name, source, unity_version)
    # Retired Royal controls can remain enabled beneath a disabled Canvas. Keep
    # candidate controls and owning Canvas evidence for subsequent input review.
    observations = []
    for size in SIZES:
        state = "real-unity-bastion-before-priority-" + size
        path = folder / (state + ".inventory.json")
        if not path.exists():
            continue
        report = json.loads(path.read_text(encoding="utf-8-sig"))
        observations.append({"state": state, "councilNamedControls": [
            row["path"] for row in report["controls"]
            if "council" in row["path"].lower() or "consejo" in row["path"].lower()],
            "enabledCanvases": [row["path"] for row in report["canvases"]
                                if row["activeInHierarchy"] and row["enabled"]]})
    result["councilObservations"] = observations
    result["councilInputParity"] = "PENDING separate first-hit and destination verification"
    result["provenance"] = "Disposable seeded Main states; canonical Vigilia UI; three real gateway commands plus two preparations."
    return result


native.validate = validate
native.patched = instrument

if __name__ == "__main__":
    sys.exit(native.main())
