"""Offline art-direction check of the SAME 31 native Blender kit sources.
Never claim this Blender image is a Unity/URP screenshot.
Actual approved deliverable remains the exact-head Unity Editor run.
"""
import bpy, math, os, json, random
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1]
source=root/"Source~"
catalog=json.loads((source/"kit_manifest.json").read_text())
assert catalog["version"]==2 and len(catalog["pieces"])==31
bpy.ops.wm.open_mainfile(filepath=str(source/"modular_kit.blend"))
for image in bpy.data.images:
    if "Hero_Atlas_256" in image.name:
        image.filepath=str(root.parent/"Phase1/Textures/Hero_Atlas_256.png")
        image.reload()
pieces={o.name.split(".")[0]:o for o in bpy.context.scene.objects if o.type=="MESH"}
assert all(item["id"] in pieces for item in catalog["pieces"])
for ob in pieces.values():ob.hide_render=True
for ob in tuple(bpy.context.scene.objects):
    if ob.type in {"CAMERA","LIGHT"}:bpy.data.objects.remove(ob,do_unlink=True)
random.seed(2161126)
instances=[]
def put(kind,center,x=0,y=0,z=0,rot=0):
    """Same axes as Unity: (Unity x,y,z) => (Blender x,z,y)."""
    ob=pieces[kind].copy();ob.data=pieces[kind].data
    bpy.context.collection.objects.link(ob)
    ob.hide_render=False
    ob.location=(center[0]+x,center[2]+z,center[1]+y)
    ob.rotation_euler=(0,0,-math.radians(rot))
    ob.name="VILLAGE / "+kind
    instances.append(kind)
    return ob
house=(-11,0,4);workshop=(11,0,4);corrupt=(0,0,-13)
def p(kind,x,y,z,rot=0):put(kind,house,x,y,z,rot)
for id,x,z,rot in [
 ("Wall_Door_Intact",-2,-4,0),("Wall_Window_Intact",2,-4,0),
 ("Wall_Window_Intact",-2,4,180),("Wall_Straight_Intact",2,4,180),
 ("Wall_Straight_Intact",-4,-2,90),("Wall_Straight_Intact",-4,2,90),
 ("Wall_Straight_Intact",4,-2,-90),("Wall_Straight_Intact",4,2,-90)]:
 p(id,x,0,z,rot)
for id,x,y,z,rot in [("Roof_Gable",0,3.67,0,0),("Tower_Bell",6,0,1,0),
 ("Roof_Tower",6,7.56,1,0),("Balcony",-1.8,2.42,-4.6,0),
 ("Shield",1.72,1.36,-4.53,0),("Beam",-4.1,0,-3.85,0),
 ("Beam",3.9,0,3.84,0),("Chimney",-2,2.5,2.4,0)]:
 p(id,x,y,z,rot)
def p(kind,x,y,z,rot=0):put(kind,workshop,x,y,z,rot)
for id,x,z,rot in [("Wall_Door_Intact",-2,-2,0),("Wall_Window_Intact",2,-2,0),
 ("Wall_Straight_Intact",-2,2,180),("Wall_Window_Intact",2,2,180),
 ("Wall_Corner_Intact",-4,0,90),("Wall_Straight_Intact",4,0,-90)]:
 p(id,x,0,z,rot)
for id,x,y,z,rot in [("Roof_Shed",-2,3.65,0,0),("Roof_Shed",2,3.65,0,0),
 ("Chimney",2.65,3.20,.95,0),("Roof_Shed",6.35,2.9,-1.25,90),
 ("Balcony",.5,2.40,2.6,180),("Shield",-2.12,1.5,-2.5,0),
 ("Beam",-3.9,0,-1.8,0),("Rock_Large",6.2,0,-3.1,0),
 ("Rock_Small",5.7,0,-4.0,0),("Fallen_Log",8.1,0,2.6,180)]:
 p(id,x,y,z,rot)
for x in (4.6,8.2):
 for z in (-3.1,.5):p("Beam",x,0,z)
def p(kind,x,y,z,rot=0):put(kind,corrupt,x,y,z,rot)
for id,x,z,rot in [
 ("Wall_Door_Damaged",-2,-4,0),("Wall_Window_Damaged",2,-4,0),
 ("Wall_Straight_Damaged",-2,4,180),("Wall_Corner_Damaged",2,4,180),
 ("Wall_Straight_Damaged",-4,-2,90),("Wall_Window_Damaged",-4,2,90),
 ("Wall_Corner_Damaged",4,-2,-90),("Wall_Straight_Damaged",4,2,-90)]:
 p(id,x,0,z,rot)
