"""Valoria Bastion-to-city continuity v1.
Designed macroform first; existing certified rock is used only as shrinkwrap donor detail.
No heightfield, no voxel union, no photogrammetry plate placement, no gameplay geometry.
"""
import bpy, os, sys, json, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
RES=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","Rescued")
OUT=sys.argv[sys.argv.index("--")+1] if "--" in sys.argv else "ValoriaBastionContinuity.glb"
REPORT=sys.argv[sys.argv.index("--")+2] if "--" in sys.argv and len(sys.argv)>sys.argv.index("--")+2 else OUT+".json"

def ub(v):
    x,y,z=v
    return Vector((x,-z,y))

def bounds(obj):
    pts=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    return Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts))),Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))

def import_glb(path,name):
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=path)
    meshes=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
    if not meshes: raise RuntimeError("No donor mesh: "+path)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0]
    if len(meshes)>1:bpy.ops.object.join()
    o=bpy.context.view_layer.objects.active;o.name=name
    return o

def normalized_copy(src,name,center,dims,yaw=0):
    o=src.copy();o.data=src.data.copy();bpy.context.scene.collection.objects.link(o);o.name=name
    lo,hi=bounds(o);s=hi-lo
    o.scale=(dims[0]/max(s.x,.001),dims[2]/max(s.y,.001),dims[1]/max(s.z,.001))
    o.rotation_euler[2]=math.radians(-yaw)
    o.location=ub(center)
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    return o

