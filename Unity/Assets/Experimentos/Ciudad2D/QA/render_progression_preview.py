#!/usr/bin/env python3
"""Illustrative chapter snapshots from the exact shared layout; never Unity proof."""
import json
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'QA'; ART=ROOT/'Sprites'; L=json.loads((QA/'city_layout.json').read_text())
LIMITS={'aserradero':1,'cuartel':2,'granero':3,'cantera':5,'bastion':1,'salon':9999}
DONE={'aserradero':2,'cuartel':3,'granero':4,'cantera':9999,'salon':9999}

def status(stage,bid):
    if stage < LIMITS[bid]:return 'locked'
    if bid=='bastion':return 'built'
    return 'built' if stage >= DONE[bid] else 'available'

def build(stage):
    result=Image.open(ART/L.get('terrain_file','Terreno.png')).convert('RGBA')
    rows=[]
    for e in L['buildings']:
        phase=status(stage,e['id'])
        if phase!='built':continue
        name=e.get('level5_filename') if stage>=5 and e.get('level5_filename') else e['filename']
        scale=e.get('level5_scale',0) if stage>=5 and e.get('level5_filename') else e['scale']
        rows.append((e['y'],e['x'],name,scale))
    for e in L['decor']:
        if stage>=e['min_bastion'] and (not e.get('max_bastion') or stage<=e['max_bastion']):
            rows.append((e['y'],e['x'],e['filename'],e['scale']))
    for y,x,name,scale in sorted(rows):
        im=Image.open(ART/name).convert('RGBA')
        w,h=round(im.width*scale),round(im.height*scale)
        im=im.resize((w,h),Image.Resampling.LANCZOS)
        result.alpha_composite(im,(round(x-w/2),round(y-h)))
    marker=ImageDraw.Draw(result,'RGBA')
    for e in L['buildings']:
        if status(stage,e['id'])!='available':continue
        x,y=e['x'],e['y']-8
        marker.ellipse((x-40,y-18,x+40,y+18),fill=(95,59,18,45),outline=(225,179,98,180),width=2)
    return result.convert('RGB')
if __name__=='__main__':
    for level in range(1,6):
        im=build(level)
        im.save(QA/f'progresion-bastion-{level}.png',optimize=True)
        print(level,{b['id']:status(level,b['id']) for b in L['buildings']})
    a=build(1).resize((470,836),Image.Resampling.LANCZOS)
    b=build(3).resize((470,836),Image.Resampling.LANCZOS)
    c=build(5).resize((470,836),Image.Resampling.LANCZOS)
    out=Image.new('RGB',(470*3+24,836),(19,26,32))
    out.paste(a,(0,0));out.paste(b,(482,0));out.paste(c,(964,0))
    out.save(QA/'progresion-I-III-V.png',optimize=True)
