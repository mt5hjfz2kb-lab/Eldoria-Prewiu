"""Second low-score group: four INDIVIDUAL modular units, not a scene:
grass ground patch, dirt ground patch, river water tile, ruined stone wall.
Retain existing textures / CC0 photographed ashlar donor; add geometric terrain
relief and built masonry ruin. Before/after fixed Blender studio views.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector,noise
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=3:raise SystemExit('Original GLB directory, best CC0 V7 wall GLB, output')
old_dir,wall_donor,out=map(Path,A)
names=['grass_ground_patch','dirt_ground_patch','river_water_tile','wall_panel_ruined_4m']
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=950;s.render.resolution_y=950
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Standalone terrain module neutral studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.2,.25,.33)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=world
def lamp(name,power,pos,c):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=c;d.size=9;d.shape='DISK'
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm daylight',1850,(-8,-9,12),(1,.85,.67))
lamp('cool fill',1580,(6,3,11),(.73,.85,1))
lamp('bright rim',800,(1,9,9),(1,1,1))
data=bpy.data.cameras.new('fixed A/B');cam=bpy.data.objects.new('fixed A/B',data)
bpy.context.collection.objects.link(cam);s.camera=cam;data.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bbox(obs):
 bpy.context.view_layer.update()
 p=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 if not p:raise RuntimeError('No mesh bounds')
 return Vector([min(x[i] for x in p) for i in range(3)]),Vector([max(x[i] for x in p) for i in range(3)])
def view(meshes):
 lo,hi=bbox(meshes);mid=(lo+hi)/2;size=hi-lo
 cam.location=mid+Vector((.94,-1.56,1.14)).normalized()*max(size.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 data.ortho_scale=max(size.x,size.y,size.z,.4)*1.69
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def plain(name,c,rough=.8,metal=0):
 m=bpy.data.materials.new(name);m.use_nodes=True
 b=m.node_tree.nodes.get('Principled BSDF')
 b.inputs['Base Color'].default_value=(*c,1);b.inputs['Roughness'].default_value=rough
 b.inputs['Metallic'].default_value=metal
 return m
stone=plain('small irregular alpine river gravel',(.33,.35,.37))
grass=plain('muted alpine green foliage',(.23,.34,.13))
earth=plain('alpine compact soil',(.40,.32,.22))
def sphere(n,xyz,r,c,scale=None):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=r,location=xyz)
 o=bpy.context.object;o.name=n;o.data.materials.append(c)
 if scale:o.scale=scale
 return o
def thin_leaf(name,root,length,width,mat,angle):
 x,y,z=root
 tip=(x+math.cos(angle)*length*.24,y+math.sin(angle)*length*.24,z+length)
 v=[(x-width/2,y,z),(x+width/2,y,z),tip]
 me=bpy.data.meshes.new(name);me.from_pydata(v,[],[(0,1,2)]);me.update()
 o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o)
 o.data.materials.append(mat);return o
def improve_grass(meshes):
 # Preserve existing V3 ground and vegetation, add actual varied microrelief
 # and physically different small tufts; avoid floating carpet of spikes.
 R=random.Random(1220);lo,hi=bbox(meshes);result=[]
 for i in range(38):
  x=lo.x+.18+R.random()*(hi.x-lo.x-.36)
  y=lo.y+.15+R.random()*(hi.y-lo.y-.30)
  z=hi.z+.009
  size=R.uniform(.046,.105)
  result.append(sphere('natural_clover_moss_hummock_'+str(i),(x,y,z),size,
      grass if i%4 else earth,(1.4,1.08,.34)))
 for i in range(25):
  x=lo.x+.22+R.random()*(hi.x-lo.x-.44)
  y=lo.y+.22+R.random()*(hi.y-lo.y-.44);z=hi.z+.005
  for k in range(3):
   result.append(thin_leaf('varied_alpine_grass_'+str(i)+'_'+str(k),
      (x,y,z),R.uniform(.07,.2),R.uniform(.012,.025),grass,R.uniform(0,6.28)))
 return meshes+result,len(result)
def improve_dirt(meshes):
 R=random.Random(441);lo,hi=bbox(meshes);extra=[]
 for j in range(42):
  x=lo.x+.16+R.random()*max(.1,hi.x-lo.x-.32)
  y=lo.y+.16+R.random()*max(.1,hi.y-lo.y-.32)
  r=R.uniform(.02,.071)
  extra.append(sphere('embedded_soil_stone_'+str(j),(x,y,hi.z+.006),r,stone if j%3 else earth,
       (R.uniform(.7,1.5),R.uniform(.7,1.4),.30)))
 return meshes+extra,len(extra)
def make_ripple_water(meshes):
 lo,hi=bbox(meshes);selected=[m for o in meshes for m in o.data.materials if m]
 mat=selected[0] if selected else plain('Alpine dusk blue water',(.055,.22,.30),.25,.08)
 # Photo water from previous GLB, but REAL 3D light catching ripple geometry.
 nx=59;ny=59;vertices=[];faces=[];uv=[]
 for iy in range(ny):
  y=lo.y+(hi.y-lo.y)*iy/(ny-1)
  for ix in range(nx):
   x=lo.x+(hi.x-lo.x)*ix/(nx-1)
   edge=min(ix/(nx-1),iy/(ny-1),1-ix/(nx-1),1-iy/(ny-1))
   atten=min(1.0,edge*20.0)
   z=(lo.z+hi.z)*.5+atten*(.026*math.sin(4.1*x+2.5*y)
        +.015*math.sin(7.9*x-3.8*y)+.006*math.cos(12.3*y))
   vertices.append((x,y,z));uv.append((ix/(nx-1),iy/(ny-1)))
 for iy in range(ny-1):
  for ix in range(nx-1):
   a=iy*nx+ix;b=a+1;c=a+nx;d=c+1
   faces.extend([(a,b,d),(a,d,c)])
 me=bpy.data.meshes.new('physically_wavy_water_4m')
 me.from_pydata(vertices,[],faces);me.update()
 ob=bpy.data.objects.new('water_tile_sunlit_evening_ripples',me)
 bpy.context.collection.objects.link(ob);me.materials.append(mat)
 layer=me.uv_layers.new(name='UVMap')
 for p in me.polygons:
  p.use_smooth=True
  for i in p.loop_indices:layer.data[i].uv=uv[me.loops[i].vertex_index]
 for old in meshes:bpy.data.objects.remove(old,do_unlink=True)
 return [ob],len(faces)
def ruined_wall(meshes):
 if not wall_donor.is_file():raise RuntimeError('Real 84-stone wall donor missing')
 for old in meshes:bpy.data.objects.remove(old,do_unlink=True)
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(wall_donor))
 donor=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 if len(donor)<75:raise RuntimeError('No real individual stone course family found')
 R=random.Random(314)
 removed=0
 # Spalled nonregular ruin silhouette; preserve the four lower courses intact.
 for o in list(donor):
  name=o.name.lower()
  row=int(name.split('ashlar_r')[1][:2]) if 'ashlar_r' in name else -1
  x=o.location.x
  if (row==5 and (x>-.95 or R.random()<.35)) or (row==4 and x>.58 and R.random()<.72):
   bpy.data.objects.remove(o,do_unlink=True);removed+=1;donor.remove(o)
 # Selected top blocks lean after damage, but tiny to preserve mortar contacts.
 for o in donor:
  if 'ashlar_r05' in o.name.lower() and R.random()<.25:
   o.rotation_euler.y+=R.uniform(-.018,.018)
 # Rubble is part of this 4m ruined wall module; no separate gameplay scene.
 lo,hi=bbox(donor)
 rubble=[]
 for j in range(9):
  x=lo.x+.15+R.random()*(hi.x-lo.x-.3)
  y=lo.y-.14+R.random()*.38;z=lo.z+.044
  rubble.append(sphere('natural_fallen_ruin_fragment_'+str(j),(x,y,z),R.uniform(.05,.12),stone,
                      (1.4,R.uniform(.7,1.1),.55)))
 return donor+rubble,removed
makers={'grass_ground_patch':improve_grass,'dirt_ground_patch':improve_dirt,
  'river_water_tile':make_ripple_water,'wall_panel_ruined_4m':ruined_wall}
results=[]
for i,name in enumerate(names,1):
 start=set(bpy.data.objects);record={'asset':name,'art_approved':False,
  'target':6,'no_scene_composition':True}
 try:
  path=old_dir/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(path))
  old=[o for o in bpy.data.objects if o not in start and o.type=='MESH']
  if not old:raise RuntimeError('No baseline')
  view(old);shoot(out/'before'/(name+'.png'))
  created,changes=makers[name](old)
  if not created:raise RuntimeError('No candidate')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in created:o.select_set(True)
  bpy.context.view_layer.objects.active=created[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  record.update({'technical':'PASS_PENDING_ART','changed_elements':changes,
   'physical_mesh_count':len(created),'source_sha':sha(path),'candidate_sha':sha(target)})
  print('LOW_SCORE_GROUND_OR_RUIN',i,name,changes,flush=True)
 except Exception as e:
  record.update({'technical':'FAILED','error':repr(e)})
  traceback.print_exc()
 finally:
  results.append(record)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'four_ground_and_ruin.json').write_text(json.dumps({
   'art_approved':False,'all_visual_pending':True,
   'source_v3_run':38813966671,'photostone_donor_run':38865543613,
   'assets':results},indent=2))
if any(x['technical']!='PASS_PENDING_ART' for x in results) or len(results)!=4:
 raise RuntimeError('Incomplete low quality group')
print('FOUR_REAL_MODULAR_CANDIDATES_DONE',flush=True)
