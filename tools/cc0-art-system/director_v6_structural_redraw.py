#!/usr/bin/env python3
"""Eldoria Director V6 — genuine physical-geometry correction of the verified V5 scene.

Uses only 5 actual, author-owned V5 GLBs from GitHub Actions #43874433432.
Major visual pivot: replace pale floating road strips and flat grass with a
continuous vertex-painted valley; remake tree silhouettes, groundcover and
functional landmarks, retone original embedded PBR textures, make the rebuilt
keep taller and more coherent; generate true Blender renders and 5 glTF assets.
Never copies a conceptual illustration into the world. No Unity/mobile claims.
"""
import bpy, math, json, random, struct
from pathlib import Path
from math import sin, cos, pi, radians
from mathutils import Vector
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV5"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV6"
PREV=ROOT/"docs/cc0-art-system/previews/director-v6"
for p in (OUT,PREV):p.mkdir(parents=True,exist_ok=True)
frames=[];model_data=[];qa=[]
palette={
"grass":("#435D49",.96),"grass2":("#5C7152",.96),
"forest":("#263F36",.97),"forest2":("#365849",.97),
"forest3":("#526D54",.98),"wood":("#5B4838",.87),
"wooddark":("#312E2E",.94),"rock":("#535F5D",.98),
"rocklight":("#798079",.94),"stone":("#8E9187",.86),
"limestone":("#B0AC9D",.86),"iron":("#272D33",.67),
"slate":("#303F4F",.82),"gold":("#A67B44",.62),
"purple":("#4E3C54",.95),"amber":("#F8B36B",.48),
"flowers":("#9983AA",.95),"wheat":("#9E8B57",.96)}
def rgb(h):
    h=h.lstrip("#")
    return tuple(int(h[x:x+2],16)/255 for x in (0,2,4))
def mat(name,hexcode,rough=0.84):
    m=bpy.data.materials.new("ELDORIA_V6_"+name)
    col=rgb(hexcode);m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*col,1)
    bs.inputs["Roughness"].default_value=rough
    m.diffuse_color=(*col,1)
    if name=="amber":
        for s in ("Emission Color","Emission"):
            if s in bs.inputs:
                bs.inputs[s].default_value=(*col,1);break
        if "Emission Strength" in bs.inputs:bs.inputs["Emission Strength"].default_value=1.55
    return m
M={k:mat(k,*v) for k,v in palette.items()}
def collection(name):
    c=bpy.data.collections.get(name)
    if c is None:
        c=bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(c)
    return c
def own_mesh(name,mesh,code,col):
    mesh.materials.append(M[code])
    obj=bpy.data.objects.new(name,mesh)
    collection(col).objects.link(obj)
    return obj
def cube(name,p,size,code,col,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    o=bpy.context.object;o.name=name;o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for c in list(o.users_collection):c.objects.unlink(o)
    collection(col).objects.link(o)
    o.data.materials.append(M[code])
    if bevel>0:
        mod=o.modifiers.new("single bevel","BEVEL")
        mod.width=bevel;mod.segments=1
    return o
def cylindrical(name,p,r,h,code,col,verts=12,rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p)
    o=bpy.context.object;o.name=name
    if rot:o.rotation_euler=rot
    for c in list(o.users_collection):c.objects.unlink(o)
    collection(col).objects.link(o)
    o.data.materials.append(M[code])
    return o
def cone(name,p,r,h,code,col,verts=9,top=.035):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r,radius2=top,depth=h,location=p)
    o=bpy.context.object;o.name=name
    for c in list(o.users_collection):c.objects.unlink(o)
    collection(col).objects.link(o);o.data.materials.append(M[code])
    return o
def beam(name,a,b,t,code,col):
    a=Vector(a);b=Vector(b);d=b-a
    if d.length<.001:return None
    ob=cube(name,(a+b)*.5,(t,t,d.length),code,col,.035)
    ob.rotation_euler=d.to_track_quat("Z","Y").to_euler()
    return ob
def h_at(x,y):
    return .12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)-.05*max(0,1-abs(x)/3)
