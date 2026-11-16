"""Phase1 family derived from the owner-approved hero; never rewrites approved hero assets."""
import bpy,math,random,json,sys,hashlib
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[1]
mode='environment'
family_out=ROOT/'Source~/EnvironmentReview';family_out.mkdir(exist_ok=True)
base_source=(ROOT/'Source~/author_hero.py').read_text()
source_hash=hashlib.sha256(base_source.encode()).hexdigest()
hero_hash=hashlib.sha256((ROOT/'Models/Reboot_Hero.fbx').read_bytes()).hexdigest()
# Execute exactly the approved construction up to export, preserving named loose components.
# Suppress its sole disk write (atlas.save); approved texture bytes are immutable.
build=base_source[:base_source.index('# Export hero as one renderer')]
build=build.replace("exec(compile(header,'reused-authoring-primitives','exec'),globals())","header=header.replace('image.save()','None')\nexec(compile(header,'reused-authoring-primitives','exec'),globals())")
if mode=='environment':
 build=build[:build.index('# Hall mass:')]
exec(compile(build,'approved-hero-construction','exec'),globals())
OUT=family_out
random.seed({'ruin':2201,'corrupt':2202,'environment':2203}[mode]);current=mode.capitalize()
bpy.context.view_layer.update()
def center(o):
 pts=[o.matrix_world@Vector(v) for v in o.bound_box]
 return sum(pts,Vector())/8

def dropped(o,location,rotation):
 # Reuse an actual removed source component, with its real material/UVs.
 bpy.context.view_layer.update();c=center(o)
 M=Matrix.Translation(Vector(location))@rotation.to_matrix().to_4x4()@Matrix.Translation(-c)
 o.matrix_world=M@o.matrix_world
 bpy.context.view_layer.update();lo=min((o.matrix_world@Vector(v)).z for v in o.bound_box)
 o.location.z+=.025-lo
 o.name='Reused fallen / '+o.name


protected={n:hashlib.sha256((ROOT/('Models/Reboot_'+n+'.fbx')).read_bytes()).hexdigest() for n in ['Hero','Ruin','Corrupt']}
current='EnvironmentV2';random.seed(161127)
placements=[(0,4.5),(-7.2,-1.9),(7.1,.6)]
def ground(x,y):
 h=.43*math.sin(x*.22+y*.11)+.32*math.sin(y*.31-.5)+.18*math.cos(x*.48-y*.15)
 for a,b in placements:
  d=max(abs(x-a)/4.8,abs(y-b)/3.2)
  h*=min(1,max(0,(d-.95)/.65))
 return -.10+h
n=81;verts=[];faces=[]
for j in range(n):
 for i in range(n):
  x=(i-40)*.65;y=-27+j*.575
  if i in (0,80):x=-90 if i==0 else 90
  if j in (0,80):y=-27 if j==0 else 19
  verts.append((x,y,ground(x,y)))
for j in range(n-1):
 for i in range(n-1):a=j*n+i;faces.append((a,a+1,a+n+1,a+n))
terrain=mesh('Rolling clearing with flat building contact',verts,faces,7)
for lp in terrain.data.loops:
 v=terrain.data.vertices[lp.vertex_index].co
 wave=.5+.22*math.sin(v.x*.47+math.sin(v.y*.36))+.2*math.cos(v.y*.52-v.x*.24)
 greens=[Vector((.48,.70,.47)),Vector((.66,.84,.55)),Vector((.87,.96,.62)),Vector((1,.93,.68))]
 t=max(0,min(2.999,wave*3));color=greens[int(t)].lerp(greens[min(3,int(t)+1)],t-int(t))
 dirt=max(math.exp(-((v.x+.5)/1.2)**2)*max(0,min(1,(4-v.y)/3)),0)
 for xx,yy in placements:
  edge=max(abs(v.x-xx)/4.3,abs(v.y-yy)/2.6);dirt=max(dirt,.9*math.exp(-max(0,edge-.8)**2*4))
 color=color.lerp(Vector((1,.65,.38)),min(.95,dirt))
 terrain.data.color_attributes['Color'].data[lp.index].color=(*color,1)
 terrain.data.uv_layers.active.data[lp.index].uv=((3+(8+min(1,max(0,v.x/40+.5))*48)/64)/4,(1+(8+min(1,max(0,v.y/40+.5))*48)/64)/4)
