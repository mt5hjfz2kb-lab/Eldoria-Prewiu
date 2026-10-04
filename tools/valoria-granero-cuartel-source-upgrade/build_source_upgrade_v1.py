import bpy, os, json, hashlib, math
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","granero-cuartel-source-upgrade-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","GraneroCuartelSourceUpgradeV1")
EVD=os.path.join(ROOT,"pipeline","evidence","valoria-granero-cuartel-source-upgrade-v1")
DOC=os.path.join(ROOT,"docs","evidence","valoria-granero-cuartel-source-upgrade-v1")
for p in (SRC,OUT,EVD,DOC): os.makedirs(p,exist_ok=True)

SOURCES={
 "granero":{"path":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb","asset":"Valoria_Granero_GCSUv1","prior_gate":{"run":36572780639,"artifact":11036129405,"verdict":"TECH PASS / VISUAL PASS"}},
 "cuartel":{"path":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb","asset":"Valoria_Cuartel_GCSUv1","prior_gate":{"run":36529204672,"artifact":11015563918,"verdict":"dedicated source accepted technically; semantic military read requires stronger composition"}},
 "cuartel_wall":{"path":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/FirstProductionDistrictV1/Valoria_DefenseWall_FPDv1.glb"},
 "cuartel_tower":{"path":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/FirstProductionDistrictV1/Valoria_DefenseTower_FPDv1.glb"}
}

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for b in iter(lambda:f.read(1048576),b""): h.update(b)
 return h.hexdigest()

def reset(): bpy.ops.wm.read_factory_settings(use_empty=True)

def import_source(rel,prefix):
 p=os.path.join(ROOT,rel)
 if not os.path.isfile(p): raise RuntimeError("Missing canonical dedicated source: "+rel)
 bpy.ops.import_scene.gltf(filepath=p)
 objs=[o for o in bpy.context.scene.objects if o.type=="MESH"]
 if not objs: raise RuntimeError("No mesh in "+rel)
 for i,o in enumerate(objs):
  o.name=f"{prefix} · authored {i:02d}"
  for poly in o.data.polygons: poly.use_smooth=True
 return objs

def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def normalize(objs,target_width=4.4,target_height=4.1):
 mn,mx=bounds(objs); size=mx-mn
 scale=min(target_width/max(size.x,size.y,.001),target_height/max(size.z,.001))
 for o in objs:o.scale*=scale
 bpy.context.view_layer.update()
 mn,mx=bounds(objs); c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=c
 bpy.context.view_layer.update()

def geometry(objs):
 tris=verts=0;mats=set()
 for o in objs:
  o.data.calc_loop_triangles(); tris+=len(o.data.loop_triangles); verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds(objs)
 return {"objects":len(objs),"vertices":verts,"triangles":tris,"materials":sorted(mats),"bounds":[round(v,4) for v in (*mn,*mx)]}

def export_glb(name,objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,name+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def look_at(obj,pt):
 direction=Vector(pt)-obj.location
 obj.rotation_euler=direction.to_track_quat("-Z","Y").to_euler()

def render_preview(kind,objs):
 mn,mx=bounds(objs); size=mx-mn; center=(mn+mx)*.5
 scene=bpy.context.scene
 scene.render.engine="BLENDER_EEVEE"
 scene.render.resolution_x=900; scene.render.resolution_y=900; scene.render.resolution_percentage=100
 scene.render.image_settings.file_format="PNG"; scene.render.film_transparent=False
 scene.world.color=(0.055,0.06,0.065)
 # neutral ground, deliberately not semantic dressing
 bpy.ops.mesh.primitive_plane_add(size=max(size.x,size.y)*3.2,location=(0,0,mn.z-.02))
 floor=bpy.context.object; floor.name="Neutral preview floor"
 mat=bpy.data.materials.new("Neutral preview floor"); mat.diffuse_color=(0.12,0.125,0.13,1); mat.roughness=.92
 floor.data.materials.append(mat)
 # simple key/fill
 bpy.ops.object.light_add(type="AREA",location=(5.5,-6.5,8.0)); key=bpy.context.object; key.data.energy=950; key.data.size=5.5; look_at(key,center)
 bpy.ops.object.light_add(type="AREA",location=(-4.0,1.5,5.0)); fill=bpy.context.object; fill.data.energy=450; fill.data.size=4.5; look_at(fill,center)
 bpy.ops.object.light_add(type="SUN",location=(0,0,8)); sun=bpy.context.object; sun.rotation_euler=(math.radians(28),math.radians(-18),math.radians(-32)); sun.data.energy=1.3
 bpy.ops.object.camera_add(location=(7.2,-8.4,6.8)); cam=bpy.context.object; cam.data.lens=58; cam.data.sensor_width=36; look_at(cam,(center.x,center.y,center.z+size.z*.05)); scene.camera=cam
 scene.render.filepath=os.path.join(DOC,kind+"-source-3q.png")
 scene.render.image_settings.color_mode="RGB"; bpy.ops.render.render(write_still=True)

def fit_component(objs,width,height,loc=(0,0,0),yaw=0,scale=(1,1,1)):
 mn,mx=bounds(objs); sz=mx-mn; k=min(width/max(sz.x,sz.y,.001),height/max(sz.z,.001))
 for o in objs:o.scale*=k
 bpy.context.view_layer.update(); mn,mx=bounds(objs); c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:
  o.location-=c
  o.rotation_euler[2]+=math.radians(yaw)
  o.scale.x*=scale[0];o.scale.y*=scale[1];o.scale.z*=scale[2]
  o.location+=Vector(loc)
 bpy.context.view_layer.update()
 return objs

def build_granero():
 reset()
 return fit_component(import_source(SOURCES["granero"]["path"],"Granero"),4.45,4.15)

def build_cuartel():
 reset()
 parts=[]
 core=import_source(SOURCES["cuartel"]["path"],"Cuartel core")
 parts+=fit_component(core,3.65,3.55,(0,.65,.05),0,(1,.96,1))
 # Martial forecourt: accepted rich defense geometry frames the barracks without turning it into a miniature castle.
 wall1=import_source(SOURCES["cuartel_wall"]["path"],"Cuartel west training-wall")
 parts+=fit_component(wall1,2.45,1.45,(-2.05,-.55,.02),8,(1,.66,.78))
 wall2=import_source(SOURCES["cuartel_wall"]["path"],"Cuartel east training-wall")
 parts+=fit_component(wall2,2.05,1.25,(2.05,-.40,.02),-7,(1,.62,.68))
 tower=import_source(SOURCES["cuartel_tower"]["path"],"Cuartel guard corner")
 parts+=fit_component(tower,1.55,2.45,(-2.25,.95,.02),4,(.78,.78,.88))
 # Recenter the complete compound for deterministic export and isolated preview.
 mn,mx=bounds(parts); c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in parts:o.location-=c
 bpy.context.view_layer.update()
 return parts

records=[]
for kind,fn in [("granero",build_granero),("cuartel",build_cuartel)]:
 cfg=SOURCES[kind]; objs=fn(); glb=export_glb(cfg["asset"],objs)
 donors=[cfg["path"]]
 if kind=="cuartel": donors += [SOURCES["cuartel_wall"]["path"],SOURCES["cuartel_tower"]["path"]]
 rec={"role":kind,"asset_id":cfg["asset"],"sources":donors,"source_sha256":[sha(os.path.join(ROOT,p)) for p in donors],"glb":os.path.relpath(glb,ROOT).replace("\\","/"),"glb_sha256":sha(glb),"geometry":geometry(objs),"prior_certification":cfg["prior_gate"],"method":"Granero: dedicated functional source normalization. Cuartel: dedicated barracks core + bounded accepted rich defensive modules forming guarded training forecourt; no MidTier donor recomposition; no primitive architecture."}
 render_preview(kind,objs)
 rec["preview"]=os.path.relpath(os.path.join(DOC,kind+"-source-3q.png"),ROOT).replace("\\","/")
 records.append(rec)

reset()
for i,rec in enumerate(records):
 bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,rec["glb"]))
 objs=[o for o in bpy.context.scene.objects if o.type=="MESH"]
 for o in objs:o.location.x+=(i*6.0-3.0)
blend=os.path.join(SRC,"Valoria_Granero_Cuartel_SourceUpgrade_v1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)
report={"schema_version":1,"program":"VALORIA GRANERO + CUARTEL SOURCE UPGRADE v1","phase":"SOURCE_ISOLATED","source_strategy":"Granero uses certified dedicated granary source; Cuartel uses dedicated barracks core plus bounded accepted rich defensive geometry for guarded military compound identity","tripo":False,"paid_credits":0,"blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),"assets":records,"source_visual_verdict":{"granero":"PENDING_VISUAL_REVIEW","cuartel":"PENDING_VISUAL_REVIEW"},"integration_allowed":False}
for p in [os.path.join(SRC,"source-upgrade-report.json"),os.path.join(EVD,"source-report.json")]:
 with open(p,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
