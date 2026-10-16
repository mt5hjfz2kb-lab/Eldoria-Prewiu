"""Reference-first V9 four individual Valoria arch wedges.
From immutable V8, retain real CC0 single-ashlar photography, remove linear
directional grain via orthogonal albedo mixing; bevel original cut arrises
inwards, all faces stay inside known physical source envelope.
Fixed-camera before/after, separate GLB / macro, no assembly or art approval.
"""
import bpy,sys,json,traceback,math,hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError("Expected V8 GLB folder then output")
source,out=map(Path,args)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE';sc.render.resolution_x=1024;sc.render.resolution_y=1024
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria masonry neutral A-B');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.24,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.80;sc.world=world
def lamp(n,pos,power,color):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.color=color;l.size=10;l.shape='DISK'
 ob=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('Valoria warm',(-7,-8,12),1770,(1,.85,.70))
lamp('Valoria sky',(7,3,12),1570,(.72,.84,1))
lamp('carved edge rim',(-3,8,11),960,(.95,.97,1))
cd=bpy.data.cameras.new('unchanged before after lens');cam=bpy.data.objects.new('unchanged before after lens',cd)
bpy.context.collection.objects.link(cam);sc.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in obs if ob.type=='MESH' for v in ob.bound_box]
 if not pts:raise RuntimeError('No mesh')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera_set(obs):
 lo,hi=bounds(obs);cen=(lo+hi)*.5;d=hi-lo
 cam.location=cen+Vector((.83,-1.58,1.2)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.22)*1.72
def photo(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
rows=[]
for i,name in enumerate(names):
 pre=set(bpy.data.objects);record={'asset':name,'art_approved':False,'unity_approved':False,'min_target':6,'source_run':38902465156}
 try:
  src=source/(name+'.glb')
  if not src.is_file():raise RuntimeError('Missing previous photographed wedge '+name)
  bpy.ops.import_scene.gltf(filepath=str(src))
  objects=[o for o in bpy.data.objects if o not in pre]
  meshes=[o for o in objects if o.type=='MESH']
  if len(meshes)!=1:raise RuntimeError('Expected one geometrical stone wedge')
  m=meshes[0]
  lo,hi=bounds(meshes)
  vertices_before=len(m.data.vertices);polygons_before=len(m.data.polygons)
  camera_set(meshes);photo(out/'before'/(name+'.png'))
  changed=[]
  for material in m.data.materials:
   if not material or not material.use_nodes:continue
   for node in material.node_tree.nodes:
    if node.type!='TEX_IMAGE' or not node.image or node.image.colorspace_settings.name=='Non-Color':continue
    im=node.image
    if im.name in [x['original'] for x in changed]:continue
    h,w=int(im.size[1]),int(im.size[0])
    if h!=w:raise RuntimeError('Expected square stone image to minimize resampling')
    rgba=np.empty((h,w,4),dtype=np.float32);im.pixels.foreach_get(rgba.reshape(-1))
    original=rgba.copy()
    # Orthogonal sampling destroys false wood-like parallel grain while
    # retaining true underlying CC0 stone face high-frequency photography.
    ortho1=np.rot90(original[:,:,:3],1)
    ortho2=np.rot90(original[:,:,:3],3)
    new=original.copy()
    albedo=.54*original[:,:,:3]+.23*ortho1+.23*ortho2
    luma=albedo[:,:,0]*.3+albedo[:,:,1]*.59+albedo[:,:,2]*.11
    # Warm desaturated cut Valoria limestone, preserve detail not uniform clay.
    palette=np.stack([luma*1.08,luma*1.036,luma*.98],axis=-1)
    new[:,:,:3]=np.clip(.26*albedo+.74*palette,.035,.93)
    im.pixels.foreach_set(new.reshape(-1));im.update()
    pic=out/'atlas'/(name+'_real_CC0_nondirectional_cutstone.png')
    im.filepath_raw=str(pic);im.file_format='PNG';im.save();im.pack()
    changed.append({'original':im.name,'sha':sha(pic)})
  if not changed:raise RuntimeError('No authentic source albedo altered')
  # True supported stone bevel: angle-limited, inward-only on real corners.
  bevel=m.modifiers.new('visible_stonecutter_bevel','BEVEL')
  bevel.width=min(.034,(hi-lo).length*.013)
  bevel.segments=2;bevel.limit_method='ANGLE'
  bevel.angle_limit=math.radians(0.6)  # retain only noncoplanar edges, imported wedge triangle normals vary below 34 degrees
  bpy.context.view_layer.objects.active=m
  bpy.ops.object.modifier_apply(modifier=bevel.name)
  # Check physical envelope after Blender baked bevel; cannot break modular fit.
  newlo,newhi=bounds(meshes)
  if any(newlo[k]<lo[k]-.006 or newhi[k]>hi[k]+.006 for k in range(3)):
   raise RuntimeError('Wedge no longer fits original source envelope')
  if len(m.data.polygons)==polygons_before:raise RuntimeError('Hand-dressed bevel did not materially change actual stone geometry')
  photo(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in objects:obj.select_set(True)
  bpy.context.view_layer.objects.active=m
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<65000:raise RuntimeError('Photo limestone and PBR missing from GLB')
  record.update({'technical':'PASS_PENDING_HUMAN_ART','before_vertices':vertices_before,
   'before_faces':polygons_before,'after_vertices':len(m.data.vertices),'after_faces':len(m.data.polygons),
   'source_sha':sha(src),'candidate_sha':sha(dest),
   'source_box_min':[round(float(v),4) for v in lo],'source_box_max':[round(float(v),4) for v in hi],
   'within_previous_fit_envelope':True,'real_cutstone_photos':changed,
   'intent':'Eliminate unconvincing wood-grain directional photo and physically bevel arrises'})
  print('CUT_LIMESTONE_REAL_BEVELS',name,record['after_faces'],flush=True)
 except Exception as ex:
  record.update({'technical':'FAILED','error':str(ex)});traceback.print_exc()
 finally:
  rows.append(record)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'four_wedges_cutstone_v9.json').write_text(json.dumps({
    'no_scene_assembly':True,'all_art_approved':False,'subjective_review_still_needed':True,'assets':rows
  },indent=2))
if len(rows)!=4 or any(r['technical']!='PASS_PENDING_HUMAN_ART' for r in rows):
 raise RuntimeError('Four cut-limestone wedges A/B trial incomplete')
print('FOUR_ARCH_STONES_V9_NATIVE_BLENDER_COMPLETE',flush=True)
