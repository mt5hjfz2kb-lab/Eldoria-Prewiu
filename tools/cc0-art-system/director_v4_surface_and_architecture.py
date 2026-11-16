#!/usr/bin/env python3
"""Eldoria Director V4: premium material/geometry study, built from TRUE V3 glTF.
- Input is the exact original 5-stage Director V3 physical GLBs.
- Adds high-value architectural identity to the Bastion and 4 industries.
- Bakes/provides PHYSICAL author-owned UV textures; no AI image as geometry.
- Produces 5 inspectible GLBs, editable .blend and REAL Blender images.
- Does not claim Unity/iPhone, commercial third-party clearance or art approval.
Run in Blender 4.0.2 with python3-numpy and python3-pil installed.
"""
from pathlib import Path
from mathutils import Vector
import bpy, math, json, random, struct
from math import sin, cos, pi, radians
import numpy as np
from PIL import Image,ImageFilter
SEED=161126
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV2"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV4"
PREV=ROOT/"docs/cc0-art-system/previews/director-v4"
for p in (OUT, PREV):p.mkdir(parents=True,exist_ok=True)
palette={
"stone":((134,136,133), "ashlar"),
"edge":((178,169,152), "ashlar"),
"dark":((79,93,99), "ashlar"),
"roof":((55,72,83), "slate"),
"wood":((116,83,58), "wood"),
"wooddark":((63,49,41), "wood"),
"rock":((108,116,111), "rock")
}
def texture(kind,base,name):
    """Original deterministic tiny tileable painted materials, not downloaded assets."""
    p=OUT/(name+".png")
    if p.exists():return p
    size=512
    rng=np.random.default_rng(SEED+sum(ord(ch) for ch in name))
    yy,xx=np.mgrid[0:size,0:size]
    grain=rng.normal(0,4,(size,size))
    if kind=="ashlar":
        row=yy//48
        shift=(row%2)*48
        blocknum=(xx+shift)//96
        variation=(np.sin(blocknum*19.7+row*4.9)*8)
        seam=(yy%48<3)|((xx+shift)%96<3)
        fine=np.sin(xx*.37+yy*.18)*2.7
        n=grain+variation+fine-np.where(seam,43,0)
    elif kind=="slate":
        row=yy//44
        shift=(row%2)*37
        seam=(yy%44<3)|((xx+shift)%74<2)
        vein=np.sin(xx*.21+yy*.33)*5+np.cos(xx*.08-yy*.13)*2
        n=grain+vein-np.where(seam,28,0)
    elif kind=="wood":
        grainlines=np.sin(xx*.18+np.sin(yy*.031)*2.2)*10+np.sin(xx*.05)*5
        seams=((xx+9)%98<3)
        n=grain*.6+grainlines-np.where(seams,24,0)
    else:
        n=grain*2+np.sin(xx*.06+yy*.17)*9+np.cos(yy*.043)*9
    v=np.stack([np.clip(base[i]+n*(1.0 if i==0 else .94 if i==1 else .90),0,255)
                for i in range(3)],axis=-1).astype(np.uint8)
    Image.fromarray(v,"RGB").save(p)
    return p
TEXTURES={k:texture(kind,color,"DirectorV4_"+k+"_Diffuse") for k,(color,kind) in palette.items()}
TERRAIN_FILE=OUT/"DirectorV4_Terrain_Detail.png"
def improve_terrain():
    if TERRAIN_FILE.exists():return
    origin=SOURCE/"DirectorV2_Terrain_Albedo.png"
    im=Image.open(origin).convert("RGB").resize((1024,1024),Image.Resampling.BICUBIC)
    x=np.asarray(im).astype(np.float32)
    rng=np.random.default_rng(20261116)
    broad=(rng.random((128,128))*255).astype(np.uint8)
    broad=np.asarray(Image.fromarray(broad).resize((1024,1024),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(13)),dtype=np.float32)
    grain=rng.normal(0,3.2,(1024,1024)).astype(np.float32)
    yx=np.mgrid[:1024,:1024]
    undul=np.sin(yx[1]*.017+np.cos(yx[0]*.025))*4.4
    # Different biological green patches and compacted paths preserve old layout.
    boost=(broad-127)*.16+grain+undul
    x[:,:,0]=x[:,:,0]*1.10+boost*.95
    x[:,:,1]=x[:,:,1]*1.13+boost*1.03
    x[:,:,2]=x[:,:,2]*1.03+boost*.83
    Image.fromarray(np.clip(x,0,255).astype(np.uint8),"RGB").save(TERRAIN_FILE)
