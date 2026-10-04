import bpy, os, json, hashlib, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","coherent-family-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","CoherentFamilyV1")
PRE=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-coherent-family-v1")
for p in (SRC,OUT,PRE):os.makedirs(p,exist_ok=True)

P={
 "gate":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Fence_WallGate_01a.fbx",
 "wall":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Fence_Wall_01b.fbx",
 "tower":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Tover_01a.fbx",
 "civic":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Town_Building_Administrative _01a.fbx",
 "workshop":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_OutBuilding_Shed_03b.fbx"
}

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1048576),b""):h.update(c)
 return h.hexdigest()

def fresh():bpy.ops.wm.read_factory_settings(use_empty=True)

def import_lod0(rel,prefix):
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.fbx(filepath=os.path.join(ROOT,rel),automatic_bone_orientation=False)
 imported=[o for o in bpy.context.scene.objects if o not in before]
 meshes=[o for o in imported if o.type=="MESH" and ("LOD0" in o.name or not any("LOD" in q.name for q in imported if q.type=="MESH"))]
 for o in list(imported):
  if o.type=="MESH" and o not in meshes:bpy.data.objects.remove(o,do_unlink=True)
 if not meshes:raise RuntimeError("No LOD0 "+rel)
 for o in meshes:o.name=prefix+" · "+o.name
 return meshes

def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 return Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts))),Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))

def normalize(objs,span,height):
 mn,mx=bounds(objs);s=mx-mn
 f=min(span/max(s.x,s.y,.001),height/max(s.z,.001))
 for o in objs:o.scale*=f
 bpy.context.view_layer.update()
 mn,mx=bounds(objs);center=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=center
 bpy.context.view_layer.update()

def transform(objs,loc=(0,0,0),yaw=0,scale=1.0):
 q=math.radians(yaw)
 for o in objs:
  o.location=Vector(loc)+Vector((o.location.x*math.cos(q)-o.location.y*math.sin(q),o.location.x*math.sin(q)+o.location.y*math.cos(q),o.location.z))
  o.rotation_euler[2]+=q
  o.scale*=scale
 bpy.context.view_layer.update()

def duplicate(objs,prefix):
 out=[]
 for o in objs:
  n=o.copy();n.data=o.data.copy();n.name=prefix+" · "+o.name
  bpy.context.scene.collection.objects.link(n);out.append(n)
 return out

def coll(objs,name):
 c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c)
 for o in objs:
  for old in list(o.users_collection):old.objects.unlink(o)
  c.objects.link(o)
 return c

def export(objs,name):
 c=coll(objs,name);bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,name+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p,c

def metrics(objs):
 tris=verts=0;mats=set()
 for o in objs:
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds(objs)
 return {"objects":len(objs),"vertices":verts,"triangles":tris,"materials":sorted(mats),"bounds":[round(x,4) for x in (*mn,*mx)]}

def preview(objs,name):
 sc=bpy.context.scene
 sc.render.engine="BLENDER_WORKBENCH";sc.display.shading.light="STUDIO";sc.display.shading.show_cavity=True;sc.display.shading.cavity_type="WORLD";sc.display.shading.color_type="MATERIAL";sc.display.shading.background_type="WORLD";sc.display.shading.background_color=(.055,.065,.08)
 mn,mx=bounds(objs);ctr=(mn+mx)*.5;span=max(*(mx-mn),.5)
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=span*1.45;cam.location=ctr+Vector((span*1.35,-span*1.7,span*1.15));cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100;sc.render.image_settings.file_format="PNG";sc.render.filepath=os.path.join(PRE,name+".png");bpy.ops.render.render(write_still=True)
 bpy.data.objects.remove(cam,do_unlink=True)

def single(aid,key,span,height):
 fresh();objs=import_lod0(P[key],aid);normalize(objs,span,height);p,c=export(objs,aid);preview(objs,aid);return p,objs

