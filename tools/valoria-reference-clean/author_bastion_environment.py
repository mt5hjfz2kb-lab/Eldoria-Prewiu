"""Isolated Bastion ensemble replacement; existing Blender source is never overwritten."""
import base64,bpy,bmesh,ast,array,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
from mathutils import Vector,Matrix
from math import sin,cos,pi
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean'
OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE=EV/'BastionEnsemble70.blend'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v):(EV/n).write_text(json.dumps(v,indent=2),encoding='utf8')
expected='635c948163ee618fcbf6e4b63585db864ef441f590aa50dd0ccc3f6c25ace644'
assert digest(SOURCE)==expected
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
original=bpy.data.objects['Bastion_Ensemble_Candidate']
def stamp(o):
 a=array.array('f',[0])*(len(o.data.vertices)*3);o.data.vertices.foreach_get('co',a)
 return (tuple(v for row in o.matrix_world for v in row),hashlib.sha256(a.tobytes()).hexdigest())
protected={o.name:stamp(o) for o in bpy.context.scene.objects if o.type=='MESH'}
points=[original.matrix_world@v.co for v in original.data.vertices]
context={'source_sha256':expected,'scope':'Immediate Bastion foundation landscape ONLY. Keep every architecture vertex and old gameplay dimensions unchanged. Two images attached: actual native70 Unity detail first, owner target second.',
 'observation':'Architecture70 improved. Smooth large green balls with black spikes and symmetrical rings of tiny pebbles still look artificial. Replace them with botanical branching shrubs: many small elongated closed leaf buds, upright asymmetric tapered stems, varied low groundcover, fewer larger angular embedded limestone rocks. No green spheres. Keep six-metre central stair clearway. Match warm olive vegetation and grouped rock planting in reference; no trees/buildings/light changes.',
 'decision':'Choose plant spatial composition from allowed foundation centres, leaf scale, stem height, crown width, density, rock scale and foliage palette. These control actual Blender geometry and materials. Irregular loam/grass shoulder will be tested independently on native terrain.'}
fields={'stem_height':[1.4,1.8,2.2],'crown_width':[1.2,1.6,1.9],'leaf_length':[.22,.29,.36],'branch_count':[7,9,11],'leaf_pairs':[7,9,11],'rock_scale':[.8,1.15,1.4],'foliage_palette':[0,1,2],'layout':[0,1,2]}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'number','enum':v} for k,v in fields.items()}}}
payload=dict(model='qwen3-vl:8b-instruct-q4_K_M',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are Eldoria local 3D art director. Select a coherent architectural and landscape treatment from actual observed source, not a score. Choose concrete geometric parameters and explain the composition. Output JSON only; never claim approval.'},{'role':'user','content':json.dumps(context),'images':[base64.b64encode((EV/n).read_bytes()).decode() for n in ['ensemble-after-bastion.png','reference-target.jpeg']]}],options={'temperature':.35,'num_ctx':8192,'num_predict':1500})
write('bastion-environment72-context.json',context);write('bastion-environment72-qwen-request.json',{'model':payload['model'],'format':schema,'context':context,'image_sha256':{n:digest(EV/n) for n in ['ensemble-after-bastion.png','reference-target.jpeg']}})
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
  log=open(EV/'bastion-environment72-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 assert any(m['name']=='qwen3-vl:8b-instruct-q4_K_M' for m in api('/api/tags')['models'])
 response=api('/api/chat',payload,360);write('bastion-environment72-qwen-response.json',response);plan=json.loads(response['message']['content'])
 assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('bastion-environment72-validated-plan.json',{'plan':plan,'art_approved':False})
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()

tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
names={'Mesh','line','execute_via_blendermcp'}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in names],type_ignores=[]),'<existing reviewed geometry tools>','exec'))
RejectedPlan=RuntimeError
stone_trial=False
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True
 p=next((x for x in m.node_tree.nodes if x.type=='BSDF_PRINCIPLED'),None)
 if p is None:p=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
 out=next((x for x in m.node_tree.nodes if x.type=='OUTPUT_MATERIAL'),None)
 if out is None:out=m.node_tree.nodes.new('ShaderNodeOutputMaterial')
 m.node_tree.links.new(p.outputs['BSDF'],out.inputs['Surface'])
 p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.87
 return m
