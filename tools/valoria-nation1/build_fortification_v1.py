import bpy, os, json, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
PBR=os.environ.get("NATION1_PBR_DIR",os.path.join(ROOT,".nation1-pbr"))
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","Nation1")
CAND=os.path.join(ROOT,"pipeline","candidates","valoria-nation1-v1")
EVID=os.path.join(ROOT,"pipeline","evidence","valoria-nation1-v1.json")
os.makedirs(OUT,exist_ok=True);os.makedirs(CAND,exist_ok=True);os.makedirs(os.path.dirname(EVID),exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)

def mat_pbr(name,diffuse,normal=None,rough=.72):
    m=bpy.data.materials.new(name);m.use_nodes=True
    nt=m.node_tree;nt.nodes.clear()
    out=nt.nodes.new("ShaderNodeOutputMaterial");bs=nt.nodes.new("ShaderNodeBsdfPrincipled")
    bs.inputs["Roughness"].default_value=rough
    nt.links.new(bs.outputs["BSDF"],out.inputs["Surface"])
    img=bpy.data.images.load(os.path.join(PBR,diffuse),check_existing=True)
    tex=nt.nodes.new("ShaderNodeTexImage");tex.image=img;tex.interpolation="Linear"
    nt.links.new(tex.outputs["Color"],bs.inputs["Base Color"])
    if normal:
        nimg=bpy.data.images.load(os.path.join(PBR,normal),check_existing=True);nimg.colorspace_settings.name="Non-Color"
        ntex=nt.nodes.new("ShaderNodeTexImage");ntex.image=nimg;ntex.interpolation="Linear"
        nm=nt.nodes.new("ShaderNodeNormalMap");nm.inputs["Strength"].default_value=.65
        nt.links.new(ntex.outputs["Color"],nm.inputs["Color"]);nt.links.new(nm.outputs["Normal"],bs.inputs["Normal"])
    return m

def mat_color(name,color,rough=.72,metal=0):
    m=bpy.data.materials.new(name);m.use_nodes=True
    bs=next(n for n in m.node_tree.nodes if n.type=="BSDF_PRINCIPLED")
    bs.inputs["Base Color"].default_value=(*color,1);bs.inputs["Roughness"].default_value=rough;bs.inputs["Metallic"].default_value=metal
    return m

STONE=mat_pbr("Nation1 Warm Limestone","stone_block_wall_diff_1k.jpg","stone_block_wall_nor_gl_1k.jpg",.78)
STONE_DARK=mat_pbr("Nation1 Foundation Stone","rock_boulder_dry_diff_1k.jpg","rock_boulder_dry_nor_gl_1k.jpg",.86)
WOOD=mat_color("Nation1 Dark Timber",(.19,.105,.055),.76)
BLUE=mat_color("Nation1 Valoria Blue",(.045,.14,.34),.64)
DARK=mat_color("Nation1 Recess",(.025,.028,.03),.92)
GOLD=mat_color("Nation1 Warm Metal",(.48,.29,.08),.44,.18)

def box(name,loc,scale,mat,bevel=.03,rot=(0,0,0),uv=True):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=rot)
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel>0:
        md=o.modifiers.new("Edge softness","BEVEL");md.width=bevel;md.segments=2;md.limit_method="ANGLE"
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=md.name)
    o.data.materials.append(mat)
    if uv:
        bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT");bpy.ops.uv.cube_project(cube_size=1.35,correct_aspect=True)
        bpy.ops.object.mode_set(mode="OBJECT");o.select_set(False)
    return o

def collection(name):
    c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c);return c

def move_to(o,col):
    for c in list(o.users_collection):c.objects.unlink(o)
    col.objects.link(o)

def join_by_material(col):
    by={}
    for o in list(col.objects):
        if o.type!="MESH":continue
        key=o.data.materials[0].name if len(o.data.materials) else "_"
        by.setdefault(key,[]).append(o)
    for key,objs in by.items():
        if len(objs)<2:continue
        bpy.ops.object.select_all(action="DESELECT")
        for o in objs:o.select_set(True)
        bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();objs[0].name=col.name+" · "+key

