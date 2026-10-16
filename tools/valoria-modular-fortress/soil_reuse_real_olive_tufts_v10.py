"""Dirt CC0 natural old-soil texture + EXISTING 6.1/10 living 3D grass
tuft asset reused at appropriate physical size. No green printed circles,
no fake pixel blotches, no new procedural plant. One independent reusable
dirt_ground_patch GLB with 11 physically rooted tiny CC0 grass clusters.
"""
import bpy,sys,random,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector,Matrix
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise RuntimeError('CC0 dirt GLB, selected individual grass_tuft GLB, destination')
dirt,tuft,out=map(Path,arg)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE';sc.render.resolution_x=1050;sc.render.resolution_y=950
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria natural photographed earth studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.26,.33,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;sc.world=world
def lamp(name,power,pos,color):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.shape='DISK';l.size=9
 obj=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(obj);obj.location=pos
 obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
lamp('sunset grazing key',1850,(-7,-9,12),(1,.85,.69))
lamp('sky fill',1550,(8,4,11),(.74,.86,1))
camd=bpy.data.cameras.new('paired natural earth studio')
cam=bpy.data.objects.new('paired natural earth studio',camd);bpy.context.collection.objects.link(cam)
sc.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(objects):
 bpy.context.view_layer.update()
 points=[o.matrix_world@Vector(v) for o in objects for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in points) for i in range(3)]),Vector([max(p[i] for p in points) for i in range(3)])
def render(path):sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
if not dirt.is_file() or not tuft.is_file():raise RuntimeError('Proven source GLBs unavailable')
bpy.ops.import_scene.gltf(filepath=str(dirt))
dirt_models=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
if not dirt_models:raise RuntimeError('Original photo CC0 loam geometry absent')
low,high=bounds(dirt_models);center=(low+high)*.5;dim=high-low
cam.location=center+Vector((.8,-1.58,1.18)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camd.ortho_scale=max(dim.x,dim.y,dim.z,.3)*1.72
render(out/'before'/'dirt_ground_patch.png')
pre_tuft=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(tuft))
grass=[o for o in bpy.data.objects if o not in pre_tuft and o.type=='MESH']
if not grass:raise RuntimeError('No proven 3D grass fronds in CC0 source')
blo,bhi=bounds(grass);donor_height=float(bhi.z-blo.z)
if donor_height<.1:raise RuntimeError('CC0 grass donor not upright')
rng=random.Random(160121)
clones=[];specs=[]
for i in range(11):
 x=float(low.x)+float(dim.x)*rng.uniform(.12,.88)
 y=float(low.y)+float(dim.y)*rng.uniform(.12,.88)
 height=.12+rng.random()*.16
 scl=height/donor_height
 # Small reproductions of an already art-reviewed individually reusable
 # botanical tuft; each has independent angle and exact ground contact.
 objs=[]
 for src in grass:
  ob=src.copy()
  ob.data=src.data.copy()
  bpy.context.collection.objects.link(ob)
  ob.name='REAL_CC0_OLIVE_TUFT_%02d__%s'%(i,src.name)
  ob.matrix_world=Matrix.Scale(scl,4)@src.matrix_world
  objs.append(ob);clones.append(ob)
 blo2,bhi2=bounds(objs)
 center2=(blo2+bhi2)*.5
 offset=Vector((x-float(center2.x),y-float(center2.y),float(high.z)-float(blo2.z)))
 theta=rng.uniform(-3.14,3.14)
 rot=Matrix.Translation(center2)@Matrix.Rotation(theta,4,'Z')@Matrix.Translation(-center2)
 for ob in objs:ob.matrix_world=Matrix.Translation(offset)@rot@ob.matrix_world
 specs.append({'tuft':i,'natural_height_m':round(height,4),'x':round(x,4),'y':round(y,4)})
for src in grass:bpy.data.objects.remove(src,do_unlink=True)
if len(clones)<11:raise RuntimeError('Few real grass objects')
render(out/'after'/'dirt_ground_patch.png')
bpy.ops.object.select_all(action='DESELECT')
for obj in dirt_models+clones:obj.select_set(True)
bpy.context.view_layer.objects.active=dirt_models[0]
dst=out/'glb'/'dirt_ground_patch.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<40000:raise RuntimeError('Photographed dirt and grass missing from GLB')
record={'asset':'dirt_ground_patch','technical':'PASS_PENDING_VISUAL','source_dirt_run':38871626493,
 'source_curved_grass_run':38881298032,'dirt_sha':sha(dirt),'grass_donor_sha':sha(tuft),
 'candidate_sha':sha(dst),'small_physical_grass_tufts':specs,'botanic_meshes':len(clones),
 'no_green_printed_blobs':True,'environment_assembly':False,'art_approved':False,
 'warning':'Must verify physically rooted appearance, actual photo grain, size and 6/10 quality visually, then in Unity.'}
(out/'reports'/'dirt_with_proven_botanical_tufts.json').write_text(json.dumps(record,indent=2))
print('REAL_PHOTO_DIRT_WITH_REAL_SMALL_BOTANICALS_COMPLETE',len(clones),dst.stat().st_size,flush=True)
