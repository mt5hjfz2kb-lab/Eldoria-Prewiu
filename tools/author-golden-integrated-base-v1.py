import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector

ROOT = Path(os.environ.get("GITHUB_WORKSPACE", os.getcwd()))
SRC = ROOT / "art-source/valoria/lookdev/golden-slice-v1/integrated-base-v1"
EVID = ROOT / "docs/evidence/valoria-golden-lookdev-slice-v1/integrated-base-v1"
SURF = ROOT / "art-source/valoria/lookdev/golden-slice-v1/surface-v2"
SRC.mkdir(parents=True, exist_ok=True)
EVID.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

ORIGIN = Vector((4.0, -22.0, 7.0))
OBJS = []

def sha256(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def mat_pbr(name, base, roughness, tex_prefix=None):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*base, 1)
    bsdf.inputs["Roughness"].default_value = roughness
    if tex_prefix:
        alb = SURF / f"{tex_prefix}_albedo.png"
        nrm = SURF / f"{tex_prefix}_normal.png"
        ao = SURF / f"{tex_prefix}_ao.png"
        sm = SURF / f"{tex_prefix}_smoothness.png"
        if alb.exists():
            im = bpy.data.images.load(str(alb), check_existing=True)
            tx = m.node_tree.nodes.new("ShaderNodeTexImage"); tx.image = im
            m.node_tree.links.new(tx.outputs["Color"], bsdf.inputs["Base Color"])
        if nrm.exists():
            im = bpy.data.images.load(str(nrm), check_existing=True); im.colorspace_settings.name = "Non-Color"
            tx = m.node_tree.nodes.new("ShaderNodeTexImage"); tx.image = im
            nm = m.node_tree.nodes.new("ShaderNodeNormalMap"); nm.inputs["Strength"].default_value = .45
            m.node_tree.links.new(tx.outputs["Color"], nm.inputs["Color"])
            m.node_tree.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
        if sm.exists():
            im = bpy.data.images.load(str(sm), check_existing=True); im.colorspace_settings.name = "Non-Color"
            tx = m.node_tree.nodes.new("ShaderNodeTexImage"); tx.image = im
            inv = m.node_tree.nodes.new("ShaderNodeMath"); inv.operation = "SUBTRACT"; inv.inputs[0].default_value = 1.0
            m.node_tree.links.new(tx.outputs["Color"], inv.inputs[1])
            m.node_tree.links.new(inv.outputs[0], bsdf.inputs["Roughness"])
        if ao.exists():
            im = bpy.data.images.load(str(ao), check_existing=True); im.colorspace_settings.name = "Non-Color"
    return m

STONE = mat_pbr("Golden Integrated Stone", (.68,.59,.47), .70, "stone")
ROCK = mat_pbr("Golden Integrated Rock", (.44,.42,.37), .82, "rock")
GROUND = mat_pbr("Golden Integrated Ground", (.47,.39,.28), .88, "ground")
SHORE = mat_pbr("Golden Integrated Wet Shore", (.31,.32,.29), .46, "shore")
VEG = mat_pbr("Golden Integrated Vegetation", (.27,.42,.24), .78, "vegetation")
BARK = mat_pbr("Golden Integrated Bark", (.22,.13,.07), .86, None)

def add_uv(obj, scale=3.0):
    if obj.type != "MESH": return
    uv = obj.data.uv_layers.new(name="MetricUV")
    obj.data.uv_layers.active = uv; uv.active_render = True
    for poly in obj.data.polygons:
        axis = max(range(3), key=lambda k: abs(poly.normal[k]))
        for li in poly.loop_indices:
            vi = obj.data.loops[li].vertex_index
            v = obj.data.vertices[vi].co
            if axis == 0: co=(v.y/scale,v.z/scale)
            elif axis == 1: co=(v.x/scale,v.z/scale)
            else: co=(v.x/scale,v.y/scale)
            uv.data[li].uv = co

def smooth_weighted(obj):
    if obj.type != "MESH": return
    for p in obj.data.polygons: p.use_smooth = True
    try:
        obj.data.set_sharp_from_angle(angle=math.radians(52))
    except Exception:
        pass
    try:
        wn = obj.modifiers.new("WeightedNormals","WEIGHTED_NORMAL"); wn.keep_sharp = True; wn.weight = 50
    except Exception:
        pass

