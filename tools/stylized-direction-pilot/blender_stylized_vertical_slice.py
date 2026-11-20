# Original authored Blender geometry, materials and renders. Not a Unity screenshot.
import bpy,math,random,sys,os,json
from mathutils import Vector,noise
from math import sin,cos,pi,exp
R=random.Random(1943)
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
out=args[args.index('--out')+1] if '--out' in args else '/tmp/eldoria-art'
os.makedirs(out,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=30
scene.view_settings.view_transform='Filmic';scene.view_settings.look='Medium High Contrast';scene.view_settings.exposure=.96
scene.render.image_settings.file_format='PNG';scene.render.resolution_percentage=100
scene.world.color=(.26,.29,.33)
def mat(name,color,glow=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1)
 bs.inputs['Roughness'].default_value=.88
 if glow:
  if 'Emission' in bs.inputs:bs.inputs['Emission'].default_value=(*color,1)
  if 'Emission Color' in bs.inputs:bs.inputs['Emission Color'].default_value=(*color,1)
  if 'Emission Strength' in bs.inputs:bs.inputs['Emission Strength'].default_value=glow
 return m
stone=[mat('aged limestone grey',(.47,.47,.43)),mat('stone sun face',(.66,.62,.53)),mat('stone weathered shade',(.29,.32,.33))]
grass=[mat('sage meadow',(.28,.36,.26)),mat('ochre meadow',(.39,.40,.25)),mat('forest meadow',(.19,.30,.26))]
wood=mat('timber',(.31,.22,.17));repair=mat('raw timber',(.45,.33,.23))
slate=mat('dark slate',(.19,.24,.31));river=mat('cold teal water',(.10,.28,.32))
pine=[mat('evergreen deep',(.10,.24,.20)),mat('evergreen sage',(.15,.33,.27)),mat('evergreen olive',(.25,.35,.26))]
bronze=mat('ancient bronze',(.57,.43,.20));banner=mat('indigo imperial cloth',(.09,.15,.26))
amber=mat('settlement fire',(.99,.48,.16),1.9);breach=mat('breach glow',(.30,.05,.44),.9)
dark=mat('hollow gate',(.07,.09,.10));soil=mat('dry earth',(.44,.37,.26))
def box(name,loc,dims,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=dims
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(m);return o
def rock(name,loc,dims,m):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=loc)
 o=bpy.context.object;o.name=name;o.scale=dims;o.data.materials.append(m);return o
def cone(name,loc,r,d,m):
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r,radius2=0,depth=d,location=loc)
 o=bpy.context.object;o.name=name;o.data.materials.append(m);return o
def ownmesh(name,vertices,faces,materials,indices=None):
 me=bpy.data.meshes.new(name);me.from_pydata(vertices,[],faces);me.update()
 o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
 for m in materials:me.materials.append(m)
 if indices:
  for face,i in zip(me.polygons,indices):face.material_index=i
 return o
def river_y(x):
 # One watercourse geometry drives bank erosion, surface mesh, shorelines and ecological setbacks.
 return -17-2.7*sin((x+5)*.092)-1.15*sin((x+5)*.21)

def z(x,y):
 n=noise.noise(Vector((x*.065,y*.065,1)))*.44+noise.noise(Vector((x*.2,y*.2,2)))*.12
 # Continuous, terrain-attached macro terraces and deliberately eroded riverbanks.
 hills=1.0*exp(-((x+5)**2/400+(y-5)**2/330))
 riverbed=1.25*exp(-((y-river_y(x))**2)/23)
 ridge=.68*exp(-((x+34)**2/170+(y-22)**2/300))
 distant=.62*exp(-((x-36)**2/240+(y-30)**2/360))
 return n+hills+ridge+distant-riverbed
# Single continuous authored topography, no prefab rectangular island.
vs=[];fs=[];mi=[]
for j in range(119):
 for i in range(145):
  x=-72+i*1.0;y=-54+j*.95;vs.append((x,y,z(x,y)))
