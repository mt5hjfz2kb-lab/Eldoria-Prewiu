#!/usr/bin/env python3
"""Read-only source gate and post-Unity integrity/actual-motion verification.

Does not create fake video frames, claim Unity compilation, or certify artwork.
Requires exactly the authentic Unity Editor RenderTexture output for --frames.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path
import shutil
import subprocess

SOURCE_SHA = "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06"
SOURCE = Path("Unity/Assets/Experimentos/Ciudad2D/QA/ArchivedScenes/"
              "Experimento_Ciudad2D_PremiumV6_44133747143.unity")
PILOT = Path("Unity/Assets/Experimentos/Ciudad2DDepthPilot")
C_SHARP = [
    PILOT / "DepthPilotActivity.cs",
    PILOT / "DepthPilotContact.cs",
    PILOT / "Editor/DepthPilotNativeCapture.cs",
    PILOT / "Tests/DepthPilotPlayModeTests.cs",
]
RELATED_DEPTH_CS = [
    Path("Unity/Assets/Experimentos/Ciudad2D/Scripts/City2DDepthSkyMotion.cs"),
    Path("Unity/Assets/Experimentos/Ciudad2D/Scripts/City2DGroundedActivity.cs"),
    Path("Unity/Assets/Experimentos/Ciudad2D/Scripts/Editor/City2DDepthPilotEvidence.cs"),
]
EXPECTED = 2 * 3 * (18 + 1)


def sha256(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def source_gate():
    assert SOURCE.exists(), f"V6 archive unavailable: {SOURCE}"
    assert sha256(SOURCE) == SOURCE_SHA, "Archived Premium V6 source altered"
    for src in C_SHARP:
        assert src.exists(), f"Missing pilot source {src}"
        s = src.read_text(encoding="utf-8")
        assert "namespace Eldoria.Experimentos.Ciudad2DDepthPilot" in s
        assert "Application.persistentDataPath" not in s
        assert "PlayerPrefs" not in s
        assert "ICommandGateway" not in s
        assert "LocalGateway" not in s
        assert "GameCommand" not in s
    for src in C_SHARP + RELATED_DEPTH_CS:
        assert src.exists(), f"Missing depth pilot C# source {src}"
        source = src.read_text(encoding="utf-8")
        # This namespace collision was missed by Tree-sitter but failed real Unity
        # with CS0234 in run 44275806196. Do not repeat the same omission.
        assert not re.search(r"(?<![\\w.])Application\\.", source), (
            f"Unqualified Unity Application: CS0234 risk in {src}")
    runtime = C_SHARP[0].read_text()
    editor = C_SHARP[2].read_text()
    assert "owner.PlotPhase == City2DPlotPhase.Built" in runtime
    assert "Sample(float seconds)" in runtime
    assert "CorrectLocalOcclusion" in editor
    assert "ReferenceSha256" in editor
    assert "DepthPilotActivity" in editor
    assert "Camera.Render()" in editor or "camera.Render()" in editor
    assert "SetEvidenceZoom" in editor and "SetEvidenceFocus" in editor
    assert "ValidateArchive();" in editor
    assert "RenderTexture" in editor
    assert "Expected 114 native A/B frames" in editor
    # Real C# parsing with tree-sitter, not a claim of compilation against Unity.
    from tree_sitter import Language, Parser
    import tree_sitter_c_sharp
    parser = Parser(Language(tree_sitter_c_sharp.language()))
    for src in C_SHARP + RELATED_DEPTH_CS:
        root = parser.parse(src.read_bytes()).root_node
        assert not root.has_error, f"C# syntax error in {src}"
    print("SOURCE_GATE_PASS: exact V6 archive; seven C# sources; "
          "Tree-sitter syntax PASS; Unity compilation NOT TESTED")


def check_frames(folder):
    from PIL import Image, ImageChops
    manifest = json.loads((folder / "evidence.json").read_text(encoding="utf-8"))
    assert manifest["engine"] == "Unity"
    assert manifest["mode"] == "Editor_RenderTexture"
    assert manifest["baseline_scene_sha256"] == SOURCE_SHA
    assert manifest["mobile_device_test"] == "NOT_PERFORMED"
    assert manifest["art_approved"] == "NO"
    assert manifest["gameplay_changed"] == "NO"
    assert manifest["fps"] == 12
    assert manifest["frames_per_trajectory"] == 18
    frames = manifest["frames"]
    assert len(frames) == EXPECTED, (len(frames), EXPECTED)
    pairs = {}
    for entry in frames:
        for name in ("variant", "trajectory", "file", "sha256", "frame", "width",
                     "height", "zoom", "focusX", "focusY", "seconds",
                     "actualCameraX", "actualCameraY", "actualOrthographicSize"):
            assert name in entry, (entry, name)
        assert entry["variant"] in ("A", "B")
        assert entry["trajectory"] in ("pan", "approach", "retreat")
        assert (entry["width"], entry["height"]) in ((390, 844), (1170, 2532))
        p = folder / entry["file"]
        assert p.is_file() and p.stat().st_size > 8000, str(p)
        assert sha256(p) == entry["sha256"], str(p)
        with Image.open(p) as im:
            assert im.size == (entry["width"], entry["height"])
        key = (entry["trajectory"], entry["frame"], entry["width"], entry["height"])
        pairs.setdefault(key, {})[entry["variant"]] = entry

    assert len(pairs) == EXPECTED // 2
    deltas = []
    for key, values in sorted(pairs.items()):
        assert set(values) == {"A", "B"}, key
        a, b = values["A"], values["B"]
        assert (a["zoom"], a["focusX"], a["focusY"], a["seconds"]) == (
            b["zoom"], b["focusX"], b["focusY"], b["seconds"]), key
        assert abs(a["actualCameraX"] - b["actualCameraX"]) <= 1e-5, key
        assert abs(a["actualCameraY"] - b["actualCameraY"]) <= 1e-5, key
        assert abs(a["actualOrthographicSize"] - b["actualOrthographicSize"]) <= 1e-5, key
        if a["width"] == 390:
            with Image.open(folder / a["file"]) as ia, Image.open(folder / b["file"]) as ib:
                bbox = ImageChops.difference(ia.convert("RGB"), ib.convert("RGB")).getbbox()
                deltas.append({"path": key[0], "frame": key[1], "changed_bbox": bbox})
    assert any(item["changed_bbox"] is not None for item in deltas), \
        "Candidate B renders exactly like A: no visual intervention detected"

    # Verify a real camera movement: a static still repeated for 18 frames cannot pass.
    for variant in ("A", "B"):
        for trajectory in ("pan", "approach", "retreat"):
            first = folder / f"{variant}_{trajectory}_00_390x844.png"
            last = folder / f"{variant}_{trajectory}_17_390x844.png"
            with Image.open(first) as before, Image.open(last) as after:
                assert ImageChops.difference(before.convert("RGB"),
                                             after.convert("RGB")).getbbox() is not None, \
                    f"Static-image sequence pretending to move: {variant} {trajectory}"
    result = {
        "source_archive_sha256": SOURCE_SHA, "real_unity_frames": len(frames),
        "exact_a_b_camera_pairs": len(pairs), "real_motion_tracks": 6,
        "subjective_depth_improvement": "UNREVIEWED",
        "browser_test": "NOT_PERFORMED", "physical_iphone": "NOT_PERFORMED",
        "diagnostic_changed_bboxes": deltas,
    }
    (folder / "validation.json").write_text(
        json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print("REAL_UNITY_NATIVE_FRAMES_SHA_CAMERA_MOTION_PASS", len(frames))
    return result


def movies(folder):
    ffmpeg = shutil.which("ffmpeg")
    if not ffmpeg:
        raise RuntimeError("ffmpeg missing: source PNG sequences remain genuine "
                           "but movies not packaged")
    for variant in ("A", "B"):
        for trajectory in ("pan", "approach", "retreat"):
            files = folder / f"{variant}_{trajectory}_%02d_390x844.png"
            out = folder / f"{variant}_{trajectory}_390x844.mp4"
            subprocess.run([
                ffmpeg, "-hide_banner", "-loglevel", "error", "-y",
                "-framerate", "12", "-start_number", "0", "-i", str(files),
                "-frames:v", "18", "-c:v", "libx264", "-crf", "19",
                "-preset", "medium", "-pix_fmt", "yuv420p",
                "-movflags", "+faststart", str(out)], check=True)
            assert out.is_file() and out.stat().st_size > 12000, out
    print("SIX_VIDEO_CLIPS_FROM_NATIVE_UNITY_FRAMES_ONLY_PASS")


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--source", action="store_true")
    ap.add_argument("--frames", type=Path)
    ap.add_argument("--movies", action="store_true")
    args = ap.parse_args()
    if args.source:
        source_gate()
    if args.frames:
        check_frames(args.frames)
        if args.movies:
            movies(args.frames)
    if not args.source and not args.frames:
        ap.error("Specify --source or --frames DIR")
