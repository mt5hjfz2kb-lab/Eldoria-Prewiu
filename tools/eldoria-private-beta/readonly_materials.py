"""Candidate2: remove CPU copies only for serialized material textures without Sprite/CPU-reader use."""
import sys,json,hashlib,re,gc
from pathlib import Path
import UnityPy
def key(o):return o.assets_file.name+':'+str(o.path_id)
def sha(b):return hashlib.sha256(b).hexdigest()
def run(source,out):
 out=Path(out);out.mkdir(parents=True,exist_ok=True)
 assert sha(Path(source).read_bytes())=='d169fee32bca1b2e26aae3693a6bd53bab683d11a8a5c8103b4aedff28c7eeb7'
 env=UnityPy.load(source);original={key(o):sha(o.get_raw_data()) for o in env.objects};material=set();deny=set()
 for o in env.objects:
  if o.type.name=='Material':
   for prop,tex in o.read().m_SavedProperties.m_TexEnvs:
    if tex.m_Texture.m_PathID:
     obj=tex.m_Texture.read().object_reader
     if obj.type.name=='Texture2D':material.add(key(obj))
  if o.type.name=='Sprite':
   t=o.read()
   for ref in [t.m_RD.texture,t.m_RD.alphaTexture]:
    if ref.m_PathID:deny.add(key(ref.read().object_reader))
 changed={};metadata={};report=[]
 for o in env.objects:
  if o.type.name!='Texture2D' or key(o) not in material or key(o) in deny:continue
  t=o.read()
  if not t.m_IsReadable or re.search('engendro|lookup|lut|mask',t.m_Name,re.I):continue
  before=o.read_typetree();metadata[key(o)]={k:v for k,v in before.items() if k!='m_IsReadable'}
  report.append({'id':key(o),'name':t.m_Name,'pixel_bytes':len(t.get_image_data()),'format':t.m_TextureFormat})
  t.m_IsReadable=False;t.save();changed[key(o)]=sha(o.data)
 assert changed
 payload=env.file.save(packer='lz4');(out/'data.unity3d').write_bytes(payload);del env;gc.collect()
 check=UnityPy.load(str(out/'data.unity3d'));found={key(o):sha(o.get_raw_data()) for o in check.objects}
 assert found.keys()==original.keys()
 for k,v in found.items():assert v==changed.get(k,original[k]),k
 for o in check.objects:
  if key(o) in changed:
   tree=o.read_typetree();assert tree['m_IsReadable']==False
   assert {k:v for k,v in tree.items() if k!='m_IsReadable'}==metadata[key(o)],key(o)
 proof={'source_bundle_sha256':'d169fee32bca1b2e26aae3693a6bd53bab683d11a8a5c8103b4aedff28c7eeb7','candidate_bundle_sha256':sha(payload),'changed_count':len(changed),'all_objects_verified':len(original),'pixel_bytes_unchanged':True,'nominal_cpu_copy_bytes':sum(x['pixel_bytes'] for x in report),'textures':report,'runtime_verified':False,'physical_iphone_verified':False}
 (out/'readonly-proof.json').write_text(json.dumps(proof,indent=2));print('READONLY_INTEGRITY_PASS',json.dumps({k:v for k,v in proof.items() if k!='textures'}),flush=True)
if __name__=='__main__':run(*sys.argv[1:])
