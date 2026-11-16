# Distinctive architecture of Valoria V7 — all geometry original.
# Runs AFTER physical V6 family and BEFORE Blender real renders.
# Not a photoshop mask, not production game assets, not a game clone.
from math import pi, sin, cos, hypot
from mathutils import Vector
VALORIA_V7_TAG = "V7"
style_count={"stone_arcades":0,"market_stalls":0,"wheeled_carts":0,"village_roofs":0,
             "buttresses":0,"resource_identity":0}

def arch_rough(label,x,y,z,rx,ry,material,theta=0):
    # Built-stone arch with actual three-dimensional, staggered voussoirs.
    count=13
    for i in range(count):
        a=pi*i/(count-1)
        ob=cube(label,(x+rx*cos(a),y,z+ry*sin(a)),
                (.54,.62,.52),material if i%3 else LIME,.075)
        ob.rotation_euler[1] = a-.5*pi
        style_count["stone_arcades"]+=1
def stones(positions,material):
    for i,(x,y,z) in enumerate(positions):
        ico("V7 warm hand-fitted cobble",(x,y,z),(.36+.10*(i%3),.32,.09),
            material if i%4 else LIME,1)

# SIGNATURE: military fortress is genuinely fortified, not a generic grey chalet.
# The grand Valorian keep gets projecting stepped abutments on its visible facade.
for sign in (-1,1):
    xx=kx+sign*8.05
    for offset in (-1.25,3.2,7.7):
        zz=kz
        y=ky-8.18+offset*0.04
        for j in range(3):
            cube("V7 flared hand-cut keep buttress",
                 (xx,y,zz+1.10+j*1.68),
                 (1.90-.17*j,1.68-.12*j,2.10),
                 STONE if j%2==0 else LIME,.25)
            style_count["buttresses"]+=1
    # Deliberate asymmetry: right tower has a watch-balcony, left a fire signal.
    if sign==1:
        cube("V7 right tower projecting oak balcony",
             (xx+3.35,ky-.78,kz+10.8),(5.8,2.2,.68),OAK,.20)
        for i in range(5):
            cube("V7 balcony hand rails",(xx+1.0+i*1.10,ky-2.05,kz+11.65),
                 (.25,.27,1.55),WOOD,.07)
        cube("V7 balcony chest-height rail",
             (xx+3.25,ky-2.15,kz+12.3),(5.75,.29,.22),COPPER,.06)
    else:
        cyl("V7 left-fire signal brazier tray",(xx-1.15,ky-5.0,kz+16.55),
            .85,.25,BRASS,12)
        ico("V7 signal embers",(xx-1.15,ky-5.0,kz+16.9),
            (.48,.45,.66),GLASS,2)
# The heavy frontage reads as a series of imposing inhabited arches.
for sign in (-1,1):
    xx=kx+sign*6.25
    arch_rough("V7 central inhabited inner arcade",xx,-6.17,kz+5.0,1.56,1.38,LIME)
    cube("V7 blackened inner arch depth",(xx,-6.04,kz+5.10),
         (1.84,.23,2.42),DARK,.20)
    cube("V7 firelit open arch",(xx,-6.23,kz+5.08),
         (.96,.11,1.68),GLASS,.08)
# Colossal approach banners use warped textile meshes rather than pasted rectangles.
def wave_flag(label,x,y,z,h,w,cloth):
    v=[];f=[]
    for j in range(6):
        for i in range(4):
            dx=(i/3-.5)*w
            zz=z-j*h/5
            v.append((x+dx,y+.08*sin(i*1.6+j*.7),zz))
    for j in range(5):
        for i in range(3):
            a=j*4+i
            f.append((a,a+1,a+5,a+4))
    ob=mesh(label,v,f,cloth)
    for p in ob.data.polygons:p.use_smooth=True
    cube(label+" / forged top rod",(x,y+.02,z+.08),
         (w+.48,.24,.16),BRASS,.04)
    return ob
for x in (kx-9.4,kx+9.4):
    wave_flag("V7 great indigo banner",x,-7.28,kz+9.4,5.6,2.1,BANNER)

