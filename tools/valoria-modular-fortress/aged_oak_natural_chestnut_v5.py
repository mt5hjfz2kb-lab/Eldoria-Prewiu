"""Valoria CC0 oak V5: undo V4 near-black old wood while retaining actual
scanned/photographed grain and physical lap/tenon/peg geometry. Match all 3
modular wood parts to warm balanced chestnut. Separate A/B and GLBs.
No additional geometry, no scene, no automatic artistic approval.
"""
import bpy,sys,hashlib,json,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,a)
names=['timber_beam_1','timber_beam_2','timber_brace_X']
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1150;s.render.resolution_y=1150
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Same warm wood studio before after')
w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.26,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.87;s.world=w
def light(n,loc,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.size=9;d.shape='DISK'
 obj=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(obj);obj.location=loc
 obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
light('oak warm key',(-7,-8,11),1950,(1,.88,.74))
light('sky cool fill',(7,4,11),1470,(.75,.84,1))
light('back edge',(-1,9,10),980,(1,.94,.81))
data=bpy.data.cameras.new('fixed complete individual oak camera')
cam=bpy.data.objects.new('fixed complete individual oak camera',data)
bpy.context.collection.objects.link(cam);s.camera=cam;data.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(p) for o in obs if o.type=='MESH' for p in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def view(meshes):
 lo,hi=bounds(meshes);cen=(lo+hi)*.5;dim=hi-lo
 cam.location=cen+Vector((.85,-1.55,1.15)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 data.ortho_scale=max(dim.x,dim.y,dim.z,.25)*1.69
def photo(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
results=[]
for name in names:
 oldobjs=set(bpy.data.objects);r={'asset':name,'art_approved':False,'desired_threshold':6}
 try:
  source=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(source))
  new=[o for o in bpy.data.objects if o not in oldobjs]
  mesh=[o for o in new if o.type=='MESH']
  if not mesh:raise RuntimeError('No real aged oak source')
  view(mesh);photo(out/'before'/(name+'.png'))
  count=0;changed=[];processed=set()
  for obj in mesh:
   for mat in obj.data.materials:
    if not mat or not mat.use_nodes:continue
    if 'photographed_aged_oak' not in mat.name.lower():continue
    for n in mat.node_tree.nodes:
     if n.type!='TEX_IMAGE' or not n.image:continue
     im=n.image
     if im.name in processed or im.colorspace_settings.name=='Non-Color':continue
     processed.add(im.name)
     arr=np.empty(im.size[0]*im.size[1]*4,dtype=np.float32)
     im.pixels.foreach_get(arr)
     px=arr.reshape((-1,4));rgb=px[:,:3]
     lum=rgb[:,0]*.29+rgb[:,1]*.61+rgb[:,2]*.1
     middle=float(np.median(lum));dev=float(np.std(lum))+.015
     local=np.clip((lum-middle)/dev,-2.2,2.2)
     natural=np.clip(.315+.079*local,.145,.51)
     target=np.stack([natural*1.27,natural*.92,natural*.65],axis=-1)
     # Keeps scanned knots and wood fibre, only color-direction artist correction.
     rgb[:]=np.clip(target*.72+rgb*.28,.03,.78)
     im.pixels.foreach_set(arr);im.update()
     path=out/'atlas'/(name+'_true_warm_chestnut_CC0.png')
     im.filepath_raw=str(path);im.file_format='PNG';im.save();im.pack()
     changed.append({'original_image':im.name,'before_median_lum':middle,
       'new_photo_albedo_sha':sha(path)})
     count+=1
  if count<1:raise RuntimeError('Missing actual photo albedo diffuse CC0 under aged oak material')
  photo(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in new:obj.select_set(True)
  bpy.context.view_layer.objects.active=mesh[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<50000:raise RuntimeError('CC0 scanned wood UV lost')
  r.update({'technical':'PASS_PENDING_VISUAL','original_sha':sha(source),
     'candidate_sha':sha(dest),'photo_maps':changed,
     'preserved_mesh_count':len(mesh),'source_run':38881134717,
     'color_guidance':'warm middle-chestnut grain matches Valoria dusk scaffolding'})
  print('ELDORIA_REAL_WARM_HANDJOINED_OAK',name,count,flush=True)
 except Exception as e:r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(r)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0:bpy.data.images.remove(im)
  (out/'reports'/'three_oak_natural_chestnut_grade.json').write_text(json.dumps({
    'all_visual_pending':True,'three_structural_parts':True,'rows':results},indent=2))
if len(results)!=3 or any(x['technical']!='PASS_PENDING_VISUAL' for x in results):
 raise RuntimeError('Three real oak candidates required')
print('THREE_WARM_OAK_CC0_PHOTO_REGRADES_READY',flush=True)
