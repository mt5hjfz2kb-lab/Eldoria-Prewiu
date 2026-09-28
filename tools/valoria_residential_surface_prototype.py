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


def make_image(name, rgb, colorspace):
    h, w = rgb.shape[:2]
    image = bpy.data.images.new(name, width=w, height=h, alpha=True)
    if rgb.ndim == 2:
        rgb = np.repeat(rgb[..., None], 3, axis=2)
    alpha = np.ones((h, w, 1), dtype=np.float32)
    rgba = np.concatenate((np.clip(rgb, 0.0, 1.0), alpha), axis=-1)
    image.pixels.foreach_set(rgba.astype(np.float32).ravel())
    image.colorspace_settings.name = colorspace
    image.pack()
    return image


def normal_from_height(name, height, strength):
    gy, gx = np.gradient(height)
    nx = -gx * strength
    ny = -gy * strength
    nz = np.ones_like(height)
    length = np.sqrt(nx * nx + ny * ny + nz * nz)
    normal = np.stack(
        (nx / length * 0.5 + 0.5, ny / length * 0.5 + 0.5, nz / length * 0.5 + 0.5),
        axis=-1,
    )
    return make_image(name, normal, "Non-Color")


def pbr_material(name, family, size=1024):
    y, x = np.mgrid[0:size, 0:size].astype(np.float32)
    u = x / float(size - 1)
    v = y / float(size - 1)

    if family == "rock":
        height = np.clip(
            0.50
            + 0.20 * np.sin(u * 11.0 + v * 9.0)
            + 0.12 * np.sin(u * 29.0 - v * 17.0)
            + 0.08 * np.cos(u * 61.0 + v * 47.0),
            0.0, 1.0
        )
        dark = np.array([0.18, 0.20, 0.195], dtype=np.float32)
        light = np.array([0.33, 0.325, 0.295], dtype=np.float32)
        rough = np.clip(0.80 + 0.13 * (1.0 - height), 0.79, 0.95)
        nstrength = 3.0
    elif family == "stone":
        mortar_h = np.exp(-((np.mod(v * 8.0, 1.0) - 0.5) / 0.055) ** 2)
        row = np.floor(v * 8.0)
        stagger = np.mod(u * 7.0 + 0.5 * np.mod(row, 2.0), 1.0)
        mortar_v = np.exp(-((stagger - 0.5) / 0.055) ** 2)
        mortar = np.clip(np.maximum(mortar_h, mortar_v), 0.0, 1.0)
        body = 0.52 + 0.13 * np.sin(u * 17.0 + v * 7.0) + 0.06 * np.cos(u * 39.0 - v * 23.0)
        height = np.clip(body * (1.0 - 0.24 * mortar), 0.0, 1.0)
        dark = np.array([0.25, 0.245, 0.225], dtype=np.float32)
        light = np.array([0.43, 0.40, 0.355], dtype=np.float32)
        rough = np.clip(0.72 + 0.14 * mortar + 0.05 * (1.0 - height), 0.71, 0.92)
        nstrength = 1.9
    elif family == "roof":
        # Dark slate-like family: strong macro bands survive isometric distance.
        bands = 0.5 + 0.20 * np.sin(v * 42.0) + 0.06 * np.sin(u * 13.0 + v * 17.0)
        height = np.clip(bands, 0.0, 1.0)
        dark = np.array([0.12, 0.135, 0.145], dtype=np.float32)
        light = np.array([0.24, 0.25, 0.245], dtype=np.float32)
        rough = np.clip(0.78 + 0.10 * (1.0 - height), 0.77, 0.91)
        nstrength = 1.6
    else:
        raise RuntimeError("Unknown family: " + family)

    base = dark + (light - dark) * height[..., None]
    base_img = make_image(name + "_BaseColor", base, "sRGB")
    rough_img = make_image(name + "_Roughness", rough, "Non-Color")
    normal_img = normal_from_height(name + "_Normal", height, nstrength)

    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()
    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    base_node = nodes.new("ShaderNodeTexImage"); base_node.image = base_img
    rough_node = nodes.new("ShaderNodeTexImage"); rough_node.image = rough_img
    normal_tex = nodes.new("ShaderNodeTexImage"); normal_tex.image = normal_img
    normal_map = nodes.new("ShaderNodeNormalMap")
    normal_map.inputs["Strength"].default_value = 0.48

    links.new(base_node.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(rough_node.outputs["Color"], bsdf.inputs["Roughness"])
    links.new(normal_tex.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    if "Metallic" in bsdf.inputs: bsdf.inputs["Metallic"].default_value = 0.0
    if "Specular IOR Level" in bsdf.inputs: bsdf.inputs["Specular IOR Level"].default_value = 0.24
    return mat, [base_img.name, rough_img.name, normal_img.name]


def classify(obj, rock, stone, roof):
    mesh = obj.data
    mesh.materials.clear()
    for mat in (rock, stone, roof):
        mesh.materials.append(mat)

    zs = [v.co.z for v in mesh.vertices]
    zmin, zmax = min(zs), max(zs)
    span = max(zmax - zmin, 1e-6)
    counts = {"rock": 0, "stone": 0, "roof": 0}

    for poly in mesh.polygons:
        n = poly.normal.normalized()
        z01 = (poly.center.z - zmin) / span
        horizontal = abs(n.z)
        vertical_axis = max(abs(n.x), abs(n.y))

        # Conservative semantic proxy:
        # - low irregular mass => rock;
        # - upper sloped planes => roof;
        # - planar/vertical built mass => architectural stone.
        if z01 < 0.36 and max(abs(n.x), abs(n.y), abs(n.z)) < 0.94:
            idx, role = 0, "rock"
        elif z01 > 0.58 and 0.25 < horizontal < 0.88:
            idx, role = 2, "roof"
        elif vertical_axis > 0.78 or horizontal > 0.90:
            idx, role = 1, "stone"
        else:
            idx, role = (0, "rock") if z01 < 0.45 else (1, "stone")

        poly.material_index = idx
        counts[role] += 1
    return counts


def main():
    a = parse_args()
    clear_scene()
    bpy.ops.import_scene.gltf(filepath=a.input)

    rock, rock_tex = pbr_material("Eldoria Rock · Residential Rescue", "rock")
    stone, stone_tex = pbr_material("Eldoria Stone · Residential Rescue", "stone")
    roof, roof_tex = pbr_material("Eldoria Roof · Residential Rescue", "roof")

    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh objects imported")

    uv_all = all(len(o.data.uv_layers) > 0 for o in meshes)
    if not uv_all:
        raise RuntimeError("Residential rescue requires UV0 on every mesh")

    totals = {"rock": 0, "stone": 0, "roof": 0}
    triangles = 0
    vertices = 0
    for obj in meshes:
        vertices += len(obj.data.vertices)
        triangles += sum(len(p.vertices) - 2 for p in obj.data.polygons)
        counts = classify(obj, rock, stone, roof)
        for k in totals: totals[k] += counts[k]

    if min(totals.values()) <= 0:
        raise RuntimeError("Residential rescue failed to produce all three semantic surface regions")

    Path(a.output).parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=a.output,
        export_format="GLB",
        export_materials="EXPORT",
        export_normals=True,
        export_texcoords=True,
    )

    report = {
        "schema_version": 1,
        "mode": "residential_surface_v1_prototype",
        "geometry_modified": False,
        "meshes": len(meshes),
        "vertices": vertices,
        "triangles": triangles,
        "uv_present_all_meshes": uv_all,
        "face_classification": totals,
        "materials": [
            {"role": "Eldoria Rock", "textures": rock_tex, "texture_size": 1024},
            {"role": "Eldoria Stone", "textures": stone_tex, "texture_size": 1024},
            {"role": "Eldoria Roof", "textures": roof_tex, "texture_size": 1024},
        ],
        "scope_limit": "Isolated visual rescue prototype only. Geometry, topology and gameplay authority remain unchanged."
    }
    Path(a.report).write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
