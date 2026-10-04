import bpy, os, json, hashlib
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-slavic-audit-v1.json")
os.makedirs(os.path.dirname(OUT),exist_ok=True)
ASSETS=[
 ("Gate","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Fence_WallGate_01a.fbx"),
 ("WallA","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Fence_Wall_01b.fbx"),
 ("WallB","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Fence_Wall_01d.fbx"),
 ("Tower","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Tover_01a.fbx"),
 ("Civic","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Town_Building_Administrative _01a.fbx"),
 ("Workshop","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_OutBuilding_Shed_03b.fbx"),
 ("Porch","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_HouseModule_Porch_01d.fbx"),
 ("Roof","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Hut_Roof_01c.fbx"),
 ("Foundation","Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Hut_Foundation_02b.fbx")
]
def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1048576),b""):h.update(c)
 return h.hexdigest()
report={"schema_version":1,"program":"VALORIA PRODUCTION ART SYSTEM RESET v1","family":"EmaceArt Slavic World Free","assets":[]}
for label,rel in ASSETS:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 p=os.path.join(ROOT,rel)
 if not os.path.isfile(p):raise RuntimeError("Missing "+p)
 bpy.ops.import_scene.fbx(filepath=p,automatic_bone_orientation=False)
 rows=[];tris=verts=0;mats=set()
 for o in bpy.context.scene.objects:
  if o.type!="MESH":continue
  o.data.calc_loop_triangles();t=len(o.data.loop_triangles);v=len(o.data.vertices)
  tris+=t;verts+=v
  names=[m.name for m in o.data.materials if m]
  mats.update(names)
  rows.append({"name":o.name,"vertices":v,"triangles":t,"materials":names,"dimensions":[round(x,4) for x in o.dimensions]})
 report["assets"].append({"label":label,"path":rel,"sha256":sha(p),"objects":len(rows),"vertices":verts,"triangles":tris,"materials":sorted(mats),"meshes":sorted(rows,key=lambda x:x["triangles"],reverse=True)})
with open(OUT,"w") as f:json.dump(report,f,indent=2)
print(json.dumps([{"label":a["label"],"objects":a["objects"],"triangles":a["triangles"],"materials":a["materials"],"names":[m["name"] for m in a["meshes"][:12]]} for a in report["assets"]],indent=2))
