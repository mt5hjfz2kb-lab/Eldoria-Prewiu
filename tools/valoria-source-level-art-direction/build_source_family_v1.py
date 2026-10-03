"""Valoria Source-Level Art Direction Proof v1.
Creates a deliberately small Blender-authored family for the played-camera proof:
Hero rock interface, Aserradero architectural kit and wall segment.
No paid generation. Outputs GLBs + .blend source + metrics.
"""
import bpy, os, json, math
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","SourceLevelArtDirection_v1")
EVID=os.path.join(ROOT,"pipeline","evidence","valoria-source-level-art-direction-proof-v1.json")
SRCBLEND=os.path.join(ROOT,"pipeline","candidates","valoria-source-level-art-direction-proof-v1","Valoria_SLAD_Source_v1.blend")
os.makedirs(OUT,exist_ok=True);os.makedirs(os.path.dirname(EVID),exist_ok=True);os.makedirs(os.path.dirname(SRCBLEND),exist_ok=True)

def mat(name,color,rough=.8,metal=0.0):
    m=bpy.data.materials.new(name);m.use_nodes=True
    b=m.node_tree.nodes.get("Principled BSDF")
    b.inputs["Base Color"].default_value=(*color,1)
    b.inputs["Roughness"].default_value=rough
    b.inputs["Metallic"].default_value=metal
    return m
bpy.ops.wm.read_factory_settings(use_empty=True)
STONE=mat("SLAD Warm Limestone",(0.47,0.43,0.35),.88)
STONE_DARK=mat("SLAD Foundation Stone",(0.31,0.30,0.27),.92)
TIMBER=mat("SLAD Dark Oak",(0.19,0.105,0.055),.78)
SLATE=mat("SLAD Blue Slate",(0.15,0.20,0.23),.9)
IRON=mat("SLAD Wrought Iron",(0.07,0.075,0.08),.58,.35)

def cube(name,loc,scale,material,bevel=.05,rot=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=(0,0,math.radians(rot)))
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        m=o.modifiers.new("SLAD bevel","BEVEL");m.width=bevel;m.segments=2;m.limit_method="ANGLE"
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
    o.data.materials.append(material);return o

def wedge(name,loc,scale,material,rot=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=(0,0,math.radians(rot)))
    o=bpy.context.object;o.name=name;o.dimensions=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bev=o.modifiers.new("SLAD soft edge","BEVEL");bev.width=.06;bev.segments=2;bev.limit_method="ANGLE"
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bev.name)
    o.data.materials.append(material);return o

def export(name,objects):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    path=os.path.join(OUT,name+".glb")
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,export_materials="EXPORT",export_yup=True)
    return path

# Hero interface: terraced retaining vocabulary that visually mediates fused rock -> civic stone.
hero=[]
hero += [cube("Hero lower retaining", (0,0,.22),(5.8,1.05,.44),STONE_DARK,.06)]
hero += [cube("Hero upper retaining", (0,.62,.48),(4.9,.72,.38),STONE,.05)]
for x in (-2.35,-1.15,1.15,2.35):
    hero.append(cube("Hero buttress", (x,-.14,.53),(.34,1.08,1.06),STONE,.05))
for x in (-2.55,-1.7,-.85,.0,.85,1.7,2.55):
    hero.append(cube("Hero coping", (x,-.02,.83),(.62,1.15,.16),STONE,.035))
hero_path=export("SLAD_HeroRockInterface_v1",hero)

# Aserradero kit: preserves canonical building, authors shared base/eaves/braces around it.
saw=[]
saw += [cube("Sawmill plinth", (0,0,.18),(4.35,3.55,.36),STONE_DARK,.07)]
saw += [cube("Sawmill dressed base", (0,0,.43),(4.10,3.30,.24),STONE,.05)]
for x in (-1.78,1.78):
    for y in (-1.35,1.35):
        saw.append(cube("Sawmill timber post",(x,y,1.05),(.22,.22,1.42),TIMBER,.025))
for y in (-1.52,1.52):
    saw.append(cube("Sawmill eave trim",(0,y,1.74),(4.18,.18,.22),TIMBER,.025))
for x in (-1.90,1.90):
    saw.append(cube("Sawmill stone pier",(x,0,.82),(.34,3.18,1.22),STONE,.04))
saw += [cube("Sawmill slate threshold",(0,-1.74,.60),(2.0,.32,.12),SLATE,.02)]
saw_path=export("SLAD_AserraderoKit_v1",saw)

# Wall segment: one representative authored curtain, same foundation/stone/timber/slate hierarchy.
wall=[]
wall += [cube("Wall foundation",(0,0,.20),(4.6,1.05,.40),STONE_DARK,.06)]
wall += [cube("Wall ashlar body",(0,0,.92),(4.35,.82,1.48),STONE,.07)]
for x in (-1.78,0,1.78):
    wall.append(cube("Wall buttress",(x,-.08,1.02),(.38,1.08,1.78),STONE,.05))
wall += [cube("Wall timber band",(0,0,1.56),(4.46,.91,.18),TIMBER,.03)]
for x in (-1.78,-.89,0,.89,1.78):
    wall.append(cube("Wall merlon",(x,0,1.94),(.54,.82,.62),STONE,.04))
for x in (-1.33,-.44,.44,1.33):
    wall.append(cube("Wall slate cap",(x,0,1.67),(.58,.94,.12),SLATE,.02))
wall_path=export("SLAD_WallSegment_v1",wall)

bpy.ops.wm.save_as_mainfile(filepath=SRCBLEND)

def tris(objs):
    n=0
    for o in objs:o.data.calc_loop_triangles();n+=len(o.data.loop_triangles)
    return n
report={"status":"PASS","source_blend":"pipeline/candidates/valoria-source-level-art-direction-proof-v1/Valoria_SLAD_Source_v1.blend",
"outputs":[
 {"name":"hero_interface","file":"SLAD_HeroRockInterface_v1.glb","triangles":tris(hero),"bytes":os.path.getsize(hero_path)},
 {"name":"aserradero_kit","file":"SLAD_AserraderoKit_v1.glb","triangles":tris(saw),"bytes":os.path.getsize(saw_path)},
 {"name":"wall_segment","file":"SLAD_WallSegment_v1.glb","triangles":tris(wall),"bytes":os.path.getsize(wall_path)}],
"shared_materials":["warm_limestone","foundation_stone","dark_oak","blue_slate","wrought_iron"],"tripo_credits":0}
with open(EVID,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("VALORIA_SLAD_BLENDER=PASS")
