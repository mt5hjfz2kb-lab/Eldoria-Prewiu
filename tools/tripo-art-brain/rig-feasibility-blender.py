#!/usr/bin/env python3
"""Isolated ZERO-CREDIT Blender armature/weight/clip feasibility trial.

Existing, SHA-locked Spawnling near GLB -> temporary copy with real bone skin and
pose-driven deformation. This is a TECHNICAL PROTOTYPE: intentionally not a
polished artist-designed creature rig, Tripo rig-check, or runtime promotion.
No network, API, paid credits, main-scene mutation, or source replacement.
"""
import argparse
import hashlib
import json
import math
import os
import sys

import bpy
from mathutils import Vector


def args():
    argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    p=argparse.ArgumentParser()
    p.add_argument('--input',required=True)
    p.add_argument('--source-sha256',required=True)
    p.add_argument('--out',required=True)
    return p.parse_args(argv)


def digest(p):
    h=hashlib.sha256()
    with open(p,'rb') as f:
        for b in iter(lambda:f.read(1024*1024),b''):
            h.update(b)
    return h.hexdigest()


def bounds(objects):
    pts=[o.matrix_world @ Vector(c) for o in objects for c in o.bound_box]
    return ([min(p[i] for p in pts) for i in range(3)],
            [max(p[i] for p in pts) for i in range(3)])


def coordinate_bones(arm,lo,hi):
    cx=(lo[0]+hi[0])/2;cy=(lo[1]+hi[1])/2;cz=(lo[2]+hi[2])/2
    dx=hi[0]-lo[0];dy=hi[1]-lo[1];dz=hi[2]-lo[2]
    bpy.ops.object.select_all(action='DESELECT')
    arm.select_set(True);bpy.context.view_layer.objects.active=arm
    bpy.ops.object.mode_set(mode='EDIT')
    def bone(name,head,tail,parent=None):
        b=arm.data.edit_bones.new(name)
        b.head=head;b.tail=tail
        if parent:b.parent=arm.data.edit_bones[parent]
    bone('root',(cx-.12*dx,cy,cz-.07*dz),(cx+.01*dx,cy,cz+.02*dz))
    bone('spine',(cx-.02*dx,cy,cz+.01*dz),(cx+.19*dx,cy,cz+.18*dz),'root')
    bone('head',(cx+.20*dx,cy,cz+.17*dz),(hi[0]-.04*dx,cy,cz+.21*dz),'spine')
    bone('tail',(cx-.16*dx,cy,cz),(lo[0]+.02*dx,cy,cz-.05*dz),'root')
    for sign,name in [(-1,'L'),(1,'R')]:
        bone(name+'_arm',(cx+.05*dx,cy+sign*.12*dy,cz+.08*dz),
             (cx+.18*dx,cy+sign*.39*dy,cz-.24*dz),'spine')
        bone(name+'_leg',(cx-.12*dx,cy+sign*.12*dy,cz-.08*dz),
             (cx-.17*dx,cy+sign*.29*dy,lo[2]+.05*dz),'root')
    bpy.ops.object.mode_set(mode='OBJECT')
    return (cx,cy,cz),(dx,dy,dz)


def weights(point,center,scale):
    x=(point[0]-center[0])/scale[0]
    y=(point[1]-center[1])/scale[1]
    z=(point[2]-center[2])/scale[2]
    # All vertices retain a stable torso or root influence; no loose mesh.
    w={'root':0.18,'spine':0.82}
    if x>.16 and z>-.01:
        factor=min(0.86,max(0.,(x-.16)/.21))
        w['head']=factor
        w['spine']*=1-factor
    if x<-.19 and abs(y)<.24:
        factor=min(.88,max(0.,(-x-.19)/.19))
        w['tail']=factor
        w['spine']*=1-factor
    if z<.02 and abs(y)>.10:
        side='R' if y>=0 else 'L'
        is_arm=x>-.01
        limb=side+('_arm' if is_arm else '_leg')
        factor=min(.88,max(0.,(abs(y)-.10)/.18))*min(1.,max(.15,(.20-z)/.30))
        w[limb]=factor
        w['spine']*=1-factor
    total=sum(w.values())
    return {k:v/total for k,v in w.items() if v>0.0001}