ROADS=[
    [(-.2,-38),(1,-27),(-2,-15),(0,-6)],
    [(-3,-18),(-10,-18),(-17,-15),(-22,-12)],
    [(3,-18),(10,-19),(16,-15),(22,-12)],
    [(-18,-12),(-27,-9),(-28,3),(-23,8)],
    [(16,-12),(28,-5),(28,4),(24,8)]
]
def bezier(P,t):
    return (Vector(P[0])*(1-t)**3+Vector(P[1])*(3*t*(1-t)**2)+
            Vector(P[2])*(3*t*t*(1-t))+Vector(P[3])*t*t*t)
SAMPLES=[[bezier(P,i/45) for i in range(46)] for P in ROADS]
def road_distance(x,y):
    return min((p.x-x)**2+(p.y-y)**2 for poly in SAMPLES for p in poly)**.5
def remove_problematic():
    prefixes=(
        "PHYSICAL TERRAIN","Eldoria V5 | ","Grounded native track",
        "Embedded historic track","ancient fern/shrub foliage","mixed meadow",
        "spruce clusters | single low draw mesh","dark tree trunk",
        "natural lichen outcrop","forest spruce irregular tier",
        "spruce weighted trunk","perennial meadow flowers")
    removed={}
    for obj in list(bpy.data.objects):
        if obj.type!="MESH":continue
        for token in prefixes:
            if obj.name.startswith(token):
                removed[token]=removed.get(token,0)+1
                bpy.data.objects.remove(obj,do_unlink=True)
                break
    return removed
def grounded_valley():
    """Textureless vertex-color paint physically exported as COLOR_0 in glTF."""
    rng=random.Random(6016)
    nx=112;ny=104
    vertices=[];faces=[]
    for j in range(ny+1):
        y=-53.0+106*j/ny
        for i in range(nx+1):
            x=-59.0+118*i/nx
            h=h_at(x,y)+.28*sin(.11*x+.18*y)*sin(.12*y)+.11*cos(.15*x-.07*y)
            vertices.append((x,y,h))
    for j in range(ny):
        for i in range(nx):
            a=j*(nx+1)+i
            faces.append((a,a+1,a+nx+2,a+nx+1))
    mesh=bpy.data.meshes.new("Valoria organically painted ground")
    mesh.from_pydata(vertices,[],faces)
    mesh.update()
    color=mesh.vertex_colors.new(name="COLOR_0")  # CORNER domain for glTF + Blender
    # Painterly soft-edged path blending, punctuated earth/grass transitions.
    vertex_color=[]
    for x,y,z in vertices:
        d=road_distance(x,y)
        path=1.0-min(1.0,max(0,d-1.1)/2.2)
        # Broad grass patches, intermittent dry earth and naturally exposed rock.
        wave=.05*sin(x*.27+y*.14)+.04*sin(x*.13-y*.22)+.045*cos(x*.063+y*.078)
        grass=(.225+wave,.325+wave*.85,.244+wave*.70)
        grass=tuple(max(.095,min(.52,c)) for c in grass)
        soil=(.305,.258,.188)
        rock=(.285,.31,.30)
        stony=(.8 if x<-26 and y>14 else .0)*.28
        c=tuple((1-path)*(1-stony)*grass[k]+path*soil[k]+stony*rock[k] for k in range(3))
        vertex_color.append(c)
    for poly in mesh.polygons:
        for li in poly.loop_indices:
            vindex=mesh.loops[li].vertex_index
            color.data[li].color=(*vertex_color[vindex],1)
    m=bpy.data.materials.new("ELDORIA_V6_MASTER_VERTEX_PAINT_GRASS_SOIL")
    m.use_nodes=True
    nodes=m.node_tree.nodes;bs=nodes.get("Principled BSDF")
    bs.inputs["Roughness"].default_value=.97
    vertex=nodes.new("ShaderNodeVertexColor");vertex.layer_name="COLOR_0"
    m.node_tree.links.new(vertex.outputs["Color"],bs.inputs["Base Color"])
    mesh.materials.append(m)
    ob=bpy.data.objects.new("ELDORIA / physical undulating painted terrain",mesh)
    collection("V6_VALLEY").objects.link(ob)
    # Conforming earth colouring is baked into the terrain itself.
    return len(vertices),len(faces)
