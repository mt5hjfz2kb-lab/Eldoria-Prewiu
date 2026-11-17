#!/usr/bin/env python3
"""Technical-only three-asset pilot verification; never an art acceptance test.
Checks immutable sources, exact exported PNG bytes, non-empty RGBA and truthful
provenance. It intentionally does NOT certify perspective, shadow, mobile or Unity.
"""
import hashlib,json
from pathlib import Path
from PIL import Image

BASE=Path(__file__).resolve().parents[2]
ART=BASE/'ArtBible'
CAND=BASE/'Sprites/AIPilot'
ENTRIES=[
 ('forja_guardia','Forja_Guardia_RGBA.png',
  'dd84edb2c00fe23b2a92e68b60aed7b8d1ceb3bccfd137bfe433a3b9d3144ab8',
  '620fb58c48dafc4963c11467c4bc10900b14404b07de228876cc2eeb139f9b8d'),
 ('observatorio_brecha','Observatorio_Brecha_RGBA.png',
  '627637f5ec3b25369f7d4072145cefc5d691cb93885b0a917e5fd52eb05414bb',
  '74d058a9b891515a6ae8d263d380f0bf8d75c528f94b6138bf15aa3ac434e014'),
 ('archivo_cartografos','Archivo_Cartografos_RGBA.png',
  '0f3a1ead1d0a84402c8493a84cbbe4a660c0e7fbcf144c5ad3138f43069396df',
  '7bb221f5f766ff41f731756ada234ed6e923e5fd0f4cecf19d98bce290cf415c')]
def hash_file(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for key,sprite_name,src_sha,spr_sha in ENTRIES:
 directory=ART/'Pilots'/key/'v1'
 manifest=json.loads((directory/'asset_manifest.json').read_text())
 src=directory/('source_original.png' if key=='archivo_cartografos' else 'source.jpg')
 spr=CAND/sprite_name
 assert src.is_file() and spr.is_file(),(key,'missing original/candidate')
 assert hash_file(src)==src_sha,(key,'original SHA drift')
 assert hash_file(spr)==spr_sha,(key,'candidate SHA drift')
 im=Image.open(spr);assert im.format=='PNG' and im.mode=='RGBA',(key,'not RGBA')
 a=im.getchannel('A')
 assert a.getextrema()==(0,255),(key,'not genuinely transparent')
 assert all(a.getpixel(p)<25 for p in ((0,0),(im.width-1,0),(0,im.height-1),(im.width-1,im.height-1))),(key,'opaque corner')
 bbox=a.point(lambda x:255 if x>=32 else 0).getbbox()
 assert bbox and bbox[2]-bbox[0]>=400 and bbox[3]-bbox[1]>=400,(key,'missing building body')
 assert max(im.size)<=2048,(key,'over-budget source resolution')
 assert manifest.get('art_approved') is False,(key,'unreviewed art marked approved')
 meta=spr.with_suffix('.png.meta');assert meta.is_file() and 'spriteAlignment: 7' in meta.read_text(),(key,'bottom pivot')
 if key=='archivo_cartografos':
  assert manifest['original_sha256']==src_sha and manifest['sprite_sha256']==spr_sha
  assert manifest['reproducible_same_generator_as_two_previous'] is False
 else:
  assert manifest['original_sha256']==src_sha and manifest['Unity_candidate_sha256']==spr_sha
  assert manifest['provider_alpha_extrema']==[0,255]
  assert (directory/'mask_from_provider.png').is_file()
 print('PILOT_SOURCE_AND_RGBA_PASS_ART_NOT_APPROVED',key,src_sha,spr_sha,im.size,bbox)
print('3_SOURCE_HASHES_AND_ALPHA_PASS | NO_UNITY_OR_ART_APPROVAL_CLAIM')
