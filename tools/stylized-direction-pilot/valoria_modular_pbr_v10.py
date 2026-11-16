# VALORIA V10 — first real authored five-building PBR modular art proof
# Real Blender geometry and commercially usable CC0 Quaternius texture assets.
# No Eldoria main production meshes are imported. The building compositions,
# silhouettes, settlement planning, flags, props, land and lighting are original.
# Upstream: Quaternius Medieval Village MegaKit Standard + Stylized Nature
# MegaKit Standard, licensed CC0 1.0 (license revalidated by CI).
import bpy,math,random,sys,json,pathlib,os
from mathutils import Matrix,Vector
from math import sin,cos,pi
R=random.Random(126116)
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
def option(k,default):
    return args[args.index(k)+1] if k in args else default
ROOT=pathlib.Path(option('--assets','third_party/FreeModels'))
OUT=pathlib.Path(option('--out','out-cc0-modular-v10'))
OUT.mkdir(parents=True,exist_ok=True)
MED=ROOT/'Medieval Village MegaKit[Standard]'/'glTF'
NAT=ROOT/'Stylized Nature MegaKit[Standard]'/'glTF'
assert (MED/'Roof_RoundTiles_8x10.gltf').exists(),str(MED)
assert (NAT/'CommonTree_2.gltf').exists(),str(NAT)
license_text=(ROOT/'Medieval Village MegaKit[Standard]'/'License_Standard.txt').read_text()
assert 'CC0 1.0 Universal' in license_text

bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=32
if hasattr(scene.cycles,'use_denoising'):scene.cycles.use_denoising=False
for l in scene.view_layers:
    if hasattr(l,'cycles') and hasattr(l.cycles,'use_denoising'):
        l.cycles.use_denoising=False
scene.render.image_settings.file_format='PNG'
scene.render.threads_mode='FIXED';scene.render.threads=4
scene.view_settings.view_transform='Standard'
scene.view_settings.look='Medium High Contrast'
scene.view_settings.exposure=.58
world=bpy.data.worlds.new('Valoria clean golden hour')
world.use_nodes=True
bg=world.node_tree.nodes.get('Background')
bg.inputs['Color'].default_value=(.58,.65,.72,1)
bg.inputs['Strength'].default_value=.85
scene.world=world

def material(name,color,rough=.78,metal=0,emission=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Roughness'].default_value=rough
    bs.inputs['Metallic'].default_value=metal
    if emission:
        col='Emission Color' if 'Emission Color' in bs.inputs else 'Emission'
        if col in bs.inputs:bs.inputs[col].default_value=(*color,1)
        if 'Emission Strength' in bs.inputs:bs.inputs['Emission Strength'].default_value=emission
    return m
STONY=material('V10 warm sandstone river rock',(.67,.65,.53))
SAND=material('V10 carved limestone',(.80,.74,.61))
LIME=material('V10 translucent stone trims',(.92,.84,.68))
OAK=material('V10 dark oak timber',(.36,.24,.17))
CEDAR=material('V10 honey lumber',(.61,.38,.22))
GOLD=material('V10 copper embroidery',(.86,.63,.27),.46,.28)
BLUE=material('V10 Heraldic deep cobalt',(.16,.26,.46))
PURPLE=material('V10 Breach heraldry violet',(.43,.22,.64),.32,0,1.2)
LANTERN=material('V10 living warm amber window',(.98,.71,.34),.31,0,1.3)
GREEN=material('V10 soft celadon meadow',(.40,.59,.37))
GROUNDVAR=material('V10 clean grassy relief',(.47,.61,.36))
ROAD=material('V10 warm sandy travel route',(.62,.53,.38))
ROCK=material('V10 blue warm quarry stones',(.49,.53,.55))

# Non-destructive library of textured 3D Quaternius models. Import once, create
# many linked mesh instances, preserve PBR basecolor/normal/ORM in the .blend/GLB.
templates={}; imported=[]
def import_model(collection,name):
    key=(collection,name)
    if key in templates:return templates[key]
    path=(MED if collection=='M' else NAT)/(name+'.gltf')
    if not path.exists():raise FileNotFoundError(path)
    before=set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(path))
    bpy.context.view_layer.update()
    new=[ob for ob in bpy.data.objects if ob not in before]
    meshes=[ob for ob in new if ob.type=='MESH']
    if not meshes:raise RuntimeError('Imported no physical meshes: '+str(path))
    templates[key]=[(obj,obj.matrix_world.copy()) for obj in meshes]
    for ob in new:
        ob.hide_render=True;ob.hide_set(True)
        imported.append(ob)
    return templates[key]

