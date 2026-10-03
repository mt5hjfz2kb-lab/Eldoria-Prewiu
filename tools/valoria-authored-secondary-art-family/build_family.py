import bpy, math, os, json
from mathutils import Vector

OUT=os.environ.get("ELDORIA_AUTHORED_FAMILY_DIR","pipeline/candidates/valoria-authored-secondary-art-family-v1")
REPORT=os.environ.get("ELDORIA_AUTHORED_FAMILY_REPORT","pipeline/evidence/valoria-authored-secondary-art-family-v1.json")
os.makedirs(OUT,exist_ok=True)
os.makedirs(os.path.dirname(REPORT),exist_ok=True)

def clear():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes,bpy.data.curves,bpy.data.materials,bpy.data.cameras,bpy.data.lights):
        pass

def mat(name,color,rough=.72,metal=0.0):
    m=bpy.data.materials.get(name)
    if m:return m
    m=bpy.data.materials.new(name)
    m.use_nodes=True
    bsdf=m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value=(*color,1)
    bsdf.inputs["Roughness"].default_value=rough
    bsdf.inputs["Metallic"].default_value=metal
    return m

STONE=mat("Eldoria_Stone",(0.56,0.53,0.46),.82)
STONE_DARK=mat("Eldoria_Stone_Dark",(0.36,0.35,0.32),.88)
PLASTER=mat("Eldoria_Warm_Plaster",(0.71,0.65,0.54),.86)
TIMBER=mat("Eldoria_Timber",(0.25,0.16,0.09),.78)
TIMBER_LITE=mat("Eldoria_Timber_Light",(0.39,0.25,0.13),.76)
ROOF=mat("Eldoria_Blue_Slate",(0.12,0.22,0.30),.74)
ROOF_EDGE=mat("Eldoria_Blue_Slate_Light",(0.20,0.31,0.39),.72)
METAL=mat("Eldoria_Iron",(0.12,0.13,0.13),.48,.15)

def apply_mods(obj):
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    for mod in list(obj.modifiers):
        try:bpy.ops.object.modifier_apply(modifier=mod.name)
        except:pass
    obj.select_set(False)

def box(name,loc,scale,material,bevel=.04,rot=(0,0,0)):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=rot)
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if material:o.data.materials.append(material)
    if bevel>0:
        b=o.modifiers.new("Bevel","BEVEL");b.width=bevel;b.segments=2
        try:b.affect='EDGES'
        except:pass
        n=o.modifiers.new("WeightedNormal","WEIGHTED_NORMAL");n.keep_sharp=True;n.weight=50
        apply_mods(o)
    return o

def cylinder(name,loc,radius,depth,material,rot=(0,0,0),verts=20,bevel=.02):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=depth,location=loc,rotation=rot)
    o=bpy.context.object;o.name=name
    if material:o.data.materials.append(material)
    if bevel>0:
        b=o.modifiers.new("Bevel","BEVEL");b.width=bevel;b.segments=2
        n=o.modifiers.new("WeightedNormal","WEIGHTED_NORMAL")
        apply_mods(o)
    return o

def beam(name,a,b,width,depth,material):
    mid=(Vector(a)+Vector(b))/2
    vec=Vector(b)-Vector(a)
    length=vec.length
    o=box(name,mid,(width,depth,length),material,bevel=min(width,depth)*.16)
    o.rotation_mode='QUATERNION'
    o.rotation_quaternion=vec.to_track_quat('Z','Y')
    return o

def stone_course(prefix,y,z,xmin,xmax,block=.78,depth=.34,height=.30):
    x=xmin;idx=0
    while x<xmax-.05:
        w=min(block*(.90+(.08*((idx%3)-1))),xmax-x)
        box(f"{prefix}_{idx}",(x+w/2,y,z),(w-.035,depth,height),STONE,bevel=.035)
        x+=w;idx+=1

def roof_half(prefix,sign,xlen,yhalf,zridge,zedge,rows=9):
    # slope sheet plus visible shingle rows
    dy=yhalf
    dz=zridge-zedge
    ang=math.atan2(dz,dy)
    length=math.sqrt(dy*dy+dz*dz)
    y=sign*yhalf/2
    z=(zridge+zedge)/2
    box(prefix+"_sheet",(0,y,z),(xlen,length,.14),ROOF,bevel=.025,rot=(sign*ang,0,0))
    for i in range(rows):
        t=(i+.35)/rows
        yy=sign*(t*yhalf)
        zz=zridge-(dz*t)
        box(f"{prefix}_shingle_{i}",(0,yy,zz),(xlen+.10,.16,.08),ROOF_EDGE,bevel=.018,rot=(sign*ang,0,0))

