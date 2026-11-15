#!/usr/bin/env python3
"""NOCTURNE — independent, editable, original stylized-medieval mobile art.
Run with: blender --background --python tools/nocturne-rebuild/author_art.py
No procedural geometry is constructed in the shipping runtime. All meshes,
textures and materials are exported here as physical glTF assets.
"""
import bpy, math, random, os, sys
from pathlib import Path
from mathutils import Vector
random.seed(16112026)
ROOT=Path(__file__).resolve().parents[2]
ASSETS=ROOT/"Unity/Assets/Eldoria/Experiments/NocturneRebuild"
MODELS=ASSETS/"Models"
UI=ASSETS/"UI"
ART=ROOT/"ArtSource/NocturneRebuild"
PREV=ART/"Previews"
for p in [MODELS,UI,ART,PREV]: p.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):
    if c.name!="Collection" and c.users==0: bpy.data.collections.remove(c)
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=8
scene.cycles.device='CPU'
scene.cycles.use_denoising=False
for render_view_layer in scene.view_layers: render_view_layer.cycles.use_denoising=False
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.view_settings.view_transform='Standard'
scene.view_settings.look='Medium High Contrast'
scene.render.resolution_x=1080
scene.render.resolution_y=720
scene.render.threads_mode='FIXED'
scene.render.threads=4
scene.camera=None
scene.world.color=(.09,.11,.14)
COL=None
def collection(name):
    c=bpy.data.collections.new(name)
    scene.collection.children.link(c)
    return c
def assign(obj,mat):
    if mat: obj.data.materials.append(mat)
    if COL:
        for c in list(obj.users_collection): c.objects.unlink(obj)
        COL.objects.link(obj)
    return obj
def rgb(h):
    h=h.lstrip('#')
    return tuple(int(h[i:i+2],16)/255 for i in (0,2,4))
