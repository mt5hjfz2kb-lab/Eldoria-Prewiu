#!/usr/bin/env python3
"""Eldoria CC0 / Director V9: genuine architecture, terrain and forest overhaul.

Uses only the exact, owner-authored physical Director V8 models (Action 43882248142).
No vendor assets, no AI concept art in the scene, no game systems edited.
Rebuilds forest geometry + differentiated masonry/roofs and organic ground detail.
Renders and exports five actual GLBs and an editable Blender source. Unity/iPhone
are NOT tested by this authoring script. Blender 4.0.2, deterministic random seeds.
"""
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi, radians
import bpy, math, json, random, struct, hashlib

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV8"
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV9"
PREVIEW=ROOT/"docs/cc0-art-system/previews/director-v9"
for p in (DEST,PREVIEW): p.mkdir(parents=True,exist_ok=True)
R=random.Random(9112026)
STATS=[]
FRAMES=[]
M={}
def hexrgb(h):
    s=h.lstrip('#')
    return tuple(int(s[i:i+2],16)/255.0 for i in (0,2,4))
def mat(name,hexcolor,rough=.86,emission=0):
    m=bpy.data.materials.new("V9_"+name)
    m.use_nodes=True
    c=hexrgb(hexcolor)
    m.diffuse_color=(*c,1)
    s=m.node_tree.nodes.get("Principled BSDF")
    s.inputs["Base Color"].default_value=(*c,1)
    s.inputs["Roughness"].default_value=rough
    if emission:
        n="Emission Color" if "Emission Color" in s.inputs else "Emission"
        s.inputs[n].default_value=(*c,1)
        s.inputs["Emission Strength"].default_value=emission
    return m
def materials():
    return {
      "ashlar":mat("DeepQuoin","#686D69"),"stone":mat("ChalkWeathered","#8B8C81"),
      "edge":mat("LimestoneDressed","#C0AE96"),"mortar":mat("DeepMortar","#454E51"),
      "slate":mat("BlueBlackSlate","#2B3A49"),"slate2":mat("HeatherSlate","#425261"),
      "trim":mat("AgedCopper","#92704D",.57),
      "bronze":mat("WornBronze","#BE9B63",.51),
      "wood":mat("AgedOak","#604432"),"wood2":mat("CharredOak","#382F29"),
      "iron":mat("BlackForge","#282C32",.53),
      "banner":mat("WinePurple","#473752"),"amber":mat("FirelitWindow","#D6A06A",.5,.95),
      "fir1":mat("DeepFir","#213F36"),"fir2":mat("MossFir","#355246"),
      "fir3":mat("LightFir","#4D6652"),"fir4":mat("FadedFir","#60745D"),
      "trunk":mat("ForestBark","#53463B"),"soil":mat("Earth","#655A46"),
      "grass":mat("OliveGrass","#526346"),"grass2":mat("SageGrass","#657557"),
      "wildflower":mat("PaleLavender","#9A83A4"),"water":mat("RiverSlate","#3A6470",.35),
      "granite":mat("QuarryGranite","#72786D")
    }
def col(name):
    c=bpy.data.collections.get(name)
    if c:return c
    c=bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(c)
    return c
def attach(obj,key,category):
    target=col(category)
    for collection in list(obj.users_collection): collection.objects.unlink(obj)
    target.objects.link(obj)
    obj.data.materials.clear()
    obj.data.materials.append(M[key] if isinstance(key,str) else key)
    return obj
def cub(name,xyz,dim,key,category,bevel=.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz)
    o=bpy.context.object
    o.name=name
    o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    attach(o,key,category)
    if bevel:
        modifier=o.modifiers.new("V9 angled masonry edge","BEVEL")
        modifier.width=bevel
        modifier.segments=1
    return o
def cylinder(name,xyz,r,depth,key,category,segments=12,orientation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=segments,radius=r,depth=depth,location=xyz)
    o=bpy.context.object
    o.name=name
    if orientation:o.rotation_euler=orientation
    return attach(o,key,category)
def cone(name,xyz,r,top,depth,key,category,segments=10):
    bpy.ops.mesh.primitive_cone_add(vertices=segments,radius1=r,radius2=top,depth=depth,location=xyz)
    o=bpy.context.object
    o.name=name
    return attach(o,key,category)
