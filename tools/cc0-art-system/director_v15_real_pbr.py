#!/usr/bin/env python3
"""Eldoria CC0 System V15 — test authentic publisher CC0 PBR materials in true V14 GLB meshes.
Source: V14 real owner-authored city from Action #43904480456.
New material inputs: official Poly Haven 1K stone_brick_wall_001 and roof_slates_02 JPG,
verified by size+MD5; license CC0. No concept render or newly generated geometry.
Writes five physically textured GLBs, an editable BLEND, four real Blender PNG.
This is a material A/B experiment. Unity and iPhone untested; art not approved.
"""
import bpy,os,json,struct,math,hashlib
from pathlib import Path
from mathutils import Vector
from math import radians

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV14"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV15"
PRE=ROOT/"docs/cc0-art-system/previews/director-v15"
PBR=Path(os.environ.get("ELDORIA_PBR_REVIEW",str(ROOT/"_pbr_temporary_review")))
for p in (OUT,PRE):p.mkdir(parents=True,exist_ok=True)
if not (PBR/"polyhaven-input-evidence.json").is_file():
    raise RuntimeError("Exact legally proven Poly Haven files have not been downloaded")
AUDIT=json.loads((PBR/"polyhaven-input-evidence.json").read_text())
if len(AUDIT["authoritative_files_verified"])!=6:
    raise RuntimeError("PBR provenance incomplete")
STATS=[]
CAPTURES=[]
def phys_images():
    roles={}
    for name in ("stone_diff","stone_normal","stone_rough","slate_diff","slate_normal","slate_rough"):
        filename=PBR/(name+".jpg")
        row=next((x for x in AUDIT["authoritative_files_verified"] if x["key"]==name),None)
        if row is None or hashlib.sha256(filename.read_bytes()).hexdigest()!=row["sha256"]:
            raise RuntimeError("Poly Haven source SHA256 mismatch: "+name)
        im=bpy.data.images.load(str(filename),check_existing=True)
        im.name="V15_CC0_PolyHaven_"+name
        if "normal" in name or "rough" in name: im.colorspace_settings.name="Non-Color"
        im.pack()
        roles[name]=im
    return roles
def new_uv_on_meshes():
    # Explicit world-coordinate UVs are actual glTF TEXCOORD, not Blender-only generated coords.
    meshes=0;loops=0
    for o in bpy.data.objects:
        if o.type!="MESH" or not o.data.materials:continue
        matnames=[m.name for m in o.data.materials if m]
        if not any(n.startswith(("V8_master_ashlar","V8_master_roof")) for n in matnames):
            continue
        if o.data.users>1:
            o.data=o.data.copy()
        data=o.data
        uv=data.uv_layers.new(name="V15_PBR_PhysicalUV")
        world=o.matrix_world
        for face in data.polygons:
            n=world.to_3x3() @ face.normal
            for li in face.loop_indices:
                v=world @ data.vertices[data.loops[li].vertex_index].co
                if abs(n.z)>.61:
                    u,vv=v.x,v.y
                elif abs(n.x)>abs(n.y):
                    u,vv=v.y,v.z
                else:
                    u,vv=v.x,v.z
                # ~3m across one stone/slate tile; preserve physical scale at mobile zoom.
                uv.data[li].uv=(u*.29,vv*.29)
                loops+=1
        meshes+=1
    if meshes<150:raise RuntimeError("No real material geometry received projected UVs")
    return {"uv_meshes":meshes,"uv_loops":loops}
def bake_stylized_cc0_diffuse(source,role):
    """Physical pre-mixed image keeps the glTF material valid in Unity glTFast."""
    import numpy as np
    w,h=source.size
    data=np.empty(w*h*4,dtype=np.float32)
    source.pixels.foreach_get(data)
    pixels=data.reshape((-1,4))
    # Subtle stylized direction instead of photographic stone pasted unchanged.
    if role=="stone":
        target=np.array([.42,.39,.35],dtype=np.float32)
        strength=.26
    else:
        target=np.array([.075,.105,.15],dtype=np.float32)
        strength=.31
    pixels[:,:3] *= (1.0-strength)
    pixels[:,:3] += target[None,:]*strength
    image=bpy.data.images.new("V15_CC0_Baked_"+role,width=w,height=h,alpha=True)
    image.pixels.foreach_set(np.clip(data,0,1))
    image.file_format="PNG"
    image.filepath_raw=str(OUT/("V15_CC0_Stylized_"+role+"_albedo.png"))
    image.save()
    print("V15_BAKED_REAL_PBR_DIFFUSE",role,w,h)
    return image

