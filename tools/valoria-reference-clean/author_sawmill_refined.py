"""Current Valoria sawmill architectural study: original source preserved, real local AI before typed MCP edits."""
import bpy,bmesh,ast,array,base64,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
from mathutils import Vector
from math import sin,cos,pi
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean';OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE=ROOT/'art-source/valoria/production/sawmill-family-v1/state2/SawmillState2V1.blend'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(EV/n).write_text(json.dumps(v,indent=2),encoding='utf8')
expected='7e7f6719806cadfef7a223bc6ef664b820ab17bc66db32fe6b27486d4236fec3'
assert digest(SOURCE)==expected
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
originals=[o for o in bpy.context.scene.objects if o.type=='MESH']
points=[o.matrix_world@v.co for o in originals for v in o.data.vertices]
lo=Vector(tuple(min(p[k] for p in points) for k in range(3)));hi=Vector(tuple(max(p[k] for p in points) for k in range(3)))
# Normalize only a review copy, never the protected library source, to a 10m envelope.
scale=10/max(hi.x-lo.x,hi.y-lo.y);origin=Vector(((hi.x+lo.x)/2,(hi.y+lo.y)/2,lo.z))
for o in originals:
 o.matrix_world.translation-=origin;o.matrix_world.translation*=scale;o.scale*=scale
scene=bpy.context.scene
for o in scene.objects:
 if o.type in {'LIGHT','CAMERA'}:o.hide_render=True
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=1100;scene.render.resolution_y=825;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Sawmill comparison');scene.world.use_nodes=True
nodes=scene.world.node_tree.nodes;bg=next((n for n in nodes if n.type=='BACKGROUND'),None) or nodes.new('ShaderNodeBackground');wo=next((n for n in nodes if n.type=='OUTPUT_WORLD'),None) or nodes.new('ShaderNodeOutputWorld');scene.world.node_tree.links.new(bg.outputs[0],wo.inputs['Surface']);bg.inputs[0].default_value=(.34,.39,.46,1);bg.inputs[1].default_value=.55
centre=Vector((0,0,2.5))
bpy.ops.object.camera_add(location=centre+Vector((-13,-17,12)));camera=bpy.context.object;camera.rotation_euler=(centre-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=16;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-6,-8,14));light=bpy.context.object;light.data.energy=2200;light.data.size=7
light.rotation_euler=(centre-light.location).to_track_quat('-Z','Y').to_euler()
def render(name):scene.render.filepath=str(EV/name);bpy.ops.render.render(write_still=True)
render('sawmill-refined-before.png')
memory_path=EV/('sawmill-refined-memory-'+os.environ.get('GITHUB_RUN_ID','local')+'.txt')
subprocess.run(['node','tools/art-direction/learning-evidence/cycle-hooks.mjs','prepare','--target','bastion1','--out',str(memory_path)],check=True,cwd=ROOT)
memory=memory_path.read_text(encoding='utf8')
context={'source_sha256':expected,'source_bounds':[list(lo),list(hi)],'history':memory[:10500],'history_sha256':hashlib.sha256(memory[:10500].encode()).hexdigest(),
'brief':'Owner rejects the existing sawmill as incoherent with Valoria. Design a NEW medieval timber workshop, not a recolor. Observe old model and actual Unity city/reference. Choose coherent spatial composition, proportions and material hierarchy. Existing tool kit supports dressed limestone piers, oak braced structural frame, overlapping blue slate roof, masonry rear workshop, open front saw bay, framed saw blade and cut-log stock. No rectangular parcel/platform, no modern circular saw or waterwheel without water. Ground contact at Z0, silhouette under Z8, X/Y footprint maximum10m. Front is Y negative. Design proportions and optional lean-to/log-yard orientation. The lower stone piers and masonry rear workshop must visually relate to Bastion stonework; slate roof matches actual retained blue palette. Explain three visual defects addressed and the new spatial composition. No self approval.'}
fields={'width':[6.4,7.2,8.0],'depth':[5.2,6.0,6.8],'eave_height':[3.3,3.8,4.2],'roof_rise':[1.6,2.0,2.4],'bay_count':[2,3],'rear_wall_fraction':[.25,.35,.45],'stone_height':[.55,.8,1.05],'frame_thickness':[.22,.28,.34],'lean_to':['left','right','none'],'lean_to_depth':[1.0,1.4],'log_yard':['front_left','front_right'],'log_count':[4,6,8],'roof_rows':[8,10,12],'timber_tone':['dark_oak','weathered_oak'],'masonry_tone':['warm_limestone','sandstone'],'brace_style':['short_knee','cross_rear']}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'string' if isinstance(v[0],str) else 'number','enum':v} for k,v in fields.items()}}}
images=['sawmill-refined-before.png','sawmill-completed-after-detail.png','bastion2-after-bastion.png']
payload=dict(model='qwen3-vl:8b-instruct-q4_K_M',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are the existing Eldoria local 3D author. Direct a new coherent sawmill from observed references using spatial tools and full architectural decisions. JSON only, no score or approval.'},{'role':'user','content':json.dumps(context),'images':[base64.b64encode((EV/n).read_bytes()).decode() for n in images]}],options={'temperature':.3,'num_ctx':8192,'num_predict':1900})
write('sawmill-refined-qwen-request.json',{'model':payload['model'],'schema':schema,'context':context,'image_sha256':{n:digest(EV/n) for n in images}})
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
  log=open(EV/'sawmill-refined-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 assert any(m['name']==payload['model'] for m in api('/api/tags')['models'])
 response=api('/api/chat',payload,360);write('sawmill-refined-qwen-response.json',response);plan=json.loads(response['message']['content']);assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('sawmill-refined-plan.json',plan)
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()
tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in {'Mesh','line','execute_via_blendermcp'}],type_ignores=[]),'<reviewed authoring tools>','exec'))
RejectedPlan=RuntimeError;stone_trial=False
# Reuse exact existing city material family, with its packed textures.
palette_source=EV/'BastionEnsemble70.blend'
assert digest(palette_source)=='635c948163ee618fcbf6e4b63585db864ef441f590aa50dd0ccc3f6c25ace644'
with bpy.data.libraries.load(str(palette_source),link=False) as (available,selected):
 selected.materials=[n for n in available.materials if n.startswith(('Warm limestone','Trial sandstone','Blue slate')) or n in ['Oak','Carved limestone','Forged iron','Recessed stone joints']]