def main_gate():
 fresh()
 tL=import_lod0(P["tower"],"Gate west tower");normalize(tL,2.25,4.25);transform(tL,(-2.65,.35,0),yaw=2)
 tR=duplicate(tL,"Gate east tower")
 # reflect placement, not mesh scale
 for o in tR:o.location.x*=-1;o.rotation_euler[2]*=-1
 g=import_lod0(P["gate"],"Gate center portal");normalize(g,2.65,2.45);transform(g,(0,-.42,.10))
 wL=import_lod0(P["wall"],"Gate west wall");normalize(wL,2.20,2.05);transform(wL,(-4.60,.15,.02),yaw=3)
 wR=duplicate(wL,"Gate east wall")
 for o in wR:o.location.x*=-1;o.rotation_euler[2]*=-1
 objs=tL+tR+g+wL+wR
 p,c=export(objs,"Valoria_MainGate_CohV1");preview(objs,"Valoria_MainGate_CohV1");return p,objs

specs=[
 ("Valoria_MainGate_CohV1","gate-composite"),
 ("Valoria_WallSegment_CohV1","wall"),
 ("Valoria_Tower_CohV1","tower"),
 ("Valoria_CivicHouse_CohV1","civic"),
 ("Valoria_Workshop_CohV1","workshop")
]
reports=[]
for aid,key in specs:
 if key=="gate-composite":p,objs=main_gate()
 elif key=="wall":p,objs=single(aid,key,4.4,2.25)
 elif key=="tower":p,objs=single(aid,key,2.65,4.35)
 elif key=="civic":p,objs=single(aid,key,3.7,3.6)
 else:p,objs=single(aid,key,3.25,2.9)
 reports.append({"asset_id":aid,"classification":"TEMPORARY","authoring_method":"single_family_lod0_reauthoring","source_family":"EmaceArt Slavic World Free","geometry":metrics(objs),"glb_path":os.path.relpath(p,ROOT).replace("\\","/"),"glb_sha256":sha(p)})

# canonical source: reimport outputs
fresh()
for r in reports:
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,r["glb_path"]))
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"];coll(objs,r["asset_id"])
blend=os.path.join(SRC,"Valoria_CoherentFamily_v1.blend");bpy.ops.wm.save_as_mainfile(filepath=blend);bsha=sha(blend)
for r in reports:
 man={"asset_id":r["asset_id"],"classification":"TEMPORARY","source":{"tool":"Blender","tool_version":"4.0.2","path":os.path.relpath(blend,ROOT).replace("\\","/"),"sha256":bsha,"reproducible":True,"authoring_method":"single_family_lod0_reauthoring","source_family":"EmaceArt Slavic World Free"},"geometry":{"triangles":r["geometry"]["triangles"],"materials":len(r["geometry"]["materials"]),"uv0":True,"normals":True},"materials":[{"slot":"source-colorsheet","family":"Eldoria_Stone","maps":["Slavic colorsheet; phase D remap pending"]}],"export":{"glb_path":r["glb_path"],"sha256":r["glb_sha256"],"scale":1.0,"forward_axis":"+Z","up_axis":"+Y"},"unity":{"resource_path":"Valoria/ProductionArt/CoherentFamilyV1/"+r["asset_id"],"owns_gameplay_collider":False,"owns_hotspot":False},"evidence":{"isolated_preview":"pipeline/evidence/valoria-production-art-coherent-family-v1/"+r["asset_id"]+".png","zoom9":"","mobile":"","technical_verdict":"PENDING_UNITY","visual_verdict":"PENDING_VISUAL","promotion_state":"CANDIDATE_NOT_PRODUCTION"}}
 with open(os.path.join(SRC,r["asset_id"]+".production-art.json"),"w") as f:json.dump(man,f,indent=2)
with open(os.path.join(SRC,"coherent-family-v1-build-report.json"),"w") as f:json.dump({"blend_sha256":bsha,"assets":reports},f,indent=2)
print(json.dumps(reports,indent=2))
