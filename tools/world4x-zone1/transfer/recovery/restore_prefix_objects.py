"""Recover complete, hash-verified owned files from an interrupted Git bundle.
Never treats an incomplete pack as a complete checkpoint. Missing objects are logged.
"""
import hashlib,json,os,pathlib,subprocess
REPO=pathlib.Path(__file__).resolve().parents[4]
env=dict(os.environ,GIT_NO_LAZY_FETCH='1')
commit='3cf47193e5667efcf557ddfdc54a487f5cd425b5'
prefixes=('docs/world4x-zone1/','tools/world4x-zone1/',
 'Unity/Assets/Eldoria/ArtTests/World4XZone1/')
extra={'Unity/Assets/Eldoria/ArtTests/World4XZone1.meta',
 'pipeline/art-production-requests/world4x-zone1-isolated-reference-prototype-20261013.json'}
result={'source_commit':commit,'state':'PARTIAL_RECOVERY_NOT_COMPLETE','restored':[],'missing':[]}
def read(kind,sha):
 p=subprocess.run(['git','cat-file',kind,sha],cwd=REPO,env=env,capture_output=True)
 return p.stdout if p.returncode==0 else None
def relevant(path):return path in extra or any(path.startswith(p) or p.startswith(path+'/') for p in prefixes) or any(p.startswith(path+'/') for p in extra)
def tree(sha,path=''):
 raw=read('tree',sha)
 if raw is None:result['missing'].append({'path':path,'sha':sha,'type':'tree'});return
 pos=0
 while pos<len(raw):
  end=raw.index(b'\0',pos);mode,name=raw[pos:end].split(b' ',1);oid=raw[end+1:end+21].hex();pos=end+21
  name=name.decode();full=(path+'/' if path else '')+name
  if not relevant(full):continue
  if mode==b'40000':tree(oid,full);continue
  data=read('blob',oid)
  if data is None:result['missing'].append({'path':full,'sha':oid,'type':'blob'});continue
  actual=hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()
  if actual!=oid:raise ValueError('Untrusted recovered object '+full)
  target=REPO/full;target.parent.mkdir(parents=True,exist_ok=True)
  if target.exists() and target.read_bytes()!=data:
   base=read('blob','34ce0ec78b2bc553f11f16562a5f18752620fb34:'+full)
   if base is not None and target.read_bytes()!=base:raise ValueError('Refuse to overwrite changed file '+full)
  target.write_bytes(data)
  result['restored'].append({'path':full,'sha':oid,'bytes':len(data)})
source=read('commit',commit)
if source is None:raise ValueError('Source commit object was not recovered')
tree(source.splitlines()[0].decode().split()[1])
out=REPO/'docs/world4x-zone1/PARTIAL_SOURCE_RECOVERY_20261014.json'
out.write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({'restored':len(result['restored']),'missing':result['missing']},indent=2))
