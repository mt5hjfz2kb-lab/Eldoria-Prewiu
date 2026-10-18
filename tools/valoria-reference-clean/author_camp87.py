"""Current Valoria camp cloth study: original source preserved, real local AI before typed MCP edits."""
import bpy,bmesh,ast,array,base64,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
from mathutils import Vector
from math import sin,cos,pi
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean';OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE=ROOT/'art-source/valoria/production/camp-family-v1/CampFamilyV1.blend'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(EV/n).write_text(json.dumps(v,indent=2),encoding='utf8')
expected='36fc16fab611198dcf9bc60767e3dd103bef53cb9c602394dbe4cc741b1b2b7f'
assert digest(SOURCE)==expected
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
originals=[o for o in bpy.context.scene.objects if o.type=='MESH']
def stamp(o):
 a=array.array('f',[0])*(len(o.data.vertices)*3);o.data.vertices.foreach_get('co',a)
 return (tuple(v for row in o.matrix_world for v in row),hashlib.sha256(a.tobytes()).hexdigest())
protected={o.name:stamp(o) for o in originals}
def has(o,suffix):return any(m and m.name.endswith(suffix) for m in o.data.materials)
tent=next(o for o in originals if has(o,'fabric_main'))
points=[tent.matrix_world@v.co for v in tent.data.vertices];lo=[min(p[k] for p in points) for k in range(3)];hi=[max(p[k] for p in points) for k in range(3)]
all_points=[o.matrix_world@v.co for o in originals for v in o.data.vertices]
world_lo=[min(p[k] for p in all_points) for k in range(3)];world_hi=[max(p[k] for p in all_points) for k in range(3)]
assert 4<hi[0]-lo[0]<5.5 and 2.4<hi[2]-lo[2]<3.3
scene=bpy.context.scene
for o in scene.objects:
 if o.type in {'LIGHT','CAMERA'}:o.hide_render=True
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=1100;scene.render.resolution_y=825;scene.render.resolution_percentage=100
scene.world=scene.world or bpy.data.worlds.new('Camp cloth study world');scene.world.use_nodes=True
nodes=scene.world.node_tree.nodes;bg=next((n for n in nodes if n.type=='BACKGROUND'),None) or nodes.new('ShaderNodeBackground');wo=next((n for n in nodes if n.type=='OUTPUT_WORLD'),None) or nodes.new('ShaderNodeOutputWorld');scene.world.node_tree.links.new(bg.outputs[0],wo.inputs['Surface']);bg.inputs[0].default_value=(.34,.39,.46,1);bg.inputs[1].default_value=.55
centre=Vector(((world_lo[0]+world_hi[0])/2,(world_lo[1]+world_hi[1])/2,(world_lo[2]+world_hi[2])/2))
bpy.ops.object.camera_add(location=centre+Vector((-11,-14,10)));camera=bpy.context.object;camera.name='Camp87 fixed structural camera';camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=11.8;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=centre+Vector((-6,-8,12)));light=bpy.context.object;light.data.energy=1800;light.data.size=7;light.rotation_euler=(centre-light.location).to_track_quat('-Z','Y').to_euler()
def render(name):scene.render.filepath=str(EV/name);bpy.ops.render.render(write_still=True)
render('camp87-blender-before.png')
# The existing history is actually delivered to the new inference; it is not training or approval.
memory_path=EV/('camp87-memory-'+os.environ.get('GITHUB_RUN_ID','local')+'.txt')
subprocess.run(['node','tools/art-direction/learning-evidence/cycle-hooks.mjs','prepare','--target','bastion1','--out',str(memory_path)],check=True,cwd=ROOT)
memory=memory_path.read_text(encoding='utf8')
context={'source_sha256':expected,'bounds_world_blender':{'tent':[lo,hi],'ensemble':[world_lo,world_hi]},'meshes':[{'name':o.name,'vertices':len(o.data.vertices),'materials':[m.name for m in o.data.materials if m]} for o in originals],
 'brief':'Current existing functional level-I barracks is a solid orange wedge. Keep the exact tent footprint, height and all poles/supplies/bench. Remodel ONLY cloth shell and ropes: curved taut roof with visible seams, actual open entrance with folded side flaps, sagging awning. Prefer restrained blue/sand cloth coherent with current Bastion; no castle replacement, new props or crowd. No closed fake black doorway. Preserve library source and gameplay parcel. Select a coherent full geometric treatment from the typed operations and values below, explain silhouette and material choices. No scores or approval.',
 'history':memory[:10500],'history_sha256':hashlib.sha256(memory[:10500].encode()).hexdigest(),'history_source_sha256':digest(memory_path)}
