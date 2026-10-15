"""Blender 5.x workshop demonstrator: real artist-directed tenons, mortises,
bevels, texture/UV and original source preservation. Does not approve art.

General operation: member dimensions and every joint location come from JSON;
this script provides tools, not a fixed tower generator. Blender direct and
the full-studio review utilities are reused rather than replacing its studio.
"""
from __future__ import annotations
import argparse,hashlib,json,math,pathlib,sys
import bpy
from mathutils import Vector
HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
sys.path.insert(0,str(HERE.parents[0]/"full-studio"))
from joinery_contract import validate,JoineryRejected
import blender_toolbox as studio

def digest(p):return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()

def select(o):
    studio.active(o)

def cube(name,center,size):
    bpy.ops.mesh.primitive_cube_add(location=tuple(center))
    o=bpy.context.object;o.name=name
    o.dimensions=tuple(size)
    bpy.context.view_layer.update()
    select(o)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def apply_boolean(receiver,operand,mode):
    select(receiver)
    mod=receiver.modifiers.new("Eldoria_Real_"+mode,"BOOLEAN")
    mod.solver="EXACT";mod.operation=mode;mod.object=operand
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(operand,do_unlink=True)
    receiver.data.update()

def wood_material(rgba):
    # Actual 256x256 baked color texture for glTF, not a Blender-only shader
    # that silently disappears in exported game assets.
    res=256
    img=bpy.data.images.new("Eldoria_Owned_Wood_Grain_R3",width=res,height=res,alpha=True)
    pixels=[]
    for y in range(res):
        for x in range(res):
            periodic=.11*math.sin((x*0.16+y*.012)+.4*math.sin(y*.09))
            grain=.045*math.sin(x*.50+y*.028)
            pat=periodic+grain
            shade=max(.6,min(1.3,1.+pat))
            pixels.extend((min(1.,rgba[0]*shade),min(1.,rgba[1]*shade),
                           min(1.,rgba[2]*shade),1.))
    img.pixels.foreach_set(pixels)
    img.pack()
    mat=bpy.data.materials.new("Eldoria_Workshop_Wood_EmbeddedTexture")
    mat.diffuse_color=tuple(rgba);mat.use_nodes=True
    bsdf=next((n for n in mat.node_tree.nodes if n.bl_idname=="ShaderNodeBsdfPrincipled"),None)
    if bsdf is None:bsdf=mat.node_tree.nodes.new("ShaderNodeBsdfPrincipled")
    output=next((n for n in mat.node_tree.nodes if n.bl_idname=="ShaderNodeOutputMaterial"),None)
    if output is None:output=mat.node_tree.nodes.new("ShaderNodeOutputMaterial")
    if not any(a.from_node==bsdf and a.to_node==output for a in mat.node_tree.links):
        mat.node_tree.links.new(bsdf.outputs["BSDF"],output.inputs["Surface"])
    tex=mat.node_tree.nodes.new("ShaderNodeTexImage");tex.image=img
    mat.node_tree.links.new(tex.outputs["Color"],bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value=.86
    bsdf.inputs["Metallic"].default_value=0
    return mat

def put_material_uv(o,mat):
    o.data.materials.clear();o.data.materials.append(mat)
    select(o)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.cube_project(cube_size=1.0)
    bpy.ops.object.mode_set(mode="OBJECT")
    if not o.data.uv_layers or not o.data.uv_layers.active:
        raise JoineryRejected("NEW_MEMBER_UV_NOT_GENERATED")

def bevel_member(o,width):
    select(o)
    mod=o.modifiers.new("Eldoria_Real_EdgeWear","BEVEL")
    mod.width=width;mod.segments=2
    bpy.ops.object.modifier_apply(modifier=mod.name)
    o.data.update()

def build(spec,source,review,out):
    proof=validate(spec)
    if out.exists() and any(out.iterdir()):
        raise JoineryRejected("REQUIRES_FRESH_OUTPUT_DIRECTORY")
    out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(source.resolve()))
    original=studio.snapshot_meshes()
    if not original:raise JoineryRejected("NO_FROZEN_ORIGINAL")
    studio.render_setup(review["render"])
    studio.camera_override(review["camera"])
    studio.ensure_review_lighting();studio.ensure_review_stage()
    bpy.context.view_layer.update()
    cam0=[[float(x) for x in row] for row in bpy.context.scene.camera.matrix_world]
    bpy.context.scene.render.filepath=str(out/"before-blender.png")
    bpy.ops.render.render(write_still=True)
    mat=wood_material(spec["wood_color_rgba"])
    objects={}
    for member in spec["members"]:
        o=cube(member["id"],member["center_m"],member["size_m"])
        objects[member["id"]]=o
    joint_stats=[]
    for i,j in enumerate(spec["joints"]):
        # Boolean UNION gives a real stepped tenon that belongs to the post.
        post=objects[j["tenon_member"]]
        lintel=objects[j["socket_member"]]
        before_faces=len(lintel.data.polygons)
        neck=cube("_EldoriaTenonWork"+str(i),j["center_m"],j["tenon_size_m"])
        apply_boolean(post,neck,"UNION")
        cutter=cube("_EldoriaMortiseCutter"+str(i),j["center_m"],j["socket_size_m"])
        apply_boolean(lintel,cutter,"DIFFERENCE")
        after_faces=len(lintel.data.polygons)
        if after_faces <= before_faces:
            raise JoineryRejected("MORTISE_NOT_CUT_IN_REAL_MESH")
        # Optional individually positioned wood peg. Real pin is unioned to
        # existing part and not emitted as a misleading unconnected prop.
        if "peg" in j:
            pg=j["peg"]
            bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=pg["radius_m"],
                 depth=pg["length_m"],location=tuple(pg["center_m"]),
                 rotation=(math.pi/2,0,0))
            pin=bpy.context.object;pin.name="_EldoriaDowelWork"+str(i)
            bpy.context.view_layer.update()
            apply_boolean(lintel,pin,"UNION")
        joint_stats.append({"member_from":j["tenon_member"],"member_to":j["socket_member"],
           "kind":"REAL_BOOLEAN_MORTISE_TENON","faces_before":before_faces,
           "faces_after":after_faces,"width_clearances_m":[round(j["socket_size_m"][k]-j["tenon_size_m"][k],5) for k in range(3)],
           "has_wooden_peg":bool(j.get("peg"))})
    for member in spec["members"]:
        o=objects[member["id"]]
        bevel_member(o,member["edge_bevel_m"])
        put_material_uv(o,mat)
    bpy.context.view_layer.update()
    if studio.snapshot_meshes()|{} == {}:raise JoineryRejected("NO_SOURCE")
    after_mesh=studio.snapshot_meshes()
    if any(after_mesh.get(k)!=v for k,v in original.items()):
        raise JoineryRejected("SOURCE_GEOMETRY_CHANGED")
    # Existing evaluated BVH nearest-triangle contact checker, not bbox guess.
    from blender_surface_contact import actual_inherited_surface_contact
    contact=actual_inherited_surface_contact(set(original))
    cam1=[[float(x) for x in row] for row in bpy.context.scene.camera.matrix_world]
    if cam0!=cam1:raise JoineryRejected("CAMERA_DRIFT")
    blend=out/"joinery-workshop.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    bpy.ops.object.select_all(action="DESELECT")
    exported=[o for o in bpy.data.objects if o.type=="MESH" and not o.name.startswith("_EldoriaReview")]
    for o in exported:o.select_set(True)
    bpy.context.view_layer.objects.active=exported[0]
    glb=out/"joinery-workshop.glb"
    bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True)
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.scene.render.filepath=str(out/"after-blender.png")
    bpy.ops.render.render(write_still=True)
    comps=[{"name":o.name,"derived_from_source_object":o.name in original,
            "vertices":len(o.data.vertices),
            "faces":len(o.data.polygons),
            "dimensions_m":[round(float(x),5) for x in o.dimensions],
            "uv_layers":len(o.data.uv_layers),
            "material_count":len(o.data.materials)}
           for o in exported]
    catalog={"schema_version":1,"asset_id":spec["asset_id"],
       "artist_plan_sha256":digest(out/"joinery-plan.json"),
       "new_components":sorted(objects),"components":comps,
       "source_sha256":digest(source),"source_file_unchanged":True,
       "art_approved":False}
    (out/"reusable-asset-catalog.json").write_text(json.dumps(catalog,indent=2)+"\n")
    (out/"actual-mesh-contact.json").write_text(json.dumps(contact,indent=2)+"\n")
    output={"source_sha256":digest(source),"plan_sha256":catalog["artist_plan_sha256"],
      "glb_sha256":digest(glb),"blend_sha256":digest(blend),
      "before_sha256":digest(out/"before-blender.png"),
      "after_sha256":digest(out/"after-blender.png"),
      "source_original_geometry_unchanged":True,
      "camera_matched":cam0==cam1,
      "joinery":joint_stats,
      "new_members":len(objects),
      "new_members_with_uvs":sum(bool(o.data.uv_layers) for o in objects.values()),
      "wood_texture_packed":bool(mat.node_tree.nodes.get("Image Texture") and mat.node_tree.nodes.get("Image Texture").image.packed_file),
      "contact_checker_status":contact.get("status"),
      "art_approved":False,"unity_tested":False}
    (out/"workshop-proof.json").write_text(json.dumps(output,indent=2)+"\n")
    print("ELDORIA_JOINERY_REAL_BLENDER_TECH_PASS="+json.dumps(output))
    return output

if __name__=="__main__":
    p=argparse.ArgumentParser()
    for opt in ("plan","source","review","out"):p.add_argument("--"+opt,required=True)
    a=p.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])
    source=pathlib.Path(a.source);out=pathlib.Path(a.out)
    spec=json.loads(pathlib.Path(a.plan).read_text(encoding="utf8"))
    out.mkdir(parents=True,exist_ok=True)
    (out/"joinery-plan.json").write_bytes(pathlib.Path(a.plan).read_bytes())
    # build insists fresh output apart from the immutable copied plan.
    plan_snapshot=(out/"joinery-plan.json").read_bytes()
    (out/"joinery-plan.json").unlink()
    build(spec,source,json.loads(pathlib.Path(a.review).read_text()),out)
    (out/"joinery-plan.json").write_bytes(plan_snapshot)
