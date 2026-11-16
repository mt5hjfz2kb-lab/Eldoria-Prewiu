"""Casa de la Campana: original mesh design, separate review asset. No pilot scene changes."""
import bpy,math,random,json,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Source~/HeroReview';OUT.mkdir(exist_ok=True)
# Reuse proven mesh/material/UV functions, not previous building geometry.
src=(ROOT/'Source~/author_pilot.py').read_text()
header=src[:src.index('# Organic continuous ground:')]
header=header.replace("image.filepath_raw=str(ROOT/'Textures/Reboot_Atlas_256.png')","image.filepath_raw=str(ROOT/'Textures/Hero_Atlas_256.png')")
header=header.replace("(112,115,101),(145,143,120),(77,83,77),(104,118,133)","(133,133,119),(167,158,133),(68,71,68),(91,115,126)")
header=header.replace("wash=.96+", "wash=.96+")
# Dark painted edge wash inside every atlas swatch, preserving modest wood grain.
header=header.replace("grain=random.uniform(-.018,.018)","grain=random.uniform(-.012,.012)\n  if k in (0,1,2,3): wash*=.70+.30*min(1,min(u-3,60-u,v-3,60-v)/8)")
exec(compile(header,'reused-authoring-primitives','exec'),globals())
random.seed(161126);current='Hero'
def tint_object(o,rgb):
 for c in o.data.color_attributes['Color'].data:c.color=(*[c.color[i]*rgb[i] for i in range(3)],1)
 return o
def solid(name,verts,faces,k,b=.045):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
 # Ensure outward normals before export.
 bpy.context.view_layer.objects.active=o;o.select_set(True)
 import bmesh
 bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()
 return finish(o,k,b)
def tapered(name,x,y,z,w,d,h,k=0,t=.93):
 vs=[]
 for level,scale in [(0,1),(h,t)]:
  for xx,yy in [(-1,-1),(1,-1),(1,1),(-1,1)]:
   vs.append((x+xx*w/2*scale+random.uniform(-.025,.025),y+yy*d/2*scale+random.uniform(-.025,.025),z+level+random.uniform(-.012,.012)))
 return solid(name,vs,[(0,3,2,1),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(4,5,6,7)],k,.055)
def slab(name,top,k,thick=.10):
 return solid(name,top+[(x,y,z-thick) for x,y,z in top],[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],k,.025)
def stone_wall(x,y,z,w,d,h):
 tapered('Recessed dark mortar core',x,y,z,w-.10,d-.10,h,2)
 # Broad, staggered irregular courses; widths and row heights differ, no square grid.
 zz=z
 row=0
 while zz<z+h-.08:
  rh=min(random.uniform(.35,.55),z+h-zz);t=1-.07*(zz-z)/h
  for side in range(4):
   length=(w if side%2==0 else d)*t;cursor=-length/2
   while cursor<length/2-.05:
    bw=min(random.uniform(.48,1.12),length/2-cursor);mid=cursor+bw/2
    if side==0:p=(x+mid,y-d*t/2,zz+rh/2);s=(bw-.028,.21,rh-.026)
    elif side==1:p=(x+w*t/2,y+mid,zz+rh/2);s=(.21,bw-.028,rh-.026)
    elif side==2:p=(x+mid,y+d*t/2,zz+rh/2);s=(bw-.028,.21,rh-.026)
    else:p=(x-w*t/2,y+mid,zz+rh/2);s=(.21,bw-.028,rh-.026)
    o=box('Hand-set stone',p,s,random.choices([0,1,2],[.76,.20,.04])[0],.045,random.uniform(-.015,.015))
    for v in o.data.vertices:v.co+=Vector((random.uniform(-.015,.015),random.uniform(-.012,.012),random.uniform(-.012,.012)))
    cursor+=bw
  zz+=rh;row+=1

