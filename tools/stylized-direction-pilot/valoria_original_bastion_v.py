# VALORIA / NEW VISUAL IDENTITY — FROM ZERO / BLENDER SOURCE
# Workstream: eldoria-stylized-direction-proof-20261115
# No legacy Eldoria GLB/FBX/prefab imported; geometry and materials originate here.
# Result is editable original 3D source, NOT a render of the playable Unity city.
import bpy, math, random, json, pathlib, sys
from math import sin, cos, pi
from mathutils import Vector
from mathutils.noise import noise_vector
R=random.Random(1626)
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
out=pathlib.Path(args[args.index('--out')+1] if '--out' in args else '/tmp/valoria-original')
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=28
# Ubuntu's distro Blender 4.0 is built without OpenImageDenoiser; render pure pathtraced samples.
if hasattr(scene.cycles,'use_denoising'): scene.cycles.use_denoising=False
if hasattr(scene.cycles,'use_preview_denoising'): scene.cycles.use_preview_denoising=False
for layer in scene.view_layers:
 if hasattr(layer,'cycles') and hasattr(layer.cycles,'use_denoising'): layer.cycles.use_denoising=False
scene.render.threads_mode='FIXED'
scene.render.threads=4
scene.render.image_settings.file_format='PNG'
scene.render.resolution_percentage=100
scene.view_settings.view_transform='Standard'
scene.view_settings.look='Medium High Contrast'
scene.view_settings.exposure=.42
scene.view_settings.gamma=1
scene.world=bpy.data.worlds.new('Valoria newly authored dusk sky')
scene.world.use_nodes=True
bg=scene.world.node_tree.nodes.get('Background')
bg.inputs['Color'].default_value=(.40,.48,.58,1)
bg.inputs['Strength'].default_value=.83

def linear(x):
 return x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4

def material(name,srgb,rough=.82,metal=0,emissive=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*srgb,1);m.use_nodes=True
 n=m.node_tree.nodes.get('Principled BSDF')
 n.inputs['Base Color'].default_value=(*(linear(v) for v in srgb),1)
 n.inputs['Roughness'].default_value=rough
 n.inputs['Metallic'].default_value=metal
 if emissive:
  if 'Emission' in n.inputs:n.inputs['Emission'].default_value=(*(linear(v) for v in srgb),1)
  if 'Emission Color' in n.inputs:n.inputs['Emission Color'].default_value=(*(linear(v) for v in srgb),1)
  if 'Emission Strength' in n.inputs:n.inputs['Emission Strength'].default_value=emissive
 return m

STONE=material('01 / warm grey Valorian stone',(.60,.60,.55))
LIME=material('02 / illuminated ashlar',( .78,.72,.60))
SHADOW=material('03 / inner masonry shaded',(.36,.39,.40))
SLATE=material('04 / graphite blue slate',(.24,.35,.50),.66)
SLATE_EDGE=material('05 / cool lighter slate highlights',(.36,.51,.63))
OAK=material('06 / oiled walnut timber',(.36,.25,.18))
WOOD=material('07 / warm cut cedar',(.61,.41,.25))
BRASS=material('08 / aged brass',(.73,.52,.26),.42,.40)
DARK=material('09 / deep entrance shadow',(.11,.16,.20))
GLASS=material('10 / firelit windows',(.98,.62,.30),.37,0,.85)
BANNER=material('11 / nocturnal heraldry',(.26,.29,.58))
BANNER_TRIM=material('12 / gold embroidery',(.84,.65,.30))
GRASS=material('13 / misty green meadow',(.36,.50,.34))
GRASS_B=material('14 / meadow relief',(.40,.53,.36))
EARTH=material('15 / compacted earth',(.59,.46,.32))
EARTH2=material('16 / warm dry path',(.68,.56,.38))
WATER=material('17 / cold teal river',(.22,.43,.48),.22)
FOAM=material('18 / subtle water light',(.51,.68,.64),.25)
PINE=[material('19 / evergreen dark',(.16,.33,.28)),material('20 / evergreen light',(.28,.43,.30)),
      material('21 / fir blue',(.25,.40,.39)),material('22 / needle deep',(.12,.27,.25))]
BARK=material('23 / tree bark',(.30,.25,.20))
ROCK=[material('24 / pebble moss',(.42,.47,.40)),material('25 / cool bank stone',(.47,.51,.54)),
      material('26 / rock warm edge',(.63,.60,.50))]
VIOLET=material('27 / distant Breach geode',(.57,.28,.71),.38,.0,1.05)
LEAF=material('28 / grass tufts',(.29,.43,.29))