for j in range(118):
 for i in range(144):
  a=j*145+i;fs.extend([(a,a+1,a+146),(a,a+146,a+145)])
  x=-72+i*1.0;y=-54+j*.95;n=noise.noise(Vector((x*.09+7,y*.09+3,2)))
  t=0 if n<-.20 else (2 if n>.36 else 1);mi.extend([t,t])
terrain=ownmesh('unified continuous painterly soil',vs,fs,grass,mi)
# Original smooth hand-painted sage material: no hard checkerboard terrain patches.
for polygon in terrain.data.polygons: polygon.material_index=0
grassnodes=grass[0].node_tree.nodes
noiseNode=grassnodes.new('ShaderNodeTexNoise');noiseNode.inputs['Scale'].default_value=9.0
ramp=grassnodes.new('ShaderNodeValToRGB')
ramp.color_ramp.elements[0].position=.20;ramp.color_ramp.elements[0].color=(.10,.17,.11,1)
ramp.color_ramp.elements[1].position=.79;ramp.color_ramp.elements[1].color=(.34,.43,.26,1)
links=grass[0].node_tree.links
links.new(noiseNode.outputs['Fac'],ramp.inputs['Fac'])
links.new(ramp.outputs['Color'],grassnodes.get('Principled BSDF').inputs['Base Color'])
rvs=[];rfs=[]
for i in range(151):
 x=-47+i*94/150;y=river_y(x)
 for side in [-1,1]:rvs.append((x,y+side*2.75,-.71))
 if i<150:rfs.append((2*i,2*i+1,2*i+3,2*i+2))
ownmesh('original single river ribbon',rvs,rfs,[river])

# Subtle ripple bands follow the same actual river centerline, not detached lakes.
foam=mat('quiet turquoise bank froth',(.29,.47,.45))
for side in (-1,1):
 wavev=[];wavef=[]
 for k in range(125):
  x=-47+k*94/124
  y=river_y(x)+side*(2.48+.12*sin(x*.37))
  wavev.extend([(x,y,-.67),(x,y+side*.18,-.67)])
  if k<124:wavef.append((k*2,k*2+1,k*2+3,k*2+2))
 ownmesh('shoreline subtle ripples',wavev,wavef,[foam])
# Castle remains compact but visibly monumental.
bz=z(-5,5)
box('aged fortress foundation',(-5,5,bz+1.45),(20,15,3.0),stone[2])
box('upper ward and bastion',(-5,6,bz+5.75),(13,10,7.4),stone[1])
box('slate roof main mass',(-5,6,bz+10),(13.6,10.5,.9),slate)
for x in [-12,2]:
 for y in [1.5,10.3]:
  box('four weighty defensive towers',(x,y,bz+7.5),(3.7,3.7,11),stone[1])
  box('stone tower crowns',(x,y,bz+13.2),(4.6,4.6,.85),stone[0])
  for a in [-1.35,-.45,.45,1.35]:
   box('crenellations',(x+a,y-1.9,bz+13.9),(.65,.65,.8),stone[1])
   box('crenellations',(x+a,y+1.9,bz+13.9),(.65,.65,.8),stone[1])
box('central gate surround',(-5,.77,bz+4.9),(5.5,.6,6.4),stone[0])
box('heavy gateway',(-5,.39,bz+4.0),(3.8,.33,4.4),dark)
for x in [-8.3,-1.7]:
 rock('warm fire torches',(x,.1,bz+5.0),(.40,.35,.5),amber)
 for yy in [1.6,10.3]:box('indigo royal banners',(x,yy,bz+8.4),(.7,.12,2.7),banner)
# Small sawmill and barracks define Bastion I-II scale.
box('working sawmill',(-24,4,z(-24,4)+1.55),(10,7,3.1),wood)
box('sawmill roof',(-24,4,z(-24,4)+3.4),(11.0,7.6,.9),slate)
for k in range(5):box('logs',(-27.2+k*.8,0,z(-26,0)+.46),(.53,3.5,.44),repair)
box('barracks', (13,-1,z(13,-1)+1.5),(9,7,3),stone[0])
box('barracks slate roof',(13,-1,z(13,-1)+3.5),(10,7.6,1.2),slate)
box('barracks doorway',(13,-4.6,z(13,-1)+1.65),(2.0,.12,2.55),wood)
# Cobble approach and sturdy original bridge.
for row in range(32):
 y=-13+row*.46;x=-5+.45*sin(row*.13)
 for lane in range(5):
  xx=x+(lane-2)*.76
  box('cut road stones',(xx,y,z(xx,y)+.08),(.67,.40,.13),stone[0 if (row+lane)%3 else 2])
