#!/usr/bin/env python3
"""Make a reversible, independent ground-contact shadow from real V3 foot alpha.
Technical approximation, not an artist-approved physically rendered shadow.
Never change color art, masters, gameplay or previous candidate versions.
"""
import hashlib,json,re
from pathlib import Path
from PIL import Image,ImageFilter
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
CANDIDATE=BASE/'Sprites/AIPilot/Versions/Forja_Guardia_RGBA_v3.png'
OUT=BASE/'Sprites/AIPilot/Versions/Forja_Guardia_ContactShadow_v3.png'
REVIEW=BASE/'ArtBible/Pilots/forja_guardia/v3'
EXPECTED_SHA="ed42684004cd891c83d933d38b195fb794f5db8449bb4436b2c043a7ab8c99ad"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 assert CANDIDATE.is_file() and sha(CANDIDATE)==EXPECTED_SHA,"Source must stay frozen"
 image=Image.open(CANDIDATE).convert('RGBA')
 alpha=np.asarray(image.getchannel('A'))
 H,W=alpha.shape
 yy,xx=np.where(alpha>64)
 if len(yy)<120000:raise ValueError("Source has insufficient real alpha")
 highest,foot=int(yy.min()),int(yy.max())
 low=max(0,foot-int(.19*(foot-highest)))
 profile=(alpha[low:foot,:]>128).mean(axis=0)
 stripe=np.uint8(np.clip(profile*255*2.2,0,255))
 smoothed=np.tile(stripe,(36,1))
 weight=np.asarray(Image.fromarray(smoothed,'L').filter(ImageFilter.GaussianBlur(24)),dtype=np.float32)[18]/255
 weight/=weight.max()+1e-6
 Y=np.arange(H,dtype=np.float32)[:,None]
 core=np.exp(-.5*((Y-(foot+6))/33)**2)
 spread=np.exp(-.5*((Y-(foot+2))/63)**2)
 edge=np.clip((H-1-Y)/28,0,1)
 shadow_alpha=np.uint8(np.clip((66*core+26*spread)*weight[None,:]*edge,0,115))
 shadow=np.zeros((H,W,4),dtype=np.uint8)
 shadow[:,:,:3]=[32,27,56] # blue-violet ambient shade, no ground tile
 shadow[:,:,3]=shadow_alpha
 if int(shadow_alpha.max())<45 or shadow_alpha[0].max()>0 or shadow_alpha[-1].max()>2:
  raise ValueError("Invalid physical contact opacity/border")
 OUT.parent.mkdir(parents=True,exist_ok=True)
 REVIEW.mkdir(parents=True,exist_ok=True)
 Image.fromarray(shadow,'RGBA').save(OUT,optimize=True)
 meta=(BASE/'Sprites/Cuartel.png.meta').read_text()
 guid=hashlib.md5(str(OUT.relative_to(ROOT)).encode()).hexdigest()
 meta=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,meta,count=1)
 meta=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',meta)
 meta=re.sub(r'textureCompression: \d+','textureCompression: 0',meta)
 OUT.with_suffix('.png.meta').write_text(meta)
 bg=Image.new('RGBA',image.size,(78,102,64,255))
 bg.alpha_composite(Image.fromarray(shadow,'RGBA'))
 bg.alpha_composite(image)
 bg.convert('RGB').resize((720,720)).save(REVIEW/'shadow_preview_OFFLINE_NOT_UNITY.jpg',quality=93)
 report={
  'schema':1,'candidate_sha256':EXPECTED_SHA,'shadow_sha256':sha(OUT),
  'shadow_path':str(OUT.relative_to(ROOT)),
  'dimensions':[W,H],'foot_bottom_px':foot,'source_foot_y_cutoff':low,
  'method':'source RGBA lower-19%-foot occupancy, horizontally 24px blurred, vertical two Gaussian layers, edge clipped',
  'alpha_range':[int(shadow_alpha.min()),int(shadow_alpha.max())],
  'separate_layer':True,'original_colour_sprite_unchanged':True,
  'art_status':'REVIEW_PENDING','unity_capture':'PENDING',
  'real_physical_shadow':False,'art_approved':False
 }
 (REVIEW/'shadow_manifest.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
 print('CITY2D_CONTACT_SHADOW_SEPARATE_ALPHA_PASS_ART_UNAPPROVED',report['shadow_sha256'])
if __name__=='__main__':main()
