import bpy, json
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
EVID=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-reset-target-v1"
ASSET=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-reset-target-v1"
REPORT=json.loads((EVID/"semantic-layer-report.json").read_text())

bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=700
scene.render.resolution_y=540
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.view_settings.view_transform='Standard'
scene.view_settings.exposure=0
scene.view_settings.gamma=1
world=bpy.data.worlds.new("World")
world.color=(0.01,0.01,0.01)
scene.world=world

cam_data=bpy.data.cameras.new("SemanticOrthographicCamera")
cam=bpy.data.objects.new("SemanticOrthographicCamera",cam_data)
scene.collection.objects.link(cam)
scene.camera=cam
cam_data.type='ORTHO'
cam_data.ortho_scale=2.0
cam_data.clip_start=0.01
cam_data.clip_end=100
cam.location=(0,0,0)
cam.rotation_euler=(Vector((0,10,0))-cam.location).to_track_quat('-Z','Y').to_euler()
base_rotation=cam.rotation_euler.copy()

aspect=700/540
half_h=1.0
half_w=half_h*aspect

def material(name,path):
    img=bpy.data.images.load(str(path))
    m=bpy.data.materials.new(name)
    m.use_nodes=True
    try:
        m.blend_method='CLIP'
        m.alpha_threshold=0.5
        m.shadow_method='NONE'
    except Exception:
        pass
    n=m.node_tree.nodes; l=m.node_tree.links; n.clear()
    out=n.new("ShaderNodeOutputMaterial")
    mix=n.new("ShaderNodeMixShader")
    trans=n.new("ShaderNodeBsdfTransparent")
    em=n.new("ShaderNodeEmission")
    tex=n.new("ShaderNodeTexImage")
    tex.image=img
    tex.interpolation='Closest'
    l.new(tex.outputs["Color"],em.inputs["Color"])
    l.new(tex.outputs["Alpha"],mix.inputs[0])
    l.new(trans.outputs[0],mix.inputs[1])
    l.new(em.outputs[0],mix.inputs[2])
    l.new(mix.outputs[0],out.inputs["Surface"])
    return m

def plane(name,y,mat):
    verts=[(-half_w,y,-half_h),(half_w,y,-half_h),(half_w,y,half_h),(-half_w,y,half_h)]
    mesh=bpy.data.meshes.new(name+"Mesh")
    mesh.from_pydata(verts,[],[(0,1,2,3)]); mesh.update()
    uv=mesh.uv_layers.new(name="UVMap")
    uvs=[(0,0),(1,0),(1,1),(0,1)]
    for poly in mesh.polygons:
        for li,vi in zip(poly.loop_indices,poly.vertices):
            uv.data[li].uv=uvs[vi]
    obj=bpy.data.objects.new(name,mesh)
    scene.collection.objects.link(obj)
    obj.data.materials.append(mat)
    return obj

# Convert measured normalized depth into ordered world-space plane depth.
stats={x["name"]:x for x in REPORT["semantic_layers"]}
def world_y(name):
    d=stats[name]["mean_depth_norm"]
    return 6.0+6.0*float(d)

names=["environment-far","environment-near","gate","bridge"]
objects={}
for name in names:
    y=world_y(name)
    objects[name]=plane(name,y,material(name,ASSET/f"semantic-{name}.png"))

def render(name,x=0.0,scale=2.0):
    cam.location=(x,0,0)
    cam.rotation_euler=base_rotation
    cam.data.ortho_scale=scale
    scene.render.filepath=str(EVID/name)
    bpy.ops.render.render(write_still=True)

render("semantic-base.png")
render("semantic-pan-left.png",-0.18)
render("semantic-pan-right.png",0.18)
render("semantic-zoom-in.png",0,1.6)
render("semantic-zoom-out.png",0,2.4)

# Occlusion probes at the center of the Gate prompt.
bx=REPORT["prompts"]["Lower gate"]["box_local"]
px=(bx[0]+bx[2])*0.5
py=(bx[1]+bx[3])*0.5
u=px/700.0
v=1.0-py/540.0
wx=(u*2-1)*half_w
wz=(v*2-1)*half_h

def sphere(name,y,color):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, radius=0.055, location=(wx,y,wz))
    o=bpy.context.object; o.name=name
    m=bpy.data.materials.new(name+"Mat"); m.use_nodes=True
    p=m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*color,1)
    p.inputs["Emission Color"].default_value=(*color,1)
    p.inputs["Emission Strength"].default_value=2
    o.data.materials.append(m)

# Green = in front of all; magenta = mid-depth; cyan = behind all.
sphere("FrontProbe",5.0,(0,1,0))
sphere("MidProbe",9.0,(1,0,1))
sphere("BackProbe",13.0,(0,1,1))
render("semantic-occlusion-debug.png")

out={
    "semantic_world_y":{n:world_y(n) for n in names},
    "gate_probe_screen_px":[px,py],
    "probe_depths":{"front":5.0,"mid":9.0,"back":13.0},
    "camera":"orthographic fixed orientation",
    "credits":0,
    "unity":False
}
(EVID/"semantic-render-report.json").write_text(json.dumps(out,indent=2)+"\n")
bpy.ops.wm.save_as_mainfile(filepath=str(ASSET/"ValoriaSemanticProjectionV1.blend"))
