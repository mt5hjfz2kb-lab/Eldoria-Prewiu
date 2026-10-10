"""Valoria fortification family: architectural quality pass v2, Blender-only, free."""
import bpy, math, random, json, os
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(os.environ.get("ELDORIA_FORTIFICATION_OUTPUT",str(ROOT/"artifacts-local/fortifications")))
OUT.mkdir(parents=True,exist_ok=True)
random.seed(20261010)
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(name,color,rough=0.88,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 bs=m.node_tree.nodes.get("Principled BSDF");bs.inputs["Base Color"].default_value=(*color,1)
 bs.inputs["Roughness"].default_value=rough;bs.inputs["Metallic"].default_value=metal
 return m
stone=[mat("limestone_%02d"%i,c) for i,c in enumerate([(0.39,.40,.36),(.47,.46,.39),(.34,.36,.35),(.49,.47,.41),(.37,.38,.34),(.43,.42,.37),(.54,.51,.44),(.30,.32,.31)])]
trim=mat("cut_limestone",(.58,.54,.46))
mortar=mat("dark_recessed_mortar",(.21,.23,.21))
wood=[mat("aged_oak_%d"%i,c,.82) for i,c in enumerate([(.23,.12,.066),(.31,.17,.087),(.18,.095,.05)])]
iron=mat("forged_iron",(.095,.10,.105),.53,.6)
moss=mat("damp_moss",(.14,.20,.12))
def cube(name,loc,size,material,bevel=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(material)
 if bevel:
  m=o.modifiers.new("worn_arris","BEVEL");m.width=bevel;m.segments=1
  o.modifiers.new("weighted_normals","WEIGHTED_NORMAL")
 return o
def frustum(name,loc,r0,r1,height,material,verts=16):
 bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r0,radius2=r1,depth=height,location=loc)
 o=bpy.context.object;o.name=name;o.data.materials.append(material);return o
def arch_segment(name,cx,cy,cz,r,a0,a1,depth,material,width=.28):
 # Curved voussoir quadrilateral, full-depth and radial joints.
 verts=[]
 for y in (-depth/2,depth/2):
  for r2,a in ((r,a0),(r+width,a0),(r+width,a1),(r,a1)):
   verts.append((cx+r2*math.cos(a),cy+y,cz+r2*math.sin(a)))
 faces=[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob);mesh.materials.append(material);return ob
def courses(name,x0,x1,y,z0,z1,seed=0):
 rng=random.Random(seed+113)
 # Solid recessed backing hides light leaks while relief masonry is varied.
 cube(name+"_core",((x0+x1)/2,y,(z0+z1)/2),(x1-x0,1.17,z1-z0),mortar)
 z=z0;row=0
 while z<z1-.06:
  h=min(z1-z,rng.uniform(.36,.58));x=x0-(.45 if row%2 else .0)
  while x<x1:
   length=rng.uniform(.55,1.45)
   start=max(x,x0);end=min(x+length,x1)
   if end-start>.17:
    c=rng.choice(stone)
    for side in (-1,1):
     yy=y+side*(.597+rng.uniform(-.012,.028))
     o=cube(name+"_irregular_stone",((start+end)/2,yy,z+h/2+rng.uniform(-.015,.015)),(end-start-.024,.095,h-.035),c,.023)
     o.rotation_euler[1]=rng.uniform(-.009,.009)
     if rng.random()<.045 and z<1.5: cube(name+"_moss",(start+.07,yy+side*.054,z+.07),(.15,.025,.085),moss)
   x+=length
  z+=h;row+=1
def crenels(name,x0,x1,y,z):
 i=0;x=x0+.19
 while x<x1-.48:
  cube(name+"_merlon",(x+.37,y,z+.47),(.74,1.24,.94),trim,.075)
  if i%3==0:cube(name+"_slit",(x+.37,y-.633,z+.47),(.095,.026,.43),iron)
  x+=1.35;i+=1
def wall(name,x=0,y=0,length=8,detail=True):
 x0=x-length/2;x1=x+length/2
 courses(name,x0,x1,y,0,4.38,seed=round((x+20)*13))
 cube(name+"_string_course",(x,y,3.89),(length+.12,1.42,.15),trim,.045)
 cube(name+"_coping",(x,y,4.46),(length+.22,1.53,.24),trim,.06)
 if detail:
  for s in (-1,1):
   for p in range(1,max(2,int(length/2))):
    px=x0+p*1.95
    cube(name+"_buttress",(px,y+s*.78,1.62),(.57,.59,3.2),stone[(p+2)%len(stone)],.075)
    cube(name+"_buttress_cap",(px,y+s*.81,3.27),(.72,.75,.2),trim,.045)
 crenels(name,x0,x1,y,4.57)
def tower(name,x=0,y=0):
 # Silhouette is a continuous tapered drum, not 180 overlapping boxes.
 frustum(name+"_recessed_drum",(x,y,3.35),2.38,2.16,6.7,mortar,24)
 rng=random.Random(53)
 for j in range(14):
  z=j*.46+.22
  for k in range(24):
   theta=(k+(j%2)*.48)*math.tau/24
   radius=2.33-(z/6.7)*.22
   face=arch_segment(name+"_curved_masonry",x,y,z, radius,theta,theta+math.tau/24-.012,.24,stone[rng.randrange(len(stone))],.12)
   # Wedges wrap around tower with continuous curved surface.
   face.rotation_euler[2]=0
 for z,radius in ((.44,2.39),(4.45,2.23),(6.65,2.19)):
  frustum(name+"_annular_band",(x,y,z),radius+.14,radius+.14,.16,trim,32)
 for k in range(12):
  t=math.tau*k/12
  if k%2==0:
   obj=cube(name+"_crown_merlon",(x+2.22*math.cos(t),y+2.22*math.sin(t),7.16),(.93,.68,.96),trim,.07)
   obj.rotation_euler[2]=t
  if k%3==0:
   # Vertical blind arrow slit on masonry face.
   obj=cube(name+"_loophole",(x+2.19*math.cos(t),y+2.19*math.sin(t),4.9),(.12,.075,.92),iron,.012)
   obj.rotation_euler[2]=t
def gate():
 for s in (-1,1):
  xx=-3.28 if s==-1 else 3.28
  courses("gate_pier_"+str(s),xx-1.18,xx+1.18,0,0,6.35,120+int(xx*10))
  cube("gate_pier_coping",(xx,0,6.46),(2.5,1.64,.2),trim,.045)
 # Arch spans an actually open passage: jambs terminate before the arch crown.
 for i in range(13):
  a=math.pi*i/13
  arch_segment("arched_voussoir",0,0,3.0,1.96,a,a+math.pi/13,1.69,trim,.45)
 cube("gate_header",(0,0,6.14),(4.42,1.43,.42),stone[2],.035)
 for s in (-1,1):
  for k in range(7):
   xx=s*(.13+(k+.5)*.25)
   cube("vertical_oak_plank",(xx,.06,1.42),(.24,.2,2.82),wood[k%3],.025)
  for z in (.38,2.42):
   cube("black_iron_strap",(s*.99,-.08,z),(1.93,.085,.11),iron,.02)
  cube("strap_hinge",(s*1.68,-.13,1.52),(.21,.12,2.5),iron,.026)
 crenels("gate_crown",-4.48,4.48,0,6.5)
 for s in (-1,1):
  tower("gate_flank_tower",s*6.37,0)
def build(kind):
 bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
 if kind=="wall":wall("wall")
 elif kind=="corner_inner" or kind=="corner_outer":
  wall("corner_first")
  old={o.name for o in bpy.data.objects}
  wall("corner_second")
  sign=-1 if kind=="corner_inner" else 1
  for o in bpy.data.objects:
   if o.name not in old and o.type=="MESH":
    px,py=o.location.x,o.location.y
    o.location.x,o.location.y=sign*3-py,3+px
    o.rotation_euler[2]+=math.pi/2
 elif kind=="tower":tower("watchtower")
 elif kind=="gate":gate()
 elif kind=="parapet":wall("reinforced_short_wall",length=5)
def scene(target,scale):
 s=bpy.context.scene;s.render.engine="CYCLES";s.cycles.samples=24
 s.render.resolution_x=960;s.render.resolution_y=640;s.render.resolution_percentage=100
 s.view_settings.view_transform="Standard";s.view_settings.look="Medium High Contrast"
 world=bpy.data.worlds.new("cloudy_world");s.world=world;world.use_nodes=True
 world.node_tree.nodes["Background"].inputs["Color"].default_value=(.36,.40,.45,1)
 light=bpy.data.lights.new("broad_key","AREA");ob=bpy.data.objects.new("broad_key",light);s.collection.objects.link(ob)
 ob.location=(-9,-12,19);light.energy=3600;light.size=9
 cam=bpy.data.cameras.new("asset_orthographic");ob=bpy.data.objects.new("asset_orthographic",cam);s.collection.objects.link(ob)
 ob.location=(16,-22,16);ob.rotation_euler=(Vector(target)-ob.location).to_track_quat("-Z","Y").to_euler()
 cam.type="ORTHO";cam.ortho_scale=scale;s.camera=ob
def export(name):
 # Render before material batching, with identical camera and light across families.
 s=bpy.context.scene;s.render.filepath=str(OUT/(name+".png"));bpy.ops.render.render(write_still=True)
 mesh=[o for o in bpy.data.objects if o.type=="MESH"]
 for o in mesh:
  bpy.ops.object.select_all(action="DESELECT");o.select_set(True);bpy.context.view_layer.objects.active=o
  for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 batches={}
 for o in mesh:batches.setdefault(o.data.materials[0].name,[]).append(o)
 for group in batches.values():
  if len(group)<2:continue
  bpy.ops.object.select_all(action="DESELECT")
  for o in group:o.select_set(True)
  bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join()
 mesh=[o for o in bpy.data.objects if o.type=="MESH"]
 bpy.ops.object.select_all(action="DESELECT")
 for o in mesh:o.select_set(True)
 bpy.context.view_layer.objects.active=mesh[0]
 bpy.ops.export_scene.gltf(filepath=str(OUT/(name+".glb")),export_format="GLB",use_selection=True)
 return dict(name=name,meshes=len(mesh),triangles=sum(len(p.vertices)-2 for o in mesh for p in o.data.polygons),glb_bytes=(OUT/(name+".glb")).stat().st_size)
manifest=[]
for kind in ["wall","corner_inner","corner_outer","tower","gate","parapet"]:
 build(kind);scene((0,0,3.4),23 if kind=="gate" else 18);manifest.append(export(kind))
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
wall("west",-9.0,0,7.2);wall("east",9.0,0,7.2);gate()
scene((0,0,4),46);assembly=export("assembled_gatehouse")
(OUT/"manifest.json").write_text(json.dumps(dict(status="architectural v2 visual review pending",version=2,pieces=manifest,assembly=assembly),indent=2))
print("ELDORIA_FORTIFICATION_EXPORT_PASS",len(manifest))
