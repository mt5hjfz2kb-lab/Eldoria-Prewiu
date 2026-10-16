# Valoria flat-access bridge: new source-editable architectural derivative of BridgeFamilyV1.
# Original source untouched and retained in separate source collection of derived .blend.
# Unity world frame: (X,Yup,Z); Blender vertex frame (X,-Z,Y).
import bpy,sys,os,math,hashlib,json
from mathutils import Vector
orig,out=sys.argv[sys.argv.index("--")+1:]
os.makedirs(out,exist_ok=True)
expected="801fd7d393c8c151c3036e793e62ff1aad88cd22b636598194b7c47e1ba2eed4"
digest=hashlib.sha256(open(orig,"rb").read()).hexdigest()
if digest!=expected: raise RuntimeError("Original approved .blend SHA changed: "+digest)
bpy.ops.wm.open_mainfile(filepath=os.path.abspath(orig))
original=[o for o in bpy.data.objects if o.type=='MESH']
assert len(original)==4
source_materials={}
for o in original:
    if 'Deck' in o.name: source_materials['paving']=o.data.materials[0] if o.data.materials else None
    if 'Parapet' in o.name: source_materials['wall']=o.data.materials[0] if o.data.materials else None
    if 'Support' in o.name: source_materials['foundation']=o.data.materials[0] if o.data.materials else None
assert all(source_materials.get(k) for k in ('paving','wall','foundation'))
for obj in bpy.data.objects:
    obj.hide_render=True
    obj.hide_set(True)
    obj.select_set(False)
new_collection=bpy.data.collections.new("VALORIA_LEVEL_BRIDGE_V1_EDITABLE_ARCHITECTURE")
bpy.context.scene.collection.children.link(new_collection)
group={}
# Blender axes X, -Z, Y to export Unity X,Y,Z at origin without scale/grade warping.
def B(x,y,z):return (x,-z,y)
def collect(ob,role):
    for c in list(ob.users_collection):c.objects.unlink(ob)
    new_collection.objects.link(ob)
    ob.hide_render=False;ob.hide_set(False)
    group.setdefault(role,[]).append(ob)
    return ob
def stone(name,role,x,y,z,sx,sy,sz,mat,bevel=0.018):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=B(x,y,z))
    ob=bpy.context.object;ob.name=name
    ob.dimensions=(sx,sz,sy) # widthX / depthZ / heightY
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    ob.data.materials.append(mat)
    if bevel:
        mod=ob.modifiers.new("Crafted edge bevel","BEVEL")
        mod.width=min(bevel,min(sx,sy,sz)*.24);mod.segments=1
        bpy.context.view_layer.objects.active=ob
        bpy.ops.object.modifier_apply(modifier=mod.name)
    for f in ob.data.polygons:f.use_smooth=False
    return collect(ob,role)
def custom_mesh(name,role,verts,faces,mat):
    me=bpy.data.meshes.new(name+"SourceMesh")
    me.from_pydata([B(*v) for v in verts],[],faces);me.update(calc_edges=True)
    ob=bpy.data.objects.new(name,me);new_collection.objects.link(ob);ob.hide_render=False
    me.materials.append(mat)
    # Intentionally metric triplanar-like planar UVs. No generated meshes assigned as final art blindly.
    uv=me.uv_layers.new(name="StoneMetric_UV_1m")
    for face in me.polygons:
        for li in face.loop_indices:
            q=me.vertices[me.loops[li].vertex_index].co
            # Face normal selects XZ floor or Z/Y side mapping.
            if abs(face.normal.z)>.65:uv.data[li].uv=(q.x,q.y)
            else:uv.data[li].uv=(q.x,q.z)
    group.setdefault(role,[]).append(ob)
    return ob
def ensure_role_join(role):
    pieces=group.get(role,[])
    if not pieces:return
    bpy.ops.object.select_all(action="DESELECT")
    for o in pieces:o.select_set(True)
    bpy.context.view_layer.objects.active=pieces[0]
    if len(pieces)>1:bpy.ops.object.join()
    pieces[0].name="VALORIA_"+role
    group[role]=[pieces[0]]
    pieces[0].select_set(False)
paving=source_materials['paving'];stonewall=source_materials['wall'];foundation=source_materials['foundation']
# STRONGLY DEFINED LEVEL WALKWAY. X ±2.12, Z [-19.65,-9.35], top Y=.380, landing at both ends.
near=-9.35;far=-19.65;length=near-far;center=(near+far)*.5
stone("StructuralBridgeDeck","DECK_CORE",0,.095,center,4.14,.255,length,foundation,.055) # top .2225
# 13 courses × 4 staggered paving stones; all tops meet exactly y=.38 (0.1575 high).
paver_rows=13;paver_columns=4;gap=.028
for ri in range(paver_rows):
    zmin=far+ri*length/paver_rows;zmax=far+(ri+1)*length/paver_rows
    z=(zmin+zmax)*.5
    for ci in range(paver_columns):
        xmin=-2.065+ci*4.13/paver_columns;xmax=-2.065+(ci+1)*4.13/paver_columns
        x=(xmin+xmax)*.5
        stagger=(.09 if ri%2==1 and ci%2==0 else -.055 if ri%2==1 else 0)
        w=xmax-xmin-gap;d=zmax-zmin-gap
        stone("HandPaver_R%02d_C%d"%(ri,ci),"PAVING",
           x+stagger,.30125,z,w,.1575,d,paving,.026)
