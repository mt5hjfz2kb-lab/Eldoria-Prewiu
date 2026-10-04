import bpy, os, json, hashlib, math
from mathutils import Vector, Matrix

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","granero-rich-camera-first-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","GraneroRichCameraFirstV1")
EVD=os.path.join(ROOT,"pipeline","evidence","valoria-granero-rich-camera-first-v1")
DOC=os.path.join(ROOT,"docs","evidence","valoria-granero-rich-camera-first-v1")
for p in (SRC,OUT,EVD,DOC): os.makedirs(p,exist_ok=True)
INPUT="Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/Valoria_Granero_GCSUv1.glb"

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for b in iter(lambda:f.read(1048576),b""):h.update(b)
 return h.hexdigest()

def reset():bpy.ops.wm.read_factory_settings(use_empty=True)

def import_glb(prefix):
 p=os.path.join(ROOT,INPUT)
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.gltf(filepath=p)
 imported=[o for o in bpy.context.scene.objects if o not in before]
 meshes=[o for o in imported if o.type=="MESH"]
 if not meshes:raise RuntimeError("No mesh in granero input")
 for i,o in enumerate(meshes):
  mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw;o.name=f"{prefix} · rich {i:02d}"
 for o in imported:
  if o.type!="MESH" and o.name in bpy.data.objects and not o.children:bpy.data.objects.remove(o,do_unlink=True)
 return meshes

def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def fit(objs,width,height,loc=(0,0,0),yaw=0,stretch=(1,1,1)):
 mn,mx=bounds(objs);sz=mx-mn
 k=min(width/max(sz.x,sz.y,.001),height/max(sz.z,.001))
 c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 root=bpy.data.objects.new("rigid-rich-component",None);bpy.context.scene.collection.objects.link(root)
 for o in objs:
  mw=o.matrix_world.copy();o.parent=root;o.matrix_world=mw
 root.matrix_world=(Matrix.Translation(Vector(loc))@Matrix.Rotation(math.radians(yaw),4,'Z')@
  Matrix.Diagonal(Vector((k*stretch[0],k*stretch[1],k*stretch[2],1)))@Matrix.Translation(-c))
 bpy.context.view_layer.update()
 for o in objs:
  mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw
 bpy.data.objects.remove(root,do_unlink=True);bpy.context.view_layer.update()
 return objs

def recenter(objs):
 mn,mx=bounds(objs);c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=c
 bpy.context.view_layer.update()

def build():
 reset();parts=[]
 # Main storage hall owns the center and stays largest.
 parts+=fit(import_glb("Granero main hall"),4.75,3.35,(0,.72,.02),0,(1.0,.84,1.0))
 # Two rich granary annexes create a camera-facing U storage court; all visible architecture is the same rich source family.
 parts+=fit(import_glb("Granero west storage annex"),3.00,2.68,(-2.15,-.92,.02),13,(.86,.72,.92))
 parts+=fit(import_glb("Granero east storage annex"),2.78,2.52,(2.08,-.76,.02),-12,(.82,.68,.88))
 recenter(parts);return parts

def geometry(objs):
 tris=verts=0;mats=set()
 for o in objs:
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds(objs)
 return {"objects":len(objs),"vertices":verts,"triangles":tris,"materials":sorted(mats),"bounds":[round(v,4) for v in (*mn,*mx)]}

def export(objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,"Valoria_Granero_RCFv1.glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def look_at(obj,pt):
 obj.rotation_euler=(Vector(pt)-obj.location).to_track_quat("-Z","Y").to_euler()

def preview(objs,sil=False):
 mn,mx=bounds(objs);c=(mn+mx)*.5;size=mx-mn
 s=bpy.context.scene;s.render.engine="BLENDER_EEVEE";s.render.resolution_x=900;s.render.resolution_y=900;s.render.resolution_percentage=100
 s.render.image_settings.file_format="PNG";s.render.film_transparent=False
 if s.world is None:s.world=bpy.data.worlds.new("preview world")
 s.world.color=(.86,.86,.86) if sil else (.06,.065,.07)
 if not sil:
  bpy.ops.mesh.primitive_plane_add(size=max(size.x,size.y)*3.2,location=(0,0,mn.z-.02))
  floor=bpy.context.object
  m=bpy.data.materials.new("neutral preview floor");m.diffuse_color=(.14,.135,.125,1);floor.data.materials.append(m)
  bpy.ops.object.light_add(type="AREA",location=(6,-8,9));key=bpy.context.object;key.data.energy=1050;key.data.size=5.5;look_at(key,c)
  bpy.ops.object.light_add(type="AREA",location=(-4,-2,5));fill=bpy.context.object;fill.data.energy=420;fill.data.size=5;look_at(fill,c)
  bpy.ops.object.light_add(type="SUN",location=(0,0,8));sun=bpy.context.object;sun.data.energy=1.2;sun.rotation_euler=(math.radians(35),math.radians(-15),math.radians(-28))
 else:
  m=bpy.data.materials.new("silhouette");m.diffuse_color=(.015,.015,.015,1);m.use_nodes=True
  bs=m.node_tree.nodes.get("Principled BSDF")
  if bs:
   bs.inputs["Base Color"].default_value=(.015,.015,.015,1);bs.inputs["Roughness"].default_value=1
  s.view_layers[0].material_override=m
  bpy.ops.object.light_add(type="AREA",location=(0,-2,10));bpy.context.object.data.energy=1200;bpy.context.object.data.size=12
 bpy.ops.object.camera_add(location=(8,-13.6,7));cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=max(size.x,size.y)*1.42
 look_at(cam,(c.x,c.y,c.z+size.z*.08));s.camera=cam
 s.render.filepath=os.path.join(DOC,"granero-rich-camera-first"+("-silhouette.png" if sil else ".png"))
 bpy.ops.render.render(write_still=True)

objs=build();glb=export(objs);geo=geometry(objs);preview(objs,False);preview(objs,True)
blend=os.path.join(SRC,"Valoria_Granero_RichCameraFirst_v1.blend");bpy.ops.wm.save_as_mainfile(filepath=blend)
report={
 "schema_version":1,"program":"VALORIA GRANERO RICH CAMERA-FIRST v1","phase":"SOURCE_ISOLATED",
 "method":"One camera-first storage compound assembled only from three instances of the already SOURCE-VISUAL-PASS rich Granero geometry. No visible primitive/procedural architecture.",
 "input":{"path":INPUT,"sha256":sha(os.path.join(ROOT,INPUT))},
 "glb":os.path.relpath(glb,ROOT).replace("\\","/"),"glb_sha256":sha(glb),"geometry":geo,
 "blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),
 "previews":[
  "docs/evidence/valoria-granero-rich-camera-first-v1/granero-rich-camera-first.png",
  "docs/evidence/valoria-granero-rich-camera-first-v1/granero-rich-camera-first-silhouette.png"
 ],
 "tripo":False,"paid_credits":0,"source_visual_verdict":"PENDING_VISUAL_REVIEW","unity_integration_allowed":False
}
for p in [os.path.join(SRC,"source-report.json"),os.path.join(EVD,"source-report.json")]:
 with open(p,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