improve_terrain()
def normalized(name):
    return name.replace(".001","").replace(".002","")
def style_key(matname):
    n=matname.lower()
    if "terrain_bakedtexture" in n:return "terrain"
    if ("roof" in n and "ridge" not in n) or "slate" in n:return "roof"
    if "limestone" in n or "chiseled" in n:return "edge"
    if "oldashlar" in n or "weatheredimperialstone" in n or "rubble" in n:return "stone"
    if "deepashlar" in n or "deepslatewall" in n:return "dark"
    if "charredoak" in n or "burnishedoak" in n:return "wooddark"
    if "d arkoak" in n or "darkoakbeams" in n or "oldoak" in n:return "wood"
    if "naturalgranite" in n or "mossyrocks" in n:return "rock"
    return None
def set_texture(mat,kind):
    mat.use_nodes=True
    tree=mat.node_tree
    bs=tree.nodes.get("Principled BSDF")
    if not bs:return False
    if kind=="terrain":path=TERRAIN_FILE
    else:path=TEXTURES[kind]
    tex=tree.nodes.new("ShaderNodeTexImage")
    tex.name="ELDORIA_V4_AUTHOR_OWNED_DIFFUSE"
    tex.image=bpy.data.images.load(str(path),check_existing=True)
    tex.interpolation="Linear"
    # Remove any previous material-only temporary links, preserve other PBR values.
    socket=bs.inputs["Base Color"]
    for l in list(socket.links):tree.links.remove(l)
    tree.links.new(tex.outputs["Color"],socket)
    bs.inputs["Roughness"].default_value=.91 if kind in ("stone","dark","edge","rock") else .79
    return True
def planar_uvs(obj,kind):
    if obj.type!="MESH" or kind=="terrain":return
    mesh=obj.data
    uv=mesh.uv_layers.active or mesh.uv_layers.new(name="DirectorV4_UV")
    # World-space planar projection on each face, with consistent texel density
    # on upright walls and roof slopes; actual UV0 exported in glTF.
    world=obj.matrix_world
    for face in mesh.polygons:
        no=obj.matrix_world.to_3x3() @ face.normal
        if abs(no.z)>.75:axes=(0,1)
        elif abs(no.y)>abs(no.x):axes=(0,2)
        else:axes=(1,2)
        density=.24 if kind in ("stone","edge","dark") else .20 if kind=="roof" else .33
        for li in face.loop_indices:
            v=world@mesh.vertices[mesh.loops[li].vertex_index].co
            uv.data[li].uv=(v[axes[0]]*density,v[axes[1]]*density)
def rebake_materials():
    mats={}
    for mat in bpy.data.materials:
        kind=style_key(mat.name)
        if kind and set_texture(mat,kind):mats[mat.name]=kind
    for obj in bpy.data.objects:
        if obj.type!="MESH":continue
        kinds={style_key(m.name) for m in obj.data.materials if m}
        kinds.discard(None)
        if kinds and "terrain" not in kinds:
            planar_uvs(obj,sorted(kinds)[0])
    return mats
def mat_flat(name,c,metal=0,rough=.8,emit=0):
    label="DirectorV4_"+name
    x=bpy.data.materials.get(label)
    if x:return x
    x=bpy.data.materials.new(label)
    x.use_nodes=True
    rgba=tuple(v/255 for v in c)+(1,)
    x.diffuse_color=rgba
    node=x.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value=rgba
    node.inputs["Roughness"].default_value=rough
    node.inputs["Metallic"].default_value=metal
    if emit:
        e=node.inputs.get("Emission Color") or node.inputs.get("Emission")
        if e:e.default_value=rgba
        strength=node.inputs.get("Emission Strength")
        if strength:strength.default_value=emit
    return x
