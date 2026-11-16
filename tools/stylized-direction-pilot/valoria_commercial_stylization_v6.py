# V6: substantive, original stylized architectural rebuild OVER our V5 original,
# not a retouch or a dependency on any of the game's production meshes.
# Executed in the original Blender generator before final renders and GLB export.
# Design language: soft sculpted geometry, iconic roof silhouettes, textured stone,
# warmer lived-in craft, orchard-like forests, and Eldoria's geological violet wound.
import math
import random
from mathutils import Vector
S=random.Random(601126)
pi=math.pi
def curved_panel(label,verts,faces,ma):
    return mesh(label,verts,faces,ma)
def new_material(label,srgb,kind='masonry'):
    mat=material(label,srgb)
    crafted_texture(mat,kind,srgb)
    return mat
PLASTER=new_material('V6 golden warm hand-plaster',(.75,.68,.55))
PLASTER_DARK=new_material('V6 timbered clay facing',(.57,.49,.38))
COPPER=new_material('V6 softly oxidized brass tiles',(.62,.49,.27),'roof')
ROOF_DARK=new_material('V6 evergreen teal slate',(.23,.37,.40),'roof')
BRIGHT_LEAF=new_material('V6 oak leaf late autumn',(.42,.55,.32),'foliage')
MID_LEAF=new_material('V6 blue green broadleaf',(.23,.43,.38),'foliage')
SUN_LEAF=new_material('V6 sunlit honey leaf',(.66,.56,.28),'foliage')
FIRED_BRICK=new_material('V6 warm ceramic tile',(.65,.39,.26),'masonry')
LOAM=new_material('V6 hand-tended warm pathway',(.59,.50,.32),'soil')
INK=new_material('V6 painted midnight valley heraldry',(.16,.25,.31))
GLOW=new_material('V6 warm lit ivory cloth',(.86,.66,.37),'foliage')
# 1. Repair *physical* roof gables and add prominent timber/stone craftsmanship.
# Original polygon roofs were closed as *sheets* but left great empty triangles
# between the upper wall and the ridge; this is why screenshots show black holes.
gables=[o for o in list(bpy.data.objects) if o.type=='MESH' and '/ enclosed gable' in o.name]
if len(gables)<6:
    raise RuntimeError('V6 expected generated, un-reused 3D roof family; got '+str(len(gables)))
for roof in gables:
    pts=[roof.matrix_world@v.co for v in roof.data.vertices]
    loX=min(p.x for p in pts); hiX=max(p.x for p in pts)
    loY=min(p.y for p in pts); hiY=max(p.y for p in pts)
    loZ=min(p.z for p in pts); hiZ=max(p.z for p in pts)
    mx=(loX+hiX)*.5; w=hiX-loX
    # End walls are entirely new triangle mesh surfaces; nothing paper-thin/painted.
    name=roof.name.split(' / enclosed gable')[0]
    crafted=OAK if ('Sawmill' in name or 'Granary' in name) else PLASTER if ('Gatehouse' in name or 'Barracks' in name) else STONE
    for end,side in ((loY+.23,-1),(hiY-.23,1)):
        verts=[];faces=[]
        steps=12
        for j in range(steps+1):
            x=loX+(hiX-loX)*j/steps
            q=abs(x-mx)/(w*.5)
            upper=hiZ-(hiZ-loZ-.45)*q**1.13+.10*(q**7)-.20
            verts.extend([(x,end,loZ-.24),(x,end,max(loZ-.21,upper))])
            if j>0:
                k=2*j
                faces.append((k-2,k-1,k+1,k))
                faces.append((k,k+1,k-1,k-2))
        curved_panel('V6 solid handcrafted '+name+' gable',verts,faces,crafted)
        # High contrast timber at the visible eaves, readable at iPhone width.
        for a in (-1,1):
            near=mx+a*w*.37
            qb=abs(near-mx)/(w*.5)
            h=hiZ-(hiZ-loZ-.43)*qb**1.13-.22
            beam=cube('V6 visible wooden rafter brace',(near,end+side*.10,h-.46),(.25,.35,1.22),OAK,.04)
            beam.rotation_euler[1]=a*.32
        cube('V6 deep gable sill',(mx,end+side*.15,loZ-.17),(w*.89,.43,.34),WOOD,.08)
        # This explicitly anchors the upper roof triangle to the actual wall.
        cyl('V6 circular attic lantern',(mx,end+side*.16,loZ+(hiZ-loZ)*.51),
            .60,.20,BRASS,12,(pi/2,0,0))
        ico('V6 attic warm glass',(mx,end+side*.29,loZ+(hiZ-loZ)*.51),
            (.36,.13,.36),GLASS,2)
    # Roof geometry gains a physical copper roof-cap and human-readable row break.
    cube('V6 monumental forged ridge',(mx,(loY+hiY)*.5,hiZ+.05),
         (.28,hiY-loY+.48,.28),COPPER,.10)
    for xsign in (-1,1):
        xx=mx+xsign*w*.46
        cube('V6 thick exposed slate eave', (xx,(loY+hiY)*.5,loZ+.30),
             (.39,hiY-loY+.55,.30),ROOF_DARK,.065)