def repoint_materials(images):
    touched=[]
    for m in bpy.data.materials:
        if not m.use_nodes:continue
        if m.name.startswith("V8_master_ashlar"):
            role="stone"
        elif m.name.startswith("V8_master_roof"):
            role="slate"
        else:continue
        nodes=m.node_tree.nodes
        bs=nodes.get("Principled BSDF")
        if bs is None:continue
        # Bake a genuinely physical diffuse texture: glTF exporters cannot
        # reliably serialize arbitrary MixRGB shader graphs into glTF PBR.
        # Direct image->Principled link is the supported, auditable path.
        mapping=nodes.new("ShaderNodeUVMap")
        mapping.uv_map="V15_PBR_PhysicalUV"
        diffuse=nodes.new("ShaderNodeTexImage")
        diffuse.image=bake_stylized_cc0_diffuse(images[role+"_diff"],role)
        m.node_tree.links.new(mapping.outputs["UV"],diffuse.inputs["Vector"])
        base=bs.inputs["Base Color"]
        for old_link in list(base.links):
            m.node_tree.links.remove(old_link)
        m.node_tree.links.new(diffuse.outputs["Color"],base)
        # Real normal-map microrelief, limited strength to avoid plastic overprocessing.
        normaltex=nodes.new("ShaderNodeTexImage")
        normaltex.image=images[role+"_normal"]
        m.node_tree.links.new(mapping.outputs["UV"],normaltex.inputs["Vector"])
        normal=nodes.new("ShaderNodeNormalMap")
        normal.inputs["Strength"].default_value=.29 if role=="stone" else .26
        normal.uv_map="V15_PBR_PhysicalUV"
        m.node_tree.links.new(normaltex.outputs["Color"],normal.inputs["Color"])
        normal_input=bs.inputs["Normal"]
        for link in list(normal_input.links):
            m.node_tree.links.remove(link)
        m.node_tree.links.new(normal.outputs["Normal"],normal_input)
        roughtex=nodes.new("ShaderNodeTexImage")
        roughtex.image=images[role+"_rough"]
        m.node_tree.links.new(mapping.outputs["UV"],roughtex.inputs["Vector"])
        rough_input=bs.inputs["Roughness"]
        for link in list(rough_input.links):m.node_tree.links.remove(link)
        m.node_tree.links.new(roughtex.outputs["Color"],rough_input)
        touched.append({"name":m.name,"role":role})
        print("V15_PHYSICAL_PBR_MAPS_LINKED",m.name,role)
    if len(touched)<2:raise RuntimeError("Stone/roof PBR maps were not attached to real source materials")
    return touched
def light():
    sc=bpy.context.scene
    for obj in list(bpy.data.objects):
        if obj.type in ("LIGHT","CAMERA"):bpy.data.objects.remove(obj,do_unlink=True)
    sc.render.engine="CYCLES";sc.cycles.samples=22
    sc.cycles.max_bounces=3
    sc.cycles.use_denoising=False
    sc.render.threads_mode="FIXED";sc.render.threads=4
    sc.view_settings.view_transform="Filmic"
    sc.view_settings.look="Medium High Contrast"
    sc.view_settings.exposure=.35
    world=bpy.data.worlds.new("V15 consistent golden-hour / blue-slate ambient")
    sc.world=world;world.use_nodes=True
    world.node_tree.nodes["Background"].inputs["Color"].default_value=(.38,.43,.51,1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value=.82
    bpy.ops.object.light_add(type="SUN",location=(0,0,40))
    sun=bpy.context.object;sun.data.energy=3.13
    sun.data.color=(1,.77,.59);sun.data.angle=radians(4.1)
    sun.rotation_euler=(radians(72),radians(7),radians(-35))
    for xx in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(xx,-6.3,3))
        point=bpy.context.object;point.data.energy=100
        point.data.color=(1,.60,.38)
        point.data.shadow_soft_size=.7
    bpy.ops.object.camera_add(location=(36,-51,39))
    camera=bpy.context.object;camera.name="V15 PBR consistent comparisons"
    camera.data.type="ORTHO";camera.data.ortho_scale=69
    camera.rotation_euler=(Vector((0,0,5))-camera.location).to_track_quat("-Z","Y").to_euler()
    sc.camera=camera
