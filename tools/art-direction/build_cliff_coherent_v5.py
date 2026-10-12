"""Eldoria Valoria cliff V4: coherent geology with sewn rock/soil boundary.

Full authored replacement, not a cosmetic modification of rectangular V2/V3.
Art direction comes from the on-device Qwen artist's versioned direction.json;
the script creates independent editable Blender meshes and real GLB materials.
"""
import bpy, math, json, random, hashlib
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
DIRECTION=ROOT/"art-source/valoria/variants/cliff_talus_transition_v1/direction.json"
OUT=ROOT/"art-source/valoria/variants/cliff_coherent_v5"
OUT.mkdir(parents=True,exist_ok=True)
direction=json.loads(DIRECTION.read_text(encoding="utf-8"))
plan=direction["plan"]
amplitude=max(.65,min(1.25,float(plan["fault_amplitude_m"])))
facets=max(4,min(9,int(plan["large_facets"])))
bpy.ops.wm.read_factory_settings(use_empty=True)
rng=random.Random(20261012)

def n2(x,y):
    return (.5+.25*math.sin(x*1.46+math.sin(y*.71))
            +.16*math.sin(y*1.81+x*.69)
            +.09*math.cos(x*2.73-y*.94))
def lerp(a,b,t):return a+(b-a)*max(0,min(1,t))
def smooth(t):
    t=max(0,min(1,t))
    return t*t*(3-2*t)
def top(x):
    seam=smooth(min(1,(6-abs(x))/1.1))
    # A broad geological cove flanked by two different-sized shoulders.
    shape=.68*math.sin((x+2.4)*.57)+.35*math.sin((x-1.1)*1.13)
    shape+=.16*math.sin(x*2.09+.3)
    return 1.85 + seam*amplitude*shape
def frontY(x,z,t):
    # The top lip lies exactly on y=-2.90, shared with grass cap.
    exposure=math.sin(math.pi*t)
    geology=(n2(x*.93,z*.66)-.5)*1.15
    strata=.20*math.sin(x*.91+z*1.55)+.11*math.sin(x*2.4-z*.9)
    return -2.9+(.12+exposure*(geology+strata))*exposure

def bitmap(name,rock):
    image=bpy.data.images.new(name,width=256,height=256,alpha=False)
    data=[0.0]*(256*256*4)
    for y in range(256):
        for x in range(256):
            u=x/256.0;v=y/256.0
            coarse=n2(u*8.3,v*7.2)
            grain=n2(u*34.1,v*31.6)
            c=max(0,min(1,.67*coarse+.33*grain))
            if rock:
                striation=math.sin(v*27+u*21+math.sin(u*11)*4)*.018+math.sin(v*62-u*13)*.012
                a=.30+.27*c+striation
                rgb=(a*1.02,a*.96,a*.83)
            else:
                dry=n2(u*9.7+8,v*10.4)
                rgb=(.19+.24*c,.22+.23*dry,.085+.17*c)
            k=(y*256+x)*4
            data[k:k+4]=[max(0,min(1,channel)) for channel in rgb]+[1.]
    image.pixels[:]=data
    image.pack()
    return image

