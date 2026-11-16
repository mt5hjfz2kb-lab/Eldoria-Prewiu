#!/usr/bin/env python3
"""ELDORIA / VALORIA — independent coherent city finishing pass.
Input: authored source from tools/cc0-art-system/author_bastion.py.
Output: REAL editable .blend + five physical .glb levels + 4 genuine Blender PNGs.
Only owned geometry and authored textures. No third-party mesh is silently bundled.
A testable painterly-medieval alternative to the rejected direct-mixed-packs study.
"""
import bpy, math, random, json, os, struct
from mathutils import Vector
from pathlib import Path
from math import sin, cos, pi, radians
random.seed(16112026)
ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"Unity/Assets/Art_Reboot/CC0System/Generated/CC0_Valoria_Source.blend"
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/CityFinish"
PREV=ROOT/"docs/cc0-art-system/previews/city-finish"
DEST.mkdir(parents=True,exist_ok=True)
PREV.mkdir(parents=True,exist_ok=True)
if not SRC.exists():raise RuntimeError("First build authored source .blend via author_bastion.py")
bpy.ops.wm.open_mainfile(filepath=str(SRC))
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=23
scene.cycles.use_denoising=False
for layer in scene.view_layers: layer.cycles.use_denoising=False
scene.render.threads_mode="FIXED"
scene.render.threads=4
scene.render.image_settings.file_format="PNG"
scene.view_settings.view_transform="Filmic"
scene.view_settings.look="Medium High Contrast"
scene.view_settings.exposure=.16
scene.render.resolution_percentage=100

def collection(name):
    old=bpy.data.collections.get(name)
    if old:return old
    c=bpy.data.collections.new(name)
    scene.collection.children.link(c)
    return c
def material(code):
    names={"stone":"ED_OldAshlar","light":"ED_LimestoneEdge",
        "dark":"ED_DeepAshlar","rubble":"ED_RubbleStone",
        "roof":"ED_BlueSlate","wood":"ED_OldOak","charred":"ED_CharredOak",
        "earth":"ED_PackedTrail","meadow":"ED_GrassOlive",
        "leaf":"ED_Pines","leaf2":"ED_MossPines","gold":"ED_AgedBronze",
        "iron":"ED_ForgedIron","cloth":"ED_WineBanners",
        "amber":"ED_WarmWindow","violet":"ED_VeilViolet"}
    key=names[code]
    m=bpy.data.materials.get(key)
    if m is None:
        # Blender discards unreferenced source materials when writing .blend.
        # Create the missing master explicitly, preserving the same palette.
        palette={"ED_WineBanners":("#493548",.90),
                 "ED_RubbleStone":("#596369",.84),
                 "ED_GrassOlive":("#556750",.93),
                 "ED_MossPines":("#61735B",.94)}
        if key not in palette:
            raise RuntimeError("Missing source master material: "+key)
        h,rough=palette[key]
        col=tuple(int(h[i:i+2],16)/255 for i in (1,3,5))
        m=bpy.data.materials.new(key)
        m.use_nodes=True
        bs=m.node_tree.nodes.get("Principled BSDF")
        bs.inputs["Base Color"].default_value=(*col,1)
        bs.inputs["Roughness"].default_value=rough
        m.diffuse_color=(*col,1)
    return m
def darken_source_materials():
    """Baked reproducible texture changes, not Blender-only color grading."""
    profiles={
        "stone":(.79,.84,.88),
        "light":(.85,.88,.88),
        "dark":(.85,.89,.94),
        "roof":(1.03,1.03,1.02),
        "earth":(.95,.96,.94),
        "meadow":(1.03,1.05,1.03)
    }
    for key,fac in profiles.items():
        m=material(key)
        if not m.use_nodes:continue
        for node in m.node_tree.nodes:
            if node.type != 'TEX_IMAGE' or not node.image:continue
            src=node.image
            new=src.copy()
            new.name="ELDORIA_CityFinish_"+key
            data=list(src.pixels[:])
            for i in range(0,len(data),4):
                for t in range(3):
                    data[i+t]=min(1,max(0,data[i+t]*fac[t]))
            new.pixels[:]=data
            path=DEST/("ELDORIA_CityFinish_"+key+"_albedo.png")
            new.filepath_raw=str(path)
            new.file_format="PNG"
            new.save()
            node.image=new
            print("ELDORIA_BAKED_MASTER_TINT",key,str(path))
darken_source_materials()
def attach(obj,m,col):
    obj.data.materials.append(material(m))
    dest=collection(col)
    for c in list(obj.users_collection):c.objects.unlink(obj)
    dest.objects.link(obj)
    return obj
