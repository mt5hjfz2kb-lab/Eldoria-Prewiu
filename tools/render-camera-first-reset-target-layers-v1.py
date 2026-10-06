import bpy, math, json
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
ASSET=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-reset-target-v1"
OUT=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-reset-target-v1"
OUT.mkdir(parents=True,exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=700
scene.render.resolution_y=540
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.view_settings.view_transform='Standard'
try:
    scene.view_settings.look='None'
except Exception:
    pass
scene.view_settings.exposure=0
scene.view_settings.gamma=1
world=bpy.data.worlds.new("World")
world.color=(0.01,0.01,0.01)
scene.world=world

cam_data=bpy.data.cameras.new("ResetTargetOrthographicCamera")
cam=bpy.data.objects.new("ResetTargetOrthographicCamera",cam_data)
scene.collection.objects.link(cam)
scene.camera=cam
cam_data.type='ORTHO'
cam_data.ortho_scale=2.0
cam_data.clip_start=0.01
cam_data.clip_end=100

# Camera looks +Y, with +Z screen-up. Orthographic fixed orientation is the reset contract.
cam.location=(0,0,0)
direction=Vector((0,10,0))-cam.location
cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
base_rotation=cam.rotation_euler.copy()

aspect=scene.render.resolution_x/scene.render.resolution_y
half_h=1.0
half_w=half_h*aspect

def layer_material(name,path):
    img=bpy.data.images.load(str(path))
    mat=bpy.data.materials.new(name)
    mat.use_nodes=True
    try:
        mat.blend_method='CLIP'
        mat.alpha_threshold=0.5
        mat.shadow_method='NONE'
    except Exception:
        pass
    nodes=mat.node_tree.nodes
    links=mat.node_tree.links
    nodes.clear()
    out=nodes.new("ShaderNodeOutputMaterial")
    mix=nodes.new("ShaderNodeMixShader")
    trans=nodes.new("ShaderNodeBsdfTransparent")
    em=nodes.new("ShaderNodeEmission")
    tex=nodes.new("ShaderNodeTexImage")
    tex.image=img
    tex.interpolation='Closest'
    links.new(tex.outputs["Color"],em.inputs["Color"])
    links.new(tex.outputs["Alpha"],mix.inputs[0])
    links.new(trans.outputs[0],mix.inputs[1])
    links.new(em.outputs[0],mix.inputs[2])
    links.new(mix.outputs[0],out.inputs["Surface"])
    return mat

def add_plane(name,y,mat):
    verts=[(-half_w,y,-half_h),(half_w,y,-half_h),(half_w,y,half_h),(-half_w,y,half_h)]
    faces=[(0,1,2,3)]
    mesh=bpy.data.meshes.new(name+"Mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    uv=mesh.uv_layers.new(name="UVMap")
    uvs=[(0,0),(1,0),(1,1),(0,1)]
    for poly in mesh.polygons:
        for li,vi in zip(poly.loop_indices,poly.vertices):
            uv.data[li].uv=uvs[vi]
    obj=bpy.data.objects.new(name,mesh)
    scene.collection.objects.link(obj)
    obj.data.materials.append(mat)
    return obj

# Far to near, then alpha layers composite into the original crop.
depth_positions=[6.0,8.0,10.0,12.0]  # DA3 low normalized depth = near; camera looks +Y, so smaller Y is nearer.
for band in reversed(range(4)):
    mat=layer_material(f"Layer{band}Mat",ASSET/f"layer-{band}.png")
    add_plane(f"DepthLayer{band}",depth_positions[band],mat)

def render(name,x=0.0,z=0.0,scale=2.0):
    cam.location=(x,0.0,z)
    cam.rotation_euler=base_rotation
    cam.data.ortho_scale=scale
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)

# Base, bounded pan, and zoom diagnostics. No camera rotation.
render("reset-layered-base.png",0,0,2.0)
render("reset-layered-pan-left.png",-0.18,0,2.0)
render("reset-layered-pan-right.png",0.18,0,2.0)
render("reset-layered-zoom-in.png",0,0,1.6)
render("reset-layered-zoom-out.png",0,0,2.4)

# Occlusion diagnostic: a magenta sphere lies between layer1 (nearer) and layer2 (farther).
# Near image content (layers 0/1) must occlude it; far content (layers 2/3) must remain behind it.
bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, radius=0.08, location=(0.0,9.0,0.0))
sphere=bpy.context.object
sphere.name="DynamicDepthProbe"
sm=bpy.data.materials.new("DynamicDepthProbeMat")
sm.diffuse_color=(1,0,1,1)
sm.use_nodes=True
pn=sm.node_tree.nodes.get("Principled BSDF")
pn.inputs["Base Color"].default_value=(1,0,1,1)
pn.inputs["Emission Color"].default_value=(1,0,1,1)
pn.inputs["Emission Strength"].default_value=2.0
sphere.data.materials.append(sm)
render("reset-layered-occlusion-debug.png",0,0,2.0)

report={
  "camera":"orthographic fixed orientation",
  "layer_depth_positions":depth_positions,
  "base_ortho_scale":2.0,
  "pan_offsets":[-0.18,0.18],
  "zoom_scales":[1.6,2.0,2.4],
  "credits":0,
  "unity":False
}
(OUT/"reset-layered-render-report.json").write_text(json.dumps(report,indent=2)+"\n")
bpy.ops.wm.save_as_mainfile(filepath=str(ASSET/"ValoriaResetTargetLayeredProjectionV1.blend"))
