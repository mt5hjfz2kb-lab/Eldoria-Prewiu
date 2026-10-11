"""Focused gatehouse art pass; preserves existing six-piece family generator."""
import bpy, math, random, os, json, sys
from pathlib import Path
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).resolve().parent))
import valoria_fortification_family as base
# Prior v2 gate render has matching 960x640 camera, target and ortho scale.
OUT=Path(os.environ.get("ELDORIA_GATE_REFERENCE_OUTPUT",str(Path(__file__).resolve().parents[2]/"artifacts-local/gate-reference")))
OUT.mkdir(parents=True,exist_ok=True)
from shutil import copyfile
baseline=Path(os.environ.get('ELDORIA_FORTIFICATION_OUTPUT',''))/'gate.png'
if baseline.is_file():copyfile(str(baseline),str(OUT/'gate_before_same_camera.png'))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
random.seed(98)
# Existing architecture is retained; this stage supplements its legibility, wear and identity.
base.gate()
# Reuse owned Eldoria production textures; no third-party download or license ambiguity.
textures=Path(__file__).resolve().parents[2]/"Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures"
stone_albedo=textures/"stone_albedo.png"
stone_normal=textures/"stone_normal.png"
if stone_albedo.exists():
 image=bpy.data.images.load(str(stone_albedo),check_existing=True)
 norm_image=bpy.data.images.load(str(stone_normal),check_existing=True) if stone_normal.exists() else None
 if norm_image:norm_image.colorspace_settings.name="Non-Color"
 for material in list(base.stone)+[base.trim,base.mortar]:
  bs=material.node_tree.nodes.get("Principled BSDF")
  tex=material.node_tree.nodes.new("ShaderNodeTexImage");tex.image=image
  material.node_tree.links.new(tex.outputs["Color"],bs.inputs["Base Color"])
  if norm_image:
   nt=material.node_tree.nodes.new("ShaderNodeTexImage");nt.image=norm_image
   nm=material.node_tree.nodes.new("ShaderNodeNormalMap");nm.inputs["Strength"].default_value=.52
   material.node_tree.links.new(nt.outputs["Color"],nm.inputs["Color"])
   material.node_tree.links.new(nm.outputs["Normal"],bs.inputs["Normal"])
# Artistic convergence V2: restrained weathered masonry instead of regular
# curtain-wall grids; modify EXISTING facade stones rather than stacking cubes.
masonry_rng=random.Random(2142)
for ob in list(bpy.data.objects):
 if ob.type!="MESH" or "_irregular_stone" not in ob.name:continue
 if not ob.name.startswith("gate_pier_"):continue
 # Different joint widths and recess depth without separating structural backing.
 ob.location.x+=masonry_rng.uniform(-.045,.045)
 ob.location.z+=masonry_rng.uniform(-.037,.037)
 ob.location.y+=(-1 if ob.location.y<0 else 1)*masonry_rng.uniform(-.029,.032)
 ob.scale.x*=masonry_rng.uniform(.91,1.07)
 ob.scale.z*=masonry_rng.uniform(.91,1.06)
 ob.rotation_euler[1]+=masonry_rng.uniform(-.021,.021)
# Material differentiation and tiling adapted to existing UV coordinates.
rough_file=textures/"stone_smoothness.png"
if rough_file.exists():
 rough_image=bpy.data.images.load(str(rough_file),check_existing=True)
 rough_image.colorspace_settings.name="Non-Color"
 for m in list(base.stone)+[base.trim]:
  bs=m.node_tree.nodes.get("Principled BSDF")
  tex=m.node_tree.nodes.new("ShaderNodeTexImage");tex.image=rough_image
  inv=m.node_tree.nodes.new("ShaderNodeInvert")
  m.node_tree.links.new(tex.outputs["Color"],inv.inputs["Color"])
  m.node_tree.links.new(inv.outputs["Color"],bs.inputs["Roughness"])
# Existing in-project wood surface provides actual grain in Blender and GLB.
wood_path=Path(__file__).resolve().parents[2]/"Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/oak.png"
if wood_path.exists():
 wood_image=bpy.data.images.load(str(wood_path),check_existing=True)
 for mat in base.wood:
  bs=mat.node_tree.nodes.get("Principled BSDF")
  tex=mat.node_tree.nodes.new("ShaderNodeTexImage");tex.image=wood_image
  mat.node_tree.links.new(tex.outputs["Color"],bs.inputs["Base Color"])
