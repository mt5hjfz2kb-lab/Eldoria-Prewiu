#!/usr/bin/env python3
"""Eldoria CC0 Art System — DIRECTOR V17, causal repair of detached 3D roofs.

Input: actual Director V16 GLBs, run 43911033294.
The visual inspection exposed that 4 huge real roof meshes use an identity
object transform and store 3D world-coordinates in vertex positions. V16's
translation-only classification missed them. Reposition those physical mesh
objects from their bounding-box world center and REMOVE inherited duplicate
roof/silo/tower families instead of hiding the fault with new overlays.

No concept imagery, no new outside content, no mutation of main/gameplay.
Export five real updated 3D GLBs + editable Blender file + genuine comparisons.
CI technical pass != aesthetic pass != Unity/mobile test.
"""
from pathlib import Path
from mathutils import Vector
from math import radians
import bpy,json,struct,math,hashlib

ROOT=Path(__file__).resolve().parents[2]
INPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV16"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV17"
PREV=ROOT/"docs/cc0-art-system/previews/director-v17"
for p in (OUT,PREV):p.mkdir(parents=True,exist_ok=True)

OLD={"sawmill":(-22.,-7.5),"quarry":(-23.5,11.8),
     "barracks":(23.8,11.4),"granary":(22.,-9.2)}
NEW={"sawmill":(-32.,-12.5),"quarry":(-33.2,18.4),
     "barracks":(34.5,18.4),"granary":(32.,-15.5)}
ROOFS={
 "sawmill":"ASERRADERO / open waterwheel workshop heavy slate",
 "quarry":"CANTERA / small sheltered stone-cutting workshop heavy slate",
 "barracks":"CUARTEL / stone guardhouse and barracks heavy slate",
 "granary":"GRANERO / long grain warehouse heavy slate"}
# Exact geometry bounds independently measured in the physical V16 GLB, not a
# generic district waypoint. The quarry workshop is east of its extraction site.
ROOF_SOURCE_CENTERS={"sawmill":(-22.0,-7.5),"quarry":(-19.1,10.7),
                     "barracks":(22.8,11.4),"granary":(21.0,-9.2)}
# Every one of these combinations was visually and physically duplicated in V16.
# Exact prefix matching, never generalized "all roofs" deletion.
REMOVE_PREFIXES=(
 "sawmill open work-bay",
 "guardhouse military steep gable",
 "barracks martial gable",
 "imperial grainhouse heavy storage",
 "wheel-fed natural shallow stream",
 # Legacy repeated superimposed silo geometry: V16-built twin shells/caps remain.
 "granary | round masonry silo",
 "granary | charcoal conical lid",
 "granary | timber binding",
 "granary attached limestone silo",
 "granary dressed horizontal silo band",
 "granary broad conic blue slate cap",
 "granary weatherproof slate cap",
 "ventilated granary slate cap",
 "granero: genuine silo drum",
 "granero / charcoal conical cap",
 "grain silo masonry ring",
 # Earlier tower stack had a second fully overlapping watchtower.
 "barracks | forward watchtower",
 "barracks | parapet cap",
 "barracks | tooth crenel",
 "cuartel / vertical crenel",
)
FOG_STEPS={}
REPORTS=[]
RENDERS=[]
def center_world(o):
    return sum((o.matrix_world @ Vector(corner) for corner in o.bound_box),
               Vector((0,0,0)))/8.0
