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

# ---------- production-detail helpers ----------

def cyl(name, loc, radius, height, mat=STONE, vertices=12, bevel=.025, rotz=0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=height,
        location=(loc[0],loc[1],loc[2]+height*.5), rotation=(0,0,math.radians(rotz)))
    o=bpy.context.object; o.name=name; o.data.materials.append(mat)
    if bevel: apply_bevel(o,bevel,3)
    return o

def cone_roof(name, loc, radius, base_z, height, mat=SLATE, vertices=12):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius, radius2=.10,
        depth=height, location=(loc[0],loc[1],base_z+height*.5))
    o=bpy.context.object; o.name=name; o.data.materials.append(mat); apply_bevel(o,.025,3)
    return o

def add_quoin_stack(objs, x, front_y, z0, rows, block_w=.34, block_h=.24, depth=.18):
    for r in range(rows):
        w=block_w*(1.18 if r%2==0 else .92)
        z=z0+r*block_h
        objs.append(cube("projecting stone quoin",(x,front_y,z),(w,depth,block_h*.88),STONE2,.012))

def add_arrow_slit(objs, x, front_y, z, h=.58):
    objs.append(cube("deep arrow slit",(x,front_y,z),(.105,.065,h),STONE2,.004))
    objs.append(cube("slit lintel",(x,front_y-.035,z+h*.54),(.34,.12,.10),STONE,.008))
    objs.append(cube("slit sill",(x,front_y-.035,z-h*.54),(.30,.12,.09),STONE,.008))

def add_corbel_band(objs, width, front_y, z, count=9):
    for i in range(count):
        x=-width*.5+(i+.5)*(width/count)
        objs.append(cube("stone corbel",(x,front_y,z),(width/count*.42,.34,.30),STONE2,.012))

def add_roof_ribs(objs, width, depth, eave_z, ridge_z, y_positions, name_prefix="roof rib"):
    # Thin authored ridge/eave accents improve roof thickness at strategic zoom.
    for y in y_positions:
        objs.append(timber_beam(name_prefix,(0,y,eave_z+.12),(width+.28,.08,.10)))
    objs.append(timber_beam(name_prefix+" ridge",(0,0,ridge_z+.02),(.10,depth+.35,.10)))

def add_battlement(objs, width, y, z, depth=.72, count=9):
    objs.append(cube("battlement parapet",(0,y,z),(width,depth,.34),STONE,.025))
    step=width/count
    for i in range(count):
        x=-width*.5+(i+.5)*step
        objs.append(cube("battlement merlon",(x,y,z+.38),(step*.52,depth+.04,.50),STONE,.02))

def add_portcullis(objs, width, front_y, bottom, top):
    bars=7
    for i in range(bars):
        x=-width*.42+(width*.84)*(i/(bars-1))
        objs.append(cube("iron portcullis vertical",(x,front_y,(bottom+top)*.5),(.055,.055,top-bottom),METAL,.004))
    for z in (bottom+.45,bottom+.92,top-.18):
        objs.append(cube("iron portcullis crossbar",(0,front_y,z),(width*.92,.06,.06),METAL,.004))

# ---------- asset 1: wall ----------

def build_wall():
    col=collection("PA_Wall_v1"); objs=[]
    core=cube("wall masonry core",(0,0,1.20),(4.9,.82,2.20),STONE,.05);objs.append(core)

    # Deep recessed bays and alternating pilasters.
    for x in (-1.55,0,1.55):
        cut=arch_cutter("blind arch cutter",.72,.93,.36,.48,1.35)
        cut.location=(x,-.34,.18)
        boolean_difference(core,cut)
        objs.append(cube("blind arch deep shadow",(x,-.445,.80),(.55,.08,.90),STONE2,.010))
        objs += add_arch_ring(x,-.455,1.08,.46,11,.18,.18,.24)

    objs += [
        cube("wall deep plinth",(0,0,.18),(5.18,1.06,.36),STONE2,.035),
        cornice("wall lower string",(0,0),5.05,.96,.56),
        cornice("wall upper string",(0,0),5.10,.94,2.18)
    ]

    for x in (-2.28,-.82,.82,2.28):
        objs.append(add_buttress(x,-.52,0,2.38,.70,.46))

    # Corner stonework remains visible even with flat lighting.
    add_quoin_stack(objs,-2.36,-.48,.34,8,.34,.24,.18)
    add_quoin_stack(objs, 2.36,-.48,.34,8,.34,.24,.18)

    add_corbel_band(objs,4.75,-.50,2.34,11)
    add_battlement(objs,5.02,0,2.58,.82,11)

    # Wall-walk lip gives a readable top silhouette.
    objs.append(cube("wall walk stone lip",(0,.18,2.46),(4.88,.42,.14),STONE2,.012))
    move_objects_to_collection(objs,col); return col

