"""Valoria V3-compatible family. Reuses existing geometric studio functions, does not regenerate gate."""
import bpy,sys,os,math,json,random,runpy
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/"art-source/valoria/production/first-asset-family-v1"
OUT.mkdir(parents=True,exist_ok=True)
GATE=ROOT/"art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.blend"
assert GATE.exists(),"Frozen hero V3 source missing"
# Reuse the established fortification authoring functions without its historical auto-build.
src=(ROOT/"tools/art-rd/valoria_fortification_family.py").read_text().split("\nmanifest=[]")[0]
ns={"__file__":str(ROOT/"tools/art-rd/valoria_fortification_family.py")}
exec(compile(src,"existing_valoria_studio","exec"),ns)
# Import existing V3 materials without touching the source artwork.
studio_material_names={m.name for m in bpy.data.materials}
with bpy.data.libraries.load(str(GATE),link=False) as (s,d):
 d.materials=list(s.materials)
available=[m for m in bpy.data.materials if m.use_nodes and m.name not in studio_material_names]
stone_candidates=[m for m in available if any(t in m.name.lower() for t in ("stone","ashlar","limestone","mortar"))]
assert len(stone_candidates)>=6, "Hero V3 material transfer failed; never fall back to flat studio colors"
if stone_candidates:
 for i in range(len(ns["stone"])): ns["stone"][i]=stone_candidates[i%len(stone_candidates)]
 ns["trim"]=stone_candidates[0]
random.seed(116)
def objset():return [o for o in bpy.context.scene.objects if o.type=="MESH"]
def reset():
 bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
def platform():
 # The earlier uniform checkerboard and rectangular bridge were rejected.
 # Author irregular worn paving that follows a broken perimeter and curved route.
 rng=random.Random(20261011)
 def stone_mesh(name,x,y,z,rx,ry,seed,material):
  local=random.Random(seed)
  count=8
  angles=[math.tau*k/count for k in range(count)]
  radii=[.78+local.uniform(-.09,.12) for _ in range(count)]
  points=[(rx*radii[k]*math.cos(t),ry*radii[k]*math.sin(t)) for k,t in enumerate(angles)]
  # Organic eight-sided paving stone, with deliberate bevel and UV mapping.
  h=.16+local.random()*.06
  v=[(px,py,-h/2) for px,py in points]+[(px,py,h/2) for px,py in points]
  faces=[tuple(reversed(range(count))),tuple(range(count,2*count))]
  for k in range(count):
   n=(k+1)%count;faces.append((k,n,n+count,k+count))
  mesh=bpy.data.meshes.new(name+'_hand_dressed')
  mesh.from_pydata(v,[],faces);mesh.update()
  mesh.materials.append(material)
  layer=mesh.uv_layers.new(name='UVMap')
  for poly in mesh.polygons:
   for li in poly.loop_indices:
    p=mesh.vertices[mesh.loops[li].vertex_index].co
    layer.data[li].uv=((p.x+x)*.3,(p.y+y)*.3)
  ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
  ob.location=(x,y,z)
  ob.rotation_euler.z=local.uniform(-.15,.15)
  mod=ob.modifiers.new('worn hand-cut paving arrises','BEVEL')
  mod.width=.036;mod.segments=2
  ob.modifiers.new('stone face weighted normals','WEIGHTED_NORMAL')
  return ob
 # An irregular tapering embankment replaces the square platform slab.
 n=32;edge=[];verts=[]
 for i in range(n):
  a=math.tau*i/n
  wobble=1+.04*math.sin(5*a)+.027*math.sin(11*a+.6)
  edge.append((4.6*math.cos(a)*wobble,3.8*math.sin(a)*wobble))
 for level,height,shrink in [(0,-.63,1.03),(1,-.08,1)]:
  for x,y in edge:verts.append((x*shrink,y*shrink,height+.045*math.sin(x*.9+y*.5)))
 faces=[];n=n
 for i in range(n):
  nxt=(i+1)%n;faces.append((i,nxt,n+nxt,n+i))
 faces.append(tuple(range(n,2*n)))
 mesh=bpy.data.meshes.new('irregular_embankment');mesh.from_pydata(verts,[],faces);mesh.update()
 mesh.materials.append(ns['stone'][5])
 foundation=bpy.data.objects.new('buried_stone_embankment',mesh)
 bpy.context.collection.objects.link(foundation)
 # Mismatched laid stones with staggered joints, no obvious tile grid.
 count=0
 for j in range(7):
  cy=-2.9+j*.91
  for i in range(9):
   cx=-3.7+i*.94+(.39 if j%2 else 0)+rng.uniform(-.17,.17)
   yy=cy+rng.uniform(-.14,.14)
   if (cx/4.1)**2+(yy/3.35)**2>1:continue
   count+=1
   stone_mesh(f'irregular_flagstone_{count}',cx,yy,.012+rng.uniform(-.022,.025),
    .60+rng.uniform(-.06,.15),.56+rng.uniform(-.10,.12),
    2000+count,ns['stone'][(i*3+j)%len(ns['stone'])])
 # A winding stone trail follows the terrain without raised rectangular stairs.
 for row in range(13):
  cy=3.2+row*.58
  center=.62*math.sin(row*.25)
  for lane in range(5):
   xx=center+(lane-2)*.67+rng.uniform(-.11,.11)
   stone_mesh(f'curving_path_flagstone_{row}_{lane}',xx,cy,
    -.045-row*.035,.42+rng.uniform(-.04,.09),.39+rng.uniform(-.05,.08),
    10000+row*11+lane,ns['stone'][(row+lane*2)%len(ns['stone'])])

