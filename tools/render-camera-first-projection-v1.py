import bpy, math, os, json
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/"references"/"VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
OUT=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-projection-v1"
OBJ=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-projection-v1"/"projection_receiver.obj"
OUT.mkdir(parents=True,exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.wm.obj_import(filepath=str(OBJ))
receiver=bpy.context.selected_objects[0]
receiver.name="CanonicalProjectionReceiver"

# Replace imported material with an emission-only material: preserve exact appearance, no relighting.
mat=bpy.data.materials.new("CanonicalProjectionEmission")
mat.use_nodes=True
nodes=mat.node_tree.nodes
links=mat.node_tree.links
nodes.clear()
out=nodes.new("ShaderNodeOutputMaterial")
em=nodes.new("ShaderNodeEmission")
tex=nodes.new("ShaderNodeTexImage")
tex.image=bpy.data.images.load(str(SRC))
tex.interpolation='Linear'
links.new(tex.outputs["Color"],em.inputs["Color"])
links.new(em.outputs["Emission"],out.inputs["Surface"])
receiver.data.materials.clear(); receiver.data.materials.append(mat)

scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=824
scene.render.resolution_y=464
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
world=bpy.data.worlds.new('ProjectionWorld')
world.color=(0.02,0.02,0.02)
scene.world=world

cam_data=bpy.data.cameras.new("ProjectionProofCamera")
cam=bpy.data.objects.new("ProjectionProofCamera",cam_data)
scene.collection.objects.link(cam)
scene.camera=cam
cam_data.type='PERSP'
cam_data.angle=math.radians(50.0)
cam_data.sensor_fit='HORIZONTAL'

def look_at(obj, target):
    direction=Vector(target)-obj.location
    obj.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()

def render(name, xoff=0.0, yaw_target_x=0.0):
    # OBJ importer converts source Y-up / -Z-forward camera-space mesh
    # into Blender Z-up / -Y-forward world coordinates.
    cam.location=(xoff,0.0,0.0)
    look_at(cam,(yaw_target_x,-10.0,0.0))
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)

render("projection-base.png",0.0,0.0)
render("projection-left-small.png",-0.35,0.0)
render("projection-right-small.png",0.35,0.0)
render("projection-left-medium.png",-0.75,0.0)
render("projection-right-medium.png",0.75,0.0)

bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-projection-v1"/"ValoriaCameraFirstProjectionV1.blend"))