def frame_wall(y,sign=1):
    z0=.52;top=2.75
    for x in (-2.45,0,2.45):
        box(f"timber_post_{sign}_{x}",(x,y,(z0+top)/2),(.18,.20,top-z0),TIMBER,bevel=.025)
    for z in (1.10,2.20,2.70):
        box(f"timber_band_{sign}_{z}",(0,y,z),(5.05,.20,.16),TIMBER,bevel=.022)
    beam(f"braceA_{sign}",(-2.35,y,1.15),(-.20,y,2.60),.14,.18,TIMBER)
    beam(f"braceB_{sign}",(.20,y,2.60),(2.35,y,1.15),.14,.18,TIMBER)

def build_sawmill():
    clear()
    # low stone foundation and courses
    box("foundation_core",(0,0,.28),(6.2,4.3,.56),STONE_DARK,bevel=.08)
    for course in range(2):
        z=.18+course*.30
        stone_course(f"front_course{course}",-2.12,z,-3.0,3.0,.80,.36,.28)
        stone_course(f"back_course{course}",2.12,z,-3.0,3.0,.80,.36,.28)
    # plaster body
    box("main_plaster",(0,0,1.65),(5.2,3.25,2.35),PLASTER,bevel=.06)
    frame_wall(-1.64,-1);frame_wall(1.64,1)
    for x in (-2.45,2.45):
        box(f"side_post_{x}",(x,0,1.65),(.20,3.28,2.35),TIMBER,bevel=.026)
    # front door and windows
    box("door_recess",(0,-1.69,1.30),(1.02,.12,1.65),TIMBER_DARK if False else STONE_DARK,bevel=.03)
    box("door_leaf",(0,-1.76,1.27),(.82,.10,1.48),TIMBER_LITE,bevel=.025)
    for x in (-1.55,1.55):
        box(f"window_recess_{x}",(x,-1.70,1.72),(.72,.10,.76),STONE_DARK,bevel=.02)
        box(f"window_top_{x}",(x,-1.78,2.03),(.82,.10,.10),ROOF_EDGE,bevel=.015)
        box(f"window_sideL_{x}",(x-.36,-1.78,1.72),(.08,.10,.72),TIMBER,bevel=.012)
        box(f"window_sideR_{x}",(x+.36,-1.78,1.72),(.08,.10,.72),TIMBER,bevel=.012)
    # roof
    roof_half("roof_front",-1,5.8,2.25,3.95,2.75,10)
    roof_half("roof_back",1,5.8,2.25,3.95,2.75,10)
    box("ridge",(0,0,3.96),(5.95,.18,.18),ROOF_EDGE,bevel=.025)
    # saw work canopy left
    canopy_x=-4.05
    for y in (-1.35,1.35):
        box(f"canopy_post_{y}",(canopy_x,y,1.15),(.18,.18,2.3),TIMBER,bevel=.025)
    box("canopy_beam",(canopy_x,0,2.25),(.22,3.05,.20),TIMBER,bevel=.025)
    box("canopy_roof",(canopy_x-.05,0,2.62),(2.55,3.35,.16),ROOF,bevel=.03,rot=(0,math.radians(-8),0))
    # large saw wheel and axle
    cylinder("saw_wheel",(canopy_x-.55,-1.48,1.05),.72,.16,TIMBER_LITE,rot=(math.radians(90),0,0),verts=24,bevel=.025)
    cylinder("saw_axle",(canopy_x-.55,-1.48,1.05),.12,.38,METAL,rot=(math.radians(90),0,0),verts=16,bevel=.01)
    # log stock
    for i,(yy,zz) in enumerate([(-.85,.28),(-.25,.28),(.35,.28),(-.55,.55),(.05,.55)]):
        cylinder(f"log_{i}",(canopy_x+1.00,yy,zz),.18,2.0,TIMBER_LITE,rot=(0,math.radians(90),0),verts=14,bevel=.018)
    # chimney
    box("chimney",(1.55,.55,3.45),(.55,.55,1.65),STONE,bevel=.06)
    box("chimney_cap",(1.55,.55,4.27),(.72,.72,.16),STONE_DARK,bevel=.035)
    return "Valoria_Authored_Aserradero_v1.glb"

