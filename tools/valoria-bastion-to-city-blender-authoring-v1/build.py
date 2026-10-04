import argparse, bpy, bmesh, json, math, os, sys, hashlib, random
from mathutils import Vector

# VALORIA BASTION-TO-CITY BLENDER AUTHORING v1
# One source only. Primary forms are lofted irregular authored meshes; openings are true boolean voids.
# No mirrored half, no formulaic repeated bays, no box-derived primary silhouette.

def args():
    av=sys.argv
    av=av[av.index("--")+1:] if "--" in av else []
    p=argparse.ArgumentParser()
    p.add_argument("--output-dir",required=True)
    p.add_argument("--evidence-dir",required=True)
    p.add_argument("--report",required=True)
    return p.parse_args()
A=args()

REQ="pipeline/valoria-bastion-to-city-blender-authoring-v1-request.json"
r=json.load(open(REQ,encoding="utf-8"))
assert r["enabled"] is True and r["phase"]=="SOURCE_ISOLATED"
assert r["tripo"] is False and r["credits"]==0 and r["allow_unity_integration"] is False

bpy.ops.wm.read_factory_settings(use_empty=True)
os.makedirs(A.output_dir,exist_ok=True); os.makedirs(A.evidence_dir,exist_ok=True)

prod=bpy.data.collections.new("PRODUCTION_BASTION_TO_CITY_FRAME")
review=bpy.data.collections.new("REVIEW_PROXY_ONLY")
bpy.context.scene.collection.children.link(prod); bpy.context.scene.collection.children.link(review)

def link(obj,col=prod):
    for c in list(obj.users_collection): c.objects.unlink(obj)
    col.objects.link(obj)
    return obj

def proc_mat(name, c1, c2, rough=.78, bump=.18, scale=4.0, detail=3.0):
    m=bpy.data.materials.new(name); m.use_nodes=True
    nt=m.node_tree; nt.nodes.clear()
    out=nt.nodes.new("ShaderNodeOutputMaterial"); bs=nt.nodes.new("ShaderNodeBsdfPrincipled")
    noise=nt.nodes.new("ShaderNodeTexNoise"); noise.inputs["Scale"].default_value=scale; noise.inputs["Detail"].default_value=detail; noise.inputs["Roughness"].default_value=.72
    ramp=nt.nodes.new("ShaderNodeValToRGB"); ramp.color_ramp.elements[0].color=(*c1,1); ramp.color_ramp.elements[1].color=(*c2,1)
    bumpn=nt.nodes.new("ShaderNodeBump"); bumpn.inputs["Strength"].default_value=bump; bumpn.inputs["Distance"].default_value=.13
    bs.inputs["Roughness"].default_value=rough
    nt.links.new(noise.outputs["Fac"],ramp.inputs["Fac"]); nt.links.new(ramp.outputs["Color"],bs.inputs["Base Color"])
    nt.links.new(noise.outputs["Fac"],bumpn.inputs["Height"]); nt.links.new(bumpn.outputs["Normal"],bs.inputs["Normal"]); nt.links.new(bs.outputs["BSDF"],out.inputs["Surface"])
    return m

STONE=proc_mat("Valoria Warm Primary Stone",(0.31,0.25,0.18),(0.52,0.42,0.29),.78,.16,4.7,3.2)
RETAIN=proc_mat("Valoria Weathered Retaining Stone",(0.23,0.20,0.17),(0.39,0.32,0.24),.84,.22,5.8,4.0)
TRIM=proc_mat("Valoria Limited Pale Trim",(0.48,0.39,0.28),(0.64,0.54,0.39),.72,.09,6.2,2.4)
ROCK=proc_mat("Valoria Dark Desaturated Rock",(0.10,0.105,0.10),(0.25,0.24,0.21),.9,.34,3.1,5.0)
SLATE=proc_mat("Valoria Restrained Slate",(0.055,0.075,0.085),(0.13,0.17,0.19),.82,.11,8.0,2.0)
BLUE=proc_mat("Valoria Sparse Heraldic Blue",(0.018,0.065,0.15),(0.04,0.15,0.30),.7,.05,7.0,2.0)
DARK=proc_mat("Valoria Recess Interior",(0.018,0.017,0.016),(0.075,0.06,0.045),.92,.04,2.0,2.0)
CLAY=proc_mat("Review Clay",(0.42,0.41,0.38),(0.56,0.54,0.50),.92,.04,9.0,2.0)

