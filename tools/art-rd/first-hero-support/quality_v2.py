"""Surface reauthoring of the preserved V1 .blend; no replacement architecture.

Run after quality_surfaces.py with Blender 4.2.5. Both images render in the
same scene, with the same camera, lights, exposure, resolution and samples.
"""
import bpy, hashlib, json, math, random
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[3]
BASE = ROOT / 'art-source/valoria/production/first-hero-gate-v1'
OUT = BASE / 'quality-v2'
TEX = OUT / 'textures'
OUT.mkdir(exist_ok=True)
assert hashlib.sha256((BASE / 'ValoriaHeroGate.blend').read_bytes()).hexdigest() == 'd4dfb8b5f4c330aa8cd5631cedc10a56fcd89cf9f8968dd49b24460f76025e06'
seed = 2811
rng = random.Random(seed)
bpy.ops.wm.open_mainfile(filepath=str(BASE / 'ValoriaHeroGate.blend'))
scene = bpy.context.scene
asset = bpy.data.collections['VALORIA_HERO_GATE']
cam = scene.camera
cam.location = (13, -19, 13)
cam.rotation_euler = (Vector((0, 0, 3.5)) - cam.location).to_track_quat('-Z', 'Y').to_euler()
cam.data.ortho_scale = 15.5
scene.render.resolution_x, scene.render.resolution_y = 1280, 960
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = str(OUT / 'before-hero-matched.png')
bpy.ops.render.render(write_still=True)

removed_quoins = 0
for ob in list(asset.objects):
    if ob.name.startswith('Corner quoin'):
        bpy.data.objects.remove(ob, do_unlink=True)
        removed_quoins += 1

# The original upper crenel course is an independent uniform cube. Integrate
# it into its existing lower course, retaining the top silhouette and count.
caps = [o for o in asset.objects if o.name.startswith('Tower crenel cap')]
lowers = [o for o in asset.objects if o.name.startswith('Tower crenel lower course')]
for cap in caps:
    lower = min(lowers, key=lambda o: (o.location - cap.location).length)
    # Local cube vertex positions are already scaled in the editable source.
    for v in lower.data.vertices:
        if v.co.z > 0:
            v.co.z += .25
            v.co.x += rng.uniform(-.035, .025)
            v.co.y += rng.uniform(-.025, .025)
            v.co.z += rng.uniform(-.04, .022)
    bpy.data.objects.remove(cap, do_unlink=True)

# The continuous tower crown reads as a cast slab. Retain its height, section
# and silhouette, but resolve the visible face as five bonded coping stones.
split_crowns = 0
for crown in [o for o in asset.objects if o.name.startswith('Tower corbelled crown')]:
    width = crown.dimensions.x
    for i in range(5):
        ob = crown.copy()
        ob.data = crown.data.copy()
        asset.objects.link(ob)
        ob.name = 'Tower crown dressed coping'
        ob.location.x = crown.location.x - width / 2 + (i + .5) * width / 5
        for v in ob.data.vertices:
            v.co.x = math.copysign((width / 5 - .010) / 2, v.co.x)
            v.co.z += rng.uniform(-.006, .006)
        split_crowns += 1
    bpy.data.objects.remove(crown, do_unlink=True)

course_names = ('Tower front ashlar', 'Tower rear ashlar', 'Tower return ashlar',
                'Tower inner return ashlar', 'Portal jamb dressed ashlar',
                'Wall socket stone', 'Wall socket back stone')
count = 0
for ob in asset.objects:
    if ob.type != 'MESH':
        continue
    n = ob.name
    if n.startswith(course_names):
        # Coherent wall faces with slight hand-set recess and chipped arrises.
        # Each stone stays in its existing course, never becomes a new module.
        if n.startswith('Tower front') or n.startswith('Portal jamb'):
            ob.location.y += rng.uniform(-.012, .024)
        elif n.startswith('Tower rear'):
            ob.location.y -= rng.uniform(-.012, .024)
        for v in ob.data.vertices:
            v.co.x += rng.uniform(-.028, .028)
            v.co.z += rng.uniform(-.025, .025)
            v.co.y += rng.uniform(-.016, .016)
        count += 1
        bevel = next((m for m in ob.modifiers if m.type == 'BEVEL'), None)
        if bevel:
            bevel.width = rng.uniform(.023, .046)
            bevel.segments = 1
    elif n.startswith(('Tower return crenel', 'Portal modest merlon', 'Wall socket crenel')):
        for v in ob.data.vertices:
            if v.co.z > 0:
                v.co.z += rng.uniform(-.07, .015)
                v.co.x += rng.uniform(-.035, .035)
        bevel = next((m for m in ob.modifiers if m.type == 'BEVEL'), None)
        if bevel:
            bevel.width = .045
            bevel.segments = 1

# Replace packed bitmap inputs in the existing material graph. These six
# original, authored maps are image data and survive GLB export/Unity import.
import numpy as np
maps = {}
for kind in ('stone', 'oak'):
    for suffix in ('basecolor', 'normal', 'roughness'):
        image = bpy.data.images.load(str(TEX / (kind + '_' + suffix + '.png')), check_existing=True)
        if suffix != 'basecolor':
            image.colorspace_settings.name = 'Non-Color'
        image.pack()
        maps[kind, suffix] = image

