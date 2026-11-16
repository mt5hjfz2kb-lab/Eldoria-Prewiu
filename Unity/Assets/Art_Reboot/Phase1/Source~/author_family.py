"""Phase1 family derived from the owner-approved hero; never rewrites approved hero assets."""
import bpy,math,random,json,sys,hashlib
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[1]
mode=next((v for v in ['ruin','corrupt','environment'] if '--'+v in sys.argv),'ruin')
family_out=ROOT/'Source~/Family';family_out.mkdir(exist_ok=True)
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

removed=[];reused=[]
if mode=='ruin':
 originals=list(o for o in bpy.context.scene.objects if o.type=='MESH')
 rubble=[];fallen_bell=None
 for o in originals:
  c=center(o);x,y,z=c;name=o.name
  tower=x>1.65 and y>-.35
  remove=False
  if 'Recessed dark mortar core' in name:remove=True
  if 'Projecting upper timber hall' in name or 'Continuous upper timber band' in name:remove=True
  if tower and z>4.85+.27*math.sin(x*3+y*4):remove=True
  roof_part=any(s in name for s in ['Overlapping bowed slate','Sagging heavy eave','Carved curved bargeboard','Curved timber gable','Heavy ridge cap'])
  if roof_part and z<7:
   keep=(x<-.95 and y>-.35) or (x<-2.12 and y<1.4)
   if 'Curved timber gable' in name:keep=False
   remove=remove or not keep
  if any(s in name for s in ['Gable king post','Gable diagonal','Round bronze','Shield split']):remove=True
  if 'Hand-set stone' in name and z<2.2:
   if (y<-1.55 and x>-.08 and z>.50+.16*math.cos(x*5)) or (x>1.32 and x<1.9 and z>.70):remove=True
  if 'Hand-set stone' in name and x<-1.9 and y>.7 and z>4.8:remove=True
  if any(s in name for s in ['Chimney stone cap','Soot in chimney']):remove=True
  if any(s in name for s in ['Door oak plank','Door iron strap']):remove=True
  if any(s in name for s in ['Arched inset','Fitted arch wedge','Arch jamb','Window bar']):
   # Preserve the left-hand upper window as the surviving inhabited side.
   remove=not(x<-1.65 and z>2.05 and z<3.0)
  if 'Front upper oak post' in name and x>-.1:remove=True
  if 'Supporting corbel' in name and x>.1:remove=True
  if 'Balcony' in name and 'knee' not in name:remove=True
  if z>6.3:remove=True
  if 'Cast bronze bell' in name:fallen_bell=o;remove=False
  if remove:
   removed.append(name)
   if ('Hand-set stone' in name or 'Overlapping bowed slate' in name) and len(rubble)<34 and random.random()<.24:rubble.append(o)
   else:bpy.data.objects.remove(o,do_unlink=True)
 if fallen_bell:
  from mathutils import Euler
  dropped(fallen_bell,(3.55,-2.2,.25),Euler((.9,.5,.15)));reused.append('original bronze bell')
 from mathutils import Euler
 for i,o in enumerate(rubble):
  a=random.uniform(-2.7,.6);r=random.uniform(1.3,3.1)
  dropped(o,(1.4+math.cos(a)*r,-.65+math.sin(a)*r,.1),Euler((random.uniform(-.4,.4),random.uniform(-.5,.5),random.uniform(-2,2))))
  reused.append(o.name)
 # Dark interior floor and short surviving wall panels expose actual interior, not a solid box.
 for j in range(5):
  for i in range(6):box('Exposed broken interior flags',(-2.5+i*.65,-1.35+j*.64,.035),(.61,.59,.10),8,.025,random.uniform(-.05,.05))
 box('Surviving upper left infill',(-2.16,-1.98,2.51),(1.42,.22,.79),0,.05)
 for yy in [.3,1.25]:
  beam('Exposed supporting crossbeam',(-2.9,yy,2.16),(1.25,yy,2.10),.23,4)
  beam('Open roof rafter',(-3.45,yy,3.19),(-.65,yy,5.90),.19,4)
 for xx in [-2.6,-1.8,-1.0]:
  beam('Surviving floor joist',(xx,-1.4,2.1),(xx,1.65,2.13),.16,4)
 beam('Snapped ridge over opening',(-.64,.5,5.94),(-.66,1.95,5.90),.25,4)
 beam('Collapsed roof beam',(-.65,-1.95,.15),(.65,.95,2.15),.25,4)
 beam('Fallen brace',(-1.6,-.45,.14),(1.85,-1.65,.18),.21,5)
 beam('Splintered upright',(-.1,.48,2.15),(-.14,.51,3.0),.17,4)
 # Jagged broken tower crown is outlined by displaced source-like stones, no clean cut cap.
 for i in range(7):
  a=i*math.tau/7
  rock((2.7+.68*math.cos(a),.85+.70*math.sin(a),4.90+random.uniform(-.1,.18)),(.30,.25,.23),0)
 for a,b in [((-3.4,-2.4,.18),(-4.7,-3.1,.15)),((-3.9,.3,.15),(-5.2,1.2,.18))]:beam('Story fallen beam',a,b,.23,4)

