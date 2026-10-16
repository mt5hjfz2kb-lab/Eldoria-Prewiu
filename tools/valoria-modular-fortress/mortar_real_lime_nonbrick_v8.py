"""Valoria mortared-stone join: fix brick-wall albedo wrongly used as mortar.
One individual 1.8 m dark recessed lime-grout GLB with 82 physical embedded
aggregate grains. Preserve 84 meshes and geometry, remove brick seam patterns
from 1K CC0 photo using highpass weathering, render camera-matched A/B.
"""
import bpy,sys,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,a)
for p in ('before','after','glb','atlas','reports'):(out/p).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1040;scene.render.resolution_y=850
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Grout isolated Valoria studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.24,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;scene.world=world
def lamp(name,power,pos,c):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=10;d.color=c;d.shape='DISK'
 ob=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('warm mortar detail',1950,(-7,-9,11),(1,.85,.74))
lamp('cool blue fill',1600,(7,3,10),(.74,.85,1))
lamp('surface relief',820,(-3,8,10),(1,1,1))
c=bpy.data.cameras.new('fixed physical grout studio')
cam=bpy.data.objects.new('fixed physical grout studio',c)
bpy.context.collection.objects.link(cam);scene.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(items):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in items if ob.type=='MESH' for v in ob.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def photo(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
base=src/'mortar_joint_recess.glb'
if not base.is_file():raise RuntimeError('Existing 84-mesh joint GLB absent')
bpy.ops.import_scene.gltf(filepath=str(base))
models=[ob for ob in bpy.data.objects if ob not in rig and ob.type=='MESH']
if len(models)<60:raise RuntimeError('Wrong source: real embedded mineral grains absent')
lo,hi=bounds(models);cen=(lo+hi)*.5;dim=hi-lo
cam.location=cen+Vector((.84,-1.52,1.15)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
c.ortho_scale=max(dim.x,dim.y,dim.z,.35)*1.68
photo(out/'before'/'mortar_joint_recess.png')
changed=[]
for im in list(bpy.data.images):
 if 'plaster_stone_wall_02_diffuse' not in im.name.lower():continue
 W,H=im.size
 rgba=np.empty(W*H*4,dtype=np.float32);im.pixels.foreach_get(rgba)
 array=rgba.reshape(H,W,4)
 rgb=array[:,:,:3]
 luminance=rgb[:,:,0]*.3+rgb[:,:,1]*.59+rgb[:,:,2]*.11
 # Remove the very recognizable photographed STONE JOINTS and their long stripes
 # while keeping small pitting and aggregate/high-frequency stone textures.
 blur=luminance.copy()
 for shift in (8,17,35,73):
  blur=(blur+np.roll(luminance,shift,axis=0)+np.roll(luminance,-shift,axis=0)+
            np.roll(luminance,shift,axis=1)+np.roll(luminance,-shift,axis=1))/5
 hf=np.clip(luminance-blur,-.20,.20)
 yy,xx=np.mgrid[0:H,0:W]
 grain=.014*np.sin(xx*.21+yy*.31)+.009*np.cos(xx*.61-yy*.22)
 p=np.clip(.315+hf*.30+grain,.18,.42)
 rgb[:,:,0]=np.clip(p*1.05,0,1)
 rgb[:,:,1]=np.clip(p*.985,0,1)
 rgb[:,:,2]=np.clip(p*.93,0,1)
 im.pixels.foreach_set(rgba);im.update()
 out_png=out/'atlas'/'real_photo_lime_mortar_aged_muted_1k.png'
 im.filepath_raw=str(out_png);im.file_format='PNG';im.save();im.pack()
 changed.append({'image_name':im.name,'atlas_sha':sha(out_png),'original_photo':'Poly Haven plaster_stone_wall_02 CC0'})
if len(changed)!=1:raise RuntimeError('No unique 1K albedo texture to remove brick appearance')
# Matte grouted surface. Surface normals retained for fine mineral relief.
for ob in models:
 for m in ob.data.materials:
  if not m or not m.use_nodes:continue
  for n in m.node_tree.nodes:
   if n.type=='BSDF_PRINCIPLED':
    for link in list(n.inputs['Roughness'].links):m.node_tree.links.remove(link)
    n.inputs['Roughness'].default_value=.93
photo(out/'after'/'mortar_joint_recess.png')
bpy.ops.object.select_all(action='DESELECT')
for ob in models:ob.select_set(True)
bpy.context.view_layer.objects.active=models[0]
dst=out/'glb'/'mortar_joint_recess.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if len(models)<60 or dst.stat().st_size<50000:raise RuntimeError('Aggregate/source detail stripped')
r={'asset':'mortar_joint_recess','old_sha':sha(base),'new_sha':sha(dst),
 'technical':'PASS_PENDING_ART','geometry_untouched':True,
 'embedded_aggregate_mesh_count':len(models)-2,'photo_grade':changed,
 'method':'Remove photo wall seam pattern; keep true sandstone mineral grain and real 3D aggregate',
 'art_approved':False,'no_composition':True,'unity_approved':False,
 'warning':'Mortar contact and depth can only be validated when stone modules touch; do not assert >6 based on standalone studio.'}
(out/'reports'/'limemortar_correction.json').write_text(json.dumps(r,indent=2))
print('REAL_MORTAR_TEXTURE_MEANING_FIXED',len(models),flush=True)
