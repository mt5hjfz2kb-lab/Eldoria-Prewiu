"""V2.1 genuine seven-piece new Qwen decisions -> BlenderMCP impactful mesh edits."""
import bpy, json, math, socket,threading,time,hashlib,importlib.util,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/asset-family-v2'
PLANS=json.loads((OUT/'qwen-v2-1-art-directions.json').read_text())['plans']
addon=Path('/tmp/eldoria-blender-mcp.py')
assert hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
spec=importlib.util.spec_from_file_location('valoria_v2_1_mcp',str(addon));mcp=importlib.util.module_from_spec(spec);sys.modules['valoria_v2_1_mcp']=mcp;spec.loader.exec_module(mcp)
for name in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+name,bpy.props.BoolProperty(default=False))
def execute(callback):
 bpy.app.driver_namespace['valoria_v2_1_do']=callback
 server=mcp.BlenderMCPServer(host='127.0.0.1',port=0)
 server.socket=socket.socket();server.socket.bind(('127.0.0.1',0))
 port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True
 server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
 response=[]
 def client():
  try:
   with socket.create_connection(('127.0.0.1',port),timeout=15) as c:
    c.settimeout(100)
    c.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['valoria_v2_1_do']()"}}).encode())
    data=b''
    while True:
     part=c.recv(65536)
     if not part:break
     data+=part
     try:response.append(json.loads(data));break
     except json.JSONDecodeError:pass
  except Exception as e:response.append({'error':str(e)})
 thread=threading.Thread(target=client,daemon=True);thread.start();deadline=time.monotonic()+110
 while thread.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.04)
 thread.join(timeout=1);server.stop()
 assert response and response[0].get('status')=='success',response
def render(folder):
 sc=bpy.context.scene
 sc.cycles.use_denoising=False;sc.view_layers[0].cycles.use_denoising=False
 for view,position,target,scale in [('hero',(13,-19,13),(0,0,2),18),('strategic',(16,-23,18),(0,0,2.5),24)]:
  cam=sc.camera;cam.location=position
  cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
  cam.data.ortho_scale=scale;sc.render.filepath=str(folder/('v2-1-'+view+'.png'))
  bpy.ops.render.render(write_still=True)
def export(asset,folder):
 bpy.ops.wm.save_as_mainfile(filepath=str(folder/(asset+'-v2-1.blend')))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.gltf(filepath=str(folder/(asset+'-v2-1.glb')),export_format='GLB',use_selection=True,export_materials='EXPORT')
def edit(ob,formula,records):
 if ob.data.users>1:ob.data=ob.data.copy()
 vertices=ob.data.vertices
 if not vertices:return
 count=0;greatest=0
 for v in vertices:
  previous=v.co.copy();formula(v.co)
  delta=(previous-v.co).length
  if delta>.0001:count+=1;greatest=max(greatest,delta)
 if count:
  ob.data.update()
  records.append({'mesh':ob.name,'vertices_modified':count,'max_change_m':round(greatest,4)})
def new_rock(name,center,size,material):
 # Integrated fractured bedrock chunks, not identical spheres or cones
 import random
 r=random.Random(hashlib.md5(name.encode()).digest())
 n=9;verts=[]
 for layer,zshift,rad in ((0,-.33,1.13),(1,.16,1.0)):
  for k in range(n):
   a=math.tau*k/n
   radius=rad*(.84+r.random()*.3)
   verts.append((center[0]+math.cos(a)*size[0]*radius,center[1]+math.sin(a)*size[1]*radius,center[2]+size[2]*zshift+size[2]*r.uniform(-.13,.13)))
 faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
 for k in range(n):faces.append((k,(k+1)%n,(k+1)%n+n,k+n))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
 if material:mesh.materials.append(material)
 ob=bpy.data.objects.new(name,mesh);bpy.context.scene.collection.objects.link(ob)
 return ob