M={
"gold":mat_flat("AgedGold",(170,128,68),.45,.54),
"cloth":mat_flat("BannerWine",(77,54,87),0,.94),
"iron":mat_flat("Iron",(43,47,55),.40,.56),
"fire":mat_flat("Fire",(252,167,87),0,.57,.7),
"vines":mat_flat("Ivy",(59,90,66),0,.92),
"moss":mat_flat("Moss",(95,109,76),0,.96),
"wheat":mat_flat("Wheat",(193,151,92),0,.95),
"darkstone":mat_flat("DarkStone",(69,77,83),0,.92),
"stone":mat_flat("Stone",(136,132,121),0,.86),
"wood":mat_flat("Timber",(103,79,56),0,.86),
"roof":mat_flat("Slate",(56,71,85),0,.79),
"window":mat_flat("AmberGlass",(192,120,61),.05,.42,1.6),
"foliage":mat_flat("Underbrush",(70,99,73),0,.98)}
def coll(name):
    x=bpy.data.collections.get(name)
    if x:return x
    x=bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(x)
    return x
def link(obj,mat):
    obj.data.materials.append(M[mat] if isinstance(mat,str) else mat)
    place=coll("DirectorV4_Architecture")
    for c in list(obj.users_collection):c.objects.unlink(obj)
    place.objects.link(obj)
    return obj
def cube(name,p,size,mat,bevel=.02):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    o=bpy.context.object
    o.name=name
    o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    link(o,mat)
    if bevel>0:
        mod=o.modifiers.new("natural dressed edge","BEVEL")
        mod.width=bevel;mod.segments=1
    return o
def cylinder(name,center,rad,depth,mat,n=12,rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=rad,depth=depth,location=center)
    o=bpy.context.object;o.name=name
    if rotation:o.rotation_euler=rotation
    return link(o,mat)
def cone(name,center,r1,r2,depth,mat,n=8):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=depth,location=center)
    o=bpy.context.object;o.name=name
    return link(o,mat)
def beam(name,a,b,width,mat):
    a=Vector(a);b=Vector(b);v=b-a
    o=cube(name,(a+b)*.5,(width,width,v.length),mat)
    o.rotation_euler=v.to_track_quat("Z","Y").to_euler()
    return o
def face(name,points,faces,mat):
    mesh=bpy.data.meshes.new(name+"_Mesh")
    mesh.from_pydata(points,[],faces)
    mesh.materials.append(M[mat])
    mesh.update()
    o=bpy.data.objects.new(name,mesh)
    coll("DirectorV4_Architecture").objects.link(o)
    return o
def ident_buildings(stage):
    # Name-based, building-specific anatomy, never generic houses;
    # detail density intentionally selective to avoid mobile overdraw.
    # Sawmill: broad-sawn timber and integrated lintel, visible from camera.
    x,y=-22.0,-7.5
    for i in range(5):
        beam("ASERRADERO | sawn boards",(x-2.8,y-5.1,.34+i*.19),
             (x+2.9,y-5.1,.34+i*.19),.16,"wood")
    for i in range(2):
        beam("ASERRADERO | iron saw guide",(x+1.2+i*.55,y-2.6,1.1),
             (x+1.2+i*.55,y-2.6,2.4),.095,"iron")
    # A soft angled timber gable distinguishes workshop from barracks.
    for sx in (-1,1):
        beam("ASERRADERO | projecting timber canopy",
             (x+sx*4.6,y-4.0,4.12),(x+sx*4.65,y-5.0,3.55),.16,"wood")
    # Granary: reserve building silhouette for two silos, each with signature
    # crown and grain vents; duplicate old feature should not look like castle turret.
    x,y=22.0,-9.2
    for k in (0,1):
        px=x+4.55+k*2.60
        for z in (1.65,3.12,4.61):
            cube("GRANERO | ventilation aperture",(px,y-.29,z),(.35,.13,.53),"iron")
        for sign in (-1,1):
            beam("GRANERO | oak stave",(px+sign*.84,y-1.3,.6),
                 (px+sign*.84,y-1.3,5.0),.13,"wood")
    for i in range(7):
        xx=x-4.3+i*.62
        beam("GRANERO | drying harvest stack",(xx,y-5.2,.12),
             (xx,y-5.2,1.23),.08,"wheat")
    # Quarry: an excavated working face with colour-graded natural strata.
    x,y=-23.5,11.8
    for k in range(5):
        for i in range(6):
            px=x-8.5+i*1.55
            py=y+8.9-k*.52
            cube("CANTERA | stepped cut stone stratum",(px,py,.18+k*.44),
                 (1.55,.65,.43),"stone",bevel=.08)
    # Training hall: banners and iron shield rack yield military iconography.
    x,y=23.8,11.4
    for k in (-1,0,1):
        px=x+k*1.7
        cylinder("CUARTEL | heavy battle shield",(px,y-3.85,1.55),
                 .68,.15,"darkstone",14,rotation=(math.pi/2,0,0))
        cylinder("CUARTEL | bronze shield boss",(px,y-3.99,1.55),
                 .22,.16,"gold",12,rotation=(math.pi/2,0,0))
    for k in (-1,1):
        cube("CUARTEL | garrison standard",(x+k*4.7,y-3.1,4.6),(.84,.14,1.5),"cloth")
