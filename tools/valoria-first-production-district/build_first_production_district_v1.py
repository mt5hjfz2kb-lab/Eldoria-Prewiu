import bpy, os, json, hashlib, math
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","first-production-district-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","FirstProductionDistrictV1")
EVD=os.path.join(ROOT,"pipeline","evidence","valoria-first-production-district-v1")
for p in (SRC,OUT,EVD): os.makedirs(p,exist_ok=True)
D={"wall":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb","corner":"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb","tower":"Unity/Assets/Eldoria/Resources/Valoria/Rescued/TowerWallRock.glb","civil":"Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece01.glb","workshop":"Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece03.glb"}
def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for b in iter(lambda:f.read(1048576),b""): h.update(b)
 return h.hexdigest()
def reset(): bpy.ops.wm.read_factory_settings(use_empty=True)
def imp(key,prefix):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,D[key]))
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 if not objs: raise RuntimeError("No mesh "+key)
 for i,o in enumerate(objs): o.name=f"{prefix} · {i:02d}"
 return objs
def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)));mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx
def fit(objs,x,z):
 mn,mx=bounds(objs);s=mx-mn;k=min(x/max(s.x,s.y,.001),z/max(s.z,.001))
 for o in objs:o.scale*=k
 bpy.context.view_layer.update();mn,mx=bounds(objs);c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=c
 bpy.context.view_layer.update()
def place(objs,loc=(0,0,0),rot=0,scale=(1,1,1)):
 for o in objs:
  o.location+=Vector(loc);o.rotation_euler[2]+=math.radians(rot);o.scale.x*=scale[0];o.scale.y*=scale[1];o.scale.z*=scale[2]
 bpy.context.view_layer.update();return objs
def dup(objs,prefix,loc=(0,0,0),rot=0,scale=(1,1,1)):
 out=[]
 for i,o in enumerate(objs):
  n=o.copy();n.data=o.data.copy();bpy.context.scene.collection.objects.link(n);n.name=f"{prefix} · {i:02d}";out.append(n)
 return place(out,loc,rot,scale)
def exp(aid,objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,aid+".glb");bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT");return p
def metrics(objs):
 tris=verts=0;m=set()
 for o in objs:
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for x in o.data.materials:
   if x:m.add(x.name)
 mn,mx=bounds(objs);return {"objects":len(objs),"vertices":verts,"triangles":tris,"materials":sorted(m),"bounds":[round(v,4) for v in (*mn,*mx)]}
def build_wall():
 reset();a=imp("wall","Defense wall");fit(a,4.7,2.35);return a
def build_tower():
 reset();c=imp("corner","Tower masonry");fit(c,2.65,2.65);parts=place(c,(0,0,0),0,(1,1,1.05));t=imp("tower","Tower crown source");fit(t,1.85,2.0);parts+=place(t,(0,0,1.65),-6,(.82,.82,.72));return parts
def build_gate():
 reset();c=imp("corner","Gate west pylon");fit(c,2.15,3.15);parts=place(c,(-1.65,0,0),0,(.82,.82,1));parts+=dup(c,"Gate east pylon",(3.30,0,0),180,(1,1,1));w=imp("wall","Gate lintel");fit(w,3.35,1.15);parts+=place(w,(0,0,2.45),0,(1,.82,.58));t=imp("tower","Gate crown");fit(t,2.25,1.55);parts+=place(t,(0,.04,3.0),0,(.82,.78,.58));return parts
def build_granero():
 reset();body=imp("civil","Granero main hall");fit(body,4.25,3.35);parts=place(body,(0,0,0),4,(1.0,.92,1.0));annex=imp("workshop","Granero loading porch");fit(annex,2.35,2.15);parts+=place(annex,(0,-1.45,.05),-6,(1.0,.72,.78));loft=imp("civil","Granero loft crown");fit(loft,2.25,1.85);parts+=place(loft,(.55,.15,2.10),4,(.72,.72,.58));return parts
def build_cuartel():
 reset();body=imp("workshop","Cuartel main block");fit(body,4.45,3.55);parts=place(body,(0,0,0),-3,(1,.94,1));base=imp("wall","Cuartel stone plinth");fit(base,4.55,1.25);parts+=place(base,(0,.10,-.02),0,(1,.82,.48));flank=imp("corner","Cuartel guarded corner");fit(flank,1.75,2.65);parts+=place(flank,(-1.75,-.20,.05),0,(.72,.72,.92));parts+=dup(flank,"Cuartel opposite corner",(3.50,0,0),180,(1,1,1));return parts
built=[]
for aid,fn in [("Valoria_DefenseWall_FPDv1",build_wall),("Valoria_DefenseTower_FPDv1",build_tower),("Valoria_MainGate_FPDv1",build_gate),("Valoria_Granero_FPDv1",build_granero),("Valoria_Cuartel_FPDv1",build_cuartel)]:
 objs=fn();p=exp(aid,objs);built.append({"asset_id":aid,"glb":os.path.relpath(p,ROOT).replace("\\","/"),"sha256":sha(p),"geometry":metrics(objs)})
reset()
for i,b in enumerate(built):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,b["glb"]));objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 for o in objs:o.location.x+=(i-2)*7
blend=os.path.join(SRC,"Valoria_FirstProductionDistrict_v1.blend");bpy.ops.wm.save_as_mainfile(filepath=blend)
report={"schema_version":1,"classification":"PRODUCTION_CANDIDATE","authoring_method":"bounded rich-source Blender reauthoring","paid_credits":0,"tripo":False,"donors":D,"blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),"assets":built}
with open(os.path.join(SRC,"first-production-district-v1-build-report.json"),"w") as f:json.dump(report,f,indent=2)
with open(os.path.join(EVD,"source-report.json"),"w") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