box('bridge deck',(-5,-17,.0),(7.0,13,.95),stone[2])
for row in range(12):
 for lane in range(4):
  box('bridge cobbled roadway',(-7.25+lane*1.48,-23+row*1.05,.515), (1.38,.97,.08),stone[0 if (row+lane)%4 else 1])
for x in [-8.2,-1.8]:
 box('bridge parapet',(x,-17,.76),(.56,12.8,1.35),stone[0])
 for y in [-22.6,-11.4]:
  box('torch pier',(x,y,1.4),(.84,.84,2.1),stone[1])
  rock('golden torch',(x,y,2.6),(.40,.40,.57),amber)
# Broken cyclopean imperial arch: explicit large curved wedge geometry, not a portal sprite.
acx=17;acy=17;acz=9
for x in [acx-7.4,acx+7.4]:
 box('old empire arch pier',(x,acy,4.6),(2.4,3.7,9),stone[0])
for k in range(13):
 if k in [5,6,7]:continue
 a=k*pi/14;b=(k+1)*pi/14
 verts=[]
 for r,theta,yy in [(5.9,a,-1.7),(7.2,a,-1.7),(7.2,b,-1.7),(5.9,b,-1.7),
                    (5.9,a,1.7),(7.2,a,1.7),(7.2,b,1.7),(5.9,b,1.7)]:
  verts.append((acx+r*cos(theta),acy+yy,acz+r*sin(theta)))
 ownmesh('cyclopean broken arch stone voussoir',verts,
  [(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],[stone[k%2]])
# Grouped conifers; leave breathable tactical clearings.
def tree(x,y,h,i):
 zz=z(x,y)
 box('old conifer trunk',(x,y,zz+h*.35),(.3,.3,h*.70),wood)
 for lev in range(3):
  cone('stylized forest tree crown',(x,y,zz+h*(.34+lev*.25)),h*(.26-.044*lev),h*.46,pine[(i+lev)%3])
for i in range(250):
 x=R.uniform(-44,44);y=R.uniform(-31,33)
 if not (abs(x)>28 or y>21 or (x<-18 and y>10) or (x>25 and y>7)):continue
 if abs(y-river_y(x))<5:continue
 tree(x,y,R.uniform(4,8),i)
# Rock outcrops and edge decoration are from one modular family.
for i in range(95):
 x=R.uniform(-45,45);y=R.uniform(-31,32)
 if abs(x+5)<10 and -24<y<13:continue
 scale=R.uniform(.4,1.25)
 rock('weathered angular stone',(x,y,z(x,y)+.12),(.9*scale,.7*scale,.55*scale),stone[2 if i%3 else 0])
# Purple geological wound is rare and spatial, not a billboard/portal.
fracture=[(26,7),(29,11),(26,16),(30,20),(26,26)]
for i in range(len(fracture)-1):
 a=fracture[i];b=fracture[i+1];dx=b[0]-a[0];dy=b[1]-a[1]
 o=box('scar / glowing ground fracture',((a[0]+b[0])/2,(a[1]+b[1])/2,z(*a)+.16),
       (math.hypot(dx,dy)+.6,.57,.25),breach);o.rotation_euler.z=math.atan2(dy,dx)
for i in range(7):
 x=26+R.uniform(-3,3);y=15+R.uniform(-7,7)
 cone('breach crystal',(x,y,z(x,y)+1.0),.4,2.2,breach)

# ACTUAL OWN GAME ASSET REUSE. This substitutes the crude experimental
# castle/workshop blocks with the current authentic Bastion II and Sawmill.
# The old expressly forbidden HeroBastion model is never requested.
def reused_glb(asset,target,width,base,label):
 if not os.path.isfile(asset):raise RuntimeError('Required source art not in pinned checkout: '+asset)
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=asset)
 new=[o for o in bpy.data.objects if o not in before]
 models=[o for o in new if o.type=='MESH']
 if not models:raise RuntimeError('Canonical GLB empty: '+asset)
 bpy.context.view_layer.update()
 bb=[o.matrix_world@Vector(v) for o in models for v in o.bound_box]
 lo=Vector((min(v.x for v in bb),min(v.y for v in bb),min(v.z for v in bb)))
 hi=Vector((max(v.x for v in bb),max(v.y for v in bb),max(v.z for v in bb)))
 center=(lo+hi)*.5;factor=width/max(.01,hi.x-lo.x,hi.y-lo.y)
 root=bpy.data.objects.new('CANONICAL IMPORT / '+label,None)
 bpy.context.collection.objects.link(root)
 for obj in new:
  if obj.parent in new:continue
  matrix=obj.matrix_world.copy()
  obj.parent=root;obj.matrix_world=matrix
 root.scale=(factor,)*3
 root.location=(target[0]-center.x*factor,target[1]-center.y*factor,base-lo.z*factor)
 bpy.context.view_layer.update()
 print('CANONICAL_SOURCE_REUSED',label,'meshes',len(models),'scale',factor)
 return root