def paving(a,b):
 a=Vector(a);b=Vector(b);d=b-a;side=Vector((-d.y,d.x,0)).normalized()
 for i in range(max(2,int(d.length/.65))):
  p=a+d*i/max(2,int(d.length/.65))
  for lane in [-1,0,1]:
   if random.random()<.10:continue
   q=p+side*lane*.48;q.z=ground(q.x,q.y)+.07
   box('Worn village paving',q,(random.uniform(.38,.47),random.uniform(.46,.62),.10),9,.045,math.atan2(d.y,d.x)-math.pi/2+random.uniform(-.23,.23))
paving((-.5,-8,0),(-.5,2.2,0));paving((-.5,-3.5,0),(-7.7,-4.0,0));paving((-.5,-3.1,0),(6.6,-1.7,0))
def pine(x,y,h):
 z=ground(x,y);lean=random.uniform(-.22,.22)
 beam('Weathered pine trunk',(x,y,z),(x+lean,y,h+z),.18 if h<5 else .25,4)
 for j in range(5):
  zz=z+h*(.23+.13*j);rr=h*(.25-.035*j);vv=[(x+lean,y,zz+h*.31)]
  for i in range(14):
   a=i*math.tau/14;r=rr*(.74 if i%2==0 else 1)*random.uniform(.88,1.1)
   vv.append((x+math.cos(a)*r,y+math.sin(a)*r,zz-random.uniform(.02,.22)))
  o=mesh('Varied pine crown',vv,[(0,i+1,(i+1)%14+1) for i in range(14)]+[tuple(range(14,0,-1))],7 if j%2 else 6)
  tint_object(o,(random.uniform(.68,.95),random.uniform(.80,1),random.uniform(.73,.92)))
for i in range(65):
 if i<38:x=random.uniform(-19,20);y=random.uniform(8.8,16.0)
 else:x=random.choice([-1,1])*random.uniform(14,21);y=random.uniform(-5,13)
 pine(x,y,random.uniform(3.4,7.4))
# Lower, shaded foreground framing; keep village doorways and path readable.
for x,y,h in [(-13,-7,6.2),(14,-5,5.8),(-17,-10,4.2)]:pine(x,y,h)
grass_vertices=[];grass_faces=[]
def tuft(x,y,size=1):
 z=ground(x,y)
 for k in range(random.randint(4,7)):
  a=random.uniform(0,math.tau);w=random.uniform(.045,.10)*size;h=random.uniform(.25,.62)*size
  dx,dy=math.cos(a)*w,math.sin(a)*w
  off=len(grass_vertices);grass_vertices.extend([(x-dx,y-dy,z),(x+dx,y+dy,z),(x+math.cos(a+.7)*.18*size,y+math.sin(a+.7)*.18*size,z+h)]);grass_vertices.extend(grass_vertices[-3:]);grass_faces.extend([(off,off+1,off+2),(off+5,off+4,off+3)])
  random.uniform(.60,.95);random.uniform(.72,1)
for i in range(390):
 x=random.uniform(-18,18);y=random.uniform(-11,20)
 if any(abs(x-a)<4.5 and abs(y-b)<2.7 for a,b in placements) or abs(x+.5)<1:continue
 tuft(x,y,random.uniform(.6,1.25))
for i in range(52):
 x=random.uniform(-17,17);y=random.uniform(-9,18)
 if any(abs(x-a)<4.6 and abs(y-b)<2.8 for a,b in placements):continue
 rock((x,y,ground(x,y)),(random.uniform(.18,.55),random.uniform(.16,.45),random.uniform(.12,.4)),2)
for x,y in [(-7,-8.8),(8.6,-7.5),(10,-8.2)]:
 rock((x,y,ground(x,y)),(1.05,.65,.65),2)
 for j in range(9):tuft(x+random.uniform(-1.2,1.2),y+random.uniform(-.7,.7),1.6)
