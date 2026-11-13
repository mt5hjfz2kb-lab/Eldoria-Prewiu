#!/usr/bin/env python3
"""Inspect newly generated source art, without modifying Unity gameplay or HUD."""
import hashlib,json,pathlib,urllib.parse,urllib.request,io
from PIL import Image,ImageOps,ImageDraw,ImageFont
ROOT=pathlib.Path(__file__).parent
REQUEST=ROOT/'fresh-art-sources.json'
OUT=ROOT/'fresh-art-inbox'
OUT.mkdir(exist_ok=True)
sources=json.loads(REQUEST.read_text())
assert set(sources)=={'panels','medallions','resources'}
cards=[];meta={}
for name,source in sources.items():
    url=source['url']
    u=urllib.parse.urlparse(url)
    assert u.scheme=='https' and u.netloc=='dnznrvs05pmza.cloudfront.net',f'Unknown art host for {name}'
    assert u.path.endswith('.png') and source['task_id'] in u.path
    request=urllib.request.Request(url,headers={'User-Agent':'Eldoria-art-production/1.0'})
    with urllib.request.urlopen(request,timeout=70) as f:
        data=f.read(22_000_000)
    assert 60_000<len(data)<22_000_000,f'Invalid source file {name}'
    image=Image.open(io.BytesIO(data))
    assert image.format=='PNG' and image.width>=1000 and image.height>=750
    image.load()
    target=OUT/(name+'.png')
    target.write_bytes(data)
    meta[name]={'task_id':source['task_id'],'size':[image.width,image.height],
                'sha256':hashlib.sha256(data).hexdigest(),'source':'Runway-generated from owner-approved screenshot; independent new asset atlas'}
    img=ImageOps.contain(image.convert('RGB'),(760,510))
    board=Image.new('RGB',(800,560),(30,30,34))
    board.paste(img,((800-img.width)//2,40+(510-img.height)//2))
    ImageDraw.Draw(board).text((14,12),name+' / '+str(image.width)+'x'+str(image.height),fill=(234,220,180))
    cards.append(board)
contact=Image.new('RGB',(800*len(cards),560),(22,22,24))
for i,card in enumerate(cards):contact.paste(card,(800*i,0))
contact.save(OUT/'source-contact-sheet.jpg',quality=86,subsampling=0)
(OUT/'manifest.json').write_text(json.dumps(meta,indent=2))
print('VIGILIA_FRESH_GENERATED_ATLAS_FETCHED',len(cards),'no Unity files touched')