def royal_bastion():
    # Stage-V alone gains an unmistakable chapel-like central entrance and
    # a truly roofed keep, instead of the old isolated triangular cap.
    # The old V3 gable remains behind the new pitched surfaces for continuity.
    y0,y1=3.16,9.8
    peak=17.55
    points=[(-3.65,y0,12.45),(0,y0,peak),(3.65,y0,12.45),
            (-3.65,y1,12.45),(0,y1,peak),(3.65,y1,12.45)]
    face("BASTION V / actually roofed royal nave",points,
         [(0,3,4,1),(1,4,5,2)],"roof")
    beam("BASTION V / one coherent bronze ridge",
         (0,y0-.14,peak+.09),(0,y1+.14,peak+.09),.16,"gold")
    for sy in (-1,1):
        for i in range(7):
            y=y0+.55+i*.87
            x=sy*2.6
            beam("BASTION V / cut stone soffit",(x,y,13.82),
                 (x,y+.63,13.82),.09,"gold")
    # A stone portal made of actual layered relief, lower than the main gable.
    y=2.92
    for side in (-1,1):
        bx=side*1.55
        cube("BASTION V / royal window buttress",(bx,y,10.1),(.37,.48,3.62),"stone",.06)
        for z in (8.60,9.68,10.76,11.82):
            cube("BASTION V / carved ashlar course",(bx,y-.27,z),(.45,.18,.13),"gold")
    # Crowned facade arch, readable from the 40-45 degree mobile camera.
    for i in range(10):
        a=math.pi*i/9
        x=1.45*cos(a)
        z=12.09+1.17*sin(a)
        cube("BASTION V / restored voussoir archivolt",
             (x,y-.28,z),(.24,.22,.35),"stone",.045)
    for side in (-1,1):
        # Strong vertical accents only on level V, no platforms.
        cone("BASTION V / bronze crest finial",
             (side*3.15,y+.35,14.3),.21,0,.9,"gold",8)
        cube("BASTION V / regal wall pennon",
             (side*2.18,y-.25,9.8),(.67,.08,1.4),"cloth",.012)
    cylinder("BASTION V / Valoria imperial sun emblem",
             (0,y-.32,10.9),.63,.15,"gold",16,(math.pi/2,0,0))
    cylinder("BASTION V / enamel purple sun interior",
             (0,y-.43,10.9),.38,.12,"cloth",16,(math.pi/2,0,0))
    for k in range(8):
        a=2*pi*k/8
        cube("BASTION V / raised imperial rays",
             (math.cos(a)*.46,y-.51,10.9+math.sin(a)*.46),
             (.09,.06,.21),"gold",.005)
def add_environment():
    # Clumps placed around city edges, not billboard grass carpets.
    rng=random.Random(SEED)
    for i in range(130):
        x=rng.uniform(-45,45);y=rng.uniform(-40,40)
        if abs(x)<16 and -13<y<16:continue
        if abs(x+22)<8 and abs(y+7.5)<5:continue
        if abs(x-22)<9 and abs(y+9.2)<6:continue
        if abs(x-23.8)<9 and abs(y-11.4)<6:continue
        if abs(x+23.5)<9 and abs(y-11.8)<6:continue
        z=.15+.12*math.sin(x*.28)*math.cos(y*.21)+.17*math.sin(x*.11+y*.16)
        cone("Eldoria V4 | low natural underbrush", (x,y,z+.32),
             rng.uniform(.25,.52),.09,rng.uniform(.4,.85),"foliage",5)
    for i in range(30):
        x=rng.uniform(-40,40);y=rng.uniform(-35,35)
        if abs(x)<12 and -15<y<18:continue
        z=.2+.12*math.sin(x*.28)*math.cos(y*.21)
        cube("Eldoria V4 | worn edge stone",(x,y,z),
             (rng.uniform(.38,1.1),rng.uniform(.42,.9),rng.uniform(.13,.35)),
             "stone",.13)