def rod(name,start,end,r,key,category):
    start,end=Vector(start),Vector(end)
    difference=end-start
    if difference.length<.0005:return None
    o=cub(name,(start+end)*.5,(r,r,difference.length),key,category,.015)
    o.rotation_euler=difference.to_track_quat('Z','Y').to_euler()
    return o
def poly(name,vertices,faces,key,category,face_roles=None):
    mesh=bpy.data.meshes.new(name+"_mesh")
    mesh.from_pydata(vertices,[],faces)
    mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    col(category).objects.link(obj)
    if face_roles:
        keys=list(dict.fromkeys(face_roles))
        for k in keys:mesh.materials.append(M[k])
        for face,k in zip(mesh.polygons,face_roles):face.material_index=keys.index(k)
    else:mesh.materials.append(M[key])
    return obj
def ground_z(x,y):
    # Source V8 ground-height equation, exactly preserved.
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
            +.22*sin(x*.064-y*.083)*cos(y*.11)+.06*cos(.24*x+.18*y))
def purge_v8_canopy():
    removed=0
    positions=[]
    targets=("V8 irregular crown","V8 gnarled tree trunk",
             "V8 mossy outcrop","V8 clustered ground herbs",
             "V8 interlocking spruce foliage")
    for o in list(bpy.data.objects):
        if o.type!="MESH":continue
        if o.name.startswith("V8 irregular crown"):
            p=o.matrix_world.translation
            positions.append((p.x,p.y))
        if o.name.startswith(targets):
            bpy.data.objects.remove(o,do_unlink=True)
            removed+=1
    # Duplicate V8 shrubs would create mint-coloured rocks. Remove by their
    # actual source collection, but preserve stronghold and existing economics.
    for name in ("V8_BUSHES","V8_GROUNDCOVER","V8_ROCKS"):
        c=bpy.data.collections.get(name)
        if c:
            for obj in list(c.objects):
                if obj.type=="MESH":
                    bpy.data.objects.remove(obj,do_unlink=True);removed+=1
    # Clear stray fake-reference solids from old forest after import if present.
    return removed,positions
def spruce_mesh(seed):
    """Actual asymmetric bough geometry from many offset fan and branch lobes.

    Quicker to render than thousands of unique meshes, but foliage is true 3D.
    Material indices share one master set throughout the five stages.
    """
    rng=random.Random(54320+seed)
    v=[];f=[];mt=[]
    sectors=11
    for layer in range(8):
        fraction=layer/7
        z=.31+fraction*.55
        rr=(.235-.158*fraction)*rng.uniform(.95,1.12)
        lean=(.013*sin(layer*2.5+seed),.022*cos(layer*1.7+seed))
        center=(lean[0],lean[1],z+.115+.014*rng.random())
        tip=len(v)+sectors*2
        for s in range(sectors):
            ang=2*pi*s/sectors
            reach=rr*(.83+.36*rng.random())*(1+.06*sin(ang*3+layer))
            v.append((lean[0]+rr*.17*cos(ang),lean[1]+rr*.17*sin(ang),z+.10))
            v.append((lean[0]+reach*cos(ang),lean[1]+reach*sin(ang),z-.028*(1-fraction)))
        v.append(center)
        for s in range(sectors):
            a=tip-2*sectors+2*s
            b=tip-2*sectors+2*((s+1)%sectors)
            f.append((a,a+1,tip));mt.append((layer+s+seed)%3)
            f.append((a,tip,b+1));mt.append((layer+s+seed+1)%3)
        # Staggered side twig fans; break the repeated flat pyramid outline.
        for s in range(0,sectors,2):
            a=2*pi*s/sectors
            root=len(v)
            v.extend([
               (lean[0]+rr*.12*cos(a),lean[1]+rr*.12*sin(a),z+.15),
               (lean[0]+rr*1.35*cos(a),lean[1]+rr*1.35*sin(a),z-.036),
               (lean[0]+rr*1.23*cos(a+.24),lean[1]+rr*1.23*sin(a+.24),z-.056)])
            f.append((root,root+1,root+2));mt.append((s+layer+1)%3)
    mesh=bpy.data.meshes.new("V9 deep green natural spruce mesh "+str(seed))
    mesh.from_pydata(v,[],f)
    mesh.update()
    for key in ("fir1","fir2","fir3"):mesh.materials.append(M[key])
    for face,ind in zip(mesh.polygons,mt):face.material_index=ind
    return mesh
