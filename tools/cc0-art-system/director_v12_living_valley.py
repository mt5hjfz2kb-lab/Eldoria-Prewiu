#!/usr/bin/env python3
"""Eldoria CC0 / Director V12: genuine architecture, terrain and forest overhaul.

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
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV12Structural"
PREVIEW=ROOT/"docs/cc0-art-system/previews/director-v12-structural"
for p in (DEST,PREVIEW): p.mkdir(parents=True,exist_ok=True)
R=random.Random(9112026)
STATS=[]
FRAMES=[]
M={}
def hexrgb(h):
    # Color management: #RRGGBB defines display-space sRGB values, but
    # Blender Principled BSDF color sockets use scene-linear RGB.
    # Feeding sRGB floats directly previously caused chalk-white masonry
    # and mint-grey firs despite their intended dark palette.
    s=h.lstrip('#')
    values=[int(s[i:i+2],16)/255.0 for i in (0,2,4)]
    def srgb_to_linear(v):
        return v/12.92 if v<=0.04045 else ((v+0.055)/1.055)**2.4
    return tuple(srgb_to_linear(v) for v in values)
def mat(name,hexcolor,rough=.86,emission=0):
    m=bpy.data.materials.new("V12_"+name)
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
        modifier=o.modifiers.new("V12 angled masonry edge","BEVEL")
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
    # Authored roof/slate and facade polygons require real UV coordinates.
    uv=mesh.uv_layers.new(name="V12_Projected_Scale_UV")
    for face in mesh.polygons:
        n=face.normal
        for li in face.loop_indices:
            p=mesh.vertices[mesh.loops[li].vertex_index].co
            if abs(n.z)>.55: xy=(p.x*.18,p.y*.18)
            elif abs(n.x)>abs(n.y): xy=(p.y*.18,p.z*.18)
            else: xy=(p.x*.18,p.z*.18)
            uv.data[li].uv=xy
    return obj
def peripheral_relief(x,y):
    """Add broad physical topography OUTSIDE gameplay building anchors.

    Central buildings, their precise interactive positions and the approach
    road remain unchanged. Back ridge and forest shoulders become genuine
    terrain displacement, never raised platforms beneath the buildings.
    """
    r=max(abs(x)/30.0,abs(y)/24.0)
    fade=max(0.,min(1.,(r-.82)/.62))
    fade=fade*fade*(3.-2.*fade)
    waves=(.72*sin(x*.089+y*.042)+.55*cos(y*.092-x*.053)
           +.37*sin(x*.154-y*.075))
    distant=max(0.,min(1.,(y-21.)/24.))
    quarry=max(0.,1.-((x+30.)/20.)**2-((y-29.)/18.)**2)
    return fade*(waves+1.35*distant+1.20*quarry)

def ground_z(x,y):
    # Source V8 height, plus one deterministic continuous topographic field.
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
            +.22*sin(x*.064-y*.083)*cos(y*.11)+.06*cos(.24*x+.18*y)
            +peripheral_relief(x,y))

def sculpt_owned_ground_mesh():
    ground=[o for o in bpy.data.objects if o.type=="MESH"
            and o.name.startswith("V8 continuous painted and textured city ground")]
    if len(ground)!=1:
        raise RuntimeError("V12_GROUND_SOURCE_NOT_UNIQUE: "+str(len(ground)))
    obj=ground[0]
    # The imported source mesh is exclusively owned by this experimental
    # scene, not referenced by the game's original serialized scene.
    verts=0
    max_shift=0.
    inv=obj.matrix_world.inverted_safe()
    for vertex in obj.data.vertices:
        wp=obj.matrix_world@vertex.co
        delta=peripheral_relief(wp.x,wp.y)
        if abs(delta)>1.e-6:
            local=inv@Vector((wp.x,wp.y,wp.z+delta))
            vertex.co=local
            max_shift=max(max_shift,abs(delta))
            verts+=1
    obj.data.update()
    if verts<500 or max_shift<.65 or max_shift>4.9:
        raise RuntimeError("V12_TERRAIN_RELIEF_NOT_GENUINE: "+str((verts,max_shift)))
    print("V12_REAL_TERRAIN_VERTICES_DISPLACED",verts,"max_m",round(max_shift,3))
    return (verts,round(max_shift,3))

def interlocking_oak_mesh(seed):
    """One shared adult oak crown composed of irregular linked icosahedra."""
    rng=random.Random(seed)
    p=[(-1,1,0),(1,1,0),(-1,-1,0),(1,-1,0),(0,-1,1),(0,1,1),
       (0,-1,-1),(0,1,-1),(1,0,-1),(1,0,1),(-1,0,-1),(-1,0,1)]
    F=[(0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),(1,5,9),
       (5,11,4),(11,10,2),(10,7,6),(7,1,8),(3,9,4),(3,4,2),
       (3,2,6),(3,6,8),(3,8,9),(4,9,5),(2,4,11),(6,2,10),
       (8,6,7),(9,8,1)]
    V=[];faces=[];indices=[]
    clusters=[(.0,.0,.81,.30),(-.23,.01,.66,.27),
              (.25,.07,.72,.24),(.02,-.26,.74,.29),
              (-.19,-.15,.96,.26),(.16,-.17,.99,.25),
              (.10,.21,.95,.24),(-.13,.27,.85,.23)]
    for idx,(x,y,z,r) in enumerate(clusters):
        off=len(V)
        x+=rng.uniform(-.05,.05);y+=rng.uniform(-.05,.05)
        r*=rng.uniform(.91,1.12)
        for px,py,pz in p:
            l=(px*px+py*py+pz*pz)**.5
            V.append((x+px/l*r,y+py/l*r*.85,z+pz/l*r*.70))
        for tri in F:
            faces.append(tuple(off+i for i in tri))
            indices.append((idx+seed+tri[0])%3)
    mesh=bpy.data.meshes.new("V12 natural broadleaf shared geometry "+str(seed))
    mesh.from_pydata(V,[],faces);mesh.update()
    for key in ("fir1","fir2","fir3"):mesh.materials.append(M[key])
    for fa,idx in zip(mesh.polygons,indices):fa.material_index=idx
    return mesh

def mixed_native_broadleaf_forest():
    rng=random.Random(120116)
    m=[interlocking_oak_mesh(i) for i in range(5)]
    trees=0
    for n in range(65):
        x=rng.uniform(-56,56);y=rng.uniform(-49,56)
        if abs(x)<26 and -29<y<27:continue
        if abs(x-23)<10 and -18<y<19:continue
        if abs(x+23)<10 and -18<y<21:continue
        h=rng.uniform(5.3,10.3)
        z=ground_z(x,y)
        root=bpy.data.objects.new("V12 ancient mixed forest oak %03d"%n,m[n%len(m)])
        col("V12_NATIVE_OAKS").objects.link(root)
        root.location=(x,y,z+h*.05)
        root.scale=(h*.68,h*.68,h*.84)
        root.rotation_euler.z=rng.uniform(-pi,pi)
        cylinder("V12 varied living oak trunk",(x,y,z+h*.30),
                 h*.067,h*.59,"trunk","V12_NATIVE_OAKS",9)
        trees+=1
    if trees<24:raise RuntimeError("V12_FOREST_DIVERSITY_TOO_LOW")
    print("V12_NATIVE_MIXED_OAKS_AUTHORED",trees)
    return trees

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
    mesh=bpy.data.meshes.new("V12 deep green natural spruce mesh "+str(seed))
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
        top=bpy.data.objects.new("V12 spruce living crown %03d"%i,meshes[i%3])
        col("V12_FOREST_CANOPY").objects.link(top)
        top.location=(x,y,root+.06)
        top.scale=(h,h,h)
        top.rotation_euler=(0,0,rng.uniform(-pi,pi))
        cylinder("V12 tapered trunk %03d"%i,(x,y,root+h*.34),
                 h*.033,h*.68,"trunk","V12_FOREST_TRUNKS",8)
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
            obj.name="V12 evergreen wild underbrush"
            obj.scale=(rng.uniform(.66,1.25),rng.uniform(.62,1.12),rng.uniform(.30,.56))
            attach(obj,rng.choice(("fir1","fir2","fir3","grass")),"V12_UNDERBRUSH")
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
        poly("V12 clustered meadow "+key,vertices,faces,key,"V12_MEADOW")
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
def prune_earlier_keep(stage):
    """Physical replacement, not overdrawing multiple previous versions.

    Source V8 includes several mutually intersecting L5 high-nave designs.
    The V12 solution removes ONLY the obsolete stage-V fabric and rebuilds a
    new singular high hall, connected chapel roof and dimensional buttresses.
    Main lower fortress, gate, tower positions and all I-IV stages are retained.
    """
    if stage<5:return 0
    prefixes=(
        "Valoria imperial restored upper hall",
        "imperial long hall",
        "BASTION V / actually roofed royal nave",
        "BASTION V / one coherent bronze ridge",
        "BASTION V / cut stone soffit",
        "BASTION V / royal window buttress",
        "BASTION V / carved ashlar course",
        "BASTION V / Valoria imperial sun emblem",
        "BASTION V / raised imperial rays",
        "BASTION V / regal wall pennon",
        "imperial high keep corner",
        "Monumental restored keep gable",
        "Royal hall gothic glass",
        "Royal hall pointed window",
        "imperial frieze",
        "plumb-cut imperial quoin",
        "Imperial crest mount",
        "imperial wing relief",
        "imperial wing lower tip",
        "raised keep",
        "upper keep |",
        "Valoria high lantern",
        "keep slate vaulted pyramidal roof",
        "restored imperial spire"
    )
    n=0
    for obj in list(bpy.data.objects):
        if obj.type=="MESH" and obj.name.startswith(prefixes):
            bpy.data.objects.remove(obj,do_unlink=True)
            n+=1
    if n<8:raise RuntimeError("V12 replacement preflight found too few old keep pieces: "+str(n))
    print("V12_OLD_OVERLAPPING_KEEP_REMOVED",n)
    return n

def pointed_lancet(name,x,y,z,width,height,category):
    """Readable deep glazed stone opening; physically separated jamb and voussoir."""
    cub(name+" shadow recess",(x,y,z+height*.5),
        (width,.20,height),"mortar",category,.035)
    cub(name+" amber interior glass",(x,y-.13,z+height*.49),
        (width*.73,.14,height*.76),"amber",category,.02)
    for side in (-1,1):
        xx=x+side*width*.52
        rod(name+" carved upright",(xx,y-.21,z),(xx,y-.21,z+height*.76),
            .175,"edge",category)
        rod(name+" stone ogive",(xx,y-.25,z+height*.76),
            (x,y-.25,z+height*1.07),.17,"edge",category)
    rod(name+" lead vertical mullion",
        (x,y-.35,z+.11),(x,y-.35,z+height*.83),.062,"iron",category)
    cub(name+" limestone sill",(x,y-.23,z-.10),
        (width+.34,.34,.20),"stone",category,.045)

def bastion(stage):
    prune_earlier_keep(stage)
    if stage<3:return
    wingcat="V12_INTEGRATED_CHAPELS"
    # The same lower fortress gradually recovers its symmetrical stone wings.
    for sign in (-1,1):
        x=sign*5.74
        cub("connected citadel transept masonry",(x,5.05,4.9),
            (4.15,6.10,6.65),"ashlar",wingcat,.095)
        if stage>=4:
            roof("connected chapel slate nave",x,1.85,8.25,8.12,
                 2.38,2.75,wingcat,7)
            for y in (2.8,6.8):
                cub("load bearing chapel buttress",
                    (x+sign*2.10,y,3.62),(.72,.92,6.8),"stone",wingcat,.07)
                cub("chapel buttress shoulder",
                    (x+sign*2.10,y,6.68),(.88,1.16,.48),
                    "edge",wingcat,.065)
    if stage<5:return

    c="V12_CITADEL_ARCHITECTURE"
    # The big visual improvement is the SHAPE: two physically intersecting
    # wall volumes, broad lower nave, rising central choir, and finished roof
    # slopes. Unlike V8/V9, no old high hall remains inside to cause z-fighting.
    cub("one continuous lower imperial nave",(0,5.78,8.86),
        (9.1,7.90,6.55),"ashlar",c,.13)
    cub("raised choir stone body",(0,6.42,13.72),
        (5.86,6.85,6.65),"stone",c,.12)
    roof("imperial high choir steep joined gable",
         0,2.82,10.15,17.02,3.44,4.38,c,12)
    # Broad exposed transept roofs read from the actual 3/4 mobile camera.
    for sign in (-1,1):
        x=sign*5.62
        roof("imperial shoulder transept pitched slate",
             x,1.95,8.60,10.34,2.48,2.55,c,9)
        cub("transept wall-to-nave transition",
            (sign*3.73,4.61,8.03),(1.35,6.86,4.8),
            "ashlar",c,.08)
        for y in (2.55,5.43,7.6):
            xx=sign*4.45
            cub("living stone flying buttress",
                (xx,y,7.50),(.67,.89,7.88),"stone",c,.075)
            cub("flying buttress stepped cap",
                (xx,y,11.45),(.92,1.06,.40),"edge",c,.05)
    # Upper hall does not present a blank broad white triangle:
    # genuinely recessed Gothic fenestration and short ashlar courses.
    pointed_lancet("royal centre rose-height lancet",
                   0,2.72,11.5,1.46,3.80,c)
    for x in (-1.74,1.74):
        pointed_lancet("royal high side lancet",
                       x,2.74,11.9,.77,2.5,c)
    for z in (9.8,11.25,12.75,14.5):
        for sign in (-1,1):
            xx=sign*2.75
            cub("choir dressed side quoin",
                (xx,2.85,z),(.61,.51,.23),"edge",c,.055)
    # The royal gateway is tied to a stepped heavy stone arch, and the
    # same entry anchor remains at the identical world coordinate.
    arch("grand castle portal stone voussoir",
         0,-6.02,.96,4.15,4.28,c)
    for sign in (-1,1):
        x=sign*2.63
        cub("gate entrance grounded stone return",
            (x,-5.78,3.08),(1.05,.94,4.05),"stone",c,.055)
        cub("gate relief corbel cap",
            (x,-6.00,5.15),(1.24,1.12,.35),"edge",c,.045)
    # The existing round guard towers remain, with a coherent later roof.
    for sign in (-1,1):
        x=sign*8.18
        cylinder("rebuilt weathered watchtower coping",
                 (x,-4.28,7.36),2.43,.42,"edge",c,12)
        cone("heavy segmented watchtower slate",
             (x,-4.28,8.87),2.43,.11,2.55,"slate",c,12)
        cylinder("low-gold pinnacle",(x,-4.28,10.32),
                 .16,.37,"bronze",c,9)
    for x in (-2.38,2.38):
        cub("Valoria heraldic standard stone bracket",
            (x,2.43,8.56),(.25,.34,.19),"bronze",c,.03)
        cub("Valoria hanging plum standard",
            (x,2.30,7.58),(.76,.11,1.71),"banner",c,.02)
    cylinder("single Valoria raised bronze seal",
             (0,2.42,9.90),.70,.14,"bronze",c,16,
             orientation=(radians(90),0,0))

def industry():
    """A function dictates the architecture; do not add generic copies of homes."""
    c="V12_DISTRICT_FINISH"
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
    scene.view_settings.exposure=.20
    world=bpy.data.worlds.new("V12 late bronze autumn dusk")
    scene.world=world;world.use_nodes=True
    bg=world.node_tree.nodes["Background"]
    bg.inputs["Color"].default_value=(.34,.39,.46,1)
    bg.inputs["Strength"].default_value=.74
    bpy.ops.object.light_add(type="SUN",location=(0,0,45))
    sun=bpy.context.object
    sun.rotation_euler=(radians(73),radians(8),radians(-34))
    sun.data.energy=3.05
    sun.data.color=(1,.74,.52)
    sun.data.angle=radians(3.9)
    for xx in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(xx,-6.5,3.3))
        lamp=bpy.context.object
        lamp.data.energy=110
        lamp.data.color=(1,.60,.36)
        lamp.data.shadow_soft_size=.8
    bpy.ops.object.camera_add(location=(36,-51,39))
    cam=bpy.context.object
    cam.name="V12 same 3D city comparison camera"
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
    print("ELDORIA_V12_REAL_BLENDER_RENDER",name,len(raw))
def export(stage):
    p=DEST/("DirectorV12_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(p),export_format="GLB",export_apply=True)
    raw=p.read_bytes()
    if raw[:4]!=b'glTF' or struct.unpack_from("<I",raw,8)[0]!=len(raw):
        raise RuntimeError("No valid physical GLB: "+str(p))
    length=struct.unpack_from("<I",raw,12)[0]
    data=json.loads(raw[20:20+length])
    if len(data.get("meshes",[]))<500 or len(data.get("images",[]))<4:
        raise RuntimeError("Real V12 3D or texture content missing")
    rec={"stage":stage,"meshes":len(data["meshes"]),"images":len(data["images"]),
        "materials":len(data.get("materials",[])),"bytes":len(raw)}
    STATS.append(rec)
    print("ELDORIA_V12_TRUE_3D",json.dumps(rec))
def main():
    global M
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        src=SOURCE/("DirectorV8_Stage"+str(stage)+".glb")
        if not src.exists():raise RuntimeError("Original source missing: "+str(src))
        bpy.ops.import_scene.gltf(filepath=str(src),import_pack_images=True)
        M=materials()
        # Physically embedded Eldoria V8 atlas reuse, not solid pale cardboard.
        imported_atlas={}
        for role,source in {
            "ashlar":"V8_master_ashlar",
            "stone":"V8_master_ashlar",
            "slate":"V8_master_roof",
            "slate2":"V8_master_roof",
            "wood":"V8_master_timber",
        }.items():
            m=bpy.data.materials.get(source)
            if not m or not m.use_nodes or not any(
                node.type=="TEX_IMAGE" and node.image
                for node in m.node_tree.nodes):
                raise RuntimeError("V12_OWNED_ATLAS_MISSING: "+source)
            M[role]=m
            imported_atlas[role]=source
        print("V12_PHYSICAL_V8_ATLASES_CONNECTED",imported_atlas)
        terrain_stats=sculpt_owned_ground_mesh()
        removed,original_positions=purge_v8_canopy()
        tree_count=forest(original_positions)
        mixed_count=mixed_native_broadleaf_forest()
        bastion(stage)
        industry()
        ambience()
        if stage==1:
            render(stage,"DirectorV12_BastionI_1600x900.png",[1600,900])
        elif stage==5:
            render(stage,"DirectorV12_BastionV_1600x900.png",[1600,900])
            render(stage,"DirectorV12_BastionV_Portrait_720x1200.png",[720,1200],True)
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"DirectorV12_Editable_Source.blend"))
        export(stage)
        print("ELDORIA_V12_TREE_REBUILD",stage,removed,tree_count,mixed_count,terrain_stats)
    record={"project":"Eldoria CC0 Art System","version":"V12 rebuilt natural forest, landmark architecture, distinct districts",
      "input_run_id":43882248142,"input_rights":"owner-authored Eldoria meshes only",
      "vendor_assets_included":False,"images_are_real_blender_renders":True,
      "unity_native_tested":False,"iphone_tested":False,"art_approved":False,
      "stages":STATS,"renders":FRAMES}
    (PREVIEW/"v12-evidence.json").write_text(json.dumps(record,indent=2)+"\n")
    print("ELDORIA_V12_REAL_ART_PASS",len(STATS),len(FRAMES))
if __name__=="__main__":
    main()
