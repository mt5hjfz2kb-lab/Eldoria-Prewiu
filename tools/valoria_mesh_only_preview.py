"""Valoria I mesh-only world-space architectural continuity experiment.
Blender 2.83+ headless compatible, original committed GLB geometry + independent terrain.
Never imports SHARP, splats, canonical projection images or private textures.
"""
import bpy, math, os, json, base64, random
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
OUT=Path(os.environ.get("VALORIA_PROOF_OUTPUT","/tmp/valoria-mesh-proof.png"))
OUT.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=20
for layer in scene.view_layers: layer.cycles.use_denoising=False
scene.render.resolution_x=1152
scene.render.resolution_y=768
scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
scene.render.filepath=str(OUT)
scene.render.film_transparent=False

def material(name,color,rough=.85):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*color,1)
    bs.inputs["Roughness"].default_value=rough
    return m
stone=material("Warm weathered structural stone",(.36,.32,.27))
trim=material("Masonry copings",(.48,.42,.34))
paving=material("Worn road paving",(.38,.345,.29))
earth=material("Earth of the plateau",(.22,.225,.16))
grass=material("Grass",(.155,.22,.13))
rock=material("Dark slate bedrock",(.17,.185,.19))
wood=material("Timber",(.20,.115,.06))
blue=material("Blue Valoria flag",(.025,.09,.24))
glow=material("Warm amber",(.75,.35,.075))
def cuboid(name,xyz,dimensions,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz)
    o=bpy.context.object;o.name=name;o.dimensions=dimensions
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        m=o.modifiers.new("bevel","BEVEL");m.width=bevel;m.segments=2
        try:bpy.ops.object.modifier_apply(modifier=m.name)
        except Exception:pass
    o.data.materials.append(mat)
    return o

# Continuous stepped upper and lower courtyards are one walkable connected rock substrate.
# Global frame: z up, main bridge / gate / road / stair / keep advance along increasing Y.
# Lower terrace z=8, upper terrace z=13.
def plateau_mesh():
    n=44; random.seed(18)
    rings=[]
    for z,rx,ry,cx,cy in [
        (-7,32,46,0,3), (1.0,27.8,42,0,3), (7.6,25.6,39.3,0,3),
        (8.0,25.1,38.9,0,3)]:
        v=[]
        for i in range(n):
            theta=2*math.pi*i/n
            jitter=1+.055*math.sin(5*theta+1.4)+.025*math.sin(11*theta)
            v.append((cx+rx*jitter*math.cos(theta),cy+ry*jitter*math.sin(theta),z))
        rings.append(v)
    verts=[v for ring in rings for v in ring]
    faces=[]
    for r in range(len(rings)-1):
        for i in range(n):faces.append((r*n+i,r*n+(i+1)%n,(r+1)*n+(i+1)%n,(r+1)*n+i))
    faces.append(tuple((len(rings)-1)*n+i for i in range(n)))
    me=bpy.data.meshes.new("continuous bedrock mesh")
    me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new("One continuous rocky plateau",me)
    scene.collection.objects.link(ob)
    ob.data.materials.append(rock)
    return ob
plateau_mesh()
cuboid("Lower enclosed courtyard",(0,-5.0,7.96),(44,54,.16),grass,.15)
cuboid("Upper enclosed citadel terrace",(0,26.4,12.89),(42,22,.20),grass,.10)
# A continuous elevated shoulder makes the upper courtyard physically supported.
cuboid("Upper plateau bedrock", (0,27.3,10.4),(43,23,5),rock,.3)
cuboid("Upper terrace edge retaining wall left",(-21,17.6,10.6),(3,2,5.6),stone,.16)
cuboid("Upper terrace edge retaining wall right",(21,17.6,10.6),(3,2,5.6),stone,.16)
# Paving, physical connection, staircase transitioning z=8 to z=13.
cuboid("Approach from bridge to gate",(0,-28.0,8.08),(7.4,15,.14),paving)
cuboid("Gate to stair main street",(0,0,8.08),(7.4,32,.14),paving)
for i in range(15):
    y=14+i*.55
    z=8.13+i*(5/15)
    cuboid(f"Continuous staircase {i+1}",(0,y,z-.25),(7.4,.59,.55),paving,.025)