def build_wall():
    clear()
    box("wall_core",(0,0,.78),(4.8,.86,1.56),STONE,bevel=.07)
    # face stones
    for row in range(4):
        z=.18+row*.38
        offset=.38 if row%2 else 0
        x=-2.35-offset
        idx=0
        while x<2.35:
            w=.72+(idx%3)*.08
            box(f"wall_stone_{row}_{idx}",(x+w/2, -.45, z),(w-.035,.15,.31),STONE_DARK if (idx+row)%5==0 else STONE,bevel=.026)
            x+=w;idx+=1
    box("wall_cap",(0,0,1.62),(4.95,1.00,.18),STONE_DARK,bevel=.035)
    for i,x in enumerate([-2.10,-1.05,0,1.05,2.10]):
        box(f"merlon_{i}",(x,0,1.92),(.58,.88,.55),STONE,bevel=.045)
    return "Valoria_Authored_Wall_v1.glb"

def build_gate():
    clear()
    # piers
    for x in (-1.65,1.65):
        box(f"pier_{x}",(x,0,1.55),(1.18,1.35,3.1),STONE,bevel=.075)
        for z in (.35,.78,1.21,1.64,2.07,2.50):
            box(f"pier_band_{x}_{z}",(x,-.70,z),(1.26,.12,.12),STONE_DARK,bevel=.02)
        box(f"pier_cap_{x}",(x,0,3.15),(1.38,1.52,.20),STONE_DARK,bevel=.04)
        box(f"pier_merlon_{x}",(x,0,3.48),(.72,1.32,.62),STONE,bevel=.05)
    # lintel / arch impression
    box("gate_lintel",(0,0,2.65),(2.35,1.10,.62),STONE,bevel=.08)
    box("gate_crossbeam",(0,-.61,2.33),(2.70,.16,.24),TIMBER,bevel=.025)
    # two timber doors
    for x in (-.58,.58):
        box(f"door_{x}",(x,-.62,1.18),(1.05,.16,2.28),TIMBER_LITE,bevel=.03)
        for z in (.55,1.18,1.82):
            box(f"strap_{x}_{z}",(x,-.72,z),(.98,.06,.08),METAL,bevel=.01)
    # restrained blue heraldry
    for x in (-1.65,1.65):
        box(f"blue_marker_{x}",(x,-.73,2.10),(.26,.05,.78),ROOF,bevel=.015)
    return "Valoria_Authored_Gate_v1.glb"

def export_asset(builder):
    filename=builder()
    # normalize origin to ground center based on world bounds
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if not objs: raise RuntimeError("No mesh objects")
    mins=[1e9,1e9,1e9];maxs=[-1e9,-1e9,-1e9]
    for o in objs:
        for c in o.bound_box:
            w=o.matrix_world @ Vector(c)
            for i in range(3): mins[i]=min(mins[i],w[i]);maxs[i]=max(maxs[i],w[i])
    cx=(mins[0]+maxs[0])/2;cy=(mins[1]+maxs[1])/2;z0=mins[2]
    for o in objs:o.location-=Vector((cx,cy,z0))
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    path=os.path.abspath(os.path.join(OUT,filename))
    bpy.ops.export_scene.gltf(filepath=path,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)
    # stats
    tris=0
    for o in objs:
        me=o.data
        me.calc_loop_triangles()
        tris+=len(me.loop_triangles)
    return {"file":filename,"path":path,"objects":len(objs),"triangles":tris,
            "materials":sorted({m.name for o in objs for m in o.data.materials if m})}

results=[]
for builder in (build_sawmill,build_wall,build_gate):
    results.append(export_asset(builder))

with open(REPORT,"w",encoding="utf-8") as f:
    json.dump({"schema_version":1,"family":"Valoria Authored Secondary Art Family v1",
               "principles":["deterministic metric modules","beveled hard-surface edges","weighted normals",
                             "shared stone/timber/plaster/blue-slate material vocabulary","source-level semantic separation"],
               "assets":results},f,indent=2)
print(json.dumps(results,indent=2))