# Dark Valoria slate provides a distinctive distant silhouette.
slate=base.mat("valoria_roof_dark_slate",(.115,.15,.18),.81)
slate_edge=base.mat("slate_ridge_edge",(.20,.24,.25),.86)
banner=base.mat("valoria_faded_burgundy",(.30,.085,.105),.92)
banner_gilt=base.mat("banner_worn_ochre",(.59,.36,.13),.74)
for sx in (-1,1):
 tx=sx*6.37
 # Steep roof remains recessed inside crenellation, with readable silhouette.
 roof=base.frustum("Valoria_pitched_watch_roof",(tx,0,8.16),1.86,.07,2.35,slate,12)
 base.frustum("slate_roof_eave",(tx,0,7.02),1.95,1.85,.16,slate_edge,24)
 boxp=base.cube("worn_roof_finial",(tx,0,9.42),(.16,.16,.46),base.iron,.04)
  # Nonuniform slate tiles follow pitched roof slope; silhouettes remain intact.
 for course in range(5):
  z=7.17+course*.40
  radius=1.81-(z-6.985)/2.35*1.79
  for j in range(12):
   angle=(j+.30*(course%2))*math.tau/12
   a=tx+radius*math.cos(angle);bb=radius*math.sin(angle)
   tile=base.cube("slate_lap_tile",(a,bb,z),(.235,.10,.065),slate_edge if (j+course)%5==0 else slate,.008)
   tile.rotation_euler[2]=angle
 # One unmistakable banner per defensive tower facing the camera.
 pole=base.cube("heraldic_banner_mast",(tx,-2.23,7.36),(.065,.075,3.2),base.iron,.014)
 cloth=base.cube("Valoria_long_pennant",(tx,-2.32,6.72),(.64,.06,1.41),banner,.025)
 base.cube("pennant_ochre_chevron",(tx,-2.38,6.62),(.39,.04,.15),banner_gilt,.015)
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
# FULL ART BRAIN FOCUS V2 — edit structural mesh, not decorative Lego.
# Controlled interventions: organic battered side masses, vaulted entry recession,
# mixed-height irregular parapet and unified roof contours.
def structural_mesh(name,verts,faces,material,bevel=.08):
 mesh=bpy.data.meshes.new(name+"_mesh")
 mesh.from_pydata(verts,[],faces);mesh.update()
 obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
 obj.data.materials.append(material)
 if bevel:
  m=obj.modifiers.new("architectural_edge_softening","BEVEL");m.width=bevel;m.segments=2
 obj.modifiers.new("weighted_normals","WEIGHTED_NORMAL")
 return obj

def battered_pier(side):
 # Six courses of a contiguous sloping profile, irregularized at large scale.
 xs=[(3.12,5.18),(3.08,5.10),(3.09,4.99),(3.04,4.91),
     (3.06,4.81),(3.03,4.71)]
 zs=[0,1.25,2.62,3.97,5.31,6.75]
 verts=[]
 for i,(inner,outer) in enumerate(xs):
  for x,y in ((side*inner,-1.67),(side*outer,-1.53),
              (side*outer,1.30),(side*inner,1.37)):
   verts.append((x,y,zs[i]))
 faces=[(3,2,1,0),(20,21,22,23)]
 for i in range(5):
  for j in range(4):
   a=i*4+j;b=i*4+(j+1)%4
   faces.append((a,b,b+4,a+4))
 return structural_mesh("organic_battered_pier_"+str(side),verts,faces,stone[2],.12)

for side in (-1,1):
 battered_pier(side)
 # Deep stone portal cheek returns into the passage, creating actual perspective.
 base.cube("deep_portal_cheek",(side*2.17,-.54,2.80),(.41,2.75,5.52),stone[4],.14)
 # Offset tower-side roof joins rather than a detachable series of boxes.
 base.frustum("tower_transition_hip",(side*5.01,.24,7.24),1.19,.48,1.18,slate,8)
 # Slender arrow recess on the main structural pier, not a black painted patch.
 base.cube("arrow_reveal",(side*3.81,-1.71,3.90),(.14,.11,1.12),patina,.025)
# Second nested radius: continuous layered arch conveys a defended tunnel.
for j in range(20):
 a0=math.pi*j/20;a1=math.pi*(j+1)/20-.004
 base.arch_segment("deep_arch_voussoir",0,-1.59,3.0,2.0,a0,a1,.38,stone[(j+2)%8],.13)
 base.arch_segment("inner_portal_recession",0,-.93,3.0,1.89,a0,a1,.33,trim,.09)
# The central gatehouse mass must read as one building, not stacked floating headers.
structural_mesh("continuous_gatehouse_roof",
 [(-4.55,-1.0,7.00),(4.55,-1.0,7.00),(4.55,1.55,7.00),(-4.55,1.55,7.00),
  (-4.16,-.68,8.20),(4.16,-.68,8.20),(4.16,1.21,8.20),(-4.16,1.21,8.20)],
 [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],stone[5],.11)
# One staggered defensive rhythm. Suppress the old identical merlons on central crown.
for ob in list(bpy.data.objects):
 if ob.name.startswith("gate_crown_merlon"):ob.hide_render=True
heights=[.96,1.08,.78,1.34,.92,1.16,.83]
widths=[.64,.75,.62,.84,.68,.76,.66]
for j,(height,width) in enumerate(zip(heights,widths)):
 x=-3.68+j*1.23
 structural_mesh("authored_variable_merlon_"+str(j),
 [(x-width/2,-1.50,8.04),(x+width/2,-1.50,8.04),
  (x+width/2,-.79,8.04),(x-width/2,-.79,8.04),
  (x-width*.43,-1.44,8.04+height),(x+width*.41,-1.44,8.04+height),
  (x+width*.41,-.86,8.04+height),(x-width*.43,-.86,8.04+height)],
 [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],
 stone[j%len(stone)],.055)
art_director_geometry={"baseline":"run_38136991434","operations":["contiguous_battered_piers","deep_nested_arched_entry","authored_nonuniform_parapet","continuous_roof_mass","tower_transitional_hips"],"artistic_status":"REVIEW_REQUIRED","no_additional_asset_families":True}

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
data={"art_director_geometry":art_director_geometry,"triangles":sum(len(p.vertices)-2 for o in objects for p in o.data.polygons),"mesh_count":len(objects),"materials":len({m.name for o in objects for m in o.data.materials}),"glb_bytes":(OUT/"valoria_gate_reference.glb").stat().st_size,"wall_to_tower_edge_contact_x":[-8.65,8.65],"visual_certification":"requires review","unity_mobile_tested":False}
(OUT/"gate_reference_manifest.json").write_text(json.dumps(data,indent=2))
# Render context after gate-only source/export has been written.
base.wall("west_attach",-12.8,0,8.3)
base.wall("east_attach",12.8,0,8.3)
save("gate_reference_assembled",48,(0,0,4.5))
print("ELDORIA_GATE_REFERENCE_PASS",data)
