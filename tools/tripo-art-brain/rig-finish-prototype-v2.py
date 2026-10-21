#!/usr/bin/env python3
"""Spawnling Blender animation v2: ZERO-CREDIT isolated, source-pinned feasibility.

Reuses Eldoria's v1 actual armature/weighting routines; v2 adds a recognizable
staged alert -> windup -> claw swipe -> recoil -> settle sequence rather than
rotating the entire creature. Technical demonstration only: not artistic approval
or a production rig. Never touches Unity/gameplay, Tripo API, existing .blend/GLB.
"""
import argparse
import hashlib
import importlib.util
import json
import math
import os
import pathlib
import sys

import bpy

HERE=pathlib.Path(__file__).resolve().parent
SPEC=importlib.util.spec_from_file_location("eldoria_spawnling_rig_v1",
                                             HERE/"rig-feasibility-blender.py")
V1=importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(V1)

CLIP="Eldoria_Spawnling_AlertSwipe_Recover_TechV2"
# Positions and poses are measured in Blender source-local armature axes, not
# screen-space pixels. Axes are a first hypothesis, NOT human-authored rigging.
POSES={
    1: {"spine":(0,0,0),"head":(0,0,0),"tail":(0,0,0),"L_arm":(0,0,0),"R_arm":(0,0,0),"L_leg":(0,0,0),"R_leg":(0,0,0)},
    10:{"spine":(0,.035,0),"head":(0,-.025,0),"tail":(0,0,.085),"L_arm":(.065,0,.020),"R_arm":(-.050,0,-.025),"L_leg":(.008,0,0),"R_leg":(-.008,0,0)},
    19:{"spine":(0,-.115,.025),"head":(0,.095,0),"tail":(0,0,-.13),"L_arm":(-.14,0,.07),"R_arm":(-.31,.085,.11),"L_leg":(.015,0,0),"R_leg":(-.015,0,0)},
    27:{"spine":(0,.185,-.06),"head":(0,-.175,-.040),"tail":(0,0,.14),"L_arm":(.20,-.035,-.15),"R_arm":(.53,-.14,-.32),"L_leg":(-.018,0,0),"R_leg":(.020,0,0)},
    34:{"spine":(0,.035,.01),"head":(0,-.075,.01),"tail":(0,0,.075),"L_arm":(.10,0,-.06),"R_arm":(.12,0,-.045),"L_leg":(0,0,0),"R_leg":(0,0,0)},
    46:{"spine":(0,-.04,0),"head":(0,.02,0),"tail":(0,0,-.06),"L_arm":(-.045,0,.045),"R_arm":(-.055,0,.02),"L_leg":(0,0,0),"R_leg":(0,0,0)},
    65:{"spine":(0,0,0),"head":(0,0,0),"tail":(0,0,0),"L_arm":(0,0,0),"R_arm":(0,0,0),"L_leg":(0,0,0),"R_leg":(0,0,0)},
}


def parser():
    argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--input",required=True)
    p.add_argument("--source-sha256",required=True)
    p.add_argument("--out",required=True)
    return p.parse_args(argv)


def make_clip(arm):
    scene=bpy.context.scene
    scene.render.fps=24
    scene.frame_start=1
    scene.frame_end=65
    arm.animation_data_create()
    for pb in arm.pose.bones:
        pb.rotation_mode="XYZ"
    for frame,pose in POSES.items():
        scene.frame_set(frame)
        for bone_name,angles in pose.items():
            pb=arm.pose.bones.get(bone_name)
            if pb is None:
                raise RuntimeError("MISSING_ANIMATABLE_BONE_"+bone_name)
            pb.rotation_euler=angles
            pb.keyframe_insert(data_path="rotation_euler",frame=frame,group=bone_name)
        if frame==1 or frame==65:
            root=arm.pose.bones["root"]
            root.location=(0,0,0)
            root.keyframe_insert(data_path="location",frame=frame,group="root")
    if not arm.animation_data.action:
        raise RuntimeError("ANIMATION_ACTION_MISSING")
    arm.animation_data.action.name=CLIP
    scene.frame_set(1)


def all_displacements(meshes,base_frame,poses):
    src=V1.pose_snapshot(meshes,base_frame)
    result={}
    for frame in poses:
        dst=V1.pose_snapshot(meshes,frame)
        if len(src)!=len(dst):
            raise RuntimeError("ANIMATION_CHANGED_TOPOLOGY")
        ds=[(a-b).length for a,b in zip(src,dst)]
        ranked=sorted(ds)
        moving=sum(d>.002 for d in ds)
        result[str(frame)]={
            "compared_with_frame":base_frame,
            "max_m":round(max(ds),6),
            "mean_m":round(sum(ds)/len(ds),6),
            "p90_m":round(ranked[int(len(ranked)*.9)],6),
            "moving_samples_gt2mm":moving,
            "samples":len(ds)
        }
    strike=result["27"]
    if not .055<=strike["max_m"]<=.35 or strike["moving_samples_gt2mm"]<150:
        raise RuntimeError("ATTACK_POSE_HAS_NO_BOUNDED_MOVEMENT: "+repr(strike))
    if result["10"]["moving_samples_gt2mm"]<30:
        raise RuntimeError("ALERT_POSE_HAS_NO_MOVEMENT")
    if result["34"]["moving_samples_gt2mm"]<30:
        raise RuntimeError("RECOVERY_POSE_HAS_NO_MOVEMENT")
    if result["65"]["max_m"]>.002:
        raise RuntimeError("LOOP_DOES_NOT_RETURN_TO_SOURCE_REST")
    return result


