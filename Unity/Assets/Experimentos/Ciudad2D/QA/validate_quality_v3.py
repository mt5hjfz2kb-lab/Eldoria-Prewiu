#!/usr/bin/env python3
"""Static source+source-alpha quality V3 gate. This never certifies visual artistry.
Only exact archived candidate bytes and original masters are allowed in Unity.
"""
import hashlib,json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'QA'
SPR=ROOT/'Sprites'
manifest=json.loads((QA/'AserraderoPremiumV1_Manifest.json').read_text())
art=manifest['source_photos_and_RGBA']
expected={'source':'4bbbb369b6bf873f801ddc14366cb1231eedafc3414546f1642024845338533a',
'rgba':'4fa333a21db0150179b6dd1158b559623d52765de90c82cc8b807ea30eaffa9e',
'shadow':'900eeab27836e647671820287a38fefb0050cea64bf991f53ad01bcc27a4ae7c'}
for k,x in expected.items():
    p=ROOT.parent.parent.parent/('') if False else None
    asset=Path(__file__).resolve().parents[5]/art[k]['file']
    if not asset.exists():
        # absolute from repository root: grandparent of Unity/Assets/Experimentos/Ciudad2D/QA
        asset=ROOT.parent.parent.parent.parent/art[k]['file']
    assert asset.exists(),art[k]
    assert hashlib.sha256(asset.read_bytes()).hexdigest()==x==(art[k]['sha256']),art[k]
    with Image.open(asset) as img:
        assert img.format=='PNG' and list(img.size)==art[k]['dimensions']
        assert img.mode==('RGB' if k=='source' else 'RGBA')
        if k!='source':
            a=img.getchannel('A')
            assert a.getextrema()[0]==0 and a.getextrema()[1]>90
            assert max(a.getpixel(xy) for xy in ((0,0),(img.width-1,0),(0,img.height-1),(img.width-1,img.height-1)))<=8
assert manifest['status']=='SOURCE_AND_ALPHA_VERIFIED_ART_NOT_APPROVED'
assert manifest['art_approved'] is False and manifest['original_City2D_buildings_untouched']
masters=json.loads((ROOT/'ArtBible/master_manifest_v1.json').read_text())
master=next(x for x in masters['masters'] if x['file']=='Aserradero_MASTER.png')
assert hashlib.sha256((ROOT/'ArtBible/Masters/Aserradero_MASTER.png').read_bytes()).hexdigest()==master['sha256']
assert hashlib.sha256((SPR/'Aserradero.png').read_bytes()).hexdigest()==master['sha256']
code=(ROOT/'Scripts/Editor/City2DQualityV3Gallery.cs').read_text()
assert 'City2DQualityV3Gallery' in code
for mode in ('A_32_original','B_23_quiet','C_23_premium','D_23_cuartel_sawmill'):
    assert mode in code
assert 'new[]{1,3,5}' in code
assert 'SetEvidenceFocus(Vector2.zero)' in code
assert 'Valoria_City_World_UI.unity' not in code
print('QUALITY_V3_FROZEN_SAWMILL_RGBA_SHADOW_MASTER_SHA_PASS; ART_AND_IPHONE_NOT_APPROVED')
