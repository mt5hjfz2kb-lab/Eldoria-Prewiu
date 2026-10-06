import bpy, math, json
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

def emission_material(name, image):
    mat=bpy.data.materials.new(name)
    mat.use_nodes=True
    nodes=mat.node_tree.nodes
    links=mat.node_tree.links
    nodes.clear()
    out=nodes.new("ShaderNodeOutputMaterial")
    em=nodes.new("ShaderNodeEmission")
    tex=nodes.new("ShaderNodeTexImage")
    tex.image=image
    tex.interpolation='Linear'
    links.new(tex.outputs["Color"],em.inputs["Color"])
    links.new(em.outputs["Emission"],out.inputs["Surface"])
    return mat

image=bpy.data.images.load(str(SRC))
mat=emission_material("CanonicalProjectionEmission",image)
receiver.data.materials.clear()
receiver.data.materials.append(mat)

scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=824
scene.render.resolution_y=464
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
world=bpy.data.worlds.new("ProjectionWorld")
world.color=(0.01,0.01,0.01)
scene.world=world

cam_data=bpy.data.cameras.new("ProjectionProofCamera")
cam=bpy.data.objects.new("ProjectionProofCamera",cam_data)
scene.collection.objects.link(cam)
scene.camera=cam
cam_data.type='PERSP'
fov=math.radians(50.0)
cam_data.angle=fov
cam_data.sensor_fit='HORIZONTAL'
cam_data.clip_start=0.01
cam_data.clip_end=100.0

def look_at(obj,target):
    direction=Vector(target)-obj.location
    obj.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()

# Exact reconstructed source camera: receiver was built in camera space and OBJ import
# maps its depth axis to Blender +Y and image vertical to Blender +Z.
cam.location=(0.0,0.0,0.0)
look_at(cam,(0.0,10.0,0.0))
base_rotation=cam.rotation_euler.copy()

# Far exact-reference plate. At canonical pose it maps the full reference exactly
# behind the depth receiver; on translation it becomes a disocclusion safety layer.
distance=24.0
aspect=scene.render.resolution_x/scene.render.resolution_y
half_w=distance*math.tan(fov*0.5)
half_h=half_w/aspect
verts=[
    (-half_w,distance,-half_h),
    ( half_w,distance,-half_h),
    ( half_w,distance, half_h),
    (-half_w,distance, half_h),
]
faces=[(0,1,2,3)]
mesh=bpy.data.meshes.new("CanonicalFarPlateMesh")
mesh.from_pydata(verts,[],faces)
mesh.update()
uv=mesh.uv_layers.new(name="UVMap")
# Loop order for the quad is 0,1,2,3.
uvs=[(0.0,0.0),(1.0,0.0),(1.0,1.0),(0.0,1.0)]
for poly in mesh.polygons:
    for li,vi in zip(poly.loop_indices,poly.vertices):
        uv.data[li].uv=uvs[vi]
far=bpy.data.objects.new("CanonicalFarProjectionPlate",mesh)
scene.collection.objects.link(far)
far.data.materials.append(mat)

corners=[receiver.matrix_world @ Vector(v) for v in receiver.bound_box]
mins=Vector((min(v.x for v in corners),min(v.y for v in corners),min(v.z for v in corners)))
maxs=Vector((max(v.x for v in corners),max(v.y for v in corners),max(v.z for v in corners)))

def render(name,xoff=0.0):
    cam.location=(xoff,0.0,0.0)
    cam.rotation_euler=base_rotation
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)

render("layered-base.png",0.0)
render("layered-left-small.png",-0.35)
render("layered-right-small.png",0.35)
render("layered-left-medium.png",-0.75)
render("layered-right-medium.png",0.75)

debug={
    "method":"depth receiver + exact far projection plate",
    "camera_orientation":"fixed",
    "camera_forward_world":"+Y",
    "receiver_bounds_min":list(mins),
    "receiver_bounds_max":list(maxs),
    "far_plate_distance":distance,
    "fov_deg":50.0,
    "aspect":aspect,
    "offsets":[0.0,-0.35,0.35,-0.75,0.75],
    "credits":0,
    "unity":False
}
(OUT/"layered-debug.json").write_text(json.dumps(debug,indent=2)+"\n")

bpy.ops.wm.save_as_mainfile(filepath=str(
    ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-projection-v1"/"ValoriaCameraFirstProjectionLayeredV2.blend"
))