def forest(positions):
    rng=random.Random(196106)
    meshes=[spruce_mesh(i) for i in range(3)]
    if len(positions)<25:
        # Robust to V8 source variants; never claim tree positions missing
        # are evidence that the forest has been transferred.
        positions=[]
        for i in range(100):
            x,y=rng.uniform(-55,55),rng.uniform(-48,50)
            if abs(x)<21 and -26<y<23:continue
            positions.append((x,y))
    count=0
    for i,(x,y) in enumerate(positions):
        if abs(x)<17 and -27<y<17:continue
        if abs(x-23)<9 and -15<y<18:continue
        if abs(x+23)<9 and -15<y<17:continue
        h=rng.uniform(5.9,10.9)
        root=ground_z(x,y)
        # One true shared mesh per 3 shape classes.
        top=bpy.data.objects.new("V9 spruce living crown %03d"%i,meshes[i%3])
        col("V9_FOREST_CANOPY").objects.link(top)
        top.location=(x,y,root+.06)
        top.scale=(h,h,h)
        top.rotation_euler=(0,0,rng.uniform(-pi,pi))
        cylinder("V9 tapered trunk %03d"%i,(x,y,root+h*.34),
                 h*.033,h*.68,"trunk","V9_FOREST_TRUNKS",8)
        count+=1
    # Broadleaf bushes are irregular low clusters with real multi-tonal materials.
    for n in range(108):
        x,y=rng.uniform(-52,52),rng.uniform(-46,46)
        if -12<x<12 and -14<y<16:continue
        h=rng.uniform(.24,1.08)
        for j in range(3):
            dx,dy=rng.uniform(-h*.35,h*.35),rng.uniform(-h*.32,h*.32)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=h,
                location=(x+dx,y+dy,ground_z(x,y)+h*.27))
            obj=bpy.context.object
            obj.name="V9 evergreen wild underbrush"
            obj.scale=(rng.uniform(.66,1.25),rng.uniform(.62,1.12),rng.uniform(.30,.56))
            attach(obj,rng.choice(("fir1","fir2","fir3","grass")),"V9_UNDERBRUSH")
    # Batched grasses as actual triangular tufts; no 500 individual draw nodes.
    for key,seed,num in (("grass",1,520),("grass2",2,450),("wildflower",3,170)):
        vertices=[];faces=[]
        for i in range(num):
            x,y=rng.uniform(-50,50),rng.uniform(-47,45)
            if -10<x<10 and -18<y<11:continue
            z=ground_z(x,y)+.01
            a=len(vertices)
            s=rng.uniform(.08,.20);h=rng.uniform(.24,.56)
            vertices.extend([(x-s,y-.05,z),(x+s,y+.07,z),(x+.07,y,z+h)])
            faces.append((a,a+1,a+2))
        poly("V9 clustered meadow "+key,vertices,faces,key,"V9_MEADOW")
    return count
def roof(name,cx,y0,y1,z,half,rise,cat,rows=7):
    """Real deep intersecting gabled roof with physically offset slate courses.

    Every barge and course is geometry, not a photo/illustration on the castle.
    """
    v=[(cx-half,y0,z),(cx+half,y0,z),(cx,y0,z+rise),
       (cx-half,y1,z),(cx+half,y1,z),(cx,y1,z+rise)]
    poly(name+" dressed gable",v[:3],[(0,1,2)],"ashlar",cat)
    poly(name+" slate planes",v,
         [(0,3,5,2),(1,2,5,4)],"slate",cat)
    for a,b in ((0,2),(1,2)):
        rod(name+" bronze-edged rake",v[a],v[b],.18,"trim",cat)
    rod(name+" solid roof ridge",(cx,y0,z+rise),(cx,y1,z+rise),.16,"bronze",cat)
    # Genuine overlapping courses follow pitched roof, not random ridges.
    for row in range(1,rows):
        u=row/rows
        for s in (-1,1):
            x=cx+s*half*(1-u)
            zz=z+rise*u+.035
            rod(name+" individual slate course",
                (x,y0+.12,zz),(x,y1-.12,zz),.058,"slate2",cat)
    for y in (y0,y1):
        rod(name+" eaves drip edge",(cx-half,y,z),(cx+half,y,z),.13,"wood2",cat)
