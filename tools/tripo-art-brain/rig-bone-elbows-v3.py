#!/usr/bin/env python3
"""Source-locked, FREE v3 anatomy/rigging hypothesis for existing Eldoria Spawnling.

This reuses the v1 rig and v2 pose renderer, adds independent distal elbow/knee
bones and redistributes previously normalised existing weights smoothly; no new
Tripo generation, Qwen attribution, Unity scene edit, or artistic approval.

Run only on a copy of the already-certified Spawnling_B2_Near.glb.
"""
import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

HERE = Path(__file__).resolve().parent


def load_relative(module_name, file_name):
    spec = importlib.util.spec_from_file_location(module_name, HERE / file_name)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


V1 = load_relative("spawnling_existing_rig_v1", "rig-feasibility-blender.py")
V2 = load_relative("spawnling_existing_rig_v2", "rig-finish-prototype-v2.py")
CLIP_NAME = "Eldoria_Spawnling_AlertSwipe_ElbowKneeV3"
DISTAL = {
    "L_arm": "L_forearm", "R_arm": "R_forearm",
    "L_leg": "L_shin", "R_leg": "R_shin",
}


def parse():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--source-sha256", required=True)
    p.add_argument("--out", required=True)
    return p.parse_args(args)


def add_bone_segments(arm):
    """Split existing physical limb bones at a real parent-local midpoint."""
    bpy.ops.object.select_all(action="DESELECT")
    arm.select_set(True)
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.mode_set(mode="EDIT")
    segments = {}
    for proximal, distal in DISTAL.items():
        parent = arm.data.edit_bones.get(proximal)
        if parent is None:
            raise RuntimeError("MISSING_SOURCE_BONE_" + proximal)
        old_head = parent.head.copy()
        old_tail = parent.tail.copy()
        midpoint = old_head.lerp(old_tail, 0.56 if proximal.endswith("arm") else 0.51)
        if (old_tail - midpoint).length < .01:
            raise RuntimeError("DISTAL_BONE_TOO_SHORT")
        child = arm.data.edit_bones.new(distal)
        parent.tail = midpoint
        child.head = midpoint
        child.tail = old_tail
        child.parent = parent
        child.use_connect = True
        segments[proximal] = {
            "proximal": proximal,
            "distal": distal,
            "base_head": [float(x) for x in old_head],
            "base_tail": [float(x) for x in old_tail],
            "bend_fraction": .56 if proximal.endswith("arm") else .51,
        }
    bpy.ops.object.mode_set(mode="OBJECT")
    if len(arm.data.bones) != 12:
        raise RuntimeError("EXPECTED_12_REAL_BONES")
    return segments


def smoother(value):
    t = max(0.0, min(1.0, value))
    return t * t * (3 - 2 * t)


def split_weight_groups(meshes, arm, segment_dict):
    """Conserve each original per-vertex weight sum, redistribute local limb weight.

    Weighted source geometry stays unchanged, but distal skinning now rotates
    with a child elbow/knee instead of swinging a whole rigid limb.
    """
    vertices = 0
    distributed = {name: {"vertex_count": 0, "weight_sum": 0.0} for name in DISTAL.values()}
    for ob in meshes:
        dest = {distal: ob.vertex_groups.new(name=distal) for distal in DISTAL.values()}
        proximal_groups = {name: ob.vertex_groups.get(name) for name in DISTAL}
        for v in ob.data.vertices:
            vertices += 1
            xyz = ob.matrix_world @ v.co
            for proximal, distal in DISTAL.items():
                src_group = proximal_groups[proximal]
                if src_group is None:
                    raise RuntimeError("MISSING_PROXIMAL_VERTEX_GROUP_" + proximal)
                try:
                    orig = src_group.weight(v.index)
                except RuntimeError:
                    continue
                if orig <= 0:
                    continue
                s = segment_dict[proximal]
                head, tail = Vector(s["base_head"]), Vector(s["base_tail"])
                vector = tail - head
                if vector.length_squared <= 1e-10:
                    raise RuntimeError("INVALID_SEGMENT_VECTOR")
                length_fraction = max(0.0, min(1.0, (xyz - head).dot(vector) / vector.length_squared))
                distal_fraction = smoother((length_fraction - .28) / .57)
                # Preserve at least a proximal contribution for smooth bending
                new_distal = orig * distal_fraction * .90
                if new_distal > .0002:
                    src_group.add([v.index], orig - new_distal, "REPLACE")
                    dest[distal].add([v.index], new_distal, "REPLACE")
                    distributed[distal]["vertex_count"] += 1
                    distributed[distal]["weight_sum"] += new_distal
    for name, info in distributed.items():
        info["weight_sum"] = round(info["weight_sum"], 5)
        if info["vertex_count"] < 25 or info["weight_sum"] < 1.0:
            raise RuntimeError("DISTAL_SKIN_WEIGHTS_TOO_SPARSE_" + name + ":" + str(info))
    return {"total_vertices": vertices, "distal_influences": distributed}