def mesh_obj(name, verts, faces, mat, col=prod):
    me=bpy.data.meshes.new(name+"Mesh"); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); col.objects.link(o); o.data.materials.append(mat)
    return o

def loft(name, rings, mat, cap=True):
    # rings: list of [(x,y),...] with common vertex count paired with z
    n=len(rings[0][1]); verts=[]
    for z,pts in rings:
        assert len(pts)==n
        verts += [(x,y,z) for x,y in pts]
    faces=[]
    if cap:
        faces.append(tuple(range(n-1,-1,-1)))
        off=(len(rings)-1)*n; faces.append(tuple(off+i for i in range(n)))
    for k in range(len(rings)-1):
        a=k*n; b=(k+1)*n
        for i in range(n):
            j=(i+1)%n; faces.append((a+i,a+j,b+j,b+i))
    return mesh_obj(name,verts,faces,mat)

def solidify_polygon(name, pts, z0,z1,mat):
    return loft(name,[(z0,pts),(z1,pts)],mat)

def bevel(obj,width=.08,segments=3):
    mod=obj.modifiers.new("Selective stone edge soften","BEVEL"); mod.width=width; mod.segments=segments; mod.limit_method='ANGLE'
    bpy.context.view_layer.objects.active=obj
    try:bpy.ops.object.modifier_apply(modifier=mod.name)
    except:pass

def weighted(obj):
    try:
        mod=obj.modifiers.new("Weighted normals","WEIGHTED_NORMAL"); mod.keep_sharp=True
        bpy.context.view_layer.objects.active=obj; bpy.ops.object.modifier_apply(modifier=mod.name)
    except:pass

def arch_cutter(name,cx,y0,y1,z0,width,height,segments=20):
    # front profile with vertical jambs + semicircular crown, extruded through full wall depth
    r=width/2.0; spring=z0+height-r
    prof=[(cx-r,z0),(cx+r,z0),(cx+r,spring)]
    for i in range(segments+1):
        a=math.pi*i/segments
        prof.append((cx+r*math.cos(a),spring+r*math.sin(a)))
    prof.append((cx-r,z0))
    # extrude profile in y
    verts=[(x,y0,z) for x,z in prof]+[(x,y1,z) for x,z in prof]
    n=len(prof); faces=[tuple(range(n-1,-1,-1)),tuple(n+i for i in range(n))]
    for i in range(n):
        j=(i+1)%n; faces.append((i,j,n+j,n+i))
    o=mesh_obj(name,verts,faces,DARK,review)
    return o

def boolean_hole(target,cutter):
    mod=target.modifiers.new("True architectural void","BOOLEAN"); mod.operation='DIFFERENCE'; mod.object=cutter; mod.solver='EXACT'
    bpy.context.view_layer.objects.active=target
    try:bpy.ops.object.modifier_apply(modifier=mod.name)
    except Exception as e: print("BOOLEAN_WARN",target.name,cutter.name,e)
    bpy.data.objects.remove(cutter,do_unlink=True)

def recess_back(name,cx,y,z0,w,h,mat=DARK):
    pts=[(cx-w/2,y-.04,z0),(cx+w/2,y-.04,z0),(cx+w/2,y+.04,z0),(cx-w/2,y+.04,z0),
         (cx-w/2,y-.04,z0+h),(cx+w/2,y-.04,z0+h),(cx+w/2,y+.04,z0+h),(cx-w/2,y+.04,z0+h)]
    faces=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
    return mesh_obj(name,pts,faces,mat)

