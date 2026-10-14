"""Export the real, normal-refreshed terrain only; preserve its geometry and materials."""
import argparse, hashlib, json, pathlib, sys
import bpy
parser = argparse.ArgumentParser()
parser.add_argument('--source', type=pathlib.Path, required=True)
parser.add_argument('--out', type=pathlib.Path, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
args.out.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(args.source.resolve()))
terrain = bpy.data.objects['ZoneOne continuous prairie source']
positions = [tuple(v.co) for v in terrain.data.vertices]
bpy.ops.object.select_all(action='DESELECT')
terrain.select_set(True); bpy.context.view_layer.objects.active = terrain
model = args.out / 'ZoneOnePrairieCandidate.glb'
bpy.ops.export_scene.gltf(filepath=str(model.resolve()), export_format='GLB', use_selection=True)
assert positions == [tuple(v.co) for v in terrain.data.vertices]
mesh = terrain.data
mesh.calc_loop_triangles()
areas = [(p.area, abs(p.normal.z)) for p in mesh.polygons]
import math
area = sum(a for a, z in areas)
proof = {'source_blend_sha256': hashlib.sha256(args.source.read_bytes()).hexdigest(),
    'model_sha256': hashlib.sha256(model.read_bytes()).hexdigest(), 'bytes': model.stat().st_size,
    'vertices': len(mesh.vertices), 'triangles': len(mesh.loop_triangles), 'mesh_count': 1,
    'geometry_unchanged_by_export': True, 'uv_layers': [uv.name for uv in mesh.uv_layers],
    'under_10_degrees_area_fraction': sum(a for a,z in areas if z > math.cos(math.radians(10))) / area,
    'under_20_degrees_area_fraction': sum(a for a,z in areas if z > math.cos(math.radians(20))) / area,
    'image_type': 'BLENDER_SOURCE_NOT_UNITY', 'art_approved': False,
    'navigation_certified': False, 'physical_device_performance_measured': False}
(args.out / 'ground-family-export.json').write_text(json.dumps(proof, indent=2) + '\n')
print(json.dumps(proof))
