"""Director-selected course overlap, executed through the existing BlenderMCP TCP queue."""
import bpy, hashlib, importlib.util, json, math, os, shutil, socket, sys, threading, time
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'art-source/valoria/production/studio-rocks-v2'
OUT=ROOT/'art-source/valoria/production/studio-rocks-v2-mcp'
OUT.mkdir(parents=True,exist_ok=True)
addon=Path(os.environ.get('ELDORIA_BLENDER_MCP_ADDON','/tmp/eldoria-blender-mcp.py'))
config=json.loads((ROOT/'pipeline/art-studio-recovery-v1.json').read_text())
assert hashlib.sha256(addon.read_bytes()).hexdigest()==config['mcp_addon_sha256']
bpy.ops.wm.open_mainfile(filepath=str(BASE/'ValoriaRockStudyV2.blend'))
for name in ('overview','cliff','mobile'):
 shutil.copy2(BASE/f'after-{name}.png',OUT/f'before-{name}.png')
spec=importlib.util.spec_from_file_location('blender_mcp',addon)
module=importlib.util.module_from_spec(spec);sys.modules['blender_mcp']=module;spec.loader.exec_module(module)
for feature in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+feature,bpy.props.BoolProperty(default=False))
changes={}
def overlap_contacts():
 objects=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('CC0 cluster directed cliff course')]
 assert len(objects)==21
 for o in objects:
  # Existing scan modules have visible side gaps; enlarge their tangential span
  # and seat their backs under the existing lawn. No displacement/noise loop.
  o.scale.x*=1.16;o.scale.y*=1.06
  r=math.hypot(o.location.x,o.location.y)
  o.location.x*=max(0,r-.24)/r;o.location.y*=max(0,r-.24)/r
 changes.update(rock_modules_seated=len(objects),tangential_scale=1.16,radial_inset=.24)
bpy.app.driver_namespace['eldoria_cluster_contact_correction']=overlap_contacts
server=module.BlenderMCPServer(host='127.0.0.1',port=0)
server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)
server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1]
server.socket.listen(1);server.running=True
server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
response=[];errors=[]
def client():
 try:
  conn=socket.create_connection(('127.0.0.1',port),timeout=5);conn.settimeout(40)
  conn.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_cluster_contact_correction']()"}}).encode())
  data=b''
  while True:
   chunk=conn.recv(65536)
   if not chunk:break
   data+=chunk
   try:response.append(json.loads(data));break
   except json.JSONDecodeError:pass
  conn.close()
 except Exception as exc:errors.append(str(exc))
thread=threading.Thread(target=client,daemon=True);thread.start();deadline=time.monotonic()+45
while thread.is_alive() and time.monotonic()<deadline:
 server._drain_command_queue();time.sleep(.05)
thread.join(timeout=1)
assert not errors and response and response[0].get('status')=='success' and changes,(errors,response)
server.stop()

scene=bpy.context.scene;camera=scene.camera
views={'overview':((50,-70,49),(0,4,0),108,1280,800),'cliff':((52,-64,18),(15,-24,-2),44,1280,800),'mobile':((50,-70,49),(0,4,0),108,640,400)}
for name,(pos,target,scale,rx,ry) in views.items():
 camera.location=pos;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
 camera.data.ortho_scale=scale;scene.render.resolution_x=rx;scene.render.resolution_y=ry
 scene.render.filepath=str(OUT/f'after-{name}.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaRockStudyV2MCP.blend'))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
rocks=[o for o in meshes if o.name.startswith('CC0 ') or o.name.startswith('Buried continuous')]
terrain=[o for o in meshes if o.name.startswith('Valoria Highlands')]
assert len(terrain)==1
bpy.ops.object.select_all(action='DESELECT')
for o in rocks:o.select_set(True)
bpy.context.view_layer.objects.active=rocks[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaScannedCliffV2MCP.glb'),export_format='GLB',use_selection=True,export_apply=True)
terrain[0].select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaEnvironmentV2MCP.glb'),export_format='GLB',use_selection=True,export_apply=True)
(OUT/'mcp-report.json').write_text(json.dumps({'transport':'real pinned BlenderMCP TCP main-thread command queue',
 'addon_sha256':hashlib.sha256(addon.read_bytes()).hexdigest(), 'decision':'director visual review of real V2 render: seat 21 gaps once',
 'response_status':response[0]['status'],'geometry_changes':changes,'same_camera_and_lighting':True,
 'visual_pass':False,'unity_tested':False,'paid_services_used':False},indent=2))