cuboid("Bastion court paved axis",(0,28.6,13.09),(8.5,22,.14),paving)
# Explicit connecting walls and ramparts, not free-standing model fragments.
for xx in (-22,22):
    cuboid("Curtain continuity west" if xx<0 else "Curtain continuity east",
           (xx,-.7,10.35),(2.6,50,5.3),stone,.1)
    cuboid("Wall cap west" if xx<0 else "Wall cap east",(xx,-.7,13.04),(3.1,50,.46),trim,.08)
    for yy in [-23,-17,-10,-3,4,11,18,24]:
        cuboid(f"Battlement x{xx} y{yy}",(xx,yy,13.58),(3.0,2.0,1.1),stone,.06)

# Existing accepted source geometry: import and normalize in a single world coordinate system.
# Read each mesh's actual axis-aligned world-space bounds and anchor its base to terrain.
FAMILIES=[
    ("Bridge","bridge","Bridge",(-0.2,-28,8.15),9.0),
    ("LowerGate","lower-gate","LowerGate",(0,-18.0,8.15),12.0),
    ("MainRoad","road","Road",(0,-1,8.20),9.0),
    ("CentralStair","stair","Stair",(0,18.0,12.0),8.0),
    ("UpperWalls","wall","Wall",(0,33.4,13.05),38.0),
    ("Bastion","bastion","Bastion",(0,32.5,13.05),19.0),
]
results=[]
for display,folder,source,anchor,target_width in FAMILIES:
    src=ROOT/"art-source"/"valoria"/"production"/(folder+"-family-v1")/(source+"FamilyV1.glb")
    if not src.exists():raise FileNotFoundError(str(src))
    before=set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(src))
    meshes=[o for o in bpy.data.objects if o not in before and o.type=="MESH"]
    if not meshes:raise RuntimeError("No mesh: "+display)
    # Place all source meshes under a shared transform. Keep their authored local relations.
    parent=bpy.data.objects.new(display+" WorldspaceRoot",None)
    scene.collection.objects.link(parent)
    for ob in meshes:
        matrix=ob.matrix_world.copy()
        ob.parent=parent; ob.matrix_world=matrix
    bpy.context.view_layer.update()
    coords=[ob.matrix_world@Vector(c) for ob in meshes for c in ob.bound_box]
    left=min(v.x for v in coords);right=max(v.x for v in coords)
    front=min(v.y for v in coords);back=max(v.y for v in coords)
    low=min(v.z for v in coords)
    scale=target_width/max(.001,right-left)
    parent.scale=(scale,scale,scale)
    # Translate actual model base and center into coherent global coordinates.
    parent.location=Vector((anchor[0]-scale*(left+right)*.5,
                            anchor[1]-scale*(front+back)*.5,
                            anchor[2]-scale*low))
    results.append({"family":display,"objects":len(meshes),"world_anchor":anchor,
                    "source_dimensions":[round(right-left,2),round(back-front,2)],
                    "scale":round(scale,4)})