beam('Foreground fallen trunk',(-10,-7.7,ground(-10,-7.7)+.25),(-7.5,-8.8,ground(-7.5,-8.8)+.25),.48,4)
for x in [-1.65,.60]:
 beam('Warm entrance torch',(x,2.25,0),(x,2.25,1.82),.11,4)
 o=rock((x,2.25,1.9),(.12,.12,.25),12);o.data.materials[0]=materials[1]
# Workshop identity is an additive, smaller side annex, preserving approved shell.
wx,wy=-7.2,-1.9
for x,y in [(wx-4.35,wy-.1),(wx-4.35,wy+2.0),(wx-2.75,wy-.1),(wx-2.75,wy+2.0)]:beam('Workshop lean-to post',(x,y,0),(x,y,2.45),.16,4)
for i in range(7):
 y=wy-.25+i*.37
 slab('Lean-to slate overhang',[(wx-4.7,y,2.35),(wx-2.45,y,3.20),(wx-2.45,y+.42,3.20),(wx-4.7,y+.42,2.35)],3,.11)
stone_wall(wx-3.6,wy+1.5,0,.95,.95,4.35)
box('Workshop chimney crown',(wx-3.6,wy+1.5,4.35),(1.2,1.2,.22),1,.05)
box('Dark workshop chimney opening',(wx-3.6,wy+1.5,4.48),(.65,.65,.04),10,.015)
box('Workbench',(wx-3.7,wy-.25,1.05),(1.3,.65,.14),4,.045)
for x in [wx-4.15,wx-3.25]:beam('Workbench leg',(x,wy-.25,.05),(x,wy-.25,1.05),.15,4)
# Wheel as a legible secondary silhouette on the workshop front.
wc=Vector((wx-3.65,wy-.65,.80));rr=.64
for i in range(12):
 a=i*math.tau/12;b=(i+1)*math.tau/12
 p=wc+Vector((math.cos(a)*rr,0,math.sin(a)*rr));q=wc+Vector((math.cos(b)*rr,0,math.sin(b)*rr));beam('Workshop wheel rim',p,q,.13,4)
 if i%2==0:beam('Wheel spoke',wc,p,.08,5)
# Extra corruption is a removable dressing layer, never overwrites approved building meshes.
cx,cy=7.1,.6
def crystal(base,tip,r):
 base=Vector(base)+Vector((cx,cy,0));tip=Vector(tip)+Vector((cx,cy,0));axis=(tip-base).normalized();u=axis.cross(Vector((0,1,0))).normalized();v=axis.cross(u);vv=[]
 for i in range(6):
  a=i*math.tau/6;vv.append(tuple(base+(u*math.cos(a)+v*math.sin(a))*r*random.uniform(.8,1.12)))
 vv.append(tuple(tip));o=mesh('Invading violet crystal',vv,[tuple(range(5,-1,-1))]+[(i,(i+1)%6,6) for i in range(6)],13,True)
 for poly in o.data.polygons:
  fac=random.uniform(.62,1)
  for li in poly.loop_indices:o.data.color_attributes['Color'].data[li].color=(fac*.85,fac*.72,fac,1)
for b,t,r in [((3.2,0,2),(4.15,-.45,5.4),.60),((2.7,.2,4.6),(1.8,-.6,7.1),.45),((2.2,.7,8.6),(1.5,.3,11.4),.43),((3.5,.5,0),(4.75,.1,3.6),.65),((3.3,-1.1,0),(2.6,-1.6,2.1),.35),((1.3,-.8,4.1),(.6,-1.4,6.2),.4),((3.6,1.1,0),(5.3,1.5,2.4),.4),((2.5,-1.6,0),(1.4,-2.0,1.25),.3),((3,-.3,5.3),(3.7,-.8,6.9),.27)]:crystal(b,t,r)
# Wide broken earth lips with jagged tapering emissive interior (not parallel neon rails).
def fissure(points,width):
 for i,(a,b) in enumerate(zip(points,points[1:])):
  a=Vector(a);b=Vector(b);d=b-a;side=Vector((-d.y,d.x,0)).normalized();w=width*(1-i/(len(points)+.2))
  for mul,k,em in [(2.8,14,False),(1,13,True)]:
   vv=[]
   for p,sgn in [(a,-1),(a,1),(b,1),(b,-1)]:
    q=p+side*w*mul*sgn*random.uniform(.65,1.3);q.z=ground(q.x,q.y)+.13+(.012 if em else 0);vv.append(q)
   mesh('Jagged invaded ground fissure',vv,[(3,2,1,0)],k,em)
  if i%2==0:rock((a.x,a.y,ground(a.x,a.y)+.08),(w*2,w*1.8,.12),14)