def render(stage,name,size,mode):
    cam=bpy.context.scene.camera
    if mode=="wide":cam.location=(36,-51,39);cam.data.ortho_scale=69
    elif mode=="close":cam.location=(25,-35,32);cam.data.ortho_scale=44
    else:cam.location=(28,-48,43);cam.data.ortho_scale=57
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    sc=bpy.context.scene
    sc.render.resolution_x,sc.render.resolution_y=size
    sc.render.resolution_percentage=100
    sc.render.image_settings.file_format="PNG"
    dst=PRE/name;sc.render.filepath=str(dst)
    bpy.ops.render.render(write_still=True)
    b=dst.read_bytes()
    if len(b)<150000 or b[:8]!=b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("Not genuine Blender render "+str(dst))
    CAPTURES.append({"stage":stage,"name":name,"size":size,"bytes":len(b)})
    print("V15_REAL_PBR_BLENDER_RENDER",name,len(b))
def export(stage):
    f=OUT/("DirectorV15_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(f),export_format="GLB",export_apply=True)
    b=f.read_bytes()
    if b[:4]!=b"glTF" or struct.unpack_from("<I",b,8)[0]!=len(b):
        raise RuntimeError("V15 3D glTF is invalid")
    n=struct.unpack_from("<I",b,12)[0]
    j=json.loads(b[20:20+n])
    if len(j.get("images",[]))<12 or len(j.get("meshes",[]))<500:
        raise RuntimeError("V15 real texture maps/materials missing from physical GLB")
    names=[x.get("name","") for x in j.get("materials",[])]
    stats={"stage":stage,"meshes":len(j.get("meshes",[])),
           "images":len(j.get("images",[])),
           "materials":len(j.get("materials",[])), "bytes":len(b)}
    STAGES.append(stats)
    print("V15_TRUE_PBR_GLB",json.dumps(stats))
STAGES=[]
def main():
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        f=SOURCE/("DirectorV14_Stage"+str(stage)+".glb")
        if not f.is_file():raise RuntimeError("V14 original physical GLB missing: "+str(f))
        bpy.ops.import_scene.gltf(filepath=str(f),import_pack_images=True)
        images=phys_images()
        uv=new_uv_on_meshes()
        mats=repoint_materials(images)
        light()
        if stage==1:render(stage,"DirectorV15_BastionI_1600x900.png",(1600,900),"wide")
        if stage==5:
            render(stage,"DirectorV15_BastionV_1600x900.png",(1600,900),"wide")
            render(stage,"DirectorV15_BastionV_Close_1600x900.png",(1600,900),"close")
            render(stage,"DirectorV15_BastionV_Portrait_720x1200.png",(720,1200),"portrait")
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV15_Editable_Source.blend"))
        export(stage)
        print("V15_TRUE_PBR_MAP_APPLICATION",stage,len(mats),uv)
    evidence={"project":"Eldoria CC0 Art System","version":"V15 genuine Poly Haven CC0 1K PBR material test",
       "source_run_id":43904480456,"publisher_assets":["stone_brick_wall_001","roof_slates_02"],
       "license":"CC0 1.0","license_url":"https://polyhaven.com/license",
       "source_md5_verified":True,"raw_asset_geometry_added":False,
       "images_are_real_blender":True,"unity_tested":False,"iphone_tested":False,
       "art_approved":False,"input_evidence":AUDIT["authoritative_files_verified"],
       "stages":STAGES,"renders":CAPTURES}
    (PRE/"v15-evidence.json").write_text(json.dumps(evidence,indent=2)+"\n")
    print("ELDORIA_V15_VERIFIED_CC0_PBR_PHYSICAL_PASS",len(STAGES),len(CAPTURES))
if __name__=="__main__":main()
