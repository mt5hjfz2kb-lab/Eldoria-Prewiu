import bpy, math, os, json, hashlib
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","defensive-family-v1")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","DefensiveFamilyV1")
PRE=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-defensive-family-v1")
for p in (SRC,OUT,PRE):os.makedirs(p,exist_ok=True)

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1048576),b""):h.update(c)
 return h.hexdigest()

def mat(name,color,rough=.8,metal=0):
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.use_nodes=True
 b=m.node_tree.nodes.get("Principled BSDF");b.inputs["Base Color"].default_value=(*color,1);b.inputs["Roughness"].default_value=rough;b.inputs["Metallic"].default_value=metal
 return m
STONE=mat("Eldoria Stone Authoring",(0.42,.34,.25),.86)
DARK=mat("Eldoria Stone Dark",(0.24,.21,.18),.9)
SLATE=mat("Eldoria Slate Authoring",(.07,.11,.16),.78)
TIMBER=mat("Eldoria Timber Authoring",(.16,.075,.03),.82)
METAL=mat("Eldoria Metal Authoring",(.12,.10,.075),.42,.45)
BLUE=mat("Eldoria Heraldry Authoring",(.025,.12,.34),.62)

def cube(name,loc,scale,material=STONE,bevel=.04):
 bpy.ops.mesh.primitive_cube_add(location=loc);o=bpy.context.object;o.name=name;o.scale=scale; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new("Authored edge bevel","BEVEL");mod.width=bevel;mod.segments=2
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 o.data.materials.append(material);return o

def cyl(name,loc,r,depth,verts=16,material=STONE):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=depth,location=loc);o=bpy.context.object;o.name=name;o.data.materials.append(material)
 b=o.modifiers.new("Authored edge bevel","BEVEL");b.width=.035;b.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name);return o

def arch_ring(prefix,z,width,height,depth,thick=.18,segments=18):
 objs=[]
 # voussoir ring made from actual wedge blocks; opening remains real negative space
 radius=width*.5
 cy=z+height-radius
 for i in range(segments+1):
  a=math.pi*(i/segments)
  x=math.cos(a)*radius
  y=math.sin(a)*radius
  ang=math.degrees(a)-90
  o=cube(prefix+f" voussoir {i:02d}",(x,-depth,cy+y),(thick*.55,depth*.55,thick*.85),STONE,.025)
  o.rotation_euler[1]=math.radians(ang);objs.append(o)
 return objs

def battlements(prefix,y,z,half_width,depth,step=.52):
 objs=[];x=-half_width
 while x<=half_width+.001:
  objs.append(cube(prefix+" merlon",(x,y,z),(.17,depth,.24),STONE,.025));x+=step
 return objs

def buttress(prefix,x,y,h,depth=.32):
 return [cube(prefix+" base",(x,y,h*.24),(.24,depth,h*.24),DARK,.035),
         cube(prefix+" shaft",(x,y,h*.60),(.18,depth*.82,h*.32),STONE,.03),
         cube(prefix+" cap",(x,y,h*.93),(.26,depth*.95,.10),STONE,.025)]

def banner(prefix,x,y,z,h=.95):
 pole=cyl(prefix+" pole",(x,y,z),.025,h*1.35,10,METAL)
 cloth=cube(prefix+" cloth",(x,y-.025,z-.05),(.22,.018,h*.42),BLUE,.01)
 return [pole,cloth]

def wall():
 objs=[]
 objs.append(cube("Wall masonry core",(0,0,1.05),(2.15,.42,1.05),STONE,.055))
 objs.append(cube("Wall plinth",(0,0,.16),(2.28,.50,.16),DARK,.035))
 objs.append(cube("Wall upper stringcourse",(0,-.02,1.88),(2.23,.49,.10),STONE,.025))
 objs+=battlements("Wall",0,2.18,2.08,.48,.50)
 for x in (-1.75,-.88,0,.88,1.75):objs+=buttress("Wall buttress",x,-.47,1.82,.25)
 # recessed arrow-slit frames read at strategic zoom
 for x in (-1.45,-.72,0,.72,1.45):
  objs.append(cube("Wall slit shadow",(x,-.435,1.20),(.055,.035,.24),DARK,.012))
  objs.append(cube("Wall slit lintel",(x,-.48,1.48),(.12,.07,.055),STONE,.012))
 return objs

def tower():
 objs=[]
 # octagonal keep with staged plinth and crown
 for i in range(8):
  a=math.radians(i*45);x=math.sin(a)*.52;y=math.cos(a)*.52
  o=cube("Tower faceted wall",(x,y,1.35),(.38,.17,1.18),STONE,.045);o.rotation_euler[2]=-a;objs.append(o)
 objs.append(cyl("Tower plinth",(0,0,.18),.86,.36,8,DARK))
 objs.append(cyl("Tower cornice",(0,0,2.42),.78,.18,8,STONE))
 for i in range(8):
  a=math.radians(i*45);x=math.sin(a)*.67;y=math.cos(a)*.67
  objs.append(cube("Tower crown merlon",(x,y,2.72),(.16,.16,.25),STONE,.025))
 # slate pyramidal crown creates Hero-compatible vertical accent
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.83,radius2=.16,depth=1.10,location=(0,0,3.36));roof=bpy.context.object;roof.name="Tower slate crown";roof.data.materials.append(SLATE);objs.append(roof)
 objs.append(cyl("Tower finial",(0,0,4.03),.055,.48,10,METAL))
 for a in (0,math.pi/2,math.pi,3*math.pi/2):
  x=math.sin(a)*.60;y=math.cos(a)*.60
  objs.append(cube("Tower arrow slit",(x,y,1.45),(.055,.035,.28),DARK,.01))
 objs+=banner("Tower heraldry",0,-.72,2.10,.85)
 return objs

