"""Bounded ribera study. Editable source; no inherited world mesh, no gameplay objects."""
import bpy,sys,math,random,json,pathlib
from mathutils import Vector
from math import sin,cos,pi,exp
out=pathlib.Path(sys.argv[sys.argv.index('--')+1]);out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
rng=random.Random(73)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*c,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.95;return m
soil=mat('Ochre meadow',(0.39,.43,.11));bank=mat('Warm cut riverbank',(.40,.32,.18));water=mat('River turquoise',(.025,.32,.40));sand=mat('Worn path',(.58,.44,.22));rock=mat('Warm grey stone',(.39,.40,.32));bark=mat('Pine bark',(.18,.12,.065));greens=[mat('Needles '+str(i),c) for i,c in enumerate([(.055,.19,.09),(.11,.28,.12),(.23,.36,.13),(.16,.32,.16)])]
def mesh(n,v,f,m):
 d=bpy.data.meshes.new(n);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
# Authorial river follows an editable Bezier guide; banks are explicit continuous cross sections.
knots=[(-94,-25),(-70,-31),(-48,-25),(-25,-33),(-5,-20),(20,-24),(43,-15),(68,-20),(94,-9)]
def river(x):
 for (a,b),(c,d) in zip(knots,knots[1:]):
  if a<=x<=c:t=(x-a)/(c-a);t=t*t*(3-2*t);return b+(d-b)*t
 return knots[0][1] if x<knots[0][0] else knots[-1][1]
def ground(x,y):return 1.8+6.5*exp(-((x+35)/25)**2-((y-25)/20)**2)+4*exp(-((x-53)/23)**2-((y-35)/26)**2)+.24*sin(x*.18)*cos(y*.15)
def guide(n,pts):
 c=bpy.data.curves.new(n,'CURVE');c.dimensions='3D';s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
 for p,co in zip(s.bezier_points,pts):p.co=co;p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 o=bpy.data.objects.new(n,c);bpy.context.collection.objects.link(o);o.hide_render=True;return o
guide('Editable river centre',[(x,y,.4) for x,y in knots])
# Two terrain banks share river boundary points: no buried water or coloured flat ground.
for side in [-1,1]:
 v=[];f=[];N=192;M=84
 for i in range(N+1):
  x=-96+i;cy=river(x);w=3.2+.55*sin(x*.13)
  for j in range(M+1):
   t=j/M;y=cy+side*(w+(78-side*cy-w)*t);z=ground(x,y)
   if t<.045:z=.65+(z-.65)*t/.045
   v.append((x,y,z))
 for i in range(N):
  for j in range(M):a=i*(M+1)+j;f.append((a,a+M+1,a+M+2,a+1) if side==1 else (a+1,a+M+2,a+M+1,a))
 o=mesh('Pilot_Terrain_'+str(side),v,f,soil);uv=o.data.uv_layers.new(name='MeadowUV')
 for p in o.data.polygons:
  for li in p.loop_indices:
   co=o.data.vertices[o.data.loops[li].vertex_index].co;uv.data[li].uv=((co.x+96)/192,(co.y+78)/156)
 # shore wall has actual height and irregular boundary
 vv=[];ff=[]
 for i in range(N+1):
  x=-96+i;y=river(x)+side*(3.2+.55*sin(x*.13));vv.extend([(x,y,.65),(x,y,.1)])
 for i in range(N):ff.append((2*i,2*i+1,2*i+3,2*i+2))
 mesh('Pilot_Cutbank_'+str(side),vv,ff,bank)
v=[];f=[]
for i in range(193):
 x=-96+i;w=3.2+.55*sin(x*.13);v.extend([(x,river(x)-w,.32),(x,river(x)+w,.32)])
