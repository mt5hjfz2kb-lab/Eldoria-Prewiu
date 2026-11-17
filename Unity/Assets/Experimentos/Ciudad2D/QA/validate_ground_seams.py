#!/usr/bin/env python3
"""Static guard: decorative ground seams never unlock gameplay content."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
builder=(ROOT/"Scripts/Editor/City2DSceneBuilder.cs").read_text(encoding="utf-8")
building=(ROOT/"Scripts/CityBuilding.cs").read_text(encoding="utf-8")
policy=(ROOT/"Scripts/City2DProgressionPolicy.cs").read_text(encoding="utf-8")
assert (ROOT/"Sprites/Props/MatojosFlores.png").is_file()
for name in ("cuartel","aserradero","granero","cantera"):
    assert ('spec.id == "'+name+'"') in builder,name
assert 'buildingComponent.ConfigureGroundSeam(seam)' in builder
assert 'plotPhase == City2DPlotPhase.Built && level > 0' in building
assert 'UpdateGroundSeamVisibility();' in building
assert 'case "cuartel": return 2;' in policy
assert 'case "granero": return 3;' in policy
assert 'case "cantera": return 5;' in policy
assert 'GameCommand' not in building
print("GROUND_SEAM_STATIC_PASS — only Unity can certify the rendered result")