def lighting(scene):
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.07
    w=scene.world
    if w:
        w.use_nodes=True
        bg=w.node_tree.nodes.get("Background")
        if bg:
            bg.inputs["Color"].default_value=(.35,.40,.52,1)
            bg.inputs["Strength"].default_value=.58
    for ob in bpy.data.objects:
        if ob.type!="LIGHT":continue
        if ob.data.type=="SUN":
            ob.data.energy=3.00
            ob.data.color=(1.0,.77,.57)
            ob.data.angle=radians(3.8)
            ob.rotation_euler=(radians(67),radians(12),radians(-36))
        if ob.data.type=="POINT":
            ob.data.energy=min(105,ob.data.energy)
def capture(stage,suffix,size=(1600,900),portrait=False):
    cam=bpy.context.scene.camera
    if cam is None:
        bpy.ops.object.camera_add(location=(35,-50,40))
        cam=bpy.context.object
        bpy.context.scene.camera=cam
    cam.data.type="ORTHO"
    cam.location=(38,-54,43) if not portrait else (33,-47,42)
    aim=Vector((0,-1,4.1))
    cam.rotation_euler=(aim-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=64 if not portrait else 55
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=24
    scene.cycles.use_denoising=False
    for layer in scene.view_layers:layer.cycles.use_denoising=False
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.render.resolution_percentage=100
    scene.render.resolution_x=size[0];scene.render.resolution_y=size[1]
    scene.render.image_settings.file_format="PNG"
    out=PREV/suffix
    scene.render.filepath=str(out)
    bpy.ops.render.render(write_still=True)
    raw=out.read_bytes()
    if len(raw)<120000 or raw[:8]!=b"\x89PNG\r\n\x1a\n":raise RuntimeError("Real Blender screenshot missing: "+str(out))
    print("ELDORIA_DIRECTOR_V4_REAL_BLENDER_IMAGE",stage,suffix,len(raw))
    return {"stage":stage,"file":suffix,"bytes":len(raw)}
def output_stage(stage):
    f=SOURCE/("DirectorV2_ValoriaStage"+str(stage)+".glb")
    if not f.exists():raise FileNotFoundError(f)
    # Preserve the V4 materials and texture datablocks across stage imports.
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.gltf(filepath=str(f),import_pack_images=True)
    mats=rebake_materials()
    ident_buildings(stage)
    if stage==5:royal_bastion()
    add_environment()
    lighting(bpy.context.scene)
    if stage in (1,5):
        frames.append(capture(stage,f"DirectorV4_Bastion{stage}_1600x900.png"))
        if stage==5:
            frames.append(capture(stage,"DirectorV4_Bastion5_720x1200.png",(720,1200),True))
            scene=bpy.context.scene
            scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV4_Valoria_Master.blend"))
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.data.objects:
        if obj.type=="MESH":obj.select_set(True)
    out=OUT/("DirectorV4_ValoriaStage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",use_selection=True,export_apply=True)
    raw=out.read_bytes()
    if raw[:4]!=b"glTF" or len(raw)<100000:raise RuntimeError("Missing physical GLB "+str(out))
    gltf_len=struct.unpack("<I",raw[12:16])[0]
    obj=json.loads(raw[20:20+gltf_len])
    entry={"stage":stage,"meshes":len(obj.get("meshes",[])),"materials":len(obj.get("materials",[])),
           "images":len(obj.get("images",[])),"bytes":len(raw),"source":"actual author-owned imported V3 physical GLB"}
    print("ELDORIA_DIRECTOR_V4_REAL_3D_EXPORT",json.dumps(entry))
    models.append(entry)
frames=[];models=[]
for stage in range(1,6):
    output_stage(stage)
report={"revision":"Director V4 baked real 3D surfaces and bespoke architecture",
        "input_run_id":43866662062,
        "owned_original_source":True,"external_assets_imported":[],
        "image_generation_as_game_art":False,"actual_blender_render":True,
        "unity_editor_render":False,"mobile_tested":False,"art_approved":False,
        "meshes":models,"frames":frames,
        "textures":[str(p.name) for p in TEXTURES.values()]+[TERRAIN_FILE.name]}
(PREV/"DirectorV4_Provenance.json").write_text(json.dumps(report,indent=2)+"\n")
print("ELDORIA_DIRECTOR_V4_REAL_ASSETS_PASS",len(models),len(frames))
