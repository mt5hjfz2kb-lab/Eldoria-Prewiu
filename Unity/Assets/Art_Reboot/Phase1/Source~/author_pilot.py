"""Original, deterministic art direction. Blender 4.3, metres, Z up.
Run: blender -b -t 4 --python author_pilot.py
Preview is Blender evidence, never a Unity screenshot.
"""
import bpy, math, random, json, sys
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
random.seed(151126)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
palette = [(112,115,101),(145,143,120),(77,83,77),(49,63,70),
           (81,64,46),(126,98,65),(49,66,54),(73,86,63),
           (76,75,61),(103,94,72),(38,44,44),(165,142,96),
           (255,170,65),(124,62,205),(58,39,70),(31,43,43)]
# Brush-stroke atlas authored by code: broad washes, grain and directional strokes.
# It is deliberately not described as a manually painted bitmap.
image=bpy.data.images.new('Reboot_Atlas_256',width=256,height=256,alpha=True)
pix=[0.0]*(256*256*4)
for y in range(256):
 for x in range(256):
  k=(y//64)*4+x//64; base=palette[k]; u=x%64; v=y%64
  wash=.96+.045*math.sin(u*.18+math.sin(v*.11))+.025*math.sin(v*.3)
  grain=random.uniform(-.018,.018)
  if k in (4,5): wash+=.065*math.sin(u*.72+math.sin(v*.08))
  if k==3: wash+=.035*math.sin(v*.32)
  for c in range(3): pix[4*(y*256+x)+c]=min(1,max(0,base[c]/255*wash+grain))
  pix[4*(y*256+x)+3]=1
image.pixels.foreach_set(pix)
image.filepath_raw=str(ROOT/'Textures/Reboot_Atlas_256.png');image.file_format='PNG';image.save()

materials=[]
for name,emission in [('Reboot_Atlas',0),('Reboot_Light',2.4)]:
 m=bpy.data.materials.new(name);m.use_nodes=True
 n=m.node_tree.nodes; l=m.node_tree.links;p=n.get('Principled BSDF')
 t=n.new('ShaderNodeTexImage');t.image=image;t.interpolation='Linear'
 v=n.new('ShaderNodeVertexColor');v.layer_name='Color'
 mix=n.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1
 l.new(t.outputs['Color'],mix.inputs[1]);l.new(v.outputs['Color'],mix.inputs[2]);l.new(mix.outputs[0],p.inputs['Base Color'])
 p.inputs['Roughness'].default_value=.87
 if emission:l.new(mix.outputs[0],p.inputs['Emission Color']);p.inputs['Emission Strength'].default_value=emission
 materials.append(m)

groups={}; current='Terrain'
def finish(o,k,bevel=0,glow=False):
 o.name=current+' / '+o.name;groups.setdefault(current,[]).append(o)
 o.data.materials.clear();o.data.materials.append(materials[int(glow)])
 bpy.context.view_layer.objects.active=o
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Soft worn arris','BEVEL');mod.width=bevel;mod.segments=1
  bpy.ops.object.modifier_apply(modifier=mod.name)
 tint=random.uniform(.88,1.08)
 o.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 uv=o.data.uv_layers.get('UVMap') or o.data.uv_layers.new(name='UVMap')
 col=o.data.color_attributes['Color']
 for face in o.data.polygons:
  normal=face.normal;drop=max(range(3),key=lambda i:abs(normal[i]));axes=[i for i in range(3) if i!=drop]
  coords=[o.data.vertices[o.data.loops[j].vertex_index].co for j in face.loop_indices]
  lo=[min(c[a] for c in coords) for a in axes];hi=[max(c[a] for c in coords) for a in axes]
  for j,c in zip(face.loop_indices,coords):
   u=(c[axes[0]]-lo[0])/max(.001,hi[0]-lo[0]);v=(c[axes[1]]-lo[1])/max(.001,hi[1]-lo[1])
   uv.data[j].uv=((k%4+(6+u*52)/64)/4,(k//4+(6+v*52)/64)/4)
   shade=tint*(.91 if c.z<0 else 1)
   col.data[j].color=(shade,shade,shade,1)
 return o
def box(name,p,s,k=0,b=.04,rot=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=s;o.rotation_euler.z=rot
 return finish(o,k,b)
def mesh(name,verts,faces,k,glow=False):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
 return finish(o,k,glow=glow)
def beam(name,a,b,w,k=4):
 mid=(Vector(a)+Vector(b))*.5;o=box(name,mid,(w,w,(Vector(b)-Vector(a)).length),k,.025)
 o.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return o
def rock(p,s,k=2):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p);o=bpy.context.object;o.name='Weathered rock';o.scale=s
 for v in o.data.vertices:v.co*=random.uniform(.88,1.12)
 return finish(o,k)
def arch(x,y,z,w,h,k=10,depth=.06):
 # Front-facing arched inset, with individually laid voussoirs.
 r=w/2; spring=z+h-r
 poly=[(x-r,y,z),(x+r,y,z),(x+r,y,spring)]+[(x+math.cos(i*math.pi/8)*r,y,spring+math.sin(i*math.pi/8)*r) for i in range(1,9)]
 o=mesh('Arched recess',poly,[tuple(range(len(poly)))],k,glow=k in (12,13))
 for side in [-1,1]:box('Arch jamb',(x+side*(r+.10),y+.035,z+(h-r)/2),(.19,.22,h-r),1,.025)
 for i in range(9):
  a=(i+.5)*math.pi/9;rr=r+.10
  q=box('Arch voussoir',(x+math.cos(a)*rr,y+.035,spring+math.sin(a)*rr),(.20,.25,.22),1,.02)
  q.rotation_euler.y=a-math.pi/2
 return o
def masonry(x,y,z,w,d,h,ruin=False):
 # Courses are broad, not thousands of tiny bricks. Separate caps and repair stones.
 box('Heavy stone body',(x,y,z+h*.5),(w,d,h),0,.075)
 for row in range(int(h/.58)):
  count=max(2,int(w/.8));bw=w/count
  for i in range(count):
   if ruin and row>2 and i>count//2:continue
   box('Uneven facing',(x-w/2+(i+.5)*bw,y-d/2-.025,z+.27+row*.58),(bw-.035,.12,.50),1 if random.random()<.22 else 0,.028)
def roof(x,y,z,w,d,h):
 # Long ridgeline on Y; two strong sloping surfaces, thick eaves and ridge caps.
 for side in [-1,1]:
  for row in range(4):
   a=row/4;b=(row+1)/4
   for j in range(5):
    ya=y-d/2+j*d/5;yb=ya+d/5+.025
    va=[(x+side*w*.5*a,ya,z+h*(1-a)-.09*math.sin(j*.7)),(x+side*w*.5*b,ya,z+h*(1-b)-.09*math.sin(j*.7)),(x+side*w*.5*b,yb,z+h*(1-b)-.09*math.sin((j+1)*.7)),(x+side*w*.5*a,yb,z+h*(1-a)-.09*math.sin((j+1)*.7))]
    mesh('Broad slate shingle',va,[(0,1,2,3) if side > 0 else (3,2,1,0)],3)
  beam('Timber eave',(x+side*w/2,y-d/2,z),(x+side*w/2,y+d/2,z),.16,4)
 for yy in [y-d/2,y+d/2]:
  mesh('Timber gable',[(x-w/2,yy,z),(x+w/2,yy,z),(x,yy,z+h)],[(0,1,2) if yy < y else (2,1,0)],4)
  for side in [-1,1]:beam('Gable brace',(x+side*w/2,yy-.04,z),(x,yy-.04,z+h),.13,5)
 beam('Ridge beam',(x,y-d/2-.08,z+h),(x,y+d/2+.08,z+h),.16,5)

# Organic continuous ground: no floating board, no rectangular plot outlines.
verts=[];faces=[];N=65
for j in range(N):
 for i in range(N):
  x=(i-(N-1)/2)*.75;y=(j-(N-1)/2)*.75
  z=-.13+.045*math.sin(x*.6)*math.cos(y*.7)+max(0,y-5)*.06
  verts.append((x,y,z))
for j in range(N-1):
 for i in range(N-1):a=j*N+i;faces.append((a,a+1,a+N+1,a+N))
mesh('Continuous forest floor',verts,faces,6)
for i in range(90):
 x=random.uniform(-10,10);y=random.uniform(-9,10)
 if abs(x)<2 and y<3:continue
 rock((x,y,-.09),(random.uniform(.15,.65),random.uniform(.18,.65),random.uniform(.06,.23)),7 if i%3 else 8)
for i in range(10):
 y=-7+i*.86
 for j in range(3):box('Worn approach flagstone',((j-1)*.73+random.uniform(-.09,.09),y,0),(.68,.77,.13),9,.055,random.uniform(-.1,.1))

current='01_Reclaimed_Hall'
cx,cy=0,2.1
box('Foundation terrace',(cx,cy,.16),(5.4,4.5,.45),2,.11)
for j in range(4):box('Entry stair',(0,-.43-j*.36,.20-j*.055),(1.9,.4,.25),1,.04)
masonry(cx,cy,.38,4.4,3.5,2.8)
roof(cx,cy,3.25,4.95,4.1,1.8)
arch(0,.29,.45,1.12,2.15,4)
for x in [-1.38,1.38]:
 arch(x,.27,1.34,.48,1.1,12)
 box('Window mullion',(x,.20,1.85),(.06,.08,.78),4,.01)
for x in [-2.16,2.16]:
 for y in [.45,3.75]:box('Repaired buttress',(x,y,1.43),(.5,.52,2.2),2,.055)
# Short asymmetrical defensive tower attached to the hall.
masonry(-2.25,3.5,.35,1.75,1.9,4.8)
box('Tower crown',(-2.25,3.5,5.12),(2.02,2.15,.28),1,.04)
for x in [-3.02,-2.25,-1.48]:
 for y in [2.55,4.45]:box('Battlement',(x,y,5.48),(.43,.42,.52),0,.045)
arch(-2.25,2.52,3.48,.38,.95,12)
beam('Renewed banner staff',(-2.3,3.5,5.1),(-2.3,3.5,6.65),.075,4)
mesh('Ochre repaired banner',[(-2.28,3.5,6.55),(-1.25,3.47,6.47),(-1.22,3.6,5.9),(-1.73,3.5,6.02),(-2.28,3.5,5.93)],[(0,1,2,3,4),(4,3,2,1,0)],11)
for x in [-1.1,1.1]:
 box('Door hinge',(x*.4,.19,1.1),(.38,.05,.07),10,.01)
# A repair timber and mixed stone patches communicate habitation without clutter.
beam('Repair lintel',(-1.7,.13,2.78),(1.7,.13,2.78),.14,5)

current='02_Roofless_Ruin'
rx,ry=-4.55,-1.55
box('Sunken plinth',(rx,ry,.03),(3.2,3.45,.22),2,.09)
for side in [-1,1]:
 for j in range(4):
  h=[1.5,1.8,2.15,2.6][j] if side<0 else [0.5,.8,1.3,1.7][j]
  box('Broken sidewall',(rx+side*1.35,ry-1.25+j*.8,h/2),(.42,.77,h),0,.055)
  rock((rx+side*1.35,ry-1.25+j*.8,h),(.26,.35,.15),7)
box('Standing rear wall',(rx,ry+1.35,1.25),(2.8,.4,2.5),0,.06)
arch(rx,ry+1.12,1.0,.7,1.4,10)
beam('Collapsed charred rafter',(rx-1.2,ry-1.1,.3),(rx+.8,ry+1.3,1.75),.18,10)
beam('Snapped beam',(rx-.7,ry+.5,.2),(rx+1.5,ry-1.25,.5),.2,4)
for i in range(18):
 x=rx+random.uniform(-1.9,1.9);y=ry+random.uniform(-2,1)
 rock((x,y,.1),(random.uniform(.14,.4),random.uniform(.17,.37),random.uniform(.14,.3)),0 if i%3 else 7)

current='03_Corrupted_Watchtower'
tx,ty=4.35,.45
box('Fractured footing',(tx,ty,.12),(3.15,3.0,.4),2,.08)
# Two offset halves leave a real geometric rupture; violet is confined to the wound.
masonry(tx-.72,ty,.32,1.20,2.45,3.5)
masonry(tx+.73,ty+.16,.32,1.1,2.3,4.1)
mesh('Visible arcane wound',[(tx-.1,ty-1.245,.4),(tx+.17,ty-1.245,.4),(tx+.33,ty-1.245,1.8),(tx+.05,ty-1.245,2.5),(tx+.22,ty-1.245,3.8),(tx-.05,ty-1.245,3.5),(tx-.2,ty-1.245,2.5),(tx+.02,ty-1.245,1.7)],[(0,1,2,3,4,5,6,7)],13,True)
for x,z in [(tx-1.1,3.95),(tx-.45,3.95),(tx+.55,4.55),(tx+1.2,4.55)]:box('Shattered crown',(x,ty-.95,z),(.42,.47,.6),2,.035,random.uniform(-.12,.12))
for i in range(7):
 x=tx+random.uniform(-1,1);y=ty-1.4-random.random()*.65;h=random.uniform(.3,1)
 bpy.ops.mesh.primitive_cone_add(vertices=5,radius1=.14,radius2=.018,depth=h,location=(x,y,h*.5+.12));o=bpy.context.object;o.name='Arcane shard';o.rotation_euler=(.15,random.uniform(-.5,.5),0);finish(o,13,glow=True)
for i in range(8):rock((tx+random.uniform(-1.8,1.8),ty+random.uniform(-1.6,1.6),.1),(.3,.27,.22),14 if i<3 else 2)

current='Terrain'
# Stones connect the three buildings without implying new gameplay parcels.
for side in [-1,1]:
 for i in range(5):box('Branch path',(side*(1.2+i*.57),-.7-i*.13,.025),(.55,.65,.12),9,.065,random.uniform(-.2,.2))
for x,y,s in [(-7,3,1.1),(-6.7,6.2,1.4),(-3.8,7.4,1.0),(0,7.8,1.15),(4.9,6.6,1.1),(7.4,3.3,1.25),(8,-1.8,.8),(-7,-4.4,.75)]:
 beam('Pine trunk',(x,y,0),(x+.12,y,4*s),.20*s,4)
 for j in range(4):
  rad=(1.35-j*.22)*s;zc=1.6*s+j*.67*s;vv=[(x+.06*j,y,zc+1.25*s)];ff=[]
  for a in range(18):
   angle=a*math.tau/18+j*.25;rr=rad*(1 if a%2 else .65)*random.uniform(.86,1.07)
   vv.append((x+math.cos(angle)*rr,y+math.sin(angle)*rr,zc-random.uniform(.05,.27)*s))
  for a in range(18):ff.append((0,1+a,1+(a+1)%18))
  ff.append(tuple(range(18,0,-1)));mesh('Irregular pine branches',vv,ff,6 if j%2 else 7)
for x,y,s in [(-7,1,1.4),(-5.3,5.5,1.3),(5.4,4.5,1.1),(7,1,1),(-7.5,-2,.8)]:rock((x,y,.22*s),(s,s*.8,s*.65),2)

# Join into four grounded export families, not one renderer per stone.
joined=[]
for key,objects in groups.items():
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=key
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True);joined.append(o)
bpy.ops.object.select_all(action='DESELECT')
for o in joined:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Models/Reboot_Pilot.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',use_mesh_modifiers=True,use_triangles=True)
bpy.ops.export_scene.gltf(filepath=str(ROOT/'Source~/Reboot_Pilot.glb'),export_format='GLB',use_selection=True,export_yup=True,export_materials='EXPORT',export_all_vertex_colors=True)
triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in joined)
(ROOT/'Evidence/geometry.json').write_text(json.dumps({'generator':'Blender '+bpy.app.version_string,'seed':151126,'triangles':triangles,'mesh_objects':len(joined),'materials':len(materials),'atlas':[256,256],'units':'metres','pivot':'world origin, ground plane z=0','families':[o.name for o in joined],'unity_verified':False},indent=2))

# Matched strategic preview. Camera tilt is 50 degrees downward.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40
scene.cycles.use_denoising=False;scene.render.resolution_x=1100;scene.render.resolution_y=1250;scene.render.resolution_percentage=100
scene.world.color=(.18,.22,.30)
world=scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.24,.31,.44,1);world.node_tree.nodes['Background'].inputs[1].default_value=.75
bpy.ops.object.light_add(type='SUN',location=(-8,-10,12));sun=bpy.context.object;sun.name='Preview warm sunset';sun.rotation_euler=(math.radians(64),math.radians(-18),math.radians(-35));sun.data.energy=3.2;sun.data.angle=.12;sun.data.color=(1,.70,.40)
bpy.ops.object.camera_add();cam=bpy.context.object;cam.name='Strategic 50 degree camera';cam.data.type='ORTHO';cam.data.ortho_scale=20
target=Vector((0,.8,1));az=math.radians(13);tilt=math.radians(50);dist=30
cam.location=target+Vector((math.sin(az)*math.cos(tilt),-math.cos(az)*math.cos(tilt),math.sin(tilt)))*dist
cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();scene.camera=cam
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast';scene.view_settings.exposure=.4
scene.view_layers[0].use_pass_mist=True;world.mist_settings.start=23;world.mist_settings.depth=32;world.mist_settings.falloff='LINEAR'
scene.use_nodes=True;nt=scene.node_tree;nt.nodes.clear();rl=nt.nodes.new('CompositorNodeRLayers');mix=nt.nodes.new('CompositorNodeMixRGB');mix.inputs[2].default_value=(.20,.27,.34,1);factor=nt.nodes.new('CompositorNodeMath');factor.operation='MULTIPLY';factor.inputs[1].default_value=.55
nt.links.new(rl.outputs['Mist'],factor.inputs[0]);nt.links.new(factor.outputs[0],mix.inputs[0]);nt.links.new(rl.outputs['Image'],mix.inputs[1]);out=nt.nodes.new('CompositorNodeComposite');nt.links.new(mix.outputs[0],out.inputs[0])
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(ROOT/'Evidence/blender_preview.png')
image.pack();image.filepath='//../Textures/Reboot_Atlas_256.png'
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Source~/Reboot_Pilot.blend'),compress=True)
if '--skip-render' not in sys.argv:bpy.ops.render.render(write_still=True)
print('REBOOT_GEOMETRY',triangles,'triangles; preview is Blender, Unity pending')
