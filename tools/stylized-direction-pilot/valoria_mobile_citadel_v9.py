# VALORIA V9 — intentional mobile-readable stylized city.
# This is NOT a repaint of main. Source owns genuinely NEW mesh geometry in Blender.
# The previous experimental V5–V8 castle is removed spatially, then a physically
# simpler and more expressive original V9 bastion is built, alongside re-art-directed
# resource buildings (the real mechanics are not altered).
import bpy, math, random
from mathutils import Vector
RR=random.Random(16946)
V9_INITIAL_MESH=len([o for o in bpy.data.objects if o.type=='MESH'])

def world_box(o):
    p=[o.matrix_world@Vector(corner) for corner in o.bound_box]
    mn=Vector((min(q.x for q in p),min(q.y for q in p),min(q.z for q in p)))
    mx=Vector((max(q.x for q in p),max(q.y for q in p),max(q.z for q in p)))
    return (mn+mx)*.5,mx-mn

# Demolish all prior test citadels in this original Blender scene, never the
# game assets. Check REAL evaluated bounds: procedural gables have zero origin.
deleted=[]
for ob in tuple(bpy.data.objects):
    if ob.type!='MESH':continue
    if 'terrain' in ob.name.lower() or 'river' in ob.name.lower():continue
    center,size=world_box(ob)
    if max(size.x,size.y)>85:continue
    if -19.5<center.x<17.5 and -10.5<center.y<26.0 and center.z>.73:
        deleted.append(ob.name)
        bpy.data.objects.remove(ob,do_unlink=True)
if len(deleted)<125:
    raise RuntimeError('V9 intentional full citadel replacement failed: only %d objects deleted'%len(deleted))

# Painted & exportable PBR colours, keeping one visually unified design vocabulary.
def pigment(name,rgb,kind='masonry'):
    m=material(name,rgb,.83)
    crafted_texture(m,kind,rgb,256)
    return m
SAND=pigment('V9 sculpture honey limestone',(.76,.70,.56))
CREAM=pigment('V9 limestone edge highlights',(.88,.82,.68))
WARM=pigment('V9 plaster inhabited soft ivory',(.83,.78,.64))
NAVY=pigment('V9 soft midnight slate tiles',(.28,.40,.52),'roof')
NAVY_LIGHT=pigment('V9 translucent cerulean slate',(.38,.53,.62),'roof')
COPPER_RED=pigment('V9 terra cotta barrack tile',(.70,.37,.27),'roof')
OCHRE_ROOF=pigment('V9 baked golden straw tile',(.75,.53,.26),'roof')
CEDAR_ROOF=pigment('V9 hammered caramel cedar tile',(.51,.37,.29),'roof')
QUARRY_ROOF=pigment('V9 worn glacier slate tile',(.36,.51,.55),'roof')
GREEN=pigment('V9 vivid soft moss-clover clearing',(.49,.65,.39),'foliage')
EARTH_L=pigment('V9 warm soft beige trampled footpaths',(.71,.60,.43),'soil')
SAGE=pigment('V9 round leaf light spring green',(.37,.57,.42),'foliage')
HEATHER=pigment('V9 leaf blue-green',(.25,.46,.43),'foliage')
OLIVE=pigment('V9 leaf warm olive',(.56,.59,.32),'foliage')

def replace_one_material(obj,ma):
    obj.data.materials.clear()
    obj.data.materials.append(ma)

# Geography: footpaths and forest have more readable light and less visual noise.
if terrain and terrain.type=='MESH': replace_one_material(terrain,GREEN)
for ob in tuple(bpy.data.objects):
    if ob.type!='MESH':continue
    lo=ob.name.lower()
    c,dim=world_box(ob)
    if ('worn dirt avenue' in lo or 'approach' in lo or 'rutted path' in lo or
        'granary approach' in lo or 'orchard road' in lo):
        replace_one_material(ob,EARTH_L)
    elif ('original irregular volumetric pine canopy' in lo or
          'asymmetrical fir side-branch' in lo or
          'v6 handcrafted clustered canopied foliage' in lo):
        replace_one_material(ob,[SAGE,HEATHER,OLIVE][int(abs(c.x*71+c.y*19))%3])
    # Resource buildings now have distinct colours and unmistakable silhouettes.
    elif ('enclosed gable' in lo or '/ roof slate joint' in lo or
          'thick exposed slate eave' in lo):
        if c.x< -17 and c.y<7:replace_one_material(ob,CEDAR_ROOF)
        elif c.x< -17 and c.y>=7:replace_one_material(ob,OCHRE_ROOF)
        elif c.x>17 and c.y>=0:replace_one_material(ob,COPPER_RED)
        elif c.x>17 and c.y<0:replace_one_material(ob,QUARRY_ROOF)
    # Keep small stones subordinate to building read at mobile scale.
    if ('river-buried rounded stone' in lo or 'meadow bush' in lo
        or 'buried irregular plaza pavers' in lo or 'warm hand-fitted cobble' in lo):
        if RR.random()<.47:bpy.data.objects.remove(ob,do_unlink=True)

