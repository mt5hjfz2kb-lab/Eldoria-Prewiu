#!/usr/bin/env python3
"""V10 ORIGINAL medieval material pass on the visually inspected sRGB-corrected V9.

Creates real repeatable 1024px ashlar/roof/wood texture atlases, UV maps and
small grounded architectural detailing in physical Blender geometry.
Consumes exact 5 previous GLB from run #43886721399. No 2D AI picture used as
source; no vendor geometry. Output editable BLEND + five GLB + Blender renders.
This is ART LOOKDEV, never a claim of Unity/WebGL/iPhone or approval.
"""
from pathlib import Path
from mathutils import Vector
from math import pi,cos,sin,radians
import math,random,struct,json,hashlib
import bpy
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
INPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV9Structural"
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV10"
PREV=ROOT/"docs/cc0-art-system/previews/director-v10"
for p in (DEST,PREV):p.mkdir(parents=True,exist_ok=True)
R=random.Random(100116)
STAGES=[];FRAMES=[]
def s2l(c):
    return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
def srgb(hexcolor):
    s=hexcolor.strip("#")
    return tuple(s2l(int(s[i:i+2],16)/255.) for i in (0,2,4))
def image_rgba(name,base,kind,sz=1024):
    """Real repeatable sRGB display texture transformed to scene-linear pixel buffer."""
    rng=np.random.default_rng(sum(map(ord,name))+10116)
    yy,xx=np.mgrid[:sz,:sz]
    noise=rng.normal(0,.019,(sz,sz)).astype(np.float32)
    a=np.zeros((sz,sz,3),np.float32)+np.array(base,np.float32)
    if kind=="ashlar":
        # Orthogonal course system with partly offset staggered vertical joins
        row=(yy//62).astype(np.int32)
        edge=(yy%62<3)|((xx+(row%2)*46)%123<3)
        noise+=np.sin((xx//123)*1.43+row*2.3)*.047
        noise[edge]-=.21
        pitted=(np.cos(xx*.31+yy*.35)+np.sin(xx*.21-yy*.26))*0.017
        noise+=pitted
        speckles=rng.random((sz,sz))<.018
        noise[speckles]-=.13
    elif kind=="slate":
        row=(yy//43).astype(np.int32)
        edge=(yy%43<3)|((xx+(row%2)*38)%91<2)
        noise[edge]-=.19
        noise+=np.cos(xx*.03+yy*.081)*.036
        noise+=np.sin(xx*.071-yy*.030)*.018
    elif kind=="wood":
        noise+=np.sin(xx*.23+np.sin(yy*.06))* .049
        noise+=np.sin(xx*.051+yy*.019)*.023
        noise[(xx%103<3)]-=.14
    if kind=="ashlar":
        # Restrained lichen bloom in some mortar crossings, not black noise
        moss=np.clip(np.sin(xx*.012)*np.cos(yy*.014)-.48,0,1)
        a[:,:,1]+=moss*.055
        a[:,:,0]-=moss*.025
    a+=noise[:,:,None]*np.array([1.0,.94,.87])
    a=np.clip(a,0,1)
    # Construct float scene-linear pixels from sRGB colour channels.
    linear=np.where(a<=.04045,a/12.92,((a+.055)/1.055)**2.4)
    rgba=np.dstack((linear,np.ones((sz,sz),np.float32)))
    im=bpy.data.images.new("V10_"+name,width=sz,height=sz,alpha=True)
    im.pixels.foreach_set(rgba.astype(np.float32).reshape(-1))
    im.update()
    im.filepath_raw=str(DEST/("V10_"+name+".png"))
    im.file_format="PNG"
    im.save()
    im.pack()
    return im
def material(name,image):
    m=bpy.data.materials.new("V10_MASTER_"+name)
    m.use_nodes=True
    n=m.node_tree.nodes.get("Principled BSDF")
    n.inputs["Roughness"].default_value=.86
    texture=m.node_tree.nodes.new("ShaderNodeTexImage")
    texture.image=image
    texture.extension='REPEAT'
    texture.interpolation='Linear'
    m.node_tree.links.new(texture.outputs["Color"],n.inputs["Base Color"])
    return m
def msolid(name,hexcode,rough=.85,metal=0):
    m=bpy.data.materials.new("V10_"+name)
    m.use_nodes=True
    shader=m.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value=(*srgb(hexcode),1)
    shader.inputs["Roughness"].default_value=rough
    shader.inputs["Metallic"].default_value=metal
    m.diffuse_color=(*srgb(hexcode),1)
    return m
T={}
def build_atlases():
    T["ashlar"]=material("ImperialAshlar",image_rgba("MasonryAtlas",(.42,.42,.385),"ashlar"))
    T["edge"]=material("LimestoneTrim",image_rgba("LimestoneAtlas",(.63,.60,.53),"ashlar"))
    T["slate"]=material("CopperSlate",image_rgba("SlateAtlas",(.18,.245,.31),"slate"))
    T["slate2"]=material("WornSlate",image_rgba("WornSlateAtlas",(.26,.315,.36),"slate"))
    T["wood"]=material("AgedOak",image_rgba("OakAtlas",(.37,.27,.19),"wood"))
    T["wood2"]=material("DeepOak",image_rgba("DarkOakAtlas",(.21,.18,.15),"wood"))
    T["iron"]=msolid("PittedForgedIron","#262D32",.58,.56)
    T["bronze"]=msolid("WornBronze","#B5915F",.56,.31)
    T["grass"]=msolid("GrassShade","#506049")
    T["rock"]=msolid("GroundStone","#77766B")
    T["moss"]=msolid("MossShade","#3E5B43")
    T["violet"]=msolid("ValoriaCloth","#493652")
def rematerial():
    source_map={
       "V9_DeepQuoin":"ashlar","V9_ChalkWeathered":"ashlar",
       "V9_LimestoneDressed":"edge","V9_BlueBlackSlate":"slate",
       "V9_HeatherSlate":"slate2","V9_AgedOak":"wood",
       "V9_CharredOak":"wood2"
    }
    count={k:0 for k in source_map}
    for o in list(bpy.data.objects):
        if o.type!="MESH":continue
        changed=False
        for i,m in enumerate(o.data.materials):
            if m is None:continue
            key=next((v for k,v in source_map.items() if m.name.startswith(k)),None)
            if key is None:continue
            count[next(k for k,v in source_map.items() if m.name.startswith(k))]+=1
            o.data.materials[i]=T[key]
            changed=True
        if changed:
            mesh=o.data
            uv=mesh.uv_layers.active or mesh.uv_layers.new(name="V10_Atlas_UV")
            for polygon in mesh.polygons:
                normal=polygon.normal
                axis=(0,1) if abs(normal.z)>.62 else (1,2) if abs(normal.x)>abs(normal.y) else (0,2)
                # The physical tile size is approximately 2.2m. Project from
                # original world position for continuity over modular joins.
                for k in polygon.loop_indices:
                    vi=mesh.loops[k].vertex_index
                    p=o.matrix_world @ mesh.vertices[vi].co
                    uv.data[k].uv=(p[axis[0]]*.19,p[axis[1]]*.19)
    return count
def col(name):
    c=bpy.data.collections.get(name)
    if c:return c
    c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c)
    return c
def add(obj,k,name,c):
    obj.name=name
    dest=col(c)
    for old in list(obj.users_collection):old.objects.unlink(obj)
    dest.objects.link(obj)
    obj.data.materials.append(T[k])
    return obj
def cube(name,p,size,k,colname,bevel=0.035):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    ob=bpy.context.object;ob.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    add(ob,k,name,colname)
    if bevel:
        m=ob.modifiers.new("V10 chamfer","BEVEL");m.width=bevel;m.segments=1
    return ob
def beam(name,p0,p1,width,key,colname):
    a,b=Vector(p0),Vector(p1)
    v=b-a
    if v.length<=.005:return None
    ob=cube(name,(a+b)*.5,(width,width,v.length),key,colname,.019)
    ob.rotation_euler=v.to_track_quat("Z","Y").to_euler()
    return ob
def cone(name,p,rad,top,deep,k,cat,verts=10):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=rad,radius2=top,
                                   depth=deep,location=p)
    return add(bpy.context.object,k,name,cat)
def ico(name,p,r,k,colname,scale=None):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=r,location=p)
    o=bpy.context.object
    if scale:o.scale=scale
    return add(o,k,name,colname)
def ground(x,y):
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
            +.22*sin(x*.064-y*.083)*cos(y*.11)+.06*cos(.24*x+.18*y))