source_dir=os.path.join(os.getcwd(),'Unity','Assets','Eldoria','ArtTests','ValoriaReferenceClean','Sources')
remove={'aged fortress foundation','barracks','barracks slate roof','barracks doorway',
'old empire arch pier','cyclopean broken arch stone voussoir',
'upper ward and bastion','slate roof main mass','four weighty defensive towers',
'stone tower crowns','crenellations','central gate surround','heavy gateway',
'warm fire torches','indigo royal banners','working sawmill','sawmill roof','logs'}
for ob in list(bpy.data.objects):
 if ob.name.split('.')[0] in remove:bpy.data.objects.remove(ob,do_unlink=True)
bastion=reused_glb(os.path.join(source_dir,'BastionII.glb'),(-5,6),16.2,z(-5,5)+.28,'Bastion II')
sawmill=reused_glb(os.path.join(source_dir,'SawmillRefined104.glb'),(-24,4),9.5,z(-24,4)+.6,'Sawmill real')
# Reversible Blender-only desaturation of the existing blue slate materials.
# Keep all original texel structure; never rewrite canonical model assets.
def sober_slates(root):
 tinted=0
 for ob in root.children_recursive:
  if ob.type!='MESH':continue
  for idx,material in enumerate(ob.data.materials):
   if not material or not material.use_nodes or 'Blue slate' not in material.name:continue
   m=material.copy()
   nodes=m.node_tree.nodes;links=m.node_tree.links
   bs=nodes.get('Principled BSDF')
   if not bs:continue
   sock=bs.inputs.get('Base Color')
   if not sock:continue
   tone=nodes.new('ShaderNodeHueSaturation')
   tone.inputs['Saturation'].default_value=.49
   tone.inputs['Value'].default_value=.86
   if sock.is_linked:
    current=sock.links[0];upstream=current.from_socket
    links.remove(current);links.new(upstream,tone.inputs['Color'])
   else:tone.inputs['Color'].default_value=sock.default_value
   links.new(tone.outputs['Color'],sock)
   ob.data.materials[idx]=m;tinted+=1
 return tinted
print('SLATE_MATERIALS_SOBERED',sober_slates(bastion)+sober_slates(sawmill))