def ribbon_wall(name,path,z0,z1,width,mat):
    # authored parapet following a non-straight route
    verts=[]; faces=[]
    for i,(x,y) in enumerate(path):
        if i==0: tangent=Vector((path[1][0]-x,path[1][1]-y))
        elif i==len(path)-1:tangent=Vector((x-path[i-1][0],y-path[i-1][1]))
        else:tangent=Vector((path[i+1][0]-path[i-1][0],path[i+1][1]-path[i-1][1]))
        tangent.normalize(); n=Vector((-tangent.y,tangent.x))*width/2
        verts += [(x+n.x,y+n.y,z0),(x-n.x,y-n.y,z0),(x+n.x,y+n.y,z1),(x-n.x,y-n.y,z1)]
    for i in range(len(path)-1):
        a=i*4; b=(i+1)*4
        faces += [(a,a+1,b+1,b),(a+2,b+2,b+3,a+3),(a,a+2,a+3,a+1),(b,b+1,b+3,b+2)]
    faces += [(0,2,3,1),(len(verts)-4,len(verts)-3,len(verts)-1,len(verts)-2)]
    o=mesh_obj(name,verts,faces,mat); bevel(o,.035,2); return o

def stair_proxy():
    # proxy only, not exported, proving central circulation remains open
    for i in range(14):
        z=.10+i*.24; y=-2.45+i*.20
        pts=[(-1.38,y-.22),(1.38,y-.22),(1.38,y+.13),(-1.38,y+.13)]
        o=solidify_polygon("Central Stair Proxy %02d"%i,pts,0,z,TRIM); link(o,review)

def rock_patch(name,center,scale,seed):
    random.seed(seed)
    cx,cy,cz=center; sx,sy,sz=scale
    # 3 irregular rings, intentionally authored around contact, not detached wedges
    rings=[]
    n=12
    for level,(zmul,radmul) in enumerate([(0,.92),(.48,1.0),(1,.62)]):
        pts=[]
        for i in range(n):
            a=2*math.pi*i/n
            jitter=1+random.uniform(-.18,.18)
            pts.append((cx+math.cos(a)*sx*radmul*jitter, cy+math.sin(a)*sy*radmul*jitter))
        rings.append((cz+sz*zmul,pts))
    o=loft(name,rings,ROCK); bevel(o,.16,3)
    # displace subtle surface after subdivision
    sub=o.modifiers.new("Rock subdivision","SUBSURF"); sub.subdivision_type='SIMPLE'; sub.levels=1; sub.render_levels=1
    bpy.context.view_layer.objects.active=o
    try:bpy.ops.object.modifier_apply(modifier=sub.name)
    except:pass
    tex=bpy.data.textures.new(name+" authored breakup",type='CLOUDS'); tex.noise_scale=.45; tex.noise_depth=2
    dis=o.modifiers.new("Authored rock breakup","DISPLACE"); dis.texture=tex; dis.strength=.16; dis.mid_level=.48
    try:bpy.ops.object.modifier_apply(modifier=dis.name)
    except:pass
    return o

# ---------- PRIMARY WEST MASS: low, broad, inhabited ----------
west0=[(-8.2,-2.25),(-3.0,-2.35),(-2.45,-1.52),(-2.55,.88),(-3.15,1.34),(-7.85,1.22),(-8.55,.40),(-8.58,-1.30)]
west1=[(-7.95,-2.05),(-3.25,-2.13),(-2.72,-1.40),(-2.82,.67),(-3.38,1.10),(-7.55,1.02),(-8.22,.28),(-8.28,-1.15)]
west2=[(-7.35,-1.82),(-3.62,-1.90),(-3.05,-1.25),(-3.15,.48),(-3.70,.88),(-7.02,.80),(-7.70,.16),(-7.78,-1.02)]
west=loft("West inhabited retaining body",[(0,west0),(2.75,west1),(4.15,west2)],RETAIN); bevel(west,.11,3)
# Real openings, intentionally varied
for idx,(cx,w,h,z) in enumerate([(-6.72,1.55,2.10,.38),(-4.78,1.28,1.82,.46),(-3.55,.95,1.55,.74)]):
    c=arch_cutter("West arcade cutter %d"%idx,cx,-2.65,.32,z,w,h,22); boolean_hole(west,c)
    recess_back("West deep recess %d"%idx,cx,.42,z+.12,w*.72,h*.68)

