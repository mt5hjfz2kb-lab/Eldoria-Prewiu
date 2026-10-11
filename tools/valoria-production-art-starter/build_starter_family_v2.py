import bpy, os, json, hashlib
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","starter-family-v2")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","StarterFamilyV2")
os.makedirs(SRC,exist_ok=True);os.makedirs(OUT,exist_ok=True)

DONORS={
 "wall":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb",
 "corner":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb",
 "tower":"Unity/Assets/Eldoria/Resources/Valoria/Rescued/TowerWallRock.glb",
 "civic":"Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece01.glb",
 "workshop":"Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece03.glb",
}

def sha256(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for ch in iter(lambda:f.read(1024*1024),b""):h.update(ch)
 return h.hexdigest()

def import_one(rel,label):
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,rel))
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 if not objs: raise RuntimeError("No mesh imported "+rel)
 for o in objs:o.name=label+" · "+o.name
 return objs

def bounds(objs):
 pts=[]
 for o in objs:
  for c in o.bound_box:pts.append(o.matrix_world@Vector(c))
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def ground_center(objs):
 mn,mx=bounds(objs)
 ctr=Vector(((mn.x+mx.x)/2,(mn.y+mx.y)/2,mn.z))
 for o in objs:o.location-=ctr
 bpy.context.view_layer.update()

def uniform_scale(objs,footprint=1.0,height=1.0):
 mn,mx=bounds(objs);size=mx-mn
 s=min(footprint/max(size.x,size.y,1e-5),height/max(size.z,1e-5))
 for o in objs:o.scale*=s
 bpy.context.view_layer.update()
 ground_center(objs)

def duplicate_objs(objs,prefix):
 out=[]
 for o in objs:
  n=o.copy();n.data=o.data.copy();n.name=prefix+" · "+o.name
  for col in o.users_collection:col.objects.link(n)
  out.append(n)
 return out

def transform(objs,loc=(0,0,0),scale=1.0):
 for o in objs:
  o.location.x+=loc[0];o.location.y+=loc[1];o.location.z+=loc[2]
  o.scale*=scale
 bpy.context.view_layer.update()

def move_to_collection(objs,name):
 col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col)
 for o in objs:
  for c in list(o.users_collection):c.objects.unlink(o)
  col.objects.link(o)
 return col

def export_collection(col,name):
 bpy.ops.object.select_all(action="DESELECT")
 for o in col.objects:o.select_set(True)
 p=os.path.join(OUT,name+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def clear():
 bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
 for c in list(bpy.data.collections):
  if c.name!="Collection":bpy.data.collections.remove(c)

def build_single(asset_id,donor,footprint=1.0,height=1.0):
 clear();objs=import_one(DONORS[donor],asset_id);ground_center(objs);uniform_scale(objs,footprint,height)
 col=move_to_collection(objs,asset_id);p=export_collection(col,asset_id)
 return p,objs

def build_gate(asset_id):
 clear()
 towerL=import_one(DONORS["tower"],asset_id+" west tower");ground_center(towerL);uniform_scale(towerL,.33,.78);transform(towerL,(-.42,0,0))
 towerR=duplicate_objs(towerL,asset_id+" east tower")
 # mirror placement without negative scale
 dx=.84
 transform(towerR,(dx,0,0))
 wallL=import_one(DONORS["wall"],asset_id+" west curtain");ground_center(wallL);uniform_scale(wallL,.42,.42);transform(wallL,(-.20,-.02,0))
 wallR=duplicate_objs(wallL,asset_id+" east curtain");transform(wallR,(.40,0,0))
 corner=import_one(DONORS["corner"],asset_id+" crown");ground_center(corner);uniform_scale(corner,.36,.28);transform(corner,(0,.03,.36))
 objs=towerL+towerR+wallL+wallR+corner
 col=move_to_collection(objs,asset_id);p=export_collection(col,asset_id)
 return p,objs

def metrics(objs):
 tris=verts=0;mats=set()
 for o in objs:
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds(objs)
 return {"vertices":verts,"triangles":tris,"materials":len(mats),"bounds":[round(x,5) for x in (*mn,*mx)]}

bpy.ops.wm.read_factory_settings(use_empty=True)
builds=[
 ("Valoria_MainGate_v2","gate"),
 ("Valoria_WallSegment_v2","wall"),
 ("Valoria_Tower_v2","tower"),
 ("Valoria_CivicHouse_v2","civic"),
 ("Valoria_Workshop_v2","workshop"),
]
reports=[]
for aid,kind in builds:
 if kind=="gate":p,objs=build_gate(aid)
 else:
  fp,ht=(1.0,.75) if kind=="wall" else ((.72,1.0) if kind=="tower" else (.90,.80))
  p,objs=build_single(aid,kind,fp,ht)
 reports.append({"asset_id":aid,"classification":"TEMPORARY","authoring_method":"certified_donor_semantic_reauthoring","donor":kind,"export":{"path":os.path.relpath(p,ROOT).replace("\\","/"),"sha256":sha256(p)},"geometry":metrics(objs)})

# Save canonical source containing all final v2 outputs re-imported as named collections.
clear()
for r in reports:
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,r["export"]["path"]))
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 move_to_collection(objs,r["asset_id"])
blend=os.path.join(SRC,"Valoria_StarterFamily_v2.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)
blend_sha=sha256(blend)
for r in reports:
 manifest={
  "asset_id":r["asset_id"],"classification":"TEMPORARY","role":"PRIMARY" if "Gate" in r["asset_id"] or "Tower" in r["asset_id"] else "SECONDARY",
  "function":r["asset_id"].replace("Valoria_","").replace("_v2",""),
  "source":{"tool":"Blender","tool_version":"4.x","path":os.path.relpath(blend,ROOT).replace("\\","/"),"sha256":blend_sha,"reproducible":True,"authoring_method":"certified_donor_semantic_reauthoring"},
  "geometry":r["geometry"],
  "materials":[{"slot":"preserved-donor-pbr","family":"Eldoria_Stone","maps":["embedded basecolor","embedded normal","embedded roughness/metallic"]}],
  "export":{"glb_path":r["export"]["path"],"sha256":r["export"]["sha256"],"scale":1.0,"forward_axis":"+Z","up_axis":"+Y"},
  "unity":{"resource_path":"Valoria/ProductionArt/StarterFamilyV2/"+r["asset_id"],"owns_gameplay_collider":False,"owns_hotspot":False},
  "evidence":{"zoom9":"","mobile":"","technical_verdict":"PENDING_UNITY","visual_verdict":"PENDING_VISUAL","promotion_state":"CANDIDATE_NOT_PRODUCTION"}
 }
 with open(os.path.join(SRC,r["asset_id"]+".production-art.json"),"w") as f:json.dump(manifest,f,indent=2)
with open(os.path.join(SRC,"starter-family-v2-build-report.json"),"w") as f:json.dump({"blend_sha256":blend_sha,"assets":reports},f,indent=2)
print(json.dumps(reports,indent=2))
