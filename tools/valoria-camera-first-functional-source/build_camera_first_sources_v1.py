import bpy, os, json, hashlib, math
from mathutils import Vector, Matrix

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","camera-first-functional-source-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","CameraFirstFunctionalSourceV1")
EVD=os.path.join(ROOT,"pipeline","evidence","valoria-camera-first-functional-source-v1")
DOC=os.path.join(ROOT,"docs","evidence","valoria-camera-first-functional-source-v1")
for p in (SRC,OUT,EVD,DOC): os.makedirs(p,exist_ok=True)

PATHS={
 "granero":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/Valoria_Granero_GCSUv1.glb",
 "cuartel":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/Valoria_Cuartel_GCSUv1.glb",
 "wall":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/FirstProductionDistrictV1/Valoria_DefenseWall_FPDv1.glb",
 "tower":"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/FirstProductionDistrictV1/Valoria_DefenseTower_FPDv1.glb"
}

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for b in iter(lambda:f.read(1048576),b""): h.update(b)
 return h.hexdigest()

def reset(): bpy.ops.wm.read_factory_settings(use_empty=True)

def mat(name,color,rough=.78,metal=0):
 m=bpy.data.materials.new(name)
 m.diffuse_color=(*color,1)
 m.use_nodes=True
 bs=m.node_tree.nodes.get("Principled BSDF")
 if bs:
  bs.inputs["Base Color"].default_value=(*color,1)
  bs.inputs["Roughness"].default_value=rough
  bs.inputs["Metallic"].default_value=metal
 return m

STONE=None;TIMBER=None;ROOF=None;DARK=None;EARTH=None
def authored_materials():
 global STONE,TIMBER,ROOF,DARK,EARTH
 STONE=mat("Eldoria Stone · camera-first",(0.34,0.31,0.27),.88)
 TIMBER=mat("Eldoria Timber · camera-first",(0.22,0.13,0.075),.82)
 ROOF=mat("Eldoria Slate · camera-first",(0.16,0.19,0.21),.90)
 DARK=mat("Functional recess · camera-first",(0.055,0.052,0.045),.96)
 EARTH=mat("Functional yard · camera-first",(0.24,0.205,0.155),.98)

def import_glb(rel,prefix):
 p=os.path.join(ROOT,rel)
 if not os.path.isfile(p): raise RuntimeError("Missing input "+rel)
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.gltf(filepath=p)
 imported=[o for o in bpy.context.scene.objects if o not in before]
 meshes=[o for o in imported if o.type=="MESH"]
 if not meshes: raise RuntimeError("No mesh in "+rel)
 for i,o in enumerate(meshes):
  mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw
  o.name=f"{prefix} · rich {i:02d}"
  for poly in o.data.polygons: poly.use_smooth=True
 for o in imported:
  if o.type!="MESH" and o.name in bpy.data.objects and len(o.children)==0:
   bpy.data.objects.remove(o,do_unlink=True)
 return meshes

def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def fit(objs,width,height,loc=(0,0,0),yaw=0,stretch=(1,1,1)):
 mn,mx=bounds(objs); sz=mx-mn
 k=min(width/max(sz.x,sz.y,.001),height/max(sz.z,.001))
 c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 root=bpy.data.objects.new("camera-first component",None);bpy.context.scene.collection.objects.link(root)
 for o in objs:
  mw=o.matrix_world.copy();o.parent=root;o.matrix_world=mw
 M=(Matrix.Translation(Vector(loc)) @ Matrix.Rotation(math.radians(yaw),4,'Z') @
    Matrix.Diagonal(Vector((k*stretch[0],k*stretch[1],k*stretch[2],1))) @ Matrix.Translation(-c))
 root.matrix_world=M;bpy.context.view_layer.update()
 for o in objs:
  mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw
 bpy.data.objects.remove(root,do_unlink=True);bpy.context.view_layer.update()
 return objs

def box(name,loc,scale,material,bevel=.08):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=scale
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if material:o.data.materials.append(material)
 if bevel>0:
  mod=o.modifiers.new("authored edge","BEVEL");mod.width=bevel;mod.segments=2
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o

