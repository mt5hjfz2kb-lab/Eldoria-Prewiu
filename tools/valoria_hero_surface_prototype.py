import argparse
import json
from pathlib import Path

import bpy
import numpy as np


def parse_args():
    import sys
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--report", required=True)
    return p.parse_args(args)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def _make_image(name, rgb, colorspace):
    h, w = rgb.shape[:2]
    image = bpy.data.images.new(name, width=w, height=h, alpha=True)
    alpha = np.ones((h, w, 1), dtype=np.float32)
    if rgb.ndim == 2:
        rgb = np.repeat(rgb[..., None], 3, axis=2)
    rgba = np.concatenate((np.clip(rgb, 0.0, 1.0), alpha), axis=-1)
    image.pixels.foreach_set(rgba.astype(np.float32).ravel())
    image.colorspace_settings.name = colorspace
    image.pack()
    return image


def _normal_from_height(name, height, strength):
    gy, gx = np.gradient(height)
    nx = -gx * strength
    ny = -gy * strength
    nz = np.ones_like(height)
    length = np.sqrt(nx * nx + ny * ny + nz * nz)
    normal = np.stack(
        (nx / length * 0.5 + 0.5, ny / length * 0.5 + 0.5, nz / length * 0.5 + 0.5),
        axis=-1,
    )
    return _make_image(name, normal, "Non-Color")


def make_pbr_material(name, family, size=1024):
    y, x = np.mgrid[0:size, 0:size].astype(np.float32)
    u = x / float(size - 1)
    v = y / float(size - 1)

    if family == "stone":
        # Large readable masonry variation first, micro-noise second.
        coarse = 0.5 + 0.18 * np.sin(u * 17.0 + v * 7.0) + 0.08 * np.cos(u * 39.0 - v * 23.0)
        joints_h = np.exp(-((np.mod(v * 8.0, 1.0) - 0.5) / 0.055) ** 2)
        row = np.floor(v * 8.0)
        stagger = np.mod(u * 7.0 + 0.5 * np.mod(row, 2.0), 1.0)
        joints_v = np.exp(-((stagger - 0.5) / 0.055) ** 2)
        joints = np.clip(np.maximum(joints_h, joints_v), 0.0, 1.0)
        height = np.clip(coarse * (1.0 - 0.25 * joints), 0.0, 1.0)
        dark = np.array([0.25, 0.245, 0.225], dtype=np.float32)
        light = np.array([0.43, 0.40, 0.355], dtype=np.float32)
        base = dark + (light - dark) * height[..., None]
        base *= (1.0 - 0.15 * joints[..., None])
        rough = np.clip(0.72 + 0.14 * joints + 0.06 * (1.0 - height), 0.70, 0.92)
        normal_strength = 0.45
    elif family == "rock":
        coarse = (
            0.50
            + 0.20 * np.sin(u * 11.0 + v * 9.0)
            + 0.12 * np.sin(u * 29.0 - v * 17.0)
            + 0.08 * np.cos(u * 61.0 + v * 47.0)
        )
        height = np.clip(coarse, 0.0, 1.0)
        dark = np.array([0.18, 0.20, 0.195], dtype=np.float32)
        light = np.array([0.33, 0.325, 0.295], dtype=np.float32)
        base = dark + (light - dark) * height[..., None]
        rough = np.clip(0.79 + 0.13 * (1.0 - height), 0.78, 0.95)
        normal_strength = 0.65
    else:
        raise RuntimeError("Unknown PBR family: " + family)

    base_img = _make_image(name + "_BaseColor", base, "sRGB")
    rough_img = _make_image(name + "_Roughness", rough, "Non-Color")
    normal_img = _normal_from_height(name + "_Normal", height, normal_strength)

    material = bpy.data.materials.new(name)
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    nodes.clear()

    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    base_node = nodes.new("ShaderNodeTexImage")
    base_node.image = base_img
    rough_node = nodes.new("ShaderNodeTexImage")
    rough_node.image = rough_img
    normal_tex = nodes.new("ShaderNodeTexImage")
    normal_tex.image = normal_img
    normal_map = nodes.new("ShaderNodeNormalMap")
    normal_map.inputs["Strength"].default_value = 0.22 if family == "stone" else 0.28

    links.new(base_node.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(rough_node.outputs["Color"], bsdf.inputs["Roughness"])
    links.new(normal_tex.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    if "Metallic" in bsdf.inputs:
        bsdf.inputs["Metallic"].default_value = 0.0
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.12

    return material, [base_img.name, rough_img.name, normal_img.name]


def classify_mesh(obj, stone, rock):
    mesh = obj.data
    mesh.materials.clear()
    mesh.materials.append(stone)
    mesh.materials.append(rock)

    zs = [v.co.z for v in mesh.vertices]
    zmin, zmax = min(zs), max(zs)
    span = max(zmax - zmin, 1e-6)

    stone_faces = 0
    rock_faces = 0
    for poly in mesh.polygons:
        n = poly.normal.normalized()
        axis = max(abs(n.x), abs(n.y), abs(n.z))
        z01 = (poly.center.z - zmin) / span

        # Diagnostic semantic split only. Architecture tends to be planar and/or
        # occupy the upper built mass; irregular lower geometry is treated as rock.
        architecture = axis >= 0.93 or (z01 >= 0.42 and axis >= 0.84)
        poly.material_index = 0 if architecture else 1
        if architecture:
            stone_faces += 1
        else:
            rock_faces += 1

    return stone_faces, rock_faces


def main():
    a = parse_args()
    clear_scene()
    bpy.ops.import_scene.gltf(filepath=a.input)

    stone, stone_textures = make_pbr_material("Eldoria Stone · Hero Prototype", "stone")
    rock, rock_textures = make_pbr_material("Eldoria Rock · Hero Prototype", "rock")

    total_stone = 0
    total_rock = 0
    triangles = 0
    vertices = 0
    mesh_count = 0
    uv_all = True

    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        mesh_count += 1
        vertices += len(obj.data.vertices)
        triangles += sum(len(p.vertices) - 2 for p in obj.data.polygons)
        uv_all = uv_all and len(obj.data.uv_layers) > 0
        s, r = classify_mesh(obj, stone, rock)
        total_stone += s
        total_rock += r

    if mesh_count == 0:
        raise RuntimeError("No mesh objects imported")
    if not uv_all:
        raise RuntimeError("Hero surface prototype requires UV0 on every mesh")
    if total_stone == 0 or total_rock == 0:
        raise RuntimeError("Hero surface prototype did not produce both Stone and Rock regions")

    Path(a.output).parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=a.output,
        export_format="GLB",
        export_materials="EXPORT",
        export_normals=True,
        export_texcoords=True,
    )

    report = {
        "schema_version": 2,
        "mode": "hero_surface_segmentation_pbr_prototype",
        "meshes": mesh_count,
        "vertices": vertices,
        "triangles": triangles,
        "uv_present_all_meshes": uv_all,
        "geometry_modified": False,
        "materials": [
            {
                "name": stone.name,
                "role": "Eldoria Stone",
                "textures": stone_textures,
                "texture_size": 1024,
                "metallic": 0.0,
            },
            {
                "name": rock.name,
                "role": "Eldoria Rock",
                "textures": rock_textures,
                "texture_size": 1024,
                "metallic": 0.0,
            },
        ],
        "face_classification": {
            "stone_faces": total_stone,
            "rock_faces": total_rock,
        },
        "scope_limit": "visual surface/identity diagnostic only; does not repair or certify traversal/interface geometry",
    }
    Path(a.report).write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
