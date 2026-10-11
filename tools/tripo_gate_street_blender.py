import argparse, json, os, sys
import bpy

TARGET = 49800
MIN_TRIS = 49500
MAX_TRIS = 50000

def parse_args():
    argv = sys.argv
    argv = argv[argv.index("--")+1:] if "--" in argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--report", required=True)
    return p.parse_args(argv)

def mesh_objects():
    return [o for o in bpy.context.scene.objects if o.type == 'MESH' and o.data is not None]

def tri_count(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)

def metrics(label):
    objs = mesh_objects()
    mats = set()
    verts = 0
    tris = 0
    uv_ok = True
    normals_ok = True
    for o in objs:
        verts += len(o.data.vertices)
        tris += tri_count(o)
        uv_ok = uv_ok and bool(o.data.uv_layers)
        normals_ok = normals_ok and len(o.data.polygons) > 0
        for m in o.data.materials:
            if m: mats.add(m.name)
    return {
        "label": label,
        "objects": len(objs),
        "vertices": verts,
        "triangles": tris,
        "materials": len(mats),
        "images": len([i for i in bpy.data.images if i.source == "FILE"]),
        "bounds": [round(max((o.matrix_world @ v.co)[axis] for o in objs for v in o.data.vertices) - min((o.matrix_world @ v.co)[axis] for o in objs for v in o.data.vertices), 6) for axis in range(3)] if objs else [],
        "uv_present_all_meshes": uv_ok,
        "normals_present": normals_ok,
    }

def ensure_materials_and_uvs():
    neutral = bpy.data.materials.get("Eldoria_Module_Neutral")
    if neutral is None:
        neutral = bpy.data.materials.new("Eldoria_Module_Neutral")
        neutral.diffuse_color = (0.46, 0.44, 0.40, 1.0)
    for o in mesh_objects():
        if not o.data.materials:
            o.data.materials.append(neutral)
        if not o.data.uv_layers:
            bpy.context.view_layer.objects.active = o
            o.select_set(True)
            bpy.ops.object.mode_set(mode='EDIT')
            bpy.ops.mesh.select_all(action='SELECT')
            bpy.ops.uv.smart_project(angle_limit=1.15192, island_margin=0.02)
            bpy.ops.object.mode_set(mode='OBJECT')
            o.select_set(False)

def apply_decimation_once(target):
    objs = mesh_objects()
    current = sum(tri_count(o) for o in objs)
    if current <= target:
        return current
    ratio = max(0.001, min(1.0, target / float(current)))
    for o in objs:
        before = tri_count(o)
        if before <= 20:
            continue
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        mod = o.modifiers.new(name="Eldoria_AutoDecimate", type='DECIMATE')
        mod.decimate_type = 'COLLAPSE'
        mod.ratio = ratio
        mod.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=mod.name)
        o.select_set(False)
    return sum(tri_count(o) for o in mesh_objects())

def main():
    a = parse_args()
    if not os.path.isfile(a.input):
        raise FileNotFoundError(a.input)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    raw = metrics("raw_import")
    if raw["triangles"] < MAX_TRIS:
        raise RuntimeError(f"Raw source unexpectedly below gate ceiling: {raw['triangles']} tris")
    ensure_materials_and_uvs()
    for _ in range(3):
        now = apply_decimation_once(TARGET)
        if MIN_TRIS <= now <= MAX_TRIS:
            break
        if now < MIN_TRIS:
            raise RuntimeError(f"Decimation undershot tolerance: {now} tris")
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    optimized = metrics("optimized")
    if not (MIN_TRIS <= optimized["triangles"] <= MAX_TRIS):
        raise RuntimeError(f"Optimized triangle gate failed: {optimized['triangles']} not in {MIN_TRIS}-{MAX_TRIS}")
    if not optimized["uv_present_all_meshes"]:
        raise RuntimeError("Optimized GLB is missing UV0 on at least one mesh")
    os.makedirs(os.path.dirname(a.output), exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=a.output,
        export_format='GLB',
        export_apply=True,
        export_materials='EXPORT',
        export_yup=True,
    )
    report = {
        "target_triangles": TARGET,
        "accepted_range": [MIN_TRIS, MAX_TRIS],
        "raw": raw,
        "optimized": optimized,
        "input_bytes": os.path.getsize(a.input),
        "output_bytes": os.path.getsize(a.output),
    }
    os.makedirs(os.path.dirname(a.report), exist_ok=True)
    with open(a.report, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2)
    print("ELDORIA_GATE_STREET_REPORT=" + json.dumps(report))

if __name__ == "__main__":
    main()