# ---------- asset 2: tower ----------

def build_tower():
    col=collection("PA_Tower_v1");objs=[]
    body=cyl("tower octagonal masonry",(0,0,0),1.48,3.25,STONE,12,.055,15);objs.append(body)
    objs.append(cyl("tower battered plinth",(0,0,0),1.66,.42,STONE2,12,.04,15))
    objs.append(cyl("tower lower belt",(0,0,.86),1.56,.16,STONE,12,.018,15))
    objs.append(cyl("tower crown course",(0,0,3.08),1.62,.20,STONE,12,.018,15))

    # Genuine front slit cuts plus architectural framing.
    for x,z in ((-.48,1.35),(.48,1.35),(0,2.18)):
        cut=cube("tower slit cutter",(x,-1.42,z),(.18,.42,.62),STONE,0)
        boolean_difference(body,cut)
        add_arrow_slit(objs,x,-1.49,z,.48)

    # Buttresses terminate below machicolation so silhouette remains octagonal.
    for x in (-1.18,1.18):
        objs.append(add_buttress(x,-1.42,0,2.45,.62,.38))

    objs.append(cyl("tower machicolation shelf",(0,0,3.22),1.73,.22,STONE,12,.018,15))
    for a in range(0,360,45):
        rad=math.radians(a)
        objs.append(cube("tower machicolation corbel",
            (math.sin(rad)*1.52,math.cos(rad)*1.52,3.08),(.24,.30,.34),STONE2,.012))

    # Proper pyramidal/conical tower roof rather than a house gable.
    objs.append(cone_roof("tower slate spire",(0,0,0),1.82,3.40,2.15,SLATE,12))
    objs.append(cyl("tower roof collar",(0,0,3.34),1.72,.12,METAL,12,.012,15))
    objs.append(cube("tower finial",(0,0,5.74),(.08,.08,.62),METAL,.008))
    objs.append(cube("tower banner",(0,-1.48,2.30),(.54,.035,1.05),BLUE,.006))
    move_objects_to_collection(objs,col); return col

# ---------- asset 3: gate ----------

def build_gate():
    col=collection("PA_Gate_v1");objs=[]

    # Central gatehouse: deep arched void, raised gable and layered masonry.
    body=cube("gatehouse main masonry",(0,0,1.58),(4.25,1.70,2.95),STONE,.055);objs.append(body)
    cutter=arch_cutter("main gate opening",1.68,1.36,.84,2.20,2.58)
    cutter.location=(0,-.12,.08); boolean_difference(body,cutter)
    objs += add_arch_ring(0,-.91,1.42,1.02,17,.25,.25,.38)
    add_portcullis(objs,1.55,-.89,.24,2.10)

    # Triangular authored gable above the rectangular mass.
    gable=custom_prism("gate ceremonial gable",[(-2.08,2.93),(2.08,2.93),(0,4.34)],1.56,(0,0,0),STONE,.035)
    objs.append(gable)
    objs.append(cube("gate gable inset",(0,-.82,3.42),(.92,.10,.72),STONE2,.012))
    objs.append(cube("gate heraldry",(0,-.90,3.48),(.52,.045,.94),BLUE,.006))

    # Layered courses / base / shoulder battlements.
    objs += [
        cube("gate deep plinth",(0,0,.18),(4.55,1.98,.36),STONE2,.04),
        cornice("gate lower string",(0,0),4.42,1.84,.58),
        cornice("gate crown string",(0,0),4.50,1.82,2.90)
    ]
    for x in (-1.72,1.72):
        objs.append(add_buttress(x,-.96,0,3.15,.78,.48))
        add_quoin_stack(objs,x,-.91,.38,10,.30,.24,.18)

    # Flank octagonal towers have their own crown + slate spire.
    for x in (-2.86,2.86):
        t=cyl("gate flank tower",(x,0,0),1.12,3.55,STONE,12,.045,15);objs.append(t)
        objs.append(cyl("gate tower plinth",(x,0,0),1.28,.42,STONE2,12,.035,15))
        objs.append(cyl("gate tower crown",(x,0,3.42),1.29,.22,STONE,12,.018,15))
        for z in (1.28,2.22):
            objs.append(cube("gate tower arrow slit",(x,-1.10,z),(.11,.07,.46),STONE2,.004))
        objs.append(cone_roof("gate tower slate spire",(x,0,0),1.39,3.62,1.72,SLATE,12))
        objs.append(cube("gate tower finial",(x,0,5.56),(.07,.07,.44),METAL,.006))
        objs.append(cube("gate tower banner",(x,-1.13,2.35),(.38,.035,.86),BLUE,.006))

    # Shoulder parapets connect main mass to towers visually.
    for x in (-2.08,2.08):
        objs.append(cube("gate shoulder wall",(x,0,2.42),(1.18,1.56,1.02),STONE,.035))
        for dx in (-.38,.0,.38):
            objs.append(cube("gate shoulder merlon",(x+dx,-.02,3.16),(.22,1.60,.42),STONE,.015))

    move_objects_to_collection(objs,col); return col

