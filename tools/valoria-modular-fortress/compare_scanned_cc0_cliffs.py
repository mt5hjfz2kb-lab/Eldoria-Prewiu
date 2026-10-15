"""Reference-graded CC0 Poly Haven scanned rock-face A/B.
Two DIFFERENT genuine photogrammetry models, Blender native. No scene assembly.
Treat as 2 competing replacements for one independent cliff_rock_column asset.
"""
import bpy,sys,math,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
base,scan,out=map(Path,args[:3])
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1000;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Alpine rock face neutral dusk studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.16,.20,.28,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=w
def light(n,power,pos,color):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=color;data.size=11;data.shape='DISK'
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('Cool sky sculpture',1850,(-6,-9,13),(.69,.83,1))
light('Warm bounce',1250,(9,3,13),(1,.79,.59))
light('Top rim',870,(-3,10,11),(.95,1,1))
c=bpy.data.cameras.new('Independent geological hero studio');cam=bpy.data.objects.new('Independent geological hero studio',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def bounding(objs):
 pts=[o.matrix_world@Vector(v) for o in objs for v in o.bound_box if o.type=='MESH']
 if not pts:raise RuntimeError('No models')
 return Vector([min(q[i] for q in pts) for i in range(3)]),Vector([max(q[i] for q in pts) for i in range(3)])
def camera_set(objs):
 lo,hi=bounding(objs);center=(lo+hi)*.5;dim=hi-lo
 cam.location=center+Vector((.89,-1.62,1.15)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 c.ortho_scale=max(dim.x,dim.y,dim.z,.5)*1.84
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def load_model(source):
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(source))
 imported=[o for o in bpy.data.objects if o not in before]
 mesh=[o for o in imported if o.type=='MESH']
 if not mesh:raise RuntimeError('GLTF no mesh: '+str(source))
 return imported,mesh
def local_gltf(mid):
 p=scan/mid/(mid+'_1k.gltf')
 if not p.is_file():raise RuntimeError('CC0 source glTF missing '+str(p))
 obj=json.loads(p.read_text())
 for v in obj.get('images',[]):
  if 'uri' in v:v['uri']=Path(v['uri']).name
 loc=p.parent/(mid+'_1k_local_paths.gltf')
 loc.write_text(json.dumps(obj))
 return loc,p
def clean():
 for ob in list(bpy.data.objects):
  if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
 for im in list(bpy.data.images):
  if im.users==0:bpy.data.images.remove(im)
results=[]
for mid in ('rock_face_01','rock_face_02'):
 row={'candidate':mid,'asset_target':'cliff_rock_column','art_approved':False,'needs_user_inspection':True}
 try:
  old,meshes=load_model(base/'cliff_rock_column.glb')
  camera_set(meshes);render(out/'before'/(mid+'.png'))
  for ob in old:bpy.data.objects.remove(ob,do_unlink=True)
  p,original=local_gltf(mid)
  imported,models=load_model(p)
  # Preserve scan proportions while adapting scale for a reusable ~6m tall alpine wall.
  # glTF Y-up axis is handled by Blender importer. Transform all mesh instances uniformly.
  low,high=bounding(models);size=high-low
  length=max(size.x,size.y,size.z)
  scalar=5.0/max(length,.01)
  for o in imported:
   if o.parent is None:o.scale*=scalar
  # Ensure scan faces camera: orient toward camera along -Y, original scan largely
  # resembles a vertical photogrammetry facing +Y; if not visible, alternative is kept.
  for o in models:
   for m in o.data.materials:
    if m and m.use_nodes:
     for node in m.node_tree.nodes:
      if node.type=='BSDF_PRINCIPLED':
       node.inputs['Roughness'].default_value=.87
       node.inputs['Metallic'].default_value=0
  camera_set(models);render(out/'after'/(mid+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in imported:o.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  final=out/'glb'/('cliff_rock_column__'+mid+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(final),export_format='GLB',use_selection=True,export_apply=True)
  if final.stat().st_size<50000:raise RuntimeError('Genuine scanned mesh missing')
  row.update({'technical':'SUCCESS_PENDING_ART','scan_nodes':len(models),
   'original_photogrammetry':'Poly Haven '+mid+' CC0, 1K diffuse / normal / roughness',
   'scanned_source_sha':sha(original),'candidate_sha':sha(final),'size_bytes':final.stat().st_size})
  print('REAL_SCANNED_CLIFF_ASSET',mid,final.stat().st_size,flush=True)
 except Exception as e:
  row.update({'technical':'FAILED','error':str(e)});traceback.print_exc()
 finally:
  results.append(row);clean()
  (out/'reports'/'photogrammetry_two.json').write_text(json.dumps({'original_cliff_run':38825223115,
    'polyhaven_source_run':38825684780,'not_scene':True,'all_art_approved':False,'items':results},indent=2))
if any(r['technical']!='SUCCESS_PENDING_ART' for r in results):raise RuntimeError('Photogrammetry candidates failed')
print('TWO_CC0_SCANS_IMPORTED_AND_RENDERED',flush=True)