fields={'roof_sag':[.12,.20,.28],'ridge_sag':[.04,.08,.12],'entry_width':[1.05,1.3,1.5],'entry_height':[1.8,2.05,2.25],'fold_depth':[.18,.30,.42],'palette':['slate_blue','weathered_blue','muted_olive'],'seam_count':[5,7,9],'trim_width':[.07,.11,.15],'awning_sag':[.10,.18,.26],'rope_radius':[.022,.030,.040]}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'string' if isinstance(v[0],str) else 'number','enum':v} for k,v in fields.items()}}}
images=['camp87-blender-before.png','chapter2-city-built.png','bastion2-after-bastion.png']
payload=dict(model='qwen3-vl:8b-instruct-q4_K_M',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are Eldoria local 3D art director. Observe actual source topology, fixed Blender before render, actual Unity camp and retained Bastion reference. Direct substantial real cloth geometry and integration through typed operations. Output JSON only. Never approve your own output.'},{'role':'user','content':json.dumps(context),'images':[base64.b64encode((EV/n).read_bytes()).decode() for n in images]}],options={'temperature':.3,'num_ctx':8192,'num_predict':1700})
write('camp87-context.json',context);write('camp87-qwen-request.json',{'model':payload['model'],'schema':schema,'context':context,'image_sha256':{n:digest(EV/n) for n in images}})
def api(path,data=None,timeout=3):
 req=urllib.request.Request('http://127.0.0.1:11434'+path,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=timeout) as r:return json.load(r)
server=None;log=None
try:
 try:api('/api/version')
 except Exception:
  users=pathlib.Path(os.environ.get('SystemDrive','C:')+'/Users');installs=list(users.glob('*/AppData/Local/Programs/Ollama/ollama.exe'));assert installs
  env=os.environ.copy();env['OLLAMA_HOST']='127.0.0.1:11434';models=list(users.glob('*/.ollama/models'))
  if models:env['OLLAMA_MODELS']=str(models[0])
  log=open(EV/'camp87-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 assert any(m['name']==payload['model'] for m in api('/api/tags')['models'])
 response=api('/api/chat',payload,360);write('camp87-qwen-response.json',response);plan=json.loads(response['message']['content']);assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('camp87-plan.json',plan)
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()
tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in {'Mesh','line','execute_via_blendermcp'}],type_ignores=[]),'<reviewed authoring tools>','exec'))
RejectedPlan=RuntimeError;stone_trial=False
def mat(name,color):
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*color,1)
 p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=.94
 return m
color={'slate_blue':(.065,.13,.22),'weathered_blue':(.10,.17,.24),'muted_olive':(.17,.20,.095)}[plan['palette']]
cloth=mat('Camp87 woven canvas',color);linen=mat('Camp87 linen seams',(.42,.34,.23));rope=mat('Camp87 hemp ropes',(.29,.23,.15))
image=bpy.data.images.new('Camp87 canvas weave',width=256,height=256);pixels=[]
for y in range(256):
 for x in range(256):
  shade=1+.025*sin(x*pi/2)*cos(y*pi/2)+.025*sin(x*.17+y*.13)
  pixels.extend([min(1,c*shade) for c in color]+[1])
