"""Zero-cost source authoring proof for Valoria Authored Secondary Art Family v1.

Outputs:
- Aserradero authored semantic-material GLB (source geometry preserved)
- WallSupport authored module GLB
- GateSupport authored module GLB
- JSON report with source/output metrics and semantic face counts

No Tripo, no paid generation.
"""
import bpy, os, json, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE", os.getcwd())
SRC=os.environ.get("ELDORIA_ASF_SOURCE", os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","Valoria_Aserradero_AP2_v1.glb"))
OUT=os.environ.get("ELDORIA_ASF_OUT", os.path.join(ROOT,"pipeline","candidates","valoria-authored-secondary-art-family-v1"))
REPORT=os.environ.get("ELDORIA_ASF_REPORT", os.path.join(ROOT,"pipeline","evidence","valoria-authored-secondary-art-family-v1.json"))
os.makedirs(OUT,exist_ok=True); os.makedirs(os.path.dirname(REPORT),exist_ok=True)

def meshes():
    return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]

def tris(objs):
    n=0
    for o in objs:
        o.data.calc_loop_triangles(); n+=len(o.data.loop_triangles)
    return n

def join(objs,name):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.object.join(); objs[0].name=name
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    return objs[0]

def principled(mat):
    if not mat or not mat.use_nodes:return None
    for n in mat.node_tree.nodes:
        if n.type=="BSDF_PRINCIPLED": return n
    return None

def semantic_copy(base,name,color,roughness):
    m=base.copy() if base else bpy.data.materials.new(name)
    m.name=name; m.use_nodes=True
    bs=principled(m)
    if bs is None:
        nodes=m.node_tree.nodes; links=m.node_tree.links
        for n in list(nodes):nodes.remove(n)
        out=nodes.new("ShaderNodeOutputMaterial"); bs=nodes.new("ShaderNodeBsdfPrincipled")
        links.new(bs.outputs["BSDF"],out.inputs["Surface"])
    # Keep linked source basecolor/normal where present. Diffuse factor gives exporter a semantic tint.
    try: m.diffuse_color=(*color,1.0)
    except: pass
    try: bs.inputs["Base Color"].default_value=(*color,1.0)
    except: pass
    try: bs.inputs["Roughness"].default_value=roughness
    except: pass
    return m

def flat_material(name,color,roughness):
    m=bpy.data.materials.new(name);m.use_nodes=True
    bs=principled(m);bs.inputs["Base Color"].default_value=(*color,1.0);bs.inputs["Roughness"].default_value=roughness
    m.diffuse_color=(*color,1.0)
    return m

def export_selected(path,objs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,
                              export_materials="EXPORT",export_yup=True,export_extras=True)

def cube(name,loc,scale,mat,bevel=.06):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel>0:
        mod=o.modifiers.new("ASF bevel","BEVEL");mod.width=bevel;mod.segments=2
        mod.limit_method="ANGLE";bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
    o.data.materials.append(mat);return o

def roof_cap(name,loc,radius,depth,mat):
    bpy.ops.mesh.primitive_cone_add(vertices=4,radius1=radius,radius2=0.05,depth=depth,location=loc,rotation=(0,0,math.radians(45)))
    o=bpy.context.object;o.name=name;o.data.materials.append(mat);return o

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=SRC)
src_objs=meshes()
if not src_objs: raise RuntimeError("Aserradero source imported no meshes")
source_metrics={"mesh_objects":len(src_objs),"triangles":tris(src_objs),"materials":sum(len(o.data.materials) for o in src_objs),"bytes":os.path.getsize(SRC)}
if not (49000 <= source_metrics["triangles"] <= 50500):
    raise RuntimeError("Unexpected Aserradero triangle count: "+str(source_metrics))
aserradero=join(src_objs,"ASF_Aserradero_Source")
base=aserradero.data.materials[0] if aserradero.data.materials else None

# Semantic source authoring: preserve geometry exactly, change only material membership.
stone=semantic_copy(base,"ASF_Stone",(0.72,0.68,0.58),0.82)
timber=semantic_copy(base,"ASF_Timber",(0.46,0.29,0.15),0.72)
roof=semantic_copy(base,"ASF_Roof",(0.24,0.33,0.38),0.88)
aserradero.data.materials.clear()
for m in (stone,timber,roof):aserradero.data.materials.append(m)

