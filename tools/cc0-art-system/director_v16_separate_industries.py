#!/usr/bin/env python3
"""Eldoria CC0 Art System / Director V16: genuinely move every industry in REAL 3D.

Source: V15 five physical commercial-safe GLBs from Action 43907312243.
- Move the COMPLETE source district geometry (building + attached machinery/stock)
  outward by 8–11 world metres, keeping same five-stage progression.
- Completely repaint the physical terrain with winding tracks to NEW sites.
- Replace abandoned raised route slabs, make independent quarry/sawmill/
  barracks/granary identities from original geometry and material families.
- Render 4 true Blender image comparisons and 4 industry-detail captures;
  export 5 physical GLBs and editable Blender source.
No 2D concept generation. No unlicensed vendor geometry. NOT Unity/mobile QA.
"""
from pathlib import Path
from mathutils import Vector
from math import sin,cos,pi,radians,sqrt
import bpy,math,random,os,json,struct,hashlib,numpy as np

ROOT=Path(__file__).resolve().parents[2]
INPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV15"
OUTPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV16"
PREVIEW=ROOT/"docs/cc0-art-system/previews/director-v16"
for p in (OUTPUT,PREVIEW): p.mkdir(parents=True,exist_ok=True)
R=random.Random(16112026)
M={}
STAGES=[]
SHOTS=[]
RELOCATION=[]
SOURCE_VERSION=43907312243
OLD={
 "sawmill":(-22.,-7.5),
 "quarry":(-23.5,11.8),
 "barracks":(23.8,11.4),
 "granary":(22.,-9.2)
}
NEW={
 "sawmill":(-32.0,-12.5),
 "quarry":(-33.2,18.4),
 "barracks":(34.5,18.4),
 "granary":(32.0,-15.5)
}
PREFIX={
 "sawmill":("sawmill","aserradero","mill chimney","freshly split lumber",
             "timber workshop","v13 large operating wheel","v14 sawmill",
             "aserradero covered","waterwheel workshop"),
 "quarry":("quarry","cantera","worked quarry","cut granite","oak quarry",
           "suspended unfinished stone","v13 quarry","v14 quarry",
           "stone-cutting workshop"),
 "barracks":("barracks","cuartel","guardhouse","military","fortified barracks",
             "v13 barracks","v14 guard rack","military yard","military purple"),
 "granary":("granary","granero","grain","harvest","burlap","imperial grainhouse",
            "ventilated granary","v13 stacked grain","v14 real tied","grain-silo")
}
EXCLUDED=("v8 continuous painted","v14 physically batched",
          "v11 spruce","v11 evergreen","icosphere","stream broken",
          "riverbank","v13 connected crossing","v13 main hall",
          "v13 ground-connected","v13 cathedral","v14 angular natural boulder")
def lin(h):
    s=h.lstrip("#"); rgb=[int(s[i:i+2],16)/255. for i in (0,2,4)]
    return tuple(x/12.92 if x<.04045 else ((x+.055)/1.055)**2.4 for x in rgb)
def mat(name,color,rough=.82,metallic=0,emit=0):
    m=bpy.data.materials.new("V16_"+name)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    col=lin(color)
    bs.inputs["Base Color"].default_value=(*col,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metallic
    if emit:
        socket="Emission Color" if "Emission Color" in bs.inputs else "Emission"
        bs.inputs[socket].default_value=(*col,1)
        bs.inputs["Emission Strength"].default_value=emit
    m.diffuse_color=(*col,1)
    return m
def materials():
    source={}
    for role,name in (("stone","V8_master_ashlar"),
                      ("roof","V8_master_roof"),("wood","V8_master_timber")):
        m=bpy.data.materials.get(name)
        if m is None: raise RuntimeError("Missing physically textured V15 atlas "+name)
        source[role]=m
    return {**source,
      "dressed":mat("AgedLimestone","#A5A094"),
      "dark":mat("FortressMortar","#394449"),
      "bronze":mat("ImperialAgedBronze","#AA8A59",.56,.37),
      "iron":mat("ForgedMetal","#343940",.48,.50),
      "wine":mat("ValoriaWine","#493653"),
      "wheat":mat("WheatOchre","#AF945E"),
      "timber":mat("FreshHewnOak","#72503A"),
      "soil":mat("CompactDirt","#70634D"),
      "water":mat("ForestCreek","#365B63",.23,.09),
      "foam":mat("CreekFoam","#B5C2B5"),
      "fern":mat("WoodlandSage","#536C4C"),
      "grass":mat("ForestMoss","#49634B"),
      "flowers":mat("Lavender","#896C91"),
      "granite":mat("QuarryGranite","#666F6D"),
      "amber":mat("WindowAmber","#C8965D",.7,0,1.5)}
def collection(name):
    c=bpy.data.collections.get(name)
    if c is None:
        c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c)
    return c