def gable(name,loc,width,depth,wall_h,rise,material):
 x=width/2;y=depth/2;z=wall_h
 verts=[(-x,-y,0),(x,-y,0),(x,y,0),(-x,y,0),
        (-x,-y,z),(x,-y,z),(x,y,z),(-x,y,z),
        (0,-y,z+rise),(0,y,z+rise)]
 faces=[(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(0,3,2,1),
        (4,5,8),(7,9,6),(4,8,9,7),(5,6,9,8)]
 me=bpy.data.meshes.new(name+" mesh");me.from_pydata(verts,[],faces);me.update()
 o=bpy.data.objects.new(name,me);bpy.context.scene.collection.objects.link(o);o.location=loc
 if material:o.data.materials.append(material)
 bev=o.modifiers.new("authored roof edge","BEVEL");bev.width=.055;bev.segments=2
 bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bev.name)
 return o

def cylinder(name,loc,radius,depth,material,vertices=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc)
 o=bpy.context.object;o.name=name
 if material:o.data.materials.append(material)
 bev=o.modifiers.new("authored edge","BEVEL");bev.width=.05;bev.segments=2
 bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bev.name)
 return o

def cone(name,loc,r1,depth,material,vertices=12):
 bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=r1,radius2=0,depth=depth,location=loc)
 o=bpy.context.object;o.name=name
 if material:o.data.materials.append(material)
 return o

def recenter(objs):
 mn,mx=bounds(objs);c=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=c
 bpy.context.view_layer.update()

def build_granero():
 reset();authored_materials();parts=[]
 core=import_glb(PATHS["granero"],"Granero core")
 parts+=fit(core,4.45,3.30,(0,.55,.04),0,(1.00,.88,1.0))
 # Camera-facing agricultural macro: unmistakable loading apron + broad porch.
 parts.append(box("Granero · loading apron",(0,-1.78,.12),(4.55,1.48,.24),STONE,.06))
 parts.append(gable("Granero · dominant loading canopy",(0,-1.20,1.76),4.45,1.62,.18,.72,ROOF))
 for x in (-1.78,-.58,.58,1.78):
  parts.append(box("Granero · canopy timber", (x,-1.68,.90),(.16,.18,1.65),TIMBER,.025))
 # Three large dark loading mouths, intentionally oversized for zoom9/mobile.
 for x in (-1.22,0,1.22):
  parts.append(box("Granero · deep loading mouth",(x,-.43,1.02),(.82,.15,1.25),DARK,.02))
 # Twin grain bins create a non-residential storage silhouette.
 for x in (-2.03,2.03):
  parts.append(cylinder("Granero · grain bin body",(x,.62,1.12),.58,2.15,TIMBER,12))
  parts.append(cone("Granero · grain bin roof",(x,.62,2.46),.72,.72,ROOF,12))
  parts.append(box("Granero · bin stone foot",(x,.62,.16),(1.15,1.15,.32),STONE,.06))
 recenter(parts);return parts

def build_cuartel():
 reset();authored_materials();parts=[]
 core=import_glb(PATHS["cuartel"],"Cuartel core")
 parts+=fit(core,4.20,3.25,(0,.95,.02),0,(1.0,.92,1.0))
 # Rebuild the screen-facing military grammar as a clear U court from rich certified defense.
 for x,y,yaw in [(-2.35,-1.05,2),(2.35,-1.05,-2)]:
  t=import_glb(PATHS["tower"],"Cuartel front guard tower")
  parts+=fit(t,1.62,3.05,(x,y,.02),yaw,(.86,.86,1.0))
 for x,y,yaw in [(-1.28,-.48,8),(1.28,-.48,-8)]:
  w=import_glb(PATHS["wall"],"Cuartel training wall")
  parts+=fit(w,2.35,1.42,(x,y,.02),yaw,(1.0,.70,.82))
 # Large open training court / gate channel is part of the source, not a later prop layer.
 parts.append(box("Cuartel · training yard",(0,-1.08,.10),(3.85,2.15,.20),EARTH,.04))
 # Strong central gate threshold without a decorative banner dependency.
 parts.append(box("Cuartel · gate threshold",(0,-2.02,.16),(1.72,.56,.30),STONE,.05))
 recenter(parts);return parts

def geometry(objs):
 tris=verts=0;mats=set()
 for o in objs:
  if o.type!="MESH":continue
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds([o for o in objs if o.type=="MESH"])
 return {"objects":len([o for o in objs if o.type=="MESH"]),"vertices":verts,"triangles":tris,"materials":sorted(mats),
         "bounds":[round(v,4) for v in (*mn,*mx)]}

