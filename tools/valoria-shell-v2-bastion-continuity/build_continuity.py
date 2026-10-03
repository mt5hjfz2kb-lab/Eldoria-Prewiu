"""Valoria Shell v2 — fixed-camera DCC set v1.
The whole player-facing set is composed in Blender around the official camera: Hero Bastion,
functional buildings, inhabited district, terrain/supports and circulation. Unity retains gameplay
authority but does not re-assemble this visual candidate from independent presentation layers.
"""
import bpy, os, sys, json, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
VAL=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria")
idx=sys.argv.index("--") if "--" in sys.argv else -1
OUT=sys.argv[idx+1] if idx>=0 else "ValoriaBastionContinuity.glb"
REPORT=sys.argv[idx+2] if idx>=0 and len(sys.argv)>idx+2 else OUT+".json"

def ub(v):
    x,y,z=v
    return Vector((x,-z,y))

def bounds(objects):
    pts=[]
    if not isinstance(objects,(list,tuple)):objects=[objects]
    for o in objects:
        if o.type!="MESH":continue
        pts.extend([o.matrix_world@Vector(c) for c in o.bound_box])
    return Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts))),Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))

def import_join(rel,name):
    path=os.path.join(VAL,rel)
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=path)
    meshes=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
    if not meshes:raise RuntimeError("No meshes: "+rel)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0]
    if len(meshes)>1:bpy.ops.object.join()
    o=bpy.context.view_layer.objects.active;o.name=name
    return o

def fit_center(o,center,dims,yaw=0):
    o.rotation_euler[2]=math.radians(-yaw)
    bpy.context.view_layer.update()
    lo,hi=bounds(o);s=hi-lo
    o.scale=(dims[0]/max(s.x,.001),dims[2]/max(s.y,.001),dims[1]/max(s.z,.001))
    bpy.context.view_layer.update()
    lo,hi=bounds(o);c=(lo+hi)*.5
    o.location+=ub(center)-c
    bpy.context.view_layer.update()
    return o

def add_asset(rel,name,center,dims,yaw=0):
    return fit_center(import_join(rel,name),center,dims,yaw)

