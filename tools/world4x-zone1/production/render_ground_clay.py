"""Matched neutral source renders reveal geography independently of grass; NOT Unity."""
import argparse, hashlib, json, math, pathlib, sys
import bpy
from mathutils import Vector
p=argparse.ArgumentParser();p.add_argument('--before',type=pathlib.Path,required=True);p.add_argument('--after',type=pathlib.Path,required=True);p.add_argument('--out',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);a.out.mkdir(parents=True,exist_ok=False)
records=[]
for version,path in [('before',a.before),('after',a.after)]:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.gltf(filepath=str(path.resolve()))
 meshes=[o for o in bpy.data.objects if o.type=='MESH' and 'ZoneOne continuous prairie source' in o.name]
 if len(meshes)!=1:raise ValueError('One named continuous terrain required')
 for o in bpy.data.objects:
  if o.type=='MESH' and o not in meshes:o.hide_render=True
 for o in list(bpy.data.objects):
  if o.type!='MESH':bpy.data.objects.remove(o,do_unlink=True)
 material=bpy.data.materials.new('Diagnostic clay only');material.use_nodes=True
 shader=material.node_tree.nodes.get('Principled BSDF');shader.inputs['Base Color'].default_value=(.46,.48,.44,1);shader.inputs['Roughness'].default_value=.9
 for o in meshes:
  positions=[list(v.co) for v in o.data.vertices]
  if o.data.has_custom_normals:o.data.normals_split_custom_set([(0.,0.,0.)]*len(o.data.loops));o.data.update()
  o.data.materials.clear();o.data.materials.append(material)
  assert positions==[list(v.co) for v in o.data.vertices]
 scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=False
 scene.world=bpy.data.worlds.new('Diagnostic sky');scene.world.use_nodes=True
 scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.5,.6,.7,1)
 scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.18
 light=bpy.data.lights.new('Same diagnostic sun','SUN');light.energy=2;light.angle=math.radians(3)
 sun=bpy.data.objects.new('Same diagnostic sun',light);scene.collection.objects.link(sun)
 sun.rotation_euler=(math.radians(50),0,math.radians(-145))
 cam=bpy.data.cameras.new('Same diagnostic camera');camera=bpy.data.objects.new('Same diagnostic camera',cam);scene.collection.objects.link(camera);scene.camera=camera
 target=Vector((0,40,0));camera.location=target+Vector((0,350*math.cos(math.radians(52)),350*math.sin(math.radians(52))))
 camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();cam.type='ORTHO';cam.ortho_scale=120;cam.sensor_fit='VERTICAL'
 scene.view_settings.view_transform='Standard';scene.view_settings.exposure=0;scene.view_settings.gamma=1
 for name,width,height in [('regional',1000,800),('mobile',390,844)]:
  scene.render.resolution_x=width;scene.render.resolution_y=height;scene.render.resolution_percentage=100
  image=a.out/(version+'-'+name+'-clay-blender.png');scene.render.filepath=str(image.resolve());bpy.ops.render.render(write_still=True)
  records.append({'version':version,'view':name,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'image_sha256':hashlib.sha256(image.read_bytes()).hexdigest(),'camera_matrix':[list(r) for r in camera.matrix_world],'resolution':[width,height]})
for view in ['regional','mobile']:
 pair=[r for r in records if r['view']==view];assert pair[0]['camera_matrix']==pair[1]['camera_matrix']
(a.out/'clay-comparison.json').write_text(json.dumps({'renderer':'CYCLES_BLENDER_NOT_UNITY','purpose':'Neutral landform visibility diagnostic; grass materials removed only in temporary render scene','art_approved':False,'same_camera_in_each_pair':True,'source_files_unchanged':True,'records':records},indent=2))