def add_wall(col):
    parts=[]
    parts += [
      box("wall core",(0,0,.82),(4.0,.52,1.48),STONE,.035),
      box("wall foundation",(0,0,.12),(4.18,.66,.24),STONE_DARK,.035),
      box("wall belt lower",(0,-.01,.42),(4.12,.61,.13),STONE,.025),
      box("wall coping",(0,0,1.56),(4.18,.64,.18),STONE,.025)
    ]
    for x in (-1.55,1.55):
        parts.append(box("wall buttress",(x,-.33,.77),(.34,.28,1.40),STONE,.025))
    for i,x in enumerate((-1.72,-1.15,-.58,0,.58,1.15,1.72)):
        parts.append(box("wall merlon",(x,0,1.91),(.35,.62,.54),STONE,.025))
    for x in (-1.15,0,1.15):
        parts.append(box("wall slit",(x,-.274,1.02),(.11,.035,.42),DARK,.006,uv=False))
    for o in parts:move_to(o,col)
    join_by_material(col)

def tower_parts(cx=0,cy=0,base_z=0,scale=1.0,prefix="tower"):
    p=[]
    p.append(box(prefix+" core",(cx,cy,base_z+1.38*scale),(2.35*scale,2.35*scale,2.62*scale),STONE,.045*scale))
    p.append(box(prefix+" base",(cx,cy,base_z+.16*scale),(2.58*scale,2.58*scale,.32*scale),STONE_DARK,.04*scale))
    p.append(box(prefix+" belt",(cx,cy,base_z+2.45*scale),(2.55*scale,2.55*scale,.18*scale),STONE,.028*scale))
    p.append(box(prefix+" parapet",(cx,cy,base_z+2.76*scale),(2.52*scale,2.52*scale,.32*scale),STONE,.03*scale))
    for sx in (-1,1):
      for sy in (-1,1):
        p.append(box(prefix+" corner buttress",(cx+sx*1.08*scale,cy+sy*1.08*scale,base_z+1.18*scale),(.26*scale,.26*scale,2.15*scale),STONE,.025*scale))
    for x in (-.94,-.47,0,.47,.94):
        p.append(box(prefix+" front merlon",(cx+x*scale,cy-1.18*scale,base_z+3.17*scale),(.30*scale,.38*scale,.46*scale),STONE,.022*scale))
        p.append(box(prefix+" back merlon",(cx+x*scale,cy+1.18*scale,base_z+3.17*scale),(.30*scale,.38*scale,.46*scale),STONE,.022*scale))
    for y in (-.72,0,.72):
        p.append(box(prefix+" left merlon",(cx-1.18*scale,cy+y*scale,base_z+3.17*scale),(.38*scale,.30*scale,.46*scale),STONE,.022*scale))
        p.append(box(prefix+" right merlon",(cx+1.18*scale,cy+y*scale,base_z+3.17*scale),(.38*scale,.30*scale,.46*scale),STONE,.022*scale))
    for z in (1.05,1.75):
        p.append(box(prefix+" slit front",(cx,cy-1.181*scale,base_z+z*scale),(.12*scale,.035*scale,.40*scale),DARK,.005,uv=False))
    return p

def add_tower(col):
    for o in tower_parts():move_to(o,col)
    # blue heraldic banner facing the official front.
    move_to(box("tower banner",(0,-1.205,1.65),(.46,.035,1.10),BLUE,.008,uv=False),col)
    move_to(box("tower banner trim",(0,-1.232,2.22),(.54,.035,.07),GOLD,.005,uv=False),col)
    join_by_material(col)

