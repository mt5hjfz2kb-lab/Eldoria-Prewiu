#!/usr/bin/env python3
"""Import already-generated Forja V3 as a NEW quarantined art-only sprite.
No existing V1/V2/master or gameplay asset is overwritten.
"""
import hashlib,io,json,re,shutil,subprocess,sys
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request,urlopen
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
DIR=BASE/'ArtBible/Pilots/forja_guardia/v3'
OUT=BASE/'Sprites/AIPilot/Versions'
SOURCE_SHA="1fb781c22a60ae61ce4a3cb28d6f394be3b15d8d344ca5e48a3afbd37877bd63"
DATASET="/datasets/73c72143-65b5-4ca0-ab62-f82d8923e412.png"
INPUT=Path(__file__).parent/'pending_forja_v3_url.json'
def sha(x):return hashlib.sha256(x).hexdigest()
def main():
 args=json.loads(INPUT.read_text(encoding='utf-8'))
 assert args['schema']==1 and args['source_sha256']==SOURCE_SHA
 url=args['url']
 p=urlsplit(url)
 if p.scheme!='https' or p.hostname!='d2jqrm6oza8nb6.cloudfront.net' or p.path!=DATASET:
  raise ValueError('Unrecognized source URL')
 with urlopen(Request(url,headers={'User-Agent':'Eldoria-V3-SourceArchive/1.0'}),timeout=90) as f:
  binary=f.read(8_000_001)
 if len(binary)>8_000_000 or sha(binary)!=SOURCE_SHA:
  raise ValueError('Source image bytes do not match locally audited SHA')
 im=Image.open(io.BytesIO(binary));im.load()
 if im.mode!='RGBA' or im.format!='PNG' or im.size!=(1536,1024):
  raise ValueError('Expected native transparent PNG 1536x1024')
 a=im.getchannel('A')
 lo,hi=a.getextrema()
 if lo!=0 or hi<245 or hi>255:
  raise ValueError('Source lacks correctly graded alpha')
 if max(a.getpixel((x,y)) for x,y in ((0,0),(1535,0),(0,1023),(1535,1023)))>20:
  raise ValueError('Image has opaque corner')
 # Preserve exact bytes and alpha; do NOT generate false texture, shadow or backplate.
 DIR.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
 source=DIR/'source_original_rgba.png';source.write_bytes(binary)
 a.save(DIR/'alpha_source.png',optimize=True)
 normal=DIR/'normalized'
 run=subprocess.run([sys.executable,str(BASE/'ArtBible/QA/preprocess_candidate.py'),
   '--source',str(source),'--out-dir',str(normal),
   '--asset-id','forja_guardia_v3','--task-id','imagegen-bc3db193-9156-4774-9616-628dd28d5b5a'],
   capture_output=True,text=True)
 if run.returncode:raise RuntimeError('Sprite normalization failed: '+run.stderr[-1000:])
 in_png=normal/'forja_guardia_v3_RGBA.png'
 dest=OUT/'Forja_Guardia_RGBA_v3.png'
 shutil.copyfile(in_png,dest)
 original_meta=(BASE/'Sprites/Cuartel.png.meta').read_text()
 guid=hashlib.md5(str(dest.relative_to(ROOT)).encode('utf-8')).hexdigest()
 original_meta=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,original_meta,count=1)
 original_meta=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',original_meta)
 original_meta=re.sub(r'textureCompression: \d+','textureCompression: 0',original_meta)
 dest.with_suffix('.png.meta').write_text(original_meta)
 pp=json.loads((normal/'forja_guardia_v3_MANIFEST.json').read_text())
 manifest={'schema':3,'id':'forja_guardia','variant':'V3_single_image_new_art',
  'status':'ART_NOT_APPROVED_UNITY_CAPTURE_PENDING',
  'generation_id':'bc3db193-9156-4774-9616-628dd28d5b5a',
  'source_origin':'ChatGPT image generation, imported into Runway-hosted asset without additional generation',
  'hosted_asset_id':'8f281509-5a09-43e8-b15b-6e2ee6b4ee42',
  'generation_reference_master_directly_attached':False,
  'visual_master':'ArtBible/Masters/Bastion_V_MASTER.png',
  'source_sha256':SOURCE_SHA,'candidate_sha256':sha(dest.read_bytes()),
  'source_dimensions':[1536,1024],'candidate_dimensions':list(Image.open(dest).size),
  'alpha_range':[lo,hi],'alpha_pixels_0':a.histogram()[0],
  'alpha_corners':[a.getpixel((x,y)) for x,y in ((0,0),(1535,0),(0,1023),(1535,1023))],
  'foot_px':pp['foot_px'],'source_preserved':True,
  'v1_and_v2_unchanged':True,'contact_shadow':'PENDING',
  'alpha_edge_visual_review':'PENDING','unity_native_art_comparison':'PENDING',
  'quality_compared_with_bastion_master':'PENDING','same_generator_3_building_proof':False,
  'art_approved':False,'physical_iphone':False}
 (DIR/'asset_manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')
 print('CITY2D_FORJA_V3_SOURCE_SHA_PASS_RGBA_QUARANTINE',manifest['candidate_sha256'])
if __name__=='__main__': main()
