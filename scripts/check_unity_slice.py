# world-pack compare runner release checkpoint 2 2026-09-29
# world-pack 4x comparison concurrency handoff 2026-09-29
# world-pack audit concurrency handoff 2026-09-29
#!/usr/bin/env python3
"""Offline source preflight. Its sparse CI inputs retain every assertion below.

This does not compile or launch the Unity player, or delete shared worktrees.
The separate native PlayMode gate uses the existing disposable game fixture.
"""
import json
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1] / "Unity"
version_lines = (root / "ProjectSettings/ProjectVersion.txt").read_text().splitlines()
assert version_lines and version_lines[0].strip() == "m_EditorVersion: 6000.3.23f1", version_lines
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
def playmode_process_budget(workflow: Path) -> int:
    marker = "      - name: PlayMode"
    text = workflow.read_text()
    assert text.count(marker) == 1, f"Expected one complete PlayMode step: {workflow}"
    step = text.split(marker, 1)[1].split("\n      - ", 1)[0]
    waits = re.findall(r"WaitForExit\((\d+)\)", step)
    assert len(waits) == 1, f"Expected a bounded PlayMode process wait: {workflow}"
    return int(waits[0])

# Both gates execute the complete integrated suite. The generic process must
# not kill that suite earlier than the measured shared UI certification does.
workflows = root.parent / ".github/workflows"
generic_budget = playmode_process_budget(workflows / "unity-slice.yml")
certified_budget = playmode_process_budget(workflows / "ui-progression-cert.yml")
assert generic_budget >= certified_budget, (
    f"Generic PlayMode budget {generic_budget}ms is shorter than the complete "
    f"certification budget {certified_budget}ms; retain every test and allow startup/teardown"
)
# BuildPlayer imports Resources and invokes graphics-package validation callbacks.
# A Null graphics device cannot resolve their compute kernels on this GPU runner.
text = (workflows / "unity-slice.yml").read_text()
marker = "      - name: Windows desktop build\n"
assert text.count(marker) == 1, "Expected one real Windows player build"
step = text.split(marker, 1)[1].split("\n      - ", 1)[0]
assert '"-force-d3d11"' in step and '"-nographics"' not in step, (
    "Windows BuildPlayer must initialize the native-tested Direct3D backend, not Null graphics"
)
# The legacy packaging helper regenerates its three-scene list. Keep its
# side effects after canonical-scene tests; otherwise33 real cases cannot boot.
assert text.index("      - name: EditMode\n") < text.index(marker)
assert text.index("      - name: PlayMode\n") < text.index(marker), (
    "Legacy SceneSetup packaging must not replace the scene list before integrated PlayMode"
)
print("PASS source preflight: editor patch, packages, versioned scenes, modules, tests, full-suite budget and graphical Windows packaging")

