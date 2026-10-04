import bpy, bmesh, math, os, json, hashlib
from mathutils import Vector

"""
VALORIA PRODUCTION ART STARTER FAMILY v1
High-fidelity architectural source generator.

This deliberately does NOT use the old "stack a few cubes and call it final art" route.
Primary masses are shaped with boolean openings, custom roof solids, arches, recessed
facades, cornices, buttresses, timber frames, foundation transitions and modular trim.

Execution target: Blender 4.x.
Outputs:
  art-source/valoria/production/starter-family/Valoria_StarterFamily_v1.blend
  Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/StarterFamily/*.glb
  art-source/.../*.production-art.json
"""

ROOT = os.environ.get("GITHUB_WORKSPACE", os.getcwd())
SRC = os.path.join(ROOT, "art-source", "valoria", "production", "starter-family")
OUT = os.path.join(ROOT, "Unity", "Assets", "Eldoria", "Resources", "Valoria", "ProductionArt", "StarterFamily")
os.makedirs(SRC, exist_ok=True)
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)

# ---------- materials ----------

def mat_principled(name, base, rough=.72, metal=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bs = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bs.inputs["Base Color"].default_value = (*base, 1)
    bs.inputs["Roughness"].default_value = rough
    bs.inputs["Metallic"].default_value = metal
    return m

STONE = mat_principled("Eldoria Stone", (0.46,0.39,0.30), .78)
STONE2 = mat_principled("Eldoria Stone Dark", (0.29,0.26,0.22), .86)
TIMBER = mat_principled("Eldoria Timber", (0.12,0.065,0.032), .78)
SLATE = mat_principled("Eldoria Slate", (0.065,0.085,0.105), .90)
METAL = mat_principled("Eldoria Metal Accent", (0.22,0.17,0.09), .48, .35)
BLUE = mat_principled("Eldoria Heraldry Blue", (0.035,0.13,0.32), .62)
WINDOW = mat_principled("Eldoria Warm Window", (0.58,0.19,0.045), .38)
PLASTER = mat_principled("Eldoria Warm Plaster", (0.52,0.40,0.27), .88)

# ---------- generic geometry ----------

def apply_bevel(o, width=.035, segments=3):
    mod = o.modifiers.new("Authored bevel", "BEVEL")
    mod.width = width
    mod.segments = segments
    mod.limit_method = "ANGLE"
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier=mod.name)