def validate_rest_weights(meshes):
    count=0
    maxerr=0.
    bad=0
    allnames=set()
    for ob in meshes:
        names={group.index:group.name for group in ob.vertex_groups}
        for vert in ob.data.vertices:
            groups=[(names[g.group],g.weight) for g in vert.groups if g.group in names]
            total=sum(w for _,w in groups)
            maxerr=max(maxerr,abs(total-1.))
            count+=1
            if abs(total-1.)>.005 or not 1<=len(groups)<=8 or any(not math.isfinite(w) or w<0 for _,w in groups):
                bad+=1
            allnames.update(name for name,_ in groups)
    if bad or not count:
        raise RuntimeError(f"INVALID_SKIN_WEIGHTS: {bad}/{count}, maxerr {maxerr:.6f}")
    if not {"head","spine","tail","L_arm","R_arm","L_leg","R_leg"}.issubset(allnames):
        raise RuntimeError("SKIN_LIMB_BONES_NOT_ASSIGNED")
    return {"vertices":count,"invalid":bad,"max_weight_sum_error":round(maxerr,8),
            "weighted_bone_names":sorted(allnames)}


def main():
    a=parser()
    if not os.path.isfile(a.input) or V1.digest(a.input)!=a.source_sha256:
        raise RuntimeError("IMMUTABLE_SOURCE_SHA_MISMATCH_NO_TRIPO")
    out=pathlib.Path(a.out)
    out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    meshes=[ob for ob in bpy.context.scene.objects if ob.type=="MESH"]
    if not meshes or any(ob.vertex_groups for ob in meshes):
        raise RuntimeError("EXPECTED_STATIC_SPAWNLING_SOURCE")
    lo,hi=V1.bounds(meshes)
    armd=bpy.data.armatures.new("Eldoria_Spawnling_TechV2_Bones")
    arm=bpy.data.objects.new("Eldoria_Spawnling_TechV2_Armature",armd)
    bpy.context.scene.collection.objects.link(arm)
    center,scale=V1.coordinate_bones(arm,lo,hi)
    influence=V1.rig_mesh(meshes,arm,center,scale)
    weights=validate_rest_weights(meshes)
    make_clip(arm)
    displacement=all_displacements(meshes,1,[10,19,27,34,46,65])
    scene=bpy.context.scene
    # Only the existing source meshes and new v2 skin/armature are exported.
    scene.frame_set(1)
    dest=out/"spawnling-rig-gesture-v2.glb"
    bpy.ops.export_scene.gltf(filepath=str(dest),export_format="GLB",
                              export_animations=True,export_skins=True,
                              export_apply=False)
    blend=out/"spawnling-rig-gesture-v2.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    # Same hero-camera setup for the entire motion: no pose-specific reframing.
    poses=V1.render_proof(str(out),center,scale,[1,10,19,27,34,46,65])
    summary={
       "schema_version":2,
       "status":"BLENDER_PROCEDURAL_ANIMATED_GESTURE_TECH_CANDIDATE",
       "source_sha256":a.source_sha256,
       "source_meshes":len(meshes),
       "output_glb_sha256":V1.digest(dest),
       "output_blend_sha256":V1.digest(blend),
       "bones":len(arm.data.bones),
       "skin_weights":weights,
       "weight_coverage_by_bone":influence,
       "animation_action":CLIP,
       "clip_frames":[1,65],
       "clip_fps":24,
       "pose_frames":list(POSES),
       "deformation_by_pose":displacement,
       "visual":{"captures":poses,"camera_fixed_for_all_poses":True,
                 "review_required":True,"unity_runtime_captures":False},
       "paid_credits":0,"tripo_api_calls":0,
       "source_unchanged":V1.digest(a.input)==a.source_sha256,
       "art_approved":False,"unity_animated_import":False,
       "mobile_profiled":False,"gameplay_integrated":False,
       "quality_disclaimer":"Procedural bone coordinates and skin weights, sample threat/attack gesture only. Not a Qwen-authored sculpt or production-rated character rig."
    }
    (out/"rig-gesture-v2-report.json").write_text(json.dumps(summary,indent=2)+"\n","utf8")
    print("SPAWNLING_GESTURE_V2_ZERO_CREDIT_TECH="+json.dumps({
         "bones":summary["bones"],"frames":summary["clip_frames"],
         "max_motion_m":displacement["27"]["max_m"],
         "credits":0,"art_approved":False}))


if __name__=="__main__":
    main()
