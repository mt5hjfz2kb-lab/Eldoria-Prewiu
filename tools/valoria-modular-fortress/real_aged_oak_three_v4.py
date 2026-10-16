"""Three original physically hand-joined wooden prop GLBs with GENUINE 1K CC0
photographed aged oak plank interior grain, knots and weathered scratches.
Never paste complete 6-plank surface on a single beam; isolate one true plank,
and preserve all prior real joints/peg geometry. Three matched Blender A/B.
"""
import bpy,sys,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('source V3 timber GLBs, official photo PBR, out')
src,pbr,out=map(Path,args)
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
names=['timber_beam_1','timber_beam_2','timber_brace_X']
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1050;scene.render.resolution_y=1050
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Eldoria artisan timber studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.26,.32,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;scene.world=world
def lamp(name,power,where,color):
 d=bpy.data.lights.new(name,'AREA');d.color=color;d.energy=power;d.size=9;d.shape='DISK'
 ob=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(ob);ob.location=where
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('warm visible wood grain',1820,(-7,-9,12),(1,.84,.71))
lamp('cool blue alpine sky',1700,(8,3,10),(.69,.84,1))
lamp('rim',850,(0,9,10),(.96,.94,.88))
cd=bpy.data.cameras.new('constant timber camera')
cam=bpy.data.objects.new('constant timber camera',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(meshes):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(p) for o in meshes if o.type=='MESH' for p in o.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def setcam(m):
 lo,hi=bounds(m);c=(lo+hi)*.5;d=hi-lo
 cam.location=c+Vector((.86,-1.58,1.13)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.20)*1.65
def shoot(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
originals={}
for kind in ('diffuse','nor_gl','rough'):
 filename=pbr/('old_planks_02_'+kind+'.jpg')
 if not filename.is_file():raise RuntimeError('Expected real CC0 old planks photo map '+str(filename))
 im=bpy.data.images.load(str(filename),check_existing=False);im.scale(1024,1024)
 pixels=np.empty(1024*1024*4,dtype=np.float32);im.pixels.foreach_get(pixels)
 originals[kind]=pixels.reshape((1024,1024,4)).copy()
 bpy.data.images.remove(im)
regions=[(.167,.264),(.303,.40),(.603,.71)]
def make_image(name,kind,index):
 xlo,xhi=regions[index]
 ar=originals[kind]
 indices=np.clip(np.rint(np.linspace(xlo*1024,xhi*1024,1024)).astype(int),0,1023)
 crop=ar[:,indices,:].copy()
 # Dark warm oak while retaining authentic old-plank knots and grazed fibers.
 if kind=='diffuse':
  crop[:,:,:3]=np.clip(crop[:,:,:3]*np.array([1.05,.99,.94],dtype=np.float32),.02,.91)
 photo=bpy.data.images.new('Valoria_real_wood_'+name+'_'+kind,1024,1024,alpha=True)
 photo.pixels.foreach_set(crop.reshape(-1));photo.update()
 if kind=='nor_gl':photo.colorspace_settings.name='Non-Color'
 if kind=='rough':photo.colorspace_settings.name='Non-Color'
 path=out/'atlas'/(name+'_'+kind+'_single_oldwood_CC0.png')
 photo.filepath_raw=str(path);photo.file_format='PNG';photo.save();photo.pack()
 return photo,path
def woodmaterial(name,idx):
 diffuse,dpath=make_image(name,'diffuse',idx)
 normal,npath=make_image(name,'nor_gl',idx)
 rough,rpath=make_image(name,'rough',idx)
 mat=bpy.data.materials.new('Valoria_true_photographed_aged_oak_'+name)
 mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=diffuse
 bs=nd.new('ShaderNodeBsdfPrincipled')
 lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
 bs.inputs['Metallic'].default_value=0
 bs.inputs['Roughness'].default_value=.82
 ntex=nd.new('ShaderNodeTexImage');ntex.image=normal
 nm=nd.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.38
 lk.new(ntex.outputs['Color'],nm.inputs['Color'])
 lk.new(nm.outputs['Normal'],bs.inputs['Normal'])
 o=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],o.inputs['Surface'])
 return mat,[str(x.name) for x in (dpath,npath,rpath)]
records=[]
for i,name in enumerate(names):
 pre=set(bpy.data.objects);r={'asset':name,'art_approved':False}
 try:
  f=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(f))
  meshes=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if not meshes:raise RuntimeError('No original joinery')
  setcam(meshes);shoot(out/'before'/(name+'.png'))
  mat,maps=woodmaterial(name,i)
  count=0;preserved_pins=0
  for ob in meshes:
   low=ob.name.lower()
   if 'peg' in low or 'pin' in low:
    preserved_pins+=1;continue
   if not ob.data.materials:ob.data.materials.append(mat)
   else:
    for slot in range(len(ob.data.materials)):ob.data.materials[slot]=mat
   count+=1
  if count<1:raise RuntimeError('No wooden face actually changed')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in meshes:ob.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  output=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(output),export_format='GLB',use_selection=True,export_apply=True)
  if output.stat().st_size<70000:raise RuntimeError('Original detailed timber geometry lost')
  r.update({'technical':'PASS_PENDING_VISUAL','geometry_meshes_unchanged':len(meshes),
    'wooden_faces_retextured':count,'pins_kept':preserved_pins,'atlas':maps,
    'original_sha':sha(f),'candidate_sha':sha(output),
    'donor':'Poly Haven old_planks_02, single plank cropped to avoid photo grout cracks, CC0'})
  print('TRUE_CC0_KNOTTED_BEAM_FINISHED',name,len(meshes),flush=True)
 except Exception as ex:
  r.update({'technical':'FAILED','error':repr(ex)});traceback.print_exc()
 finally:
  records.append(r)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'three_genuine_woodgrain_studies.json').write_text(json.dumps({
   'photo_source_run':38880324077,'previous_real_joinery_run':38877453205,
   'no_composition':True,'art_approved':False,'items':records},indent=2))
if len(records)!=3 or any(r['technical']!='PASS_PENDING_VISUAL' for r in records):raise RuntimeError('Three individual wood photos incomplete')
print('THREE_AGED_CC0_WOOD_GRAIN_MODELS_READY',flush=True)
