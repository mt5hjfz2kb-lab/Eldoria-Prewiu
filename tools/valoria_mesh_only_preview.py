import bpy, math, os, sys, json
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
OUT=Path(os.environ.get("VALORIA_PROOF_OUTPUT","/tmp/valoria-mesh-proof.png"))
OUT.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
sc=bpy.context.scene
sc.render.engine="CYCLES";sc.cycles.samples=16
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
("Bastion","bastion",.604878,.842604,.24,.21,61),
("TerrainCliffSupport","rock-terrain",.72,.36,.90,.60,65)]
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
    lo=Vector(tuple(min(c[i] for c in coords) for i in range(3)))
    hi=Vector(tuple(max(c[i] for c in coords) for i in range(3)))
    center=(lo+hi)*.5
    size=hi-lo
    scale=min(w*2*depth*tan*aspect/max(.001,size.x),h*2*depth*tan/max(.001,size.z))
    parent.scale=(scale,)*3
    bpy.context.view_layer.update()
    coords=[ob.matrix_world@Vector(corner) for ob in loaded for corner in ob.bound_box]
    center=sum(coords,Vector())/len(coords)
    target=cam.location+forward*depth+right*((u-.5)*2*depth*tan*aspect)+up*((v-.5)*2*depth*tan)
    parent.location+=target-center
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
print("MESH_ONLY_PREVIEW",OUT)
