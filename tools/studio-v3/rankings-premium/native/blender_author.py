"""Real Blender 5.2 authoring via verified local MCP add-on, fixed typed actions."""
import bpy,math,sys,json,pathlib,hashlib,importlib.util,socket,threading,time
from mathutils import Vector
P=pathlib.Path;args=sys.argv[sys.argv.index("--")+1:];source,out,addon=map(P,args);out.mkdir(parents=True,exist_ok=False)
design=json.loads(source.read_text());pin="eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5"
assert hashlib.sha256(addon.read_bytes()).hexdigest()==pin
def num(x,a,b):assert type(x) in (int,float) and a<=x<=b;return float(x)
points=int(num(design["points"],8,16));bev=num(design["bevel"],.02,.12)
def rgb(k):assert len(design[k])==3;return tuple(num(x,0,1) for x in design[k])
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
def material(name,c,metal,rough):
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*c,1);n=m.node_tree.nodes;links=m.node_tree.links;bs=n.get("Principled BSDF");bs.inputs["Base Color"].default_value=(*c,1);bs.inputs["Metallic"].default_value=metal;bs.inputs["Roughness"].default_value=rough
 tex=n.new("ShaderNodeTexNoise");tex.inputs["Scale"].default_value=56;tex.inputs["Detail"].default_value=3;bump=n.new("ShaderNodeBump");bump.inputs["Strength"].default_value=.19;bump.inputs["Distance"].default_value=.028;links.new(tex.outputs["Fac"],bump.inputs["Height"]);links.new(bump.outputs["Normal"],bs.inputs["Normal"])
 return m
gold=material("Chased Valoria bronze-gold",rgb("metal"),.88,.29);dark=material("Oxidised engraved recess",(.13,.067,.029),.75,.5)
enamel=material("Obsidian violet enamel",rgb("enamel"),.25,.37);gem=material("Faceted amethyst",rgb("gem"),.35,.18)
def mesh(n,verts,faces,mat,b=.03):
 me=bpy.data.meshes.new(n);me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new(n,me);scene.collection.objects.link(ob);ob.data.materials.append(mat)
 if b:
  mod=ob.modifiers.new("Rounded forged edges","BEVEL");mod.width=b;mod.segments=3
  mod=ob.modifiers.new("Weighted normals","WEIGHTED_NORMAL")
 return ob
def cyl(n,r,dep,z,mat,xy=(0,0),vertices=96):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=dep,location=(xy[0],xy[1],z));ob=bpy.context.object;ob.name=n;ob.data.materials.append(mat);m=ob.modifiers.new("Chased rim bevel","BEVEL");m.width=bev*.6;m.segments=3
 return ob
def ring(n,r,minor,z,mat):
 bpy.ops.mesh.primitive_torus_add(major_segments=96,minor_segments=16,major_radius=r,minor_radius=minor,location=(0,0,z));bpy.context.object.name=n;bpy.context.object.data.materials.append(mat)
def plate(n,coords,z,depth,mat):
 v=[(x,y,z+k*depth) for k in range(2) for x,y in coords];N=len(coords);f=[tuple(reversed(range(N))),tuple(range(N,2*N))]+[(i,(i+1)%N,(i+1)%N+N,i+N) for i in range(N)];return mesh(n,v,f,mat,bev*.5)
def rosette():
 for i in range(points):
  a=2*math.pi*i/points;r=1.19; x,y=r*math.cos(a),r*math.sin(a)
  cyl("Antique rivet "+str(i),.045,.05,.24,gold,(x,y),20)
  # leaf-shaped relief inlaid around ring
  ca,sa=math.cos(a),math.sin(a)
  coords=[(ca*u-sa*v,sa*u+ca*v) for u,v in [(1.03,0),(1.15,-.055),(1.29,0),(1.15,.055)]]
  plate("Engraved leaf "+str(i),coords,.19,.06,gold)
def build():
 cyl("Bronze medallion back",1.38,.17,.03,dark);cyl("Hammered forged bezel",1.32,.18,.15,gold);cyl("Deep enamel face",1.08,.12,.25,enamel)
 ring("Outer polished lip",1.33,.065,.19,gold);ring("Inner golden fillet",1.07,.035,.32,gold);ring("Double groove",1.15,.016,.25,dark);rosette()
 # A pierced geometric crown-like Valoria shield, independently authored, not a new gameplay trophy.
 plate("Heraldic relief shield",[(0,.8),(.65,.43),(.54,-.38),(0,-.82),(-.54,-.38),(-.65,.43)],.32,.12,gold)
 plate("Amethyst inset",[(0,.55),(.41,.27),(.34,-.27),(0,-.55),(-.34,-.27),(-.41,.27)],.45,.04,enamel)
 for k in range(4):
  a=math.pi*k/2;ca,sa=math.cos(a),math.sin(a)
  plate("Radial crest leaf "+str(k),[(ca*u-sa*v,sa*u+ca*v) for u,v in [(.16,0),(.44,.09),(.55,0),(.44,-.09)]],.48,.06,gold)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=.24,location=(0,0,.59));o=bpy.context.object;o.name="Hand-cut amethyst";o.scale=(.72,.94,.47);o.data.materials.append(gem)