def cube(name, loc, scale, mat, bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.object
    o.name = name
    o.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        apply_bevel(o, bevel, 3)
    o.data.materials.append(mat)
    return o

def custom_prism(name, verts2d, depth, loc=(0,0,0), mat=None, bevel=.025):
    # Extrude a custom x/z polygon along Y.
    y0, y1 = -depth*.5, depth*.5
    verts = [(x,y0,z) for x,z in verts2d] + [(x,y1,z) for x,z in verts2d]
    n = len(verts2d)
    faces = []
    faces.append(tuple(range(n)))
    faces.append(tuple(range(n,2*n))[::-1])
    for i in range(n):
        j = (i+1)%n
        faces.append((i,j,n+j,n+i))
    me = bpy.data.meshes.new(name+" Mesh")
    me.from_pydata(verts, [], faces)
    me.update()
    o = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(o)
    o.location = loc
    if mat: o.data.materials.append(mat)
    if bevel: apply_bevel(o, bevel, 3)
    return o

def arch_cutter(name, width, spring_z, radius, depth, total_height):
    # Union-like cutter built from box + half-cylinder.
    parts=[]
    parts.append(cube(name+" lower",(0,0,total_height*.28),(width,depth,total_height*.56),STONE,0))
    bpy.ops.mesh.primitive_cylinder_add(vertices=64, radius=radius, depth=depth, location=(0,0,spring_z), rotation=(math.pi/2,0,0))
    cyl=bpy.context.object; cyl.name=name+" arch"
    # Cut lower half by intersecting with a box using boolean.
    clip=cube(name+" clip",(0,0,spring_z+radius*.5),(width*1.3,depth*1.2,radius),(STONE),0)
    mod=cyl.modifiers.new("Half arch","BOOLEAN");mod.operation="INTERSECT";mod.solver="EXACT";mod.object=clip
    bpy.context.view_layer.objects.active=cyl;bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(clip, do_unlink=True)
    parts.append(cyl)
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:p.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    parts[0].name=name
    return parts[0]

def boolean_difference(target, cutter):
    mod=target.modifiers.new("Architectural opening","BOOLEAN")
    mod.operation="DIFFERENCE";mod.solver="EXACT";mod.object=cutter
    bpy.context.view_layer.objects.active=target
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(cutter, do_unlink=True)

def roof_gable_solid(name, width, depth, eave_z, ridge_z, thickness=.12, overhang=.18, mat=SLATE):
    w=width*.5+overhang; d=depth*.5+overhang
    verts=[
        (-w,-d,eave_z),(0,-d,ridge_z),(w,-d,eave_z),
        (-w,d,eave_z),(0,d,ridge_z),(w,d,eave_z),
        (-w,-d,eave_z-thickness),(0,-d,ridge_z-thickness),(w,-d,eave_z-thickness),
        (-w,d,eave_z-thickness),(0,d,ridge_z-thickness),(w,d,eave_z-thickness)
    ]
    faces=[
        (0,1,4,3),(1,2,5,4),       # top planes
        (8,7,10,11),(7,6,9,10),    # underside
        (0,6,7,1),(1,7,8,2),
        (3,4,10,9),(4,5,11,10),
        (0,3,9,6),(2,8,11,5)
    ]
    me=bpy.data.meshes.new(name+" Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
    apply_bevel(o,.025,3)
    return o

def cornice(name, center, width, depth, z, mat=STONE):
    return cube(name,(center[0],center[1],z),(width,depth,.15),mat,.025)

def timber_beam(name, loc, scale, rotz=0):
    o=cube(name,loc,scale,TIMBER,.012);o.rotation_euler[2]=math.radians(rotz);return o

def add_window_recess(parent_mass, x, y_front, z, w=.46, h=.58, depth=.28, frame=True):
    made=[]
    cut=cube("window cutter",(x,y_front,z),(w,depth,h),STONE,0)
    boolean_difference(parent_mass,cut)
    recess=cube("window recess",(x,y_front+depth*.28,z),(w*.82,.08,h*.84),WINDOW,.006)
    made.append(recess)
    if frame:
        for dx,dz,sx,sz in [
            (-w*.5-.035,0,.07,h+.16),(w*.5+.035,0,.07,h+.16),
            (0,h*.5+.035,w+.16,.07),(0,-h*.5-.035,w+.16,.07)]:
            made.append(timber_beam("window frame",(x+dx,y_front-.055,z+dz),(sx,.09,sz)))
    return made

def add_door_recess(parent_mass, x, y_front, z_bottom, w=.62, h=1.18, depth=.38):
    cut=cube("door cutter",(x,y_front,z_bottom+h*.5),(w,depth,h),STONE,0)
    boolean_difference(parent_mass,cut)
    return [cube("door leaf",(x,y_front+depth*.30,z_bottom+h*.5),(w*.86,.10,h*.92),TIMBER,.012)]

def add_buttress(x,y,z,h,depth=.48,width=.42):
    # Tapered profile instead of plain cube.
    poly=[(-width*.5,0),(width*.5,0),(width*.38,h),( -width*.38,h)]
    return custom_prism("stone buttress",poly,depth,(x,y,z),STONE,.02)

def add_arch_ring(cx, front_y, spring_z, radius, count=13, depth=.22, block_w=.30, block_h=.38):
    made=[]
    for i in range(count):
        a=math.pi*(i/(count-1))
        x=cx+math.cos(a)*radius
        z=spring_z+math.sin(a)*radius
        o=cube("arch voussoir",(x,front_y,z),(block_w,depth,block_h),STONE,.015)
        o.rotation_euler[1]=a-math.pi/2
        made.append(o)
    return made

def collection(name):
    c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c);return c

def move_objects_to_collection(objs,col):
    for o in objs:
        if o is None: continue
        for c in list(o.users_collection): c.objects.unlink(o)
        col.objects.link(o)

# ---------- asset 1: wall ----------

def build_wall():
    col=collection("PA_Wall_v1"); objs=[]
    core=cube("wall masonry core",(0,0,1.25),(4.6,.72,2.35),STONE,.045);objs.append(core)
    # recessed blind arches break the long plane.
    for x in (-1.45,0,1.45):
        cut=arch_cutter("blind arch cutter",.58,.95,.29,.38,1.15)
        cut.location=(x,-.31,.22)
        boolean_difference(core,cut)
        objs.append(cube("blind arch shadow",(x,-.365,.83),(.44,.06,.84),STONE2,.012))
    # deep base + cornice + buttresses
    objs += [cube("wall plinth",(0,0,.18),(4.9,.92,.36),STONE2,.035),
             cornice("wall lower string",(0,0),4.78,.84,.64),
             cornice("wall upper string",(0,0),4.82,.82,2.28)]
    for x in (-2.18,2.18):
        objs.append(add_buttress(x,-.46,.0,2.42,.60,.52))
    # crenellation with real gaps and rear parapet mass
    objs.append(cube("wall parapet",(0,0,2.58),(4.72,.78,.42),STONE,.03))
    for x in (-2.0,-1.34,-.67,0,.67,1.34,2.0):
        objs.append(cube("wall merlon",(x,-.02,3.02),(.38,.80,.48),STONE,.025))
    move_objects_to_collection(objs,col); return col

# ---------- asset 2: tower ----------

def build_tower():
    col=collection("PA_Tower_v1");objs=[]
    body=cube("tower body",(0,0,1.55),(2.85,2.85,2.85),STONE,.06);objs.append(body)
    # recessed windows on front and side; genuine cut geometry.
    for x in (-.62,.62):
        objs += add_window_recess(body,x,-1.36,1.55,.36,.70,.30)
    # doorway
    objs += add_door_recess(body,0,-1.38,.18,.70,1.25,.34)
    objs += [cube("tower plinth",(0,0,.20),(3.15,3.15,.40),STONE2,.045),
             cornice("tower belt",(0,0),3.02,3.02,1.02),
             cornice("tower crown",(0,0),3.08,3.08,2.92)]
    # corner buttresses
    for sx in (-1,1):
        for sy in (-1,1):
            b=add_buttress(sx*1.40,sy*1.40,0,2.95,.56,.46);b.rotation_euler[2]=math.radians(45*(sx*sy));objs.append(b)
    # machicolation shelf
    objs.append(cube("tower machicolation",(0,0,3.18),(3.32,3.32,.22),STONE,.028))
    for x in (-1.22,-.61,0,.61,1.22):
        objs.append(cube("front corbel",(x,-1.57,3.02),(.24,.34,.34),STONE2,.018))
    roof=roof_gable_solid("tower high roof",3.25,3.25,3.48,5.10,.16,.20,SLATE);roof.rotation_euler[2]=math.radians(90);objs.append(roof)
    objs.append(cube("tower finial",(0,0,5.32),(.08,.08,.54),METAL,.01))
    banner=cube("tower banner",(0,-1.50,2.15),(.52,.035,1.10),BLUE,.008);objs.append(banner)
    move_objects_to_collection(objs,col); return col

# ---------- asset 3: gate ----------

def build_gate():
    col=collection("PA_Gate_v1");objs=[]
    # central gatehouse with actual arched void.
    body=cube("gatehouse",(0,0,1.65),(4.25,1.55,3.05),STONE,.055);objs.append(body)
    cutter=arch_cutter("main gate opening",1.72,1.40,.86,2.0,2.6)
    cutter.location=(0,-.10,.12)
    boolean_difference(body,cutter)
    objs += add_arch_ring(0,-.83,1.48,1.02,15,.24,.28,.42)
    # recessed portcullis + door thickness.
    objs.append(cube("recessed gate",(0,.44,1.02),(1.54,.10,1.90),TIMBER,.015))
    # flank piers / buttresses
    for x in (-1.72,1.72):
        objs.append(add_buttress(x,-.88,0,3.22,.72,.54))
    objs += [cube("gate plinth",(0,0,.20),(4.58,1.82,.40),STONE2,.04),
             cornice("gate lower course",(0,0),4.45,1.68,.62),
             cornice("gate crown",(0,0),4.55,1.72,3.08)]
    # side towers are not full duplicate boxes; custom octagonal cylinders + roof.
    for x in (-2.72,2.72):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=1.16,depth=3.25,location=(x,0,1.62),rotation=(0,0,math.radians(22.5)))
        t=bpy.context.object;t.name="gate octagonal tower";t.data.materials.append(STONE);apply_bevel(t,.035,3);objs.append(t)
        objs.append(cube("tower base",(x,0,.20),(2.35,2.35,.40),STONE2,.04))
        roof=roof_gable_solid("gate tower roof",2.30,2.30,3.32,4.62,.14,.14,SLATE);roof.location.x=x;roof.rotation_euler[2]=math.radians(90);objs.append(roof)
        objs.append(cube("tower banner",(x,-1.12,2.08),(.44,.035,.96),BLUE,.008))
    move_objects_to_collection(objs,col); return col