def add(o,key,group):
    o.data.materials.append(M[key])
    for c in list(o.users_collection):c.objects.unlink(o)
    collection(group).objects.link(o)
    return o
def cub(name,loc,size,key,group="V16_ARCH",bev=.03):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object;o.name=name;o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    add(o,key,group)
    if bev:
        mod=o.modifiers.new("Dressed edges","BEVEL");mod.width=bev;mod.segments=1
        o.modifiers.new("weighted normals","WEIGHTED_NORMAL")
    return o
def cylinder(name,loc,radius,depth,key,group="V16_ARCH",verts=12,rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=depth,location=loc)
    o=bpy.context.object;o.name=name
    if rotation:o.rotation_euler=rotation
    return add(o,key,group)
def cone(name,loc,radius,top,depth,key,group="V16_ARCH",verts=9):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=radius,radius2=top,depth=depth,location=loc)
    o=bpy.context.object;o.name=name
    return add(o,key,group)
def beam(name,a,b,width,key,group="V16_ARCH"):
    a,b=Vector(a),Vector(b);d=b-a
    if d.length<.001:return None
    o=cub(name,(a+b)*.5,(width,width,d.length),key,group,.013)
    o.rotation_euler=d.to_track_quat("Z","Y").to_euler()
    return o
def geom(name,verts,faces,key,group="V16_ARCH"):
    mesh=bpy.data.meshes.new(name+"_mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.materials.append(M[key]);mesh.update()
    uv=mesh.uv_layers.new(name="UV")
    for face in mesh.polygons:
        for li in face.loop_indices:
            co=mesh.vertices[mesh.loops[li].vertex_index].co
            uv.data[li].uv=(co.x*.14,co.y*.14)
    o=bpy.data.objects.new(name,mesh);collection(group).objects.link(o)
    return o
def ground_z(x,y):
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
           +.22*sin(x*.064-y*.083)*cos(y*.11)+.06*cos(.24*x+.18*y))
def identify(o):
    if o.type!="MESH" or any(o.name.lower().startswith(x) for x in EXCLUDED):
        return None
    name=o.name.lower()
    pos=o.matrix_world.translation
    for region in ("sawmill","quarry","barracks","granary"):
        if not name.startswith(PREFIX[region]):continue
        cx,cy=OLD[region]
        if abs(pos.x-cx)<17.0 and abs(pos.y-cy)<15.0:
            return region
    return None
def relocate(stage):
    groups={s:[] for s in OLD}
    for o in list(bpy.data.objects):
        region=identify(o)
        if region is not None:groups[region].append(o)
    out={}
    for region,objects in groups.items():
        if len(objects)<15:
            raise RuntimeError("Cannot move complete industrial district "+region+"; found "+str(len(objects)))
        before=OLD[region];after=NEW[region]
        delta=Vector((after[0]-before[0],after[1]-before[1],
                      max(-.23,min(.23,ground_z(*after)-ground_z(*before)))))
        for ob in objects:
            matrix=ob.matrix_world.copy()
            matrix.translation+=delta
            ob.matrix_world=matrix
        old_dist=sqrt(before[0]*before[0]+before[1]*before[1])
        new_dist=sqrt(after[0]*after[0]+after[1]*after[1])
        assert new_dist-old_dist>7,(region,old_dist,new_dist)
        out[region]={"objects_moved":len(objects),"old":before,"new":after,
                     "delta":tuple(round(x,3) for x in delta),
                     "old_distance":round(old_dist,3),"new_distance":round(new_dist,3)}
        print("V16_DISTRICT_PHYSICALLY_RELOCATED",stage,region,out[region])
    return out
