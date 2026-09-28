import argparse
import json
from pathlib import Path

import bpy


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


def make_material(name, color, roughness):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Metallic"].default_value = 0.0
    bsdf.inputs["Roughness"].default_value = roughness
    return m


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

        # Diagnostic segmentation only: deliberate planar masonry tends to be
        # axis-aligned; irregular lower geometry tends to be rock.
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

    stone = make_material("Eldoria Stone Prototype", (0.34, 0.32, 0.29), 0.78)
    rock = make_material("Eldoria Rock Prototype", (0.20, 0.22, 0.21), 0.92)

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
        "mode": "hero_surface_segmentation_prototype",
        "meshes": mesh_count,
        "vertices": vertices,
        "triangles": triangles,
        "uv_present_all_meshes": uv_all,
        "materials": [
            {"name": stone.name, "roughness": 0.78, "role": "Eldoria Stone"},
            {"name": rock.name, "roughness": 0.92, "role": "Eldoria Rock"},
        ],
        "face_classification": {
            "stone_faces": total_stone,
            "rock_faces": total_rock,
        },
        "scope_limit": "visual surface diagnostic only; does not repair or certify traversal/interface geometry",
    }
    Path(a.report).write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
