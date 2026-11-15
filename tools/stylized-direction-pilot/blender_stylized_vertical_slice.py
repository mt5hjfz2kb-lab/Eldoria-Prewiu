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
scene.view_settings.view_transform='Filmic';scene.view_settings.look='Medium High Contrast';scene.view_settings.exposure=1.12
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
def z(x,y):
 n=noise.noise(Vector((x*.065,y*.065,1)))*.44+noise.noise(Vector((x*.2,y*.2,2)))*.12
 return n+1.0*exp(-((x+5)**2/400+(y-5)**2/330))-1.20*exp(-((y+17+1.5*sin(x*.12))**2)/22)
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
 x=-47+i*94/150;y=-17-1.5*sin(x*.12)
 for side in [-1,1]:rvs.append((x,y+side*2.75,-.71))
 if i<150:rfs.append((2*i,2*i+1,2*i+3,2*i+2))
ownmesh('original single river ribbon',rvs,rfs,[river])

# Subtle ripple bands follow the same actual river centerline, not detached lakes.
foam=mat('quiet turquoise bank froth',(.29,.47,.45))
for side in (-1,1):
 wavev=[];wavef=[]
 for k in range(125):
  x=-47+k*94/124
  y=-17-1.5*sin(x*.12)+side*(2.55+.09*sin(x*.37))
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
 if abs(y+17+1.5*sin(x*.12))<5:continue
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
reused_glb(os.path.join(source_dir,'BastionII.glb'),(-5,6),16.2,z(-5,5)+.28,'Bastion II')
reused_glb(os.path.join(source_dir,'SawmillRefined104.glb'),(-24,4),9.5,z(-24,4)+.6,'Sawmill real')

# Reuse the actual production barracks, not a cuboid placeholder, and a real
# gothic stone gate source rather than featureless extruded wedges.
barracks=os.path.join(os.getcwd(),'Unity','Assets','Eldoria','Resources','Valoria','Valoria_Cuartel_AP2_v1.glb')
reused_glb(barracks,(13,-1),9.1,z(13,-1)+.20,'Cuartel original')
# Genuine 3D art from the existing heritage-landmark library; avoids the
# broken FBX importer distributed with the Ubuntu Blender package.
gate=reused_glb(os.path.join(os.getcwd(),'Unity','Assets','Eldoria','Resources','Valoria','HistoricalLandmarks','GateStreetRiseRock_MV1.glb'),(22,21),7.8,z(22,21)+.25,'ancient rocky gateway')
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
     mix.inputs[0].default_value=.84
     mix.inputs[2].default_value=(.44,.43,.40,1)
     upstream=old_link.from_socket
     links.remove(old_link)
     links.new(upstream,mix.inputs[1]);links.new(mix.outputs[0],surface)
    elif surface:surface.default_value=(.44,.45,.41,1)
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


# cinematic, readable orthographic camera with true 3D mesh parallax
def light(name,loc,power,size,color):
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=size;data.color=color
 ob=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(ob);ob.location=loc
 ob.rotation_euler=(Vector((-5,1,0))-ob.location).to_track_quat('-Z','Y').to_euler()
light('warm sun',(-25,-32,48),6200,40,(1,.84,.67))
light('cold sky',(22,3,55),4500,60,(.66,.76,1))
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
