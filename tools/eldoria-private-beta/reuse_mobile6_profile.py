"""Reuse exact previously shipped texture/cubemap bytes in a fresh native build.
Only exact serialized source matches qualify. No new compression or art decisions.
Serialized integrity does not certify browser rendering or physical iPhone stability.
"""
import argparse, gc, hashlib, json
from pathlib import Path
import UnityPy
SOURCE='b809309b21009787b6bcf07f46a1f02d5ad653ebeb8d1563a80bc7c8d6d6e309'
REFERENCE='e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1'
ALLOWED={'m_Width','m_Height','m_MipCount','m_CompleteImageSize','m_TextureFormat','m_IsReadable','image data'}
TYPES={'Texture2D','Cubemap'}
def sha(b):return hashlib.sha256(b).hexdigest()
def filehash(p):
 with open(p,'rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def key(o):return o.assets_file.name+':'+str(o.path_id)
def fixed(t):return {k:v for k,v in t.items() if k not in ALLOWED}
def catalog(source,reference):
 assert filehash(source)==SOURCE,'Unexpected reference parent'
 assert filehash(reference)==REFERENCE,'Unexpected mobile6 reference'
 env=UnityPy.load(str(source));before={};metadata={}
 for o in env.objects:
  k=key(o);before[k]=(sha(o.get_raw_data()),o.type.name)
  if o.type.name in TYPES:metadata[k]=(fixed(o.read_typetree()),tuple(o.version))
 del env;gc.collect()
 env=UnityPy.load(str(reference));seen=set();transforms={};count=0
 for o in env.objects:
  k=key(o);seen.add(k);raw=o.get_raw_data();h=sha(raw)
  assert k in before and o.type.name==before[k][1],'Reference membership/type changed'
  if h==before[k][0]:continue
  assert o.type.name in TYPES,('Reference changed non-texture object',k)
  assert (fixed(o.read_typetree()),tuple(o.version))==metadata[k],('Reference changed protected metadata',k)
  prior=transforms.get(before[k][0]);assert prior is None or prior['sha256']==h,'Ambiguous exact transform'
  transforms[before[k][0]]={'raw':raw,'sha256':h,'type':o.type.name,'version':tuple(o.version)};count+=1
 assert seen==set(before) and count>0
 del env,before,metadata;gc.collect()
 print('REFERENCE_PROFILE_VERIFIED',count,'changed objects',len(transforms),'exact transforms',flush=True)
 return transforms

def run(a):
 assert filehash(a.fresh)==a.expected_fresh_sha256,'Unexpected fresh input'
 out=Path(a.output);out.mkdir(parents=True,exist_ok=False)
 transforms=catalog(a.parent,a.reference)
 env=UnityPy.load(a.fresh);before={};changed={};rows=[]
 for o in env.objects:
  raw=o.get_raw_data();h=sha(raw);k=key(o);before[k]=h
  if h not in transforms:continue
  t=transforms[h];assert o.type.name==t['type'] and tuple(o.version)==t['version']
  o.set_raw_data(t['raw']);changed[k]=t['sha256']
  rows.append({'object':k,'type':o.type.name,'before_sha256':h,'after_sha256':t['sha256'],'before_bytes':len(raw),'after_bytes':len(t['raw'])})
 assert changed,'No exact profile transforms matched'
 target=out/'data.unity3d';target.write_bytes(env.file.save(packer='lz4'))
 del env,transforms;gc.collect()
 check=UnityPy.load(str(target));after={key(o):sha(o.get_raw_data()) for o in check.objects}
 assert set(before)==set(after),'Fresh object membership changed'
 for k,h in after.items():assert h==changed.get(k,before[k]),('Unexpected fresh object mutation',k)
 proof={'source_sha256':a.expected_fresh_sha256,'candidate_sha256':filehash(target),'reference_parent_sha256':SOURCE,'reference_profile_sha256':REFERENCE,'all_objects_verified':len(before),'changed_textures':len(changed),'exact_serialized_matches_only':True,'changes':rows,'runtime_verified':False,'physical_iphone_verified':False,'release_for_external_testers':False}
 (out/'integrity.json').write_text(json.dumps(proof,indent=2))
 print('FRESH_PROFILE_INTEGRITY_PASS',len(changed),target.stat().st_size,flush=True)
if __name__=='__main__':
 p=argparse.ArgumentParser()
 for n in ('parent','reference','fresh','expected-fresh-sha256','output'):p.add_argument('--'+n,required=True)
 run(p.parse_args())
