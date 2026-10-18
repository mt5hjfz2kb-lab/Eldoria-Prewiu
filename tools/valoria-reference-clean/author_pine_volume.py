"""Isolated Bastion ensemble replacement; existing Blender source is never overwritten."""
import base64,bpy,bmesh,ast,array,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
from mathutils import Vector,Matrix
from math import sin,cos,pi
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean'
OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE=EV/'Valoria_Reference_Clean.blend'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(EV/n).write_text(json.dumps(v,indent=2),encoding='utf8')
expected=json.loads((EV/'source-art.json').read_text())['blend_sha256']
assert digest(SOURCE)==expected
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
original=bpy.data.objects['Fir_tree_group']
def stamp(o):
 a=array.array('f',[0])*(len(o.data.vertices)*3);o.data.vertices.foreach_get('co',a)
 return (tuple(v for row in o.matrix_world for v in row),hashlib.sha256(a.tobytes()).hexdigest())
protected={o.name:stamp(o) for o in bpy.context.scene.objects if o.type=='MESH'}
points=[original.matrix_world@v.co for v in original.data.vertices]

# Actual bark topology is inspected before model inference; no invented vertex IDs.
world=[original.matrix_world@v.co for v in original.data.vertices]
keys={};vertex_key=[];canonical=[]
for i,p in enumerate(world):
 key=tuple(round(float(v),4) for v in p)
 if key not in keys:keys[key]=len(canonical);canonical.append(i)
 vertex_key.append(keys[key])
parent=list(range(len(canonical)))
def root(i):
 while parent[i]!=i:parent[i]=parent[parent[i]];i=parent[i]
 return i
bark_indices={i for i,m in enumerate(original.data.materials) if m and 'bark' in m.name.lower()}
leaf_indices={i for i,m in enumerate(original.data.materials) if m and 'leav' in m.name.lower()}
assert bark_indices and leaf_indices
for f in original.data.polygons:
 if f.material_index not in bark_indices:continue
 ids=[vertex_key[i] for i in f.vertices];r=root(ids[0])
 for i in ids[1:]:parent[root(i)]=r
components={}
for f in original.data.polygons:
 if f.material_index not in bark_indices:continue
 r=root(vertex_key[f.vertices[0]]);c=components.setdefault(r,{'faces':[],'vertices':set()});c['faces'].append(f.index);c['vertices'].update(f.vertices)
trunks=[]
for ident,c in components.items():
 ps=[world[i] for i in c['vertices']];lo=Vector([min(p[k] for p in ps) for k in range(3)]);hi=Vector([max(p[k] for p in ps) for k in range(3)])
 if hi.z-lo.z>1 and len(c['vertices'])>30:trunks.append(dict(id=ident,lo=lo,hi=hi,centre=(lo+hi)*.5,faces=c['faces']))
groups=[]
for t in sorted(trunks,key=lambda t:t['centre'].x):
 group=next((g for g in groups if (t['centre'].xy-g['centre'].xy).length<.35),None)
 if group is None:group={'centre':t['centre'].copy(),'trunks':[]};groups.append(group)
 group['trunks'].append(t)
targets=[]
for xy in [(32,59),(5,57),(-5,54)]:
 g=min(groups,key=lambda g:(g['centre'].x-xy[0])**2+(g['centre'].y-xy[1])**2)
 assert (g['centre'].x-xy[0])**2+(g['centre'].y-xy[1])**2<4
 assert g not in targets;targets.append(g)
def nearest(p):return min(range(len(groups)),key=lambda i:(p.x-groups[i]['centre'].x)**2+(p.y-groups[i]['centre'].y)**2)
target_ids=[groups.index(g) for g in targets]
leaf_faces={i:[] for i in target_ids}
for f in original.data.polygons:
 if f.material_index not in leaf_indices:continue
 p=sum((world[i] for i in f.vertices),Vector())/len(f.vertices);i=nearest(p)
 if i in leaf_faces:leaf_faces[i].append(f.index)
for i in target_ids:
 g=groups[i];ids={v for f in leaf_faces[i] for v in original.data.polygons[f].vertices};ps=[world[v] for v in ids];g['leaf_lo']=Vector([min(p[k] for p in ps) for k in range(3)]);g['leaf_hi']=Vector([max(p[k] for p in ps) for k in range(3)])
context={'scope':'Three existing pines near Bastion ONLY. Other scene geometry, heightfield, tree locations and architecture preserved.',
 'actual_topology':{'source_sha256':expected,'tree_groups':len(groups),'targets':[{'centre':list(g['centre']),'overlapping_bark_components':len(g['trunks']),'leaf_bounds':[list(g['leaf_lo']),list(g['leaf_hi'])]} for g in targets]},
 'observed_unity':'Native74/73 authentic image: flat sawtooth pine foliage. Existing CC0-derived medium source combines tall and medium branches; original manifest warns about duplication. Actual topology shows several nearly coincident bark components per tree. Keep tallest existing trunk, remove overlapping duplicate trunks in target copy only, and replace target planar foliage with closed curved bough volumes. Do not stack another tree over existing canopy.',
 'reference':'Owner target has layered evergreen crowns with rounded branch masses, visible dark interstices, drooping branch tips and coherent taper. Match complete silhouette and shading from real player camera, not tiny color tweaks.',
 'decision':'Choose bough layer count, radial branching, fullness, droop, taper, crown width and palette BEFORE actual Blender geometry. Only typed approved operations will execute. No scores or approval.'}
