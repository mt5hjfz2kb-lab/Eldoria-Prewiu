#!/usr/bin/env python3
"""Eldoria Spawnling v4: controlled, articulated swipe using existing 12-bone
v3 skinning. Zero Tripo API credits. Independent isolated test, never game edit.

The animation is procedural technician-authored, NOT evidence of Qwen authorship
or ART_APPROVED. Rest/mesh/source preserved. Requires genuine visual+Unity QA.
"""
import hashlib
import importlib.util
import json
import os
import sys
from pathlib import Path
import bpy

HERE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location("eldoria_spawnling_v3_reuse",HERE/"rig-bone-elbows-v3.py")
V3=importlib.util.module_from_spec(spec)
spec.loader.exec_module(V3)
V1=V3.V1
V2=V3.V2
CLIP="Eldoria_Spawnling_ExpressiveClawSwipe_V4"

def animate_v4(arm):
    scene=bpy.context.scene
    scene.render.fps=24
    scene.frame_start=1
    scene.frame_end=65
    arm.animation_data_create()
    for bone in arm.pose.bones:
        bone.rotation_mode="XYZ"
    # A targeted reach, not whole-body translation. More extension than v3;
    # forearm counter-bend and a braced lead leg preserve anatomical contact.
    for frame,orig in V2.POSES.items():
        scene.frame_set(frame)
        pose={name:tuple(a) for name,a in orig.items()}
        if frame==19:
            pose.update({"spine":(0,-.065,.015),"R_arm":(-.36,.09,.12),"head":(0,.06,0)})
        if frame==27:
            pose.update({
                "spine":(0,.075,-.025),"head":(0,-.10,-.015),
                "R_arm":(.78,-.22,-.39),
                "L_arm":(.12,-.04,-.08),
                "R_leg":(-.045,0,0),"L_leg":(.045,0,0),
                "tail":(0,0,.08)
            })
        if frame==34:
            pose.update({"spine":(0,.045,-.01),"R_arm":(.24,-.07,-.13)})
        pose.update({
            "R_forearm": {
                1:(0,0,0),10:(-.05,.015,-.02),
                19:(-.36,.12,.08),
                27:(.26,.17,-.31),
                34:(-.18,.045,.075),
                46:(-.045,0,.015),65:(0,0,0)
            }[frame],
            "L_forearm": {
                1:(0,0,0),10:(.025,0,0),
                19:(-.045,0,.04),27:(.11,-.035,-.055),
                34:(.045,0,-.035),46:(-.015,0,.015),65:(0,0,0)
            }[frame],
            "R_shin": {
                1:(0,0,0),10:(-.016,0,0),
                19:(-.045,.005,0),27:(.035,0,-.025),
                34:(-.015,0,0),46:(0,0,0),65:(0,0,0)
            }[frame],
            "L_shin": {
                1:(0,0,0),10:(.018,0,0),
                19:(.04,-.005,0),27:(-.03,0,.02),
                34:(.015,0,0),46:(0,0,0),65:(0,0,0)
            }[frame]
        })
        for name,angles in pose.items():
            b=arm.pose.bones.get(name)
            if b is None:raise RuntimeError("MISSING_ANIMATION_BONE_"+name)
            b.rotation_euler=angles
            b.keyframe_insert(data_path="rotation_euler",frame=frame,group=name)
        if frame in (1,65):
            root=arm.pose.bones["root"]
            root.location=(0,0,0)
            root.keyframe_insert(data_path="location",frame=frame,group="root")
    if not arm.animation_data.action:raise RuntimeError("EMPTY_REAL_ANIMATION")
    arm.animation_data.action.name=CLIP
    scene.frame_set(1)

def main():
    a=V3.parse()
    source=Path(a.input)
    if not source.is_file() or V1.digest(str(source))!=a.source_sha256:
        raise RuntimeError("SOURCE_SHA_MISMATCH_NO_REGENERATION")
    out=Path(a.out)
    out.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(source))
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    if not meshes or any(o.vertex_groups for o in meshes):
        raise RuntimeError("EXPECTED_STATIC_UNSKINNED_SOURCE")
    lo,hi=V1.bounds(meshes)
    rig=bpy.data.armatures.new("Eldoria_Spawnling_V4_Segmented_Bones")
    arm=bpy.data.objects.new("Eldoria_Spawnling_V4_Armature",rig)
    bpy.context.scene.collection.objects.link(arm)
    center,scale=V1.coordinate_bones(arm,lo,hi)
    original_weights=V1.rig_mesh(meshes,arm,center,scale)
    bone_segments=V3.add_bone_segments(arm)
    distal_weights=V3.split_weight_groups(meshes,arm,bone_segments)
    weight_qa=V2.validate_rest_weights(meshes)
    if len(arm.data.bones)!=12 or distal_weights["total_vertices"]!=weight_qa["vertices"]:
        raise RuntimeError("RIG_MISSING_BONES_OR_WEIGHTS")
    animate_v4(arm)
    displacements=V2.all_displacements(meshes,1,[10,19,27,34,46,65])
    attack=displacements["27"]
    if not .17<=attack["max_m"]<=.45 or attack["moving_samples_gt2mm"]<300:
        raise RuntimeError("V4_STRIKE_NOT_SUITABLE_FOR_VISUAL_REVIEW:"+str(attack))
    if displacements["65"]["max_m"]>.002:
        raise RuntimeError("NON_LOOPING_ANIMATION")
    bpy.context.scene.frame_set(1)
    glb=out/"spawnling-rig-expressive-swipe-v4.glb"
    bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",export_animations=True,export_skins=True,export_apply=False)
    blend=out/"spawnling-rig-expressive-swipe-v4.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    poses=V1.render_proof(str(out),center,scale,[1,10,19,27,34,46,65])
    report={
        "schema_version":4,"asset":"spawnling",
        "status":"V4_BLENDER_TECH_CANDIDATE_INDEPENDENT_ART_REVIEW_REQUIRED",
        "source_sha256":a.source_sha256,"source_unchanged":V1.digest(str(source))==a.source_sha256,
        "glb_sha256":V1.digest(str(glb)),"blend_sha256":V1.digest(str(blend)),
        "bone_count":len(arm.data.bones),"skin_weights":weight_qa,
        "source_vertices":weight_qa["vertices"],"bone_segments":bone_segments,
        "distal_weight_coverage":distal_weights,
        "clip_name":CLIP,"clip_frames":[1,65],"fps":24,"deformation_by_pose":displacements,
        "fixed_camera_blender_captures":poses,"tripo_api_calls":0,"credits_spent":0,
        "qwen_artist_authored":False,"independent_art_approved":False,
        "unity_skinned_import_pass":False,"mobile_pass":False,"gameplay_integrated":False
    }
    (out/"rig-expressive-v4-report.json").write_text(json.dumps(report,indent=2)+"\n","utf8")
    print("SPAWNLING_V4_REAL_RIG_BOUNDED_STRIKE",json.dumps({"bones":12,"attack_m":attack["max_m"],"credits":0}))
if __name__=="__main__":
    main()
