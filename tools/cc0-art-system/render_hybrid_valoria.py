#!/usr/bin/env python3
"""Real vendor/free-mesh experiment composed around owned Eldoria Bastion I–V.
Reads original (privately staged) Quaternius assets + *owned* generated .blend;
saves only flattened comparison PNGs and non-sensitive audit JSON.
Does NOT redistribute third-party geometry or claim Unity integration.
"""
import bpy
import colorsys
import hashlib
import json
import math
import os
import random
from mathutils import Vector
from pathlib import Path

random.seed(16112026)
ROOT = Path(__file__).resolve().parents[2]
PRIVATE = Path(os.environ["ELDORIA_VENDOR_REVIEW"])
OWN = ROOT / "Unity/Assets/Art_Reboot/CC0System/Generated/CC0_Valoria_Source.blend"
OUT = ROOT / "_vendor_hybrid_output"
OUT.mkdir(parents=True,exist_ok=True)
if not PRIVATE.exists() or not OWN.is_file():
    raise RuntimeError("Private Quaternius inputs or owned Blender source absent")
bpy.ops.wm.open_mainfile(filepath=str(OWN))
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=22
scene.cycles.use_denoising=False
for layer in scene.view_layers:
    layer.cycles.use_denoising=False
scene.render.threads_mode="FIXED"
scene.render.threads=4
scene.view_settings.view_transform="Filmic"
scene.view_settings.look="Medium High Contrast"
scene.view_settings.exposure=.32
scene.render.image_settings.file_format="PNG"
# Store a detailed evidence log of what was actually incorporated.
evidence={
    "approach":"true Quaternius RTS + modular ruins + forest meshes combined temporarily with original Eldoria Bastion",
    "hybrid_source_downloads":"exact files from original Quaternius Google Drive by file ID",
    "source_artifacts_public":False,
    "actual_quaternius_blender_import":False,
    "unity_native_import":False,
    "art_approved":False,
    "commercial_redistribution_certified":False,
    "composite_evidence_images":[],
    "models":[]
}
# Origin semantics: owner's new Bastion is kept. Temporary workshop is kept,
# while provisional cattle/shops and artificial conifers are hidden.
town=bpy.data.collections.get("Town")
if town:
    for obj in town.objects:
        obj.hide_render=not (obj.name.startswith("Timber workshop") or
                             obj.name.startswith("linen-covered supply"))
fol=bpy.data.collections.get("Foliage")
if fol:
    for obj in fol.objects: obj.hide_render=True
# Physical source model without artificial road stand: all imported bases are
# buried slightly into the same coherent terrain, not placed on floating pads.
def dimensions(objlist):
    points=[]
    for o in objlist:
        if o.type != "MESH": continue
        for vert in o.bound_box:
            points.append(o.matrix_world @ Vector(vert))
    if not points:
        raise RuntimeError("Imported asset produced no mesh")
    return (Vector([min(p[i] for p in points) for i in range(3)]),
            Vector([max(p[i] for p in points) for i in range(3)]))

def tone_color(c,role="generic"):
    r,g,b=c[:3]
    h,s,v=colorsys.rgb_to_hsv(r,g,b)
    # Muted dark fantasy palette: preserve material differentiation.
    if role=="ruin":
        return (min(.69,r*.82+.13),min(.69,g*.85+.12),min(.67,b*.84+.12),1)
    if role=="foliage":
        return (.31+.12*v,.39+.11*v,.33+.12*v,1)
    if s<.11:
        return (v*.72,v*.74,v*.76,1)
    if .15<h<.47: # greens are sage, not saturated cartoon green
        return (.27+v*.15,.37+v*.17,.30+v*.16,1)
    if h<.12 or h>.93: # red roof / crest -> dark graphite slate, retains brightness
        return (.29+v*.11,.32+v*.14,.36+v*.16,1)
    if .10<=h<.20: # blonde timber -> aged medium oak
        return (.32+v*.16,.25+v*.14,.20+v*.12,1)
    return (r*.76,g*.77,b*.8,1)

def style_imported(meshes,role,preview_muted=True):
    # Remap only temporary Blender materials, never publisher source files.
    for mesh in meshes:
        for slot in mesh.material_slots:
            if not slot.material: continue
            mat=slot.material
            # Imported materials can be reused by other Quaternius objects;
            # every imported entity is intentionally treated consistently.
            if mat.get("ED_HYBRID_STYLED"):continue
            mat["ED_HYBRID_STYLED"]=True
            mat.use_nodes=True
            bs=mat.node_tree.nodes.get("Principled BSDF")
            if not bs:continue
            diffuse=tuple(mat.diffuse_color)
            new=tone_color(diffuse,role)
            inp=bs.inputs.get("Base Color")
            if inp and inp.is_linked:
                old_link=inp.links[0]
                upstream=old_link.from_socket
                mat.node_tree.links.remove(old_link)
                tint=mat.node_tree.nodes.new("ShaderNodeMixRGB")
                tint.blend_type="MULTIPLY"
                tint.inputs[0].default_value=.8
                tint.inputs[2].default_value=new
                mat.node_tree.links.new(upstream,tint.inputs[1])
                mat.node_tree.links.new(tint.outputs[0],inp)
            elif inp:
                inp.default_value=new
            mat.diffuse_color=new
            bs.inputs["Roughness"].default_value=.88 if role!="foliage" else .95

