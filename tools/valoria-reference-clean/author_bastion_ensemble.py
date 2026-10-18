"""Isolated Bastion ensemble replacement; existing Blender source is never overwritten."""
import bpy,bmesh,ast,array,hashlib,json,math,os,pathlib,random,subprocess,sys,time,urllib.request
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
original=bpy.data.objects['Bastion_I']
def stamp(o):
 a=array.array('f',[0])*(len(o.data.vertices)*3);o.data.vertices.foreach_get('co',a)
 return (tuple(v for row in o.matrix_world for v in row),hashlib.sha256(a.tobytes()).hexdigest())
protected={o.name:stamp(o) for o in bpy.context.scene.objects if o.type=='MESH'}
points=[original.matrix_world@v.co for v in original.data.vertices]
context={'source_sha256':expected,'object':'Bastion_I','actual_world_bounds':[[min(p[k] for p in points) for k in range(3)],[max(p[k] for p in points) for k in range(3)]],
 'reference_observation':'Owner reference: compact warm stone keep, emphatic corbelled crenellations, substantial sill/corner blocks, softly swept blue roofs, dark recessed oak door, grouped leafy vegetation and embedded rocks knit terrace to meadow.',
 'unity_observation':'Native62 authentic image: rigid thin roof planes, brick-like uniform wall courses, thin window outlines, terrace reads as disconnected slab, green polygonal boulders in place of foliage. Lighting is already warm and baked. Fix entire Bastion ensemble, not a tint or tiny shrub patch.',
 'preserve':'Same hall and tower bases, terrace 18x5, six stairs exact 6m width/.46 run/.19 rise, entry centre, flag extent, gameplay colliders and camera. Front is negative Blender Y; source building translated +5.5Y. All other structures protected.',
 'operations':'Remodel Bastion only using separate editable mesh: curved overlapping slate roof, stepped corbel cornices and capped merlons, staggered corner quoins, recessed framed windows, segmented oak door with ironwork; group shrubs/rocks outside stair clearway. Your choices determine actual geometry before Blender executes.'}
fields={'roof_rise':[3.4,3.7,4.0],'eave_sweep':[.12,.22,.30],'cornice_projection':[.30,.40,.48],'merlon_width':[.70,.78,.86],'course_height':[.52,.58,.62],'window_width':[.80,.95,1.05],'window_height':[1.7,1.9,2.1],'quoin_width':[.55,.65,.75],'foliage_height':[1.3,1.6,1.9],'foliage_spread':[1.6,1.9,2.2],'rock_scale':[.65,.85,1.05]}
schema={'type':'object','required':['rationale']+list(fields),'additionalProperties':False,'properties':{'rationale':{'type':'string'},**{k:{'type':'number','enum':v} for k,v in fields.items()}}}
payload=dict(model='qwen3:14b',stream=False,format=schema,think=False,keep_alive=0,messages=[{'role':'system','content':'You are Eldoria local 3D art director. Select a coherent architectural and landscape treatment from actual observed source, not a score. Choose concrete geometric parameters and explain the composition. Output JSON only; never claim approval.'},{'role':'user','content':json.dumps(context)}],options={'temperature':.35,'num_ctx':8192,'num_predict':1500})
write('bastion-ensemble-context.json',context);write('bastion-ensemble-qwen-request.json',payload)
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
  log=open(EV/'bastion-ensemble-ollama.log','w');server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
  for i in range(40):
   try:api('/api/version');break
   except Exception:time.sleep(1)
  else:raise RuntimeError('Existing Ollama unavailable')
 assert any(m['name']=='qwen3:14b' for m in api('/api/tags')['models'])
 response=api('/api/chat',payload,240);write('bastion-ensemble-qwen-response.json',response);plan=json.loads(response['message']['content'])
 assert set(plan)==set(fields)|{'rationale'}
 for k,values in fields.items():assert plan[k] in values,(k,plan[k])
 write('bastion-ensemble-validated-plan.json',{'plan':plan,'art_approved':False})
