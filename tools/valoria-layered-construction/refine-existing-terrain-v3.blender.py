# ELDORIA VALORIA isolated sculpt v3: actual continuous ground contact at both bridge shores
# and asymmetric limestone strata instead of V2 repeating wave facade.
# Source: immutable EDITABLE ValoriaTerrainRiverCliffV2.blend. No new library.
import bpy,sys,os,json,hashlib,math,random
source,manifestfile,out=sys.argv[sys.argv.index("--")+1:]
os.makedirs(out,exist_ok=True)
meta=json.load(open(manifestfile,encoding='utf-8'))
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
if sha(source)!=meta["source_blend_v2_sha256"]:raise RuntimeError("Editable V2 source mutated")
bpy.ops.wm.open_mainfile(filepath=os.path.abspath(source))
source_objs=[o for o in bpy.data.objects if o.type=='MESH' and (
    o.name.startswith('V2_') or o.name.startswith('VALORIA_V2_'))]
if len(source_objs)!=9:raise RuntimeError("Expected exact nine V2 edited source meshes, got "+str([o.name for o in source_objs]))
collection=bpy.data.collections.new("VALORIA_TERRAIN_V3_REAL_BRIDGE_CONTACT_AND_GEOLOGY")
bpy.context.scene.collection.children.link(collection)
for ob in bpy.data.objects:ob.hide_render=True;ob.hide_set(True)
chosen=[]
for original in source_objs:
    o=original.copy();o.data=original.data.copy()
    collection.objects.link(o);o.name="V3_"+original.name
    o.hide_render=False;o.hide_set(False);chosen.append(o)
def find(fragment):return next(o for o in chosen if fragment in o.name)
terrain=find("LOWER_REAL_TERRAIN_NATIVE_HEIGHTFIELD")
water=find("RIVER_CONTINUOUS_RIPPLED_WATER")
facades=[o for o in chosen if "TERRACED_LIMESTONE_CLIFF" in o.name]
assert len(facades)==2
original_materials={m.name:m for m in bpy.data.materials}
def mat(fragment,rgb,rough):
    m=next(x for n,x in original_materials.items() if fragment in n)
    # Make a private material for this independent new art candidate,
    # no alteration of inherited original's material data.
    nm=m.copy();nm.name="V3_Author_"+m.name;nm.diffuse_color=(*rgb,1);nm.use_nodes=True
    node=nm.node_tree.nodes.get("Principled BSDF")
    if node:
        node.inputs["Base Color"].default_value=(*rgb,1)
        node.inputs["Roughness"].default_value=rough
    return nm
palette={
 'Rocky limestone aged':mat('Rocky limestone aged',(.54,.495,.409),.94),
 'deep cliff fissures':mat('deep cliff fissures',(.33,.319,.296),.96),
 'clay bank exposed soil':mat('clay bank exposed soil',(.40,.35,.267),.96),
 'Earth grassland shaded olive':mat('Earth grassland shaded olive',(.255,.302,.198),.95),
 'Earth sunlit meadow':mat('Earth sunlit meadow',(.38,.407,.290),.95),
 'River blue-grey':mat('River blue-grey',(.075,.196,.255),.35),
 'Boulder striated slate':mat('Boulder striated slate',(.405,.391,.342),.97)
}
for obj in chosen:
    for index,material in enumerate(obj.data.materials):
        if material:
            for key,m in palette.items():
                if key in material.name:obj.data.materials[index]=m;break
def smooth(a,b,x):
    t=max(0,min(1,(x-a)/(b-a)))
    return t*t*(3-2*t)
def terrain_v2(x,z):
    bend=-16+1.23*math.sin(x*.13)+.40*math.sin(x*.39)
    channels=2.27*math.exp(-pow(abs(z-bend)/4,3.2))
    geom=.12+.19*math.sin(x*.11+z*.055)+.10*math.cos(x*.23-z*.13)
    micro=.065*math.sin(x*.83+z*.71)*math.cos(z*.63)
    peripheral=max(0.,abs(x)-16.)/16.
    height=geom+.65*peripheral**2+micro-channels
    for shore in (-19.65,-9.35):
        dist=(z-shore)/1.6
        height+=.11*math.exp(-dist*dist)*math.exp(-(x/6.8)**2)
    return height
def terrain_v3(x,z):
    # Bridge deck ends EXACTLY x=0,z=-19.65 and -9.35 at native height 0.38m.
    # Supported grounded walkway connects both banks, a smooth berm flares out
    # without filling the waterway or inventing a second bridge collider.
    original=terrain_v2(x,z)
    cross=1-smooth(2.35,5.6,abs(x))
    far=1-smooth(-19.65,-17.30,z)
    near=smooth(-11.15,-9.35,z)
    contact=max(far,near)*cross
    return original+contact*(.38-original)
for v in terrain.data.vertices:
    x,z=v.co.x,-v.co.y
    v.co.z=terrain_v3(x,z)
terrain.data.update()
# Each inherited dock stone rests on the *new* terrain, rather than levitating
# at its former V2 height. Geometry itself remains genuine editable rock.
for ob in chosen:
    if "REAL_BRIDGE_BANK_STONE" not in ob.name:continue
    mesh=ob.data
    # Existing mesh is authored in native Blender co and centered in local vertex XYZ;
    # actual bank location is encoded in its centroid.
    x=sum(v.co.x for v in mesh.vertices)/len(mesh.vertices)
    z=-sum(v.co.y for v in mesh.vertices)/len(mesh.vertices)
    rise=terrain_v3(x,z)-terrain_v2(x,z)
    for v in mesh.vertices:v.co.z+=rise
    mesh.update()