# continuous edge plinth and substantial parapets, true chamfer coping.
for side in (-1,1):
    x=side*2.20
    stone("BaseKerb_%+d"%side,"KERBS",x,.34,center,.48,.20,length,foundation,.035)
    for level in range(3):
        yy=.48+level*.22
        # staggered ashlars with fine shallow joints; never offset beyond bridge footprint
        stones=12+level%2
        for i in range(stones):
            a=far+i*length/stones;b=far+(i+1)*length/stones
            z=(a+b)*.5
            stone("Parapet_%+d_course%d_%02d"%(side,level,i),
              "PARAPETS",x,yy,z,.38,.205,b-a-.018,stonewall,.017)
    stone("Coping_%+d"%side,"COPING",x,1.205,center,.52,.20,length,foundation,.049)
    for index,at in enumerate((far+.46,near-.46)):
        stone("BridgePier_%+d_%d"%(side,index),"END_PIERS",
          side*2.18,-.37,at,.85,1.80,.77,foundation,.065)
        stone("BridgePierCap_%+d_%d"%(side,index),"END_CAPS",
          side*2.18,1.44,at,.94,.24,.86,stonewall,.070)
# The structural opening is made from irregular face meshes, not an arbitrary series of slabs.
# Arch opening along the bridge longitudinal Z, supported by two feet into riverbed (-2.2).
archcentre=center;half=3.46
for side in (-1,1):
    xa=side*2.0;xb=side*2.38
    # End arch shoulders and a stone spandrel ring over the true arch opening:
    for i in range(28):
        z0=archcentre-half+2*half*i/28;z1=archcentre-half+2*half*(i+1)/28
        def inner_ordinate(z):
            u=max(-1,min(1,(z-archcentre)/half))
            return -1.78+1.52*math.sqrt(max(0,1-u*u))
        y0=inner_ordinate(z0);y1=inner_ordinate(z1)
        v=[(xa,-.023,z0),(xa,-.023,z1),(xa,y1,z1),(xa,y0,z0),
           (xb,-.023,z0),(xb,-.023,z1),(xb,y1,z1),(xb,y0,z0)]
        faces=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7)]
        custom_mesh("ArchVoussoir_%+d_%02d"%(side,i),"ARCH_SPANDRELS",v,faces,
                    stonewall if i%4 else foundation)
    for z in (far+1.15,near-1.15):
        stone("LoadPier_%+d_%d"%(side,int(z*10)),"ARCH_FOOTINGS",
          side*2.20,-1.12,z,.78,2.2,.76,foundation,.043)
# Tailor abutment taper to ground lips, keep visible bridge paved level.
for side in (-1,1):
    stone("AbutmentCurb_%+d"%side,"ABUTMENTS",
       side*1.73,.22,far+.40,.50,.25,.78,stonewall,.023)
    stone("AbutmentGate_%+d"%side,"ABUTMENTS",
       side*1.73,.22,near-.40,.50,.25,.78,stonewall,.023)
for role in list(group):ensure_role_join(role)
selected=[o for x in group.values() for o in x]
# render source clay from fitted perspective; preview is NOT official Unity camera.
for o in bpy.data.objects:o.select_set(False)
for o in selected:o.select_set(True)
bpy.context.view_layer.objects.active=selected[0]
# Save source variant keeping all original editable objects hidden, unmodified.
blend=os.path.join(out,"BridgeLevelAccessV1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
glb=os.path.join(out,"BridgeLevelAccessV1.glb")
bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True)
assert os.path.getsize(glb)>30000
# Vertex extents in target Unity frame (X,Y,Z), prove identity native scale.
vertices=[]
for o in selected:
    vertices += [( (o.matrix_world @ v.co).x,(o.matrix_world @ v.co).z,-(o.matrix_world @ v.co).y )
                for v in o.data.vertices]
mins=[min(v[i] for v in vertices) for i in range(3)]
maxs=[max(v[i] for v in vertices) for i in range(3)]
triangles=sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in selected)
assert -2.8<mins[0]<-2.1 and 2.1<maxs[0]<2.8
assert mins[2]>=far-.08 and maxs[2]<=near+.08
assert triangles<30000,triangles
# Source power uses actual (3D) architectural facets/materials not greybox in place of final acceptance.
manifest={"source_original_sha256":digest,"generated_source_sha256":hashlib.sha256(open(blend,"rb").read()).hexdigest(),
  "generated_glb_sha256":hashlib.sha256(open(glb,"rb").read()).hexdigest(),
  "generated_scene_classification":"ART CANDIDATE NOT_APPROVED",
  "source_provenance":"BridgeFamilyV1 editable original plus newly authored bridge geometry, original preserved",
  "unity_bounds_min":mins,"unity_bounds_max":maxs,
  "target_road_y":.38,"deck_walkway_y":.38,"bridge_z_limits":[far,near],
  "authored_triangles":triangles,"objects_after_join":len(selected),
  "pavers":paver_rows*paver_columns,"arch_sections":56,"source_original_unchanged":True,
  "new_objects":[o.name for o in selected],
  "projection":"native metres Unity XYZ; no mesh warping, no import scale tricks"}
with open(os.path.join(out,"BridgeLevelAccessV1-manifest.json"),"w") as f:json.dump(manifest,f,indent=2)
print("VALORIA_SOURCE_BRIDGE_VARIANT_PASS "+json.dumps(manifest))