def tex_image(name,base,kind):
    n=128
    image=bpy.data.images.new(name,width=n,height=n,alpha=True)
    px=[0.]*(n*n*4)
    r,g,b=rgb(base)
    seed=sum(ord(x) for x in name)
    for y in range(n):
        for x in range(n):
            grain=(math.sin((x*37+y*11+seed)*.131)+math.cos((x*3-y*17+seed)*.43))*.017
            variation=grain
            if kind=='stone':
                row=y//24
                seam=(y%24 in (0,1)) or ((x+12*(row%2))%43 in (0,1))
                variation+=-.17 if seam else .028*math.sin((x//43+row)*1.79)
            elif kind=='wood':
                variation+=.058*math.sin(x*.43)+.018*math.sin(x*.09+y*.23)
                if x%32 in (0,1): variation-=.17
            elif kind=='slate':
                row=y//21
                variation+=-.15 if y%21 in (0,1) or (x+14*(row%2))%33 in (0,1) else .017*math.sin(x+y)
            elif kind=='soil':
                variation+=.035*math.sin(x*.14+y*.33)
            o=(y*n+x)*4
            px[o:o+4]=[min(1,max(0,r+variation)),min(1,max(0,g+variation*.93)),
                min(1,max(0,b+variation*.86)),1]
    image.pixels[:]=px
    image.filepath_raw=str(ART/(name+".png"))
    image.file_format='PNG'
    image.save()
    return image
def ui_sprite(name,kind):
    """Author RGBA nine-slice sprites as physical game assets, not runtime rectangles."""
    n=256
    im=bpy.data.images.new(name,width=n,height=n,alpha=True)
    px=[0.0]*(n*n*4)
    for y in range(n):
        for x in range(n):
            d=min(x,y,n-1-x,n-1-y)
            chamfer=(x+y<23 or x+(n-1-y)<23 or (n-1-x)+y<23 or
                     (n-1-x)+(n-1-y)<23)
            if chamfer:rgba=(0,0,0,0)
            else:
                noise=.011*math.sin((x*13+y*17)*.052)+.009*math.cos((x*7-y*11)*.101)
                if kind=="panel":
                    base=(.078,.093,.111)
                    if d<=2:base=(.13,.13,.13)
                    elif d<=7:base=(.43,.33,.22)
                    elif d<=10:base=(.72,.54,.33)
                    elif d<=15:base=(.16,.18,.19)
                    elif d<=19:base=(.31,.27,.22)
                    else:
                        grad=(1-y/n)*.022
                        base=(.075+grad,.088+grad,.104+grad)
                elif kind=="primary":
                    grad=.045*(1-y/n)
                    base=(.47+grad,.295+grad,.13+grad)
                    if d<=3:base=(.10,.105,.11)
                    elif d<=8:base=(.74,.57,.32)
                    elif d<=12:base=(.26,.20,.13)
                    elif d<=15:base=(.69,.45,.22)
                else:
                    grad=.018*(1-y/n)
                    base=(.105+grad,.122+grad,.135+grad)
                    if d<=3:base=(.08,.09,.10)
                    elif d<=8:base=(.55,.43,.29)
                    elif d<=11:base=(.21,.24,.26)
                    elif d<=14:base=(.37,.30,.21)
                rgba=(max(0,min(1,base[0]+noise)),max(0,min(1,base[1]+noise)),
                      max(0,min(1,base[2]+noise)),1)
            o=(y*n+x)*4
            px[o:o+4]=rgba
    im.pixels[:]=px
    im.filepath_raw=str(UI/(name+".png"))
    im.file_format="PNG"
    im.save()
    print("UI_SPRITE",name,Path(im.filepath_raw).stat().st_size)
for sprite_name,sprite_kind in (
    ("NocturnePanel","panel"),("NocturnePrimary","primary"),
    ("NocturneSecondary","secondary"),("NocturneDock","panel")):
    ui_sprite(sprite_name,sprite_kind)

def material(name,hexcode,kind=None,metallic=0,rough=.88):
    m=bpy.data.materials.new(name)
    m.diffuse_color=(*rgb(hexcode),1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*rgb(hexcode),1)
    bs.inputs["Metallic"].default_value=metallic
    bs.inputs["Roughness"].default_value=rough
    if kind:
        im=tex_image(name+"_albedo",hexcode,kind)
        tn=m.node_tree.nodes.new("ShaderNodeTexImage")
        tn.image=im
        m.node_tree.links.new(tn.outputs["Color"],bs.inputs["Base Color"])
    return m
M={
"stone":material("Ashlar - lichen slate","#767c7b","stone"),
"stone_light":material("Chiseled edge","#a3a69b","stone"),
"stone_dark":material("Shadow masonry","#40494e","stone"),
"wood":material("Oiled oak","#66503e","wood"),
"wood_dark":material("Charred oak","#332f2e","wood"),
"roof":material("Blue charcoal slate","#343f48","slate"),
"roof_a":material("Muted cobalt slate","#4c5660","slate"),
"bronze":material("Aged bronze","#a88450",metallic=.42,rough=.48),
"iron":material("Forged iron","#22282a",metallic=.54,rough=.55),
"cloth":material("Wine purple canvas","#593e52",rough=.96),
"cloth_light":material("Embroidered mauve","#957581",rough=.96),
"earth":material("Old meadow","#465247","soil"),
"soil":material("Packed road","#84745c","soil"),
"leaf":material("Cypress sage","#465a4e",rough=.96),
"leaf_b":material("Dusty pine","#5e7160",rough=.96),
"trunk":material("Tree bark","#484236","wood"),
"glow":material("Amber lamp","#ffc16a",rough=.55),
"violet":material("Corruption violet","#8050aa",rough=.35),
"glass":material("Cathedral window","#9e8b69",rough=.3),
}
def cube(name,at,size,mat,bevel=.0,rot=None):
    bpy.ops.mesh.primitive_cube_add(size=1, location=at)
    o=bpy.context.object; o.name=name
    if rot: o.rotation_euler=rot
    o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    assign(o,mat)
    if bevel>0:
        mo=o.modifiers.new("Soft hand-cut edges","BEVEL");mo.width=bevel;mo.segments=1
        if hasattr(mo,'affect'): mo.affect='EDGES'
        no=o.modifiers.new("Weighted corner normals","WEIGHTED_NORMAL")
    return o
def cyl(name,at,r,depth,mat,n=10):
    bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=depth,location=at)
    o=bpy.context.object;o.name=name;return assign(o,mat)
def cone(name,at,r1,r2,depth,mat,n=9):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=depth,location=at)
    o=bpy.context.object;o.name=name;return assign(o,mat)