# Source-owned albedo PNGs packed inside the original Blender file and GLB.
# Avoid shader-only Blender noise that would disappear on export to Unity.
def crafted_texture(ma,kind,color,size=256):
 image=bpy.data.images.new('Original_'+ma.name[:18]+'_'+kind,width=size,height=size,alpha=True)
 pixels=[0.0]*(size*size*4)
 for y in range(size):
  for x in range(size):
   tone=(1+.028*sin(x*.27)*cos(y*.31)+.018*cos(x*.62+y*.57)*sin(y*.41) if kind in ('foliage','soil') else 1+.060*sin(x*.043+2*sin(y*.034))+.050*cos(y*.049+x*.021)+.021*sin(x*.47+y*.18))
   if kind=='roof':
    row=y//27;tx=(x+row%2*23)%51;ty=y%27
    if tx<2 or ty<2:tone*=.70
    elif tx<5 or ty<5:tone*=1.13
   elif kind=='masonry':
    row=y//39;tx=(x+row%2*34)%69;ty=y%39
    if tx<3 or ty<3:tone*=.76
    elif tx<6 or ty<6:tone*=1.11
   elif kind=='wood':
    tone*=1+.095*sin(x*.13+sin(y*.026)*3)+.04*cos(x*.42)
    if x%57<2:tone*=.82
   elif kind=='foliage':
    tone*=1+.032*sin(x*.065+y*.032)+.016*cos(x*.14-y*.07)
   elif kind=='soil':tone*=1+.16*sin(x*.026+y*.045)
   ix=(y*size+x)*4
   for ch in range(3):pixels[ix+ch]=min(.99,max(.012,color[ch]*tone))
   pixels[ix+3]=1
 image.pixels.foreach_set(pixels)
 image.pack()
 tex=ma.node_tree.nodes.new('ShaderNodeTexImage')
 tex.name='SOURCE OWNED portable painted atlas'
 tex.image=image
 tex.extension='REPEAT'
 ma.node_tree.links.new(tex.outputs['Color'],ma.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
 return image

for mat,kind,color in (
 (STONE,'masonry',(.66,.65,.59)),(LIME,'masonry',(.78,.75,.66)),
 (SHADOW,'masonry',(.39,.42,.42)),(SLATE,'roof',(.29,.40,.57)),
 (SLATE_EDGE,'roof',(.42,.54,.66)),(OAK,'wood',(.39,.27,.20)),
 (WOOD,'wood',(.62,.44,.29)),(BARK,'wood',(.34,.29,.24)),
 (GRASS,'foliage',(.40,.53,.37)),(GRASS_B,'foliage',(.45,.57,.39)),
 (EARTH,'soil',(.59,.47,.36)),(EARTH2,'soil',(.70,.58,.40))):
 crafted_texture(mat,kind,color)
for mat in PINE:crafted_texture(mat,'foliage',tuple(mat.diffuse_color[:3]))
for mat in ROCK:crafted_texture(mat,'masonry',tuple(mat.diffuse_color[:3]))


def cube(name,location,dims,ma,bevel=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location)
 ob=bpy.context.object;ob.name=name;ob.dimensions=dims
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 ob.data.materials.append(ma)
 if bevel:
  m=ob.modifiers.new('designed chamfer','BEVEL');m.width=bevel;m.segments=2
  ob.modifiers.new('weighted normals','WEIGHTED_NORMAL')
 return ob

def cyl(name,loc,radius,depth,ma,vertices=10,rotation=None):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc)
 o=bpy.context.object;o.name=name;o.data.materials.append(ma)
 if rotation:o.rotation_euler=rotation
 return o

def ico(name,loc,scale,ma,sub=1):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=loc)
 ob=bpy.context.object;ob.name=name;ob.scale=scale;ob.data.materials.append(ma)
 return ob

def mesh(name,vertices,faces,ma):
 m=bpy.data.meshes.new(name);m.from_pydata(vertices,[],faces);m.update()
 obj=bpy.data.objects.new(name,m);bpy.context.collection.objects.link(obj);m.materials.append(ma)
 uv=m.uv_layers.new(name='Painted atlas world UV')
 for poly in m.polygons:
  for li in poly.loop_indices:
   co=m.vertices[m.loops[li].vertex_index].co
   uv.data[li].uv=(co.x/(23.0 if 'terrain' in name else 7.5),co.y/(23.0 if 'terrain' in name else 7.5))
 return obj

