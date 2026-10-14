"""Real Cycles relief A/B. Only normal refresh and renderer differ from historical Workbench."""
import argparse, hashlib, json, pathlib, sys, struct
import bpy

parser = argparse.ArgumentParser()
parser.add_argument('--before', type=pathlib.Path, required=True)
parser.add_argument('--after', type=pathlib.Path, required=True)
parser.add_argument('--out', type=pathlib.Path, required=True)
args = parser.parse_args(sys.argv[sys.argv.index('--')+1:])
if args.out.exists() and any(args.out.iterdir()):
    raise ValueError('Fresh evidence directory required')
args.out.mkdir(parents=True, exist_ok=True)
proof = {'image_type': 'MATCHED_BLENDER_CYCLES_NOT_UNITY', 'art_approved': False}
camera = None
coordinates = []
other_meshes = None
for label, source in [('before', args.before), ('after', args.after)]:
    bpy.ops.wm.open_mainfile(filepath=str(source.resolve()))
    terrain = bpy.data.objects['ZoneOne continuous prairie source']
    other = {o.name: hashlib.sha256(b''.join(struct.pack('<fff', *v.co)
        for v in o.data.vertices)).hexdigest() for o in bpy.data.objects if o.type == 'MESH' and o != terrain}
    if other_meshes is None:
        other_meshes = other
    assert other == other_meshes, 'Another source asset changed'
    coordinates.append([tuple(v.co) for v in terrain.data.vertices])
    if terrain.data.has_custom_normals:
        terrain.data.normals_split_custom_set([(0., 0., 0.)] * len(terrain.data.loops))
        terrain.data.update()
    scene = bpy.context.scene
    matrix = [list(row) for row in scene.camera.matrix_world]
    if camera is None:
        camera = matrix
    assert matrix == camera, 'Do not recenter candidate'
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 32
    scene.cycles.use_denoising = False
    scene.render.resolution_x = 1000
    scene.render.resolution_y = 800
    scene.render.resolution_percentage = 100
    image = args.out / (label + '-relief-blender.png')
    scene.render.filepath = str(image.resolve())
    bpy.ops.render.render(write_still=True)
    proof[label] = {'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
                    'image_sha256': hashlib.sha256(image.read_bytes()).hexdigest()}
before, after = coordinates
assert len(before) == len(after)
changed = [i for i, (a, b) in enumerate(zip(before, after)) if a != b]
assert all(a[:2] == b[:2] for a, b in zip(before, after))
assert all(before[i] == after[i] for i, p in enumerate(before)
           if abs(p[0]) >= 119.99 or abs(p[1]) >= 119.99 or (-40 <= p[0] <= 4 and -52 <= p[1] <= -8))
proof.update(camera_matrix=camera, changed_vertices=len(changed),
             other_meshes_unchanged=True,
             protected_boundary_and_city_unchanged=True, camera_equivalent=True,
             render_engine='CYCLES', dimensions=[1000, 800])
(args.out / 'relief-comparison.json').write_text(json.dumps(proof, indent=2) + '\n')
