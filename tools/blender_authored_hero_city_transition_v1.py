import argparse, json, math, os, sys
import bpy
from mathutils import Vector

def args():
    argv=sys.argv
    argv=argv[argv.index("--")+1:] if "--" in argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--output",required=True)
    p.add_argument("--report",required=True)
    return p.parse_args(argv)

def activate(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj

def rounded_box(name, loc, dims, bevel=.30):
    bpy.ops.mesh.primitive_cube_add(location=loc)
    o=bpy.context.object
    o.name=name
    o.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel>0:
        m=o.modifiers.new(name+" bevel","BEVEL")
        m.width=bevel
        m.segments=3
        m.limit_method='ANGLE'
        activate(o)
        bpy.ops.object.modifier_apply(modifier=m.name)
    return o

def rock(name, loc, scale, subdivisions=2):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions,radius=1.0,location=loc)
    o=bpy.context.object
    o.name=name
    o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def join(objects,name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join()
    objects[0].name=name
    return objects[0]

def smart_uv(obj):
    activate(obj)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(66),island_margin=.025)
    bpy.ops.object.mode_set(mode='OBJECT')

a=args()
bpy.ops.wm.read_factory_settings(use_empty=True)

# CAMERA-AUTHORED MASS, not a sampled height field:
# four overlapping inhabited rock benches converge toward the Bastion,
# with asymmetric shoulders and deep buttresses. They are remeshed into one solid.
parts=[]
parts += [
    rounded_box("lower inhabited bench",(-.15,-3.90,-.35),(14.8,4.2,1.45),.55),
    rounded_box("middle civic bench",(.10,-.75,.65),(12.4,3.7,1.55),.48),
    rounded_box("upper district bench",(-.05,2.15,1.65),(9.8,3.4,1.70),.44),
    rounded_box("hero connector",(.0,4.75,2.55),(7.2,3.0,2.05),.40),
]
# Rock shoulders and vertical buttresses intentionally break rectangular silhouettes.
shoulders=[
    (-7.0,-4.6,-.70,(2.7,2.8,2.25)),(6.8,-4.5,-.72,(2.9,2.6,2.35)),
    (-6.15,-1.5,.08,(2.7,3.0,2.65)),(6.0,-1.25,.15,(2.9,2.9,2.55)),
    (-5.15,1.65,1.05,(2.5,2.7,2.75)),(5.05,1.75,1.10,(2.6,2.8,2.70)),
    (-3.85,4.20,2.02,(2.4,2.5,2.85)),(3.85,4.25,2.05,(2.45,2.5,2.9)),
    (-7.35,-2.9,-1.0,(1.65,2.15,3.1)),(7.25,-2.7,-1.05,(1.75,2.2,3.2)),
    (-5.55,.35,-.05,(1.65,2.0,3.25)),(5.55,.5,-.05,(1.7,2.0,3.15)),
]
for i,(x,y,z,sc) in enumerate(shoulders):
    parts.append(rock("rock shoulder %02d"%i,(x,y,z),sc,2))

mass=join(parts,"HeroCityRockMass")
activate(mass)
# Sculpt-like voxel union creates one continuous geological body rather than stacked decks.
mass.data.remesh_voxel_size=.16
mass.data.remesh_voxel_adaptivity=.12
bpy.ops.object.voxel_remesh()
for p in mass.data.polygons:p.use_smooth=True

# Add restrained geological breakup after union.
tex=bpy.data.textures.new("strata breakup",type='CLOUDS')
tex.noise_scale=.55
tex.noise_depth=2
disp=mass.modifiers.new("rock strata breakup","DISPLACE")
disp.texture=tex
disp.strength=.11
disp.texture_coords='GLOBAL'
activate(mass)
bpy.ops.object.modifier_apply(modifier=disp.name)
smooth=mass.modifiers.new("macro smoothing","LAPLACIANSMOOTH")
smooth.iterations=2
smooth.lambda_factor=.16
bpy.ops.object.modifier_apply(modifier=smooth.name)
dec=mass.modifiers.new("production decimate","DECIMATE")
dec.ratio=.62
bpy.ops.object.modifier_apply(modifier=dec.name)
smart_uv(mass)

# Central civic circulation: broad landings and short stair flights, slightly dog-legged.
route=[]
route_spec=[
    ((0,-5.25,.48),(3.2,2.0,.18)),
    ((-.25,-3.65,.74),(2.85,1.45,.18)),
    ((.15,-2.25,1.03),(2.70,1.35,.18)),
    ((-.18,-.95,1.30),(2.55,1.25,.18)),
    ((.18,.25,1.58),(2.40,1.15,.18)),
    ((-.12,1.38,1.86),(2.25,1.08,.18)),
    ((.10,2.42,2.15),(2.15,1.00,.18)),
    ((0,3.38,2.45),(2.05,.90,.18)),
    ((0,4.20,2.76),(1.95,.82,.18)),
]
for i,(loc,dims) in enumerate(route_spec):
    route.append(rounded_box("route step %02d"%i,loc,dims,.06))
route_obj=join(route,"CivicRoute")
smart_uv(route_obj)

# Three continuous retaining masonry bands; side buttresses make them structural.
walls=[]
for idx,(y,z,w) in enumerate([(-2.10,.54,11.7),(.82,1.51,9.4),(3.25,2.42,7.3)]):
    walls.append(rounded_box("retaining wall %d"%idx,(0,y,z),(w,.32,.90),.08))
    for sx in (-1,1):
        x=sx*(w*.42)
        walls.append(rounded_box("retaining buttress %d %d"%(idx,sx),(x,y-.18,z-.18),(.58,.85,1.28),.10))
wall_obj=join(walls,"RetainingMasonry")
smart_uv(wall_obj)

# One hero landing closes the final visual gap below the Bastion entrance.
landing=rounded_box("HeroLanding",(0,5.12,3.05),(4.9,1.20,.28),.08)
smart_uv(landing)

# Placeholder materials preserve submesh identities for Unity renderer routing.
for obj,label,color in [
    (mass,"Rock",(0.30,.28,.25,1)),
    (route_obj,"Stone",(0.42,.40,.36,1)),
    (wall_obj,"Stone",(0.40,.38,.34,1)),
    (landing,"Stone",(0.44,.42,.38,1)),
]:
    mat=bpy.data.materials.new("Eldoria_"+label)
    mat.diffuse_color=color
    obj.data.materials.append(mat)

# Ensure normals/tangents are stable.
for obj in (mass,route_obj,wall_obj,landing):
    activate(obj)
    bpy.ops.object.shade_smooth_by_angle() if hasattr(bpy.ops.object,'shade_smooth_by_angle') else None

os.makedirs(os.path.dirname(a.output),exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(filepath=a.output,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

triangles={}
total=0
for obj in (mass,route_obj,wall_obj,landing):
    obj.data.calc_loop_triangles()
    triangles[obj.name]=len(obj.data.loop_triangles)
    total+=triangles[obj.name]
report={
    "method":"camera-authored voxel-unified Hero-to-city rock section with integrated retaining bands and civic circulation; no heightfield/grid terrain",
    "objects":[o.name for o in (mass,route_obj,wall_obj,landing)],
    "triangles":triangles,
    "total_triangles":total,
    "output":a.output,
    "bytes":os.path.getsize(a.output),
    "tripo_credits":0
}
with open(a.report,'w',encoding='utf-8') as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