def add_gate(col):
    parts=[]
    # Two strong square towers.
    parts.extend(tower_parts(-2.25,0,0,1.03,"gate west"))
    parts.extend(tower_parts(2.25,0,0,1.03,"gate east"))
    # Side shoulders and bridge above the opening.
    parts.append(box("gate west shoulder",(-1.25,0,1.08),(1.30,.68,1.92),STONE,.04))
    parts.append(box("gate east shoulder",(1.25,0,1.08),(1.30,.68,1.92),STONE,.04))
    parts.append(box("gate lintel",(0,0,2.32),(1.90,.72,.48),STONE,.04))
    parts.append(box("gate top course",(0,0,2.63),(2.12,.76,.18),STONE,.025))
    # Arch ring from real stone blocks; open center remains visible.
    r=1.03
    for i in range(11):
        a=math.radians(180-i*18)
        x=r*math.cos(a);z=1.27+r*math.sin(a)
        rot=(0,-a+math.pi/2,0)
        parts.append(box("gate voussoir",(x,-.38,z),(.35,.24,.46),STONE,.018,rot=rot))
    # timber gate panels, recessed behind masonry.
    parts.append(box("gate door west",(-.46,.19,.92),(.88,.16,1.72),WOOD,.018))
    parts.append(box("gate door east",(.46,.19,.92),(.88,.16,1.72),WOOD,.018))
    # Blue banners on the two towers.
    parts.append(box("gate west banner",(-2.25,-1.255,1.75),(.46,.035,1.18),BLUE,.008,uv=False))
    parts.append(box("gate east banner",(2.25,-1.255,1.75),(.46,.035,1.18),BLUE,.008,uv=False))
    for o in parts:move_to(o,col)
    join_by_material(col)

def add_stair(col):
    parts=[]
    steps=12;depth=.34;height=.085
    for i in range(steps):
        y=-1.75+i*depth
        z=.06+i*height
        w=4.55-i*.035
        parts.append(box("stair tread",(0,y,z),(w,.43,.12),STONE,.018))
    parts.append(box("stair lower landing",(0,-2.18,.08),(5.05,.92,.16),STONE,.025))
    parts.append(box("stair upper landing",(0,2.02,1.08),(4.20,.82,.16),STONE,.025))
    # stepped cheeks give weight without becoming a rock wrapper.
    for side in (-1,1):
        for i in range(6):
            y=-1.55+i*.68;z=.18+i*.17
            parts.append(box("stair cheek",(side*(2.30-i*.025),y,z),(.30,.78,.52+i*.035),STONE_DARK,.02))
    for o in parts:move_to(o,col)
    join_by_material(col)

def export_collection(col,filename):
    bpy.ops.object.select_all(action="DESELECT")
    objs=[o for o in col.objects if o.type=="MESH"]
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    path=os.path.join(OUT,filename)
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
    if not os.path.isfile(path) or os.path.getsize(path)<10000:raise RuntimeError("Bad export "+path)
    return path,objs

wall_col=collection("Nation1_Wall_v1");add_wall(wall_col)
tower_col=collection("Nation1_Tower_v1");add_tower(tower_col)
gate_col=collection("Nation1_Gate_v1");add_gate(gate_col)
stair_col=collection("Nation1_Stair_v1");add_stair(stair_col)

exports=[]
for col,fn in [(wall_col,"Nation1_Wall_v1.glb"),(tower_col,"Nation1_Tower_v1.glb"),(gate_col,"Nation1_Gate_v1.glb"),(stair_col,"Nation1_Stair_v1.glb")]:
    p,objs=export_collection(col,fn);exports.append({"file":fn,"bytes":os.path.getsize(p),"objects":len(objs),"vertices":sum(len(o.data.vertices) for o in objs)})

blend=os.path.join(CAND,"Valoria_Nation1_Fortification_v1.blend")
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=blend)
with open(EVID,"w",encoding="utf-8") as f:
    json.dump({"status":"PASS","blender_version":bpy.app.version_string,"source_blend":"pipeline/candidates/valoria-nation1-v1/Valoria_Nation1_Fortification_v1.blend","exports":exports,"materials":["Warm Limestone PBR","Foundation Stone PBR","Dark Timber","Valoria Blue","Recess"],"tripo_credits":0},f,indent=2)
print("VALORIA_NATION1_BLENDER_FORTIFICATION=PASS")
print(json.dumps(exports,indent=2))