def custom(name,verts,faces,mat):
    mesh=bpy.data.meshes.new(name)
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    o=bpy.data.objects.new(name,mesh)
    COL.objects.link(o)
    o.data.materials.append(mat)
    return o
def ridge_roof(name,x,y,z,w,d,rise):
    # Gabled roof, two solid sloping panels; long axis Y; eaves cast a shadow.
    ang=math.atan2(rise,w/2)
    panel=(w/2)/math.cos(ang)
    for sign in (-1,1):
        cx=x+sign*w/4
        cz=z+rise/2
        cube(name+("/left" if sign<0 else "/right"),(cx,y,cz),
             (panel+.15,d+.35,.21),M["roof"],.04,(0,-sign*ang,0))
    # Thin decorative ridge and gable end framing, no silhouette-breaking spikes.
    cube(name+"/ridge",(x,y,z+rise+.12),(.17,d+.38,.20),M["bronze"],.025)
def quoin(x,y,z,height,signx,signy):
    for iz in range(int(height/.52)):
        dz=z+.25+iz*.52
        cube("Corner ashlar",(x+signx*.07,y+signy*.07,dz),(.51,.54,.31),M["stone_light"],.03)
def door(x,y,z=0,scale=1.):
    cube("Recessed gate shadow",(x,y,z+1.17*scale),(1.78*scale,.12,2.52*scale),M["iron"],.055)
    cube("Two oak gate leaves",(x,y-.09,z+1.12*scale),(1.48*scale,.11,2.24*scale),M["wood_dark"],.045)
    for ix in [-.42,.42]:
        cube("Gate iron hinge",(x+ix*scale,y-.17,z+1.23*scale),(.07,.07,2.1*scale),M["iron"],.008)
    for ih in (.40,1.15,1.86):
        cube("Gate horizontal bracing",(x,y-.20,z+ih*scale),(1.56*scale,.09,.08),M["bronze"],.014)
    cyl("Gate seal",(x,y-.26,z+1.16*scale),.15*scale,.08,M["bronze"],12)
    for tx in (-.95,.95):
        cube("Gate pier",(x+tx*scale,y,z+1.3*scale),(.42*scale,.5,2.6*scale),M["stone_light"],.065)
    cube("Lintel crown",(x,y,z+2.53*scale),(2.42*scale,.68,.36*scale),M["stone_light"],.055)
def window(x,y,z,sz=.64):
    cube("Window recessed iron",(x,y,z),(.33*sz,.105,.72*sz),M["iron"],.05)
    cube("Warm glass inset",(x,y-.067,z),(.18*sz,.035,.48*sz),M["glass"],.025)
    cube("Window sill",(x,y-.15,z-.34*sz),(.57*sz,.22,.12),M["stone_light"],.015)
def banner(x,y,z,scale=1):
    # Distinct fabric shape and bronze embroidered band.
    v=[(x-.34*scale,y,z),(x+.34*scale,y,z),
       (x+.29*scale,y-.07*scale,z-1.36*scale),
       (x,y-.14*scale,z-1.65*scale),
       (x-.29*scale,y-.07*scale,z-1.36*scale)]
    custom("Hanging heraldic cloth",v,[(0,1,2,3,4)],M["cloth"])
    cube("Bronze hanging rod",(x,y,z+.045*scale),(.84*scale,.08,.09),M["bronze"],.02)
    cube("Heraldic band",(x,y-.07*scale,z-.45*scale),(.47*scale,.04,.09),M["cloth_light"],.0)