def export(name,objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,name+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def look_at(obj,pt):
 obj.rotation_euler=(Vector(pt)-obj.location).to_track_quat("-Z","Y").to_euler()

def render(kind,objs,silhouette=False):
 mn,mx=bounds([o for o in objs if o.type=="MESH"]);center=(mn+mx)*.5;size=mx-mn
 scene=bpy.context.scene;scene.render.engine="BLENDER_EEVEE"
 scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
 scene.render.image_settings.file_format="PNG";scene.render.film_transparent=False
 if scene.world is None:scene.world=bpy.data.worlds.new("Camera-first world")
 scene.world.color=(.78,.79,.80) if silhouette else (.065,.07,.075)
 bpy.ops.mesh.primitive_plane_add(size=max(size.x,size.y)*3.2,location=(0,0,mn.z-.025))
 floor=bpy.context.object;floor.name="preview floor"
 fm=mat("preview floor mat",(.70,.71,.72) if silhouette else (.15,.145,.135),.95);floor.data.materials.append(fm)
 if not silhouette:
  bpy.ops.object.light_add(type="AREA",location=(6,-7,9));key=bpy.context.object;key.data.energy=1100;key.data.size=5.5;look_at(key,center)
  bpy.ops.object.light_add(type="AREA",location=(-4,-2,5));fill=bpy.context.object;fill.data.energy=420;fill.data.size=5;look_at(fill,center)
  bpy.ops.object.light_add(type="SUN",location=(0,0,8));sun=bpy.context.object;sun.data.energy=1.25;sun.rotation_euler=(math.radians(35),math.radians(-15),math.radians(-28))
 else:
  sm=mat("silhouette override",(.025,.025,.025),1.0);scene.view_layers[0].material_override=sm
  scene.render.engine="BLENDER_WORKBENCH";scene.display.shading.light='FLAT';scene.display.shading.color_type='SINGLE';scene.display.shading.single_color=(.025,.025,.025)
 bpy.ops.object.camera_add(location=(8.0,-13.6,7.0));cam=bpy.context.object
 cam.data.type="ORTHO";cam.data.ortho_scale=max(size.x,size.y)*1.45
 look_at(cam,(center.x,center.y,center.z+size.z*.08));scene.camera=cam
 suffix="-silhouette-camera.png" if silhouette else "-camera-first.png"
 scene.render.filepath=os.path.join(DOC,kind+suffix);bpy.ops.render.render(write_still=True)

records=[]
for kind,fn,name in [
 ("granero",build_granero,"Valoria_Granero_CFSv1"),
 ("cuartel",build_cuartel,"Valoria_Cuartel_CFSv1")]:
 objs=fn();glb=export(name,objs);geo=geometry(objs)
 render(kind,objs,False);render(kind,objs,True)
 records.append({
  "role":kind,"asset_id":name,"glb":os.path.relpath(glb,ROOT).replace("\\","/"),
  "glb_sha256":sha(glb),"geometry":geo,
  "camera_authorship":"macro silhouette authored for canonical southeast orthographic/mobile read before Unity placement",
  "previews":[
   os.path.relpath(os.path.join(DOC,kind+"-camera-first.png"),ROOT).replace("\\","/"),
   os.path.relpath(os.path.join(DOC,kind+"-silhouette-camera.png"),ROOT).replace("\\","/")
  ]
 })

reset()
# Persist both assets in one blend by importing their generated GLBs side by side.
for i,rec in enumerate(records):
 bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,rec["glb"]))
 for o in [o for o in bpy.context.scene.objects if o.type=="MESH" and abs(o.location.x)<100]:
  if i==0:o.location.x-=4.0
  else:o.location.x+=4.0
blend=os.path.join(SRC,"Valoria_CameraFirst_Functional_Source_v1.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend)

report={
 "schema_version":1,
 "program":"VALORIA CAMERA-FIRST FUNCTIONAL SOURCE v1",
 "phase":"SOURCE_ISOLATED",
 "method":"New camera-first source authoring, not a placement iteration. Granero retains rich approved core but adds large authored loading canopy, loading mouths and twin grain-bin macro silhouette. Cuartel retains rich approved core and uses certified rich defense modules to form a symmetric U-shaped guarded training court with twin front towers.",
 "tripo":False,"paid_credits":0,
 "inputs":{k:{"path":v,"sha256":sha(os.path.join(ROOT,v))} for k,v in PATHS.items()},
 "blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),
 "assets":records,
 "source_visual_verdict":{"granero":"PENDING_VISUAL_REVIEW","cuartel":"PENDING_VISUAL_REVIEW"},
 "unity_integration_allowed":False
}
for p in [os.path.join(SRC,"source-report.json"),os.path.join(EVD,"source-report.json")]:
 with open(p,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
