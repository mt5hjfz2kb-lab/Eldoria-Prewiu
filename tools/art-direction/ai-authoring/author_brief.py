#!/usr/bin/env python3
"""Create REAL scene-aware Qwen artistic direction using local Ollama and versioned system role.
Requires two genuine images; it does not execute Blender or certify art by itself.
"""
import argparse,base64,hashlib,json,pathlib,urllib.request,urllib.parse,urllib.error,time
HERE=pathlib.Path(__file__).resolve().parent
ALLOWED={'sculpt_region','reprofile_crest','sculpt_path','bridge_boundary_loops','construct_mesh','assign_material','place_existing_asset'}
def digest(data):return hashlib.sha256(data).hexdigest()
def main():
 p=argparse.ArgumentParser()
 p.add_argument('--reference',type=pathlib.Path,required=True)
 p.add_argument('--unity-before',type=pathlib.Path,required=True)
 p.add_argument('--target-object',required=True)
 p.add_argument('--target-mode',choices=['existing','new'],default='existing',help='new allows the artist to create a previously absent mesh')
 p.add_argument('--inventory',type=pathlib.Path)
 p.add_argument('--mesh-context',type=pathlib.Path,help='Optional existing Blender mesh vertex indices, border loops and anchoring coordinates')
 p.add_argument('--out',required=True,type=pathlib.Path)
 p.add_argument('--model',default='qwen3-vl:4b-instruct')
 p.add_argument('--endpoint',default='http://127.0.0.1:11434/api/chat')
 a=p.parse_args()
 uri=urllib.parse.urlsplit(a.endpoint)
 if uri.scheme!='http' or uri.hostname not in ('localhost','127.0.0.1') or uri.username or uri.password or uri.path!='/api/chat':
  raise ValueError('Only loopback Ollama /api/chat is permitted')
 inputs=[];hashes={}
 for key,image in [('reference',a.reference),('unity_before',a.unity_before)]:
  if not image.is_file() or image.suffix.lower() not in ('.png','.jpg','.jpeg'):raise ValueError('Genuine image missing: '+str(image))
  raw=image.read_bytes()
  if len(raw)>10_000_000:raise ValueError('Input image too large')
  hashes[key]=digest(raw);inputs.append(base64.b64encode(raw).decode())
 role=(HERE/'ARTIST_SYSTEM.md').read_bytes()
 # The artist must see actual source candidates, not a fictional generic inventory.
 repo=HERE.parents[2]
 if a.inventory:
  if not a.inventory.is_file():raise ValueError('Explicit asset inventory does not exist')
  inventory=a.inventory.read_text('utf8')[:4000]
 else:
  candidates=[]
  for directory in [repo/'art-source/valoria/production',repo/'Unity/Assets/Eldoria']:
   if directory.is_dir():
    for suffix in ('*.glb','*.blend','*.fbx','*.prefab','*.mat'):
     candidates.extend(directory.rglob(suffix))
  candidates=[x for x in candidates if x.is_file() and not x.name.endswith('.blend1')]
  def priority(p):
   name=p.name.lower();location=p.as_posix().lower()
   if 'hero' in name and ('v3' in name or 'gate' in name):return (0,location)
   if 'starter-family-v3' in location:return (1,location)
   if any(x in name for x in ['cliff','rock','terrain','gate','tower','wall','path','stair','bridge']):return (2,location)
   if 'vegetation' in location or 'foliage' in location:return (3,location)
   return (4,location)
  candidates=sorted(set(candidates),key=priority)[:50]
  inventory=('Existing candidate 3D sources (frozen source SHA, quality and license MUST be checked): '+
   ('; '.join(f'{x.relative_to(repo)} ({x.stat().st_size} bytes)' for x in candidates) if candidates else
    'NO LOCAL SOURCE FILES FOUND; inspect active art branches before assuming NEW required'))[:5500]
 journal=HERE.parent/'ARTIST_LEARNING_JOURNAL.md'
 memory=journal.read_text('utf8')[-2600:] if journal.is_file() else 'NO PREVIOUS LEARNING JOURNAL AVAILABLE; record this limitation.'
 memory_hash=digest(journal.read_bytes()) if journal.is_file() else None
 mesh_context='NO MESH TOPOLOGY MANIFEST; DO NOT GUESS VERTEX IDS FOR BRIDGE OPERATIONS.'
 mesh_context_sha=None
 if a.mesh_context:
  if not a.mesh_context.is_file() or a.mesh_context.stat().st_size>25000:raise ValueError('Invalid mesh context manifest')
  mesh_context=a.mesh_context.read_text('utf8')
  topology=json.loads(mesh_context)
  if topology.get('schema_version')!=1 or topology.get('target')!=a.target_object or not isinstance(topology.get('boundary_loops'),list):raise ValueError('Mesh topology manifest target/schema mismatch')
  mesh_context_sha=digest(mesh_context.encode('utf8'))
 prompt=('Compare left REFERENCE and right GENUINE UNITY screenshot. Diagnose and DIRECT NEW 3D geometry, '
  'not just noise parameters. Valid action parameter formats: '
  'sculpt_region center [x,y,z], radius, delta [x,y,z]; '
  'reprofile_crest axis x|y, height_axis y|z, control_points [[pos,height],...], band_min, band_max; '
  'sculpt_path knots [{center:[x,y,z],delta:[x,y,z],radius:meters}, ...] for coherent spatial changes along curved cliff/road edges; '
  'bridge_boundary_loops loop_a:[vertex_indices...], loop_b:[same_count_indices...] only using VERIFIED open boundary loops from mesh manifest; '
  'construct_mesh vertices [[x,y,z]...], faces [[indices]...], material_color [r,g,b]; '
  'assign_material rgb [r,g,b], roughness; '
  'place_existing_asset source_object, location [x,y,z], rotation [x,y,z], scale [x,y,z]. '
  f'Use exactly target name {a.target_object}, with target mode {a.target_mode}: for existing, edit the existing Blender object and never construct a duplicate; for new, construct_mesh must be the first operation, followed by edits of that new object. Current inventory: {inventory}. '
  f'Previously verified art failures and learnings (avoid repeating): {memory}. '
  f'Verified mesh topology/anchors: {mesh_context[:11000]}. '
  'Output strictly JSON; model must choose artist-directed geometric edit if structural deficit.')
 payload={'model':a.model,'stream':False,'think':False,'format':'json',
  'options':{'temperature':0.25,'num_predict':2600,'num_ctx':8192},
  'messages':[{'role':'system','content':role.decode('utf8')},
              {'role':'user','content':prompt,'images':inputs}]}
 request=urllib.request.Request(a.endpoint,data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 started=time.monotonic()
 try:
  with urllib.request.urlopen(request,timeout=540) as response:reply=json.load(response)
 except urllib.error.HTTPError as exc:
  # Loopback-only Ollama endpoint: preserve its bounded error text for root-cause diagnosis.
  details=exc.read(2048).decode('utf8','replace')
  raise RuntimeError(f'Local Ollama HTTP {exc.code}: {details}') from exc
 inference_seconds=round(time.monotonic()-started,3)
 design=json.loads(reply['message']['content'])
 if len(design.get('observed_defect',''))<35 or len(design.get('design_intent',''))<35:raise ValueError('Insufficient artist diagnosis')
 if design.get('asset_decision') not in ['REUSE','MODIFY','NEW']:raise ValueError('Reuse-vs-new decision missing')
 ops=design.get('planned_actions')
 if not isinstance(ops,list) or not 1<=len(ops)<=8:raise ValueError('1-8 authoring operations required')
 if a.target_mode=='new' and ops[0].get('type')!='construct_mesh':raise ValueError('NEW asset must begin with actual mesh creation')
 if a.target_mode=='existing' and any(op.get('type')=='construct_mesh' for op in ops):raise ValueError('Cannot construct new mesh with an existing source object')
 ids=set()
 for op in ops:
  if op.get('type') not in ALLOWED or not op.get('id') or op['id'] in ids:raise ValueError('Invalid/duplicate artist action')
  if op.get('target')!=a.target_object:raise ValueError('Unauthorized Blender target')
  if op.get('type')=='bridge_boundary_loops':
   if not a.mesh_context:raise ValueError('Boundary bridge forbidden without verified source mesh context')
   verified=[x.get('indices') for x in topology['boundary_loops'] if isinstance(x,dict)]
   pr=op.get('parameters') or {}
   if pr.get('loop_a') not in verified or pr.get('loop_b') not in verified or pr.get('loop_a')==pr.get('loop_b'):
    raise ValueError('Qwen must select two DISTINCT actual Blender boundary loops')

  if not isinstance(op.get('parameters'),dict) or len(op.get('rationale',''))<15:raise ValueError('Unspecified modelling intent')
  ids.add(op['id'])
 a.out.mkdir(parents=True,exist_ok=True)
 evidence={'model':a.model,'system_role':'system','system_prompt_sha256':digest(role),'reference_sha256':hashes['reference'],
  'unity_before_sha256':hashes['unity_before'],'artist_learning_journal_sha256':memory_hash,
   'asset_inventory_sha256':digest(inventory.encode('utf8')),'mesh_context_sha256':mesh_context_sha,'inference_seconds':inference_seconds,'target_object':a.target_object,'target_mode':a.target_mode,'direction':design,'self_approval':False}
 (a.out/'artist-inference.json').write_text(json.dumps(evidence,indent=2)+'\n','utf8')
 (a.out/'artist-plan.json').write_text(json.dumps(ops,indent=2)+'\n','utf8')
 print('REAL_LOCAL_ARTIST_DIRECTION_RECORDED',len(ops),'actions')
if __name__=='__main__':main()
