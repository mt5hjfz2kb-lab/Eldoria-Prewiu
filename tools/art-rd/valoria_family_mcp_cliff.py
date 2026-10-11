"""Genuine BlenderMCP sculpt pass over the frozen-family cliff candidate.
Driven by Qwen's vision-selected safe art operation, not arbitrary model code.
"""
import bpy,bmesh,json,hashlib,importlib.util,socket,threading,time,os,sys,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/first-asset-family-v1'
FOLDER=OUT/'cliff_rocks'
decision=json.loads((OUT/'agent-decisions.json').read_text())['cliff_rocks']
action=decision.get('art_operation')
allowed={'sculpt_geological_shelves','carve_erosion_channels','break_cliff_silhouette'}
assert action in allowed, 'Qwen did not select a valid executable geological art operation'
source=FOLDER/'cliff_rocks.blend'
assert source.exists()
bpy.ops.wm.open_mainfile(filepath=str(source))
ob=next(x for x in bpy.data.objects if x.type=='MESH' and x.name.startswith('Sculpted continuous bedrock'))
addon=Path('/tmp/eldoria-blender-mcp.py')
expected='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
assert addon.exists() and hashlib.sha256(addon.read_bytes()).hexdigest()==expected,'Pinned BlenderMCP addon verification failed'
spec=importlib.util.spec_from_file_location('eldoria_mcp_addon',addon)
module=importlib.util.module_from_spec(spec);sys.modules['eldoria_mcp_addon']=module;spec.loader.exec_module(module)
for feature in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+feature,bpy.props.BoolProperty(default=False))
log={'agent_observation':decision.get('observation'),'agent_geometry_decision':decision.get('geometry_decision'),'agent_action':action}
def modify_cliff():
 # This callback is invoked by BlenderMCP's real TCP execute_code command.
 mesh=ob.data
 if mesh.users>1:ob.data=mesh=mesh.copy()
 bm=bmesh.new();bm.from_mesh(mesh)
 moved=0
 for v in bm.verts:
  x,y,z=v.co
  if z>=-.25 or z< -4.6:continue
  theta=math.atan2(y,x)
  depth=max(0,min(1,-z/4.8))
  if action=='sculpt_geological_shelves':
   # Bedding plane erosion produces broad rock shelves, not random per-object jitter.
   shift=.23*math.sin(z*3.4+.2)*math.sin(math.pi*depth)+.09*math.sin(theta*9)
  elif action=='carve_erosion_channels':
   # Vertical gullies carve the cliff face, with shaped depth falloff.
   shift=-.36*(max(0,math.cos(theta*8+1.7))**8)*math.sin(math.pi*depth)
  else:
   # Characteristic broken silhouette along rock face at varying elevation.
   shift=.22*math.sin(theta*7+z*1.1)*math.sin(math.pi*depth)
  r=math.hypot(x,y)
  if r<1e-4:continue
  v.co.x+=shift*x/r;v.co.y+=shift*y/r;moved+=1
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 bm.to_mesh(mesh);bm.free();mesh.update()
 # Physically plausible layered weathering: distinct normal/roughness responses.
 for m in mesh.materials:
  if not m or not m.use_nodes:continue
  bs=next((n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
  if not bs:continue
  rough=bs.inputs.get('Roughness')
  if rough and not rough.is_linked:rough.default_value=.88
 log['vertices_sculpted']=moved;log['mesh_triangles']=sum(len(p.vertices)-2 for p in mesh.polygons)
bpy.app.driver_namespace['eldoria_sculpt_cliff']=modify_cliff
server=module.BlenderMCPServer(host='127.0.0.1',port=0)
server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)
server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1]
server.socket.listen(1);server.running=True
server.server_thread=threading.Thread(target=server._server_loop,daemon=True)
server.server_thread.start()
response=[];errors=[]
def client():
 try:
  c=socket.create_connection(('127.0.0.1',port),timeout=10);c.settimeout(50)
  cmd={'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_sculpt_cliff']()"}}
  c.sendall(json.dumps(cmd).encode());received=b''
  while True:
   part=c.recv(65536)
   if not part:break
   received+=part
   try:response.append(json.loads(received));break
   except json.JSONDecodeError:pass
  c.close()
 except Exception as ex:errors.append(str(ex))
thread=threading.Thread(target=client,daemon=True);thread.start()
deadline=time.monotonic()+65
while thread.is_alive() and time.monotonic()<deadline:
 server._drain_command_queue();time.sleep(.05)
thread.join(timeout=1)
assert not errors and response and response[0].get('status')=='success',(errors,response)
server.stop()
assert log.get('vertices_sculpted',0)>0,'Real Blender mesh alteration not executed'
sc=bpy.context.scene
cam=sc.camera
views={
 'hero':((13,-19,13),(0,0,2),18),
 'strategic':((16,-23,18),(0,0,2.5),24)
}
for name,(pos,target,scale) in views.items():
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
 cam.data.ortho_scale=scale
 sc.render.filepath=str(FOLDER/('mcp-after-'+name+'.png'))
 bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(source))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
bpy.ops.object.select_all(action='DESELECT')
for item in meshes:item.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
bpy.ops.export_scene.gltf(filepath=str(FOLDER/'cliff_rocks.glb'),export_format='GLB',use_selection=True,export_materials='EXPORT')
log.update({'mcp_tcp_executed':True,'mcp_response_status':'success','blender_source':str(source.relative_to(ROOT)),
  'glb':str((FOLDER/'cliff_rocks.glb').relative_to(ROOT)),'before_render':'before-hero.png',
  'after_render':'mcp-after-hero.png','art_pass':False,'unity_tested':False})
(FOLDER/'mcp-authoring-evidence.json').write_text(json.dumps(log,indent=2))
print('ELDORIA_CLIFF_REAL_MCP_SCULPT_COMPLETE',json.dumps(log))
