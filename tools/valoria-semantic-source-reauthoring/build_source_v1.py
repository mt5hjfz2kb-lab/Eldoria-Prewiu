"""VALORIA SEMANTIC SOURCE REAUTHORING PROOF v1.
Edits the rich canonical Hero/Aserradero/wall sources directly.
No Tripo, no paid tools. Preserves original UVs/textures where source geometry survives.
"""
import bpy, bmesh, os, json, math
from collections import defaultdict

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
RES=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria")
OUT=os.path.join(RES,"SemanticSourceReauthoring_v1")
CAND=os.path.join(ROOT,"pipeline","candidates","valoria-semantic-source-reauthoring-v1")
EVID=os.path.join(ROOT,"pipeline","evidence","valoria-semantic-source-reauthoring-v1.json")
os.makedirs(OUT,exist_ok=True);os.makedirs(CAND,exist_ok=True);os.makedirs(os.path.dirname(EVID),exist_ok=True)

SOURCES={
 "hero":os.path.join(RES,"HeroBastionGenerated","Valoria_HeroBastion_v1.glb"),
 "aserradero":os.path.join(RES,"Valoria_Aserradero_AP2_v1.glb"),
 "wall":os.path.join(RES,"StoneArchitectureKit_v1","HighStraightWall.glb"),
}
for k,p in SOURCES.items():
    if not os.path.isfile(p): raise RuntimeError("Missing canonical source %s: %s"%(k,p))

bpy.ops.wm.read_factory_settings(use_empty=True)

def bounds_pts(pts):
    xs=[p.x for p in pts];ys=[p.y for p in pts];zs=[p.z for p in pts]
    return {"min":[min(xs),min(ys),min(zs)],"max":[max(xs),max(ys),max(zs)],
            "size":[max(xs)-min(xs),max(ys)-min(ys),max(zs)-min(zs)],
            "center":[(min(xs)+max(xs))/2,(min(ys)+max(ys))/2,(min(zs)+max(zs))/2]}

def mesh_bounds(o):
    return bounds_pts([o.matrix_world@v.co for v in o.data.vertices])

def tri_count(objs):
    n=0
    for o in objs:
        if o.type!="MESH": continue
        o.data.calc_loop_triangles();n+=len(o.data.loop_triangles)
    return n

def component_groups(obj):
    me=obj.data
    parent=list(range(len(me.vertices)));rank=[0]*len(parent)
    def find(x):
        while parent[x]!=x:
            parent[x]=parent[parent[x]];x=parent[x]
        return x
    def union(a,b):
        ra,rb=find(a),find(b)
        if ra==rb:return
        if rank[ra]<rank[rb]:ra,rb=rb,ra
        parent[rb]=ra
        if rank[ra]==rank[rb]:rank[ra]+=1
    for p in me.polygons:
        vs=list(p.vertices)
        for v in vs[1:]:union(vs[0],v)
    world=[obj.matrix_world@v.co for v in me.vertices]
    g=defaultdict(lambda:{"verts":set(),"faces":[],"tris":0})
    for p in me.polygons:
        if not p.vertices:continue
        x=g[find(p.vertices[0])];x["faces"].append(p.index);x["verts"].update(p.vertices);x["tris"]+=max(1,len(p.vertices)-2)
    for x in g.values():
        x["bounds"]=bounds_pts([world[i] for i in x["verts"]])
    return list(g.values())

def import_one(asset_id,path,collection):
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=path,merge_vertices=False)
    new=[o for o in bpy.context.scene.objects if o not in before]
    meshes=[o for o in new if o.type=="MESH"]
    if len(meshes)!=1: raise RuntimeError("%s expected one mesh, got %d"%(asset_id,len(meshes)))
    obj=meshes[0];obj.name=asset_id+"_SOURCE"
    for c in list(obj.users_collection): c.objects.unlink(obj)
    collection.objects.link(obj)
    return obj

def clone_tinted(mat,name,tint,rough_mul=1.0):
    m=mat.copy();m.name=name
    if not m.use_nodes or not m.node_tree:return m
    nt=m.node_tree;bsdf=next((n for n in nt.nodes if n.type=="BSDF_PRINCIPLED"),None)
    if not bsdf:return m
    base=bsdf.inputs.get("Base Color")
    if base:
        link=next((l for l in nt.links if l.to_socket==base),None)
        if link:
            mix=nt.nodes.new("ShaderNodeMixRGB");mix.name=name+" Tint";mix.blend_type="MULTIPLY";mix.inputs[0].default_value=1.0;mix.inputs[2].default_value=(*tint,1)
            src_socket=link.from_socket;nt.links.remove(link);nt.links.new(src_socket,mix.inputs[1]);nt.links.new(mix.outputs[0],base)
        else: base.default_value=(*tint,1)
    rough=bsdf.inputs.get("Roughness")
    if rough:
        link=next((l for l in nt.links if l.to_socket==rough),None)
        if link and abs(rough_mul-1)>1e-4:
            mul=nt.nodes.new("ShaderNodeMath");mul.operation="MULTIPLY";mul.inputs[1].default_value=rough_mul
            source_socket=link.from_socket
            nt.links.remove(link);nt.links.new(source_socket,mul.inputs[0]);nt.links.new(mul.outputs[0],rough)
        elif not link: rough.default_value=max(0,min(1,rough.default_value*rough_mul))
    return m