palette=[(.068,.13,.017),(.085,.145,.022),(.048,.11,.019)][int(plan['foliage_palette'])]
leaves=[mat('Native72 olive foliage '+str(i),tuple(c*(.78+i*.13) for c in palette)) for i in range(5)]
twigs=mat('Native72 woody stems',(.065,.042,.016))
rocks=[mat('Native72 foundation limestone '+str(i),(.25+i*.025,.225+i*.024,.18+i*.022)) for i in range(4)]
created=[]
def perform(action):
 assert action==operation and not created
 g=Mesh('Bastion_Environment72');rng=random.Random(72018)
 layouts=[
 [(-10.7,31.4,1.0),(10.8,33.0,1.0),(-8.0,27.5,.7),(8.4,27.3,.8),(-11.1,38.5,.9),(11.2,39.0,.9)],
 [(-10.8,32.1,1.0),(11.0,34.0,1.0),(-8.1,27.6,.7),(8.4,27.3,.8),(-11.0,39.0,.8),(11.1,38.8,.9)],
 [(-10.8,31.7,1.0),(10.9,32.6,1.0),(-8.1,27.3,.8),(8.6,27.5,.7),(-11.0,38.5,.9),(11.2,39.2,.8)]]
 centres=layouts[int(plan['layout'])]
 # Closed elongated leaves, aligned on actual woody branchlets, no sphere crowns.
 def leaf_at(point,direction,length,width,material):
  d=Vector(direction).normalized();u=d.cross(Vector((0,0,1)))
  if u.length<.05:u=Vector((1,0,0))
  u.normalize();v=d.cross(u).normalized();centre=Vector(point)
  verts=[centre-d*length*.50,centre+d*length*.62]
  for j in range(6):
   a=j*pi/3;verts.append(centre+u*cos(a)*width+v*sin(a)*width*.55)
  faces=[]
  for j in range(6):a=2+j;b=2+(j+1)%6;faces.extend([(0,b,a),(1,a,b)])
  g.add([tuple(p) for p in verts],faces,material)
 for ci,(cx,cy,sc) in enumerate(centres):
  h=plan['stem_height']*sc;spread=plan['crown_width']*sc
  for stem in range(int(plan['branch_count'])):
   a=stem*2.39996+ci*.7;rad=spread*.43*math.sqrt(stem/max(1,plan['branch_count']-1))
   bottom=Vector((cx+cos(a)*rad*.25,cy+sin(a)*rad*.25,.30))
   top=Vector((cx+cos(a)*rad,cy+sin(a)*rad,.35+h*(1-.43*rad/max(.1,spread*.43))*rng.uniform(.9,1.08)))
   line(g,bottom,top,.027,twigs)
   for j in range(int(plan['leaf_pairs'])):
    t=.20+.78*j/max(1,plan['leaf_pairs']-1);mid=bottom.lerp(top,t);angle=a+j*2.399
    for side in [-1,1]:
     direction=Vector((cos(angle)*side,sin(angle)*side,.75+t*.35)).normalized()
     length=plan['leaf_length']*rng.uniform(.85,1.15)*sc;centre=mid+direction*length*.42
     leaf_at(centre,direction,length*1.9,length*.58,leaves[min(4,int(t*4))])
  # A subsidiary lower bush makes an irregular rooted silhouette, not a symmetrical ball.
  for j in range(24):
   a=rng.random()*2*pi;r=rng.uniform(.1,spread*.58)
   x=cx+cos(a)*r;y=cy+sin(a)*r;z=.35+rng.uniform(.10,.30)
   leaf_at((x,y,z),(cos(a),sin(a),.65),plan['leaf_length']*1.35,plan['leaf_length']*.38,leaves[1+j%3])
  # Unequal angular rocks overlap into embedded groups on the outer side.
  for j in range(5):
   s=plan['rock_scale']*sc*([1,.62,.38,.28,.22][j])
   x=cx+(1 if cx>0 else -1)*(.65+j*.22)+rng.uniform(-.30,.30);y=cy+.35+j*.19+rng.uniform(-.35,.35)
   rings=[]
   for z,scale in [(-.22,1.02),(.35,1),(.85,.62)]:
    ring=[]
    for k in range(7):
     a=2*pi*k/7+.14;rr=scale*rng.uniform(.85,1.15);ring.append((x+cos(a)*s*rr,y+sin(a)*s*.8*rr,.35+s*z+rng.uniform(-.06,.06)*s))
    rings.extend(ring)
   fs=[tuple(reversed(range(7))),tuple(range(14,21))]
   for ring in range(2):
    for k in range(7):fs.append((ring*7+k,ring*7+(k+1)%7,(ring+1)*7+(k+1)%7,(ring+1)*7+k))
   g.add(rings,fs,rocks[ci%4])
  # Curved solid grass blades join stone groups to meadow at visible scale.
  for patch in range(9):
   a=rng.random()*2*pi;r=rng.uniform(.8,spread);x=cx+cos(a)*r;y=cy+sin(a)*r
   for blade in range(5):
    a=rng.random()*2*pi;h=rng.uniform(.28,.62);w=.035
    v=[(x-w*sin(a),y+w*cos(a),.34),(x+w*sin(a),y-w*cos(a),.34),(x+.18*cos(a)+w*sin(a),y+.18*sin(a)-w*cos(a),.34+h*.65),(x+.18*cos(a)-w*sin(a),y+.18*sin(a)+w*cos(a),.34+h*.65),(x+.30*cos(a),y+.30*sin(a),.34+h),(x,y,.36)]
    g.add(v,[(0,1,2,3),(3,2,4),(1,0,5),(2,1,5),(4,2,5),(3,4,5),(0,3,5)],leaves[2+blade%3])
 obj=g.finish(.0);created.append(obj)
 # Smooth leaves only; the angular stone/branch forms keep their authored creases.
 for p in obj.data.polygons:p.use_smooth=obj.data.materials[p.material_index] in leaves
operation={'id':'bastion-environment-native72','op':'replace_foundation_planting_only','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert len(created)==1 and all(stamp(bpy.data.objects[n])==s for n,s in protected.items())
bpy.data.objects['Bastion_Ensemble_Grounding'].hide_render=True
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BastionEnvironment72.blend'))
bpy.ops.object.select_all(action='DESELECT');created[0].select_set(True);bpy.context.view_layer.objects.active=created[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'BastionEnvironment72.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected
write('bastion-environment72-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'source_preserved':True,'architecture70_preserved':True,'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','qwen_model':payload['model'],'plan':plan,'glb_sha256':digest(OUT/'BastionEnvironment72.glb'),'blend_sha256':digest(EV/'BastionEnvironment72.blend'),'art_approved':False,'triangles':sum(len(p.vertices)-2 for p in created[0].data.polygons)})
print('VALORIA_BASTION_ENVIRONMENT_BLENDER_COMPLETE')