# ---------- asset 4: civic house ----------

def build_civic_house():
    col=collection("PA_CivicHouse_v1");objs=[]
    base=cube("civic stone ground floor",(0,0,.72),(3.55,2.55,1.32),STONE,.045);objs.append(base)
    objs += add_door_recess(base,0,-1.22,.16,.72,1.10,.36)
    for x in (-1.04,1.04):
        objs += add_window_recess(base,x,-1.20,.78,.46,.55,.28)
    upper=cube("civic upper floor",(0,0,1.78),(3.32,2.38,.88),PLASTER,.035);objs.append(upper)
    for x in (-1.02,0,1.02):
        objs += add_window_recess(upper,x,-1.12,1.80,.42,.52,.24)
    # Structural timber grid + diagonal bracing, with projecting floor beam.
    objs.append(timber_beam("projecting sill",(0,-1.24,1.31),(3.48,.16,.18)))
    for x in (-1.46,-.72,0,.72,1.46):
        objs.append(timber_beam("upper post",(x,-1.22,1.78),(.14,.12,.92)))
    for x in (-.72,.72):
        objs.append(timber_beam("diagonal brace",(x,-1.235,1.77),(1.10,.10,.13),34 if x<0 else -34))
    # Deep eaves and roof thickness.
    objs.append(roof_gable_solid("civic slate roof",3.65,2.72,2.28,3.60,.16,.24,SLATE))
    # Dormer with actual recess.
    dorm=cube("dormer body",(0,-.58,2.72),(1.04,.78,.72),PLASTER,.025);objs.append(dorm)
    objs += add_window_recess(dorm,0,-.96,2.73,.38,.40,.16)
    dorm_roof=roof_gable_solid("dormer roof",1.24,.92,3.04,3.52,.12,.12,SLATE);dorm_roof.location.y=-.58;objs.append(dorm_roof)
    # Porch and foundation transition.
    objs.append(cube("stone entry stoop",(0,-1.52,.12),(1.38,.72,.24),STONE2,.025))
    porch=roof_gable_solid("porch roof",1.52,1.10,1.30,1.74,.10,.14,SLATE);porch.location.y=-1.48;objs.append(porch)
    for x in (-.58,.58): objs.append(timber_beam("porch post",(x,-1.77,.70),(.12,.12,1.18)))
    # Chimney with cap.
    objs.append(cube("chimney",(1.05,.38,2.84),(.38,.38,1.35),STONE2,.025))
    objs.append(cube("chimney cap",(1.05,.38,3.53),(.48,.48,.14),STONE,.018))
    move_objects_to_collection(objs,col); return col

