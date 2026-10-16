"""Five individually art-directed natural ground assets: 3 Quaternius CC0 shrubs,
2 CC0 grass tuft sources. Build asymmetric overlapping REAL leaf/grass geometry
rather than symmetric cones or synthetic colored rocks. Same-camera Blender A/B.
"""
import bpy,sys,random,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,arg)
for d in ('before','after','glb','reports'):(out/d).mkdir(exist_ok=True,parents=True)
names=['shrub_1','shrub_2','shrub_3','grass_tuft_1','grass_tuft_2']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=980;s.render.resolution_y=980
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria natural editorial studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.24,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
def area(n,pos,power,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=8
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('warm distant dusk',(-6,-8,11),1650,(1,.87,.7))
area('cool alpine sky',(8,2,10),1540,(.67,.81,1))
area('back-lit fronds',(2,8,11),910,(1,.98,.91))
c=bpy.data.cameras.new('locked individual plant compare');cam=bpy.data.objects.new('locked individual plant compare',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def bb(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camset(obs):
 lo,hi=bb(obs);cen=(lo+hi)*.5;dim=hi-lo
 cam.location=cen+Vector((.9,-1.55,1.16)).normalized()*max(dim.length,1)*3.2
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 c.ortho_scale=max(dim.x,dim.y,dim.z,.3)*1.95
def photo(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def recolor(models,target):
 # Photo-detail preserved: leafy pixels retain locally different brightness;
 # avoid cyan pines and uniform dull flat monotone tint.
 images=set()
 for o in models:
  for m in o.data.materials:
   if not m or not m.use_nodes:continue
   for n in m.node_tree.nodes:
    if n.type=='TEX_IMAGE' and n.image:images.add(n.image)
 if not images:raise RuntimeError('CC0 photofoliage albedo not found')
 for im in images:
  size=int(im.size[0]*im.size[1])
  rgba=np.empty(size*4,dtype=np.float32)
  im.pixels.foreach_get(rgba);pixels=rgba.reshape((-1,4))
  rgb=pixels[:,:3]
  lum=np.dot(rgb,np.array([.30,.59,.11],dtype=np.float32))
  rel=np.clip((lum-float(lum.mean()))/(float(lum.std())+.027),-2.1,2.1)
  base=np.array(target,dtype=np.float32)
  # leave fine illumination/vein variation and subtle natural green/yellow variation
  rgb[:]=np.clip(base[None,:]*(1+.20*rel[:,None])+.12*(rgb-rgb.mean(axis=0)),.022,.95)
  im.pixels.foreach_set(rgba);im.update();im.pack()
 return len(images)
results=[]
for idx,name in enumerate(names):
 original=src/(name+'.glb');pre=set(bpy.data.objects)
 rec={'asset':name,'art_approved':False,'minimum_requested':6,'phase':'individual_only'}
 try:
  bpy.ops.import_scene.gltf(filepath=str(original))
  models=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if not models:raise RuntimeError('CC0 plant GLB has no geometry')
  camset(models);photo(out/'before'/(name+'.png'))
  foliage='shrub' in name
  color=[(.17,.27,.13),(.22,.32,.16),(.18,.30,.14)][idx] if foliage else (.20,.325,.14)
  tex_count=recolor(models,color)
  rng=random.Random(145+idx*15)
  clones=[]
  for j in range(3 if not foliage else 2):
   for ob in models:
    duplicate=ob.copy();duplicate.data=ob.data.copy()
    bpy.context.collection.objects.link(duplicate)
    duplicate.name='real_leaf_frond_cluster_'+str(j)
    fraction=rng.uniform(.58,.79) if foliage else rng.uniform(.70,.91)
    duplicate.scale*=fraction
    dims=ob.dimensions
    if foliage:
     duplicate.location.x+=(-1 if j%2 else 1)*float(dims.x)*rng.uniform(.23,.31)
     duplicate.location.y+=(-1 if j%2 else 1)*float(dims.y)*rng.uniform(.06,.14)
     duplicate.location.z-=float(dims.z)*rng.uniform(.04,.12)
    else:
     duplicate.location.x+=(-1 if j%2 else 1)*float(dims.x)*rng.uniform(.08,.18)
     duplicate.location.y+=(-1 if j%2 else 1)*float(dims.y)*rng.uniform(.10,.22)
     duplicate.location.z-=float(dims.z)*.015
    duplicate.rotation_euler.z+=rng.uniform(-1.5,1.5)
    clones.append(duplicate)
  photo(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in models+clones:ob.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  final=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(final),export_format='GLB',use_selection=True,export_apply=True)
  if final.stat().st_size<18000:raise RuntimeError('GLB empty')
  rec.update({'status':'BLENDER_PASS_VISUAL_PENDING','original_sha':sha(original),
     'candidate_sha':sha(final),'foliage_material_images_recolored':tex_count,
     'added_real_leaf_geometry':len(clones),
     'license':'Quaternius CC0 stylized nature donor'})
  print('REAL_BRANCHES_BUSH_GRASS',idx+1,'/5',name,len(clones),flush=True)
 except Exception as e:
  rec.update({'status':'FAILED','error':str(e)});traceback.print_exc()
 finally:
  results.append(rec)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0:bpy.data.images.remove(im)
  (out/'reports'/'quaternius_five_realfoliage.json').write_text(json.dumps({
    'source_run':38819524063,'new_art_status':'VISUAL_GATE_PENDING',
    'art_approved':False,'assets':results},indent=2))
if len(results)!=5 or any(x['status']!='BLENDER_PASS_VISUAL_PENDING' for x in results):
 raise RuntimeError('CC0 shrub/grass studio failed')
print('QUATERNIUS_5_FOLIAGE_REAL_GEOMETRY_DONE',flush=True)