def gable(name,x,y,z,width,depth,rise,ma):
 # Actual enclosed 3D pitched roof, not a folded sheet with black/open gable ends.
 # Eaves are oversized, laminated and slightly upturned for mobile silhouette.
 subdivisions=8
 xvals=[(-.5+i/subdivisions)*width for i in range(subdivisions+1)]
 yvals=[-depth*.53,depth*.53]
 def height(i):
  q=abs(xvals[i])/(width*.5)
  return z+rise*(1-q**1.13)+.22*(q**7)
 # Top, underside and end faces are real mesh polygons: no see-through gables.
 v=[];f=[]
 for lift in (0,-.43):
  for yy in yvals:
   for i,xx in enumerate(xvals):
    v.append((x+xx,y+yy,height(i)+lift))
 stride=subdivisions+1
 for i in range(subdivisions):
  f.append((i,i+1,stride+i+1,stride+i))
  f.append((2*stride+i,3*stride+i,3*stride+i+1,2*stride+i+1))
  for row in (0,1):
   a=row*stride+i;b=a+2*stride
   f.append((a,b,b+1,a+1))
 for row in (0,1):
  for i in (0,subdivisions):
   a=row*stride+i;b=a+2*stride
   opposite=(1-row)*stride+i
   f.append((a,opposite,opposite+2*stride,b))
 roof=mesh(name+' / enclosed gable',v,f,ma)
 for poly in roof.data.polygons:poly.use_smooth=True
 # Two warm finished timber end-fascias and a long copper ridge.
 for side in (-1,1):
  yy=y+side*depth*.54
  for i in range(subdivisions):
   x1=x+xvals[i];x2=x+xvals[i+1];h1=height(i);h2=height(i+1)
   dx=x2-x1;dz=h2-h1
   ob=cube(name+' / carved gable fascia',((x1+x2)*.5,yy,(h1+h2)*.5-.10),
           (math.hypot(dx,dz)+.04,.22,.22),OAK,.025)
   ob.rotation_euler[1]=-math.atan2(dz,dx)
 cyl(name+' / warm metal ridge',(x,y,z+rise+.06),.08,depth*1.11,BRASS,10,(pi/2,0,0))
 # Rhythmic staggered slate seams: few broad bands, readable on a phone.
 for sign in (-1,1):
  for i in range(1,5):
   q=.18*i
   xx=x+sign*q*width*.5
   h=z+rise*(1-q**1.13)+.22*q**7+.024
   strip=cube(name+' / roof slate joint', (xx,y,h),
              (.045,depth*1.02,.047),SLATE_EDGE,.015)
   strip.rotation_euler[1]=sign*math.atan2(rise*(1-q**1.13)-rise*(1-(q+.12)**1.13),width*.06)
 return roof


def wall(name,x,y,z,w,d,h):
 cube(name+' / mass',(x,y,z+h*.5),(w,d,h),STONE,.23)
 cube(name+' / foot',(x,y,z+.18),(w+.42,d+.40,.35),SHADOW,.12)
 cube(name+' / stone cornice',(x,y,z+h-.23),(w+.30,d+.30,.33),LIME,.10)
 # Handcut cornerstones scaled enough to read at phone dimensions.
 for xx in (x-w*.47,x+w*.47):
  for yy in (y-d*.47,y+d*.47):
   for k in range(max(2,int(h/2.7))):
    cube('ashlar corner facing',(xx,yy,z+.65+k*2.4),(.58,.58,.70),LIME,.08)

def window(x,y,z,face='front',size=1):
 if face=='front':
  cube('deep fitted mullion',(x,y,z),(.90*size,.24,.98*size),OAK,.08)
  cube('warm chamfered luminous aperture',(x,y-.14,z),(.56*size,.12,.65*size),GLASS,.06)
  cube('stone lintel',(x,y,z+.60*size),(1.10*size,.31,.19*size),LIME,.07)
 else:
  cube('deep fitted mullion',(x,y,z),(.24,.9*size,.98*size),OAK,.08)
  cube('warm chamfered luminous aperture',(x+.14,y,z),(.12,.55*size,.65*size),GLASS,.06)

def banner(x,y,z,height=3):
 cube('indigo heraldic banner',(x,y,z),(.95,.12,height),BANNER,.06)
 cube('trimmed heraldic upper edge',(x,y-.09,z+height*.50),(1.08,.16,.12),BANNER_TRIM)
 cube('embroidered vertical sign',(x,y-.09,z),(.16,.15,height*.55),BANNER_TRIM)

def tree(x,y,height,kind=0):
 zz=ground(x,y);trunkH=height*.40
 cyl('hand-authored crooked pine trunk',(x,y,zz+trunkH*.5),height*.065,trunkH,BARK,7)
 for j in range(4):
  t=j/4
  sx=height*(.28-j*.048);sy=height*(.19-j*.026);sh=height*(.22-j*.025)
  xx=x+sin(x*2.9+j*2)*height*.032
  yy=y+cos(y*2.4+j)*height*.034
  ico('original irregular volumetric pine canopy',(xx,yy,zz+trunkH+height*(.10+j*.13)),
      (sx,sy,sh),PINE[(kind+j)%len(PINE)])
 # Cluster sibling branches to interrupt the symmetrical pine-cone outline.
 if kind%2==0:
  for side in (-1,1):
   ico('asymmetrical fir side-branch',(x+side*height*.17,y+height*.07*side,zz+height*.63),
       (height*.12,height*.15,height*.17),PINE[(kind+2)%len(PINE)])