def clear_obsolete_road_meshes():
    # Rebuild roads as an actual diffuse terrain texture; no walkway left
    # behind at the old site when whole industries are moved.
    markers=("grounded native track","embedded historic track",
             "route |","route scattered","road |","side lane uneven",
             "quarry track","granary service lane","sawmill supply lane",
             "barracks route")
    victims=[o for o in bpy.data.objects if o.type=="MESH"
             and o.name.lower().startswith(markers)]
    for o in victims:bpy.data.objects.remove(o,do_unlink=True)
    return len(victims)
def curve_points(P,count=66):
    pts=np.asarray(P,dtype=np.float32)
    t=np.linspace(0,1,count,dtype=np.float32)
    return ((1-t)**3)[:,None]*pts[0]+(3*(1-t)**2*t)[:,None]*pts[1]+(3*(1-t)*t*t)[:,None]*pts[2]+(t*t*t)[:,None]*pts[3]
def finish_terrain(stage):
    candidates=[o for o in bpy.data.objects if o.type=="MESH"
                and o.name.startswith("V8 continuous painted and textured city ground")]
    if len(candidates)!=1:raise RuntimeError("There is not exactly one existing physical city ground mesh")
    ground=candidates[0]
    resolution=896
    Y,X=np.mgrid[:resolution,:resolution].astype(np.float32)
    x=(X+.5)/resolution*112-56
    y=(Y+.5)/resolution*104-52
    # Multi-scale terrain variance: forest greens, warm loam, rocky erosion.
    rough=np.sin(x*.15+y*.09)*.044+np.sin(x*.083-y*.18)*.036
    noise=np.sin(x*.57+y*.33)*.018+np.cos(x*1.09-y*.69)*.011
    richness=rough+noise
    rgb=np.empty((resolution,resolution,4),dtype=np.float32)
    rgb[:,:,0]=.32+richness+.016*np.sin(x*.056)
    rgb[:,:,1]=.405+richness+.030*np.cos(y*.075+x*.036)
    rgb[:,:,2]=.31+richness*.86+.011*np.cos(y*.15)
    # True connected paths leading to all new buildings. Curved footprints,
    # narrow organic edges, no physically raised panels.
    paths=[
      ([(-.1,-27),(.5,-20),(-1,-13),(0,-7)],3.0),
      ([(-7,-9),(-13,-18),(-27,-21),(-32,-12.5)],1.95),
      ([(-7,2),(-15,14),(-28,9),(-33.2,18.4)],1.65),
      ([(8,1),(19,14),(27,14),(34.5,18.4)],1.75),
      ([(8,-8),(18,-17),(27,-19),(32,-15.5)],1.8)
    ]
    path_mask=np.zeros((resolution,resolution),dtype=np.float32)
    for points,width in paths:
        for v in curve_points(points,65):
            d2=(x-v[0])**2+(y-v[1])**2
            path_mask=np.maximum(path_mask,np.exp(-d2/(width*width*.62)))
    path_mask=np.clip(path_mask*.80,0,.92)
    rgb[:,:,0]=rgb[:,:,0]*(1-path_mask)+(.38+noise)*path_mask
    rgb[:,:,1]=rgb[:,:,1]*(1-path_mask)+(.33+noise)*path_mask
    rgb[:,:,2]=rgb[:,:,2]*(1-path_mask)+(.26+noise*.7)*path_mask
    # Real extracted rock gradient at quarry, not a black square platform.
    cliff=np.exp(-((x+34)**2/155+(y-19)**2/86))*.33
    rgb[:,:,0]=rgb[:,:,0]*(1-cliff)+.55*cliff
    rgb[:,:,1]=rgb[:,:,1]*(1-cliff)+.51*cliff
    rgb[:,:,2]=rgb[:,:,2]*(1-cliff)+.47*cliff
    rgb[:,:,:3]=np.clip(rgb[:,:,:3],.10,.85)
    rgb[:,:,3]=1.
    image=bpy.data.images.new("V16_RealTerrain_ToSeparatedBuildings",
                              width=resolution,height=resolution,alpha=True)
    image.pixels.foreach_set(rgb.ravel())
    image.file_format="PNG"
    image.filepath_raw=str(OUTPUT/"V16_Grounded_DistrictPaths.png")
    image.save()
    original=ground.data.materials[0]
    assert original and original.use_nodes
    material=original.copy();material.name="V16_RealPaintedDistrictTerrain"
    ground.data.materials[0]=material
    bs=material.node_tree.nodes.get("Principled BSDF")
    if bs is None:raise RuntimeError("No PBR terrain shader")
    for link in list(bs.inputs["Base Color"].links):
        material.node_tree.links.remove(link)
    uv=ground.data.uv_layers.new(name="V16_DistrictWorldUV")
    for face in ground.data.polygons:
        for li in face.loop_indices:
            p=ground.matrix_world @ ground.data.vertices[ground.data.loops[li].vertex_index].co
            uv.data[li].uv=((p.x+56)/112,(p.y+52)/104)
    tex=material.node_tree.nodes.new("ShaderNodeTexImage")
    tex.image=image
    uvmap=material.node_tree.nodes.new("ShaderNodeUVMap")
    uvmap.uv_map=uv.name
    material.node_tree.links.new(uvmap.outputs["UV"],tex.inputs["Vector"])
    material.node_tree.links.new(tex.outputs["Color"],bs.inputs["Base Color"])
    return {"terrain":ground.name,"resolution":resolution,"road_count":len(paths)}
