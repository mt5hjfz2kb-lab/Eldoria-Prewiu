"""Isolated cliff A/B. Baseline uses existing family studio; B starts as independent AI-directed topology."""
import bpy, json, os, math, hashlib, socket, threading, time, importlib.util, sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/"art-source/valoria/experiments/cliff-ai-authorship-ab"
OUT.mkdir(parents=True,exist_ok=True)
MODE=os.environ["ELDORIA_CLIFF_AB_MODE"]
def scene_reset():
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
def setup():
    s=bpy.context.scene;s.render.engine="CYCLES";s.cycles.samples=24
    s.render.resolution_x=960;s.render.resolution_y=640;s.render.resolution_percentage=100
    s.render.film_transparent=True
    w=bpy.data.worlds.new("Matched studio world");w.use_nodes=True
    w.node_tree.nodes["Background"].inputs["Color"].default_value=(.35,.38,.43,1)
    light=bpy.data.lights.new("Matched area key","AREA");o=bpy.data.objects.new("Matched area key",light);s.collection.objects.link(o)
    o.location=(-7,-10,15);light.energy=2700;light.size=8
    camera=bpy.data.cameras.new("Matched orthographic");o=bpy.data.objects.new("Matched orthographic",camera);s.collection.objects.link(o)
    camera.type="ORTHO";s.camera=o
def render_pair(label):
    s=bpy.context.scene
    for name,loc,target,scale in [("hero",(13,-19,13),(0,0,2),18),("strategic",(16,-23,18),(0,0,2.5),24)]:
        cam=s.camera;cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat("-Z","Y").to_euler()
        cam.data.ortho_scale=scale
        s.render.filepath=str(OUT/(label+"-"+name+".png"))
        bpy.ops.render.render(write_still=True)
def export(label):
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(label+".blend")))
    meshes=[x for x in bpy.context.scene.objects if x.type=="MESH"]
    assert meshes
    bpy.ops.object.select_all(action="DESELECT")
    for x in meshes:x.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0]
    bpy.ops.export_scene.gltf(filepath=str(OUT/(label+".glb")),export_format="GLB",use_selection=True,export_materials="EXPORT")
    return {"meshes":len(meshes),"triangles":sum(sum(len(p.vertices)-2 for p in x.data.polygons) for x in meshes)}
def baseline():
    # Exact historical authoring function and PBR donor, executed ONLY for the cliff.
    f=ROOT/"tools/art-rd/valoria_first_family_v1.py"
    original=f.read_text().split("kinds={")[0]
    ns={"__file__":str(f),"__name__":"eldoria_baseline_module"}
    exec(compile(original,str(f),"exec"),ns)
    ns["reset"]();ns["rocks"]();ns["setup_camera"]()
    render_pair("A");details=export("A")
    (OUT/"A-provenance.json").write_text(json.dumps({"source":str(f.relative_to(ROOT)),"authoring":"existing generated cliff and CC0 donor","asset":"cliff_rocks","statistics":details},indent=2))
def mat(name,color,roughness):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*color,1);bs.inputs["Roughness"].default_value=roughness
    return m
