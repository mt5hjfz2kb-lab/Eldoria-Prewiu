"""Create camera-locked, authentic Blender clay A/B of frozen terrain and artist-edited geometry.
Not a Unity capture; cannot establish ART_APPROVED. Uses the exact same camera and studio light in each.
blender -b --python-exit-code 1 --python render_artist_ab.py -- --before frozen.blend --after artist-output.blend --target "Existing mesh" --out screenshots
"""
import argparse,hashlib,json,math,pathlib,sys
import bpy
from mathutils import Vector
p=argparse.ArgumentParser()
p.add_argument('--before',required=True,type=pathlib.Path)
p.add_argument('--after',required=True,type=pathlib.Path)
p.add_argument('--target',required=True)
p.add_argument('--out',required=True,type=pathlib.Path)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
if not a.before.is_file() or not a.after.is_file():raise ValueError('Both real Blender files are required')
if a.before.resolve()==a.after.resolve():raise ValueError('The same source cannot be compared to itself')
a.out.mkdir(parents=True,exist_ok=True)
def collect(path):
 bpy.ops.wm.open_mainfile(filepath=str(path.resolve()))
 obj=bpy.data.objects.get(a.target)
 if not obj or obj.type!='MESH':raise ValueError('Requested terrain object missing: '+a.target)
 corners=[obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
 lo=Vector([min(c[i] for c in corners) for i in range(3)])
 hi=Vector([max(c[i] for c in corners) for i in range(3)])
 return {'lo':list(lo),'hi':list(hi),'verts':len(obj.data.vertices),'faces':len(obj.data.polygons)}
source=collect(a.before);dest=collect(a.after)
if source['lo']==dest['lo'] and source['hi']==dest['hi'] and source['verts']==dest['verts'] and source['faces']==dest['faces']:
 print('WARNING: identical target bounds/counts; verify exact geometry hashes before claiming progress')
center=Vector([(min(source['lo'][i],dest['lo'][i])+max(source['hi'][i],dest['hi'][i]))/2 for i in range(3)])
size=max(3.0,max(max(source['hi'][i],dest['hi'][i])-min(source['lo'][i],dest['lo'][i]) for i in range(3)))
def render(path,name):
 bpy.ops.wm.open_mainfile(filepath=str(path.resolve()))
 for item in list(bpy.data.objects):
  if item.type in ('CAMERA','LIGHT'):bpy.data.objects.remove(item,do_unlink=True)
 scene=bpy.context.scene
 # Neutral material override deliberately removes unrelated PBR variance.
 mat=bpy.data.materials.new('Eldoria_Authoring_Proof_Clay')
 mat.diffuse_color=(0.47,0.44,0.40,1)
 mat.use_nodes=True
 bsdf=mat.node_tree.nodes.get('Principled BSDF')
 if bsdf:
  bsdf.inputs['Base Color'].default_value=(0.47,0.44,0.40,1)
  bsdf.inputs['Roughness'].default_value=0.95
 for item in bpy.data.objects:
  if item.type=='MESH':
   item.data.materials.clear()
   item.data.materials.append(mat)
 cam_data=bpy.data.cameras.new('QA_Locked_Camera')
 cam=bpy.data.objects.new('QA_Locked_Camera',cam_data)
 scene.collection.objects.link(cam)
 cam.location=center+Vector((size*1.10,-size*1.35,size*1.0))
 direction=center-cam.location
 cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
 cam_data.type='ORTHO';cam_data.ortho_scale=size*1.9
 scene.camera=cam
 light_data=bpy.data.lights.new('QA_Locked_Area','AREA')
 light=bpy.data.objects.new('QA_Locked_Area',light_data)
 scene.collection.objects.link(light)
 light.location=center+Vector((-size*0.8,-size*0.65,size*1.3))
 light_data.energy=1800;light_data.shape='DISK';light_data.size=size*1.25
 scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=12
 for layer in scene.view_layers:
  if hasattr(layer,'cycles'):layer.cycles.use_denoising=False # System Blender does not ship OpenImageDenoiser
 scene.render.resolution_x=640;scene.render.resolution_y=480;scene.render.resolution_percentage=100
 scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
 scene.world.color=(0.09,0.09,0.09)
 scene.render.filepath=str((a.out/name).resolve())
 bpy.ops.render.render(write_still=True)
 return hashlib.sha256((a.out/name).read_bytes()).hexdigest()
before_hash=render(a.before,'before-blender-clay.png')
after_hash=render(a.after,'after-blender-clay.png')
if before_hash==after_hash:raise ValueError('A/B Blender frames are identical; no visible geometric proof')
manifest={'scope':'Blender clay A/B only, NOT Unity, NOT ART_APPROVED',
          'target':a.target,'camera':'orthographic camera and clay/light anchored to combined source/edited target bounds',
          'source_file_sha256':hashlib.sha256(a.before.read_bytes()).hexdigest(),
          'result_file_sha256':hashlib.sha256(a.after.read_bytes()).hexdigest(),
          'before_image_sha256':before_hash,'after_image_sha256':after_hash,
          'source_geometry':source,'edited_geometry':dest}
(a.out/'comparison.json').write_text(json.dumps(manifest,indent=2)+'\n','utf8')
print('BLENDER_AB_RENDERED',json.dumps(manifest))