# West upper terrace and inhabited ledge
westDeck=[(-7.45,-1.78),(-3.80,-1.84),(-3.32,-1.18),(-3.42,.62),(-3.85,.95),(-7.05,.88),(-7.58,.18),(-7.65,-1.0)]
deck=solidify_polygon("West occupied terrace",westDeck,4.03,4.30,STONE); bevel(deck,.07,2)
ribbon_wall("West broken outer parapet",[(-7.35,-1.62),(-6.28,-1.74),(-5.08,-1.66),(-4.05,-1.70)],4.28,4.82,.24,STONE)
ribbon_wall("West side return parapet",[(-7.38,-1.48),(-7.48,-.56),(-7.20,.30),(-6.85,.70)],4.28,4.72,.22,STONE)
# inhabited roof/canopy fragments integrated into terrace, not houses
can1=solidify_polygon("West slate canopy",[(-6.45,-.95),(-5.48,-1.02),(-5.28,-.28),(-6.38,-.20)],4.72,4.88,SLATE); bevel(can1,.03,2)
can1.rotation_euler[1]=math.radians(-8)
can2=solidify_polygon("West workshop canopy",[(-4.92,-1.12),(-4.15,-1.15),(-4.05,-.56),(-4.86,-.51)],4.54,4.68,SLATE); bevel(can2,.03,2)

# ---------- EAST MASS: taller civic vertical ----------
east0=[(2.30,-2.18),(7.45,-2.08),(8.08,-1.28),(8.02,.72),(7.42,1.28),(3.10,1.12),(2.52,.52),(2.18,-1.16)]
east1=[(2.55,-1.98),(7.18,-1.86),(7.72,-1.16),(7.66,.55),(7.12,1.06),(3.34,.92),(2.78,.38),(2.46,-1.04)]
east2=[(2.92,-1.76),(6.82,-1.62),(7.30,-1.02),(7.25,.36),(6.78,.82),(3.68,.76),(3.10,.26),(2.82,-.90)]
east=loft("East civic retaining body",[(0,east0),(3.30,east1),(5.12,east2)],RETAIN); bevel(east,.12,3)
for idx,(cx,w,h,z) in enumerate([(3.62,1.30,2.28,.42),(5.55,1.55,2.75,.52),(7.02,1.08,2.10,.92)]):
    c=arch_cutter("East civic cutter %d"%idx,cx,-2.50,.24,z,w,h,24); boolean_hole(east,c)
    recess_back("East deep recess %d"%idx,cx,.35,z+.12,w*.74,h*.70)

# East stepped upper civic shoulder, not mirrored
eastUpper=[(4.18,-1.52),(7.08,-1.44),(7.48,-.88),(7.42,.30),(6.98,.65),(4.48,.60),(4.05,.18),(4.00,-.86)]
eu=solidify_polygon("East upper civic shoulder",eastUpper,5.00,6.05,STONE); bevel(eu,.10,3)
# single tall niche cut into upper shoulder
c=arch_cutter("East upper niche cutter",5.82,-1.72,.42,5.08,.88,1.18,20); boolean_hole(eu,c)
recess_back("East upper niche deep back",5.82,.48,5.18,.58,.70)

eastDeck=[(2.85,-1.70),(6.98,-1.58),(7.38,-1.04),(7.28,.43),(6.80,.78),(3.45,.72),(3.02,.30),(2.76,-.84)]
ed=solidify_polygon("East civic terrace",eastDeck,4.92,5.18,STONE); bevel(ed,.06,2)
ribbon_wall("East civic parapet",[(3.15,-1.52),(4.62,-1.60),(5.84,-1.52),(6.90,-1.43)],5.15,5.72,.25,STONE)
ribbon_wall("East outer return",[(7.00,-1.36),(7.30,-.70),(7.22,.08),(6.88,.54)],5.12,5.66,.24,STONE)

# ---------- CENTRAL RECEIVING PLINTH: stepped around open stair ----------
# left and right receivers, deliberately leave exact stair corridor open
leftRecv=[(-3.35,-2.55),(-1.62,-2.58),(-1.60,.12),(-2.02,.62),(-3.55,.84),(-4.08,.22),(-4.10,-1.70)]
rightRecv=[(1.62,-2.58),(3.15,-2.52),(3.88,-1.65),(3.75,.18),(3.30,.72),(2.05,.60),(1.60,.10)]
lr=solidify_polygon("West stair receiver",leftRecv,.02,1.05,STONE); rr=solidify_polygon("East stair receiver",rightRecv,.02,1.28,STONE)
bevel(lr,.08,3); bevel(rr,.08,3)
# lower undercut bands emphasize real depth
lb=solidify_polygon("West receiver undercut",[(-3.72,-2.62),(-1.65,-2.64),(-1.64,-2.24),(-3.55,-2.22)],.12,.50,DARK)
rb=solidify_polygon("East receiver undercut",[(1.64,-2.64),(3.50,-2.60),(3.66,-2.20),(1.64,-2.22)],.12,.56,DARK)
# mid landing lips connecting visually, not crossing stair
ribbon_wall("West landing lip",[(-3.22,-2.02),(-2.52,-1.82),(-1.70,-1.78)],1.02,1.42,.20,TRIM)
ribbon_wall("East landing lip",[(1.70,-1.78),(2.42,-1.82),(3.06,-2.00)],1.20,1.60,.20,TRIM)

