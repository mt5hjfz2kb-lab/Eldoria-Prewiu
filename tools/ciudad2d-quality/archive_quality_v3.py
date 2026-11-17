#!/usr/bin/env python3
"""Permanently archive source-exact Unity V3 A/B/C/D screenshots, no fabricated
renders or artistic ratings. Comparison montages are clearly labeled derivatives.
"""
from pathlib import Path
from PIL import Image,ImageDraw
from io import BytesIO
import hashlib,json,zipfile
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/Assets/Experimentos/Ciudad2D/QA'
REQUEST=Path(__file__).parent/'quality_v3_archive_request.json'
ZIP=Path('/tmp/ciudad2d-premium-v3-exact-unity.zip')
MODES=('A_32_original','B_23_quiet','C_23_premium','D_23_cuartel_sawmill')
def sha(b):return hashlib.sha256(b).hexdigest()
def main():
 req=json.loads(REQUEST.read_text())
 assert req['schema']==1 and req['run_id']>0 and req['artifact_id']>0
 assert len(req['source_sha'])==40 and ZIP.is_file() and ZIP.stat().st_size>12_000_000
 with zipfile.ZipFile(ZIP) as z:
  paths=[n for n in z.namelist() if n.endswith('Ciudad2DQualityV3Evidence/evidence.json')]
  if len(paths)!=1:raise ValueError('No unique genuine V3 Unity evidence file')
  base=paths[0].rsplit('/',1)[0]+'/'
  report=json.loads(z.read(paths[0]).decode('utf-8-sig'))
  assert report['engine']=='Unity' and report['mode']=='Editor_RenderTexture'
  assert tuple(report['modes'])==MODES and len(report['captures'])==32
  assert report['main_modified'] is False and report['gameplay_modified'] is False
  assert report['art_approved'] is False and report['physical_iphone_test']=='PENDING'
  assert report['sawmill_candidate_sha256']=='4fa333a21db0150179b6dd1158b559623d52765de90c82cc8b807ea30eaffa9e'
  assert report['sawmill_shadow_sha256']=='900eeab27836e647671820287a38fefb0050cea64bf991f53ad01bcc27a4ae7c'
  images={}
  for x in report['captures']:
   data=z.read(base+x['file'])
   if sha(data)!=x['sha256']:raise ValueError('Unity frame differs from source evidence '+x['file'])
   im=Image.open(BytesIO(data));im.load()
   assert im.size==(x['width'],x['height']) and im.format=='PNG'
   images[x['file']]=im.convert('RGB')
 assert len(images)==32
 OUT.mkdir(parents=True,exist_ok=True)
 deriv=[]
 measures={}
 for stage in (1,3,5):
  pic=Image.new('RGB',(1560,893),(25,34,43))
  draw=ImageDraw.Draw(pic)
  originals=[]
  for i,mode in enumerate(MODES):
   name=f'{mode}_bastion{stage}_390x844_standard.png'
   im=images[name];pic.paste(im,(390*i,45)); originals.append(np.asarray(im,np.int16))
   draw.text((390*i+9,15),f'B{stage}: '+['A original 32','B fondo 23','C +Cuartel','D +Aserradero'][i],fill=(246,219,166))
  out=OUT/f'ArtQualityV3_Bastion{stage}_A_B_C_D.jpg'
  if out.exists():raise ValueError('Cannot overwrite previous V3 proof')
  pic.save(out,quality=95,subsampling=0)
  deriv.append({'file':str(out.relative_to(ROOT)),'sha256':sha(out.read_bytes())})
  measures[str(stage)]={
   'MAE_background_A_to_B':round(float(np.abs(originals[0]-originals[1]).mean()),5),
   'MAE_sawmill_C_to_D':round(float(np.abs(originals[2]-originals[3]).mean()),5)}
  if stage>=3 and measures[str(stage)]['MAE_sawmill_C_to_D']<=0.03:
   raise ValueError('Premium Sawmill did not render in true Unity')
 for focus in ('cuartel','aserradero'):
  sheet=Image.new('RGB',(1560,893),(25,34,43))
  draw=ImageDraw.Draw(sheet)
  for i,mode in enumerate(MODES):
   name=f'{mode}_bastion5_1170x2532_{focus}_close.png'
   im=images[name].resize((390,844),Image.Resampling.LANCZOS)
   sheet.paste(im,(390*i,45))
   draw.text((390*i+10,16),['A original','B quiet','C Cuartel','D Cuartel + Sawmill'][i],fill=(240,214,164))
  out=OUT/f'ArtQualityV3_{focus.title()}Detail_A_B_C_D.jpg'
  sheet.save(out,quality=95)
  deriv.append({'file':str(out.relative_to(ROOT)),'sha256':sha(out.read_bytes())})
 proof={'schema':1,'engine':'Unity','mode':'Editor_RenderTexture',
  'source_run_id':req['run_id'],'source_artifact_id':req['artifact_id'],'source_sha':req['source_sha'],
  'source_zip_sha256':sha(ZIP.read_bytes()),'native_frames':report['captures'],
  'native_frames_count':len(report['captures']),'review_derivatives_not_native_captures':deriv,
  'control_A_preserved':True,'original_gameplay_or_saves_modified':False,
  'editor_playmode_for_candidate_selection_certified':False,
  'physical_iPhone':'NOT_TESTED',
  'art_approved':False,'granary_and_quarry_upgrades_complete':False,
  'screen_differences_are_diagnostics_not_art_scores':measures}
 (OUT/'ArtQualityV3_NATIVE_EVIDENCE.json').write_text(json.dumps(proof,indent=2,ensure_ascii=False)+'\n')
 print('CITY2D_FOUR_MODE_32_AUTHENTIC_UNITY_SHA_PASS_ART_NOT_APPROVED',measures)
if __name__=='__main__':main()