def block(name,p,size,m,col="CityDetail",rot=0,bevel=.02):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    o=bpy.context.object
    o.name=name
    o.dimensions=size
    o.rotation_euler[2]=rot
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    attach(o,m,col)
    if bevel:
        v=o.modifiers.new("light masonry bevel","BEVEL")
        v.width=bevel
        v.segments=1
        o.modifiers.new("weighted normal","WEIGHTED_NORMAL")
    return o
def cylinder(name,p,r,h,m,col="CityDetail",v=12,rotate=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=v,radius=r,depth=h,location=p)
    o=bpy.context.object;o.name=name
    if rotate:o.rotation_euler=rotate
    return attach(o,m,col)
def cone(name,p,r1,r2,h,m,col="CityDetail",v=10):
    bpy.ops.mesh.primitive_cone_add(vertices=v,radius1=r1,radius2=r2,depth=h,location=p)
    o=bpy.context.object;o.name=name
    return attach(o,m,col)
def sphere(name,p,r,m,col="CityDetail",segments=8,rings=5,scale=None):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=r,location=p)
    o=bpy.context.object;o.name=name
    if scale:o.scale=scale
    return attach(o,m,col)
def shifted_cottages():
    # Physical distance from medieval citadel: each district keeps original geometry.
    sources={
        "Timber workshop":((-14,-3.0),(-22,-7.5)),
        "Granary":((11,-2),(22,-9.2)),
        "Barracks lodge":((12,12),(23.8,11.4)),
        "Stone quarry":((-12,14),(-23.5,11.8))
    }
    deltas={k:(to[0]-fr[0],to[1]-fr[1]) for k,(fr,to) in sources.items()}
    t=collection("Town")
    for o in t.objects:
        if o.name.startswith(("Timber workshop","Granary","Barracks lodge","Stone quarry")):
            typ=next(k for k in sources if o.name.startswith(k))
            o.location.x+=deltas[typ][0]
            o.location.y+=deltas[typ][1]
        # The original misc ornaments were not bound to building positions;
        # hide duplicate crates / lamps and replace them individually.
        elif o.name.startswith(("sconce","torch","linen-covered supply")):
            o.hide_render=True
    return {k:to for k,(fr,to) in sources.items()}
sites=shifted_cottages()

# The initial proof road was an assembly of visible raised rectangular slabs:
# it looked like a black ladder in the camera. Replace in the source scene,
# not just hide from Blender, so Unity GLB can never reintroduce it.
old_route=bpy.data.collections.get("Route")
if old_route:
    for old_obj in list(old_route.objects):
        bpy.data.objects.remove(old_obj,do_unlink=True)

def terrain_height(x,y):
    # Exact sampling formula used by the authored terrain in author_bastion.py.
    return (.12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)
            -.05*max(0,1-abs(x)/3))

