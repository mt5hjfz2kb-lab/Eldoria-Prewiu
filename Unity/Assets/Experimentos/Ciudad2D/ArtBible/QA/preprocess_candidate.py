#!/usr/bin/env python3
"""Convert one source RGBA to a foot-anchored Unity trial sprite, preserving original.
Opaque JPG / black-matte sources REQUIRE a separately reviewed mask.
The result is never art approved automatically.
"""
import argparse,hashlib,json
from pathlib import Path
from PIL import Image
ap=argparse.ArgumentParser()
ap.add_argument('--source',type=Path,required=True)
ap.add_argument('--out-dir',type=Path,required=True)
ap.add_argument('--asset-id',required=True)
ap.add_argument('--mask',type=Path)
ap.add_argument('--task-id',default='unknown')
ap.add_argument('--foot-x',type=float)
a=ap.parse_args()
a.out_dir.mkdir(parents=True,exist_ok=True)
raw=a.source.read_bytes();image=Image.open(a.source).convert('RGBA');W,H=image.size
alpha=Image.open(a.mask).convert('L') if a.mask else image.getchannel('A')
if alpha.size!=image.size:raise ValueError('Mask size differs from image')
lo,hi=alpha.getextrema()
if lo==hi or hi<220:raise ValueError('Alpha missing: supply reviewed mask, no fake transparency')
if max(alpha.getpixel((x,y)) for x,y in [(0,0),(W-1,0),(0,H-1),(W-1,H-1)])>24:
    raise ValueError('Opaque border/matte: reviewer must supply a mask')
image.putalpha(alpha)
bbox=alpha.point(lambda x:255 if x>=24 else 0).getbbox()
if bbox is None:raise ValueError('Empty silhouette')
m=max(8,round(max(image.size)*.03))
crop=(max(0,bbox[0]-m),max(0,bbox[1]-m),min(W,bbox[2]+m),min(H,bbox[3]+m))
image=image.crop(crop);side=max(image.size)+2*m
out=Image.new('RGBA',(side,side),(0,0,0,0))
out.alpha_composite(image,((side-image.width)//2,side-image.height-m))
bounds=out.getchannel('A').point(lambda x:255 if x>24 else 0).getbbox()
foot=(round((bounds[0]+bounds[2])/2) if a.foot_x is None else round(a.foot_x*side),bounds[3]-1)
if not(bounds[0]<=foot[0]<=bounds[2]):raise ValueError('Foot x outside image')
dst=a.out_dir/(a.asset_id+'_RGBA.png');out.save(dst,optimize=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
meta={'schema':1,'id':a.asset_id,'status':'QUARANTINED_NOT_APPROVED',
 'source_task':a.task_id,'source_sha256':hashlib.sha256(raw).hexdigest(),
 'mask_sha256':sha(a.mask) if a.mask else None,'crop_rect':crop,
 'sprite_size':[side,side],'sprite_sha256':sha(dst),
 'foot_px':foot,'foot_norm':[round(foot[0]/side,5),round(foot[1]/side,5)],
 'unity_native_capture':'PENDING','art_review':'PENDING'}
(a.out_dir/(a.asset_id+'_MANIFEST.json')).write_text(json.dumps(meta,indent=2)+'\n')
print('RGBA_CANDIDATE_TECH_PASS_ART_NOT_APPROVED',json.dumps(meta))