if mode=='corrupt':
 originals=list(o for o in bpy.context.scene.objects if o.type=='MESH')
 for o in originals:
  x,y,z=center(o);name=o.name
  if 'Hand-set stone' in name and x>1.8 and y<.1 and z>1.6 and abs(x-(3.3-.11*z))<.26:
   removed.append(name);bpy.data.objects.remove(o,do_unlink=True);continue
  if 'Overlapping bowed slate' in name and ((1.25<x<2.15 and -.2<y<1.3 and z<6) or (2.1<x<2.8 and .0<y<1.1 and z>8.5)):
   removed.append(name);bpy.data.objects.remove(o,do_unlink=True);continue
  if x>1.5 or (x>.5 and z>3.4):tint_object(o,(.73,.78,.94))
 def crystal(base,tip,r):
  # Six irregular sides, embedded root below actual wall/roof skin.
  base=Vector(base);tip=Vector(tip);axis=(tip-base).normalized();u=axis.cross(Vector((0,1,0))).normalized();v=axis.cross(u)
  vv=[]
  for i in range(6):
   a=i*math.tau/6;vv.append(tuple(base+(u*math.cos(a)+v*math.sin(a))*r*random.uniform(.82,1.12)))
  vv.append(tuple(tip));ff=[tuple(range(5,-1,-1))]+[(i,(i+1)%6,6) for i in range(6)]
  o=mesh('Embedded violet crystal',vv,ff,13,glow=True)
  for c in o.data.color_attributes['Color'].data:c.color=(.70,.55,1,1)
  return o
 def crack(points,width=.055,ground=False):
  # Dark fracture + narrow luminous interior, follows actual surface rather than a hovering beam.
  for a,b in zip(points[:-1],points[1:]):
   a=Vector(a);b=Vector(b);delta=b-a
   side=(Vector((-delta.y,delta.x,0)) if ground else Vector((delta.z,0,-delta.x))).normalized()
   for mult,k,glow in [(2.0,14,False),(1,13,True)]:
    off=Vector((0,0,.008 if glow else 0)) if ground else Vector((0,-.008 if glow else 0,0))
    vv=[tuple(a-side*width*mult+off),tuple(a+side*width*mult+off),tuple(b+side*width*mult+off),tuple(b-side*width*mult+off)]
    mesh('Luminous structural fracture' if glow else 'Dark structural split',vv,[(3,2,1,0) if ground else (0,1,2,3)],k,glow=glow)
 crack([(3.35,-.23,.2),(3.1,-.23,1.45),(3.27,-.18,2.4),(2.78,-.17,3.3),(2.93,-.13,4.45),(2.55,-.10,5.65)],.045)
 crack([(3.24,-.23,2.4),(2.5,-.23,2.7),(2.15,-.23,2.65)],.03)
 for b,t,r in [((2.9,-.10,2.45),(2.15,-1.1,4.30),.36),((3.18,-.1,2.15),(3.85,-.75,3.50),.25),((1.6,.6,3.95),(.95,.28,6.1),.42),((1.95,.8,4.0),(2.4,1.1,5.45),.26),((2.45,.65,8.9),(2.05,.4,10.85),.30),((2.8,.9,9.1),(3.4,1.0,10.3),.23),((3.7,-.2,.0),(4.2,-.6,2.25),.46),((3.4,-.3,0),(2.8,-.8,1.35),.29)]:crystal(b,t,r)
 for angle,length in [(-1.4,3.8),(-2.5,3.0),(.2,2.3)]:
  points=[(3.4,-.2,.02)]
  for i in range(1,5):points.append((3.4+math.cos(angle)*length*i/4+random.uniform(-.15,.15),-.2+math.sin(angle)*length*i/4+random.uniform(-.13,.13),.02))
  crack(points,.035,True)

