"""Isolated Blender finishing of twelve CC0 nature/rock donors, zero payments.
Source is authentic Quaternius donor GLB from verified Blender A/B artifact.
Never changes gameplay/Unity or declares ART_APPROVED. Keep individual A/B camera.
"""
import bpy,sys,math,json,hashlib,random,traceback
from pathlib import Path
import numpy as np
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)<2:raise SystemExit('source-glb-folder output-folder')
source=Path(arg[0]);out=Path(arg[1])
for f in ('glb','before','after','reports'):(out/f).mkdir(parents=True,exist_ok=True)
names=['pine_small','pine_medium','pine_tall','boulder_1','boulder_2','boulder_3','cliff_stone_chip','shrub_1','shrub_2','shrub_3','grass_tuft_1','grass_tuft_2']
tints={'pine_small':(91,137,106),'pine_medium':(91,143,111),'pine_tall':(79,127,100),
'shrub_1':(108,154,91),'shrub_2':(126,173,105),'shrub_3':(95,141,86),
'grass_tuft_1':(169,206,95),'grass_tuft_2':(156,195,87)}
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
try:scene.render.engine='BLENDER_EEVEE'
except Exception:scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=768;scene.render.resolution_y=768
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX' if 'AgX' in [item.name for item in scene.view_settings.bl_rna.properties['view_transform'].enum_items] else 'Standard'
world=bpy.data.worlds.new('Single module neutral dusk backdrop');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.23,.29,.36,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.75;scene.world=world
def light(name,pos,power,color,size):
 ld=bpy.data.lights.new(name,'AREA');ld.energy=power;ld.color=color;ld.shape='DISK';ld.size=size
 ob=bpy.data.objects.new(name,ld);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('Soft amber key',(-8,-10,14),1450,(1,.86,.72),10)
light('Cool blue sky fill',(7,3,11),1550,(.69,.79,1),10)
light('Neutral bright rim',(-2,8,10),900,(1,1,1),9)
c=bpy.data.cameras.new('Single asset paired review camera');cam=bpy.data.objects.new('Single asset paired review camera',c)
bpy.context.collection.objects.link(cam);scene.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
records=[]
def digest(path):
 h=hashlib.sha256()
 with open(path,'rb') as f:
  for ch in iter(lambda:f.read(1048576),b''):h.update(ch)
 return h.hexdigest()
def bbox(meshes):
 pts=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box if o.type=='MESH']
 if not pts:raise RuntimeError('no bound points')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def setup_camera(meshes):
 bpy.context.view_layer.update()
 low,high=bbox(meshes);mid=(low+high)*.5;ext=high-low
 v=Vector((.73,-1.40,1.05)).normalized()
 cam.location=mid+v*max(ext.length*.78,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 c.ortho_scale=max(ext.x,ext.y,ext.z,.3)*1.7
def render(path):
 scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def retint(image,kind,target):
 if image is None or image.size[0]<4:return None
 count=image.size[0]*image.size[1]*4
 vals=np.empty(count,dtype=np.float32);image.pixels.foreach_get(vals)
 arr=vals.reshape((-1,4));rgb=arr[:,:3];alpha=arr[:,3]
 valid=alpha>.07
 if not np.any(valid):return None
 intensity=rgb[:,0]*.23+rgb[:,1]*.61+rgb[:,2]*.16
 median=float(np.median(intensity[valid]))
 if kind=='stone':
  rgb[:]=np.clip(rgb*np.array([.94,.94,1.02],dtype=np.float32),0,1)
 else:
  toning=np.clip(.87+.36*(intensity-median)/(median+1e-4),.60,1.24)
  rgb[:]=np.clip(np.asarray(target,dtype=np.float32)[None,:]/255*toning[:,None],0,1)
 image.pixels.foreach_set(vals);image.update()
 return {'size':list(image.size),'pixels_modified':int(valid.sum()),
         'tint_srgb':list(target) if target is not None else None}
def clean():
 for ob in list(bpy.data.objects):
  if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
 for im in list(bpy.data.images):
  if im.users==0:bpy.data.images.remove(im)
for index,name in enumerate(names,1):
 glb=source/(name+'.glb')
 item={'asset':name,'original_source_sha':digest(glb) if glb.is_file() else '',
       'external_author':'Quaternius','external_license':'CC0',
       'art_approved':False,'target_score':8,'status':'VISUAL_REVIEW_REQUIRED'}
 try:
  bpy.ops.import_scene.gltf(filepath=str(glb))
  imported=[o for o in bpy.data.objects if o not in rig]
  meshes=[o for o in imported if o.type=='MESH']
  if not meshes:raise RuntimeError('Imported 0 meshes')
  setup_camera(meshes)
  render(out/'before'/(name+'.png'))
  tint_count=0;edits=[]
  for obj in meshes:
   for mat in obj.data.materials:
    if mat is None or not mat.use_nodes:continue
    mat_name=mat.name.lower()
    kind=('leaf' if name.startswith(('pine','shrub')) and 'leav' in mat_name
        else 'grass' if name.startswith('grass') and 'grass' in mat_name
        else 'stone' if name.startswith(('boulder','cliff')) and mat_name
        else '')
    if not kind:continue
    for node in mat.node_tree.nodes:
     if node.type!='TEX_IMAGE' or node.image is None:continue
     # Avoid tinting source bark. Preserve PNG alpha and UV.
     detail=retint(node.image,kind,tints.get(name))
     if detail:
      tint_count+=1;detail['material']=mat.name;edits.append(detail)
      node.image.pack()
    for node in mat.node_tree.nodes:
     if node.type=='BSDF_PRINCIPLED':
      node.inputs['Roughness'].default_value=.89
      node.inputs['Metallic'].default_value=0
  if tint_count==0:raise RuntimeError('No UV texture was changed; reject silent no-op')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in imported:o.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  final=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(final),export_format='GLB',use_selection=True,export_apply=True)
  if final.stat().st_size<3000:raise RuntimeError('Export suspiciously small')
  item.update({'status':'TECH_PASS_VISUAL_PENDING','material_edits':edits,
   'export_sha':digest(final),'file_bytes':final.stat().st_size,
   'paired_before_sha':digest(out/'before'/(name+'.png')),
   'paired_after_sha':digest(out/'after'/(name+'.png'))})
  print('ELDORIA_INDIVIDUAL_RECOLOR',index,'/',len(names),name,len(edits),flush=True)
 except Exception as e:
  item['status']='TECH_FAIL';item['error']=str(e)
  print('ASSET_FAILURE',name,str(e),flush=True);traceback.print_exc()
 finally:
  records.append(item)
  (out/'reports'/'v4_individual_manifest.json').write_text(json.dumps({
   'producer':'Native Blender isolated 12 object PBR uplift','base_artifact':38815831008,
   'status':'TECH_ONLY_NO_ART_APPROVAL','items':records,'count':len(records),
   'no_composition':True},indent=2))
  clean()
print('INDIVIDUAL_DONE',sum(r['status']=='TECH_PASS_VISUAL_PENDING' for r in records),len(names),flush=True)
if any(r['status']!='TECH_PASS_VISUAL_PENDING' for r in records):raise RuntimeError('One or more asset candidates failed')