def curved_roof(x,y,z,w,d,h):
 # Heavy flared eaves; concave slope, undulating ridge, individually thick tiles.
 def point(side,u,v):
  return (x+side*(w/2*u)+.07*math.sin(v*3),y-d/2+v*d,z+h*max(0,1-u)**1.35-.07*math.sin(v*math.pi)+.06*math.sin(v*9)*u)
 for side in [-1,1]:
  for row in range(7):
   u0=row/7;u1=min(1.025,(row+1)/7+.025)
   for j in range(9):
    v0=max(0,(j-(row%2)*.45)/8.55);v1=min(1,(j+1-(row%2)*.45)/8.55)
    if v1<=v0:continue
    top=[point(side,u0,v0),point(side,u1,v0),point(side,u1,v1),point(side,u0,v1)]
    o=slab('Overlapping bowed slate',top,3,.11)
    tint_object(o,[(.89,.95,1.04),(1.12,1.08,1.00),(.98,1.03,1.04)][random.randrange(3)])
  for j in range(8):beam('Sagging heavy eave',point(side,1,j/8),point(side,1,(j+1)/8),.24,4)
 for v in [0,1]:
  # Broad gable is filled timber/plaster beneath roof; silhouette is not a plain triangle.
  pts=[point(-1,1,v)]+[point(-1,1-i/7,v) for i in range(1,8)]+[point(1,i/7,v) for i in range(1,8)]
  o=mesh('Curved timber gable',pts,[tuple(range(len(pts)))],5)
  sol=o.modifiers.new('Gable thickness','SOLIDIFY');sol.thickness=.09;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=sol.name)
  for side in [-1,1]:
   for i in range(7):beam('Carved curved bargeboard',point(side,i/7,v),point(side,(i+1)/7,v),.21,4)
 for j in range(9):beam('Heavy ridge cap',point(1,0,j/9),point(1,0,(j+1)/9),.23,4)

# Hall mass: wide squat battered masonry, projecting first floor, off-centre entry.
stone_wall(-.6,0,0,4.65,3.65,2.15)
tapered('Projecting upper timber hall',-.64,-.10,2.12,5.15,3.97,1.12,1,.96)
for zz in [2.14,3.18]:box('Continuous upper timber band',(-.64,-.1,zz),(5.32,4.12,.21),4,.055)
for xx in [-3.15,-1.85,-.55,.8,1.85]:
 beam('Front upper oak post',(xx,-2.10,2.14),(xx+.045,-2.03,3.28),.17,4)
for xx in [-2.7,-1.25,.2,1.4]:
 beam('Supporting corbel',(xx,-1.7,1.61),(xx,-2.12,2.13),.22,5)
for x in [-2.8,1.55]:
 tapered('Battered buttress',x,-1.8,0,.65,.92,1.9,0,.60)
 box('Sloped buttress cap',(x,-1.8,1.83),(.51,.60,.18),1,.055)
curved_roof(-.64,-.12,3.25,6.63,4.9,2.75)
# Entrance porch with bowed mini-roof; door drawn forward of masonry, deep dark arch.
arch(-.55,-1.96,.10,1.45,1.95,10)
for i in range(6):
 xx=-1.19+i*.25;hh=1.57+.23*math.sin((i+.5)/6*math.pi)
 box('Door oak plank',(xx,-2.015,.16+hh/2),(.228,.11,hh),4,.022)
for z in [.62,1.33]:box('Door iron strap',(-.55,-2.09,z),(1.28,.08,.09),10,.018)
for i in range(3):box('Worn entry step',(-.55,-2.18-i*.34,.12-i*.035),(1.85,.40,.22),0,.05)
# Three windows and diagonal timbering under gable.
for xx in [-2.24,.99]:
 arch(xx,-2.19,2.20,.62,.67,12)
 box('Window bar',(xx,-2.23,2.52),(.055,.10,.52),4,.01)
beam('Gable king post',(-.64,-2.62,3.24),(-.64,-2.62,5.94),.19,4)
for xx in [-2.50,1.17]:beam('Gable diagonal',(xx,-2.60,3.28),(-.64,-2.60,4.65),.15,4)
# Round dormer medallion in front gable, wood ring and bronze emblem.
bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,radius=1,location=(-.64,-2.66,4.52));o=bpy.context.object;o.name='Round bronze gable shield';o.scale=(.43,.075,.43);finish(o,11,.01)
beam('Shield split-chevron',(-.90,-2.75,4.37),(-.64,-2.75,4.68),.07,4)
beam('Shield split-chevron',(-.64,-2.75,4.68),(-.39,-2.75,4.37),.07,4)
# Tower is deliberately offset to right/front so silhouette and belfry remain exposed.
tx,ty=2.25,.12
stone_wall(tx,ty,0,1.68,1.90,5.72)
for zz in [1.75,4.85,5.70]:box('Tower irregular belt',(tx,ty,zz),(1.78,2.0,.18),1,.035)
arch(tx,ty-1.00,3.88,.53,1.0,12)
box('Tower window mullion',(tx,ty-1.06,4.37),(.06,.07,.8),4,.012)
# Open belfry. No enclosing box: posts, four arches/struts and suspended bronze bell.
for xx in [-.73,.73]:
 for yy in [-.81,.81]:beam('Belfry upright',(tx+xx,ty+yy,5.75),(tx+xx*.92,ty+yy*.95,7.62),.21,4)
