"""Evidence board from actual GLB reimport renders, without image substitutes."""
import sys
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'art-source/valoria/production/asset-family-v2/quality-v3'
tiles=[('Puerta V3 · mínimo congelado',ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3/after-hero-matched.png')]
labels={'wall_straight':'Muro recto · 8 m','watchtower':'Torre completa V3','wall_corner':'Esquina · 90°','wall_connections':'Unión · 2 m','stone_path_platform':'Camino · 4 × 8 m','cliff_rocks':'Acantilado · 12 m','defensive_props':'Estandartes, braseros y defensas'}
tiles += [(label,BASE/name/'roundtrip-hero.png') for name,label in labels.items()]
board=Image.new('RGB',(2200,976),(25,31,40));draw=ImageDraw.Draw(board)
try:font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',22)
except OSError:font=ImageFont.load_default()
for i,(label,path) in enumerate(tiles):
    x=(i%4)*550;y=(i//4)*488
    with Image.open(path) as im:
        im=ImageOps.contain(im.convert('RGB'),(534,440));board.paste(im,(x+8+(534-im.width)//2,y+8+(440-im.height)//2))
    draw.text((x+12,y+454),label,font=font,fill=(233,238,247))
output=Path(sys.argv[1]).resolve();output.parent.mkdir(parents=True,exist_ok=True);board.save(output)
print(output)