def tower(x,y,basez,height,r=.92,roof=True,level=1):
    cyl("Octagonal masonry turret",(x,y,basez+height/2),r,height,M["stone"],10)
    cyl("Raised rim ring",(x,y,basez+height-.08),r+0.1,.18,M["stone_light"],10)
    for i in range(10):
        a=math.tau*i/10
        px=x+math.cos(a)*r*.92;py=y+math.sin(a)*r*.92
        cube("Parapet merlon",(px,py,basez+height+.24),(.37,.38,.52),M["stone_light"],.035,rot=(0,0,a))
    if roof:
        cone("Hand-cut slate turret roof",(x,y,basez+height+1.04),r+.2,.13,1.65,M["roof"],10)
        cyl("Bronze finial",(x,y,basez+height+2.03),.12,.35,M["bronze"],10)
    window(x,y-r-.05,basez+height*.63,.7)
def crenellated_wall(x,y,z,w,alongx=True):
    cube("Curtain wall",(x,y,z+.92),(w,.67,1.84) if alongx else (.67,w,1.84),M["stone"],.09)
    count=max(2,int(w/.63))
    for i in range(count):
        p=-w/2+(i+.5)*w/count
        if i%2==0:
            a=(x+p,y,z+2.01) if alongx else (x,y+p,z+2.01)
            cube("Hand-cut crenel",a,(w/count*.66,.76,.43) if alongx else (.76,w/count*.66,.43),M["stone_light"],.035)
def bastion(level):
    global COL
    COL=collection("Bastion_{:02d}_source".format(level))
    # Heavy layered base, not a tall cliff.
    cube("Rock-and-masonry plinth",(0,0,.34),(10.2+level*.25,8.8+level*.21,.68),M["stone_dark"],.19)
    cube("Dressed foundation",(0,0,.77),(9.5+level*.21,8.2+level*.16,.45),M["stone_light"],.09)
    hw=3.15+level*.24
    hd=2.40+level*.14
    zh=2.5+level*.47
    cube("Main keep - ashlar walls",(0,.38,1.03+zh/2),(hw*2,hd*2,zh),M["stone"],.09)
    # Limiting the number of roof silhouette changes makes levels clearly related.
    ridge_roof("Keep",0,.38,1.03+zh,hw*2+.65,hd*2+.44,1.45+level*.09)
    for sx in (-1,1):
        for sy in (-1,1):
            quoin(sx*(hw-.25),.38+sy*(hd-.25),1.1,zh-.18,sx,sy)
    for sx in (-1.4,1.4):
        window(sx,-hd-0.03,2.34+level*.20)
    # Entrance points toward player's camera.
    door(0,-hd-.08,0.9,1)
    cube("Gate overhang",(0,-hd-.28,4.00), (2.55,.94,.20),M["roof_a"],.04)
    banner(hw-.85,-hd-.18,2.98+zh*.40,.64)
    # Gate apron path and inhabited wing.
    cube("Gate paving",(0,-hd-1.28,.84),(2.44,2.28,.12),M["soil"],.025)
    for ix in (-.76,0,.76):
        cube("Paved joint",(ix,-hd-1.26,.92),(.02,2.0,.02),M["stone_light"])
    if level>=2:
        wingh=2.35+.21*level
        cube("Western gallery",(-hw-1.21,.20,1.0+wingh/2),(2.35,3.65,wingh),M["stone"],.065)
        ridge_roof("Gallery west",-hw-1.21,.20,1.0+wingh,2.86,4.08,.84)
        tower(hw+.60,-hd-.33,.93,3.15+.42*level,1.02,True,level)
    if level>=3:
        wingh=2.52+.20*level
        cube("Eastern gallery",(hw+1.16,.15,1+wingh/2),(2.36,3.7,wingh),M["stone"],.06)
        ridge_roof("Gallery east",hw+1.16,.15,1+wingh,2.84,4.1,.86)
        tower(-hw-.61,-hd-.33,.93,3.1+.43*level,1.02,True,level)
        crenellated_wall(0,hd+1.01,.92,hw*2+1.4)
    if level>=4:
        tower(-hw-.57,hd+.51,.93,4.04+.39*level,.93,True,level)
        # Noble stone-faced atrium, small lantern overlooking the keep.
        cyl("Central beacon platform",(0,.38,1.04+zh+1.6),.58,.22,M["stone_light"],10)
        cyl("Lantern light",(0,.38,1.04+zh+2.1),.19,.68,M["glow"],8)
        cone("Beacon cap",(0,.38,1.04+zh+2.65),.46,.10,.55,M["bronze"],10)
        crenellated_wall(-hw-1.4,.4,.89,hd*2+1.3,False)
    if level>=5:
        tower(hw+.57,hd+.51,.93,4.04+.39*level,.93,True,level)
        for sx in [-1,1]:
            banner(sx*(hw+.26),-hd-.34,4.2+zh*.16,.83)
        crenellated_wall(hw+1.4,.4,.89,hd*2+1.3,False)
        # One symbolic landmark, not dozens of glittering props.
        cone("Crown of Valoria",(0,.38,1.03+zh+2.30),.42,.09,.67,M["bronze"],8)
    # Meaningful micro-detail on playable mobile silhouette.
    for i in range(4+level):
        x=-hw+.37+i*(hw*2-.74)/(3+level)
        cube("Cornice stone",(x,-hd-.12,1.02+zh-.14),(.40,.28,.17),M["stone_light"],.018)
    return COL
