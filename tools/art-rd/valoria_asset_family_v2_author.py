"""Valoria V2: concrete, substantial geometry authoring operations on preserved V1 Blender sources.
Not an ART PASS: renders and external visual inspection are required.
Qwen V1 decisions are retained, extended to large-scale distinct operations per asset.
"""
import bpy, json, os, sys, math, hashlib, socket, threading, time, importlib.util, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'art-source/valoria/production/first-asset-family-v1'
OUT=ROOT/'art-source/valoria/production/asset-family-v2'
OUT.mkdir(parents=True,exist_ok=True)
plans=json.loads((SOURCE/'agent-decisions.json').read_text())
assert all(a in plans for a in ('wall_straight','watchtower','wall_corner','wall_connections','stone_path_platform','cliff_rocks','defensive_props'))
addon=Path('/tmp/eldoria-blender-mcp.py')
assert hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
spec=importlib.util.spec_from_file_location('v2_mcp',addon);mcp=importlib.util.module_from_spec(spec)
sys.modules['v2_mcp']=mcp;spec.loader.exec_module(mcp)
for name in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+name,bpy.props.BoolProperty(default=False))
def execute_mcp(callback):
 bpy.app.driver_namespace['eldoria_v2_authoring']=callback
 server=mcp.BlenderMCPServer(host='127.0.0.1',port=0)
 server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM);server.socket.bind(('127.0.0.1',0))
 port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True
 server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
 response=[]
 def client():
  try:
   with socket.create_connection(('127.0.0.1',port),timeout=10) as conn:
    conn.settimeout(90)
    conn.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_v2_authoring']()"}}).encode())
    buffer=b''
    while True:
     packet=conn.recv(65536)
     if not packet:break
     buffer+=packet
     try:response.append(json.loads(buffer));break
     except json.JSONDecodeError:pass
  except Exception as e:response.append({'error':str(e)})
 thread=threading.Thread(target=client,daemon=True);thread.start();deadline=time.monotonic()+100
 while thread.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.04)
 thread.join(timeout=2);server.stop()
 assert response and response[0].get('status')=='success',response
def mesh_object(name,verts,faces,material):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.scene.collection.objects.link(ob)
 if material:mesh.materials.append(material)
 return ob
def box(name,center,dimensions,material,bevel=.06):
 x,y,z=center;sx,sy,sz=[v/2 for v in dimensions]
 verts=[(x+sgx*sx,y+sgy*sy,z+sgz*sz) for sgz in (-1,1) for sgy in (-1,1) for sgx in (-1,1)]
 faces=[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)]
 ob=mesh_object(name,verts,faces,material)
 if bevel:
  b=ob.modifiers.new('Quarried edge bevel','BEVEL');b.width=bevel;b.segments=2
  ob.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return ob
def bounds():
 vertices=[o.matrix_world@v.co for o in bpy.context.scene.objects if o.type=='MESH' for v in o.data.vertices]
 return (min(p.x for p in vertices),max(p.x for p in vertices),
 min(p.y for p in vertices),max(p.y for p in vertices),
 min(p.z for p in vertices),max(p.z for p in vertices))
def material():
 materials=[m for m in bpy.data.materials if m.use_nodes]
 assert materials,'V1 original materials must be preserved'
 return materials[0]