def add_sawmill():
    c="V16_Sawmill_Independent"
    x,y=NEW["sawmill"]
    # Roofed timber yard and huge visible load platform.
    for zz in (0.48,1.12,1.76):
        for i in range(8):
            xx=x-3.0+i*.83
            beam("V16 separated sawmill long log-stack",(xx,y-5.8,zz),
                 (xx,y-2.25,zz),.22,"timber",c)
    for xx in (x-4.3,x+4.0):
        for yy in (y-4.7,y-2.4):
            beam("V16 large open sawmill stockyard upright",(xx,yy,.35),
                 (xx,yy,3.45),.28,"wood",c)
    # Large waterwheel next to actual water channel; avoid detached prop illusion.
    wheel=Vector((x-5.25,y-2.1,2.25))
    for i in range(18):
        a=2*pi*i/18;b=2*pi*(i+1)/18
        beam("V16 sawmill actual large wheel dark oak rim",
             wheel+Vector((0,2.25*cos(a),2.25*sin(a))),
             wheel+Vector((0,2.25*cos(b),2.25*sin(b))),.19,"wood",c)
    for i in range(8):
        a=2*pi*i/8
        beam("V16 waterwheel functional timber spokes",wheel,
             wheel+Vector((0,2.24*cos(a),2.24*sin(a))),.14,"timber",c)
    cylinder("V16 real iron waterwheel hub",wheel,.35,.35,"iron",c,12,(0,pi/2,0))
    # Actual narrow glistening stream feeding the mill; no tiled river plane.
    verts=[]
    faces=[]
    for i in range(18):
        t=i/17
        xx=x-6.4 + 5.1*t
        yy=y-4.0 + 1.8*sin(t*pi)
        w=1.0+.25*sin(t*4)
        verts.extend(((xx,yy-w,ground_z(xx,yy)-.045),
                      (xx,yy+w,ground_z(xx,yy)-.045)))
        if i:
            j=2*i;faces.append((j-2,j-1,j+1,j))
    geom("V16 physically modeled silver-blue millstream",verts,faces,"water",c)
    for i in range(10):
        xx=x-6.5+i*.53; yy=y-2.0+sin(i*.2)*1.5
        cub("V16 water-splashed mill rocks",(xx,yy,.22),(.45,.40,.38),"granite",c,.11)
