import argparse, json, math, os, sys
import bpy
from mathutils import Vector

def parse_args():
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

def bounds(obj):
    pts=[obj.matrix_world @ Vector(c) for c in obj.bound_box]
    mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return mn,mx

def unity_to_blender(v):
    x,y,z=v
    return Vector((x,-z,y))

def import_fbx(path,name):
    before=set(bpy.context.scene.objects)
    bpy.ops.wm.fbx_import(filepath=path)
    imported=[o for o in bpy.context.scene.objects if o not in before and o.type=='MESH']
    if not imported:
        raise RuntimeError("No mesh imported from "+path)
    if len(imported)>1:
        bpy.ops.object.select_all(action='DESELECT')
        for o in imported:o.select_set(True)
        bpy.context.view_layer.objects.active=imported[0]
        bpy.ops.object.join()
    o=imported[0]
    o.name=name
    activate(o)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def normalized_copy(source,name,unity_center,unity_dims,yaw=0,pitch=0,roll=0):
    o=source.copy()
    o.data=source.data.copy()
    bpy.context.scene.collection.objects.link(o)
    o.name=name
    mn,mx=bounds(o)
    size=mx-mn
    o.scale=(unity_dims[0]/max(size.x,.001),unity_dims[2]/max(size.y,.001),unity_dims[1]/max(size.z,.001))
    o.rotation_euler=(math.radians(pitch),math.radians(roll),math.radians(-yaw))
    o.location=unity_to_blender(unity_center)
    activate(o)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def rounded_box(name,unity_center,unity_dims,bevel=.18):
    bpy.ops.mesh.primitive_cube_add(location=unity_to_blender(unity_center))
    o=bpy.context.object
    o.name=name
    o.dimensions=(unity_dims[0],unity_dims[2],unity_dims[1])
    activate(o)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        m=o.modifiers.new(name+" bevel","BEVEL")
        m.width=bevel;m.segments=3;m.limit_method='ANGLE'
        bpy.ops.object.modifier_apply(modifier=m.name)
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
    bpy.ops.uv.smart_project(angle_limit=math.radians(60),island_margin=.02)
    bpy.ops.object.mode_set(mode='OBJECT')

