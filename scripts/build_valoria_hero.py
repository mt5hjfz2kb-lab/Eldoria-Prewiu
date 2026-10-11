"""Author one original Valoria fortress fragment in Blender 4.5.

Outputs a baked FBX and three review renders. It never touches runtime Valoria.
Coordinates in this source: X lateral, Y depth (negative toward the viewer), Z up.
"""

import math
import os
import random
import sys
import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from valoria_hero_materials import make_textures

OUT = os.environ.get("ELDORIA_HERO_OUTPUT", os.path.abspath("Unity/Assets/Eldoria/ArtTests/OriginalHero"))
SEED = random.Random(7119)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
images = make_textures()


def material(name, color, image=None, roughness=.82, metal=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metal
    if image:
        node = m.node_tree.nodes.new("ShaderNodeTexImage")
        node.image = images[image]
        m.node_tree.links.new(node.outputs["Color"], bsdf.inputs["Base Color"])
    return m


MATERIAL = {
    "stone": material("VALORIA limestone", (.66,.62,.55),"limestone"),
    "pale": material("VALORIA carved limestone", (.80,.76,.67),"limestone"),
    "old": material("VALORIA ancestral stone", (.49,.51,.46),"oldstone"),
    "shadow": material("VALORIA recess", (.07,.10,.12),roughness=.94),
    "roof": material("VALORIA slate", (.13,.19,.26),"slate"),
    "roofold": material("VALORIA oxidized slate", (.20,.22,.22),"slate"),
    "timber": material("VALORIA weathered oak", (.30,.21,.14),"oak"),
    "iron": material("VALORIA iron", (.19,.23,.24),roughness=.48,metal=.7),
    "blue": material("VALORIA heraldic blue", (.035,.10,.26),roughness=.7),
    "gold": material("VALORIA worn brass", (.47,.32,.13),roughness=.52,metal=.65),
    "glass": material("VALORIA dim window", (.12,.19,.23),roughness=.35),
    "warm": material("VALORIA warm window", (.9,.53,.22),roughness=.3),
    "rock": material("VALORIA cliff strata", (.43,.40,.35),"rock"),
    "earth": material("VALORIA earth", (.27,.29,.25),roughness=1),
    "forest": material("VALORIA forest", (.12,.21,.15),roughness=1),
}


class Mesh:
    def __init__(self, name, mat):
        self.name, self.mat = name, mat
        self.vertices, self.faces, self.uv = [], [], []

    def face(self, vertices, uvs=None):
        start = len(self.vertices)
        self.vertices.extend(vertices)
        self.faces.append(tuple(range(start, start + len(vertices))))
        self.uv.extend(uvs or [(v[0]*.26,v[2]*.26) for v in vertices])

    def box(self, x, y, z, w, d, h, turn=0, phase=0):
        a,b,c = w/2,d/2,h/2
        local = [(-a,-b,-c),(a,-b,-c),(a,b,-c),(-a,b,-c),
                 (-a,-b,c),(a,-b,c),(a,b,c),(-a,b,c)]
        angle=math.radians(turn)
        co,si=math.cos(angle),math.sin(angle)
        pts=[(x+co*px-si*py,y+si*px+co*py,z+pz) for px,py,pz in local]
        for ids in [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]:
            quad=[pts[i] for i in ids]
            self.face(quad, [(i*.26+phase, j*.26+phase) for i,j in
                             ((0,0),(w if ids[0] in (0,4,3,7) else d,0),
                              (w if ids[0] in (0,4,3,7) else d,h),(0,h))])

    def prism(self, points, y0, y1, phase=0):
        # Arbitrary X/Z silhouette extruded in depth, used for gabled masonry.
        front=[(x,y0,z) for x,z in points]
        back=[(x,y1,z) for x,z in points]
        area=sum(points[i][0]*points[(i+1)%len(points)][1] -
                 points[(i+1)%len(points)][0]*points[i][1] for i in range(len(points)))
        front_out=front if area>0 else list(reversed(front))
        back_out=list(reversed(back)) if area>0 else back
        self.face(front_out,[(x*.29+phase,z*.29+phase) for x,_,z in front_out])
        self.face(back_out,[(x*.29+phase,z*.29+phase) for x,_,z in back_out])
        for i in range(len(points)):
            k=(i+1)%len(points)
            q=[front[i],front[k],back[k],back[i]]
            if area>0:q.reverse()
            self.face(q,[(0,0),(math.dist(points[i],points[k])*.3,0),
                         (math.dist(points[i],points[k])*.3,(y1-y0)*.3),(0,(y1-y0)*.3)])

    def finish(self, bevel=0):
        mesh=bpy.data.meshes.new(self.name)
        mesh.from_pydata(self.vertices,[],self.faces)
        mesh.update()
        obj=bpy.data.objects.new(self.name,mesh)
        bpy.context.collection.objects.link(obj)
        mesh.materials.append(self.mat)
        layer=mesh.uv_layers.new(name="ValoriaUV")
        for polygon in mesh.polygons:
            for j,li in enumerate(polygon.loop_indices):
                layer.data[li].uv=self.uv[polygon.loop_start+j]
        if bevel:
            mod=obj.modifiers.new("Soft hand-cut stone edges", "BEVEL")
            mod.width=bevel
            mod.segments=2
            mod.affect="EDGES"
            mod.limit_method="ANGLE"
            mod.angle_limit=math.radians(30)
            bpy.context.view_layer.objects.active=obj
            bpy.ops.object.modifier_apply(modifier=mod.name)
            normal=obj.modifiers.new("Weighted masonry normals", "WEIGHTED_NORMAL")
            bpy.ops.object.modifier_apply(modifier=normal.name)
        return obj


B={key:Mesh("VA Hero / "+key,MATERIAL[key]) for key in MATERIAL}


def box(k,x,y,z,w,d,h,turn=0):
    B[k].box(x,y,z,w,d,h,turn,SEED.random()*4)


def arch(cx,y,base,width,height,thick,depth,key="pale",segments=11):
    """Full-depth, real open arch: separate jambs and voussoir ring."""
    half=width/2
    radius=half
    spring=base+height-radius
    for side in [-1,1]:
        box(key,cx+side*(half+thick/2),y,base+(spring-base)/2,thick,depth,spring-base)
        box("old",cx+side*(half+thick+.11),y,base+.34,.22,depth+.24,.68)
    for i in range(segments):
        a=math.pi*i/segments
        b=math.pi*(i+1)/segments
        inner=lambda t: (cx+radius*math.cos(t),spring+radius*math.sin(t))
        outer=lambda t: (cx+(radius+thick)*math.cos(t),spring+(radius+thick)*math.sin(t))
        pts=[inner(a),inner(b),outer(b),outer(a)]
        B[key if i%5 else "old"].prism(pts,y-depth/2,y+depth/2,phase=i*.3)
    box("pale",cx,y,base-.13,width+2*thick+.4,depth+.44,.23)


def bay(x,y,z,w=1.05,h=2.9,front=-1,lit=False,stone="stone"):
    """Deep embrasure with paired columns, mullion, keystone and drip ledge."""
    d=.20
    box("shadow",x,y+front*.09,z,w,d,h)
    box("warm" if lit else "glass",x,y+front*.22,z,w*.64,.035,h*.73)
    for side in (-1,1):
        box("pale",x+side*(w/2+.16),y+front*.3,z,.26,.55,h+.30)
        box("old",x+side*(w/2+.16),y+front*.36,z-h*.46,.40,.67,.17)
    box("pale",x,y+front*.39,z+h/2+.16,w+.72,.72,.29)
    box("pale",x,y+front*.43,z-h/2-.08,w+.66,.79,.25)
    box("pale",x,y+front*.41,z+h/2+.34,.29,.54,.39)
    box("iron",x,y+front*.27,z,.055,.07,h*.74)


def buttress(x,y,z,height,forward=.3):
    box("old",x,y-forward,z+height/2,.96,1.20+forward,height)
    box("pale",x,y-forward-.17,z+height-.16,1.22,1.48+forward,.36)
    box("stone",x,y-forward-.38,z+height*.38,1.45,1.73+forward,.33)
    box("old",x,y-forward-.17,z+.25,1.50,1.47+forward,.50)


def masonry_courses(x0,x1,y,z0,z1,offset=.045):
    """Shallow individually offset ashlar joints, confined to a solid wall face."""
    for course in range(int((z1-z0)/.66)):
        zz=z0+course*.66
        box("old" if course%6==0 else "pale",(x0+x1)/2,y-offset,zz,x1-x0,.07,.043)
        xx=x0+(.31 if course%2 else .76)
        while xx<x1-.16:
            box("old",xx,y-offset-.02,zz+.32,.035,.083,.54)
            xx+=1.35+(course%3)*.06


def parapet(x0,x1,y,z,damaged=False):
    box("pale",(x0+x1)/2,y,z,x1-x0,.55,.25)
    x=x0+.32
    index=0
    while x<x1-.15:
        if not damaged or index%7 not in (4,5):
            box("old" if index%4==0 else "stone",x,y,z+.5,.52,.63,.79-(.20 if damaged and index%5==0 else 0))
        x+=.97
        index+=1


def pitched_roof(x,y,eave,w,d,rise,key="roof",ridge_axis="y"):
    """Steep weathered pitched roof with visibly overlapping uneven slate courses."""
    if ridge_axis=="x":
        # Same roof rotated by ninety degrees around its centre.
        for sign in [-1,1]:
            lower=(x,y+sign*d/2,eave)
            upper=(x,y,eave+rise)
            verts=[(x-w/2,lower[1],lower[2]),(x+w/2,lower[1],lower[2]),
                   (x+w/2,upper[1],upper[2]),(x-w/2,upper[1],upper[2])]
            if sign>0: verts.reverse()
            B[key].face(verts,[(0,0),(w*.48,0),(w*.48,rise*.48),(0,rise*.48)])
        box("timber",x,y,eave+rise+.03,w+.33,.22,.26)
    else:
        for sign in [-1,1]:
            lo=x+sign*w/2
            verts=[(lo,y-d/2,eave),(lo,y+d/2,eave),
                   (x,y+d/2,eave+rise),(x,y-d/2,eave+rise)]
            if sign<0: verts.reverse()
            B[key].face(verts,[(0,0),(d*.45,0),(d*.45,rise*.5),(0,rise*.5)])
        box("timber",x,y,eave+rise+.02,.22,d+.33,.22)
    # Raised courses supply actual shadow and break a single triangular roof silhouette.
    count=max(3,int((w if ridge_axis=="y" else d)/.52))
    for t in range(count):
        ratio=(t+.25)/count
        for sign in (-1,1):
            if ridge_axis=="y":
                px=x+sign*w/2*(1-ratio)
                z=eave+rise*ratio+.04
                box(key,px,y,z,.16,d+.24,.065,turn=0)
            else:
                py=y+sign*d/2*(1-ratio)
                z=eave+rise*ratio+.04
                box(key,x,py,z,w+.24,.16,.065)
    for side in (-1,1):
        if ridge_axis=="y":box("timber",x+side*(w/2+.06),y,eave-.06,.18,d+.36,.29)
        else:box("timber",x,y+side*(d/2+.06),eave-.06,w+.36,.18,.29)


def turret(cx,cy,base,radius,top,sides=10,roof=True,weather=0):
    # Many-sided masonry is one structural body, with asymmetrical buttresses and cornice.
    vertices=[]
    for i in range(sides):
        a=2*math.pi*i/sides+.10
        vertices.append((cx+math.cos(a)*radius,cy+math.sin(a)*radius))
    for i in range(sides):
        x0,y0=vertices[i]
        x1,y1=vertices[(i+1)%sides]
        cx0,cy0=(x0+x1)/2,(y0+y1)/2
        length=math.hypot(x1-x0,y1-y0)
        angle=math.degrees(math.atan2(y1-y0,x1-x0))
        box("stone" if weather<.5 else "old",cx0,cy0,(base+top)/2,length+.12,.43,top-base,angle)
        if i%3==0:
            box("pale",cx0,cy0,top-.36,length+.35,.74,.46,angle)
        if i in (1,4,7) and top-base>10:
            wx=cx0+math.sin(math.radians(angle))*.27
            wy=cy0-math.cos(math.radians(angle))*.27
            box("shadow",wx,wy,top-4,.72,.1,2.4,angle)
            box("pale",wx,wy,top-2.64,.94,.23,.23,angle)
    box("pale",cx,cy,top-.17,radius*2.14,radius*2.14,.39,turn=12)
    if roof:
        # The lead-covered spire contains multiple setbacks and a lantern.
        for low,r0,r1 in [(top,radius+0.5,radius*.58),(top+2.4,radius*.58,radius*.29)]:
            for i in range(sides):
                a=2*math.pi*i/sides+.10
                b=2*math.pi*(i+1)/sides+.10
                ra=r0 if low==top else radius*.58
                bottom=[(cx+ra*math.cos(a),cy+ra*math.sin(a),low),
                        (cx+ra*math.cos(b),cy+ra*math.sin(b),low)]
                upper=[(cx+r1*math.cos(b),cy+r1*math.sin(b),low+2.4),
                       (cx+r1*math.cos(a),cy+r1*math.sin(a),low+2.4)]
                B["roof" if i%3 else "roofold"].face(bottom+upper,[(0,0),(1,0),(1,1),(0,1)])
            box("gold",cx,cy,low+2.37,.28,.28,.28)
        box("iron",cx,cy,top+5.2,.095,.095,1.7)
        box("gold",cx,cy,top+6.04,.44,.08,.08)
    else:
        for i in range(sides):
            if (i+weather*7)%6<2:continue
            a=2*math.pi*i/sides+.10
            px=cx+radius*math.cos(a)
            py=cy+radius*math.sin(a)
            box("old",px,py,top+.47,.52,.51,.97,math.degrees(a))


# A terraced geological base under the entire building, not a flat plinth.
for row in range(5):
    x0=-18-row*.48
    x1=15+row*.72
    y0=-10-row*.85
    y1=9+row*.2
    z=-2.65+row*.48
    contour=[]
    for i in range(26):
        a=2*math.pi*i/26
        cx=(x0+x1)/2+(x1-x0)/2*math.cos(a)
        cy=(y0+y1)/2+(y1-y0)/2*math.sin(a)
        wobble=1+.055*math.sin(a*7+row*.7)+.04*math.sin(a*13+row)
        contour.append((cx*wobble,cy*wobble,z+.27*math.sin(a*4+row)))
    for i in range(len(contour)):
        a=contour[i]
        c=contour[(i+1)%len(contour)]
        lower1=(a[0]*1.08,a[1]*1.06,z-1.1-row*.17)
        lower2=(c[0]*1.08,c[1]*1.06,z-1.1-row*.17)
        B["rock"].face([a,c,lower2,lower1])
box("rock",-1,0,1.0,27,15,3.1)
box("stone",-1,0,2.62,25,13.8,.45)

# Connected keep bodies. Steps and offsets follow a continuous defensive footprint.
box("stone",-1.0,1.2,9.4,11.4,7.6,13.0)
box("old",-4.9,2.2,8.2,6.3,9.7,10.6)
box("stone",4.2,2.1,12.9,6.4,6.3,20.1)
box("old",-3.0,4.7,13.8,7.8,3.5,15.2)
box("stone",-1,1.2,16.05,11.8,7.9,.38)
box("pale",-1,1.2,14.55,11.8,7.9,.30)
box("pale",4.2,2.1,22.98,6.8,6.7,.35)
box("stone",-1.4,-.55,5.4,14.5,7.6,5.8)
box("old",6.2,-.65,6.4,5.5,6.2,7.7)

# Hall gables and dark, offset, multilevel roofline.
B["stone"].prism([(-6.7,16.2),(4.5,16.2),(-1.1,20.8)],-2.65,5.05)
pitched_roof(-1.1,1.15,16.4,11.8,8.1,4.6)
pitched_roof(-5.3,3.3,13.8,6.0,6.0,3.4,key="roofold",ridge_axis="x")
pitched_roof(4.1,2.4,23.25,6.9,6.8,3.9,ridge_axis="x")
pitched_roof(6.6,-.75,10.9,5.8,6.5,2.9,key="roofold")

# The taller dominant watchtower is integrated into the hall, with a lantern.
turret(5.7,3.5,4.2,2.68,26.3,sides=12,roof=True)
turret(-7.0,.1,2.7,2.4,17.3,sides=9,roof=True)
turret(9.2,-2.2,2.8,1.8,14.4,sides=10,roof=False,weather=.35)

# The main tower rises from the hall, with an open belfry, multiple galleries
# and an asymmetrical watch platform. All features are authored into this mesh.
for level in (11.6,16.4,21.7,25.3):
    for sx in (3.12,8.34):
        box("old",sx,3.5,level,.46,5.98,.28)
    box("pale",5.7,1.04,level,5.73,.48,.31)
for zz in (15.4,20.3,24.4):
    bay(5.65,.81,zz,w=.83,h=2.12,lit=zz==20.3)
for sx in (3.22,8.10):
    buttress(sx,.84,5.5,15.1,forward=.11)
box("old",5.7,.13,24.9,6.18,2.55,.48)
for sx in (3.10,4.43,5.76,7.09,8.42):
    box("pale",sx,-.97,25.5,.24,.29,.95)
box("iron",5.76,-1.24,25.98,5.6,.1,.12)

# Carved porches, deep reveals, and a varied arcade visibly belong to the
# older masonry body instead of being detached decoration.
for xx in (-4.35,-1.85,.85):
    arch(xx,-3.08,4.17,1.34,3.2,.28,.48,key="pale",segments=7)
    box("shadow",xx,-3.04,5.15,1.24,.08,2.0)
    box("glass",xx,-3.14,5.13,.70,.08,1.42)
for xx in (-5.7,-.65,2.8):
    buttress(xx,-2.94,7.90,8.28,forward=.06)
masonry_courses(-6.60,3.90,-2.62,12.96,16.02)
masonry_courses(2.32,5.75,-1.92,4.18,10.78)

# Front rampart connects hall to towers. A real gate opening interrupts its wall.
for x,w in [(-9.0,4.5),(-4.0,3.0),(2.9,4.3),(8.0,3.7)]:
    box("old",x,-5.5,5.2,w,1.55,5.2)
parapet(x-w/2,x+w/2,-6.13,8.05,damaged=(x<0))
arch(-.35,-5.5,2.66,3.2,5.85,.68,1.7)
arch(-.35,-4.6,2.66,3.2,5.85,.38,.55,key="old")
# Wooden doors sit deep inside the opening, leaving a shaded stone tunnel.
for sx in (-1.09,.39):
    box("timber",sx,-3.97,4.43,1.32,.24,3.18)
    for zz in (3.24,4.31,5.39):
        box("iron",sx,-4.12,zz,1.20,.12,.14)
box("iron",-.35,-4.35,5.95,3.05,.17,.17)
for bx in [-10.8,-7.7,-4.6,3.6,7.1,10.2]:
    buttress(bx,-5.6,2.68,6.1,forward=.45)

# Two eras of built masonry: arcades and bracing behind the rebuilt wing.
for cx in [-10.8,-6.1]:
    arch(cx,7.2,3.1,2.45,11.6,.67,1.7,key="old")
box("old",-8.55,7.2,14.64,9.3,1.9,1.8)
box("old",-10.9,7.2,17.6,2.1,1.9,4.5)
box("old",-6.15,7.2,16.4,1.8,1.9,2.3)
box("old",-8.4,7.2,19.2,4.9,1.4,.5,turn=7)
parapet(-12.6,-7.2,7.2,19.7,damaged=True)

# Deep facade bays: rhythm breaks around the central heraldic balcony.
for x in [-4.9,-2.6,2.2,4.5]:
    for zz in [8.9,12.9]:
        bay(x,-2.76,zz,lit=(x==-2.6 and zz==8.9))
for zz in (10.8,15.2,20.0):
    bay(7.43,.55,zz,w=.72,h=2.3,lit=(zz==15.2))
for x in (-7.3,-4.6,3.4,7.2):
    box("pale",x,-2.95,15.95,.36,.77,.60)

# Projecting royal balcony and gallery turn otherwise flat facades into inhabited spaces.
box("old",-.25,-3.54,12.92,4.4,1.44,.43)
for sx in (-1.96,1.45):
    box("pale",sx,-4.10,13.7,.35,.38,1.58)
    box("pale",sx,-3.72,12.35,.53,1.95,.72)
box("iron",-.25,-4.22,13.50,3.55,.12,.17)
for x in (-1.8,-1.1,-.4,.3,1.0,1.7):
    box("iron",x,-4.22,13.20,.065,.07,.72)
box("blue",-.25,-4.45,11.28,1.05,.065,2.88)
box("gold",-.25,-4.54,12.63,.27,.05,.48)

# Timber reconstruction: cross-bracing, slate foot bridge and selective scaffold.
for i in range(7):
    x=4.4+i*.67
    box("timber",x,5.50,7.9,.12,.15,7.3)
box("timber",6.8,5.55,11.4,5.5,.19,.20)
box("timber",6.8,5.55,7.0,5.5,.19,.19)
for x in (5.1,7.1,9.1):
    box("timber",x,6.2,5.5,.20,2.6,5.3,turn=15)

# One small lower-city wing is attached by the stepped road to the main gate.
box("old",-10.9,-6.5,2.34,6.1,5.6,5.5)
box("timber",-10.9,-9.45,3.9,6.4,.22,.20)
pitched_roof(-10.9,-6.6,5.48,7,6.55,3.4,key="roofold",ridge_axis="x")
for x in (-12.3,-10.8,-9.3):
    bay(x,-9.38,3.55,w=.64,h=1.47,lit=(x==-10.8))
box("timber",-10.85,-9.9,1.0,.96,.35,2.2)
box("iron",-10.85,-10.13,1.08,.68,.1,1.68)
for step in range(12):
    zz=-.2+step*.247
    yy=-11.3+step*.43
    box("old",-5.2,yy,zz,4.2,.49,.24,turn=-4+step*.7)

# A broken flying passage attaches the ancestral vault to the hall and makes
# the ruins read as an earlier, larger building embedded in the newer fortress.
arch(-8.3,5.67,10.80,2.95,7.6,.77,1.70,key="old")
box("old",-8.35,5.67,18.73,9.1,1.81,.39,turn=-4)
for xx in (-11.45,-9.32,-7.11):
    box("old",xx,5.68,19.26,.38,1.80,.80,turn=-8)

# Small fractured rock strata along the shoulders of the architecture.
for i in range(45):
    ang=SEED.uniform(-math.pi,math.pi)
    radius=SEED.uniform(13.1,17.3)
    xx=radius*math.cos(ang)
    yy=radius*.69*math.sin(ang)-.2
    height=SEED.uniform(.45,2.0)
    box("rock",xx,yy,-1.15+height*.35,SEED.uniform(.7,1.9),SEED.uniform(.6,1.8),height,SEED.uniform(-25,25))

architectural=[]
for key,mesh in B.items():
    if not mesh.faces:continue
    architectural.append(mesh.finish(.045 if key in ("stone","old","pale","timber","rock") else 0))

# Render-only environment supports the hero silhouette and never enters the FBX.
# A continuous undulating ground eliminates the floating, rectangular display
# plinth of the first review. The authored rocky terrace is partially buried.
ground_mat=MATERIAL["earth"]
N=44
verts=[]
faces=[]
for iy in range(N+1):
    yy=-69+iy*138/N
    for ix in range(N+1):
        xx=-85+ix*170/N
        radius=math.hypot(xx*.86,yy)
        hills=.9*math.sin(xx*.105+yy*.03)+1.2*math.sin(yy*.081)+1.1*math.cos(xx*.054-yy*.065)
        if radius>21:hills+=min(5.5,(radius-21)*.11)
        verts.append((xx,yy,-3.35+hills))
for iy in range(N):
    for ix in range(N):
        j=iy*(N+1)+ix
        faces.append((j,j+1,j+N+2,j+N+1))
mesh=bpy.data.meshes.new("Valley topography")
mesh.from_pydata(verts,[],faces)
mesh.update()
ground=bpy.data.objects.new("REVIEW ONLY / irregular valley",mesh)
bpy.context.collection.objects.link(ground)
mesh.materials.append(ground_mat)

def render_only_box(name,loc,scale,mat):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object
    o.name=name
    o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mat)
    return o