def ground(x,y):
 # Continuous terrain without boxes, raised islands or visible building plots.
 n=.23*sin(x*.12)+.18*cos(y*.17)+.10*sin(x*.27+y*.18)
 shoulder=.70*math.exp(-((x/23)**2+((y-8)/21)**2))
 river=-.98*math.exp(-((y+29+1.8*sin(x*.11))**2)/14)
 return n+shoulder+river

# authored single undulating ground, seamless paths and river embankment.
verts=[];faces=[];width=241;depth=241
for iy in range(depth):
 y=-120+iy*1.00
 for ix in range(width):
  x=-120+ix*1.00;verts.append((x,y,ground(x,y)))
for iy in range(depth-1):
 for ix in range(width-1):
  a=iy*width+ix;faces.append((a,a+1,a+1+width,a+width))
terrain=mesh('ONE unified sculpted terrain, no legacy assets',verts,faces,GRASS)
for p in terrain.data.polygons:p.use_smooth=True
# Permanent albedo of the terrain is embedded, with no viewport-only node override.

def ribbon(name,poly,width,ma,offset=.052):
 v=[];f=[]
 for index,(x,y) in enumerate(poly):
  if index==0:dx,dy=poly[1][0]-x,poly[1][1]-y
  elif index==len(poly)-1:dx,dy=x-poly[index-1][0],y-poly[index-1][1]
  else:dx,dy=poly[index+1][0]-poly[index-1][0],poly[index+1][1]-poly[index-1][1]
  mag=max(1e-4,math.hypot(dx,dy)); nx=-dy/mag;ny=dx/mag
  w=width*(.91+.09*sin(index*.56))
  for sign in (-1,1):
   xx=x+sign*nx*w*.5;yy=y+sign*ny*w*.5
   v.append((xx,yy,ground(xx,yy)+offset))
  if index>0:
   a=2*(index-1);f.append((a,a+1,a+3,a+2))
 return mesh(name,v,f,ma)

# Large walkway radiating from the keep; roads are purposefully distinct.
ribbon('central worn dirt avenue / not a flat plaza',[(0,-11+i*.65) for i in range(31)],4.5,EARTH)
ribbon('sawmill orchard road',[(-1,-9),(-6,-13),(-13,-16),(-20,-17),(-30,-17)],2.7,EARTH)
ribbon('granary approach',[(-7,1),(-15,9),(-25,15),(-32,17)],2.5,EARTH2)
ribbon('barracks rutted path',[(8,1),(16,4),(23,9),(30,13)],3.1,EARTH)
ribbon('quarry delivery path',[(5,-11),(12,-16),(21,-19),(30,-21)],3.4,EARTH2)

# River with curved bank; whole scene stays a continuous 3D landscape.
riverline=[(x*.7,-29-1.8*sin((x*.7)*.11)) for x in range(-69,70)]
ribbon('broad winding teal river',riverline,8.5,WATER,-.39)
ribbon('subtle flowing reflective center',riverline,2.0,FOAM,-.36)
for i in range(28):
 x=-50+i*3.6;y=-29-1.8*sin(x*.11)
 for side in (-1,1):
  dx=side*(4.2+R.uniform(.5,2.4))
  ico('river-buried rounded stone',(x+R.uniform(-.6,.6),y+dx,ground(x,y+dx)-.16),
      (R.uniform(.65,1.5),R.uniform(.40,.85),R.uniform(.22,.52)),R.choice(ROCK))

# THE BASTION — original silhouette, deliberate disproportion and imperial identity.
kx,ky=-2,8;kz=ground(kx,ky)
wall('Bastion V / tall central keep',kx,ky,kz,22,16,11.5)
gable('The great roof of Valoria',kx,ky,kz+12.1,25,18,5.6,SLATE)
# A stepped gatehouse strongly visible from the south, sunk into the keep.
wall('South gatehouse',kx,-2,kz,10,7,9.0)
gable('Gatehouse signature blue-black roof',kx,-2,kz+9.2,12.2,8.3,4,SLATE)
cube('gothic portcullis oak',(kx,-5.65,kz+3.1),(3.5,.46,5.8),DARK,.24)
for xx in (-4.65,.65):
 cube('massive gate piers',(xx,-5.9,kz+3.8),(.85,.74,7.1),LIME,.12)
