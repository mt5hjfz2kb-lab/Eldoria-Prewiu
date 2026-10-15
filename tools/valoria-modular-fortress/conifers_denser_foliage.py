"""Eldoria conifer studio: increase physical branch density with real CC0 leaf geometry.
Only three independent pines, no composition. Separate bark and foliage, preserve CC0.
"""
import bpy,sys,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
import numpy as np
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src=Path(args[0]);out=Path(args[1])
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['pine_small','pine_medium','pine_tall']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=900;s.render.resolution_y=900;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Evergreen lookdev');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.28,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.75;s.world=world
def lamp(n,pos,e,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=e;d.color=color;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm key',(-8,-9,14),1900,(1,.85,.69))
lamp('cool fill',(8,3,11),1500,(.67,.81,1))
lamp('rim',(3,6,12),950,(.9,.95,1))
camdata=bpy.data.cameras.new('Single pine A/B');cam=bpy.data.objects.new('Single pine A/B',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(objs):
 pts=[o.matrix_world@Vector(v) for o in objs for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def set_camera(objs):
 lo,hi=bounds(objs);mid=(lo+hi)/2;ext=hi-lo
 cam.location=mid+Vector((.95,-1.65,1.25)).normalized()*max(ext.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camdata.ortho_scale=max(ext.x,ext.y,ext.z)*1.7
def render(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def hashfile(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def fix_leaf_colors(objs):
 images=set()
 for obj in objs:
  for m in obj.data.materials:
   if not m or 'leav' not in m.name.lower():continue
   if not m.use_nodes:continue
   for node in m.node_tree.nodes:
    if node.type=='TEX_IMAGE' and node.image:images.add(node.image)
 count=0
 for im in images:
  vals=np.empty(im.size[0]*im.size[1]*4,dtype=np.float32)
  im.pixels.foreach_get(vals);pix=vals.reshape(-1,4)
  # Correct an observed cyan bias without destroying genuine albedo detail.
  pix[:,0]=np.clip(pix[:,0]*1.15,0,1)
  pix[:,1]=np.clip(pix[:,1]*.96,0,1)
  pix[:,2]=np.clip(pix[:,2]*.68,0,1)
  im.pixels.foreach_set(vals);im.update();im.pack()
  count+=1
 return count
records=[]
for name in names:
 before=set(bpy.data.objects)
 row={'asset':name,'art_approved':False,'quality_target':8}
 try:
  original=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  allobjs=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
  leaves=[o for o in allobjs if any(m and 'leav' in m.name.lower() for m in o.data.materials)]
  if not leaves:raise RuntimeError('No separate genuine pine leaf geometry for '+name)
  set_camera(allobjs);render(out/'before'/(name+'.png'))
  copies=[]
  for leaf in leaves:
   for j in range(2):
    dupe=leaf.copy();dupe.data=leaf.data.copy()
    bpy.context.collection.objects.link(dupe)
    dupe.rotation_euler.z+=(1.22 if j==0 else 2.51)
    dupe.scale*=.94 if j==0 else .88
    dupe.location.z+=.015 if j==0 else -.04
    copies.append(dupe)
  altered=fix_leaf_colors(allobjs+copies)
  if altered==0:raise RuntimeError('Leaf albedo was not corrected')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in allobjs+copies:ob.select_set(True)
  bpy.context.view_layer.objects.active=allobjs[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<15000:raise RuntimeError('GLB bad')
  row.update({'status':'GLB_OK_ART_PENDING','leaf_mesh_duplicates':len(copies),
   'images_recolored':altered,'original_sha':hashfile(original),'candidate_sha':hashfile(dest)})
  print('REAL_BOU_GROWTH',name,'leaf copies',len(copies),flush=True)
 except Exception as e:
  row['status']='FAILED';row['error']=str(e);traceback.print_exc()
 finally:
  records.append(row)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for img in list(bpy.data.images):
   if img.users==0:bpy.data.images.remove(img)
  (out/'reports'/'evergreen_3_review.json').write_text(json.dumps({'no_composition':True,'assets':records,'all_art_approved':False,'donor':'Quaternius CC0'},indent=2))
print('THREE_CONIFER_BATCH_DONE',len(records),flush=True)
if any(r['status']!='GLB_OK_ART_PENDING' for r in records):raise RuntimeError('Conifer batch incomplete')