finally:
 if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
 if log:log.close()
# Reuse reviewed primitive/UV/MCP functions, never execute the full city builder.
tree=ast.parse((ROOT/'tools/valoria-reference-clean/author_scene.py').read_text())
names={'Mesh','mat','arch','line','banner','flame_shape','execute_via_blendermcp'}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in names],type_ignores=[]),'<existing reviewed geometry tools>','exec'))
RejectedPlan=RuntimeError
def mat(n,c,rough=.8,metal=0):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True
 p=next((x for x in m.node_tree.nodes if x.type=='BSDF_PRINCIPLED'),None)
 if p is None:p=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled')
 out=next((x for x in m.node_tree.nodes if x.type=='OUTPUT_MATERIAL'),None)
 if out is None:out=m.node_tree.nodes.new('ShaderNodeOutputMaterial')
 m.node_tree.links.new(p.outputs['BSDF'],out.inputs['Surface'])
 p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 return m
stone=[bpy.data.materials['Warm limestone %d'%i] for i in range(6)]
trial_stone=[bpy.data.materials['Trial sandstone %d'%i] for i in range(6)]
for variable,name in [('trim','Carved limestone'),('mortar','Recessed stone joints'),('wood','Oak'),('iron','Forged iron'),('gold','Heraldic gold'),('cloth','Royal blue cloth'),('flame','Torch golden core'),('ember','Torch orange envelope')]:globals()[variable]=bpy.data.materials[name]
blue=[bpy.data.materials['Blue slate %d'%i] for i in range(5)]
stone_trial=stone_global=True;surface_trial_stats={'mapped_boxes':0}
patch_node=next(n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='stone_patches' for t in n.targets));exec(compile(ast.Module(body=[patch_node],type_ignores=[]),'<reviewed UV rectangles>','exec'))
dark=mat('Bastion recess shadow',(.028,.022,.014));leaf=[mat('Ensemble foliage '+str(i),(.035+i*.012,.085+i*.020,.012+i*.006)) for i in range(5)]
rock=[mat('Ensemble embedded stone '+str(i),(.18+i*.035,.17+i*.030,.13+i*.026)) for i in range(4)]
def courses(g,x,y,z,w,d,h,stone_size=1.1,openings=()):
 # Hollow wall core gives genuine recess depth; openings affect front wall only.
 front=y-d/2;rows=max(1,round(h/plan['course_height']));rh=h/rows
 g.box((x,y+d/2-.25,z+h/2),(w,.5,h),mortar)
 for side in [-1,1]:g.box((x+side*(w/2-.25),y,z+h/2),(.5,d,h),mortar)
 for row in range(rows):
  zz=z+(row+.5)*rh;intervals=[(-w/2,w/2)]
  for ox,oz,ow,oh in openings:
   if z+row*rh<oz+oh and z+(row+1)*rh>oz:
    a,b=ox-x-ow/2,ox-x+ow/2;intervals=[part for l,r in intervals for part in [(l,min(r,a)),(max(l,b),r)] if part[1]-part[0]>.02]
  for l,r in intervals:g.box((x+(l+r)/2,front+.18,zz),(r-l,.4,rh),mortar)
  for face,L,C in [('front',w,front),('back',w,y+d/2),('left',d,x-w/2),('right',d,x+w/2)]:
   cuts=[-L/2]+[q for q in [-L/2+(row%2)*stone_size*.5+i*stone_size for i in range(math.ceil(L/stone_size)+1)] if -L/2+.08<q<L/2-.08]+[L/2]
   for a,b in zip(cuts,cuts[1:]):
    spans=[(a,b)] if face!='front' else [(max(a,l),min(b,r)) for l,r in intervals if min(b,r)-max(a,l)>.08]
    for aa,bb in spans:
     p=(x+(aa+bb)/2,C,zz) if face in ('front','back') else (C,y+(aa+bb)/2,zz)
     s=(bb-aa-.025,.18,rh-.025) if face in ('front','back') else (.18,bb-aa-.025,rh-.025)
     g.box(p,s,random.choice(stone),wear=.009)
 g.box((x,y,z+.15),(w+.20,d+.20,.30),trim)
