"""Bounded ribera study. Editable source; no inherited world mesh, no gameplay objects."""
import bpy,sys,math,random,json,pathlib
from mathutils import Vector
from math import sin,cos,pi,exp
out=pathlib.Path(sys.argv[sys.argv.index('--')+1]);out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
rng=random.Random(73)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;m.node_tree.nodes.clear();bs=m.node_tree.nodes.new('ShaderNodeBsdfPrincipled');bs.name='Principled BSDF';output=m.node_tree.nodes.new('ShaderNodeOutputMaterial');m.node_tree.links.new(bs.outputs['BSDF'],output.inputs['Surface']);bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Roughness'].default_value=.95;return m
soil=mat('Ochre meadow',(0.39,.43,.11));bank=mat('Warm cut riverbank',(.40,.32,.18));water=mat('River turquoise',(.025,.32,.40));sand=mat('Worn path',(.58,.44,.22));rock=mat('Warm grey stone',(.39,.40,.32));bark=mat('Pine bark',(.18,.12,.065));greens=[mat('Needles '+str(i),c) for i,c in enumerate([(.028,.115,.035),(.045,.16,.045),(.08,.21,.055),(.06,.18,.055)])]
def mesh(n,v,f,m):
 d=bpy.data.meshes.new(n);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
# Authorial river follows an editable Bezier guide; banks are explicit continuous cross sections.
knots=[(-94,21),(-70,9),(-48,2),(-25,-12),(-5,-18),(20,-31),(43,-33),(68,-44),(94,-52)]
def river(x):
 for (a,b),(c,d) in zip(knots,knots[1:]):
  if a<=x<=c:t=(x-a)/(c-a);t=t*t*(3-2*t);return b+(d-b)*t
 return knots[0][1] if x<knots[0][0] else knots[-1][1]
def ground(x,y):return 3.0+7.5*exp(-((x+35)/25)**2-((y-25)/20)**2)+4*exp(-((x-53)/23)**2-((y-35)/26)**2)+.24*sin(x*.18)*cos(y*.15)
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
   if t<.045:z=2.7+(z-2.7)*t/.045
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
  x=-96+i;y=river(x)+side*(3.2+.55*sin(x*.13));vv.extend([(x,y,2.7),(x,y,.1)])
 for i in range(N):ff.append((2*i,2*i+1,2*i+3,2*i+2))
 mesh('Pilot_Cutbank_'+str(side),vv,ff,bank)
v=[];f=[]
for i in range(193):
 x=-96+i;w=3.2+.55*sin(x*.13);v.extend([(x,river(x)-w,.32),(x,river(x)+w,.32)])
for i in range(192):f.append((i*2,i*2+2,i*2+3,i*2+1))
mesh('Pilot_River',v,f,water)
# Actual Bezier paths provide paint strokes on the UV terrain; no floating angular ribbons.
from mathutils.geometry import interpolate_bezier
path_samples=[]
paths=[[(-96,45),(-67,35),(-49,23),(-25,13),(0,10),(28,4),(58,-3),(96,-15)],[(0,10),(10,26),(0,46),(15,78)],[(-96,-18),(-70,-25),(-48,-39),(-25,-46),(0,-52),(23,-61),(68,-70)]]
for k,pts in enumerate(paths):
 curve=guide('Editable path '+str(k),[(x,y,ground(x,y)+.04) for x,y in pts]);bpy.context.view_layer.update();bp=curve.data.splines[0].bezier_points;sample=[]
 for a,b in zip(bp,bp[1:]):sample.extend(interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,48)[:-1])
 path_samples.append([(v.x,v.y) for v in sample])
