"""Actual export roundtrip captures, including strategic LOD2 inspection.

Imports the files that will be delivered, never a substitute rendering mesh.
The report records technical evidence; artistic approval remains independent.
"""
import bpy, hashlib, json, os
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / 'art-source/valoria/production/asset-family-v2/quality-v3'
PRESETS = {'watchtower': (13.3, 3.5), 'wall_straight': (11.7, 2.5),
           'wall_corner': (11.7, 2.5), 'wall_connections': (8, 2.5),
           'stone_path_platform': (12.4, .3), 'cliff_rocks': (18, -1.4),
           'defensive_props': (10.4, 2.5)}
reports = []
for asset, (scale, target_z) in PRESETS.items():
    for level in (0, 2):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        path = BASE / asset / (asset + (f'-LOD{level}' if level else '') + '.glb')
        bpy.ops.import_scene.gltf(filepath=str(path))
        scene = bpy.context.scene
        meshes = [o for o in scene.objects if o.type == 'MESH']
        assert meshes and all(len(o.data.vertices) for o in meshes)
        scene.render.engine = 'CYCLES'
        scene.cycles.samples = int(os.environ.get('ELDORIA_FAMILY_SAMPLES', '32'))
        scene.cycles.use_denoising = True
        scene.render.resolution_x = 1100
        scene.render.resolution_y = 850
        scene.render.resolution_percentage = 100
        scene.render.image_settings.file_format = 'PNG'
        scene.view_settings.view_transform = 'AgX'
        scene.view_settings.look = 'None'
        world = bpy.data.worlds.new('Matched V3 environment')
        world.use_nodes = True
        world.node_tree.nodes['Background'].inputs['Color'].default_value = (.17, .22, .31, 1)
        world.node_tree.nodes['Background'].inputs['Strength'].default_value = .5
        scene.world = world
        for name, pos, energy, color, size in [
            ('Sunset key', (-7, -10, 14), 1800, (1, .77, .53), 7),
            ('Sky fill', (6, -1, 11), 1150, (.49, .64, 1), 6),
            ('Warm rim', (-4, 7, 10), 1700, (1, .71, .48), 6)]:
            light = bpy.data.lights.new(name, 'AREA')
            ob = bpy.data.objects.new(name, light)
            scene.collection.objects.link(ob)
            ob.location = pos
            ob.rotation_euler = (Vector((0, 0, 2)) - ob.location).to_track_quat('-Z', 'Y').to_euler()
            light.energy, light.color, light.size = energy, color, size
            light.shape = 'DISK'
        cam = bpy.data.cameras.new('Actual GLB review')
        ob = bpy.data.objects.new('Actual GLB review', cam)
        scene.collection.objects.link(ob)
        scene.camera = ob
        target = Vector((0, 0, target_z))
        ob.location = target + Vector((13, -19, 13) if level == 0 else (16, -23, 18))
        ob.rotation_euler = (target - ob.location).to_track_quat('-Z', 'Y').to_euler()
        cam.type = 'ORTHO'
        cam.ortho_scale = scale * (1 if level == 0 else 1.35)
        output = BASE / asset / ('roundtrip-hero.png' if level == 0 else 'roundtrip-LOD2-strategic.png')
        scene.render.filepath = str(output)
        bpy.ops.render.render(write_still=True)
        bpy.context.view_layer.update()
        points = [o.matrix_world @ Vector(c) for o in meshes for c in o.bound_box]
        reports.append({'asset': asset, 'lod': level, 'file_sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                        'meshes': len(meshes), 'triangles': sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons),
                        'bounds_m': [[min(v[k] for v in points), max(v[k] for v in points)] for k in range(3)],
                        'capture': str(output.relative_to(BASE))})
(BASE / 'roundtrip-evidence.json').write_text(json.dumps({'exports': reports, 'art_pass': False,
                                                         'unity_pass': False}, indent=2))
