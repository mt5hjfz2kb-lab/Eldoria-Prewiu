"""Genuine hand-worn microgeometry pilot: five CC0-PBR modules, individual A/B only.
Chips must be geometry cuts, not destructive image recoloring; Blender 5.2.
"""
import bpy,sys,math,json,hashlib,random,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src=Path(args[0]);out=Path(args[1])
for sub in ('glb','before','after','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
names=['stone_ashlar_01','stone_ashlar_02','stone_ashlar_03','arch_keystone_major','arch_voussoir_major']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene
s.render.engine='BLENDER_EEVEE';s.render.resolution_x=960;s.render.resolution_y=960
s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('neutral CC0 stone evaluation');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def light(n,p,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=8
 ob=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(ob);ob.location=p
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('amber key',(-6,-8,12),1600,(1,.85,.7))
light('sky fill',(8,4,11),1500,(.66,.8,1))
light('rim',(2,7,9),750,(1,1,1))
cd=bpy.data.cameras.new('A/B locked lens');cam=bpy.data.objects.new('A/B locked lens',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def view(objs):
 pts=[ob.matrix_world@Vector(c) for ob in objs for c in ob.bound_box]
 lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)])
 m=(lo+hi)/2;ext=hi-lo
 cam.location=m+Vector((.86,-1.53,1.14)).normalized()*max(ext.length,1)*3.2
 cam.rotation_euler=(m-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(ext.x,ext.y,ext.z,.3)*1.74
def render(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def hashfile(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def chips(ob,seed):
 R=random.Random(seed)
 mesh=ob.data
 if len(mesh.vertices)<4:raise RuntimeError('No usable mesh')
 local=np_minmax(mesh)
 low,high=local
 dims=high-low
 total=0
 for j in range(3):
  # Millimetric wear on junction corners; keep the modular bounding dimensions.
  signs=[R.choice([0,1]) for _ in range(3)]
  p=Vector(tuple((high[k] if signs[k] else low[k]) for k in range(3)))
  # cutter center touches corner but does not significantly shrink whole block
  size=min(dims)*R.uniform(.09,.15)
  cutter_radius=size
  # at most 15% along each axis to avoid a fragmented block
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=cutter_radius,location=ob.matrix_world@p)
  cutter=bpy.context.object;cutter.name='temporary_real_corner_chip'
  cutter.scale=(1.02,R.uniform(.75,1.35),R.uniform(.55,1.1))
  cutter.rotation_euler=(R.random()*.8,R.random()*.8,R.random()*.8)
  bpy.context.view_layer.objects.active=ob
  mod=ob.modifiers.new('physical_corner_chip_'+str(j),'BOOLEAN')
  mod.object=cutter;mod.operation='DIFFERENCE';mod.solver='EXACT'
  try:
   bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
   bpy.ops.object.modifier_apply(modifier=mod.name)
  finally:
   bpy.data.objects.remove(cutter,do_unlink=True)
  total+=1
 return total
def np_minmax(mesh):
 verts=[v.co for v in mesh.vertices]
 return (Vector([min(p[i] for p in verts) for i in range(3)]),
         Vector([max(p[i] for p in verts) for i in range(3)]))
records=[]
for n in names:
 pre=set(bpy.data.objects);r={'asset':n,'art_approved':False,'target':8}
 try:
  original=src/(n+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  objs=[x for x in bpy.data.objects if x not in pre and x.type=='MESH']
  if not objs:raise RuntimeError('No mesh')
  view(objs);render(out/'before'/(n+'.png'))
  count=0
  for i,o in enumerate(objs):
   count+=chips(o,300+i*13+sum(ord(c) for c in n))
   bevel=o.modifiers.new('fine worn bevel','BEVEL');bevel.width=.009;bevel.segments=2
   o.modifiers.new('normal polish to keep dressed planes','WEIGHTED_NORMAL')
  render(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in objs:o.select_set(True)
  bpy.context.view_layer.objects.active=objs[0]
  candidate=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(candidate),export_format='GLB',use_selection=True,export_apply=True)
  if candidate.stat().st_size<15000:raise RuntimeError('Too small')
  r.update({'technical':'PASS_PENDING_ART','physical_chips':count,'source_sha':hashfile(original),'candidate_sha':hashfile(candidate)})
  print('GEOMETRIC_WEAR',n,count,flush=True)
 except Exception as e:
  r['technical']='FAILED';r['error']=str(e);traceback.print_exc()
 finally:
  records.append(r)
  for x in list(bpy.data.objects):
   if x not in rig:bpy.data.objects.remove(x,do_unlink=True)
  for x in list(bpy.data.images):
   if x.users==0:bpy.data.images.remove(x)
  (out/'reports'/'microchipped_pbr_status.json').write_text(json.dumps({'run_source':38822723551,'no_composition':True,'art_approved':False,'assets':records},indent=2))
if len(records)!=5 or any(r['technical']!='PASS_PENDING_ART' for r in records):raise RuntimeError('Some geometry failed')
print('GEOMETRIC_WEAR_COMPLETE_FIVE',flush=True)