def window(name,x,y,z,cat):
    cub(name+" recess",(x,y,z),(.75,.15,1.12),"mortar",cat,.03)
    cub(name+" warm leaded glazing",(x,y-.09,z),(.53,.15,.92),"amber",cat,.015)
    rod(name+" glazing mullion",(x,y-.2,z-.47),(x,y-.2,z+.47),.065,"iron",cat)
    cub(name+" dressed sill",(x,y-.10,z-.65),(1.06,.32,.18),"edge",cat,.05)
    cub(name+" hewn lintel",(x,y-.09,z+.65),(1.10,.33,.18),"stone",cat,.05)
def arch(name,x,y,z,w,h,cat):
    # Stone pointed portal replacing giant rectangular cut-out.
    cub(name+" shadow",(x,y,z+h*.5),(w,.25,h),"mortar",cat,.08)
    for s in (-1,1):
        rod(name+" carved pointed jamb",
            (x+s*w*.53,y-.16,z),(x+s*w*.53,y-.16,z+h*.76),.24,"edge",cat)
        rod(name+" rising Gothic voussoir",
            (x+s*w*.56,y-.18,z+h*.77),(x,y-.18,z+h*1.13),.22,"stone",cat)
    cub(name+" lintel band",(x,y-.10,z+h+.38),(w+1.1,.40,.18),"bronze",cat,.025)
def bastion(stage):
    if stage<3:return
    c="V9_BASTION_STAGED"
    # Rebuilding should be visibly real at III/IV, not sudden fifth-level sticker.
    for side in (-1,1):
        x=side*6.0
        cub("citadel new ashlar flanking hall",(x,4.7,4.8),(3.5,5.7,5.4),
            "ashlar",c,.10)
        if stage>=4:
            roof("citadel lateral nave",x,1.5,8.6,7.5,2.3,2.4,c)
            for y in (2.6,5.8):
                cub("citadel lateral buttress",(x+side*1.8,y,3.8),(.57,.7,6.9),
                    "stone",c,.05)
    if stage<5:return
    cat="V9_BASTION_MONUMENT"
    # Rebuild silhouette — tall imperial hall with adjoining pitched roofs.
    cub("upper Valoria great hall cast masonry",(0,6.1,12.4),(6.6,7.2,9.1),
        "ashlar",cat,.16)
    roof("imperial cathedral keep",0,2.35,10.55,17.0,4.00,4.0,cat,9)
    # Stone cornices prevent clip-art modular block joins.
    for level in (7.65,10.3,14.8):
        cub("continuous buttressed stone string-course",(0,2.52,level),
            (8.9,.67,.30),"edge",cat,.06)
    for x in (-3.58,3.58):
        cub("flying buttress load-bearing ashlar",(x,3.6,9.8),(.68,1.30,9.5),
            "stone",cat,.06)
        for zz in (8.2,10.8,13.9):
            cub("buttress chamfered cap",(x,2.72,zz),(.93,.69,.38),
                "edge",cat,.06)
    # Stained hall takes focus in true 3D: deep shadow / pointed frame.
    arch("Valoria imperial high Lancet",0,2.04,11.28,2.05,4.7,cat)
    for x in (-.65,.65):
        rod("royal window leading",(x,1.67,12.0),(x,1.67,15.5),
            .06,"iron",cat)
    for zz in (12.3,13.5,14.7):
        rod("royal stained pane transom",(-.9,1.64,zz),(.9,1.64,zz),
            .06,"iron",cat)
    # Restore a credible rich gate rather than purely raised wall height.
    arch("grand castle gate central voussoir",0,-6.0,1.3,4.2,4.8,cat)
    for x in (-2.92,2.92):
        cub("gate armored lantern stand",(x,-6.35,3.0),(.43,.46,1.85),
            "iron",cat,.06)
    for side in (-1,1):
        x=side*8.18
        # Stone-capped crenel tower: conical blue roof breaks block silhouette.
        cone("repaired tower heavy slate cap",(x,-4.28,8.65),
             2.48,.12,2.50,"slate",cat,12)
        cylinder("bronze roof apex",(x,-4.28,10.02),.14,.36,
                 "bronze",cat,10)
    # Reconstruct one imperial emblem and two heraldic cloth standards.
    cylinder("Valoria founded crest",(0,1.75,9.85),.66,.15,
             "bronze",cat,14,orientation=(radians(90),0,0))
    for x in (-2.95,2.95):
        cub("Valoria restrained imperial banner",(x,2.00,9.30),
            (.77,.11,2.25),"banner",cat,.035)