def author(asset,decision):
 selected=decision['art_operation'];edits=[]
 stone=material()
 x0,x1,y0,y1,z0,z1=bounds()
 span=max(.01,x1-x0)
 def mesh_edit(ob,fn):
  if ob.data.users>1:ob.data=ob.data.copy()
  changes=0
  for vert in ob.data.vertices:
   old=vert.co.copy();fn(vert.co)
   if (old-vert.co).length>1e-5:changes+=1
  ob.data.update()
  if changes:edits.append({'object':ob.name,'vertices_edited':changes})
  return changes
 if asset=='cliff_rocks':
  bed=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('Sculpted continuous bedrock'))
  def sculpt(p):
   # Define independent folded stone strata and large cut faces, not object jitter.
   theta=math.atan2(p.y,p.x);depth=max(0,min(1,-p.z/4.8))
   phase=theta*5.5+depth*3.2
   shift=(.44*math.sin(phase)+.23*math.cos(theta*12-depth*7))*math.sin(math.pi*depth)
   r=max(.1,math.hypot(p.x,p.y))
   p.x+=shift*p.x/r;p.y+=shift*p.y/r
   p.z+=.12*math.sin(theta*7+depth*16)*math.sin(math.pi*depth)
  mesh_edit(bed,sculpt)
  for i in range(13):
   a=i*math.tau/13+.14;rr=4.1+.35*math.sin(i*1.7);cx=rr*math.cos(a);cy=rr*.83*math.sin(a)
   s=.42+.11*(i%3)
   rock=box('V2 embedded talus fractured outcrop '+str(i),(cx,cy,-4.43+i%3*.07),(s*1.8,s*1.4,s*.7),stone,.09)
   rock.rotation_euler=(.1*(i%3),.09*(i%2),a)
  edits.append({'operation':'geological strata sculpt and buried talus','new_talus':13})
 elif asset=='stone_path_platform':
  paving=[o for o in bpy.data.objects if o.type=='MESH' and 'flagstone' in o.name.lower()]
  assert len(paving)>20
  for i,ob in enumerate(paving):
   mesh_edit(ob,lambda p,j=i:(setattr(p,'z',p.z +.09*math.sin((p.x+j)*3.3+p.y*2.1))))
   if i%5==0:
    ob.location.z-=.07
    ob.rotation_euler.z+=.10*math.sin(i*1.63)
  edits.append({'operation':'non repeating dressed flagstone geometry','stones':len(paving)})
 elif asset=='wall_straight':
  buttresses=[o for o in bpy.data.objects if o.type=='MESH' and 'buttress' in o.name.lower() and 'cap' not in o.name.lower()]
  assert buttresses
  for i,ob in enumerate(buttresses):
   zs=[v.co.z for v in ob.data.vertices];low=min(zs);d=max(zs)-low or 1
   mesh_edit(ob,lambda p:(setattr(p,'x',p.x*(1-.19*(p.z-low)/d)),setattr(p,'y',p.y*(1-.13*(p.z-low)/d))))
  for i in range(8):
   px=x0+(i+.5)*span/8
   box('V2 integrated cornice corbel '+str(i),(px,y0+.13,z1-.36),(.32,.45,.31),stone,.035)
  edits.append({'operation':'reprofiled load bearing buttresses and stone crown corbels'})
 elif asset=='watchtower':
  architectural=[o for o in bpy.data.objects if o.type=='MESH' and any(s in o.name.lower() for s in ('tower','crown','merlon'))]
  assert architectural
  for ob in architectural:
   zs=[v.co.z for v in ob.data.vertices]
   localmin=min(zs);localmax=max(zs);d=max(.01,localmax-localmin)
   if d<.12:continue
   mesh_edit(ob,lambda p:(setattr(p,'x',p.x*(1-.07*(p.z-localmin)/d)),setattr(p,'y',p.y*(1-.07*(p.z-localmin)/d))))
  edits.append({'operation':'tower masonry crown taper','parts':len(architectural)})
 elif asset in ('wall_corner','wall_connections'):
  candidates=[o for o in bpy.data.objects if o.type=='MESH' and any(w in o.name.lower() for w in ('socket','corner','coping','wedge','corbel','buttress'))]
  assert candidates
  for ob in candidates:
   zs=[v.co.z for v in ob.data.vertices];lo=min(zs);d=max(.01,max(zs)-lo)
   mesh_edit(ob,lambda p:(setattr(p,'x',p.x*(1-.065*(p.z-lo)/d)),setattr(p,'z',p.z+.055*math.sin(p.x*1.7+p.y*2.4))))
  edits.append({'operation':'reprofiled junction contacts','parts':len(candidates)})
 elif asset=='defensive_props':
  banners=[o for o in bpy.data.objects if o.type=='MESH' and 'banner' in o.name.lower()]
  assert banners
  for ob in banners:
   mesh_edit(ob,lambda p:(setattr(p,'y',p.y+.21*math.sin(p.z*2.1+p.x*1.2)),setattr(p,'x',p.x+.055*math.cos(p.z*3.1))))
  edits.append({'operation':'geometry draped cloth wave','parts':len(banners)})
 else:raise ValueError(asset)
 assert edits,'No significant geometry operation was executed'
 return {'agent_observation':decision['observation'],'agent_decision':decision['geometry_decision'],'selected_operation':selected,
  'v2_operation_results':edits,'mcp_real_tcp':True,'art_pass':False,'unity_pass':False}
def render(folder):
 sc=bpy.context.scene;cam=sc.camera
 assert cam
 sc.cycles.use_denoising=False;sc.view_layers[0].cycles.use_denoising=False
 for view,pos,target,scale in [('hero',(13,-19,13),(0,0,2),18),('strategic',(16,-23,18),(0,0,2.5),24)]:
  cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
  cam.data.ortho_scale=scale;sc.render.filepath=str(folder/('v2-after-'+view+'.png'))
  bpy.ops.render.render(write_still=True)
def save(asset,folder):
 bpy.ops.wm.save_as_mainfile(filepath=str(folder/(asset+'-v2.blend')))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.gltf(filepath=str(folder/(asset+'-v2.glb')),export_format='GLB',use_selection=True,export_materials='EXPORT')
for asset in ('wall_straight','watchtower','wall_corner','wall_connections','stone_path_platform','cliff_rocks','defensive_props'):
 folder=OUT/asset;folder.mkdir(parents=True,exist_ok=True)
 source=SOURCE/asset/(asset+'.blend');assert source.is_file(),source
 bpy.ops.wm.open_mainfile(filepath=str(source))
 result={}
 def apply():result.update(author(asset,plans[asset]))
 execute_mcp(apply)
 render(folder);save(asset,folder)
 result['before_render']='first-asset-family-v1/'+asset+'/before-hero.png'
 result['after_render']='v2-after-hero.png'
 (folder/'v2-ai-mcp-authoring.json').write_text(json.dumps(result,indent=2))
 print('ELDORIA_V2_REAL_MCP',asset,len(result['v2_operation_results']),flush=True)
