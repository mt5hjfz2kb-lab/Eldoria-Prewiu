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
with bpy.data.libraries.load(str(GATE),link=False) as (s,d):
 d.materials=list(s.materials)
available=[m for m in bpy.data.materials if m.use_nodes]
stone_candidates=[m for m in available if any(t in m.name.lower() for t in ("stone","ashlar","limestone","mortar"))]
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
 rng=random.Random(441)
 for i in range(34):
  x=rng.uniform(-4.4,4.4);y=rng.uniform(-3.6,3.6)
  height=rng.uniform(.4,2.0)
  ob=ns["frustum"](f"cliff_stratum_{i}",(x,y,-1.4-height/2),rng.uniform(.48,.94),rng.uniform(.15,.55),height,ns["stone"][i%8],7)
  ob.rotation_euler=(rng.uniform(-.2,.2),rng.uniform(-.15,.15),rng.random()*6.28)
 ns["cube"]("cliff_top",(0,0,-.22),(8.5,7.3,.35),ns["stone"][5],.10)
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
 sc=bpy.context.scene;sc.render.engine="CYCLES";sc.cycles.samples=20
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
 sc.camera.data.ortho_scale=24
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
# The agent must emit machine-readable changes. Never fabricate agent authorship.
decision_file=OUT/"agent-decisions.json"
if len(sys.argv)>sys.argv.index("--")+1 if "--" in sys.argv else False:
 pass
phase=os.environ.get("ELDORIA_FAMILY_PHASE","before")
decisions=json.loads(decision_file.read_text()) if phase=="after" and decision_file.exists() else {}
manifest={"baseline":"unchanged hero V3; reuse source materials","phase":phase,"assets":{},"agent_decisions":decisions}
for name,create in list(kinds.items())+[("assembled_family",assemble)]:
 reset();create();setup_camera(scale=33 if name=="assembled_family" else 18)
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