zs=[v.co.z for v in aserradero.data.vertices]; z0=min(zs); z1=max(zs); zh=max(1e-6,z1-z0)
counts={"stone":0,"timber":0,"roof":0}
for p in aserradero.data.polygons:
    h=(p.center.z-z0)/zh
    nz=abs(p.normal.z)
    if h>0.40 and nz>0.28:
        p.material_index=2;counts["roof"]+=1
    elif h<0.28 or (h<0.44 and nz<0.22):
        p.material_index=0;counts["stone"]+=1
    else:
        p.material_index=1;counts["timber"]+=1
aserradero["eldoria_family"]="AuthoredSecondaryArtFamily_v1"
aserradero["eldoria_semantic_faces"]=counts
aserradero_path=os.path.join(OUT,"Valoria_ASF_Aserradero_v1.glb")
export_selected(aserradero_path,[aserradero])
authored_metrics={"triangles":tris([aserradero]),"materials":len(aserradero.data.materials),"semantic_polygons":counts,"bytes":os.path.getsize(aserradero_path)}
if authored_metrics["triangles"]!=source_metrics["triangles"]:
    raise RuntimeError("Semantic authoring unexpectedly changed source triangle count")

# Shared support palette: intentionally simple authored surfaces; Unity proof applies the same coherence shader family.
sstone=flat_material("ASF_Stone_Support",(0.63,0.61,0.55),0.84)
stimber=flat_material("ASF_Timber_Support",(0.34,0.22,0.12),0.76)
sroof=flat_material("ASF_Roof_Support",(0.22,0.30,0.34),0.90)

wall=[]
wall.append(cube("ASF_Wall_StoneBody",(0,0,0.48),(4.0,0.72,0.96),sstone,.08))
wall.append(cube("ASF_Wall_TimberCap",(0,0,1.00),(4.12,0.82,0.20),stimber,.05))
for x in (-1.55,0,1.55):
    wall.append(cube("ASF_Wall_Buttress",(x,-.05,.62),(.34,.92,1.24),sstone,.06))
wall_path=os.path.join(OUT,"Valoria_ASF_WallSupport_v1.glb");export_selected(wall_path,wall)

gate=[]
for x in (-1.22,1.22):
    gate.append(cube("ASF_Gate_Pier",(x,0,1.02),(.72,.88,2.04),sstone,.08))
    gate.append(cube("ASF_Gate_TimberBand",(x,-.02,1.38),(.84,.98,.20),stimber,.04))
    gate.append(roof_cap("ASF_Gate_RoofCap",(x,0,2.33),.62,.72,sroof))
gate.append(cube("ASF_Gate_Crossbeam",(0,0,1.86),(1.95,.64,.30),stimber,.05))
gate.append(cube("ASF_Gate_StoneLintel",(0,.03,2.10),(1.90,.72,.26),sstone,.05))
gate_path=os.path.join(OUT,"Valoria_ASF_GateSupport_v1.glb");export_selected(gate_path,gate)

report={
 "status":"PASS",
 "source":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb",
 "source_metrics":source_metrics,
 "aserradero_authored":authored_metrics,
 "outputs":{
   "aserradero":{"path":"pipeline/candidates/valoria-authored-secondary-art-family-v1/Valoria_ASF_Aserradero_v1.glb","bytes":os.path.getsize(aserradero_path)},
   "wall_support":{"path":"pipeline/candidates/valoria-authored-secondary-art-family-v1/Valoria_ASF_WallSupport_v1.glb","triangles":tris(wall),"bytes":os.path.getsize(wall_path)},
   "gate_support":{"path":"pipeline/candidates/valoria-authored-secondary-art-family-v1/Valoria_ASF_GateSupport_v1.glb","triangles":tris(gate),"bytes":os.path.getsize(gate_path)}
 },
 "geometry_change_aserradero":False,
 "shared_language":["ASF_Stone","ASF_Timber","ASF_Roof"],
 "tripo_credits":0
}
with open(REPORT,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("ELDORIA_ASF_REPORT="+json.dumps(report))