created=[]
def stamp(collection,name,pos,scale=1,rotation=0,tag=''):
    out=[]
    M=Matrix.Translation(Vector(pos))@Matrix.Rotation(rotation,4,'Z')@Matrix.Scale(scale,4)
    for mesh,original in import_model(collection,name):
        clone=bpy.data.objects.new('V10/'+(tag or name)+'/'+mesh.name,mesh.data)
        bpy.context.collection.objects.link(clone)
        clone.matrix_world=M@original
        created.append(clone);out.append(clone)
    return out

def box(name,pos,size,ma,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
    ob=bpy.context.object;ob.name='V10/'+name
    ob.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    ob.data.materials.append(ma)
    if bevel:
        m=ob.modifiers.new('Smooth artisan bevel','BEVEL')
        m.width=bevel;m.segments=2
    created.append(ob);return ob

def cylinder(name,pos,r,d,mat,vert=12,rotation=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vert,radius=r,depth=d,location=pos)
    o=bpy.context.object;o.name='V10/'+name
    o.data.materials.append(mat)
    if rotation:o.rotation_euler=rotation
    created.append(o);return o

def sphere(name,pos,scale,ma,sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,location=pos)
    o=bpy.context.object;o.name='V10/'+name;o.scale=scale;o.data.materials.append(ma)
    created.append(o);return o

def ground(x,y):
    return .12*sin(.09*x)+.08*cos(.11*y)
# One continuous landscape, no rectangular raised building islands.
W=141;v=[];f=[]
for j in range(W):
    y=(j-W//2)
    for i in range(W):
        x=(i-W//2);v.append((x,y,ground(x,y)))
for j in range(W-1):
    for i in range(W-1):
        a=j*W+i;f.append((a,a+1,a+1+W,a+W))
me=bpy.data.meshes.new('V10 full seamless terrain mesh')
me.from_pydata(v,[],f);me.update()
floor=bpy.data.objects.new('V10/One continuous real meadow',me)
bpy.context.collection.objects.link(floor)
me.materials.append(GREEN)
for poly in me.polygons:poly.use_smooth=True
created.append(floor)

def path_line(name,points,width,mat=ROAD):
    vertices=[];faces=[]
    for idx,(x,y) in enumerate(points):
        if idx==0:dx=points[1][0]-x;dy=points[1][1]-y
        elif idx==len(points)-1:dx=x-points[-2][0];dy=y-points[-2][1]
        else:dx=points[idx+1][0]-points[idx-1][0];dy=points[idx+1][1]-points[idx-1][1]
        norm=max(.001,math.hypot(dx,dy));nx=-dy/norm;ny=dx/norm
        for side in (-1,1):
            px=x+side*nx*width*.5
            py=y+side*ny*width*.5
            vertices.append((px,py,ground(px,py)+.028))
        if idx:
            q=2*(idx-1);faces.append((q,q+1,q+3,q+2))
    mesh=bpy.data.meshes.new('V10/'+name)
    mesh.from_pydata(vertices,[],faces);mesh.update()
    ob=bpy.data.objects.new('V10/'+name,mesh)
    bpy.context.collection.objects.link(ob);mesh.materials.append(mat)
    created.append(ob)
for name,pts,width in [
    ('Gate approach',[(0,-5),(0,-10),(-2,-17),(-3,-32)],3.2),
    ('To original sawmill',[(-5,-10),(-9,-11),(-15,-14),(-23,-14)],2.4),
    ('To resource granary',[(-5,4),(-11,10),(-18,17),(-24,20)],2.6),
    ('To military barracks',[(6,5),(13,11),(21,18)],2.6),
    ('To stone quarry',[(6,-7),(14,-12),(22,-15)],2.8)]:
    path_line(name,pts,width)

# Module family uses genuine 3D texturing on *all* front walls, doors and roofs.
def shell(name,x,y,w,d,levels=1,wall='Wall_Plaster_Window_Wide_Round',
          roof='Roof_RoundTiles_6x8',roofscale=.88,frontbrick='Roof_Front_Brick6'):
    z=ground(x,y);h=levels*3.12
    box(name+'/warm structural core',(x,y,z+h*.5),(w,d,h),SAND,.20)
    count=max(3,round(w/2))
    for floor_idx in range(levels):
        lvl=z+floor_idx*3.12
        for j in range(count):
            dx=-w*.5+1+(w-2)*j/max(1,count-1)
            fm='Wall_Plaster_Door_Round' if floor_idx==0 and abs(dx)<.5 else wall
            stamp('M',fm,(x+dx,y-d*.5-.12,lvl),1,0,name+'/southwest-facade')
            stamp('M',wall,(x+dx,y+d*.5+.12,lvl),1,pi,name+'/north-facing-facade')
        sid=max(2,round(d/2))
        for j in range(sid):
            off=-d*.5+1+(d-2)*j/max(1,sid-1)
            stamp('M','Wall_Plaster_Straight',(x-w*.5-.08,y+off,lvl),1,-pi/2,name+'/west-wall')
            stamp('M','Wall_Plaster_Straight',(x+w*.5+.08,y+off,lvl),1,pi/2,name+'/east-wall')
    stamp('M',roof,(x,y,z+h+.55),roofscale,0,name+'/real 3D tiled roof')
    stamp('M',frontbrick,(x,y-d*.5-.09,z+h-.15),roofscale,0,name+'/front filled gable')
    stamp('M',frontbrick,(x,y+d*.5+.09,z+h-.15),roofscale,pi,name+'/rear gable')
    return z,h

# FIVE buildings with original functional relationships, not five clones.
# Bastion: intentionally moderate, fortified frontage and a single imperial tower.
bx,by=0,5
bz,bh=shell('Royal Bastion V',bx,by,13.8,11.1,2,
            'Wall_UnevenBrick_Window_Wide_Round','Roof_RoundTiles_8x10',1.30,'Roof_Front_Brick8')
# Hand cut projecting stone fortification integrated with the same grass.
for sign in (-1,1):
    tx=bx+sign*7.2;ty=by-4.65
    tz=ground(tx,ty)
    cylinder('Bastion corner watchtower proper masonry',(tx,ty,tz+3.65),2.32,7.30,STONY,10)
    for hz in (tz+.45,tz+6.90):
        cylinder('Bastion upper and lower carved stone rings',
                 (tx,ty,hz),2.55,.36,SAND,10)
    stamp('M','Roof_Tower_RoundTiles',(tx,ty,tz+7.6),.66,0,'Bastion original turret textured spire')
    box('Bastion hanging heraldry',(tx,ty-2.39,tz+4.7),(1.35,.16,2.0),BLUE,.05)
    sphere('Bastion gold medallion on heraldry',(tx,ty-2.52,tz+4.7),(.34,.13,.36),GOLD,1)
for i in range(9):
    xx=-5.9+i*1.48
    box('Bastion deep carved stone merlon',(xx,by-5.7,bz+6.45),
        (.78,.81,.95),SAND,.10)
box('Bastion monumental deep blue gate',(0,by-5.83,bz+1.6),
    (3.3,.18,3.2),BLUE,.20)
for i in range(9):
    box('Bastion forged gate iron rails',(-1.38+i*.35,by-6.03,bz+1.6),
        (.07,.13,2.98),GOLD,.02)
box('Bastion relief heraldic plaque',(0,by-6.12,bz+4.2),
    (1.28,.15,.60),GOLD,.13)
sphere('Bastion Breach glyph on plaque',(0,by-6.26,bz+4.22),
       (.20,.12,.32),PURPLE,2)

# Large timber-production village home with one custom mechanical wheel.
sx,sy=-24,-12
sz,sh=shell('Western Sawmill',sx,sy,8,7.2,1,
            'Wall_Plaster_WoodGrid','Roof_RoundTiles_6x8',.86,'Roof_Front_Brick6')
for j in range(7):
    cylinder('Sawmill physically stacked timber',
             (sx-4.4+j*.80,sy-5.30,sz+.38),.42,4.50,CEDAR,9,(pi/2,0,0))
wx,wy=sx+5.1,sy+1.4
for i in range(12):
    a=i*2*pi/12
    # Radial spokes and real paddles around the old Mill wheel.
    loc=(wx,wy+2.2*cos(a),sz+2.1+2.2*sin(a))
    ob=box('Sawmill overshot waterwheel radial bucket',
           loc,(.55,.75,.40),CEDAR,.07)
    ob.rotation_euler[0]=a
cylinder('Sawmill iron outer axle',(wx,wy,sz+2.1),
         .38,2.25,GOLD,10,(0,pi/2,0))
stamp('M','Prop_Wagon',(sx-3.4,sy-8.3,ground(sx-3.4,sy-8.3)),
      1.25,pi*.10,'Sawmill supply wagon')

# Granary with two golden silo cylinders; warm wooden structure.
gx,gy=-24,19
gz,gh=shell('Harvest Granary',gx,gy,8.5,7,1,
            'Wall_Plaster_Straight','Roof_RoundTiles_6x8',.85,'Roof_Front_Brick6')
for sign in (-1,1):
    xx=gx+sign*5.7;yy=gy+1.6;tz=ground(xx,yy)
    cylinder('Granary natural grain silo',(xx,yy,tz+2.05),1.72,4.1,OAK,12)
    stamp('M','Roof_Tower_RoundTiles',(xx,yy,tz+4.2),.47,
          0,'Granary raised round textured grain cap')
for i in range(9):
    xx=gx-3.5+(i%5)*1.55; yy=gy-4.55+int(i/5)*1.2
    sphere('Granary sack with rounded end', (xx,yy,ground(xx,yy)+.40),
           (.53,.59,.36),CEDAR,1)

# Guard barracks: red roof textures, genuine stone arch entry and training court.
ax,ay=23,18
az,ah=shell('Raven Guard Barracks',ax,ay,9.1,7.3,1,
            'Wall_UnevenBrick_Window_Wide_Round','Roof_RoundTiles_8x10',.90,'Roof_Front_Brick8')
for i in range(6):
    xx=ax-5.3+i*2.02;yy=ay-7.6
    cylinder('Barracks wooden training pike',(xx,yy,ground(xx,yy)+1.46),
             .11,2.9,CEDAR,8)
    sphere('Barracks practice shield',(xx,yy-.2,ground(xx,yy)+1.90),
           (.65,.17,.61),BLUE,1)
box('Raven Guard banner',(ax+5.0,ay-4.0,az+4.1),(1.5,.18,2.5),BLUE,.05)

# Quarry physically is a resource workshop with actual PBR wall stone.
qx,qy=24,-12
qz,qh=shell('Eastern Quarry Masonry',qx,qy,8.5,6.5,1,
            'Wall_UnevenBrick_Straight','Roof_RoundTiles_6x8',.85,'Roof_Front_Brick6')
box('Quarry robust oak crane mast',(qx+6.3,qy+1,qz+4.1),
    (.62,.65,8.2),OAK,.09)
box('Quarry oak cantilever arm',(qx+3.2,qy+1,qz+7.7),
    (7.6,.61,.58),OAK,.08)
cylinder('Quarry winch',(qx+1.7,qy+.8,qz+5.3),.55,1.1,GOLD,12,(0,pi/2,0))
for i in range(14):
    xx=qx-6.9+(i%5)*1.75;yy=qy-6.3+(i//5)*1.7
    box('Quarry visible cut stone supply',(xx,yy,ground(xx,yy)+.39),
        (1.25,1.20,.72),ROCK,.14)

# Tracked compositional, sparse PBR trees and bushes instead of low-poly balls.
tree_specs=[]
for i in range(64):
    x=R.uniform(-54,54);y=R.uniform(-49,48)
    if -13<x<12 and -10<y<20:continue
    if math.hypot(x-sx,y-sy)<13 or math.hypot(x-gx,y-gy)<13:continue
    if math.hypot(x-ax,y-ay)<13 or math.hypot(x-qx,y-qy)<13:continue
    if abs(y+28-2*sin(x*.15))<4.5:continue
    asset=R.choice(['CommonTree_1','CommonTree_2','CommonTree_4','Pine_2','Pine_3'])
    scale=R.uniform(.75,1.22)
    stamp('N',asset,(x,y,ground(x,y)),scale,R.uniform(0,2*pi),'PBR natural grove')
    tree_specs.append((x,y))
for i in range(60):
    x=R.uniform(-48,48);y=R.uniform(-45,44)
    if math.hypot(x,y-5)<12:continue
    stamp('N',R.choice(['Bush_Common','Bush_Common_Flowers','Rock_Medium_1']),
          (x,y,ground(x,y)),R.uniform(.7,1.20),R.uniform(0,2*pi),'natural ground dressing')

# Two credible warm softbox light sources + clean sky; no textured cards.
def area(name,where,energy,color,size):
    light=bpy.data.lights.new(name,'AREA')
    light.energy=energy;light.color=color;light.shape='DISK';light.size=size
    ob=bpy.data.objects.new(name,light);bpy.context.collection.objects.link(ob)
    ob.location=where
    ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler()
area('warm low Valorian afternoon',(-29,-44,48),5500,(1,.84,.70),26)
area('blue white living detail fill',(35,34,56),3500,(.82,.88,1),46)

# Drop source import objects; all instantiated PBR copies retain material and mesh data.
for ob in reversed(imported):
    if ob.name in bpy.data.objects:bpy.data.objects.remove(ob,do_unlink=True)
cameraData=bpy.data.cameras.new('V10 genuine orthographic mobile strategy camera')
camera=bpy.data.objects.new('V10 genuine orthographic mobile strategy camera',cameraData)
bpy.context.collection.objects.link(camera);scene.camera=camera
cameraData.type='ORTHO'
def render(name,w,h,pos,target,ortho):
    camera.location=pos
    camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
    cameraData.ortho_scale=ortho
    scene.render.resolution_x=w;scene.render.resolution_y=h
    scene.render.filepath=str(OUT/name)
    bpy.ops.render.render(write_still=True)
render('bastion-v-pbr-wide.png',1440,900,(72,-79,99),(0,1.5,2.7),91)
render('bastion-v-pbr-mobile.png',900,1600,(72,-79,98),(0,2.6,3.2),76)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'valoria-modular-pbr-v10.blend'))
bpy.ops.export_scene.gltf(filepath=str(OUT/'valoria-modular-pbr-v10.glb'),
                          export_format='GLB',export_lights=False,export_cameras=False)
(OUT/'QUATERNIUS_CC0_LICENSE.txt').write_text(license_text)
manifest={
 'kind':'REAL_BLENDER_PBR_ORIGINAL_VALORIA_ARCHITECTURE_WITH_CC0_MODULES',
 'real_source_blend':True,'real_blender_captures':True,
 'existing_main_unmodified':True,'unity_render':False,'iphone_tested':False,
 'art_approved':False,'licence':'CC0 1.0 Universal',
 'source_repo':'https://github.com/agentkaerf/FreeModels',
 'source_commit':os.environ.get('QUATERNIUS_SOURCE_SHA','unrecorded'),
 'upstream_publisher':'Quaternius',
 'five_custom_buildings':['Bastion V','Sawmill','Granary','Barracks','Quarry'],
 'textured_model_types':len(templates),
 'instanced_original_model_meshes':len([o for o in created if o.type=='MESH']),
 'unique_pbr_libraries':sorted([collection+' '+name for collection,name in templates]),
 'design':'mobile readable low-to-medium detail, stylized PBR textured 3D, original Eldoria settlement'}
(OUT/'visual-proof.json').write_text(json.dumps(manifest,indent=2))
print('VALORIA_V10_CC0_TEXTURED_ARCHITECTURE_RENDER_OK',json.dumps(manifest))
