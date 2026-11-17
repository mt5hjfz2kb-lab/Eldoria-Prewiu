#!/usr/bin/env python3
"""Import a NEW Cuartel artwork as an isolated, source-traced premium candidate.
Does not replace any original or approve artistic quality; no gameplay edits.
The authored source has a near-uniform charcoal matte, unlike pre-existing
RGBA City2D sprites. Connected-background extraction is deliberate and local.
"""
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request,urlopen
from io import BytesIO
import hashlib,json,re,cv2,numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
DEST=CITY/'Sprites/Candidates'
QA=CITY/'QA'
REQUEST=Path(__file__).parent/'pending_cuartel_premium.json'
EXPECTED_SOURCE='644c3ffdf8429a710606fbff0d09b72671a5c8e5e18b152e4d761c821f9157ae'
EXPECTED_RESULT='1a83ed05571a4d847417df5053fb496ecfecbd6b208da41737656c9802707948'
DATASET='/datasets/1ce6ec50-03fd-4c60-b5f0-0d33cb7ddce9.png'
def sha(data):return hashlib.sha256(data).hexdigest()
def save_meta(path):
    old=(CITY/'Sprites/Cuartel.png.meta').read_text(encoding='utf-8')
    guid=hashlib.md5(str(path.relative_to(ROOT)).encode('utf-8')).hexdigest()
    old=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,old,count=1)
    old=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',old)
    old=re.sub(r'textureCompression: \d+','textureCompression: 0',old)
    path.with_suffix('.png.meta').write_text(old,encoding='utf-8')
