#!/usr/bin/env python3
"""Hard source/alpha/size/Unity-script gate for 4 premium City2D candidates.
This DOES NOT approve art, live device FPS, touch response, or GPU memory.
"""
from pathlib import Path
from PIL import Image
import hashlib,json
ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'QA'
CAND=ROOT/'Sprites/Candidates'
MASTERS=ROOT/'ArtBible/Masters'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
base=json.loads((ROOT/'ArtBible/master_manifest_v1.json').read_text())
for item in base['masters']:
    file=MASTERS/item['file']
    assert file.exists() and sha(file)==item['sha256'],'Original master changed: '+item['file']
    sprite=ROOT/item['source']
    assert sprite.exists() and sha(sprite)==item['sha256'],'Canonical art changed: '+item['source']
cuartel=json.loads((QA/'cuartel_premium_v2_manifest.json').read_text())
cuartelshadow=json.loads((QA/'CuartelPremiumV2_ShadowManifest.json').read_text())
two=json.loads((QA/'SawmillGranaryPremium_Manifest.json').read_text())
quarry=json.loads((QA/'Cantera_Premium_V1_Manifest.json').read_text())
assert quarry['art_approved'] is False and quarry['city_original_unchanged']
assert two['status']=='TWO_CANDIDATES_ARCHIVED_ART_NOT_APPROVED'
expected={
'cuartel':('Cuartel_Premium_V2.png',cuartel['candidate_sha256'],
           'Cuartel_Premium_ShadowV2.png',cuartelshadow['shadow_sha256'],1),
'aserradero':(two['entries']['aserradero']['candidate'],
           two['entries']['aserradero']['candidate_sha256'],
           two['entries']['aserradero']['separate_shadow'],
           two['entries']['aserradero']['shadow_sha256'],2),
'granero':(two['entries']['granero']['candidate'],
           two['entries']['granero']['candidate_sha256'],
           two['entries']['granero']['separate_shadow'],
           two['entries']['granero']['shadow_sha256'],2),
'cantera':(quarry['candidate'],quarry['candidate_sha256'],
           quarry['separate_contact_shadow'],quarry['shadow_sha256'],2)
}
records=[]
for building,(file,imagehash,shadow,shadowhash,ratio) in expected.items():
    artwork=CAND/file
    contact=CAND/shadow
    for key,path,expectedhash in [('artwork',artwork,imagehash),('contact_shadow',contact,shadowhash)]:
        assert path.is_file() and sha(path)==expectedhash,(building,key,'hash mismatch')
        assert path.with_suffix('.png.meta').is_file()
    with Image.open(artwork) as im:
        assert im.format=='PNG' and im.mode=='RGBA'
        w,h=im.size
        alpha=im.getchannel('A')
        assert min(w,h)>=1200 and max(w,h)<=2048,(building,'quality pixels')
        assert alpha.getextrema()==(0,255),(building,'source must contain real alpha')
        assert all(alpha.getpixel(q)==0 for q in [(0,0),(w-1,0),(0,h-1),(w-1,h-1)]),(building,'edge rectangle')
    with Image.open(contact) as sh:
        assert sh.format=='PNG' and sh.mode=='RGBA'
        assert round(w/sh.width)==ratio and round(h/sh.height)==ratio
        assert sh.getchannel('A').getextrema()[0]==0
        records.extend([{'building':building,'kind':'sprite','dimensions':[w,h],'rgba32_bytes':w*h*4},
                        {'building':building,'kind':'contact_shadow','dimensions':list(sh.size),'rgba32_bytes':sh.width*sh.height*4}])
script=(ROOT/'Scripts/Editor/City2DQualityV5Gallery.cs').read_text()
assert 'class City2DQualityV5Gallery' in script
assert 'F_23_all_four_premium' in script
assert 'Cantera_Premium_V1.png' in script
assert 'Cantera_Premium_V1_Shadow.png' in script
assert 'new[]{1,3,5}' in script
assert 'Vector2.zero' in script
assert 'Valoria_City_World_UI.unity' not in script
preview=(ROOT/'Scripts/Editor/City2DPremiumPreviewV2.cs').read_text()
assert 'class City2DPremiumPreviewV2' in preview
assert 'Swap(buildings,"cantera"' in preview
assert 'PremiumV2.unity' in preview
assert 'IsLiveBound' in preview
test=(QA/'PlayModeProof/City2DPremiumPlayModeTestsV2.cs').read_text()
assert 'PremiumV2Scene_GenuinePlayMode_StagesSpritesShadowsPointer' in test
assert 'Assert.AreEqual(4,ActiveShadows()' in test
assert 'Cantera_Premium_V1' in test
qa=(QA/'run_premium_preview_v2_native.ps1').read_text()
testline=qa[qa.find('# Unity 6000.3:'):]
assert "'-runTests'" in testline
assert "'-quit'" not in testline.split("'-runTests'")[0].split("$args=@(")[-1]
total=sum(item['rgba32_bytes'] for item in records)
report={'schema':1,'scope':'hypothetical simultaneously loaded four premium sprites and shadows',
        'bytes_if_RGBA32_no_mips':total,'estimated_MiB':round(total/1048576,2),
        'status':'STATIC_FORMAT_AND_PROVENANCE_PASS_NOT_RUNTIME_MEASUREMENT',
        'not_measured_on_any_iPhone':True,'not_art_approved':True,
        'individual_textures':records,
        'future_optimization':'Evaluate ASTC 4x4 on architecture and 6x6 or 8x8 on shadow only in a separate iOS/WebGL quality gate, never modify masters.'}
(QA/'QualityV5_TheoreticalTextureBudget.json').write_text(json.dumps(report,indent=2)+'\n')
print('CITY2D_4_PREMIUM_SOURCE_RGBA_SHADOW_SHA_PASS; THEORETICAL_TEXTURE_MiB',report['estimated_MiB'])
print('NO_ART_RATING, NO_DEVICE_PROFILE; REAL_PLAYMODE_PENDING')