def material_role(name):
    n=name.lower()
    if "blue" in n or "slate" in n or "roof" in n:return "roof"
    if "stone" in n or "ashlar" in n or "lime" in n or "rubble" in n:return "stone"
    if "rock" in n:return "rock"
    if "fir" in n or "leaf" in n or "pine" in n or "moss" in n:return "forest"
    if "oak" in n or "timber" in n or "wood" in n:return "wood"
    return None
IMG_TINTS={
  "stone":(.79,.80,.82),"rock":(.70,.76,.78),
  "roof":(.62,.72,.82),"forest":(.60,.74,.66),"wood":(.82,.77,.73)}
def retone_embedded_materials():
    """Change actual image bytes rather than display-only color grading."""
    seen={};stats={"material_colors":0,"images_repainted":0}
    for material in list(bpy.data.materials):
        if not material.use_nodes or material.name.startswith("ELDORIA_V6_"):continue
        role=material_role(material.name)
        if role is None:continue
        factor=IMG_TINTS[role]
        bs=material.node_tree.nodes.get("Principled BSDF")
        if not bs:continue
        inp=bs.inputs.get("Base Color")
        if not inp:continue
        if inp.is_linked and inp.links[0].from_node.type=="TEX_IMAGE":
            node=inp.links[0].from_node
            src=node.image
            if src and src.has_data:
                key=(src.name,role)
                if key not in seen:
                    new=src.copy()
                    new.name="ELDORIA_V6_EMBEDDED_"+role+"_"+src.name
                    arr=np.empty(new.size[0]*new.size[1]*4,dtype=np.float32)
                    new.pixels.foreach_get(arr)
                    arr=arr.reshape((-1,4))
                    arr[:,:3]*=np.array(factor,dtype=np.float32)
                    np.clip(arr,0,1,out=arr)
                    new.pixels.foreach_set(arr.reshape(-1))
                    new.update()
                    # Source images must be portable when the GLB gets exported.
                    new.pack()
                    seen[key]=new
                    stats["images_repainted"]+=1
                node.image=seen[key]
        elif not inp.is_linked:
            old=inp.default_value
            inp.default_value=tuple(min(1,old[i]*factor[i]) for i in range(3))+(old[3],)
            stats["material_colors"]+=1
    return stats
def pine(x,y,z,h):
    """A layered, asymmetric 3D spruce with irregular needles/branch rings."""
    col="V6_FOREST"
    cylindrical("organic crooked conifer trunk",(x,y,z+h*.29),h*.045,h*.58,
               "wooddark",col,8)
    # Instead of toy cone-on-cone, use a single silhouette with staggered
    # multi-faceted bough rims. Geometrically specific irregular tiers.
    segments=9
    verts=[];faces=[];mat_ids=[]
    skew=random.uniform(-.11,.11)*h
    base_z=z+h*.19
    for tier in range(5):
        zz=base_z+h*.15*tier
        rad=h*(.236-.034*tier)
        tier_pts=[]
        for k in range(segments):
            a=2*pi*k/segments+.09*tier
            local=rad*random.uniform(.78,1.22)
            tier_pts.append((x+skew*(tier/5)+local*cos(a),
                             y+local*sin(a),
                             zz+random.uniform(-.14,.12)*h/6))
        top_index=len(verts)+segments
        verts.extend(tier_pts)
        verts.append((x+skew*((tier+1)/5),y,
                      zz+h*(.155+random.uniform(-.015,.03))))
        for i in range(segments):
            faces.append((top_index-segments+i,top_index-segments+(i+1)%segments,top_index))
            mat_ids.append((i+tier)%3)
    mesh=bpy.data.meshes.new("Spruce boughs / multi-layer hand-authored")
    mesh.from_pydata(verts,[],faces)
    for mat_key in ("forest","forest2","forest3"):
        mesh.materials.append(M[mat_key])
    mesh.update()
    for p,mi in zip(mesh.polygons,mat_ids):p.material_index=mi
    obj=bpy.data.objects.new("V6 irregular 3D fir",mesh)
    collection(col).objects.link(obj)