for ix in (-8,-4,0,4,8):
 window(kx+ix,ky-8.1,kz+8.8,'front',.93)
for sx in (-1,1):
 tx=kx+sx*11.5;ty=ky+4;th=15.3
 wall('Twin Valorian bastions',tx,ty,kz,5.7,5.7,th)
 gable('Eastern-western sentry caps',tx,ty,kz+th+.2,7.4,7.0,3.5,SLATE_EDGE)
 window(tx,ty-2.9,kz+10.8,'front',1.08)
 banner(tx,ty-3.15,kz+6.1,2.7)
# A taller asymmetrical tower makes the tier-five fantasy recognizable.
wall('Imperial bellwatch',kx+4.5,ky+7.2,kz+7,6.6,6.5,11.2)
gable('Bellwatch slate spire',kx+4.5,ky+7.2,kz+18.6,8.2,7.5,5.0,SLATE)
for yy in (ky+4.0,ky+10.7): window(kx+4.5,yy,kz+15,'front',1.0)
cyl('Sole Valorian roof pennant mast',(kx+4.5,ky+7.2,kz+24.4),.10,2.9,BRASS,9)
banner(kx+4.5,ky+7.2,kz+25.3,1.2)

def house(name,x,y,w,d,h,roof_height,roof_color,wall_mat=STONE):
 z=ground(x,y)
 cube(name+' / physical structural plinth',(x,y,z+.22),(w+.8,d+.8,.45),SHADOW,.19)
 cube(name+' / inhabited body',(x,y,z+h*.5),(w,d,h),wall_mat,.22)
 cube(name+' / overhanging cornice',(x,y,z+h-.18),(w+.35,d+.35,.32),LIME,.09)
 gable(name+' / oversized curved slate roof',x,y,z+h+.10,w+1.15,d+1.3,roof_height,roof_color)
 window(x,y-d*.51,z+h*.62,'front',.94)
 return z

# SAWMILL / craft silhouette, open timber log mechanics, not generic houses.
x,y=-30,-10; sz=house('Sawmill of Westbank',x,y,12,8,4.9,3.65,SLATE_EDGE,OAK)
for side in (-1,1):
 cube('sawmill exposed outer support',(x+side*5.7,y-3.4,sz+2.2),(.62,.68,4.1),WOOD,.12)
for k in range(6):
 cyl('usable timber logs',(x-6.0+k*.9,y-7.3,sz+.45),.42,5.1,WOOD,8,(pi/2,0,0))
# Real waterwheel with spokes and ring, readable from the 3/4 camera.
cx,cy,cz=x+7.1,y-.6,sz+2.4
for k in range(12):
 a=2*pi*k/12
 px=cx;py=cy+2.5*cos(a);pz=cz+2.5*sin(a)
 cube('waterwheel radial paddle',(px,py,pz),(.36,.65,.73),WOOD,.09)
a1=None
for k in range(12):
 a=2*pi*k/12
 end=(cy+2.45*cos(a),cz+2.45*sin(a))
 start=(cy+2.45*cos(a+2*pi/12),cz+2.45*sin(a+2*pi/12))
 mid=((end[0]+start[0])/2,(end[1]+start[1])/2)
 o=cube('sawmill ring section',(cx,mid[0],mid[1]),(.52,.55,1.45),BRASS,.06)
 o.rotation_euler[0]=a+pi/12
cyl('sawmill wheel shaft',(cx,cy,cz),.35,2.4,OAK,12,(0,pi/2,0))

# BARRACKS / same material family but a recognizable defensive outline.
x,y=28,10; qz=house('Raven Guard barracks',x,y,14.3,9,5.5,3.9,SLATE,STONE)
for sx in (-1,1):
 wall('Barracks forward watchtower',x+sx*6.7,y-2.9,qz,3.7,3.7,7.3)
 gable('Barracks tower weathered roof',x+sx*6.7,y-2.9,qz+7.6,4.6,4.2,2.8,SLATE_EDGE)
banner(x,y-4.7,qz+3.8,3.0)
for k in range(3):
 cube('training practice target upright',(x-6+k*2.3,y-9,qz+1.4),(.24,.29,2.7),OAK,.04)
 ico('training target shield',(x-6+k*2.3,y-9.2,qz+2.0),(.70,.18,.71),LIME)

# GRANARY / silhouette is round silo + golden fabric, not the barracks.
x,y=-30,19;gz=house('Granary of the Great Harvest',x,y,11.5,7.6,4.7,3.2,SLATE_EDGE,OAK)
for k in (-1,1):
 sx=x+k*7.4
 cyl('golden harvest storage silo',(sx,y+1.0,gz+3.1),2.15,6.0,LIME,12)
 ico('silo heavy conical thatch cap',(sx,y+1,gz+6.2),(2.35,2.35,1.5),SLATE,1)
