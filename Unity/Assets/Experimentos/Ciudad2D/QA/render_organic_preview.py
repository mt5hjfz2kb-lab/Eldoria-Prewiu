#!/usr/bin/env python3
"""Reproducible scene composition from the same layout consumed by Unity/browser.
These images are Pillow offline composites, NEVER Unity screenshots.
"""
import json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'QA'
LAYOUT=json.loads((OUT/'city_layout.json').read_text(encoding='utf-8'))
SPRITES=ROOT/'Sprites'

def render(level:int):
    items=[]
    for b in LAYOUT['buildings']:
        filename=b['level5_filename'] if b.get('level5_filename') and level>=5 else b['filename']
        scale=b['level5_scale'] if b.get('level5_filename') and level>=5 else b['scale']
        items.append((filename,b['x'],b['y'],scale))
    for d in LAYOUT['decor']:
        items.append((d['filename'],d['x'],d['y'],d['scale']))
    out=Image.open(SPRITES/LAYOUT.get('terrain_file','Terreno.png')).convert('RGBA')
    for filename,x,y,scale in sorted(items,key=lambda row:row[2]):
        im=Image.open(SPRITES/filename).convert('RGBA')
        w,h=round(im.width*scale),round(im.height*scale)
        im=im.resize((w,h),Image.Resampling.LANCZOS)
        out.alpha_composite(im,(round(x-w/2),round(y-h)))
    # Unity uses a SpriteRenderer color that keeps glow alpha modest.
    sprite=Image.open(SPRITES/'Props/HaloAntorcha.png').convert('RGBA')
    for fx in LAYOUT['glows']:
        d=round(fx['radius']*2)
        halo=sprite.resize((d,d),Image.Resampling.BILINEAR)
        halo.putalpha(halo.getchannel('A').point(lambda a:round(a*fx['opacity'])))
        out.alpha_composite(halo,(round(fx['x']-d/2),round(fx['y']-d/2)))
    return out.convert('RGB')

if __name__=='__main__':
    a=render(1);b=render(5)
    a.save(OUT/'organic-I.png',optimize=True)
    b.save(OUT/'organic-V.png',optimize=True)
    ratio=.70;w=round(a.width*ratio);h=round(a.height*ratio)
    board=Image.new('RGB',(w*2+20,h),(18,23,30))
    board.paste(a.resize((w,h),Image.Resampling.LANCZOS),(0,0))
    board.paste(b.resize((w,h),Image.Resampling.LANCZOS),(w+20,0))
    board.save(OUT/'organic-I-V.png',optimize=True)
    print('OFFLINE_ART_COMPOSITE:',OUT/'organic-I-V.png',a.size,b.size)