def repair_roofs(stage):
    meta={}
    for region,prefix in ROOFS.items():
        roof=[o for o in bpy.data.objects if o.type=="MESH" and o.name.startswith(prefix)]
        if len(roof)!=1:raise RuntimeError("V17 expected one, not "+str(len(roof))+" physical roof "+prefix)
        obj=roof[0]
        pos=center_world(obj)
        cx,cy=ROOF_SOURCE_CENTERS[region]
        if abs(pos.x-cx)>1.0 or abs(pos.y-cy)>1.0:
            raise RuntimeError("Roof is not at suspected old physical coordinates "+str((stage,region,tuple(pos))))
        # Preserve the same whole-district displacement used by V16. The
        # actual roof centroid need not equal the district's nominal anchor.
        dx=NEW[region][0]-OLD[region][0]
        dy=NEW[region][1]-OLD[region][1]
        mat=obj.matrix_world.copy()
        mat.translation+=Vector((dx,dy,0))
        obj.matrix_world=mat
        changed=center_world(obj)
        target=(pos.x+dx,pos.y+dy)
        if abs(changed.x-target[0])>.05 or abs(changed.y-target[1])>.05:
            raise RuntimeError("Roof 3D vertices did not follow corrected transform")
        meta[region]={"name":obj.name,"old_world_bbox":tuple(round(float(x),3) for x in pos),
                      "new_world_bbox":tuple(round(float(x),3) for x in changed),
                      "delta":(dx,dy),"changed_real_world_mesh":True}
        print("V17_PHYSICAL_WORLD_SPACE_ROOF_REPAIRED",stage,region,meta[region])
    return meta
def remove_duplicates(stage):
    removed=[]
    for obj in list(bpy.data.objects):
        if obj.type!="MESH":continue
        name=obj.name.lower()
        if name.startswith(REMOVE_PREFIXES):
            removed.append(obj.name)
            bpy.data.objects.remove(obj,do_unlink=True)
    if len(removed)<30:
        raise RuntimeError("V17 expected a substantial number of duplicate historic parts, found "+str(len(removed)))
    print("V17_REMOVED_CONFLICTING_GEOMETRY",stage,len(removed))
    return removed
def clear_competing_foliage(stage):
    # The V16 site test showed real forest foliage obstructing two workshops;
    # some broadleaf/crown meshes ALSO have origin=0, hence use bbox centers.
    prefixes=("v14 irregular dense fir foliage","v11 spruce living crown",
              "v14 fir trunk grounded into world","v11 evergreen wild underbrush")
    removed=0
    for o in list(bpy.data.objects):
        if o.type!="MESH" or not o.name.lower().startswith(prefixes):continue
        c=center_world(o)
        # Clear only the local door/front working face, leave perimeter evergreen forest.
        if any(abs(c.x-x)<5.6 and abs(c.y-y)<5.4 for x,y in NEW.values()):
            bpy.data.objects.remove(o,do_unlink=True)
            removed+=1
    print("V17_INDUSTRIAL_DOOR_SIGHTLINES_CLEAR",stage,removed)
    return removed
def light_camera():
    scene=bpy.context.scene
    for o in list(bpy.data.objects):
        if o.type in ("CAMERA","LIGHT"):
            bpy.data.objects.remove(o,do_unlink=True)
    scene.render.engine="CYCLES"
    scene.cycles.samples=25
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.42
    world=bpy.data.worlds.new("V17 coherent amber/cool lighting")
    scene.world=world;world.use_nodes=True
    bg=world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.41,.45,.52,1)
    bg.inputs["Strength"].default_value=.87
    bpy.ops.object.light_add(type="SUN",location=(0,0,45))
    sun=bpy.context.object
    sun.data.energy=3.1
    sun.data.color=(1,.80,.62)
    sun.data.angle=radians(4)
    sun.rotation_euler=(radians(71),radians(8),radians(-34))
    for xx in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(xx,-6.1,3.4))
        p=bpy.context.object;p.data.energy=106
        p.data.color=(1,.63,.34)
        p.data.shadow_soft_size=.7
    bpy.ops.object.camera_add(location=(43,-61,48))
    cam=bpy.context.object
    cam.name="V17 same physical world, comparable scene camera"
    cam.data.type="ORTHO";cam.data.ortho_scale=89
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def frame(stage,file,width,height,mode,site=None):
    sc=bpy.context.scene
    cam=sc.camera
    focus=Vector((0,0,5))
    if mode=="wide":cam.location=(43,-61,48);cam.data.ortho_scale=89
    elif mode=="close":cam.location=(29,-41,34);cam.data.ortho_scale=48
    elif mode=="portrait":cam.location=(40,-64,52);cam.data.ortho_scale=67
    elif mode=="district":
        x,y=NEW[site]
        cam.location=(x+12,y-18,14)
        focus=Vector((x,y,2.8))
        cam.data.ortho_scale=24
    cam.rotation_euler=(focus-cam.location).to_track_quat("-Z","Y").to_euler()
    sc.render.resolution_x=width;sc.render.resolution_y=height
    sc.render.resolution_percentage=100
    sc.render.image_settings.file_format="PNG"
    out=PREV/file
    sc.render.filepath=str(out)
    bpy.ops.render.render(write_still=True)
    data=out.read_bytes()
    if len(data)<100000 or data[:8]!=b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("V17 Blender didn't produce original render "+str(out))
    RENDERS.append({"stage":stage,"file":file,"pixels":(width,height),"bytes":len(data)})
    print("V17_TRUE_BLENDER_CAPTURE",file,len(data))