for id,x,y,z,rot in [("Roof_Gable",0,3.67,0,0),("Tower_Broken",6,0,1.4,0),
 ("Corrupt_Roots",-2.4,0,-4.4,0),("Corrupt_Roots",2.3,0,-4.45,0),
 ("Corrupt_Roots",4.25,0,-1.6,-90),("Corrupt_Roots",6.45,1.5,1.25,180),
 ("Corrupt_Roots",1.6,3.88,-1.85,90),("Crystal_Large",4.8,0,-4.1,0),
 ("Crystal_Medium",-2.75,0,-4.4,0),("Crystal_Medium",5.9,0,2.1,0),
 ("Crystal_Small",2.4,0,-4.8,0),("Crystal_Small",7.1,0,-1.8,0),
 ("Rock_Large",7.8,0,3.3,0),("Rock_Medium",-5.9,0,-3.3,0),
 ("Fallen_Log",8.1,0,-2.4,0)]:
 p(id,x,y,z,rot)

# Context: same three varied pine sizes, framing paths and grounded foliage.
pines=[(-28,6),(-29,-6),(-26,19),(27,8),(30,-11),(25,20),
 (-20,26),(-13,27),(0,27),(12,28),(20,24),(-30,-22),(-19,-24),
 (-12,-27),(4,-29),(14,-28),(26,-23),(-31,0),(32,0),(-21,14),(20,15)]
for i,(x,z) in enumerate(pines):
 put(["Pine_Small","Pine_Medium","Pine_Tall"][i%3],(0,0,0),x,0,z,random.randrange(0,360))
 if i%3==0:put("Shrubs",(0,0,0),x+1.4,0,z-1.8)
for i,(x,z) in enumerate([(-11,-1),(-9,-6),(-6,-9),(-2,-9),
 (2,-8),(5,-6),(8,-2),(11,-1)]):
 put("Path_Slab",(0,0,0),x,.02,z,(-11 if i%2==0 else 14))
 if i%2==0:put("Tall_Grass",(0,0,0),x+1.85,0,z+.6)
# A plain review floor, not source art: source textures remain EXACT.
bpy.ops.mesh.primitive_plane_add(size=2,location=(0,-4,-.13))
floor=bpy.context.object;floor.name="REVIEW ONLY / ground"
floor.scale=(49,40,1)
mat=bpy.data.materials.new("REVIEW ONLY mossy ground")
mat.use_nodes=True
bs=mat.node_tree.nodes.get("Principled BSDF")
bs.inputs["Base Color"].default_value=(.31,.37,.27,1);bs.inputs["Roughness"].default_value=.93
floor.data.materials.append(mat)

def light(name,pos,color,power,size):
 bpy.ops.object.light_add(type="AREA",location=pos)
 ob=bpy.context.object;ob.name=name;ob.data.energy=power;ob.data.color=color
 ob.data.shape="DISK";ob.data.size=size
light("Warm low amber",(0,-29,29),(1,.72,.48),4600,26)
light("Blue violet wrap",(-30,30,31),(.58,.65,1),3200,31)
light("Warm residence fill",(20,-20,14),(1,.86,.65),1200,19)
world=bpy.data.worlds.new("Dusk warm horizon")
world.use_nodes=True
world.node_tree.nodes.get("Background").inputs["Color"].default_value=(.24,.21,.32,1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value=.82
bpy.context.scene.world=world
# Approximate Unity's 42 degree pitched architectural comparison camera.
bpy.ops.object.camera_add(location=(9,-52,49))
camera=bpy.context.object;target=Vector((0,-2.0,3.2))
camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO';camera.data.ortho_scale=59
scene=bpy.context.scene;scene.camera=camera
scene.render.engine="CYCLES";scene.cycles.samples=24;scene.cycles.use_denoising=False
for layer in scene.view_layers:layer.cycles.use_denoising=False
scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
scene.view_settings.view_transform="AgX"
scene.view_settings.exposure=.52
out=os.environ.get("KIT_VILLAGE_PREVIEW_OUTPUT",str(source/"Evidence/blender-village-review.png"))
Path(out).parent.mkdir(parents=True,exist_ok=True)
scene.render.filepath=out;bpy.ops.render.render(write_still=True)
assert Path(out).exists() and Path(out).stat().st_size>50000
print("BLENDER_ASSEMBLED_KIT_PASS","objects",len(instances),"source-pieces",len(set(instances)),"out",out,flush=True)