# ---------- asset 5: workshop ----------

def build_workshop():
    col=collection("PA_Workshop_v1");objs=[]
    # Irregular footprint creates non-box silhouette.
    poly=[(-1.95,0),(1.55,0),(1.72,1.70),(-1.62,1.82)]
    shell=custom_prism("workshop masonry shell",poly,2.55,(0,0,.0),STONE,.045)
    shell.rotation_euler[0]=math.radians(90)  # custom prism axes adapted below by final transforms
    # Use explicit authored visible masses for robustness.
    bpy.data.objects.remove(shell,do_unlink=True)
    hall=cube("workshop main hall",(-.18,0,.78),(3.55,2.60,1.48),STONE,.045);objs.append(hall)
    objs += add_door_recess(hall,.42,-1.25,.14,.92,1.24,.36)
    objs += add_window_recess(hall,-1.02,-1.24,.82,.50,.58,.28)
    # side annex offsets silhouette and function.
    annex=cube("workshop timber annex",(1.64,.32,.64),(1.25,1.95,1.18),PLASTER,.035);objs.append(annex)
    objs += add_window_recess(annex,1.64,-.62,.72,.38,.46,.20)
    for x in (-1.56,-.78,0,.78,1.56):
        objs.append(timber_beam("workshop frame",(x,-1.28,.82),(.16,.12,1.56)))
    objs.append(timber_beam("workshop lintel",(0,-1.29,1.40),(3.35,.13,.16)))
    # lean-to functional canopy
    canopy=roof_gable_solid("workshop main roof",3.85,2.92,1.54,2.90,.15,.22,SLATE);objs.append(canopy)
    lean=cube("lean-to roof",(1.85,-.25,1.52),(1.62,1.85,.12),SLATE,.02);lean.rotation_euler[1]=math.radians(-14);objs.append(lean)
    for z in (.36,.92):
        objs.append(timber_beam("annex horizontal",(1.64,-.66,z),(1.18,.11,.13)))
    # work identity
    objs.append(cube("forge chimney",(-1.28,.55,2.18),(.42,.42,1.80),STONE2,.03))
    objs.append(cube("forge chimney cap",(-1.28,.55,3.10),(.54,.54,.16),STONE,.02))
    objs.append(cube("workbench",(1.70,-1.10,.42),(1.15,.48,.18),TIMBER,.02))
    for x in (1.30,2.10):objs.append(timber_beam("workbench leg",(x,-1.10,.22),(.12,.12,.46)))
    move_objects_to_collection(objs,col); return col