def outbuilding(family):
    global COL
    COL=collection(family+"_source")
    cube(family+" raised sill",(0,0,.24),(4.25,3.38,.48),M["stone_dark"],.11)
    h=2.72
    cube(family+" body",(0,0,.55+h/2),(3.82,3.06,h),M["stone"],.10)
    ridge_roof(family+" roof",0,0,.55+h,4.37,3.47,1.36)
    door(0,-1.60,.50,.68)
    for xx in (-1.23,1.23): window(xx,-1.57,2.02,.78)
    if family=="Sawmill":
        for y in (-.9,0,.9):
            cyl("Seasoned log",(2.13,y,.56),.34,2.7,M["wood"],8).rotation_euler[1]=math.pi/2
        cube("Mill canopy",(2.25,.10,2.5),(2.0,2.75,.18),M["roof"],.035)
    if family=="Barracks":
        for x in (-1.24,1.24):
            cube("Training spears",(x,-1.90,1.62),(.11,.11,2.4),M["wood"],.012)
            cone("Spear tip",(x,-1.90,2.90),.14,0,.44,M["iron"],8)
        banner(1.4,-1.72,3.48,.65)
    if family=="Granary":
        for x in (-1.05,0,1.05):
            cyl("Grain barrel",(x,-2.05,.60),.37,.92,M["wood"],10)
            for zz in (.32,.84): cyl("Iron hoop",(x,-2.05,zz),.40,.075,M["iron"],10)
    if family=="Stoneworks":
        for x,y,z,r in [(2.3,-.6,.35,.44),(2.7,.6,.31,.37),(2.1,1.3,.42,.53)]:
            cone("Dressed quarry boulder",(x,y,z),r+.13,r*.7,z*1.8,M["stone_light"],7)
        cube("Crane beam",(2.3,0,3.42),(.13,3.1,.13),M["wood"],.01)
    return COL
def pine():
    global COL
    COL=collection("Pine_source")
    cone("Pine trunk",(0,0,.85),.20,.15,1.7,M["trunk"],8)
    for n in range(3):
        cone("Pine crown tier",(0,0,1.8+n*.72),1.14-n*.21,0,2.35-n*.20,M["leaf_b" if n%2 else "leaf"],8)
    return COL
def rock():
    global COL
    COL=collection("Rock_source")
    for x,y,z,s in [(-.18,0,.38,1),(0.68,.12,.29,.76),(-.62,.50,.22,.62)]:
        cone("Faceted rock",(x,y,z),.68*s,.34*s,.76*s,M["stone_dark" if s<.9 else "stone"],7)
    return COL