for k in range(5):
 ico('grain deliveries in burlap',(x-4+k*1.45,y-5.6,gz+.48),(.72,.62,.62),EARTH2)

# QUARRY / open-air rig physically connected to dressed blocks.
x,y=28,-15;qz=house('Eastern stoneworks',x,y,11.3,7.2,4.1,3.0,SLATE,SHADOW)
cube('massive crane vertical oak',(x+6.1,y+2,qz+4.8),(.9,.9,9.8),OAK,.13)
cube('quarry crane cross timber',(x+3.2,y+2,qz+9.3),(7.5,.76,.78),WOOD,.13)
cyl('quarry winch',(x+0.4,y+1.9,qz+7.4),.72,1.65,BRASS,12,(0,pi/2,0))
for j in range(14):
 xx=x-7+(j%5)*1.9;yy=y-7+(j//5)*1.6
 cube('dressed stone yard',(xx,yy,qz+.38),(.95+R.random()*.6,1.15,.63),ROCK[j%3],.08)

# SIGNATURE ART PASS — original, physical, mobile-readable architecture (not recolor).
# Real defenses around the gate; different masses, repeated consistent craftsmanship.
for side in (-1,1):
 xx=kx+side*12.4
 wall('fortress defensive forecourt wing',xx,-1.5,kz,10.2,3.9,4.6)
 for dx in (-3.5,-1.25,1.25,3.5):
  cube('gatehouse merlon on walk',(xx+dx,-1.5,kz+5.02),
       (.95,2.6,1.05),LIME,.12)
 # The projecting flared sloped base buttresses prevent flat box-like walls.
 for yoffset in (-5.2,1.1,8.5):
  bx=kx+side*(11.0 if yoffset<5 else 9.8)
  cube('bastion curved base buttress',(bx,ky+yoffset,kz+3.65),
       (1.6,1.6,7.2),STONE,.29)
  cube('honed external stone ledge',(bx,ky+yoffset,kz+7.0),
       (1.90,1.87,.34),LIME,.11)
# Sculpted gate arch voussoirs and central heraldic device, expressed in actual 3D.
for a in range(11):
 angle=pi*(a/10)
 ax=kx+2.62*cos(angle);az=kz+6.45+2.00*sin(angle)
 block=cube('arched royal entry voussoir',(ax,-6.11,az),
            (.74,.64,.50),LIME,.085)
 block.rotation_euler[1]=angle
ico('cast bronze emblem medallion',(kx,-6.38,kz+8.70),
    (1.2,.21,1.2),BRASS,2)
for sy in (-1,1):
 cube('Valorian crossed crest',(kx+sy*.28,-6.63,kz+8.7),(.18,.17,1.50),
      BANNER_TRIM,.045)
# Intentional rough staggered ashlar on the accessible front wall and towers.
for floor in (3.0,5.45,8.5):
 for i in range(11):
  x=kx-9.0+i*1.75+(.75 if floor==5.45 else 0)
  if abs(x-kx)<2.7 and floor<6.3:continue
  cube('laid hand-dressed stone face',(x,ky-8.14,kz+floor),
       (1.50,.18,.32),LIME if i%4==0 else STONE,.11)
for side in (-1,1):
 tx=kx+side*11.5
 for k in range(3):
  window(tx,ky+1.09,kz+4.0+k*3.5,'front',.72)
# Portal approach is physically anchored in the existing sculpted dirt path.
for i in range(4):
 y=-8.3+i*.83;x=kx
 cube('gate arrival dressed stone step',(x,y,ground(x,y)+.11+i*.04),
      (6.25-i*.28,.73,.24),STONE if i%2 else LIME,.065)
# Contrast silhouette with small crafted inhabited structures.
for x,y,height in [(-35,-9,4.0),(-25,-11,4.1),(34,10,4.7),(22,13,4.6),
                   (-26,20,4.9),(29,-13,4.2)]:
 z=ground(x,y)
 cube('warm timber braced beam',(x,y,z+height*.42),(.34,.38,height*.83),WOOD,.09)
 cube('timber gable wall bracket',(x,y,z+height*.85),(1.30,.52,.29),OAK,.06)
# Four strategically positioned chimneys with believable roof penetration.
for xx,yy,zz in [(-30,-10,sz+7.4),(28,10,qz+9.3),(-30,19,gz+7.3),(-2,8,kz+17.5)]:
 cube('aged masonry chimney shaft',(xx+2,yy+1.5,zz),(1.20,1.30,3.9),SHADOW,.18)
 cube('chimney rain cover',(xx+2,yy+1.5,zz+2.0),(1.65,1.7,.36),LIME,.10)
# Public space life is represented by work props, not new unapproved houses.
for j in range(4):
 cube('cordwood stack near sawmill',(-26+j*.85,-19.5,ground(-26,-19.5)+.45),
      (.65,3.2,.82),WOOD,.06)
for j in range(5):
 ico('granary golden storage basket',(-36+j*1.2,13.8,ground(-36+j*1.2,13.8)+.49),
     (.48,.58,.46),EARTH2,1)
for j in range(9):
 x=21+(j%3)*1.7;y=-23+(j//3)*1.45
 cube('stonecutters pallet loaded ashlar',(x,y,ground(x,y)+.37),
      (1.25,1.07,.59),STONE if j%2 else LIME,.10)
# Soft unlit vegetation patches add a screen-space layer between ground and masonry.
for i in range(115):
 x=R.uniform(-52,52);y=R.uniform(-43,42)
 if abs(y+29+1.8*sin(x*.11))<6.5:continue
 if (x+2)**2+(y-8)**2<295:continue
 if (x+30)**2+(y+10)**2<100:continue
 if (x-28)**2+(y-10)**2<110:continue
 plant=ico('clustered wild sage scrub',(x,y,ground(x,y)+.33),
           (R.uniform(.33,.85),R.uniform(.32,.79),R.uniform(.34,.72)),
           R.choice((GRASS_B,LEAF,PINE[1])),1)
 # Intentional asymmetry and colour family unified.
# Real crafted lantern posts (later mobile UI does not rely on these as buttons).
for xx,yy in [(-7,-12),(5,-11),(-22,-18),(16,1),(20,-17),(-27,14)]:
 zz=ground(xx,yy)
 cyl('roadway lantern support',(xx,yy,zz+1.7),.14,3.2,OAK,9)
 cube('lantern mounted iron cap',(xx,yy,zz+3.23),(.83,.75,.35),DARK,.06)
 ico('lantern small physical amber window',(xx,yy,zz+2.83),(.40,.38,.52),GLASS,1)

# Irregular, half-buried low-poly rock at natural terrain edges.
for i in range(95):
 x=R.uniform(-52,52);y=R.uniform(-43,45)
 if abs(x)<21 and -15<y<27:continue
 ico('mossed buried valley outcrop',(x,y,ground(x,y)-.08),
     (R.uniform(.48,1.6),R.uniform(.48,1.7),R.uniform(.3,.82)),ROCK[i%3])
# Non-repetitive tree groves. No uniform cones or screenshot-backed billboard trees.
count=0
for i in range(95):
 x=R.uniform(-52,52);y=R.uniform(-43,42)
 if abs(x)<19 and -20<y<29:continue
 if (x+30)**2+(y+10)**2<160 or (x-28)**2+(y-10)**2<220:continue
 if (x+30)**2+(y-19)**2<190 or (x-28)**2+(y+15)**2<190:continue
 if abs(y+29+1.8*sin(x*.11))<8:continue
 if abs(x)<5 and y<0:continue
 tree(x,y,R.uniform(4.7,9.5),i%4);count+=1
# Ground-cover clusters share one mesh to keep source editable and scalable.
gv=[];gf=[]
for i in range(650):
 x=R.uniform(-51,51);y=R.uniform(-38,44)
 if abs(x)<12 and -17<y<20:continue
 if abs(y+29+1.8*sin(x*.11))<5:continue
 z=ground(x,y)+.055; radius=R.uniform(.11,.3);height=R.uniform(.23,.67)
 start=len(gv)
 gv.extend([(x-radius,y,z),(x+radius,y,z),(x+radius*.3,y+radius*.2,z+height)])
 gf.append((start,start+1,start+2))
mesh('author-batched meadow grasses / no pixel sprites',gv,gf,LEAF)

# Small, distant, geological Breach — Eldoria narrative identity, not neon everywhere.
for i in range(7):
 x=42+R.uniform(-4,4);y=28+R.uniform(-3,3);z=ground(x,y)
 ico('ancient obsidian around fissure',(x,y,z+.38),(R.uniform(.5,1.1),.6,.75),DARK)
 ico('violet rift mineral sliver',(x,y,z+.9),(.25,.22,R.uniform(.65,1.2)),VIOLET)

# Stone bridge physically spans the original river. No fake cliff or floating road.
cube('arched river bridge central deck',(0,-29,ground(0,-29)+1.0),(8.1,13.5,.83),STONE,.20)
for sx in (-3.8,3.8):
 cube('heavy original bridge parapets',(sx,-29,ground(0,-29)+1.7),(.65,13.3,1.55),LIME,.15)
for k in range(12):
 for j in range(4):
  x=-2.4+j*1.6;y=-35.3+k*1.05
  cube('readable masonry bridge paver',(x,y,ground(0,-29)+1.46),
       (1.42,.9,.12),R.choice([STONE,LIME,SHADOW]),.055)

# V6 procedural commercial stylization, authored independently from legacy models.
exec(compile(open('tools/stylized-direction-pilot/valoria_commercial_stylization_v6.py',encoding='utf-8').read(),
             'valoria_commercial_stylization_v6.py','exec'),globals())

# V7 distinct architectural and lived-in city art. Source remains original and editable.
exec(compile(open('tools/stylized-direction-pilot/valoria_characterful_city_v7.py',encoding='utf-8').read(),
             'valoria_characterful_city_v7.py','exec'),globals())

# V8 substantial skyline rebuilding; removes the former oversized house-like roof.
exec(compile(open('tools/stylized-direction-pilot/valoria_citadel_massing_v8.py',encoding='utf-8').read(),
             'valoria_citadel_massing_v8.py','exec'),globals())

# V9 separate compact alternative. Substantial geometry rebuild and authored palette.
exec(compile(open('tools/stylized-direction-pilot/valoria_mobile_citadel_v9.py',encoding='utf-8').read(),
             'valoria_mobile_citadel_v9.py','exec'),globals())

def area(name,location,power,color,size):
 light=bpy.data.lights.new(name,'AREA');light.energy=power;light.color=color;light.shape='DISK';light.size=size
 obj=bpy.data.objects.new(name,light);bpy.context.collection.objects.link(obj)
 obj.location=location
 obj.rotation_euler=(Vector((0,0,1))-obj.location).to_track_quat('-Z','Y').to_euler()

area('low golden legibility sun',(-34,-47,61),6800,(1,.84,.68),27)
area('blue cloud shadow fill',(36,24,48),4100,(.68,.80,1),40)
for pos in [(-2,-5,kz+7),(-30,-14,sz+4),(28,5,qz+7)]:
 d=bpy.data.lights.new('inhabited amber lantern','POINT');d.energy=185
 d.color=(1,.51,.23);d.shadow_soft_size=1.6
 o=bpy.data.objects.new('inhabited amber lantern',d);bpy.context.collection.objects.link(o);o.location=pos

data=bpy.data.cameras.new('camera isometric strategy real 3D')
camera=bpy.data.objects.new('camera isometric strategy real 3D',data)
bpy.context.collection.objects.link(camera);scene.camera=camera;data.type='ORTHO'

def frame(name,resX,resY,position,target,scale):
 camera.location=position
 camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
 camera.data.ortho_scale=scale
 scene.render.resolution_x=resX;scene.render.resolution_y=resY
 scene.render.filepath=str(out/name)
 bpy.ops.render.render(write_still=True)

# Distinct from legacy Valoria: iconic readable vertical masses, bold roofs and warm/cool contrast.
frame('original-bastion-v-landscape.png',1600,1000,(70,-83,111),(0,4,2.7),101)
frame('original-bastion-v-portrait.png',900,1600,(70,-83,110),(0,6,2.9),78)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'valoria-original-identity.blend'))
# Genuine 3D GLB; original editable project is the source of truth.
bpy.ops.export_scene.gltf(filepath=str(out/'valoria-original-bastion-v.glb'),
                          export_format='GLB',export_cameras=False,export_lights=False)
