#!/usr/bin/env python3
"""Technical truth gate for reversible density + premium Cuartel QA.
This test does not certify premium art or actual iPhone hardware.
"""
from pathlib import Path
from PIL import Image
import hashlib,json
ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'QA'
SP=ROOT/'Sprites'
AB=ROOT/'ArtBible'
layout=json.loads((QA/'city_layout.json').read_text())
control=json.loads((AB/'city_layout_32decor_control_A_20261117.json').read_text())
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
assert layout['schema']==4 and control['schema']==4
assert len(layout['decor'])==23 and len(control['decor'])==32
assert layout['buildings']==control['buildings']
assert layout['glows']==control['glows']
assert layout['terrain_file']==control['terrain_file']
assert layout['terrain_previous_file']==control['terrain_previous_file']
expected_removed={'decor_01','decor_02','decor_20','decor_21','decor_22',
                  'decor_23','decor_24','decor_27','decor_30'}
old={x['id']:x for x in control['decor']}
new={x['id']:x for x in layout['decor']}
assert set(old)-set(new)==expected_removed
assert all(new[k]==old[k] for k in new),'Existing decor source/position changed'
manifest=json.loads((QA/'cuartel_premium_v2_manifest.json').read_text())
shadow=json.loads((QA/'CuartelPremiumV2_ShadowManifest.json').read_text())
assets={
 SP/'Candidates/Cuartel_Premium_SourceV2.png':'644c3ffdf8429a710606fbff0d09b72671a5c8e5e18b152e4d761c821f9157ae',
 SP/'Candidates/Cuartel_Premium_V2.png':'0dcb8edb8237621f13893893edce20fec51deb1248df8bcbc3a89ea3990b4062',
 SP/'Candidates/Cuartel_Premium_ShadowV2.png':'9d128fe0a7d0c229c1c3b14b098eaef17e6df79ac557ed283a3893bb4a22c5a6',
}
for path,expected in assets.items():
    assert path.is_file() and digest(path)==expected,path
    with Image.open(path) as im:
        if path.name.endswith('SourceV2.png'):
            assert im.size==(1536,1024) and im.mode=='RGB'
        else:
            assert im.size==(1479,1479) and im.mode=='RGBA'
            a=im.getchannel('A')
            assert a.getextrema()[0]==0 and a.getextrema()[1]>90
            assert max(a.getpixel(q) for q in ((0,0),(1478,0),(0,1478),(1478,1478)))<3
    if not path.name.endswith('SourceV2.png'):assert path.with_suffix('.png.meta').is_file()
assert manifest['candidate_sha256']==assets[SP/'Candidates/Cuartel_Premium_V2.png']
assert manifest['art_approved'] is False and manifest['legacy_cuartel_unchanged']
assert shadow['shadow_sha256']==assets[SP/'Candidates/Cuartel_Premium_ShadowV2.png']
assert shadow['separate_layer'] is True and shadow['art_approved'] is False
gallery=(ROOT/'Scripts/Editor/City2DQualityV2Gallery.cs').read_text()
for name in ('A_32_original','B_23_quiet','C_23_premium',
             'City2DSceneBuilder.Rebuild();','RenderTexture','CheckSha(Candidate,ExactCandidateSha)',
             'gameplay_modified','physical_iphone_test'):
    assert name in gallery,name
assert 'Valoria_City_World_UI.unity' not in gallery
print('CITY2D_QUALITY_V2_SOURCE_HASH_AND_DENSITY_CONTRACT_PASS: 32=>23 props; exact independent Cuartel+shadow; original controls immutable')
print('UNITY_NATIVE, ART QUALITY and physical iPhone are separate gates, NOT proven here')