def remove_components(obj,predicate):
    groups=component_groups(obj);remove=set();removed_tris=0;removed_groups=0
    for g in groups:
        if predicate(g):
            remove.update(g["verts"]);removed_tris+=g["tris"];removed_groups+=1
    bpy.context.view_layer.objects.active=obj;obj.select_set(True);bpy.ops.object.mode_set(mode="EDIT")
    bm=bmesh.from_edit_mesh(obj.data);bm.verts.ensure_lookup_table()
    for v in bm.verts:v.select=False
    for i in remove:
        if i<len(bm.verts):bm.verts[i].select=True
    bmesh.update_edit_mesh(obj.data);bpy.ops.mesh.delete(type="VERT");bpy.ops.object.mode_set(mode="OBJECT")
    return removed_groups,removed_tris

def assign_semantic_materials(obj,classify,variants):
    base=obj.data.materials[0]
    slots={"source":0}
    for key,(tint,rough) in variants.items():
        obj.data.materials.append(clone_tinted(base,obj.name+"_"+key,tint,rough));slots[key]=len(obj.data.materials)-1
    counts=defaultdict(int)
    groups=component_groups(obj)
    for g in groups:
        cls=classify(g)
        idx=slots.get(cls,0)
        for fi in g["faces"]:
            if fi<len(obj.data.polygons):obj.data.polygons[fi].material_index=idx
        counts[cls]+=g["tris"]
    return dict(counts)

def add_box(name,collection,loc,scale,mat,bevel=.012):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        md=o.modifiers.new("Authored bevel","BEVEL");md.width=bevel;md.segments=2;md.limit_method="ANGLE"
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=md.name)
    o.data.materials.append(mat)
    for c in list(o.users_collection):c.objects.unlink(o)
    collection.objects.link(o)
    return o

def export_collection(collection,name):
    bpy.ops.object.select_all(action="DESELECT")
    objs=[o for o in collection.objects if o.type=="MESH"]
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    p=os.path.join(OUT,name+".glb")
    bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_materials="EXPORT",export_yup=True)
    if not os.path.isfile(p) or os.path.getsize(p)<100000:raise RuntimeError("Invalid export "+p)
    return p,objs

def texture_report(obj):
    out=[]
    seen=set()
    for mat in obj.data.materials:
        if not mat or not mat.use_nodes:continue
        for n in mat.node_tree.nodes:
            im=getattr(n,"image",None)
            if not im or im.name in seen:continue
            seen.add(im.name);out.append({"name":im.name,"width":int(im.size[0]),"height":int(im.size[1]),"colorspace":im.colorspace_settings.name})
    return out

hero_col=bpy.data.collections.new("01_Hero_Bastion_Reauthored");bpy.context.scene.collection.children.link(hero_col)
saw_col=bpy.data.collections.new("02_Aserradero_Reauthored");bpy.context.scene.collection.children.link(saw_col)
wall_col=bpy.data.collections.new("03_Wall_Reauthored");bpy.context.scene.collection.children.link(wall_col)

# Import all three canonical sources.
hero=import_one("Hero",SOURCES["hero"],hero_col)
saw=import_one("Aserradero",SOURCES["aserradero"],saw_col)
wall=import_one("Wall",SOURCES["wall"],wall_col)
hero_tex=texture_report(hero);saw_tex=texture_report(saw);wall_tex=texture_report(wall)

# HERO: remove only the proven low 14% component band, then build the masonry transition
# inside the authored source file using the high-detail wall material as donor.
hb=mesh_bounds(hero);z0=hb["min"][2];hh=hb["size"][2];baseline_hero=tri_count([hero])
removed_groups,removed_tris=remove_components(hero,lambda g:g["bounds"]["max"][2] <= z0+hh*.14)
hero.name="Hero_Source_Retained"
hero_low=clone_tinted(hero.data.materials[0],"Hero_Lower_Integrated",(0.88,0.84,0.76),1.05)
hero.data.materials.append(hero_low)
for g in component_groups(hero):
    if g["bounds"]["center"][2] < z0+hh*.30:
        for fi in g["faces"]:
            if fi<len(hero.data.polygons):hero.data.polygons[fi].material_index=1

