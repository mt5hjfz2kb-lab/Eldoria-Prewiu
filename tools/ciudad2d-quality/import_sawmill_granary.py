#!/usr/bin/env python3
"""Archive pre-existing Sawmill + Granary art with a deterministic reviewed cutout.

This does NOT generate art, touch game logic, replace original sprites, or
certify artistic consistency. Signed input URLs exist only in a one-use
handoff, deleted by CI after successful archival.
"""
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request,urlopen
from io import BytesIO
import hashlib,json,re
import numpy as np, cv2
from PIL import Image,ImageFilter

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
DEST=CITY/'Sprites/Candidates'
QA=CITY/'QA'
INPUT=Path(__file__).parent/'pending_two_art.json'
SOURCES={
 'aserradero':{
  'dataset':'/datasets/eda3cd16-b593-4053-bf42-31de4aba2b4f.png',
  'sha':'4bbbb369b6bf873f801ddc14366cb1231eedafc3414546f1642024845338533a',
  'stem':'Aserradero_Premium_V2','threshold':15,
  'generation_id':'93593bfc-8521-46eb-a402-4a71a563ae6f',
  'calibration':[14,14,20]},
 'granero':{
  'dataset':'/datasets/ad5ff29f-a3d0-4d47-a923-d95516464ca2.png',
  'sha':'a824e4b0dc9b935ba6e5bdae253187ce2d424bc61c305269930917b7b83438e4',
  'stem':'Granero_Premium_V1','threshold':18,
  'generation_id':'2baa3194-653f-44b7-aae8-7a2cc48b690a',
  'calibration':[7,7,11]}
}
def sha(data):return hashlib.sha256(data).hexdigest()
def meta(path):
 template=(CITY/'Sprites/Cuartel.png.meta').read_text(encoding='utf-8')
 guid=hashlib.md5(str(path.relative_to(ROOT)).encode('utf-8')).hexdigest()
 template=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,template,count=1)
 template=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',template)
 template=re.sub(r'textureCompression: \d+','textureCompression: 0',template)
 path.with_suffix('.png.meta').write_text(template,encoding='utf-8')
def download(url,cfg):
 p=urlsplit(url)
 if p.scheme!='https' or p.hostname!='d2jqrm6oza8nb6.cloudfront.net' or p.path!=cfg['dataset']:
  raise ValueError('Reject unknown signed source endpoint')
 with urlopen(Request(url,headers={'User-Agent':'ValoriaApprovedSourceArchive/1.0'}),timeout=85) as f:
  binary=f.read(9_000_001)
 if len(binary)<400_000 or len(binary)>9_000_000 or sha(binary)!=cfg['sha']:
  raise ValueError('Original generation source SHA mismatch (do not substitute artwork)')
 image=Image.open(BytesIO(binary)); image.load()
 if image.format!='PNG' or image.size!=(1536,1024) or image.mode!='RGB':
  raise ValueError('Unexpected exact original format/dimensions')
 return binary,np.asarray(image,dtype=np.uint8)
