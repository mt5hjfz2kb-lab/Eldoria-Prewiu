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
 "cuartel":{"path":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb","asset":"Valoria_Cuartel_GCSUv1","prior_gate":{"run":36529204672,"artifact":11015563918,"verdict":"production dedicated source / PBR preserved"}}
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

records=[]
for kind,cfg in SOURCES.items():
 reset()
 objs=import_source(cfg["path"],kind.capitalize())
 normalize(objs,4.45 if kind=="granero" else 4.55,4.15 if kind=="granero" else 4.25)
 glb=export_glb(cfg["asset"],objs)
 rec={"role":kind,"asset_id":cfg["asset"],"source":cfg["path"],"source_sha256":sha(os.path.join(ROOT,cfg["path"])),"glb":os.path.relpath(glb,ROOT).replace("\\","/"),"glb_sha256":sha(glb),"geometry":geometry(objs),"prior_certification":cfg["prior_gate"],"method":"dedicated functional source selection + normalization/cleanup; no MidTier donor recomposition; no primitive architecture"}
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
report={"schema_version":1,"program":"VALORIA GRANERO + CUARTEL SOURCE UPGRADE v1","phase":"SOURCE_ISOLATED","source_strategy":"replace weak MidTier recomposition with already-certified dedicated functional 3D sources","tripo":False,"paid_credits":0,"blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),"assets":records,"source_visual_verdict":{"granero":"PENDING_VISUAL_REVIEW","cuartel":"PENDING_VISUAL_REVIEW"},"integration_allowed":False}
for p in [os.path.join(SRC,"source-upgrade-report.json"),os.path.join(EVD,"source-report.json")]:
 with open(p,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