def road_soil_material():
    name="ED_CityFinish_RoadLoam"
    m=bpy.data.materials.get(name)
    if m:return m
    m=bpy.data.materials.new(name)
    m.diffuse_color=(.35,.30,.24,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(.35,.30,.24,1)
    bs.inputs["Roughness"].default_value=.98
    return m
ROAD_LOAM=road_soil_material()

def organic_road(name,points,width=2.4,n=30,stone_density=.5):
    """Continuous curved terrain-hugging loam, with sparse broken historical setts.

    A strip of contiguous triangles is legitimate terrain dressing (not a raised
    platform or a checkerboard) and works in imported GLB / URP. The same function
    creates the main entry road and the four narrow service footpaths.
    """
    P=[Vector(p) for p in points]
    assert len(P)==4
    verts=[]
    faces=[]
    uvraw=[]
    for i in range(n):
        t=i/(n-1)
        pos=((1-t)**3*P[0]+3*(1-t)**2*t*P[1]+3*(1-t)*t*t*P[2]+t**3*P[3])
        tan=(3*(1-t)**2*(P[1]-P[0])+6*(1-t)*t*(P[2]-P[1])+
             3*t*t*(P[3]-P[2]))
        if tan.length<.001:tan=Vector((0,1,0))
        tan.normalize()
        lateral=Vector((-tan.y,tan.x,0))
        # Organic narrowing, irregular banks rather than parallel tile edges.
        local_width=width*(1.0-.08*t+random.uniform(-.055,.055))
        for sign in (-1,1):
            p=pos+lateral*(sign*local_width*.5)
            z=terrain_height(p.x,p.y)+.047
            verts.append((p.x,p.y,z))
            uvraw.append((t*5, float(sign>0)))
        if i>0:
            off=2*i
            faces.append((off-2,off,off+1,off-1))
        if i%2==0 and random.random()<stone_density:
            # One or two free-form setts per metre, no rhythmic grid.
            for _ in range(random.choice((1,1,2))):
                p=pos+lateral*random.uniform(-local_width*.35,local_width*.35)
                z=terrain_height(p.x,p.y)+.096
                block(name+" | embedded broken flagstone",
                      (p.x,p.y,z),(.39+random.random()*.3,.37+random.random()*.36,.10),
                      random.choice(("stone","light","dark","earth")),"Roads",
                      rot=math.atan2(tan.y,tan.x)+random.uniform(-.27,.27),bevel=.025)
    mesh=bpy.data.meshes.new("GroundedRoadMesh_"+name)
    mesh.from_pydata(verts,[],faces)
    mesh.materials.append(ROAD_LOAM)
    mesh.update()
    road=bpy.data.objects.new("Grounded native track | "+name,mesh)
    collection("Roads").objects.link(road)
    uv=mesh.uv_layers.new(name="terrainRoadUV")
    for p in mesh.polygons:
        for idx in p.loop_indices:
            uv.data[idx].uv=uvraw[mesh.loops[idx].vertex_index]
    return road

organic_road("Bastion approach",
    [(-.2,-29,0),(.9,-22,0),(-2.2,-13,0),(0,-6.1,0)],
    width=3.65,n=52,stone_density=.88)
organic_road("Sawmill supply lane",
    [(-2.7,-17,0),(-10.0,-19,0),(-15.0,-16.0,0),(-22.0,-12.0,0)],
    width=1.85,n=25,stone_density=.43)
organic_road("Granary service lane",
    [(2.6,-18.0,0),(8.0,-18.0,0),(16,-14,0),(22.0,-11.6,0)],
    width=1.9,n=25,stone_density=.38)
organic_road("Quarry track",
    [(-18.0,-12.0,0),(-25.0,-7.0,0),(-28.0,2.0,0),(-23.0,8.0,0)],
    width=1.70,n=21,stone_density=.28)
organic_road("Barracks route",
    [(16.5,-12.0,0),(28.0,-5.0,0),(29.0,3.0,0),(24.0,7.0,0)],
    width=1.7,n=20,stone_density=.38)

def industrial_detail():
    # Sawmill x=-22, y=-7.5: blade, beams, stacked wood; no artificial footprint base.
    x,y=sites["Timber workshop"]
    for q in range(6):
        block("sawmill | harvested logs", (x-3.1+q*.37,y-4.2,.35+q*.03),
              (.28,2.7,.29),"wood","Craft",rot=.06)
    cylinder("sawmill | cast-iron saw wheel",(x+2.7,y-3.5,1.45),.72,.13,
             "iron","Craft",v=14,rotate=(radians(90),0,0))
    for i in range(9):
        a=2*pi*i/9
        block("sawmill | saw tooth", (x+2.7+cos(a)*.74,y-3.5,1.45+sin(a)*.74),
              (.18,.16,.18),"light","Craft",rot=a)
    block("sawmill | saw bench",(x+2.8,y-2.2,.78),(2.6,1.2,.27),"wood","Craft")
    block("sawmill | timber awning",(x+2.8,y-1.4,2.86),(3.4,1.6,.19),"wood","Craft")

    # Granary x=22, y=-9.2: two distinctive silos/standing grain bags.
    x,y=sites["Granary"]
    for i in range(2):
        # Place silos on the camera-facing east side, outside building walls.
        sx=x+4.45+i*2.15
        sy=y-.8
        cylinder("granary | round masonry silo",(sx,sy,2.05),1.0,3.9,"stone","GranaryDetail",12)
        cone("granary | charcoal conical lid",(sx,sy,4.73),1.3,.1,1.55,
             "roof","GranaryDetail",12)
        cylinder("granary | timber binding",(sx,sy,1.15),1.04,.15,"wood","GranaryDetail",12)
    for i in range(12):
        px=x+random.uniform(-3.8,1.8)
        py=y-4.0+random.uniform(-.5,1.2)
        sphere("granary | grain sack",(px,py,.34),.43,
               "earth","GranaryDetail",8,5,scale=(1.15,.75,.82))
    for row in range(5):
        for col in range(12):
            px=x+3.9+col*.19+random.uniform(-.06,.06)
            py=y-5.0+row*.4+random.uniform(-.06,.06)
            # Crops grow in an irregular patch, NOT in four square parcels.
            cone("granary | golden harvest spike",(px,py,.54),
                 .045,.0,1.07,"gold","GranaryDetail",5)

    # Barracks x=23.8, y=11.4: independent fortified watch and training.
    x,y=sites["Barracks lodge"]
    cylinder("barracks | forward watchtower",(x+4.7,y+1.9,3.28),1.65,6.2,
             "dark","BarracksDetail",12)
    cylinder("barracks | parapet cap",(x+4.7,y+1.9,6.5),1.84,.38,
             "light","BarracksDetail",12)
    for i in range(12):
        a=2*pi*i/12
        block("barracks | tooth crenel",(x+4.7+1.52*cos(a),y+1.9+1.52*sin(a),7.04),
              (.62,.53,.83),"stone","BarracksDetail",rot=a)
    for k in range(3):
        block("barracks | spear rack upright",(x-5.4+k*.41,y-3,1.68),
              (.1,.14,3.1),"wood","BarracksDetail")
        cone("barracks | steel pike point",(x-5.4+k*.41,y-3,3.27),
             .095,0,.48,"iron","BarracksDetail",6)
    block("barracks | banner pole",(x+4.65,y+1.9,8.45),(.13,.13,2.2),
          "wood","BarracksDetail")
    block("barracks | long purple pennon",(x+5.18,y+1.9,8.7),(.91,.12,1.38),
          "cloth","BarracksDetail")

    # Stone quarry x=-23.5 y=11.8: timber crane, broken rock and working face.
    x,y=sites["Stone quarry"]
    for i in range(22):
        a=2*pi*i/22
        rr=random.uniform(.7,3.9)
        px=x-4.2+rr*cos(a)
        py=y-3.4+rr*sin(a)
        cylinder("quarry | extracted uneven stone",(px,py,.24),random.uniform(.24,.78),
                 random.uniform(.28,.78),random.choice(["stone","dark","light"]),
                 "QuarryDetail",8)
    for s in (-1,1):
        block("quarry | timber hoist post",(x+2.6+s*1.8,y-3.8,2.7),
              (.30,.31,5.3),"wood","QuarryDetail")
    block("quarry | suspended oak crossbeam",(x+2.6,y-3.8,5.28),(4.5,.34,.31),
          "wood","QuarryDetail")
    cylinder("quarry | iron winch",(x+2.6,y-3.8,4.92),.49,.36,
             "iron","QuarryDetail",10,rotate=(radians(90),0,0))
    block("quarry | dangling lifting chain",(x+2.6,y-3.8,3.66),(.065,.065,2.23),
          "iron","QuarryDetail")
industrial_detail()

# Controlled environmental storytelling: scattered living nature, not empty synthetic platforms.
def make_tree(x,y,height=4.8):
    cylinder("deciduous | twisted trunk",(x,y,height*.22),height*.075,
             height*.44,"wood","LandscapeDetail",7)
    for dx,dy,dz,scale in [(-.4,.2,.65,.74),(.5,.1,.69,.65),(0,-.38,.84,.78)]:
        sphere("deciduous | stylized natural canopy",
               (x+dx*height*.27,y+dy*height*.27,height*dz),1,
               random.choice(["leaf","leaf2"]),"LandscapeDetail",9,6,
               scale=(height*.23*scale,height*.19*scale,height*.23*scale))
for i in range(38):
    x=random.uniform(-45,45);y=random.uniform(-40,40)
    if abs(x)<33 and -22<y<22:continue
    if x<-32 and -17<y<17:continue
    make_tree(x,y,random.uniform(3.9,8.2))
# Add small clumps to anchor building walls into the grass.
for pos in [(-27,-12),(-26,9),(18,-15),(29,8),(-12,-9),(12,-9)]:
    for j in range(11):
        x=pos[0]+random.uniform(-2.8,2.8)
        y=pos[1]+random.uniform(-1.3,2.1)
        cone("edge-of-path native meadow clump",(x,y,.19),
             random.uniform(.10,.23),0,random.uniform(.25,.58),
             random.choice(["leaf","leaf2","meadow"]),"LandscapeDetail",5)
# Heraldry/founder identity on rebuilt stronghold. Not cheesy extra buildings.
for s in (-1,1):
    x=s*3.7
    block("royal banner | forged dark bracket",(x,-5.08,5.05),
          (.17,.39,.20),"iron","Stage5Detail")
    block("royal banner | Valoria plum cloth",(x,-5.37,4.1),
          (.89,.12,1.68),"cloth","Stage5Detail")
    block("royal banner | bronze edge",(x,-5.45,3.31),
          (.91,.11,.08),"gold","Stage5Detail")

# soften harsh windows, late-day aura. Do not spend game runtime on volumetric fog.
window=material("amber")
if window.use_nodes:
    bs=window.node_tree.nodes.get("Principled BSDF")
    if bs:
        emission=bs.inputs.get("Emission Strength")
        if emission: emission.default_value=.95
scene.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.28,.34,.43,1)
scene.world.node_tree.nodes["Background"].inputs["Strength"].default_value=.69
for o in bpy.data.objects:
    if o.type=="LIGHT" and o.data.type=="SUN":
        o.data.energy=3.12
        o.data.color=(1.0,.73,.53)
        o.data.angle=radians(3.5)
