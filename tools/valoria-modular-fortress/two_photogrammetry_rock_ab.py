"""Individual original-to-authentic-scan Blender A/B for cliff chip and boulder 2.
Use official 1K CC0 glTF with complete maps, preserve scanned full geometry and UV.
Each object's camera is LOCKED once from union of original/donor dimensions;
never transform both images independently and call them comparable.
"""
import bpy,sys,json,math,hashlib,traceback
from pathlib import Path
from mathutils import Vector,Matrix
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=3:raise SystemExit('original-63-GLBs, CC0 source folder, output')
original,scan,out=map(Path,A)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
items=[('cliff_stone_chip','rock_07'),('boulder_2','boulder_01')]
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1024
scene.render.resolution_y=1024;scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Alpine single true rock CC0 study');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.17,.22,.30,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.79;scene.world=world
def lamp(n,power,loc,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=10
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=loc
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('cold sky sculpt',2000,(-7,-9,13),(.80,.88,1))
lamp('amber bounce',950,(7,3,11),(1,.84,.70))
lamp('cliff rim',1150,(-2,8,12),(.9,.96,1))
cd=bpy.data.cameras.new('Matched union extent camera');cam=bpy.data.objects.new('Matched union extent camera',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bbox(objects):
 bpy.context.view_layer.update()
 p=[o.matrix_world@Vector(v) for o in objects if o.type=='MESH' for v in o.bound_box]
 return Vector([min(v[i] for v in p) for i in range(3)]),Vector([max(v[i] for v in p) for i in range(3)])
def view_union(a,b):
 aa,bb=bbox(a);cc,dd=bbox(b)
 lo=Vector(tuple(min(aa[i],cc[i]) for i in range(3)))
 hi=Vector(tuple(max(bb[i],dd[i]) for i in range(3)))
 cen=(lo+hi)*.5;size=hi-lo
 cam.location=cen+Vector((.89,-1.59,1.15)).normalized()*max(size.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(size.x,size.y,size.z,.25)*1.78
def shoot(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def load(source):
 pre=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(source))
 obs=[x for x in bpy.data.objects if x not in pre]
 meshes=[x for x in obs if x.type=='MESH']
 if not meshes:raise RuntimeError('No imported geometry')
 return obs,meshes
def local_glTF(mid):
 directory=scan/mid
 source=next(iter(directory.glob('*_1k.gltf')),None)
 if source is None:
  candidate=next(iter(directory.glob('*.glb')),None)
  if candidate:return candidate,candidate
  raise RuntimeError('CC0 no 1K glTF '+mid)
 gltf=json.loads(source.read_text(encoding='utf-8'))
 for element in gltf.get('images',[])+gltf.get('buffers',[]):
  if isinstance(element.get('uri'),str) and not element['uri'].startswith('data:'):
   element['uri']=Path(element['uri']).name
 target=directory/(mid+'_local_import.gltf')
 target.write_text(json.dumps(gltf),encoding='utf-8')
 return target,source
results=[]
for asset,mid in items:
 original_objects=[];models=[]
 record={'asset':asset,'cc0_source':mid,'art_approved':False,'target_min':6,'no_scene_assembly':True}
 try:
  oldpath=original/(asset+'.glb')
  if not oldpath.is_file():raise RuntimeError('Baseline absent '+str(oldpath))
  olds,omesh=load(oldpath)
  sourcefile,actualfile=local_glTF(mid)
  models,nmesh=load(sourcefile)
  oldlo,oldhi=bbox(omesh);oldcenter=(oldlo+oldhi)/2;oldsize=oldhi-oldlo
  dlow,dhigh=bbox(nmesh);dsize=dhigh-dlow
  if min(dsize.x,dsize.y,dsize.z)<.00001:raise RuntimeError('Scanned donor collapsed')
  # Preserve scanned shape aspect and thickness, adapt largest source axis to
  # the largest native 3D module dimension.
  target=max(oldsize.x,oldsize.y,oldsize.z)
  ratio=target/max(dsize.x,dsize.y,dsize.z)
  for ob in models:
   if ob.parent is None:ob.matrix_world=Matrix.Scale(ratio,4)@ob.matrix_world
  bpy.context.view_layer.update()
  dlo,dhi=bbox(nmesh);dmid=(dlo+dhi)*.5
  for ob in models:
   if ob.parent is None:ob.matrix_world=Matrix.Translation(oldcenter-dmid)@ob.matrix_world
  bpy.context.view_layer.update()
  materials=[m for o in nmesh for m in o.data.materials if m and m.use_nodes]
  photos=[n.image.name for m in materials for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
  if not photos:raise RuntimeError('Lost verified donor photographs')
  for m in materials:
   for n in m.node_tree.nodes:
    if n.type=='BSDF_PRINCIPLED':
     n.inputs['Roughness'].default_value=.84
     n.inputs['Metallic'].default_value=0
  # Camera locked across BOTH alternatives at UNION A/B bounds.
  view_union(omesh,nmesh)
  for ob in models:ob.hide_render=True
  shoot(out/'before'/(asset+'.png'))
  for ob in olds:ob.hide_render=True
  for ob in models:ob.hide_render=False
  shoot(out/'after'/(asset+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in models:ob.select_set(True)
  bpy.context.view_layer.objects.active=nmesh[0]
  dest=out/'glb'/(asset+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<40000:raise RuntimeError('Real scanned source collapsed')
  record.update({'technical':'PASS_PENDING_ART','real_mesh_count':len(nmesh),'source_sha':sha(actualfile),
   'original_sha':sha(oldpath),'candidate_sha':sha(dest),
   'photo_texture_count':len(photos),'photo_images':sorted(set(photos)),
   'cc0_author':'Poly Haven CC0','scale_relative_to_old_max_dimension':ratio})
  print('AUTHENTIC_PHOTOGRAMMETRY_A_B',asset,mid,len(nmesh),dest.stat().st_size,flush=True)
 except Exception as e:
  record.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(record)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0:bpy.data.images.remove(im)
  (out/'reports'/'two_real_CC0_rock_scans.json').write_text(json.dumps({
   'purpose':'Two individual model replacements to lift lowest rock assets, no scene',
   'not_art_approved':True,'assets':results},indent=2))
if len(results)!=2 or any(x['technical']!='PASS_PENDING_ART' for x in results):
 raise RuntimeError('Some real 1K CC0 boulder test incomplete')
print('TWO_PHOTOGRAPHIC_STONE_INDEPENDENTS_DONE',flush=True)
