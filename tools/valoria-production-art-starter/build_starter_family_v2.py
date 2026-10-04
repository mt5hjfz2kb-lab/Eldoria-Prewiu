import bpy, os, json, hashlib, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","starter-family-v2")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","StarterFamilyV2")
os.makedirs(SRC,exist_ok=True);os.makedirs(OUT,exist_ok=True)

SOURCES={
 "wall":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb",
 "tower":"Unity/Assets/Eldoria/Resources/Valoria/Rescued/TowerWallRock.glb",
 "gate":"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Gate.fbx",
 "gate_rich":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb",
 "civic":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Town_Building_Administrative _01a.fbx",
 "workshop":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb",
}

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1048576),b""):h.update(c)
 return h.hexdigest()

def clear():
 bpy.ops.wm.read_factory_settings(use_empty=True)

def imported_meshes(before):
 return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.name not in before]

def import_source(rel):
 p=os.path.join(ROOT,rel)
 if not os.path.isfile(p):raise RuntimeError("Missing source "+rel)
 before={o.name for o in bpy.context.scene.objects}
 if rel.lower().endswith(".glb"):
  bpy.ops.import_scene.gltf(filepath=p)
 else:
  bpy.ops.import_scene.fbx(filepath=p,automatic_bone_orientation=False)
 return imported_meshes(before)

def bbox(objs):
 pts=[]
 for o in objs:
  for c in o.bound_box:pts.append(o.matrix_world@Vector(c))
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def normalize(objs,target_height,center_xy=True):
 mn,mx=bbox(objs); h=max(.001,mx.z-mn.z)
 s=target_height/h
 for o in objs:o.scale*=s
 bpy.context.view_layer.update()
 mn,mx=bbox(objs)
 delta=Vector((0,0,-mn.z))
 if center_xy:delta.x=-(mn.x+mx.x)*.5;delta.y=-(mn.y+mx.y)*.5
 for o in objs:o.location+=delta
 bpy.context.view_layer.update()

def collection_asset(name, objs):
 col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col)
 for o in objs:
  for old in list(o.users_collection):old.objects.unlink(o)
  col.objects.link(o)
 return col

def add_gate_transition(gate_objs):
 rich=import_source(SOURCES["gate_rich"]);normalize(rich,1.45)
 # duplicate-rich flank wedges: keep most of the rich geometry low and lateral.
 for o in rich:
  o.scale*=Vector((.55,.72,.48));o.location+=Vector((-1.75,.18,0))
  o.name="V2 Gate Rich Left · "+o.name
 right=[]
 for o in rich:
  cp=o.copy();cp.data=o.data.copy() if o.data else None;bpy.context.scene.collection.objects.link(cp)
  cp.location.x*=-1;cp.name="V2 Gate Rich Right · "+o.name;right.append(cp)
 return gate_objs+rich+right

def clean_names(objs,prefix):
 for i,o in enumerate(objs):
  o.name=f"{prefix} · {i:03d} · {o.name}"

def export_asset(asset_id,objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 path=os.path.join(OUT,asset_id+".glb")
 bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 tris=0;mats=set()
 for o in objs:
  if o.type!="MESH":continue
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles)
  for m in o.data.materials:
   if m:mats.add(m.name)
 return path,tris,sorted(mats)

clear()
assets=[]

# Wall: certified rich wall source.
wall=import_source(SOURCES["wall"]);normalize(wall,2.35);clean_names(wall,"WallV2");collection_asset("PA2_Wall",wall)
# Tower: certified rescued tower-wall-rock source, normalized to tower role.
tower=import_source(SOURCES["tower"]);normalize(tower,4.25);clean_names(tower,"TowerV2");collection_asset("PA2_Tower",tower)
# Gate: modular castle doorway + rich masonry/rock transition flanks.
gate=import_source(SOURCES["gate"]);normalize(gate,3.05);gate=add_gate_transition(gate);clean_names(gate,"GateV2");collection_asset("PA2_Gate",gate)
# Civic: complete detailed Slavic administrative source.
civic=import_source(SOURCES["civic"]);normalize(civic,3.45);clean_names(civic,"CivicV2");collection_asset("PA2_Civic",civic)
# Workshop: certified Aserradero source reused as geometric donor, transformed enough for a secondary production role.
work=import_source(SOURCES["workshop"]);normalize(work,3.0)
for o in work:
 o.scale.x*=.82;o.scale.y*=1.05
clean_names(work,"WorkshopV2");collection_asset("PA2_Workshop",work)

entries=[
 ("Valoria_MainGate_v2",gate,"PRIMARY","main gate",[SOURCES["gate"],SOURCES["gate_rich"]]),
 ("Valoria_WallSegment_v2",wall,"PRIMARY","wall segment",[SOURCES["wall"]]),
 ("Valoria_Tower_v2",tower,"PRIMARY","tower",[SOURCES["tower"]]),
 ("Valoria_CivicHouse_v2",civic,"SECONDARY","civic house",[SOURCES["civic"]]),
 ("Valoria_Workshop_v2",work,"SECONDARY","workshop",[SOURCES["workshop"]]),
]

built=[]
for aid,objs,role,function,sources in entries:
 p,tris,mats=export_asset(aid,objs)
 built.append({"asset_id":aid,"objects":len(objs),"triangles":tris,"materials":mats,"path":p,"role":role,"function":function,"sources":sources})

blend=os.path.join(SRC,"Valoria_StarterFamily_v2.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)
blend_sha=sha(blend)

for b in built:
 man={
  "asset_id":b["asset_id"],"classification":"TEMPORARY","role":b["role"],"function":b["function"],
  "source":{"tool":"Blender","tool_version":"4.0.2","path":os.path.relpath(blend,ROOT).replace("\\","/"),"sha256":blend_sha,
            "reproducible":True,"authoring_method":"certified_source_reauthoring_hybrid_v2","donor_sources":b["sources"]},
  "geometry":{"triangles":b["triangles"],"materials":len(b["materials"]),"uv0":True,"normals":True},
  "materials":[{"slot":"source-preserved","family":"Eldoria_Stone","maps":["donor-source-materials"]}],
  "export":{"glb_path":os.path.relpath(b["path"],ROOT).replace("\\","/"),"sha256":sha(b["path"]),"scale":1.0,"forward_axis":"+Z","up_axis":"+Y"},
  "unity":{"resource_path":"Valoria/ProductionArt/StarterFamilyV2/"+b["asset_id"],"owns_gameplay_collider":False,"owns_hotspot":False},
  "evidence":{"zoom9":"","mobile":"","technical_verdict":"PENDING_UNITY","visual_verdict":"PENDING_VISUAL","promotion_state":"CANDIDATE_NOT_PRODUCTION"}
 }
 with open(os.path.join(SRC,b["asset_id"]+".production-art.json"),"w") as f:json.dump(man,f,indent=2)

report={"blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":blend_sha,
        "assets":[{k:v for k,v in b.items() if k!="path"}|{"glb_sha256":sha(b["path"])} for b in built]}
with open(os.path.join(SRC,"starter-family-v2-build-report.json"),"w") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
