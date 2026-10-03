import bpy, json, os
from collections import defaultdict

sources=[
 ("aserradero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb"),
 ("cuartel","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb"),
 ("granero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb"),
]
root=os.environ["GITHUB_WORKSPACE"]
out=os.environ["ELDORIA_FUNCTIONAL_AUDIT"]
report={"schema_version":1,"assets":[]}

def world_bounds(obj, indices=None):
    me=obj.data
    ids=indices if indices is not None else range(len(me.vertices))
    pts=[obj.matrix_world @ me.vertices[i].co for i in ids]
    if not pts:return None
    xs=[p.x for p in pts];ys=[p.y for p in pts];zs=[p.z for p in pts]
    return {"min":[min(xs),min(ys),min(zs)],"max":[max(xs),max(ys),max(zs)],
            "size":[max(xs)-min(xs),max(ys)-min(ys),max(zs)-min(zs)]}

for asset_id,rel in sources:
    src=os.path.join(root,rel)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=src,merge_vertices=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    asset={"id":asset_id,"source":rel,"mesh_objects":[],"triangle_count":0,"vertex_count":0,
           "materials":{},"images":[],"connected_components":0}
    seen_images={}
    for obj in meshes:
        me=obj.data;me.calc_loop_triangles()
        asset["triangle_count"]+=len(me.loop_triangles)
        asset["vertex_count"]+=len(me.vertices)

        parent=list(range(len(me.vertices)))
        def find(x):
            while parent[x]!=x:
                parent[x]=parent[parent[x]];x=parent[x]
            return x
        def union(a,b):
            ra,rb=find(a),find(b)
            if ra!=rb:parent[rb]=ra
        for p in me.polygons:
            vs=list(p.vertices)
            for v in vs[1:]:union(vs[0],v)
        comps=set(find(i) for i in range(len(me.vertices)))
        asset["connected_components"]+=len(comps)

        bymat=defaultdict(lambda:{"tris":0,"verts":set()})
        for p in me.polygons:
            bymat[p.material_index]["tris"]+=max(1,len(p.vertices)-2)
            bymat[p.material_index]["verts"].update(p.vertices)

        slot_names=[]
        for idx,mat in enumerate(me.materials):
            slot_names.append(mat.name if mat else None)
            key=(mat.name if mat else "NONE")+"#"+str(idx)
            entry=asset["materials"].setdefault(key,{"name":mat.name if mat else None,"triangles":0,"objects":[],"bounds":[],"nodes":[]})
            if idx in bymat:
                entry["triangles"]+=bymat[idx]["tris"]
                entry["objects"].append(obj.name)
                entry["bounds"].append(world_bounds(obj,bymat[idx]["verts"]))
            if mat and mat.use_nodes and mat.node_tree:
                for node in mat.node_tree.nodes:
                    if node.type=="TEX_IMAGE" and getattr(node,"image",None):
                        im=node.image
                        dims=[int(im.size[0]),int(im.size[1])]
                        seen_images[im.name]={"name":im.name,"size":dims,"filepath":im.filepath}
                    if node.type in ("BSDF_PRINCIPLED","TEX_IMAGE","NORMAL_MAP","MIX_RGB"):
                        entry["nodes"].append(node.type)

        asset["mesh_objects"].append({
            "name":obj.name,"triangles":len(me.loop_triangles),"vertices":len(me.vertices),
            "material_slots":slot_names,"connected_components":len(comps),"bounds":world_bounds(obj)
        })

    asset["images"]=list(seen_images.values())
    meaningful=[m for m in asset["materials"].values() if m["triangles"]>max(100,asset["triangle_count"]*.03)]
    asset["semantic_surface_separation_possible"]=len(meaningful)>=2
    asset["meaningful_material_count"]=len(meaningful)
    report["assets"].append(asset)

os.makedirs(os.path.dirname(out),exist_ok=True)
with open(out,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("VALORIA_FUNCTIONAL_SOURCE_AUDIT=PASS")
for a in report["assets"]:
    print(a["id"],"tris",a["triangle_count"],"mats",len(a["materials"]),"meaningful",a["meaningful_material_count"],
          "images",len(a["images"]),"separable",a["semantic_surface_separation_possible"])
