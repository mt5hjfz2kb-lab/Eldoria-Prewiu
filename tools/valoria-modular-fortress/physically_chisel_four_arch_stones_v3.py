"""Physical artisan arris and broken corners on four genuine radial CC0 voussoirs.
Use mesh booleans at upper visible front stone corners and bevel worn arrises,
not arbitrary photo color changes. Native Blender before/after same lens.
Never promote automatically: 6/10 decision only on actual comparison.
"""
import bpy,sys,json,hashlib,random,traceback
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,a)
for d in ('before','after','glb','reports'):(out/d).mkdir(exist_ok=True,parents=True)
names=['arch_keystone_major','arch_keystone_minor','arch_voussoir_major','arch_voussoir_minor']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1050;s.render.resolution_y=1050
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria cut-stone closeup');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.25,.33,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def light(name,power,pos,color):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.shape='DISK';l.size=9
 ob=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('golden chisel edge',1850,(-7,-9,12),(1,.85,.74))
light('cool dusk',1560,(8,3,11),(.72,.83,1))
light('rim',1100,(-3,9,10),(.92,.96,1))
camera_data=bpy.data.cameras.new('fixed radial stone comparison')
camera=bpy.data.objects.new('fixed radial stone comparison',camera_data)
bpy.context.collection.objects.link(camera);s.camera=camera;camera_data.type='ORTHO'
rig=set(bpy.data.objects)
def box(o):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for v in o.bound_box]
 return Vector([min(q[i] for q in pts) for i in range(3)]),Vector([max(q[i] for q in pts) for i in range(3)])
def camset(o):
 lo,hi=box(o);cen=(lo+hi)*.5;d=hi-lo
 camera.location=cen+Vector((.88,-1.55,1.10)).normalized()*max(d.length,1)*3
 camera.rotation_euler=(cen-camera.location).to_track_quat('-Z','Y').to_euler()
 camera_data.ortho_scale=max(d.x,d.y,d.z,.18)*1.65
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
records=[]
for name in names:
 pre=set(bpy.data.objects);r={'asset':name,'art_approved':False,'desired_floor':6}
 try:
  source=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(source))
  models=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if len(models)!=1:raise RuntimeError('One stone per asset required')
  ob=models[0];camset(ob);shoot(out/'before'/(name+'.png'))
  lo,hi=box(ob);dims=hi-lo
  if dims.x<.20 or dims.z<.13:raise RuntimeError('Stone bounds implausible')
  rng=random.Random(991+len(name))
  num_chips=4
  chips=[]
  for n in range(num_chips):
   # Physically remove edge chips from each real upper face side, not holes in center.
   side=(-1 if n%2==0 else 1)
   upper=(n<2)
   x=float(lo.x if side<0 else hi.x)+side*(-.017)
   y=float(lo.y)+float(dims.y)*(.13 if n<3 else .84)
   z=float(hi.z if upper else lo.z)
   radius=min(.080,float(dims.x)*.14,float(dims.z)*.22)
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,
     location=(x,y,z))
   cut=bpy.context.object;cut.name='temporary_hand_split_stone_corner'
   cut.scale=(rng.uniform(.83,1.25),rng.uniform(.90,1.22),rng.uniform(.75,1.06))
   cut.rotation_euler=(rng.uniform(-.8,.8),rng.uniform(-1,1),rng.uniform(-1.1,1.1))
   modifier=ob.modifiers.new('true_stone_corner_chisel_%d'%n,'BOOLEAN')
   modifier.operation='DIFFERENCE';modifier.solver='EXACT';modifier.object=cut
   bpy.ops.object.select_all(action='DESELECT')
   ob.select_set(True);bpy.context.view_layer.objects.active=ob
   bpy.ops.object.modifier_apply(modifier=modifier.name)
   bpy.data.objects.remove(cut,do_unlink=True)
   chips.append(round(radius,4))
  bevel=ob.modifiers.new('hand_smooth_dressed_edge','BEVEL')
  bevel.width=min(.023,float(dims.x)*.05,float(dims.z)*.10)
  bevel.segments=2;bevel.limit_method='ANGLE'
  bpy.ops.object.select_all(action='DESELECT')
  ob.select_set(True);bpy.context.view_layer.objects.active=ob
  bpy.ops.object.modifier_apply(modifier=bevel.name)
  if len(ob.data.polygons)<15:raise RuntimeError('Unstable stone Boolean geometry')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
  path=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_apply=True)
  if path.stat().st_size<25000:raise RuntimeError('Photographic PBR lost')
  r.update({'technical':'PASS_PENDING_ART','source_sha':sha(source),'candidate_sha':sha(path),
   'actual_individually_removed_corner_fragments':len(chips),'chip_radius_m':chips,
   'geometric_faces_after':len(ob.data.polygons),'preserved_mat_count':len(ob.data.materials)})
  print('REAL_PHYSICAL_STONE_CORNER_CHIPS',name,len(ob.data.polygons),flush=True)
 except Exception as e:
  r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'four_chiseled_radial_arch_stones.json').write_text(json.dumps({
   'source_run':38873426930,'no_scene':True,'art_approved':False,'items':records},indent=2))
if any(x['technical']!='PASS_PENDING_ART' for x in records):raise RuntimeError('4 standalone wedge models not all ready')
print('FOUR_HAND_CHISELED_ARCH_STONES_COMPLETE',flush=True)