def main():
    req=json.loads(REQUEST.read_text(encoding='utf-8'))
    assert req['schema']==1 and req['source_sha256']==EXPECTED_SOURCE
    p=urlsplit(req['url'])
    if p.scheme!='https' or p.hostname!='d2jqrm6oza8nb6.cloudfront.net' or p.path!=DATASET:
        raise ValueError('Unrecognized source URL')
    with urlopen(Request(req['url'],headers={'User-Agent':'Eldoria-CuartelSourceV2/1.0'}),timeout=90) as f:
        binary=f.read(8_000_001)
    if not 500_000<len(binary)<8_000_000 or sha(binary)!=EXPECTED_SOURCE:
        raise ValueError('Artwork source bytes do not match pre-audited source SHA')
    image=Image.open(BytesIO(binary));image.load()
    if image.format!='PNG' or image.size!=(1536,1024) or image.mode!='RGB':
        raise ValueError('Expected actual original 1536x1024 matte source')
    DEST.mkdir(parents=True,exist_ok=True);QA.mkdir(parents=True,exist_ok=True)
    source=DEST/'Cuartel_Premium_SourceV2.png';source.write_bytes(binary)
    rgb=np.asarray(image,np.uint8)
    h,w=rgb.shape[:2]
    corners=np.concatenate([rgb[:70,:70].reshape(-1,3),rgb[:70,-70:].reshape(-1,3),
                            rgb[-70:,:70].reshape(-1,3),rgb[-70:,-70:].reshape(-1,3)])
    backdrop=np.median(corners,axis=0)
    if not np.array_equal(backdrop,np.array([20,20,24])):
        raise ValueError('Uncalibrated matte; do not run a generic threshold')
    diff=np.max(abs(rgb.astype(np.int16)-backdrop.astype(np.int16)),axis=2)
    near_matte=(diff<18).astype(np.uint8)
    _,labels,_,_=cv2.connectedComponentsWithStats(near_matte,connectivity=8)
    ext=np.unique(np.r_[labels[0,:],labels[-1,:],labels[:,0],labels[:,-1]])
    foreground=(~np.isin(labels,ext)).astype(np.uint8)
    _,components,stats,_=cv2.connectedComponentsWithStats(foreground,connectivity=8)
    significant=np.where(stats[1:,cv2.CC_STAT_AREA]>=120)[0]+1
    body=np.isin(components,significant).astype(np.uint8)
    # Mask correction around source-pixel dark roof/navy cloth only. These
    # details closely match the black backdrop, and are visibly lost by
    # unconstrained chroma keying. Never modify RGB/painterly materials.
    roof_roi=np.zeros((h,w),dtype=np.uint8)
    roof_roi[305:785,850:1450]=1
    closed=cv2.morphologyEx(body,cv2.MORPH_CLOSE,
        cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(37,37)))
    rescue=(closed>0)&(body==0)&(diff>7)&(roof_roi>0)
    restored=int(rescue.sum());body[rescue]=1
    alpha=cv2.GaussianBlur(body.astype(np.float32),(3,3),.65)
    alpha[(~np.isin(labels,ext)) & (body>0)]=np.maximum(
        alpha[(~np.isin(labels,ext)) & (body>0)],.985)
    rgba=np.dstack((rgb,(np.clip(alpha,0,1)*255).astype(np.uint8)))
    rgba[rgba[:,:,3]==0,:3]=0
    base=Image.fromarray(rgba,'RGBA')
    bbox=base.getchannel('A').getbbox()
    if bbox is None or bbox[2]-bbox[0]<900 or bbox[3]-bbox[1]<700:
        raise ValueError('Premium Cuartel silhouette lost')
    margin=40
    crop=base.crop((max(0,bbox[0]-margin),max(0,bbox[1]-margin),
                    min(w,bbox[2]+margin),min(h,bbox[3]+margin)))
    side=max(crop.size)+80
    output=Image.new('RGBA',(side,side),(0,0,0,0))
    output.alpha_composite(crop,((side-crop.width)//2,side-crop.height-45))
    target=DEST/'Cuartel_Premium_V2.png'
    output.save(target,optimize=True)
    if sha(target.read_bytes())!=EXPECTED_RESULT or output.size!=(1479,1479):
        raise ValueError('Image alpha/crop differs from locally inspected candidate')
    mask=DEST/'Cuartel_Premium_MaskV2.png'
    output.getchannel('A').save(mask,optimize=True)
    for path in (source,target,mask):save_meta(path)
    layers={}
    for name,color in [('grey',(45,57,70,255)),('green',(85,110,73,255))]:
        canvas=Image.new('RGBA',(side,side),color);canvas.alpha_composite(output)
        canvas.thumbnail((680,680),Image.Resampling.LANCZOS)
        out=QA/('CuartelPremiumV2_'+name+'_OFFLINE_NOT_UNITY.jpg')
        canvas.convert('RGB').save(out,quality=91)
        layers[name]={'file':str(out.relative_to(ROOT)),'sha256':sha(out.read_bytes())}
    info={'schema':2,'state':'ORIGINAL_AND_RGBA_ARCHIVED_NOT_ART_APPROVED',
        'asset':'Cuartel_Premium_V2','source_generation_id':'58988476-fb28-45a1-afe2-ca0cc81e48d4',
        'hosted_image_asset_id':'825a691e-090f-48d1-a9f2-70c375aef87e',
        'generator':'first_party_image_generation','direct_master_image_conditioning_proven':False,
        'source_original_sha256':EXPECTED_SOURCE,'source_original_dimensions':[1536,1024],
        'candidate_sha256':EXPECTED_RESULT,'candidate_dimensions':[side,side],
        'source_alpha_extracted':'connected neutral matte; scoped roof/flag repair of %d source pixels'%restored,
        'source_rgb_unchanged':True,'mask_sha256':sha(mask.read_bytes()),
        'alpha_extrema':list(output.getchannel('A').getextrema()),
        'crop_rect':list(bbox),'bottom_center_pivot':True,
        'preview_images':layers,
        'legacy_cuartel_unchanged':True,'forja_v3_unchanged':True,'bastion_v_unchanged':True,
        'no_pedestal_rectangle_in_new_original':True,'native_unity_capture':'PENDING',
        'contact_shadow_independent':'PENDING','art_approved':False,
        'physical_iphone':'PENDING'}
    (QA/'cuartel_premium_v2_manifest.json').write_text(json.dumps(info,indent=2,ensure_ascii=False)+'\n')
    print('CUARTEL_PREMIUM_V2_EXACT_SOURCE_AND_ALPHA_PASS_NOT_ART_APPROVED',EXPECTED_RESULT)
if __name__=='__main__':main()