for ang,length in [(-1.35,4.1),(-2.6,3.8),(.35,2.8)]:
 pts=[(cx+3.35,cy-.2,0)]
 for i in range(1,8):pts.append((cx+3.35+math.cos(ang)*length*i/7+random.uniform(-.23,.23),cy-.2+math.sin(ang)*length*i/7+random.uniform(-.23,.23),0))
 fissure(pts,.14)
# Dark moss at joints, fallen slate and a visibly twisted timber brace on the invaded side.
for i in range(24):
 z=random.uniform(.3,5.3);x=cx+random.uniform(2.1,3.5);y=cy-.23
 rock((x,y,z),(random.uniform(.14,.32),.10,random.uniform(.08,.24)),14 if i%3 else 6)
for i in range(12):
 x=cx+random.uniform(2.7,5.5);y=cy+random.uniform(-2.6,.6)
 box('Fallen invaded slate',(x,y,ground(x,y)+.13),(.52,.40,.09),3,.025,random.uniform(-1,1))
pts=[(cx+3.6,cy-.4,1.8),(cx+4.1,cy-.65,2.55),(cx+3.85,cy-.7,3.3),(cx+4.35,cy-.25,3.9)]
for a,b in zip(pts,pts[1:]):beam('Twisted invaded brace',a,b,.19,4)
grass=mesh('Scattered tall grass clusters',grass_vertices,grass_faces,7)
tint_object(grass,(.78,.9,.60))
removed=[];reused=[]
bpy.context.view_layer.update()
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
source_parts=len(objects)
for o in objects:
 bpy.context.view_layer.objects.active=o
 for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 if o.data.validate(verbose=True,clean_customdata=False):raise RuntimeError('Invalid derived source '+o.name)
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();model=bpy.context.object;model.name='Family_'+mode
bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
fbx=ROOT/('Models/Reboot_EnvironmentV2.fbx')
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True,use_triangles=True)
gltf=dict(filepath=str(OUT/('environmentv2'+'.glb')),export_format='GLB',use_selection=True,export_yup=True)
if 'export_all_vertex_colors' in bpy.ops.export_scene.gltf.get_rna_type().properties:gltf['export_all_vertex_colors']=True
bpy.ops.export_scene.gltf(**gltf)
tri=sum(len(p.vertices)-2 for p in model.data.polygons)
(OUT/('environmentv2'+'.json')).write_text(json.dumps({'mode':'environmentv2','triangles':tri,'source_parts':source_parts,'removed_parts':len(removed),'reused_fallen_parts':len(reused),'approved_author_sha256':source_hash,'approved_hero_fbx_sha256':hero_hash,'approved_buildings_sha256':protected,'unity_verified':False},indent=2))
assert hashlib.sha256((ROOT/'Models/Reboot_Hero.fbx').read_bytes()).hexdigest()==hero_hash
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=20;scene.cycles.use_denoising=False
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.32,.37,.46,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.light_add(type='AREA',location=(-6,-9,13));bpy.context.object.data.energy=1700;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=7;bpy.context.object.rotation_euler=(Vector((0,0,3))-bpy.context.object.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=15 if mode!='environment' else 48
target=Vector((.2,0,3.9));az=math.radians(13);tilt=math.radians(50)
cam.location=target+Vector((math.sin(az)*math.cos(tilt),-math.cos(az)*math.cos(tilt),math.sin(tilt)))*45
cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();scene.camera=cam
scene.render.resolution_x=1000;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG';scene.render.filepath=str(OUT/(mode+'-blender.png'))
image.pack();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/('environmentv2'+'.blend')),compress=True)
if '--skip-render' not in sys.argv:bpy.ops.render.render(write_still=True)
assert all(hashlib.sha256((ROOT/('Models/Reboot_'+n+'.fbx')).read_bytes()).hexdigest()==h for n,h in protected.items())
print('ENVIRONMENT_GEOMETRY_COMPLETE',tri)
