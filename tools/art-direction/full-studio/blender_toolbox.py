"""Eldoria Full Studio: AI-directed, real Blender authoring. No templates for assets.

Run with Blender 4.x/5.x:
 blender -b --python-exit-code 1 --python blender_toolbox.py -- \
   --plan plan.json --out /tmp/eldoria-studio --texture-root /safe/source
The AI chooses each mesh, operation and parameter. This is an EXECUTOR, not a
generator of prescribed castles. No arbitrary AI Python, subprocess or eval.
"""
from __future__ import annotations
import argparse, hashlib, json, math, pathlib, sys, time
from collections import defaultdict
import bpy, bmesh
from mathutils import Vector
from mathutils import noise as bnoise

HERE = pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
from contract import load_plan, MAX_OUTPUT_TRIANGLES, RejectedPlan

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def obj(name, kind="MESH"):
    o=bpy.data.objects.get(name)
    if o is None or (kind is not None and o.type!=kind):
        raise RejectedPlan("Missing object or wrong type: "+str(name))
    return o

def active(o):
    bpy.ops.object.mode_set(mode="OBJECT") if bpy.context.object and bpy.context.object.mode!="OBJECT" else None
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.context.view_layer.objects.active=o

def nvec(value, default):
    return Vector(value if value is not None else default)

def mk_primitive(name,p):
    shape=p["shape"]
    ops={"cube":bpy.ops.mesh.primitive_cube_add,
         "cylinder":bpy.ops.mesh.primitive_cylinder_add,
         "cone":bpy.ops.mesh.primitive_cone_add,
         "plane":bpy.ops.mesh.primitive_plane_add,
         "uv_sphere":bpy.ops.mesh.primitive_uv_sphere_add,
         "ico_sphere":bpy.ops.mesh.primitive_ico_sphere_add,
         "torus":bpy.ops.mesh.primitive_torus_add}
    kwargs={"location":tuple(p.get("location",[0,0,0])),"rotation":tuple(p.get("rotation",[0,0,0]))}
    if shape in ("cylinder","cone"):kwargs["vertices"]=int(p.get("segments",24))
    if shape in ("uv_sphere","ico_sphere"):
        if shape=="uv_sphere":kwargs["segments"]=int(p.get("segments",32));kwargs["ring_count"]=int(p.get("rings",16))
        else:kwargs["subdivisions"]=int(p.get("subdivisions",2))
    ops[shape](**kwargs)
    o=bpy.context.object;o.name=name;o.scale=p.get("scale",[1,1,1])
    return o

def modified_mesh(name,p):
    mesh=bpy.data.meshes.new(name+"_AI_geometry")
    mesh.from_pydata(p["vertices"],[],p["faces"])
    mesh.validate(verbose=False);mesh.update()
    o=bpy.data.objects.new(name,mesh)
    bpy.context.scene.collection.objects.link(o)
    active(o)
    return o