def landmarks(stage):
    cat="V10_ARCHITECTURAL"
    if stage==1:
        # Substantial visible repair scaffolding without V5 ornament.
        for x in (-3.6,3.6):
            beam("unfinished wall construction post",(x,1.8,.2),(x,1.8,6.2),
                 .18,"wood",cat)
        return
    # Relieve masonry foundations all along the gate face, consistent
    # throughout growing levels rather than V5 decorative stickers.
    for side in (-1,1):
        for xx in (2.15,3.62,5.15,6.66):
            x=xx*side
            cube("dressed gate-quoin block",(x,-5.13,2.5),
                 (.61,.62,4.8),"ashlar",cat,.07)
        for h in (1.0,2.9,4.5):
            beam("stone string-course continuous",
                 (side*2.13,-5.43,h),(side*7.45,-5.43,h),
                 .19,"edge",cat)
    if stage<4:return
    # Window recess and stone buttress transition for restored upper hall.
    for side in (-1,1):
        x=side*5.9
        for z in (4.5,6.0,7.3):
            cube("sloped side chapel buttress course",(x,2.3,z),(.85,.52,.42),
                 "edge",cat,.05)
    if stage<5:return
    cat="V10_BASTION5"
    # Twelve converging dressed voussoirs in a real arched gate, not floating slabs.
    arch_center_z=4.14
    rr=2.21
    for k in range(13):
        ang=pi*k/12
        x=rr*cos(ang)
        z=arch_center_z+1.24*sin(ang)
        cub=cube("arched castle gate visible voussoir",(x,-6.41,z),
                 (.53,.38,.44),"edge",cat,.055)
        cub.rotation_euler[1]=-.25*cos(ang)
    for i in range(4):
        y=-6.45-(i+1)*.45
        z=.12+(4-i)*.105
        cube("limestone processional gate step",(0,y,z),
             (5.0+.25*i,.60,.19),"edge",cat,.035)
    # Stone quoins and mullioned dormers change 3D silhouette, not merely shader.
    for side in (-1,1):
        for yy in (4.5,7.25):
            x=side*3.75
            for zz in (12,13.6,15.3):
                cube("massive imperial fitted ashlar corner",
                     (x,yy,zz),(.56,.71,.42),"ashlar",cat,.062)
        x=side*1.95
        cube("granite buttressed dormer front",(x,3.08,17.25),
             (1.0,.52,1.68),"ashlar",cat,.075)
        cube("glowing dormer slit",(x,2.73,17.15),
             (.47,.13,.95),"bronze",cat,.025)
        cone("slate dormer upper canopy",(x,3.1,18.35),
             .80,.01,1.05,"slate",cat,8)
    # Main crown crest physically present on the curtain wall.
    cube("Valoria heraldic banner tablet",(0,-6.45,6.25),
         (1.60,.22,.87),"violet",cat,.06)
    for side in (-1,1):
        cube("imperial banner side tab",(side*.78,-6.59,6.20),
             (.13,.28,1.04),"bronze",cat,.03)
