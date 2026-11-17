#!/usr/bin/env python3
"""Version B2: restore ONLY original-photo smoke and navy flag removed by SAM3.
Read two immutable V1 sources. Add no new geometry, filters or material color.
Save as a NEW candidate; V1 and the approved masters remain byte-for-byte intact.
"""
from pathlib import Path
import hashlib,json,re,subprocess,sys,shutil
import numpy as np,cv2
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
V1=BASE/'ArtBible/Pilots/forja_guardia/v1'
V2=BASE/'ArtBible/Pilots/forja_guardia/v2'
TARGET=BASE/'Sprites/AIPilot/Versions/Forja_Guardia_RGBA_v2.png'
SHA_SRC='dd84edb2c00fe23b2a92e68b60aed7b8d1ceb3bccfd137bfe433a3b9d3144ab8'
SHA_ALPHA='e6a314dc1b90b0186c72430590568c6db845e880de0e4f17067af6a1ebffe37c'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def main():
 assert sha(V1/'source.jpg')==SHA_SRC, 'Source photo unexpectedly modified'
 assert sha(V1/'provider_cutout.png')==SHA_ALPHA, 'Original provider segmentation modified'
 src=np.asarray(Image.open(V1/'source.jpg').convert('RGB')).astype(np.float32)
 cut=np.asarray(Image.open(V1/'provider_cutout.png').convert('RGBA')).astype(np.float32)
 H,W=src.shape[:2]
 assert (W,H)==(1024,1024)
 y,x=np.indices((H,W))
 bg=np.array([19,17,18],dtype=np.float32)
 # Original tiny banner: provider erased dark navy fabric but kept its ornament.
 # Local blue chroma mask intentionally cannot affect other roofs/heraldry.
 blue=src[:,:,2]-(src[:,:,0]+src[:,:,1])/2
 bannerROI=(x>=606)&(x<=709)&(y>=84)&(y<=137)
 bluealpha=np.clip((blue-4)/12,0,1)*bannerROI.astype(np.float32)
 visible=(bluealpha>.5).astype(np.uint8)
 visible=cv2.morphologyEx(visible,cv2.MORPH_CLOSE,
  cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(5,5)))
 bluealpha=np.maximum(bluealpha,visible*.84)
 bluealpha=cv2.GaussianBlur(bluealpha.astype(np.float32),(3,3),.6)
 # Source-photo forge smoke, isolated above the fire. No fabricated plume.
 # Mask excludes the already-preserved roof/structure alpha.
 ell=((x-764)/123)**2+((y-267)/142)**2
 spatial=np.clip((1.28-ell)/.45,0,1)
 smoke=np.clip((src.mean(axis=2)-bg.mean()-5)/53,0,.46)
 smoke=smoke*spatial*(1-cut[:,:,3]/255)
 smoke=cv2.GaussianBlur(smoke.astype(np.float32),(5,5),1.3)
 newalpha=np.maximum(bluealpha,smoke)
 # Decontaminate only extracted flag/smoke against observed near-black matte.
 threshold=newalpha>.015
 color=np.zeros_like(src)
 color[threshold]=np.clip((src[threshold]-bg*(1-newalpha[threshold,None]))/
  np.maximum(.05,newalpha[threshold,None]),0,255)
 fx=Image.fromarray(np.dstack([color.astype(np.uint8),
  (np.clip(newalpha,0,1)*255).astype(np.uint8)]),'RGBA')
 orig=Image.fromarray(cut.astype(np.uint8),'RGBA')
 revised=orig.copy();revised.alpha_composite(fx)
 V2.mkdir(parents=True,exist_ok=True); TARGET.parent.mkdir(parents=True,exist_ok=True)
 fx.save(V2/'original_smoke_and_banner_FX_rgba.png',optimize=True)
 revised.save(V2/'provider_cutout_targeted_repaired.png',optimize=True)
 # Localized source trace and original preserved: normalization does not change
 # lighting or invent texture, only real alpha padding/foot placement.
 normalize=[sys.executable,str(BASE/'ArtBible/QA/preprocess_candidate.py'),
  '--source',str(V2/'provider_cutout_targeted_repaired.png'),
  '--out-dir',str(V2/'normalized'),'--asset-id','forja_guardia_v2',
  '--task-id','SAME_PHOTO_REPAIR_NO_GENERATION']
 p=subprocess.run(normalize,capture_output=True,text=True)
 if p.returncode:raise RuntimeError('Original asset normalization failed '+p.stderr[-800:])
 shutil.copyfile(V2/'normalized/forja_guardia_v2_RGBA.png',TARGET)
 meta=(BASE/'Sprites/Cuartel.png.meta').read_text()
 guid=hashlib.md5(str(TARGET.relative_to(ROOT)).encode()).hexdigest()
 meta=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,meta,count=1)
 meta=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',meta)
 meta=re.sub(r'textureCompression: \d+','textureCompression: 0',meta)
 TARGET.with_suffix('.png.meta').write_text(meta)
 # Offline A/B proof on same neutral color background, not from Unity.
 proof=Image.new('RGB',(1200,600),(36,45,55))
 for i,(im,name) in enumerate([(orig,'B1 original provider'),(revised,'B2 recovered flag/smoke')]):
  bgim=Image.new('RGBA',(1024,1024),(48,63,74,255))
  bgim.alpha_composite(im)
  rendered=bgim.convert('RGB').resize((600,600),Image.Resampling.LANCZOS)
  proof.paste(rendered,(600*i,0))
 proof.save(V2/'QA_BGR_COMPARISON_OFFLINE_NOT_UNITY.jpg',quality=94)
 info=json.loads((V2/'normalized/forja_guardia_v2_MANIFEST.json').read_text())
 report={
  'schema':2,'id':'forja_guardia','variant':'B2_original_photo_alpha_recovery',
  'status':'QUARANTINED_NOT_ART_APPROVED',
  'v1_original_sha256':SHA_SRC,'v1_provider_cutout_sha256':SHA_ALPHA,
  'v1_candidate_preserved':True,'generated_new_image':False,
  'changed_regions':['original navy flag x606:709 y84:137',
                     'original forge smoke ellipse center(764,267) radii(123,142)'],
  'smoke_fx_sha256':sha(V2/'original_smoke_and_banner_FX_rgba.png'),
  'repaired_cutout_sha256':sha(V2/'provider_cutout_targeted_repaired.png'),
  'new_candidate_path':str(TARGET.relative_to(ROOT)),
  'new_candidate_sha256':sha(TARGET),
  'new_candidate_dimensions':list(Image.open(TARGET).size),
  'foot_pixel':info['foot_px'],'shadow_separate':'PENDING',
  'Unity_native_capture_v2':False,'art_approved':False,'iPhone':False
 }
 (V2/'asset_manifest.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
 print('FORJA_V2_TARGETED_SOURCE_ALPHA_REPAIRED_NOT_ART_APPROVED',report['new_candidate_sha256'])
if __name__=='__main__':main()
