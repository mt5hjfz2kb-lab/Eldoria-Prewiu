"""Prototype: free local VLM orders a constrained GEOMETRY edit through actual BlenderMCP TCP.
No canonical Unity changes; visual PASS is intentionally NOT inferred from tests.
"""
import bpy,addon_utils,base64,json,os,re,math,socket,threading,time,urllib.request
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
out=Path(os.environ.get("ELDORIA_GEOMETRY_OUTPUT",str(root/'artifacts-local/geometry-agent')))
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(root/'Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb'))
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=12
sc.render.resolution_x=960;sc.render.resolution_y=640;sc.render.resolution_percentage=100
sc.render.image_settings.file_format='PNG'
w=bpy.data.worlds.new('Valoria slate daylight');sc.world=w;w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.25,.31,.40,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8
light=bpy.data.lights.new('Soft key','AREA');lo=bpy.data.objects.new('Soft key',light);sc.collection.objects.link(lo)
lo.location=(-38,-24,66);light.energy=22000;light.size=32
camd=bpy.data.cameras.new('Fixed owner camera');cam=bpy.data.objects.new('Fixed owner camera',camd);sc.collection.objects.link(cam);sc.camera=cam
cam.location=(55,-82,78);cam.rotation_euler=(Vector((0,6,8))-cam.location).to_track_quat('-Z','Y').to_euler();camd.type='ORTHO';camd.ortho_scale=94
before=out/'before.png';after=out/'after.png'
sc.render.filepath=str(before);bpy.ops.render.render(write_still=True)
ref=root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg'
prompt=("You are a visual art reviewer. Image1 is actual game-candidate Blender terrain and fort; image2 is the desired high-detail medieval strategy-game reference. "
        "Choose exactly ONE safe non-zero rocky-cliff sculpt strength from 0.22,0.36,0.52,0.68, depending on whether the cliffs look too angular/smooth. "
        "Reply only as JSON with key rock_relief_strength and brief reason (at most 8 words). No repeated explanations.")
images=[base64.b64encode(x.read_bytes()).decode('ascii') for x in [before,ref]]
payload={'model':'hf.co/Qwen/Qwen3-VL-2B-Instruct-GGUF:Q4_K_M','stream':False,'format':'json',
         'messages':[{'role':'user','content':prompt,'images':images}],'options':{'temperature':0,'num_predict':95}}
req=urllib.request.Request("http://127.0.0.1:11434/api/chat",data=json.dumps(payload).encode('utf8'),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(req,timeout=210) as f: resp=json.loads(f.read().decode('utf8'))
answer=resp.get('message',{}).get('content','')
(out/'model-response.txt').write_text(answer,encoding='utf8')
# Qwen JSON can encode the same safe numeric choice as a JSON string.
try:
    decision=json.loads(answer)
    value=decision['rock_relief_strength']
    if isinstance(value,bool) or not isinstance(value,(str,int,float)):
        raise ValueError('Invalid geometry strength type')
    strength=float(value)
except (ValueError,TypeError,KeyError,json.JSONDecodeError) as exc:
    raise RuntimeError('Local model did not return a valid geometry instruction') from exc
if strength not in (0.22,0.36,0.52,0.68):raise RuntimeError('Non-whitelisted strength')
rocknames=[ob.name for ob in bpy.data.objects if ob.type=='MESH' and 'Dark slate bedrock' in ob.name]
if not rocknames:raise RuntimeError('No target rock geometry in actual city source')
addon_utils.modules_refresh()
addon_utils.enable('blender_mcp',default_set=False,persistent=False)
import blender_mcp
server=blender_mcp.BlenderMCPServer(host='127.0.0.1',port=0)
server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)
server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1];server.socket.listen(2)
server.running=True
server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
# Executed ONLY through MCP TCP from VLM-chosen bounded parameters.
code=("import bpy\n"
      "targets=[o for o in bpy.data.objects if o.type=='MESH' and 'Dark slate bedrock' in o.name]\n"
      "assert targets\n"
      "for ob in targets:\n"
      " tex=bpy.data.textures.new('Eldoria AI-sculpted erosion',type='CLOUDS')\n"
      " tex.noise_scale=2.1\n"
      " mod=ob.modifiers.new('AI organic rock stratum erosion','DISPLACE')\n"
      " mod.texture=tex\n"
      " mod.strength="+repr(strength)+"\n")
reply=[];errors=[]
def call():
 try:
  c=socket.create_connection(('127.0.0.1',port),timeout=5);c.settimeout(15)
  c.sendall(json.dumps({'type':'execute_code','params':{'code':code}}).encode('utf8'))
  buf=b''
  while True:
   chunk=c.recv(4096)
   if not chunk:break
   buf+=chunk
   try:reply.append(json.loads(buf.decode('utf8')));break
   except json.JSONDecodeError:pass
  c.close()
 except Exception as ex:errors.append(str(ex))
worker=threading.Thread(target=call,daemon=True);worker.start();deadline=time.monotonic()+20
while worker.is_alive() and time.monotonic()<deadline:
 server._drain_command_queue();time.sleep(.06)
worker.join(timeout=.5)
if errors or not reply or reply[0].get('status')!='success':raise RuntimeError('Real MCP TCP editing failed '+str((errors,reply)))
mods=sum(any(m.name=='AI organic rock stratum erosion' for m in o.modifiers) for o in bpy.data.objects if o.type=='MESH')
assert mods==len(rocknames),(mods,rocknames)
server.stop()
sc.render.filepath=str(after);bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'agent-geometry.blend'))
def sample(p):
 im=bpy.data.images.load(str(p),check_existing=False);vals=list(im.pixels[::503]);bpy.data.images.remove(im);return vals
a=sample(before);b=sample(after)
delta=sum(abs(x-y) for x,y in zip(a,b))/max(1,len(a))
if delta<.001:raise RuntimeError('Agent geometry edit did not materially alter full render')
report={'mcp_tcp_agent_execution':True,'local_free_vision':True,'strength':strength,'source_rock_meshes':rocknames,
        'terrain_modifiers_added':mods,'mean_pixel_delta':round(delta,6),'visual_pass':False,'unity_tested':False,
        'note':'Geometric technical proof; visible before/after requires subjective inspection against reference.'}
(out/'report.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_AI_MCP_GEOMETRY_EDIT_PASS',json.dumps(report))