image.pixels.foreach_set(pixels);image.filepath_raw=str(OUT/'Camp87Canvas.png');image.file_format='PNG';image.save();image.pack()
node=cloth.node_tree.nodes.new('ShaderNodeTexImage');node.image=image;cloth.node_tree.links.new(node.outputs['Color'],next(n for n in cloth.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'])
created=[]
def perform(action):
 assert action==operation and not created
 for old in originals:
  if old==tent or (has(old,'rope') and len(old.data.materials)==1):continue
  copy=old.copy();copy.data=old.data.copy();copy.name='Camp87 '+old.name;bpy.context.collection.objects.link(copy);created.append(copy)
  if has(copy,'fabric_secondary'):
   bm=bmesh.new();bm.from_mesh(copy.data);ids={i for i,m in enumerate(copy.data.materials) if m and m.name.endswith('fabric_secondary')};faces=[f for f in bm.faces if f.material_index in ids];edges=list({e for f in faces for e in f.edges});bmesh.ops.subdivide_edges(bm,edges=edges,cuts=7,use_grid_fill=True)
   faces=[f for f in bm.faces if f.material_index in ids];verts={v for f in faces for v in f.verts};ps=[copy.matrix_world@v.co for v in verts];mn=[min(p[k] for p in ps) for k in range(3)];mx=[max(p[k] for p in ps) for k in range(3)]
   for v in verts:
    p=copy.matrix_world@v.co;u=(p.x-mn[0])/max(.01,mx[0]-mn[0]);t=(p.y-mn[1])/max(.01,mx[1]-mn[1]);p.z-=plan['awning_sag']*sin(pi*u)*sin(pi*t);v.co=copy.matrix_world.inverted()@p
   bm.to_mesh(copy.data);bm.free();copy.data.update()
 g=Mesh('Camp87 sculpted canvas');details=Mesh('Camp87 seams and guys');cx=(lo[0]+hi[0])/2;xf=lo[1];yb=hi[1];z=lo[2];half=(hi[0]-lo[0])/2;peak=hi[2];eave=z+.25*(peak-z);depth=yb-xf
 def roof(x,t):
  u=abs(x-cx)/half
  return peak-(peak-eave)*u-plan['roof_sag']*sin(pi*u)-plan['ridge_sag']*sin(pi*t)*(1-u)+.018*sin(t*6*pi)*sin(pi*u)
 for side in [-1,1]:
  for i in range(16):
   for j in range(20):
    v=[]
    for u,t in [(i/16,j/20),((i+1)/16,j/20),((i+1)/16,(j+1)/20),(i/16,(j+1)/20)]:
     x=cx+side*half*u;v.append((x,xf+depth*t,roof(x,t)))
    g.add(v,[(0,1,2,3)],cloth)
  g.add([(cx+side*half,xf,z),(cx+side*half,yb,z),(cx+side*half,yb,eave),(cx+side*half,xf,eave)],[(0,1,2,3)],cloth)
  for seam in range(int(plan['seam_count'])+1):
   t=seam/plan['seam_count'];old=None
   for k in range(17):
    x=cx+side*half*k/16;p=(x,xf+depth*t,roof(x,t)+.008)
    if old:line(details,old,p,plan['trim_width']*.12,linen)
    old=p
 for end,y,direction in [('front',xf,-1),('rear',yb,1)]:
  door=plan['entry_width']/2;top=z+plan['entry_height'];cuts=sorted(set([cx-half+2*half*k/32 for k in range(33)]+[cx-door,cx+door]));t=0 if end=='front' else 1
  for a,b in zip(cuts,cuts[1:]):
   inside=abs((a+b)/2-cx)<door;bottom=top if inside else z
   g.add([(a,y,bottom),(b,y,bottom),(b,y,roof(b,t)),(a,y,roof(a,t))],[(0,1,2,3)],cloth)
  for side in [-1,1]:
   # Folded fabric lies beside a genuine open aperture; the centre has no wall or black plug.
   for i in range(6):
    for j in range(12):
     v=[]
     for u,tv in [(i/6,j/12),((i+1)/6,j/12),((i+1)/6,(j+1)/12),(i/6,(j+1)/12)]:
      v.append((cx+side*(door+.38*u),y+direction*(plan['fold_depth']*sin(pi*tv)+.055*sin(u*4*pi)),z+plan['entry_height']*tv))
     g.add(v,[(0,1,2,3)],cloth)
   line(details,(cx+side*door,y+direction*.015,z),(cx+side*door,y+direction*.015,top),plan['trim_width']*.15,linen)
  line(details,(cx-door,y,top),(cx+door,y,top),plan['trim_width']*.15,linen)
 for side in [-1,1]:
  for t in [.06,.94]:
   start=Vector((cx+side*half,xf+depth*t,eave));end=Vector((max(world_lo[0]+.05,min(world_hi[0]-.05,cx+side*(half+.62))),max(world_lo[1]+.05,min(world_hi[1]-.05,xf+depth*t+(-.45 if t<.5 else .45))),z-.02))
   old=start
   for k in range(1,13):
    p=start.lerp(end,k/12);p.z-=.028*sin(pi*k/12);line(details,old,p,plan['rope_radius'],rope);old=p
   line(details,end-Vector((0,0,.15)),end+Vector((.04,0,.18)),.045,rope)
 fabric=g.finish(0);solid=fabric.modifiers.new('Editable canvas thickness','SOLIDIFY');solid.thickness=.018;bpy.context.view_layer.objects.active=fabric;bpy.ops.object.modifier_apply(modifier=solid.name)
 for p in fabric.data.polygons:p.use_smooth=True
 created.extend([fabric,details.finish(0)])
operation={'id':'camp87-cloth-geometry','op':'remesh_taut_cloth_open_both_entrances_fold_flaps_sculpt_awning_and_rebuild_guys','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert created and all(stamp(bpy.data.objects[n])==v for n,v in protected.items()) and digest(SOURCE)==expected
for old in originals:old.hide_render=True
render('camp87-blender-after.png')
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'CampCloth87.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in created:o.select_set(True)
bpy.context.view_layer.objects.active=created[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'CampCloth87.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
write('camp87-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'original_blend_sha256':expected,'original_source_preserved':True,'original_scene_not_modified':True,'qwen_model':payload['model'],'memory_sha256':digest(memory_path),'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','plan':plan,'glb_sha256':digest(OUT/'CampCloth87.glb'),'blend_sha256':digest(EV/'CampCloth87.blend'),'triangles':sum(len(p.vertices)-2 for o in created for p in o.data.polygons),'art_approved':False,'unity_capture_pending':True})
print('VALORIA_CAMP_AUTHOR_COMPLETE')
