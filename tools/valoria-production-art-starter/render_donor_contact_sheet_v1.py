import bpy, os, math, json
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-donor-visual-v1")
os.makedirs(OUT,exist_ok=True)

ASSETS=[
 ("HeroBastion","Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb"),
 ("MidTier01","Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece01.glb"),
 ("MidTier02","Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece02.glb"),
 ("MidTier03","Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece03.glb"),
 ("MidTier04","Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece04.glb"),
 ("TowerWallRock","Unity/Assets/Eldoria/Resources/Valoria/Rescued/TowerWallRock.glb"),
 ("CornerWallL","Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb"),
 ("HighStraightWall","Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb"),
 ("RockToWallTransition","Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb"),
 ("Aserradero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb"),
 ("Cuartel","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb"),
 ("Granero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb"),
]

def bounds(objects):
    pts=[]
    for o in objects:
        if o.type!="MESH": continue
        for c in o.bound_box: pts.append(o.matrix_world @ Vector(c))
    xs=[p.x for p in pts]; ys=[p.y for p in pts]; zs=[p.z for p in pts]
    return Vector((min(xs),min(ys),min(zs))),Vector((max(xs),max(ys),max(zs)))

def setup_world():
    w=bpy.context.scene.world
    if w is None:
        w=bpy.data.worlds.new("Valoria Donor Review World")
        bpy.context.scene.world=w
    w.color=(0.018,0.022,0.030)
    w.use_nodes=True
    bg=w.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(0.025,0.035,0.055,1)
    bg.inputs["Strength"].default_value=.45

def ground(size=8):
    bpy.ops.mesh.primitive_plane_add(size=size, location=(0,0,0))
    p=bpy.context.object
    m=bpy.data.materials.new("Neutral Ground")
    m.diffuse_color=(.075,.085,.075,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(.075,.085,.075,1)
    bs.inputs["Roughness"].default_value=.96
    p.data.materials.append(m)

def light():
    bpy.ops.object.light_add(type="AREA", location=(4,-5,7))
    key=bpy.context.object;key.data.energy=1100;key.data.shape='DISK';key.data.size=5.5
    key.rotation_euler=(math.radians(25),0,math.radians(38))
    bpy.ops.object.light_add(type="AREA", location=(-4,-1,4))
    fill=bpy.context.object;fill.data.energy=550;fill.data.size=4.0
    bpy.ops.object.light_add(type="AREA", location=(0,4,5))
    rim=bpy.context.object;rim.data.energy=750;rim.data.size=3.0

def camera_for(lo,hi):
    center=(lo+hi)*.5
    extent=max((hi-lo).x,(hi-lo).y,(hi-lo).z)
    bpy.ops.object.camera_add()
    cam=bpy.context.object
    cam.data.type='ORTHO'
    cam.data.ortho_scale=max(extent*1.45,.5)
    cam.location=center+Vector((extent*1.9,-extent*2.3,extent*1.65))
    direction=center-cam.location
    cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
    bpy.context.scene.camera=cam

def render_asset(label,rel):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    setup_world()
    path=os.path.join(ROOT,rel)
    bpy.ops.import_scene.gltf(filepath=path)
    objs=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    lo,hi=bounds(objs)
    # put lowest point on ground and center X/Y
    delta=Vector((-(lo.x+hi.x)*.5,-(lo.y+hi.y)*.5,-lo.z))
    for o in objs:o.location+=delta
    lo,hi=bounds(objs)
    ground(max((hi-lo).x,(hi-lo).y)*5+1)
    light();camera_for(lo,hi)
    sc=bpy.context.scene
    sc.render.engine='BLENDER_EEVEE'
    sc.render.resolution_x=640;sc.render.resolution_y=640;sc.render.resolution_percentage=100
    sc.render.image_settings.file_format='PNG'
    sc.render.film_transparent=False
    sc.render.filepath=os.path.join(OUT,label+".png")
    sc.view_settings.look='AgX - Medium High Contrast'
    bpy.ops.render.render(write_still=True)
    return {"label":label,"path":rel,"png":os.path.relpath(sc.render.filepath,ROOT).replace("\\","/")}

report={"schema_version":1,"program":"VALORIA PRODUCTION ART SYSTEM RESET v1","renders":[]}
for label,rel in ASSETS:
    report["renders"].append(render_asset(label,rel))
with open(os.path.join(ROOT,"pipeline","evidence","valoria-production-art-donor-visual-v1.json"),"w",encoding="utf8") as f:
    json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
