"""Apply one allowlisted art decision through REAL existing BlenderMCP TCP transport.
No arbitrary AI code. No paid integrations, telemetry or remote services.
"""
import bpy,bmesh,json,os,sys,socket,threading,time,importlib.util,hashlib,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'art-source/valoria/production/studio-rocks-v1'
bpy.ops.wm.open_mainfile(filepath=str(OUT/'ValoriaRockStudy.blend'))
plan_path=OUT/'qwen-art-decision.json'
plan=json.loads(plan_path.read_text()) if plan_path.exists() else {}
if plan.get('status')=='unavailable_or_invalid' or not plan.get('next_action'):
 plan={'next_action':'bury_seams','decision':'reject_candidate','source':'assistant_direct_visual_review','reason':'v2: summit cut opens cavities and grass-to-rock contacts still require continuous buried support.'}
 (OUT/'director-contact-decision.json').write_text(json.dumps(plan,indent=2))
if plan['next_action'] in ('change_donor','retain_baseline','keep_for_unity_test'):
 (OUT/'mcp-contact-report.json').write_text(json.dumps({'status':'no_geometry_correction_selected','decision':plan,'mcp_used':False,'visual_pass':False},indent=2));print('Candidate rejected; no forced correction');raise SystemExit(0)
addon=Path(os.environ.get('ELDORIA_BLENDER_MCP_ADDON','/tmp/eldoria-blender-mcp.py'))
assert addon.is_file()
config=json.loads((ROOT/'pipeline/art-studio-recovery-v1.json').read_text())
assert hashlib.sha256(addon.read_bytes()).hexdigest()==config['mcp_addon_sha256']
# Blender ships requests; no addon registration or integrations are enabled.
spec=importlib.util.spec_from_file_location('blender_mcp',addon);module=importlib.util.module_from_spec(spec);sys.modules['blender_mcp']=module;spec.loader.exec_module(module)
for feature in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+feature,bpy.props.BoolProperty(default=False))
changes={} 
def repair():
 objects=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('CC0 directed cliff course')]
 assert objects
 mesh=objects[0].data
 bm=bmesh.new();bm.from_mesh(mesh)
 boundaries=[e for e in bm.edges if e.is_boundary]
 filled=bmesh.ops.holes_fill(bm,edges=boundaries,sides=0)['faces'] if boundaries else []
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free();mesh.update()
 for o in objects:o.rotation_euler.x=0;o.rotation_euler.y=0;o.location.z-=.34
 # A buried continuous receiver from the exact existing support, no new noise generator.
 prior=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_visual_slice_environment_v2.glb'))
 receiver=[o for o in bpy.data.objects if o not in prior and o.type=='MESH'][0]
 bm=bmesh.new();bm.from_mesh(receiver.data)
 rock_indices={i for i,m in enumerate(receiver.data.materials) if m and 'Weathered layered granite' in m.name}
 discard=[f for f in bm.faces if f.material_index not in rock_indices or not any(math.hypot(v.co.x,v.co.y)>35 for v in f.verts)]
 bmesh.ops.delete(bm,geom=discard,context='FACES');bm.to_mesh(receiver.data);bm.free();receiver.data.update()
 for v in receiver.data.vertices:
  r=math.hypot(v.co.x,v.co.y)
  if r>37:v.co.x*=max(0,(r-3.0))/r;v.co.y*=max(0,(r-3.0))/r
 receiver.name='Buried continuous cliff contact receiver - existing geometry'
 mat=objects[0].data.materials[0];receiver.data.materials.clear();receiver.data.materials.append(mat)
 for p in receiver.data.polygons:p.material_index=0;p.use_smooth=True
 changes.update(rock_instances_adjusted=len(objects),cut_caps_filled=len(filled),receiver_triangles=sum(len(p.vertices)-2 for p in receiver.data.polygons))
bpy.app.driver_namespace['eldoria_contact_repair']=repair
server=module.BlenderMCPServer(host='127.0.0.1',port=0);server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM);server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True;server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
response=[];errors=[]
def client():
 try:
  c=socket.create_connection(('127.0.0.1',port),timeout=5);c.settimeout(40)
  c.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_contact_repair']()"}}).encode());data=b''
  while True:
   part=c.recv(65536)
   if not part:break
   data+=part
   try:response.append(json.loads(data));break
   except json.JSONDecodeError:pass
  c.close()
 except Exception as e:errors.append(str(e))
th=threading.Thread(target=client,daemon=True);th.start();deadline=time.monotonic()+45
while th.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.05)
th.join(timeout=1);assert not errors and response and response[0].get('status')=='success',(errors,response)
server.stop();assert changes
# Same camera, same world and lights; state loaded from genuine previous source.
s=bpy.context.scene;cam=s.camera
views={'overview':((50,-70,49),(0,4,0),108,1280,800),'cliff':((52,-64,18),(15,-24,-2),44,1280,800),'mobile':((50,-70,49),(0,4,0),108,640,400)}
for name,(pos,target,scale,rx,ry) in views.items():
 old=OUT/f'after-{name}.png'
 if old.exists():old.replace(OUT/f'pre-mcp-{name}.png')
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale;s.render.resolution_x=rx;s.render.resolution_y=ry;s.render.filepath=str(old);bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaRockStudy.blend'))
all_meshes=[o for o in bpy.data.objects if o.type=='MESH'];rock_meshes=[o for o in all_meshes if o.name.startswith('CC0 ') or o.name.startswith('Buried continuous')]
bpy.ops.object.select_all(action='DESELECT')
for o in rock_meshes:o.select_set(True)
bpy.context.view_layer.objects.active=rock_meshes[0];bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaScannedCliffCandidate.glb'),export_format='GLB',use_selection=True,export_apply=True)
terrain=[o for o in all_meshes if o.name.startswith('Valoria Highlands')]
assert len(terrain)==1
for o in terrain:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaEnvironmentCandidate.glb'),export_format='GLB',use_selection=True,export_apply=True)
report={'mcp_tcp_execution':True,'addon_sha256':hashlib.sha256(addon.read_bytes()).hexdigest(),'decision':plan,'response_status':response[0]['status'],'geometry_changes':changes,'same_render_conditions':True,'visual_pass':False,'unity_tested':False,'services_cost_eur':0}
(OUT/'mcp-contact-report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
