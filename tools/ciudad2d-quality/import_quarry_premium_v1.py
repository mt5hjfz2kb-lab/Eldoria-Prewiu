#!/usr/bin/env python3
"""Archive one REAL source illustration of Valoria quarry, its transparent RGBA
cutout and independent contact shadow without touching original City2D artwork.

This is NOT a source-quality pass, Unity screenshot or device test. Alpha errors
must be reviewed over multiple backgrounds before art can be approved.
"""
from pathlib import Path
from urllib.parse import urlsplit
from urllib.request import Request,urlopen
from io import BytesIO
import hashlib,json,re
import numpy as np,cv2
from PIL import Image, ImageDraw, ImageFilter

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
DEST=CITY/'Sprites/Candidates'
QA=CITY/'QA'
INPUT=Path(__file__).parent/'pending_quarry_premium.json'
HOST='d2jqrm6oza8nb6.cloudfront.net'
DATASET='/datasets/dfa4dca0-ea9d-44b2-8dad-ce27d763a3da.png'
GEN='d3717480-cc51-4acc-88c0-58a64a64ef47'
ASSET='5e88bd34-1f54-46aa-8b38-2b0a95630245'
STEM='Cantera_Premium_V1'

def digest(data): return hashlib.sha256(data).hexdigest()
def verify_download(url):
    parts=urlsplit(url)
    if parts.scheme!='https' or parts.hostname!=HOST or parts.path!=DATASET:
        raise ValueError('Unrecognized immutable quarry source path')
    with urlopen(Request(url,headers={'User-Agent':'EldoriaQuarryPremiumQA/1.0'}), timeout=90) as remote:
        raw=remote.read(9_000_001)
    if not 600_000<len(raw)<9_000_000:
        raise ValueError('Wrong source byte size')
    pic=Image.open(BytesIO(raw)); pic.load()
    if pic.format!='PNG' or pic.size!=(1536,1024) or pic.mode not in ('RGB','RGBA'):
        raise ValueError('Expected standalone quarry PNG 1536x1024')
    return raw,pic.convert('RGB')
def write_meta(path):
    template=(CITY/'Sprites/Cantera.png.meta').read_text()
    guid=hashlib.md5(str(path.relative_to(ROOT)).encode()).hexdigest()
    template=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,template,count=1)
    template=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',template)
    template=re.sub(r'textureCompression: \d+','textureCompression: 0',template)
    path.with_suffix('.png.meta').write_text(template)