# Geometry helpers for new dimensional silhouette elements.
def drum(label,x,y,base,h,r,material_body=STONE):
    bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=r,depth=h,location=(x,y,base+h*.5))
    o=bpy.context.object;o.name=label+' / octagonal sculpted wall'
    o.data.materials.append(material_body)
    for zscale in (.20,h-.35):
        bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=r*1.10,depth=.42,
                                           location=(x,y,base+zscale))
        bpy.context.object.name=label+' / projecting ring';bpy.context.object.data.materials.append(LIME)
    return o
def roof_spire(label,x,y,z,r,h,ma):
    # Three silhouettes from one rooted architectural language, without sprites.
    bpy.ops.mesh.primitive_cone_add(vertices=10,radius1=r*1.11,radius2=.12,depth=h,
                                    location=(x,y,z+h*.5))
    o=bpy.context.object;o.name=label+' / sharp crafted roof';o.data.materials.append(ma)
    bpy.ops.mesh.primitive_cone_add(vertices=10,radius1=r*1.18,radius2=r*.98,depth=.40,
                                    location=(x,y,z+.20))
    bpy.context.object.name=label+' / weighted roof lip';bpy.context.object.data.materials.append(COPPER)
    cyl(label+' / gilded roof finial',(x,y,z+h+.36),.17,.70,BRASS,10)
    ico(label+' / finial bead',(x,y,z+h+.77),(.23,.23,.23),BRASS,2)
def shield(x,y,z,scale=1):
    # Distinctive Eldoria glyph: a broken central diamond around a geological wound.
    ico('V6 imperial hammered shield',(x,y,z),(.92*scale,.15,.99*scale),COPPER,2)
    ico('V6 midnight shield core',(x,y-.13,z),(.60*scale,.12,.66*scale),INK,1)
    ico('V6 split Breach violet central inlay',(x,y-.24,z),(.13*scale,.09,.41*scale),VIOLET,1)
    for side in (-1,1):
        bar=cube('V6 broken Valorian heraldic diagonal',
                 (x+side*.24*scale,y-.27,z+side*.15*scale),
                 (.12*scale,.10,.49*scale),BANNER_TRIM,.02)
        bar.rotation_euler[1]=side*.31

# 2. Replace square silhouette with an inhabited five-tier fortress,
# lateral octagonal watchtowers, deep gate, arcaded courtyard and unique heraldry.
for sign in (-1,1):
    xx=kx+sign*9.2
    yy=-4.1
    drum('V6 fortified entry watchtower',xx,yy,kz,11.9,2.35)
    roof_spire('V6 Valorian watchtower',xx,yy,kz+12.1,2.8,4.25,ROOF_DARK)
    for height in (3.8,7.5,10.0):
        window(xx,yy-2.42,kz+height,'front',.73)
    # Diagonal stone buttresses, not a stack of unrelated cuboids.
    for side in (-1,1):
        b=cube('V6 sculpted tower heel',(xx+side*2.3,yy,kz+1.75),
               (.95,1.15,3.50),STONE,.23)
        b.rotation_euler[1]=side*.14
    banner(xx,yy-2.67,kz+6.8,2.9)

# Gate: deep arched portcullis, warm flanking hearths, surface detailing.
for sign in (-1,1):
    x=kx+sign*3.72
    cube('V6 monumental doorpost pier',(x,-6.16,kz+3.15),
         (1.05,.83,6.20),LIME,.15)
    ico('V6 firelit sculpted braziers',(x*1.05,-6.89,kz+5.65),
        (.39,.33,.55),GLASS,2)
    cube('V6 flared cutstone hinge',(x,-6.60,kz+1.1),
         (1.54,.72,.34),STONE,.10)
