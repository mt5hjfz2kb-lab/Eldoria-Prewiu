#!/usr/bin/env python3
"""Propose a LOCAL background-detail hierarchy variant, preserving terrain A.

Not a global sharpness/color filter. The original terrain bytes never change,
and the B variant is NOT activated without genuine same-camera Unity A/B.
"""
from pathlib import Path
import hashlib,json,re
import numpy as np,cv2
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
SOURCE=CITY/'Sprites/Terreno_Organico.png'
OUTPUT=CITY/'Sprites/Candidates/Terreno_Organico_FocoV1.png'
EXPECTED='9e257856139e39c5245dc8196a2131816bdd87ac4635b5fae679f6e65cb24db6'
QA=CITY/'QA'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 if sha(SOURCE)!=EXPECTED:raise ValueError('Original terrain SHA drift, refusing edit')
 im=Image.open(SOURCE);im.load()
 if im.mode!='RGB' or im.size!=(941,1672):
  raise ValueError('Unexpected original terrain dimensions')
 src=np.asarray(im,np.uint8)
 H,W=src.shape[:2]
 y,x=np.indices((H,W),dtype=np.float32)
 # Physical building anchors are taken ONLY from the existing immutable layout.
 layout=json.loads((QA/'city_layout.json').read_text())
 assert len(layout['buildings'])==6
 mask=np.zeros((H,W),dtype=np.float32)
 affected=[]
 for row in layout['buildings']:
  if row['id']=='salon':continue  # not authored as independent gameplay building
  rx,ry=(125,165) if row['id'] in ('cuartel','aserradero','granero') else (145,190)
  cx,cy=row['x'],row['y']
  dist=((x-cx)/rx)**2+((y-cy)/ry)**2
  contribution=np.exp(-dist*1.6)*.72
  mask=np.maximum(mask,contribution)
  affected.append({'id':row['id'],'center':[cx,cy],'radius':[rx,ry]})
 # Exclude margins and weak tails, ensuring we do not grade the whole world.
 mask[mask<.065]=0
 border=np.minimum.reduce([x,W-1-x,y,H-1-y])
 mask*=np.clip(border/65,0,1)
 # Existing roads and stone pavements must remain crisp: focus primarily on
 # foliage and high-frequency grass texture, preserving 3D drawing and streets.
 red=src[:,:,0].astype(np.float32)
 green=src[:,:,1].astype(np.float32)
 blue=src[:,:,2].astype(np.float32)
 foliage=np.clip((green-red+13)/24,0,1)*np.clip((green-blue+12)/20,0,1)
 weight=mask*(.38+.56*foliage)
 smoothed=cv2.bilateralFilter(src,9,32,8).astype(np.float32)
 delta=(smoothed-src.astype(np.float32))*weight[...,None]
 # Never darken or brighten the environment globally and never alter the
 # physically required streets by more than a few percent.
 result=np.clip(src.astype(np.float32)+delta,0,255).round().astype(np.uint8)
 diff=np.max(np.abs(result.astype(np.int16)-src.astype(np.int16)),axis=2)
 changed=int((diff>2).sum())
 if not 10000<changed<600000:
  raise ValueError('Focal retouch either ineffective or global')
 OUTPUT.parent.mkdir(parents=True,exist_ok=True);Image.fromarray(result,'RGB').save(OUTPUT,optimize=True)
 # Source A and B files saved in original resolution; these previews are NOT Unity.
 sheet=Image.new('RGB',(941*2,1672))
 sheet.paste(im,(0,0));sheet.paste(Image.fromarray(result,'RGB'),(941,0))
 sheet.thumbnail((1120,1000),Image.Resampling.LANCZOS)
 QA.mkdir(parents=True,exist_ok=True)
 preview=QA/'TerrenoFocoV1_A_B_OFFLINE_NOT_UNITY.jpg'
 sheet.save(preview,quality=92)
 doc={'schema':1,'status':'TERRAIN_CANDIDATE_UNAPPROVED',
  'original_sha256':EXPECTED,'candidate_sha256':sha(OUTPUT),
  'candidate':'Sprites/Candidates/Terreno_Organico_FocoV1.png',
  'size':[941,1672],'original_retained':True,
  'changed_pixels_above_2_RGB':changed,'changed_fraction':round(changed/(W*H),4),
  'max_channel_delta':int(diff.max()),'mean_abs_RGB':round(float(np.abs(delta).mean()),4),
  'editable_rois':affected,'method':'source-locked bilateral microdetail reduction in vegetated building-adjacent areas, roads protected',
  'no_global_filter':True,'no_extra_terrain_or_geometry':True,
  'preview_sha256':sha(preview),'Unity_AB':'PENDING','art_approved':False,
  'iPhone':'PENDING'}
 (QA/'TerrenoFocoV1_Manifest.json').write_text(json.dumps(doc,indent=2,ensure_ascii=False)+'\n')
 print('TERRAIN_LOCAL_HIERARCHY_CANDIDATE_SHA_VALIDATED_NOT_PROMOTED',doc['changed_fraction'])
if __name__=='__main__':main()
