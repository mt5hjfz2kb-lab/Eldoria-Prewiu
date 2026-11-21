"""Measure server download, decoded player entries and their largest Unity objects."""
import gzip, json, shutil, struct, sys
from pathlib import Path
import UnityPy
from collections import Counter
src=next(Path('player/Build').glob('*.data.unityweb'))
raw=Path('regional-decoded.data')
with gzip.open(src,'rb') as z,raw.open('wb') as out:shutil.copyfileobj(z,out,4*1024**2)
report={'download':src.stat().st_size,'decoded':raw.stat().st_size,'entries':[],'largest':[]}
def save():Path('world-browser-evidence/payload.json').write_text(json.dumps(report,indent=2))
with raw.open('rb') as f:
 magic=f.read(16)
 if magic!=b'UnityWebData1.0\0':raise ValueError(magic)
 end=struct.unpack('<I',f.read(4))[0]
 while f.tell()<end:
  offset,size,length=struct.unpack('<III',f.read(12))
  name=f.read(length).decode();report['entries'].append([name,offset,size])
 save()
 for name,offset,size in report['entries']:
  if not (name.endswith('.assets') or name.endswith('.unity3d')):continue
  path=Path(name).name
  with open(path,'wb') as out:
   f.seek(offset);remaining=size
   while remaining:
    data=f.read(min(remaining,4*1024**2));out.write(data);remaining-=len(data)
  env=UnityPy.load(path)
  types=Counter();details=[]
  for o in env.objects:
   types[o.type.name]+=o.byte_size
   if o.byte_size<300000:continue
   n=''
   if o.type.name in ('Texture2D','Mesh','TextAsset','AudioClip','Cubemap','AnimationClip','Sprite'):
    try:
     o.reset();n=o.reader.read_aligned_string()
    except Exception:pass
   details.append([o.byte_size,o.type.name,n,o.assets_file.name,o.path_id])
  report['largest']+=sorted(details,reverse=True)[:110];report.setdefault('types',{})[name]=dict(types)
  save()
