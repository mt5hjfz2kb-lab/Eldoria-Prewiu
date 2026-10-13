#!/usr/bin/env python3
"""Experimental GPT-6-directed broad environment composition on verified Valoria GLB.

Alternative to repeated camera-invisible Qwen 4B micro-sculpts. This is not a
Qwen-authored model inference and NEVER issues ART_APPROVED/Unity PASS.
One canonical terrain mesh is changed; all architectural meshes stay intact.
"""
import argparse,hashlib,json,math,pathlib,struct,sys
import bpy
from mathutils import Vector

p=argparse.ArgumentParser()
p.add_argument('--source',type=pathlib.Path,required=True)
p.add_argument('--out',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
a.out.mkdir(parents=True,exist_ok=True)
source=a.source.read_bytes()
if source[:4]!=b'glTF' or struct.unpack('<I',source[8:12])[0]!=len(source):
 raise RuntimeError('Frozen original Unity scene GLB is invalid')
src_sha=hashlib.sha256(source).hexdigest()

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(a.source.resolve()))
target='Valoria natural playable hilltop and rock scarps'
terrain=bpy.data.objects.get(target)
if not terrain or terrain.type!='MESH' or not 12000<=len(terrain.data.vertices)<=16000:
 raise RuntimeError('Expected original authored terrain missing/mismatched')
vertex_count=len(terrain.data.vertices)
face_count=len(terrain.data.polygons)
uv_count=len(terrain.data.uv_layers)
material_names=[m.name if m else '' for m in terrain.data.materials]
if len(material_names)<3 or uv_count<1:raise RuntimeError('Cannot alter untextured or unsuitable terrain')

# Create the same daylight/gameplayer camera ONCE and keep it absolutely fixed.
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.eevee.taa_render_samples=24
scene.render.resolution_x=960;scene.render.resolution_y=640
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.view_settings.view_transform='Standard'
scene.view_settings.look='Medium High Contrast'
scene.view_settings.exposure=0
scene.world.color=(.16,.21,.27)
world=bpy.data.worlds.new('A/B immutable daylight world')
world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.28,.36,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.7
scene.world=world
sun=bpy.data.lights.new('A/B sun same direction','SUN')
sun.energy=2.2;sun.color=(1.0,.88,.77)
light=bpy.data.objects.new('A/B sun same direction',sun)
scene.collection.objects.link(light)
light.rotation_euler=(math.radians(32),math.radians(-21),math.radians(-39))
cam=bpy.data.cameras.new('A/B verified player-equivalent camera')
camera=bpy.data.objects.new('A/B verified player-equivalent camera',cam)
scene.collection.objects.link(camera);scene.camera=camera
camera.location=(32,-76,49)
camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler()
cam.type='ORTHO';cam.ortho_scale=77
bpy.context.view_layer.update()
camera_matrix=[[round(float(v),8) for v in row] for row in camera.matrix_world]

def render(dest):
 scene.render.filepath=str(dest.resolve())
 bpy.ops.render.render(write_still=True)
 if not dest.is_file():raise RuntimeError('Genuine Blender A/B image absent')

before=a.out/'before-player-camera.png'
render(before)
original_non_terrain={}
for ob in bpy.data.objects:
 if ob.type=='MESH' and ob!=terrain:
  original_non_terrain[ob.name]=(len(ob.data.vertices),len(ob.data.polygons),
    tuple(round(float(t),6) for r in ob.matrix_world for t in r))
coords=[tuple(float(t) for t in vertex.co) for vertex in terrain.data.vertices]

def sstep(v):
 v=max(0.0,min(1.0,v));return v*v*(3-2*v)

