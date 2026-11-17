#!/usr/bin/env python3
"""Deterministic lightweight atmospheric wisps for the isolated Valoria 2.5D city.

These are deliberately NOT new 3D assets and not a replacement for painted
smoke in existing buildings: only tiny, subtle animated overdubs.
All source bytes/version info stay in the branch for reproducibility.
"""
from pathlib import Path
import hashlib, json, re
import numpy as np
from PIL import Image, ImageFilter

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
OUTPUT=CITY/'Sprites/Props'
QA=CITY/'QA'
W,H=160,240

def sha(file):
    return hashlib.sha256(file.read_bytes()).hexdigest()

def make_wisp(index):
    rng=np.random.default_rng(37+index*83)
    Y,X=np.mgrid[0:H,0:W].astype(np.float32)
    u=X/W
    v=Y/H
    components=[
        (.48+index*.03,.83,.19,.11,.70),
        (.55+index*.05,.63,.23,.15,1.0),
        (.44-index*.05,.46,.21,.13,.75),
        (.57,.30,.24,.16,.62),
        (.49,.14,.17,.09,.26)
    ]
    shape=np.zeros((H,W),np.float32)
    for cx,cy,sx,sy,weight in components:
        d=((u-cx)/sx)**2+((v-cy)/sy)**2
        shape=1-(1-shape)*(1-weight*np.exp(-.5*d))
    noise=np.zeros((H,W),np.float32)
    for gw,gh,amp in [(4,6,.23),(10,15,.19),(32,48,.075)]:
        n=(rng.random((gh,gw))*255).astype(np.uint8)
        n=np.asarray(Image.fromarray(n,'L').resize((W,H),Image.Resampling.BICUBIC),dtype=np.float32)/255
        noise+=(n-.5)*amp
    beta=np.clip(shape*(.86+noise)-.06,0,1)
    alpha=np.uint8(np.clip(beta*132,0,125))
    rgba=np.zeros((H,W,4),dtype=np.uint8)
    rgba[:,:,:3]=[191,194,202] # thin grey/lavender, not new light
    rgba[:,:,3]=alpha
    return Image.fromarray(rgba,'RGBA').filter(ImageFilter.GaussianBlur(1.25))

def main():
    settings=json.loads((QA/'ambient_life_v1.json').read_text())
    assert settings['schema']==1 and len(settings['effects'])==10
    OUTPUT.mkdir(parents=True,exist_ok=True)
    template=(OUTPUT/'HaloAntorcha.png.meta').read_text()
    assets=[]
    for i,letter in enumerate(('A','B')):
        output=OUTPUT/('Humo_Valoria_'+letter+'.png')
        sprite=make_wisp(i)
        lo,hi=sprite.getchannel('A').getextrema()
        assert 0<=lo<=5 and hi<=130 and hi>=55
        sprite.save(output,optimize=True)
        meta=template
        guid=hashlib.md5(str(output.relative_to(ROOT)).encode()).hexdigest()
        meta=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,meta,count=1)
        # BottomCenter: authored plume base rests at chimney foot.
        meta=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',meta)
        meta=re.sub(r'(?m)^  spritePivot: .*','  spritePivot: {x: 0.5, y: 0}',meta)
        meta=re.sub(r'textureCompression: \d+','textureCompression: 0',meta)
        output.with_suffix('.png.meta').write_text(meta,encoding='utf-8')
        assets.append({'name':output.name,'sha256':sha(output),
            'dimensions':[W,H],'alpha_min':lo,'alpha_max':hi,
            'seed':37+i*83,'script':'tools/ciudad2d-ambient/build_ambient_sprites.py'})
    report={'schema':1,'state':'TECHNICAL_SOURCE_PREP_ART_REVIEW_PENDING',
        'assets':assets,'all_original_city_sprites_unchanged':True,
        'master_images_untouched':True,'unity_capture':'PENDING',
        'playmode':'PENDING','device':'PENDING',
        'no_ai_generation_credits_used':True,
        'source_technique':'fixed Gaussian wisps with seeded low-frequency noise; alpha blur',
        'notes':'Subtle overlay at chimney coordinates; standalone sprites cannot replace original painted chimney smoke'}
    (QA/'ambient_art_manifest_v1.json').write_text(
        json.dumps(report,indent=2,ensure_ascii=False)+'\n')
    print('VALORIA_LIFE_2_SOURCE_SPRITES_HASHED_NOT_ART_APPROVED',
          [(a['name'],a['sha256']) for a in assets])

if __name__=='__main__':
    main()