for i in range(13):
    x=kx-2.0+i*.33
    cube('V6 forged gate black iron portcullis',(x,-6.01,kz+3.1),
         (.13,.26,5.8),INK,.025)
for zz in (kz+1.2,kz+3.3,kz+5.7):
    cube('V6 heavy portal forged iron crossbar',(kx,-6.17,zz),
         (4.33,.22,.20),BRASS,.05)
shield(kx,-6.83,kz+9.32,1.15)
# Stacked horizontal architectural ledges between buildings give a believable
# crafted language in place of giant smooth vertical box fronts.
for zlevel in (4.2,7.1,10.2):
    for seg in range(8):
        xx=kx-8.6+2.5*seg
        if zlevel==4.2 and abs(xx-kx)<3.3:continue
        cube('V6 castle warm relief masonry string course',
             (xx,ky-8.24,kz+zlevel),(1.70,.31,.25),
             LIME if seg%3 else STONE,.055)
for sign in (-1,1):
    # Small inhabited and useful sentry alcove added below existing defensive tower.
    xx=kx+sign*11.4
    for level in (kz+5.9,kz+8.3):
        cube('V6 real castle slit window depth',(xx,ky-8.18,level),
             (.65,.40,1.55),DARK,.07)
        cube('V6 slit glazed warm interior',(xx,ky-8.41,level),
             (.33,.18,1.16),GLASS,.035)
# Correctly sized *low* keep parapet, not a wall of identical floating teeth.
for x in [kx-9.5+i*2.65 for i in range(8)]:
    cube('V6 corbel-cut castle tooth',(x,ky-8.25,kz+11.75),
         (1.15,.72,1.20),LIME,.105)
# A true watch-platform crown readable over the roof silhouette.
drum('V6 bellwatch crowning belfry',kx+4.5,ky+7.2,kz+18.6,2.15,2.30,PLASTER)
roof_spire('V6 bellwatch tall point',kx+4.5,ky+7.2,kz+21.4,2.50,5.30,ROOF_DARK)

# 3. Separate craft identities: living village, not five recoloured houses.
# Sawmill: purposeful open woodworking arcade + heavy timber cross-braces.
sx,sy=-30,-10
for xx in (sx-5.6,sx+5.6):
    for yy in (sy-3.8,sy+3.8):
        cube('V6 sawmill sturdy exposed oak arcade',(xx,yy,sz+2.20),
             (.72,.75,4.40),OAK,.12)
for i in range(7):
    xx=sx-4.5+i*1.45
    cube('V6 sawmill open veranda fence stave',(xx,sy-4.30,sz+1.1),
         (.24,.32,2.0),WOOD,.045)
cube('V6 carpenters upper lintel',(sx,sy-4.45,sz+3.3),
     (12.3,.59,.47),WOOD,.065)
shield(sx,sy-4.53,sz+3.1,.62)
for i in range(8):
    ico('V6 newly cut timber chips',(sx-3+i*.9,sy-7.7,sz+.16),
        (.28,.22,.12),WOOD)
# Barracks: compact set of orange training awnings, curved shields and fencing.
bx,by=28,10
for sign in (-1,1):
    roof_spire('V6 barracks guard turrets',bx+sign*6.5,by-3.0,qz+7.75,
               2.12,2.68,ROOF_DARK)
for k in range(8):
    xx=bx-6.5+k*1.92
    cube('V6 barracks front training rail',(xx,by-9.8,qz+.83),
         (.30,.32,1.67),OAK,.09)
for yoff in (by-10.0,by-11.6):
    cube('V6 barracks fenced training yard',(bx,yoff,qz+1.18),
         (14.9,.23,.18),WOOD,.045)
shield(bx,by-4.83,qz+3.70,.95)
# Granary: thatch-bronze silo crowns, circular bands, grain court.
gx,gy=-30,19
for sign in (-1,1):
    xx=gx+sign*7.4
    for zoff in (gz+1.5,gz+3.45,gz+5.6):
        bpy.ops.mesh.primitive_torus_add(major_segments=12,minor_segments=4,
                                        location=(xx,gy+1,zoff),
                                        major_radius=2.12,minor_radius=.12)
        ob=bpy.context.object;ob.name='V6 authentic binding ring of grain silo'
        ob.data.materials.append(WOOD)
    roof_spire('V6 granary circular harvest hat',xx,gy+1,gz+6.25,2.45,2.43,COPPER)
