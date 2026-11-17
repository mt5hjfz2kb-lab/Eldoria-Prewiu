#!/usr/bin/env python3
"""Import one existing, source-hashed Archivo illustration as an isolated AI-art candidate.
Not a gameplay building, not artistic approval, not a native Unity capture.
The original is generated through a DIFFERENT provider from the prior two; provenance
must never imply a proven common visual production method.
"""
from __future__ import annotations
import hashlib,io,json,re,shutil,subprocess,sys
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request,urlopen
import cv2,numpy as np
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
PILOT=BASE/'ArtBible/Pilots/archivo_cartografos/v1'
OUT=BASE/'Sprites/AIPilot'
INPUT=Path(__file__).parent/'pending_archivo_url.json'
ASSET='archivo_cartografos'
EXPECTED_SOURCE_SHA='0f3a1ead1d0a84402c8493a84cbbe4a660c0e7fbcf144c5ad3138f43069396df'
SOURCE_GENERATION='f785b806-0718-4667-bf58-251d3400d3e2'
SOURCE_ASSET='0e90eb09-99c3-4acf-9766-e17b5c41d051'
def sha(v):return hashlib.sha256(v).hexdigest()
def import_png(url):
 p=urlsplit(url)
 if p.scheme!='https' or p.hostname!='d2jqrm6oza8nb6.cloudfront.net' or '/datasets/96d5a1cb-f1b3-497c-904f-6b730ffe7358.png' not in p.path:
  raise ValueError('Rejecting unverified source URL')
 with urlopen(Request(url,headers={'User-Agent':'ValoriaArtPilot/1.0'}),timeout=90) as reply:
  raw=reply.read(9_000_001)
 if sha(raw)!=EXPECTED_SOURCE_SHA:raise ValueError('Generated original differs from immutable original SHA-256')
 img=Image.open(io.BytesIO(raw));img.load()
 if img.format!='PNG' or img.mode not in ('RGB','RGBA') or img.size!=(1536,1024):
  raise ValueError('Unexpected source asset dimensions/format')
 return raw,img.convert('RGB')
def mask_from_dark_border(rgb):
 """Flood only the near-uniform exterior charcoal matte; never threshold full building.
 Targeted lower-architecture closing preserves uncut dark entrance interiors,
 while upper finial/roof holes avoid aggressive closing.
 """
 arr=np.asarray(rgb,dtype=np.uint8)
 H,W=arr.shape[:2]
 edge=np.concatenate((arr[:60,:,:].reshape(-1,3),arr[-60:,:,:].reshape(-1,3),arr[:,:60,:].reshape(-1,3),arr[:,-60:,:].reshape(-1,3)))
 backdrop=np.median(edge,axis=0)
 if not (np.max(backdrop)<46 and np.min(backdrop)>18):
  raise ValueError('Unrecognized source backdrop; segmentation calibrated only to original')
 d=np.sqrt(np.sum((arr.astype(np.float32)-backdrop)**2,axis=2))
 matte=(d<23).astype(np.uint8)
 _,labels,_,_=cv2.connectedComponentsWithStats(matte,connectivity=8)
 exterior=np.unique(np.concatenate([labels[0,:],labels[-1,:],labels[:,0],labels[:,-1]]))
 binary=(~np.isin(labels,exterior)).astype(np.uint8)
 small=cv2.morphologyEx(binary,cv2.MORPH_CLOSE,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(7,7)))
 large=cv2.morphologyEx(binary,cv2.MORPH_CLOSE,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(31,31)))
 # Targeted physical-building region, not global cleanup of the roof.
 weight=np.clip((np.arange(H,dtype=np.float32)-470)/65,0,1)[:,None]
 blended=(small*(1-weight)+large*weight).astype(np.float32)
 alpha=np.clip(cv2.GaussianBlur(blended,(3,3),.64)*255,0,255).astype(np.uint8)
 a=alpha.astype(np.float32)/255
 out=arr.astype(np.float32)
 fringe=(a>.025)&(a<.99)
 out[fringe]=np.clip((out[fringe]-backdrop*(1-a[fringe,None]))/np.maximum(.05,a[fringe,None]),0,255)
 out[a<=.025]=0
 return Image.fromarray(np.dstack((out.astype(np.uint8),alpha)),'RGBA'),Image.fromarray(alpha,'L'),backdrop.tolist()
