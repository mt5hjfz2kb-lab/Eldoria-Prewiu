#!/usr/bin/env python3
"""Make a separate physically modest foot shadow for premium Cuartel B.
Exact RGBA source and color painting remain untouched; not a raytraced shadow.
"""
from pathlib import Path
from PIL import Image,ImageFilter
import numpy as np,hashlib,re,json
ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
ROOTSPR=CITY/'Sprites/Candidates'
SOURCE=ROOTSPR/'Cuartel_Premium_V2.png'
DEST=ROOTSPR/'Cuartel_Premium_ShadowV2.png'
EXPECTED_SOURCE='0dcb8edb8237621f13893893edce20fec51deb1248df8bcbc3a89ea3990b4062'
EXPECTED_SHADOW='9d128fe0a7d0c229c1c3b14b098eaef17e6df79ac557ed283a3893bb4a22c5a6'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    if sha(SOURCE)!=EXPECTED_SOURCE:raise ValueError('Premium Cuartel RGBA source drift')
    im=Image.open(SOURCE).convert('RGBA')
    alpha=np.asarray(im.getchannel('A'))
    h,w=alpha.shape
    if (w,h)!=(1479,1479):raise ValueError('Wrong source dimensions')
    ys,xs=np.where(alpha>56)
    if len(ys)<50000:raise ValueError('Missing source silhouette')
    foot=int(ys.max());top=int(ys.min());low=max(0,foot-int((foot-top)*.18))
    profile=(alpha[low:foot,:]>128).mean(axis=0)
    stripe=np.uint8(np.clip(profile*255*2.1,0,255))
    blur=np.asarray(Image.fromarray(np.tile(stripe,(50,1)),'L').filter(
        ImageFilter.GaussianBlur(24)),dtype=np.float32)[25]/255.
    blur/=max(float(blur.max()),1e-6)
    y=np.arange(h,dtype=np.float32)[:,None]
    vertical=(np.exp(-.5*((y-(foot+4))/40)**2)*75+
              np.exp(-.5*((y-(foot+5))/75)**2)*26)
    edge=np.clip((h-1-y)/38,0,1)
    a=np.uint8(np.clip(vertical*blur[None,:]*edge,0,105))
    out=np.zeros((h,w,4),np.uint8)
    out[:,:,:3]=[32,28,50]
    out[:,:,3]=a
    Image.fromarray(out,'RGBA').save(DEST,optimize=True)
    if sha(DEST)!=EXPECTED_SHADOW:raise ValueError('Shadow differs from independent pixel-checked local sample')
    template=(CITY/'Sprites/Cuartel.png.meta').read_text()
    guid=hashlib.md5(str(DEST.relative_to(ROOT)).encode()).hexdigest()
    template=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,template,count=1)
    template=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',template)
    template=re.sub(r'textureCompression: \d+','textureCompression: 0',template)
    DEST.with_suffix('.png.meta').write_text(template)
    info={'schema':1,'source_sha256':EXPECTED_SOURCE,'shadow_sha256':EXPECTED_SHADOW,
          'size':[w,h],'foot_px':foot,'separate_layer':True,
          'shadow_is_illustrative_not_physical':True,'source_unchanged':True,
          'art_approved':False,'unity_AB':'PENDING','iphone':'PENDING'}
    (CITY/'QA/CuartelPremiumV2_ShadowManifest.json').write_text(json.dumps(info,indent=2)+'\n')
    print('CUARTEL_V2_SEPARATE_CONTACT_SHADOW_SHA_PASS_NOT_ART_APPROVED')
if __name__=='__main__':main()
