import bpy, json, os, math, hashlib

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-donor-audit-v1.json")
os.makedirs(os.path.dirname(OUT),exist_ok=True)

ASSETS=[
 ("Aserradero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb"),
 ("Cuartel","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb"),
 ("Granero","Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb"),
]

def sha256(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for chunk in iter(lambda:f.read(1024*1024),b""):h.update(chunk)
 return h.hexdigest()

def world_bounds(o):
 corners=[o.matrix_world @ __import__("mathutils").Vector(c) for c in o.bound_box]
 xs=[v.x for v in corners];ys=[v.y for v in corners];zs=[v.z for v in corners]
 return [min(xs),min(ys),min(zs),max(xs),max(ys),max(zs)]

def mat_info(m):
 d={"name":m.name,"shader":None,"textures":[]}
 if m.use_nodes and m.node_tree:
  for n in m.node_tree.nodes:
   if n.type=="BSDF_PRINCIPLED":d["shader"]="Principled BSDF"
   if n.type=="TEX_IMAGE" and getattr(n,"image",None):
    d["textures"].append({"node":n.name,"image":n.image.name,"filepath":n.image.filepath})
 return d

report={"schema_version":1,"program":"VALORIA PRODUCTION ART SYSTEM RESET v1","assets":[]}
for label,rel in ASSETS:
 path=os.path.join(ROOT,rel)
 if not os.path.isfile(path):raise SystemExit("missing donor "+path)
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=path)
 meshes=[]
 mats={}
 total_tris=0
 for o in bpy.context.scene.objects:
  if o.type!="MESH":continue
  o.data.calc_loop_triangles()
  tris=len(o.data.loop_triangles);total_tris+=tris
  mi=[]
  for m in o.data.materials:
   if not m:continue
   mi.append(m.name)
   if m.name not in mats:mats[m.name]=mat_info(m)
  meshes.append({
   "name":o.name,
   "parent":o.parent.name if o.parent else None,
   "vertices":len(o.data.vertices),
   "triangles":tris,
   "materials":mi,
   "location":[round(x,5) for x in o.location],
   "dimensions":[round(x,5) for x in o.dimensions],
   "bounds":[round(x,5) for x in world_bounds(o)]
  })
 meshes.sort(key=lambda x:x["triangles"],reverse=True)
 report["assets"].append({
  "label":label,"path":rel,"sha256":sha256(path),"mesh_count":len(meshes),
  "total_triangles":total_tris,"materials":list(mats.values()),"meshes":meshes
 })

with open(OUT,"w",encoding="utf8") as f:json.dump(report,f,indent=2)
print(json.dumps({"assets":[{"label":a["label"],"mesh_count":a["mesh_count"],"total_triangles":a["total_triangles"],"materials":len(a["materials"])} for a in report["assets"]]},indent=2))