for yy in [-.84,.84]:
 beam('Belfry lintel',(tx-.83,ty+yy,7.54),(tx+.82,ty+yy,7.54),.22,5)
 for side in [-1,1]:beam('Belfry angle brace',(tx+side*.70,ty+yy,6.80),(tx+side*.22,ty+yy,7.54),.14,4)
beam('Bell yoke',(tx-.60,ty-.56,7.08),(tx+.58,ty-.56,7.08),.17,4)
# Turned bell profile, hollow mouth visually dark, bevelled lip.
verts=[];faces=[];profile=[(.18,7.03),(.26,6.88),(.29,6.55),(.43,6.36),(.46,6.30),(.35,6.32)]
for rad,z in profile:
 for i in range(16):a=i*math.tau/16;verts.append((tx+rad*math.cos(a),ty-.56+rad*math.sin(a),z))
for j in range(len(profile)-1):
 for i in range(16):a=j*16+i;b=j*16+(i+1)%16;faces.append((a,b,b+16,a+16))
solid('Cast bronze bell',verts,faces,11,.02)
beam('Bell clapper',(tx,ty-.56,6.65),(tx+.02,ty-.56,6.23),.09,10)
curved_roof(tx,ty,7.63,2.35,2.35,1.13)
# Cantilever balcony, separate light silhouette, not a platform under building.
box('Balcony floor',(3.25,-.22,2.45),(1.65,1.76,.18),4,.04)
for yy in [-1.0,.54]:
 for xx in [2.7,3.3,4.0]:beam('Balcony baluster',(xx,yy,2.45),(xx,yy,3.13),.09,4)
 beam('Balcony handrail',(2.65,yy,3.16),(4.05,yy,3.16),.15,5)
beam('Balcony outer rail',(4.0,-1,3.16),(4.0,.54,3.16),.15,5)
for yy in [-.8,.45]:beam('Balcony supporting knee',(2.87,yy,1.60),(3.86,yy,2.40),.20,4)
# Crooked chimney emerging behind left slope.
stone_wall(-2.30,1.12,4.20,.66,.78,1.62)
box('Chimney stone cap',(-2.30,1.12,5.85),(.90,1.0,.22),1,.05)
box('Soot in chimney',(-2.30,1.12,5.975),(.55,.65,.05),10,.01)
# Small irregular moss clusters on selected seams, not all over silhouette.
for x,y,z in [(-2.8,-1.88,.20),(-1.8,-1.9,.12),(1.5,-1.8,.14),(2.95,-.85,.20),(-2.8,-1.2,3.4)]:
 for i in range(3):rock((x+i*.10,y,z),(random.uniform(.10,.20),.08,.05),7)
# Export hero as one renderer with slots, enabling future damage variants from same editable source.
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
for o in objects:
 bpy.context.view_layer.objects.active=o
 for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 if o.data.validate(verbose=True):raise RuntimeError('Invalid mesh '+o.name)
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();hero=bpy.context.object;hero.name='Hero_Casa_Campana'
bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Models/Reboot_Hero.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True,use_triangles=True)
gltf=dict(filepath=str(OUT/'Reboot_Hero.glb'),export_format='GLB',use_selection=True,export_yup=True)
if 'export_all_vertex_colors' in bpy.ops.export_scene.gltf.get_rna_type().properties:gltf['export_all_vertex_colors']=True
bpy.ops.export_scene.gltf(**gltf)
tri=sum(len(p.vertices)-2 for p in hero.data.polygons)
(OUT/'geometry.json').write_text(json.dumps({'triangles':tri,'roof_width':6.63,'conventional_roof_width':5.1,'ratio':1.3,'camera_unity':[50,-13,0],'unity_verified':False},indent=2))
# Honest Blender lookdev, matched 50 degree inclination and 13 degree azimuth.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=False
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.32,.37,.46,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
bpy.ops.object.light_add(type='AREA',location=(-6,-9,13));bpy.context.object.data.energy=1700;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=7;bpy.context.object.rotation_euler=(Vector((0,0,3))-bpy.context.object.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=12.2
target=Vector((.25,0,3.6));az=math.radians(13);tilt=math.radians(50)
cam.location=target+Vector((math.sin(az)*math.cos(tilt),-math.cos(az)*math.cos(tilt),math.sin(tilt)))*30
cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();scene.camera=cam
scene.render.resolution_x=1100;scene.render.resolution_y=1250;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
image.pack();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Reboot_Hero.blend'),compress=True)
scene.render.filepath=str(OUT/'blender-hero.png')
if '--skip-render' not in sys.argv:bpy.ops.render.render(write_still=True)
print('HERO_GEOMETRY_COMPLETE',tri)