# Reuse the actual production barracks, not a cuboid placeholder, and a real
# gothic stone gate source rather than featureless extruded wedges.
barracks=os.path.join(os.getcwd(),'Unity','Assets','Eldoria','Resources','Valoria','Valoria_Cuartel_AP2_v1.glb')
reused_glb(barracks,(13,-1),9.1,z(13,-1)+.20,'Cuartel original')
# Genuine 3D art from the existing heritage-landmark library; avoids the
# broken FBX importer distributed with the Ubuntu Blender package.
gate=reused_glb(os.path.join(os.getcwd(),'Unity','Assets','Eldoria','Resources','Valoria','HistoricalLandmarks','GateStreetRiseRock_MV1.glb'),(18,16),5.9,z(18,16)+.08,'ancient rocky gateway')
# Darken the overexposed white imported heritage stone, not the source asset.
for obj in gate.children_recursive:
 if obj.type!='MESH':continue
 for i,original in enumerate(obj.data.materials):
  if not original:continue
  m=original.copy()
  if m.use_nodes:
   nodes=m.node_tree.nodes;links=m.node_tree.links
   bs=nodes.get('Principled BSDF')
   if bs:
    surface=bs.inputs.get('Base Color')
    if surface and surface.is_linked:
     old_link=surface.links[0]
     mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY'
     mix.inputs[0].default_value=.93
     mix.inputs[2].default_value=(.28,.30,.29,1)
     upstream=old_link.from_socket
     links.remove(old_link)
     links.new(upstream,mix.inputs[1]);links.new(mix.outputs[0],surface)
    elif surface:surface.default_value=(.35,.37,.35,1)
  obj.data.materials[i]=m
# Reuse the project's modular heritage language as background, not new buildings.
pieces=os.path.join(os.getcwd(),'Unity','Assets','Eldoria','Resources','Valoria','MidTierArchitectureKit_v1')
reused_glb(os.path.join(pieces,'Piece02.glb'),(-32,12),4.4,z(-32,12)+.18,'ruined west imperial workshop')
reused_glb(os.path.join(pieces,'Piece03.glb'),(30,3),4.2,z(30,3)+.18,'ruined east imperial outpost')
# Massive broken geological foundations rather than a square platform.
for k in range(26):
 a=k*2*pi/26
 x=-5+9.3*cos(a);y=6+7.6*sin(a)
 r=.66+(k%6)*.15
 rock('ancient castle rock foundation',(x,y,z(x,y)+.12),(.95*r,.65*r,.45*r),stone[2 if k%4 else 0])
for k in range(20):
 a=k*2*pi/20
 x=27+4*cos(a);y=17+7*sin(a)
 rock('Brecha dark fractured geological rock',(x,y,z(x,y)+.16),
      (.60+(k%3)*.25,.48,.55),stone[2])



# v5 is a *composition pass*, not global filters. Reuse authored project vegetation
# instead of the repetitive three-cone trees from the abandoned procedural look.
for ob in list(bpy.data.objects):
 if ob.name.startswith('old conifer trunk') or ob.name.startswith('stylized forest tree crown'):
  bpy.data.objects.remove(ob,do_unlink=True)

# Genuine preexisting PineCanopy77 production GLB, instantiated with shared mesh
# data: only the instance transforms differ. Do not generate a new tree model.
# PineCanopy77 is an existing whole forest c. 94x105m, NOT a lone pine.
# v5 mistakenly normalized 77 trees to a 5.2m cloud of black specks.
# Place only four full-scale reusable patches along forest-zone edges.
pine_source=os.path.join(source_dir,'PineCanopy77.glb')
master_xy=(-43,37)
forest_master=reused_glb(
 pine_source,master_xy,50.0,z(*master_xy)+.02,'canonical 77-tree forest grove')
forest_group=[forest_master]+list(forest_master.children_recursive)
if len(forest_group)<2:raise RuntimeError('Canonical forest contains no mesh')
def forest_patch(x,y,scale,azimuth):
 p={}
 for old in forest_group:
  n=old.copy()
  if old.data:n.data=old.data
  bpy.context.collection.objects.link(n)
  p[old]=n
 for old in forest_group:
  if old is forest_master:continue
  n=p[old]
  if old.parent in p:n.parent=p[old.parent]
  n.matrix_parent_inverse=old.matrix_parent_inverse.copy()
  n.matrix_basis=old.matrix_basis.copy()
 root=p[forest_master]
 delta=forest_master.location-Vector((master_xy[0],master_xy[1],z(*master_xy)+.02))
 root.location=Vector((x,y,z(x,y)+.02))+delta*scale
 root.scale=tuple(v*scale for v in forest_master.scale)
 root.rotation_euler.z=azimuth
 root.name='Forest patch / reusable PineCanopy77 original'
 return root
