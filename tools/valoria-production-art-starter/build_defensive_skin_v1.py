import bpy,os,json,hashlib,math
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","defensive-skin-v1");OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","DefensiveSkinV1");PRE=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-defensive-skin-v1")
for p in (SRC,OUT,PRE):os.makedirs(p,exist_ok=True)
WALL="Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb"
CORNER="Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb"

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1048576),b""):h.update(c)
 return h.hexdigest()
def imp(rel,prefix):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,rel));o=[x for x in bpy.context.scene.objects if x not in before and x.type=="MESH"]
 for x in o:x.name=prefix+" · "+x.name
 return o
def bb(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box];return Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts))),Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
def norm(objs,w,h):
 lo,hi=bb(objs);d=hi-lo;s=min(w/max(d.x,.001),h/max(d.z,.001))
 for o in objs:o.scale*=s
 bpy.context.view_layer.update();lo,hi=bb(objs);c=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
 for o in objs:o.location-=c
def dup(objs):
 out=[]
 for o in objs:
  n=o.copy();n.data=o.data.copy()
  for c in o.users_collection:c.objects.link(n)
  out.append(n)
 return out
def xform(objs,x=0,y=0,z=0,rz=0):
 for o in objs:o.location+=Vector((x,y,z));o.rotation_euler[2]+=math.radians(rz)
def simple_mat(name,col):
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);return m
SLATE=None;BLUE=None
def roof(loc,r=.72,h=.95):
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r,radius2=.10,depth=h,location=loc);o=bpy.context.object;o.name="Authored slate crown";o.data.materials.append(SLATE);return o
def core(name,loc,scale):
 bpy.ops.mesh.primitive_cube_add(location=loc);o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 # support geometry deliberately hidden behind donor skin
 o.data.materials.append(simple_mat("Support dark stone",(.19,.18,.17)));return o
def banner(x,y,z):
 bpy.ops.mesh.primitive_cube_add(location=(x,y,z));o=bpy.context.object;o.name="Heraldic banner";o.scale=(.22,.025,.62);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(BLUE);return o

def wall_asset():
 skin=imp(WALL,"Wall skin");norm(skin,4.4,2.35)
 # retain rich donor as visible face; shallow support behind only
 core("Wall support",(0,.22,1.05),(2.05,.20,.98))
 return [o for o in bpy.context.scene.objects if o.type=="MESH"]

def tower_asset():
 # four narrow rich wall skins wrap a support octagonal/cross tower; avoids rock mass.
 front=imp(WALL,"Tower front skin");norm(front,1.55,2.65);xform(front,y=-.50)
 back=dup(front);xform(back,y=1.0,rz=180)
 left=dup(front);xform(left,x=-.50,y=.50,rz=90)
 right=dup(front);xform(right,x=1.0,rz=-90)
 core("Tower support",(0,0,1.28),(.68,.68,1.25))
 roof((0,0,3.18),.90,1.10);banner(0,-.73,2.05)
 return [o for o in bpy.context.scene.objects if o.type=="MESH"]

def gate_asset():
 # paired corner donors provide sculpted piers; wall skins bridge them without closing opening
 L=imp(CORNER,"Gate west pier");norm(L,1.30,2.75);xform(L,x=-1.55,y=.05,rz=-10)
 R=dup(L);xform(R,x=3.10,rz=20)
 bridge=imp(WALL,"Gate bridge skin");norm(bridge,2.55,1.15);xform(bridge,z=2.15)
 # trim lintel and roof lantern are authored accents, not the visible masonry body
 core("Gate hidden bridge support",(0,.20,2.32),(1.20,.22,.42))
 roof((0,0,3.35),.58,.82);banner(0,-.60,2.45)
 return [o for o in bpy.context.scene.objects if o.type=="MESH"]

def export(aid,builder):
 bpy.ops.wm.read_factory_settings(use_empty=True)
 global SLATE,BLUE;SLATE=simple_mat("Eldoria Slate",(.06,.10,.16));BLUE=simple_mat("Eldoria Heraldry",(.02,.10,.32))
 objs=builder();col=bpy.data.collections.new(aid);bpy.context.scene.collection.children.link(col)
 for o in objs:
  for c in list(o.users_collection):c.objects.unlink(o)
  col.objects.link(o)
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 glb=os.path.join(OUT,aid+".glb");bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 # workbench preview
 lo,hi=bb(objs);ctr=(lo+hi)*.5;span=max(*(hi-lo),.5);sc=bpy.context.scene
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=span*1.45;cam.location=ctr+Vector((span*1.45,-span*1.8,span*1.2));cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 sc.render.engine="BLENDER_WORKBENCH";sc.display.shading.light="STUDIO";sc.display.shading.show_cavity=True;sc.display.shading.cavity_type="WORLD";sc.display.shading.color_type="MATERIAL";sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100;sc.render.image_settings.file_format="PNG";sc.render.filepath=os.path.join(PRE,aid+".png");bpy.ops.render.render(write_still=True)
 return {"asset_id":aid,"glb":os.path.relpath(glb,ROOT).replace("\\","/"),"sha256":sha(glb)}
rep=[export("Valoria_MainGate_SkinV1",gate_asset),export("Valoria_WallSegment_SkinV1",wall_asset),export("Valoria_Tower_SkinV1",tower_asset)]
bpy.ops.wm.read_factory_settings(use_empty=True)
for r in rep:bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,r["glb"]))
blend=os.path.join(SRC,"Valoria_DefensiveSkin_v1.blend");bpy.ops.wm.save_as_mainfile(filepath=blend)
with open(os.path.join(SRC,"defensive-skin-v1-build-report.json"),"w") as f:json.dump({"classification":"TEMPORARY","method":"rich_donor_skin_over_authored_support","blend_sha256":sha(blend),"assets":rep},f,indent=2)
