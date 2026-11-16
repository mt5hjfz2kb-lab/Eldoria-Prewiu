#!/usr/bin/env python3
"""Eldoria CC0 Art System — Director V8 / physical city art redesign.

Inputs: five ORIGINAL, owner-authored V7 GLB from exact verified Action #43880753054.
No download of third-party meshes, no conceptual image as a scene texture.
Outputs: five genuine textured GLBs, editable Stage-V .blend, three native Blender
PNG renders and per-stage provenance/metrics. NO Unity, Android or iPhone claim.
Original procedural source is reproducible on Ubuntu Blender 4.0.2 and NumPy.
"""
from pathlib import Path
from mathutils import Vector
from math import sin,cos,pi,radians
import bpy,math,json,random,struct,hashlib
import numpy as np

ROOT=Path(__file__).resolve().parents[2]
INPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV7"
DEST=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV8"
PREV=ROOT/"docs/cc0-art-system/previews/director-v8"
for p in (DEST,PREV):p.mkdir(parents=True,exist_ok=True)
W=118;H=106;X0=-59.;Y0=-53.
STAGES=[];FRAMES=[]
PALETTE={
    "stone":"#807C73","edge":"#AC9D87","shadow":"#444D54",
    "roof":"#293848","wood":"#654735","darkwood":"#2E302F",
    "bronze":"#A88550","iron":"#2A2F34",
    "leaf1":"#284337","leaf2":"#355241","leaf3":"#49624B",
    "bark":"#4B4138","sedge":"#54684C","moss":"#3E5644",
    "amber":"#F2AD61","purple":"#493855","grain":"#A78A50",
    "earth":"#5C5140","rock":"#545E5B","rock2":"#77786C"
}
def rgb(c):
    c=c.lstrip("#")
    return tuple(int(c[n:n+2],16)/255. for n in (0,2,4))
def group(n):
    c=bpy.data.collections.get(n)
    if c is None:
        c=bpy.data.collections.new(n)
        bpy.context.scene.collection.children.link(c)
    return c
def solid(n,hex,rough=.85,emit=0.):
    mat=bpy.data.materials.new("V8_"+n)
    mat.use_nodes=True
    v=rgb(hex);p=mat.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*v,1.)
    p.inputs["Roughness"].default_value=rough
    if emit:
        for s in ("Emission Color","Emission"):
            if s in p.inputs:p.inputs[s].default_value=(*v,1.);break
        if "Emission Strength" in p.inputs:p.inputs["Emission Strength"].default_value=emit
    mat.diffuse_color=(*v,1.)
    return mat
def texture_image(name,arr):
    # Required physically saved/baked PNG: no procedural Eevee-only shader.
    arr=np.clip(arr,0,1).astype(np.float32)
    h,w,_=arr.shape
    im=bpy.data.images.new(name,width=w,height=h,alpha=True)
    if arr.shape[2]==3:
        rgba=np.dstack((arr,np.ones((h,w),np.float32)))
    else:rgba=arr
    im.pixels.foreach_set(rgba.reshape(-1))
    im.update()
    im.filepath_raw=str(DEST/(name+".png"))
    im.file_format="PNG"
    im.save()
    im.pack()
    return im
def texmat(name,image,rough=.86):
    m=solid(name,"#FFFFFF",rough)
    p=m.node_tree.nodes.get("Principled BSDF")
    t=m.node_tree.nodes.new("ShaderNodeTexImage")
    t.image=image
    t.interpolation='Linear'
    m.node_tree.links.new(t.outputs["Color"],p.inputs["Base Color"])
    return m
