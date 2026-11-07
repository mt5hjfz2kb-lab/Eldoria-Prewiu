import gzip, hashlib, json, shutil, sys, zipfile, urllib.request, concurrent.futures
from pathlib import Path, PurePosixPath
import brotli
def sha(b): return hashlib.sha256(b).hexdigest()
def safe(name):
 p=PurePosixPath(name.split('?')[0])
 if p.is_absolute() or '..' in p.parts: raise ValueError('unsafe path')
 return p.as_posix()
def prepare(package,site):
 site=Path(site); out=site/'beta-native325'; out.mkdir(parents=True,exist_ok=False)
 assert sha(Path(package).read_bytes())=='fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771'
 with zipfile.ZipFile(package) as z:
  assert len(z.infolist())==19
  for i in z.infolist():
   name=safe(i.filename); assert i.file_size<100000000 and not ((i.external_attr>>16)&0o170000)==0o120000
   p=out/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(z.read(i))
 m=json.loads((out/'build-manifest.json').read_text())
 proofs=[]
 for target,parts in m['decoded_parts'].items():
  for part in parts:
   old=out/safe(part['url']); raw=brotli.decompress(old.read_bytes()) if part.get('encoding')=='br' else gzip.decompress(old.read_bytes())
   assert len(raw)==part['decoded_bytes']
   if part.get('decoded_sha256'): assert sha(raw)==part['decoded_sha256']
   new=old.with_suffix('.gz'); new.write_bytes(gzip.compress(raw,compresslevel=9,mtime=0))
   assert sha(gzip.decompress(new.read_bytes()))==sha(raw)
   part.update(url=new.relative_to(out).as_posix(),encoding='gzip',decoded_sha256=sha(raw))
   proofs.append({'path':part['url'],'decoded_sha256':sha(raw),'decoded_bytes':len(raw)})
   if old!=new: old.unlink()
 for key in ['framework','code']:
  url=m[key];p=out/safe(url);raw=gzip.decompress(p.read_bytes());assert len(raw)==m['decoded_sizes'][url]
  m['decoded_parts'][url]=[{'url':safe(url),'encoding':'gzip','decoded_bytes':len(raw),'decoded_sha256':sha(raw)}]
  proofs.append({'path':safe(url),'decoded_sha256':sha(raw),'decoded_bytes':len(raw)})
 m['delivery_adapter']='github-pages-exact-gzip-v1'
 (out/'build-manifest.json').write_text(json.dumps(m,indent=2)+'\n')
 identity={'revision':'native325','source_sha':'948ffa9837168a0b68dab80be76b66350422b23a','original_zip_sha256':'fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771','game_bytes_unchanged':True,'tutorial_unchanged':True,'clock_unchanged':True,'telemetry_gameplay_connected':False,'physical_mobile_tested':False,'decoded_files':proofs}
 (out/'release-identity.json').write_text(json.dumps(identity,indent=2)+'\n')
 import importlib.util
 spec=importlib.util.spec_from_file_location('startup_patch',Path(__file__).with_name('patch-startup.py'));mod=importlib.util.module_from_spec(spec);spec.loader.exec_module(mod)
 mod.patch_startup(out)
 identity['startup_diagnostic']='stages-v3-opt-in'
 (out/'release-identity.json').write_text(json.dumps(identity,indent=2)+'\n')
 return identity
def mirror_previous(site):
 base='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/'
 def get(path):
  req=urllib.request.Request(base+path,headers={'Accept-Encoding':'identity','User-Agent':'Eldoria-beta-preserve'})
  with urllib.request.urlopen(req,timeout=120) as r: return r.read()
 m=json.loads(get('unity-owner/build-manifest.json'))
 paths=['index.html','unity-owner/index.html','unity-owner/build-manifest.json','unity-owner/play.js','unity-owner/style.css','unity-owner/city.png','unity-owner/release-identity.json','unity-owner/TemplateData/webmemd-icon.png','unity-owner/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb']
 paths+=['unity-owner/'+safe(m['loader'])]
 for k in ['framework','code','data']:
  paths+=['unity-owner/'+safe(p['url']) for p in m.get('decoded_parts',{}).get(m[k],[{'url':m[k]}])]
 def save(name):
  b=get(name);p=Path(site)/safe(name);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(b)
  return {'path':name,'bytes':len(b),'sha256':sha(b)}
 with concurrent.futures.ThreadPoolExecutor(4) as ex: results=list(ex.map(save,sorted(set(paths))))
 return results
if __name__=='__main__':
 package,site=sys.argv[1:]; identity=prepare(package,site);previous=mirror_previous(site)
 (Path(site)/'.nojekyll').touch()
 total=sum(p.stat().st_size for p in Path(site).rglob('*') if p.is_file())
 assert total<1073741824, f'Pages site exceeds 1 GiB: {total}'
 print(json.dumps({'bytes':total,'previous_files':len(previous),'beta_identity':identity},indent=2))