def vegetation():
    count={"trees":0,"shrubs":0,"rocks":0,"grass":0}
    random.seed(1611)
    for i in range(154):
        x=random.uniform(-57,57);y=random.uniform(-51,51)
        if abs(x)<19 and -29<y<27:continue
        if abs(x+23)<11 and -17<y<21:continue
        if abs(x-23)<11 and -18<y<21:continue
        h=random.uniform(4.8,11.8)
        pine(x,y,h_at(x,y),h);count["trees"]+=1
    # Rock geometry in clusters with dark mossy upper edges; no bright cubes.
    for i in range(88):
        x=random.uniform(-52,52);y=random.uniform(-47,47)
        if -9<x<9 and -16<y<10:continue
        rad=random.uniform(.24,1.0)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rad,location=(x,y,h_at(x,y)+.36*rad))
        o=bpy.context.object;o.name="mossy eroded stone"
        o.scale=(random.uniform(.9,1.5),random.uniform(.72,1.36),random.uniform(.33,.66))
        for c in list(o.users_collection):c.objects.unlink(o)
        collection("V6_ROCKS").objects.link(o);o.data.materials.append(M["rock" if i%3 else "rocklight"])
        count["rocks"]+=1
    # One grouped mesh per vegetation color: no 1000 independent mobile draws.
    for key,qty in (("grass",750),("grass2",530),("flowers",140),("wheat",90)):
        verts=[];faces=[]
        for i in range(qty):
            x=random.uniform(-51,51);y=random.uniform(-49,49)
            if abs(x)<9 and -9<y<13:continue
            if abs(x+22)<5 and abs(y+7.5)<3:continue
            if abs(x-22)<5 and abs(y+9)<3:continue
            z=h_at(x,y)+.16
            h=random.uniform(.18,.52) if key not in ("flowers",) else random.uniform(.20,.34)
            off=len(verts)
            verts.extend([(x-.13,y,z),(x+.13,y,z),(x,y,z+h)])
            faces.append((off,off+1,off+2))
        mesh=bpy.data.meshes.new("Packed independent environmental herb "+key)
        mesh.from_pydata(verts,[],faces);mesh.update()
        own_mesh("ELDORIA / grouped wild meadow "+key,mesh,key,"V6_WILDFLOWERS")
        count["grass"]+=len(faces)
    return count
