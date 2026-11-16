#!/usr/bin/env python3
"""QA offline reproducible para el experimento 2.5D; NO certifica Unity/PlayMode."""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Sprites"
EXPECT = {
    "Terreno.png": (941, 1672),
    "Bastion_Nivel01.png": (762, 699),
    "Bastion_Nivel05.png": (764, 854),
    "Cuartel.png": (860, 860),
    "SalonHeroes.png": (860, 860),
    "Aserradero.png": (860, 860),
    "Granero.png": (860, 860),
    "Cantera.png": (860, 860),
}

def verify():
    manifest = json.loads((ROOT / 'QA' / 'asset_manifest.json').read_text(encoding='utf-8'))
    assert set(manifest['sprites']) == set(EXPECT), 'Incompleto o archivos sobrantes'
    for fn, expected_size in EXPECT.items():
        file = SPRITES / fn
        assert file.exists(), f'Falta {fn}'
        with Image.open(file) as img:
            assert img.size == expected_size, (fn, img.size)
            assert img.mode in ('RGB', 'RGBA'), (fn, img.mode)
            if fn.startswith('Bastion_'):
                assert img.mode == 'RGBA'
                assert img.getpixel((0,0))[3] == 0, 'Contorno sin alfa'
        sha = hashlib.sha256(file.read_bytes()).hexdigest()
        assert sha == manifest['sprites'][fn]['sha256'], f'SHA diferente en {fn}'
    building = (ROOT/'Scripts'/'CityBuilding.cs').read_text(encoding='utf-8')
    assert 'actualLevel < 5 ? 1 : (actualLevel / 5) * 5' in building
    bridge = (ROOT/'Scripts'/'City2DStateBridge.cs').read_text(encoding='utf-8')
    assert 'ICommandGateway' in bridge and 'ApplySnapshot(PlayerState' in bridge
    assert '.Execute(' not in bridge and '.Save(' not in bridge
    builder = (ROOT/'Scripts'/'Editor'/'City2DSceneBuilder.cs').read_text(encoding='utf-8')
    assert 'new CityBuilding.VisualTier(1, first' in builder
    assert 'new CityBuilding.VisualTier(5, fifth' in builder
    assert 'Valoria_City_World_UI.unity' not in builder
    # Las callbacks del Editor no sobreviven al guardado de la escena.
    # Requerir reconstruccion real del registro en Awake y Start de Unity.
    assert 'private void Awake() => RebindSceneBuildings();' in bridge
    assert 'private void Start()' in bridge and 'RebindSceneBuildings();' in bridge
    assert 'building.gameObject.scene == gameObject.scene' in bridge
    camera = (ROOT/'Scripts'/'City2DCameraFit.cs').read_text(encoding='utf-8')
    assert 'touchPanThresholdPixels' in camera and 'IsOverUi(position)' in camera
    assert 'SetEvidenceViewport(int width, int height)' in camera
    assert 'size.y * 0.51f' in camera, 'Viewport retrato regresivo'
    hud = (ROOT/'Scripts'/'City2DPilotHud.cs').read_text(encoding='utf-8')
    assert 'CanvasScaler.ScaleMode.ConstantPixelSize' in hud
    assert 'new Vector2(60, 46)' in hud, 'Hitos mas pequenos que objetivo tactil'
    native = (ROOT/'Scripts'/'Editor'/'City2DNativeEvidence.cs').read_text(encoding='utf-8')
    assert native.count('CaptureOne(evidenceRoot') == 6, 'Faltan encuadres del gate Unity'
    assert 'new PlayerState' in native and 'bridge.ApplySnapshot(fixture)' in native
    assert 'domain_snapshot_read_only_pass' in native
    assert 'art_approved' in native and 'PENDING' in native
    assert (ROOT/'Scripts'/'Editor'/'City2DNativeEvidence.cs.meta').exists()
    print('OFFLINE PASS: 8 sprites, SHA, hito 4->5, registro post-serializacion, gestos, HUD y gate nativo preparado')

if __name__ == '__main__':
    verify()