# one common stage/camera per comparison
cam=scene.camera
cam.data.type="ORTHO"
def cam_at(location,target,scale):
    cam.location=Vector(location)
    cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
    cam.data.ortho_scale=scale
def show_stage(stage):
    for n in range(2,6):
        c=bpy.data.collections.get("Stage"+str(n))
        if c:c.hide_render=n>stage
    d=bpy.data.collections.get("Stage5Detail")
    if d:d.hide_render=stage<5
def shot(stage,name,w,h,location,target,scale):
    show_stage(stage)
    cam_at(location,target,scale)
    scene.render.resolution_x=w
    scene.render.resolution_y=h
    p=PREV/name
    scene.render.filepath=str(p)
    bpy.ops.render.render(write_still=True)
    raw=p.read_bytes()
    assert len(raw)>70000 and raw[:8]==b'\x89PNG\r\n\x1a\n',(name,len(raw))
    print("COHERENT_CITY_REAL_BLENDER_IMAGE",name,len(raw))

# Wide A/B, close hero and portrait. WIDE image tests separation and terrain.
skip=os.environ.get("ELDORIA_SKIP_FINISH_RENDER")=="1"
if not skip:
    shot(1,"CC0_City_BastionI_1600x900.png",1600,900,
         (46,-60,50),(0,-1,4),70)
    shot(5,"CC0_City_BastionV_1600x900.png",1600,900,
         (46,-60,50),(0,-1,4),70)
    shot(5,"CC0_City_BastionV_Close_1600x900.png",1600,900,
         (31,-42,36),(0,1,5),48)
    shot(5,"CC0_City_BastionV_Portrait_720x1200.png",720,1200,
         (34,-50,42),(0,-2,5),48)