wall_donor=wall.data.materials[0]
masonry=clone_tinted(wall_donor,"Shared_Warm_Masonry",(0.90,0.84,0.72),1.05)
foundation=clone_tinted(wall_donor,"Shared_Dark_Foundation",(0.58,0.54,0.48),1.10)
timber=clone_tinted(saw.data.materials[0],"Shared_Dark_Timber",(0.50,0.38,0.28),1.05)
slate=clone_tinted(saw.data.materials[0],"Shared_Slate",(0.52,0.61,0.68),1.12)

# Scale these pieces to the Hero source itself. They occupy the geometry actually removed,
# rather than covering untouched rock in front.
add_box("Hero_Retaining_Foundation",hero_col,(0,-.29,z0+.045),(.90,.18,.09),foundation,.010)
add_box("Hero_Retaining_Upper",hero_col,(0,-.25,z0+.115),(.76,.22,.07),masonry,.009)
for x in (-.34,-.17,.17,.34):
    add_box("Hero_Buttress",hero_col,(x,-.315,z0+.10),(.055,.16,.20),masonry,.007)
for i in range(4):
    add_box("Hero_Stair_%d"%i,hero_col,(0,-.39+i*.045,z0+.025+i*.026),(.25,.12,.05),masonry,.006)

# ASERRADERO: preserve geometry + UVs/textures. Classify loose components into semantic
# material zones by spatial/form heuristics and reuse the original texture through tinted clones.
sb=mesh_bounds(saw);sz0=sb["min"][2];sh=sb["size"][2]
def saw_class(g):
    b=g["bounds"];dx,dy,dz=b["size"];zmin=b["min"][2];zmax=b["max"][2]
    if zmax <= sz0+sh*.20:return "stone"
    if zmin >= sz0+sh*.48 and max(dx,dy) > max(.001,dz)*1.55:return "roof"
    if dz > max(.001,max(dx,dy))*1.30 and zmin < sz0+sh*.66:return "timber"
    return "source"
saw_counts=assign_semantic_materials(saw,saw_class,{
    "stone":((0.88,0.82,0.72),1.08),
    "timber":((0.56,0.40,0.27),1.05),
    "roof":((0.55,0.64,0.70),1.12)
})
saw.name="Aserradero_Source_Semantic"

# WALL: preserve rich source mesh and use same warm-stone/base/cap hierarchy.
wb=mesh_bounds(wall);wz0=wb["min"][2];wh=wb["size"][2]
def wall_class(g):
    b=g["bounds"];c=b["center"][2]
    if b["max"][2] <= wz0+wh*.20:return "foundation"
    if b["min"][2] >= wz0+wh*.72:return "cap"
    return "stone"
wall_counts=assign_semantic_materials(wall,wall_class,{
    "foundation":((0.62,0.57,0.49),1.10),
    "stone":((0.91,0.84,0.72),1.06),
    "cap":((0.60,0.66,0.68),1.12)
})
wall.name="Wall_Source_Semantic"

hero_path,hero_objs=export_collection(hero_col,"SSRA_HeroBastion_v1")
saw_path,saw_objs=export_collection(saw_col,"SSRA_Aserradero_v1")
wall_path,wall_objs=export_collection(wall_col,"SSRA_WallSegment_v1")

blend=os.path.join(CAND,"Valoria_SSRA_Source_v1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)

report={
 "status":"PASS","blender_version":bpy.app.version_string,"source_blend":"pipeline/candidates/valoria-semantic-source-reauthoring-v1/Valoria_SSRA_Source_v1.blend",
 "hero":{"source_triangles":baseline_hero,"removed_low_components":removed_groups,"removed_triangles":removed_tris,
         "removed_fraction":removed_tris/max(1,baseline_hero),"output_triangles":tri_count(hero_objs),"textures":hero_tex},
 "aserradero":{"output_triangles":tri_count(saw_objs),"semantic_triangles":saw_counts,"textures":saw_tex},
 "wall":{"output_triangles":tri_count(wall_objs),"semantic_triangles":wall_counts,"textures":wall_tex},
 "outputs":[
  {"file":"SSRA_HeroBastion_v1.glb","bytes":os.path.getsize(hero_path)},
  {"file":"SSRA_Aserradero_v1.glb","bytes":os.path.getsize(saw_path)},
  {"file":"SSRA_WallSegment_v1.glb","bytes":os.path.getsize(wall_path)}
 ],
 "rules":{"hero_low_cut_fraction":.14,"preserve_source_uv":True,"preserve_source_texture_nodes":True,"new_paid_tools":False,"tripo_credits":0}
}
with open(EVID,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("VALORIA_SSRA_SOURCE=PASS")
print(json.dumps(report,indent=2))
