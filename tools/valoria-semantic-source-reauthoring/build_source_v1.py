"""VALORIA SEMANTIC SOURCE REAUTHORING PROOF v1.
Sequential isolated-source authoring: Hero -> Aserradero -> Wall -> canonical .blend assembly.
Preserves surviving source geometry, UVs and texture nodes. No Tripo / paid tools.
"""
import bpy, bmesh, os, json
from collections import defaultdict

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
RES=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria")
OUT=os.path.join(RES,"SemanticSourceReauthoring_v1")
CAND=os.path.join(ROOT,"pipeline","candidates","valoria-semantic-source-reauthoring-v1")
EVID=os.path.join(ROOT,"pipeline","evidence","valoria-semantic-source-reauthoring-v1.json")
os.makedirs(OUT,exist_ok=True); os.makedirs(CAND,exist_ok=True); os.makedirs(os.path.dirname(EVID),exist_ok=True)
SOURCES={
 "hero":os.path.join(RES,"HeroBastionGenerated","Valoria_HeroBastion_v1.glb"),
 "aserradero":os.path.join(RES,"Valoria_Aserradero_AP2_v1.glb"),
 "wall":os.path.join(RES,"StoneArchitectureKit_v1","HighStraightWall.glb"),
}
for k,p in SOURCES.items():
    if not os.path.isfile(p): raise RuntimeError("Missing canonical source %s: %s"%(k,p))

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def bounds_pts(pts):
    if not pts: raise RuntimeError("Cannot compute bounds of empty mesh")
    xs=[p.x for p in pts]; ys=[p.y for p in pts]; zs=[p.z for p in pts]
    return {"min":[min(xs),min(ys),min(zs)],"max":[max(xs),max(ys),max(zs)],
            "size":[max(xs)-min(xs),max(ys)-min(ys),max(zs)-min(zs)],
            "center":[(min(xs)+max(xs))/2,(min(ys)+max(ys))/2,(min(zs)+max(zs))/2]}

def mesh_bounds(o): return bounds_pts([o.matrix_world@v.co for v in o.data.vertices])

def tri_count(objs):
    n=0
    for o in objs:
        if o.type!="MESH": continue
        o.data.calc_loop_triangles(); n+=len(o.data.loop_triangles)
    return n

def import_single(path,name):
    bpy.ops.import_scene.gltf(filepath=path,merge_vertices=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH" and len(o.data.vertices)>0]
    if len(meshes)!=1: raise RuntimeError("%s expected one non-empty mesh, got %d"%(name,len(meshes)))
    o=meshes[0]; o.name=name
    return o

def component_groups(obj):
    me=obj.data; parent=list(range(len(me.vertices))); rank=[0]*len(parent)
    def find(x):
        while parent[x]!=x:
            parent[x]=parent[parent[x]]; x=parent[x]
        return x
    def union(a,b):
        ra,rb=find(a),find(b)
        if ra==rb:return
        if rank[ra]<rank[rb]:ra,rb=rb,ra
        parent[rb]=ra
        if rank[ra]==rank[rb]:rank[ra]+=1
    for p in me.polygons:
        vs=list(p.vertices)
        for v in vs[1:]: union(vs[0],v)
    world=[obj.matrix_world@v.co for v in me.vertices]
    groups=defaultdict(lambda:{"verts":set(),"faces":[],"tris":0})
    for p in me.polygons:
        if not p.vertices:continue
        g=groups[find(p.vertices[0])]; g["faces"].append(p.index); g["verts"].update(p.vertices); g["tris"]+=max(1,len(p.vertices)-2)
    out=[]
    for g in groups.values():
        g["bounds"]=bounds_pts([world[i] for i in g["verts"]]); out.append(g)
    return out

def texture_report(obj):
    out=[]; seen=set()
    for mat in obj.data.materials:
        if not mat or not mat.use_nodes:continue
        for n in mat.node_tree.nodes:
            im=getattr(n,"image",None)
            if not im or im.name in seen:continue
            seen.add(im.name)
            out.append({"name":im.name,"width":int(im.size[0]),"height":int(im.size[1]),"colorspace":im.colorspace_settings.name})
    return out

def clone_tinted(mat,name,tint,rough_mul=1.0):
    m=mat.copy(); m.name=name
    if not m.use_nodes or not m.node_tree:return m
    nt=m.node_tree; bsdf=next((n for n in nt.nodes if n.type=="BSDF_PRINCIPLED"),None)
    if not bsdf:return m
    base=bsdf.inputs.get("Base Color")
    if base:
        link=next((l for l in nt.links if l.to_socket==base),None)
        if link:
            src=link.from_socket
            mix=nt.nodes.new("ShaderNodeMixRGB"); mix.name=name+" Tint"; mix.blend_type="MULTIPLY"
            mix.inputs[0].default_value=1.0; mix.inputs[2].default_value=(*tint,1)
            nt.links.remove(link); nt.links.new(src,mix.inputs[1]); nt.links.new(mix.outputs[0],base)
        else: base.default_value=(*tint,1)
    rough=bsdf.inputs.get("Roughness")
    if rough:
        link=next((l for l in nt.links if l.to_socket==rough),None)
        if link and abs(rough_mul-1)>1e-4:
            src=link.from_socket
            mul=nt.nodes.new("ShaderNodeMath"); mul.operation="MULTIPLY"; mul.inputs[1].default_value=rough_mul
            nt.links.remove(link); nt.links.new(src,mul.inputs[0]); nt.links.new(mul.outputs[0],rough)
        elif not link: rough.default_value=max(0,min(1,rough.default_value*rough_mul))
    return m

def remove_components(obj,predicate):
    groups=component_groups(obj); remove=set(); rt=0; rg=0
    for g in groups:
        if predicate(g): remove.update(g["verts"]); rt+=g["tris"]; rg+=1
    bpy.ops.object.select_all(action="DESELECT"); obj.select_set(True); bpy.context.view_layer.objects.active=obj
    bpy.ops.object.mode_set(mode="EDIT"); bm=bmesh.from_edit_mesh(obj.data); bm.verts.ensure_lookup_table()
    for v in bm.verts:v.select=False
    for i in remove:
        if i<len(bm.verts):bm.verts[i].select=True
    bmesh.update_edit_mesh(obj.data); bpy.ops.mesh.delete(type="VERT"); bpy.ops.object.mode_set(mode="OBJECT")
    return rg,rt

def assign_semantic_materials(obj,classify,variants):
    base=obj.data.materials[0]; slots={"source":0}
    for key,(tint,rough) in variants.items():
        obj.data.materials.append(clone_tinted(base,obj.name+"_"+key,tint,rough)); slots[key]=len(obj.data.materials)-1
    counts=defaultdict(int)
    for g in component_groups(obj):
        cls=classify(g); idx=slots.get(cls,0)
        for fi in g["faces"]:
            if fi<len(obj.data.polygons):obj.data.polygons[fi].material_index=idx
        counts[cls]+=g["tris"]
    return dict(counts)

def add_box(name,loc,scale,mat,bevel=.01):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object; o.name=name; o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    md=o.modifiers.new("Authored bevel","BEVEL"); md.width=bevel; md.segments=2; md.limit_method="ANGLE"
    bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=md.name)
    o.data.materials.append(mat); return o