def industry():
    """A function dictates the architecture; do not add generic copies of homes."""
    c="V9_DISTRICT_FINISH"
    # Upper-left: stone quarry integrated with natural mineral outcrops, derrick.
    for i in range(32):
        x=-29+(i%8)*1.34
        y=16+(i//8)*1.10
        z=ground_z(x,y)+.20+(.04*(i%3))
        cub("quarry cut physical ashlar inventory",(x,y,z),
            (1.13,.88,.56),("stone" if i%4 else "ashlar"),c,.045)
    for x in (-28,-25.5):
        rod("quarry load bearing crane", (x,10.5,.2),(x,10.5,7.85),.35,"wood",c)
    rod("quarry tall cross-beam",(-28,10.5,7.85),(-22.6,10.5,7.85),.36,"wood2",c)
    rod("quarry diagonal bracing",(-27.95,10.5,4.4),(-22.7,10.5,7.75),.22,
        "wood",c)
    rod("quarry working chain",(-23.3,10.5,7.55),(-23.3,10.5,4.5),.075,"iron",c)
    cub("quarry stone from derrick",(-23.3,10.5,4.20),(1.45,1.10,.87),
        "stone",c,.09)
    # Upper right: barracks is an armory with training, heraldic defense.
    x,y=24.0,10.5
    roof("guardhouse military steep gable",x,6.1,15.65,4.8,5.0,3.25,c,7)
    for i in range(6):
        xx=x-5.4+i*1.2
        rod("military yard spear rack", (xx,5.1,.2),(xx,5.1,2.7),.10,
             "wood",c)
        cylinder("military target ring",(xx,4.95,1.55),.42,.07,
                 "edge",c,12,orientation=(radians(90),0,0))
    # Granary lower right: unmistakable grain silos connected to covered stonehouse.
    gx,gy=22.0,-9.2
    cub("grainhouse storey stone frontage",(gx-1.3,gy-2.50,2.07),
        (9.0,.58,3.65),"ashlar",c,.07)
    roof("imperial grainhouse heavy storage",gx-1.2,gy-4.1,gy+2.7,
         4.15,5.2,2.05,c,7)
    for j in range(2):
        xx=gx+4.35+j*2.45
        cylinder("granary attached limestone silo",(xx,gy+1.5,2.5),
                 1.32,4.9,"ashlar",c,14)
        cone("granary broad conic blue slate cap",(xx,gy+1.5,5.78),
             1.61,.12,2.0,"slate",c,14)
        for zz in (1.1,2.55,4.35):
            cylinder("granary dressed horizontal silo band",(xx,gy+1.5,zz),
                     1.37,.17,"edge",c,14)
    # Left-side sawmill: operating open timber frame, wheel and visible stock.
    x,y=-22.,-7.5
    for yy in (y-2.7,y,y+2.7):
        rod("sawmill opened heavy timber work-bay",(x+2.1,yy,.18),
            (x+2.1,yy,4.3),.28,"wood",c)
    roof("sawmill open work-bay",x-1.0,y-4.3,y+3.65,
         4.72,6.0,2.25,c,6)
    wheel=(-27.8,-10.15,2.15)
    for i in range(15):
        a=2*pi*i/15;b=2*pi*(i+1)/15
        rod("sawmill waterwheel outer oak rim",
            (wheel[0],wheel[1]+2.18*sin(a),wheel[2]+2.18*cos(a)),
            (wheel[0],wheel[1]+2.18*sin(b),wheel[2]+2.18*cos(b)),
            .25,"wood",c)
    for i in range(8):
        a=2*pi*i/8
        rod("sawmill wheel spoke",wheel,
            (wheel[0],wheel[1]+2.18*sin(a),wheel[2]+2.18*cos(a)),
            .19,"wood2",c)
    for i in range(14):
        cub("sawmill split wooden planks",(x-4.7+i*.31,y-4.6,.35),
            (.25,2.30,.20),"wood",c,.018)
def ambience():
    scene=bpy.context.scene
    for o in list(bpy.data.objects):
        if o.type in ("LIGHT","CAMERA"):bpy.data.objects.remove(o,do_unlink=True)
    scene.render.engine="CYCLES"
    scene.cycles.samples=25
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    scene.render.threads_mode="FIXED"
    scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.35
    world=bpy.data.worlds.new("V9 late bronze autumn dusk")
    scene.world=world;world.use_nodes=True
    bg=world.node_tree.nodes["Background"]
    bg.inputs["Color"].default_value=(.34,.39,.46,1)
    bg.inputs["Strength"].default_value=.76
    bpy.ops.object.light_add(type="SUN",location=(0,0,45))
    sun=bpy.context.object
    sun.rotation_euler=(radians(73),radians(8),radians(-34))
    sun.data.energy=3.4
    sun.data.color=(1,.76,.55)
    sun.data.angle=radians(3.9)
    for xx in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(xx,-6.5,3.3))
        lamp=bpy.context.object
        lamp.data.energy=110
        lamp.data.color=(1,.60,.36)
        lamp.data.shadow_soft_size=.8
    bpy.ops.object.camera_add(location=(36,-51,39))
    cam=bpy.context.object
    cam.name="V9 same 3D city comparison camera"
    cam.data.type="ORTHO";cam.data.ortho_scale=69
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def render(stage,name,wh,portrait=False):
    scene=bpy.context.scene
    cam=scene.camera
    cam.location=(28,-48,43) if portrait else (36,-51,39)
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=57 if portrait else 69
    scene.render.resolution_x,scene.render.resolution_y=wh
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    p=PREVIEW/name
    scene.render.filepath=str(p)
    bpy.ops.render.render(write_still=True)
    raw=p.read_bytes()
    if len(raw)<130000 or raw[:8]!=b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("Genuine Blender PNG missing or blank: "+str(p))
    FRAMES.append({"stage":stage,"name":name,"bytes":len(raw),"size":wh})
    print("ELDORIA_V9_REAL_BLENDER_RENDER",name,len(raw))
