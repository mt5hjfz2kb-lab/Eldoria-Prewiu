#!/usr/bin/env python3
"""Asset acceptance checks; render authenticity comes from Blender CLI log."""
from PIL import Image
from pathlib import Path
import argparse,hashlib,json
p=argparse.ArgumentParser();p.add_argument('--dir',required=True,type=Path);a=p.parse_args()
root=a.dir
data=json.loads((root/'blender-pilot-manifest.json').read_text())
assert data['art_approved'] is False
assert set(data['buildings'])=={'Cuartel','Aserradero'}
assert len(data['pieces'])>=6, 'Actual kit reuse missing'
assert (root/'Valoria_Buildings_Master.blend').read_bytes().startswith(b'BLENDER')
assert hashlib.sha256((root/'Valoria_Buildings_Master.blend').read_bytes()).hexdigest()==data['master_sha256']
rows={}
for name in ('Cuartel','Aserradero'):
    row=data['buildings'][name]
    assert row['mesh_objects']>=10
    for kind,key in [('Color','color_sha256'),('ContactShadow','shadow_sha256')]:
        p=root/(name+'_'+kind+'.png')
        assert p.is_file() and hashlib.sha256(p.read_bytes()).hexdigest()==row[key]
        with Image.open(p) as im:
            assert im.size==(data['camera']['image_size'],)*2,im.size
            assert im.mode=='RGBA',im.mode
            a=im.getchannel('A')
            box=a.getbbox()
            assert box is not None and box[2]-box[0]>120 and box[3]-box[1]>80,(name,kind,box)
            # Shadow catcher must have actual alpha, not fake opaque floor.
            opaque=sum(1 for v in a.getdata() if v>=250)
            partial=sum(1 for v in a.getdata() if 0<v<250)
            assert partial>2000 or (kind=='Color' and opaque>10000),(name,kind,'no visible content',partial,opaque)
            assert a.getpixel((0,0))<10,(name,kind,'opaque rectangular background')
            rows[name+'_'+kind]={'bbox':box,'partial':partial,'opaque':opaque}
print('BLENDER_PILOT_COLOR_ALPHA_SHADOW_PASS',json.dumps(rows))
