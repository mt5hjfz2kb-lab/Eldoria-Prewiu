#!/usr/bin/env python3
"""Static QA for real Unity Editor PlayMode and reversible grounding A/B.
A static check is NOT PlayMode success and must never claim it.
"""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
asm = json.loads((root / "QA/PlayModeProof/Ciudad2D.PlayModeProof.asmdef").read_text(encoding="utf-8"))
test = (root / "QA/PlayModeProof/City2DRealPlayModeTests.cs").read_text(encoding="utf-8")
bake = (root / "Scripts/Editor/City2DGroundingBake.cs").read_text(encoding="utf-8")
native = (root / "Scripts/Editor/City2DNativeEvidence.cs").read_text(encoding="utf-8")
assert asm["includePlatforms"] == ["Editor"]
assert "TestAssemblies" in asm["optionalUnityReferences"]
assert "nunit.framework.dll" in asm["precompiledReferences"]
assert "yield return new EnterPlayMode()" in test
assert "EditorSceneManager.LoadSceneAsyncInPlayMode" in test
assert "Application.isPlaying" in test
assert "ExecuteEvents.pointerClickHandler" in test
assert "Physics2D.OverlapPoint" in test
assert "City2DPlayModeEvidence" in test
assert "yield return new ExitPlayMode()" in test
assert "Grounded.png" in bake
assert "GetPixels32()" in bake and "distance[i]" in bake
assert "City2DGroundingBake.EnsureGenerated()" in native
assert '"  \\"grounding_art_comparison_only\\":true' in native
assert 'groundedComparison' in native
assert "candidateCuartel ? \\"-cuartel-candidato\\"" in native
assert "ArtBake.ForBuilding" in (root / "Scripts/Editor/City2DSceneBuilder.cs").read_text(encoding="utf-8")
assert "GroundingBake.ForBuilding" not in (root / "Scripts/Editor/City2DSceneBuilder.cs").read_text(encoding="utf-8")
print("CITY2D_PLAYMODE_STATIC_PREPARED — Native Unity Test Runner result required")