def irregular_slab(name,unity_center,width,depth,thickness,cut=.75):
    x=width*.5;z=depth*.5;y=thickness*.5
    pts=[
      (-x+cut,-y,-z),(x-cut,-y,-z),(x,-y,-z+cut),(x,-y,z-cut),
      (x-cut,-y,z),(-x+cut,-y,z),(-x,-y,z-cut),(-x,-y,-z+cut),
      (-x+cut,y,-z),(x-cut,y,-z),(x,y,-z+cut),(x,y,z-cut),
      (x-cut,y,z),(-x+cut,y,z),(-x,y,z-cut),(-x,y,-z+cut)
    ]
    verts=[unity_to_blender((px+unity_center[0],py+unity_center[1],pz+unity_center[2])) for px,py,pz in pts]
    faces=[(0,1,2,3,4,5,6,7),(8,15,14,13,12,11,10,9)]
    for i in range(8):faces.append((i,(i+1)%8,(i+1)%8+8,i+8))
    me=bpy.data.meshes.new(name+"Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.scene.collection.objects.link(o)
    bev=o.modifiers.new(name+" edge","BEVEL");bev.width=.08;bev.segments=2
    activate(o);bpy.ops.object.modifier_apply(modifier=bev.name)
    return o

a=parse_args()
bpy.ops.wm.read_factory_settings(use_empty=True)
workspace=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
mesh_dir=os.path.join(workspace,"Unity","Assets","EmaceArt","Slavic World Free","Meshes")
sources=[
    import_fbx(os.path.join(mesh_dir,"EA03_Environment_Rock_Head_01a.fbx"),"SourceRockA"),
    import_fbx(os.path.join(mesh_dir,"EA03_Environment_Rock_Head_01b.fbx"),"SourceRockB"),
    import_fbx(os.path.join(mesh_dir,"EA03_Environment_Rock_CatHead_01a.fbx"),"SourceRockC"),
    import_fbx(os.path.join(mesh_dir,"EA03_Environment_Rock_Flat_04c.fbx"),"SourceRockD"),
]
for s in sources:s.hide_render=True;s.hide_viewport=True

placements=[
    (0,(-5.6,-.25,-4.0),(5.2,2.5,4.1),18,0,3),
    (1,(-2.0,-.10,-4.1),(5.3,2.3,4.2),-12,4,0),
    (2,(2.0,-.08,-4.0),(5.2,2.4,4.2),14,-3,2),
    (3,(5.6,-.22,-3.8),(5.1,2.5,4.0),-20,2,-2),
    (1,(-5.0,.65,-.7),(4.6,2.3,3.6),28,0,4),
    (2,(-1.7,.75,-.5),(4.5,2.2,3.5),-15,3,0),
    (0,(1.7,.76,-.45),(4.5,2.2,3.5),16,-3,0),
    (3,(5.0,.62,-.6),(4.6,2.3,3.6),-30,0,-4),
    (2,(-4.0,1.35,2.6),(4.0,2.4,3.2),32,2,4),
    (0,(-1.3,1.48,2.9),(4.0,2.3,3.2),-8,-2,0),
    (1,(1.5,1.48,2.9),(4.0,2.3,3.2),10,3,0),
    (3,(4.1,1.36,2.7),(4.0,2.4,3.2),-34,-2,-4),
    (1,(-3.0,1.80,5.0),(3.6,2.3,2.9),25,0,3),
    (2,(0.0,1.92,5.25),(4.0,2.5,3.0),0,-2,0),
    (0,(3.0,1.82,5.0),(3.6,2.3,2.9),-25,0,-3),
]
rocks=[]
for i,(src,c,d,yaw,pitch,roll) in enumerate(placements):
    rocks.append(normalized_copy(sources[src],"EnvelopeRock%02d"%i,c,d,yaw,pitch,roll))

bridges=[
    rounded_box("BridgeLower",(0,-.45,-3.7),(10.0,1.2,3.2),.35),
    rounded_box("BridgeMiddle",(0,.45,-.5),(8.8,1.2,2.8),.32),
    rounded_box("BridgeUpper",(0,1.05,2.8),(7.0,1.15,2.6),.30),
    rounded_box("BridgeHero",(0,1.35,5.0),(5.2,1.05,2.1),.28),
]
mass=join(rocks+bridges,"RockEnvelope")
activate(mass)
mass.data.remesh_voxel_size=.095
mass.data.remesh_voxel_adaptivity=.06
bpy.ops.object.voxel_remesh()
for p in mass.data.polygons:p.use_smooth=True
dec=mass.modifiers.new("EnvelopeDecimate","DECIMATE");dec.ratio=.58
bpy.ops.object.modifier_apply(modifier=dec.name)
smart_uv(mass)

terraces=[
    irregular_slab("TerraceLower",(0,.18,-3.55),12.0,3.4,.18,1.05),
    irregular_slab("TerraceMiddle",(0,1.10,-.15),9.4,2.8,.16,.90),
    irregular_slab("TerraceUpper",(0,1.92,3.15),7.1,2.45,.15,.72),
]
terrace_obj=join(terraces,"TerraceStone");smart_uv(terrace_obj)

walls=[]
for level,(uz,uy,half_w) in enumerate([(-1.95,.55,5.15),(1.55,1.42,4.05),(4.35,2.20,2.95)]):
    for side in (-1,1):
        cx=side*(half_w*.58)
        walls.append(rounded_box("Wall_%d_%d"%(level,side),(cx,uy,uz),(half_w*.78,.92,.34),.07))
        walls.append(rounded_box("Buttress_%d_%d"%(level,side),(side*(half_w*.92),uy-.12,uz-.18),(.48,1.18,.74),.08))
wall_obj=join(walls,"RetainingMasonry");smart_uv(wall_obj)

steps=[]
n=14
for i in range(n):
    t=i/(n-1)
    uz=-4.65+9.55*t
    uy=.27+2.08*(t*t*(3-2*t))
    ux=.22*math.sin(t*math.pi*2.2)
    steps.append(rounded_box("Step_%02d"%i,(ux,uy,uz),(1.55,.13,.78),.035))
step_obj=join(steps,"CivicSteps");smart_uv(step_obj)
landing=irregular_slab("HeroLanding",(0,2.45,5.45),3.9,1.45,.18,.42)
smart_uv(landing)

for obj,label,color in [
    (mass,"Rock",(0.31,.29,.26,1)),
    (terrace_obj,"Ground",(0.38,.35,.30,1)),
    (wall_obj,"Stone",(0.42,.40,.36,1)),
    (step_obj,"Stone",(0.44,.42,.38,1)),
    (landing,"Stone",(0.44,.42,.38,1)),
]:
    mat=bpy.data.materials.new("Eldoria_"+label);mat.diffuse_color=color;obj.data.materials.append(mat)

for s in sources:bpy.data.objects.remove(s,do_unlink=True)

os.makedirs(os.path.dirname(a.output),exist_ok=True)
bpy.ops.object.select_all(action='DESELECT')
out_objects=[mass,terrace_obj,wall_obj,step_obj,landing]
for o in out_objects:o.select_set(True)
bpy.context.view_layer.objects.active=mass
bpy.ops.export_scene.gltf(filepath=a.output,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

triangles={}
for o in out_objects:
    o.data.calc_loop_triangles();triangles[o.name]=len(o.data.loop_triangles)
report={
 "method":"existing Slavic rock kitbash -> watertight voxel union; direct real-frame coordinates; embedded irregular terraces, retaining masonry and narrow civic steps",
 "objects":[o.name for o in out_objects],"triangles":triangles,
 "total_triangles":sum(triangles.values()),"output":a.output,
 "bytes":os.path.getsize(a.output),"tripo_credits":0
}
with open(a.report,'w',encoding='utf-8') as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