def load_model(file_name,position,max_width=8.0,role="building",bury=.12):
    f=PRIVATE/file_name
    if not f.is_file():raise RuntimeError("Expected original vendor model missing: "+str(f))
    before={o.as_pointer() for o in bpy.data.objects}
    ext=f.suffix.lower()
    if ext==".gltf":
        bpy.ops.import_scene.gltf(filepath=str(f),import_pack_images=True)
    elif ext==".fbx":
        bpy.ops.import_scene.fbx(filepath=str(f),use_image_search=False)
    else:
        raise ValueError(ext)
    imported=[o for o in bpy.data.objects if o.as_pointer() not in before]
    meshes=[o for o in imported if o.type=="MESH"]
    lo,hi=dimensions(meshes)
    length=hi-lo
    horizontal=max(length.x,length.y)
    if horizontal<0.001:raise RuntimeError("Empty imported bounding box")
    scale=min(max_width/horizontal,9/max(.001,length.z))
    root=bpy.data.objects.new("TEMP_VENDOR_"+file_name,None)
    scene.collection.objects.link(root)
    for o in imported:
        if o.parent is None:
            matrix=o.matrix_world.copy()
            o.parent=root
            o.matrix_world=matrix
    center=(lo+hi)*.5
    root.scale=(scale,scale,scale)
    root.location=(position[0]-center.x*scale,
                   position[1]-center.y*scale,
                   position[2]-lo.z*scale-bury)
    style_imported(meshes,role)
    evidence["models"].append({"name":file_name,"type":role,
                               "mesh_count":len(meshes),"size":[round(float(x),4) for x in length],
                               "scale":round(float(scale),5),
                               "position":position,
                               "publisher_file_sha256":hashlib.sha256(f.read_bytes()).hexdigest()})
    return root

# True Quaternius buildings: no falsely labeled town center or giant model
# converted into Bastion. They deliberately stay separate from main stronghold.
load_model("Barracks_SecondAge_Level3.gltf",(23.2,11.0,.0),10.2)
load_model("Storage_FirstAge_Level1.gltf",(22.8,-9.0,.0),8.8)
load_model("Mine.gltf",(-20.3,12.3,.0),8.6)
load_model("Logs.gltf",(-18.9,-7.8,0),3.4,role="building",bury=.06)

# Reclaimed ruins on the perimeter, never platform or elevated square plinth.
load_model("Wall_ArchRound_Broken.fbx",(-26.1,17.0,0),3.7,"ruin",.18)
load_model("Wall_Broken.fbx",(-24.6,19.8,0),4.0,"ruin",.18)
load_model("Wall_Overgrown.fbx",(-26.7,22.5,0),3.9,"ruin",.18)
load_model("Arch_Gothic.fbx",(-20.6,23.2,0),4.2,"ruin",.18)
# Foliage belonging to same publisher, repeated as two variations.
for name,pos,width in (
    ("PineTree_2.fbx",(-27.8,5.6,.0),3.3),
    ("PineTree_4.fbx",(-29.0,-10.5,.0),4.0),
    ("PineTree_4.fbx",(27.0,19.4,.0),3.6),
    ("PineTree_2.fbx",(30.0,-12.5,.0),3.8),
    ("PineTree_4.fbx",(-31.0,21.2,.0),4.4),
    ("PineTree_2.fbx",(32.7,6.0,.0),3.2)):
    load_model(name,pos,width,"foliage",.10)
for p in ((-24,-12,0),(-27,-8,0),(28,0,0),(19,24,0)):
    load_model("Rock_1.fbx",p,1.9,"ruin",.15)

# Camera must reveal level progression without altering its framing.
cam=scene.camera
cam.data.type="ORTHO"
cam.data.ortho_scale=67
cam.location=(41,-55,46)
target=Vector((0,1.2,4.2))
cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler()
scene.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.24,.30,.37,1)
scene.world.node_tree.nodes["Background"].inputs["Strength"].default_value=.63
for light in [o for o in bpy.data.objects if o.type=="LIGHT" and o.data.type=="SUN"]:
    light.data.energy=2.9
    light.data.color=(1.0,.78,.61)
def set_stage(stage):
    for i in range(2,6):
        c=bpy.data.collections.get("Stage"+str(i))
        if c:c.hide_render=i>stage
def render(stage,name,width,height,ortho=None):
    set_stage(stage)
    cam.data.ortho_scale=ortho or 67
    scene.render.resolution_x=width
    scene.render.resolution_y=height
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)
    p=OUT/name
    if not p.exists() or p.stat().st_size<80000:
        raise RuntimeError("Blender render missing/empty: "+str(p))
    evidence["composite_evidence_images"].append({"file":name,"bytes":p.stat().st_size,
        "stage":stage,"width":width,"height":height,"type":"genuine Blender render, not Unity"})
    print("HYBRID_NATIVE_BLENDER_RENDER",name,p.stat().st_size)

render(1,"Eldoria_Hybrid_SourceModels_Bastion1_1600x900.png",1600,900)
render(5,"Eldoria_Hybrid_SourceModels_Bastion5_1600x900.png",1600,900)
render(5,"Eldoria_Hybrid_SourceModels_Bastion5_720x1200.png",720,1200,51)
evidence["actual_quaternius_blender_import"]=True
evidence["source_raw_archived"]=False
(OUT/"Eldoria_Hybrid_Proof.json").write_text(json.dumps(evidence,indent=2)+"\n")
files=[x.name for x in OUT.iterdir()]
assert all(x.endswith(".png") or x.endswith(".json") for x in files),files
print("ELDORIA_HYBRID_REAL_MODELS_PASS",len(evidence["models"]),len(evidence["composite_evidence_images"]))
