"""Persistent local BlenderMCP session. No artistic operations are preprogrammed."""
import bpy,addon_utils,pathlib,sys,os,json,time,hashlib,importlib.util,socket,threading
out,source=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None)
assert addon is not None,'Existing BlenderMCP addon not found'
p=pathlib.Path(addon.__file__);digest=hashlib.sha256(p.read_bytes()).hexdigest()
assert digest=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5','Unexpected existing addon identity'
bpy.ops.wm.open_mainfile(filepath=str(source))
bpy.ops.wm.save_as_mainfile(filepath=str(out/'working-copy.blend'))
for provider in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):
 key='blendermcp_use_'+provider
 if not hasattr(bpy.types.Scene,key):setattr(bpy.types.Scene,key,bpy.props.BoolProperty(default=False))
 setattr(bpy.context.scene,key,False)
server=addon.BlenderMCPServer(host='127.0.0.1',port=0)
server.socket=socket.socket();server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1];server.socket.listen(2);server.running=True
server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
bpy.app.driver_namespace['interactive_output']=str(out)
def snapshot(label):
 scene=bpy.context.scene
 for view in ('panoramica-vertical','panoramica-horizontal'):
  scene.camera=bpy.data.objects[view];scene.render.resolution_x=390 if view.endswith('vertical') else 1280;scene.render.resolution_y=844 if view.endswith('vertical') else 720;scene.render.resolution_percentage=100
  scene.render.filepath=str(out/(label+'-'+view+'.png'));bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(out/(label+'.blend')))
 data={'label':label,'pid':os.getpid(),'objects':[{'name':o.name,'type':o.type,'location':list(o.location),'dimensions':list(o.dimensions),'vertices':len(o.data.vertices) if o.type=='MESH' else None} for o in scene.objects]}
 (out/(label+'-scene.json')).write_text(json.dumps(data,indent=2))
bpy.app.driver_namespace['interactive_snapshot']=snapshot
(out/'ready.json').write_text(json.dumps({'pid':os.getpid(),'port':port,'addon_sha256':digest,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'blender':bpy.app.version_string,'started':time.time()}))
try:
 deadline=time.monotonic()+3600
 while time.monotonic()<deadline and not (out/'stop.request').exists():
  server._drain_command_queue();time.sleep(.05)
finally:
 bpy.ops.wm.save_as_mainfile(filepath=str(out/'working-copy.blend'));server.stop()