def gate():
 objs=[]
 # two authored flanking piers
 for sx in (-1,1):
  x=sx*1.42
  objs.append(cube("Gate pier plinth",(x,0,.22),(.58,.62,.22),DARK,.045))
  objs.append(cube("Gate pier shaft",(x,0,1.42),(.48,.54,1.08),STONE,.055))
  objs.append(cube("Gate pier capital",(x,0,2.47),(.62,.65,.16),STONE,.035))
  objs+=buttress("Gate outer buttress",sx*1.92,-.48,2.12,.30)
 # bridge wall above opening and layered arch surround; real open center
 objs.append(cube("Gate bridge",(0,.02,2.15),(1.05,.52,.42),STONE,.045))
 objs+=arch_ring("Gate arch",.22,1.62,2.25,.56,.22,20)
 # recessed jamb stacks
 for sx in (-1,1):
  objs.append(cube("Gate inner jamb",(sx*.92,-.54,1.05),(.16,.12,.92),DARK,.025))
  objs.append(cube("Gate outer jamb",(sx*1.10,-.50,1.15),(.13,.16,1.08),STONE,.025))
 # machicolation/cornice rhythm
 objs.append(cube("Gate upper cornice",(0,0,2.62),(2.05,.66,.12),STONE,.025))
 for x in (-1.75,-1.15,-.58,0,.58,1.15,1.75):
  objs.append(cube("Gate machicolation",(x,-.55,2.78),(.16,.16,.18),DARK,.02))
 objs+=battlements("Gate",0,3.03,1.92,.58,.48)
 # central slate roof lantern and heraldry
 cube("Gate lantern base",(0,0,3.25),(.38,.36,.28),STONE,.035)
 bpy.ops.mesh.primitive_cone_add(vertices=4,radius1=.58,radius2=.08,depth=.78,location=(0,0,3.78),rotation=(0,0,math.radians(45)));r=bpy.context.object;r.name="Gate slate lantern roof";r.data.materials.append(SLATE);objs.append(r)
 objs+=banner("Gate heraldry",0,-.72,2.30,1.05)
 return [o for o in bpy.context.scene.objects if o.type=="MESH"]

def export_asset(aid,builder):
 bpy.ops.wm.read_factory_settings(use_empty=True)
 global STONE,DARK,SLATE,TIMBER,METAL,BLUE
 STONE=mat("Eldoria Stone Authoring",(0.42,.34,.25),.86);DARK=mat("Eldoria Stone Dark",(0.24,.21,.18),.9);SLATE=mat("Eldoria Slate Authoring",(.07,.11,.16),.78);TIMBER=mat("Eldoria Timber Authoring",(.16,.075,.03),.82);METAL=mat("Eldoria Metal Authoring",(.12,.10,.075),.42,.45);BLUE=mat("Eldoria Heraldry Authoring",(.025,.12,.34),.62)
 objs=builder()
 col=bpy.data.collections.new(aid);bpy.context.scene.collection.children.link(col)
 for o in objs:
  for c in list(o.users_collection):c.objects.unlink(o)
  col.objects.link(o)
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 glb=os.path.join(OUT,aid+".glb");bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 # Workbench isolated preview
 sc=bpy.context.scene
 if sc.world is None:sc.world=bpy.data.worlds.new("PreviewWorld")
 sc.display.shading.light="STUDIO";sc.display.shading.show_cavity=True;sc.display.shading.cavity_type="WORLD";sc.display.shading.color_type="MATERIAL"
 mn=Vector((min((o.matrix_world@Vector(c)).x for o in objs for c in o.bound_box),min((o.matrix_world@Vector(c)).y for o in objs for c in o.bound_box),min((o.matrix_world@Vector(c)).z for o in objs for c in o.bound_box)))
 mx=Vector((max((o.matrix_world@Vector(c)).x for o in objs for c in o.bound_box),max((o.matrix_world@Vector(c)).y for o in objs for c in o.bound_box),max((o.matrix_world@Vector(c)).z for o in objs for c in o.bound_box)))
 ctr=(mn+mx)*.5;span=max(*(mx-mn),.5)
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=span*1.45;cam.location=ctr+Vector((span*1.4,-span*1.8,span*1.25));cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 sc.render.engine="BLENDER_WORKBENCH";sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100;sc.render.image_settings.file_format="PNG";sc.render.filepath=os.path.join(PRE,aid+".png");bpy.ops.render.render(write_still=True)
 tris=0
 for o in objs:o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles)
 return {"asset_id":aid,"triangles":tris,"glb":os.path.relpath(glb,ROOT).replace("\\","/"),"sha256":sha(glb)}

reports=[export_asset("Valoria_MainGate_DefV1",gate),export_asset("Valoria_WallSegment_DefV1",wall),export_asset("Valoria_Tower_DefV1",tower)]
# assemble canonical blend by importing final GLBs
bpy.ops.wm.read_factory_settings(use_empty=True)
for r in reports:bpy.ops.import_scene.gltf(filepath=os.path.join(ROOT,r["glb"]))
blend=os.path.join(SRC,"Valoria_DefensiveFamily_v1.blend");bpy.ops.wm.save_as_mainfile(filepath=blend)
report={"schema_version":1,"classification":"TEMPORARY","authoring_method":"manual_high_fidelity_defensive_reauthoring","blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),"assets":reports}
with open(os.path.join(SRC,"defensive-family-v1-build-report.json"),"w") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
