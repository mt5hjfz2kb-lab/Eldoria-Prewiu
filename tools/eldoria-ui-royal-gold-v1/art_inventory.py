"""Inspect the actual recovered PNG bytes. This is not a Unity render or art approval."""
from pathlib import Path
import base64, hashlib, json, os
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parents[2]
source=root/'tools/eldoria-ui-royal-gold-v1/recovered'
out=root/'docs/ui-royal-gold-v1';out.mkdir(parents=True,exist_ok=True)
files=sorted(source.glob('*.png'));assert len(files)==20,'Expected the twenty preserved blobs'
canvas=Image.new('RGB',(400,360),(17,23,34));draw=ImageDraw.Draw(canvas);items=[]
for i,p in enumerate(files):
 raw=p.read_bytes();sha=hashlib.sha1(b'blob '+str(len(raw)).encode()+b'\0'+raw).hexdigest();assert sha==p.stem
 im=Image.open(p).convert('RGBA');item={'index':i,'blob':sha,'size':list(im.size),'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest()};items.append(item)
 im.thumbnail((72, sixty:=64),Image.Resampling.LANCZOS);x=(i%5)*80+(80-im.width)//2;y=(i//5)*90
 canvas.paste(im,(x,y),im);draw.text(((i%5)*80+4,y+66),str(i)+' '+sha[:5],fill=(224,205,160))
canvas.save(out/'recovered-art-contact.png')
small=canvas.quantize(colors=24);small.save(out/'recovered-art-contact-small.png',optimize=True)
raw=(out/'recovered-art-contact-small.png').read_bytes();enc=base64.b64encode(raw).decode();(out/'recovered-art-contact.base64.txt').write_text('\n'.join(enc[i:i+100] for i in range(0,len(enc),100)))
(out/'recovered-art.json').write_text(json.dumps({'source_sha':os.environ.get('GITHUB_SHA'),'run_id':os.environ.get('GITHUB_RUN_ID'),'native_unity':False,'items':items},indent=2))
print('RECOVERED_PNG_INTEGRITY_PASS',len(items),'contact_b64_length',len(enc))
