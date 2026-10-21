#!/usr/bin/env python3
"""Neutral, camera-locked Blender QA renders of LODs exported by the existing
Eldoria Blender capability proof. NOT artistic approval or game render.
No generated geometry, Tripo API, scene modification or source overwrite.
Run with: blender -b --python tools/visual-lod-qa/render.py -- --source ... --lod-dir ... --out ...
"""
import argparse
import hashlib
import json
import math
import os
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def args():
    a = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--source", required=True)
    p.add_argument("--lod-dir", required=True)
    p.add_argument("--out", required=True)
    p.add_argument("--size", type=int, default=640)
    p.add_argument("--texture-variant", default="", help="Optional static extra LOD2 texture-only GLB candidate")
    result = p.parse_args(a)
    if result.size < 256 or result.size > 1024:
        raise ValueError("Invalid QA render size")
    if not os.path.isfile(result.source):
        raise FileNotFoundError(result.source)
    for i in range(3):
        if not os.path.isfile(os.path.join(result.lod_dir, f"lod{i}.glb")):
            raise FileNotFoundError(f"lod{i}.glb")
    if result.texture_variant and not os.path.isfile(result.texture_variant):
        raise FileNotFoundError(result.texture_variant)
    os.makedirs(result.out, exist_ok=True)
    return result


def clean_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def import_asset(glb):
    bpy.ops.import_scene.gltf(filepath=str(glb))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("Imported GLB contains no 3D mesh")
    return meshes


def bounds(meshes):
    points = [obj.matrix_world @ Vector(v) for obj in meshes for v in obj.bound_box]
    lo = [min(v[k] for v in points) for k in range(3)]
    hi = [max(v[k] for v in points) for k in range(3)]
    center = Vector([(a + b) / 2 for a, b in zip(lo, hi)])
    dims = [b - a for a, b in zip(lo, hi)]
    diagonal = math.sqrt(sum(d*d for d in dims))
    if diagonal < 0.05:
        raise RuntimeError("Unreasonable source dimensions")
    return center, dims, diagonal


def make_light(name, loc, center, power, size):
    data = bpy.data.lights.new(name=name, type="AREA")
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    obj.rotation_euler = (center - obj.location).to_track_quat("-Z", "Y").to_euler()
    data.energy = power
    data.shape = "DISK"
    data.size = size


def set_world_and_camera(center, diag, size):
    scene = bpy.context.scene
    # Cycles CPU is stable on headless GitHub Ubuntu; no GPU/display needed.
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 12
    # Ubuntu's free Blender 4.0.2 build does not include OpenImageDenoiser.
    # Explicitly disable render denoising; do not install paid/GPU dependencies.
    if hasattr(scene.cycles, "use_denoising"):
        scene.cycles.use_denoising = False
    if hasattr(bpy.context.view_layer.cycles, "use_denoising"):
        bpy.context.view_layer.cycles.use_denoising = False
    scene.render.threads_mode = "FIXED"
    scene.render.threads = 4
    scene.render.resolution_x = size
    scene.render.resolution_y = size
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.look = "Medium High Contrast"
    scene.render.filepath = ""
    scene.world.use_nodes = True
    bg = scene.world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value = (0.11, 0.13, 0.16, 1)
    bg.inputs["Strength"].default_value = 0.8
    camera_data = bpy.data.cameras.new("LOD_QA_Locked_Camera")
    camera = bpy.data.objects.new("LOD_QA_Locked_Camera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = diag * 1.65
    scene.camera = camera
    make_light("LOD_QA_Key", center + Vector((diag*1.1, -diag*1.3, diag*1.7)),
               center, 500 * (diag / 2)**2, diag * 1.7)
    make_light("LOD_QA_Fill", center + Vector((-diag*1.2, diag*0.5, diag*0.8)),
               center, 250 * (diag / 2)**2, diag * 2.5)
    return camera


def keep_qa_equipment():
    for obj in list(bpy.data.objects):
        if obj.name.startswith("LOD_QA_"):
            continue
        bpy.data.objects.remove(obj, do_unlink=True)


def sha256(filepath):
    h = hashlib.sha256()
    with open(filepath, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def main():
    a = args()
    clean_scene()
    mesh = import_asset(a.source)
    center, dims, diagonal = bounds(mesh)
    keep_qa_equipment()  # Clears source without editing the frozen file.
    camera = set_world_and_camera(center, diagonal, a.size)
    views = {
        "isometric": (1.25, -1.50, 1.05),
        "strategic": (1.30, -1.35, 2.20),
    }
    outputs = []
    models = [(str(lod), os.path.join(a.lod_dir, f"lod{lod}.glb")) for lod in range(3)]
    if a.texture_variant:
        models.append(("2_1k", a.texture_variant))
    for lod, source_path in models:
        imported = import_asset(source_path)
        for label, axis in views.items():
            camera.location = center + Vector(axis).normalized() * diagonal * 2.7
            camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
            png = os.path.join(a.out, f"lod{lod}-{label}.png")
            bpy.context.scene.render.filepath = png
            bpy.ops.render.render(write_still=True)
            if not Path(png).is_file() or Path(png).stat().st_size < 5000:
                raise RuntimeError(f"Missing or empty genuine Blender render: {png}")
            outputs.append({
                "lod": lod, "view": label,
                "file": os.path.basename(png), "sha256": sha256(png),
                "camera_world": [round(float(x), 6) for x in camera.location],
                "camera_target_world": [round(float(x), 6) for x in center],
                "ortho_scale": round(float(camera.data.ortho_scale), 6),
                "resolution": [a.size, a.size],
                "render_engine": bpy.context.scene.render.engine,
                "kind": "BLENDER_ISOLATED_QA_NOT_UNITY_GAMEPLAY"
            })
        for ob in imported:
            # Mesh import may have parent nodes; remove them as well before next LOD.
            if ob.name in bpy.data.objects:
                bpy.data.objects.remove(ob, do_unlink=True)
        keep_qa_equipment()
    result = {
        "source": a.source, "source_sha256": sha256(a.source),
        "bounds": [round(float(x), 6) for x in dims],
        "camera_lock": "Same original source center, view vectors, orthographic scale, lighting and resolution for all three LODs.",
        "visual_verdict": "REVIEW_REQUIRED",
        "mobile_device_profiled": False,
        "tripo_credits_used": 0,
        "texture_variant_previewed": bool(a.texture_variant),
        "captures": outputs
    }
    with open(os.path.join(a.out, "render-evidence.json"), "w", encoding="utf-8") as f:
        json.dump(result, f, indent=2)
    print("ELDORIA_TRIPO_LOD_RENDER_EVIDENCE=" + json.dumps({"captures": len(outputs), "status": "REVIEW_REQUIRED"}))


if __name__ == "__main__":
    main()
