#!/usr/bin/env python3
"""Offline PIL montage; never attribute these images to Unity screenshots."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).resolve().parents[1]
p=root/'Sprites'
placements=[
 ('Cuartel.png',(206,547),.189*1254/860),
 ('SalonHeroes.png',(719,546),.195*1254/860),
 ('Aserradero.png',(172,964),.213*1254/860),
 ('Granero.png',(751,962),.211*1254/860),
 ('Cantera.png',(470,1178),.225*1254/860),
]

def city(level):
    canvas=Image.open(p/'Terreno.png').convert('RGBA')
    art='Bastion_Nivel05.png' if level>=5 else 'Bastion_Nivel01.png'
    sprites=placements+[(art,(470,824),0.47 if level>=5 else 0.43)]
    for name,(x,y),scale in sorted(sprites,key=lambda v:v[1][1]):
        im=Image.open(p/name).convert('RGBA')
        w,h=round(im.width*scale),round(im.height*scale)
        im=im.resize((w,h),Image.Resampling.LANCZOS)
        canvas.alpha_composite(im,(round(x-w/2),round(y-h)))
    return canvas.convert('RGB')

if __name__=='__main__':
    one, five=city(1),city(5)
    five.save(root/'QA'/'portrait-hito-V.png',optimize=True)
    # Evidencia lado a lado, baja resolución suficiente para comparar en GitHub.
    factor=0.64
    one=one.resize((round(941*factor),round(1672*factor)),Image.Resampling.LANCZOS)
    five=five.resize(one.size,Image.Resampling.LANCZOS)
    combined=Image.new('RGB',(one.width*2+24,one.height),(23,27,35))
    combined.paste(one,(0,0));combined.paste(five,(one.width+24,0))
    combined.save(root/'QA'/'preview-I-V.png',optimize=True)
    # Horizontal: width covers terrain, camera pan centers 170px above terrain center.
    level5=city(5)
    crop_height=round(941*1.04/(844/390))
    center_y=round(1672/2-170)
    half=crop_height//2
    left,right=0,941
    frame=level5.crop((left,center_y-half,right,center_y-half+crop_height))
    # Al ser más ancho el viewport que el terreno, 4% de margen sutil de ambiente.
    margin=round((941*1.04-941)/2)
    out=Image.new('RGB',(941+margin*2,crop_height),(29,40,49))
    out.paste(frame,(margin,0))
    out=out.resize((844,390),Image.Resampling.LANCZOS)
    out.save(root/'QA'/'landscape-hito-V-844x390.png',optimize=True)
    print('saved',[(f.name,f.stat().st_size) for f in (root/'QA').glob('*.png')])