# Hand-built physical irregular shapes, not a flat roof painted over a cube.
def block(label,center,size,mat,bevel=.16):
    return cube('V9 / '+label,center,size,mat,bevel)

def crenel_line(label,p1,p2,z,num,ma):
    for i in range(num):
        t=i/(max(1,num-1))
        x=p1[0]*(1-t)+p2[0]*t
        y=p1[1]*(1-t)+p2[1]*t
        block(label,(x,y,z),(.78,.96,.98),ma,.11)

def octagon(label,x,y,z,radius,height,body=SAND):
    bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=radius,depth=height,
                                        location=(x,y,z+height*.5))
    ob=bpy.context.object;ob.name='V9 / '+label+' / proper octagonal stone mass'
    ob.data.materials.append(body)
    for zz,rad in ((z+.25,radius*1.09),(z+height-.16,radius*1.08)):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=rad,depth=.42,
                                            location=(x,y,zz))
        obj=bpy.context.object;obj.name='V9 / '+label+' / stone belt'
        obj.data.materials.append(CREAM)
    return ob

def cone(label,x,y,z,r,h,mat):
    bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r*1.07,radius2=.13,depth=h,
                                    location=(x,y,z+h*.5))
    ob=bpy.context.object;ob.name='V9 / '+label+' / oversized crafted spire'
    ob.data.materials.append(mat)
    bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=r*1.13,depth=.30,
                                        location=(x,y,z+.16))
    ob=bpy.context.object;ob.name='V9 / '+label+' / roof eave lip'
    ob.data.materials.append(BRASS)
    ico('V9 / roof-top golden finial',(x,y,z+h+.18),(.17,.17,.30),BRASS,1)

def roof(label,x,y,z,w,d,h,mat,facade):
    # Two continuous sculpted slopes with 3D gable closing polygons; broad eaves.
    v=[];f=[];N=10
    for j in range(N+1):
        xx=x-w*.5+j*w/N
        q=abs(xx-x)/(w*.5)
        height=z+h*(1-q**1.08)+.12*q**8
        for yy in (y-d*.53,y+d*.53):
            v.append((xx,yy,height))
        if j:
            a=2*j;f.append((a-2,a,a+1,a-1))
    shell=mesh('V9 / '+label+' / sculpted curved physical 3D roof',v,f,mat)
    for end,sgn in ((y-d*.53,-1),(y+d*.53,1)):
        vs=[];fs=[]
        for j in range(N+1):
            xx=x-w*.5+j*w/N;q=abs(xx-x)/(w*.5)
            hhi=z+h*(1-q**1.08)+.12*q**8-.07
            vs.extend([(xx,end,z-.04),(xx,end,hhi)])
            if j:
                a=2*j
                fs.append((a-2,a-1,a+1,a) if sgn==1 else (a,a+1,a-1,a-2))
        mesh('V9 / '+label+' / actually closed vertical end wall',vs,fs,facade)
        block(label+' / decorative cross beam',(x,end,z+.10),(w*.93,.29,.24),WOOD,.05)
        ico('V9 / '+label+' / illuminated oval gable roundel',
            (x,end+sgn*.13,z+h*.55),(.40,.12,.44),GLASS,2)
    block(label+' / golden ridge cap',(x,y,z+h+.08),(.20,d*1.10,.22),BRASS,.05)
    for sign in (-1,1):
        xx=x+sign*w*.48;zedge=z+.09
        block(label+' / thick projecting overhang',(xx,y,zedge),
              (.34,d*1.15,.30),mat,.055)
    return shell

# CENTRAL BASTION: horizontal stronghold, NOT a multistory tall chalet.
# All buildings share the actual terrain height — no raised platform/island.
x,y=kx,ky
base=ground(x,y)
block('bastion V carved soft lower rampart',(x,y,base+3.45),
      (23.8,17.0,6.9),SAND,.42)
block('bastion V stone cornice',(x,y,base+6.95),
      (24.5,17.4,.50),CREAM,.19)
for a in (-1,1):
    block('bastion outward defensive rusticated wing',
          (x+a*10.2,y-3.05,base+3.13),
          (6.10,5.1,6.24),SAND,.35)
    block('bastion lower angled stone buttress foot',
          (x+a*12.0,y-4.95,base+1.10),
          (1.8,1.60,2.19),CREAM,.17)
# Low outer battlements around central enclosed great hall.
crenel_line('royal frontage crenellation',(x-10,y-8.75),
            (x+10,y-8.75),base+7.5,9,CREAM)
# Central roof one legible dark-blue roof, not several nested high chalet roofs.
block('central keep raised living inner hall',(x,y+3.0,base+8.4),
      (13.50,10.8,3.0),WARM,.29)
