#!/usr/bin/env python3
"""Preserve two already-generated Valoria candidates and audit actual alpha.
A successful import is NEVER artistic approval, PlayMode or hardware QA.
"""
import hashlib, io, json, re, shutil, subprocess, sys
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request, urlopen
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
ART=BASE/'ArtBible'
SPRITES=BASE/'Sprites'
PILOTS=ART/'Pilots'
OUT=SPRITES/'AIPilot'
INPUT=Path(__file__).parent/'pending_import.json'
TASKS={
 'forja_guardia':('Forja_Guardia_RGBA.png','512b4dd8-4197-474f-8758-d06ab28e06b0','a382070c-3986-4a01-b192-d33a9e68612f'),
 'observatorio_brecha':('Observatorio_Brecha_RGBA.png','1aba8e1c-1d9c-4ae9-ab08-ec825470db9e','5fec8a9e-b251-4654-9692-64fd1df6d4bc')
}
def sha(data):return hashlib.sha256(data).hexdigest()
def fetch(url,task_id,suffix):
 p=urlsplit(url)
 if p.scheme!='https' or p.hostname!='dnznrvs05pmza.cloudfront.net' or task_id not in p.path or not p.path.endswith(suffix):
  raise ValueError('Unverified source identity: '+task_id)
 with urlopen(Request(url,headers={'User-Agent':'EldoriaArtBible/1.0'}),timeout=80) as r: data=r.read(15_000_001)
 if not 10_000<len(data)<15_000_000:raise ValueError('Source size invalid')
 return data
def open_image(data,expected):
 im=Image.open(io.BytesIO(data));im.load()
 if im.format!=expected or min(im.size)<512 or max(im.size)>4096:raise ValueError('Unexpected image format/dimensions')
 return im
def create_meta(target):
 template=(SPRITES/'Cuartel.png.meta').read_text()
 guid=hashlib.md5(str(target.relative_to(ROOT)).encode()).hexdigest()
 template=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,template,count=1)
 template=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',template)
 template=re.sub(r'textureCompression: \d+','textureCompression: 0',template)
 target.with_suffix('.png.meta').write_text(template)
def tile(im,title):
 bg=Image.new('RGBA',(512,535),(39,49,62,255))
 dr=ImageDraw.Draw(bg)
 for y in range(32,535,20):
  for x in range(0,512,20):
   c=(75,83,83,255) if ((x+y)//20)%2 else (118,120,114,255)
   dr.rectangle((x,y,x+19,y+19),fill=c)
 pic=im.convert('RGBA');pic.thumbnail((500,470),Image.Resampling.LANCZOS)
 bg.alpha_composite(pic,((512-pic.width)//2,34+(475-pic.height)//2))
 dr.text((10,9),title,fill='white')
 return bg.convert('RGB')
def main():
 recipe=json.loads(INPUT.read_text())
 assert recipe.get('schema')==1 and set(recipe['entries'])==set(TASKS),'Only known pre-existing tasks'
 PILOTS.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
 manifests=[];tiles=[]
 for key,(target_name,source_id,alpha_id) in TASKS.items():
  data=recipe['entries'][key]
  folder=PILOTS/key/'v1';folder.mkdir(parents=True,exist_ok=True)
  source_bytes=fetch(data['original_url'],source_id,'.jpg')
  alpha_bytes=fetch(data['alpha_url'],alpha_id,'.png')
  source=open_image(source_bytes,'JPEG')
  cutout=open_image(alpha_bytes,'PNG').convert('RGBA')
  (folder/'source.jpg').write_bytes(source_bytes)
  (folder/'provider_cutout.png').write_bytes(alpha_bytes)
  alpha=cutout.getchannel('A')
  alpha.save(folder/'mask_from_provider.png',optimize=True)
  lo,hi=alpha.getextrema();hist=alpha.histogram()
  transparent=sum(hist[:16])/(cutout.width*cutout.height)
  corners=[alpha.getpixel((x,y)) for x,y in [(0,0),(cutout.width-1,0),(0,cutout.height-1),(cutout.width-1,cutout.height-1)]]
  if lo!=0 or hi!=255 or transparent<.02 or max(corners)>24:
   raise ValueError('Candidate does not have auditable true transparency: '+key)
  work=folder/'normalized'
  cmd=[sys.executable,str(ART/'QA/preprocess_candidate.py'),'--source',str(folder/'provider_cutout.png'),'--out-dir',str(work),'--asset-id',key,'--task-id',alpha_id]
  p=subprocess.run(cmd,capture_output=True,text=True)
  if p.returncode:raise RuntimeError('Preprocess failed '+key+': '+p.stderr[-500:])
  candidate=work/(key+'_RGBA.png')
  target=OUT/target_name
  shutil.copyfile(candidate,target);create_meta(target)
  norm=json.loads((work/(key+'_MANIFEST.json')).read_text())
  manifest={
   'schema':1,'id':key,'state':'QUARANTINED_VISUAL_NOT_APPROVED',
   'original_task_id':source_id,'alpha_task_id':alpha_id,
   'references':'ArtBible/master_manifest_v1.json; previous generation master sheet',
   'original_file':'source.jpg','original_sha256':sha(source_bytes),
   'provider_cutout_file':'provider_cutout.png','provider_sha256':sha(alpha_bytes),
   'mask_file':'mask_from_provider.png','mask_sha256':sha((folder/'mask_from_provider.png').read_bytes()),
   'original_dimensions':list(source.size),'provider_dimensions':list(cutout.size),
   'provider_alpha_extrema':[lo,hi],'transparent_fraction':round(transparent,5),'corner_alphas':corners,
   'Unity_candidate_path':str(target.relative_to(ROOT)),'Unity_candidate_sha256':sha(target.read_bytes()),
   'Unity_candidate_dimensions':list(Image.open(target).size),
   'foot_pixel':norm['foot_px'],'foot_placement_review':'PENDING',
   'contact_shadow_layer':'PENDING','edge_and_smoke_review':'PENDING',
   'source_URL_not_preserved':True,'art_approved':False,'Unity_capture':False,'PlayMode':False,'iPhone':False
  }
  (folder/'asset_manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
  manifests.append(manifest)
  tiles.extend([tile(source,'ORIGINAL JPG '+key),tile(cutout,'SAM3 TRUE ALPHA '+key),tile(Image.open(target),'NORMALIZED — UNAPPROVED '+key)])
  print('RECOVERED',key,'candidate_sha256',manifest['Unity_candidate_sha256'],'alpha_range',lo,hi)
 contact=Image.new('RGB',(1536,535*2))
 for i,t in enumerate(tiles):contact.paste(t,((i%3)*512,(i//3)*535))
 contact.save(PILOTS/'QA_2_CANDIDATES_OFFLINE_NOT_UNITY.png',optimize=True)
 (PILOTS/'recovery_manifest.json').write_text(json.dumps({'schema':1,'state':'TWO_OF_THREE_NOT_ART_APPROVED','entries':manifests,'control_A_preserved':True,'engine_capture':False},ensure_ascii=False,indent=2)+'\n')
 print('CITY2D_TWO_PROVIDER_ALPHA_CUTOUTS_QUARANTINED_SHA_VERIFIED_NO_ART_APPROVAL')
if __name__=='__main__': main()
