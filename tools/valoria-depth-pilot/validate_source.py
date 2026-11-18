#!/usr/bin/env python3
"""Strict static preflight; deliberately NOT a native Unity or visual approval test."""
from __future__ import annotations

import hashlib
import json
import pathlib
import re

REPO = pathlib.Path(__file__).resolve().parents[2]
ROOT = REPO / "Unity/Assets/Experimentos/Ciudad2D"
A = ROOT / "QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity"
EXPECTED = "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06"
C = ROOT / "Scripts/Editor/City2DDepthPilotEvidence.cs"
SKY = ROOT / "Scripts/City2DDepthSkyMotion.cs"
LIFE = ROOT / "Scripts/City2DGroundedActivity.cs"
SHADER = ROOT / "Shaders/City2DDepthSkyMask.shader"


def require(condition: bool, message: str):
    if not condition:
        raise AssertionError(message)


def read(path: pathlib.Path) -> str:
    require(path.is_file(), f"Missing source: {path}")
    return path.read_text(encoding="utf-8")


def main():
    require(A.is_file(), "Unity original Premium V6 scene absent")
    original_sha = hashlib.sha256(A.read_bytes()).hexdigest()
    require(original_sha == EXPECTED,
            f"Premium V6 IMMUTABLE scene changed: {original_sha} != {EXPECTED}")

    editor, sky, life, shader = (read(p) for p in (C, SKY, LIFE, SHADER))
    project = json.loads(read(REPO / "Unity/Packages/manifest.json"))
    require(project["dependencies"].get("com.unity.render-pipelines.universal") == "17.3.0",
            "Depth shader package version requires review")

    require('EditorSceneManager.OpenScene(SourceScene,OpenSceneMode.Single)' in editor,
            "A must render the exact archived Unity scene")
    require("EditorSceneManager.SaveScene(control,CandidateScene,true)" in editor,
            "B must Save As Copy, never overwrite A")
    require('source_branch="art/ciudad-2d-hitos-1-5-20261116"' in editor,
            "Incorrect frozen source identity")
    require("SourceSha256" in editor and EXPECTED in editor,
            "SHA lock missing")
    require("!manifest.scene_A_was_not_overwritten" in editor,
            "Post-capture SHA check missing")
    require('var expectedPoses=new Dictionary<string,CameraPose>' in editor,
            "Per-pose A/B pairing absent")
    require("Vector3.Distance(expected.position,position)>0.0001f" in editor,
            "A/B camera position equality not enforced")
    require("Mathf.Abs(expected.ortho-cam.orthographicSize)>0.0001f" in editor,
            "A/B scale equality not enforced")
    require('2*(24+9)' in editor and "MobileFps = 8" in editor and "FramesPerPhase = 8" in editor,
            "Declared route and capture count inconsistent")
    for part in ('"pan_xy"', '"zoom_in"', '"zoom_out"', "new[]{390,844}",
                 "new[]{1170,2532}", "Camera.Render()", "GetComponent<City2DCameraFit>"):
        # The camera render invocation is expressed as camera.Render(), not literal Camera.Render().
        if part == "Camera.Render()":
            part = "camera.Render();"
        require(part in editor, f"Native A/B recording element absent: {part}")
    require("File.WriteAllBytes(absolute,image)" in editor,
            "Original Unity PNG frames must be written")
    require("JsonUtility.ToJson(manifest,true)" in editor,
            "Reproducible SHA and pose manifest absent")
    require("bridge.SetDemoBastionLevel(5)" in editor,
            "Controlled Bastion V state missing")
    require('AddExistingGroundedMotion("Atrezzo_decor_04",workshop' in editor,
            "Existing cart animation expected")
    require('AddExistingGroundedMotion("Atrezzo_decor_00",bastion' in editor,
            "Existing canopy animation expected")

    require("City2DDepthSkyMotion" in sky and "Vector2.ClampMagnitude" in sky,
            "Bounded far-only parallax absent")
    require('Shader.Find("Eldoria/City2D/DepthSkyMask")' in sky,
            "Sky shader not bound")
    require('"Eldoria/City2D/DepthSkyMask"' in shader and
            'smoothstep(_MaskStart, _MaskEnd, i.uv.y)' in shader,
            "Sky shader must fade only with original terrain UV")
    require("_MaskStart\", .80f" in sky and "_MaskEnd\", .90f" in sky,
            "Only distant top of terrain should have transparent fade")

    for term in ("ICommandGateway", "LocalGateway", "GameCommand", "PlayerPrefs",
                 "File.WriteAllText(PlayerState", "PlayerState.Save"):
        require(term not in editor + sky + life,
                f"Presentation layer illegally touches gameplay: {term}")

    metas = [
        ROOT / "Scripts/City2DDepthSkyMotion.cs.meta",
        ROOT / "Scripts/City2DGroundedActivity.cs.meta",
        ROOT / "Scripts/Editor/City2DDepthPilotEvidence.cs.meta",
        ROOT / "Shaders.meta",
        ROOT / "Shaders/City2DDepthSkyMask.shader.meta",
    ]
    guids = []
    for m in metas:
        found = re.search(r"(?m)^guid: ([0-9a-f]{32})$", read(m))
        require(found is not None, f"Missing stable Unity GUID {m}")
        guids.append(found.group(1))
    require(len(set(guids)) == len(guids), "Duplicate Unity GUID")

    print("VALORIA_DEPTH_PILOT_STATIC_SOURCE_CONTRACT_PASS")
    print("IMMUTABLE_V6_SHA256=", original_sha)
    print("CAMERA_CAPTURE_ROUTE=3 movements; identical A/B; mobile24+retina9 per variant")
    print("ARTISTIC_VISUAL_REVIEW=PENDING")
    print("UNITY_EDITOR_RENDER=PENDING; WEBGL=PENDING; IPHONE_PHYSICAL=PENDING")
    print("STATIC_PASS_DOES_NOT_MEAN_DEPTH_IMPROVEMENT")


if __name__ == "__main__":
    main()
