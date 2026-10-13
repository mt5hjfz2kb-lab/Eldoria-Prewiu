#!/usr/bin/env python3
"""Source-locked player-camera ray visibility map (read only; no geometry edits).

Identify which world-space faces occupy centre of the actual strategic player
frame. Avoid blind south-cliff sculpting when the camera is looking at a
protected buildable terrace. Source, camera parameters, dimensions and
ray hits are stored as reviewable evidence, not an ART_PASS.
"""
import argparse
import hashlib
import json
import math
import pathlib
import statistics
import sys

import bpy
from mathutils import Vector

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def stats(points):
    if not points:return {'hits':0}
    positions=[p['local'] for p in points]
    z=[p[2] for p in positions];y=[p[1] for p in positions];x=[p[0] for p in positions]
    return {'hits':len(points),'median_local_x':round(statistics.median(x),3),
       'median_local_y':round(statistics.median(y),3),
       'median_local_z':round(statistics.median(z),3),
       'z_min':round(min(z),3),'z_max':round(max(z),3),
       'share_playable_z_ge_neg1':round(sum(v>=-1 for v in z)/len(z),5),
       'share_editable_scarp_z_lt_neg1':round(sum(v<-1 for v in z)/len(z),5),
       'y_min':round(min(y),3),'y_max':round(max(y),3)}

def main():
    p=argparse.ArgumentParser()
    p.add_argument('--source',required=True,type=pathlib.Path)
    p.add_argument('--source-sha256',required=True)
    p.add_argument('--target',required=True)
    p.add_argument('--out',required=True,type=pathlib.Path)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    if not a.source.exists() or sha(a.source)!=a.source_sha256:
        raise ValueError('Camera probe source is not the real frozen input')
    bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
    obj=bpy.data.objects.get(a.target)
    if obj is None or obj.type!='MESH':raise ValueError('Original scarp absent')
    cam_data=bpy.data.cameras.new('Locked_Strategic_Player')
    cam=bpy.data.objects.new('Locked_Strategic_Player',cam_data)
    bpy.context.scene.collection.objects.link(cam)
    cam.location=Vector((32.,-76.,49.))
    cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler()
    cam_data.type='ORTHO';cam_data.ortho_scale=52.
    scene=bpy.context.scene
    scene.camera=cam;scene.render.resolution_x=640;scene.render.resolution_y=480
    scene.render.resolution_percentage=100
    bpy.context.view_layer.update()
    depsgraph=bpy.context.evaluated_depsgraph_get()
    quat=cam.rotation_euler.to_quaternion()
    forward=quat@Vector((0.,0.,-1.))
    right=quat@Vector((1.,0.,0.))
    up=quat@Vector((0.,1.,0.))
    width,height=640,480
    # Vertical scale (52) times width/height is the exact orthographic span.
    half_y=cam_data.ortho_scale/2.
    half_x=half_y*width/height
    regions={'upper':[],'central':[],'bottom':[],'left':[],'right':[],'all':[]}
    grid=[]
    for v in range(0,height,8):
      for u in range(0,width,8):
        sx=((u+.5)/width*2.-1.)*half_x
        sy=(1.-(v+.5)/height*2.)*half_y
        origin=cam.location+right*sx+up*sy
        hit,location,normal,face,hitobj,matrix=scene.ray_cast(depsgraph,origin,forward,distance=500.)
        if not hit or hitobj is None or hitobj.name!=a.target:continue
        local=obj.matrix_world.inverted()@location
        point={'pixel':[u,v],'local':[round(float(z),4) for z in local],
             'face_index':int(face)}
        regions['all'].append(point)
        if 40<=v<115:regions['upper'].append(point)
        if 115<=v<360 and 80<=u<560:regions['central'].append(point)
        if 360<=v<480:regions['bottom'].append(point)
        if u<80:regions['left'].append(point)
        if u>=560:regions['right'].append(point)
        if v in (128,160,192,224,256,288,320,352,400) and (u%32==0):
            grid.append(point)
    summary={region:stats(points) for region,points in regions.items()}
    if summary['all']['hits']<150 or summary['central']['hits']<60:
        raise ValueError('Camera proof rays insufficient or unexpectedly not hitting original terrain')
    evidence={
      'source_blend_sha256':sha(a.source),'camera_location':[32,-76,49],
      'camera_target':[0,0,0],'orthographic_scale':52,
      'image_pixels':[640,480],'target':a.target,
      'sampling':'8px image-grid primary ray casts against original Blender scene',
      'region_summary':summary,
      'central_grid_samples':grid,
      'read_only':True,'art_approved':False}
    a.out.mkdir(parents=True,exist_ok=True)
    out=a.out/'source-camera-surface-map.json'
    out.write_text(json.dumps(evidence,indent=2)+'\n')
    print('BASTION1_REAL_STRATEGIC_CAMERA_SURFACE_3D_HITS',json.dumps(summary))
if __name__=='__main__':main()
