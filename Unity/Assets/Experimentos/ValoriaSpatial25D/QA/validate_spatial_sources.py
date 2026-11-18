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
EDITOR = ROOT / "Editor/SpatialPilotNativeEvidence.cs"
SCRIPT_META = ROOT / "SpatialPilotBootstrap.cs.meta"
MARKER = "--- !u!1 &700000010\n"
BEFORE_ROOT = "--- !u!1660057539 &9223372036854775807\nSceneRoots:"
ROOT_NEW = "SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n  - {fileID: 700000011}\n"
ROOT_OLD = "SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n"

def verify() -> dict:
    assert all(p.is_file() for p in (A, CONTROL, B, CODE, EDITOR, SCRIPT_META)), "Required file missing"
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
    source = CODE.read_text(encoding="utf-8")
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