def add_quarry():
    x,y=NEW["quarry"];c="V16_Quarry_Separated"
    # Terraced rock extraction, carved into a hillside, not stacked cuboid house.
    for row in range(3):
        for j in range(6-row):
            xx=x-5.0+(j+row*.32)*1.35
            yy=y+3.5+row*1.55
            z=.35+row*.70
            cub("V16 quarry staggered extraction ledge",(xx,yy,z),
                (1.37,1.12,.67+.4*row),"granite",c,.16)
    # Strong distinctive two-legged oak overhead crane above cut bedrock.
    for xx in (x-4,x+1.8):
        beam("V16 quarry crane braced timber feet",
             (xx,y+.6,.4),(xx,y+.6,7.2),.33,"wood",c)
    beam("V16 quarry tall crane beam",(x-5,y+.6,7.06),
         (x+3.5,y+.6,7.06),.37,"timber",c)
    beam("V16 quarry lifting wire",(x-.5,y+.6,6.9),(x-.5,y+.6,3.2),
         .065,"iron",c)
    cub("V16 hanging quarry stone",(x-.5,y+.6,2.8),(1.3,1.1,1.15),
        "dressed",c,.13)
    for j in range(22):
        xx=x+R.uniform(-5.9,5.0);yy=y+R.uniform(-5.0,2.8)
        cub("V16 weathered finished limestone stone stock",
            (xx,yy,.21),(R.uniform(.45,1.0),R.uniform(.35,.78),R.uniform(.33,.55)),
            R.choice(("stone","granite")),"V16_Quarry_Stock",.10)
def add_barracks():
    x,y=NEW["barracks"];c="V16_Barracks_Separated"
    # C-shaped protective training yard with recognizable independent silhouette.
    for j in range(6):
        xx=x-5.3+j*1.7
        cub("V16 military perimeter defensive low wall",
            (xx,y-6.8,.77),(1.45,.52,1.43),"stone",c,.08)
    for sign in (-1,1):
        cub("V16 military yard returned wing",
            (x+sign*5.4,y-5.1,1.25),(.75,3.3,2.30),"dressed",c,.09)
    for j in range(4):
        xx=x-3.4+j*2.25
        cylinder("V16 barracks strong circular target",
                 (xx,y-4.7,1.62),.57,.17,"wood",c,14,(pi/2,0,0))
        cylinder("V16 barracks bronze target center",
                 (xx,y-4.80,1.62),.13,.18,"bronze",c,12,(pi/2,0,0))
        beam("V16 barracks raised target support",
             (xx,y-4.45,.22),(xx,y-4.45,1.23),.16,"timber",c)
    for i in range(9):
        xx=x+3.2+i*.28
        beam("V16 barracks military spear stock",(xx,y-2.5,.27),
             (xx,y-2.5,3.11),.067,"wood",c)
        cone("V16 barracks dark spearhead",(xx,y-2.5,3.22),
             .12,.018,.31,"iron",c,6)
    # Corner drum tower reinforced, separate from fortress keep.
    cylinder("V16 forward barracks watch drum",(x+5.0,y+1.7,4.1),
             1.85,7.8,"stone",c,12)
    for i in range(10):
        a=2*pi*i/10
        xx=x+5+1.65*cos(a);yy=y+1.7+1.65*sin(a)
        cub("V16 watchtower carved crenel",(xx,yy,8.32),(.52,.51,.83),
            "dressed",c,.04)
