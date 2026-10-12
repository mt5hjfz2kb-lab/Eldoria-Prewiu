"""Run ALLOWLISTED Qwen-selected 3D authoring operations, never arbitrary LLM code.
blender -b --python-exit-code 1 --python execute_artist_plan.py -- --source frozen.blend --plan artist-plan.json --out variantdir --transport direct
Preferred BlenderMCP transport: --transport blendermcp --mcp-addon verified-addon.py --mcp-sha256 <pinned-sha>
"""
import argparse,hashlib,importlib.util,json,math,pathlib,socket,sys,threading,time
import bpy
from mathutils import Vector
p=argparse.ArgumentParser()
p.add_argument('--source',required=True,type=pathlib.Path)
p.add_argument('--plan',required=True,type=pathlib.Path)
p.add_argument('--out',required=True,type=pathlib.Path)
p.add_argument('--transport',choices=['direct','blendermcp'],default='direct')
p.add_argument('--mcp-addon',type=pathlib.Path)
p.add_argument('--mcp-sha256')
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
if not a.source.is_file() or a.source.suffix.lower()!='.blend':raise ValueError('Original .blend required')
if not a.plan.is_file():raise ValueError('Real Qwen plan missing')
if a.out.resolve()==a.source.parent.resolve():raise ValueError('Never overwrite source directory')
operations=json.loads(a.plan.read_text('utf8'))
ALLOWED={'sculpt_region','reprofile_crest','construct_mesh','assign_material','place_existing_asset'}
if not isinstance(operations,list) or not 1<=len(operations)<=8:raise ValueError('Invalid plan length')
if len(set(x.get('id') for x in operations))!=len(operations):raise ValueError('Duplicate ids')
for op in operations:
 if op.get('type') not in ALLOWED or not op.get('target') or len(op.get('rationale',''))<15 or not isinstance(op.get('parameters'),dict):raise ValueError('Untrusted or imprecise operation')
bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
def sha(b):return hashlib.sha256(b).hexdigest()
def state():
 return sha(json.dumps({o.name:[tuple(round(v,6) for v in vert.co) for vert in o.data.vertices] for o in bpy.data.objects if o.type=='MESH'},sort_keys=True).encode())
source_mesh=state()
def vec(v,n=3):
 if not isinstance(v,list) or len(v)!=n:raise ValueError('Invalid spatial vector')
 v=[float(x) for x in v]
 if any(not math.isfinite(x) or abs(x)>10000 for x in v):raise ValueError('Unsafe coordinate')
 return v
completed=[]
def act(identifier):
 op=next(x for x in operations if x['id']==identifier)
 kind=op['type'];t=op['target'];pr=op['parameters']
 obj=bpy.data.objects.get(t)
 n=0;objects=1
 if kind=='construct_mesh':
  if obj:raise ValueError('New target mesh must not exist')
  verts=pr.get('vertices');faces=pr.get('faces')
  if not isinstance(verts,list) or not 4<=len(verts)<=256 or not isinstance(faces,list) or not 3<=len(faces)<=512:raise ValueError('Artist mesh budget')
  verts=[vec(v) for v in verts]
  for f in faces:
   if not isinstance(f,list) or not 3<=len(f)<=12 or any(type(i)!=int or i<0 or i>=len(verts) for i in f):raise ValueError('Invalid faces')
  mesh=bpy.data.meshes.new(t+'_authored');mesh.from_pydata(verts,[],faces);mesh.update()
  obj=bpy.data.objects.new(t,mesh);bpy.context.scene.collection.objects.link(obj);n=len(verts)
  color=vec(pr.get('material_color',[.4,.38,.34]))
  mat=bpy.data.materials.new(t+'_artist');mat.diffuse_color=(*color,1);obj.data.materials.append(mat)
 else:
  if not obj:raise ValueError('Source object missing: '+t)
  if kind in ['sculpt_region','reprofile_crest']:
   if obj.type!='MESH':raise ValueError('Can only edit mesh')
   if kind=='sculpt_region':
    center=Vector(vec(pr.get('center')));delta=Vector(vec(pr.get('delta')))
    radius=float(pr.get('radius',0))
    if not .01<=radius<=100 or delta.length<.005:raise ValueError('Displacement out of bounds')
    for vertex in obj.data.vertices:
     d=(vertex.co-center).length
     if d<radius:vertex.co+=delta*(1-(d/radius)**2)**2;n+=1
   else:
    axis=pr.get('axis');ha=pr.get('height_axis')
    if axis not in ['x','y'] or ha not in ['y','z'] or axis==ha:raise ValueError('Invalid profile axes')
    points=pr.get('control_points')
    if not isinstance(points,list) or not 3<=len(points)<=24:raise ValueError('3..24 artist control points required')
    points=sorted((vec(x,2) for x in points))
    if len(set(pt[0] for pt in points))!=len(points):raise ValueError('Overlapping profile coordinates')
    lower=float(pr.get('band_min',-1000));upper=float(pr.get('band_max',1000))
    if lower>=upper:raise ValueError('Invalid sculpt band')
    for v in obj.data.vertices:
     t=float(getattr(v.co,axis));h=float(getattr(v.co,ha))
     if t<points[0][0] or t>points[-1][0] or h<lower or h>upper:continue
     i=next((j for j in range(len(points)-1) if points[j][0]<=t<=points[j+1][0]),len(points)-2)
     lo,hi=points[i],points[i+1];u=(t-lo[0])/(hi[0]-lo[0])
     desired=lo[1]*(1-u)+hi[1]*u
     setattr(v.co,ha,desired);n+=1
   if not n:raise ValueError('Zero vertices sculpted')
   obj.data.update()
  elif kind=='assign_material':
   rgb=vec(pr.get('rgb'));rough=float(pr.get('roughness',.85))
   if not 0<=rough<=1:raise ValueError('Roughness 0..1')
   material=obj.active_material.copy() if obj.active_material else bpy.data.materials.new(t+'_artist_mat')
   material.use_nodes=True;bsdf=material.node_tree.nodes.get('Principled BSDF')
   bsdf.inputs['Base Color'].default_value=(*rgb,1);bsdf.inputs['Roughness'].default_value=rough
   if obj.type=='MESH':
    if obj.active_material:obj.active_material=material
    else:obj.data.materials.append(material)
  elif kind=='place_existing_asset':
   source=bpy.data.objects.get(pr.get('source_object',''))
   if not source:raise ValueError('Donor object missing')
   ob=source.copy()
   if source.data:ob.data=source.data.copy()
   ob.name=t+'_artist';ob.location=vec(pr.get('location'));ob.rotation_euler=vec(pr.get('rotation',[0,0,0]));ob.scale=vec(pr.get('scale',[1,1,1]))
   bpy.context.scene.collection.objects.link(ob);n=len(ob.data.vertices) if ob.type=='MESH' else 0
 completed.append({'id':identifier,'type':kind,'target':t,'changed_vertices':n,'changed_objects':objects,'applied':True})