def export(c,name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in c.all_objects:
        if o.type=='MESH': o.hide_set(False);o.select_set(True)
    if not any(o.select_get() for o in c.all_objects): raise RuntimeError("no selected models "+name)
    path=MODELS/(name+".glb")
    bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,
        export_apply=True,export_yup=True,export_texcoords=True,export_normals=True)
    if path.stat().st_size<1000: raise RuntimeError("GLB too small "+str(path))
    print("EXPORTED",name,path.stat().st_size)
def instance_collection(c,name,at,rot=0,scale=1.0):
    # Mesh copies stay in the preview context. Source collection remains invisible
    # in rendering so it cannot overlap the bastion at the world origin.
    from mathutils import Matrix
    for o in c.objects:
        if o.type!='MESH': continue
        ob=o.copy()
        ob.data=o.data
        ob.name=name+"/"+o.name
        COL.objects.link(ob)
        v=Vector((o.location.x*scale,o.location.y*scale,o.location.z*scale))
        a=math.cos(rot)*v.x-math.sin(rot)*v.y
        b=math.sin(rot)*v.x+math.cos(rot)*v.y
        ob.location=(at[0]+a,at[1]+b,at[2]+v.z)
        ob.rotation_euler[2]+=rot
        ob.scale=(ob.scale.x*scale,ob.scale.y*scale,ob.scale.z*scale)
    return None

# Plot foundation is a real mesh asset (timber stakes and folded plans), not
# a disappearing game target or a placeholder PNG on top of the scene.
COL=collection("Construction_plot_source")
cube("Staked plot plinth",(0,0,.08),(4.1,3.12,.16),M["soil"],.02)
for sx in (-1,1):
    for sy in (-1,1):
        x=sx*1.75;y=sy*1.24
        cube("Oak survey stake",(x,y,.62),(.13,.13,1.2),M["wood_dark"],.025)
        cone("Stake tip",(x,y,1.28),.12,0,.26,M["bronze"],7)
for sx in [-1,1]:
    cube("Folded plan board",(sx*.48,-.1,.33),(.86,1.17,.09),M["wood"],.023,
         rot=(0,0,sx*.11))
cube("Plan parchment",(0,-.13,.385),(1.65,1.02,.027),M["stone_light"],.015)
export(COL,"ConstructionPlot")
COL.hide_render=True

# First produce every distinct independent physical asset; no barred legacy assets.
base=[]
for family in ["Sawmill","Barracks","Granary","Stoneworks"]:
    c=outbuilding(family);export(c,family);c.hide_render=True;base.append((family,c))
p=pine();export(p,"Pine");p.hide_render=True
r=rock();export(r,"Rock");r.hide_render=True
lvls=[]
for level in range(1,6):
    c=bastion(level);export(c,"Bastion_{:02d}".format(level));c.hide_render=True;lvls.append(c)
# Separate scenic context (not part of the building prefabs): an editable authored
# environment which makes composition and camera validation meaningful.
COL=collection("Preview_environment")
cube("Meadow foundation",(0,0,-.29),(27,22,.56),M["earth"],.25)
cube("Road to gate",(0,-8.1,.045),(2.75,9.0,.10),M["soil"],.04)
for k in range(10):
    y=-12.0+k*.88
    for x in [-.93,.03,.99]:
        if (k+int(x*10))%4!=0:
            cube("Individual road slab",(x,y,.12),(.8,.73,.075),M["stone_light"],.02)
export(COL,"CityGround")
for fam,c,pos in [
    ("Sawmill",base[0][1],(-8.5,-3.5,0)),
    ("Barracks",base[1][1],(8.5,-3.5,0)),
    ("Granary",base[2][1],(-8.8,5.2,0)),
    ("Stoneworks",base[3][1],(8.9,5.7,0))]:
    instance_collection(c,fam+" preview",pos,rot=.12 if pos[0]>0 else -.14)
for x,y,s in [(-12,2,1),(-11,8,.9),(12,2,.95),(12,8,1),(-4,9,.9),(5,10,.95),(-12,-10,.87),(12,-10,1)]:
    instance_collection(p,"Pine scenic", (x,y,0),scale=s)
