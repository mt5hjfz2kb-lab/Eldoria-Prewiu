#!/usr/bin/env python3
"""Offline source preflight. This does not compile or launch the Unity player."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1] / "Unity"
version = (root / "ProjectSettings/ProjectVersion.txt").read_text().strip()
assert version == "m_EditorVersion: 6000.3.23f1", version
manifest = json.loads((root / "Packages/manifest.json").read_text())
assert "com.unity.render-pipelines.universal" in manifest["dependencies"]
assert "com.unity.inputsystem" in manifest["dependencies"]
settings = (root / "ProjectSettings/EditorBuildSettings.asset").read_text()
for scene in ("Bootstrap", "Valoria", "Frontier"):
    path = root / f"Assets/Eldoria/Scenes/{scene}.unity"
    assert path.is_file() and (Path(str(path) + ".meta")).is_file(), scene
    assert f"path: Assets/Eldoria/Scenes/{scene}.unity" in settings, scene
    assert f"Eldoria {scene} scene marker" in path.read_text(), scene
for module in ("Domain", "Application", "Infrastructure", "Presentation"):
    assert list((root / f"Assets/Eldoria/Scripts/{module}").glob("*.asmdef")), module
for layer in ("EditMode", "PlayMode"):
    assert list((root / f"Assets/Eldoria/Tests/{layer}").glob("*Tests.cs")), layer
print("PASS source preflight: editor patch, packages, versioned scenes, modules and tests")