def custom_mesh(name, verts, faces, mat, uvscale=3.0, smooth=True):
    me = bpy.data.meshes.new(name+"_Mesh")
    me.from_pydata(verts, [], faces); me.update()
    obj = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    add_uv(obj, uvscale)
    if smooth: smooth_weighted(obj)
    OBJS.append(obj); return obj

def tapered_prism(name, center, length_x, depth_y, z0, z1, foot_expand, top_inset, mat, yaw=0.0):
    cx,cy = center
    sections = [
        (z0, length_x+foot_expand, depth_y+foot_expand*.55),
        (z0+.55, length_x+foot_expand*.35, depth_y+foot_expand*.18),
        (z1-.45, length_x-top_inset, depth_y-top_inset*.35),
        (z1, length_x-top_inset*.45, depth_y-top_inset*.12),
    ]
    verts=[]
    for z,lx,dy in sections:
        for sx,sy in [(-1,-1),(1,-1),(1,1),(-1,1)]:
            x=sx*lx*.5; y=sy*dy*.5
            c=math.cos(yaw); s=math.sin(yaw)
            verts.append((cx+x*c-y*s,cy+x*s+y*c,z))
    faces=[]
    for k in range(len(sections)-1):
        a=k*4;b=(k+1)*4
        for i in range(4):
            n=(i+1)%4; faces.append((a+i,b+i,b+n,a+n))
    faces.append((0,3,2,1)); t=(len(sections)-1)*4; faces.append((t,t+1,t+2,t+3))
    return custom_mesh(name, verts, faces, mat, 2.6, False)

def arch_band(name, cx, cy, zbase, rx, rz, depth, thickness, mat, segments=24, start=0.0, end=math.pi):
    verts=[]; faces=[]
    for side in (-1,1):
        y=cy + side*depth*.5
        for ring in (0,1):
            rr_x=rx + (thickness if ring==1 else 0)
            rr_z=rz + (thickness if ring==1 else 0)
            for i in range(segments+1):
                t=start+(end-start)*i/segments
                verts.append((cx+math.cos(t)*rr_x, y, zbase+math.sin(t)*rr_z))
    ring_count=segments+1
    # front and back band surfaces
    for side_i in range(2):
        off=side_i*2*ring_count
        for i in range(segments):
            a=off+i; b=off+i+1; c=off+ring_count+i+1; d=off+ring_count+i
            faces.append((a,b,c,d))
    # connect inner and outer around depth
    for ring in range(2):
        f=ring*ring_count; b=2*ring_count+ring*ring_count
        for i in range(segments):
            faces.append((f+i,b+i,b+i+1,f+i+1))
    # end caps
    for i in (0,segments):
        faces.append((i,ring_count+i,3*ring_count+i,2*ring_count+i))
    return custom_mesh(name, verts, faces, mat, 2.1, True)

def cap_curve(name, pts, bevel, mat):
    cu=bpy.data.curves.new(name+"_Curve","CURVE"); cu.dimensions="3D"; cu.resolution_u=2
    spl=cu.splines.new("BEZIER"); spl.bezier_points.add(len(pts)-1)
    for p,co in zip(spl.bezier_points,pts):
        p.co=co; p.handle_left_type="AUTO"; p.handle_right_type="AUTO"
    cu.bevel_depth=bevel; cu.bevel_resolution=3; cu.resolution_u=12
    obj=bpy.data.objects.new(name,cu); bpy.context.collection.objects.link(obj); obj.data.materials.append(mat)
    OBJS.append(obj); return obj

# --- Lower Gate structural edge profile ---
# Existing macro opening remains centered near x=4.6, y=-24.4, base z~7.
gate_cx=4.55; gate_y=-24.45
tapered_prism("GateEdge_WestBuriedPlinth",(-3.25,gate_y),5.7,2.7,6.52,11.45,1.65,.58,STONE,math.radians(-2.5))
tapered_prism("GateEdge_EastBuriedPlinth",(12.35,gate_y+.10),5.3,2.6,6.46,11.15,1.85,.52,STONE,math.radians(2.0))
arch_band("GateEdge_DeepArchLip",gate_cx,gate_y-.18,7.05,4.08,4.25,1.22,.52,STONE,28)
# asymmetric stepped lower shoulders and selective edge hierarchy
cap_curve("GateEdge_WestShoulderCap",[(-6.1,-25.05,8.05),(-4.2,-24.95,8.34),(-1.7,-24.82,8.12)],.18,STONE)
cap_curve("GateEdge_EastShoulderCap",[(9.8,-24.78,8.08),(12.0,-24.72,8.39),(15.1,-24.52,8.17)],.18,STONE)

