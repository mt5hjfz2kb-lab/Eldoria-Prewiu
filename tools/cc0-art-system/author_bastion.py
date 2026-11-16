#!/usr/bin/env python3
"""ELDORIA CC0 SYSTEM — owned physical 3D proof, NOT claimed as imported Quaternius.
Blender 3.x: blender -b -t 4 --python-exit-code 1 --python tools/cc0-art-system/author_bastion.py
Copyright: original geometry, palettes and procedural texture generation for Eldoria.
All output names and camera positions deterministic. No paid/external sources.
"""
import bpy
import math
import random
import json
from pathlib import Path
from mathutils import Vector
from math import radians, sin, cos, pi

ROOT = Path(__file__).resolve().parents[2]
ASSET = ROOT / "Unity/Assets/Art_Reboot/CC0System"
OUT = ASSET / "Generated"
PREVIEW = ROOT / "docs/cc0-art-system/previews"
for p in (OUT, PREVIEW):
    p.mkdir(parents=True, exist_ok=True)
random.seed(161126)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):
    if c.name != "Collection" and c.users == 0:
        bpy.data.collections.remove(c)
S = bpy.context.scene
S.render.engine = 'CYCLES'
S.cycles.samples = 20
S.render.threads_mode = 'FIXED'
S.render.threads = 4
S.render.resolution_percentage = 100
S.render.image_settings.file_format = 'PNG'
S.view_settings.view_transform = 'Filmic'
S.view_settings.look = 'Medium High Contrast'
S.view_settings.exposure = 0.12
S.view_settings.gamma = 1.0
S.render.film_transparent = False
S.render.image_settings.color_mode = 'RGBA'
S.render.resolution_x, S.render.resolution_y = 1440, 900

COLS = {}
def group(name):
    if name not in COLS:
        c = bpy.data.collections.new(name)
        S.collection.children.link(c)
        COLS[name] = c
    return COLS[name]
def move_to(obj,collection):
    c = group(collection)
    for prev in list(obj.users_collection):
        prev.objects.unlink(obj)
    c.objects.link(obj)
    return obj
def rgb(c):
    c = c.lstrip('#')
    return tuple(int(c[i:i+2], 16)/255 for i in (0,2,4))
