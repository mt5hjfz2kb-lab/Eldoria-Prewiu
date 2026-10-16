# Genuine visual terrain and rocky retaining mesh over certified city route footprint.
# Data derived from actual 22-footprint master plan; does NOT change Unity colliders or gameplay.
# Blender coordinates (x,-unity_z,unity_y). Outputs editable .blend and true-scale GLB.
import bpy,sys,os,json,math,hashlib
from mathutils import Vector
source,out=sys.argv[sys.argv.index("--")+1:]
os.makedirs(out,exist_ok=True)
with open(source,encoding="utf-8") as fh:layout=json.load(fh)
assert layout["expected_footprint_count"]==22 and layout["stair"]["count"]==12
def footprint(name):
    return next(x for x in layout["footprints"] if x["id"]==name)
assert abs(footprint("VPD · L1 landing")["center"][1]-2.55)<.001
assert abs(footprint("VPD · lower entry apron")["center"][1]-.30)<.001
bpy.ops.wm.read_factory_settings(use_empty=True)
col=bpy.data.collections.new("VALORIA_AUTHORED_TERRAIN_RIVER_CLIFF_V1")
bpy.context.scene.collection.children.link(col)
def b(x,y,z):return (x,-z,y)
def mat(name,color,roughness):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1)
    m.use_nodes=True
    node=m.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value=(*color,1)
    node.inputs["Roughness"].default_value=roughness
    return m
earth=mat("Eldoria Earth grassland shaded olive",(.285,.322,.220),.95)
earth2=mat("Eldoria Earth sunlit meadow",(.35,.355,.243),.94)
soil=mat("Eldoria clay bank exposed soil",(.375,.332,.250),.96)
limestone=mat("Eldoria Rocky limestone aged",(.43,.414,.355),.91)
limestone2=mat("Eldoria deep cliff fissures",(.305,.312,.296),.93)
water=mat("Eldoria River blue-grey",(.08,.205,.268),.36)
rock=mat("Eldoria Boulder striated slate",(.315,.327,.314),.94)
def smooth(a,b,x):
    q=max(0,min(1,(x-a)/(b-a)))
    return q*q*(3-2*q)
def surface(x,z):
    # Elevation controlled by terrain/mature district envelope; exact roads/plots remain separate in Unity.
    noise=.075*math.sin(x*.27)*math.sin(z*.30)+.045*math.sin(x*.93+z*.57)
    rim=max(0,(abs(x)-16)/16)
    highground=.34*rim*rim+.08*math.sin(x*.12+z*.17)*rim
    riverCentre=-16+.16*math.sin(.11*x)
    river=smooth(riverCentre-3.65,riverCentre-2.15,z)*(
       1-smooth(riverCentre+2.5,riverCentre+3.9,z))
    lip=.115*math.exp(-((x/7.0)**2+((z+9.4)/3.7)**2))
    h=.10+noise+highground+lip-2.46*river
    # Low-frequency riverbed sediment instead of the old planar cut.
    return h
created=[]
def mesh_obj(name,pts,faces,mats,material_indices=None,metric_uv=.18):
    me=bpy.data.meshes.new(name)
    me.from_pydata([b(*v) for v in pts],[],faces);me.update(calc_edges=True)
    ob=bpy.data.objects.new(name,me);col.objects.link(ob)
    for m in mats:me.materials.append(m)
    if material_indices:
        for i,j in enumerate(material_indices):me.polygons[i].material_index=j
    uv=me.uv_layers.new(name="MetreMappedUV")
    for poly in me.polygons:
        for li in poly.loop_indices:
            q=me.vertices[me.loops[li].vertex_index].co
            if abs(poly.normal.z)>.55:uv.data[li].uv=(q.x*metric_uv,q.y*metric_uv)
            else:uv.data[li].uv=(q.x*metric_uv,q.z*metric_uv)
    created.append(ob)
    return ob
# Single continuous geologically relieved surface from external west to east districts.
dx=.75;dz=.75
nx=int(64/dx);nz=int(59/dz)
x0=-32;z0=-29
points=[];faces=[];material_indices=[]
for iz in range(nz+1):
    z=z0+iz*59/nz
    for ix in range(nx+1):
        x=x0+ix*64/nx
        points.append((x,surface(x,z),z))
for iz in range(nz):
    for ix in range(nx):
        k=iz*(nx+1)+ix
        # Reverse orientation to ensure Blender normal +Z and Unity normal +Y.
        faces.append((k,k+nx+1,k+nx+2,k+1))
        cx=x0+(ix+.5)*64/nx;cz=z0+(iz+.5)*59/nz
        dist=abs(cz-(-16+.16*math.sin(cx*.11)))
        material_indices.append(2 if dist<5.2 else (1 if abs(cx)>18 or math.sin(cx*.21+cz*.29)>.36 else 0))
