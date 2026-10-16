"""Native Blender color-only repair of two genuine Poly Haven CC0 1K scanned rocks.
Do NOT procedural-remodel actual scan geometry. Preserve photo details and normals,
lift warm wet red appearance to matte cold blue-gray Valoria cliffs/pebbles.
Fixed camera before/after, separate GLB, evidence report. No art auto-approval.
"""
import bpy,sys,json,hashlib,traceback,math
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError('Args source-folder, output-folder')
source,out=map(Path,args)
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
names=['boulder_2','cliff_stone_chip']
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1040;sc.render.resolution_y=1040
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria rock island cool lookdev');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;sc.world=world
def lamp(n,position,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.size=10;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=position
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('sunset amber key',(-7,-9,13),1900,(1,.86,.75))
lamp('blue alpine sky',(8,3,12),1750,(.74,.83,1))
lamp('rim',(-2,9,10),1000,(.93,.97,1))
camd=bpy.data.cameras.new('same rock A B lens');cam=bpy.data.objects.new('same rock A B lens',camd)
bpy.context.collection.objects.link(cam);sc.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def box(models):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(p) for o in models if o.type=='MESH' for p in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def setcam(models):
 lo,hi=box(models);mid=(lo+hi)*.5;d=hi-lo
 cam.location=mid+Vector((.85,-1.56,1.13)).normalized()*max(d.length,1)*2.85
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(d.x,d.y,d.z,.35)*1.72
def shoot(path):sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
records=[]
for name in names:
 oldobjs=set(bpy.data.objects);r={'asset':name,'art_approved':False,'target_min_score':6}
 try:
  original=source/(name+'.glb')
  if not original.is_file():raise RuntimeError('Missing actual Poly Haven donor GLB')
  bpy.ops.import_scene.gltf(filepath=str(original))
  objs=[o for o in bpy.data.objects if o not in oldobjs]
  meshes=[o for o in objs if o.type=='MESH']
  if not meshes:raise RuntimeError('Empty source 3D scan')
  setcam(meshes)
  before_verts=sum(len(o.data.vertices) for o in meshes)
  before_poly=sum(len(o.data.polygons) for o in meshes)
  shoot(out/'before'/(name+'.png'))
  modifications=[]
  images_seen=set()
  for obj in meshes:
   for m in obj.data.materials:
    if not m or not m.use_nodes:continue
    nodes=m.node_tree.nodes;links=m.node_tree.links
    for n in nodes:
     if n.type=='BSDF_PRINCIPLED':
      # Truly matte rock, not glossy wet or satin.
      for wire in list(n.inputs['Roughness'].links):links.remove(wire)
      n.inputs['Roughness'].default_value=.86
      for wire in list(n.inputs['Metallic'].links):links.remove(wire)
      n.inputs['Metallic'].default_value=0.0
    for n in nodes:
     if n.type!='TEX_IMAGE' or not n.image:continue
     im=n.image
     imname=im.name.lower()
     if not ('diff' in imname or 'basecolor' in imname or 'albedo' in imname):continue
     if im.name in images_seen:continue
     images_seen.add(im.name)
     size=im.size[:]
     array=np.empty(size[0]*size[1]*4,dtype=np.float32)
     im.pixels.foreach_get(array);rgba=array.reshape((-1,4));pixels=rgba[:,:3]
     lum=np.dot(pixels,np.array([.3,.59,.11],dtype=np.float32))
     average=float(np.median(lum));stdev=float(np.std(lum))
     contrast=np.clip((lum-average)/(stdev+.015),-2.3,2.3)
     base=.34 if name=='boulder_2' else .30
     gray=np.clip(base+.10*contrast,.13,.53)
     target=np.stack([gray*.94,gray*.97,gray*1.055],axis=-1)
     # Original photo detail is preserved through contrast and a small original chroma contribution.
     source_gray=lum[:,None]
     coloration=np.clip(pixels-source_gray,-.09,.09)
     rgba[:,:3]=np.clip(target+.06*coloration,.022,.78)
     im.pixels.foreach_set(array);im.update()
     new=out/'atlas'/(name+'_alpine_grey_CC0.png')
     im.filepath_raw=str(new);im.file_format='PNG';im.save();im.pack()
     modifications.append({'name':im.name,'image':new.name,'source_median':average,
       'source_stddev':stdev,'graded_sha':sha(new)})
  if not modifications:raise RuntimeError('No CC0 scanned diffuse texture detected')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in objs:obj.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  glb=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
  if glb.stat().st_size<60000:raise RuntimeError('Scan lost geometry')
  after_verts=sum(len(o.data.vertices) for o in meshes)
  after_poly=sum(len(o.data.polygons) for o in meshes)
  if (before_verts,before_poly)!=(after_verts,after_poly):raise RuntimeError('Scanned topology changed')
  r.update({'technical':'PASS_PENDING_ART','mesh_count':len(meshes),'original_sha':sha(original),
    'candidate_sha':sha(glb),'unchanged_vertex_count':after_verts,'unchanged_face_count':after_poly,
    'baked_photo_grade':modifications,'license':'Poly Haven CC0'})
  print('SCANNED_GEOLOGY_COLOR_CORRECTED',name,after_verts,flush=True)
 except Exception as exc:
  r.update({'technical':'FAILED','error':repr(exc)});traceback.print_exc()
 finally:
  records.append(r)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  for image in list(bpy.data.images):
   if image.users==0:bpy.data.images.remove(image)
  (out/'reports'/'two_grey_scanned_rocks.json').write_text(json.dumps({
   'source_run':38873968905,'needs_art_review':True,'assets':records,
   'no_scene_assembly':True},indent=2))
if len(records)!=2 or any(r['technical']!='PASS_PENDING_ART' for r in records):
 raise RuntimeError('Corrected geology study is incomplete')
print('TWO_REAL_SCANNED_ROCK_PHOTOGRAMMETRY_GRADED',flush=True)
