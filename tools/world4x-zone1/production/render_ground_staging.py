"""Blender diagnostic from planned strategic poses. NOT Unity or a renderer-parity certificate."""
import argparse, hashlib, json, math, pathlib, sys
import bpy
from mathutils import Vector
parser=argparse.ArgumentParser()
parser.add_argument('--before',type=pathlib.Path,required=True)
parser.add_argument('--after',type=pathlib.Path,required=True)
parser.add_argument('--out',type=pathlib.Path,required=True)
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:])
if args.out.exists() and any(args.out.iterdir()):raise ValueError('Fresh evidence required')
args.out.mkdir(parents=True,exist_ok=True)
records=[]
for label,path in [('before',args.before),('after',args.after)]:
 bpy.ops.wm.open_mainfile(filepath=str(path.resolve()))
 scene=bpy.context.scene
 # Same world origin and 52 degree strategy orientation in every zoom.
 focus=Vector((0,40,0))
 camera=scene.camera;camera.location=focus+Vector((0,350*math.cos(math.radians(52)),350*math.sin(math.radians(52))))
 camera.rotation_euler=(focus-camera.location).to_track_quat('-Z','Y').to_euler()
 camera.data.type='ORTHO';camera.data.sensor_fit='VERTICAL'
 scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=False
 scene.view_settings.view_transform='Standard';scene.view_settings.exposure=0;scene.view_settings.gamma=1
 for light in bpy.data.objects:
  if light.type=='LIGHT' and light.data.type=='SUN':
   light.rotation_euler=(math.radians(48),0,math.radians(-145))
   light.data.energy=1.4;light.data.color=(1,.92,.78)
 for name,zoom,width,height in [('regional',60,1280,900),('local',32,1280,900),('mobile',60,390,844)]:
  camera.data.ortho_scale=2*zoom
  scene.render.resolution_x=width;scene.render.resolution_y=height;scene.render.resolution_percentage=100
  image=args.out/(label+'-'+name+'-blender.png');scene.render.filepath=str(image.resolve())
  bpy.ops.render.render(write_still=True)
  records.append({'version':label,'view':name,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
   'image_sha256':hashlib.sha256(image.read_bytes()).hexdigest(),'camera_matrix':[list(r) for r in camera.matrix_world],
   'zoom_half_vertical_m':zoom,'resolution':[width,height]})
for view in ['regional','local','mobile']:
 pair=[r for r in records if r['view']==view]
 assert pair[0]['camera_matrix']==pair[1]['camera_matrix']
(args.out/'staging-comparison.json').write_text(json.dumps({'image_type':'BLENDER_DIAGNOSTIC_NOT_UNITY',
 'camera_equivalent_within_each_pair':True,'world_coordinates_unchanged':True,'records':records,
 'same_mesh_world_for_all_zooms':True,'renderer_parity_certified':False,'art_approved':False},indent=2)+'\n')