fields={'crown_layers':[5,6,7],'radial_boughs':[6,7,8],'bough_fullness':[.26,.34,.42],'tip_droop':[.10,.20,.30],'crown_taper':[1.05,1.25,1.45],'width_ratio':[.84,.92,1.0],'core_ratio':[.22,.28,.34],'foliage_palette':[0,1,2]}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'number','enum':v} for k,v in fields.items()}}}
payload=dict(model='qwen3-vl:8b-instruct-q4_K_M',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are Eldoria local 3D art director. Select a coherent architectural and landscape treatment from actual observed source, not a score. Choose concrete geometric parameters and explain the composition. Output JSON only; never claim approval.'},{'role':'user','content':json.dumps(context),'images':[base64.b64encode((EV/n).read_bytes()).decode() for n in ['ground73-after-bastion.png','reference-target.jpeg']]}],options={'temperature':.35,'num_ctx':8192,'num_predict':1500})
write('pine-volume77-context.json',context);write('pine-volume77-qwen-request.json',{'model':payload['model'],'format':schema,'context':context,'image_sha256':{n:digest(EV/n) for n in ['ground73-after-bastion.png','reference-target.jpeg']}})
def api(path,data=None,timeout=3):
 req=urllib.request.Request('http://127.0.0.1:11434'+path,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=timeout) as response:return json.load(response)
server=None;log=None
try:
 try:api('/api/version')
 except Exception:
  users=pathlib.Path(os.environ.get('SystemDrive','C:')+'/Users');installs=list(users.glob('*/AppData/Local/Programs/Ollama/ollama.exe'));assert installs
  env=os.environ.copy();env['OLLAMA_HOST']='127.0.0.1:11434';models=list(users.glob('*/.ollama/models'))
  if models:env['OLLAMA_MODELS']=str(models[0])
  log=open(EV/'pine-volume77-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 assert any(m['name']=='qwen3-vl:8b-instruct-q4_K_M' for m in api('/api/tags')['models'])
 response=api('/api/chat',payload,360);write('pine-volume77-qwen-response.json',response);plan=json.loads(response['message']['content'])
 assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('pine-volume77-validated-plan.json',{'plan':plan,'art_approved':False})
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()

tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
names={'Mesh','execute_via_blendermcp'}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in names],type_ignores=[]),'<reviewed mesh tools>','exec'))
RejectedPlan=RuntimeError;stone_trial=False
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True
 p=next((n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
 if p is None:p=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
 out=next((n for n in m.node_tree.nodes if n.type=='OUTPUT_MATERIAL'),None)
 if out is None:out=m.node_tree.nodes.new('ShaderNodeOutputMaterial')
 m.node_tree.links.new(p.outputs['BSDF'],out.inputs['Surface']);p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.9
 return m
palette=[(.024,.065,.018),(.030,.075,.019),(.020,.058,.022)][int(plan['foliage_palette'])]
leaves=[mat('Native77 sculpted pine '+str(i),tuple(c*(.77+.16*i) for c in palette)) for i in range(4)]
created=[];proof={}
def perform(action):
 assert action==operation and not created
 rng=random.Random(77018);canopy=Mesh('Pine77 closed boughs')
 removed=set()
 for gid in target_ids:
  g=groups[gid];highest=max(g['trunks'],key=lambda t:t['hi'].z);cx=highest['centre'].x;cy=highest['centre'].y;base=highest['lo'].z
  height=g['leaf_hi'].z-base;radius=min((g['leaf_hi'].x-g['leaf_lo'].x)*.5,(g['leaf_hi'].y-g['leaf_lo'].y)*.5)*plan['width_ratio']
  removed.update(leaf_faces[gid])
  for t in g['trunks']:
   if t is not highest:removed.update(t['faces'])
  for layer in range(int(plan['crown_layers'])):
   t=layer/max(1,plan['crown_layers']-1);z=base+height*(.22+.70*t);r=radius*(1-.85*t)**plan['crown_taper']
   for branch in range(int(plan['radial_boughs'])):
    a=2*pi*branch/plan['radial_boughs']+layer*2.39996+rng.uniform(-.10,.10)
    u=Vector((cos(a),sin(a),0));v=Vector((-sin(a),cos(a),0));verts=[]
    for j in range(6):
     f=j/5;centre=Vector((cx,cy,z))+u*r*(.06+.94*f)
     centre.z+=r*(.22*sin(pi*f)-plan['tip_droop']*f)
     width=max(.018,r*plan['bough_fullness']*sin(pi*f)**.6)
     for k in range(8):
      angle=2*pi*k/8;point=centre+v*cos(angle)*width+Vector((0,0,sin(angle)*width*.62));verts.append(tuple(point))
    faces=[tuple(reversed(range(8))),tuple(range(40,48))]
    for j in range(5):
     for k in range(8):faces.append((j*8+k,j*8+(k+1)%8,(j+1)*8+(k+1)%8,(j+1)*8+k))
    canopy.add(verts,faces,leaves[min(3,int(t*3)+branch%2)])
  # A narrow continuous inner crown joins branch tiers without leaving artificial gaps.
  vv=[]
  for j in range(6):
   t=j/5;rr=max(.014,radius*plan['core_ratio']*(1-t)**.8)
   for k in range(8):vv.append((cx+cos(k*pi/4)*rr,cy+sin(k*pi/4)*rr,base+height*(.18+.82*t)))
  ff=[tuple(reversed(range(8))),tuple(range(40,48))]
  for j in range(5):
   for k in range(8):ff.append((j*8+k,j*8+(k+1)%8,(j+1)*8+(k+1)%8,(j+1)*8+k))
  canopy.add(vv,ff,leaves[1])
 new=canopy.finish(0)
 for f in new.data.polygons:f.use_smooth=True
 new.data.update()
 # Rebuild only the face list, preserving every outside vertex, UV and corner normal.
 # Blender5 Mesh.corner_normals / normals_split_custom_set are the documented native APIs.
 kept=[f for f in original.data.polygons if f.index not in removed];inv=original.matrix_world.inverted();normal=original.matrix_world.transposed().to_3x3()
 verts=[tuple(v.co) for v in original.data.vertices]+[tuple(inv@v.co) for v in new.data.vertices];offset=len(original.data.vertices)
 faces=[tuple(f.vertices) for f in kept]+[tuple(offset+i for i in f.vertices) for f in new.data.polygons]
 mesh=bpy.data.meshes.new('Fir tree group with three sculpted crowns');mesh.from_pydata(verts,[],faces);mesh.update()
 materials=list(original.data.materials)
 for m in new.data.materials:
  if m not in materials:materials.append(m)
 for m in materials:mesh.materials.append(m)
 for f,src in zip(mesh.polygons,kept):f.material_index=src.material_index;f.use_smooth=src.use_smooth
 for f,src in zip(list(mesh.polygons)[len(kept):],new.data.polygons):f.material_index=materials.index(new.data.materials[src.material_index]);f.use_smooth=True
 for old_uv in original.data.uv_layers:
  uv=mesh.uv_layers.new(name=old_uv.name);i=0
  for f in kept:
   for li in f.loop_indices:uv.data[i].uv=old_uv.data[li].uv;i+=1
  for f in new.data.polygons:
   for li in f.loop_indices:uv.data[i].uv=new.data.uv_layers.active.data[li].uv;i+=1
 normals=[tuple(original.data.corner_normals[li].vector) for f in kept for li in f.loop_indices]
 normals += [tuple((normal@new.data.corner_normals[li].vector).normalized()) for f in new.data.polygons for li in f.loop_indices]
 assert len(normals)==len(mesh.loops);mesh.normals_split_custom_set(normals)
 obj=bpy.data.objects.new('Fir_tree_group_Canopy77',mesh);bpy.context.collection.objects.link(obj);obj.matrix_world=original.matrix_world.copy()
 assert all((obj.data.vertices[i].co-original.data.vertices[i].co).length==0 for f in kept for i in f.vertices)
 proof.update({'removed_faces':len(removed),'retained_original_faces':len(kept),'retained_vertices_uvs_normals':True,'target_groups':len(targets),'overlapping_trunks_removed':sum(len(g['trunks'])-1 for g in targets)})
 bpy.data.objects.remove(new,do_unlink=True);created.append(obj)
operation={'id':'pine-closed-volume-native77','op':'replace_three_canopies_remove_duplicate_trunks','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert len(created)==1 and all(stamp(bpy.data.objects[n])==s for n,s in protected.items())
original.hide_render=True;bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'PineCanopy77.blend'))
bpy.ops.object.select_all(action='DESELECT');created[0].select_set(True);bpy.context.view_layer.objects.active=created[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'PineCanopy77.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected
write('pine-volume77-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'source_preserved':True,'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','qwen_model':payload['model'],'plan':plan,'scope_proof':proof,'glb_sha256':digest(OUT/'PineCanopy77.glb'),'blend_sha256':digest(EV/'PineCanopy77.blend'),'art_approved':False,'triangles':sum(len(p.vertices)-2 for p in created[0].data.polygons)})
print('VALORIA_PINE_VOLUME_BLENDER_COMPLETE')