def rig_mesh(objects,arm,center,scale):
    influence={b.name:0 for b in arm.data.bones}
    for obj in objects:
        groups={bone.name:obj.vertex_groups.new(name=bone.name) for bone in arm.data.bones}
        for vertex in obj.data.vertices:
            world=obj.matrix_world @ vertex.co
            ws=weights(world,center,scale)
            for name,w in ws.items():
                groups[name].add([vertex.index],w,'REPLACE')
                influence[name]+=1
        before_world=obj.matrix_world.copy()
        obj.parent=arm
        obj.matrix_parent_inverse=arm.matrix_world.inverted()
        obj.matrix_world=before_world
        mod=obj.modifiers.new('Eldoria_Experimental_Armature','ARMATURE')
        mod.object=arm
        mod.use_vertex_groups=True
    return influence


def keyframe(arm):
    s=bpy.context.scene
    s.render.fps=24;s.frame_start=1;s.frame_end=40
    for b in arm.pose.bones:b.rotation_mode='XYZ'
    for f in (1,9,18,28,40):
        phase=2*math.pi*(f-1)/39
        for name in ('spine','head','tail','L_arm','R_arm','L_leg','R_leg'):
            b=arm.pose.bones[name]
            b.rotation_euler=(0.,0.,0.)
            if name=='spine':b.rotation_euler[1]=.04*math.sin(phase)
            if name=='head':b.rotation_euler[1]=.11*math.sin(phase)
            if name=='tail':b.rotation_euler[2]=.13*math.sin(phase)
            if name=='L_arm':b.rotation_euler[0]=.15*math.sin(phase)
            if name=='R_arm':b.rotation_euler[0]=-.15*math.sin(phase)
            if name.endswith('_leg'):b.rotation_euler[0]=.015*math.sin(phase)
            b.keyframe_insert(data_path='rotation_euler',frame=f,group=name)
    if arm.animation_data and arm.animation_data.action:
        arm.animation_data.action.name='Eldoria_Spawnling_RigFeasibility_Idle'
    s.frame_set(1)


