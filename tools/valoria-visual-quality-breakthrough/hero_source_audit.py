import bpy, json, os
from collections import defaultdict

src=os.environ.get("ELDORIA_HERO_GLB")
out=os.environ.get("ELDORIA_HERO_AUDIT")
if not src or not out:
    raise RuntimeError("ELDORIA_HERO_GLB / ELDORIA_HERO_AUDIT required")

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src, merge_vertices=False)

def world_vertex(obj,co):
    return obj.matrix_world @ co

def bounds(points):
    if not points:
        return None
    xs=[p.x for p in points]; ys=[p.y for p in points]; zs=[p.z for p in points]
    return {"min":[min(xs),min(ys),min(zs)],"max":[max(xs),max(ys),max(zs)],
            "size":[max(xs)-min(xs),max(ys)-min(ys),max(zs)-min(zs)],
            "center":[(min(xs)+max(xs))/2,(min(ys)+max(ys))/2,(min(zs)+max(zs))/2]}

report={"source":src,"objects":[],"summary":{}}
total_tris=0; mesh_count=0; all_points=[]

for obj in [o for o in bpy.context.scene.objects if o.type=="MESH"]:
    mesh_count+=1
    me=obj.data
    me.calc_loop_triangles()
    tri_count=len(me.loop_triangles)
    total_tris+=tri_count
    verts=[world_vertex(obj,v.co) for v in me.vertices]
    all_points.extend(verts)

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
        if rank[ra]==rank[rb]: rank[ra]+=1

    for p in me.polygons:
        vs=list(p.vertices)
        if len(vs)>1:
            for v in vs[1:]:
                union(vs[0],v)

    face_comp=defaultdict(lambda:{"faces":0,"tris":0,"verts":set(),"materials":set()})
    mat_face=defaultdict(lambda:{"faces":0,"tris":0,"verts":set()})
    for p in me.polygons:
        if not p.vertices: continue
        root=find(p.vertices[0])
        c=face_comp[root]
        c["faces"]+=1; c["tris"]+=max(1,len(p.vertices)-2)
        c["verts"].update(p.vertices); c["materials"].add(p.material_index)
        m=mat_face[p.material_index]
        m["faces"]+=1; m["tris"]+=max(1,len(p.vertices)-2); m["verts"].update(p.vertices)

    comps=[]
    for root,c in face_comp.items():
        pts=[verts[i] for i in c["verts"]]
        comps.append({"faces":c["faces"],"tris":c["tris"],"vertex_count":len(c["verts"]),
                      "material_indices":sorted(c["materials"]),"bounds":bounds(pts)})
    comps.sort(key=lambda x:x["tris"],reverse=True)

    mats=[]
    for idx,m in mat_face.items():
        pts=[verts[i] for i in m["verts"]]
        mat_name=me.materials[idx].name if idx < len(me.materials) and me.materials[idx] else None
        mats.append({"index":idx,"name":mat_name,"faces":m["faces"],"tris":m["tris"],
                     "vertex_count":len(m["verts"]),"bounds":bounds(pts)})
    mats.sort(key=lambda x:x["tris"],reverse=True)

    report["objects"].append({"name":obj.name,"triangle_count":tri_count,"vertex_count":len(me.vertices),
        "material_slots":[m.name if m else None for m in me.materials],"bounds":bounds(verts),
        "connected_component_count":len(comps),"connected_components":comps[:60],"materials":mats})

report["summary"]={"mesh_object_count":mesh_count,"triangle_count":total_tris,"bounds":bounds(all_points),"largest_components":[]}
for o in report["objects"]:
    for c in o["connected_components"]:
        report["summary"]["largest_components"].append({"object":o["name"],**c})
report["summary"]["largest_components"].sort(key=lambda x:x["tris"],reverse=True)
report["summary"]["largest_components"]=report["summary"]["largest_components"][:80]

os.makedirs(os.path.dirname(out),exist_ok=True)
with open(out,"w",encoding="utf-8") as f:
    json.dump(report,f,indent=2)
print("VALORIA_HERO_AUDIT=PASS")
print(json.dumps(report["summary"],indent=2))