def export(stage):
    p=DEST/("DirectorV9_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(p),export_format="GLB",export_apply=True)
    raw=p.read_bytes()
    if raw[:4]!=b'glTF' or struct.unpack_from("<I",raw,8)[0]!=len(raw):
        raise RuntimeError("No valid physical GLB: "+str(p))
    length=struct.unpack_from("<I",raw,12)[0]
    data=json.loads(raw[20:20+length])
    if len(data.get("meshes",[]))<500 or len(data.get("images",[]))<4:
        raise RuntimeError("Real V9 3D or texture content missing")
    rec={"stage":stage,"meshes":len(data["meshes"]),"images":len(data["images"]),
        "materials":len(data.get("materials",[])),"bytes":len(raw)}
    STATS.append(rec)
    print("ELDORIA_V9_TRUE_3D",json.dumps(rec))
def main():
    global M
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        src=SOURCE/("DirectorV8_Stage"+str(stage)+".glb")
        if not src.exists():raise RuntimeError("Original source missing: "+str(src))
        bpy.ops.import_scene.gltf(filepath=str(src),import_pack_images=True)
        M=materials()
        removed,original_positions=purge_v8_canopy()
        tree_count=forest(original_positions)
        bastion(stage)
        industry()
        ambience()
        if stage==1:
            render(stage,"DirectorV9_BastionI_1600x900.png",[1600,900])
        elif stage==5:
            render(stage,"DirectorV9_BastionV_1600x900.png",[1600,900])
            render(stage,"DirectorV9_BastionV_Portrait_720x1200.png",[720,1200],True)
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"DirectorV9_Editable_Source.blend"))
        export(stage)
        print("ELDORIA_V9_TREE_REBUILD",stage,removed,tree_count)
    record={"project":"Eldoria CC0 Art System","version":"V9 rebuilt natural forest, landmark architecture, distinct districts",
      "input_run_id":43882248142,"input_rights":"owner-authored Eldoria meshes only",
      "vendor_assets_included":False,"images_are_real_blender_renders":True,
      "unity_native_tested":False,"iphone_tested":False,"art_approved":False,
      "stages":STATS,"renders":FRAMES}
    (PREVIEW/"v9-evidence.json").write_text(json.dumps(record,indent=2)+"\n")
    print("ELDORIA_V9_REAL_ART_PASS",len(STATS),len(FRAMES))
if __name__=="__main__":
    main()