# ---------- asset 4: civic house ----------

def build_civic_house():
    col=collection("PA_CivicHouse_v1");objs=[]
    base=cube("civic stone ground floor",(0,0,.72),(3.55,2.55,1.32),STONE,.045);objs.append(base)
    objs += add_door_recess(base,0,-1.22,.16,.72,1.10,.36)
    for x in (-1.04,1.04):
        objs += add_window_recess(base,x,-1.20,.78,.46,.55,.28)

    # Projecting corner quoins give masonry depth independent of textures.
    add_quoin_stack(objs,-1.70,-1.30,.24,5,.30,.24,.16)
    add_quoin_stack(objs, 1.70,-1.30,.24,5,.30,.24,.16)

    upper=cube("civic upper floor",(0,0,1.78),(3.32,2.38,.88),PLASTER,.035);objs.append(upper)
    for x in (-1.02,0,1.02):
        objs += add_window_recess(upper,x,-1.12,1.80,.42,.52,.24)

    objs.append(timber_beam("projecting sill",(0,-1.24,1.31),(3.48,.16,.18)))
    objs.append(timber_beam("upper head beam",(0,-1.24,2.22),(3.48,.14,.15)))
    for x in (-1.46,-.72,0,.72,1.46):
        objs.append(timber_beam("upper post",(x,-1.22,1.78),(.14,.12,.92)))
    for x in (-1.05,-.35,.35,1.05):
        objs.append(timber_beam("diagonal brace",(x,-1.235,1.77),(.92,.10,.12),32 if x<0 else -32))

    objs.append(roof_gable_solid("civic slate roof",3.72,2.78,2.28,3.64,.18,.28,SLATE))
    add_roof_ribs(objs,3.70,2.78,2.28,3.64,(-1.20,1.20),"civic roof rib")

    dorm=cube("dormer body",(0,-.62,2.73),(1.08,.82,.72),PLASTER,.025);objs.append(dorm)
    objs += add_window_recess(dorm,0,-1.01,2.74,.38,.40,.16)
    dorm_roof=roof_gable_solid("dormer roof",1.30,.98,3.05,3.56,.12,.12,SLATE);dorm_roof.location.y=-.62;objs.append(dorm_roof)

    # Small timber balcony/porch creates a readable inhabited facade.
    objs.append(cube("stone entry stoop",(0,-1.54,.12),(1.42,.76,.24),STONE2,.025))
    porch=roof_gable_solid("porch roof",1.62,1.16,1.34,1.80,.10,.14,SLATE);porch.location.y=-1.52;objs.append(porch)
    for x in (-.62,.62): objs.append(timber_beam("porch post",(x,-1.82,.72),(.12,.12,1.22)))
    objs.append(timber_beam("porch beam",(0,-1.82,1.28),(1.38,.12,.12)))

    objs.append(cube("chimney",(1.05,.38,2.84),(.40,.40,1.38),STONE2,.025))
    objs.append(cube("chimney cap",(1.05,.38,3.56),(.50,.50,.15),STONE,.018))
    objs.append(cube("civic hanging banner",(-1.43,-1.31,1.66),(.32,.035,.72),BLUE,.005))
    move_objects_to_collection(objs,col); return col

# ---------- asset 5: workshop ----------

