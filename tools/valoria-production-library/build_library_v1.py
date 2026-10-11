import bpy, math, os, json, random
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SOURCE=os.path.join(ROOT,"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/DefensiveFamilyV1")
DEST=os.path.join(ROOT,"art-source/valoria/production/library-v1")
OUT=os.path.join(ROOT,"Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/LibraryV1")
for p in (DEST,OUT):os.makedirs(p,exist_ok=True)
random.seed(20261011)
def mat(n,c,rough=.82,metal=0):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True
 p=m.node_tree.nodes.get("Principled BSDF");p.inputs["Base Color"].default_value=(*c,1)
 p.inputs["Roughness"].default_value=rough;p.inputs["Metallic"].default_value=metal
 return m
limestone=mat("Valoria worn pale limestone",(.36,.32,.27))
dark=mat("Valoria recessed joints",(.13,.12,.11))
moss=mat("Valoria muted moss",(.13,.19,.105))
iron=mat("Valoria oxidized iron",(.14,.12,.10),.6,.3)
def block(n,pos,size,material):
 bpy.ops.mesh.primitive_cube_add(size=1, location=pos)
 o=bpy.context.object;o.name=n;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(material);b=o.modifiers.new("Stone edge wear","BEVEL")
 b.width=min(size)*.065;b.segments=2;bpy.ops.object.modifier_apply(modifier=b.name)
 return o
def render(name):
 bpy.ops.object.camera_add(location=(10,-14,10));cam=bpy.context.object
 cam.rotation_euler=(Vector((0,0,1.4))-cam.location).to_track_quat('-Z','Y').to_euler()
 cam.data.type='ORTHO';cam.data.ortho_scale=10; bpy.context.scene.camera=cam
 bpy.ops.object.light_add(type='AREA',location=(0,-4,9));bpy.context.object.data.energy=1250;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=8
 world=bpy.data.worlds.new("Valoria neutral evidence") if not bpy.data.worlds else bpy.data.worlds[0]
 bpy.context.scene.world=world;world.use_nodes=True;world.node_tree.nodes["Background"].inputs["Color"].default_value=(.26,.30,.37,1)
 scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=48
 scene.render.resolution_x=960;scene.render.resolution_y=720;scene.render.resolution_percentage=100
 scene.render.image_settings.file_format='PNG';scene.render.filepath=os.path.join(DEST,name+".png");bpy.ops.render.render(write_still=True)
 bpy.data.objects.remove(cam,do_unlink=True)
 for o in list(bpy.data.objects):
  if o.type=='LIGHT':bpy.data.objects.remove(o,do_unlink=True)
def process(label,source,kind):
 bpy.ops.wm.read_factory_settings(use_empty=True)
 global limestone, dark, moss, iron
 limestone=mat("Valoria worn pale limestone",(.36,.32,.27))
 dark=mat("Valoria recessed joints",(.13,.12,.11))
 moss=mat("Valoria muted moss",(.13,.19,.105))
 iron=mat("Valoria oxidized iron",(.14,.12,.10),.6,.3)
 bpy.ops.import_scene.gltf(filepath=os.path.join(SOURCE,source+".glb"))
 imported=[o for o in bpy.context.scene.objects if o.type=='MESH']
 if not imported:raise RuntimeError("Missing imported mesh: "+source)
 # Existing historical meshes remain intact; add separate authored detailing objects.
 if kind=="wall":
  for x in [i*.44 for i in range(-4,5)]:
   for z in (.46,.92,1.38,1.76):
    block("Coursed stone facing",(x,-.57,z),(.37,.055,.10),limestone)
   if abs(x)>1.25:block("Moss in damp buttress joint",(x,-.61,.25),(.25,.065,.09),moss)
 elif kind=="tower":
  for level in range(1,6):
   z=.35+level*.37
   for i in range(10):
    a=math.tau*(i+.3*(level%2))/10
    o=block("Tower worn ashlar",(.72*math.sin(a),.72*math.cos(a),z),(.26,.075,.12),limestone)
    o.rotation_euler.z=-a
  for i in range(8):
   a=i*math.tau/8
   block("Tower base lichen",(.78*math.sin(a),.78*math.cos(a),.16),(.19,.12,.055),moss)
 elif kind=="gate":
  for x in (-1.85,1.85):
   for z in (.45,.9,1.35,1.8,2.25):
    block("Gate ashlar on flanking pier",(x,-.69,z),(.46,.09,.20),limestone)
  for x in (-1.25,1.25):
   for z in (1.8,2.15):
    block("Gate iron anchor plate",(x,-.78,z),(.14,.04,.16),iron)
  for i in range(11):
   theta=math.pi*i/10
   x=1.28*math.cos(theta);z=2.1+1.15*math.sin(theta)
   block("Gate voussoir stone",(x,-.82,z),(.26,.15,.19),limestone)
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(DEST,label+".blend"))
 render(label)
 bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,label+".glb"),export_format='GLB',export_apply=True,export_materials='EXPORT')
 return {"id":label,"mesh_count":sum(o.type=='MESH' for o in bpy.data.objects),"source":source,"technical_status":"PENDING_UNITY","art_status":"REWORK_UNTIL_INDEPENDENT_COMPARISON"}
report=[]
for a,b,c in [("Valoria_Wall_LibraryV1","Valoria_WallSegment_DefV1","wall"),("Valoria_Tower_LibraryV1","Valoria_Tower_DefV1","tower"),("Valoria_Gate_LibraryV1","Valoria_MainGate_DefV1","gate")]:
 report.append(process(a,b,c))
with open(os.path.join(DEST,"manifest.json"),"w") as f:json.dump({"license":"Project-authored transformation of existing Eldoria sources","assets":report,"art_pass":False,"unity_pass":False},f,indent=2)
print(json.dumps(report))
