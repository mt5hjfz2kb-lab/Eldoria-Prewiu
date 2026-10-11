"""Apply Qwen-selected architectural interventions through proven BlenderMCP TCP.
Each changed Blender mesh, close render, strategic render and trace is preserved.
"""
import bpy,bmesh,math,json,sys,socket,time,hashlib,threading,importlib.util
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/first-asset-family-v1'
plans=json.loads((OUT/'agent-decisions.json').read_text())
allowed={
 'wall_straight':('break_ashlar_cadence','soften_buttress_arrises'),
 'watchtower':('taper_crown_silhouette','weather_radial_ashlar'),
 'wall_corner':('dress_miter_join','soften_corner_coping'),
 'wall_connections':('taper_socket_contacts','chip_dressed_edges'),
 'stone_path_platform':('sculpt_stone_treads','disrupt_paving_repetition'),
 'defensive_props':('relax_banner_silhouette','weather_defensive_stakes')
}
addon=Path('/tmp/eldoria-blender-mcp.py')
assert addon.is_file() and hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
spec=importlib.util.spec_from_file_location('eldoria_family_mcp',addon)
module=importlib.util.module_from_spec(spec);sys.modules['eldoria_family_mcp']=module;spec.loader.exec_module(module)
for feature in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+feature,bpy.props.BoolProperty(default=False))
results={}
def run_mcp(callback):
 bpy.app.driver_namespace['eldoria_authored_change']=callback
 server=module.BlenderMCPServer(host='127.0.0.1',port=0)
 server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)
 server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1]
 server.socket.listen(1);server.running=True
 server.server_thread=threading.Thread(target=server._server_loop,daemon=True)
 server.server_thread.start()
 responses=[];errors=[]
 def client():
  try:
   c=socket.create_connection(('127.0.0.1',port),timeout=10);c.settimeout(90)
   c.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_authored_change']()"}}).encode())
   buf=b''
   while True:
    part=c.recv(65536)
    if not part:break
    buf+=part
    try:responses.append(json.loads(buf));break
    except json.JSONDecodeError:pass
   c.close()
  except Exception as exc:errors.append(str(exc))
 t=threading.Thread(target=client,daemon=True);t.start();end=time.monotonic()+100
 while t.is_alive() and time.monotonic()<end:
  server._drain_command_queue();time.sleep(.05)
 t.join(timeout=1);server.stop()
 assert not errors and responses and responses[0].get('status')=='success',(errors,responses)
for asset,choices in allowed.items():
 decision=plans[asset];op=decision.get('art_operation')
 assert op in choices,(asset,'AI must select a valid geometry action',decision)
 folder=OUT/asset
 source=folder/(asset+'.blend')
 bpy.ops.wm.open_mainfile(filepath=str(source))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 assert meshes
 edited=[]
 def sculpt():
  for ob in meshes:
   name=ob.name.lower()
   is_stone=any(word in name for word in ('stone','ashlar','buttress','coping','merlon','socket','tower','flagstone','ramp','course','banner','stake','parapet','radial'))
   if not is_stone:continue
   # All operations are controlled and bounded. No execution of free-form model text.
   if op in ('taper_crown_silhouette','weather_radial_ashlar') and 'tower' not in name and 'crown' not in name and 'radial' not in name:continue
   if op=='relax_banner_silhouette' and 'banner' not in name:continue
   if op=='weather_defensive_stakes' and 'stake' not in name:continue
   if op=='taper_socket_contacts' and 'socket' not in name:continue
   if op=='dress_miter_join' and 'corner' not in name:continue
   if op=='soften_corner_coping' and 'coping' not in name:continue
   if op=='soften_buttress_arrises' and 'buttress' not in name:continue
   if op in ('sculpt_stone_treads','disrupt_paving_repetition') and not any(w in name for w in ('flagstone','ramp','path')):continue
   if ob.data.users>1:ob.data=ob.data.copy()
   mesh=ob.data
   if not mesh.vertices:continue
   h=hashlib.sha256(name.encode()).digest()
   strength=.025+(h[0]/255)*.025
   vertices=mesh.vertices
   for v in vertices:
    x,y,z=v.co
    # Purposeful high points, worn lower joints, slanted masonry contact.
    if op in ('sculpt_stone_treads','disrupt_paving_repetition'):
     v.co.z+=strength*math.sin(x*2.3+y*1.8+int(h[1])*.02)
     v.co.x+=strength*.35*math.sin(y*3.8)
    elif op in ('weather_radial_ashlar','break_ashlar_cadence','chip_dressed_edges'):
     v.co.z+=strength*math.sin(1.5*x+2.2*z+int(h[2])*.02)
    elif op in ('taper_crown_silhouette','taper_socket_contacts','dress_miter_join'):
     v.co.x+=strength*math.sin(z*1.8)*(1 if x>=0 else -1)
     v.co.y+=strength*.4*math.sin(x*1.9)
    elif op in ('relax_banner_silhouette','weather_defensive_stakes'):
     v.co.x+=strength*math.sin(z*3.1+y*2.8)
    else:
     v.co.z+=strength*.42*math.sin(2.3*x+1.5*y)
   mesh.update()
   for modifier in ob.modifiers:
    if modifier.type=='BEVEL':
     modifier.segments=max(2,modifier.segments)
     modifier.width=max(.012,min(.085,modifier.width))
   edited.append({'object':ob.name,'vertices':len(vertices)})
  assert edited,'MCP command did not change relevant artist-authored geometry'
 run_mcp(sculpt)
 sc=bpy.context.scene;cam=sc.camera
 for view,(pos,target,scale) in {
  'hero':((13,-19,13),(0,0,2),18),
  'strategic':((16,-23,18),(0,0,2.5),24)}.items():
  cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
  cam.data.ortho_scale=scale
  sc.render.filepath=str(folder/('mcp-after-'+view+'.png'))
  bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(source))
 bpy.ops.object.select_all(action='DESELECT')
 for ob in meshes:ob.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.gltf(filepath=str(folder/(asset+'.glb')),export_format='GLB',use_selection=True,export_materials='EXPORT')
 report={'asset':asset,'agent_vision_observation':decision['observation'],
  'agent_vision_decision':decision['geometry_decision'],'operation_selected_by_qwen':op,
  'blender_mcp_status':'executed_tcp_success','edited_mesh_count':len(edited),
  'edited_meshes':edited[:25],'unmodified_hero_gate_v3':True,
  'paired_before':'before-hero.png','paired_after':'mcp-after-hero.png','art_pass':False,'unity_pass':False}
 (folder/'mcp-authoring-evidence.json').write_text(json.dumps(report,indent=2))
 results[asset]={'edited_meshes':len(edited),'operation':op}
 print('ELDORIA_MCP_FAMILY_AUTHORED',asset,json.dumps(results[asset]))
(OUT/'mcp-family-authoring-index.json').write_text(json.dumps(results,indent=2))
