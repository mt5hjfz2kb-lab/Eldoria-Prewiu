#!/usr/bin/env python3
"""Authentic same-camera PBR BEFORE/AFTER; preserve texture shading, no clay override."""
import argparse
import hashlib
import json
import math
import pathlib
import sys

import bpy
from mathutils import Vector
p=argparse.ArgumentParser()
p.add_argument('--before',required=True,type=pathlib.Path)
p.add_argument('--after',required=True,type=pathlib.Path)
p.add_argument('--out',required=True,type=pathlib.Path)
p.add_argument('--target',required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
a.out.mkdir(parents=True,exist_ok=True)
if a.before.resolve()==a.after.resolve() or not a.before.is_file() or not a.after.is_file():
 raise ValueError('Real source and different PBR result are mandatory')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def render(path,label):
 bpy.ops.wm.open_mainfile(filepath=str(path.resolve()))
 target=bpy.data.objects.get(a.target)
 if not target or len(target.data.vertices)!=12601:raise ValueError('Original source target missing')
 for o in list(bpy.data.objects):
  if o.type in ('CAMERA','LIGHT'):bpy.data.objects.remove(o,do_unlink=True)
 scene=bpy.context.scene
 data=bpy.data.cameras.new('SourceLockedPBRStrategicCamera')
 cam=bpy.data.objects.new('SourceLockedPBRStrategicCamera',data)
 scene.collection.objects.link(cam)
 cam.location=(32,-76,49)
 cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler()
 data.type='ORTHO';data.ortho_scale=52;scene.camera=cam
 sun_data=bpy.data.lights.new('QA_PBR_Sun','SUN')
 sun_data.energy=2.4;sun_data.angle=math.radians(13)
 sun=bpy.data.objects.new('QA_PBR_Sun',sun_data);scene.collection.objects.link(sun)
 sun.rotation_euler=(math.radians(27),math.radians(-24),math.radians(-38))
 scene.world=bpy.data.worlds.new('QA_PBR_Fixed_Daylight')
 scene.world.use_nodes=True
 scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.32,.37,.43,1)
 scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.55
 scene.render.engine='CYCLES';scene.cycles.samples=12;scene.cycles.device='CPU'
 for layer in scene.view_layers:
  if hasattr(layer,'cycles'):layer.cycles.use_denoising=False
 scene.view_settings.view_transform='AgX'
 scene.render.image_settings.file_format='PNG'
 scene.render.resolution_x=768;scene.render.resolution_y=576;scene.render.resolution_percentage=100
 scene.render.filepath=str((a.out/label).resolve())
 bpy.ops.render.render(write_still=True)
 return sha(a.out/label)
result={'scope':'Independent textured Blender PBR comparison, NOT Unity or ART_APPROVED',
 'camera_location':[32,-76,49],'camera_target':[0,0,0],'ortho_scale':52,
 'source_sha256':sha(a.before),'result_sha256':sha(a.after),
 'before_png_sha256':render(a.before,'before-pbr.png'),
 'after_png_sha256':render(a.after,'after-pbr.png'),
 'art_approved':False}
(a.out/'comparison.json').write_text(json.dumps(result,indent=2)+'\n')
print('ORIGINAL_CC0_PBR_SOURCE_LOCKED_AB',json.dumps(result))
