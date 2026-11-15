"""Isolated same-engine residency trial; NOT a native-source or phone certificate.
Keep all geometry/collision bytes; change Read/Write on four audited static meshes.
Never apply to terrain, resources, heroes, creatures, skins or arbitrary meshes.
"""
import gc,gzip,hashlib,json,struct,sys
from pathlib import Path
import UnityPy
from UnityPy.helpers import TypeTreeHelper
from mobile_site import candidate

SOURCE='4eb972ac2f97aa1377950180504a5fefeec4ebd4d0c58c358601cbe9ae01a99d'
TARGETS={'sharedassets0.assets:296':'Bastion_II_GI',
         'sharedassets0.assets:306':'Bastion_I',
         'sharedassets0.assets:377':'Bastion-0',
         'sharedassets0.assets:378':'Bastion-1'}
def sha(b):return hashlib.sha256(b).hexdigest()
def key(o):return o.assets_file.name+':'+str(o.path_id)
def run(site,output):
 site=Path(site);out=Path(output);out.mkdir(exist_ok=False,parents=True)
 manifest=json.loads((site/'build-manifest.json').read_text());data=out/'source.data'
 with data.open('wb') as dst:
  for part in manifest['decoded_parts'][manifest['data']]:
   b=gzip.decompress((site/part['url']).read_bytes())
   assert len(b)==part['decoded_bytes'] and sha(b)==part['decoded_sha256'];dst.write(b)
 with data.open('rb') as src:
  assert src.read(16)==b'UnityWebData1.0\0';end=struct.unpack('<I',src.read(4))[0]
  h=src.read(end-20);pos=0;found=[]
  while pos<len(h):
   offset,size,n=struct.unpack_from('<III',h,pos);pos+=12;name=h[pos:pos+n];pos+=n
   if name==b'data.unity3d':found.append((offset,size))
  assert pos==len(h) and len(found)==1
  src.seek(found[0][0]);bundle=src.read(found[0][1]);assert sha(bundle)==SOURCE
 env=UnityPy.load(bundle);del bundle
 original={key(o):sha(o.get_raw_data()) for o in env.objects};changes={};report=[];usage=[]
 for o in env.objects:
  if o.type.name not in ('MeshFilter','MeshCollider','SkinnedMeshRenderer'):continue
  t=o.read();p=getattr(t,'m_Mesh',None)
  if p is None or not p.m_PathID:continue
  mesh=p.deref();k=key(mesh)
  if k not in TARGETS:continue
  assert o.type.name!='SkinnedMeshRenderer'
  go=t.m_GameObject.read();u=dict(mesh=k,type=o.type.name,name=go.m_Name)
  if o.type.name=='MeshCollider':
   assert not t.m_Convex and t.m_CookingOptions==30
   assert mesh.read().m_BakedTriangleCollisionMesh
   u['cooking']=t.m_CookingOptions;u['baked_collision']=True
  usage.append(u)
 assert set(u['mesh'] for u in usage)==set(TARGETS)
 for o in env.objects:
  k=key(o)
  if k not in TARGETS:continue
  assert o.type.name=='Mesh'
  # Read only the small prefix, avoiding copies of huge vertex/index arrays.
  o.reset();start=o.reader.Position;prefix={};offset=None
  config=TypeTreeHelper.TypeTreeConfig(True,o.assets_file,False)
  for node in o._get_typetree_node().m_Children:
   if node.m_Name=='m_IsReadable':offset=o.reader.Position-start
   prefix[node.m_Name]=TypeTreeHelper.read_value(node,o.reader,config)
   if node.m_Name=='m_KeepIndices':break
  assert prefix['m_Name']==TARGETS[k] and prefix['m_IsReadable']
  assert not prefix['m_KeepVertices'] and not prefix['m_KeepIndices']
  assert not prefix['m_BindPose'] and offset is not None
  raw=o.get_raw_data();assert raw[offset]==1
  modified=raw[:offset]+b'\x00'+raw[offset+1:]
  assert len(raw)==len(modified) and raw[:offset]==modified[:offset] and raw[offset+1:]==modified[offset+1:]
  o.set_raw_data(modified);changes[k]=sha(modified)
  report.append(dict(object=k,name=TARGETS[k],changed_byte_offset=offset,serialized_bytes=len(raw),before_sha256=sha(raw),after_sha256=changes[k]))
  del raw,modified
 assert set(changes)==set(TARGETS)
 result=out/'data.unity3d';result.write_bytes(env.file.save(packer='lz4'));del env;gc.collect()
 check=UnityPy.load(str(result));actual={key(o):sha(o.get_raw_data()) for o in check.objects}
 assert actual.keys()==original.keys()
 assert all(v==changes.get(k,original[k]) for k,v in actual.items())
 del check;gc.collect()
 proof=dict(source_bundle_sha256=SOURCE,candidate_bundle_sha256=sha(result.read_bytes()),
  all_objects_verified=len(original),changes=report,usage=usage,geometry_collision_and_textures_byte_identical=True,
  changed_fields_only=['Mesh.m_IsReadable'],native_source_implementation=False,runtime_verified=False,physical_iphone_verified=False)
 (out/'integrity.json').write_text(json.dumps(proof,indent=2))
 candidate(site,data,result,out/'site','integrated-mobile21-static-mesh-trial')
 identity=json.loads((out/'site/release-identity.json').read_text())
 identity.update(texture_payload_modified=False,preexisting_mobile6_profile_reused=True,
  delivery_adapter_trial='FOUR_STATIC_MESH_READABILITY_FLAGS_ONLY',native_journey_verified=False,
  native_destination_captures_verified=False,delivered_data_sha256=None,
  source_bundle_sha256=SOURCE,static_mesh_trial_proof=proof)
 (out/'site/release-identity.json').write_text(json.dumps(identity,indent=2))
 print(json.dumps(proof),flush=True)
if __name__=='__main__':run(*sys.argv[1:])