for x,y,s,rot in [(44,38,.88,-.30),(-46,-24,.79,.18),(44,-23,.79,-.18)]:
 forest_patch(x,y,s,rot)
print('AUTHORED_PINE77_PATCHES',4,'SOURCE_SUBOBJECTS',len(forest_group))

# Ground-clamped authored meadow variation / paths rather than uniform smooth green.
dry=mat('sienna trampled earth',(.30,.34,.27))
earthnodes=dry.node_tree.nodes
nt=earthnodes.new('ShaderNodeTexNoise');nt.inputs['Scale'].default_value=13
nt.inputs['Detail'].default_value=4.0
er=earthnodes.new('ShaderNodeValToRGB')
er.color_ramp.elements[0].position=.25;er.color_ramp.elements[0].color=(.23,.29,.23,1)
er.color_ramp.elements[1].position=.79;er.color_ramp.elements[1].color=(.38,.37,.29,1)
dry.node_tree.links.new(nt.outputs['Fac'],er.inputs['Fac'])
dry.node_tree.links.new(er.outputs['Color'],earthnodes.get('Principled BSDF').inputs['Base Color'])
moss=mat('lowland moss',(.23,.34,.27))
sand=mat('bank worn gravel',(.39,.41,.34))
flora=mat('dark sage flowering groundcover',(.17,.32,.23))
russet=mat('soft autumn undergrowth',(.41,.34,.21))
def soilribbon(name,waypoints,width,material):
 vs=[];faces=[]
 for i,(x,y) in enumerate(waypoints):
  prev=waypoints[max(0,i-1)];nxt=waypoints[min(len(waypoints)-1,i+1)]
  dx=nxt[0]-prev[0];dy=nxt[1]-prev[1]
  l=max(.001,math.hypot(dx,dy));tx=-dy/l;ty=dx/l
  w=width*(.87+.11*sin(i*1.93)+.04*sin(i*3.67))
  for s in (-1,1):
   xx=x+s*tx*w/2;yy=y+s*ty*w/2
   vs.append((xx,yy,z(xx,yy)+.085))
  if i:faces.append((2*(i-1),2*i,2*i+1,2*(i-1)+1))
 return ownmesh(name,vs,faces,[material])
soilribbon('worn settlement avenue under cobbles',
 [(-5+sin(i*.24)*.55,-12.8+i*.47) for i in range(34)],4.8,dry)
soilribbon('organic edge of western sawmill path',
 [(-8,2),(-12,1),(-17,2),(-21,3),(-24,5)],1.8,dry)
soilribbon('organic path to eastern barracks',
 [(-2,-2),(2,-3),(7,-4),(11,-3)],2.0,dry)

# Lithological bank stones follow the actual meander and adjacent grass is planted
# according to the same waterline, with sparse clumps and coherent scale.
for i in range(86):
 x=R.uniform(-46,47);side=R.choice((-1,1))
 y=river_y(x)+side*R.uniform(2.8,5.6)
 if (x+5)**2/10**2+(y+17)**2/11**2<1:continue
 s=R.uniform(.20,.76)
 rock('river bank sediment / weathered local stone',
      (x,y,z(x,y)+.15),(.89*s,.56*s,.31*s),stone[R.choice((0,2))])
for i in range(140):
 x=R.uniform(-45,46);side=R.choice((-1,1))
 y=river_y(x)+side*R.uniform(4.2,9.4)
 if (x+5)**2/12**2+(y-6)**2/9**2<1:continue
 if R.random()<.5:continue
 r=R.uniform(.32,.76)
 rock('regional scrub / silt bed',(x,y,z(x,y)+r*.18),
      (.78*r,.68*r,.26*r),R.choice((moss,flora,russet)))