# Architectural grandeur without floating bases: city defense quarter merges into soil.
for j in range(9):
    x=kx-11.3+j*2.85
    z=ground(x,-13.4)
    cube("V7 monumental gate paving stone",
         (x,-13.4,z+.02),(2.35,2.50,.10),
         STONE if j%3 else LIME,.095)
# A slight semicircular market square creates a living, human-scaled city center.
plaza_x,plaza_y=-16,-12
for i in range(58):
    a=2*pi*i/58
    r=5.9+.24*sin(i*.9)
    x=plaza_x+r*cos(a);y=plaza_y+2.9*sin(a)
    cube("V7 circular artisan-set piazza paving",
         (x,y,ground(x,y)+.045),
         (.90,.73,.11),LIME if i%5==0 else STONE,.06)

# WHOLE NEW MULTI-PART 3D MARKET ACTIVITY: original functional objects.
def market_stall(label,x,y,roofmat):
    z=ground(x,y)
    for sx in (-1,1):
        for sy in (-1,1):
            cube(label+" / shaped wooden upright",(x+sx*1.55,y+sy*1.05,z+1.65),
                 (.18,.18,3.25),OAK,.065)
    cube(label+" / solid display counter",(x,y,z+1.16),
         (3.45,2.35,.30),WOOD,.14)
    for k in range(6):
        bx=x-1.16+(k%3)*.96;by=y-.60+(k//3)*1.2
        ico(label+" / baskets of settlement supplies",(bx,by,z+1.48),
            (.39,.35,.32),EARTH2 if k%2 else SUN_LEAF,1)
    # Double-angled textile awning is authored geometry. 5° pitch, no image plane.
    verts=[(x-1.90,y-1.35,z+3.55),(x+1.90,y-1.35,z+3.55),
           (x+1.90,y+.0,z+4.20),(x-1.90,y+.0,z+4.20),
           (x-1.90,y+1.35,z+3.55),(x+1.90,y+1.35,z+3.55)]
    mesh(label+" / three-dimensional peaked canopy",verts,
         [(0,1,2,3),(3,2,5,4)],roofmat)
    for yy in (-1.35,1.35):
        cube(label+" / canopy embroidered hem",(x,y+yy,z+3.53),
             (3.9,.19,.23),BANNER_TRIM,.04)
    style_count["market_stalls"]+=1
# Keep open avenues and all four resource buildings visible and separated.
market_stall("V7 westbank grain exchange",-18.3,-14.9,BANNER)
market_stall("V7 mason's colored awning",14.8,-17.4,COPPER)

# Articulated service wagons help establish human scale and genuine settlement life.
def original_wagon(name,x,y,sx=1):
    z=ground(x,y)
    cube(name+" / shaped solid cargo cart",(x,y,z+1.02),
         (3.2,2.15,.68),OAK,.20)
    cube(name+" / cargo base inset",(x,y,z+1.40),
         (2.7,1.75,.23),WOOD,.07)
    for side in (-1,1):
        for xx in (x-1.05,x+1.05):
            cyl(name+" / circular carved heavy wheel",(xx,y+side*1.26,z+.78),
                .74,.23,WOOD,12,(pi/2,0,0))
            cyl(name+" / iron wheel hub",(xx,y+side*1.41,z+.78),
                .16,.13,BRASS,12,(pi/2,0,0))
    for i in range(7):
        ico(name+" / hand-stacked cargo",(x-.9+i*.30,y,z+1.83),
            (.26,.41,.41),ROCK[i%3] if sx==-1 else EARTH2,1)
    cube(name+" / hitch shafts",(x+2.3,y,z+.75),
         (2.2,.20,.17),OAK,.04)
    style_count["wheeled_carts"]+=1
original_wagon("V7 sawmill timber supply wagon",-38,-17)
original_wagon("V7 quarry stone delivery cart",36,-24,-1)
original_wagon("V7 granary golden harvest carriage",-37,27)

# RESOURCE BUILDINGS: recognizable at silhouette level, not mere house recoloring.
# WEST SAWMILL gets an elevated, separate shingle-roofed cutting annex.
a_x,a_y=-39,-3
a_z=ground(a_x,a_y)
for xx in (a_x-2.2,a_x+2.2):
    for yy in (a_y-1.6,a_y+1.6):
        cube("V7 sawmill woodpile shelter leg",
             (xx,yy,a_z+2.0),(.42,.42,4.0),WOOD,.11)
gable("V7 original timber-cutting annex",a_x,a_y,a_z+4.05,6.2,5.8,2.45,ROOF_DARK)
for i in range(7):
    cube("V7 sawmill stacking rack shafts",
         (a_x-2.7+i*.9,a_y-2.6,a_z+.49),
         (.30,4.1,.60),OAK,.07)
style_count["resource_identity"]+=1

# GRANARY gets physical vaulted straw bins beneath its dramatically shaped silos.
for side in (-1,1):
    xx=-30+side*6.3
    yy=19+7.0
    b=ground(xx,yy)
    for i in range(3):
        ico("V7 harvested hay bale rounded forms",
            (xx+(i-1)*1.34,yy,b+.7),
            (.78,.71,.72),SUN_LEAF,2)
    cube("V7 granary carved storage rack",
         (xx,yy-.8,b+.39),(5.5,.51,.75),WOOD,.11)
style_count["resource_identity"]+=1

# BARRACKS gets a true projecting drill-house yard, original mini-turret with flags.
qbx,qby=35.2,19
qz2=ground(qbx,qby)
drum("V7 military drill-house",qbx,qby,qz2,5.0,2.22,STONE)
roof_spire("V7 military gilt watch",qbx,qby,qz2+5.22,2.50,3.0,ROOF_DARK)
wave_flag("V7 military standard",qbx,qby-2.4,qz2+5.1,2.6,1.5,BANNER)
for i in range(7):
    xx=22.5+i*1.75
    yy=18.8
    cyl("V7 upright training lances",(xx,yy,ground(xx,yy)+1.45),
        .10,2.65,OAK,8)
style_count["resource_identity"]+=1

# QUARRY gets a crane pulley and actual excavated stone strata, not a flat toy shop.
qc_x,qc_y=34.0,-18
for ring in range(3):
    radius=6.0+ring*2.4
    for i in range(13):
        a=pi*.17+pi*1.65*i/12
        x=qc_x+radius*cos(a);y=qc_y+radius*.63*sin(a)
        zo=ground(x,y)
        ico("V7 sculpted exposed quarry walls",
            (x,y,zo+.26),
            (1.25+.16*ring,1.00,.58+.12*ring),
            ROCK[(i+ring)%3],1)
for j in range(11):
    xx=23+(j%4)*1.35;yy=-21+(j//4)*1.45
    z=ground(xx,yy)
    cube("V7 neatly stacked carved quarry materials",
         (xx,yy,z+.30),(1.11,1.07,.55),
         LIME if j%3==0 else STONE,.08)
style_count["resource_identity"]+=1

# ECOLOGY AND NARRATIVE: Brecha is an actual broken seam in the landscape.
# Existing rare rocks are complemented with one unmistakable outcrop near the horizon.
bx,by=40,27
for i in range(13):
    ang=i*.93
    rr=2.7+(i%4)*.64
    xx=bx+rr*cos(ang);yy=by+rr*sin(ang)
    zz=ground(xx,yy)
    ico("V7 Brecha ancient obsidian protecting fissure",(xx,yy,zz+.56),
        (.58,.49,.98),ROCK[1] if i%3==0 else DARK,1)
for xx,yy,scale in ((39.7,28.0,1.0),(42.2,29.1,1.25),(38.1,25.0,.81)):
    zz=ground(xx,yy)
    ico("V7 geological violet crystal erupts from real ground",
        (xx,yy,zz+1.05*scale),
        (.51*scale,.32*scale,1.35*scale),VIOLET,1)

# Thin picture-book silhouettes must not look like floating platforms: all props
# are grounded by the same terrain function, and no sprites substitute 3D geometry.
print("VALORIA_V7_ARCHITECTURE_PASS",style_count)