# Small props and houses only within enclosed courtyard, away from route.
for side in [-1,1]:
    for i in range(4):
        x=side*(11+(i%2)*6);y=-11+(i//2)*13
        cuboid(f"Settlement cottage {side} {i}",(x,y,9.0),(5.0,5.2,2.2),wood,.16)
        cuboid(f"Slate pitched roof proxy {side} {i}",(x,y,10.2),(5.7,5.8,.48),rock,.18)
        cuboid(f"Storage near building {side} {i}",(x+1.6,y-2.8,8.45),(1.6,1.0,1),wood,.05)
# Architecturally anchored volume completion, independent of source GLB camera proxies.
# The underlying imported GLBs remain visible, but no longer define the structural topology.
def tower(name,x,y,z0,h,w=4.1):
    cuboid(name+" mass",(x,y,z0+h/2),(w,w,h),stone,.10)
    cuboid(name+" coping",(x,y,z0+h+.15),(w+.40,w+.40,.34),trim,.08)
    for ax,ay in [(0,w*.32),(0,-w*.32),(w*.32,0),(-w*.32,0)]:
        cuboid(name+" battlement",(x+ax,y+ay,z0+h+.62),
                (1.15,1.15,.88),stone,.045)
tower("Lower gate west tower",-6,-18,8.1,6.7,4.4)
tower("Lower gate east tower",6,-18,8.1,6.7,4.4)
# Gate frame with a real dark pass-through zone; never block main route visually.
cuboid("Lower gate lintel",(0,-18,14.25),(8.0,3.5,1.45),stone,.14)
for xx in (-3.95,3.95):
    cuboid("Lower arched gate side post",(xx,-18,11.0),(1.1,3.5,5.6),trim,.08)
cuboid("Lower gate shadow threshold",(0,-17.93,9.3),(7.2,.12,2.1),rock,.04)
tower("Upper stronghold keep",-1.5,34,13.05,10,8.5)
tower("Upper west rear guard",-15,35,13.05,7,4)
tower("Upper east rear guard",15,35,13.05,7.6,4)
# Continuous high curtain, with gap revealing dominant keep.
cuboid("Upper curtain left",(-11.5,37.3,16.1),(14,2.4,6.1),stone,.10)
cuboid("Upper curtain right",(11.5,37.3,16.1),(14,2.4,6.1),stone,.10)
cuboid("Upper courtyard command hall",(9,29,15.1),(11,8,4.0),stone,.14)
cuboid("Command hall slate cap",(9,29,17.2),(11.8,8.8,.36),rock,.12)
# Keep lower to upper transition must be clearly bounded by supporting retaining face.
for xx in (-17,-12,12,17):
    cuboid("Terrace buttress",(xx,16.55,10.85),(1.8,2.0,5.9),stone,.08)
# Timber details, exposed on the plausible service side; a sparse frontier settlement.
for x,y in [(-15,-6),(-11,7),(13,-4),(12,9)]:
    cuboid("Open work lean-to",(x,y,9.3),(4.7,3.1,.2),wood,.05)
    for ox in (-2,2):
        cuboid("Lean-to post",(x+ox,y-1.2,8.7),(.28,.28,1.55),wood,.02)
    cuboid("Stacked timber",(x+.6,y+.3,8.3),(2.0,1.15,.4),wood)
# Environmental silhouette: natural trees remain away from approach, not on streets.
bark=material("Dark forest bark",(.085,.065,.05))
pine=material("Cool fir needles",(.075,.14,.115))
for x,y,z,h in [(-28,-21,7.4,6),(-29,8,7.4,7.2),(29,6,7.4,6.5),
                 (27,31,7.5,8),(-26,34,7.4,7.8),(-31,37,3,8),(30,-27,2.5,8.5)]:
    cuboid("Fir trunk",(x,y,z+h*.27),(.48,.48,h*.55),bark)
    bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=h*.22,radius2=0,
                                    depth=h*.72,location=(x,y,z+h*.63))
    bpy.context.object.name="Fir canopy";bpy.context.object.data.materials.append(pine)
# Smooth road blocks and river threshold to secure one unified central route.
cuboid("Bridge lower abutment",(0,-35,6.7),(10.0,5.0,3.3),rock,.23)
cuboid("Bridge continuous walkway",(0,-29,8.15),(7.7,14,.23),paving,.08)
cuboid("Bridge gate connecting sill",(0,-20.5,8.12),(7.7,4,.21),paving,.03)
# Animated-look static light sources for proof only.
for i,y in enumerate((-19,-5,10,24,35)):
    for x in (-5,5):
        cuboid("Torch standard",(x,y,9 if y<12 else 14),(0.24,.24,2.1),wood)
        cuboid("Emissive torch cue",(x,y,10.1 if y<12 else 15.1),(.33,.33,.4),glow)