def crown(g,x,y,z,w,d):
 p=plan['cornice_projection'];mw=plan['merlon_width']
 for height,extra,th in [(z-.40,.12,.16),(z-.18,p*.7,.20),(z+.02,p,.22)]:
  for sy in [-1,1]:g.box((x,y+sy*d/2,height),(w+extra*2,.32+extra,th),trim)
  for sx in [-1,1]:g.box((x+sx*w/2,y,height),(.32+extra,d+extra*2,th),trim)
 for side in [-1,1]:
  for j in range(round(w/1.15)+1):
   X=x-w/2+j*w/round(w/1.15);g.box((X,y+side*(d/2-.04),z-.48),(.28,.48,.48),trim)
  for j in range(round(d/1.15)+1):g.box((x+side*w/2,y-d/2+j*d/round(d/1.15),z-.48),(.48,.28,.48),trim)
 for side in [-1,1]:
  for j in range(round(w/1.2)+1):
   X=x-w/2+j*w/round(w/1.2);g.box((X,y+side*d/2,z+.51),(mw,.72,.84),stone[4]);g.box((X,y+side*d/2,z+.98),(mw+.13,.85,.15),trim)
  for j in range(1,round(d/1.2)):
   Y=y-d/2+j*d/round(d/1.2);g.box((x+side*w/2,Y,z+.51),(.72,mw,.84),stone[4]);g.box((x+side*w/2,Y,z+.98),(.85,mw+.13,.15),trim)
def roof(g,x,y,z,w,d,rise):
 rows=10;cols=round(w/ .62);sweep=plan['eave_sweep']
 def H(t):return z+rise*t+sweep*(1-t)**4
 for side in [-1,1]:
  for row in range(rows):
   a=row/rows;b=min(1.015,(row+1.10)/rows)
   for col in range(cols+1):
    l=max(-w/2-.45,-w/2-.45+(col-.5*(row%2))*(w+.9)/cols)+.008;r=min(w/2+.45,-w/2-.45+(col+1-.5*(row%2))*(w+.9)/cols)-.008
    if r-l<.03:continue
    ya=y+side*(d/2+.50)*(1-a);yb=y+side*(d/2+.50)*(1-b)
    za=H(a);zb=H(b);v=[(x+l,ya,za),(x+r,ya,za),(x+r,yb,zb),(x+l,yb,zb),(x+l,ya,za-.07),(x+r,ya,za-.07),(x+r,yb,zb-.07),(x+l,yb,zb-.07)]
    g.add(v,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7),(0,3,7,4),(1,5,6,2)],blue[random.choice([1,2,2,3])])
  for X in [-w/2-.48,w/2+.48]:
   for j in range(5):a=j/5;b=(j+1)/5;line(g,(x+X,y+side*(d/2+.53)*(1-a),H(a)-.10),(x+X,y+side*(d/2+.53)*(1-b),H(b)-.10),.13,wood)
  line(g,(x-w/2-.52,y+side*(d/2+.52),H(0)-.12),(x+w/2+.52,y+side*(d/2+.52),H(0)-.12),.13,wood)
 for j in range(round(w/.65)):g.box((x-w/2+(j+.5)*w/round(w/.65),y,z+rise+.10),(w/round(w/.65)-.02,.34,.22),blue[2])
def window(g,x,y,z,w,h):
 g.box((x,y+.32,z+h/2),(w,.10,h),dark)
 g.box((x,y-.18,z-.07),(w+.65,.65,.23),trim)
 for s in [-1,1]:
  for j in range(3):g.box((x+s*(w/2+.12),y-.08,z+(j+.5)*(h-w/2)/3),(.24,.40,(h-w/2)/3-.018),trim)
 arch(g,x,y-.08,z+h-w/2,w/2,.24,.40,trim,segments=9)
 for dx in [-.20,.20]:g.box((x+dx*w,y+.15,z+h*.47),(.055,.07,h*.8),wood)
