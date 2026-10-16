"""Two real low-altitude landscape tiles for Valoria: actual scanned CC0 soils,
Quaternius grass geometry, author-approved stone pebble geometry, uneven edges.
Keep each 4x4m modular tile. Reusable NOT scene assembly. A/B native Blender.
"""
import bpy,sys,math,random,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector,Matrix
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(a)!=4:raise RuntimeError('dirt source GLB grass source GLB tuft donor GLB output')
dirt,grass,tuft,out=map(Path,a)
for t in ('before','after','glb','reports'):(out/t).mkdir(parents=True,exist_ok=True)
names=[('dirt_ground_patch',dirt),('grass_ground_patch',grass)]
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1050;s.render.resolution_y=950
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria natural grass and hillside patch studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;s.world=w
def area(name,power,pos,color):
 l=bpy.data.lights.new(name,'AREA');l.color=color;l.energy=power;l.shape='DISK';l.size=12
 ob=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('dusk sun',1850,(-9,-8,13),(1,.86,.72))
area('cold skylight',1750,(8,4,12),(.7,.81,1))
area('soft terrain rim',850,(-2,10,12),(.95,.98,1))
camera_data=bpy.data.cameras.new('individual lookdev static camera')
cam=bpy.data.objects.new('individual lookdev static camera',camera_data)
bpy.context.collection.objects.link(cam);s.camera=cam;camera_data.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bbox(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def setcam(obs):
 lo,hi=bbox(obs);mid=(lo+hi)/2;d=hi-lo
 cam.location=mid+Vector((.8,-1.6,1.48)).normalized()*max(d.length,1)*2.8
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camera_data.ortho_scale=max(d.x,d.y,d.z,.3)*1.6
def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
before=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(tuft))
donors=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
if not donors:raise RuntimeError('CC0 real grass donor empty')
low,high=bbox(donors)
center=(low+high)*.5
bottom=Vector((center.x,center.y,low.z))
templ=[(o.name,o.data,o.matrix_world.copy()) for o in donors]
for o in list(bpy.data.objects):
 if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
records=[]
for name,path in names:
 pre=set(bpy.data.objects);rec={'asset':name,'min_target':6,'no_assembly':True,'art_approved':False}
 try:
  bpy.ops.import_scene.gltf(filepath=str(path))
  existing=[o for o in bpy.data.objects if o not in pre]
  models=[o for o in existing if o.type=='MESH']
  if not models:raise RuntimeError('No photographic CC0 ground GLB')
  low,high=bbox(models)
  dim=high-low
  if not 3.5<dim.x<5.5 or not 3.5<dim.y<5.5:raise RuntimeError('Not a 4x4m terrain module')
  setcam(models);shoot(out/'before'/(name+'.png'))
  rng=random.Random(161010 if 'dirt' in name else 161011)
  tufts=[]
  for k in range(13 if 'dirt' in name else 9):
   x=rng.uniform(float(low.x)+.21,float(high.x)-.21)
   y=rng.uniform(float(low.y)+.21,float(high.y)-.21)
   z=float(high.z)+.008
   scale=rng.uniform(.08,.18) if 'grass' in name else rng.uniform(.06,.145)
   angle=rng.uniform(-math.pi,math.pi)
   for idx,(original_name,data,M) in enumerate(templ):
    o=bpy.data.objects.new('real_ground_quaternius_tuft_%d_%d'%(k,idx),data)
    bpy.context.collection.objects.link(o)
    o.matrix_world=Matrix.Translation((x,y,z))@Matrix.Rotation(angle,4,'Z')@Matrix.Scale(scale,4)@Matrix.Translation(-bottom)@M
    tufts.append(o)
  # Light physical weathering in dirt with tiny stable native irregular pebbles
  stones=[]
  for i in range(18 if 'dirt' in name else 7):
   x=rng.uniform(float(low.x)+.15,float(high.x)-.15)
   y=rng.uniform(float(low.y)+.15,float(high.y)-.15)
   z=float(high.z)+.009
   radius=rng.uniform(.018,.043)
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=radius,location=(x,y,z))
   ob=bpy.context.object;ob.name='shale_soil_microstone_'+str(i)
   ob.scale=(rng.uniform(.9,1.6),rng.uniform(.75,1.15),rng.uniform(.35,.58))
   ob.rotation_euler.z=rng.uniform(0,math.pi)
   mat=bpy.data.materials.get('Eldoria_pebble_gravel_in_soil')
   if not mat:
    mat=bpy.data.materials.new('Eldoria_pebble_gravel_in_soil');mat.use_nodes=True
    bs=mat.node_tree.nodes['Principled BSDF']
    bs.inputs['Base Color'].default_value=(.25,.24,.21,1);bs.inputs['Roughness'].default_value=.96
   ob.data.materials.append(mat);stones.append(ob)
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  new=models+tufts+stones
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  glb=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
  if glb.stat().st_size<12000:raise RuntimeError('GLB failed')
  rec.update({'technical':'PASS_PENDING_VISUAL','grass_foliage_clusters':13 if 'dirt' in name else 9,
    'actual_grass_mesh_objects':len(tufts),'soil_stones':len(stones),
    'source_sha':sha(path),'new_sha':sha(glb),
    'known_limitation':'No user-approved scattered rocks; scene-scale and repetition must be visually judged'})
  print('ELDORIA_GENUINE_TERRAIN_PATCH',name,len(new),flush=True)
 except Exception as e:
  rec.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(rec)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'two_reallife_terrain_patches.json').write_text(json.dumps({
   'no_scene':True,'art_approved':False,'assets':records},indent=2))
if any(r['technical']!='PASS_PENDING_VISUAL' for r in records):
 raise RuntimeError('Terrain two-asset quality pilot incomplete')
print('TWO_STANDALONE_LANDSCAPE_PATCHES_DONE',flush=True)
