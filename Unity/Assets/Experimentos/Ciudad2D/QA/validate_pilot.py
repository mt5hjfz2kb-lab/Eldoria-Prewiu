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
    "Terreno_Organico.png": (941, 1672),
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
    layout = json.loads((ROOT/'QA'/'city_layout.json').read_text(encoding='utf-8'))
    bastion = next(b for b in layout['buildings'] if b['id']=='bastion')
    assert bastion['filename']=='Bastion_Nivel01.png'
    assert bastion['level5_filename']=='Bastion_Nivel05.png'
    assert 'new CityBuilding.VisualTier(5,Sprite(spec.level5_filename)' in builder
    assert 'Valoria_City_World_UI.unity' not in builder
    print('OFFLINE PASS: 8 core sprites, alpha, SHA, bands 1/5 in shared layout, read-only bridge, isolated scene')

if __name__ == '__main__':
    verify()