# Restrained broadleaf silhouettes are only backdrop markers for this gate.
for i in range(32):
    angle=i*2.39996
    radius=21+(i%8)*3.2
    xx=radius*math.cos(angle)
    yy=radius*.72*math.sin(angle)
    height=3.3+(i%4)*.6
    render_only_box("REVIEW ONLY / distant tree trunk",(xx,yy,-4+height/2),(.18,.18,height),MATERIAL["timber"])
    for tuft in range(6):
        direction=tuft*2.39996
        spread=.30+(.24 if tuft%2 else .48)
        bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=1,
            location=(xx+math.cos(direction)*spread,yy+math.sin(direction)*spread,
                      -4+height+(.12 if tuft%3 else .44)))
        tree=bpy.context.object
        tree.name="REVIEW ONLY / distant forest canopy"
        tree.scale=(.82,.73,.72)
        tree.data.materials.append(MATERIAL["forest"])

# Two interleaved valley ridges provide atmospheric context without building
# an unrelated city; their pale profile remains outside the exported model.
for bank,distance,base,amplitude in (("distant",51,2,5.5),("horizon",82,8,7.8)):
    profile=[]
    for i in range(80):
        xx=-140+i*280/79
        zz=base+amplitude*(.42+.27*math.sin(xx*.05+distance)+.19*math.sin(xx*.11))
        profile.append((xx,distance,zz))
    vertices=profile+[(v[0],distance,-9) for v in profile]
    polys=[(i,i+1,i+1+len(profile),i+len(profile)) for i in range(len(profile)-1)]
    rmesh=bpy.data.meshes.new(bank)
    rmesh.from_pydata(vertices,[],polys)
    rmesh.materials.append(material("REVIEW ONLY / "+bank,
        (.31,.40,.45) if bank=="distant" else (.51,.61,.66)))
    ridge=bpy.data.objects.new("REVIEW ONLY / "+bank,rmesh)
    bpy.context.collection.objects.link(ridge)

