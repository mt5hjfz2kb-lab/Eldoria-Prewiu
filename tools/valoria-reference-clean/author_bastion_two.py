"""Bastion II evolves the retained70 mesh; I, functional base and whole world stay intact."""
import bpy,bmesh,ast,array,base64,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
from mathutils import Vector,Matrix
from math import sin,cos,pi
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean';OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE=EV/'BastionEnsemble70.blend'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(EV/n).write_text(json.dumps(v,indent=2),encoding='utf8')
expected='635c948163ee618fcbf6e4b63585db864ef441f590aa50dd0ccc3f6c25ace644';assert digest(SOURCE)==expected
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
original=bpy.data.objects['Bastion_Ensemble_Candidate']
def stamp(o):
 a=array.array('f',[0])*(len(o.data.vertices)*3);o.data.vertices.foreach_get('co',a)
 return (tuple(v for row in o.matrix_world for v in row),hashlib.sha256(a.tobytes()).hexdigest())
protected={o.name:stamp(o) for o in bpy.context.scene.objects if o.type=='MESH'}
# Actual connected components before Qwen: crown pieces are moved intact, never stretched.
me=original.data;parents=list(range(len(me.vertices)))
def root(i):
 while parents[i]!=i:parents[i]=parents[parents[i]];i=parents[i]
 return i
for edge in me.edges:
 a,b=map(root,edge.vertices);parents[b]=a
components={}
for v in me.vertices:components.setdefault(root(v.index),[]).append(v.index)
crown=[];bounds=[]
for ids in components.values():
 points=[me.vertices[i].co for i in ids];lo=[min(p[k] for p in points) for k in range(3)];hi=[max(p[k] for p in points) for k in range(3)]
 if lo[0]>-3.1 and hi[0]<3.1 and lo[1]>25.2 and hi[1]<31.1 and lo[2]>10.22:
  crown.extend(ids);bounds.append([lo,hi])
assert len(crown)>500 and max(me.vertices[i].co.z for i in crown)>16
memory=EV/('bastion2-memory-'+os.environ.get('GITHUB_RUN_ID','local')+'.txt')
subprocess.run(['node','tools/art-direction/learning-evidence/cycle-hooks.mjs','prepare','--target','bastion1','--out',str(memory)],check=True)
context={'source_sha256':expected,'mesh_vertices':len(me.vertices),'central_crown_component_count':len(bounds),'crown_bounds':bounds[:6],
 'scope':'Evolution I to II of retained Bastion only. Preserve all lower geometry, door, six stairs, footprint, surrounding plants and camera. Move existing central crown plus flag upward intact; fill intervening tower wall with matching masonry and new recessed upper window. Add two coherent blue slate hip roofs inside flank tower crowns. Existing tower bodies/merlons remain. No recolor substitution, no unrelated second castle.',
 'actual_unity_images':['Retained74/77-before closeup','original style reference'],
 'art_direction':'Recognizable second tier: central watch chamber and stronger blue-roof skyline, compact warm limestone identity. Choose dimensions within safe bounds from observed I; do not score approval. Retain main hall roof and functional base. Original I must be swapped, never overlaid with II.',
 'verified_recent_lessons':'Native70 windows/portal/cornice improved actual Unity. Materials must reuse validated URP donor maps, transient glTF references previously became white. Native77 pine lobes rejected; vegetation unchanged. Preserve BakedGI and noBatchingStatic.',
 'historic_memory':memory.read_text(encoding='utf8'),'memory_sha256':digest(memory)}