def rocks():
 # A SINGLE CONTINUOUS, sculpted geological cliff shell; no floating slab,
 # repeated cones or detached stone pillars. Shared top rim and cut face vertices.
 rng=random.Random(441)
 nx,ny=31,27
 verts=[]
 def crown(x,y):
  return .22*math.sin(x*.59)+.15*math.cos(y*.66)+.09*math.sin(1.2*x+.73*y)
 # Irregular exposed cliff perimeter, following the same undulating plateau.
 perimeter=[]
 n=112
 for k in range(n):
  a=math.tau*k/n
  # ellipse radial outline with multi-octave geological variation
  t=1+.07*math.sin(5*a+.9)+.04*math.cos(11*a)+.017*math.sin(23*a)
  x=4.6*math.cos(a)*t;y=3.9*math.sin(a)*t
  perimeter.append((x,y,crown(x,y)))
 rings=8
 # Stratified cliff face: shared boundaries are watertight and slant inwards.
 for j in range(rings+1):
  u=j/rings
  for k,(x,y,top) in enumerate(perimeter):
   a=math.tau*k/n
   vertical=-4.8*u
   geological=.19*math.sin(a*9+u*4)+.13*math.sin(a*21-u*7)
   inset=.45*u+.12*math.sin(a*7+u*11)*math.sin(math.pi*u)
   verts.append((x*(1-inset/6)+geological*math.cos(a),y*(1-inset/6)+geological*math.sin(a),top+vertical))
 faces=[];materials=[]
 for j in range(rings):
  for k in range(n):
   nxt=(k+1)%n
   v0=j*n+k;v1=j*n+nxt;v2=(j+1)*n+nxt;v3=(j+1)*n+k
   faces.append((v0,v1,v2,v3));materials.append((j//2+k//11)%8)
 # Closed sculpted crown and cliff share the SAME perimeter vertices.
 # Concentric radial rings ensure every face meets the exposed cliff edge.
 prior=0
 for layer,ratio in enumerate((.78,.55,.32,.12)):
  offset=len(verts)
  for k,(x,y,top) in enumerate(perimeter):
   x2=x*ratio;y2=y*ratio
   z=crown(x2,y2)+.09*math.sin(k*.39+layer*.72)
   verts.append((x2,y2,z))
   faces.append((prior+k,prior+(k+1)%n,offset+(k+1)%n,offset+k))
   materials.append((k//11+layer//2)%8)
  prior=offset
 center_index=len(verts);verts.append((0,0,crown(0,0)))
 for k in range(n):
  faces.append((prior+k,prior+(k+1)%n,center_index))
  materials.append((k//14)%8)
 mesh=bpy.data.meshes.new('sculpted_geological_continuum')
 mesh.from_pydata(verts,[],faces);mesh.update()
 ob=bpy.data.objects.new('Sculpted continuous bedrock face and irregular crown',mesh)
 bpy.context.collection.objects.link(ob)
 for material in ns['stone']:mesh.materials.append(material)
 for poly,index in zip(mesh.polygons,materials):poly.material_index=index;poly.use_smooth=True
 bevel=ob.modifiers.new('Subtle cliff facet weathering','BEVEL');bevel.width=.035;bevel.segments=2
 ob.modifiers.new('Weighted geological shading','WEIGHTED_NORMAL')
 # Integrated talus at the foot, only partially visible and buried into cliff.
 for i in range(19):
  a=math.tau*i/19+.06*math.sin(i*2)
  x=4.18*math.cos(a);y=3.55*math.sin(a)
  size=.48+(i*7%9)*.075
  o=ns['frustum'](f'embedded_talus_{i}',(x,y,-4.35),size*1.2,size*.57,size,ns['stone'][i%8],7)
  o.rotation_euler=(.10*math.sin(i),.15*math.cos(i),a)
 # Preserve the recovered Eldoria CC0 artistic rock donor, including its real
 # scanned topology and albedo/normal, rather than inventing a substitute.
 import importlib.util
 cc0_file=ROOT/'tools/art-rd/valoria_family_cc0_scanned_cliff.py'
 spec=importlib.util.spec_from_file_location('eldoria_scanned_rock',str(cc0_file))
 cc0=importlib.util.module_from_spec(spec);spec.loader.exec_module(cc0)
 cc0.dress_continuous_cliff_with_cc0()

def fittings():
 for j in range(2):
  x=(-2.5,2.5)[j]
  ns["cube"](f"torch_iron_bracket_{j}",(x,-.6,2.2),(.17,.6,.16),ns["iron"],.02)
  ns["cube"](f"torch_brazier_{j}",(x,-.92,2.42),(.4,.38,.3),ns["iron"],.04)
  ns["frustum"](f"torch_flame_{j}",(x,-.92,2.8),.19,.03,.7,ns["mat"]("torch_amber_%s"%j,(.92,.37,.055),.4),9)
 for s in (-1,1):
  x=3*s
  ns["cube"]("banner_pole",(x,0,3.25),(.12,.12,3.2),ns["wood"][0],.025)
  ns["cube"]("banner_cloth",(x+s*.43,-.06,3.35),(.85,.08,1.65),ns["mat"]("faded_oxblood_%s"%s,(.32,.055,.045)),.025)
 for i in range(6):
  ns["cube"]("defense_stake",(i*.82-2.1,1.1,1.0),(.18,.18,2),ns["wood"][i%3],.06)
def source_hero_tower(x=0,y=0):
 # Preserve the artistic quality of the frozen V3 masonry: extract the
 # architecture's existing right tower as a standalone compatible module.
 with bpy.data.libraries.load(str(GATE),link=False) as (src,dst):
  assert 'VALORIA_HERO_GATE' in src.collections
  dst.collections=['VALORIA_HERO_GATE']
 coll=dst.collections[0]
 # Different V3 cohorts use Tower front/return/crown/merlon naming.
 right=[o for o in coll.objects if o.type=='MESH' and
        o.name.startswith(('Tower ','Tower_')) and o.matrix_world.translation.x>0.5]
 assert len(right)>=30,'Frozen V3 has too few right-tower meshes; stop, never substitute primitive'
 verts=[o.matrix_world@v.co for o in right for v in o.data.vertices]
 assert verts,'Hero V3 tower extraction returned no vertices'
 xmin=min(v.x for v in verts);xmax=max(v.x for v in verts)
 ymin=min(v.y for v in verts);ymax=max(v.y for v in verts)
 centerx=(xmin+xmax)*.5;centery=(ymin+ymax)*.5
 for original in right:
  part=original.copy()
  part.data=original.data.copy()
  bpy.context.scene.collection.objects.link(part)
  part.name='Hero V3 extracted watchtower - '+original.name
  part.matrix_world=original.matrix_world.copy()
  part.matrix_world.translation.x+=x-centerx
  part.matrix_world.translation.y+=y-centery
 return len(right)

def connections():
 ns["wall"]("socket_short",0,0,4.2,False)
 for s in (-1,1):
  ns["cube"]("socket_wedge",(s*2.18,0,2.1),(.55,1.55,4.25),ns["stone"][3],.06)
  ns["cube"]("socket_corbel",(s*2.18,0,4.26),(.76,1.77,.25),ns["trim"],.06)
def corner():
 ns["wall"]("corner_A",-2.5,0,5,False)
 # Keep corner second wall perpendicular and connected at 90 degrees.
 existing=set(bpy.data.objects)
 ns["wall"]("corner_B",2.5,0,5,False)
 for o in set(bpy.data.objects)-existing:
  o.location.x,o.location.y=0.0-o.location.y,o.location.x-2.5
  o.rotation_euler.z+=math.pi/2
def setup_camera(target=(0,0,2),scale=17,strategic=False):
 sc=bpy.context.scene;sc.render.engine="CYCLES";sc.cycles.samples=20;sc.cycles.use_denoising=False;sc.view_layers[0].cycles.use_denoising=False
 sc.render.resolution_x=960;sc.render.resolution_y=640
 sc.render.resolution_percentage=100
 sc.render.image_settings.file_format="PNG"
 sc.render.film_transparent=True
 sc.world=bpy.data.worlds.new("ValoriaStudioLight");sc.world.use_nodes=True
 sc.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.35,.38,.43,1)
 lamp=bpy.data.lights.new("studio_key","AREA");lo=bpy.data.objects.new("studio_key",lamp);sc.collection.objects.link(lo)
 lo.location=(-7,-10,15);lamp.energy=2700;lamp.size=8
 cam=bpy.data.cameras.new("matched_ortho");co=bpy.data.objects.new("matched_ortho",cam);sc.collection.objects.link(co)
 co.location=(16,-23,18) if strategic else (13,-19,13)
 co.rotation_euler=(Vector(target)-co.location).to_track_quat("-Z","Y").to_euler()
 cam.type="ORTHO";cam.ortho_scale=scale;sc.camera=co
def export(name,phase):
 folder=OUT/name;folder.mkdir(parents=True,exist_ok=True)
 sc=bpy.context.scene;sc.render.filepath=str(folder/(phase+"-hero.png"));bpy.ops.render.render(write_still=True)
 sc.camera.location=(16,-23,18)
 sc.camera.rotation_euler=(Vector((0,0,2.5))-sc.camera.location).to_track_quat("-Z","Y").to_euler()
 sc.camera.data.ortho_scale=55 if name=="assembled_family" else 24
 sc.render.filepath=str(folder/(phase+"-strategic.png"));bpy.ops.render.render(write_still=True)
 if phase=="after":
  bpy.ops.wm.save_as_mainfile(filepath=str(folder/(name+".blend")))
  meshes=objset();bpy.ops.object.select_all(action="DESELECT")
  for ob in meshes: ob.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  bpy.ops.export_scene.gltf(filepath=str(folder/(name+".glb")),export_format="GLB",use_selection=True,export_materials="EXPORT")
 return {"objects":len(objset()),"phase":phase}
kinds={"wall_straight":lambda:ns["wall"]("valoria_wall",length=8),
"watchtower":source_hero_tower,
"wall_corner":corner,
"wall_connections":connections,
"stone_path_platform":platform,
"cliff_rocks":rocks,
"defensive_props":fittings}
def assemble():
 # The full architectural family sits on the REAL continuous cliff module.
 rocks()
 for item in objset():
  item.location.x*=2.9;item.location.y*=2.6
  item.scale.x*=2.9;item.scale.y*=2.6
 ns["wall"]("west",-9,0,7,False);ns["wall"]("east",9,0,7,False)
 source_hero_tower(-13,0);platform();fittings()
 # Use the FROZEN, existing Hero V3 GLB; never regenerate its architecture.
 hero=ROOT/"art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb"
 assert hero.exists(), "Frozen hero V3 GLB is required for real assembly"
 bpy.ops.import_scene.gltf(filepath=str(hero))
 for ob in bpy.context.selected_objects: ob.name="FROZEN_V3_"+ob.name
# The agent must emit machine-readable changes. Never fabricate agent authorship.
decision_file=OUT/"agent-decisions.json"
if len(sys.argv)>sys.argv.index("--")+1 if "--" in sys.argv else False:
 pass
phase=os.environ.get("ELDORIA_FAMILY_PHASE","before")
decisions=json.loads(decision_file.read_text()) if phase=="after" and decision_file.exists() else {}
manifest={"baseline":"unchanged hero V3; reuse source materials","phase":phase,"assets":{},"agent_decisions":decisions}
for name,create in list(kinds.items())+[("assembled_family",assemble)]:
 reset();create();setup_camera(scale=47 if name=="assembled_family" else 18)
 if phase=="after":
  tweak=decisions.get(name,{})
  # Qwen-chosen changes to geometry and stone layout, not just image scoring.
  variation=float(tweak.get("masonry_irregularity",.025))
  variation=max(.008,min(.07,variation))
  for idx,ob in enumerate(objset()):
   if any(z in ob.name.lower() for z in ("stone","ashlar","stratum","flagstone","buttress")):
    rng=random.Random(1000+idx)
    ob.location.z+=rng.uniform(-variation,variation)
    if idx%5==0: ob.rotation_euler.z+=rng.uniform(-variation*.6,variation*.6)
  manifest["assets"][name]=export(name,phase)
 else:manifest["assets"][name]=export(name,phase)
(OUT/("manifest-"+phase+".json")).write_text(json.dumps(manifest,indent=2))
print("ELDORIA_FAMILY_"+phase.upper()+"_EXPORTED",len(manifest["assets"]))
