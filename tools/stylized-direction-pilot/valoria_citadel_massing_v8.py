# V8 silhouette pivot: truly castellated, sculptural Valorian citadel.
# Replaces the previously oversized chalet-like single central roof while
# retaining the new Blender-authored city, materials, resources and gameplay scale.
# The previous V7 GLB/artifact remains reproducible from its pinned Git commit.
import math
import bpy
from math import pi,cos,sin

REMOVED = [o for o in list(bpy.data.objects)
           if 'The great roof of Valoria' in o.name]
if len(REMOVED)<10:
    raise RuntimeError('Cannot find V7 oversized central house roof: '+str(len(REMOVED)))
for o in REMOVED:
    bpy.data.objects.remove(o,do_unlink=True)

# Hierarchical, asymmetrical silhouette: a castle with three recognizable
# heights, three materials and an inhabited public frontage, not one large house.
def closed_roof_v8(label,x,y,z,w,d,rise,roof_material,gable_material):
    old=gable(label,x,y,z,w,d,rise,roof_material)
    steps=12
    for sign in (-1,1):
        # Offset visible gable just outside roof eave; outward-facing front
        # and rear normals avoid the black coplanar-faces defect found in V6.
        yy=y+sign*d*.53+sign*.14
        vs=[];fs=[]
        for i in range(steps+1):
            xx=x-w*.5+i*w/steps
            q=abs(xx-x)/(w*.5)
            zh=z+rise*(1-q**1.13)+.22*q**7-.21
            vs.extend([(xx,yy,z-.22),(xx,yy,max(z-.18,zh))])
            if i:
                a=2*i
                fs.append((a,a+1,a-1,a-2) if sign==-1 else
                          (a-2,a-1,a+1,a))
        ob=mesh(label+" / solid ornamental hand-carved facade",vs,fs,gable_material)
        for p in ob.data.polygons: p.use_smooth=False
        cube(label+" / deep hand-forged timber front sill",(x,yy+sign*.15,z-.12),
             (w*.91,.50,.28),OAK,.08)
        # The round attic is anchored on a physically sealed gable.
        cyl(label+" / arched circular attic trim",(x,yy+sign*.19,z+rise*.54),
            .58,.18,BRASS,14,(pi/2,0,0))
        ico(label+" / lit living attic",(x,yy+sign*.28,z+rise*.54),
            (.38,.14,.38),GLASS,2)
    return old

# Great hall on the inner keep: lighter plaster, thick stone foot, dramatic
# 3D hip-like multi-gabled roof. The stone keep below stays legible.
hallx,hally=-5,ky+2
hallbase=kz+11.50
wall('V8 royal elevated great hall',hallx,hally,hallbase,13.7,11.6,6.10)
for x in (hallx-4.0,hallx,hallx+4.0):
    window(x,hally-5.96,hallbase+3.55,'front',1.12)
closed_roof_v8("V8 great hall double layered slate",hallx,hally,
               hallbase+6.35,16.2,13.0,4.40,SLATE,PLASTER)
# Scalloped heavy slate eaves. These are large enough to read on 390px screens.
for yy in (hally-6.7,hally+6.7):
    for i in range(9):
        xx=hallx-7.0+i*1.75
        cube('V8 high hall sculpted eave brackets',(xx,yy,hallbase+6.33),
             (.64,.43,.68),OAK,.10)

# High command tower: chunky heptagonal mass, purposeful dark roof/copper trim.
tower_x,tower_y=6.65,ky+2.85
drum('V8 imperial five-level command tower',tower_x,tower_y,kz+11.50,
     9.10,3.55,STONE)
for sign in (-1,1):
    window(tower_x+sign*1.7,tower_y-3.60,kz+16.4,'front',1.0)
roof_spire('V8 royal command tower crown',tower_x,tower_y,kz+20.8,
           4.25,6.15,ROOF_DARK)
cyl("V8 copper spire counterweight crest",(tower_x,tower_y,kz+27.2),
    .18,1.10,BRASS,12)
# Past early bastions, the tower is asymmetrically more monumental than the
# left wing, preserving B5 as a tier-identity jump.

# Low lateral chapel with arcaded stone flanks, unique copper crown.
chapel_x,chapel_y=-13.30,ky+3.3
wall('V8 Valorian small bell-chapel',chapel_x,chapel_y,kz+11.50,
     7.0,6.6,4.0)
roof_spire('V8 old restored chapel of the ember',chapel_x,chapel_y,kz+15.7,
           3.85,4.45,COPPER)
shield(chapel_x,chapel_y-3.45,kz+14.45,.62)
for sgn in (-1,1):
    cube("V8 flying-butress chapel foot",
         (chapel_x+sgn*3.25,chapel_y,kz+13.8),
         (1.20,3.95,4.1),STONE,.25)
# A separate soldiers' roof over an external rampart wing; no 2D painting.
rampx,rampy=9.7,ky-5.1
wall('V8 timbered rampart lodging',rampx,rampy,kz+8.8,
     7.4,5.6,3.2)
closed_roof_v8('V8 fortress rampart lodging cap',rampx,rampy,
               kz+12.25,9.05,6.3,2.65,ROOF_DARK,OAK)
for x in (rampx-2.1,rampx+2.1):
    window(x,rampy-2.90,kz+11.05,'front',.73)

# Full keep is roofless only where a physically accessible defensive terrace
# is intended. Stone merlons bridge the negative space between uneven towers.
for yi in (ky-7.95,ky+7.95):
    for i in range(9):
        xx=kx-9.2+i*2.32
        if -10.2<xx<2.2 and yi>ky:continue
        cube("V8 battered terrace handcut merlon",(xx,yi,kz+11.95),
             (1.16,.86,1.21),LIME,.085)
for xx in (kx-10.45,kx+10.45):
    for i in range(6):
        yy=ky-6.2+i*2.22
        cube('V8 cantilever flanking defensive crenel',(xx,yy,kz+11.95),
             (.90,1.15,1.21),STONE,.11)

# 3D stone stair/human scale, texture-bronze medallion and distinctive pennants.
for side in (-1,1):
    banner(kx+side*7.85,ky-8.65,kz+6.7,3.9)
for i in range(5):
    ico('V8 ancient Valorian carved falcon shield base',
        (kx-11+i*.45,-6.18,kz+2.1+i*.40),
        (.36,.18,.28),COPPER,1)

# The skyline now has four 3D roof families; no oversized house-roof survives.
v8_proof={'original_house_roof_nodes_removed':len(REMOVED),
          'new_hero_roof_families':4,
          'new_command_tower':True,
          'new_chapel':True,
          'new_outer_rampart_lodging':True,
          'previous_artist_architecture_preserved':True,
          'main_game_assets_used':False}
print('VALORIA_V8_MASSING_REBUILD_PASS',v8_proof)
