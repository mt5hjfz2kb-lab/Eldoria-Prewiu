"""Candidate-only exact texture aliasing, with conservative opaque-reference protection.
Never modifies the source bundle. Must pass browser/device verification separately.
"""
import argparse,collections,gc,hashlib,json,pathlib,struct
import UnityPy
from UnityPy.classes.PPtr import PPtr

def sha(b): return hashlib.sha256(b).hexdigest()
def key(o): return (o.assets_file.name,o.path_id)
def walk(t):
 if isinstance(t,dict):
  if 'm_FileID' in t and 'm_PathID' in t: yield t
  else:
   for v in t.values(): yield from walk(v)
 elif isinstance(t,(list,tuple)):
  for v in t: yield from walk(v)
def apply(source,destination,expected):
 source=pathlib.Path(source); assert sha(source.read_bytes())==expected
 out=pathlib.Path(destination);out.mkdir(parents=True,exist_ok=False)
 env=UnityPy.load(str(source));objects={key(o):o for o in env.objects}
 before={k:sha(o.get_raw_data()) for k,o in objects.items()}
 groups=collections.defaultdict(list)
 for k,o in objects.items():
  if o.type.name!='Texture2D':continue
  t=o.read_typetree();raw=o.get_raw_data()
  # Static imported material maps only; excludes dynamic font/UI/portrait textures.
  # Frozen runtime mutation audit found setters on generated Texture2D/RenderTexture
  # instances, not on these immutable non-readable imported material maps.
  if not t.get('m_Name','').startswith(('Valoria_','Eldoria_StoneArchitectureKit_','Eldoria_MultiPiece_StoneKit_','TerrainTerraceKit_','Eldoria Stone · Hero Prototype_','Eldoria Rock · Hero Prototype_','NormalGL_28f57ff9-','Color_28f57ff9-','ORM_28f57ff9-','ORM_351d1034-','Spawnling / retained skin','Spawnling / retained PBR')):continue
  if t.get('m_IsReadable',True) or t.get('m_StreamData',{}).get('size',0) or len(raw)<1000000:continue
  groups[(k[0],sha(raw))].append(k)
 groups={g:ks for g,ks in groups.items() if len(ks)>1}
 candidate_ids={k[1] for ks in groups.values() for k in ks}
 protected=set();unknown=[];parsed={};refs=[]
 for k,o in objects.items():
  try:
   if not any(n.m_Type.startswith('PPtr<') for n in o._get_typetree_node().traverse()):continue
   t=o.read_typetree();pointers=list(walk(t))
   # Require lossless serialization before touching any pointer-bearing object.
   original=o.get_raw_data();o.save_typetree(t)
   if o.data!=original:raise ValueError('typetree roundtrip is not byte-exact')
   o.data=None
   parsed[k]=t
  except Exception as e:
   o.data=None
   raw=o.get_raw_data();hits={i for i in candidate_ids if struct.pack('<q',i) in raw}
   protected.update(hits);unknown.append({'object':k,'reason':str(e)[:180],'protected_ids':sorted(hits)})
   continue
  for p in pointers:
   if not p['m_PathID']:continue
   try: target=key(PPtr(m_FileID=p['m_FileID'],m_PathID=p['m_PathID'],assetsfile=o.assets_file).deref())
   except (FileNotFoundError,KeyError):
    if p['m_PathID'] in candidate_ids:protected.add(p['m_PathID'])
    continue
   refs.append((k,p,target))
 aliases={};saved=0
 for ks in groups.values():
  ks=sorted(ks);canonical=next((k for k in ks if k[1] in protected),ks[0])
  for k in ks:
   if k==canonical or k[1] in protected:continue
   aliases[k]=canonical;saved+=len(objects[k].get_raw_data())
 assert aliases,'No safe exact texture aliases'
 changed=set();redirected=[]
 for k,p,target in refs:
  if target not in aliases:continue
  canonical=aliases[target];assert target[0]==canonical[0]
  p['m_PathID']=canonical[1];changed.add(k)
  redirected.append({'owner':k,'old':target,'new':canonical,'file_id':p['m_FileID']})
 expected_hash=dict(before)
 for k in changed:
  objects[k].save_typetree(parsed[k]);expected_hash[k]=sha(objects[k].data)
 for k in aliases:
  o=objects[k];del o.assets_file.objects[o.path_id];o.assets_file.mark_changed();del expected_hash[k]
 output=out/'data.unity3d';output.write_bytes(env.file.save(packer='lz4'))
 del parsed,refs,objects,env;gc.collect()
 check=UnityPy.load(str(output));after={key(o):o for o in check.objects}
 assert set(after)==set(expected_hash),'Object membership mismatch'
 for k,o in after.items():assert sha(o.get_raw_data())==expected_hash[k],('Unexpected serialized change',k)
 for r in redirected:
  owner=after[tuple(r['owner'])];target=PPtr(m_FileID=r['file_id'],m_PathID=r['new'][1],assetsfile=owner.assets_file).deref()
  assert key(target)==tuple(r['new']) and before[key(target)]==sha(target.get_raw_data())
 # Reopen and confirm every modified typetree contains no reference to removed objects.
 for k in changed:
  o=after[k]
  for p in walk(o.read_typetree()):
   if not p['m_PathID']:continue
   try:target=key(PPtr(m_FileID=p['m_FileID'],m_PathID=p['m_PathID'],assetsfile=o.assets_file).deref())
   except (FileNotFoundError,KeyError):
    assert p['m_PathID'] not in {x[1] for x in aliases};continue
   assert target not in aliases
 proof={'source_sha256':expected,'candidate_sha256':sha(output.read_bytes()),'original_objects':len(before),'all_objects_verified':len(after),'removed_exact_texture_objects':len(aliases),'serialized_bytes_removed':saved,'references_redirected':len(redirected),'changed_reference_objects':len(changed),'aliases':[{'old':k,'canonical':v,'exact_texture_sha256':before[k]} for k,v in aliases.items()],'protected_path_ids':sorted(protected),'unknown_objects':unknown,'redirected':redirected,'runtime_verified':False,'physical_iphone_verified':False,'release_for_external_testers':False}
 (out/'integrity.json').write_text(json.dumps(proof,indent=2));print(json.dumps({k:v for k,v in proof.items() if k not in ('aliases','unknown_objects','redirected','protected_path_ids')}),flush=True)
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--source',required=True);p.add_argument('--destination',required=True);p.add_argument('--expected-sha256',required=True);a=p.parse_args();apply(a.source,a.destination,a.expected_sha256)