def animate(arm):
    scene = bpy.context.scene
    scene.render.fps = 24
    scene.frame_start = 1
    scene.frame_end = 65
    arm.animation_data_create()
    for b in arm.pose.bones:
        b.rotation_mode = "XYZ"
    for frame, baseline in V2.POSES.items():
        scene.frame_set(frame)
        pose = {name: tuple(values) for name, values in baseline.items()}
        # Lower upper-arm swing; a new hinged forearm provides attack articulation.
        if frame == 19:
            pose["R_arm"] = (-.27, .07, .095)
        if frame == 27:
            pose["R_arm"] = (.46, -.11, -.25)
            pose["L_arm"] = (.17, -.035, -.12)
        pose.update({
            "L_forearm": {
                1:(0,0,0),10:(.02,0,0),19:(-.07,0,.03),27:(.08,-.04,-.09),
                34:(.06,0,-.035),46:(-.02,0,.015),65:(0,0,0)
            }[frame],
            "R_forearm": {
                1:(0,0,0),10:(-.035,0,-.025),19:(-.27,.11,.06),27:(.16,.10,.07),
                34:(-.13,.04,.025),46:(-.025,0,0),65:(0,0,0)
            }[frame],
            "L_shin": {
                1:(0,0,0),10:(.018,0,0),19:(.035,0,0),27:(-.025,0,.018),
                34:(.012,0,0),46:(0,0,0),65:(0,0,0)
            }[frame],
            "R_shin": {
                1:(0,0,0),10:(-.016,0,0),19:(-.030,0,0),27:(.026,0,-.016),
                34:(-.010,0,0),46:(0,0,0),65:(0,0,0)
            }[frame],
        })
        for name, angles in pose.items():
            bone = arm.pose.bones.get(name)
            if bone is None:
                raise RuntimeError("MISSING_POSE_BONE_" + name)
            bone.rotation_euler = angles
            bone.keyframe_insert(data_path="rotation_euler", frame=frame, group=name)
        if frame in (1,65):
            root = arm.pose.bones["root"]
            root.location = (0,0,0)
            root.keyframe_insert(data_path="location", frame=frame, group="root")
    if arm.animation_data.action is None:
        raise RuntimeError("NO_NEW_ANIMATION_ACTION")
    arm.animation_data.action.name = CLIP_NAME
    scene.frame_set(1)


def main():
    a = parse()
    source = Path(a.input)
    if not source.is_file() or V1.digest(str(source)) != a.source_sha256:
        raise RuntimeError("SOURCE_HASH_MISMATCH_FORBID_REGENERATION")
    out = Path(a.out)
    out.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(source))
    meshes = [x for x in bpy.context.scene.objects if x.type=="MESH"]
    if not meshes or any(x.vertex_groups for x in meshes):
        raise RuntimeError("EXPECTED_STATIC_UNSKINNED_SOURCE")
    lo, hi = V1.bounds(meshes)
    bones = bpy.data.armatures.new("Eldoria_Spawnling_V3_Segmented_Bones")
    arm = bpy.data.objects.new("Eldoria_Spawnling_V3_Armature", bones)
    bpy.context.scene.collection.objects.link(arm)
    center,scale = V1.coordinate_bones(arm,lo,hi)
    original_influence = V1.rig_mesh(meshes,arm,center,scale)
    segments = add_bone_segments(arm)
    splits = split_weight_groups(meshes,arm,segments)
    weight_qa = V2.validate_rest_weights(meshes)
    if splits["total_vertices"] != weight_qa["vertices"]:
        raise RuntimeError("SOURCE_VERTEX_COUNT_CHANGED")
    animate(arm)
    dynamics = V2.all_displacements(meshes,1,[10,19,27,34,46,65])
    clip = out / "spawnling-rig-elbow-knee-v3.glb"
    bpy.context.scene.frame_set(1)
    bpy.ops.export_scene.gltf(filepath=str(clip),
                              export_format="GLB", export_animations=True,
                              export_skins=True,export_apply=False)
    blend = out / "spawnling-rig-elbow-knee-v3.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    poses=V1.render_proof(str(out),center,scale,[1,10,19,27,34,46,65])
    result={
        "schema_version":3,"status":"EXPERIMENTAL_DISTAL_BONE_TECH_CANDIDATE",
        "source_sha256":a.source_sha256,
        "source_meshes":len(meshes),"source_vertices":weight_qa["vertices"],
        "bone_count":len(arm.data.bones),"distal_bone_segments":segments,
        "original_weight_coverage":original_influence,
        "new_distal_weight_coverage":splits,
        "skin_weights":weight_qa,"clip":CLIP_NAME,"clip_frames":[1,65],"fps":24,
        "deformation_by_pose":dynamics,
        "visual":{"captures":poses,"review_required":True,
                  "fixed_blender_camera":True,"unity_captures":False},
        "glb_sha256":V1.digest(str(clip)),"blend_sha256":V1.digest(str(blend)),
        "source_unchanged":V1.digest(str(source))==a.source_sha256,
        "new_tripo_credits":0,"tripo_api_calls":0,"qwen_inference":"NOT_EXECUTED",
        "art_approved":False,"unity_import_tested":False,"gameplay_integrated":False,
        "mobile_profiled":False,
        "note":"V3 geometry unchanged, 12-bone procedural articulation/weights candidate. Independent pose quality and engine import still required."
    }
    (out/"rig-elbow-knee-v3-report.json").write_text(json.dumps(result,indent=2)+"\n","utf8")
    print("SPAWNLING_V3_SEGMENTED_RIG_TECH",
          json.dumps({"bones":result["bone_count"],
                      "strike_delta":dynamics["27"]["max_m"],
                      "distal_bones":list(DISTAL.values()),
                      "credits":0,"art_approved":False}))


if __name__=="__main__":
    main()