def ground_mesh(name,z0,z1,y,half0,half1):
    # One irregular inhabited terrace, full width, with a sloped/rock front instead of a floating board.
    xs=13;verts=[];faces=[]
    for row,(z,half,yy) in enumerate([(z0,half0,y-.36),(z0+.42,half0-.25,y),(z1,half1,y)]):
        for i in range(xs):
            t=i/(xs-1);x=-half+2*half*t
            edge=abs(t-.5)*2
            x+=.16*math.sin(i*1.37+row*.9)*edge
            verts.append(tuple(ub((x,yy,z))))
    for r in range(2):
        for i in range(xs-1):
            a=r*xs+i;b=a+1;c=a+xs;d=c+1
            faces += [(a,d,c,b)]
    me=bpy.data.meshes.new(name+"Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.scene.collection.objects.link(o)
    return o

bpy.ops.wm.read_factory_settings(use_empty=True)
objects=[]

# Exact focal: same visible bounds as the certified Unity Hero Bastion renderer audit.
objects.append(add_asset("HeroBastionGenerated/Valoria_HeroBastion_v1.glb","DCC_HeroBastion",
    (0,7.14,8.75),(12.80,9.24,10.96),0))

# Terrain is authored around the architecture, not added underneath afterwards.
tiers=[
    ground_mesh("DCC_LowerTerrace",-7.5,-2.35,-.15,11.7,9.6),
    ground_mesh("DCC_MiddleTerrace",-2.75,1.85,.85,9.8,7.7),
    ground_mesh("DCC_HeroTerrace",1.45,5.25,2.22,7.8,5.7)
]
objects += tiers

# Functional anchors are composed into the lower/middle retaining sequence.
objects += [
 add_asset("Valoria_Aserradero_AP2_v1.glb","DCC_Aserradero",(-6.25,1.65,-1.15),(4.7,3.3,4.1),18),
 add_asset("Valoria_Cuartel_AP2_v1.glb","DCC_Cuartel",(6.10,1.70,-.95),(5.0,3.4,4.3),342),
 add_asset("Valoria_Granero_BIII_v1.glb","DCC_Granero",(-2.55,2.15,2.05),(3.1,2.7,3.25),12)
]

# Dense inhabited district. Deliberate overlap/burial makes one roofline rather than isolated plots.
mid=[
 ("Piece01",(-7.65,.95,-4.65),(3.0,2.45,3.0),20),
 ("Piece04",(-4.55,1.05,-4.40),(2.7,2.45,2.8),10),
 ("Piece02",(-1.35,1.00,-4.70),(2.7,2.45,2.8),5),
 ("Piece03",(2.00,1.05,-4.60),(2.8,2.55,2.9),355),
 ("Piece01",(5.25,1.00,-4.25),(2.8,2.45,2.9),346),
 ("Piece04",(7.85,.95,-4.40),(2.8,2.45,2.9),338),
 ("Piece03",(-5.15,1.85,.25),(2.8,2.65,2.9),18),
 ("Piece01",(1.45,1.82,.50),(2.65,2.55,2.75),350),
 ("Piece02",(4.70,1.82,.25),(2.75,2.65,2.85),342)
]
for i,(piece,c,d,yaw) in enumerate(mid):
    objects.append(add_asset("MidTierArchitectureKit_v1/"+piece+".glb","DCC_Mid_%02d"%i,c,d,yaw))

# Structural wall vocabulary follows the same three terrain levels and visually locks architecture to rock.
walls=[
 ("HighStraightWall",(-5.4,.55,-2.15),(4.0,1.45,.72),5),
 ("HighStraightWall",(5.4,.55,-2.10),(4.0,1.45,.72),175),
 ("CornerWallL",(-8.25,.62,-1.45),(2.5,1.8,2.0),96),
 ("CornerWallL",(8.20,.62,-1.40),(2.5,1.8,2.0),264),
 ("RockToWallTransition",(-4.65,1.55,1.95),(3.0,2.0,2.0),30),
 ("RockToWallTransition",(4.65,1.55,2.00),(3.0,2.0,2.0),210),
 ("HighStraightWall",(-3.55,2.18,4.55),(2.8,1.65,.72),8),
 ("HighStraightWall",(3.55,2.18,4.55),(2.8,1.65,.72),172)
]
for i,(piece,c,d,yaw) in enumerate(walls):
    objects.append(add_asset("StoneArchitectureKit_v1/"+piece+".glb","DCC_Wall_%02d"%i,c,d,yaw))

# Existing certified rock modules are seam cover, never the substrate.
rocks=[
 ("ResidentialTerraceRock",(-8.55,-.15,-2.8),(5.2,2.7,4.0),30),
 ("ResidentialTerraceRock",(8.45,-.12,-2.7),(5.2,2.7,4.0),210),
 ("TerraceStairRock",(-6.8,.80,1.6),(4.4,2.6,3.4),35),
 ("TerraceStairRock",(6.8,.82,1.7),(4.4,2.6,3.4),215),
 ("StreetLandingTransition",(-4.45,1.75,4.5),(3.8,2.3,3.0),32),
 ("StreetLandingTransition",(4.45,1.78,4.55),(3.8,2.3,3.0),212)
]
for i,(piece,c,d,yaw) in enumerate(rocks):
    objects.append(add_asset("Rescued/"+piece+".glb","DCC_Rock_%02d"%i,c,d,yaw))

# Camera-authored central circulation: broad landings linked by short flights; no continuous ribbon.
stone=bpy.data.materials.new("DCC_Stone");stone.diffuse_color=(.40,.38,.34,1)
def box(name,c,d):
    bpy.ops.mesh.primitive_cube_add(size=1,location=ub(c));o=bpy.context.object;o.name=name
    o.dimensions=(d[0],d[2],d[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    be=o.modifiers.new("edge","BEVEL");be.width=.055;be.segments=2;bpy.ops.object.modifier_apply(modifier=be.name)
    o.data.materials.append(stone);return o
for i,(z,y,w) in enumerate([(-5.7,.02,2.8),(-2.1,.78,2.55),(1.45,1.72,2.4),(4.75,2.55,2.25)]):
    objects.append(box("DCC_Landing_%d"%i,(0,y,z),(w,.14,1.25)))
for i in range(12):
    t=i/11;z=-5.0+9.25*t;y=.18+2.15*t
    objects.append(box("DCC_Step_%02d"%i,(0,y,z),(1.42,.13,.62)))

# Export the authored visual set as one scene root; source materials on real assets stay intact.
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=objects[0]
os.makedirs(os.path.dirname(OUT),exist_ok=True)
bpy.ops.export_scene.gltf(filepath=OUT,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

tri={}
for o in objects:
    if o.type=="MESH":o.data.calc_loop_triangles();tri[o.name]=len(o.data.loop_triangles)
with open(REPORT,"w",encoding="utf-8") as f:
    json.dump({
      "method":"fixed-camera complete DCC set assembly",
      "focal_bounds_unity":{"center":[0,7.14,8.75],"size":[12.8,9.24,10.96]},
      "includes":["Hero Bastion","Aserradero","Cuartel","Granero","MidTier Architecture","Stone Architecture","terrain terraces","rock seams","circulation"],
      "objects":len(objects),"total_triangles":sum(tri.values()),"triangles":tri,
      "output_bytes":os.path.getsize(OUT),"tripo_credits":0
    },f,indent=2)
print("VALORIA_DCC_SET_BUILT",len(objects),sum(tri.values()))
