"""Valoria stone V4: visible sculptural cutting of four stone arch modules.
Cause-based follow up after V3 tiny erosion failed qualitative A/B.
Retain licensed photo UVs, actual tapered arch shapes, exact donor dimension.
Larger hand-dressed angled arrises + wide chipped corners, independent A/B.
NEVER ART_APPROVED automatically. No scene or Unity changes.
"""
import bpy,bmesh,sys,json,math,random,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError('four V3 arc-stone GLB folder and output dir expected')
src,out=map(Path,args)
names=['arch_keystone_major','arch_keystone_minor','arch_voussoir_major','arch_voussoir_minor']
for p in ('before','after','glb','reports'):(out/p).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1080;scene.render.resolution_y=1080
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Single stone sculpt A/B');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.79;scene.world=world
def lamp(name,pos,power,col):
 dat=bpy.data.lights.new(name,'AREA');dat.energy=power;dat.color=col;dat.shape='DISK';dat.size=8
 o=bpy.data.objects.new(name,dat);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('Warm chisel light',(-7,-8,12),2000,(1,.83,.70))
lamp('Cool quarry fill',(8,3,12),1550,(.71,.83,1))
lamp('Chipped edges rim',(0,10,11),1100,(1,.96,.85))
cd=bpy.data.cameras.new('stone view same lens')
cam=bpy.data.objects.new('stone view same lens',cd);bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(ms):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in ms for c in o.bound_box if o.type=='MESH']
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def frame(ms):
 lo,hi=bbox(ms);mid=(lo+hi)*.5;dim=hi-lo
 cam.location=mid+Vector((.83,-1.56,1.21)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(dim.x,dim.y,dim.z,.3)*1.8
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def snap(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
results=[]
for j,name in enumerate(names):
 pre=set(bpy.data.objects);row={'asset':name,'art_approved':False,'requires_HUMAN_visual_review':True}
 try:
  path=src/(name+'.glb')
  if not path.is_file():raise RuntimeError('Source V3 physically shaped stone missing')
  bpy.ops.import_scene.gltf(filepath=str(path))
  obs=[o for o in bpy.data.objects if o not in pre]
  meshes=[o for o in obs if o.type=='MESH']
  if len(meshes)!=1:raise RuntimeError('Expect exactly one masonry block')
  obj=meshes[0];old_lo,old_hi=bbox(meshes);orig_dim=old_hi-old_lo
  frame(meshes);snap(out/'before'/(name+'.png'))
  # High-quality explicitly sculpted arris at ALL dressed-stone edges.
  # Bevel must change silhouette visibly, unlike barely perceptible V3 chips.
  bevel=obj.modifiers.new('wide_hand_chiseled_arris_3_5cm','BEVEL')
  bevel_amount=.035 if 'major' in name else .027
  bevel.width=bevel_amount
  bevel.segments=2;bevel.limit_method='ANGLE';bevel.angle_limit=math.radians(24)
  bpy.ops.object.select_all(action='DESELECT')
  obj.select_set(True);bpy.context.view_layer.objects.active=obj
  bpy.ops.object.modifier_apply(modifier=bevel.name)
  # Restrict meaningful cuts to physical CORNERS, use exact triangular chisel
  # chip meshes. Cutter positions in world coordinates, partial intersection.
  rng=random.Random(20161016+j*57)
  after_bevel_lo,after_bevel_hi=bbox(meshes)
  cuts=[]
  # Open-top voussoir source topology yields off-envelope Boolean spikes; avoid destructive operation.
  # Architectural keystones retain the already validated chisel method.
  for ix,iy,iz in ([(0,0,1),(1,0,1),(0,1,1)] if 'keystone' in name else []):
   p=Vector([after_bevel_lo.x if ix==0 else after_bevel_hi.x,
             after_bevel_lo.y if iy==0 else after_bevel_hi.y,
             after_bevel_lo.z if iz==0 else after_bevel_hi.z])
   rad=.10 if 'major' in name else .073
   p+=Vector(((.015 if ix==0 else -.015),(.015 if iy==0 else -.015),-.011))
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rad,location=p)
   cutter=bpy.context.object;cutter.name='TEMP_CHISEL_BLADE'
   cutter.scale=(rng.uniform(1.1,1.35),rng.uniform(.9,1.2),rng.uniform(.55,.85))
   cutter.rotation_euler=(rng.uniform(.0,1.1),rng.uniform(-.7,.7),rng.uniform(-1,1))
   mod=obj.modifiers.new('genuine_large_corner_chisel','BOOLEAN')
   mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
   bpy.ops.object.select_all(action='DESELECT')
   obj.select_set(True);bpy.context.view_layer.objects.active=obj
   bpy.ops.object.modifier_apply(modifier=mod.name)
   bpy.data.objects.remove(cutter,do_unlink=True)
   cuts.append({'radius':round(rad,3),'corner':[ix,iy,iz]})
  bpy.context.view_layer.update()
  new_lo,new_hi=bbox(meshes);new_dim=new_hi-new_lo
  # Expected carved narrowing, never move or enlarge modular outward bounds.
  if any(float(new_dim[i]-orig_dim[i])>.002 for i in range(3)):
   raise RuntimeError('Sculpture violates modular outward envelope')
  # Existing CC0 texture and UVs must survive the physical carving.
  if not obj.data.materials or not obj.data.uv_layers:
   raise RuntimeError('Original professional photo PBR or UVs lost')
  snap(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in obs:ob.select_set(True)
  bpy.context.view_layer.objects.active=obj
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<40000:raise RuntimeError('Empty exported photostone')
  row.update({'technical':'PASS_PENDING_VISUAL','original_sha256':sha(path),'candidate_sha256':sha(target),
    'cutting':cuts,'bevel_m':bevel_amount,'vertices':len(obj.data.vertices),
    'source_run':38879660792,'material':'Preserved original CC0 PBR limestone',
    'no_assembly':True})
  print('REAL_VISIBLE_DRESSED_STONE',name,len(obj.data.vertices),flush=True)
 except Exception as e:
  row.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(row)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'stone_chisel_arch_four_v4.json').write_text(json.dumps({
    'all_art_approved':False,'comparison_source':'stone V3 tiny chips',
    'method':'blender applied bevel+real chipped geometry 3 corners','rows':results},indent=2))
if len(results)!=4 or any(x['technical']!='PASS_PENDING_VISUAL' for x in results):
 raise RuntimeError('Incomplete physical cut asset batch')
print('ALL_FOUR_ARCH_CHISEL_CANDIDATES_EXPORT_COMPLETE',flush=True)