fields={'central_raise':[1.8,2.1,2.4],'upper_window_width':[.65,.80,.95],'flank_roof_rise':[1.25,1.6,1.95],'flank_roof_inset':[.48,.60,.70],'roof_rows':[6,8,10]}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'number','enum':v} for k,v in fields.items()}}}
images=['pine77-before-bastion.png','reference-target.jpeg']
payload=dict(model='qwen3-vl:8b-instruct-q4_K_M',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are Eldoria local 3D artist. Inspect genuine images and measured source; direct the proposed Bastion II evolution through concrete geometry decisions. No score or approval. JSON only.'},{'role':'user','content':json.dumps(context),'images':[base64.b64encode((EV/n).read_bytes()).decode() for n in images]}],options={'temperature':.3,'num_ctx':8192,'num_predict':1400})
write('bastion2-context.json',context);write('bastion2-qwen-request.json',{'model':payload['model'],'context':context,'schema':schema,'image_sha256':{n:digest(EV/n) for n in images}})
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
  log=open(EV/'bastion2-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 response=api('/api/chat',payload,360);write('bastion2-qwen-response.json',response);plan=json.loads(response['message']['content'])
 assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('bastion2-plan.json',plan)
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()
# Reuse reviewed masonry/UV tools, not full scene builder or original authoring cycle.
tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
names={'Mesh','line','arch','execute_via_blendermcp'}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in names],type_ignores=[]),'<reviewed geometry tools>','exec'))
ensemble=ast.parse((ROOT/'tools/valoria-reference-clean/author_bastion_ensemble.py').read_text())
exec(compile(ast.Module(body=[n for n in ensemble.body if isinstance(n,ast.FunctionDef) and n.name in {'courses','window','quoins'}],type_ignores=[]),'<reviewed masonry tools>','exec'))
RejectedPlan=RuntimeError
stone=[bpy.data.materials['Warm limestone %d'%i] for i in range(6)];trial_stone=[bpy.data.materials['Trial sandstone %d'%i] for i in range(6)]
blue=[bpy.data.materials['Blue slate %d'%i] for i in range(5)]
for var,name in [('trim','Carved limestone'),('mortar','Recessed stone joints'),('wood','Oak'),('gold','Heraldic gold'),('dark','Bastion recess shadow')]:globals()[var]=bpy.data.materials[name]
stone_trial=stone_global=True;surface_trial_stats={'mapped_boxes':0}
patch=next(n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='stone_patches' for t in n.targets));exec(compile(ast.Module(body=[patch],type_ignores=[]),'<reviewed UV patches>','exec'))
# Shared helper parameters retain the existing coursing; creative values come from current inference.
plan['course_height']=.58;plan['quoin_width']=.65
created=[]
def perform(action):
 assert action==operation and not created
 candidate=original.copy();candidate.data=original.data.copy();candidate.name='Bastion_II';bpy.context.collection.objects.link(candidate)
 for i in crown:candidate.data.vertices[i].co.z+=plan['central_raise']
 candidate.data.update()
 g=Mesh('Bastion_I');random.seed(78018);rise=plan['central_raise'];window_z=10.85;window_h=min(1.6,rise-.55)
 courses(g,0,28.2,10.68,4.8,4.4,rise,openings=[(0,window_z,plan['upper_window_width'],window_h)])
 window(g,0,25.90,window_z,plan['upper_window_width'],window_h)
 quoins(g,0,28.2,10.68,4.8,4.4,rise)
 # Four tapered, overlapping tiled hip faces: readable complete roofs within crenellated flank rims.
 for x in [-8.4,8.4]:
  wx=3.5/2-plan['flank_roof_inset'];dy=5.2/2-plan['flank_roof_inset'];base=7.52;peak=base+plan['flank_roof_rise'];rows=int(plan['roof_rows'])
  corners=[Vector((x-wx,29.5-dy,base)),Vector((x+wx,29.5-dy,base)),Vector((x+wx,29.5+dy,base)),Vector((x-wx,29.5+dy,base))];tip=Vector((x,29.5,peak))
  for face in range(4):
   A=corners[face];B=corners[(face+1)%4]
   for row in range(rows):
    a=row/rows;b=min(.985,(row+1.08)/rows);la=A.lerp(tip,a);ra=B.lerp(tip,a);lb=A.lerp(tip,b);rb=B.lerp(tip,b);cols=max(1,round((ra-la).length/.43))
    for col in range(cols):
     u=(col+.012)/cols;v=(col+.988)/cols;p=[la.lerp(ra,u),la.lerp(ra,v),lb.lerp(rb,v),lb.lerp(rb,u)];normal=(p[1]-p[0]).cross(p[2]-p[0]).normalized();q=[v-normal*.055 for v in p]
     g.add([tuple(v) for v in p+q],[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],blue[1+(col+row)%3])
   line(g,tuple(A),tuple(tip),.075,blue[3])
  g.box((x,29.5,peak+.1),(.18,.18,.22),gold)
 appendix=g.finish(.025,1);appendix.matrix_world=original.matrix_world.copy()
 bpy.ops.object.select_all(action='DESELECT');candidate.select_set(True);appendix.select_set(True);bpy.context.view_layer.objects.active=candidate;bpy.ops.object.join()
 created.append(candidate)
operation={'id':'bastion-two-evolution','op':'raise_existing_crown_and_author_watch_chamber_and_flank_hip_roofs','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert len(created)==1 and all(stamp(bpy.data.objects[n])==s for n,s in protected.items())
candidate=created[0]
# Joining preserves copied original vertex order; all functional lower vertices must match exactly.
for i,v in enumerate(original.data.vertices):
 if v.co.z<10.22:assert (candidate.data.vertices[i].co-v.co).length<1e-6
for o in bpy.context.scene.objects:o.hide_render=o!=candidate
# Structural preview only: identical Blender camera/light, explicitly not a Unity capture.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
scene.render.resolution_x=960;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.world=scene.world or bpy.data.worlds.new('Bastion study world');scene.world.use_nodes=True
scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.25,.28,.31,1)
scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.6
bpy.ops.object.camera_add(location=(-24,-8,31));camera=bpy.context.object;camera.name='Bastion I-II structural preview camera';camera.rotation_euler=(Vector((0,35.5,7))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=29;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-20,15,30));light=bpy.context.object;light.data.energy=3500;light.data.shape='DISK';light.data.size=12;light.rotation_euler=(Vector((0,32,5))-light.location).to_track_quat('-Z','Y').to_euler()
for label,obj in [('before',original),('after',candidate)]:
 original.hide_render=obj!=original;candidate.hide_render=obj!=candidate
 scene.render.filepath=str(EV/('bastion2-blender-'+label+'.png'));bpy.ops.render.render(write_still=True)
original.hide_render=True;candidate.hide_render=False
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BastionII.blend'))
bpy.ops.object.select_all(action='DESELECT');candidate.select_set(True);bpy.context.view_layer.objects.active=candidate
bpy.ops.export_scene.gltf(filepath=str(OUT/'BastionII.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected
write('bastion2-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'source_blend_sha256':expected,'source_preserved':True,'lower_geometry_unchanged':True,'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','qwen_model':payload['model'],'moved_crown_vertices':len(crown),'plan':plan,'glb_sha256':digest(OUT/'BastionII.glb'),'blend_sha256':digest(EV/'BastionII.blend'),'triangles':sum(len(p.vertices)-2 for p in candidate.data.polygons),'art_approved':False})
print('VALORIA_BASTION_TWO_BLENDER_COMPLETE')
