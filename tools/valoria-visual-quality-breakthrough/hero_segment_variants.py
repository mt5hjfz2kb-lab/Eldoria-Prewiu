import bpy, bmesh, json, os
from collections import defaultdict

src=os.environ["ELDORIA_HERO_GLB"]
out_dir=os.environ["ELDORIA_HERO_SEGMENT_DIR"]
report_path=os.environ["ELDORIA_HERO_SEGMENT_REPORT"]
fractions=[0.14,0.20,0.26]

def import_source():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=src, merge_vertices=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    if len(meshes)!=1:
        raise RuntimeError("Expected exactly one Hero mesh, got %d"%len(meshes))
    return meshes[0]

def component_groups(obj):
    me=obj.data
    parent=list(range(len(me.vertices))); rank=[0]*len(parent)
    def find(x):
        while parent[x]!=x:
            parent[x]=parent[parent[x]]
            x=parent[x]
        return x
    def union(a,b):
        ra,rb=find(a),find(b)
        if ra==rb:return
        if rank[ra]<rank[rb]: ra,rb=rb,ra
        parent[rb]=ra
        if rank[ra]==rank[rb]:rank[ra]+=1
    for p in me.polygons:
        vs=list(p.vertices)
        if len(vs)>1:
            for v in vs[1:]:union(vs[0],v)
    groups=defaultdict(lambda:{"verts":set(),"faces":[],"max_z":-1e9,"tris":0})
    world=[obj.matrix_world @ v.co for v in me.vertices]
    for p in me.polygons:
        if not p.vertices:continue
        root=find(p.vertices[0]);g=groups[root]
        g["faces"].append(p.index);g["verts"].update(p.vertices);g["tris"]+=max(1,len(p.vertices)-2)
    for g in groups.values():
        g["max_z"]=max(world[i].z for i in g["verts"])
    return groups

os.makedirs(out_dir,exist_ok=True)
baseline=import_source()
all_world=[baseline.matrix_world @ v.co for v in baseline.data.vertices]
zmin=min(p.z for p in all_world); zmax=max(p.z for p in all_world); height=zmax-zmin
baseline.data.calc_loop_triangles()
baseline_tris=len(baseline.data.loop_triangles)
del baseline

report={"source":src,"baseline_triangles":baseline_tris,"zmin":zmin,"zmax":zmax,"height":height,"variants":[]}

for frac in fractions:
    obj=import_source()
    groups=component_groups(obj)
    threshold=zmin+height*frac
    remove_roots=[root for root,g in groups.items() if g["max_z"]<=threshold]
    remove_verts=set()
    remove_tris=0
    for root in remove_roots:
        remove_verts.update(groups[root]["verts"]); remove_tris+=groups[root]["tris"]

    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    bm=bmesh.from_edit_mesh(obj.data)
    bm.verts.ensure_lookup_table()
    for v in bm.verts:v.select=False
    for i in remove_verts:
        if i < len(bm.verts):bm.verts[i].select=True
    bmesh.update_edit_mesh(obj.data)
    bpy.ops.mesh.delete(type='VERT')
    bpy.ops.object.mode_set(mode='OBJECT')

    obj.data.calc_loop_triangles()
    remaining=len(obj.data.loop_triangles)
    name="Valoria_HeroBastion_segmented_low%02d"%round(frac*100)
    obj.name=name
    out=os.path.join(out_dir,name+".glb")
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.gltf(filepath=out,export_format='GLB',use_selection=True)
    if not os.path.isfile(out) or os.path.getsize(out)<100000:
        raise RuntimeError("Invalid export "+out)
    report["variants"].append({
        "fraction":frac,"threshold_z":threshold,"removed_components":len(remove_roots),
        "removed_triangles":remove_tris,"removed_fraction":remove_tris/max(1,baseline_tris),
        "remaining_triangles":remaining,"path":out,"bytes":os.path.getsize(out)
    })

os.makedirs(os.path.dirname(report_path),exist_ok=True)
with open(report_path,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("VALORIA_HERO_SEGMENTATION=PASS")
print(json.dumps(report,indent=2))