for i in range(192):f.append((i*2,i*2+2,i*2+3,i*2+1))
mesh('Pilot_River',v,f,water)
# Winding main footpath and a fork climb round the hill, never across water.
for k,pts in enumerate([[(-96,3),(-67,8),(-49,-1),(-25,5),(0,2),(28,9),(58,8),(96,22)],[(0,2),(8,24),(0,43),(15,78)]]):
 guide('Editable path '+str(k),[(x,y,ground(x,y)+.04) for x,y in pts]);v=[];f=[]
 for (x,y),(xx,yy) in zip(pts,pts[1:]):
  for j in range(18):
   t=j/18;a=x+(xx-x)*t;b=y+(yy-y)*t;dx=xx-x;dy=yy-y;L=math.hypot(dx,dy)
   for s in [-1,1]:X=a-s*dy/L*1.15;Y=b+s*dx/L*1.15;v.append((X,Y,ground(X,Y)+.065))
 for i in range(len(v)//2-1):f.append((2*i,2*i+1,2*i+3,2*i+2))
 mesh('Pilot_Path_'+str(k),v,f,sand)
# Paint a real reusable meadow image with directional ochre/olive pigment and fine grain.
import numpy as np
S=1024;yy,xx=np.mgrid[0:S,0:S];r=np.random.default_rng(73);p=.035*np.sin(xx*.035+np.sin(yy*.018)*2)+.025*np.cos(yy*.065)+r.normal(0,.008,(S,S));rgba=np.ones((S,S,4),dtype=np.float32)
for c,base in enumerate([.59,.60,.28]):rgba[:,:,c]=np.clip(base+p*(1 if c<2 else .6),0,1)
im=bpy.data.images.new('Meadow painted pigment',width=S,height=S);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/'meadow.png');im.file_format='PNG';im.save();im.pack();tex=soil.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;soil.node_tree.links.new(tex.outputs['Color'],soil.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
# Recognizable conifers: solid irregular lobed boughs, dark gaps, woody trunk, tapered crown.
masters=[]
for family in range(3):
 parts=[]
 bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=.24,radius2=.10,depth=5.7,location=(0,0,2.85));tr=bpy.context.object;tr.data.materials.append(bark);parts.append(tr)
 for tier,(h,w) in enumerate([(1.5,1.65),(2.5,1.48),(3.5,1.18),(4.4,.86),(5.2,.48)]):
  vs=[];fs=[];segments=12
  for ring,(z,rr) in enumerate([(h-.25,w*.68),(h,w),(h+.48,w*.68),(h+1.3,0.04)]):
   for j in range(segments):a=j*2*pi/segments+family*.3;rad=rr*(1+.14*sin(j*2.8+tier));vs.append((cos(a)*rad,sin(a)*rad,z+.12*sin(j*2.1)))
  for ring in range(3):
   for j in range(segments):a=ring*segments+j;b=ring*segments+(j+1)%segments;fs.append((a,b,b+segments,a+segments))
  o=mesh('Bough',vs,fs,greens[(tier+family)%4]);parts.append(o)
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=tr;bpy.ops.object.join();tr.name='Pilot_TreeSource_'+str(family);tr.location=(0,0,2.85);bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');masters.append(tr)
# Deliberately arranged clusters with clearing/path and river exclusions.
centres=[(-65,26,12,10),(-36,36,10,9),(28,31,13,11),(65,47,13,12),(-63,-55,13,8),(-9,-57,14,9),(51,-48,15,10),(-10,65,12,8)]
positions=[]
for cx,cy,rx,ry in centres:
 for j in range(26):
  a=rng.random()*2*pi;d=math.sqrt(rng.random());x=cx+cos(a)*rx*d;y=cy+sin(a)*ry*d
  if abs(y-river(x))<7 or any((x-X)**2+(y-Y)**2<6 for X,Y in positions):continue
  positions.append((x,y));src=masters[rng.randrange(3)];o=src.copy();o.data=src.data;bpy.context.collection.objects.link(o);o.name='Pilot_Pine';o.location=(x,y,ground(x,y));s=rng.uniform(.70,1.23);o.scale=(s,s,s);o.rotation_euler.z=rng.uniform(-pi,pi)
for o in masters:bpy.data.objects.remove(o,do_unlink=True)
for cx,cy in [(-36,23),(56,32),(-73,-43),(28,-46)]:
 for j in range(7):
  x=cx+rng.uniform(-5,5);y=cy+rng.uniform(-4,4);bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(x,y,ground(x,y)+.5));o=bpy.context.object;o.name='Pilot_Rock';o.scale=(rng.uniform(1,2.6),rng.uniform(1,2),rng.uniform(1,3));o.rotation_euler=(.2,.3,rng.random());o.data.materials.append(rock)
# Save source and export the exact authored geography, not a runtime reconstruction.
bpy.ops.object.select_all(action='DESELECT')
for o in bpy.context.scene.objects:
 if o.type=='MESH':o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.world.color=(.28,.28,.28)
bpy.ops.object.light_add(type='SUN',location=(-40,-50,80));bpy.context.object.rotation_euler=(.45,-.4,-.6);bpy.context.object.data.energy=2
bpy.ops.object.camera_add(location=(0,-120,150));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=158;scene.camera=cam
scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.resolution_percentage=100;scene.view_settings.view_transform='Standard'
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Pilot.blend'));scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'authoring.json').write_text(json.dumps({'scope':'bounded riverbank pilot, no gameplay assets','trees':len(positions),'source':'editable meshes and Bezier guides','reference':'01_vista_cercana.png lower riverbank','art_approved':False},indent=2))