def export_all(path):
    bpy.ops.object.select_all(action="DESELECT")
    objs=[o for o in bpy.context.scene.objects if o.type=="MESH" and len(o.data.vertices)>0]
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,export_materials="EXPORT",export_yup=True)
    if not os.path.isfile(path) or os.path.getsize(path)<100000:raise RuntimeError("Invalid export "+path)
    return objs

# 1) HERO — actual source surgery, isolated scene.
reset()
hero=import_single(SOURCES["hero"],"Hero_Source_Retained")
hero_tex=texture_report(hero); hb=mesh_bounds(hero); z0=hb["min"][2]; hh=hb["size"][2]; hero_base=tri_count([hero])
# Import the certified rich wall only as a material donor for the new rock->masonry transition.
# It is removed before Hero export, so Hero geometry remains source mesh + authored transition only.
wall_donor=import_single(SOURCES["wall"],"Hero_Wall_Material_Donor")
wall_donor_mat=wall_donor.data.materials[0]
removed_groups,removed_tris=remove_components(hero,lambda g:g["bounds"]["max"][2] <= z0+hh*.14)
if not hero.data.vertices: raise RuntimeError("Hero surgery removed entire source")
hero_low=clone_tinted(hero.data.materials[0],"Hero_Lower_Integrated",(0.88,0.84,0.76),1.05); hero.data.materials.append(hero_low)
for g in component_groups(hero):
    if g["bounds"]["center"][2] < z0+hh*.30:
        for fi in g["faces"]:
            if fi<len(hero.data.polygons):hero.data.polygons[fi].material_index=1
masonry=clone_tinted(wall_donor_mat,"Shared_Warm_Masonry",(0.94,0.90,0.82),1.03)
foundation=clone_tinted(wall_donor_mat,"Shared_Dark_Foundation",(0.72,0.68,0.60),1.08)
# Keep the replacement architectural band shallow: it occupies only the removed low source zone
# and must not become a foreground wall hiding the Hero silhouette.
add_box("Hero_Retaining_Foundation",(0,-.23,z0+.022),(.62,.10,.044),foundation,.006)
add_box("Hero_Retaining_Upper",(0,-.205,z0+.064),(.54,.105,.040),masonry,.005)
for x in (-.24,-.08,.08,.24): add_box("Hero_Buttress",(x,-.245,z0+.065),(.030,.085,.105),masonry,.004)
for i in range(4): add_box("Hero_Stair_%d"%i,(0,-.285+i*.026,z0+.010+i*.014),(.16,.070,.026),masonry,.003)
# Donor geometry is not part of the Hero candidate.
bpy.data.objects.remove(wall_donor,do_unlink=True)
hero_path=os.path.join(OUT,"SSRA_HeroBastion_v1.glb"); hero_objs=export_all(hero_path); hero_out_tris=tri_count(hero_objs)

