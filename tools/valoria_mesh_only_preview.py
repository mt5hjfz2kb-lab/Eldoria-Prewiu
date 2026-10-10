import bpy, math, os, sys, json
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
OUT=Path(os.environ.get("VALORIA_PROOF_OUTPUT","/tmp/valoria-mesh-proof.png"))
OUT.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
sc=bpy.context.scene
sc.render.engine="CYCLES";sc.cycles.samples=16
# Ubuntu apt Blender is built without OpenImageDenoiser; avoid unsupported CPU denoising.
for layer in sc.view_layers: layer.cycles.use_denoising=False
sc.render.resolution_x=1120;sc.render.resolution_y=770;sc.render.resolution_percentage=100
sc.render.image_settings.file_format="PNG";sc.render.filepath=str(OUT)
sc.world.color=(.08,.095,.12)
cam_data=bpy.data.cameras.new("MatchedLayoutCamera");cam=bpy.data.objects.new("MatchedLayoutCamera",cam_data)
sc.collection.objects.link(cam);sc.camera=cam
cam.location=Vector((30.8183,63.0934,-84.6726))
look=(Vector((0,0,0))-cam.location)
cam.rotation_euler=look.to_track_quat("-Z","Y").to_euler()
cam_data.type="PERSP";cam_data.lens=35
cam_data.sensor_fit="VERTICAL"
cam_data.angle=math.radians(44.42281)
aspect=sc.render.resolution_x/sc.render.resolution_y
tan=math.tan(cam_data.angle*.5)
right=cam.rotation_euler.to_matrix()@Vector((1,0,0))
up=cam.rotation_euler.to_matrix()@Vector((0,1,0))
forward=cam.rotation_euler.to_matrix()@Vector((0,0,-1))
FAMILIES=[
("Bridge","bridge",.288618,.100592,.28,.22,36),
("LowerGate","lower-gate",.394309,.319527,.21,.225,42),
("MainRoad","road",.467480,.508876,.14,.25,48),
("CentralStair","stair",.534959,.673373,.126,.12,54),
("UpperWalls","wall",.788618,.801183,.51,.21,60),
("Bastion","bastion",.604878,.842604,.24,.21,61)
]
results=[]
for display,folder,u,v,w,h,depth in FAMILIES:
    basename={"MainRoad":"Road","CentralStair":"Stair","UpperWalls":"Wall","TerrainCliffSupport":"RockTerrain"}.get(display,display)
    src=ROOT/"art-source"/"valoria"/"production"/(folder+"-family-v1")/(basename+"FamilyV1.glb")
    if not src.exists():raise FileNotFoundError(src)
    before=set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(src))
    loaded=[o for o in bpy.data.objects if o not in before and o.type=="MESH"]
    if not loaded:raise RuntimeError("Empty family: "+display)
    parent=bpy.data.objects.new(display+"_PreviewRoot",None);sc.collection.objects.link(parent)
    for ob in loaded:
        mw=ob.matrix_world.copy();ob.parent=parent;ob.matrix_world=mw
    bpy.context.view_layer.update()
    coords=[ob.matrix_world@Vector(corner) for ob in loaded for corner in ob.bound_box]
    def extent(axis):
        values=[(p-cam.location).dot(axis) for p in coords]
        return max(values)-min(values)
    # Scale by actual camera-plane coordinates (not world axes).
    width=max(.001,extent(right))
    height=max(.001,extent(up))
    scale=min(w*2*depth*tan*aspect/width,h*2*depth*tan/height)
    if not math.isfinite(scale) or scale<=0:raise RuntimeError("Bad scale "+display)
    parent.scale=(scale,)*3
    bpy.context.view_layer.update()
    coords=[ob.matrix_world@Vector(corner) for ob in loaded for corner in ob.bound_box]
    xvals=[p.dot(right) for p in coords]
    yvals=[p.dot(up) for p in coords]
    zvals=[p.dot(forward) for p in coords]
    middle=right*((min(xvals)+max(xvals))*.5)+up*((min(yvals)+max(yvals))*.5)+forward*((min(zvals)+max(zvals))*.5)
    target=cam.location+forward*depth+right*((u-.5)*2*depth*tan*aspect)+up*((v-.5)*2*depth*tan)
    parent.location+=target-middle
    bpy.context.view_layer.update()
    results.append({"family":display,"mesh_count":len(loaded),"input":str(src.relative_to(ROOT)),"scale":scale})
world=sc.world
world.use_nodes=True
bg=world.node_tree.nodes.get("Background");bg.inputs["Color"].default_value=(.11,.14,.2,1)
bg.inputs["Strength"].default_value=.75
light_data=bpy.data.lights.new("SoftSun","SUN");light_data.energy=2
light=bpy.data.objects.new("SoftSun",light_data);sc.collection.objects.link(light)
light.rotation_euler=(math.radians(30),math.radians(-25),math.radians(10))
bpy.ops.render.render(write_still=True)
OUT.with_suffix(".json").write_text(json.dumps({"purpose":"independent Blender-preview, NOT Unity or commercial visual certification","families":results},indent=2))
# Persist a small review thumbnail as UTF-8 on the isolated branch.
# This enables independent visual inspection without binary-artifact API access.
import base64
thumbnail=bpy.data.images.load(str(OUT))
thumbnail.scale(420,289)
thumbnail.filepath_raw=str(OUT.with_name("valoria-mesh-review.jpg"))
thumbnail.file_format="JPEG"
thumbnail.save()
thumb=Path(thumbnail.filepath_raw)
review=ROOT/"docs/evidence/valoria-mesh-only-prototype"
review.mkdir(parents=True,exist_ok=True)
(review/"preview.jpg.base64.txt").write_text(base64.b64encode(thumb.read_bytes()).decode("ascii"))
print("MESH_ONLY_PREVIEW",OUT, "review_bytes",thumb.stat().st_size)