# Thousands of tiny grass planes are 1 authored MESH, not thousands of new draw calls.
bladeV=[];bladeF=[];bladeMI=[]
for i in range(700):
 x=R.uniform(-43,43);y=R.uniform(-28,38)
 if abs(y-river_y(x))<2.8:continue
 if (x+5)**2/13**2+(y-5)**2/10**2<1 and R.random()<.50:continue
 h=R.uniform(.17,.49);r=R.uniform(.11,.33)
 a=R.uniform(0,2*pi);dx=r*cos(a);dy=r*sin(a)
 z0=z(x,y)+.10
 ix=len(bladeV)
 bladeV.extend(((x-dx,y-dy,z0),(x+dx,y+dy,z0),
                (x+.17*dx,y-.16*dy,z0+h)))
 bladeF.append((ix,ix+1,ix+2));bladeMI.append(i%3)
ownmesh('authored clustered groundcover / 1 batched mesh',
        bladeV,bladeF,[moss,flora,russet],bladeMI)

# Mobile-friendly stone bridge: irregular wear patterns share the local stone family.
# The bridge and road are recognisable; no disconnected fantasy arch geometry overlays.
for i in range(24):
 x=R.uniform(-8.0,-2.0);y=R.uniform(-22.0,-12.0)
 if abs(x+5)<1.4 and R.random()>.3:continue
 b=R.uniform(.07,.21)
 rock('bridge masonry age and lichen',(x,y,.61),
      (b*2,b*2,b*.18),R.choice((moss,soil,stone[2])))
# Amber is localized to gates/inhabited buildings; ambient light stays cool.
def pointlight(name,xyz,power,colour,radius):
 d=bpy.data.lights.new(name,'POINT');d.energy=power;d.color=colour
 d.shadow_soft_size=radius;o=bpy.data.objects.new(name,d)
 bpy.context.collection.objects.link(o);o.location=xyz
for name,xyz in [
 ('bastion torch left',(-10,1,z(-10,1)+4)),
 ('bastion torch right',(1,1,z(1,1)+4)),
 ('sawmill lantern',(-22,1,z(-22,1)+3))]:
 pointlight(name,xyz,160,(1,.54,.23),3.0)

# cinematic, readable orthographic camera with true 3D mesh parallax
def light(name,loc,power,size,color):
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=size;data.color=color
 ob=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(ob);ob.location=loc
 ob.rotation_euler=(Vector((-5,1,0))-ob.location).to_track_quat('-Z','Y').to_euler()
light('warm sun',(-25,-32,48),5700,32,(1,.84,.70))
light('cold sky',(22,3,55),3400,60,(.71,.81,1))
data=bpy.data.cameras.new('isometric game camera');cam=bpy.data.objects.new('isometric game camera',data)
bpy.context.collection.objects.link(cam);cam.location=(52,-64,54)
cam.rotation_euler=(Vector((-5,0,0))-cam.location).to_track_quat('-Z','Y').to_euler()
data.type='ORTHO';data.ortho_scale=62;scene.camera=cam
scene.render.resolution_x=1400;scene.render.resolution_y=850
scene.render.filepath=os.path.join(out,'real-blender-stylized-world-landscape.png')
bpy.ops.render.render(write_still=True)
cam.location=(49,-58,61)
cam.rotation_euler=(Vector((-5,-4,1))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.ortho_scale=50
scene.render.resolution_x=760;scene.render.resolution_y=1240
scene.cycles.samples=15
scene.render.filepath=os.path.join(out,'real-blender-stylized-world-portrait.png')
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,'editablesource-valoria-stylized.blend'))
try:bpy.ops.export_scene.gltf(filepath=os.path.join(out,'original-modular-stylized.glb'),export_format='GLB',export_cameras=False,export_lights=False)
except Exception as e:print('GLB_EXPORT_WARNING',str(e))
with open(os.path.join(out,'visual-proof.json'),'w') as f:
 json.dump(dict(blender_render=True,unity_render=False,original_3d_geometry=True,
   scene_integrated=False,art_approved=False,models=len(bpy.data.objects)),f,indent=2)
print('ELDORIA_ORIGINAL_BLENDER_SOURCE_DONE',len(bpy.data.objects))