# ---------- UV / cleanup / export ----------

def uv_all(col):
    for o in col.objects:
        if o.type!="MESH": continue
        bpy.context.view_layer.objects.active=o;o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT");bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=.02)
        bpy.ops.object.mode_set(mode="OBJECT");o.select_set(False)

def shade_normals(col):
    for o in col.objects:
        if o.type!="MESH": continue
        for p in o.data.polygons:p.use_smooth=False

def export_collection(col, filename):
    bpy.ops.object.select_all(action="DESELECT")
    for o in col.objects:
        if o.type=="MESH":o.select_set(True)
    path=os.path.join(OUT,filename)
    bpy.ops.export_scene.gltf(filepath=path, export_format="GLB", use_selection=True,
                              export_apply=True, export_yup=True, export_materials="EXPORT")
    return path

def sha256(path):
    h=hashlib.sha256()
    with open(path,"rb") as f:
        for chunk in iter(lambda:f.read(1024*1024),b""):h.update(chunk)
    return h.hexdigest()

families=[
    ("Valoria_MainGate_v1", build_gate),
    ("Valoria_WallSegment_v1", build_wall),
    ("Valoria_Tower_v1", build_tower),
    ("Valoria_CivicHouse_v1", build_civic_house),
    ("Valoria_Workshop_v1", build_workshop),
]

manifest_assets=[]
for asset_id,builder in families:
    col=builder();uv_all(col);shade_normals(col)
    glb=export_collection(col,asset_id+".glb")
    tris=sum(len(o.data.loop_triangles) if (o.data.calc_loop_triangles() or True) else 0 for o in col.objects if o.type=="MESH")
    manifest_assets.append({
        "asset_id":asset_id,
        "classification":"TEMPORARY",
        "role":"PRIMARY" if "Gate" in asset_id or "Tower" in asset_id else "SECONDARY",
        "function":asset_id.replace("Valoria_","").replace("_v1",""),
        "geometry":{"triangles":tris,"materials":len({m.name for o in col.objects if o.type=="MESH" for m in o.data.materials if m}),"uv0":True,"normals":True},
        "materials":[
            {"slot":"stone","family":"Eldoria_Stone","maps":["authoring-procedural-placeholder"]},
            {"slot":"timber","family":"Eldoria_Timber","maps":["authoring-procedural-placeholder"]},
            {"slot":"slate","family":"Eldoria_Slate","maps":["authoring-procedural-placeholder"]}
        ],
        "export":{"glb_path":os.path.relpath(glb,ROOT).replace("\\","/"),"sha256":sha256(glb),"scale":1.0,"forward_axis":"+Z","up_axis":"+Y"},
        "unity":{"resource_path":"Valoria/ProductionArt/StarterFamily/"+asset_id,"owns_gameplay_collider":False,"owns_hotspot":False},
        "evidence":{"zoom9":"","mobile":"","technical_verdict":"PENDING_UNITY","visual_verdict":"PENDING_VISUAL","promotion_state":"CANDIDATE_NOT_PRODUCTION"}
    })

blend_path=os.path.join(SRC,"Valoria_StarterFamily_v1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend_path)
blend_sha=sha256(blend_path)

for m in manifest_assets:
    m["source"]={"tool":"Blender","tool_version":"4.x","path":os.path.relpath(blend_path,ROOT).replace("\\","/"),"sha256":blend_sha,"reproducible":True,"authoring_method":"architectural_boolean_custom_mesh"}
    manifest_path=os.path.join(SRC,m["asset_id"]+".production-art.json")
    with open(manifest_path,"w",encoding="utf8") as f:json.dump(m,f,indent=2)

with open(os.path.join(SRC,"starter-family-build-report.json"),"w",encoding="utf8") as f:
    json.dump({"source_blend":os.path.relpath(blend_path,ROOT).replace("\\","/"),"source_sha256":blend_sha,"assets":manifest_assets},f,indent=2)

print("VALORIA_PRODUCTION_ART_STARTER_SOURCE=BUILT")
