"""Blender offline preview from ACTUAL Phase2 kit .blend source.
Independent visual QA only; NOT a substitute for genuine Unity screenshots.
"""
import bpy, math, os, json, sys
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1]
src=root/"Source~"
blend=src/"modular_kit.blend"
manifest=json.loads((src/"kit_manifest.json").read_text())
assert len(manifest["pieces"])==31
assert blend.exists() and blend.stat().st_size>10000
bpy.ops.wm.open_mainfile(filepath=str(blend))
for image in bpy.data.images:
    if "Hero_Atlas_256" in image.name:
        image.filepath=str(root.parent/"Phase1/Textures/Hero_Atlas_256.png")
        image.reload()
mesh_names={x.name.split(".")[0] for x in bpy.data.objects if x.type=="MESH"}
assert all(x["id"] in mesh_names for x in manifest["pieces"]),("Missing source meshes",mesh_names)
for x in bpy.context.scene.objects:
    if x.type=="CAMERA" or x.type=="LIGHT":bpy.data.objects.remove(x,do_unlink=True)
# Temporary review floor: not a kit material or kit mesh, not exported to Unity.
bpy.ops.mesh.primitive_plane_add(size=2,location=(23.9,10.6,-.20))
ground=bpy.context.object
ground.name="STUDIO ONLY / 8x4 specimen floor"
ground.scale=(30,19,1)
mat=bpy.data.materials.new("STUDIO ONLY soft-slate stone")
mat.diffuse_color=(.23,.24,.27,1);mat.use_nodes=True
bs=mat.node_tree.nodes.get("Principled BSDF")
bs.inputs["Base Color"].default_value=(.32,.32,.36,1)
bs.inputs["Roughness"].default_value=.92
ground.data.materials.append(mat)
def light(name,loc,power,temperature,size):
    bpy.ops.object.light_add(type='AREA',location=loc)
    ob=bpy.context.object;ob.name=name;ob.data.energy=power;ob.data.shape='DISK';ob.data.size=size
    ob.data.color=temperature
light("Dusk broad key",(5,-30,40),3500,(1,.74,.48),30)
light("Violet reflection",(48,30,28),2600,(.64,.65,1),30)
light("Neutral overhead",(23,8,45),1400,(.78,.83,1),35)
world=bpy.data.worlds.new("review dusk hemisphere")
world.use_nodes=True
back=world.node_tree.nodes.get("Background")
back.inputs["Color"].default_value=(.16,.15,.24,1)
back.inputs["Strength"].default_value=.65
bpy.context.scene.world=world
bpy.ops.object.camera_add(location=(25.6,-52,65))
camera=bpy.context.object;target=Vector((23.9,10.6,3.1))
camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO'
camera.data.ortho_scale=73
bpy.context.scene.camera=camera
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=20
scene.cycles.use_denoising=True
scene.render.resolution_x=2000
scene.render.resolution_y=1125
scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
scene.render.image_settings.file_format="PNG"
output=os.environ.get("KIT_BLENDER_PREVIEW_OUTPUT",str(src/"Evidence/blender-kit-review.png"))
Path(output).parent.mkdir(parents=True,exist_ok=True)
scene.render.filepath=output
bpy.ops.render.render(write_still=True)
assert Path(output).exists() and Path(output).stat().st_size>50000
print("KIT_STUDIO_SOURCE_PREVIEW_PASS",output,len(manifest["pieces"]),manifest["total_triangles"],flush=True)