def image_texture(name, color, kind):
    size = 128
    r,g,b = rgb(color)
    im = bpy.data.images.new("ED_CC0_" + name, width=size, height=size)
    pix = [0.0]*(size*size*4)
    tag = sum(map(ord,name))
    for y in range(size):
        for x in range(size):
            n = (.5*sin(x*.41+y*.67+tag)+.5*cos(x*.11-y*.29+tag))*.055
            if kind=="ashlar":
                line = (y%22 in (0,1) or (x+(y//22%2)*18)%43 in (0,1))
                n += -.23 if line else .019*cos((x//43+y//22)*2.1)
            elif kind=="wood":
                n += .085*sin(x*.25+sin(y*.18)) - (.14 if x%38 in (0,1) else 0)
            elif kind=="roof":
                n += -.17 if y%17 in (0,1) or (x+(y//17%2)*14)%29 in (0,1) else .015*cos(x+y)
            elif kind=="earth":
                n += .038*cos(x*.3+y*.19) + .035*sin(x*.13-y*.41)
            elif kind=="leaf":
                n += .065*cos((x+y)*.38)
            elif kind=="metal":
                n += .027*sin(x*.43-y*.57)
            o = (y*size+x)*4
            pix[o:o+4] = [max(0,min(1,r+n)),max(0,min(1,g+n*.91)),
                         max(0,min(1,b+n*.84)),1.0]
    im.pixels[:] = pix
    path = OUT / ("ED_CC0_"+name+".png")
    im.filepath_raw=str(path)
    im.file_format="PNG"
    im.save()
    return im
def material(name, color, kind=None, rough=.79, metal=0, emit=0):
    m=bpy.data.materials.new("ED_"+name)
    m.use_nodes=True
    shader=m.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Metallic"].default_value=metal
    shader.inputs["Roughness"].default_value=rough
    shader.inputs["Base Color"].default_value=(*rgb(color),1)
    m.diffuse_color=(*rgb(color),1)
    if kind:
        image=image_texture(name,color,kind)
        tex=m.node_tree.nodes.new("ShaderNodeTexImage")
        tex.image=image
        m.node_tree.links.new(tex.outputs["Color"],shader.inputs["Base Color"])
    if emit:
        shader.inputs["Emission"].default_value=(*rgb(color),1)
        shader.inputs["Emission Strength"].default_value=emit
    return m

M={
 "wall":material("OldAshlar","#777D84","ashlar"),
 "wall_light":material("LimestoneEdge","#A1A097","ashlar"),
 "wall_dark":material("DeepAshlar","#505D64","ashlar"),
 "rubble":material("RubbleStone","#596369","ashlar"),
 "roof":material("BlueSlate","#354451","roof"),
 "roof_alt":material("WornSlate","#54606A","roof"),
 "wood":material("OldOak","#735543","wood"),
 "wood_dark":material("CharredOak","#342F2C","wood"),
 "soil":material("PackedTrail","#756551","earth"),
 "grass":material("GrassOlive","#556750","earth"),
 "grass2":material("GrassSage","#69745A","earth"),
 "leaf":material("Pines","#445A4C","leaf"),
 "leaf2":material("MossPines","#61735B","leaf"),
 "moss":material("Moss","#536C53","leaf"),
 "gold":material("AgedBronze","#AC8452","metal",rough=.47,metal=.45),
 "iron":material("ForgedIron","#292C31","metal",rough=.48,metal=.58),
 "amber":material("WarmWindow","#E7A65F",emit=2.6),
 "fire":material("TorchFlame","#FFC46F",emit=3.7),
 "violet":material("VeilViolet","#8052AE",emit=2.5),
 "void":material("BreachGlass","#211B32",rough=.24),
 "cloth":material("WineBanners","#655065"),
 "river":material("ShadowRiver","#3E606A",rough=.21,metal=.15)
}
def apply(obj,mat,col):
    if mat:obj.data.materials.append(M[mat] if isinstance(mat,str) else mat)
    move_to(obj,col)
    return obj
def cube(name, xyz, size, mat, col="World", bevel=0.0, rot=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz)
    o=bpy.context.object
    o.name=name
    o.dimensions=size
    o.rotation_euler[2]=rot
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    apply(o,mat,col)
    if bevel>0:
        mod=o.modifiers.new("worn masonry bevel","BEVEL")
        mod.width=bevel
        mod.segments=1
        o.modifiers.new("weighted normals","WEIGHTED_NORMAL")
    return o
def cyl(name, xyz, radius, depth, mat, col="World", vertices=10, rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=xyz)
    o=bpy.context.object
    o.name=name
    if rotation:o.rotation_euler=rotation
    return apply(o,mat,col)
def cone(name,xyz,base,top,depth,mat,col="World",verts=8):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=base,radius2=top,depth=depth,location=xyz)
    o=bpy.context.object
    o.name=name
    return apply(o,mat,col)
def plane_poly(name,verts,faces,mat,col="World"):
    mesh=bpy.data.meshes.new(name+"_Mesh")
    mesh.from_pydata(verts,[],faces)
    mesh.update()
    obj=bpy.data.objects.new(name,mesh)
    group(col).objects.link(obj)
    if mat:obj.data.materials.append(M[mat])
    # UVs allow image textures to survive GLB export.
    import bmesh
    bm=bmesh.new()
    bm.from_mesh(mesh)
    uv=bm.loops.layers.uv.verify()
    for face in bm.faces:
        for loop in face.loops:
            co=loop.vert.co
            loop[uv].uv=(co.x*.15,co.y*.15)
    bm.to_mesh(mesh)
    bm.free()
    return obj
def roof(name,cx,cy,z,w,d,rise,col="World"):
    # Four-sided, real sloped slate roof with ridge crest and stone barge boards.
    v=[
      (cx-w/2,cy-d/2,z),(cx+w/2,cy-d/2,z),
      (cx-w/2,cy+d/2,z),(cx+w/2,cy+d/2,z),
      (cx,cy-d/2,z+rise),(cx,cy+d/2,z+rise)
    ]
    plane_poly(name+" | pitched slate",v,[(0,1,4),(0,4,5,2),(1,3,5,4),(2,5,3)],"roof",col)
    cube(name+" | ridge bronze",(cx,cy,z+rise+.07),(.17,d+.18,.14),"gold",col)
    for x in (-w/2,w/2):
        # Barge boards visually anchor the roof to timber structure
        cube(name+" | fascia",(cx+x,cy,z-.13),(.22,d+.16,.19),"wood_dark",col)
def arch_gate(cx,cy,z=0,col="Stage1"):
    cube("gate passage | deep void",(cx,cy,z+2.0),(3.05,1.0,4.1),"void",col)
    for sx in (-1,1):
        cube("gate ashlar jamb",(cx+sx*2.0,cy,z+2.05),(.85,1.5,4.1),"wall_light",col,.06)
        for n in range(4):
            cube("gate block bevel",(cx+sx*2.0,cy-.8,z+.5+n*.83),(.95,.18,.29),"wall_dark",col,.05)
    cube("gate | archivolt",(cx,cy,z+4.35),(5.35,1.7,.72),"wall_light",col,.12)
    for n in range(7):
        x = cx - 1.32 + n*.44
        cube("portcullis vertical metal",(x,cy-.59,z+1.85),(.085,.09,3.55),"iron",col)
    for h in (1.15,2.35,3.55):
        cube("portcullis cross",(cx,cy-.65,z+h),(2.95,.10,.1),"iron",col)
def lamps(x,y,z,col="World"):
    cube("sconce iron stem",(x,y,z+.4),(.13,.20,.75),"iron",col)
    cube("sconce brass cup",(x,y,z+.78),(.35,.3,.16),"gold",col)
    cone("torch amber living flame",(x,y,z+1.07),.18,.045,.64,"fire",col,verts=6)
def window(x,y,z,col,wide=.62):
    cube("recessed warm casement",(x,y,z),(wide,.11,1.0),"amber",col)
    cube("window lead divider",(x,y-.07,z),(.07,.11,1.05),"iron",col)
    cube("window sill",(x,y-.07,z-.57),(wide+.35,.31,.16),"wall_light",col)
    cube("window lintel",(x,y-.07,z+.57),(wide+.35,.30,.18),"wall_dark",col)
def battlements(y,xmin,xmax,z,col,front=True):
    cube("rampart stone coping",((xmin+xmax)/2,y,z),(xmax-xmin,.86,.24),"wall_light",col,.06)
    count=int((xmax-xmin)/1.4)
    for i in range(count+1):
        x=xmin+i*(xmax-xmin)/max(count,1)
        cube("individual crenel", (x,y,z+.57),(.81,.82,.98),"wall",col,.05)
def stone_tower(x,y,r,h,col,ruin=False,roofed=False):
    cyl("watchtower core", (x,y,h/2),r,h,"wall",col,12)
    cyl("watchtower shadow ring",(x,y,.48),r+.18,.62,"wall_dark",col,12)
    cyl("watchtower light cornice",(x,y,h-.32),r+.14,.44,"wall_light",col,12)
    if not ruin:
        for i in range(12):
            a=2*pi*i/12
            cube("tower merlon",(x+(r+.03)*cos(a),y+(r+.03)*sin(a),h+.4),(.74,.66,.95),"wall",col,.045,rot=a)
    else:
        for i in (0,1,2,4,6,8,10):
            a=2*pi*i/12
            cube("ruin surviving tooth",(x+(r+.03)*cos(a),y+(r+.03)*sin(a),h+.4),(.72,.68,random.uniform(.3,1.0)),"rubble",col,.04,rot=a)
    if roofed:
        cone("sentinel conical slate roof",(x,y,h+1.15),r+1.0,.06,2.4,"roof",col,12)
        cone("finial",(x,y,h+2.46),.16,0,.7,"gold",col,10)
def fortress():
    # Implicit dimensions are preserved from I to V; new stages add connected fabric.
    base="Stage1"
    cube("bastion | great hall", (0,5,3.0),(10.2,8.0,6.0),"wall",base,.13)
    cube("bastion | ashlar plinth",(0,5,.5),(10.7,8.5,1.0),"wall_dark",base,.15)
    for sy in (-1,1):
        for sx in (-1,1):
            cube("great hall vertical buttress",(sx*4.83,5+sy*3.7,3.1),(.95,.95,6.1),"wall_dark",base,.09)
    for x in (-3.1,-1.1,1.1,3.1):
        window(x,.92,3.45,base)
    cube("entry court | fortified foundation",(0,-1.8,.6),(17,7.2,1.0),"wall_dark",base,.10)
    # Arch and secure entrance anchor stays identical at every stage.
    arch_gate(0,-5.48,.9,base)
    for s in (-1,1):
        stone_tower(s*8.2,-4.3,1.95,5.25,base,ruin=True)
        cube("front curtain | scarred",(s*5.8,-4.4,2.0),(4.8,1.3,3.6),"wall",base,.1)
    cube("old wall | rear foot",(0,10.6,1.4),(17.0,1.3,2.7),"rubble",base,.09)
    # Narrative deterioration: stones and fragment groups linger at I even as repaired.
    for n in range(22):
        x=random.choice([-1,1])*random.uniform(9,14)
        y=random.uniform(-6,11)
        s=random.uniform(.35,.88)
        cube("loose legacy ashlar",(x,y,.30*s),(s,s*.75,s*.55),
             random.choice(["wall","rubble","wall_dark"]),base,.04,rot=random.uniform(-1,1))
    lamps(-1.7,-5.8,1.5,base)
    lamps( 1.7,-5.8,1.5,base)
    # Stage 2: repaired parapets, perimeter wall corners.
    s2="Stage2"
    battlements(-4.4,-7.8,-2.0,3.72,s2)
    battlements(-4.4,2.0,7.8,3.72,s2)
    for x in (-8.2,8.2):
        cyl("tower rebuilt upper stone",(x,-4.3,5.78),1.93,1.4,"wall",s2,12)
    # Stage 3: surviving silhouette becomes a roofed great hall.
    s3="Stage3"
    roof("great hall",0,5,6.2,11.1,8.9,3.0,s3)
    for x in (-3.5,3.5):
        cube("roof dormer",(x,.65,7.32),(1.3,.75,1.25),"wall",s3,.06)
        roof("dormer",x,.58,7.95,1.6,1.0,.7,s3)
    # Stage 4: lateral stratification and rebuilt towers.
    s4="Stage4"
    for s in (-1,1):
        stone_tower(s*8.2,-4.3,2.15,7.7,s4,roofed=True)
        cube("side annex",(s*6.95,5.8,2.5),(4.8,5.5,5.0),"wall",s4,.09)
        roof("side annex",s*6.95,5.8,5.15,5.2,6.0,2.2,s4)
        window(s*6.85,2.97,3.45,s4)
    # Stage 5: the player's monumental transformation; not a color change.
    s5="Stage5"
    cube("raised keep",(0,6.3,9.05),(5.4,5.5,6.4),"wall_light",s5,.12)
    for x in (-1.6,1.6):
        window(x,3.48,8.9,s5)
    roof("upper keep",0,6.3,12.3,6.2,6.2,4.2,s5)
    stone_tower(0,10.7,1.9,13.6,s5,roofed=True)
    for sx in (-1,1):
        cube("great keep corner finial", (sx*2.85,3.35,11.9),(.4,.4,1.7),"gold",s5,.03)
    cube("gate monumental heraldry", (0,-6.0,5.4),(1.65,.35,1.9),"gold",s5,.10)
    cone("gate crest flame", (0,-6.2,6.6),.56,.04,.96,"gold",s5,8)
    for sx in (-1,1):
        cube("upper wall renewed", (sx*5.4,-4.2,5.3),(4.7,.75,2.5),"wall_light",s5,.08)
    battlements(-4.2,-7.9,-2.2,6.55,s5)
    battlements(-4.2,2.2,7.9,6.55,s5)
def cottage(name,x,y,w,d,h,col,industrial=False):
    cube(name+" | foundation",(x,y,.35),(w+.3,d+.4,.65),"wall_dark",col,.08)
    cube(name+" | stone and wood",(x,y,h/2+.5),(w,d,h),"wall",col,.07)
    for sx in (-1,1):
        cube(name+" | corner posts",(x+sx*(w/2-.2),y-d/2-.12,1.8),(.33,.42,3.45),"wood",col)
    roof(name,x,y,h+.75,w+.85,d+.65,w*.28,col)
    window(x-w/4,y-d/2-.08,2.25,col,.55)
    cube(name+" | wooden door",(x+w/4,y-d/2-.12,1.45),(1.12,.24,2.85),"wood_dark",col,.05)
    if industrial:
        cube(name+" | open work awning",(x+w*.5,y-1.1,2.4),(w*.8,3.0,.24),"wood",col,.04)
        for i in range(4):
            cube(name+" | timber stock",(x+w*.45+i*.35,y-3.1,.36+i*.03),
                 (.28,2.1,.25),"wood",col,.02)
def terrain():
    n=46
    verts=[]
    faces=[]
    for iy in range(n+1):
        y=-29.0+iy*60/n
        for ix in range(n+1):
            x=-32+ix*64/n
            ht=.12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)
            ht -= .05*max(0,1-abs(x)/3)
            verts.append((x,y,ht))
    for iy in range(n):
        for ix in range(n):
            a=iy*(n+1)+ix
            faces.append((a,a+1,a+n+2,a+n+1))
    plane_poly("worn Valoria terrain mesh",verts,faces,"grass","World")
    # Painterly patches add transitions, not geometric construction parcels.
    for k in range(75):
        x=random.uniform(-28,28);y=random.uniform(-26,28)
        if abs(x)<3.0 and y<2:continue
        r=random.uniform(.5,2.6)
        theta=[2*pi*j/9 for j in range(9)]
        verts=[(x+r*cos(t),y+r*.66*sin(t),.07) for t in theta]
        plane_poly("irregular meadow colour patch",verts,[tuple(range(9))],
                   random.choice(["grass2","moss","soil"]),"GroundPatches")
    # Valley water cut: narrow scenic river, not a wall of water.
    cube("river channel",(-26.8,-3,-.27),(5.2,56,.17),"river","World",.14,rot=.08)
    for sy in (-1,1):
        for j in range(10):
            xx=-23.0+random.uniform(-2,2)
            yy=-27+j*5.9+random.uniform(-1,1)
            cyl("riverbank boulder",(xx,yy,.2),random.uniform(.25,.7),random.uniform(.3,.7),
                random.choice(["wall","wall_dark"]),"World",7)
def paths():
    # Real flagstones are laid along a curve from the foreground to the entrance.
    for k in range(29):
        t=k/28
        y=-26+20*t
        x=2.6*sin(t*pi)*sin(t*1.8)-.1
        half=2.75-1.45*t
        cube("route | irregular limestone slab",(x+random.uniform(-.15,.15),y,.11),
             (half*2,random.uniform(.7,1.04),.16),
             random.choice(["soil","wall_light","wall"]),"Route",.08,
             rot=random.uniform(-.05,.05))
        for sx in (-1,1):
            if k%2==0:
                cyl("route scattered stone",(x+sx*(half+random.uniform(.35,1.5)),
                       y+random.uniform(-.35,.35),.25),random.uniform(.22,.53),.32,
                    "wall_dark","Route",8)
    # Smaller linked roads imply a functioning economy.
    for xo,direc in ((-15,-1),(15,1)):
        for i in range(11):
            xx=xo+(i-5)*.36
            yy=-9+i*.95
            cube("side lane uneven flagstone",(xx,yy,.12),(1.15,.95,.12),"soil","Route",.04,rot=.13*direc)
def forest():
    # Dense at edges and behind the castle; preserve stage I/V visibility.
    for i in range(91):
        x=random.uniform(-30,30);y=random.uniform(-24,28)
        if abs(x)<17 and -11<y<15:continue
        if x<-23 and abs(y)<22:continue
        h=random.uniform(3.6,8.5)
        cyl("pine trunk",(x,y,h*.24),.20,h*.48,"wood_dark","Foliage",7)
        for level in range(3):
            hz=h*(.42+level*.18)
            base=h*(.19-.038*level)
            cone("pine canopy", (x,y,hz),base,0,h*.40,
                 random.choice(["leaf","leaf2"]),"Foliage",7)
    for i in range(66):
        x=random.uniform(-30,30);y=random.uniform(-27,28)
        if abs(x)<12 and y<12:continue
        cyl("pebble on meadow",(x,y,.18),random.uniform(.15,.55),random.uniform(.18,.5),
            random.choice(["wall","rubble","wall_dark"]),"Foliage",7)
    for i in range(150):
        x=random.uniform(-29,29); y=random.uniform(-27,27)
        if abs(x)<3 and y<1: continue
        cone("worn dry grass tuft",(x,y,.15),random.uniform(.10,.23),0,
            random.uniform(.27,.56),random.choice(["grass2","leaf","moss"]),"Foliage",5)
def ruins_and_breach():
    x,y=-18.9,7
    stone_tower(x,y,2.35,7.0,"Ruins",ruin=True)
    for n in range(12):
        dx=random.uniform(-4,4);dy=random.uniform(-4,4)
        cube("ruined ancient masonry",(x+dx,y+dy,.3),(.9,random.uniform(.5,1.2),.6),
             random.choice(["rubble","wall_dark"]),"Ruins",.09,rot=random.uniform(-.8,.8))
    cube("torn buttress",(x-2.8,y+.8,1.45),(.9,4.4,2.8),"wall_dark","Ruins",.1,rot=.12)
    # Geologic fissure as fractured ground and embedded shards: nothing towers like a portal.
    x,y=22,12
    for i in range(7):
        theta=i*.6
        r=2.7+i*.45
        xx=x+sin(theta)*r
        yy=y+cos(theta)*r
        cube("breach opened bedrock",(xx,yy,.27),(.75,1.55,.38),
             "void" if i%3==0 else "wall_dark","Breach",.06,rot=theta)
    for i in range(8):
        theta=2*pi*i/8
        rad=random.uniform(.7,3.0)
        xx=x+cos(theta)*rad; yy=y+sin(theta)*rad
        cone("embedded violet fissure crystal",(xx,yy,random.uniform(.35,.85)),
             random.uniform(.12,.38),.02,random.uniform(.7,1.8),
             "violet","Breach",5)
    for j in range(20):
        xj=x+random.uniform(-4.1,4.1); yj=y+random.uniform(-3.5,3.5)
        cube("corrupted soil fracture",(xj,yj,.1),
             (random.uniform(.1,.26),random.uniform(.8,2.3),.09),
             "void","Breach",.02,rot=random.uniform(-1,1))
def composition():
    terrain()
    paths()
    fortress()
    cottage("Timber workshop", -14,-3.0,7.3,5.8,3.5,"Town",True)
    cottage("Granary",11,-2,6.6,5.5,3.6,"Town")
    cottage("Barracks lodge",12,12,7.3,6.5,4.2,"Town")
    cottage("Stone quarry", -12,14,7.2,5.6,3.1,"Town")
    forest()
    ruins_and_breach()
    # Mark places as inhabited by lanterns and occupational details.
    for x,y in ((-17,-5),(8,-3),(-17,12),(16,10)):
        lamps(x,y,1.2,"Town")
        cube("linen-covered supply crate",(x+1.1,y,.7),(.9,1.1,1.15),"wood","Town",.08)
def light_and_camera():
    world=bpy.data.worlds.new("Eldoria warm-to-cool gradient sky")
    S.world=world
    world.use_nodes=True
    bg=world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.31,.38,.48,1)
    bg.inputs["Strength"].default_value=.72
    bpy.ops.object.light_add(type='SUN',location=(0,0,45))
    sun=bpy.context.object
    sun.name="Sunset | amber low sun"
    sun.rotation_euler=(radians(66),radians(12),radians(-38))
    sun.data.energy=2.15
    sun.data.color=(1.0,.79,.56)
    sun.data.angle=radians(2.8)
    # No broad local point-light field, only physically motivated torches.
    for x in (-1.7,1.7):
        bpy.ops.object.light_add(type='POINT',location=(x,-6.2,3.0))
        light=bpy.context.object
        light.name="Gate torch point (1 of 2)"
        light.data.energy=125
        light.data.color=(1,.54,.28)
        light.data.shadow_soft_size=1.1
    bpy.ops.object.camera_add(location=(29,-43,35))
    cam=bpy.context.object
    cam.name="Eldoria | cinematic camera sandbox"
    target=Vector((0,1.5,4.6))
    direction=target-cam.location
    cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
    cam.data.type='ORTHO'
    cam.data.ortho_scale=51.0
    S.camera=cam
    return cam
def output(stage,name,width,height,camera_x=None):
    for i in range(2,6):
        COLS["Stage"+str(i)].hide_render=(i>stage)
    # Update mesh visibility to export exactly the intended stage.
    if camera_x is not None:
        S.camera.location.x = camera_x
        target=Vector((0,1,4.6))
        S.camera.rotation_euler=(target-S.camera.location).to_track_quat('-Z','Y').to_euler()
    S.render.resolution_x=width
    S.render.resolution_y=height
    S.render.filepath=str(PREVIEW/name)
    bpy.ops.render.render(write_still=True)
    p=PREVIEW/name
    assert p.stat().st_size>15000,(name,p.stat().st_size)
    print("ELDORIA_REAL_BLENDER_RENDER",p,p.stat().st_size)
def export_glb(stage,name):
    bpy.ops.object.select_all(action='DESELECT')
    count=0
    for o in bpy.data.objects:
        if o.type=='MESH' and (not o.users_collection or
            all(not c.name.startswith("Stage") or int(c.name[-1])<=stage for c in o.users_collection)):
            o.select_set(True); count+=1
    assert count>90,count
    filename=OUT/name
    bpy.ops.export_scene.gltf(filepath=str(filename),export_format='GLB',
                              use_selection=True,export_apply=True)
    assert filename.stat().st_size>30000,(filename,filename.stat().st_size)
    print("ELDORIA_PHYSICAL_GLTF",name,count,filename.stat().st_size)
    bpy.ops.object.select_all(action='DESELECT')
def main():
    composition()
    cam=light_and_camera()
    # L5 is the protagonist; render both levels using IDENTICAL framing.
    output(5,"CC0_BastionV_Lookdev_1440x900.png",1440,900)
    output(1,"CC0_BastionI_Lookdev_1440x900.png",1440,900)
    output(5,"CC0_BastionV_Portrait_720x1200.png",720,1200,camera_x=22)
    S.camera.location.x=29
    S.camera.rotation_euler=(Vector((0,1.5,4.6))-S.camera.location).to_track_quat('-Z','Y').to_euler()
    export_glb(1,"CC0_Valoria_Stage1.glb")
    export_glb(5,"CC0_Valoria_Stage5.glb")
    for i in range(2,6):
        COLS["Stage"+str(i)].hide_render=False
    S.render.filepath=""
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"CC0_Valoria_Source.blend"))
    manifest={
      "producer":"Blender script owned original composition, no external packs actually imported",
      "source":"tools/cc0-art-system/author_bastion.py",
      "native_unity_render":False,
      "blender_render":True,
      "commercial_assets_used":["only owned procedural geometry/textures"],
      "external_sources_imported":[],
      "stage1_glb":"CC0_Valoria_Stage1.glb","stage5_glb":"CC0_Valoria_Stage5.glb",
      "blend_source":"CC0_Valoria_Source.blend","art_approved":False,
      "renders":["CC0_BastionV_Lookdev_1440x900.png",
                 "CC0_BastionI_Lookdev_1440x900.png",
                 "CC0_BastionV_Portrait_720x1200.png"]
    }
    (PREVIEW/"render-evidence.json").write_text(json.dumps(manifest,indent=2))
    print("ELDORIA_CC0_AUTHORING_PASS",json.dumps(manifest))
if __name__=="__main__":
    main()