if mode=='environment':
 current='Environment'
 # Continuous ground: three grass washes, dirt aprons and branching approaches.
 placements=[(0,7),( -10,-2),(10,0)]
 n=65;verts=[];faces=[]
 for j in range(n):
  for i in range(n):
   x=(i-32)*.75;y=(j-32)*.75
   if i in (0,64):x=-72 if i==0 else 72
   if j in (0,64):y=-72 if j==0 else 72
   z=-.12+.035*math.sin(x*.55)*math.cos(y*.48)
   verts.append((x,y,z))
 for j in range(n-1):
  for i in range(n-1):a=j*n+i;faces.append((a,a+1,a+n+1,a+n))
 terrain=mesh('Continuous three-state clearing',verts,faces,7)
 for lp in terrain.data.loops:
  v=terrain.data.vertices[lp.vertex_index].co
  wave=.5+.26*math.sin(v.x*.39+math.sin(v.y*.43))+.22*math.cos(v.y*.50-v.x*.18)
  color=Vector((.55,.78,.50)).lerp(Vector((1.0,1.0,.85)),max(0,min(1,wave)))
  dirt=math.exp(-v.x*v.x/2.2)*max(0,min(1,(8-v.y)/4))
  for xx,yy in placements:
   edge=max(abs(v.x-xx)/4.35,abs(v.y-yy)/2.8);dirt=max(dirt,.91*math.exp(-max(0,edge-.9)**2*3))
  color=color.lerp(Vector((1.0,.72,.45)),min(.95,dirt))
  terrain.data.color_attributes['Color'].data[lp.index].color=(*color,1)
  terrain.data.uv_layers.active.data[lp.index].uv=((3+(6+(max(0,min(1,v.x/48+.5)))*52)/64)/4,(1+(6+(max(0,min(1,v.y/48+.5)))*52)/64)/4)
 def paving(a,b,broken=False):
  a=Vector(a);b=Vector(b);delta=b-a;num=max(1,int(delta.length/.82));perp=Vector((-delta.y,delta.x,0)).normalized()
  for i in range(num):
   p=a+delta*i/num
   for lane in [-1,0,1]:
    if broken and random.random()<.27:continue
    q=p+perp*(lane*.55)+Vector((random.uniform(-.06,.06),random.uniform(-.06,.06),0))
    box('Irregular approach flagstone',q,(random.uniform(.43,.57),random.uniform(.61,.78),.11),9,.045,math.atan2(delta.y,delta.x)-math.pi/2+random.uniform(-.13,.13))
 paving((-.55,-15,-.03),(-.55,4.4,-.03))
 paving((-.55,-7,-.03),(-10.55,-4.9,-.03),True)
 paving((-.55,-6,-.03),(9.45,-2.9,-.03),True)
 def pine(x,y,h):
  beam('Bent pine trunk',(x,y,-.08),(x+.08,y,h),.16,4)
  for j in range(5):
   z=h*(.30+.12*j);r=h*(.25-.035*j);vv=[(x+.03*j,y,z+h*.28)]
   for i in range(14):
    a=i*math.tau/14;rr=r*(.75 if i%2==0 else 1)*random.uniform(.90,1.06)
    vv.append((x+math.cos(a)*rr,y+math.sin(a)*rr,z-random.uniform(.05,.15)))
   ff=[(0,i+1,(i+1)%14+1) for i in range(14)]+[tuple(range(14,0,-1))]
   mesh('Low irregular pine canopy',vv,ff,6 if j%2 else 7)
 for x,y,h in [(-16,4,3.8),(-16,9,4.5),(-12,13,3.5),(-7,15,4.3),(-3,17,3.1),(5,17,4.0),(10,15,3.8),(16,10,4.4),(17,5,3.5),(18,-1,2.8),(-17,-6,2.5)]:pine(x,y,h)
 for i in range(55):
  x=random.uniform(-18,18);y=random.uniform(-12,17)
  if any(abs(x-xx)<5 and abs(y-yy)<3.5 for xx,yy in placements) or abs(x)<1.8:continue
  rock((x,y,-.03),(random.uniform(.18,.55),random.uniform(.15,.45),random.uniform(.08,.3)),7 if i%3 else 2)
 for a,b in [((-15,-5,.20),(-12.8,-6.1,.19)),((-15.5,-1,.17),(-14.2,1.4,.20))]:
  beam('Fallen pine by ruin',a,b,.27,4)
  for i in [0.3,.7]:
   p=Vector(a).lerp(Vector(b),i);beam('Broken pine branch',p,p+Vector((.3,.5,.16)),.08,5)
 bpy.ops.mesh.primitive_cylinder_add(vertices=9,radius=.31,depth=.45,location=(-14.3,-3.9,.15));finish(bpy.context.object,4,.025)
 # Two emissive torches for the inhabited hero; the scene supplies one shared point light.
 for x in [-1.65,.60]:
  beam('Living hall torch post',(x,4.75,.02),(x,4.75,1.82),.11,4)
  rock((x,4.75,1.89),(.13,.13,.25),12).data.materials[0]=materials[1]

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
fbx=ROOT/('Models/Reboot_'+mode.capitalize()+'.fbx')
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True,use_triangles=True)
gltf=dict(filepath=str(OUT/(mode+'.glb')),export_format='GLB',use_selection=True,export_yup=True)
if 'export_all_vertex_colors' in bpy.ops.export_scene.gltf.get_rna_type().properties:gltf['export_all_vertex_colors']=True
bpy.ops.export_scene.gltf(**gltf)
tri=sum(len(p.vertices)-2 for p in model.data.polygons)
(OUT/(mode+'.json')).write_text(json.dumps({'mode':mode,'triangles':tri,'source_parts':source_parts,'removed_parts':len(removed),'reused_fallen_parts':len(reused),'approved_author_sha256':source_hash,'approved_hero_fbx_sha256':hero_hash,'unity_verified':False},indent=2))
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
image.pack();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(mode+'.blend')),compress=True)
if '--skip-render' not in sys.argv:bpy.ops.render.render(write_still=True)
print('FAMILY_GEOMETRY_COMPLETE',mode,tri)