# Pigment is painted in strokes following the landscape: openings, forest floor, winding paths.
import numpy as np
S=1536;yy,xx=np.mgrid[0:S,0:S];rand=np.random.default_rng(73);rgba=np.ones((S,S,4),dtype=np.float32);rgba[:,:,:3]=(.62,.65,.29);rgba[:,:,:3]+=rand.normal(0,.012,(S,S,1))
def dab(x,y,radius,colour,opacity=1):
 px=(x+96)/192*S;py=(y+78)/156*S;rx=radius/192*S;ry=radius/156*S
 x0=max(0,int(px-rx-1));x1=min(S,int(px+rx+2));y0=max(0,int(py-ry-1));y1=min(S,int(py+ry+2))
 if x0>=x1 or y0>=y1:return
 Y,X=np.mgrid[y0:y1,x0:x1];d=np.sqrt(((X-px)/rx)**2+((Y-py)/ry)**2);a=(np.clip((1-d)*4,0,1)*opacity)[:,:,None];rgba[y0:y1,x0:x1,:3]=rgba[y0:y1,x0:x1,:3]*(1-a)+np.array(colour)*a
# Broken grass patches and ochre openings, not sinusoidal bands or uniform fill.
for i in range(1800):
 x=rng.uniform(-96,96);y=rng.uniform(-78,78);rad=rng.uniform(.45,2.2);c=rng.choice([(.56,.61,.22),(.67,.67,.30),(.49,.58,.23),(.71,.66,.35)])
 dab(x,y,rad,c,rng.uniform(.18,.45))
for sample in path_samples:
 for x,y in sample:dab(x,y,2.0,(.68,.58,.34),.45);dab(x,y,1.12,(.74,.62,.38),.92)
# Separate shallow rim and deeper channel, both mesh surfaces exported from the source.
shallow=mat('Shallow turquoise',(.06,.46,.49));deep=mat('Deep current',(.025,.26,.36));foam=mat('Pale river edge',(.30,.62,.59))
for name,offset,width,material in [('Pilot_Deep',0,1.6,deep),('Pilot_RimSouth',-1,.22,foam),('Pilot_RimNorth',1,.22,foam)]:
 vv=[];ff=[]
 for i in range(193):
  x=-96+i;w=3.2+.55*sin(x*.13);cy=river(x)+offset*(w-.32)
  vv.extend([(x,cy-width,.34),(x,cy+width,.34)])
 for i in range(192):ff.append((2*i,2*i+2,2*i+3,2*i+1))
 mesh(name,vv,ff,material)
# Change of asset method: extract a previously sculpted crown from the project's editable art.
# This is a copy of an existing asset; no external generation/model invocation or source mutation.
asset=pathlib.Path('world4x-source/Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/PineCanopy77.glb').resolve()
if not asset.exists():raise RuntimeError('Existing sculpted canopy asset missing')
before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(asset));imported=[o for o in bpy.data.objects if o not in before];vs=[];fs=[];mi=[]
for obj in imported:
 if obj.type!='MESH':continue
 for poly in obj.data.polygons:
  m=obj.data.materials[poly.material_index]
  if not m or not m.name.startswith('Native77 sculpted pine'):continue
  if sum(obj.data.vertices[i].co.x for i in poly.vertices)/len(poly.vertices)<20:continue
  ids=[]
  for i in poly.vertices:ids.append(len(vs));vs.append(obj.matrix_world@obj.data.vertices[i].co)
  fs.append(ids);mi.append(int(m.name.split('pine ')[-1].split('.')[0])%4)
