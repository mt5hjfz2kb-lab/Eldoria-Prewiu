#!/usr/bin/env python3
"""Static safety/quality contract for experimental art bake. No Unity claim."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
base=(ROOT/"Scripts/Editor")
bake=(base/"City2DArtBake.cs").read_text(encoding="utf-8")
builder=(base/"City2DSceneBuilder.cs").read_text(encoding="utf-8")
camera=(ROOT/"Scripts/City2DCameraFit.cs").read_text(encoding="utf-8")
native=(base/"City2DNativeEvidence.cs").read_text(encoding="utf-8")
assert 'private const string Generated = Folder + "Generated/"' in bake
assert '"Cuartel.png", "Aserradero.png", "Granero.png", "Cantera.png"' in bake
assert '"Bastion_Nivel05.png"' not in bake and '"SalonHeroes.png"' not in bake
assert "File.ReadAllBytes(raw)" in bake
assert "File.WriteAllBytes(target, polished.EncodeToPNG())" in bake
assert 'Generated + Path.GetFileNameWithoutExtension(name)' in bake
assert "q.a / 255f" in bake and "strength = 0.65f" in bake
assert 'AssetDatabase.ImportAsset(targetAsset, ImportAssetOptions.ForceSynchronousImport)' in bake
assert 'City2DArtBake.EnsureGenerated()' in builder
assert 'City2DArtBake.ForBuilding(spec.id,spec.filename)' in builder
assert 'SetEvidenceZoom' in camera and 'zoom = 1f; // Each evidence frame' in camera
assert '"-detalle"' in native and '1.42f' in native
assert '"Generated/Cuartel_Crisp.png"' in native
assert 'quality_crisp_bake_pass' in native
print("CITY2D_ART_BAKE_CONTRACT_PASS: isolated reversible candidates, alpha weighted contrast, Unity detail capture")