def quoins(g,x,y,z,w,d,h):
 for row in range(round(h/.65)):
  for sx in [-1,1]:
   for sy in [-1,1]:g.box((x+sx*(w/2-.15),y+sy*(d/2-.10),z+(row+.5)*h/round(h/.65)),(plan['quoin_width'] if row%2 else .42,.42 if row%2 else plan['quoin_width'],h/round(h/.65)-.025),trim)
def torch(g,x,y,z,scale=1):
 courses(g,x,y,z,.85*scale,.85*scale,scale);g.box((x,y,z+1.05*scale),(scale,scale,.15*scale),trim);flame_shape(g,x,y,z+1.15*scale,.23*scale,.8*scale,ember);flame_shape(g,x,y-.14*scale,z+1.15*scale,.12*scale,.52*scale,flame,.4)
created=[]
def perform(action):
 assert action==operation and not created
 random.seed(66018);g=Mesh('Bastion_I')
 ww=plan['window_width'];wh=plan['window_height'];rise=plan['roof_rise']
 courses(g,0,32,.38,17,10,5.4,openings=[(x,2.85,ww,wh) for x in [-4.8,4.8]])
 roof(g,0,32,5.9,17,10,rise)
 for X in [-8.5,8.5]:g.add([(X,27,5.75),(X,37,5.75),(X,32,5.9+rise)],[(0,1,2)],stone[3])
 courses(g,0,28.2,.38,4.8,4.4,10.3,openings=[(0,1.5,3.2,4.1),(0,7.0,ww,wh)])
 crown(g,0,28.2,10.8,4.8,4.4);roof(g,0,28.2,10.85,3.2,2.8,.65);quoins(g,0,28.2,.38,4.8,4.4,9.8)
 for x in [-8.4,8.4]:
  courses(g,x,29.5,.38,3.5,5.2,6.8);crown(g,x,29.5,7.3,3.5,5.2);quoins(g,x,29.5,.38,3.5,5.2,6.6);banner(g,x,26.52,6.6,1.3,2.5)
 window(g,0,25.90,7.0,ww,wh)
 for x in [-4.8,4.8]:window(g,x,26.90,2.85,ww,wh)
 # Portal: separate profiled closed oak planks set inside a deep stone reveal.
 for i in range(12):
  l=-1.6+i*3.2/12+.012;r=-1.6+(i+1)*3.2/12-.012;top=lambda x:4+math.sqrt(max(0,1.6**2-x*x))
  v=[(x,y,z) for y in [26.02,26.18] for x,z in [(l,1.5),(r,1.5),(r,top(r)),(l,top(l))]];g.add(v,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7),(0,3,7,4),(1,5,6,2)],wood)
 for s in [-1,1]:
  for j in range(5):g.box((s*1.83,25.83,1.5+(j+.5)*.5),(.44,.63,.48),trim)
 arch(g,0,25.83,4,1.6,.44,.63,trim,segments=15)
 g.box((0,25.81,5.74),(.48,.72,.42),trim)
 for z in [2.05,3.15]:
  for s in [-1,1]:g.box((s*.8,25.94,z),(1.37,.07,.11),iron)
 for s in [-1,1]:
  for j in range(8):
   a=j*pi/4;b=(j+1)*pi/4;line(g,(s*.27+.12*cos(a),25.87,2.8+.12*sin(a)),(s*.27+.12*cos(b),25.87,2.8+.12*sin(b)),.028,iron)
 for x in [-5.8,5.8]:
  for j in range(3):courses(g,x,26.65,1.5+j*1.05,.85-j*.14,.92-j*.18,1.05)
 # Identical functional platform and treads, now bounded by articulated parapets.
 courses(g,0,25,.38,18,5,1.15)
 for row in range(7):
  for col in range(22):g.box((-8.85+(col+.5)*17.7/22,22.64+(row+.5)*4.7/7,1.565),(17.7/22-.025,4.7/7-.025,.075),random.choice(stone))
 for i in range(6):g.box((0,20.25+i*.46,.38+(i+1)*.095),(6,.49,(i+1)*.19),trim)
 for s in [-1,1]:
  for j in range(5):g.box((s*(4.2+j*.93),22.55,1.87),(.86,.52,.58),stone[3]);g.box((s*(4.2+j*.93),22.55,2.20),(.94,.63,.12),trim)
 for x in [-3.6,3.6]:courses(g,x,22,.38,1.2,3.4,.9);torch(g,x,22.9,1.3)
 for x in [-2.6,2.6]:
  line(g,(x,25.7,2.7),(x,25.1,3.05),.08,iron);line(g,(x,25.1,2.95),(x,25.1,3.8),.10,wood);flame_shape(g,x,25.1,3.7,.16,.6,ember);flame_shape(g,x,25.,3.7,.075,.4,flame,.4)
 line(g,(0,28.2,11.35),(0,28.2,17),.075,wood);banner(g,1.2,28.2,16.6,2.4,1.5)
 new=g.finish(.04,2);new.name='Bastion_Ensemble_Candidate';new.location.y=5.5;created.append(new)
 # Mesh foliage comprises overlapping, smooth small crowns with actual leaflets.
 env=Mesh('Bastion_Ensemble_Grounding');rng=random.Random(66019)
 bpy.ops.mesh.primitive_uv_sphere_add(segments=9,ring_count=5,radius=1);tmp=bpy.context.object;iv=[v.co.copy() for v in tmp.data.vertices];ff=[tuple(p.vertices) for p in tmp.data.polygons];bpy.data.objects.remove(tmp,do_unlink=True)
 for cx,cy in [(-10.7,32),(10.9,32.4),(-8.4,27.8),(8.6,28.2),(-11.0,38.7),(11.1,38.8)]:
  for j in range(15):
   a=rng.random()*2*pi;r=rng.random()*plan['foliage_spread'];h=plan['foliage_height']*(1-.50*r/plan['foliage_spread']);sx=rng.uniform(.28,.52);sy=rng.uniform(.28,.52);sz=rng.uniform(.35,.65);x=cx+cos(a)*r;y=cy+sin(a)*r;z=.25+h*.45
   env.add([(x+p.x*sx,y+p.y*sy,z+p.z*sz) for p in iv],ff,leaf[j%5])
  for j in range(7):
   a=rng.random()*2*pi;r=plan['foliage_spread']*rng.uniform(.8,1.1);s=plan['rock_scale']*rng.uniform(.4,.8);x=cx+cos(a)*r;y=cy+sin(a)*r
   env.add([(x+p.x*s,y+p.y*s*.72,.20+p.z*s*.65) for p in iv],ff,rock[j%4])
 ground=env.finish(0)
 for p in ground.data.polygons:p.use_smooth=True
 created.append(ground)
operation={'id':'bastion-ensemble-native66','op':'remodel_existing_bastion_ensemble','plan':plan}
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None);assert addon is not None,'Existing BlenderMCP addon unavailable'
execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
assert len(created)==2
assert all(stamp(bpy.data.objects[n])==s for n,s in protected.items()),'Original source geometry changed'
# Isolated library: retain original in saved source; export only the replacement group.
original.hide_render=True
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BastionEnsemble66.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in created:o.select_set(True)
bpy.context.view_layer.objects.active=created[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'BastionEnsemble66.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected
write('bastion-ensemble-result.json',{'source_sha':os.environ.get('GITHUB_SHA'),'source_preserved':True,'transport':'ACTUAL_BLENDERMCP_TYPED_ACTION','qwen_model':'qwen3:14b','plan':plan,'glb_sha256':digest(OUT/'BastionEnsemble66.glb'),'blend_sha256':digest(EV/'BastionEnsemble66.blend'),'art_approved':False,'objects':[{'name':o.name,'triangles':sum(len(p.vertices)-2 for p in o.data.polygons)} for o in created]})
print('VALORIA_BASTION_ENSEMBLE_BLENDER_COMPLETE')
