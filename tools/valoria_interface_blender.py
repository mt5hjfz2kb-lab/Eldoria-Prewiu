"""Non-destructive interface metadata pass for six certified Valoria GLBs.

Tripo geometry is one fused mesh; until walkability is proven, do not delete
unclassified architecture. Explicit GLB nodes provide repeatable snap targets.
"""
import argparse
import json
import os
import sys
import bpy
from mathutils import Vector

SOCKETS = {
    "GateStreetRiseRock_MV1": {
        "Street_In": (0.0, 0.045, 0.35),
        "Street_Out": (0.0, 0.285, -0.25),
        "RockOverlap_Left": (-0.35, 0.05, 0.0),
        "RockOverlap_Right": (0.35, 0.05, 0.0),
    },
    "StreetLandingTransition": {
        "Street_In": (0.0, 0.085, 0.35),
        "Street_Out": (0.0, 0.170, -0.35),
        "Terrace_L1": (0.0, 0.125, 0.0),
        "RockOverlap_Left": (-0.19, 0.03, 0.0),
        "RockOverlap_Right": (0.19, 0.03, 0.0),
    },
    "ResidentialTerraceRock": {
        "Frontage_L1": (-0.25, 0.115, 0.32),
        "Terrace_L2": (-0.25, 0.205, -0.33),
        "RockOverlap_Left": (-0.40, 0.04, 0.0),
        "RockOverlap_Right": (0.40, 0.04, 0.0),
    },
    "TerraceStairRock": {
        "Street_In": (0.0, 0.030, 0.35),
        "Terrace_L2": (0.0, 0.270, -0.35),
        "RockOverlap_Left": (-0.30, 0.04, 0.0),
        "RockOverlap_Right": (0.30, 0.04, 0.0),
    },
    "RockTerrainSeamFiller": {
        "RockOverlap_Left": (-0.38, 0.12, 0.0),
        "RockOverlap_Right": (0.38, 0.12, 0.0),
        "RockOverlap_In": (0.0, 0.12, 0.38),
        "RockOverlap_Out": (0.0, 0.31, -0.38),
    },
    "TowerWallRock": {
        "RockOverlap_Left": (-0.36, 0.08, 0.0),
        "RockOverlap_Right": (0.36, 0.08, 0.0),
        "DefensiveEdge": (0.0, 0.18, -0.30),
    },
}


def metrics():
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    for m in meshes:
        m.data.calc_loop_triangles()
    return {
        "triangles": sum(len(m.data.loop_triangles) for m in meshes),
        "blender_vertices": sum(len(m.data.vertices) for m in meshes),
        "mesh_objects": len(meshes),
        "materials": sum(len(m.data.materials) for m in meshes),
        "uv_all": all(m.data.uv_layers for m in meshes),
    }


def main():
    argv = sys.argv[sys.argv.index("--") + 1:]
    p = argparse.ArgumentParser()
    for key in ("input", "output", "family", "report"):
        p.add_argument("--" + key, required=True)
    a = p.parse_args(argv)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    before = metrics()
    if not (49500 <= before["triangles"] <= 50000 and before["uv_all"] and before["materials"]):
        raise RuntimeError("Certified source failed geometry/material/UV audit: " + str(before))
    if a.family not in SOCKETS:
        raise RuntimeError("Unrecognized certified family: " + a.family)
    for name, (x, y_up, z_forward) in SOCKETS[a.family].items():
        # glTF uses Y-up and Blender Z-up; +Z glTF becomes -Y Blender.
        empty = bpy.data.objects.new("Socket_" + name, None)
        bpy.context.scene.collection.objects.link(empty)
        empty.location = Vector((x, -z_forward, y_up))
        empty.empty_display_size = 0.018
        empty["eldoria_interface_v1"] = name
        empty["glb_xyz"] = [x, y_up, z_forward]
    after = metrics()
    if before != after:
        raise RuntimeError("Interface metadata unexpectedly changed mesh: " + str((before, after)))
    os.makedirs(os.path.dirname(a.output), exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=a.output, export_format="GLB", export_apply=True,
                              export_materials="EXPORT", export_yup=True, export_extras=True)
    report = {"family": a.family, "before": before, "after": after,
              "sockets_source_xyz": SOCKETS[a.family], "geometry_edited": False,
              "input_bytes": os.path.getsize(a.input), "output_bytes": os.path.getsize(a.output)}
    with open(a.report, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2)
    print("ELDORIA_INTERFACE_REPORT=" + json.dumps(report))


if __name__ == "__main__":
    main()
