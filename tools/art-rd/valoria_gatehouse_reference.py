"""Focused gatehouse art pass; preserves existing six-piece family generator."""
import bpy, math, random, os, json, sys
from pathlib import Path
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).resolve().parent))
import valoria_fortification_family as base
OUT=Path(os.environ.get("ELDORIA_GATE_REFERENCE_OUTPUT",str(Path(__file__).resolve().parents[2]/"artifacts-local/gate-reference")))
OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
random.seed(98)
# Existing architecture is retained; this stage supplements its legibility, wear and identity.
base.gate()
def box(n,p,s,m,b=.02):return base.cube(n,p,s,m,b)
stone=base.stone;oak=base.wood;iron=base.iron;trim=base.trim
patina=base.mat("soot_oxide",(.18,.18,.16))
highlight=base.mat("warm_stone_chips",(.57,.49,.36))
gold=base.mat("aged_brass",(.42,.28,.105),.47,.72)
def ring(name,center,outer,inner,material):
 x,y,z=center
 for i in range(16):
  a=math.tau*i/16
  obj=box(name,(x+outer*math.cos(a),y,z+outer*math.sin(a)),(.21,.09,.13),material,.014)
  obj.rotation_euler[1]=-a
# Enhance central gateway without doubling entire masonry, architectural details favor camera-visible silhouette.
for side in (-1,1):
 x=side*3.3
 # chamfered layered piers and engaged buttresses
 for z in (1.05,3.08,5.06):
  box("pier_buttress_staged",(x+side*1.08,-.99,z),(.56,.62,1.66),stone[5 if z<2 else 1],.09)
  box("pier_buttress_cap",(x+side*1.08,-1.02,z+.87),(.73,.79,.16),trim,.05)
 for zz in (2.0,4.1):
  box("pier_recess",(x,-.68,zz),(.15,.065,.52),patina,.025)
 # timber transverse braces and oversize forged rivets
 for z in (.52,2.18):
  box("oak_gate_brace",(side*.99,-.205,z),(1.83,.14,.15),oak[1],.03)
  for i in range(5):
   box("hand_forged_rivet",(side*(.25+i*.34),-.305,z),(.075,.055,.078),iron,.02)
 # prominent transverse iron strap hinge plates, visible at hero camera
 for zz in (.78,2.08):
  box("hinge_plate",(side*1.48,-.26,zz),(.52,.095,.31),iron,.038)
  for dz in (-.08,.08):
   box("hinge_rivet",(side*1.48,-.33,zz+dz),(.09,.07,.09),gold,.022)
# dark recessed portcullis teeth visible between open masonry and wooden leaves
for i in range(-5,6):
 xx=i*.28
 box("portcullis_spike",(xx,.30,2.75),(.095,.13,.28),iron,.014)
# vault transition: extra voussoirs establish nested load-bearing arch
for i in range(17):
 a=math.pi*i/17
 base.arch_segment("inner_arch_rib",0,-.89,3.0,1.76,a,a+math.pi/17-.006,.15,stone[(i+2)%8],.14)
# arch keystone on facade, intentional singular focal detail
box("prominent_keystone",(0,-.91,5.16),(.35,.18,.60),highlight,.075)
# two towers: silhouettes, machicolation corbels, weather caps, practical loopholes
for sx in (-1,1):
 xx=sx*6.37
 for k in range(16):
  a=math.tau*k/16
  x=xx+2.22*math.cos(a);y=2.22*math.sin(a)
  ob=box("projecting_parapet_corbel",(x,y,6.73),(.50,.47,.52),trim,.055)
  ob.rotation_euler[2]=a
 for a in (-math.pi*.35,-math.pi*.65):
  x=xx+2.24*math.cos(a);y=2.24*math.sin(a)
  ob=box("arrow_slit_depth",(x,y,3.07),(.16,.11,.84),patina,.025)
  ob.rotation_euler[2]=a
# shield / sigil: simple sculpted crest, centered on gate lintel
box("heraldic_field",(0,-.81,5.87),(.69,.13,.74),iron,.075)
box("heraldic_vertical",(0,-.91,5.89),(.12,.07,.53),gold,.025)
box("heraldic_crossbar",(0,-.91,5.91),(.43,.07,.11),gold,.025)
# controlled moss/soot clusters at base and lintel; sparse large readable patches
for s in (-1,1):
 for j in range(22):
  rng=random.Random(1000+j+int(s*35))
  x=s*(2.27+rng.random()*2.15);z=.08+rng.random()*1.28
  obj=box("biological_patina",(x,-.668-rng.random()*.025,z),(.07+rng.random()*.18,.035,.06+rng.random()*.15),base.moss,.015)
 for j in range(8):
  rng=random.Random(900+j+int(s*35))
  x=s*(2.2+rng.random()*2.1);z=4.55+rng.random()*1.54
  box("smoke_stain",(x,-.655,z),(.12+rng.random()*.2,.025,.15+rng.random()*.15),patina,.014)
def setup(scale=23,target=(0,0,3.4)):
 base.scene(target,scale)
def save(name,scale=23,target=(0,0,3.4)):
 setup(scale,target)
 bpy.context.scene.render.filepath=str(OUT/(name+".png"))
 bpy.ops.render.render(write_still=True)
save("gate_reference_hero")
save("gate_reference_mobile",37,(0,0,3.3))
# Save editable source before packing geometry.
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"valoria_gate_reference.blend"))
# Blender GLB writer (without modifying the original six module pipeline)
objects=[o for o in bpy.data.objects if o.type=="MESH"]
for ob in objects:
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 for mod in list(ob.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
batches={}
for ob in objects:batches.setdefault(ob.data.materials[0].name,[]).append(ob)
for grouping in batches.values():
 if len(grouping)<2:continue
 bpy.ops.object.select_all(action='DESELECT')
 for ob in grouping:ob.select_set(True)
 bpy.context.view_layer.objects.active=grouping[0];bpy.ops.object.join()
objects=[o for o in bpy.data.objects if o.type=="MESH"]
bpy.ops.object.select_all(action='DESELECT')
for ob in objects:ob.select_set(True)
bpy.context.view_layer.objects.active=objects[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/"valoria_gate_reference.glb"),export_format='GLB',use_selection=True)
data={"triangles":sum(len(p.vertices)-2 for o in objects for p in o.data.polygons),"mesh_count":len(objects),"materials":len({m.name for o in objects for m in o.data.materials}),"glb_bytes":(OUT/"valoria_gate_reference.glb").stat().st_size,"gate_joint_centers_x":[-4.5,4.5],"visual_certification":"requires review","unity_mobile_tested":False}
(OUT/"gate_reference_manifest.json").write_text(json.dumps(data,indent=2))
print("ELDORIA_GATE_REFERENCE_PASS",data)