# --- Bridge support / edge ---
bridge_cx=4.50; bridge_y=-30.10
# tapered spring blocks deliberately offset
tapered_prism("BridgeEdge_WestAbutment",(-.15,-29.72),3.8,3.2,.20,4.75,1.25,.42,STONE,math.radians(-3))
tapered_prism("BridgeEdge_EastAbutment",(9.12,-29.80),3.55,3.1,.18,4.55,1.35,.38,STONE,math.radians(2.5))
arch_band("BridgeEdge_LoadArch",bridge_cx,bridge_y,1.00,4.55,3.35,1.38,.44,STONE,30)
cap_curve("BridgeEdge_CamberedDeckCap",[(-.2,-29.47,4.65),(2.1,-29.58,4.90),(4.6,-29.60,5.02),(7.0,-29.58,4.91),(9.25,-29.43,4.66)],.17,STONE)

# --- Continuous authored banks ---
def bank_mesh(name, x_outer, x_inner_fn, side):
    ys=[-35.4,-34.1,-32.7,-31.2,-29.7,-28.0,-26.2,-24.7,-22.8,-20.5,-18.0]
    cols=11
    verts=[]
    for j,y in enumerate(ys):
        t=j/(len(ys)-1)
        inner=x_inner_fn(y)
        for i in range(cols):
            u=i/(cols-1)
            x=x_outer*(1-u)+inner*u
            # designed grade: shore shelf -> fractured rise -> upper ground
            rise=7.0/(1+math.exp(-(y+29.0)*1.28))
            terrace=.14*math.sin((u*2.7+j*.31)*math.pi)+.08*math.sin(x*.43+y*.28)
            erosion=.12*math.sin(y*.72 + u*5.7) * (1.0-abs(2*u-1)*.35)
            if y < -33.0: rise=.18+(y+35.4)*.18
            z=rise+terrace+erosion
            # keep central route shoulder slightly calmer
            if u>.72 and y>-27: z=6.95+.06*math.sin(y*.55+x*.34)
            verts.append((x,y,z))
    faces=[]
    for j in range(len(ys)-1):
        for i in range(cols-1):
            a=j*cols+i; b=a+1; c=a+cols; d=c+1
            if (i+j)%2: faces += [(a,c,b),(b,c,d)]
            else: faces += [(a,c,d),(a,d,b)]
    obj=custom_mesh(name,verts,faces,GROUND,3.4,True)
    # assign material by screen-space geological zone
    obj.data.materials.append(ROCK); obj.data.materials.append(SHORE)
    for p in obj.data.polygons:
        cz=sum(obj.data.vertices[v].co.z for v in p.vertices)/len(p.vertices)
        cy=sum(obj.data.vertices[v].co.y for v in p.vertices)/len(p.vertices)
        if cy < -33.0 or cz < .8: p.material_index=2
        elif cz < 5.35: p.material_index=1
        else: p.material_index=0
    return obj

west=bank_mesh("IntegratedBank_West",-18.5,lambda y: 1.0 + .32*math.sin((y+29)*.52),-1)
east=bank_mesh("IntegratedBank_East",27.5,lambda y: 8.0 + .28*math.sin((y+31)*.57),1)

# Authored rock bedding ridges: structural fracture lines, not cosmetic overlays.
for idx,(pts,rad) in enumerate([
    ([(-14.2,-31.4,2.5),(-10.2,-30.0,3.2),(-6.4,-28.8,4.1),(-2.6,-27.8,4.9)],.24),
    ([(11.0,-31.5,2.3),(14.0,-30.0,3.15),(18.1,-28.6,4.05),(22.2,-27.1,4.8)],.25),
    ([(-12.4,-28.8,4.2),(-8.6,-27.5,5.0),(-4.7,-26.4,5.7)],.18),
    ([(12.3,-28.7,4.0),(16.4,-27.2,4.9),(20.4,-26.2,5.55)],.18),
]):
    cap_curve(f"RockStrata_Ridge_{idx:02d}",pts,rad,ROCK)

