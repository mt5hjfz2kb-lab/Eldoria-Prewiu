"""Blender individual sculpt/material microrelief pilot on 3 Valoria ashlars.
No composition and no automatic approval. Compare same source/camera.
"""
import bpy,bmesh,sys,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector,noise
import numpy as np
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)<2:raise RuntimeError('Expected source GLB folder, output')
source=Path(args[0]);out=Path(args[1])
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['stone_ashlar_01','stone_ashlar_02','stone_ashlar_03']
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Neutral Valoria ashlar A-B');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.23,.29,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.83
scene.world=w
def light(name,power,pos,color):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.shape='DISK';l.size=9
 ob=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(ob)
 ob.location=pos;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('amber warm key',1500,(-7,-7,10),(1,.86,.71))
light('neutral cool fill',1600,(5,4,9),(.65,.80,1))
light('bright rim',850,(3,10,7),(1,1,1))
camd=bpy.data.cameras.new('Quality camera');cam=bpy.data.objects.new('Quality camera',camd)
bpy.context.collection.objects.link(cam);scene.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def hashfile(path):
 h=hashlib.sha256()
 with open(path,'rb') as f:
  for k in iter(lambda:f.read(1048576),b''):h.update(k)
 return h.hexdigest()
def bbox(objs):
 p=[obj.matrix_world@Vector(c) for obj in objs for c in obj.bound_box if obj.type=='MESH']
 return Vector([min(v[i] for v in p) for i in range(3)]),Vector([max(v[i] for v in p) for i in range(3)])
def camera_for(objs):
 bpy.context.view_layer.update();lo,hi=bbox(objs);center=(lo+hi)*.5
 extent=hi-lo
 cam.location=center+Vector((.92,-1.75,1.45)).normalized()*max(extent.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(extent.x,extent.y,extent.z)*1.77
def render(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sculpt(obj):
 # Preserve hand-laid block silhouette and joining planes; perform fine localized
 # chips/pitting predominantly inside the broad limestone faces.
 mesh=obj.data
 bm=bmesh.new();bm.from_mesh(mesh)
 edges=list(bm.edges);bmesh.ops.subdivide_edges(bm,edges=edges,cuts=3,use_grid_fill=True)
 bm.normal_update()
 coords=np.array([tuple(v.co) for v in bm.verts],dtype=float)
 lo=coords.min(axis=0);hi=coords.max(axis=0);dim=np.maximum(hi-lo,1e-6)
 scale=float(np.max(dim))
 count=0
 for v in bm.verts:
  ratios=[min(1,max(0,float((v.co[i]-lo[i])/dim[i]))) for i in range(3)]
  axis=min(range(3),key=lambda i:min(ratios[i],1-ratios[i]))
  adj=[i for i in range(3) if i!=axis]
  edgemargin=min(min(ratios[i],1-ratios[i]) for i in adj)
  fade=min(1.0,max(0.0,(edgemargin-.025)*4.0))
  if fade<=0:continue
  local=v.co.copy()
  n=noise.noise(local*18)*.68+noise.noise(local*7)*.32
  disp=n*.021*scale*fade
  v.co+=v.normal*disp
  count+=1
 bm.to_mesh(mesh);bm.free();mesh.update()
 return count
def texture_weave(obj,seed):
 rng=np.random.default_rng(seed)
 edits=[]
 for mat in obj.data.materials:
  if mat is None or not mat.use_nodes:continue
  for node in mat.node_tree.nodes:
   if node.type!='TEX_IMAGE' or node.image is None:continue
   image=node.image;N=image.size[0];M=image.size[1]
   if N<64 or M<64:continue
   pix=np.empty(N*M*4,dtype=np.float32);image.pixels.foreach_get(pix)
   arr=pix.reshape((M,N,4))
   raw=rng.normal(size=(M,N)).astype(np.float32)
   fy=np.fft.fftfreq(M)[:,None];fx=np.fft.fftfreq(N)[None,:]
   radius=np.sqrt(fx*fx+fy*fy)
   cloud=np.fft.ifft2(np.fft.fft2(raw)*(1+np.square(radius*39))**-1.18).real.astype(np.float32)
   cloud=(cloud-cloud.mean())/(cloud.std()+1e-6)
   grit=rng.normal(0,.015,size=(M,N)).astype(np.float32)
   pits=(rng.random((M,N))>.993).astype(np.float32)
   attenuation=np.clip(1+.15*cloud+grit-.16*pits,.67,1.24)
   arr[:,:,:3]=np.clip(arr[:,:,:3]*attenuation[:,:,None],0,1)
   image.pixels.foreach_set(pix);image.update();image.pack()
   edits.append({'material':mat.name,'pixel_count':N*M})
 return edits
record=[]
for i,name in enumerate(names):
 before={o for o in bpy.data.objects}
 item={'asset':name,'target':8.0,'art_approved':False}
 try:
  original=source/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  imported=[o for o in bpy.data.objects if o not in before]
  meshes=[o for o in imported if o.type=='MESH']
  if not meshes:raise RuntimeError('No GLB meshes')
  camera_for(meshes);render(out/'before'/(name+'.png'))
  changed=sum(sculpt(o) for o in meshes)
  edits=[]
  for obj in meshes:edits+=texture_weave(obj,2217+i*13)
  if changed<30 or not edits:raise RuntimeError('Insufficient real face edits')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in imported:o.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  item.update({'technical':'EXPORTED_VISUAL_PENDING',
   'sculpt_vertices':changed,'materials':edits,
   'source_sha':hashfile(original),'candidate_sha':hashfile(target),
   'before_sha':hashfile(out/'before'/(name+'.png')),
   'after_sha':hashfile(out/'after'/(name+'.png'))})
  print('ARTISAN_STONE',name,changed,len(edits),flush=True)
 except Exception as e:
  item.update({'technical':'FAILED','error':str(e)})
  traceback.print_exc()
 finally:
  record.append(item)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  for img in list(bpy.data.images):
   if img.users==0:bpy.data.images.remove(img)
  (out/'reports'/'stone_3_qa.json').write_text(json.dumps({'items':record,'no_scene':True,
    'blender_provisional_only':True,'user_art_approved':False},indent=2))
if any(x['technical']!='EXPORTED_VISUAL_PENDING' for x in record):raise RuntimeError('Stone candidate batch incomplete')
print('ARTISAN_STONE_THREE_TECH_PASS')