material_count = 0
for mat in bpy.data.materials:
    if not mat.use_nodes:
        continue
    kind = 'stone' if ('Limestone' in mat.name or 'Foundation_Rock' in mat.name) else ('oak' if 'Weathered_Oak' in mat.name else None)
    if not kind:
        continue
    bs = mat.node_tree.nodes.get('Principled BSDF')
    for node in mat.node_tree.nodes:
        if node.type != 'TEX_IMAGE' or not node.image:
            continue
        name = node.image.name.lower()
        suffix = next((s for s in ('basecolor', 'normal', 'roughness') if s in name or (s == 'basecolor' and '_albedo' in name)), None)
        if not suffix:
            continue
        source = maps[kind, suffix]
        if suffix == 'basecolor' and kind == 'stone':
            # Keep V1's restrained per-stone tonal variation without a checkerboard.
            factor = 1.0
            if 'Limestone_' in mat.name:
                idx = int(mat.name.split('Limestone_')[-1])
                factor = [.92, .96, .985, 1., 1.015, 1.04][idx]
            elif 'Carved_' in mat.name:
                factor = 1.035
            clone = source.copy()
            clone.name = mat.name + '_quality_albedo'
            px = np.empty(len(source.pixels), dtype=np.float32)
            source.pixels.foreach_get(px)
            px = px.reshape(-1, 4)
            px[:, :3] *= factor
            clone.pixels.foreach_set(px.ravel())
            clone.pack()
            node.image = clone
        else:
            node.image = source
    if kind == 'stone':
        for node in mat.node_tree.nodes:
            if node.type == 'NORMAL_MAP':
                node.inputs['Strength'].default_value = .72
    elif kind == 'oak':
        for node in mat.node_tree.nodes:
            if node.type == 'NORMAL_MAP':
                node.inputs['Strength'].default_value = .60
    material_count += 1

# Cubes were authored with UVMap and fitted arch meshes with WorldScaleUV.
# Joining by material made the arch's missing UVMap collapse to one texel.
# Keep the cubes' visually approved unwrap and rename the arch's sole map.
for ob in asset.objects:
    if ob.type != 'MESH':
        continue
    layers = ob.data.uv_layers
    uv = layers.get('UVMap')
    if uv:
        other = layers.get('WorldScaleUV')
        if other:
            layers.remove(other)
    else:
        uv = layers.get('WorldScaleUV')
        if uv:
            uv.name = 'UVMap'
    if uv:
        layers.active = uv
        uv.active_render = True

scene.render.filepath = str(OUT / 'after-hero-matched.png')
bpy.ops.render.render(write_still=True)
cam.location = (16, -23, 18)
cam.rotation_euler = (Vector((0, 0, 4)) - cam.location).to_track_quat('-Z', 'Y').to_euler()
cam.data.ortho_scale = 24
scene.render.resolution_x, scene.render.resolution_y = 1280, 720
scene.render.filepath = str(OUT / 'after-strategic.png')
bpy.ops.render.render(write_still=True)
cam.location = (5, -12, 7)
cam.rotation_euler = (Vector((1.8, -.6, 3.7)) - cam.location).to_track_quat('-Z', 'Y').to_euler()
cam.data.ortho_scale = 7.1
scene.render.resolution_x, scene.render.resolution_y = 1000, 1000
scene.render.filepath = str(OUT / 'after-detail.png')
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / 'ValoriaHeroGate.blend'))

# GLB is a snapshot; do not join/triangulate the editable source.
objects = [o for o in asset.objects if o.type == 'MESH']
depsgraph = bpy.context.evaluated_depsgraph_get()
for ob in objects:
    if ob.modifiers:
        evaluated = ob.evaluated_get(depsgraph)
        ob.data = bpy.data.meshes.new_from_object(evaluated, preserve_all_data_layers=True, depsgraph=depsgraph)
        ob.modifiers.clear()
groups = {}
for ob in objects:
    groups.setdefault(ob.data.materials[0].name, []).append(ob)
for group in groups.values():
    if len(group) < 2:
        continue
    bpy.ops.object.select_all(action='DESELECT')
    for ob in group:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = group[0]
    bpy.ops.object.join()
objects = [o for o in asset.objects if o.type == 'MESH']
bpy.ops.object.select_all(action='DESELECT')
for ob in objects:
    ob.select_set(True)
path = OUT / 'ValoriaHeroGate.glb'
bpy.ops.export_scene.gltf(filepath=str(path), export_format='GLB', use_selection=True,
                          export_apply=True, export_texcoords=True, export_normals=True,
                          export_materials='EXPORT')
report = {'baseline_commit': '4d9ac9ebfb713898c37572604b2c03238afec4e5',
          'baseline_blend_sha256': hashlib.sha256((BASE / 'ValoriaHeroGate.blend').read_bytes()).hexdigest(),
          'reference_sha256': hashlib.sha256((ROOT / 'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg').read_bytes()).hexdigest(),
          'seed': seed, 'stone_meshes_reworked': count, 'superposed_quoins_removed': removed_quoins,
          'split_crenel_courses_integrated': len(caps), 'pbr_materials_reworked': material_count,
          'bonded_coping_stones': split_crowns,
          'glb_bytes': path.stat().st_size, 'blender_version': bpy.app.version_string,
          'unity_tested': False, 'art_acceptance': 'pending actual visual review'}
(OUT / 'quality-report.json').write_text(json.dumps(report, indent=2) + '\n')
print('ELDORIA_QUALITY_V2_EXPORTED', json.dumps(report))