def procedural_atlas(name,base,kind,n=1024):
    rng=np.random.default_rng(sum(ord(x) for x in name)+161126)
    yy,xx=np.mgrid[:n,:n]
    a=np.zeros((n,n,3),np.float32)+np.array(base,np.float32)
    noise=rng.normal(0,.015,(n,n)).astype(np.float32)
    if kind=="stone":
        seam=(yy%46<2)|((xx+(yy//46%2)*31)%87<2)
        offset=np.sin((xx//87)*1.7+(yy//46)*2.9)*.042
        noise+=offset
        noise[seam]-=.17
        noise+=(np.sin(xx*.07+yy*.09)*.018)
    elif kind=="roof":
        seam=(yy%27<2)|((xx+(yy//27%2)*26)%58<2)
        noise[seam]-=.115
        noise+=np.sin((xx*.043+yy*.027))*.022
    elif kind=="wood":
        noise+=np.sin(xx*.21+np.sin(yy*.12))*.045
        noise+=(np.sin(xx*.047+yy*.015)*.025)
        noise[xx%71<2]-=.13
    a+=noise[:,:,None]*np.array([1.0,.95,.87])
    return texture_image("ED_V8_"+name,a)
def allmaterials():
    mats={k:solid(k,v,rough=.78 if k in ("bronze","iron") else .93,
                  emit=1.6 if k=="amber" else 0.)
          for k,v in PALETTE.items()}
    mats["ashlar"]=texmat("master_ashlar",
             procedural_atlas("AshlarSlate",(0.48,0.48,0.46),"stone"),.87)
    mats["slate"]=texmat("master_roof",
             procedural_atlas("DeepShingles",(0.18,0.26,0.34),"roof"),.79)
    mats["timber"]=texmat("master_timber",
             procedural_atlas("AgedOak",(0.38,0.28,0.22),"wood"),.9)
    return mats
M={}
def assign(o,kind,col):
    t=group(col)
    for old in list(o.users_collection):old.objects.unlink(o)
    t.objects.link(o)
    o.data.materials.clear()
    o.data.materials.append(M[kind])
    return o
def cube(n,at,dim,k,col,bevel=.0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=at)
    o=bpy.context.object;o.name=n;o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    assign(o,k,col)
    if bevel:
        m=o.modifiers.new("soft masonry edge","BEVEL");m.width=bevel;m.segments=1
    return o
def cyl(n,at,r,h,k,col,vertices=12,rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=h,location=at)
    o=bpy.context.object;o.name=n
    if rotation is not None:o.rotation_euler=rotation
    return assign(o,k,col)
def cone(n,at,r,top,h,k,col,vertices=12):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=r,radius2=top,depth=h,location=at)
    o=bpy.context.object;o.name=n
    return assign(o,k,col)
def beam(n,a,b,width,k,col):
    a=Vector(a);b=Vector(b);delta=b-a
    if delta.length<.001:return
    o=cube(n,(a+b)*.5,(width,width,delta.length),k,col,.015)
    o.rotation_euler=delta.to_track_quat("Z","Y").to_euler()
    return o
def meshobj(n,verts,faces,k,col,matindices=None):
    mesh=bpy.data.meshes.new(n+"_mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    if matindices:
        for matname in matindices["names"]:mesh.materials.append(M[matname])
        for p,v in zip(mesh.polygons,matindices["ids"]):p.material_index=v
        o=bpy.data.objects.new(n,mesh);group(col).objects.link(o)
    else:
        o=bpy.data.objects.new(n,mesh);group(col).objects.link(o)
        mesh.materials.append(M[k])
    return o
def ground_height(x,y):
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
            +.22*sin(x*.064-y*.083)*cos(y*.11)
            +.06*cos(.24*x+.18*y))
CURVES=[
  [(-.3,-40),(1.4,-27),(-2.1,-16),(0,-6.2)],
  [(-3,-18),(-12,-21),(-18,-14),(-22,-9)],
  [(3,-17),(11,-19),(17,-14),(22,-11)],
  [(-18,-11),(-27,-6),(-28,4),(-23,9)],
  [(16,-12),(27,-6),(28,4),(24,8)]
]
def roads_xy(x,y):
    # Vectorized road-distance in painting; x/y are same-shape NumPy arrays.
    d=np.full(x.shape,1e6,np.float32)
    for control in CURVES:
        p=np.asarray(control,dtype=np.float32)
        for t in np.linspace(0,1,38):
            q=p[0]*(1-t)**3+3*p[1]*t*(1-t)**2+3*p[2]*(t*t)*(1-t)+p[3]*(t**3)
            d=np.minimum(d,(x-q[0])**2+(y-q[1])**2)
    return np.sqrt(d)
def natural_ground():
    # Make the 1024^2 ground texture from several deterministic spatial scales.
    n=1024
    x=np.linspace(X0,X0+W,n,dtype=np.float32)
    y=np.linspace(Y0,Y0+H,n,dtype=np.float32)
    X,Y=np.meshgrid(x,y)
    rng=np.random.default_rng(20261116)
    broad=(.025*np.sin(X*.085+Y*.055)+.026*np.sin(X*.22-Y*.18)
           +.017*np.cos(X*.38+Y*.27))
    fine=rng.normal(0,.014,(n,n)).astype(np.float32)
    dark=np.stack([.272+broad+.4*fine,.348+broad+.6*fine,
                   .255+.68*broad+.42*fine],axis=-1)
    grass2=np.stack([.34+.5*broad,.41+.68*broad,.29+.65*broad],axis=-1)
    patch=(.5+.5*np.sin(X*.21+Y*.10)*np.cos(X*.11-Y*.27))[:,:,None]
    ground=dark*(1-patch*.40)+grass2*patch*.40
    # Quarry geological region is irregular color ramp, not inserted plaza.
    quarry=((X+28)/12)**2+((Y-19)/11)**2
    stone_blend=np.clip(1-quarry,0,1)[:,:,None]*.55
    stone=np.array([.39,.39,.37],np.float32)[None,None,:]
    ground=ground*(1-stone_blend)+stone*stone_blend
    d=roads_xy(X,Y)
    lane=np.clip((2.5-d)/1.6,0,1)[:,:,None]
    soil=np.stack([.405+broad*.66,.345+broad*.57,.275+broad*.50],axis=-1)
    # Fine grain and worn wheel marks physically in the base color map.
    ruts=np.clip(np.exp(-((d-.70)/.18)**2)*.045,0,.06)
    soil[:,:,:3]-=ruts[:,:,None]
    ground=ground*(1-lane)+soil*lane
    # Mid-frequency warm loam exposed by foot traffic, not a solid painted stripe.
    tex=texture_image("ED_V8_ValleyGrassSoil",ground)
    v=[]
    nx,ny=128,116
    for j in range(ny+1):
        yy=Y0+H*j/ny
        for i in range(nx+1):
            xx=X0+W*i/nx
            v.append((xx,yy,ground_height(xx,yy)))
    faces=[]
    for j in range(ny):
        for i in range(nx):
            a=j*(nx+1)+i
            faces.append((a,a+1,a+nx+2,a+nx+1))
    mesh=bpy.data.meshes.new("Valoria genuine rounded height field")
    mesh.from_pydata(v,[],faces);mesh.update()
    mesh.materials.append(texmat("valley_baked_diffuse",tex,.98))
    obj=bpy.data.objects.new("V8 continuous painted and textured city ground",mesh)
    group("V8_ENVIRONMENT").objects.link(obj)
    uv=mesh.uv_layers.new(name="UVMap")
    for p in mesh.polygons:
        for k in p.loop_indices:
            idx=mesh.loops[k].vertex_index
            x,y,z=v[idx]
            uv.data[k].uv=((x-X0)/W,(y-Y0)/H)
    return {"ground_vertices":len(v),"ground_faces":len(faces),"texture_px":n}
def remove_previous_lookdev():
    tokens=(
      "ELDORIA / physical undulating painted terrain",
      "ELDORIA / grouped wild meadow",
      "V6 irregular 3D fir","organic crooked conifer trunk",
      "mossy eroded stone","restored crenel on high lantern",
      "Valoria high lantern ashlar drum","restored imperial spire",
      "golden summit pennon","imperial plum pennon cloth",
      "keep slate vaulted pyramidal roof",
      "V5 roof lantern",
      "Eldoria V7 / asymmetrical branched spruce",
      "Eldoria V7 / bramble and sage bush",
      "dark living fir trunk",
      "joined lateral slate nave",
      "flying chapel buttress",
      "granary slate cap",
      "fortress linked lateral chapel")
    deleted={}
    for o in list(bpy.data.objects):
        if o.type!="MESH":continue
        for token in tokens:
            if o.name.startswith(token):
                deleted[token]=deleted.get(token,0)+1
                bpy.data.objects.remove(o,do_unlink=True);break
    return deleted
def rematerial_existing():
    """Replace incompatible imported families with THREE shared owned tile atlases."""
    stats={"stone":0,"roof":0,"wood":0,"retained":0}
    for o in list(bpy.data.objects):
        if o.type!="MESH":continue
        n=o.name.lower()
        # Preserve authored signature gold, purple and warm windows.
        if any(x in n for x in ("window","torch","banner","herald",
                                "pennon","flag","stone-gate void")):
            stats["retained"]+=1;continue
        matnames=" ".join(m.name.lower() for m in o.data.materials if m)
        if ("roof" in n or "slate" in n or "shingle" in n or "roof" in matnames
            or "slate" in matnames):
            kind="slate";stats["roof"]+=1
        elif (any(x in n for x in ("beam","sawn","oak","timber","log",
                                     "plank","scaffold","gantry","truss"))
              or any(x in matnames for x in ("oak","timber","wood"))):
            kind="timber";stats["wood"]+=1
        elif any(x in matnames for x in ("stone","ashlar","limestone","rubble")):
            kind="ashlar";stats["stone"]+=1
        else:
            stats["retained"]+=1;continue
        o.data.materials.clear();o.data.materials.append(M[kind])
    return stats
def tree_mesh(seed):
    """Natural irregular radial bough mass, not a pyramid or toy stacked cone."""
    rng=random.Random(seed)
    V=[];F=[];I=[]
    rings=9
    for level in range(7):
        z=.20+level*.106
        radius=(.246-level*.028)*rng.uniform(.9,1.09)
        top=z+.206+rng.uniform(-.02,.023)
        offset=rng.uniform(-.04,.04)
        topi=len(V)+rings
        for k in range(rings):
            t=2*pi*k/rings
            r=radius*rng.uniform(.83,1.19)
            V.append((r*cos(t)+offset,r*sin(t),z+.021*sin(k*1.4+level)))
        V.append((offset*.8,0,top))
        for k in range(rings):
            F.append((topi-rings+k,topi-rings+(k+1)%rings,topi))
            I.append((k+level+seed)%3)
        # low dense drooping branchlets give non-conic edges
        for k in range(0,rings,2):
            t=2*pi*(k+.4)/rings
            r=radius*rng.uniform(1.1,1.33)
            base=len(V)
            V.extend([(radius*.45*cos(t),radius*.45*sin(t),z+.145),
                      (r*cos(t),r*sin(t),z-.035),
                      ((r+.04)*cos(t+.13),(r+.04)*sin(t+.13),z-.07)])
            F.append((base,base+1,base+2));I.append((level+k+1)%3)
    return V,F,I
def botanical_forest():
    rng=random.Random(16112027)
    v,faces,inds=tree_mesh(128)
    # Mesh is genuinely shared across trees: 3 bough materials plus a trunk.
    mesh=bpy.data.meshes.new("V8 interlocking spruce foliage exemplar")
    mesh.from_pydata(v,[],faces);mesh.update()
    for k in ("leaf1","leaf2","leaf3"):mesh.materials.append(M[k])
    for p,i in zip(mesh.polygons,inds):p.material_index=i
    tree_count=0
    for i in range(139):
        x=rng.uniform(-55,55);y=rng.uniform(-49,50)
        if abs(x)<20 and -29<y<26:continue
        if abs(x+23)<11 and -17<y<21:continue
        if abs(x-23)<11 and -19<y<22:continue
        h=rng.uniform(6.6,12.2)
        o=bpy.data.objects.new("V8 irregular crown "+str(i),mesh)
        group("V8_CANOPY").objects.link(o)
        o.location=(x,y,ground_height(x,y)+h*.11)
        o.scale=(h*.90,h*.92,h*.96)
        o.rotation_euler[2]=rng.uniform(-pi,pi)
        cyl("V8 gnarled tree trunk",(x,y,ground_height(x,y)+h*.38),
            h*.035,h*.70,"bark","V8_CANOPY",vertices=7)
        tree_count+=1
    # Shared grass clump geometry: each batch is one mesh, not 2,000 draw calls.
    grass_count=0
    for batch,k,n in ((0,"sedge",480),(1,"moss",320),(2,"grain",100)):
        verts=[];fs=[]
        for i in range(n):
            x=rng.uniform(-53,53);y=rng.uniform(-47,47)
            if -8<x<8 and -10<y<10:continue
            if (x+23)**2+(y+7.5)**2<22:continue
            height=rng.uniform(.18,.54)
            base=ground_height(x,y)+.07
            for blade in range(3):
                q=2*pi*blade/3
                a=len(verts)
                verts.extend([(x+.17*cos(q),y+.15*sin(q),base),
                              (x+.09*cos(q+.6),y+.10*sin(q+.6),base),
                              (x+.11*cos(q+.3),y+.12*sin(q+.3),base+height)])
                fs.append((a,a+1,a+2))
        meshobj("V8 clustered ground herbs "+str(batch),verts,fs,k,"V8_GROUNDCOVER")
        grass_count+=n
    for i in range(100):
        x=rng.uniform(-51,51);y=rng.uniform(-46,46)
        if -12<x<12 and -11<y<12:continue
        rad=rng.uniform(.2,.73)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rad,
                     location=(x,y,ground_height(x,y)+rad*.42))
        o=bpy.context.object;o.name="V8 mossy outcrop"
        o.scale=(rng.uniform(.8,1.5),rng.uniform(.8,1.3),rng.uniform(.4,.75))
        assign(o,"rock" if i%3 else "rock2","V8_ROCKS")
    for i in range(100):
        x=rng.uniform(-48,48);y=rng.uniform(-46,46)
        if -12<x<12 and -13<y<16:continue
        r=rng.uniform(.4,.98)
        for j in range(3):
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=r,
                location=(x+rng.uniform(-.4,.4),y+rng.uniform(-.4,.4),
                         ground_height(x,y)+r*.34))
            assign(bpy.context.object,
                   rng.choice(("leaf1","leaf2","moss")),"V8_BUSHES")
    return {"trees":tree_count,"clustered_grasses":grass_count,"outcrops":100,"shrubs":100}
def gable(n,x,front,back,z,wide,rise,col):
    """Stone pointed front gable and TRUE slate roof planes linked to the walls."""
    verts=[(x-wide/2,front,z),(x+wide/2,front,z),(x,front,z+rise),
           (x-wide/2,back,z),(x+wide/2,back,z),(x,back,z+rise)]
    meshobj(n+" facade",verts[:3],[(0,1,2)],"ashlar",col)
    face=meshobj(n+" long intersecting slate",verts,
              [(0,3,5,2),(1,2,5,4)],"slate",col)
    # Ensure roof surfaces use same physical atlas.
    for a,b in ((0,2),(1,2)):
        beam(n+" stone bargeboard",verts[a],verts[b],.19,"edge",col)
    beam(n+" ridge",(x,front,z+rise),(x,back,z+rise),.12,"bronze",col)
    return face
def distinctive_fortress(stage):
    if stage==1:
        # Early roofing: original remains visibly incomplete; only repair supports.
        for i in range(4):
            x=-3+i*1.8
            beam("early defensive temporary rafter",(x,2,6.4),(x,8.3,6.7),
                 .13,"wood","V8_EARLY")
        return
    if stage<4:return
    col="V8_BASTION_REBUILT"
    # Architectural wings connect, rather than an isolated tower pasted on top.
    for side in (-1,1):
        x=side*5.20
        cube("integrated chapel masonry return",(x,5.5,5.1),
             (3.65,6.65,6.1),"ashlar",col,.14)
        gable("side chapel "+str(side),x,2.0,9.2,8.3,4.0,2.55,col)
        cube("chapel flying wall strut",(x,1.86,4.2),
             (.54,1.18,5.9),"edge",col,.07)
    if stage<5:return
    # The high hall is integral to the keep roof and has broad slope, not a white
    # turret stacked above it. A readable 2.5D silhouette at phone resolution.
    cube("Valoria imperial restored upper hall",(0,6.8,12.8),
         (7.2,7.6,8.5),"ashlar",col,.12)
    gable("imperial long hall",0,2.9,11.6,17.0,8.1,4.2,col)
    for x in (-3.45,3.45):
        cube("integrated ashlar buttress",(x,2.45,11.1),
             (.52,1.36,12.5),"edge",col,.06)
        for z in (7.1,9.5,11.9,14.5):
            cube("stratified stone cap",(x,1.64,z),
                 (.72,.33,.21),"shadow",col,.04)
    # Recessed amber glass in a tall dressed Gothic frame.
    cube("historic stained glass recess",(0,2.56,13.27),
         (1.27,.14,4.03),"amber",col,.05)
    for x in (-.76,.76):
        beam("ogive vertical jamb",(x,2.26,11.3),(x,2.26,15.65),
             .19,"edge",col)
    beam("ogive west rising",(-.81,2.3,15.65),(0,2.3,16.72),.18,"edge",col)
    beam("ogive east rising",(.81,2.3,15.65),(0,2.3,16.72),.18,"edge",col)
    beam("glazing lead sash",(-.08,2.25,11.4),(-.08,2.25,15.7),.07,"iron",col)
    for z in (12.3,13.5,14.7):
        beam("lead gothic transverse",( -.66,2.24,z),(.66,2.24,z),.07,"iron",col)
    # Two distinct noble roofs, additional masonry on gate towers, no floating disc.
    for side in (-1,1):
        x=side*8.2
        cyl("fortress gate defense tower cap",(x,-4.3,7.85),
            2.15,.38,"edge",col,vertices=12)
        cone("ornamented slate watch roof",(x,-4.3,9.3),
             2.5,.10,2.25,"slate",col,vertices=12)
        cyl("low bronze spire tip",(x,-4.3,10.65),.08,.55,"bronze",col,vertices=8)
    # One restrained heraldic mark, no random golden decoration scattered.
    cyl("Valoria central heraldic mount",(0,2.16,10.5),.63,.13,
        "bronze",col,vertices=16,rotation=(pi/2,0,0))
    cube("Valoria violet royal banner",(0,2.06,8.95),(.90,.11,2.08),
         "purple",col,.03)
def economic_details():
    col="V8_ECONOMIC"
    rng=random.Random(715)
    # Sawmill: open machinery and timber that silhouette from the camera.
    for i in range(12):
        x=-26.0+(i%4)*.42
        y=-12.8+(i//4)*.45
        cyl("sawmill grouped timber supplies",(x,y,.44),.24,3.1,
            "timber",col,vertices=8,rotation=(0,pi/2,0))
    for j in range(4):
        beam("sawmill exposed industrial roof support",
             (-26.8+j*1.7,-12.1,3.0),(-26.8+j*1.7,-5.3,4.5),
             .23,"darkwood",col)
    # Quarried stone: tiered natural rock face behind actual lifting apparatus.
    for i in range(37):
        x=-31+rng.uniform(-7,11);y=15.6+rng.uniform(-.6,9.4)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,
             radius=rng.uniform(.65,2.10),
             location=(x,y,ground_height(x,y)+rng.uniform(.4,1.3)))
        o=bpy.context.object;o.name="worked quarry geological shelf"
        o.scale=(rng.uniform(.8,1.5),rng.uniform(.6,1.0),rng.uniform(.3,.64))
        assign(o,rng.choice(("stone","rock","rock2")),"V8_QUARRY")
    beam("quarry long counterweighted derrick",(-28.2,14.5,4.9),(-21.2,15.8,10.4),
         .40,"timber","V8_QUARRY")
    beam("quarry suspended chain",(-21.2,15.8,10.4),(-21.2,15.8,4.9),
         .075,"iron","V8_QUARRY")
    cube("quarry chiseled suspended block",(-21.2,15.8,4.4),(1.2,1.4,.94),
         "edge","V8_QUARRY",.07)
    # Granary: large physically cylindrical silos, recognizably different.
    for x in (26.6,29.6):
        cyl("granary cylindrical carved silo",(x,-8.1,3.22),1.47,6.25,
            "ashlar","V8_GRANARY",vertices=14)
        for zz in (1.1,2.45,3.83,5.12,6.25):
            cyl("granary ashlar banded masonry",(x,-8.1,zz),1.53,.16,
                "edge","V8_GRANARY",vertices=14)
        cone("granary weatherproof slate cap",(x,-8.1,7.04),1.74,.1,
             1.65,"slate","V8_GRANARY",vertices=14)
    # Barracks: target racks, proper storehouse and a watch turret.
    cube("fortified barracks weapon store", (23.1,13.2,2.0),
         (7.4,5.1,3.9),"ashlar",col,.08)
    gable("barracks martial gable",23.1,10.55,15.8,4.15,8.1,2.3,col)
    for x in (19.5,21.4,23.3,25.2):
        beam("cuartel standing training pike",(x,6.4,.65),
             (x,6.4,3.6),.075,"iron",col)
        cone("cuartel tip",(x,6.4,3.8),.12,.02,.35,"iron",col,vertices=8)
    cube("military purple standard",(27.5,11.3,6.8),
         (.8,.13,1.4),"purple",col,.018)
def mood_and_camera():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=20
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    scene.render.threads_mode="FIXED"
    scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.15
    world=bpy.data.worlds.new("V8 blue-lilac twilight ambient")
    scene.world=world;world.use_nodes=True
    world.node_tree.nodes["Background"].inputs["Color"].default_value=(.33,.37,.49,1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value=.65
    for o in list(bpy.data.objects):
        if o.type in ("LIGHT","CAMERA"):bpy.data.objects.remove(o,do_unlink=True)
    bpy.ops.object.light_add(type='SUN',location=(0,0,40))
    light=bpy.context.object
    light.name="sunset / low gold"
    light.rotation_euler=(radians(73),radians(9),radians(-45))
    light.data.energy=2.8
    light.data.color=(1.,.72,.49)
    light.data.angle=radians(4)
    for x in (-1.8,1.8):
        bpy.ops.object.light_add(type="POINT",location=(x,-5.8,3.45))
        l=bpy.context.object;l.name="gate dusk torch point (1 of 2)"
        l.data.energy=76;l.data.color=(1.,.56,.27);l.data.shadow_soft_size=1.
    bpy.ops.object.camera_add(location=(36,-51,38))
    cam=bpy.context.object;cam.name="V8 identical I/V comparison"
    cam.data.type="ORTHO";cam.data.ortho_scale=69
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def render(name,stage,size,portrait=False):
    scene=bpy.context.scene;cam=scene.camera
    cam.location=(28,-48,42) if portrait else (36,-51,38)
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=57 if portrait else 69
    scene.render.resolution_x=size[0];scene.render.resolution_y=size[1]
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    p=PREV/name
    scene.render.filepath=str(p)
    bpy.ops.render.render(write_still=True)
    data=p.read_bytes()
    if data[:8]!=b"\x89PNG\r\n\x1a\n" or len(data)<120000:
        raise RuntimeError("NO AUTHENTIC BLENDER FRAME "+str(p))
    FRAMES.append({"name":name,"stage":stage,"bytes":len(data),"size":list(size)})
    print("V8_REAL_BLENDER_FRAME",name,len(data))
def pack_physical():
    for im in bpy.data.images:
        if im.source=="FILE" and not im.packed_file:
            try:im.pack()
            except Exception as e:raise RuntimeError("Unpacked texture: "+im.name) from e
def export(stage):
    p=DEST/("DirectorV8_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(p),export_format="GLB",export_apply=True)
    b=p.read_bytes()
    if b[:4]!=b"glTF" or struct.unpack_from("<I",b,8)[0]!=len(b):
        raise RuntimeError("Not real GLB "+str(p))
    n=struct.unpack_from("<I",b,12)[0]
    j=json.loads(b[20:20+n])
    if len(j.get("meshes",[]))<500 or len(j.get("images",[]))<4:
        raise RuntimeError("Missing physical mesh or textures "+str(p))
    entry={"stage":stage,"size":len(b),"meshes":len(j["meshes"]),
           "materials":len(j.get("materials",[])),"images":len(j["images"])}
    STAGES.append(entry)
    print("V8_PHYSICAL_STAGE",json.dumps(entry))
def build(stage):
    global M
    bpy.ops.wm.read_factory_settings(use_empty=True)
    M=allmaterials()
    src=INPUT/("DirectorV7_Stage"+str(stage)+".glb")
    if not src.exists():raise RuntimeError("Pinned V7 model missing "+str(src))
    bpy.ops.import_scene.gltf(filepath=str(src),import_pack_images=True)
    deleted=remove_previous_lookdev()
    treated=rematerial_existing()
    env=natural_ground()
    forest=botanical_forest()
    distinctive_fortress(stage)
    economic_details()
    mood_and_camera()
    if stage==1:render("DirectorV8_BastionI_1600x900.png",1,(1600,900))
    elif stage==5:
        render("DirectorV8_BastionV_1600x900.png",5,(1600,900))
        render("DirectorV8_BastionV_Portrait_720x1200.png",5,(720,1200),True)
        pack_physical()
        bpy.context.scene.render.filepath=""
        bpy.ops.wm.save_as_mainfile(filepath=str(DEST/"DirectorV8_Editable_Source.blend"))
    export(stage)
    return {"level":stage,"source_removed":deleted,"reused_rematerialized":treated,
            "ground":env,"forest":forest}
STATS=[build(level) for level in range(1,6)]
record={"project":"Eldoria CC0 Art System","version":"Director V8 — materially different geometry+material lookdev",
        "source_run":43880753054,"source_rights":"original Eldoria 3D only; no vendor source",
        "blender_render":True,"native_unity_tested":False,"mobile_tested":False,
        "art_approved":False,"source_redrawn":True,"stages":STAGES,"frames":FRAMES,
        "source_checks":STATS}
(PREV/"v8-evidence.json").write_text(json.dumps(record,indent=2)+"\n")
print("ELDORIA_V8_AUTHORED_REAL_3D_PASS",len(STAGES),len(FRAMES))