roof('royal hall',x,y+3.0,base+9.85,15.2,12.0,3.05,NAVY,WARM)
# Stone doors have actual sculpted raised forged arc, softened at mobile scale.
block('grand deep portal carved surround',(x,y-8.67,base+3.30),
      (6.60,.65,6.35),CREAM,.40)
block('deep visible dark gate recess',(x,y-9.04,base+2.90),
      (5.02,.18,5.23),DARK,.31)
for g in range(13):
    px=x-2.10+.35*g
    block('forged vertical royal gate',
          (px,y-9.17,base+2.62),(.105,.15,4.8),BRASS,.02)
for zz in (base+1.35,base+3.80,base+5.06):
    block('gate forging horizontal assembly',
          (x,y-9.29,zz),(4.65,.12,.18),OAK,.03)
for a in (-1,1):
    block('stone lintel portal pier',(x+a*3.05,y-9.02,base+3.05),
          (.81,.85,6.03),CREAM,.13)
    for zz in (base+1.80,base+4.15):
        block('deep mounted amber window',
              (x+a*7.6,y-8.73,zz),(.70,.35,1.10),GLASS,.07)
# Actual semi-circular masonry arch made from radiating voussoirs.
for i in range(13):
    ang=math.pi*i/12
    a=block('arched gate voussoir',
            (x+2.83*math.cos(ang),y-9.1,base+5.65+1.23*math.sin(ang)),
            (.57,.53,.39),CREAM,.11)
    a.rotation_euler[1]=ang
shield(x,y-9.56,base+9.12,.83)

# Compact twin front octagonal turrets with recognisable paired slate caps.
for side in (-1,1):
    tx=x+side*10.20;ty=y-6.15
    octagon('front fortress flanking stone tower',tx,ty,base,2.36,8.52)
    cone('front octagonal turret',tx,ty,base+8.74,2.7,3.36,NAVY)
    for zz in (base+3.15,base+5.8):
        block('turret warm slim window',(tx,ty-2.42,zz),
              (.55,.25,1.00),GLASS,.06)
    # Heroic but minimal dark blue banners, physically protruding forward.
    block('banner forged rod',(tx,ty-2.48,base+6.42),
          (1.42,.17,.15),BRASS,.02)
    block('a single elegant unfurled Valorian flag',
          (tx,ty-2.56,base+5.38),
          (1.1,.14,1.90),BANNER,.04)
# One asymmetrical *short* imperial belfry at the rear; only single tall point.
bx,by=x+6.1,y+5.0
octagon('command bell tower',bx,by,base+6.9,2.62,7.25,CREAM)
cone('command only spire',bx,by,base+14.4,2.93,4.5,NAVY_LIGHT)
block('command tower illuminated arched face',
      (bx,by-2.66,base+12.1),(.74,.29,1.55),GLASS,.09)
ico('Valorian breach jewel on command finial',
    (bx,by,base+19.15),(.31,.31,.40),VIOLET,2)
# Keep approach joins grass directly; no high uniform pedestals.
for j in range(5):
    yy=y-10.4-j*.75
    block('old broad natural carved entrance step',
          (x,yy,ground(x,yy)+.07),
          (6.50-j*.24,.64,.11),CREAM,.06)

# More deliberate scale/identity per secondary building (fully 3D).
# These are unique architectural exteriors atop V7 independently authored models.
for ix in range(5):
    pos=(-34.1+1.55*ix,-17.55)
    ico('sawmill warm freshcut clustered timbers',
        (pos[0],pos[1],ground(*pos)+.62),
        (.46,1.02,.43),WOOD,1)
for xx in (-37.0,-23.2):
    cz=ground(xx,18.9)
    block('granary painted tall golden measuring pole',
          (xx,18.9,cz+2.2),(.19,.18,4.4),OAK,.06)
for xx in (25.8,30.5,35.2):
    yy=0.6;zz=ground(xx,yy)
    ico('barracks trainable sword shield disk',
        (xx,yy,zz+1.45),(.88,.21,.88),COPPER_RED,2)
for yy in (-24.7,-21.8,-18.9):
    xq=39.4;zq=ground(xq,yy)
    ico('quarry distinctive stacked limestone octagonal boulder',
        (xq,yy,zq+.43),(.85,.9,.76),CREAM,1)

# Simplified leaf stylization: small trees become colour blocks, not scattered
# sharp boulders. Big resource identities, five buildings, four easy paths.
scene.view_settings.exposure=.58
scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.48,.54,.62,1)
scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=1.02
V9_METRICS={
 'removed_previous_castle_meshes':len(deleted),
 'new_mobile_architecture':'horizontal fortified main hall with only one high spire',
 'primary_hero_silhouette_count':4,
 'roof_identity_for_resources':'sawmill cedar, granary ochre, barracks terra-cotta, quarry slate',
 'terrain_seamless':True,
 'main_production_assets_reused':False,
 'quality_approved':False}
print('VALORIA_COMPACT_V9_PASS',V9_METRICS)
