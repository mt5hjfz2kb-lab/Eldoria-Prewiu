#!/usr/bin/env python3
"""Preserve a curated, source-hashed screenshot pair from an authentic Unity run.
Read-only input: GitHub Actions artifact zip (must be downloaded separately).
Creates no in-game assets; only JPG comparison and provenance JSON.
"""
from pathlib import Path
import hashlib,io,json,zipfile
import numpy as np
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
QA=ROOT/'Unity/Assets/Experimentos/Ciudad2D/QA'
PILOT=ROOT/'Unity/Assets/Experimentos/Ciudad2D/ArtBible/Pilots/forja_guardia/v3'
CONFIG=Path(__file__).parent/'persist_ab_request.json'
ZIP=Path('/tmp/city2d-unity-forja-v3-isolated.zip')
def sha(b):return hashlib.sha256(b).hexdigest()
def main():
 cfg=json.loads(CONFIG.read_text())
 assert cfg['schema']==1 and cfg['run_id']==44047998748 and cfg['artifact_id']==14312049118
 assert cfg['source_sha']=='cda19dcca27f0ecc96b5a2bff389d1c4afbbf409'
 if not ZIP.is_file() or ZIP.stat().st_size<100000000:raise RuntimeError('Wrong/missing native zipped evidence')
 with zipfile.ZipFile(ZIP) as z:
  prefix=[p for p in z.namelist() if p.endswith('Ciudad2DForjaV3Evidence/evidence.json')]
  if len(prefix)!=1:raise ValueError('Native evidence folder missing/ambiguous')
  root=prefix[0].rsplit('/',1)[0]+'/'
  report=json.loads(z.read(root+'evidence.json').decode('utf-8-sig'))
  assert report['engine']=='Unity' and report['mode']=='Editor_RenderTexture'
  assert report['forja_variant']=='V3' and report['forja_isolated_one_variable'] is True
  assert report['other_two_original_sprites_locked'] is True
  assert report['forja_contact_shadow_separate'] is True and report['shadow_is_footprint_approximation'] is True
  assert report['gallery_only'] and report['art_approved'] is False
  assert len(report['captures'])==10
  content={}
  for entry in report['captures']:
   if entry['file'] in content:raise ValueError('Duplicate capture')
   data=z.read(root+entry['file'])
   assert sha(data)==entry['sha256'],entry['file']
   img=Image.open(io.BytesIO(data));img.load()
   assert img.size==(entry['width'],entry['height']),entry['file']
   content[entry['file']]=data
  required=['A_original_390x844.png','B_new_ai_390x844.png','A_original_1170x2532_forja_detail.png','B_new_ai_1170x2532_forja_detail.png']
  assert all(k in content for k in required)
 QA.mkdir(parents=True,exist_ok=True)
 PILOT.mkdir(parents=True,exist_ok=True)
 # Full native (unresampled) comparison 390 × 844, no post-edit.
 old=Image.open(io.BytesIO(content[required[0]])).convert('RGB')
 new=Image.open(io.BytesIO(content[required[1]])).convert('RGB')
 difference=np.abs(np.asarray(old,dtype=np.int16)-np.asarray(new,dtype=np.int16))
 control_mid=float(difference[370:610,200:390].mean())
 control_south=float(difference[630:810,150:390].mean())
 changed=float(difference[160:370,0:210].mean())
 assert control_mid==0 and control_south==0,"Non-forge scene unexpectedly changed"
 assert changed>3,"Forge swap was not visible"
 board=Image.new('RGB',(780,884),(20,29,39))
 board.paste(old,(0,40));board.paste(new,(390,40))
 draw=ImageDraw.Draw(board)
 draw.text((8,13),'A · ORIGINAL — Unity Editor 390x844',fill='white')
 draw.text((395,13),'B · FORJA V3 + SOMBRA — Unity Editor 390x844',fill='white')
 comparison=QA/'ForjaV3_NATIVE_AB_390x844.jpg'
 board.save(comparison,quality=95)
 # Crop the EXACT same pixel rectangle in the two independent genuine frames.
 sample=Image.new('RGB',(1050,484),(20,29,39))
 for i,key in enumerate(required[2:]):
  im=Image.open(io.BytesIO(content[key])).convert('RGB')
  crop=im.crop((70,370,1120,1240)).resize((525,435),Image.Resampling.LANCZOS)
  sample.paste(crop,(525*i,42))
  ImageDraw.Draw(sample).text((i*525+9,11),('A · ORIGINAL' if i==0 else 'B · FORJA V3, SOMBRA'),fill='white')
 close=QA/'ForjaV3_NATIVE_DETAIL_A_B.jpg';sample.save(close,quality=95)
 provenance={
  'schema':1,'repo':'mt5hjfz2kb-lab/Eldoria-Prewiu',
  'branch':'art/ciudad-2d-hitos-1-5-20261116',
  'native_run_id':cfg['run_id'],'artifact_id':cfg['artifact_id'],
  'head_sha':cfg['source_sha'],'engine':report['engine'],'mode':report['mode'],
  'capture_count':len(report['captures']),
  'original_captures':[{'file':e['file'],'sha256':e['sha256'],'size':[e['width'],e['height']]} for e in report['captures']],
  'curated_jpgs':[{'file':str(p.relative_to(ROOT)),'sha256':sha(p.read_bytes())} for p in (comparison,close)],
  'pixel_test':{'forge_region_mae':round(changed,4),'nonforge_mid_mae':control_mid,'nonforge_south_mae':control_south,'unchanged_controls':True},
  'curated_images_are_crops_of_native_Unity_screenshots':True,'curated_images_not_new_Unity_captures':True,
  'forja_contact_shadow_separate':True,'forja_v3_art_approved':False,
  'three_buildings_homogeneous':False,'physical_iPhone':'PENDING',
  'staging_only_no_gameplay_or_main_modification':True,
  'source_zip_sha256':sha(ZIP.read_bytes())
 }
 (PILOT/'UNITY_REAL_AB_EVIDENCE.json').write_text(json.dumps(provenance,ensure_ascii=False,indent=2)+'\n')
 print('NATIVE_UNITY_FORJA_V3_AB_PERSISTED_WITH_PROVENANCE',provenance['pixel_test'])
if __name__=='__main__':main()