def add_granary():
    x,y=NEW["granary"];c="V16_Granary_Separated"
    # Storage has two recognisable giant grain silos and its own loading dock.
    for j in range(2):
        xx=x+5.15+j*2.7
        cylinder("V16 grain silo standing stone shell",(xx,y+1.3,2.85),
                 1.46,5.65,"stone",c,14)
        cone("V16 slate vent hood on silo",(xx,y+1.3,6.54),
             1.76,.13,1.98,"roof",c,14)
        for z in (1.02,3.2,5.27):
            cylinder("V16 bronze structural binding",(xx,y+1.3,z),
                     1.49,.15,"bronze",c,14)
    for j in range(5):
        beam("V16 granary dock protective timber colonnade",
             (x-4.2+j*2.0,y-4.1,.22),
             (x-4.2+j*2.0,y-4.1,3.40),.21,"wood",c)
    for i in range(32):
        xx=x-5.0+(i%8)*.58; yy=y-6.15+(i//8)*.70
        cub("V16 roof-protected stacked grain sacks",(xx,yy,.32),
            (.48,.54,.57),"wheat",c,.17)
    # Visible source of harvest in a curved field; 500 wheat stalks as one mesh.
    verts=[];faces=[]
    for i in range(380):
        px=x+R.uniform(-3.8,5.6);py=y+R.uniform(-11.4,-7.6)
        n=len(verts);h=R.uniform(.6,1.12)
        verts.extend([(px-.06,py,.11),(px+.06,py,.11),(px,py,h)])
        faces.append((n,n+1,n+2))
    geom("V16 integrated batched real grain field",verts,faces,"wheat",c)
def flora():
    # High-detail edge-scatter only in free spaces, never cover the separated assets.
    vertices={"fern":[],"flowers":[]}
    faces={"fern":[],"flowers":[]}
    for i in range(620):
        x=R.uniform(-52,52);y=R.uniform(-42,41)
        if abs(x)<12 and -15<y<15:continue
        if any(abs(x-cx)<8 and abs(y-cy)<8 for cx,cy in NEW.values()):continue
        mat="fern" if i%4 else "flowers"
        t=R.uniform(.22,.72)
        z=ground_z(x,y)+.05
        verts=vertices[mat];tris=faces[mat];idx=len(verts)
        verts.extend(((x-.08,y-.02,z),(x+.08,y+.02,z),
                      (x+R.uniform(-.03,.03),y+R.uniform(-.03,.03),z+t)))
        tris.append((idx,idx+1,idx+2))
    for name in vertices:
        geom("V16 one physically batched forest meadow foliage "+name,
             vertices[name],faces[name],name,"V16_Terrain_Botany")
def rigs_and_cameras():
    scene=bpy.context.scene
    for o in list(bpy.data.objects):
        if o.type in ("LIGHT","CAMERA"):bpy.data.objects.remove(o,do_unlink=True)
    scene.render.engine="CYCLES"
    scene.cycles.samples=23
    scene.cycles.max_bounces=3
    scene.cycles.use_denoising=False
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.45
    world=bpy.data.worlds.new("V16 sunset warm/cool professional real-time target")
    scene.world=world;world.use_nodes=True
    bg=world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.41,.43,.51,1)
    bg.inputs["Strength"].default_value=.90
    bpy.ops.object.light_add(type="SUN",location=(0,0,45))
    sun=bpy.context.object;sun.name="V16 warm late directional light"
    sun.data.energy=3.23;sun.data.color=(1,.79,.61)
    sun.data.angle=radians(4.0)
    sun.rotation_euler=(radians(71),radians(8),radians(-34))
    for xx in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(xx,-6.1,3.4))
        fire=bpy.context.object;fire.data.energy=108
        fire.data.color=(1,.63,.34);fire.data.shadow_soft_size=.7
    bpy.ops.object.camera_add(location=(43,-61,48))
    cam=bpy.context.object;cam.name="V16 identical same-stage comparison camera"
    cam.data.type="ORTHO";cam.data.ortho_scale=89
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def shot(stage,name,sz,mode="wide",focus=None):
    cam=bpy.context.scene.camera
    target=Vector((0,0,5))
    if mode=="wide":cam.location=(43,-61,48);cam.data.ortho_scale=89
    elif mode=="close":cam.location=(29,-41,34);cam.data.ortho_scale=48
    elif mode=="portrait":cam.location=(40,-64,52);cam.data.ortho_scale=67
    elif mode=="district":
        x,y=NEW[focus]
        cam.location=(x+12,y-18,14)
        cam.data.ortho_scale=24
        target=Vector((x,y,2.8))
    cam.rotation_euler=(target-cam.location).to_track_quat("-Z","Y").to_euler()
    scene=bpy.context.scene;scene.render.resolution_x,scene.render.resolution_y=sz
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    path=PREVIEW/name
    scene.render.filepath=str(path)
    bpy.ops.render.render(write_still=True)
    raw=path.read_bytes()
    if len(raw)<100000 or raw[:8]!=b"\x89PNG\r\n\x1a\n":
        raise RuntimeError("V16 not genuine Blender render "+str(path))
    SHOTS.append({"stage":stage,"name":name,"size":sz,"bytes":len(raw)})
    print("V16_TRUE_BLENDER_RENDER",name,len(raw))