def author(asset,plan):
 operation=plan['operation'];strength=float(plan['strength']);records=[]
 meshes=[ob for ob in bpy.data.objects if ob.type=='MESH']
 material=next((m for m in bpy.data.materials if m.use_nodes),None)
 assert meshes and material
 if asset=='wall_straight':
  candidates=[o for o in meshes if 'buttress' in o.name.lower() and 'cap' not in o.name.lower()]
  if operation=='rebuild_buttress_proportions':
   for o in candidates:
    zs=[v.co.z for v in o.data.vertices];low=min(zs);height=max(.1,max(zs)-low)
    edit(o,lambda p:(setattr(p,'x',p.x*(1-.34*strength*(p.z-low)/height)),setattr(p,'y',p.y*(1-.24*strength*(p.z-low)/height))),records)
  else:
   for o in meshes:
    if 'ashlar' not in o.name.lower() and 'stone' not in o.name.lower() and 'course' not in o.name.lower():continue
    key=int(hashlib.md5(o.name.encode()).hexdigest()[:4],16)
    edit(o,lambda p:(setattr(p,'z',p.z+.14*strength*math.sin(key*.4+p.x*5+p.y*3)),setattr(p,'y',p.y+.11*strength*math.cos(p.x*4+p.z*2))),records)
 elif asset=='watchtower':
  # Real review rejected the V2 tower's skewed masonry courses.
  # Algebraically undo the exact previous local-axis taper, restoring the
  # historically coherent source geometry before choosing new AI intervention.
  for o in meshes:
   if 'tower' not in o.name.lower():continue
   zs=[v.co.z for v in o.data.vertices]
   if not zs:continue
   localmin=min(zs);d=max(.01,max(zs)-localmin)
   if d<.12:continue
   edit(o,lambda p:(setattr(p,'x',p.x/(1-.07*(p.z-localmin)/d)),setattr(p,'y',p.y/(1-.07*(p.z-localmin)/d))),records)
  candidates=[o for o in meshes if 'tower' in o.name.lower()]
  for o in candidates:
   key=int(hashlib.md5(o.name.encode()).hexdigest()[:4],16)
   if operation=='reshape_crown_silhouette':
    edit(o,lambda p:(setattr(p,'x',p.x*(1-.18*strength*max(0,min(1,(p.z-3.2)/2.8)))),setattr(p,'y',p.y*(1-.11*strength*max(0,min(1,(p.z-3.2)/2.8))))),records)
   else:
    edit(o,lambda p:(setattr(p,'y',p.y+.14*strength*math.sin(p.z*4.1+key*.03)),setattr(p,'z',p.z+.045*strength*math.cos(p.x*3.1))),records)
 elif asset in ('wall_corner','wall_connections'):
  for o in meshes:
   name=o.name.lower()
   if not any(w in name for w in ('corner','coping','buttress','socket','wedge','ashlar','stone','corbel')):continue
   key=int(hashlib.sha256(o.name.encode()).hexdigest()[:4],16)
   if operation in ('interlock_corner_stones','carve_modular_contacts'):
    edit(o,lambda p:(setattr(p,'x',p.x+.14*strength*math.sin(p.z*2.8+key*.03)),setattr(p,'z',p.z+.06*strength*math.cos(p.y*3+p.z*4))),records)
   else:
    edit(o,lambda p:(setattr(p,'x',p.x*(1-.16*strength*max(0,min(1,p.z/4.2)))),setattr(p,'y',p.y+.045*strength*math.sin(p.x*2+p.z*4))),records)
 elif asset=='stone_path_platform':
  for o in meshes:
   name=o.name.lower()
   if operation=='erode_platform_perimeter':
    if 'embankment' not in name:continue
    edit(o,lambda p:(setattr(p,'x',p.x*(1+.14*strength*math.sin(math.atan2(p.y,p.x)*9))),setattr(p,'y',p.y*(1+.11*strength*math.cos(math.atan2(p.y,p.x)*7)))),records)
   else:
    if 'flagstone' not in name and 'path' not in name:continue
    edit(o,lambda p:(setattr(p,'z',p.z+.18*strength*math.sin(2.8*p.x+2.1*p.y)),setattr(p,'x',p.x+.07*strength*math.cos(p.y*4))),records)
 elif asset=='cliff_rocks':
  bed=next(o for o in meshes if o.name.startswith('Sculpted continuous bedrock'))
  def geological(p):
   x,y,z=p.x,p.y,p.z
   if z>=0 or z<=-4.8:return
   a=math.atan2(y,x);u=min(1,max(0,-z/4.8));r=max(.1,math.hypot(x,y))
   if operation=='sculpt_major_geological_shelves':
    shift=.85*strength*math.sin(5*u*math.pi+.3)*math.sin(math.pi*u)+.15*math.sin(a*9)
   else:
    shift=-1.10*strength*(max(0,math.cos(a*8+u*3))**5)*math.sin(math.pi*u)
   p.x+=shift*x/r;p.y+=shift*y/r
  edit(bed,geological,records)
  # Debris-rock clusters physically embedded in the base.
  for i in range(11):
   a=math.tau*i/11+.15;rad=4.16+.20*math.sin(i*1.9)
   new_rock('V2_1 artist-scattered talus '+str(i),(rad*math.cos(a),rad*.87*math.sin(a),-4.48),
    (.37+.18*(i%3),.32+.10*(i%2),.52+.11*(i%3)),material)
  records.append({'integrated_talus_meshes':11})
 elif asset=='defensive_props':
  if operation=='resculpt_hanging_fabric':
   candidates=[o for o in meshes if 'banner' in o.name.lower()]
   for o in candidates:
    edit(o,lambda p:(setattr(p,'y',p.y+.29*strength*math.sin(p.z*2.5+p.x*1.8)),setattr(p,'x',p.x+.12*strength*math.sin(p.z*3.7))),records)
  else:
   candidates=[o for o in meshes if any(s in o.name.lower() for s in ('torch','pedestal','stake','mounting'))]
   for o in candidates:
    edit(o,lambda p:(setattr(p,'x',p.x*(1+.18*strength*math.sin(p.z*1.5))),setattr(p,'y',p.y+.07*strength*math.cos(p.z*3))),records)
 else:raise ValueError(asset)
 assert records,(asset,operation,'no geometry modified')
 return {'raw_qwen_diagnosis':plan['observation'],'new_ai_authored_design_choice':operation,'ai_intent':plan['architectural_intent'],'ai_chosen_strength':strength,
 'blender_mcp_executed':True,'changed_geometry':records,'art_pass':False,'unity_pass':False}
for asset,plan in PLANS.items():
 folder=OUT/asset;source=folder/(asset+'-v2.blend')
 assert source.exists(),source
 bpy.ops.wm.open_mainfile(filepath=str(source))
 result={}
 def modify():result.update(author(asset,plan))
 execute(modify)
 render(folder);export(asset,folder)
 (folder/'v2-1-mcp-evidence.json').write_text(json.dumps(result,indent=2))
 print("VALORIA_AI_CREATION_DIRECTED_V2_1",asset,plan['operation'],len(result['changed_geometry']),flush=True)