def pose_snapshot(meshes,frame):
    bpy.context.scene.frame_set(frame)
    dg=bpy.context.evaluated_depsgraph_get()
    out=[]
    for obj in meshes:
        evaluated=obj.evaluated_get(dg)
        mesh=evaluated.to_mesh()
        try:
            stride=max(1,len(mesh.vertices)//2500)
            out.extend([evaluated.matrix_world@mesh.vertices[i].co
                        for i in range(0,len(mesh.vertices),stride)])
        finally:evaluated.to_mesh_clear()
    return out


def movement(meshes):
    a=pose_snapshot(meshes,1)
    b=pose_snapshot(meshes,9)
    if len(a)!=len(b):raise RuntimeError('Animated topology changed')
    displacement=[(x-y).length for x,y in zip(a,b)]
    m=max(displacement)
    moving=sum(d>.002 for d in displacement)
    if not (m>.004 and m<.35 and moving>20):
        raise RuntimeError('NO_MEANINGFUL_SAFE_SKELETAL_DEFORMATION: '+str((m,moving)))
    return {'max_sample_displacement_m':round(m,6),'moving_sample_vertices_gt2mm':moving,
            'samples':len(a),'source_frame':1,'posed_frame':9}


def render_proof(out,center,scale,frames):
    scene=bpy.context.scene
    scene.render.engine='CYCLES';scene.cycles.samples=8
    # Ubuntu's apt Blender package does not include OpenImageDenoiser.
    # Disable layer denoising explicitly, as in the existing artist A/B renderer.
    for layer in scene.view_layers:
        if hasattr(layer,'cycles'):
            layer.cycles.use_denoising=False
    # Hosted Ubuntu Blender build lacks OpenImageDenoiser.
    for layer in scene.view_layers:
        if hasattr(layer,'cycles'):
            layer.cycles.use_denoising=False

    scene.render.resolution_x=600;scene.render.resolution_y=480
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.film_transparent=False
    if scene.world is None:
        scene.world=bpy.data.worlds.new('RigQA_World')
    scene.world.color=(.10,.10,.12)
    diag=math.sqrt(sum(z*z for z in scale))
    data=bpy.data.cameras.new('RigQA_Camera')
    cam=bpy.data.objects.new('RigQA_Camera',data)
    scene.collection.objects.link(cam)
    cam.location=Vector(center)+Vector((diag*1.2,-diag*1.4,diag*1.0))
    cam.rotation_euler=(Vector(center)-cam.location).to_track_quat('-Z','Y').to_euler()
    data.type='ORTHO';data.ortho_scale=diag*1.70
    scene.camera=cam
    lamp=bpy.data.lights.new('RigQA_Light','AREA')
    light=bpy.data.objects.new('RigQA_Light',lamp)
    scene.collection.objects.link(light)
    light.location=Vector(center)+Vector((-diag,.1*diag,1.4*diag))
    light.rotation_euler=(Vector(center)-light.location).to_track_quat('-Z','Y').to_euler()
    lamp.energy=650;lamp.size=diag*2
    previews=[]
    for frame in frames:
        scene.frame_set(frame)
        name='spawnling-pose-%02d.png'%frame
        dest=os.path.join(out,name);scene.render.filepath=dest
        bpy.ops.render.render(write_still=True)
        if not os.path.isfile(dest) or os.path.getsize(dest)<4000:
            raise RuntimeError('MISSING_REAL_BLENDER_PREVIEW: '+name)
        previews.append({'frame':frame,'filename':name,'sha256':digest(dest)})
    return previews


def main():
    a=args()
    if not os.path.isfile(a.input) or digest(a.input)!=a.source_sha256:
        raise RuntimeError('SOURCE_GLB_HASH_MISMATCH_NO_GENERATION')
    os.makedirs(a.out,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if not meshes:raise RuntimeError('NO_MESH')
    if any(o.vertex_groups for o in meshes):raise RuntimeError('EXPECTED_STATIC_SOURCE')
    lo,hi=bounds(meshes)
    armd=bpy.data.armatures.new('Eldoria_Spawnling_Test_Bones')
    arm=bpy.data.objects.new('Eldoria_Spawnling_Test_Armature',armd)
    bpy.context.scene.collection.objects.link(arm)
    center,scale=coordinate_bones(arm,lo,hi)
    influence=rig_mesh(meshes,arm,center,scale)
    keyframe(arm)
    dynamics=movement(meshes)
    if influence['head']<25 or sum(influence[k] for k in ['L_arm','R_arm'])<30:
        raise RuntimeError('RIG_WEIGHTS_DID_NOT_CAPTURE_HEAD_AND_ARMS')
    output=os.path.join(a.out,'spawnling-rig-tech-test.glb')
    bpy.context.scene.frame_set(1)
    bpy.ops.export_scene.gltf(filepath=output,export_format='GLB',
        export_animations=True,export_skins=True,export_apply=False)
    blends=os.path.join(a.out,'spawnling-rig-tech-test.blend')
    bpy.ops.wm.save_as_mainfile(filepath=blends)
    previews=render_proof(a.out,center,scale,[1,9])
    result={
      'schema_version':1,'status':'BLENDER_FREE_RIG_TECH_CANDIDATE',
      'source_file':a.input,'source_sha256':a.source_sha256,
      'output_glb_sha256':digest(output),'output_blend_sha256':digest(blends),
      'weights_vertex_influence_counts':influence,'bones':len(arm.data.bones),
      'animation_action':arm.animation_data.action.name,
      'source_meshes':len(meshes),'deformation':dynamics,
      'visual':{'captures':previews,'blender_technical_preview_only':True},
      'tripo_api_calls':0,'paid_credits':0,
      'original_unchanged':digest(a.input)==a.source_sha256,
      'art_approved':False,'mobile_profiled':False,'unity_import_tested':False,
      'gameplay_integrated':False,
      'note':'Scripted approximate automatic weights, not Qwen-authored aesthetic revision. Requires face/limb deformation judgement, high-quality weight cleanup, native Unity animated import and mobile profile before use.'
    }
    with open(os.path.join(a.out,'rig-proof.json'),'w',encoding='utf8') as f:
        json.dump(result,f,indent=2)
    print('EXPERIMENTAL_BLENDER_SKINNED_MOVEMENT_TECH_ONLY='+json.dumps({
        'bones':result['bones'],'deformation':dynamics,'paid_credits':0}))


if __name__=='__main__':main()