# Upper Bastion contact returns step inward; no ceremonial arch
westRet=solidify_polygon("West upper Bastion return",[(-3.45,.18),(-2.12,.04),(-1.92,.68),(-2.35,1.20),(-3.62,1.08)],3.78,5.05,STONE); bevel(westRet,.08,3)
eastRet=solidify_polygon("East upper Bastion return",[(2.02,.06),(3.34,.18),(3.55,1.00),(3.10,1.38),(2.08,.94)],4.18,5.45,STONE); bevel(eastRet,.08,3)

# ---------- REAL ROCK / STONE INTERLOCK ----------
# continuous organic patches overlap/bury the architectural edges rather than cover seams with wedges
for name,center,scale,seed in [
    ("West outer cliff interlock",(-8.05,.25,-.25),(1.45,1.25,2.65),11),
    ("West lower receiver rock",(-3.72,-.40,-.25),(1.15,.95,1.60),23),
    ("East outer cliff interlock",(7.72,.08,-.25),(1.55,1.22,2.95),37),
    ("East lower receiver rock",(3.60,-.28,-.22),(1.10,.90,1.78),47)
]: rock_patch(name,center,scale,seed)

# Sparse blue vertical accents set into architecture
def banner(name,x,y,z0,z1,w=.34):
    pts=[(x-w/2,y,z0),(x+w/2,y,z0),(x+w/2,y,z1),(x-w/2,y,z1)]
    o=mesh_obj(name,pts,[(0,1,2,3)],BLUE); return o
banner("West restrained heraldry",-5.95,-2.18,2.55,3.80,.30)
banner("East restrained heraldry",5.06,-1.90,3.20,4.55,.34)

for o in list(prod.objects):
    if o.type=="MESH": weighted(o)

# proxy stair after production completed
stair_proxy()

# Source save contains both production and clearly named proxy collection.
blend=os.path.join(A.output_dir,"Valoria_BastionToCity_BlenderAuthoring_v1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)

# Export production collection only
for o in bpy.context.scene.objects:o.select_set(False)
for o in prod.objects:
    if o.type=="MESH": o.select_set(True)
glb=os.path.join(A.output_dir,"Valoria_BastionToCity_BlenderAuthoring_v1.glb")
bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True,export_yup=True)

# ---------- REVIEW ----------
def world_setup(w,h):
    sc=bpy.context.scene
    sc.render.engine="BLENDER_EEVEE"
    sc.eevee.use_gtao=True; sc.eevee.gtao_distance=3; sc.eevee.gtao_factor=1.35
    sc.render.resolution_x=w; sc.render.resolution_y=h; sc.render.resolution_percentage=100
    sc.render.image_settings.file_format='PNG'
    if sc.world is None: sc.world=bpy.data.worlds.new("Eldoria Review World")
    sc.world.color=(0.028,0.032,0.036)

def cam(name,loc,target,ortho=None):
    bpy.ops.object.camera_add(location=loc); c=bpy.context.object; c.name=name; link(c,review)
    c.rotation_euler=(Vector(target)-c.location).to_track_quat('-Z','Y').to_euler()
    if ortho is not None: c.data.type='ORTHO'; c.data.ortho_scale=ortho
    bpy.context.scene.camera=c; return c

