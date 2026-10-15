"""Photo scanned Poly Haven CC0 cliff_rock_column 02, non-destructive preserved source
and two color-graded standalone alternatives. Genuine unchanged geometry, same camera.
Correct toward Valoria rocky alpine cool blue-gray with warm sunset lighting.
"""
import bpy,sys,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src=Path(arg[0]);out=Path(arg[1])
for d in ('before','after','glb','reports'):(out/d).mkdir(exist_ok=True,parents=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1050;s.render.resolution_y=1050
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria rock neutral comparison');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.16,.20,.28,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=world
def lamp(n,pos,power,col):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=power;ld.color=col;ld.shape='DISK';ld.size=10
 ob=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('cool alpine fill',(-6,-9,12),2000,(.69,.82,1))
lamp('warm sunset',(8,3,11),1100,(1,.82,.67))
lamp('top accent',(-2,9,10),880,(.86,.96,1))
ca=bpy.data.cameras.new('single-scanned-cliff-A-B');cam=bpy.data.objects.new('single-scanned-cliff-A-B',ca)
bpy.context.collection.objects.link(cam);s.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
path=src/'cliff_rock_column__rock_face_02.glb'
if not path.is_file():raise RuntimeError('Real CC0 scan source missing')
bpy.ops.import_scene.gltf(filepath=str(path))
models=[ob for ob in bpy.data.objects if ob not in rig and ob.type=='MESH']
if not models:raise RuntimeError('Genuine scan has no geometry')
pts=[ob.matrix_world@Vector(c) for ob in models for c in ob.bound_box]
lo=Vector([min(v[i] for v in pts) for i in range(3)])
hi=Vector([max(v[i] for v in pts) for i in range(3)])
mid=(lo+hi)*.5;size=hi-lo
cam.location=mid+Vector((.88,-1.6,1.12)).normalized()*max(size.length,1)*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
ca.ortho_scale=max(size.x,size.y,size.z)*1.8
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
render(out/'before'/'rock_face_02.png')
imgs=[]
for o in models:
 for mat in o.data.materials:
  if mat is None or not mat.use_nodes:continue
  for node in mat.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image:
    name=node.image.name.lower()
    if 'diff' in name or 'base' in name or 'color' in name:
     if node.image not in imgs:imgs.append(node.image)
if not imgs:raise RuntimeError('No identified original source albedo to grade')
original={}
for im in imgs:
 N,M=im.size
 arr=np.empty(N*M*4,dtype=np.float32)
 im.pixels.foreach_get(arr)
 original[im.name]=arr
results=[]
for variant in ('stone_cool','stone_neutral'):
 try:
  for im in imgs:
   arr=original[im.name].copy()
   imdata=arr.reshape((-1,4))
   rgb=imdata[:,:3]
   lightness=np.dot(rgb,np.array([.26,.62,.12],dtype=np.float32))
   if variant=='stone_cool':
    # photo texture details preserved but hue/saturation restrained:
    # cold dark slate lavender/grey of Valoria cliff
    target=np.stack((lightness*.72,lightness*.76,lightness*.82),axis=-1)
    rgb[:]=np.clip(rgb*.34+target*.66,.013,.97)
   else:
    target=np.stack((lightness*.86,lightness*.83,lightness*.84),axis=-1)
    rgb[:]=np.clip(rgb*.48+target*.52,.013,.97)
   im.pixels.foreach_set(arr);im.update();im.pack()
  render(out/'after'/('rock_face_02_'+variant+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in models:ob.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  dest=out/'glb'/('cliff_rock_column_'+variant+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  results.append({'variant':variant,'asset':'cliff_rock_column','mesh_count':len(models),'src_sha':sha(path),
   'output_sha':sha(dest),'output_bytes':dest.stat().st_size,
   'technical':'PASS_PENDING_VISUAL','license':'Poly Haven CC0',
   'art_approved':False,'source_run':38825975637})
  print('SCANNED_ROCKFACE02_COLOR_STUDY',variant,dest.stat().st_size,flush=True)
 except Exception as e:
  results.append({'variant':variant,'technical':'FAIL','error':repr(e)})
  traceback.print_exc()
 (out/'reports'/'scanned_cliff_palette.json').write_text(json.dumps({'two_color_variants_one_asset':True,
  'art_approved':False,'subjective_visual_scoring_pending':True,'results':results},indent=2))
if len(results)!=2 or any(r['technical']!='PASS_PENDING_VISUAL' for r in results):raise RuntimeError('Incomplete scan palette test')