def life_and_ground():
    cat="V10_GROUNDED_STORY"
    rng=random.Random(20261116)
    for _ in range(250):
        x,y=rng.uniform(-54,54),rng.uniform(-49,50)
        if abs(x)<11 and -15<y<14:continue
        if (x-23)**2+(y+8)**2<80:continue
        if (x+23)**2+(y+7)**2<75:continue
        if (x-24)**2+(y-10)**2<60:continue
        if (x+24)**2+(y-12)**2<60:continue
        h=rng.uniform(.15,.65)
        # Tiny natural pebbles and wild fern clusters; no elevated tiles.
        ico("weathered natural earth outcrop",(x,y,ground(x,y)+h*.28),
            h,rng.choice(("rock","moss","grass")),cat,
            (rng.uniform(.72,1.35),rng.uniform(.75,1.3),rng.uniform(.35,.67)))
    # Quarry, the unmistakable cut-stone stockpile; no fake city district added.
    for i in range(6):
        x=-30+i*1.21;y=16+math.sin(i*.8)*.4
        cube("cut stone saw mark stock",(x,y,ground(x,y)+.55),
             (.93,.72,1.00),"ashlar",cat,.045)
    # Sawmill age/patina as physical saw-blade and workbench frames.
    for i in range(7):
        x=-25+i*.53
        beam("sawmill sawn oak main stock",
             (x,-12,ground(x,-12)+.35),
             (x,-9,ground(x,-9)+.35),.18,"wood",cat)
    # Barracks keeps its visible defensive function.
    for i in range(4):
        x=20+i*1.4
        beam("barracks reinforced spear standard",
             (x,5.6,.2),(x,5.6,3.4),.095,"iron",cat)