def isolate(image):
    pixels=np.asarray(image,np.uint8)
    h,w=pixels.shape[:2]
    corners=np.concatenate([pixels[:70,:70].reshape(-1,3),
                            pixels[:70,-70:].reshape(-1,3),
                            pixels[-70:,:70].reshape(-1,3),
                            pixels[-70,-70:].reshape(1,3)])
    # Preserve actual corner RGB as a recorded, source-dependent estimate,
    # not invented black or a global hue/saturation transform.
    bg=np.median(corners,axis=0)
    if float(bg.max())>60 or float(bg.min())<3:
        raise ValueError('Quarry original is not a near-charcoal isolated building')
    dist=np.max(np.abs(pixels.astype(np.int16)-bg[None,None,:]),axis=2)
    matte=(dist<19).astype(np.uint8)
    _,label,_,_=cv2.connectedComponentsWithStats(matte,connectivity=8)
    boundary_labels=np.unique(np.r_[label[0,:],label[-1,:],label[:,0],label[:,-1]])
    initial=(~np.isin(label,boundary_labels)).astype(np.uint8)
    _,parts,stats,_=cv2.connectedComponentsWithStats(initial,connectivity=8)
    keep=np.where(stats[:,cv2.CC_STAT_AREA]>60)[0]
    keep=keep[keep!=0]
    silhouette=np.isin(parts,keep).astype(np.uint8)
    # Close small seams in masonry/roof without bridging the crane gap.
    smooth=cv2.morphologyEx(silhouette,cv2.MORPH_CLOSE,
                            cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(7,7)))
    silhouette=np.maximum(silhouette,smooth)
    # Roof and masonry only: preserve dark slates that share charcoal color.
    strong=cv2.morphologyEx(silhouette,cv2.MORPH_CLOSE,
                            cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(23,23)))
    Y,X=np.indices((h,w))
    safe=(X<800)&(Y>170)&(Y<815)&(strong>0)&(silhouette==0)&(dist>7)
    rescued=int(safe.sum())
    silhouette[safe]=1
    alpha=cv2.GaussianBlur(silhouette.astype(np.float32),(3,3),.6)
    alpha=np.clip(alpha,0,1)
    # Remove near-black source matte contamination only on antialias edges.
    rgb=pixels.astype(np.float32)
    fringe=(alpha>.035)&(alpha<.96)
    colour=rgb.copy()
    colour[fringe]=np.clip((rgb[fringe]-bg[None,:]*(1-alpha[fringe,None]))/
                            np.maximum(.09,alpha[fringe,None]),0,255)
    output=np.dstack([colour.astype(np.uint8),np.round(alpha*255).astype(np.uint8)])
    output[alpha<.005,:3]=0
    sprite=Image.fromarray(output,'RGBA')
    bbox=sprite.getchannel('A').getbbox()
    if bbox is None or bbox[2]-bbox[0]<1000 or bbox[3]-bbox[1]<600:
        raise ValueError('Quarry silhouette lost architecture or crane')
    margin=34
    cut=sprite.crop((max(0,bbox[0]-margin),max(0,bbox[1]-margin),
                     min(w,bbox[2]+margin),min(h,bbox[3]+margin)))
    side=max(cut.size)+76
    if side>2048:raise ValueError('Too large for mobile candidate')
    square=Image.new('RGBA',(side,side),(0,0,0,0))
    square.alpha_composite(cut,((side-cut.width)//2, side-cut.height-42))
    a=square.getchannel('A')
    if a.getextrema()[0]!=0 or a.getextrema()[1]<235:
        raise ValueError('Quarry cutout alpha invalid')
    if max(a.getpixel(p) for p in ((0,0),(side-1,0),(0,side-1),(side-1,side-1)))>0:
        raise ValueError('Opaque background/platform at corner')
    return square,{'source_background_rgb':[float(v) for v in bg],
      'source_bbox':list(bbox),'rescued_dark_roof_pixels':rescued,
      'maximum_alpha':a.getextrema()[1],'side':side,
      'method':'border-connected near-charcoal removal, 7px close + roof-only 23px restoration; no global RGB filters'}
def contact_shadow(image):
    alpha=np.asarray(image.getchannel('A'))
    n=alpha.shape[0]
    ys,xs=np.where(alpha>85)
    if len(ys)<50000:raise ValueError('Lacking authored building area')
    top,bottom=int(ys.min()),int(ys.max())
    low=max(top,bottom-int((bottom-top)*.16))
    weights=(alpha[low:bottom]>120).mean(axis=0)
    stripe=np.clip(weights*255*2.0,0,255).astype(np.uint8)
    stripe=np.asarray(Image.fromarray(np.tile(stripe,(58,1)),'L')
        .filter(ImageFilter.GaussianBlur(24)),dtype=np.float32)[29]/255
    stripe/=max(stripe.max(),.001)
    yy=np.arange(n,dtype=np.float32)[:,None]
    fall=(72*np.exp(-.5*((yy-(bottom+5))/36)**2)+
          25*np.exp(-.5*((yy-(bottom+2))/68)**2))
    edge=np.clip((n-1-yy)/27,0,1)
    rgba=np.zeros((n,n,4),dtype=np.uint8)
    rgba[:,:,:3]=[34,29,52]
    rgba[:,:,3]=np.uint8(np.clip(fall*stripe[None,:]*edge,0,104))
    return Image.fromarray(rgba,'RGBA').resize(((n+1)//2,(n+1)//2),
        Image.Resampling.LANCZOS),bottom
def main():
    task=json.loads(INPUT.read_text())
    if task.get('schema')!=1 or task.get('source_asset_id')!=ASSET:
        raise ValueError('Unknown one-time quarry source')
    binary,image=verify_download(task['url'])
    DEST.mkdir(parents=True,exist_ok=True);QA.mkdir(parents=True,exist_ok=True)
    source=DEST/(STEM+'_Source.png')
    source.write_bytes(binary); write_meta(source)
    result,seg=isolate(image)
    target=DEST/(STEM+'.png')
    result.save(target,optimize=True);write_meta(target)
    mask=DEST/(STEM+'_Mask.png')
    result.getchannel('A').save(mask,optimize=True);write_meta(mask)
    shade,foot=contact_shadow(result)
    shadow=DEST/(STEM+'_Shadow.png')
    shade.save(shadow,optimize=True);write_meta(shadow)
    previews={}
    for name,color in [('black',(21,22,27,255)),('grey',(113,116,116,255)),
                        ('green',(82,108,71,255))]:
        board=Image.new('RGBA',result.size,color)
        board.alpha_composite(shade.resize(result.size,Image.Resampling.BILINEAR))
        board.alpha_composite(result)
        board.thumbnail((760,760),Image.Resampling.LANCZOS)
        path=QA/(STEM+'_'+name+'_OFFLINE_NOT_UNITY.jpg')
        board.convert('RGB').save(path,quality=94)
        previews[name]=digest(path.read_bytes())
    # Also test against the actual textured ground; do not claim this is Unity.
    terrain=Image.open(CITY/'Sprites/Terreno_Organico.png').convert('RGBA')
    section=terrain.crop((155,375,786,1006)).resize((760,760),Image.Resampling.LANCZOS)
    res=result.resize((680,680),Image.Resampling.LANCZOS)
    sh=shade.resize((680,680),Image.Resampling.LANCZOS)
    section.alpha_composite(sh,(40,40));section.alpha_composite(res,(40,40))
    terrain_path=QA/(STEM+'_terrain_OFFLINE_NOT_UNITY.jpg')
    section.convert('RGB').save(terrain_path,quality=94)
    previews['real_ground_offline_sha256']=digest(terrain_path.read_bytes())
    art={
       'schema':1,'asset_id':'cantera','status':'QUARANTINED_PREMIUM_SOURCE_ART_NOT_APPROVED',
       'model':'ChatGPT image generation; exact model seed not disclosed',
       'generation_id':GEN,'hosted_source_asset_id':ASSET,
       'actual_master_ref_image_conditioning_verified':False,
       'master_visual_standard':'ArtBible/Masters/Bastion_V_MASTER.png',
       'source_original_sha256':digest(binary),'source_dimensions':[1536,1024],
       'source':source.name,'candidate':target.name,
       'candidate_sha256':digest(target.read_bytes()),'dimensions':list(result.size),
       'mask':mask.name,'mask_sha256':digest(mask.read_bytes()),
       'separate_contact_shadow':shadow.name,'shadow_sha256':digest(shadow.read_bytes()),
       'shadow_dimensions':list(shade.size),'shadow_scale_correction':result.width/shade.width,
       'source_foot_original_image_px':foot,'segmentation':seg,
       'previews':previews,
       'known_art_caveat':'Raised geological bench is architecturally justified as an excavation; verify no large square tile in Unity. Crane ropes and smoke require close alpha QA.',
       'city_original_unchanged':True,'physical_iphone':'NOT_TESTED',
       'native_unity':'PENDING','art_approved':False}
    (QA/(STEM+'_Manifest.json')).write_text(json.dumps(art,indent=2,ensure_ascii=False)+'\n')
    print('CAN TERA_PREMIUM_SOURCE_RGBA_SHADOW_ARCHIVED_NO_UNITY_OR_ART_APPROVAL',art['source_original_sha256'],art['candidate_sha256'])
if __name__=='__main__':main()