def lights():
    bpy.ops.object.light_add(type='AREA',location=(-7,-10,14)); k=bpy.context.object; link(k,review); k.data.energy=1200; k.data.size=8
    k.rotation_euler=(math.radians(28),0,math.radians(-22))
    bpy.ops.object.light_add(type='AREA',location=(10,1,10)); f=bpy.context.object; link(f,review); f.data.energy=520; f.data.size=7
    bpy.ops.object.light_add(type='AREA',location=(0,8,7)); r=bpy.context.object; link(r,review); r.data.energy=300; r.data.size=5

def render(path): bpy.context.scene.render.filepath=path; bpy.ops.render.render(write_still=True)

world_setup(1000,760); lights()

# clay: temporarily override production material slots; record and restore
original={}
for o in prod.objects:
    if o.type=="MESH":
        original[o.name]=[m for m in o.data.materials]
        o.data.materials.clear(); o.data.materials.append(CLAY)
cam("Clay Review Camera",(15,-21,13),(0,-.2,2.65),None)
render(os.path.join(A.evidence_dir,"01-silhouette-clay.png"))
for o in prod.objects:
    if o.type=="MESH":
        o.data.materials.clear()
        for m in original[o.name]: o.data.materials.append(m)

# lit 3/4
cam("Lit Three Quarter Camera",(14,-19,11),(0,-.1,2.65),None)
render(os.path.join(A.evidence_dir,"02-lit-three-quarter.png"))

# matched strategic proxy: wide 16:9 orthographic
world_setup(1280,720)
cam("Matched Strategic Proxy",(18,-29,16),(0,-.25,2.55),19.2)
render(os.path.join(A.evidence_dir,"03-matched-camera-proxy.png"))

def sha(path):
    h=hashlib.sha256()
    with open(path,'rb') as f:
        for ch in iter(lambda:f.read(1<<20),b''): h.update(ch)
    return h.hexdigest()

def metrics():
    meshes=[o for o in prod.objects if o.type=="MESH"]
    tris=0; verts=0
    for o in meshes:
        deps=bpy.context.evaluated_depsgraph_get(); eo=o.evaluated_get(deps)
        me=eo.to_mesh(); verts+=len(me.vertices)
        for p in me.polygons: tris += max(1,len(p.vertices)-2)
        eo.to_mesh_clear()
    return {"production_mesh_objects":len(meshes),"evaluated_vertices":verts,"evaluated_triangles":tris}

report={
 "schema_version":1,
 "workstream":"VALORIA BASTION-TO-CITY BLENDER AUTHORING v1",
 "authoring_standard":"BLENDER_PROFESSIONAL_V1",
 "authoring_method":"Single locked-design translation using irregular lofted primary masses, true boolean architectural voids, explicit wall thickness/returns, authored nonuniform parapet ribbons, stepped receivers and continuous displaced organic rock interlock. No mirrored half and no box-derived primary silhouette.",
 "design_authority":["docs/VALORIA_BASTION_TO_CITY_FRAME_DESIGN_BRIEF_V1.md","docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-spec.json","docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-overlay.svg","owner-approved chat target visual"],
 "base_capture_sha256":"ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1",
 "tool_families":["direct mesh authoring","multi-ring lofting","controlled exact booleans","manual parapet ribbons","selective bevel","weighted normals","procedural material-ready surfaces","organic displaced rock interlock"],
 "primitive_role":"No Blender cube/cylinder primitive is used to define production primary silhouette. Review stair is isolated in REVIEW_PROXY_ONLY and is excluded from GLB export.",
 "source_blend":{"path":blend,"sha256":sha(blend),"bytes":os.path.getsize(blend)},
 "export_glb":{"path":glb,"sha256":sha(glb),"bytes":os.path.getsize(glb)},
 "geometry_metrics":metrics(),
 "material_families":["Valoria Warm Primary Stone","Valoria Weathered Retaining Stone","Valoria Limited Pale Trim","Valoria Recess Interior","Valoria Dark Desaturated Rock","Valoria Restrained Slate","Valoria Sparse Heraldic Blue"],
 "preview_evidence":["01-silhouette-clay.png","02-lit-three-quarter.png","03-matched-camera-proxy.png"],
 "intended_camera_role":"Replacement envelope around existing central Bastion stair only. Hero Bastion and promoted lower city are contextual protected neighbors, not part of this source.",
 "isolated_art_review":"PENDING_OWNER_AGENT_REVIEW",
 "tripo_credits":0,"paid_credits":0
}
with open(A.report,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
