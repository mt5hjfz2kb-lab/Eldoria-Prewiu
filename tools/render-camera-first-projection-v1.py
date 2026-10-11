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
cam_data.clip_start=0.01
cam_data.clip_end=100.0

# Use actual imported world-space bounds so axis conversion cannot make the proof look away.
corners=[receiver.matrix_world @ Vector(v) for v in receiver.bound_box]
mins=Vector((min(v.x for v in corners),min(v.y for v in corners),min(v.z for v in corners)))
maxs=Vector((max(v.x for v in corners),max(v.y for v in corners),max(v.z for v in corners)))
center=(mins+maxs)*0.5
dims=maxs-mins
print("RECEIVER_BOUNDS_MIN",tuple(round(x,5) for x in mins))
print("RECEIVER_BOUNDS_MAX",tuple(round(x,5) for x in maxs))
print("RECEIVER_CENTER",tuple(round(x,5) for x in center))
print("RECEIVER_DIMS",tuple(round(x,5) for x in dims))

def look_at(obj, target):
    direction=Vector(target)-obj.location
    obj.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()

def render(name, xoff=0.0):
    cam.location=(xoff,0.0,0.0)
    look_at(cam,center)
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)

# Solid diagnostic proves camera/frustum independently of UV/material.
solid=bpy.data.materials.new("ProjectionDiagnosticWhite")
solid.use_nodes=True
pn=solid.node_tree.nodes.get("Principled BSDF")
pn.inputs["Base Color"].default_value=(1.0,0.2,0.1,1.0)
pn.inputs["Roughness"].default_value=1.0
receiver.data.materials.clear(); receiver.data.materials.append(solid)
render("projection-debug-solid.png",0.0)

# Restore exact canonical unlit projection.
receiver.data.materials.clear(); receiver.data.materials.append(mat)
render("projection-base.png",0.0)
render("projection-left-small.png",-0.35)
render("projection-right-small.png",0.35)
render("projection-left-medium.png",-0.75)
render("projection-right-medium.png",0.75)

debug={
    "bounds_min":list(mins),
    "bounds_max":list(maxs),
    "center":list(center),
    "dimensions":list(dims),
    "camera_base":list(cam.location),
    "target":list(center)
}
(OUT/"render-debug.json").write_text(json.dumps(debug,indent=2)+"\n")

bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-projection-v1"/"ValoriaCameraFirstProjectionV1.blend"))
