"""Six actual organic CC0 nature asset A/B tests for under-six modules.
Use genuine Quaternius leaf geometry from individually best saved GLB donors;
copy only leaves on pine, preserve trunk, stagger shrub/grass silhouette.
Do not synthesize a cone. Never declare >6 without separate visual review.
"""
import bpy,sys,math,json,random,hashlib,traceback
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise SystemExit('v4 CC0 shrub and grass, v5 authentic pine donor, output')
src,pines,out=map(Path,arg)
N=['grass_tuft_1','grass_tuft_2','shrub_1','shrub_2','shrub_3','pine_medium']
for sub in ('glb','before','after','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=950;s.render.resolution_y=950
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Eldoria meadow and pine independent art studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.17,.21,.28,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;s.world=w
def light(n,power,pos,col):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=col;data.shape='DISK';data.size=10
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('soft sunset canopy',1700,(-8,-9,13),(1,.87,.73))
light('northern sky fill',1430,(7,3,12),(.7,.83,1))
light('tree silhouette rim',900,(-2,9,12),(.89,.96,1))
data=bpy.data.cameras.new('identical individual study camera')
cam=bpy.data.objects.new('identical individual study camera',data)
bpy.context.collection.objects.link(cam);s.camera=cam;data.type='ORTHO'
rig=set(bpy.data.objects)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def bbox(objs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in objs if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def studio(objs):
 lo,hi=bbox(objs);c=(lo+hi)*.5;e=hi-lo
 cam.location=c+Vector((.91,-1.59,1.17)).normalized()*max(e.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 data.ortho_scale=max(e.x,e.y,e.z,.3)*1.70
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def shade_foliage(objs):
 count=0
 for o in objs:
  if o.type!='MESH':continue
  for mat in o.data.materials:
   if not mat or not mat.use_nodes:continue
   if not any(x in mat.name.lower() for x in ('leaf','leav','grass','foliage')):continue
   for bs in mat.node_tree.nodes:
    if bs.type=='BSDF_PRINCIPLED':
     bs.inputs['Roughness'].default_value=.87
     bs.inputs['Metallic'].default_value=0
     count+=1
 return count
reports=[]
for name in N:
 start=set(bpy.data.objects)
 row={'asset':name,'visual_target':6,'art_approved':False,'no_scene':True}
 try:
  original=(pines if name=='pine_medium' else src)/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  objs=[o for o in bpy.data.objects if o not in start and o.type=='MESH']
  if not objs:raise RuntimeError('No real donor greenery geometry')
  studio(objs);shoot(out/'before'/(name+'.png'))
  rng=random.Random(910+sum(ord(x) for x in name))
  copies=[]
  if name.startswith('pine'):
   chosen=[o for o in objs if any(m and 'leav' in m.name.lower() for m in o.data.materials)]
   if not chosen:raise RuntimeError('Conifer lacks independent needle leaf meshes')
   for i in range(3):
    # Add denser lower branches (not another tall cone) while preserving
    # original trunk and branch pivot from Quaternius.
    for leaf in chosen[:2]:
     dup=leaf.copy();dup.data=leaf.data.copy()
     bpy.context.collection.objects.link(dup)
     dup.name=name+'_natural_lower_bough_'+str(i)
     dup.rotation_euler.z+=.52+i*1.54
     dup.scale*=.77 if i==0 else (.86 if i==1 else .71)
     dup.location.z-=.35+i*.21
     copies.append(dup)
  elif name.startswith('shrub'):
   ob=objs[0]
   for i in range(3):
    dupe=ob.copy();dupe.data=ob.data.copy()
    bpy.context.collection.objects.link(dupe)
    dupe.name=name+'_distinct_bough_cluster_'+str(i)
    dupe.rotation_euler.z+=rng.uniform(-.8,.8)
    dupe.scale*=rng.uniform(.68,.8)
    dupe.location.x+=(-.20 if i==0 else .20 if i==1 else .045)
    dupe.location.y+=rng.uniform(-.16,.16)
    dupe.location.z+=rng.uniform(-.07,.08)
    copies.append(dupe)
  else:
   ob=objs[0]
   for i in range(3):
    dupe=ob.copy();dupe.data=ob.data.copy()
    bpy.context.collection.objects.link(dupe)
    dupe.name=name+'_asymmetric_bunch_'+str(i)
    dupe.scale*=rng.uniform(.62,.88)
    dupe.rotation_euler.z+=rng.uniform(-2.6,2.6)
    dupe.location.x+=(-.19 if i==0 else .19 if i==1 else .04)
    dupe.location.y+=rng.uniform(-.19,.19)
    dupe.location.z-=rng.uniform(.01,.06)
    copies.append(dupe)
  shade_foliage(objs+copies)
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in objs+copies:o.select_set(True)
  bpy.context.view_layer.objects.active=objs[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<10000:raise RuntimeError('Output missing original CC0 PBR textures')
  row.update({'technical':'PASS_PENDING_VISUAL',
   'source_author':'Quaternius CC0 Stylized Nature MegaKit',
   'copied_authentic_foliage_meshes':len(copies),
   'source_sha':sha(original),'candidate_sha':sha(dest)})
  print('ELDORIA_REAL_DENSER_ORGANIC_FOLIAGE',name,len(copies),flush=True)
 except Exception as e:
  row.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  reports.append(row)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'six_CC0_foliage_scattered.json').write_text(json.dumps({
   'donor_v4_run':38819524063,'pine_selected_run':38823354618,
   'all_art_approved':False,'items':reports},indent=2))
if len(reports)!=6 or any(x['technical']!='PASS_PENDING_VISUAL' for x in reports):
 raise RuntimeError('Incomplete six foliage A/B')
print('SIX_REAL_FOLIAGE_GEOMETRY_QUALITY_CANDIDATES_DONE',flush=True)