# Procedural mid-frequency PBR variation: avoid flat prototype color slabs.
for mat in [stone,trim,paving,earth,grass,rock,wood,pine]:
    nodes=mat.node_tree.nodes
    links=mat.node_tree.links
    bs=nodes.get("Principled BSDF")
    if not bs:continue
    base=tuple(bs.inputs["Base Color"].default_value[:3])
    noise=nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value=3.2 if mat in [rock,grass,earth] else 8.0
    noise.inputs["Detail"].default_value=3.0
    ramp=nodes.new("ShaderNodeValToRGB")
    colors=[tuple(max(0.001,c*.62) for c in base)+(1,),
            tuple(min(.98,c*1.36) for c in base)+(1,)]
    ramp.color_ramp.elements[0].color=colors[0]
    ramp.color_ramp.elements[1].color=colors[1]
    links.new(noise.outputs["Fac"],ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"],bs.inputs["Base Color"])
# Author a restrained emissive torch hue with a separate warm point light cluster.
torch_bs=glow.node_tree.nodes.get("Principled BSDF")
torch_bs.inputs["Emission"].default_value=(1,.30,.035,1)
if "Emission Strength" in torch_bs.inputs:torch_bs.inputs["Emission Strength"].default_value=2
for x,y,z in [(-6,-18,16),(6,-18,16),(0,24,17),(0,34,24)]:
    lamp=bpy.data.lights.new("Amber fire contrast","POINT")
    lamp.energy=420;lamp.color=(1,.42,.18)
    ob=bpy.data.objects.new("Amber fire contrast",lamp)
    scene.collection.objects.link(ob);ob.location=(x,y,z)
world=bpy.data.worlds.new("Cold forest dusk")
scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs["Color"].default_value=(.09,.12,.18,1)
world.node_tree.nodes["Background"].inputs["Strength"].default_value=2.3
ld=bpy.data.lights.new("Raking soft key","AREA");l=bpy.data.objects.new("Raking soft key",ld);scene.collection.objects.link(l)
l.location=(5,-20,62);ld.energy=13000;ld.size=25
# Explicit architectural composition camera, not SHARP camera-space placement.
cam_d=bpy.data.cameras.new("Valoria Worldspace Camera")
cam=bpy.data.objects.new("Valoria Worldspace Camera",cam_d)
scene.collection.objects.link(cam);scene.camera=cam
cam.location=(55,-82,78)
target=Vector((0,6,8))
cam.rotation_euler=(target-Vector(cam.location)).to_track_quat("-Z","Y").to_euler()
cam_d.type="ORTHO";cam_d.ortho_scale=102
# Export all visual mesh geometry in the same continuous world space for Unity import.
# Blender-specific noise shaders are not a Unity material certification.
asset_out=ROOT/"Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb"
asset_out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
for ob in bpy.data.objects:
    if ob.type=="MESH":ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(asset_out), export_format="GLB",
                          use_selection=True,export_apply=True,export_yup=True)
if not asset_out.exists() or asset_out.stat().st_size<10000:
    raise RuntimeError("Worldspace GLB export missing or invalid")
if asset_out.stat().st_size>22000000:
    raise RuntimeError("Worldspace GLB exceeds prototype repository size budget")
bpy.ops.render.render(write_still=True)
report={"result":"BLENDER WORLDSPACE VISUAL PROOF ONLY; UNITY NOT TESTED",
        "basis":"single scene coordinates, world-space mesh bases and a connected authored substrate",
        "no_sharp":True,"families":results}
OUT.with_suffix(".json").write_text(json.dumps(report,indent=2))
# Store direct-review thumbnail in branch as UTF-8 for independent visual inspection.
img=bpy.data.images.load(str(OUT));img.scale(540,360)
thumb=OUT.with_name("valoria-mesh-review.jpg")
img.filepath_raw=str(thumb);img.file_format="JPEG";img.save()
review=ROOT/"docs/evidence/valoria-mesh-only-prototype"
review.mkdir(parents=True,exist_ok=True)
(review/"preview.jpg.base64.txt").write_text(base64.b64encode(thumb.read_bytes()).decode("ascii"))
print("WORLDSPACE PROOF OUTPUT",str(OUT))