def b_make(plan):
    # Whole-shape geometry authored from Qwen-decided 12 profile curves, not A vertices.
    scene_reset();setup()
    profiles=plan["profiles"];n=len(profiles);assert n==12
    layers=plan["levels"]; assert len(layers)==7
    for p in profiles:
        assert len(p)==4 and all(isinstance(v,(int,float)) for v in p)
    # Each profile specifies top XY edge and full-face relief (slope + flared talus).
    vertices=[]
    for height_idx,u in enumerate([0,.12,.27,.43,.60,.79,1]):
        for k,(x,y,slope,foot) in enumerate(profiles):
            a=math.atan2(y,x)
            # gradual physical cliff face and embedded flared scree skirt
            radius=1-slope*math.sin(math.pi*u)+foot*u*u
            z=layers[height_idx]+.12*math.sin(k*1.87+u*3.7)
            vertices.append((x*radius,y*radius,z))
    # Geological planes follow designed uneven ring of crown with smaller internal plateau
    for depth in range(1,6):
        for x,y,slope,foot in profiles:
            vertices.append((x*(1-depth/6),y*(1-depth/6),.12*math.sin(x*.6+y*.3)*(1-depth/6)))
    vertices.append((0,0,0))
    faces=[]
    for j in range(6):
        for k in range(n):
            faces.append((j*n+k,j*n+(k+1)%n,(j+1)*n+(k+1)%n,(j+1)*n+k))
    for j in range(5):
        r0=0 if j==0 else 7*n+(j-1)*n
        r1=7*n+j*n
        for k in range(n):faces.append((r0+k,r0+(k+1)%n,r1+(k+1)%n,r1+k))
    for k in range(n):faces.append((11*n+k,11*n+(k+1)%n,len(vertices)-1))
    mesh=bpy.data.meshes.new("AI designed geological cliff continuous topology")
    mesh.from_pydata(vertices,[],faces);mesh.update()
    ob=bpy.data.objects.new("B IA-authored independent stratified cliff",mesh);bpy.context.collection.objects.link(ob)
    tones=[(.37,.34,.31),(.46,.42,.35),(.31,.33,.34),(.51,.47,.39)]
    for i,c in enumerate(tones):mesh.materials.append(mat("Layered rock "+str(i),c,.86))
    mesh.uv_layers.new(name="UVMap")
    for poly in mesh.polygons:
        j=poly.index//n
        poly.material_index=(j//2+poly.index%7//3)%4
        for li in poly.loop_indices:
            v=mesh.vertices[mesh.loops[li].vertex_index].co
            mesh.uv_layers.active.data[li].uv=(v.x*.22+v.z*.04,v.y*.22+v.z*.07)
    return ob
def perform_mcp(plan,ob):
    # AI-directed real BlenderMCP geometry operation with pinned, verified addon.
    addon=Path("/tmp/eldoria-blender-mcp.py")
    assert hashlib.sha256(addon.read_bytes()).hexdigest()=="eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5"
    spec=importlib.util.spec_from_file_location("eldoria_mcp_ab",addon);module=importlib.util.module_from_spec(spec)
    sys.modules["eldoria_mcp_ab"]=module;spec.loader.exec_module(module)
    for name in ("polyhaven","hyper3d","sketchfab","polypizza","hunyuan3d","tripo"):
        setattr(bpy.types.Scene,"blendermcp_use_"+name,bpy.props.BoolProperty(default=False))
    action=plan["intervention"]
    assert action in ("cut_strata","carve_gullies","fracture_overhang")
    def sculpt():
        changed=0
        for v in ob.data.vertices[:84]:
            x,y,z=v.co
            a=math.atan2(y,x)
            if action=="cut_strata":dr=.16*math.sin(z*3.7+plan["fracture_phase"])
            elif action=="carve_gullies":dr=-.31*max(0,math.sin(a*7+plan["fracture_phase"]))**8
            else:dr=.22*math.cos(a*5+z*.75+plan["fracture_phase"])
            r=math.hypot(x,y)
            if r>1e-4:
                v.co.x+=dr*x/r;v.co.y+=dr*y/r;changed+=1
        ob.data.update()
        bpy.app.driver_namespace["eldoria_ab_changed"]=changed
    bpy.app.driver_namespace["eldoria_ab_sculpt"]=sculpt
    server=module.BlenderMCPServer(host="127.0.0.1",port=0)
    server.socket=socket.socket();server.socket.bind(("127.0.0.1",0))
    port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True
    server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start()
    results=[]
    def remote():
        try:
            with socket.create_connection(("127.0.0.1",port),timeout=15) as sock:
                sock.settimeout(50)
                sock.sendall(json.dumps({"type":"execute_code","params":{"code":"import bpy\nbpy.app.driver_namespace['eldoria_ab_sculpt']()"}}).encode())
                data=b""
                while True:
                    data+=sock.recv(65536)
                    try:results.append(json.loads(data));break
                    except ValueError:pass
        except Exception as exc:results.append({"error":str(exc)})
    t=threading.Thread(target=remote,daemon=True);t.start()
    stop=time.monotonic()+70
    while t.is_alive() and time.monotonic()<stop:
        server._drain_command_queue();time.sleep(.05)
    t.join(timeout=1);server.stop()
    assert results and results[0].get("status")=="success",results
    return {"blender_mcp_result":results[0].get("status"),"vertices_modified":bpy.app.driver_namespace.get("eldoria_ab_changed"),"operation":action}
if MODE=="A":
    baseline()
elif MODE=="B":
    plan=json.loads((OUT/"B-ai-art-direction.json").read_text())["plan"]
    ob=b_make(plan)
    render_pair("B-before-correction")
    evidence=perform_mcp(plan,ob)
    render_pair("B")
    evidence["statistics"]=export("B")
    evidence["agent_decision"]=plan
    (OUT/"B-mcp-evidence.json").write_text(json.dumps(evidence,indent=2))
else:raise ValueError(MODE)