if not fs:raise RuntimeError('No source crown extracted; never replace with formula fallback')
lo=Vector([min(v[k] for v in vs) for k in range(3)]);hi=Vector([max(v[k] for v in vs) for k in range(3)]);centre=(lo+hi)*.5;scale=5.5/(hi.z-lo.z)
vs=[((v.x-centre.x)*scale,(v.y-centre.y)*scale,(v.z-lo.z)*scale+.9) for v in vs]
for obj in imported:bpy.data.objects.remove(obj,do_unlink=True)
master=mesh('Pilot_SculptedPine',vs,fs,greens[0]);master.data.materials.clear()
for material in greens:master.data.materials.append(material)
for poly,idx in zip(master.data.polygons,mi):poly.material_index=idx;poly.use_smooth=True
import bmesh
bm=bmesh.new();bm.from_mesh(master.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.0001);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(master.data);bm.free()
bpy.context.view_layer.objects.active=master;master.select_set(True);dec=master.modifiers.new('Mobile crown reduction','DECIMATE');dec.ratio=.45;bpy.ops.object.modifier_apply(modifier=dec.name)
bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=.22,radius2=.09,depth=4.5,location=(0,0,2.25));tr=bpy.context.object;tr.data.materials.append(bark)
bpy.ops.object.select_all(action='DESELECT');master.select_set(True);tr.select_set(True);bpy.context.view_layer.objects.active=master;bpy.ops.object.join();master.name='Pilot_SculptedPine';masters=[master]
# Deliberately arranged clusters with clearing/path and river exclusions.
centres=[(-70,52,12,10),(-36,36,10,9),(28,31,13,11),(65,47,13,12),(-63,-55,13,8),(-9,-66,14,7),(51,-57,12,8),(-10,65,12,8),(-72,-9,9,6),(65,15,12,7)]
positions=[]
for cx,cy,rx,ry in centres:
 for j in range(26):
  a=rng.random()*2*pi;d=math.sqrt(rng.random());x=cx+cos(a)*rx*d;y=cy+sin(a)*ry*d
  if abs(y-river(x))<7 or min((x-X)**2+(y-Y)**2 for sample in path_samples for X,Y in sample)<9 or any((x-X)**2+(y-Y)**2<6 for X,Y in positions):continue
  positions.append((x,y));dab(x,y,3.2,(.36,.46,.17),.45);dab(x+.6,y+.5,1.9,(.28,.36,.12),.38);src=masters[0];o=src.copy();o.data=src.data;bpy.context.collection.objects.link(o);o.name='Pilot_Pine';o.location=(x,y,ground(x,y));s=rng.uniform(.70,1.23);o.scale=(s,s,s);o.rotation_euler.z=rng.uniform(-pi,pi)
for o in masters:bpy.data.objects.remove(o,do_unlink=True)
for cx,cy in [(-36,23),(56,32),(-73,-43),(28,-46)]:
 for j in range(7):
  x=cx+rng.uniform(-5,5);y=cy+rng.uniform(-4,4);bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(x,y,ground(x,y)+.5));o=bpy.context.object;o.name='Pilot_Rock';o.scale=(rng.uniform(1,2.6),rng.uniform(1,2),rng.uniform(1,3));o.rotation_euler=(.2,.3,rng.random());o.data.materials.append(rock)
# Pack the completed painted surface after forest-floor strokes.
im=bpy.data.images.new('Ribera painted meadow',width=S,height=S);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/'meadow.png');im.file_format='PNG';im.save();im.pack();tex=soil.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;soil.node_tree.links.new(tex.outputs['Color'],soil.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
# Fine upright grass punctuation and bank stones, grouped in the source for efficient export.
vs=[];fs=[]
for i in range(950):
 x=rng.uniform(-94,94);y=rng.uniform(-76,76)
 if abs(y-river(x))<5 or min((x-X)**2+(y-Y)**2 for sample in path_samples for X,Y in sample)<2:continue
 z=ground(x,y);h=rng.uniform(.25,.8);a=len(vs);vs.extend([(x-.16,y,z),(x+.16,y,z),(x+.04,y,z+h),(x,y-.16,z),(x,y+.16,z),(x,y+.06,z+h*.8)]);fs.extend([(a,a+1,a+2),(a+3,a+4,a+5)])
mesh('Pilot_Grass',vs,fs,greens[2])
# Organize editable authoring layers, retaining curves independently of final meshes.
for name,prefixes in [('01_Terrain',('Pilot_Terrain',)),('02_Water_Banks',('Pilot_River','Pilot_Cutbank','Pilot_Rim','Pilot_Deep')),('03_Paths',('Pilot_Path',)),('04_Forest',('Pilot_Pine','Pilot_Grass')),('05_Rocks',('Pilot_Rock',)),('00_Editable_Guides',('Editable',))]:
 collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
 for obj in list(bpy.context.scene.objects):
  if obj.name.startswith(prefixes):
   for old in list(obj.users_collection):old.objects.unlink(obj)
   collection.objects.link(obj)
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