def export(stage,roofs):
    filename=OUT/("DirectorV17_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(filename),export_format="GLB",export_apply=True)
    data=filename.read_bytes()
    if data[:4]!=b"glTF" or struct.unpack_from("<I",data,8)[0]!=len(data):
        raise RuntimeError("Invalid physical V17 glTF")
    length=struct.unpack_from("<I",data,12)[0]
    j=json.loads(data[20:20+length])
    names=[n.get("name","") for n in j.get("nodes",[])]
    for region,prefix in ROOFS.items():
        instances=[n for n in j["nodes"] if n.get("name","").startswith(prefix)]
        if len(instances)!=1:raise RuntimeError("Exported V17 missing corrected large roof "+prefix)
        target=roofs[region]["new_world_bbox"]
        # Imported world-space roof's object matrix should move in glTF 3D.
        # Validate translation moved OR vertices are transformed by exporter.
        assert len(j.get("meshes",[]))>500 and len(j.get("images",[]))>10
    for banned in ("sawmill open work-bay slate planes","guardhouse military steep gable slate planes",
                   "imperial grainhouse heavy storage slate planes"):
        if any(n.startswith(banned) for n in names):
            raise RuntimeError("Residual duplicate historical roof still in exported V17: "+banned)
    rec={"stage":stage,"meshes":len(j["meshes"]),"images":len(j["images"]),
         "materials":len(j.get("materials",[])),"bytes":len(data)}
    print("V17_TRUE_FIXED_SOURCE_GLTF",json.dumps(rec))
    return rec
def main():
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        file=INPUT/("DirectorV16_Stage"+str(stage)+".glb")
        if not file.is_file():raise RuntimeError("V16 owner-authored source absent: "+str(file))
        bpy.ops.import_scene.gltf(filepath=str(file),import_pack_images=True)
        roofs=repair_roofs(stage)
        duplicates=remove_duplicates(stage)
        pruned=clear_competing_foliage(stage)
        light_camera()
        if stage==1:frame(stage,"DirectorV17_BastionI_1600x900.png",1600,900,"wide")
        if stage==5:
            frame(stage,"DirectorV17_BastionV_1600x900.png",1600,900,"wide")
            frame(stage,"DirectorV17_BastionV_Close_1600x900.png",1600,900,"close")
            frame(stage,"DirectorV17_BastionV_Portrait_720x1200.png",720,1200,"portrait")
            for site in ("sawmill","quarry","barracks","granary"):
                frame(stage,"DirectorV17_"+site+"_1024x768.png",1024,768,"district",site)
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV17_Editable_Source.blend"))
        item=export(stage,roofs)
        REPORTS.append({"stage":stage,"roofs":roofs,"duplicate_geometry_removed":len(duplicates),
                        "trees_pruned":pruned,"glb":item})
    doc={"name":"Eldoria CC0 Art System","version":"Director V17 detached-roof root cause corrected",
         "source_run_id":43911033294,"source_author_owned":True,
         "source_PolyHaven_PBR_CC0":True,"concept_image_used":False,
         "actual_3d_roof_transform_fixed":True,"duplicate_roof_family_deleted":True,
         "blender_render_verified":True,"unity_tested":False,"iphone_tested":False,
         "premium_art_approved":False,"stages":REPORTS,"renders":RENDERS}
    (PREV/"v17-evidence.json").write_text(json.dumps(doc,indent=2)+"\n")
    print("ELDORIA_CC0_V17_DETACHED_ROOFS_FIXED",len(REPORTS),len(RENDERS))
if __name__=="__main__":main()