def render(stage,name,size,portrait=False):
    scene=bpy.context.scene
    cam=scene.camera
    if cam is None:
        bpy.ops.object.camera_add(location=(36,-51,39))
        cam=bpy.context.object;scene.camera=cam
    cam.location=(30,-48,43) if portrait else (36,-51,39)
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.type="ORTHO";cam.data.ortho_scale=57 if portrait else 69
    scene.render.engine="CYCLES"
    scene.cycles.samples=22
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.render.resolution_x,scene.render.resolution_y=size
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.render.filepath=str(PREV/name)
    # Avoid making data look better by changing the baseline exposure/camera.
    scene.view_settings.exposure=.07
    bpy.ops.render.render(write_still=True)
    p=PREV/name
    if p.stat().st_size<120000 or p.read_bytes()[:8]!=b'\x89PNG\r\n\x1a\n':
        raise RuntimeError("Not a real V10 Blender capture: "+str(p))
    FRAMES.append({"stage":stage,"file":name,"dimensions":size,"bytes":p.stat().st_size})
    print("V10_GENUINE_BLENDER_FRAME",name,p.stat().st_size)
def export(stage):
    path=DEST/("DirectorV10_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",export_apply=True)
    b=path.read_bytes()
    if b[:4]!=b"glTF" or struct.unpack_from('<I',b,8)[0]!=len(b):
        raise RuntimeError("Failed to write physical GLB: "+str(path))
    n=struct.unpack_from('<I',b,12)[0]
    j=json.loads(b[20:20+n])
    if len(j.get("images",[]))<8 or len(j.get("meshes",[]))<500:
        raise RuntimeError("V10 tile textures or geometry missing")
    e={"stage":stage,"meshes":len(j["meshes"]),"materials":len(j["materials"]),
       "images":len(j["images"]),"bytes":len(b)}
    STAGES.append(e)
    print("V10_REAL_GLB_EXPORT",json.dumps(e))
def main():
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        src=INPUT/("DirectorV9_Stage"+str(stage)+".glb")
        if not src.exists():raise RuntimeError("Verified V9 input missing "+str(src))
        bpy.ops.import_scene.gltf(filepath=str(src),import_pack_images=True)
        T.clear();build_atlases()
        replaced=rematerial()
        landmarks(stage)
        life_and_ground()
        if stage==1:
            render(stage,"DirectorV10_BastionI_1600x900.png",[1600,900])
        if stage==5:
            render(stage,"DirectorV10_BastionV_1600x900.png",[1600,900])
            render(stage,"DirectorV10_BastionV_Portrait_720x1200.png",[720,1200],True)
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"DirectorV10_Editable_Source.blend"))
        export(stage)
        print("V10_PHYSICAL_TILE_REPLACEMENT",stage,json.dumps(replaced))
    evidence={"project":"Eldoria CC0 Art System","version":"V10 physically textured stone, slate, oak and monument geometry",
              "source_run":43886721399,"source_rights":"only owner-authored Eldoria meshes",
              "vendor_assets_used":False,"ai_illustration_used_as_model":False,
              "blender_render":True,"native_unity_tested":False,
              "iphone_tested":False,"art_approved":False,
              "source_file":"DirectorV10_Editable_Source.blend",
              "stages":STAGES,"frames":FRAMES}
    (PREV/"v10-evidence.json").write_text(json.dumps(evidence,indent=2)+"\n")
    print("V10_AUTHORED_MATERIALS_REAL_ART_PASS",len(STAGES),len(FRAMES))
if __name__=="__main__":
    main()