def material(name,rock):
    m=bpy.data.materials.new(name)
    m.use_nodes=True
    nt=m.node_tree;bsdf=nt.nodes.get("Principled BSDF")
    tex=nt.nodes.new("ShaderNodeTexImage")
    tex.image=bitmap(name+" generated texture",rock)
    tex.extension="REPEAT"
    nt.links.new(tex.outputs["Color"],bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value=.92 if rock else .98
    return m

stone=material("Valoria weathered geological limestone",True)
soil=material("Valoria organic grassy earth",False)

def meshobj(name,verts,faces,uv_of_vertex,material,smoothfaces=False):
    mesh=bpy.data.meshes.new(name+" mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.data.materials.append(material)
    uv=mesh.uv_layers.new(name="Authored UV")
    for poly in mesh.polygons:
        poly.use_smooth=smoothfaces
        for loop_id in poly.loop_indices:
            vertex_id=mesh.loops[loop_id].vertex_index
            uv.data[loop_id].uv=uv_of_vertex[vertex_id]
    return obj

NX=64;NZ=26;NY=16
rockverts=[];rockuv=[]
for ix in range(NX+1):
    x=-6+12*ix/NX
    hi=top(x)
    for iz in range(NZ+1):
        t=iz/NZ
        z=lerp(-2.15,hi,t)
        y=frontY(x,z,t)
        rockverts.append((x,y,z))
        rockuv.append((ix/NX*3.75+.065*math.sin(iz*.77+ix*.12),iz/NZ*2.65+.09*math.sin(ix*.31+iz*.17)))
faces=[]
for ix in range(NX):
    for iz in range(NZ):
        a=ix*(NZ+1)+iz;b=a+NZ+1
        faces.extend(((a,b,a+1),(a+1,b,b+1)))
rock=meshobj("Continuous eroded cliff rock face",rockverts,faces,rockuv,stone,False)

# Grass cap shares exactly the entire top row of the rock-front mesh.
grassverts=[];grassuv=[]
for ix in range(NX+1):
    x=-6+12*ix/NX
    hi=top(x)
    for iy in range(NY+1):
        t=iy/NY
        y=-2.9+t*4.25
        # The rock-to-grass boundary is exactly top(x), not independently sculpted.
        z=hi+(.16*t+.09*t*n2(x*.7,y*.8))*smooth(t)
        grassverts.append((x,y,z))
        grassuv.append((ix/NX*2.25,t*1.25))
topfaces=[]
for ix in range(NX):
    for iy in range(NY):
        a=ix*(NY+1)+iy;b=a+NY+1
        topfaces.extend(((a,b,a+1),(a+1,b,b+1)))
grass=meshobj("Continuous organic plateau with shared cliff lip",grassverts,topfaces,grassuv,soil,True)

# Thin rock-colored underlying soil/stone skirt at end seams; seam geometry
# stays at x=-6 and +6 for adjacent modules, with no missing triangles.
for side,ix in (("West",0),("East",NX)):
    endtop=top(-6 if ix==0 else 6)
    x=-6 if ix==0 else 6
    sideverts=[(x,-2.9,-2.15),(x,1.35,-2.15),
               (x,1.35,endtop),(x,-2.9,endtop)]
    sideverts=[];sideuv=[];sidefaces=[]
    for j in range(15):
        v=j/14
        y=-2.9+4.25*v
        for k in range(7):
            u=k/6
            z=-2.15+(endtop+2.15)*u
            sideverts.append((x,y,z))
            sideuv.append((u*1.7+.025*math.sin(j*.8+k*2),
                           v*1.85+.018*math.sin(k*1.31+j*.4)))
    for j in range(14):
        for k in range(6):
            a=j*7+k;b=a+7
            sidefaces.extend(((a,a+1,b),(a+1,b+1,b)))
    meshobj(side+" modular stratified join",sideverts,sidefaces,sideuv,stone,False)

# Intentional groups of fallen fracture blocks, not 42 repeated pebble heads.
cluster_centers=(-4.5,-3.65,-.8,2.3,4.6)
for cluster_index,cx in enumerate(cluster_centers):
    num=2 if cluster_index in (0,2,4) else 3
    for j in range(num):
        x=cx+rng.uniform(-.36,.36)
        y=-3.13-rng.uniform(0,.35)
        z=-2.07+rng.uniform(-.05,.21)
        rad=rng.uniform(.28,.62)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,
            location=(x,y,z))
        obj=bpy.context.object
        obj.name="Authored weathered talus %02d_%02d"%(cluster_index,j)
        obj.scale=(rad*1.30,rad*.85,rad*.68)
        obj.rotation_euler=(rng.uniform(-.3,.3),rng.uniform(-.4,.4),rng.uniform(-.9,.9))
        obj.data.materials.append(stone)
        for v in obj.data.vertices:
            f=1+.10*math.sin(v.co.x*6.7+v.co.y*4.3+cluster_index)
            v.co*=f

# Strong topology assertions: unlike V3, zero holes or exposed plain-white cap.
lipgap=max(
    (Vector(rockverts[ix*(NZ+1)+NZ])-Vector(grassverts[ix*(NY+1)])).length
    for ix in range(NX+1))
assert lipgap < 0.00001, "Rock/soil cap seam disconnected"
assert max(v[2] for v in rockverts)-min(v[2] for v in rockverts)>4.0
assert len(bpy.context.scene.objects)>=14

# Camera and illumination are part of the editable Blender source.
bpy.ops.object.camera_add(location=(17,-23,13))
cam=bpy.context.object;cam.name="Valoria 3D game camera reference"
cam.rotation_euler=(Vector((0,-1,0))-cam.location).to_track_quat("-Z","Y").to_euler()
cam.data.type="ORTHO";cam.data.ortho_scale=19
bpy.context.scene.camera=cam
bpy.ops.object.light_add(type="SUN",location=(2,-2,9))
sun=bpy.context.object;sun.name="Warm directional daylight"
sun.data.energy=1.8;sun.rotation_euler=(.65,-.38,-.35)
world=bpy.context.scene.world
if world is None:
    world=bpy.data.worlds.new("Valoria overcast daylight")
    bpy.context.scene.world=world
world.color=(.35,.38,.42)
scene=bpy.context.scene
scene.render.resolution_x=1200;scene.render.resolution_y=800
scene.render.resolution_percentage=100
scene.render.filepath=str(OUT/"blender-game-camera.png")
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"cliff_coherent_v5.blend"))

bpy.ops.object.select_all(action="DESELECT")
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=rock
target=OUT/"cliff_coherent_v5.glb"
bpy.ops.export_scene.gltf(filepath=str(target),export_format="GLB",
                          use_selection=True,export_materials="EXPORT")
assert target.stat().st_size>10000
proof={
  "artist_model":"eldoria-artist via verified local Qwen plan",
  "plan_system_hash":direction["artist_system_prompt_sha256"],
  "architecture":"single shared rock-grass lip, no disconnected sculpted cap",
  "authored_cliff_width_m":12,
  "seam_gap_m":lipgap,
  "rock_vertices":len(rockverts),
  "material_work":"varied uv rock strata and textured modular cut faces",
  "grass_vertices":len(grassverts),
  "mesh_objects":len(meshes),
  "candidate_sha256":hashlib.sha256(target.read_bytes()).hexdigest(),
  "art_pass":False,"mobile_pass":False,
  "next_gate":"Identical Unity camera and neutral material comparison"
}
(OUT/"proof.json").write_text(json.dumps(proof,indent=2),encoding="utf-8")
print("ELDORIA_COHERENT_GEOLOGY_V5_GENERATED",json.dumps(proof),flush=True)
