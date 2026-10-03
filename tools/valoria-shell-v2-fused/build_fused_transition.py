"""Build a camera-authored fused Hero-to-city rock foundation from certified Eldoria support meshes.
This is deliberately NOT a terrain heightfield/grid. It imports existing certified rock/terrace
assets, overlaps them into the actual fixed-camera footprint, then voxel-remeshes the union into
one manifold visual-only shell. Gameplay geometry remains in Unity and is never exported here.
"""
import bpy, math, os, sys, json
from mathutils import Vector

ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),"..",".."))
RES=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","Rescued")

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

def ub(x,y,z):
    return Vector((x,-z,y))

def world_bounds(obj):
    pts=[obj.matrix_world @ Vector(c) for c in obj.bound_box]
    lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return lo,hi

def import_support(filename,name,anchor,span,height,yaw):
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(RES,filename))
    new=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
    if not new:
        raise RuntimeError("No mesh imported from "+filename)
    bpy.ops.object.select_all(action="DESELECT")
    for o in new:o.select_set(True)
    bpy.context.view_layer.objects.active=new[0]
    if len(new)>1:bpy.ops.object.join()
    obj=bpy.context.view_layer.objects.active
    obj.name=name
    obj.rotation_euler[2]=math.radians(-yaw)
    bpy.context.view_layer.update()
    lo,hi=world_bounds(obj)
    size=hi-lo
    scale=min(span/max(size.x,size.y,0.001),height/max(size.z,0.001))
    obj.scale*=scale
    bpy.context.view_layer.update()
    lo,hi=world_bounds(obj)
    center=(lo+hi)*0.5
    target=ub(*anchor)
    obj.location += Vector((target.x-center.x,target.y-center.y,target.z-lo.z))
    bpy.context.view_layer.update()
    return obj

def add_lobe(name,anchor,scale,yaw=0.0):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1.0,location=ub(*anchor))
    o=bpy.context.object;o.name=name
    o.scale=(scale[0],scale[2],scale[1])
    o.rotation_euler[2]=math.radians(-yaw)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o

def add_road_bed(name,anchor,width,length,height,yaw=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0,location=ub(*anchor))
    o=bpy.context.object;o.name=name
    o.dimensions=(width,length,height)
    o.rotation_euler[2]=math.radians(-yaw)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bevel=o.modifiers.new("soft rock edges","BEVEL")
    bevel.width=min(.38,height*.28);bevel.segments=3
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    return o

parts=[
    import_support("ResidentialTerraceRock.glb","lower west terrace",(-5.35,-.55,-3.05),7.4,3.25,16),
    import_support("ResidentialTerraceRock.glb","lower east terrace",(5.30,-.52,-2.90),7.4,3.25,196),
    import_support("RockTerrainSeamFiller.glb","lower west seam",(-2.65,-.15,-2.65),5.0,2.55,28),
    import_support("RockTerrainSeamFiller.glb","lower east seam",(2.65,-.13,-2.55),5.0,2.55,208),
    import_support("RockTerrainSeamFiller.glb","middle west seam",(-4.35,.62,.95),5.8,2.70,42),
    import_support("RockTerrainSeamFiller.glb","middle east seam",(4.30,.64,1.05),5.8,2.70,222),
    import_support("TowerWallRock.glb","upper west buttress",(-3.65,1.38,4.30),4.9,2.75,18),
    import_support("TowerWallRock.glb","upper east buttress",(3.65,1.40,4.35),4.9,2.75,198),
    add_lobe("lower central bedrock",(0,-1.05,-2.35),(4.9,1.55,3.4),8),
    add_lobe("lower west shoulder",(-5.4,-1.05,-.75),(4.1,1.65,3.1),24),
    add_lobe("lower east shoulder",(5.3,-1.02,-.65),(4.1,1.65,3.1),-24),
    add_lobe("middle central saddle",(0,.10,1.15),(4.0,1.45,2.9),0),
    add_lobe("upper west shoulder",(-3.0,.72,3.65),(3.3,1.38,2.55),18),
    add_lobe("upper east shoulder",(3.0,.74,3.72),(3.3,1.38,2.55),-18),
    add_road_bed("lower civic landing",(0,.18,-3.20),2.35,4.8,.70,0),
    add_road_bed("middle civic landing",(0,1.10,.10),2.25,3.6,.68,0),
    add_road_bed("upper civic landing",(0,2.05,3.65),2.15,3.6,.66,0),
]

bpy.ops.object.select_all(action="DESELECT")
for o in parts:
    o.select_set(True)
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
bpy.context.view_layer.objects.active=parts[0]
bpy.ops.object.join()
obj=bpy.context.object
obj.name="ValoriaFusedHeroCityRock"

obj.data.remesh_voxel_size=.18
obj.data.remesh_voxel_adaptivity=.08
obj.data.use_remesh_preserve_volume=True
bpy.ops.object.voxel_remesh()

dec=obj.modifiers.new("fixed-camera decimate","DECIMATE")
dec.decimate_type="COLLAPSE";dec.ratio=.58
bpy.context.view_layer.objects.active=obj
bpy.ops.object.modifier_apply(modifier=dec.name)

bpy.ops.object.shade_smooth()
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="SELECT")
bpy.ops.mesh.normals_make_consistent(inside=False)
try:
    bpy.ops.uv.smart_project(island_margin=.02)
except TypeError:
    bpy.ops.uv.smart_project()
bpy.ops.object.mode_set(mode="OBJECT")

mat=bpy.data.materials.new("FusedRock")
mat.diffuse_color=(.46,.45,.42,1)
obj.data.materials.append(mat)

lo,hi=world_bounds(obj)
report={
    "method":"certified_support_overlap_voxel_remesh",
    "source_assets":["ResidentialTerraceRock","RockTerrainSeamFiller","TowerWallRock"],
    "vertices":len(obj.data.vertices),
    "polygons":len(obj.data.polygons),
    "bounds_blender":{"min":list(lo),"max":list(hi)},
    "voxel_size":obj.data.remesh_voxel_size,
    "decimate_ratio":.58
}
idx=sys.argv.index("--") if "--" in sys.argv else -1
out=sys.argv[idx+1] if idx>=0 else "ValoriaFusedTransition.glb"
report_path=sys.argv[idx+2] if idx>=0 and len(sys.argv)>idx+2 else os.path.splitext(out)[0]+".json"
bpy.ops.export_scene.gltf(filepath=os.path.abspath(out),export_format="GLB",export_apply=True,
                          export_materials="EXPORT",export_yup=True)
with open(report_path,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("VALORIA_FUSED_TRANSITION_BUILT",json.dumps(report))