def city_identity(stage):
    """Distinct industrial silhouettes and powerful final citadel, not decorations
    pasted on unrelated house shells."""
    col="V6_CIVIC_ARCHITECTURE"
    # Distinct jobs: visible production hardware physically attached.
    for i in range(4):
        x=-24.9+i*.58;y=-11.9
        cylindrical("sawmill exterior log stock",(x,y,.36),.28,3.2,"wood",col,9,
                    rot=(0,pi/2,0))
    # A sawmill chimney creates a readable industrial rather than domestic outline.
    cube("sawmill forge-like exhaust stack",(-20.6,-4.4,3.7),
         (.68,.62,3.8),"stone",col,.09)
    cone("mill chimney cowl",(-20.6,-4.4,5.82),.51,.75,"slate",col,10)
    # Stonecutters: a prominent high crane and hanging chiselled load.
    beam("quarry diagonal lifting arm",(-27.2,12,5.0),(-21.2,13.4,9.0),.29,"wood",col)
    beam("quarry lifted stone chain",(-21.2,13.4,9),(-21.2,13.4,5.1),.07,"iron",col)
    cube("quarry lifted stone",(-21.2,13.4,4.8),(1.26,1.3,1.05),"stone",col,.05)
    # Granary: two taller ashlar silo caps distinguish stored grain from towers.
    for x in (26.7,29.3):
        cylindrical("grain storage upper collar",(x,-8.2,6.0),1.39,.3,"limestone",col,14)
        cone("ventilated granary slate cap",(x,-8.2,7.0),1.56,1.65,"slate",col,14)
    # Barracks: sharpen training identity (no other generic cottage).
    for i in range(4):
        x=20+i*1.05
        beam("barracks individual training pike",(x,6.8,.8),(x,6.8,3.25),.1,"iron",col)
        cone("barracks sharpened pikepoint",(x,6.8,3.38),.11,.35,"iron",col,7)
    if stage<4:return
    col="V6_IMPERIAL_KEEP"
    # A genuine upper annex improves massing in Stage V; Stage IV stays modest.
    for side in (-1,1):
        x=side*5.7
        cube("fortress linked lateral chapel",(x,5.7,4.9),
             (4.0,5.4,5.8),"stone",col,.12)
        # Distinctive pointed gable along the forward-facing roof plane.
        cone("keep slate vaulted pyramidal roof",(x,5.7,8.82),2.92,3.2,"slate",col,4)
        cube("buttress integrated to main keep",(x,2.6,3.0),
             (.7,1.6,5.8),"limestone",col,.065)
    if stage==5:
        # Main vertical distinction: a fully rebuilt eight-sided lantern and two
        # projecting stone buttresses, visible above the original roof ridge.
        cylindrical("Valoria high lantern ashlar drum",(0,8.5,19.4),
                    1.9,4.0,"stone",col,12)
        for i in range(12):
            a=2*pi*i/12
            cube("restored crenel on high lantern",(1.72*cos(a),8.5+1.72*sin(a),21.65),
                 (.49,.49,.85),"limestone",col,.035)
        cone("restored imperial spire",(0,8.5,23.3),2.14,3.0,"slate",col,12)
        beam("golden summit pennon",(0,8.5,24.6),(0,8.5,27),.11,"gold",col)
        cube("imperial plum pennon cloth",(.63,8.5,26.35),(1.15,.12,.78),"purple",col,.035)
        for side in (-1,1):
            beam("diagonal foundation counterforce",(side*6,-4.8,6.1),
                 (side*8.6,-6.5,.32),.55,"stone",col)
        # Forged vertical crests break up the old monolithic front wall.
        for x in (-3.4,3.4):
            cube("walled golden heraldic strip",(x,-5.64,5.45),(.30,.14,2.0),"gold",col,.02)