# Embedded shore stones with designed flattened forms
def irregular_rock(name,c,scale,phase,wet=False):
    cx,cy,cz=c; sx,sy,sz=scale; seg=18; rings=8; verts=[(cx,cy,cz+sz)]; faces=[]
    for j in range(1,rings):
        ph=math.pi*j/rings
        for i in range(seg):
            th=2*math.pi*i/seg
            amp=1+.11*math.sin(th*3+phase)+.055*math.sin(th*5+ph*2+phase*.7)
            verts.append((cx+sx*math.sin(ph)*math.cos(th)*amp,
                          cy+sy*math.sin(ph)*math.sin(th)*(1+.05*math.sin(th*4+phase)),
                          cz+sz*math.cos(ph)*(1+.04*math.cos(th*3+phase))))
    bot=len(verts); verts.append((cx,cy,cz-sz*.62))
    for i in range(seg): faces.append((0,1+i,1+(i+1)%seg))
    for j in range(rings-2):
        a=1+j*seg;b=a+seg
        for i in range(seg):
            n=(i+1)%seg; faces += [(a+i,b+i,a+n),(a+n,b+i,b+n)]
    last=1+(rings-2)*seg
    for i in range(seg): faces.append((last+i,bot,last+(i+1)%seg))
    return custom_mesh(name,verts,faces,SHORE if wet else ROCK,2.2,True)

for spec in [
    ("ShoreRock_W0",(-3.8,-34.0,.48),(1.6,1.0,.62),.4,True),
    ("ShoreRock_W1",(-7.2,-33.1,.78),(1.25,.9,.76),1.2,False),
    ("ShoreRock_E0",(11.0,-33.7,.55),(1.55,1.05,.65),2.0,True),
    ("ShoreRock_E1",(15.1,-32.8,.95),(1.35,.92,.82),2.8,False),
]:
    irregular_rock(*spec)

# --- Three asymmetrical conifers ---
def conifer(name, pos, height, radius, phase):
    x,y,z=pos
    # tapered trunk curve
    cu=bpy.data.curves.new(name+"_TrunkCurve","CURVE"); cu.dimensions="3D"
    sp=cu.splines.new("BEZIER"); sp.bezier_points.add(2)
    pts=[(x,y,z),(x+.08*math.sin(phase),y+.05*math.cos(phase),z+height*.48),(x-.10*math.cos(phase),y+.08*math.sin(phase),z+height*.92)]
    for p,co in zip(sp.bezier_points,pts): p.co=co;p.handle_left_type="AUTO";p.handle_right_type="AUTO"
    cu.bevel_depth=radius*.10;cu.bevel_resolution=3
    tr=bpy.data.objects.new(name+"_Trunk",cu);bpy.context.collection.objects.link(tr);tr.data.materials.append(BARK);OBJS.append(tr)
    # branch whorls as swept curves with non-uniform spread
    for level,(zf,spread) in enumerate([(0.28,1.0),(0.40,.92),(0.53,.78),(0.65,.64),(0.76,.48),(0.85,.31)]):
        count=5 if level<3 else 4
        for k in range(count):
            a=2*math.pi*k/count + phase + level*.37
            r=radius*spread*(.86+.18*math.sin(k*1.7+phase))
            start=Vector((x,y,z+height*zf))
            mid=start+Vector((math.cos(a)*r*.55,math.sin(a)*r*.55,-height*.025))
            end=start+Vector((math.cos(a)*r,math.sin(a)*r,-height*(.06+.01*level)))
            cap_curve(f"{name}_Branch_{level}_{k}",[start,mid,end],radius*(.055 if level<2 else .043),VEG)

conifer("TreeIntegrated_WestGate",(-6.0,-20.2,6.95),8.6,2.4,.25)
conifer("TreeIntegrated_ForegroundWest",(-9.5,-32.2,1.7),9.5,2.75,1.15)
conifer("TreeIntegrated_EastBank",(22.6,-27.0,4.5),8.9,2.55,2.0)

# Convert curves to mesh for deterministic export and add UVs.
for obj in list(OBJS):
    if obj.type=="CURVE":
        bpy.context.view_layer.objects.active=obj; obj.select_set(True)
        bpy.ops.object.convert(target="MESH"); obj.select_set(False)
        add_uv(obj,2.0); smooth_weighted(obj)

# Shared pivot and source save.
for o in OBJS:
    if o.name not in bpy.context.scene.objects: continue
    bpy.context.scene.cursor.location=ORIGIN
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR");o.select_set(False)

blend=SRC/"GoldenIntegratedBaseV1.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(blend))

