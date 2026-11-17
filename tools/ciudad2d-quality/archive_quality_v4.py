#!/usr/bin/env python3
"""Archive source-pinned Unity 5-way I/III/V art comparison from 45 actual PNGs.
Review montages are DERIVATIVES, not a claim that an offline board is a Unity frame.
No score, gameplay modification, physical iPhone or art approval is implied.
"""
from pathlib import Path
from PIL import Image,ImageDraw
from io import BytesIO
import hashlib,json,zipfile
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/Assets/Experimentos/Ciudad2D/QA'
CFG=Path(__file__).parent/'quality_v4_archive_request.json'
ZIP=Path('/tmp/ciudad2d-v4-unity-native.zip')
MODES=('A_32_original','B_23_quiet','C_23_premium',
       'D_23_cuartel_sawmill','E_23_cuartel_sawmill_granary')
SHA_SAW='d692c25b525a50b5e9f227df6d4b721a5269173a7acffcf7b044dd7a2da6c518'
SHA_GRAN='699c8c15f5147cc634ea39d1197e13058831786f6da60cab81d711f118118229'
def sha(data):return hashlib.sha256(data).hexdigest()
def main():
 cfg=json.loads(CFG.read_text())
 assert cfg['schema']==1 and cfg['run_id']>0 and cfg['artifact_id']>0 and len(cfg['source_sha'])==40
 assert ZIP.exists() and ZIP.stat().st_size>15_000_000
 with zipfile.ZipFile(ZIP) as z:
  recs=[n for n in z.namelist() if n.endswith('Ciudad2DQualityV4Evidence/evidence.json')]
  if len(recs)!=1:raise ValueError('No unique original Unity V4 manifest')
  root=recs[0].rsplit('/',1)[0]+'/'
  report=json.loads(z.read(recs[0]).decode('utf-8-sig'))
  assert report['engine']=='Unity' and report['mode']=='Editor_RenderTexture'
  assert tuple(report['modes'])==MODES and len(report['captures'])==45
  assert report['gameplay_modified'] is False and report['main_modified'] is False
  assert report['art_approved'] is False and report['physical_iphone_test']=='PENDING'
  assert report['sawmill_candidate_sha256']==SHA_SAW
  assert report['granary_candidate_sha256']==SHA_GRAN
  assert report['bastion_I_1_to_4_V_5_unchanged'] is True
  images={}
  for v in report['captures']:
   data=z.read(root+v['file'])
   if sha(data)!=v['sha256']:raise ValueError('Not original Unity frame '+v['file'])
   im=Image.open(BytesIO(data));im.load()
   if im.format!='PNG' or im.size!=(v['width'],v['height']):raise ValueError('Dimension mismatch')
   images[v['file']]=im.convert('RGB')
 assert len(images)==45
 OUT.mkdir(parents=True,exist_ok=True)
 derived=[]
 measures={}
 def persist(image,filename):
  dest=OUT/filename
  if dest.exists():raise ValueError('No overwrite of earlier review: '+filename)
  image.save(dest,quality=95)
  derived.append({'file':str(dest.relative_to(ROOT)),'sha256':sha(dest.read_bytes())})
 for stage in (1,3,5):
  board=Image.new('RGB',(1950,892),(22,31,42))
  draw=ImageDraw.Draw(board)
  arr=[]
  for i,mode in enumerate(MODES):
   name=f'{mode}_bastion{stage}_390x844_standard.png'
   im=images[name];board.paste(im,(390*i,48))
   arr.append(np.asarray(im,np.int16))
   draw.text((390*i+8,16),f'B{stage}: '+['A original 32','B 23 props','C +Cuartel','D +Aserradero','E +Granero'][i],fill=(235,216,179))
  persist(board,f'ArtQualityV4_Bastion{stage}_A_B_C_D_E.jpg')
  measures[str(stage)]={
   'MAE_C_to_D':round(float(np.abs(arr[2]-arr[3]).mean()),5),
   'MAE_D_to_E':round(float(np.abs(arr[3]-arr[4]).mean()),5)}
  if stage==5 and min(measures[str(stage)].values())<.04:raise ValueError('New Sawmill/Granary not visible on V')
  if stage==3 and measures[str(stage)]['MAE_D_to_E']>.01:
   raise ValueError('Granary appears on stage III plot before it is built')
 for focus in ('cuartel','aserradero','granero'):
  sheet=Image.new('RGB',(1950,892),(22,31,42))
  draw=ImageDraw.Draw(sheet)
  for i,mode in enumerate(MODES):
   name=f'{mode}_bastion5_1170x2532_{focus}_close.png'
   im=images[name].resize((390,844),Image.Resampling.LANCZOS)
   sheet.paste(im,(390*i,48))
   draw.text((390*i+8,16),MODES[i],fill=(233,215,171))
  persist(sheet,f'ArtQualityV4_{focus.title()}Detail_A_B_C_D_E.jpg')
 evidence={'schema':1,'engine':'Unity','mode':'Editor_RenderTexture',
  'source_run_id':cfg['run_id'],'source_artifact_id':cfg['artifact_id'],'source_git_sha':cfg['source_sha'],
  'original_source_zip_sha256':sha(ZIP.read_bytes()),'exact_unity_frames_count':45,
  'source_unity_png_sha':report['captures'],'derived_comparisons_not_native_captures':derived,
  'source_sawmill_sha256':SHA_SAW,'source_granary_sha256':SHA_GRAN,
  'original_32_prop_control_preserved':True,'no_main_or_gameplay_modification':True,
  'physical_iphone_test':'NOT_TESTED','art_approved':False,
  'quarry_original_still_unapproved':True,
  'mobile_pixel_differences_are_not_quality_scores':measures}
 (OUT/'ArtQualityV4_NATIVE_EVIDENCE.json').write_text(json.dumps(evidence,indent=2,ensure_ascii=False)+'\n')
 print('NATIVE_UNITY_45_FRAMES_SOURCE_SHA_VERIFIED_NOT_ART_APPROVED',measures)
if __name__=='__main__':main()