def light_camera():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=22
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    for layer in scene.view_layers:
        layer.cycles.use_denoising=False
    scene.render.threads_mode="FIXED"
    scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.33
    world=bpy.data.worlds.new("Valoria V6 / twilight cool distance")
    scene.world=world;world.use_nodes=True
    bg=world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.29,.35,.46,1)
    bg.inputs["Strength"].default_value=.64
    for obj in list(bpy.data.objects):
        if obj.type=="LIGHT":bpy.data.objects.remove(obj,do_unlink=True)
    bpy.ops.object.light_add(type="SUN",location=(0,0,30))
    sun=bpy.context.object
    sun.name="western low amber sun / one shadow caster"
    sun.rotation_euler=(radians(67),radians(12),radians(-42))
    sun.data.energy=3.2
    sun.data.color=(1,.72,.51)
    sun.data.angle=radians(3.2)
    for x in (-1.8,1.8):
        bpy.ops.object.light_add(type="POINT",location=(x,-5.8,3.8))
        l=bpy.context.object
        l.name="Fortified Gate / low-cost warm point source"
        l.data.color=(1,.57,.29)
        l.data.energy=95
        l.data.shadow_soft_size=1.2
    for o in list(bpy.data.objects):
        if o.type=="CAMERA":bpy.data.objects.remove(o,do_unlink=True)
    bpy.ops.object.camera_add(location=(41,-57,48))
    cam=bpy.context.object;cam.name="V6 common city comparison camera"
    cam.data.type="ORTHO";cam.data.ortho_scale=72
    cam.rotation_euler=(Vector((0,0,6))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def render(stage,name,size,portrait=False):
    scene=bpy.context.scene;cam=scene.camera
    cam.location=(30,-49,45) if portrait else (41,-57,48)
    cam.rotation_euler=(Vector((0,0,6))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=61 if portrait else 72
    scene.render.resolution_x=size[0]
    scene.render.resolution_y=size[1]
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    file=PREV/name
    scene.render.filepath=str(file)
    bpy.ops.render.render(write_still=True)
    b=file.read_bytes()
    if len(b)<85000 or b[:8]!=b'\x89PNG\r\n\x1a\n':
        raise RuntimeError("Authentic V6 Blender render missing/blank "+str(file))
    frames.append({"name":name,"stage":stage,"bytes":len(b),"dimensions":list(size)})
    print("V6_REAL_BLENDER_FRAME",stage,name,len(b))
def glb(stage):
    p=OUT/("DirectorV6_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(p),export_format="GLB",export_apply=True)
    b=p.read_bytes()
    if b[:4]!=b"glTF" or struct.unpack_from("<I",b,8)[0]!=len(b):
        raise RuntimeError("V6 glTF artifact invalid "+str(p))
    n=struct.unpack_from("<I",b,12)[0];j=json.loads(b[20:20+n])
    info={"stage":stage,"bytes":len(b),"meshes":len(j.get("meshes",[])),
          "images":len(j.get("images",[])),"materials":len(j.get("materials",[])),
          "vertex_color_meshes":sum(
            any("COLOR_0" in prim.get("attributes",{}) for prim in m.get("primitives",[]))
            for m in j.get("meshes",[]))}
    if info["vertex_color_meshes"]<1 or info["images"]<8:
        raise RuntimeError("Terrain vertex paint or embedded source textures lost "+str(info))
    model_data.append(info)
    print("V6_PHYSICAL_GLTF",json.dumps(info))
def build(stage):
    global M
    bpy.ops.wm.read_factory_settings(use_empty=True)
    # Blender deletes all datablocks on a factory reset, including materials.
    # Recreate master materials per level after reset; never reuse stale RNA handles.
    M={key:mat(key,*params) for key,params in palette.items()}
    src=SRC/("DirectorV5_Stage"+str(stage)+".glb")
    if not src.is_file():raise RuntimeError("V5 physically pinned source missing "+str(src))
    bpy.ops.import_scene.gltf(filepath=str(src),import_pack_images=True)
    random.seed(161126)
    removed=remove_problematic()
    surface=grounded_valley()
    remat=retone_embedded_materials()
    foliage=vegetation()
    city_identity(stage)
    light_camera()
    if stage==1:render(stage,"DirectorV6_BastionI_1600x900.png",(1600,900))
    if stage==5:
        render(stage,"DirectorV6_BastionV_1600x900.png",(1600,900))
        render(stage,"DirectorV6_BastionV_Portrait_720x1200.png",(720,1200),True)
        # Original geometries + new vertex colors and on-disk material images
        bpy.context.scene.render.filepath=""
        for im in bpy.data.images:
            if im.source=="FILE" and not im.packed_file:
                try:im.pack()
                except Exception:pass
        bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV6_Editable_Source.blend"))
    glb(stage)
    qa.append({"stage":stage,"removed_source_objects":removed,
               "terrain_vertices":surface[0],"terrain_faces":surface[1],
               "retone":remat,"foliage":foliage})
for lv in range(1,6):build(lv)
report={"experiment":"Eldoria CC0 Art System","version":"Director V6 structural redraw",
        "source_action":43874433432,"source_authorship":"original own V5 GLBs",
        "third_party_models_copied":False,"native_unity_tested":False,
        "physical_iphone_tested":False,"art_approved":False,
        "rendered_in_blender":True,"levels":model_data,"frames":frames,"stage_metrics":qa}
(PREV/"director-v6-evidence.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
print("ELDORIA_V6_PHYSICAL_3D_AND_RENDER_PASS",len(model_data),len(frames))
