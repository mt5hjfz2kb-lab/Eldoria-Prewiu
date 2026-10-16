"""Valoria masonry V5: sculpt FOUR isolated true arch stones with visibly worn
historic ARRIS corners and a carved face, preserving physical envelope / CC0 photo.
No assembled arch; no claims of 6 or 8 until actual A/B inspection.
"""
import bpy, sys, math, random, traceback, hashlib, json
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError('Need 4 existing stone GLB dir and output')
src,out=map(Path,args)
names=['arch_keystone_major','arch_keystone_minor','arch_voussoir_major','arch_voussoir_minor']
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1000;scene.render.resolution_y=1000
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria medieval carved stone studio')
world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.23,.30,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.81;scene.world=world
def lamp(name,power,position,color):
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.size=10;data.shape='DISK';data.color=color
 ob=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(ob)
 ob.location=position;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('warm stone key',1700,(-7,-9,12),(1,.85,.71))
lamp('cold sky fill',1530,(8,3,11),(.71,.84,1))
lamp('relief rim',900,(-1,9,10),(.98,.97,.91))
cd=bpy.data.cameras.new('fixed arch wedge evidence')
cam=bpy.data.objects.new('fixed arch wedge evidence',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(models):
 bpy.context.view_layer.update()
 p=[ob.matrix_world@Vector(v) for ob in models for v in ob.bound_box if ob.type=='MESH']
 return Vector([min(x[i] for x in p) for i in range(3)]),Vector([max(x[i] for x in p) for i in range(3)])
def frame(models):
 lo,hi=bbox(models);center=(lo+hi)*.5;extent=hi-lo
 cam.location=center+Vector((.9,-1.65,1.13)).normalized()*max(extent.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(extent.x,extent.y,extent.z,.3)*1.76
def render(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
results=[]
for index,name in enumerate(names):
 original=src/(name+'.glb')
 prior=set(bpy.data.objects)
 r={'asset':name,'art_approved':False,'test_not_publication':True}
 try:
  if not original.is_file():raise RuntimeError('Missing original stone '+name)
  bpy.ops.import_scene.gltf(filepath=str(original))
  mesh=[o for o in bpy.data.objects if o not in prior and o.type=='MESH']
  if len(mesh)!=1:raise RuntimeError('One stone expected, got '+str(len(mesh)))
  obj=mesh[0]
  lo,hi=bbox(mesh)
  extent=hi-lo
  frame(mesh);render(out/'before'/(name+'.png'))
  # Scatter modest yet definitely visible angular chip cuts along EXPOSED edges.
  # Difference booleans only remove geometry; no stretching beyond modular envelope.
  # Use actual world-space cutter locations with object's existing imported transforms.
  rng=random.Random(202611+index*503)
  faces_before=len(obj.data.polygons)
  cuts=[]
  width=float(extent.x);height=float(extent.z)
  radius_base=min(width*.15,height*.24)
  for k in range(5):
   edge=-1 if k%2==0 else 1
   x=float(lo.x if edge<0 else hi.x)+(-edge)*rng.uniform(.007,.022)
   y=float(lo.y)+rng.uniform(-.025,.07)
   z=float(lo.z+height*(.15+.17*k))
   if k in (0,3):z=float(hi.z)-rng.uniform(.014,.05)
   radius=radius_base*rng.uniform(.85,1.08)
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,
     location=(x,y,z))
   cutter=bpy.context.object;cutter.name='TEMP_limestone_chisel_cutter'
   cutter.scale=(rng.uniform(.63,.95),rng.uniform(.76,1.24),rng.uniform(.56,.96))
   cutter.rotation_euler=tuple(rng.uniform(-.7,.7) for q in range(3))
   m=obj.modifiers.new('true hand chipped arris '+str(k),'BOOLEAN')
   m.operation='DIFFERENCE';m.solver='EXACT';m.object=cutter
   bpy.ops.object.select_all(action='DESELECT')
   obj.select_set(True);bpy.context.view_layer.objects.active=obj
   bpy.ops.object.modifier_apply(modifier=m.name)
   bpy.data.objects.remove(cutter,do_unlink=True)
   cuts.append({'index':k,'radius_m':round(radius,4)})
  # Edge wear: about 1 cm measured. It creates real lit bevel planes,
  # while preserving photographed medieval limestone material and normal.
  bev=obj.modifiers.new('chiselled arris light catches','BEVEL')
  bev.width=.009 if 'minor' in name else .015;bev.segments=2
  bev.limit_method='ANGLE';bev.angle_limit=math.radians(25)
  bpy.ops.object.select_all(action='DESELECT')
  obj.select_set(True);bpy.context.view_layer.objects.active=obj
  bpy.ops.object.modifier_apply(modifier=bev.name)
  bpy.context.view_layer.update()
  after_lo,after_hi=bbox([obj])
  if any(float(after_lo[i])<float(lo[i])-.03 or float(after_hi[i])>float(hi[i])+.03 for i in range(3)):
   raise RuntimeError('Chiselling exceeds modular original envelope')
  faces_after=len(obj.data.polygons)
  if faces_after<=faces_before+10:raise RuntimeError('Real chisel geometry invisible/no-op')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  obj.select_set(True);bpy.context.view_layer.objects.active=obj
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<30000:raise RuntimeError('Export lost CC0 photo detail')
  r.update({'technical':'PASS_PENDING_VISUAL','true_chisels':len(cuts),
    'sculpted_faces_before':faces_before,'sculpted_faces_after':faces_after,
    'source_sha':sha(original),'candidate_sha':sha(dest),
    'world_bounds_before':[[round(float(a),3) for a in lo],[round(float(a),3) for a in hi]],
    'world_bounds_after':[[round(float(a),3) for a in after_lo],[round(float(a),3) for a in after_hi]]})
  print('PHYSICAL_STONE_FACE_CHISEL',name,faces_before,'to',faces_after,flush=True)
 except Exception as e:
  r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'four_physical_arch_stones_v5.json').write_text(json.dumps({
   'source_run':38888410182,'no_composition':True,
   'no_claim_floor6':True,'assets':results},indent=2))
if len(results)!=4 or any(x['technical']!='PASS_PENDING_VISUAL' for x in results):
 raise RuntimeError('Four arch stone sculpt batch incomplete')
print('FOUR_INDIVIDUAL_CHISELED_ARCH_ASSETS_COMPLETE',flush=True)
