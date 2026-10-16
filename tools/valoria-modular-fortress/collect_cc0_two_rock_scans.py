"""Fetch only FREE official Poly Haven 1K GLTF scanned boulders and chipped limestone rock + exact dependencies.
Never download 2K/4K/8K, never request billing. Provide immutable source hashes.
"""
import urllib.request,urllib.parse,pathlib,hashlib,json,sys,zipfile,io,os,time
out=pathlib.Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
IDS=('boulder_01','rock_07')
agent={'User-Agent':'Eldoria-PublicDomainReferenceArtist/1.0'}
def load(url):
 err=None
 for j in range(3):
  try:
   with urllib.request.urlopen(urllib.request.Request(url,headers=agent),timeout=65) as r:return r.read()
  except Exception as e:
   err=e;time.sleep(j+1)
 raise RuntimeError('Network fetch failed '+url+' '+repr(err))
def walk(obj,path=()):
 if isinstance(obj,dict):
  if isinstance(obj.get('url'),str):yield (path,obj)
  for key,v in obj.items():
   if isinstance(v,(dict,list)):yield from walk(v,path+(str(key),))
 elif isinstance(obj,list):
  for i,v in enumerate(obj):yield from walk(v,path+(str(i),))
manifest={'provider':'Poly Haven','license':'CC0','use':'isolated reference-first model donor tests only',
 'cost_eur':0,'models':{}}
for mid in IDS:
 d=json.loads(load('https://api.polyhaven.com/files/'+mid).decode())
 candidates=list(walk(d))
 preview=[{'path':'/'.join(path),'url':x['url'].split('?')[0][-110:],
           'size':x.get('size',0)} for path,x in candidates]
 print('3D_MODEL_PROVIDER_MANIFEST',mid,json.dumps(preview)[:13000],flush=True)
 # Prefer 1k glTF with packed dependency metadata or 1k zip with all dependencies.
 scored=[]
 for path,entry in candidates:
  stem='/'.join(path).lower();url=entry['url'].lower().split('?')[0]
  if '1k' not in stem and '_1k.' not in url:continue
  if not ('gltf' in stem or 'gltf' in url or 'glb' in url):continue
  ending=url.rsplit('.',1)[-1]
  if ending not in ('gltf','glb','zip'):continue
  score=10 if ending=='glb' else (8 if ending=='zip' else 6)
  if 'gltf' in stem:score+=3
  if ending=='gltf' and entry.get('include'):score+=1
  scored.append((score,path,entry))
 if not scored:raise RuntimeError('No official 1K GLTF from source '+mid)
 _,chosen,main=sorted(scored,key=lambda e:e[0],reverse=True)[0]
 directory=out/mid;directory.mkdir(exist_ok=True)
 url=main['url'];b=load(url)
 if len(b)<1000:raise RuntimeError('Bad geometry '+mid)
 name=urllib.parse.unquote(urllib.parse.urlparse(url).path.rsplit('/',1)[-1])
 mainpath=directory/name;mainpath.write_bytes(b)
 saved=[{'name':name,'url':url,'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}]
 if name.lower().endswith('.zip'):
  with zipfile.ZipFile(io.BytesIO(b)) as z:
   for member in z.infolist():
    if member.is_dir() or '..' in pathlib.PurePosixPath(member.filename).parts:continue
    dest=directory/pathlib.PurePosixPath(member.filename).name
    bb=z.read(member);dest.write_bytes(bb)
    saved.append({'name':dest.name,'bytes':len(bb),'sha256':hashlib.sha256(bb).hexdigest()})
 elif name.lower().endswith('.gltf'):
  gltf=json.loads(b)
  want=[v['uri'] for v in gltf.get('buffers',[])+gltf.get('images',[]) if 'uri' in v and not v['uri'].startswith('data:')]
  include=main.get('include') or {}
  paths=list(walk(include))
  for want_name in want:
   want_file=pathlib.PurePosixPath(want_name).name
   match=[(p,x) for p,x in paths if want_file.lower() in x['url'].lower()]
   if not match:
    match=[(p,x) for p,x in candidates if want_file.lower() in x['url'].lower() and ('1k' in '/'.join(p).lower() or '_1k' in x['url'].lower())]
   if not match:raise RuntimeError('Dependency '+want_file+' not present in 1K include for '+mid)
   _,ref=match[0];blob=load(ref['url']);target=directory/want_file;target.write_bytes(blob)
   saved.append({'name':target.name,'url':ref['url'],'bytes':len(blob),'sha256':hashlib.sha256(blob).hexdigest()})
 else:
  print('PACKED_GLB_NO_DEPENDENCIES',mid,flush=True)
 # Blender importer will inspect all maps, no geometry used before license verification.
 manifest['models'][mid]={'chosen_tree_path':'/'.join(chosen),'files':saved,
  'source_asset_page':'https://polyhaven.com/a/'+mid}
 print('HIGH_DETAIL_CC0_MODEL_FETCHED',mid,name,'dependency_count',len(saved),flush=True)
(out/'official_cc0_models.json').write_text(json.dumps(manifest,indent=2))