def save_meta(file):
 original=(BASE/'Sprites/Cuartel.png.meta').read_text()
 guid=hashlib.md5(str(file.relative_to(ROOT)).encode()).hexdigest()
 original=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,original,count=1)
 original=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',original)
 original=re.sub(r'textureCompression: \d+','textureCompression: 0',original)
 file.with_suffix('.png.meta').write_text(original)
def preview(src,mask):
 # Offline inspection, never represented as evidence from Unity.
 canvas=Image.new('RGB',(1024,3*370),(33,42,50))
 for i,(color,label) in enumerate([((32,32,32),'NEGRO'),((124,124,124),'GRIS'),((55,81,51),'VERDE')]):
  tile=Image.new('RGBA',src.size,color+(255,))
  tile.alpha_composite(src);tile=tile.convert('RGB')
  tile.thumbnail((1024,360))
  canvas.paste(tile,((1024-tile.width)//2,i*370+10))
 canvas.save(PILOT/'QA_ALPHA_BG_OFFLINE_NOT_UNITY.jpg',quality=94)
def main():
 d=json.loads(INPUT.read_text())
 if d.get('schema')!=1 or d.get('source_asset_id')!=SOURCE_ASSET:
  raise ValueError('Unknown one-time source')
 raw,rgb=import_png(d['download_url'])
 PILOT.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
 src=PILOT/'source_original.png';src.write_bytes(raw)
 rgba,mask,bg=mask_from_dark_border(rgb)
 mask.save(PILOT/'mask_review_candidate.png',optimize=True)
 rgba.save(PILOT/'cutout_rgba.png',optimize=True)
 lo,hi=mask.getextrema()
 corners=[mask.getpixel((x,y)) for x,y in [(0,0),(rgb.width-1,0),(0,rgb.height-1),(rgb.width-1,rgb.height-1)]]
 transparent=np.mean(np.asarray(mask)<16)
 if lo!=0 or hi!=255 or transparent<.15 or max(corners)>24:
  raise ValueError('True alpha gate not satisfied')
 preview(rgba,mask)
 proc=PILOT/'normalized'
 x=subprocess.run([sys.executable,str(BASE/'ArtBible/QA/preprocess_candidate.py'),
  '--source',str(PILOT/'cutout_rgba.png'),'--out-dir',str(proc),
  '--asset-id',ASSET,'--task-id',SOURCE_GENERATION],text=True,capture_output=True)
 if x.returncode:raise RuntimeError('Normalization failed '+x.stderr[-800:])
 candidate=proc/(ASSET+'_RGBA.png');target=OUT/'Archivo_Cartografos_RGBA.png'
 shutil.copyfile(candidate,target);save_meta(target)
 info=json.loads((proc/(ASSET+'_MANIFEST.json')).read_text())
 report={
  'schema':1,'id':ASSET,'model':'first_party_image_generation_not_nano_banana',
  'generation_id':SOURCE_GENERATION,'runway_upload_asset_id':SOURCE_ASSET,
  'model_seed':'unknown','exact_provider_prompt':'not_exposed',
  'reference_images':'visual comparison with six masters recorded; direct reference-conditioning not independently verified',
  'status':'QUARANTINED_NOT_ART_APPROVED','original_source':'source_original.png',
  'original_sha256':EXPECTED_SOURCE_SHA,'original_dimensions':[rgb.width,rgb.height],
  'source_type':'PNG_RGB_mattee','segmentation_method':'border_flood_RGBd23; ROI_close_7_to31; soft_edge_3; v1',
  'background_rgb':bg,'mask_sha256':sha((PILOT/'mask_review_candidate.png').read_bytes()),
  'cutout_sha256':sha((PILOT/'cutout_rgba.png').read_bytes()),
  'sprite_path':str(target.relative_to(ROOT)),'sprite_sha256':sha(target.read_bytes()),
  'sprite_dimensions':list(Image.open(target).size),'foot_pixel':info['foot_px'],
  'alpha_extrema':[lo,hi],'transparent_fraction':round(float(transparent),4),
  'manual_alpha_edge_approval':False,'contact_shadow_separate':'PENDING',
  'Unity_native_captures':False,'PlayMode':False,'iPhone':False,'art_approved':False,
  'reproducible_same_generator_as_two_previous':False
 }
 (PILOT/'asset_manifest.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
 print('ARCHIVO_ORIGINAL_HASH_MATCHED_ALPHA_CANDIDATE_QUARANTINED',report['sprite_sha256'])
if __name__=='__main__':main()
