#!/usr/bin/env python3
"""Reproducible sub-50px ember sprite to replace invisible old halo-only sparks.
Small source, no external image generation, no dynamic lights or particles.
"""
from pathlib import Path
import hashlib,json,re,numpy as np
from PIL import Image,ImageFilter
ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
OUT=CITY/'Sprites/Props/Chispa_Valoria_V2.png'
EXPECTED='ff8e034f20932b657fef9755d2caade53b38a951f96954c06a39605985a95422'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    W,H=48,72
    y,x=np.mgrid[0:H,0:W].astype(np.float32)
    u=x/W;v=y/H
    a=np.exp(-.5*((u-.48)/.15)**2-.5*((v-.71)/.17)**2)*.72
    b=np.exp(-.5*((u-.515)/.11)**2-.5*((v-.57)/.25)**2)*.45
    m=np.clip(a+b,0,1)
    pixels=np.zeros((H,W,4),dtype=np.uint8)
    pixels[:,:,0]=255
    pixels[:,:,1]=np.uint8(140+72*np.clip(.95-v,0,1))
    pixels[:,:,2]=np.uint8(58+73*np.clip(.87-v,0,1))
    pixels[:,:,3]=np.uint8(np.clip(m*235,0,238))
    image=Image.fromarray(pixels,'RGBA').filter(ImageFilter.GaussianBlur(.5))
    OUT.parent.mkdir(parents=True,exist_ok=True)
    image.save(OUT,optimize=True)
    if sha(OUT)!=EXPECTED:raise ValueError('Deterministic ember sprite differs from inspected prototype')
    template=(CITY/'Sprites/Props/HaloAntorcha.png.meta').read_text()
    guid=hashlib.md5(str(OUT.relative_to(ROOT)).encode()).hexdigest()
    template=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,template,count=1)
    template=re.sub(r'textureCompression: \d+','textureCompression: 0',template)
    OUT.with_suffix('.png.meta').write_text(template)
    info={'schema':2,'asset':str(OUT.relative_to(ROOT)),'sha256':EXPECTED,
         'size':[W,H],'alpha_extrema':list(image.getchannel('A').getextrema()),
         'original_HaloAntorcha_unchanged':True,'motion_script_unchanged':True,
         'asset_art_approved':False,'unity_capture':'PENDING','physical_iPhone':'PENDING'}
    (CITY/'QA/ambient_ember_v2_manifest.json').write_text(json.dumps(info,indent=2)+'\n')
    print('CITY2D_SOURCE_ONLY_NEW_EMBER_SHA_PASS',EXPECTED)
if __name__=='__main__':main()
