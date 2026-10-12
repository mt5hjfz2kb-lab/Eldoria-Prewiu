"""Controlled cliff variant: Qwen visual direction -> BlenderMCP mesh edit -> GLB.
The frozen V3 .blend and .glb remain untouched. A workflow supplies the two images.
"""
import json, os, sys, pathlib, hashlib, time
ROOT=pathlib.Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/variants/unity-cliff-v1'
OUT.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'art-source/valoria/production/asset-family-v2/quality-v3/cliff_rocks/cliff_rocks.blend'
if '--infer' in sys.argv:
 import base64, urllib.request
 from PIL import Image, ImageOps, ImageDraw
 target=ROOT/'references/VALORIA_FAMILY_OWNER_TARGET_20261011.jpg'
 current=pathlib.Path(os.environ['VALORIA_UNITY_CLIFF_SCREENSHOT'])
 assert SOURCE.exists() and target.exists() and current.exists()
 board=Image.new('RGB',(640,360),'#353d46')
 for i,p in enumerate((target,current)):
  with Image.open(p) as im:
   im=ImageOps.contain(im.convert('RGB'),(320,340))
   board.paste(im,(i*320+(320-im.width)//2,(360-im.height)//2))
 board.save(OUT/'art-direction-input.jpg',quality=88)
 prompt=('You are the external 3D artist for Valoria. Left: aspirational medieval cliff/fortress. Right: genuine Unity capture of approved V3 meshes. '
         'Diagnose why the cliff reads as a rectangular extruded slab and choose a decisive sculptural correction in Blender. '
         'Maintain the 12m modular span, world-unit contact at crest z=0 and source Hero V3 architecture. '
         'Reply ONLY JSON with diagnosis, artistic_intent, fault_amplitude_m (0.4 to 1.3), large_facets (4 to 9), '
         'crest_recede_m (0.2 to 0.9), talus_clusters (3 to 8), and specific_visible_change. '
         'Choose numbers as an artist based on the two images. No self-approval.')
 payload={'model':'qwen3-vl:4b-instruct','stream':False,'think':False,'format':'json',
          'options':{'temperature':.18,'num_predict':600,'num_ctx':2048},
          'messages':[{'role':'user','content':prompt,'images':[base64.b64encode((OUT/'art-direction-input.jpg').read_bytes()).decode()]}]}
 req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=600) as r:reply=json.load(r)
 raw=reply['message']['content'];plan=json.loads(raw)
 for k,lo,hi in [('fault_amplitude_m',.4,1.3),('large_facets',4,9),('crest_recede_m',.2,.9),('talus_clusters',3,8)]:
  assert lo<=float(plan[k])<=hi,(k,plan[k])
 for k in ('diagnosis','artistic_intent','specific_visible_change'):assert len(plan[k])>=25,(k,plan[k])
 (OUT/'direction.json').write_text(json.dumps({'plan':plan,'raw_qwen_reply':raw,'model':payload['model'],
  'prompt':prompt,'target_sha256':hashlib.sha256(target.read_bytes()).hexdigest(),
  'unity_capture_sha256':hashlib.sha256(current.read_bytes()).hexdigest(),
  'source_blend_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'self_approval':False},indent=2))
 print('EXTERNAL_ARTIST_DIRECTION',plan,flush=True)
 sys.exit(0)

import bpy, math, socket, threading, importlib.util
from mathutils import Vector
plan=json.loads((OUT/'direction.json').read_text())['plan']
amp=float(plan['fault_amplitude_m']);facets=int(plan['large_facets']);recede=float(plan['crest_recede_m']);clusters=int(plan['talus_clusters'])
addon=pathlib.Path('/tmp/eldoria-blender-mcp.py')
assert hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
spec=importlib.util.spec_from_file_location('valoria_cliff_mcp',str(addon));mcp=importlib.util.module_from_spec(spec);sys.modules['valoria_cliff_mcp']=mcp;spec.loader.exec_module(mcp)
for name in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 setattr(bpy.types.Scene,'blendermcp_use_'+name,bpy.props.BoolProperty(default=False))
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
bed=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('Continuous angular escarpment bedrock'))
crest=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('Continuous soil and grass cliff crest'))
old=bed.data.copy();old.name='Frozen bedrock for comparable clay capture'
change_log={}
def snapshot(path):
 sc=bpy.context.scene
 if not sc.camera:
  cam=bpy.data.cameras.new('Art inspection orthographic');obj=bpy.data.objects.new('Art inspection orthographic',cam);sc.collection.objects.link(obj);sc.camera=obj
 sc.camera.location=(15,-22,14)
 sc.camera.rotation_euler=(Vector((0,0,-1.7))-sc.camera.location).to_track_quat('-Z','Y').to_euler()
 sc.camera.data.type='ORTHO';sc.camera.data.ortho_scale=19
 sc.render.resolution_x=1200;sc.render.resolution_y=800;sc.render.resolution_percentage=100
 sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
snapshot(OUT/'before-blender.png')
def author():
 # Faults are broad planar masses; keep x seam vertices fixed for modular joins.
 # Bedrock face and crest are modified in one operation to avoid a detached cap.
 count=0;max_delta=0
 for v in bed.data.vertices:
  x,y,z=v.co
  if y>0.9 or z<-3.55:continue
  u=max(0,min(1,-z/3.5))
  seam=max(0,min(1,(6-abs(x))/.68))
  t=(x+6)/12*facets
  facet=(1-2*abs((t%1)-.5)) # triangular geological planes
  broad=math.sin(t*math.pi*.72)+.45*math.sin(t*math.pi*1.37)
  change=seam*(.35+u*.65)*amp*(.6*facet+.4*broad)
  if y<-1.0:
   v.co.y=y-change
   if z>-.5:v.co.y+=seam*recede*(1-u)
   count+=1;max_delta=max(max_delta,abs(change))
 bed.data.update()
 for v in crest.data.vertices:
  x,y,z=v.co
  if y>-1.0:continue
  seam=max(0,min(1,(6-abs(x))/.68))
  t=(x+6)/12*facets;facet=1-2*abs((t%1)-.5)
  v.co.y=y-seam*amp*(.6*facet+.4*math.sin(t*math.pi*.72))*.35+seam*recede
 crest.data.update()
 # Large, unequal talus wedges visually connect the face to the ground.
 # Their mesh geometry is artist-directed by Qwen's chosen facet and cluster counts.
 rock=bed.data.materials[0]
 for i in range(clusters):
  x=-5.2+10.4*i/max(1,clusters-1)
  w=.7+(i%3)*.19;h=.58+(i%4)*.22
  verts=[(x-w,-2.3,-3.48),(x+w,-2.24,-3.46),(x+.33*w,-2.92-h*.4,-3.45),
         (x-.3*w,-2.95-h*.4,-3.42),(x,-2.55,-3.44+h)]
  faces=[(0,3,4),(3,2,4),(2,1,4),(1,0,4),(0,1,2,3)]
  mesh=bpy.data.meshes.new('AI-directed fractured talus '+str(i));mesh.from_pydata(verts,[],faces);mesh.update()
  mesh.materials.append(rock)
  ob=bpy.data.objects.new(mesh.name,mesh);bpy.context.scene.collection.objects.link(ob)
 change_log.update({'bedrock_vertices_changed':count,'maximum_displacement_m':max_delta,'crest_vertices_changed':len(crest.data.vertices),
  'fault_facets':facets,'talus_wedges':clusters,'blender_mcp_executed':True,'source_geometry_preserved_separately':True})
 assert count>1000 and max_delta>.15
bpy.app.driver_namespace['valoria_cliff_author']=author
server=mcp.BlenderMCPServer(host='127.0.0.1',port=0)
server.socket=socket.socket();server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1]
server.socket.listen(1);server.running=True
server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
responses=[]
def client():
 with socket.create_connection(('127.0.0.1',port),timeout=15) as sock:
  sock.settimeout(120);sock.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['valoria_cliff_author']()"}}).encode())
  data=b''
  while True:
   data+=sock.recv(65536)
   try:responses.append(json.loads(data));return
   except json.JSONDecodeError:pass
thread=threading.Thread(target=client,daemon=True);thread.start();deadline=time.monotonic()+130
while thread.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.04)
thread.join(timeout=1);server.stop()
assert responses and responses[0].get('status')=='success',responses
snapshot(OUT/'after-blender.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'cliff_rocks-unity-v1.blend'))
bpy.ops.object.select_all(action='DESELECT')
for obj in bpy.context.scene.objects:
 if obj.type=='MESH':obj.select_set(True)
bpy.context.view_layer.objects.active=bed
bpy.ops.export_scene.gltf(filepath=str(OUT/'cliff_rocks-unity-v1.glb'),export_format='GLB',use_selection=True,export_materials='EXPORT')
change_log.update({'variant_glb_sha256':hashlib.sha256((OUT/'cliff_rocks-unity-v1.glb').read_bytes()).hexdigest(),
 'variant_blend_sha256':hashlib.sha256((OUT/'cliff_rocks-unity-v1.blend').read_bytes()).hexdigest(),
 'visual_gate':'UNREVIEWED', 'unity_import_gate':'UNRUN'})
(OUT/'edit-proof.json').write_text(json.dumps(change_log,indent=2))
print('EXTERNAL_ARTIST_BLENDER_MCP_VARIANT',change_log,flush=True)
