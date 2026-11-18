#!/usr/bin/env python3
"""Static provenance check only. Does NOT validate Unity rendering, art, or PlayMode."""
from __future__ import annotations
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CITY = ROOT.parent / "Ciudad2D"
A = CITY / "QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity"
CONTROL = ROOT / "Scenes/Valoria_Spatial_A_Control.unity"
B = ROOT / "Scenes/Valoria_Spatial_B_Pilot.unity"
CODE = ROOT / "SpatialPilotBootstrap.cs"
WORKER = ROOT / "SpatialPilotWorker.cs"
ART = ROOT / "SpatialPilotActorArt.cs"
WALL = ROOT / "SpatialTerrainForeground.cs"
LAYOUT = ROOT / "Data/foreground_contours_v1.json"
SHADER = ROOT / "Shaders/SpatialForegroundCrop.shader"
EDITOR = ROOT / "Editor/SpatialPilotNativeEvidence.cs"
SCRIPT_META = ROOT / "SpatialPilotBootstrap.cs.meta"
MARKER = "--- !u!1 &700000010\n"
BEFORE_ROOT = "--- !u!1660057539 &9223372036854775807\nSceneRoots:"
ROOT_NEW = "SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n  - {fileID: 700000011}\n"
ROOT_OLD = "SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n"

def verify() -> dict:
    assert all(p.is_file() for p in (A, CONTROL, B, CODE, WORKER, ART, WALL, LAYOUT, SHADER, EDITOR, SCRIPT_META)), "Required file missing"
    base = A.read_bytes()
    assert CONTROL.read_bytes() == base, 'Isolated A control must match archival original exactly'
    candidate = B.read_bytes()
    assert candidate.count(MARKER.encode()) == 1, "B-only component absent or duplicated"
    assert candidate.count(BEFORE_ROOT.encode()) == 1, "B lost canonical SceneRoots"
    prefix, rest = candidate.split(MARKER.encode(), 1)
    _, suffix = rest.split(BEFORE_ROOT.encode(), 1)
    without_component = prefix + BEFORE_ROOT.encode() + suffix
    assert without_component.count(ROOT_NEW.encode()) == 1, "B root only once"
    restored = without_component.replace(ROOT_NEW.encode(), ROOT_OLD.encode(), 1)
    assert restored == base, "B differs from A beyond the isolated presentation bootstrap"
    source = "\\n".join(p.read_text(encoding="utf-8") for p in (CODE, WORKER, ART, WALL))
    native = EDITOR.read_text(encoding="utf-8")
    meta = SCRIPT_META.read_text(encoding="utf-8")
    guid = "4f88a89cd5134ddc8c10e4ff743dbef5"
    assert f"guid: {guid}" in meta and f"guid: {guid}" in B.read_text(encoding="utf-8")
    assert all(s in source for s in [
        "City2DPlotPhase.Built", "CityBuilding", "SpatialPilotWorker",
        'CreateWorker("aserradero"', 'CreateWorker("cuartel"',
        'CreateWorker("granero"', "FootOrder", "SampleAt"])
    for forbidden in ("GameCommand", "SaveGame", "LocalGateway", "PlayerState", "ICommandGateway"):
        assert forbidden not in source, f"Unexpected game-domain access: {forbidden}"
    for source_text in (CODE.read_text(encoding="utf-8"), EDITOR.read_text(encoding="utf-8")):
        assert "UnityEngine.Application." in source_text
        import re
        assert not re.search(r"(?<!UnityEngine\\.)\\bApplication\\.", source_text), "Unqualified Application may bind Eldoria.Application namespace"
    # Five convex fragments share the source terrain texture; no masking by green patches.
    data = json.loads(LAYOUT.read_text(encoding="utf-8"))
    assert data["schema"] == 1 and data["coordinate_width"] == 941 and data["coordinate_height"] == 1672
    assert len(data["regions"]) == 5
    wall_source = WALL.read_text(encoding="utf-8")
    assert "sharedCutoutMaterial.mainTexture = texture" in wall_source
    assert '"UniversalPipeline"' in SHADER.read_text(encoding="utf-8")
    for region in data["regions"]:
        points = region["contour"]
        assert 3 <= len(points) <= 14
        assert 0 <= region["foot_y"] <= 1672
        for point in points:
            assert 0 <= point["x"] <= 941 and 0 <= point["y"] <= 1672
        signs = set()
        for j in range(len(points)):
            a,b,c = points[j],points[(j+1)%len(points)],points[(j+2)%len(points)]
            cross = (b["x"]-a["x"])*(c["y"]-b["y"])-(b["y"]-a["y"])*(c["x"]-b["x"])
            if abs(cross)>1: signs.add(cross>0)
        assert len(signs)==1, f"Non-convex foreground {region['id']}"
    assert "ForegroundRegionCount" in source and "drawGroundForeground = true" in source
    assert "Camera.main" in native and "RenderTexture" in native
    assert "EditorSceneManager.OpenScene" in native and "Sha(File.ReadAllBytes(sourceOnDisk))" in native
    assert "SetDemoBastionLevel" in native and "File.WriteAllBytes" in native
    return {
        "status": "STATIC_ONLY_PASS",
        "unity_run": "NOT_EXECUTED",
        "art_approved": False,
        "source_A_exactly_recoverable_from_B": True,
        "isolated_A_byte_equal_to_original_archive": True,
        "V6_archived_bytes": len(base),
        "B_scene_bytes": len(candidate),
        "B_additional_scene_bytes": len(candidate) - len(base),
        "source_modified": False
    }

if __name__ == "__main__":
    print(json.dumps(verify(), indent=2))
