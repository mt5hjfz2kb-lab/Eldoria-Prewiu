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
                "spine":(0,.052,-.020),"head":(0,-.10,-.015),
                "R_arm":(.52,-.145,-.26),
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
                27:(.17,.11,-.18),
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

def corrected_limb_weights(point,center,scale,original_weights=V1.weights):
    """V4.1 technical weight repair from measured source mesh connectivity.
    Feet occupy normalized x<=.13 below z=-.15; forearms start x>=.20.
    Original x>-.01 split incorrectly bound toes to a swiping arm.
    Smoothly transfer that existing influence to the same-side leg; keep
    positions, topology, bones, poses, materials and total weight unchanged.
    """
    ws=original_weights(point,center,scale)
    x=(point[0]-center[0])/scale[0]
    blend=V3.smoother((x-.14)/.12)
    for side in ("L","R"):
        arm=side+"_arm";leg=side+"_leg"
        transferred=ws.get(arm,0.0)*(1.0-blend)
        if transferred:
            ws[arm]-=transferred
            ws[leg]=ws.get(leg,0.0)+transferred
    return {name:w for name,w in ws.items() if w>.0000001}

def rig_with_repaired_weights(meshes,arm,center,scale):
    original=V1.weights
    V1.weights=corrected_limb_weights
    try:return V1.rig_mesh(meshes,arm,center,scale)
    finally:V1.weights=original

def verify_feet(meshes,center,scale):
    """Compare every actual evaluated foot vertex, never a sparse sample."""
    scene=bpy.context.scene
    feet={}
    for ob in meshes:
        ids=[]
        for v in ob.data.vertices:
            p=ob.matrix_world@v.co
            x=(p.x-center[0])/scale[0];z=(p.z-center[2])/scale[2]
            if -.19<x<.14 and z<-.28:ids.append(v.index)
        if ids:feet[ob.name]=ids
    def capture(frame):
        scene.frame_set(frame);dg=bpy.context.evaluated_depsgraph_get();out=[]
        for ob in meshes:
            ids=feet.get(ob.name,[])
            if not ids:continue
            ev=ob.evaluated_get(dg);mesh=ev.to_mesh()
            try:out.extend(ev.matrix_world@mesh.vertices[i].co for i in ids)
            finally:ev.to_mesh_clear()
        return out
    base=capture(1)
    if len(base)<150:raise RuntimeError("INSUFFICIENT_ACTUAL_FOOT_VERTICES")
    maximum=0.0
    for frame in [10,19,27,34,46,65]:
        maximum=max(maximum,max((a-b).length for a,b in zip(base,capture(frame))))
    scene.frame_set(1)
    if maximum>.055:raise RuntimeError("FOOT_STILL_DRIVEN_BY_ATTACK_ARM:"+str(maximum))
    return {"vertices":len(base),"max_displacement_m":maximum,"limit_m":.055}

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
    original_weights=rig_with_repaired_weights(meshes,arm,center,scale)
    bone_segments=V3.add_bone_segments(arm)
    distal_weights=V3.split_weight_groups(meshes,arm,bone_segments)
    weight_qa=V2.validate_rest_weights(meshes)
    if len(arm.data.bones)!=12 or distal_weights["total_vertices"]!=weight_qa["vertices"]:
        raise RuntimeError("RIG_MISSING_BONES_OR_WEIGHTS")
    animate_v4(arm)
    displacements=V2.all_displacements(meshes,1,[10,19,27,34,46,65])
    feet=verify_feet(meshes,center,scale)
    attack=displacements["27"]
    if not .17<=attack["max_m"]<=.35 or attack["moving_samples_gt2mm"]<300:
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
        "schema_version":4,"asset":"spawnling","weight_revision":"V4.1_FOOT_ARM_SEPARATION",
        "foot_motion":feet,
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