loaded={m.name:m for m in selected.materials if m}
def group(prefix):
 result=[m for n,m in loaded.items() if n.startswith(prefix)];assert result,prefix;return result
stone=group('Warm limestone' if plan['masonry_tone']=='warm_limestone' else 'Trial sandstone');blue=group('Blue slate');wood=loaded['Oak'];trim=loaded['Carved limestone']
def material(name,color,roughness=.85,metal=0):
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*color,1);p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=roughness;p.inputs['Metallic'].default_value=metal;return m
iron=material('Sawmill forged iron',(.075,.085,.09),.65,.65)
cut=material('Sawmill exposed end grain',(.36,.23,.12))
# Small packed timber atlas with longitudinal grain, shared across all woodwork.
wood=wood.copy();wood.name='Sawmill weathered structural oak';tone=(.16,.09,.045) if plan['timber_tone']=='dark_oak' else (.24,.18,.12)
tex=bpy.data.images.new('Sawmill oak grain',width=256,height=256);pixels=[]
for y in range(256):
 for x in range(256):
  f=1+.12*sin(x*.45+sin(y*.025)*2)+.06*sin(x*1.7+y*.015)
  pixels.extend([min(1,c*f) for c in tone]+[1])
tex.pixels.foreach_set(pixels);tex.filepath_raw=str(OUT/'SawmillRefinedOak.png');tex.file_format='PNG';tex.save();tex.pack()
p=next(n for n in wood.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node=wood.node_tree.nodes.new('ShaderNodeTexImage');node.image=tex;wood.node_tree.links.new(node.outputs['Color'],p.inputs['Base Color'])
created=[];random.seed(104)
def perform(action):
 assert action==operation and not created
 w=plan['width'];d=plan['depth'];e=plan['eave_height'];rise=plan['roof_rise'];s=plan['stone_height'];th=plan['frame_thickness'];bays=int(plan['bay_count'])
 masonry=Mesh('Sawmill limestone footings and workshop');frame=Mesh('Sawmill oak frame');slates=Mesh('Sawmill layered blue slate');work=Mesh('Sawmill functional saw and timber');hardware=Mesh('Sawmill iron joinery')
 def pier(x,y,width,depth,height):
  rows=max(2,round(height/.28))
  for j in range(rows):masonry.box((x,y,(j+.5)*height/rows),(width-.02,depth-.02,height/rows-.018),random.choice(stone),wear=.008)
 for y in [-d/2,d/2]:
  for i in range(bays+1):
   x=-w/2+i*w/bays;pier(x,y,th+.32,th+.32,s)
   frame.box((x,y,(s+e)/2),(th,th,e-s),wood)
   for side in [-1,1]:
    if -.01<=i+side<=bays+.01:line(frame,(x,y,e-.85),(x+side*.85,y,e-.08),th*.36,wood)
   for z in [s+.15,e-.35]:hardware.box((x,y-th/2-.025,z),(th+.03,.04,.09),iron)
  frame.box((0,y,e),(w+th,th,th),wood)
 rear=d/2-d*plan['rear_wall_fraction']
 # Rear masonry room encloses storage; front bay is genuinely open.
 for side in [-1,1]:
  x=side*w/2
  for row in range(max(2,round(e/.4))):
   z=.2+row*.4
   if z>e-.15:break
   n=max(2,round((d/2-rear)/.65))
   for col in range(n):masonry.box((x,rear+(col+.5)*(d/2-rear)/n,z),(.22,(d/2-rear)/n-.025,.375),random.choice(stone),wear=.008)
 for i in range(int(w/.7)):
  for row in range(max(2,round(e/.4))):
   z=.2+row*.4
   if z<e-.15:masonry.box((-w/2+(i+.5)*w/int(w/.7),d/2,z),(w/int(w/.7)-.025,.22,.375),random.choice(stone),wear=.008)
 if plan['brace_style']=='cross_rear':
  for sign in [-1,1]:line(frame,(-w/2,d/2+.14,e if sign==1 else s),(w/2,d/2+.14,s if sign==1 else e),th*.35,wood)
 # Ridge runs along Y; two tiled roof slopes, overlap and real thickness.
 rows=int(plan['roof_rows']);cols=max(8,round(d/.45));half=w/2+.4
 for side in [-1,1]:
  for row in range(rows):
   t0=row/rows;t1=min(1,(row+1.15)/rows);x0=side*half*(1-t0);x1=side*half*(1-t1);z0=e+rise*t0+.08;z1=e+rise*t1+.08
   for col in range(cols+1):
    y0=max(-d/2-.4,-d/2-.4+(col-.5*(row%2))*(d+.8)/cols)+.009;y1=min(d/2+.4,-d/2-.4+(col+1-.5*(row%2))*(d+.8)/cols)-.009
    if y1-y0<.03:continue
    v=[(x0,y0,z0),(x0,y1,z0),(x1,y1,z1),(x1,y0,z1),(x0,y0,z0-.07),(x0,y1,z0-.07),(x1,y1,z1-.07),(x1,y0,z1-.07)]
    slates.add(v,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],random.choice(blue))
  for y in [-d/2-.4,d/2+.4]:line(frame,(side*half,y,e),(0,y,e+rise),th*.4,wood)
 for i in range(cols):slates.box((0,-d/2-.4+(i+.5)*(d+.8)/cols,e+rise+.13),(.24,(d+.8)/cols-.015,.17),blue[i%len(blue)])
 for y in [-d/2,d/2]:
  frame.box((0,y,e),(w,th,th),wood);line(frame,(0,y,e),(0,y,e+rise-.08),th*.4,wood)
 # Traditional vertical frame saw and log feed table, visible through front open bay.
 sy=-.4;span=min(2.8,w*.4)
 for x in [-span/2,span/2]:
  frame.box((x,sy,1.65),(.2,.2,3.3),wood)
  for y in [sy-.6,sy+.6]:work.box((x,y,.7),(.18,.18,1.4),wood)
 for z in [.65,3.3]:frame.box((0,sy,z),(span+.2,.2,.2),wood)
 work.box((0,sy,1.45),(span+1,1.2,.18),wood);hardware.box((0,sy-.15,2),(.10,.035,2.15),iron)
 for k in range(18):hardware.add([(-.05,sy-.18,.95+k*.1),(-.16,sy-.18,1+k*.1),(-.05,sy-.18,1.05+k*.1)],[(0,1,2)],iron)
 line(work,(-span/2-.4,sy,1.77),(span/2+.7,sy,1.77),.25,wood)
 yard=(-1 if plan['log_yard']=='front_left' else 1)*(w/2-1.1)
 for k in range(int(plan['log_count'])):
  x=yard+(k%3-1)*.48;z=.25+(k//3)*.43;y=-d/2+.5
  line(work,(x,y-.65,z),(x,y+1.1,z),.23,wood);line(work,(x,y-.66,z),(x,y-.64,z),.205,cut)
 if plan['lean_to']!='none':
  side=-1 if plan['lean_to']=='left' else 1;ext=min(plan['lean_to_depth'],4.9-w/2);x=side*(w/2+ext)
  for y in [rear,d/2]:
   pier(x,y,.4,.4,.4);frame.box((x,y,1.5),(.17,.17,2.6),wood)
  for j in range(6):
   y=rear+(j+.5)*(d/2-rear)/6
   line(slates,(side*w/2,y,3.3),(x,y,2.8),.12,blue[j%len(blue)])
 created.extend([masonry.finish(.015),frame.finish(.018),slates.finish(0),work.finish(.012),hardware.finish(0)])
operation={'id':'sawmill-refined-architecture','op':'author_new_workshop_from_spatial_plan','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert digest(SOURCE)==expected and created
for o in originals:o.hide_render=True
render('sawmill-refined-after.png')
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'SawmillRefined104.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in created:o.select_set(True)
bpy.context.view_layer.objects.active=created[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'SawmillRefined104.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
write('sawmill-refined-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'original_blend_sha256':expected,'original_source_preserved':True,'qwen_model':payload['model'],'memory_sha256':digest(memory_path),'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','plan':plan,'glb_sha256':digest(OUT/'SawmillRefined104.glb'),'blend_sha256':digest(EV/'SawmillRefined104.blend'),'triangles':sum(len(p.vertices)-2 for o in created for p in o.data.polygons),'art_approved':False,'unity_capture_pending':True})
print('VALORIA_SAWMILL_AUTHOR_COMPLETE')