def make_macroform():
    # Unity-space section, front/lower city -> Hero Bastion.
    # Each pair creates a horizontal inhabited terrace followed by a rock riser.
    profile=[
      (-6.2,-0.80,9.8),
      (-4.45,-0.80,9.5),
      (-4.05, 0.08,9.0),
      (-2.45, 0.08,8.7),
      (-2.02, 0.82,8.1),
      (-0.30, 0.82,7.8),
      ( 0.15, 1.48,7.1),
      ( 1.95, 1.48,6.8),
      ( 2.42, 2.08,6.0),
      ( 4.15, 2.08,5.6),
      ( 4.62, 2.58,4.9),
      ( 6.18, 2.58,4.45),
      ( 6.55, 2.90,4.10)
    ]
    xs=31
    verts=[];faces=[];mat_index=[];rock_verts=set()
    rows=[]
    for ri,(z,y,half) in enumerate(profile):
        row=[]
        for i in range(xs):
            t=i/(xs-1);x=-half+2*half*t
            # camera-authored asymmetry: keep the silhouette natural without breaking terrace planarity.
            x+=0.14*math.sin(t*math.pi*2.3 + ri*.43)
            yy=y
            if ri%2==0 and ri not in (0,len(profile)-1):
                yy+=0.025*math.sin(t*math.pi*4.0+ri)
            row.append(len(verts));verts.append(tuple(ub((x,yy,z))))
        rows.append(row)
    for r in range(len(rows)-1):
        # horizontal segments are even->odd; risers are odd->even.
        is_riser=(r%2==1)
        for i in range(xs-1):
            a,b=rows[r][i],rows[r][i+1];c,d=rows[r+1][i+1],rows[r+1][i]
            faces.append((a,b,c,d));mat_index.append(0 if is_riser else 1)
            if is_riser:
                rock_verts.update((a,b,c,d))
    # side skirts hide the authored shell edges from approved camera envelope.
    for side in (0,xs-1):
        chain=[rows[r][side] for r in range(len(rows))]
        bottom=[]
        for r,(z,y,half) in enumerate(profile):
            x=verts[chain[r]][0]
            idx=len(verts);verts.append((x,verts[chain[r]][1],-2.2))
            bottom.append(idx)
        for r in range(len(chain)-1):
            faces.append((chain[r],chain[r+1],bottom[r+1],bottom[r]));mat_index.append(0)
    me=bpy.data.meshes.new("BastionContinuityMacroformMesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new("BastionContinuityMacroform",me);bpy.context.scene.collection.objects.link(o)
    vg=o.vertex_groups.new(name="RockFaces")
    vg.add(list(rock_verts),1.0,'REPLACE')
    rock=bpy.data.materials.new("Rock");rock.diffuse_color=(.34,.32,.29,1)
    ground=bpy.data.materials.new("Ground");ground.diffuse_color=(.34,.29,.22,1)
    o.data.materials.append(rock);o.data.materials.append(ground)
    for p,mi in zip(o.data.polygons,mat_index):p.material_index=mi
    return o

def make_box(name,center,dims,mat):
    bpy.ops.mesh.primitive_cube_add(size=1,location=ub(center))
    o=bpy.context.object;o.name=name;o.dimensions=(dims[0],dims[2],dims[1])
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    b=o.modifiers.new("edge soften","BEVEL");b.width=min(.10,dims[1]*.18);b.segments=2
    bpy.ops.object.modifier_apply(modifier=b.name)
    o.data.materials.append(mat)
    return o

bpy.ops.wm.read_factory_settings(use_empty=True)
macro=make_macroform()

# Existing certified rock is only a geometric donor. It is never exported.
src=import_glb(os.path.join(RES,"ResidentialTerraceRock.glb"),"RockDonorSource")
donors=[
 normalized_copy(src,"DonorLower",(0,.35,-3.0),(18.0,3.2,4.2),8),
 normalized_copy(src,"DonorMiddle",(0,1.35,.8),(14.5,3.0,4.0),-9),
 normalized_copy(src,"DonorUpper",(0,2.25,4.7),(10.0,2.7,3.4),6)
]
bpy.data.objects.remove(src,do_unlink=True)
bpy.ops.object.select_all(action='DESELECT')
for o in donors:o.select_set(True)
bpy.context.view_layer.objects.active=donors[0];bpy.ops.object.join();donor=bpy.context.object;donor.name="RockDonorCombined"

# Professional workflow analogue: designed macroform keeps composition authority;
# shrinkwrap only conforms rock-face vertices and has a strict projection limit.
bpy.context.view_layer.objects.active=macro
sw=macro.modifiers.new("Rock donor conform","SHRINKWRAP")
sw.target=donor;sw.vertex_group="RockFaces";sw.wrap_method='NEAREST_SURFACEPOINT';sw.wrap_mode='ON_SURFACE'
sw.offset=.015;sw.project_limit=.32
bpy.ops.object.modifier_apply(modifier=sw.name)
bev=macro.modifiers.new("Terrace edge soften","BEVEL");bev.width=.055;bev.segments=2;bev.limit_method='ANGLE'
bpy.ops.object.modifier_apply(modifier=bev.name)

# Structural retaining walls are aligned to the terrace risers, not scattered props.
stone=bpy.data.materials.new("Stone");stone.diffuse_color=(.40,.38,.34,1)
retaining=[]
for level,(z,y,w) in enumerate([(-4.12,-.02,8.25),(-2.10,.72,7.35),(.05,1.38,6.35),(2.32,1.98,5.25),(4.52,2.49,4.30)]):
    gap=1.55
    retaining.append(make_box(f"RetainingWall_{level}_L",(-(w+gap)*.25,y,z),((w-gap)*.50,.62,.30),stone))
    retaining.append(make_box(f"RetainingWall_{level}_R",((w+gap)*.25,y,z),((w-gap)*.50,.62,.30),stone))

# Narrow civic circulation grows through the landform as stairs + landings.
route=[]
levels=[(-5.35,-.66),(-4.15,-.16),(-3.05,.08),(-2.08,.61),(-1.15,.82),(-.05,1.25),(.95,1.48),(2.05,1.86),(3.12,2.08),(4.22,2.38),(5.25,2.58),(6.05,2.78)]
for i,(z,y) in enumerate(levels):
    x=.28*math.sin(i*.58)
    route.append(make_box(f"CivicStep_{i:02d}",(x,y,z),(1.45,.12,.72),stone))
# three authored landings create readable urban pauses.
for i,(z,y,w) in enumerate([(-2.65,.12,2.65),(1.65,1.52,2.45),(5.55,2.63,2.25)]):
    route.append(make_box(f"CivicLanding_{i}",(0,y,z),(w,.12,1.15),stone))

# Keep donor out of export.
donor.hide_render=True;donor.hide_viewport=True
bpy.ops.object.select_all(action='DESELECT')
out_objs=[macro]+retaining+route
for o in out_objs:o.select_set(True)
bpy.context.view_layer.objects.active=macro
os.makedirs(os.path.dirname(OUT),exist_ok=True)
bpy.ops.export_scene.gltf(filepath=OUT,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

tri={}
for o in out_objs:
    if o.type=="MESH":o.data.calc_loop_triangles();tri[o.name]=len(o.data.loop_triangles)
report={
 "method":"designed terraced macroform + limited shrinkwrap from certified ResidentialTerraceRock donor",
 "donor":"Unity/Assets/Eldoria/Resources/Valoria/Rescued/ResidentialTerraceRock.glb",
 "voxel_union":False,"photogrammetry_placement":False,"terrain_heightfield":False,
 "objects":[o.name for o in out_objs],"triangles":tri,"total_triangles":sum(tri.values()),
 "output_bytes":os.path.getsize(OUT),"tripo_credits":0
}
with open(REPORT,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
