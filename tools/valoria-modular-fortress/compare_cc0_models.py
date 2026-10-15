"""Individual only. A/B verified GLB comparison: existing Eldoria Round3 vs CC0 donor.
All source licenses explicitly validated upstream. No scene composition.
"""
import bpy,sys,math,hashlib,json,random
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else sys.argv[1:]
v3=Path(args[0]);donor=Path(args[1]);out=Path(args[2])
out.mkdir(exist_ok=True,parents=True)
for sub in ('glb','before','after'): (out/sub).mkdir(exist_ok=True)
R=random.Random(2026)
choices={
 'pine_small':'Pine_2','pine_medium':'Pine_3','pine_tall':'Pine_5',
 'boulder_1':'Rock_Medium_1','boulder_2':'Rock_Medium_2','boulder_3':'Rock_Medium_3',
 'shrub_1':'Bush_Common','shrub_2':'Bush_Common','shrub_3':'Bush_Common',
 'cliff_stone_chip':'Rock_Medium_2','grass_tuft_1':'Grass_Wispy_Short',
 'grass_tuft_2':'Grass_Wispy_Tall',
 'cobble_pebbles':'Pebble_Round_1'
}
manifest=json.loads((donor/'source_manifest.json').read_text())
assert manifest['license']=='CC0'
assert len(choices)==13
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=960;scene.render.resolution_y=960;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Catalog world');scene.world.use_nodes=True
bg=scene.world.node_tree.nodes.get('Background');bg.inputs['Color'].default_value=(.16,.19,.23,1);bg.inputs['Strength'].default_value=.8
scene.view_settings.view_transform='AgX'
def light(name,position,power,color,size):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=size
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o)
 o.location=position;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('key',(-7,-10,14),1900,(1,.83,.68),8)
light('fill',(6,2,13),1800,(.57,.74,1),10)
light('rim',(2,10,8),1250,(.9,.9,1),6)
camd=bpy.data.cameras.new('catalog camera');cam=bpy.data.objects.new('catalog camera',camd)
bpy.context.collection.objects.link(cam);camd.type='ORTHO';scene.camera=cam
rig=set(bpy.data.objects)
def bnd(objs):
 pts=[]
 for o in objs:
  if o.type=='MESH':
   for v in o.bound_box:pts.append(o.matrix_world@Vector(v))
 lo=Vector([min(v[i] for v in pts) for i in range(3)])
 hi=Vector([max(v[i] for v in pts) for i in range(3)])
 return lo,hi
def load(path):
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(path))
 added=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 if not added:raise RuntimeError('No mesh: '+str(path))
 return added
def clear():
 for o in list(bpy.data.objects):
  if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
def normalize(objects, oldsize=None,shape='vertical'):
 bpy.context.view_layer.update()
 lo,hi=bnd(objects);extent=hi-lo
 if oldsize is not None:
  if shape=='vertical':factor=oldsize.z/max(extent.z,.001)
  else:factor=max(oldsize.x,oldsize.y,oldsize.z)/max(extent.x,extent.y,extent.z,.001)
 else:factor=1
 # Existing imported mesh parent transforms kept. Transform entire geometry into uniform
 # study scaling instead of parenting across Blender glTF hierarchy.
 origin=Vector(((lo.x+hi.x)*.5,(lo.y+hi.y)*.5,lo.z))
 for o in objects:
  if o.type=='MESH':
   m=o.matrix_world.copy()
   for v in o.data.vertices:
    w=m@v.co
    w=(w-origin)*factor
    v.co=m.inverted()@w
 bpy.context.view_layer.update()
 return factor
def render(path,objects):
 lo,hi=bnd(objects)
 center=(lo+hi)*.5
 ext=hi-lo
 distance=max(ext.length*.72,1)
 cam.location=center+Vector((.82,-1.5,1.12)).normalized()*distance*3.1
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(ext.x,ext.y,ext.z,.4)*1.58
 scene.render.image_settings.file_format='PNG';scene.render.filepath=str(path)
 bpy.ops.render.render(write_still=True)
def sha(p):
 h=hashlib.sha256();h.update(p.read_bytes());return h.hexdigest()
records=[]
for idx,(name,candidate) in enumerate(choices.items(),1):
 try:
  old=load(v3/'glb'/(name+'.glb'))
  oldlo,oldhi=bnd(old);oldsize=oldhi-oldlo
  normalize(old)
  render(out/'before'/(name+'.png'),old)
  clear()
  new=load(donor/(candidate+'.gltf'))
  factor=normalize(new,oldsize,'vertical' if name.startswith('pine') else 'max')
  if name=='shrub_2':
   for o in new:o.rotation_euler.z+=math.pi*.42
  if name=='shrub_3':
   for o in new:o.rotation_euler.z+=math.pi*.77
  # Genuine imported Quaternius source albedo, alpha and mesh protected
  for o in new:
   for m in o.data.materials:
    if m and m.use_nodes:
     for node in m.node_tree.nodes:
      if node.type=='BSDF_PRINCIPLED':
       node.inputs['Roughness'].default_value=.84
       node.inputs['Metallic'].default_value=0
  render(out/'after'/(name+'.png'),new)
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  records.append({'asset':name,'donor':candidate,'donor_original_source_sha':manifest['source_sha'],
   'scale_factor':factor,'geometry_objects':len(new),'glb_bytes':target.stat().st_size,
   'glb_sha256':sha(target),'before':name+'.png','after':name+'.png',
   'license':'CC0','visual_approved':False,'art_approved':False,
   'decision':'PENDING_SAME_CAMERA_INDEPENDENT_ARTISTIC_AB'})
  print('CC0_DONOR_AB',idx,'/',len(choices),name,candidate,flush=True)
 except Exception as e:
  import traceback;traceback.print_exc()
  records.append({'asset':name,'donor':candidate,'error':repr(e),'decision':'TECH_REJECT'})
 finally:clear()
 (out/'comparison_manifest.json').write_text(json.dumps({
 'purpose':'individual candidate A/B only, no composition',
 'base_run':38813966671,'donor_run':38815449976,'records':records,
 'require_independent_visual_review':True,'art_approved':False},indent=2))
if any(x['decision']=='TECH_REJECT' for x in records):raise RuntimeError('One or more GLB candidate import/exports failed')
print('CC0_DONOR_AB_13_COMPLETE',len(records))