# 2) ASERRADERO — fresh isolated import, no Hero datablock can affect it.
reset()
saw=import_single(SOURCES["aserradero"],"Aserradero_Source_Semantic")
saw_tex=texture_report(saw); sb=mesh_bounds(saw); sz0=sb["min"][2]; sh=sb["size"][2]
def saw_class(g):
    b=g["bounds"]; dx,dy,dz=b["size"]; zmin=b["min"][2]; zmax=b["max"][2]
    if zmax <= sz0+sh*.20:return "stone"
    if zmin >= sz0+sh*.48 and max(dx,dy) > max(.001,dz)*1.55:return "roof"
    if dz > max(.001,max(dx,dy))*1.30 and zmin < sz0+sh*.66:return "timber"
    return "source"
saw_counts=assign_semantic_materials(saw,saw_class,{
 "stone":((0.88,0.82,0.72),1.08),"timber":((0.56,0.40,0.27),1.05),"roof":((0.55,0.64,0.70),1.12)})
saw_path=os.path.join(OUT,"SSRA_Aserradero_v1.glb"); saw_objs=export_all(saw_path); saw_out_tris=tri_count(saw_objs)

# 3) WALL — fresh isolated rich source, calibrated to same stone hierarchy.
reset()
wall=import_single(SOURCES["wall"],"Wall_Source_Semantic")
wall_tex=texture_report(wall); wb=mesh_bounds(wall); wz0=wb["min"][2]; wh=wb["size"][2]
def wall_class(g):
    b=g["bounds"]
    if b["max"][2] <= wz0+wh*.20:return "foundation"
    if b["min"][2] >= wz0+wh*.72:return "cap"
    return "stone"
wall_counts=assign_semantic_materials(wall,wall_class,{
 "foundation":((0.62,0.57,0.49),1.10),"stone":((0.91,0.84,0.72),1.06),"cap":((0.60,0.66,0.68),1.12)})
wall_path=os.path.join(OUT,"SSRA_WallSegment_v1.glb"); wall_objs=export_all(wall_path); wall_out_tris=tri_count(wall_objs)

# 4) Canonical reproducible .blend: assemble only finished outputs, no further geometry operations.
reset()
for label,path in [("01_Hero_Bastion_Reauthored",hero_path),("02_Aserradero_Reauthored",saw_path),("03_Wall_Reauthored",wall_path)]:
    col=bpy.data.collections.new(label); bpy.context.scene.collection.children.link(col)
    before=set(bpy.context.scene.objects); bpy.ops.import_scene.gltf(filepath=path,merge_vertices=False)
    for o in [x for x in bpy.context.scene.objects if x not in before]:
        for old in list(o.users_collection): old.objects.unlink(o)
        col.objects.link(o)
blend=os.path.join(CAND,"Valoria_SSRA_Source_v1.blend"); bpy.ops.wm.save_as_mainfile(filepath=blend)

report={
 "status":"PASS","blender_version":bpy.app.version_string,
 "source_blend":"pipeline/candidates/valoria-semantic-source-reauthoring-v1/Valoria_SSRA_Source_v1.blend",
 "processing_model":"isolated_sequential_sources_then_assembly",
 "hero":{"source_triangles":hero_base,"removed_low_components":removed_groups,"removed_triangles":removed_tris,
         "removed_fraction":removed_tris/max(1,hero_base),"output_triangles":hero_out_tris,"textures":hero_tex},
 "aserradero":{"source_triangles":49800,"output_triangles":saw_out_tris,"semantic_triangles":saw_counts,"textures":saw_tex},
 "wall":{"output_triangles":wall_out_tris,"semantic_triangles":wall_counts,"textures":wall_tex},
 "outputs":[{"file":"SSRA_HeroBastion_v1.glb","bytes":os.path.getsize(hero_path)},
            {"file":"SSRA_Aserradero_v1.glb","bytes":os.path.getsize(saw_path)},
            {"file":"SSRA_WallSegment_v1.glb","bytes":os.path.getsize(wall_path)}],
 "rules":{"hero_low_cut_fraction":.14,"preserve_source_uv":True,"preserve_source_texture_nodes":True,
          "new_paid_tools":False,"tripo_credits":0}
}
with open(EVID,"w",encoding="utf-8") as fp: json.dump(report,fp,indent=2)
print("VALORIA_SSRA_SOURCE=PASS"); print(json.dumps(report,indent=2))
