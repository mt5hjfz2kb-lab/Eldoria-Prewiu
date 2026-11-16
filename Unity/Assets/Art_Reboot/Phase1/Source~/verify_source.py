"""Source and export integrity only; explicitly NOT Unity or mobile QA."""
from pathlib import Path
import hashlib,json,re,struct,subprocess
ROOT=Path(__file__).resolve().parents[1]
REPO=ROOT.parents[3]
base='2d034db21e3da53b65ec187c79f074b334012bc1'
original='Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity'
before=subprocess.check_output(['git','show',base+':'+original],cwd=REPO)
tracked_original=subprocess.check_output(['git','show','HEAD:'+original],cwd=REPO)
assert tracked_original==before,'Original scene changed in branch'
if (REPO/original).exists():
 assert (REPO/original).read_bytes()==before,'Original scene changed in working tree'
scene=(ROOT/'Scenes/Eldoria_Visual_Sandbox.unity').read_text()
if 'Reboot_Phase1_Authored' not in scene:
 assert scene==re.sub('m_IsActive: 1','m_IsActive: 0',before.decode()),'Unexpected sandbox modification before Unity assembly'
else:
 # Unity reserializes the assembled scene. Preserve every original GameObject
 # by file ID and keep it inactive, while allowing the new authored hierarchy.
 def gameobjects(text):
  result={}
  for block in re.split(r'(?m)^--- !u!',text)[1:]:
   match=re.match(r'1 &(-?\d+)\nGameObject:',block)
   if match:result[match.group(1)]=block
  return result
 original_objects=gameobjects(before.decode()); current_objects=gameobjects(scene)
 assert original_objects, 'No original scene objects found'
 for fileid in original_objects:
  assert fileid in current_objects, 'Original object missing: '+fileid
  assert re.search(r'(?m)^  m_IsActive: 0$',current_objects[fileid]),'Active original object: '+fileid

glb=(ROOT/'Source~/Reboot_Pilot.glb').read_bytes()
magic,version,length=struct.unpack_from('<III',glb)
assert magic==0x46546c67 and version==2 and length==len(glb)
n,kind=struct.unpack_from('<II',glb,12);g=json.loads(glb[20:20+n])
assert len(g['images'])==1,'Multiple bitmap textures'
assert len(g['materials'])==2,'Unexpected material count'
assert all('COLOR_0' in p['attributes'] and 'TEXCOORD_0' in p['attributes'] for m in g['meshes'] for p in m['primitives'])
graph=(ROOT/'Materials/Reboot_Atlas.shadergraph').read_text();decoder=json.JSONDecoder();objects=[]
while graph.strip():
 x,n=decoder.raw_decode(graph.lstrip());objects.append(x);graph=graph.lstrip()[n:]
ids={o['m_ObjectId'] for o in objects};assert len(ids)==len(objects)
def check(x):
 if isinstance(x,dict):
  if isinstance(x.get('m_Id'),str) and x['m_Id']:assert x['m_Id'] in ids,'Broken graph reference'
  for v in x.values():check(v)
 elif isinstance(x,list):
  for v in x:check(v)
check(objects)
assert not any('CustomFunction' in o['m_Type'] for o in objects)
for meta in ROOT.parent.rglob('*.meta'):
 assert re.search(r'^guid: [0-9a-f]{32}$',meta.read_text(),re.M),str(meta)
result={'status':'SOURCE_INTEGRITY_PASS','original_scene_sha256':hashlib.sha256(before).hexdigest(),'original_scene_unchanged':True,'sandbox_original_objects_inactive':True,'gltf_one_atlas':True,'gltf_vertex_colors_present':True,'shadergraph_references_valid':True,'unity_compiled':False,'unity_rendered':False,'mobile_tested':False}
(ROOT/'Evidence/source_integrity.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2))