# AI art direction (not generated geometry from a stock noise-only tool):
# 1. Break the entire foreground scarps' monotonous planar read, centered
#    on the SOUTH player-facing cliff from x=-36..36,y~-30.
# 2. Give the silhouette asymmetrical large-scale ridges that extend into
#    the slope rather than tiny isolated divots.
# 3. Pull the east/west side contacts into the valley without changing the
#    castle, road or any buildable plateau coordinates.
# 4. Preserve PBR face assignments, UV0, object hierarchy, all other meshes.
changes=0
max_movement=0.0
protected_changes=0
for v,xyz in zip(terrain.data.vertices,coords):
 x,y,z=xyz
 if -34<y<36 and abs(x)<22 and z>-1:
  continue # mandatory original gameplay parcel/road protection
 f_front=sstep((y+48)/17)*(1-sstep((y+19)/12))
 f_width=1-sstep((abs(x)-29)/12)
 envelope=max(0.0,f_front*f_width)
 f_side=sstep((abs(x)-20)/11)*(1-sstep((abs(x)-41)/10))
 y_side=(1-sstep((abs(y-6)-25)/15))
 side=envelope*.15+max(0.,f_side*y_side)
 # Explicit 3D sculpture designed for the actual camera (metres).
 rise=(2.7*math.sin(.145*x+.25*math.sin(.10*y))+
       1.15*math.sin(.39*x+.16*y)+.45*math.sin(.78*x-.21*y))*envelope
 lateral=-1.15*math.sin(.18*x+.09*y)*envelope
 shoulder=(.85*math.cos(.25*y+.12*x))*side
 horizontal=.34*math.sin(.28*y+.16*x)*side
 dz=rise+shoulder
 dy=lateral+horizontal
 movement=math.hypot(dy,dz)
 if movement<.002:continue
 if movement>5.05:raise RuntimeError('Scene-directed art too strong for safe full city')
 v.co.y+=dy;v.co.z+=dz
 changes+=1;max_movement=max(max_movement,movement)
 if -34<y<36 and abs(x)<22 and z>-1:protected_changes+=1
terrain.data.update()
if changes<400 or protected_changes or len(terrain.data.vertices)!=vertex_count or len(terrain.data.polygons)!=face_count:
 raise RuntimeError('Required broad safe environment sculpt did not occur')
if len(terrain.data.uv_layers)!=uv_count or [m.name if m else '' for m in terrain.data.materials]!=material_names:
 raise RuntimeError('UV or PBR material slots changed unexpectedly')
for ob in bpy.data.objects:
 if ob.name in original_non_terrain:
  now=(len(ob.data.vertices),len(ob.data.polygons),
    tuple(round(float(t),6) for r in ob.matrix_world for t in r))
  if now!=original_non_terrain[ob.name]:
   raise RuntimeError('Protected architectural object altered: '+ob.name)
after=a.out/'after-player-camera.png'
render(after)
# The input is an existing scene; do not replace or rebuild game logic.
blend=a.out/'Bastion1GPTDirectorAlternative.blend'
glb=a.out/'Bastion1GPTDirectorAlternative.glb'
bpy.ops.wm.save_as_mainfile(filepath=str(blend.resolve()),compress=True)
bpy.ops.export_scene.gltf(filepath=str(glb.resolve()),export_format='GLB',export_cameras=False,export_lights=False)
proof={'status':'EXPERIMENTAL_DIRECTOR_AUTHORED_NOT_ART_APPROVED',
 'authorship':'GPT-6-directed environment geometry via typed Blender script, NOT Qwen inference',
 'camera':'locked player-equivalent orthographic Blender camera NOT Unity',
 'source_run':38356683511,'source_glb_sha256':src_sha,
 'source_terrain_vertices':vertex_count,'source_terrain_faces':face_count,
 'changed_vertices':changes,'max_movement_m':round(max_movement,5),
 'protected_parcel_changes':protected_changes,'architecture_count_untouched':len(original_non_terrain),
 'preserved_uv_count':uv_count,'preserved_materials':material_names,
 'reference_camera_matrix':camera_matrix,
 'before_png_sha256':hashlib.sha256(before.read_bytes()).hexdigest(),
 'after_png_sha256':hashlib.sha256(after.read_bytes()).hexdigest(),
 'blend_sha256':hashlib.sha256(blend.read_bytes()).hexdigest(),
 'glb_sha256':hashlib.sha256(glb.read_bytes()).hexdigest(),
 'unity_art_pass':False,'mobile_perf_pass':False,'gameplay_pass':False,
 'main_promotion_allowed':False}
(a.out/'independent-art-proof.json').write_text(json.dumps(proof,indent=2)+'\n')
print('BROAD_DIRECTOR_AUTHORED_ART_ALTERNATIVE_NOT_APPROVED',json.dumps(proof),flush=True)