world=bpy.context.scene.world or bpy.data.worlds.new("Valoria clear air")
bpy.context.scene.world=world
world.use_nodes=True
world.node_tree.nodes["Background"].inputs["Color"].default_value=(.48,.60,.74,1)
world.node_tree.nodes["Background"].inputs["Strength"].default_value=.6

def lamp(name,kind,loc,energy,color,size=5):
    data=bpy.data.lights.new(name,kind)
    data.energy=energy
    data.color=color
    if kind=='AREA':data.shape='DISK';data.size=size
    obj=bpy.data.objects.new(name,data)
    bpy.context.collection.objects.link(obj)
    obj.location=loc
    rot=Vector((-1,0,9))-obj.location
    obj.rotation_euler=rot.to_track_quat('-Z','Y').to_euler()
    return obj

lamp("Golden directional afternoon",'SUN',(-13,-19,33),2.2,(1,.80,.61))
lamp("Cool aerial fill",'AREA',(19,14,25),1900,(.50,.66,1),16)
lamp("Gate embers",'POINT',(-.2,-5.7,6.5),290,(1,.45,.19))

def camera(name,loc,target,ortho):
    bpy.ops.object.camera_add(location=loc)
    obj=bpy.context.object
    obj.name=name
    obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
    obj.data.type='ORTHO'
    obj.data.ortho_scale=ortho
    obj.data.lens=47
    return obj

cameras=[
    camera("establishing",(49,-61,40),(-1,-.5,14),64),
    camera("approach",(18,-55,31),(-.6,-.4,13),52),
    camera("oblique",(-53,-43,38),(-1,1,13),60),
]

scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=int(os.environ.get("ELDORIA_HERO_SAMPLES","24"))
scene.render.resolution_x=1280
scene.render.resolution_y=720
scene.render.resolution_percentage=int(os.environ.get("ELDORIA_HERO_RESOLUTION","75"))
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'

os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='DESELECT')
for obj in architectural:obj.select_set(True)
bpy.context.view_layer.objects.active=architectural[0]
fbx=os.path.join(OUT,"valoria_hero_fragment.fbx")
bpy.ops.export_scene.fbx(filepath=fbx,use_selection=True,axis_forward='-Z',axis_up='Y',
                         apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,path_mode='RELATIVE')
for cam in cameras:
    scene.camera=cam
    scene.render.filepath=os.path.join(OUT,"review-"+cam.name+".png")
    bpy.ops.render.render(write_still=True)
print("ELDORIA_HERO_READY",fbx)