def isolate(rgb,kind,cfg):
 H,W=rgb.shape[:2]
 corners=np.concatenate([rgb[:70,:70].reshape(-1,3),rgb[:70,-70:].reshape(-1,3),
                         rgb[-70:,:70].reshape(-1,3),rgb[-70:,-70:].reshape(-1,3)])
 backdrop=np.median(corners,axis=0)
 if np.max(np.abs(backdrop-np.array(cfg['calibration'])))>5:
  raise ValueError('Background not same as locally reviewed isolated image')
 diff=np.max(np.abs(rgb.astype(np.int16)-backdrop[None,None,:]),axis=2)
 matte=(diff<cfg['threshold']).astype(np.uint8)
 _,labels,_,_=cv2.connectedComponentsWithStats(matte,connectivity=8)
 exterior=np.unique(np.r_[labels[0,:],labels[-1,:],labels[:,0],labels[:,-1]])
 foreground=(~np.isin(labels,exterior)).astype(np.uint8)
 _,components,stats,_=cv2.connectedComponentsWithStats(foreground,connectivity=8)
 significant=np.where(stats[:,cv2.CC_STAT_AREA]>=60)[0]
 significant=significant[significant!=0]
 body=np.isin(components,significant).astype(np.uint8)
 body=cv2.morphologyEx(body,cv2.MORPH_CLOSE,
                       cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(11,11)))
 rescued=0
 if kind=='aserradero':
  broad=cv2.morphologyEx(body,cv2.MORPH_CLOSE,
                         cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(35,35)))
  Y=np.arange(H)[:,None]
  rescue=(Y<530)&(broad>0)&(body==0)&(diff>8)
  rescued=int(rescue.sum())
  body[rescue]=1
 a=cv2.GaussianBlur(body.astype(np.float32),(3,3),.65)
 source=np.dstack((rgb,(np.clip(a,0,1)*255).astype(np.uint8)))
 source[source[:,:,3]==0,:3]=0
 image=Image.fromarray(source,'RGBA')
 bbox=image.getchannel('A').getbbox()
 if bbox is None or bbox[2]-bbox[0]<1000 or bbox[3]-bbox[1]<640:
  raise ValueError('Missing functional architecture silhouette')
 margin=35
 cropped=image.crop((max(0,bbox[0]-margin),max(0,bbox[1]-margin),
                     min(W,bbox[2]+margin),min(H,bbox[3]+margin)))
 side=max(cropped.size)+80
 if side>2048:raise ValueError('Source does not fit 2048-pixel mobile art budget')
 output=Image.new('RGBA',(side,side),(0,0,0,0))
 output.alpha_composite(cropped,((side-cropped.width)//2,side-cropped.height-45))
 if output.getchannel('A').getextrema()!=(0,255):
  raise ValueError('Actual transparency required')
 if max(output.getchannel('A').getpixel(xy) for xy in
    [(0,0),(side-1,0),(0,side-1),(side-1,side-1)])!=0:
  raise ValueError('Opaque border or rectangular display mat')
 return output,dict(background_rgb=[float(x) for x in backdrop],
                    threshold=cfg['threshold'],bbox_original=list(bbox),
                    rescued_roof_source_pixels=rescued)
def shadow_from_actual_foot(sprite):
 a=np.asarray(sprite.getchannel('A'))
 n=a.shape[0];ys,xs=np.where(a>80)
 if len(ys)<40000:raise ValueError('Not enough physical building pixels for shadow')
 bottom=int(ys.max());top=int(ys.min());low=max(top,bottom-int((bottom-top)*.17))
 profile=(a[low:bottom]>120).mean(axis=0)
 stripe=np.uint8(np.clip(profile*255*2.2,0,255))
 weighted=np.asarray(Image.fromarray(np.tile(stripe,(60,1)),'L')
                     .filter(ImageFilter.GaussianBlur(26)),dtype=np.float32)[30]/255
 weighted/=max(float(weighted.max()),.001)
 y=np.arange(n,dtype=np.float32)[:,None]
 vertical=(np.exp(-.5*((y-(bottom+5))/36)**2)*74+
           np.exp(-.5*((y-(bottom+3))/65)**2)*29)
 edge=np.clip((n-1-y)/24,0,1)
 rgba=np.zeros((n,n,4),dtype=np.uint8);rgba[:,:,:3]=[34,29,52]
 rgba[:,:,3]=np.uint8(np.clip(vertical*weighted[None,:]*edge,0,110))
 # Shadow source at half resolution to save device texture memory;
 # Unity gallery applies a real-world 2x scale for matching dimensions.
 full=Image.fromarray(rgba,'RGBA')
 half=full.resize(((n+1)//2,(n+1)//2),Image.Resampling.LANCZOS)
 return half,bottom
def main():
 req=json.loads(INPUT.read_text(encoding='utf-8'))
 if req.get('schema')!=1 or set(req['urls'])!=set(SOURCES):
  raise ValueError('Missing two exact known generated originals')
 QA.mkdir(parents=True,exist_ok=True);DEST.mkdir(parents=True,exist_ok=True)
 output={}
 for kind,cfg in SOURCES.items():
  binary,rgb=download(req['urls'][kind],cfg)
  stem=cfg['stem']
  sourcefile=DEST/(stem+'_Source.png');sourcefile.write_bytes(binary);meta(sourcefile)
  sprite,diagnosis=isolate(rgb,kind,cfg)
  candidate=DEST/(stem+'.png');sprite.save(candidate,optimize=True);meta(candidate)
  shadow,foot=shadow_from_actual_foot(sprite)
  shadowfile=DEST/(stem+'_Shadow.png');shadow.save(shadowfile,optimize=True);meta(shadowfile)
  mask=DEST/(stem+'_Mask.png');sprite.getchannel('A').save(mask,optimize=True);meta(mask)
  sw,sh=sprite.size
  if max(sw,sh)>2048 or sprite.mode!='RGBA' or shadow.mode!='RGBA':
   raise ValueError('Mobile sprite import budget invalid')
  bg=Image.new('RGBA',sprite.size,(83,109,74,255))
  layer=shadow.resize(sprite.size,Image.Resampling.BILINEAR)
  bg.alpha_composite(layer);bg.alpha_composite(sprite)
  bg.thumbnail((760,760),Image.Resampling.LANCZOS)
  qa=QA/(stem+'_GREEN_OFFLINE_NOT_UNITY.jpg')
  bg.convert('RGB').save(qa,quality=93)
  output[kind]={
   'source_filename':sourcefile.name,'source_sha256':sha(binary),
   'candidate':candidate.name,'candidate_sha256':sha(candidate.read_bytes()),
   'source_dimensions':[1536,1024],'candidate_dimensions':list(sprite.size),
   'mask_sha256':sha(mask.read_bytes()),
   'separate_shadow':shadowfile.name,'shadow_sha256':sha(shadowfile.read_bytes()),
   'shadow_dimensions':list(shadow.size),
   'shadow_scale_correction':sprite.width/shadow.width,
   'preview_sha256':sha(qa.read_bytes()),'generation_id':cfg['generation_id'],
   'direct_reference_image_conditioning_proven':False,
   'background_extraction':diagnosis,
   'bottom_center_sprite_pivot':True,'original_RGB_painterly_pixels_preserved':True,
   'not_art_approved':True,'native_Unity_AB':'PENDING','iPhone_physical':'PENDING'}
 (QA/'SawmillGranaryPremium_Manifest.json').write_text(json.dumps({
  'schema':1,'source_retrieval':'Runway-hosted original generated illustration URLs; not a new paid generation',
  'processing':'Connected neutral-matte extraction, restricted roof rescue for sawmill, exact foot + half-res contact shadow',
  'status':'TWO_CANDIDATES_ARCHIVED_ART_NOT_APPROVED',
  'original_buildings_unmodified':True,'Bastion_I_V_unchanged':True,
  'entries':output},indent=2,ensure_ascii=False)+'\n')
 print('ASERRADERO_GRANERO_SOURCES_RGBA_SHADOW_PREPARED',json.dumps(output))
if __name__=='__main__':main()