ob=mesh_obj("VALORIA_LOWER_REAL_TERRAIN_NATIVE_HEIGHTFIELD",points,faces,[earth,earth2,soil],material_indices,.13)
for p in ob.data.polygons:p.use_smooth=True
# Heightfield landscape stays below official plot/road collider geometry; no changes to PlayerState.
# Rebuild upper retaining facade as rock strata with a dedicated stair mouth left clear.
for side in (-1,1):
    xlo,xhi=(2.03,9.20) if side==1 else (-9.20,-2.03)
    cols=22
    verts=[]
    for i in range(cols+1):
        x=xlo+(xhi-xlo)*i/cols
        grain=.15*math.sin(i*1.67+side*.7)+.075*math.sin(i*.42)
        verts.extend([
            (x,2.51+.11*math.sin(i*.55),4.72+grain),
            (x,1.64+.11*math.sin(i*.9),3.96+grain*.9),
            (x,.83+.14*math.sin(i*.64),2.87+grain),
            (x,.055+surface(x,2.15)*.22,1.85+grain)
        ])
    tris=[];material_slots=[]
    for i in range(cols):
        for tier in range(3):
            a=i*4+tier;d=(i+1)*4+tier
            tris.append((a,d,a+1));material_slots.append((i+tier)%5==0 and 1 or 0)
            tris.append((d,d+1,a+1));material_slots.append((i+tier)%5==0 and 1 or 0)
    mesh_obj("VALORIA_LOWER_TO_UPPER_FACETED_STONE_FLANK_"+str(side),
         verts,tris,[limestone,limestone2],material_slots,.21)
# A true riverbank water surface with slight velocity ripples, entirely within excavated channel.
water_points=[];water_faces=[]
nxx=72;nzz=10
for j in range(nzz+1):
    t=j/nzz
    for i in range(nxx+1):
        x=-32+64*i/nxx
        c=-16+.16*math.sin(.11*x)
        z=c-2.27+4.57*t
        y=-1.87+.021*math.sin(2.1*x+.7*z)+.01*math.sin(x*3+.45*z)
        water_points.append((x,y,z))
for j in range(nzz):
    for i in range(nxx):
        k=j*(nxx+1)+i
        water_faces.append((k,k+nxx+1,k+nxx+2,k+1))
mesh_obj("VALORIA_RIVER_CONTINUOUS_RIPPLED_WATER",water_points,water_faces,[water],None,.035)
# Side boulders tied to measured bank positions, not floating above arbitrary Y.
for i in range(72):
    x=-31+(i*41%63)
    if abs(x)<5.4:continue # bridge approach must remain clear
    z=(-20.1 if i%2 else -11.4)+.55*math.sin(i*1.9)
    y=surface(x,z)+.1
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=b(x,y,z))
    obj=bpy.context.object
    obj.name="Riverbank_real_boulder_%03d"%i
    obj.scale=(.26+.19*(i%4),.22+.10*(i%5),.19+.06*(i%4))
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for old in list(obj.users_collection):old.objects.unlink(obj)
    col.objects.link(obj);obj.data.materials.append(rock)
    created.append(obj)
# Join stones for mobile draw/instance sanity, keep terrain and cliffs separately editable.
boulders=[x for x in created if x.name.startswith("Riverbank_real_boulder_")]
if boulders:
    bpy.ops.object.select_all(action="DESELECT")
    for o in boulders:o.select_set(True)
    bpy.context.view_layer.objects.active=boulders[0]
    bpy.ops.object.join()
    boulders[0].name="VALORIA_RIVERBANK_AUTHOR_ROCKS"
    created=[o for o in created if o not in boulders[1:]]
for ob in bpy.data.objects:ob.select_set(False)
for ob in created:ob.select_set(True)
bpy.context.view_layer.objects.active=created[0]
blend=os.path.join(out,"ValoriaTerrainRiverCliffV1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
glb=os.path.join(out,"ValoriaTerrainRiverCliffV1.glb")
bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True)
assert os.path.getsize(glb)>20000
v=[]
for ob in created:
    v += [ob.matrix_world@vv.co for vv in ob.data.vertices]
tri=sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in created)
manifest={
 "source_certified_layout":source,"layout_footprints":22,"stair_steps_protected":12,
 "unity_route_and_colliders_unmodified":True,"mesh_count":len(created),
 "triangles":tri,"source_blend_sha256":hashlib.sha256(open(blend,'rb').read()).hexdigest(),
 "export_glb_sha256":hashlib.sha256(open(glb,'rb').read()).hexdigest(),
 "world_unity_x_extent":[-32,32],"world_unity_z_extent":[-29,30],
 "river_z_center":-16,"river_water_y":-1.87,
 "upper_landing_y":footprint("VPD · L1 landing")["center"][1],
 "min_mesh_y":min(q.z for q in v),"max_mesh_y":max(q.z for q in v),
 "classification":"VISUAL TERRAIN SOURCE CANDIDATE — not ART_APPROVED nor runtime gameplay certified",
 "source_objects":[o.name for o in created]
}
with open(os.path.join(out,"ValoriaTerrainRiverCliffV1-manifest.json"),"w") as f:json.dump(manifest,f,indent=2,ensure_ascii=False)
print("VALORIA_NATIVE_TERRAIN_SOURCE_PASS "+json.dumps(manifest))