def lighting():
 for n,pos,power,color,size in [("Warm gallery key",(-3,-4,7),720,(1,.78,.47),4),("Cool fill",(4,2,5),520,(.56,.67,1),3),("Top highlight",(0,4,5),450,(1,.9,.72),2)]:
  d=bpy.data.lights.new(n,"AREA");d.energy=power;d.color=color;d.shape="DISK";d.size=size;o=bpy.data.objects.new(n,d);scene.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,.1))-o.location).to_track_quat("-Z","Y").to_euler()
 cam=bpy.data.cameras.new("Medallion orthographic");cam.type="ORTHO";cam.ortho_scale=3.2;o=bpy.data.objects.new("Medallion camera",cam);scene.collection.objects.link(o);o.location=(0,-1.4,7.5);o.rotation_euler=(Vector((0,0,.22))-o.location).to_track_quat("-Z","Y").to_euler();scene.camera=o
 scene.world=bpy.data.worlds.new("Gallery ambient");scene.world.use_nodes=True;scene.world.node_tree.nodes["Background"].inputs[0].default_value=(.20,.16,.25,1);scene.world.node_tree.nodes["Background"].inputs[1].default_value=.45
 scene.render.engine="CYCLES";scene.cycles.device="CPU";scene.cycles.samples=24;scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100;scene.render.film_transparent=True;scene.render.image_settings.file_format="PNG";scene.render.image_settings.color_mode="RGBA"
spec=importlib.util.spec_from_file_location("eldoria_pr198_verified_addon",str(addon.resolve()));module=importlib.util.module_from_spec(spec);sys.modules[spec.name]=module;spec.loader.exec_module(module)
assert hasattr(module,"BlenderMCPServer")
for provider in ("polyhaven","hyper3d","sketchfab","polypizza","hunyuan3d","tripo"):
 k="blendermcp_use_"+provider
 if not hasattr(bpy.types.Scene,k):setattr(bpy.types.Scene,k,bpy.props.BoolProperty(default=False))
actions={"build":build,"lighting":lighting};executed=[]
def perform(identifier):
 assert identifier in actions and identifier not in executed
 actions[identifier]();executed.append(identifier)
bpy.app.driver_namespace["eldoria_pr198_known_action"]=perform
server=module.BlenderMCPServer(host="127.0.0.1",port=0);server.socket=socket.socket();server.socket.bind(("127.0.0.1",0));port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True;server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
receipts=[]
try:
 for identifier in actions:
  reply=[];err=[]
  def client():
   try:
    with socket.create_connection(("127.0.0.1",port),timeout=10) as sock:
     sock.settimeout(90);cmd='import bpy\nbpy.app.driver_namespace["eldoria_pr198_known_action"]('+repr(identifier)+')';sock.sendall(json.dumps({"type":"execute_code","params":{"code":cmd}}).encode());data=b""
     while len(data)<2000000:
      chunk=sock.recv(65536)
      if not chunk:raise RuntimeError("MCP socket closed")
      data+=chunk
      try:reply.append(json.loads(data));return
      except json.JSONDecodeError:pass
   except Exception as e:err.append(str(e))
  th=threading.Thread(target=client,daemon=True);th.start();deadline=time.monotonic()+95
  while th.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.04)
  th.join(1);assert not err and reply and reply[0].get("status")=="success" and identifier in executed,(identifier,err,reply);receipts.append({"id":identifier,"status":"success"})
finally:
 server.stop();bpy.app.driver_namespace.pop("eldoria_pr198_known_action",None)
assert executed==["build","lighting"]
scene.render.filepath=str((out/"medallion.png").resolve())
bpy.ops.wm.save_as_mainfile(filepath=str((out/"medallion.blend").resolve()));bpy.ops.render.render(write_still=True)
proof={"actual_blender":bpy.app.version_string,"transport":"installed BlenderMCP loopback known action","addon_sha256":pin,"actions":receipts,"mesh_objects":len([o for o in scene.objects if o.type=="MESH"]),"png_sha256":hashlib.sha256((out/"medallion.png").read_bytes()).hexdigest(),"blend_sha256":hashlib.sha256((out/"medallion.blend").read_bytes()).hexdigest(),"game_modified":False,"art_approved":False}
(out/"blender-proof.json").write_text(json.dumps(proof,indent=2));print("PR198_BLENDER_MCP_PROOF",json.dumps(proof),flush=True)