# Export in Golden local-shell coordinate convention.
for o in OBJS:
    if o.name in bpy.context.scene.objects: o.location -= ORIGIN
bpy.ops.object.select_all(action="DESELECT")
for o in OBJS:
    if o.name in bpy.context.scene.objects: o.select_set(True)
glb=SRC/"GoldenIntegratedBaseV1.glb"
bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True,export_apply=True,export_yup=True,
                          export_materials="EXPORT",export_normals=True,export_tangents=True)
for o in OBJS:
    if o.name in bpy.context.scene.objects: o.location += ORIGIN
bpy.context.view_layer.update()

# Isolated hero previews.
scene=bpy.context.scene; scene.render.engine="BLENDER_EEVEE"
scene.render.resolution_x=1400;scene.render.resolution_y=900;scene.render.resolution_percentage=100
world=bpy.data.worlds.new("Golden Integrated Neutral");world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.18,.20,.23,1)
world.node_tree.nodes["Background"].inputs[1].default_value=.52;scene.world=world
center=Vector((4.5,-27.0,5.0))
for loc,energy,size,warm in [
    ((-18,-44,30),1800,11,(1.0,.83,.66)),
    ((24,-17,20),820,9,(.65,.76,1.0)),
]:
    bpy.ops.object.light_add(type="AREA",location=loc);l=bpy.context.object;l.data.energy=energy;l.data.size=size
    l.data.color=warm;l.rotation_euler=(center-l.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";scene.camera=cam
clay=bpy.data.materials.new("Integrated Clay");clay.diffuse_color=(.56,.54,.50,1);clay.roughness=.9
views=[
    ("source-lit-three-quarter",(28,-50,27),41,False),
    ("source-official-proxy",(24,-54,29),38,False),
    ("source-clay",(28,-50,27),41,True),
]
previews=[]
for name,loc,span,use_clay in views:
    cam.location=loc;cam.data.ortho_scale=span;cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.view_layers[0].material_override=clay if use_clay else None
    out=EVID/(name+".png");scene.render.filepath=str(out);bpy.ops.render.render(write_still=True)
    previews.append(str(out.relative_to(ROOT)).replace("\\","/"))
scene.view_layers[0].material_override=None

tris=verts=0; bounds=[]; mats=set()
for o in OBJS:
    if o.type!="MESH" or o.name not in bpy.context.scene.objects: continue
    o.data.calc_loop_triangles(); tris+=len(o.data.loop_triangles); verts+=len(o.data.vertices)
    for m in o.data.materials:
        if m:mats.add(m.name)
    bounds += [o.matrix_world@Vector(c) for c in o.bound_box]
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
report={
    "authoring_standard":"BLENDER_PROFESSIONAL_V1",
    "family":"GoldenIntegratedBaseV1",
    "method":"OWNER-AUTHORIZED METHOD B / integrated Lower Gate + Bridge edge profiles + continuous substrate micro-environment reauthor",
    "primitive_role":"No primitive stack used for primary silhouette. Primary architecture uses custom tapered section meshes, swept profile curves and explicit arch bands; terrain uses continuous authored bank meshes.",
    "tool_families":["mesh_edit","curves_profiles","selective_profile_depth","custom_weighted_normals","uv_unwrap_texel_density","authored_terrain_mesh","controlled_organic_form"],
    "source":str(blend.relative_to(ROOT)).replace("\\","/"),
    "export":str(glb.relative_to(ROOT)).replace("\\","/"),
    "source_sha":sha256(blend),"export_sha":sha256(glb),
    "geometry_metrics":{"triangles":tris,"vertices":verts,"objects":len([o for o in OBJS if o.name in bpy.context.scene.objects]),"materials":sorted(mats),"bounds_world_blender":[lo,hi],"uv":True,"normals":True,"tangents_exported":True},
    "surface_basis":"Golden Surface V2 reused for stone/rock/ground/shore/vegetation where compatible",
    "preview_evidence":previews,
    "unity_placement":{"position":[ORIGIN.x,ORIGIN.z,ORIGIN.y],"rotation_y":180,"scale":1},
    "scope_guards":{"camera_changed":False,"macro_gate_position_changed":False,"macro_bridge_position_changed":False,"gameplay_geometry":False,"colliders":0,"tripo_credits":0},
    "isolated_art_review":"PENDING"
}
(EVID/"source-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print(json.dumps(report,indent=2))
