#!/usr/bin/env python3
"""Non-Unity structural contract, layout and raw-image integrity checks."""
import hashlib,json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'QA';S=ROOT/'Sprites'
manifest=json.loads((QA/'city_layout.json').read_text(encoding='utf-8'))
assert manifest['schema']==3
assert [manifest['coordinate_width'],manifest['coordinate_height']]==[941,1672]
bs=manifest['buildings'];decor=manifest['decor'];fx=manifest['glows']
assert len(bs)==6 and len(decor)==16 and len(fx)==7
assert set(b['id'] for b in bs)=={'bastion','cuartel','salon','aserradero','granero','cantera'}
assert len({b['id'] for b in bs})==len(bs) and len({x['id'] for x in decor})==len(decor)
assert sum(bool(b.get('level5_filename')) for b in bs)==1
assert next(b for b in bs if b['id']=='bastion')['level5_filename']=='Bastion_Nivel05.png'
for data in bs+decor:
    assert 0<=data['x']<=941 and 0<=data['y']<=1672 and .1 <= data['scale']<=.6, data
    if data in bs: name=data['filename']
    else: name=data['filename']
    assert (S/name).is_file(),name
new=json.loads((QA/'organic_new_assets.json').read_text(encoding='utf-8'))
for name,info in new.items():
    path=S/'Props'/(name+'.png')
    with Image.open(path) as im:
        assert im.size == tuple(info['size']) and im.mode=='RGBA'
        assert im.getchannel('A').getextrema()[0]==0 and im.getchannel('A').getextrema()[1]>=240
        # Edges must not accidentally turn into opaque rectangles when alpha is composited.
        assert im.getpixel((0,0))[3]<=5
    assert hashlib.sha256(path.read_bytes()).hexdigest()==info['sha256'],name
for g in fx:
    assert 10<=g['radius']<=60 and 0<g['opacity']<=.5
    assert 0<=g['x']<=941 and 0<=g['y']<=1672
builder=(ROOT/'Scripts/Editor/City2DSceneBuilder.cs').read_text(encoding='utf-8')
assert 'QA/city_layout.json' in builder and 'JsonUtility.FromJson<Layout>' in builder
assert 'SpriteRenderer' in builder and 'OrderFor(spec.y,terrain)' in builder
assert 'EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive)' in builder
assert 'Valoria_City_World_UI.unity' not in builder
assert 'Assets/Eldoria' not in builder
assert 'EditorSceneManager.SaveScene(isolated, ScenePath)' in builder
state=(ROOT/'Scripts/City2DStateBridge.cs').read_text(encoding='utf-8')
assert 'ApplySnapshot(PlayerState snapshot)' in state and '.Execute(' not in state and '.Save(' not in state
web=(QA/'live-preview/index.html').read_text(encoding='utf-8')
assert "fetch('../city_layout.json'" in web and 'layout.decor.length!==16' in web
assert 'b.file=artLevel===5?b.level5File:b.baseFile' in web
print('CITY2D_LAYOUT3_OFFLINE_PASS: 6 buildings, 16 decor placements, 7 glows, 6 new premium assets, isolated scene, game state read-only')
