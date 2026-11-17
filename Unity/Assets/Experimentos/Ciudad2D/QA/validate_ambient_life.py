#!/usr/bin/env python3
"""Static technical contract for the illustrated Valoria living-city experiment.
Unity animation is certified ONLY by actual native Unity/PlayMode jobs, not here.
"""
from pathlib import Path
import hashlib, json
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
layout=json.loads((ROOT/'QA/city_layout.json').read_text())
amb=json.loads((ROOT/'QA/ambient_life_v1.json').read_text())
arts=json.loads((ROOT/'QA/ambient_art_manifest_v1.json').read_text())
policy=json.loads((ROOT/'ArtBible/BASTION_SHARED_TIERS_ACCEPTED_v2.json').read_text())
assert layout['schema']==4 and len(layout['buildings'])==6
assert layout['buildings'][0]['id']=='bastion'
assert layout['buildings'][0]['filename']=='Bastion_Nivel01.png'
assert layout['buildings'][0]['level5_filename']=='Bastion_Nivel05.png'
assert policy['required_unique_bastion_sprites']==2 and policy['main_migration_allowed'] is False
assert amb['schema']==1 and amb['coordinate_width']==941 and amb['coordinate_height']==1672
assert amb['effect_count']==len(amb['effects'])==10
assert amb['no_gateway_or_saves'] and amb['no_player_state_changes']
ids=[e['id'] for e in amb['effects']]
assert len(ids)==len(set(ids))
types=[e['kind'] for e in amb['effects']]
assert types.count('smoke')==6 and types.count('ember')==4
owners=[e['owner'] for e in amb['effects'] if e['kind']=='smoke']
assert sorted(owners)==sorted(['cuartel']*2+['aserradero']*2+['granero']*2)
assert all(30<=e['x']<=910 and 40<=e['y']<=1630 for e in amb['effects'])
assert all(1<=e['min_bastion']<=5 and 0<e['opacity']<=.55 and 1<=e['period']<=8 for e in amb['effects'])
assert arts['state']=='TECHNICAL_SOURCE_PREP_ART_REVIEW_PENDING' and len(arts['assets'])==2
assert arts['all_original_city_sprites_unchanged'] and arts['no_ai_generation_credits_used']
for source in arts['assets']:
    p=ROOT/'Sprites/Props'/source['name']
    assert hashlib.sha256(p.read_bytes()).hexdigest()==source['sha256']
    with Image.open(p) as im:
        assert im.size==tuple(source['dimensions']) and im.mode=='RGBA'
        assert im.getchannel('A').getextrema()==(0,source['alpha_max'])
    assert p.with_suffix('.png.meta').is_file()
scripts=ROOT/'Scripts'
for name in ['City2DAmbientMotion.cs','City2DAmbientGating.cs','City2DStateBridge.cs',
             'Editor/City2DAmbientSceneFactory.cs','Editor/City2DAmbientNativeEvidence.cs']:
    assert (scripts/name).is_file(),name
assert 'City2DAmbientSceneFactory.Populate' in (scripts/'Editor/City2DSceneBuilder.cs').read_text()
assert 'ApplyStage(snapshot.BastionLevel)' in (scripts/'City2DStateBridge.cs').read_text()
print('VALORIA_LIVING_CITY_TEN_EFFECTS_SOURCE_SHA_AND_BUILT_PLOT_CONTRACT_PASS')
print('NO NEW BASTION_II_III_IV_ART_REQUIRED | NATIVE_ANIMATION_PROOF_PENDING')
