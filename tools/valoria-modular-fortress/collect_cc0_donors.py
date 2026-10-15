"""Fetch precisely named free CC0 glTF models and dependencies from pinned GitHub SHA.
Never fetch Pro/Source paid Quaternius assets.
Reference source: Quaternius Stylized Nature MegaKit [Standard] from FreeModels mirror.
"""
from pathlib import Path
import json,urllib.parse,urllib.request,hashlib,time
import sys
out=Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
SOURCE_SHA="db3df04d1e4714298a09510b26fb6de6645138a2"
base="https://raw.githubusercontent.com/agentkaerf/FreeModels/"+SOURCE_SHA+"/"
folder="Stylized Nature MegaKit[Standard]/glTF/"
names=["Pine_1","Pine_2","Pine_3","Pine_4","Pine_5",
"Rock_Medium_1","Rock_Medium_2","Rock_Medium_3",
"Bush_Common","Grass_Wispy_Tall","Grass_Wispy_Short",
"RockPath_Round_Small_1","RockPath_Round_Small_2",
"Pebble_Round_1","Pebble_Round_2","Pebble_Round_3"]
downloaded={}
def save(path):
 if path in downloaded:return
 url=base+urllib.parse.quote(path,safe='/')
 req=urllib.request.Request(url,headers={'User-Agent':'Eldoria CC0 source verification'})
 err=None
 for i in range(3):
  try:
   with urllib.request.urlopen(req,timeout=60) as r:b=r.read()
   break
  except Exception as e:
   err=e;time.sleep(2+i*2)
 else:raise RuntimeError(f'Cannot read pinned CC0 donor source {path}: {err}')
 if len(b)<10:raise RuntimeError('Empty source '+path)
 dst=out/Path(path).name
 dst.write_bytes(b)
 downloaded[path]={'source_url':url,'size':len(b),'sha256':hashlib.sha256(b).hexdigest()}
 return b
licensepath="Stylized Nature MegaKit[Standard]/License_Standard.txt"
license=save(licensepath).decode('utf-8',errors='replace')
if 'CC0' not in license and 'Creative Commons Zero' not in license:raise RuntimeError('Donor source has no verified CC0 license')
for name in names:
 f=folder+name+".gltf"
 gltf=json.loads(save(f))
 for item in gltf.get('buffers',[]):
  if 'uri' in item:save(folder+item['uri'])
 for item in gltf.get('images',[]):
  if 'uri' in item:save(folder+item['uri'])
 for item in gltf.get('materials',[]):
  if item.get('alphaMode')=='MASK':assert item.get('name')
(out/'source_manifest.json').write_text(json.dumps({
 'source_repository':'agentkaerf/FreeModels',
 'original_author':'Quaternius',
 'original_pack':'Stylized Nature MegaKit Standard',
 'license':'CC0',
 'source_sha':SOURCE_SHA,'models':names,
 'files':downloaded,'cost_eur':0,
 'source_approved':False,
 'reason':'Geometry and images are candidate donors, not automatically art-approved'
},indent=2))
print('CC0_DONOR_RETRIEVED',len(names),'models',len(downloaded),'verified source files',flush=True)
