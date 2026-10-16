"""Valoria 4 true keystones: repair stone-face photographic PBR with a CROPPED
single dressed ashlar face from real Poly Haven CC0 medieval_blocks_03.
Avoid both failures: V6 photographed full wall grid; V7 naturally rocky/pitted
geology. Preserve V7 actual wedge geometry, UV normals and exact envelope.
Native Blender A/B of four separate pieces, never approve art automatically.
"""
import bpy,sys,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
newbase,donorbase,out=map(Path,args)
for d in ('glb','before','after','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
patches=[
(327,431,646,599),
(333,229,620,390),
(153,30,450,191),
(341,649,636,789),
]
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1060;s.render.resolution_y=1060
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria assembled-stone-free standalone studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def lamp(n,pos,power,c):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.color=c;l.size=9;l.shape='DISK'
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('dusk warm key',(-7,-8,11),1800,(1,.85,.70))
lamp('cloud blue fill',(7,3,11),1500,(.72,.84,1))
lamp('top rim',(-3,8,10),960,(.95,.97,1))
camd=bpy.data.cameras.new('locked identical stone photography')
cam=bpy.data.objects.new('locked identical stone photography',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera(obs):
 lo,hi=bounds(obs);center=(lo+hi)*.5;d=hi-lo
 cam.location=center+Vector((.8,-1.52,1.17)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(d.x,d.y,d.z,.3)*1.72
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
records=[]
for index,name in enumerate(names):
 pre=set(bpy.data.objects)
 row={'asset':name,'art_approved':False,'target':6}
 try:
  current=newbase/(name+'.glb')
  donor=donorbase/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(current))
  objects=[o for o in bpy.data.objects if o not in pre]
  meshes=[o for o in objects if o.type=='MESH']
  if len(meshes)!=1:raise RuntimeError('Not one individual wedge')
  camera(meshes);render(out/'before'/(name+'.png'))
  # Load authentic photographed block wall only into a TEMPORARY donor scene
  # and pull one inner stone photo rectangle, not the grout joints.
  before_donor=set(bpy.data.objects)
  bpy.ops.import_scene.gltf(filepath=str(donor))
  donor_models=[o for o in bpy.data.objects if o not in before_donor]
  textures=[]
  for obj in donor_models:
   if obj.type!='MESH':continue
   for mat in obj.data.materials:
    if not mat or not mat.use_nodes:continue
    for node in mat.node_tree.nodes:
     if node.type=='TEX_IMAGE' and node.image and node.image.colorspace_settings.name!='Non-Color':
      if node.image not in textures:textures.append(node.image)
  if not textures:raise RuntimeError('Authentic photographed CC0 ashlar texture missing')
  photoim=max(textures,key=lambda im:im.size[0]*im.size[1])
  W,H=photoim.size
  data=np.empty(W*H*4,dtype=np.float32);photoim.pixels.foreach_get(data)
  rgba=data.reshape((H,W,4))
  x0,y0,x1,y1=patches[index]
  # patches coordinates in TOP-origin source original photographed wall.
  piece=rgba[H-y1:H-y0,x0:x1,:]
  if min(piece.shape[:2])<95:raise RuntimeError('Stone face crop too small')
  ys=np.minimum((np.arange(1024)*piece.shape[0]/1024).astype(int),piece.shape[0]-1)
  xs=np.minimum((np.arange(1024)*piece.shape[1]/1024).astype(int),piece.shape[1]-1)
  grade=piece[ys[:,None],xs[None,:]].copy()
  # Keep photoreal grain, nudge toward warm grey cut limestone.
  grade[:,:,:3]=np.clip(grade[:,:,:3]*np.array([1.03,1.045,1.03],dtype=np.float32),.02,.98)
  newim=bpy.data.images.new('Valoria_clean_single_ashlar_face_'+name,1024,1024,alpha=True)
  newim.pixels.foreach_set(grade.reshape(-1));newim.update()
  pic=out/'atlas'/(name+'_authentic_single_block_CC0.png')
  newim.filepath_raw=str(pic);newim.file_format='PNG';newim.save();newim.pack()
  for obj in donor_models:bpy.data.objects.remove(obj,do_unlink=True)
  # Replace ONLY visible albedo: original normal and geometry unchanged.
  replaced=0
  for obj in meshes:
   for material in obj.data.materials:
    if not material or not material.use_nodes:continue
    for node in material.node_tree.nodes:
     if node.type=='TEX_IMAGE' and node.image and node.image.colorspace_settings.name!='Non-Color':
      node.image=newim;replaced+=1
  if replaced<1:raise RuntimeError('No visual color material replaced')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in objects:o.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  dst=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<90000:raise RuntimeError('Photo stone export lacks PBR/geometry')
  row.update({'technical':'PASS_PENDING_MANUAL_ART','old_sha':sha(current),
     'candidate_sha':sha(dst),'atlas_sha':sha(pic),'donor_sha':sha(donor),
     'original_photography':'Poly Haven CC0 medieval_blocks_03, patch of one real carved stone only',
     'original_wedge_geometry_unchanged':True,'material_slots_replaced':replaced})
  print('CROPPED_TRUE_STONE_RATHER_THAN_WALL_PHOTO',name,flush=True)
 except Exception as e:
  traceback.print_exc();row.update({'technical':'FAIL','error':repr(e)})
 finally:
  records.append(row)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'four_cc0_single_ashlar_stone_faces.json').write_text(json.dumps({
    'reference_photo_source_run':38888410182,'radial_wedge_run':38901831398,
    'single_modular_assets_only':True,'all_art_approved':False,'items':records},indent=2))
if len(records)!=4 or any(r['technical']!='PASS_PENDING_MANUAL_ART' for r in records):
 raise RuntimeError('Four true stone crops incomplete')
print('FOUR_REAL_DRESSED_CC0_STONES_COMPLETE',flush=True)