def add_texture(name,p):
    import random
    random.seed(int(p.get("seed",17)))
    res=int(p.get("resolution",512))
    tex=bpy.data.textures.new(name+"_noise",type="CLOUDS") if p["method"]=="NOISE" else None
    if tex:
        tex.noise_scale=float(p["scale"]);tex.noise_depth=3
    img=bpy.data.images.new(name,width=res,height=res,alpha=True)
    a,b=p["color_a"],p["color_b"];px=[]
    for y in range(res):
        for x in range(res):
            mix=max(0.,min(1.,(tex.evaluate(((x+.5)/res,(y+.5)/res,0.)).intensity if tex else ((x//max(1,res//12)+y//max(1,res//12))%2))))
            px.extend(tuple(a[i]*(1-mix)+b[i]*mix for i in range(4)))
    img.pixels.foreach_set(px);img.update()
    img.pack()
    return img

def material(name,p,texture_root):
    o=obj(name)
    color=p["base_color"]
    mat=bpy.data.materials.new("Eldoria_AI_"+name+"_"+str(len(bpy.data.materials)))
    mat.diffuse_color=color;mat.use_nodes=True
    bsdf=mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value=color
    bsdf.inputs["Roughness"].default_value=p["roughness"]
    bsdf.inputs["Metallic"].default_value=p.get("metallic",0)
    if p.get("texture_image"):
        img=bpy.data.images.get(p["texture_image"])
        if img is None:raise RejectedPlan("Missing generated AI texture")
    elif p.get("texture_path"):
        source=(texture_root / p["texture_path"]).resolve()
        if not source.is_relative_to(texture_root.resolve()) or not source.is_file():
            raise RejectedPlan("Texture missing/outside authorized source")
        img=bpy.data.images.load(str(source),check_existing=True)
        img.pack()
    else:img=None
    if img:
        node=mat.node_tree.nodes.new("ShaderNodeTexImage");node.image=img
        mat.node_tree.links.new(node.outputs["Color"],bsdf.inputs["Base Color"])
    o.data.materials.clear();o.data.materials.append(mat)
    return mat

def sculpt(o,p):
    center=Vector(p["center"]);delta=Vector(p["delta"])
    radius=float(p["radius"])
    count=0
    for vert in o.data.vertices:
        d=(vert.co-center).length
        if d>=radius:continue
        t=1-d/radius
        w=t*t*(3-2*t)
        vert.co+=delta*w;count+=1
    o.data.update()
    if not count:raise RejectedPlan("AI chose sculpt center outside real geometry")
    return count

def edit_face_operation(o,kind,p):
    bm=bmesh.new()
    try:
        bm.from_mesh(o.data);bm.verts.ensure_lookup_table()
        bm.faces.ensure_lookup_table();bm.edges.ensure_lookup_table()
        fi=p.get("faces") or list(range(len(bm.faces)))
        if any(type(i) is not int or i<0 or i>=len(bm.faces) for i in fi):
            raise RejectedPlan("AI selected non-existent face index")
        selected=[bm.faces[i] for i in fi]
        if kind=="extrude":
            result=bmesh.ops.extrude_discrete_faces(bm,faces=selected)
            moved=set(v for face in result["faces"] for v in face.verts)
            bmesh.ops.translate(bm,verts=list(moved),vec=Vector(p["offset"]))
        elif kind=="inset":
            bmesh.ops.inset_individual(bm,faces=selected,thickness=float(p["thickness"]),depth=float(p.get("depth",0.)))
        elif kind=="subdivide":
            edges=set(e for face in selected for e in face.edges)
            bmesh.ops.subdivide_edges(bm,edges=list(edges),cuts=int(p.get("cuts",1)),use_grid_fill=True)
        bm.to_mesh(o.data);o.data.update()
    finally:bm.free()

def modifier(o,p):
    kind=p["kind"];opt=p.get("options",{})
    m=o.modifiers.new("Eldoria_AI_"+kind+"_"+str(len(o.modifiers)),kind)
    if kind=="BEVEL":
        m.width=float(opt.get("width",.08));m.segments=int(opt.get("segments",2))
        if hasattr(m,"affect"):m.affect="EDGES"
    elif kind=="SUBSURF":
        m.levels=int(opt.get("levels",1));m.render_levels=int(opt.get("render_levels",1))
    elif kind=="SOLIDIFY":m.thickness=float(opt.get("thickness",.08))
    elif kind=="DECIMATE":m.ratio=float(opt.get("ratio",.7))
    elif kind=="REMESH":
        m.mode="VOXEL" if hasattr(m,"mode") and hasattr(m,"voxel_size") else "SHARP"
        if hasattr(m,"voxel_size"):m.voxel_size=float(opt.get("voxel_size",.2))
    elif kind=="MIRROR":
        m.use_axis=[opt.get("axis","X").upper()==a for a in "XYZ"]
    elif kind=="ARRAY":
        m.count=int(opt.get("count",3));m.relative_offset_displace=tuple(opt.get("offset",[1.0,0.,0.]))
    elif kind=="SIMPLE_DEFORM":
        m.deform_method="TWIST";m.angle=float(opt.get("angle",.1))
    elif kind=="DISPLACE":
        tex=bpy.data.textures.new("Eldoria_AI_displacement",type="CLOUDS")
        tex.noise_scale=float(opt.get("texture_scale",.65))
        m.texture=tex;m.strength=float(opt.get("strength",.15))
    if p.get("apply",False):
        active(o);bpy.ops.object.modifier_apply(modifier=m.name)

def camera_override(spec):
    if not spec:return
    o=bpy.data.objects.get("_EldoriaReviewCamera")
    if not o:
        cam=bpy.data.cameras.new("_EldoriaReviewCamera")
        o=bpy.data.objects.new("_EldoriaReviewCamera",cam);bpy.context.scene.collection.objects.link(o)
    o.location=spec.get("location",[9,-15,11])
    o.rotation_euler=spec.get("rotation",[math.radians(63),0,math.radians(34)])
    o.data.type=spec.get("projection","ORTHO")
    o.data.ortho_scale=float(spec.get("ortho_scale",21))
    bpy.context.scene.camera=o

def render_setup(settings):
    scene=bpy.context.scene
    engine=settings.get("engine","CYCLES")
    try:scene.render.engine=engine
    except TypeError:scene.render.engine="CYCLES"
    scene.render.resolution_x=int(settings.get("width",960))
    scene.render.resolution_y=int(settings.get("height",720))
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.render.film_transparent=bool(settings.get("transparent",False))
    if scene.render.engine=="CYCLES":
        scene.cycles.samples=int(settings.get("samples",16))
        scene.cycles.use_denoising=False

def ensure_review_lighting():
    if any(o.type=="LIGHT" for o in bpy.data.objects):return
    dat=bpy.data.lights.new("_EldoriaReviewSun","SUN")
    dat.energy=2.5
    light=bpy.data.objects.new("_EldoriaReviewSun",dat)
    bpy.context.scene.collection.objects.link(light)
    light.rotation_euler=(math.radians(30),math.radians(-15),math.radians(-30))

def snapshot_meshes():
    r={}
    for o in bpy.data.objects:
        if o.type!="MESH":continue
        r[o.name]=hashlib.sha256(json.dumps({
           "v":[[round(c,6) for c in v.co] for v in o.data.vertices],
           "f":[list(p.vertices) for p in o.data.polygons]},sort_keys=True).encode()).hexdigest()
    return r

def triangles_evaluated():
    deps=bpy.context.evaluated_depsgraph_get()
    total=0
    for o in bpy.data.objects:
        if o.type!="MESH":continue
        eo=o.evaluated_get(deps)
        me=eo.to_mesh()
        try:
            me.calc_loop_triangles();total+=len(me.loop_triangles)
        finally:eo.to_mesh_clear()
    return total

def run_action(a,texroot):
    kind,name,p=a["op"],a.get("target"),a["params"]
    if kind=="primitive":mk_primitive(name,p)
    elif kind=="mesh":modified_mesh(name,p)
    elif kind=="duplicate":
        source=obj(p["source"]);o=source.copy();o.data=source.data.copy();o.name=name
        bpy.context.scene.collection.objects.link(o)
        o.location=p.get("location",list(source.location))
        if "rotation" in p:o.rotation_euler=p["rotation"]
        if "scale" in p:o.scale=p["scale"]
    elif kind=="transform":
        o=obj(name)
        if "location" in p:o.location=p["location"]
        if "rotation" in p:o.rotation_euler=p["rotation"]
        if "scale" in p:o.scale=p["scale"]
        if p.get("apply",False):active(o);bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    elif kind=="sculpt":sculpt(obj(name),p)
    elif kind in ("extrude","inset","subdivide"):edit_face_operation(obj(name),kind,p)
    elif kind=="bevel":
        active(obj(name));modifier(obj(name),{"kind":"BEVEL","options":{"width":p["width"],"segments":p.get("segments",2)},"apply":p.get("apply",True)})
    elif kind=="modifier":modifier(obj(name),p)
    elif kind=="boolean":
        o=obj(name);other=obj(p["operand"])
        active(o);m=o.modifiers.new("Eldoria_AI_boolean","BOOLEAN")
        m.operation=p["operation"];m.object=other
        bpy.ops.object.modifier_apply(modifier=m.name)
        if p.get("delete_operand",False):bpy.data.objects.remove(other,do_unlink=True)
    elif kind=="material":material(name,p,texroot)
    elif kind=="texture":add_texture(name,p)
    elif kind=="uv":
        o=obj(name);active(o);bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        method=p["method"]
        if method=="smart":bpy.ops.uv.smart_project(island_margin=.03)
        elif method=="cube":bpy.ops.uv.cube_project(cube_size=float(p.get("cube_size",1)))
        elif method=="sphere":bpy.ops.uv.sphere_project()
        elif method=="cylinder":bpy.ops.uv.cylinder_project()
        bpy.ops.object.mode_set(mode="OBJECT")
    elif kind=="join":
        sources=[obj(n) for n in p["sources"]]
        bpy.ops.object.select_all(action="DESELECT")
        for o in sources:o.select_set(True)
        bpy.context.view_layer.objects.active=sources[0]
        bpy.ops.object.join()
        sources[0].name=name
    elif kind=="delete":
        o=obj(name);bpy.data.objects.remove(o,do_unlink=True)
    elif kind=="light":
        data=bpy.data.lights.new(name,p.get("kind","AREA"))
        data.energy=float(p.get("energy",500))
        if data.type=="AREA":data.shape="DISK";data.size=float(p.get("size",5))
        o=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(o)
        o.location=p.get("location",[4,-4,9]);o.rotation_euler=p.get("rotation",[0,0,0])
    elif kind=="camera":
        data=bpy.data.cameras.new(name)
        data.type=p.get("projection","ORTHO");data.ortho_scale=float(p.get("ortho_scale",20))
        o=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(o)
        o.location=p["location"];o.rotation_euler=p["rotation"]
    elif kind=="render_settings":render_setup(p)
    elif kind=="asset_info":
        for k,v in p.items():
            if isinstance(v,(str,int,float,bool)) and str(k).startswith("eldoria_"):
                bpy.context.scene[k]=v

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--plan",type=pathlib.Path,required=True)
    parser.add_argument("--source",type=pathlib.Path)
    parser.add_argument("--out",type=pathlib.Path,required=True)
    parser.add_argument("--texture-root",type=pathlib.Path,default=pathlib.Path("."))
    parser.add_argument("--review",type=pathlib.Path)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])
    plan=load_plan(args.plan)
    out=args.out.resolve()
    if out.exists() and list(out.iterdir()):raise RejectedPlan("Fresh output directory required; never overwrite previous proof")
    out.mkdir(parents=True,exist_ok=True)
    if args.source:
        require_source=args.source.resolve()
        if require_source.suffix.lower()==".blend":bpy.ops.wm.open_mainfile(filepath=str(require_source))
        elif require_source.suffix.lower() in (".glb",".gltf"):
            bpy.ops.wm.read_factory_settings(use_empty=True)
            bpy.ops.import_scene.gltf(filepath=str(require_source))
        else:raise RejectedPlan("Source must be editable .blend or glTF/GLB")
        srcsha=digest(args.source)
    else:
        bpy.ops.wm.read_factory_settings(use_empty=True);srcsha=None
    review=json.loads(args.review.read_text()) if args.review else {}
    render_setup(review.get("render",{"samples":8,"width":960,"height":720}))
    camera_override(review.get("camera",{"location":[11,-18,14],"rotation":[math.radians(58),0,math.radians(31)],"ortho_scale":22}))
    ensure_review_lighting()
    bpy.context.view_layer.update()
    camera_hash=json.dumps(
        [[float(x) for x in row] for row in bpy.context.scene.camera.matrix_world])
    originals=snapshot_meshes()
    before=out/"before-blender.png";bpy.context.scene.render.filepath=str(before)
    bpy.ops.render.render(write_still=True)
    journal=[];started=time.monotonic()
    for act in plan["actions"]:
        previous=snapshot_meshes()
        run_action(act,args.texture_root.resolve())
        now=snapshot_meshes()
        journal.append({"id":act["id"],"op":act["op"],"target":act.get("target"),
                        "reason":act["reason"],"geometry_changed":previous!=now,
                        "mesh_count":len(now),"originals_changed":[k for k,v in originals.items() if now.get(k)!=v]})
    camera_override(review.get("camera",{"location":[11,-18,14],"rotation":[math.radians(58),0,math.radians(31)],"ortho_scale":22}))
    render_setup(review.get("render",{"samples":8,"width":960,"height":720}))
    bpy.context.view_layer.update()
    final_cam=json.dumps([[float(x) for x in row] for row in bpy.context.scene.camera.matrix_world])
    ensure_review_lighting()
    triangle_count=triangles_evaluated()
    if triangle_count<1 or triangle_count>MAX_OUTPUT_TRIANGLES:
        raise RejectedPlan("Evaluated geometry outside mobile review budget: "+str(triangle_count))
    blend=out/"artist-asset.blend";bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    glb=out/"artist-asset.glb";bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=False)
    after=out/"after-blender.png";bpy.context.scene.render.filepath=str(after)
    bpy.ops.render.render(write_still=True)
    result={"state":"TECHNICAL_SOURCE_ONLY","blender_execution":True,"ai_self_approval":False,
            "unity_tested":False,"mobile_device_tested":False,"art_approved":False,
            "artist_plan_sha256":digest(args.plan),"source_sha256":srcsha,
            "before_blender_sha256":digest(before),"after_blender_sha256":digest(after),
            "blend_sha256":digest(blend),"glb_sha256":digest(glb),
            "matched_review_camera":camera_hash==final_cam,
            "evaluated_triangles":triangle_count,"action_count":len(journal),
            "execution":journal,"elapsed_seconds":round(time.monotonic()-started,3)}
    (out/"execution-proof.json").write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_FULL_STUDIO_REAL_BLENDER_PASS",json.dumps({k:result[k] for k in
         ("action_count","evaluated_triangles","matched_review_camera","glb_sha256")}))
if __name__=="__main__":
    main()
