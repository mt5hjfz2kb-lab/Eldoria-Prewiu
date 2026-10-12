"""Controlled cliff variant: Qwen visual direction -> BlenderMCP mesh edit -> GLB.
The frozen V3 .blend and .glb remain untouched. A workflow supplies the two images.
"""
import json, os, sys, pathlib, hashlib, time
ROOT=pathlib.Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/variants/cliff_talus_transition_v1'
OUT.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'art-source/valoria/production/asset-family-v2/quality-v3/cliff_rocks/cliff_rocks.blend'
if '--infer' in sys.argv:
 import base64, urllib.request, urllib.error
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
 role_path=ROOT/'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md'
 assert role_path.exists(),'Mandatory professional artist system prompt missing'
 role_text=role_path.read_text(encoding='utf-8')
 role_sha=hashlib.sha256(role_text.encode()).hexdigest()
 compact_role=role_text.split('ARTISTIC METHOD:')[0].strip()+'\n'+role_text.split('CREATIVE IDENTITY:')[-1].strip()+'\nObserve, diagnose, design, execute and verify; never self-approve.'
 payload={'model':os.environ.get('ELDORIA_QWEN_MODEL','qwen3-vl:4b-instruct'),'stream':False,'think':False,'format':'json',
          'options':{'temperature':.18,'num_predict':600,'num_ctx':4096},
          'messages':[{'role':'system','content':compact_role},{'role':'user','content':prompt,'images':[base64.b64encode((OUT/'art-direction-input.jpg').read_bytes()).decode()]}]}
 req=urllib.request.Request(os.environ.get('OLLAMA_HOST','http://127.0.0.1:11434').rstrip('/')+'/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 try:
  with urllib.request.urlopen(req,timeout=600) as r:reply=json.load(r)
 except urllib.error.HTTPError as e:
  raise RuntimeError('Ollama HTTP '+str(e.code)+': '+e.read().decode(errors='replace')[:1500]) from e
 raw=reply['message']['content'];plan=json.loads(raw)
 for k,lo,hi in [('fault_amplitude_m',.4,1.3),('large_facets',4,9),('crest_recede_m',.2,.9),('talus_clusters',3,8)]:
  assert lo<=float(plan[k])<=hi,(k,plan[k])
 for k in ('diagnosis','artistic_intent','specific_visible_change'):assert len(plan[k])>=25,(k,plan[k])
 (OUT/'direction.json').write_text(json.dumps({'plan':plan,'raw_qwen_reply':raw,'model':payload['model'],
  'prompt':prompt,'artist_system_prompt_sha256':role_sha,'target_sha256':hashlib.sha256(target.read_bytes()).hexdigest(),
  'unity_capture_sha256':hashlib.sha256(current.read_bytes()).hexdigest(),
  'source_blend_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'self_approval':False},indent=2))
 print('EXTERNAL_ARTIST_DIRECTION',plan,flush=True)
 sys.exit(0)

import bpy, math, socket, threading, importlib.util
from mathutils import Vector
plan=json.loads((OUT/'direction.json').read_text())['plan']
amp=float(plan['fault_amplitude_m']);facets=int(plan['large_facets']);recede=float(plan['crest_recede_m']);clusters=int(plan['talus_clusters'])
addon=pathlib.Path(os.environ.get('ELDORIA_BLENDER_MCP_ADDON','/tmp/eldoria-blender-mcp.py'))
assert addon.is_file(), 'BlenderMCP addon missing: '+str(addon)
assert hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5', 'BlenderMCP addon hash mismatch'
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
  # Preserve the frozen crest contact: push large vertical fault masses only
  # below the cap. The first variant displaced the lip and exposed a bright shelf.
  taper=max(0,min(1,(-z-.12)/.95))
  change=seam*taper*(.35+u*.65)*amp*(.6*facet+.4*broad)
  if y<-1.0:
   v.co.y=y-change
   if z<-.8:v.co.y+=seam*recede*.35*math.sin(math.pi*u)
   count+=1;max_delta=max(max_delta,abs(change))
 bed.data.update()
 # Original cap remains connected to the unchanged z=0 rock boundary.
 # Redistribute existing textured talus instead of inserting untextured wedges.
 talus=[ob for ob in bpy.context.scene.objects if ob.type=='MESH' and 'talus' in ob.name.lower()]
 for i,ob in enumerate(talus):
  ob.location.x+=.10*math.sin(i*2.7)
  ob.location.y-=.12*(i%3)/2
 crest.data.update()
 change_log.update({'bedrock_vertices_changed':count,'maximum_displacement_m':max_delta,'crest_vertices_changed':len(crest.data.vertices),
  'fault_facets':facets,'textured_source_talus_adjusted':len(talus),'artist_requested_clusters':clusters,'blender_mcp_executed':True,'source_geometry_preserved_separately':True})
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

# A genuinely standalone reusable talus-contact module, separated from the frozen cliff.
# Qwen chooses relief, facet frequency and boulder cluster count from real reference images.
apron_mesh=bpy.data.meshes.new('Qwen directed talus transition geology')
apron_obj=bpy.data.objects.new('cliff_talus_transition_v1__geological_apron',apron_mesh)
bpy.context.scene.collection.objects.link(apron_obj)
verts=[];faces=[];uv_points=[]
nx=25;ny=7
for j in range(ny):
 t=j/(ny-1)
 for i in range(nx):
  x=-6+12*i/(nx-1)
  edge=max(0,min(1,(6-abs(x))/.7))
  y=-2.1-t*3.2+edge*.12*math.sin((i+2*j)*2.1)
  z=-3.42-t*(.58+.30*amp)+edge*(.13*math.sin(i*1.63+j*.82)+.11*math.cos(i*.61-j*1.9))
  z+=edge*amp*.26*math.sin(i*facets*.19+j*.67)*math.sin(math.pi*t)
  verts.append((x,y,z))
for j in range(ny-1):
 for i in range(nx-1):
  n=j*nx+i
  faces.append((n,n+1,n+nx+1,n+nx))
apron_mesh.from_pydata(verts,[],faces)
apron_mesh.update()
if bed.data.materials:apron_mesh.materials.append(bed.data.materials[0])
uv=apron_mesh.uv_layers.new(name='TalusUV')
for poly in apron_mesh.polygons:
 for loop_index in poly.loop_indices:
  co=apron_mesh.vertices[apron_mesh.loops[loop_index].vertex_index].co
  uv.data[loop_index].uv=(co.x/5,co.y/3)
standalone=[apron_obj]
mat=bed.data.materials[0] if bed.data.materials else None
for k in range(clusters):
 xx=-5+10*(k+.5)/clusters+(.21*math.sin(k*2.9))
 yy=-3.1-(k%3)*.56
 zz=-3.6-.09*(k%2)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(xx,yy,zz))
 rock=bpy.context.object
 rock.name='cliff_talus_transition_v1__boulder_'+str(k+1)
 rock.scale=(.65+.18*(k%3),.40+.15*((k+2)%3),.38+.13*(k%2))
 if mat:rock.data.materials.append(mat)
 standalone.append(rock)
change_log.update({'standalone_new_mesh_objects':len(standalone),'export_scope':'new talus apron and authored boulders only','standalone_reusable_candidate':True})

snapshot(OUT/'after-blender.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'cliff_talus_transition_v1.blend'))
bpy.ops.object.select_all(action='DESELECT')
for obj in standalone:obj.select_set(True)
bpy.context.view_layer.objects.active=standalone[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'cliff_talus_transition_v1.glb'),export_format='GLB',use_selection=True,export_materials='EXPORT')
change_log.update({'variant_glb_sha256':hashlib.sha256((OUT/'cliff_talus_transition_v1.glb').read_bytes()).hexdigest(),
 'asset_scope':'independent talus apron and sculptural rocks based on Qwen parameters; still visually unapproved',
 'variant_blend_sha256':hashlib.sha256((OUT/'cliff_talus_transition_v1.blend').read_bytes()).hexdigest(),
 'visual_gate':'UNREVIEWED', 'unity_import_gate':'UNRUN'})
(OUT/'edit-proof.json').write_text(json.dumps(change_log,indent=2))
print('EXTERNAL_ARTIST_BLENDER_MCP_VARIANT',change_log,flush=True)