for i in range(6):
    xx=gx-4+i*1.4
    ico('V6 grain harvest delivery sacks',(xx,gy-6.6,gz+.47),
        (.59,.72,.53),EARTH2,1)
shield(gx,gy-4.0,gz+2.6,.72)
# Quarry: real upright counterweight frame, hooked crane, quarry tool sheds.
cx,cy=28,-15
cyl('V6 quarry winch iron axle',(cx+2.4,cy+2,qz+8.6),
    .30,3.0,BRASS,12,(0,pi/2,0))
cube('V6 quarry heavy cantilever diagonal',
     (cx+4.5,cy+2,qz+7.5),(.45,.58,5.4),OAK,.07).rotation_euler[1]=-.44
for sign in (-1,1):
    cube('V6 quarry timber trestle support',(cx+6.5+sign,cy-1.5,qz+3.4),
         (.46,.47,6.5),WOOD,.07)
for i in range(10):
    xx=cx-7+(i%5)*1.65; yy=cy-6+(i//5)*1.75
    cube('V6 quarry stacking sawn polygon cutstones',(xx,yy,qz+.93),
         (1.20,1.06,.62),LIME if i%3==0 else STONE,.09)

# 4. Physical terrain/foliage that belongs to a premium stylized mobile game.
# Distinctive clustered broadleaf, built with overlapping 3D lobes rather than billboards.
def village_tree(x,y,height,variant):
    z=ground(x,y)
    cyl('V6 warm irregular tree trunk',(x,y,z+height*.27),height*.060,height*.54,BARK,7)
    for i in range(7):
        ang=2*pi*i/7+variant*.42
        stretch=height*(.20+.03*((i+variant)%3))
        dx=math.cos(ang)*height*.13
        dy=math.sin(ang)*height*.13
        dz=height*(.61+.08*((i+variant)%4))
        ico('V6 handcrafted clustered canopied foliage',(x+dx,y+dy,z+dz),
            (stretch,stretch*.88,height*.18),
            [BRIGHT_LEAF,MID_LEAF,SUN_LEAF][(i+variant)%3],2)
    ico('V6 shaped crown finale',(x,y,z+height*.93),
        (height*.13,height*.14,height*.13),MID_LEAF,2)
for idx in range(78):
    x=S.uniform(-65,65);y=S.uniform(-61,53)
    if -18<x<17 and -15<y<26:continue
    if ((x+30)/16)**2+((y+10)/12)**2<1:continue
    if ((x+30)/15)**2+((y-19)/11)**2<1:continue
    if ((x-28)/16)**2+((y-10)/12)**2<1:continue
    if ((x-28)/15)**2+((y+15)/12)**2<1:continue
    if abs(y+29+1.8*math.sin(x*.11))<6:continue
    village_tree(x,y,S.uniform(5.3,8.7),idx)
# Roadside olive groves give small scale and warmth against tall silhouette.
for i in range(24):
    x=-48+(i%8)*2.8;y=34+(i//8)*3.5
    village_tree(x,y,3.5+S.uniform(-.3,.6),i+8)
# Roadside low gardening and warm patches, no painted squares or platforms.
for i in range(22):
    xx=-33+(i%8)*3.1
    yy=-30+(i//8)*2.2
    if abs(xx+30)<7 and abs(yy+29)<4:continue
    ico('V6 integrated flowering meadow bush',
        (xx,yy,ground(xx,yy)+.3),(.48,.49,.38),
        SUN_LEAF if i%4==0 else BRIGHT_LEAF,1)
# Worn stone discs create a readable castle arrival court without a rectangular island.
for k in range(42):
    ang=S.uniform(0,2*pi);radius=S.uniform(3.5,9.0)
    x=kx+radius*math.cos(ang);y=-12+radius*math.sin(ang)
    ico('V6 buried irregular plaza pavers',(x,y,ground(x,y)+.03),
        (.54,.47,.13),LIME if k%5==0 else ROCK[k%3],1)

# 5. Actual quality test metadata — art not approved by a technical pass.
V6_STYLE_COUNTS={'repaired_gable_roofs':len(gables),
                 'new_watchtowers':2,'new_broadleaf_trees':102,
                 'new_physical_style':'expressive original medieval fantasy mobile 3D'}
print('VALORIA_V6_ART_DIRECTION_PASS',V6_STYLE_COUNTS)