def gltf(stage):
    path=OUTPUT/("DirectorV16_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",export_apply=True)
    b=path.read_bytes()
    if b[:4]!=b"glTF" or struct.unpack_from("<I",b,8)[0]!=len(b):
        raise RuntimeError("V16 physical GLB export malformed")
    n=struct.unpack_from("<I",b,12)[0]
    doc=json.loads(b[20:20+n])
    names=[o.get("name","") for o in doc.get("nodes",[])]
    for key in ("V16 grain silo standing stone shell",
                "V16 forward barracks watch drum",
                "V16 quarry crane braced timber feet",
                "V16 sawmill actual large wheel dark oak rim"):
        if not any(o.startswith(key) for o in names):
            raise RuntimeError("Required genuine district model missing: "+key)
    if len(doc.get("meshes",[]))<500 or len(doc.get("images",[]))<9:
        raise RuntimeError("V16 lost original 3D atlas or PBR maps")
    return {"stage":stage,"meshes":len(doc["meshes"]),"images":len(doc["images"]),
            "materials":len(doc["materials"]),"bytes":len(b)}
def main():
    global M
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        path=INPUT/("DirectorV15_Stage"+str(stage)+".glb")
        if not path.exists():raise RuntimeError("Original physical GLB missing: "+str(path))
        bpy.ops.import_scene.gltf(filepath=str(path),import_pack_images=True)
        M=materials()
        moved=relocate(stage)
        removed=clear_obsolete_road_meshes()
        ground=finish_terrain(stage)
        add_sawmill()
        add_quarry()
        add_barracks()
        add_granary()
        flora()
        rigs_and_cameras()
        if stage==1:shot(stage,"DirectorV16_BastionI_1600x900.png",(1600,900))
        if stage==5:
            shot(stage,"DirectorV16_BastionV_1600x900.png",(1600,900))
            shot(stage,"DirectorV16_BastionV_Close_1600x900.png",(1600,900),"close")
            shot(stage,"DirectorV16_BastionV_Portrait_720x1200.png",(720,1200),"portrait")
            for k in ("sawmill","quarry","barracks","granary"):
                shot(stage,"DirectorV16_"+k+"_1024x768.png",(1024,768),"district",k)
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT/"DirectorV16_Editable_Source.blend"))
        model=gltf(stage)
        STAGES.append(model)
        RELOCATION.append({"stage":stage,"districts":moved,"old_route_meshes_removed":removed,"painted_world":ground})
        print("V16_TRUE_GLTF_AND_RELOCATIONS",stage,json.dumps(model))
    evidence={"project":"Eldoria CC0 Art System","release":"Director V16/real separated industry",
       "source_run_id":SOURCE_VERSION,"source_materials":"V15 owner authored + documented Poly Haven CC0",
       "no_concept_images_used":True,"no_vendor_3d_geometry_added":True,
       "blender_render_verified":True,"unity_verified":False,"iphone_verified":False,
       "art_approved":False,"reposition_by_world_units":RELOCATION,
       "physical_stages":STAGES,"captured_real_images":SHOTS}
    (PREVIEW/"v16-evidence.json").write_text(json.dumps(evidence,indent=2)+"\n")
    print("ELDORIA_CC0_V16_REAL_DISTRICTS_PASS",len(STAGES),len(SHOTS))
if __name__=="__main__":main()