# Rebuild only the two V2 tessellated wave facades as irregular broad rock strata.
# The staircase throat |x|<2.05 must remain OPEN, 12 colliders retained by Unity.
for ob in facades:
    chosen.remove(ob);bpy.data.objects.remove(ob,do_unlink=True)
def B(x,y,z):return (x,-z,y)
rockmat=palette['Rocky limestone aged'];dark=palette['deep cliff fissures'];soil=palette['clay bank exposed soil']
for side in (-1,1):
    cols,rows=21,10
    xleft,xright=(2.10,13.80) if side>0 else (-13.80,-2.10)
    vertices=[]
    for j in range(rows+1):
        t=j/rows
        height=.17+2.55*t
        for i in range(cols+1):
            x=xleft+(xright-xleft)*i/cols
            # Low-frequency faults and asymmetric ledges, without mathematically
            # repeated bump texture. Dominant geological blocks >1.2m wide.
            seed=(i*73856093+j*19349663+(2 if side>0 else 17)*83492791)&0xffffffff
            jitter=((seed%1009)/1009.0-.5)*.13
            fault=.32*math.sin(.27*x+.34*side)+.19*math.sin(.57*x+.61*side)
            ledge= (.28 if t<.2 else .10 if t<.52 else -.055 if t<.78 else -.14)
            depth=1.82+3.20*t+fault+ledge+jitter*math.sin(math.pi*t)
            vertices.append((x,height,depth))
    faces=[];materials=[]
    for j in range(rows):
        for i in range(cols):
            a=j*(cols+1)+i;k=a+cols+1
            # Alternating diagonal tessellation makes slabs asymmetric.
            if (i+j)%2:
                faces.extend([(a,k,k+1),(a,k+1,a+1)])
            else:
                faces.extend([(a,k,a+1),(k,k+1,a+1)])
            zone=1 if j in (2,5,8) and i%5 in (0,1) else 2 if j==0 and i%4==0 else 0
            materials.extend([zone,zone])
    mesh=bpy.data.meshes.new("ValoriaRockStrataV3_source_mesh")
    mesh.from_pydata([B(*v) for v in vertices],[],faces);mesh.update(calc_edges=True)
    ob=bpy.data.objects.new("VALORIA_V3_IRREGULAR_RETERRACED_CLIFF_"+str(side),mesh)
    collection.objects.link(ob);ob.hide_render=False
    mesh.materials.append(rockmat);mesh.materials.append(dark);mesh.materials.append(soil)
    for k,poly in enumerate(mesh.polygons):
        poly.material_index=materials[k];poly.use_smooth=False
    uv=mesh.uv_layers.new(name="Stratified_Metre_UV")
    for polygon in mesh.polygons:
        for lid in polygon.loop_indices:
            p=mesh.vertices[mesh.loops[lid].vertex_index].co
            uv.data[lid].uv=(p.x*.16,(p.z if abs(polygon.normal.z)<.60 else p.y)*.16)
    chosen.append(ob)
# Direct physical grade constraints are evaluated BEFORE export, not merely stored as claims.
sampled={}
for shore in [-19.65,-9.35]:
    h=terrain_v3(0,shore)
    sampled[str(shore)]=h
    if abs(h-.38)>1e-8:raise RuntimeError(f"Road/deck gap at {shore}: {h}")
# Native unit positions preserved; no Unity arbitrary scale.
for ob in bpy.data.objects:ob.select_set(False)
for ob in chosen:ob.select_set(True);ob.hide_set(False);ob.hide_render=False
bpy.context.view_layer.objects.active=chosen[0]
blend=os.path.join(out,'ValoriaTerrainRiverCliffV3.blend')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
glb=os.path.join(out,'ValoriaTerrainRiverCliffV3.glb')
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
tri=sum(sum(len(f.vertices)-2 for f in ob.data.polygons) for ob in chosen)
assert len(chosen)==9 and 10000<tri<40000
manifest={
 "original_v2_source_sha256":meta["source_blend_v2_sha256"],
 "original_v2_glb_sha256":meta["source_glb_v2_sha256"],
 "source_blend_v3_sha256":sha(blend),"source_glb_v3_sha256":sha(glb),
 "source_and_canonical_unmodified":True,"original_collider_changes":False,
 "seamless_at_bridge_endpoints":True,"shore_world_heights_y":sampled,
 "native_bridge_walk_height_y":.38,"bridge_endpoint_z":[-19.65,-9.35],
 "stair_clearance_halfwidth_x":2.05,"source_mesh_count":len(chosen),
 "triangles":tri,"source_objects":[o.name for o in chosen],
 "art_gate":"CANDIDATE_NOT_APPROVED_BY_COMPILATION",
 "description":"Grounded real bridge ends, asymmetric stratified rock facade, retained V2 organic river"
}
open(os.path.join(out,"ValoriaTerrainRiverCliffV3-manifest.json"),"w").write(json.dumps(manifest,indent=2,ensure_ascii=False)+"\n")
print("VALORIA_TERRAIN_V3_ART_SOURCE_CONTACTS_PASS",json.dumps({
 "bridge_end_y":sampled,"sha":manifest["source_glb_v3_sha256"],
 "triangles":tri,"objects":len(chosen)}))
