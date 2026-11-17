#!/usr/bin/env python3
"""Static source/memory truth gate, not an aesthetic or physical-device score."""
import hashlib,json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
SP=ROOT/'Sprites'; QA=ROOT/'QA'
M=json.loads((QA/'SawmillGranaryPremium_Manifest.json').read_text())
assert M['schema']==1 and M['original_buildings_unmodified'] is True
assert M['status']=='TWO_CANDIDATES_ARCHIVED_ART_NOT_APPROVED'
assert set(M['entries'])=={'aserradero','granero'}
orig=json.loads((ROOT/'ArtBible/master_manifest_v1.json').read_text())
master={x['source'].split('/')[-1]:x['sha256'] for x in orig['masters']}
for name in ('Aserradero.png','Granero.png','Cantera.png','Cuartel.png','Bastion_Nivel05.png'):
 data=(SP/name).read_bytes()
 assert hashlib.sha256(data).hexdigest()==master[name],name+' master changed'
budget=[]
for key,entry in M['entries'].items():
 for kind,path,digest in [
   ('original',entry['source_filename'],entry['source_sha256']),
   ('candidate',entry['candidate'],entry['candidate_sha256']),
   ('shadow',entry['separate_shadow'],entry['shadow_sha256'])]:
  p=SP/'Candidates'/path
  assert p.is_file() and hashlib.sha256(p.read_bytes()).hexdigest()==digest,p
  with Image.open(p) as im:
   assert im.format=='PNG'
   if kind=='original':assert im.mode=='RGB' and im.size==(1536,1024)
   else:
    assert im.mode=='RGBA'
    a=im.getchannel('A')
    assert a.getextrema()[0]==0 and a.getextrema()[1]>=75
    assert all(a.getpixel(point)==0 for point in [(0,0),(im.width-1,0),(0,im.height-1),(im.width-1,im.height-1)])
   if kind!='original':budget.append({'id':key,'kind':kind,'size':list(im.size),'rgba32_bytes':im.width*im.height*4})
  assert p.with_suffix('.png.meta').is_file()
 assert entry['shadow_scale_correction']==2
 assert entry['native_Unity_AB']=='PENDING' and entry['not_art_approved'] is True
c=(QA/'cuartel_premium_v2_manifest.json')
cuartel=json.loads(c.read_text())
assert cuartel['art_approved'] is False
for name in ('Cuartel_Premium_V2.png','Cuartel_Premium_ShadowV2.png'):
 p=SP/'Candidates'/name
 assert p.is_file()
 with Image.open(p) as im:budget.append({'id':'cuartel','kind':name,'size':list(im.size),'rgba32_bytes':im.width*im.height*4})
script=(ROOT/'Scripts/Editor/City2DQualityV4Gallery.cs').read_text()
for token in ['City2DQualityV4Gallery','A_32_original','B_23_quiet','C_23_premium',
 'D_23_cuartel_sawmill','E_23_cuartel_sawmill_granary','GranaryCandidate','SawmillCandidate',
 'granaryShade.enabled=','granary.transform.localScale*2f','SetEvidenceFocus',
 'gameplay_modified','physical_iphone_test']:
 assert token in script, token
assert 'Valoria_City_World_UI.unity' not in script
combined=sum(x['rgba32_bytes'] for x in budget)
report={'schema':1,'platform':'hypothetical uncompressed GPU RGBA32 for all QA candidates simultaneously',
 'estimated_RGBA32_MiB':round(combined/2**20,2),'source_calculation':'width*height*4 bytes; no mipmaps',
 'not_profiled_on_iPhone':True,'not_a_runtime_memory_measurement':True,'textures':budget}
(QA/'QualityV4_TheoreticalMemory.json').write_text(json.dumps(report,indent=2)+'\n')
print('QUALITY_V4_SOURCE_SHA_AND_45_FRAME_GALLERY_STATIC_PASS | UNCOMPRESSED_QA_TEXTURE_MB',report['estimated_RGBA32_MiB'])
print('PLAYMODE_INTERACTION_FOR_NEW_ART_UNTESTED | ART_APPROVAL_PENDING | IPHONE_PENDING')