geo=len([o for o in bpy.data.objects if o.type=='MESH'])
manifest={'original_authored_geometry':True,
 'legacy_models_imported':False,'blender_render':True,'unity_render':False,
 'art_approved':False,'physical_mobile_tested':False,
 'level':'Bastion V','mesh_objects':geo,'pine_trees':count,
 'concept_only':False,'real_editable_blender_source':True,
 'look':'mature accessible 3D mobile strategy, original Eldoria identity',
 'source_script':'tools/stylized-direction-pilot/valoria_original_bastion_v.py',
 'original_art_pass':'tools/stylized-direction-pilot/valoria_commercial_stylization_v6.py',
 'style_pass':'V6 substantial geometry + hand-crafted roof repairs and ecology',
 'art_pass_counts':V6_STYLE_COUNTS,
 'architectural_identity_pass':'tools/stylized-direction-pilot/valoria_characterful_city_v7.py',
 'architectural_components_v7':style_count,
 'compact_mobile_v9':V9_METRICS,
 'mobile_language_art':'low horizontal keep, five separated recognizable functional areas, four resource roof palettes',
 'citadel_v8':v8_proof}
(out/'visual-proof.json').write_text(json.dumps(manifest,indent=2))
print('VALORIA_ORIGINAL_BLENDER_PASS',json.dumps(manifest))