# Original, fully editable geometry; no third-party FBX/glTF imported.
for n in range(2,6):
    c=bpy.data.collections.get("Stage"+str(n))
    if c:c.hide_render=False
scene.render.filepath=""
bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"CC0_Valoria_CityFinish.blend"))
def export(stage):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.data.objects:
        if obj.type!="MESH":continue
        colnames=[c.name for c in obj.users_collection]
        if any(c.startswith("Stage") and c != "Stage5Detail" and
               int(c.replace("Stage",""))>stage
               for c in colnames if c.startswith("Stage") and c != "Stage5Detail"):
            continue
        if "Stage5Detail" in colnames and stage<5:continue
        obj.select_set(True)
    target=DEST/("CC0_Valoria_CityStage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(target),export_format="GLB",
                              use_selection=True,export_apply=True)
    assert target.read_bytes()[:4]==b"glTF" and target.stat().st_size>60000
    print("COHERENT_CITY_PHYSICAL_GLB",stage,target.stat().st_size)
for lv in range(1,6):export(lv)
info={"artistic_direction":"single coherent original Valoria architecture master",
      "original_source_script":"tools/cc0-art-system/author_bastion.py",
      "finish_source_script":"tools/cc0-art-system/recompose_city.py",
      "blender_real_render":not skip,"unity_native_render":False,
      "author_owned_export":True,"third_party_3d_in_exports":False,
      "commercial_source_redistribution":"owned meshes only",
      "provisional_art_not_approved":True,
      "stage_count":5,"buildings":["Bastion","Sawmill","Granary","Barracks","Quarry"],
      "assets":[f"CC0_Valoria_CityStage{i}.glb" for i in range(1,6)],
      "source":"CC0_Valoria_CityFinish.blend",
      "renders":[] if skip else [
        "CC0_City_BastionI_1600x900.png","CC0_City_BastionV_1600x900.png",
        "CC0_City_BastionV_Close_1600x900.png","CC0_City_BastionV_Portrait_720x1200.png"]}
(PREV/"city-finish-evidence.json").write_text(json.dumps(info,indent=2)+"\n")
print("ELDORIA_CC0_COHERENT_CITY_FINISH_PASS",info["stage_count"])