for x,y,s in [(-10,-8,1.2),(10,-9,.9),(3,9,1),(-7,8,.7)]:
    instance_collection(r,"Rock scenic",(x,y,0),scale=s)
# Ground for the frontier uses a distinct original sculpted ground-and-rift
# geometry source. Terrain and the Breach will not rely on shader magic.
COL=collection("Frontier_ground_source")
cube("Frontier forest floor",(0,0,-.43),(38,33,.75),M["earth"],.17)
for i in range(15):
    x=-12.8+i*1.65
    y=-6.6+1.6*math.sin(i*.39)
    cube("Frontier winding stone route",(x,y,.028),(1.39,1.11,.09),
         M["soil"],.08,rot=(0,0,.16*math.cos(i*.39)))
# Breach strata: a geological crack rim and 3 recessed glowing seams;
# each is an actual mesh. Corruption remains a local visual landmark.
for sign in (-1,1):
    verts=[]
    for i in range(9):
        t=i/8
        yy=-3.2+t*8.8
        jitter=.30*math.sin(i*1.81)
        xx=6.7+sign*(.65+.25*math.sin(t*math.pi))+jitter
        verts.extend(((xx,yy,.00),(xx+sign*.62,yy,.38)))
    faces=[]
    for i in range(8):
        a=i*2;faces.append((a,a+1,a+3,a+2))
    custom("Breach geological broken lip",verts,faces,M["stone_dark"])
for i in range(3):
    yy=-2.5+i*2.8
    cube("Breached inner violet seam",(6.65,yy,-.04),(.45,2.65,.06),M["violet"],.07,
         rot=(0,0,.12*math.sin(i*2.14)))
export(COL,"WorldGround")
COL.hide_render=True
COL=scene.collection.children.get("Preview_environment")
# Three real lights; diffuse materials remain readable at low exposure.
world=bpy.data.worlds.new("Smoky twilight")
scene.world=world
world.use_nodes=True
bg=world.node_tree.nodes.get("Background")
bg.inputs["Color"].default_value=(.31,.37,.45,1)
bg.inputs["Strength"].default_value=.67
def area(name,loc,power,color,size):
    ld=bpy.data.lights.new(name,"AREA");ld.energy=power;ld.color=color;ld.shape='DISK';ld.size=size
    ob=bpy.data.objects.new(name,ld);scene.collection.objects.link(ob);ob.location=loc
    ob.rotation_euler=(Vector((0,0,2.8))-ob.location).to_track_quat('-Z','Y').to_euler()
area("Late sun",(13,-10,21),1700,(1,.77,.52),18)
area("Blue atmospheric fill",(-12,6,16),900,(.60,.75,1),21)
area("Soft courtyard bounce",(0,-8,7),480,(1,.81,.64),15)
camdata=bpy.data.cameras.new("Ortho isometric")
camera=bpy.data.objects.new("Ortho isometric",camdata)
scene.collection.objects.link(camera)
scene.camera=camera
camera.location=(17.5,-22.5,19.8)
target=Vector((0,0,2.05))
camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
camdata.type='ORTHO';camdata.ortho_scale=25.4
# Keep fully editable source scene with all five levels in separate collections.
for i,col in enumerate(lvls):
    col.hide_render=i!=0
    col.hide_viewport=False
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(ART/"Nocturne_Master.blend"))
for i,c in enumerate(lvls,1):
    for j,other in enumerate(lvls,1):other.hide_render=(i!=j)
    scene.render.filepath=str(PREV/("Bastion_{:02d}.png".format(i)))
    bpy.ops.render.render(write_still=True)
    out=Path(scene.render.filepath)
    if not out.exists() or out.stat().st_size<10000:
        raise RuntimeError("render absent or blank: "+str(out))
    print("RENDERED",out,out.stat().st_size)
print("NOCTURNE_ART_EXPORT_PASS",len(lvls),"bastions +",len(base),"support + 2 nature")