def build_workshop():
    col=collection("PA_Workshop_v1");objs=[]
    hall=cube("workshop main hall",(-.18,0,.78),(3.55,2.60,1.48),STONE,.045);objs.append(hall)
    objs += add_door_recess(hall,.42,-1.25,.14,.92,1.24,.36)
    objs += add_window_recess(hall,-1.02,-1.24,.82,.50,.58,.28)

    add_quoin_stack(objs,-1.78,-1.33,.24,5,.30,.24,.16)
    add_quoin_stack(objs, 1.40,-1.33,.24,5,.30,.24,.16)

    annex=cube("workshop timber annex",(1.64,.32,.64),(1.25,1.95,1.18),PLASTER,.035);objs.append(annex)
    objs += add_window_recess(annex,1.64,-.62,.72,.38,.46,.20)
    for x in (-1.56,-.78,0,.78,1.56):
        objs.append(timber_beam("workshop frame",(x,-1.28,.82),(.16,.12,1.56)))
    objs.append(timber_beam("workshop lintel",(0,-1.29,1.40),(3.35,.13,.16)))
    for x in (-1.10,-.38,.38,1.10):
        objs.append(timber_beam("workshop brace",(x,-1.30,1.03),(.86,.10,.12),28 if x<0 else -28))

    main_roof=roof_gable_solid("workshop main roof",3.90,2.96,1.54,2.96,.17,.24,SLATE);objs.append(main_roof)
    add_roof_ribs(objs,3.88,2.96,1.54,2.96,(-1.18,1.18),"workshop roof rib")

    # Functional side canopy with real supports and raised hoist beam.
    lean=cube("lean-to roof",(1.92,-.30,1.56),(1.72,1.92,.12),SLATE,.02);lean.rotation_euler[1]=math.radians(-14);objs.append(lean)
    for x in (1.34,2.42):
        objs.append(timber_beam("lean-to post",(x,-1.05,.72),(.13,.13,1.35)))
    objs.append(timber_beam("hoist beam",(1.90,-1.10,1.52),(1.55,.13,.14)))
    objs.append(timber_beam("hoist arm",(1.92,-1.55,1.52),(.13,.95,.13)))
    objs.append(cube("hoist hook",(1.92,-1.95,1.10),(.08,.08,.48),METAL,.006))

    for z in (.36,.92):
        objs.append(timber_beam("annex horizontal",(1.64,-.66,z),(1.18,.11,.13)))

    objs.append(cube("forge chimney",(-1.28,.55,2.18),(.44,.44,1.86),STONE2,.03))
    objs.append(cube("forge chimney cap",(-1.28,.55,3.13),(.58,.58,.16),STONE,.02))
    objs.append(cube("workbench",(1.70,-1.10,.42),(1.15,.48,.18),TIMBER,.02))
    for x in (1.30,2.10):objs.append(timber_beam("workbench leg",(x,-1.10,.22),(.12,.12,.46)))
    objs.append(cube("metal stock rack",(2.20,-.72,.50),(.16,.72,.88),METAL,.01))
    move_objects_to_collection(objs,col); return col

# ---------- UV / cleanup / export ----------

def uv_all(col):
    # Headless-safe deterministic box projection; no editor-area-dependent bpy.ops.uv calls.
    scale=.35
    for o in col.objects:
        if o.type!="MESH": continue
        me=o.data
        if len(me.uv_layers)==0:
            uv=me.uv_layers.new(name="UVMap")
        else:
            uv=me.uv_layers.active
        me.update()
        for poly in me.polygons:
            n=poly.normal
            ax=max(range(3), key=lambda i: abs(n[i]))
            for li in poly.loop_indices:
                co=me.vertices[me.loops[li].vertex_index].co
                if ax==0:
                    u,v=co.y,co.z
                elif ax==1:
                    u,v=co.x,co.z
                else:
                    u,v=co.x,co.y
                uv.data[li].uv=(u*scale,v*scale)
        me.update()

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

PREVIEW=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-starter-previews")
os.makedirs(PREVIEW,exist_ok=True)

def render_preview(col, asset_id):
    scene=bpy.context.scene
    scene.render.engine="BLENDER_WORKBENCH"
    scene.render.resolution_x=768; scene.render.resolution_y=768; scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.display.shading.light="STUDIO"
    scene.display.shading.color_type="MATERIAL"
    scene.display.shading.show_shadows=True
    scene.display.shading.show_cavity=True
    scene.display.shading.cavity_type="WORLD"
    scene.render.film_transparent=False
    if scene.world is None:
        scene.world=bpy.data.worlds.new("PreviewWorld")
    scene.world.color=(0.055,0.065,0.075)

    for cc in bpy.data.collections:
        if cc.name.startswith("PA_"): cc.hide_render=(cc!=col)

    objs=[o for o in col.objects if o.type=="MESH"]
    if not objs:return
    mins=Vector((1e9,1e9,1e9)); maxs=Vector((-1e9,-1e9,-1e9))
    for o in objs:
        for corner in o.bound_box:
            w=o.matrix_world @ Vector(corner)
            mins=Vector((min(mins.x,w.x),min(mins.y,w.y),min(mins.z,w.z)))
            maxs=Vector((max(maxs.x,w.x),max(maxs.y,w.y),max(maxs.z,w.z)))
    center=(mins+maxs)*.5
    span=max(maxs.x-mins.x,maxs.y-mins.y,maxs.z-mins.z)

    cam_data=bpy.data.cameras.get("PreviewCamera") or bpy.data.cameras.new("PreviewCamera")
    cam=bpy.data.objects.get("PreviewCamera") or bpy.data.objects.new("PreviewCamera",cam_data)
    if cam.name not in scene.collection.objects: scene.collection.objects.link(cam)
    scene.camera=cam
    cam.data.lens=58
    cam.location=center+Vector((span*1.35,-span*1.75,span*.95))
    cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler()

    scene.render.filepath=os.path.join(PREVIEW,asset_id+".png")
    bpy.ops.render.render(write_still=True)

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
    render_preview(col,asset_id)
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