if a.transport=='direct':
 for op in operations:act(op['id'])
else:
 if not a.mcp_addon or not a.mcp_addon.is_file() or not a.mcp_sha256 or sha(a.mcp_addon.read_bytes())!=a.mcp_sha256:raise ValueError('Pinned addon SHA256 required for genuine BlenderMCP')
 spec=importlib.util.spec_from_file_location('eldoria_verified_mcp',str(a.mcp_addon))
 mcp=importlib.util.module_from_spec(spec);sys.modules[spec.name]=mcp;spec.loader.exec_module(mcp)
 for key in ['polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo']:
  setattr(bpy.types.Scene,'blendermcp_use_'+key,bpy.props.BoolProperty(default=False))
 bpy.app.driver_namespace['eldoria_artist_action']=act
 server=mcp.BlenderMCPServer(host='127.0.0.1',port=0)
 server.socket=socket.socket();server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1]
 server.socket.listen(1);server.running=True
 server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
 try:
  for op in operations:
   response=[];error=[]
   def client():
    try:
     with socket.create_connection(('127.0.0.1',port),timeout=15) as sock:
      sock.settimeout(120)
      command='import bpy\nbpy.app.driver_namespace["eldoria_artist_action"]('+repr(op['id'])+')'
      sock.sendall(json.dumps({'type':'execute_code','params':{'code':command}}).encode())
      data=b''
      while len(data)<2_000_000:
       packet=sock.recv(65536)
       if not packet:raise RuntimeError('MCP socket closed')
       data+=packet
       try:response.append(json.loads(data));return
       except json.JSONDecodeError:pass
    except Exception as err:error.append(str(err))
   th=threading.Thread(target=client,daemon=True);th.start();deadline=time.monotonic()+130
   while th.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.05)
   th.join(timeout=1)
   if error or not response or response[0].get('status')!='success':raise RuntimeError('Actual BlenderMCP command did not succeed: '+str(error or response))
 finally:server.stop()
if len(completed)!=len(operations):raise ValueError('Not all artist decisions executed')
result_mesh=state()
if source_mesh==result_mesh and any(o['type'] in ['sculpt_region','reprofile_crest','construct_mesh'] for o in operations):raise ValueError('Source/result geometry identical')
a.out.mkdir(parents=True,exist_ok=True)
blend=a.out/'artist-output.blend';glb=a.out/'artist-output.glb'
bpy.ops.wm.save_as_mainfile(filepath=str(blend.resolve()))
bpy.ops.export_scene.gltf(filepath=str(glb.resolve()),export_format='GLB',use_selection=False)
proof={'transport':a.transport,'source_sha256':sha(a.source.read_bytes()),'plan_sha256':sha(a.plan.read_bytes()),
 'source_mesh_sha256':source_mesh,'result_mesh_sha256':result_mesh,'result_blend_sha256':sha(blend.read_bytes()),
 'result_glb_sha256':sha(glb.read_bytes()),'executed_actions':completed,'self_approval':False}
(a.out/'artist-execution.json').write_text(json.dumps(proof,indent=2)+'\n','utf8')
print('REAL_ARTIST_BLENDER_EXECUTION',len(completed),'actions','mesh_change',source_mesh!=result_mesh)
