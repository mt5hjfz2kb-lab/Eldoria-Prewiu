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
 ns["cube"]("platform_base",(0,0,-.32),(8,7,.64),ns["mortar"],.16)
 for j in range(7):
  for i in range(8):
   x=-3.5+i+.08*((i*11+j*7)%3-1);y=-3+j+.04*(i%3)
   ns["cube"](f"flagstone_{i}_{j}",(x,y,.04),(.93,.91,.14),ns["stone"][(i*3+j)%8],.055)
 for j in range(10):
  ns["cube"](f"path_ramp_{j}",(0,3.5+j*.6,-.1-j*.035),(4.1,.55,.23),ns["stone"][(j*3)%8],.06)
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
"watchtower":lambda:ns["tower"]("valoria_watchtower"),
"wall_corner":corner,
"wall_connections":connections,
"stone_path_platform":platform,
"cliff_rocks":rocks,
"defensive_props":fittings}
def assemble():
 ns["wall"]("west",-9,0,7,False);ns["wall"]("east",9,0,7,False)
 ns["tower"]("watch",-14,0);platform();fittings()
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
