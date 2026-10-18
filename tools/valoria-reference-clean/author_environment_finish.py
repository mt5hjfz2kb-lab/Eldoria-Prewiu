"""Subordinate scene-finishing tools for the existing Eldoria local artist.

Run with the already installed Blender on the reserved runner, from the repo root.
The source GLBs are immutable. Qwen sees the real integrated-player screenshots
before directing a bounded quarry shape and a forest assembled from the retained
Valoria pine. The reviewed BlenderMCP executor is reused, not reimplemented.
"""
import ast, base64, bpy, bmesh, hashlib, json, math, os, pathlib, random
import subprocess, sys, time, urllib.request
from mathutils import Vector, Matrix
from math import sin, cos, pi

ROOT = pathlib.Path.cwd()
EV = ROOT / 'docs/evidence/valoria-reference-clean'
SRC = ROOT / 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
OUT = SRC / 'EnvironmentFinish'
OUT.mkdir(parents=True, exist_ok=True)
EV.mkdir(parents=True, exist_ok=True)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def write(name, value):
    (EV / name).write_text(json.dumps(value, indent=2), encoding='utf8')

sources = {
    'world': ROOT / 'Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb',
    'pine': SRC / 'PineCanopy77.glb',
}
for source in sources.values():
    assert source.is_file(), 'Retained source missing: ' + str(source)
source_hashes = {key: digest(path) for key, path in sources.items()}
images = [EV / 'integration-world-16x9.png', EV / 'integration-city-art.png']
for path in images:
    assert path.is_file(), 'Actual native111 player screenshot required: ' + str(path)

# Import into a disposable Blender document. No original .blend is overwritten.
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(sources['world']))
world_objects = list(bpy.context.scene.objects)
forest_anchor = bpy.data.objects.get('Wood resource existing timber stock')
quarry_anchor = bpy.data.objects.get('Existing Holotna rock.001')
assert forest_anchor and quarry_anchor

def children_meshes(root):
    return [o for o in (root, *root.children_recursive) if o.type == 'MESH']

def bounds(objects):
    points = [o.matrix_world @ v.co for o in objects for v in o.data.vertices]
    assert points
    return (Vector([min(p[k] for p in points) for k in range(3)]),
            Vector([max(p[k] for p in points) for k in range(3)]))

def normalized_copy(objects, name, width=None):
    lo, hi = bounds(objects)
    origin = Vector(((lo.x + hi.x) / 2, (lo.y + hi.y) / 2, lo.z))
    scale = width / max(hi.x - lo.x, hi.y - lo.y) if width else 1
    out = []
    for source in objects:
        obj = bpy.data.objects.new(name + ' / ' + source.name, source.data.copy())
        bpy.context.collection.objects.link(obj)
        matrix = Matrix.Diagonal((scale, scale, scale, 1)) @ Matrix.Translation(-origin) @ source.matrix_world
        obj.data.transform(matrix)
        out.append(obj)
    return out

before_forest = normalized_copy(children_meshes(forest_anchor), 'Before forest', 14)
before_quarry = normalized_copy(children_meshes(quarry_anchor), 'Before quarry', 17)
rock_material = children_meshes(quarry_anchor)[0].data.materials[0]
for obj in world_objects:
    obj.hide_render = True

# Recover one of the three already authored volumetric pines, including its
# retained trunk and UVs. This is an extraction from the exact source, not a new
# conifer generator. Vertex positions are read before the artist is consulted.
known = set(bpy.context.scene.objects)
bpy.ops.import_scene.gltf(filepath=str(sources['pine']))
pine_objects = [o for o in bpy.context.scene.objects if o not in known]
pine_mesh = next(o for o in pine_objects if o.type == 'MESH')
world_vertices = [pine_mesh.matrix_world @ v.co for v in pine_mesh.data.vertices]
target_xy = Vector((32, 59))
faces = []
for polygon in pine_mesh.data.polygons:
    centre = sum((world_vertices[i] for i in polygon.vertices), Vector()) / len(polygon.vertices)
    if (centre.xy - target_xy).length < 3.6:
        faces.append(polygon)
assert len(faces) > 300, 'Retained volumetric pine extraction is empty'
used = sorted({i for f in faces for i in f.vertices})
remap = {i: j for j, i in enumerate(used)}
lo = Vector([min(world_vertices[i][k] for i in used) for k in range(3)])
hi = Vector([max(world_vertices[i][k] for i in used) for k in range(3)])
assert 5 < hi.z - lo.z < 22 and hi.x - lo.x < 8 and hi.y - lo.y < 8
origin = Vector(((lo.x + hi.x) / 2, (lo.y + hi.y) / 2, lo.z))
tree_data = bpy.data.meshes.new('Retained authored Valoria pine')
tree_data.from_pydata([tuple(world_vertices[i] - origin) for i in used], [],
                     [[remap[i] for i in f.vertices] for f in faces])
for material in pine_mesh.data.materials:
    tree_data.materials.append(material)
for source, output in zip(faces, tree_data.polygons):
    output.material_index = source.material_index
    output.use_smooth = source.use_smooth
for source_uv in pine_mesh.data.uv_layers:
    output_uv = tree_data.uv_layers.new(name=source_uv.name)
    for source_face, output_face in zip(faces, tree_data.polygons):
        for old_loop, new_loop in zip(source_face.loop_indices, output_face.loop_indices):
            output_uv.data[new_loop].uv = source_uv.data[old_loop].uv
for obj in pine_objects:
    obj.hide_render = True
bark = next((m for m in tree_data.materials if m and 'bark' in m.name.lower()), rock_material)

memory_path = EV / ('environment-finish-memory-' + os.environ.get('GITHUB_RUN_ID', 'local') + '.txt')
subprocess.run(['node', 'tools/art-direction/learning-evidence/cycle-hooks.mjs', 'prepare',
                '--target', 'bastion1', '--out', str(memory_path)], check=True, cwd=ROOT)
memory = memory_path.read_text(encoding='utf8')[:9600]
context = {
    'source_sha256': source_hashes,
    'retained_pine_bounds': [list(lo), list(hi)],
    'retained_pine_faces': len(faces),
    'actual_memory': memory,
    'actual_memory_sha256': hashlib.sha256(memory.encode()).hexdigest(),
    'brief': (
        'Continue the current integrated Valoria. Image 1 is the actual world player view; '
        'image 2 is the actual current city. The wood destination currently reads as giant '
        'loose planks and the quarry as isolated boulders. Author two recognizable, compact '
        'destinations. FOREST: arrange 8-11 copies of the retained volumetric city pine around '
        'a clear near-side logging entrance, with two stacks of cut logs; choose every tree '
        'position/scale as a spatial composition, not a grid or ring. QUARRY: design an open '
        'working limestone escarpment with 2-3 excavation benches, irregular broken cap, '
        'exposed extraction faces, a small wooden lifting derrick and cut-stone stock away '
        'from the entry. The seven rim control points run left-to-right; y rises to the rear '
        'in the middle and returns towards the front at the ends. A small route in front '
        'must stay open. No podium, rectangular parcel, floor slab, castle or modern machine. '
        'Bastion is not redesigned. Also choose worn-earth access width in the city and '
        'three stretches of the repetitive river bank where existing stones should be '
        'reused at a lower, embedded scale; exclude the bridge corridor X -18 to +18 '
        'and distribute the stretches on both sides. World and city must retain the actual earthy '
        'limestone, timber and olive-foliage family. Blender X/Y horizontal, Z up. '
        'Explain three visible defects and the specific composition that fixes them. '
        'Do not score or approve your own work.'),
}

def number(lo, hi):
    return {'type': 'number', 'minimum': lo, 'maximum': hi}

def record(fields):
    return {'type': 'object', 'additionalProperties': False,
            'required': list(fields), 'properties': fields}

fields = {
    'rationale': {'type': 'string'},
    'forest_trees': {'type': 'array', 'minItems': 8, 'maxItems': 11,
                     'items': record({'x': number(-10, 10), 'y': number(0, 10), 'scale': number(.60, .90)})},
    'log_yard_side': {'type': 'string', 'enum': ['left', 'right']},
    'quarry_width': number(15, 20), 'quarry_depth': number(10, 14),
    'quarry_height': number(4.5, 6.5),
    'quarry_benches': {'type': 'integer', 'minimum': 2, 'maximum': 3},
    'quarry_rim': {'type': 'array', 'minItems': 7, 'maxItems': 7,
                   'items': record({'x': number(-1, 1), 'y': number(0, 1), 'height': number(.65, 1)})},
    'derrick_side': {'type': 'string', 'enum': ['left', 'right']},
    'city_path_width': number(1.5, 2.2),
    'shore_gaps': {'type': 'array', 'minItems': 3, 'maxItems': 3,
                   'items': record({'x': number(-62, 62), 'width': number(5, 9), 'scale': number(.35, .60)})},
}
schema = record(fields)
payload = dict(model='qwen3-vl:8b-instruct-q4_K_M', stream=False, format=schema,
               think=False, keep_alive=0,
               messages=[{'role': 'system', 'content': 'You are the existing Eldoria local 3D artist. Direct concrete spatial authoring of the retained game assets. JSON only. No new tool system, no paid provider, no approval.'},
                         {'role': 'user', 'content': json.dumps(context),
                          'images': [base64.b64encode(p.read_bytes()).decode() for p in images]}],
               options={'temperature': .25, 'num_ctx': 8192, 'num_predict': 2700})
write('environment-finish-qwen-request.json', {'model': payload['model'], 'schema': schema,
      'context': context, 'image_sha256': {p.name: digest(p) for p in images}})

def api(path, data=None, timeout=3):
    req = urllib.request.Request('http://127.0.0.1:11434' + path,
          data=None if data is None else json.dumps(data).encode(),
          headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=timeout) as response:
        return json.load(response)

server = None
log = None
try:
    try:
        api('/api/version')
    except Exception:
        # The same installation recovery used by author_sawmill_refined.py.
        users = pathlib.Path(os.environ.get('SystemDrive', 'C:') + '/Users')
        installs = list(users.glob('*/AppData/Local/Programs/Ollama/ollama.exe'))
        assert installs, 'Existing Ollama installation not found'
        env = os.environ.copy()
        env['OLLAMA_HOST'] = '127.0.0.1:11434'
        models = list(users.glob('*/.ollama/models'))
        if models:
            env['OLLAMA_MODELS'] = str(models[0])
        log = open(EV / 'environment-finish-ollama.log', 'w')
        server = subprocess.Popen([str(installs[0]), 'serve'], env=env, stdout=log, stderr=subprocess.STDOUT)
        for _ in range(40):
            try:
                api('/api/version')
                break
            except Exception:
                time.sleep(1)
        else:
            raise RuntimeError('Existing Ollama did not start')
    assert any(m['name'] == payload['model'] for m in api('/api/tags')['models'])
    response = api('/api/chat', payload, 420)
    write('environment-finish-qwen-response.json', response)
    plan = json.loads(response['message']['content'])
    assert set(plan) == set(fields)
    assert 8 <= len(plan['forest_trees']) <= 11 and len(plan['quarry_rim']) == 7
    assert 15 <= plan['quarry_width'] <= 20 and 10 <= plan['quarry_depth'] <= 14
    assert 4.5 <= plan['quarry_height'] <= 6.5 and plan['quarry_benches'] in (2, 3)
    assert 1.5 <= plan['city_path_width'] <= 2.2
    assert plan['log_yard_side'] in ('left', 'right') and plan['derrick_side'] in ('left', 'right')
    for point in plan['forest_trees']:
        assert -10 <= point['x'] <= 10 and 0 <= point['y'] <= 10 and .60 <= point['scale'] <= .90
    for point in plan['quarry_rim']:
        assert -1 <= point['x'] <= 1 and 0 <= point['y'] <= 1 and .65 <= point['height'] <= 1
    assert len(plan['shore_gaps']) == 3
    for gap in plan['shore_gaps']:
        assert -62 <= gap['x'] <= 62 and 5 <= gap['width'] <= 9 and .35 <= gap['scale'] <= .60
    write('environment-finish-plan.json', plan)
finally:
    if server:
        subprocess.run(['taskkill.exe', '/PID', str(server.pid), '/T', '/F'], capture_output=True)
        server.wait(timeout=20)
    if log:
        log.close()

# Existing geometry primitives and transport; these remain subordinate tools to
# the newly recorded Qwen plan. No Python from the model is ever executed.
tree = ast.parse((ROOT / 'tools/valoria-reference-clean/author_scene.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n, (ast.FunctionDef, ast.ClassDef))
     and n.name in {'Mesh', 'line', 'execute_via_blendermcp'}], type_ignores=[]), '<retained Eldoria author tools>', 'exec'))
RejectedPlan = RuntimeError
stone_trial = False

def material(name, color, roughness=.9):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Roughness'].default_value = roughness
    return m

cut = material('Local exposed timber endgrain', (.34, .21, .105))
oak = material('Local weathered oak equipment', (.105, .066, .031))
iron = material('Local iron lifting tackle', (.055, .063, .067), .62)
stone = [material('Quarry fresh limestone ' + str(i), (.35 + .022*i, .30 + .022*i, .225 + .020*i)) for i in range(4)]
forest = []
quarry = []
operation = {'id': 'integrated-environment-finish', 'op': 'author_forest_and_quarry_from_spatial_plan', 'plan': plan}

def perform(action):
    assert action == operation and not forest and not quarry
    rng = random.Random(20261018)
    for i, p in enumerate(plan['forest_trees']):
        # Keep a visible three-metre logging entrance, even if the author puts a
        # tree into the protected approach. Reject rather than silently relocate.
        assert not (abs(p['x']) < 1.6 and p['y'] < -.5), 'Tree overlaps protected logging entrance'
        obj = bpy.data.objects.new('Forest retained Valoria pine %02d' % i, tree_data)
        bpy.context.collection.objects.link(obj)
        obj.location = (p['x'], p['y'], 0)
        obj.rotation_euler.z = (i * 2.39996) % (2*pi)
        obj.scale = (p['scale'],) * 3
        forest.append(obj)
    logs = Mesh('Forest cut timber and sawbuck')
    side = -1 if plan['log_yard_side'] == 'left' else 1
    for pile in range(2):
        for row, count in [(0, 3), (1, 2)]:
            for j in range(count):
                x = side * (3.4 + pile * 3.6)
                y = -4.8 + (j - 1 + row*.5)*.58
                z = .32 + row*.53
                line(logs, (x-1.45, y, z), (x+1.45, y, z), .30, bark)
                line(logs, (x-1.46, y, z), (x-1.44, y, z), .265, cut)
                line(logs, (x+1.44, y, z), (x+1.46, y, z), .265, cut)
    forest.append(logs.finish(.005))

    escarpment = Mesh('Quarry cut faces and broken benches')
    profile = sorted(plan['quarry_rim'], key=lambda p: p['x'])
    assert profile[-1]['x'] - profile[0]['x'] > 1.4, 'Quarry rim does not span the working face'
    w, d, h = plan['quarry_width'], plan['quarry_depth'], plan['quarry_height']
    tiers = plan['quarry_benches']
    for index, (a, b) in enumerate(zip(profile, profile[1:])):
        ax, ay, bx, by = a['x']*w*.5, a['y']*d*.6, b['x']*w*.5, b['y']*d*.6
        tangent = Vector((bx-ax, by-ay, 0)).normalized()
        normal = Vector((-tangent.y, tangent.x, 0))
        for tier in range(tiers):
            retreat = tier*.64
            z0a = -.12 if tier == 0 else h*tier/tiers*a['height']-.04
            z0b = -.12 if tier == 0 else h*tier/tiers*b['height']-.04
            za, zb = h*(tier+1)/tiers*a['height'], h*(tier+1)/tiers*b['height']
            front_a = Vector((ax, ay, 0)) + normal*retreat
            front_b = Vector((bx, by, 0)) + normal*retreat
            back_a = front_a + normal*(2.3-tier*.4)
            back_b = front_b + normal*(2.3-tier*.4)
            vertices = [(front_a.x, front_a.y, z0a), (front_b.x, front_b.y, z0b),
                        (front_b.x, front_b.y, zb), (front_a.x, front_a.y, za),
                        (back_a.x, back_a.y, z0a), (back_b.x, back_b.y, z0b),
                        (back_b.x, back_b.y, zb+.10), (back_a.x, back_a.y, za+.10)]
            escarpment.add(vertices, [(0,1,2,3),(4,7,6,5),(3,2,6,7),(0,4,5,1),(0,3,7,4),(1,5,6,2)], stone[(index+tier)%4])
    # The upper cap retains the already licensed natural rock surface material.
    for i, p in enumerate(profile):
        escarpment.box((p['x']*w*.5, p['y']*d*.6+1.8, h*p['height']+.10),
                       (1.1+rng.random()*.7, .85+rng.random()*.6, .52), rock_material,
                       wear=.12)
    quarry.append(escarpment.finish(.055))
    stock = Mesh('Quarry extracted limestone stock')
    side = -1 if plan['derrick_side'] == 'left' else 1
    for row in range(2):
        for col in range(3-row):
            stock.box((-side*(3.8+col*.95), -2.8+row*.85, .43), (.83,.68,.82), stone[(row+col)%4], wear=.045)
    quarry.append(stock.finish(.045))
    tackle = Mesh('Quarry timber derrick and hanging stone')
    x, y = side*w*.34, -.8
    line(tackle, (x,y,0), (x,y,4.8), .19, oak)
    line(tackle, (x-1.3,y-.65,0), (x,y,3.1), .12, oak)
    line(tackle, (x+1.3,y+.65,0), (x,y,3.1), .12, oak)
    line(tackle, (x-side*2.8,y,4.25), (x+side*.85,y,4.25), .16, oak)
    line(tackle, (x,y,4.7), (x-side*2.8,y,4.25), .04, iron)
    line(tackle, (x-side*2.8,y,4.25), (x-side*2.8,y,1.2), .027, iron)
    tackle.box((x-side*2.8,y,.8), (.85,.76,.75), stone[1], wear=.045)
    quarry.append(tackle.finish(.012))

transport = 'BLENDER_DIRECT_EXISTING_TYPED_EXECUTOR'
try:
    import addon_utils
    addon = next((m for m in addon_utils.modules() if m.__name__ == 'blender_mcp'), None)
    addon_path = pathlib.Path(addon.__file__) if addon else None
    pinned = 'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
    if addon_path and digest(addon_path) == pinned:
        execute_via_blendermcp([operation], perform, addon_path, pinned)
        transport = 'ACTUAL_BLENDERMCP_TYPED_ACTION'
    else:
        perform(operation)
except Exception as exc:
    if forest or quarry:
        raise
    write('environment-finish-mcp-fallback.json', {'reason': str(exc), 'source_preserved': True,
          'fallback': 'same validated local typed action in the existing Blender process'})
    perform(operation)

assert forest and quarry
for group in (forest, quarry, before_forest, before_quarry):
    for obj in group:
        obj.hide_render = True

# Equal-camera, equal-light Blender comparisons are a lightweight review. The
# integrated editor pass separately records the actual Unity A/B images.
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 20
scene.render.resolution_x, scene.render.resolution_y = 1100, 825
scene.render.resolution_percentage = 100
scene.world = bpy.data.worlds.new('Environment comparison sky')
scene.world.use_nodes = True
bg = scene.world.node_tree.nodes.get('Background')
bg.inputs[0].default_value = (.33,.38,.43,1)
bg.inputs[1].default_value = .7
bpy.ops.mesh.primitive_plane_add(size=90, location=(0,0,-.18))
ground = bpy.context.object
ground.name = 'Review floor (excluded from game assets)'
ground.data.materials.append(material('Review olive ground', (.18,.22,.105)))
bpy.ops.object.light_add(type='AREA', location=(-14,-18,27))
light = bpy.context.object
light.data.energy, light.data.size = 8500, 10
light.rotation_euler = (Vector((0,0,0))-light.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(25,-33,29))
camera = bpy.context.object
camera.data.type, camera.data.ortho_scale = 'ORTHO', 33
camera.rotation_euler = (Vector((0,2,2.3))-camera.location).to_track_quat('-Z','Y').to_euler()
scene.camera = camera
for name, group in [('forest-before',before_forest),('forest-after',forest),('quarry-before',before_quarry),('quarry-after',quarry)]:
    for obj in group:
        obj.hide_render = False
    scene.render.filepath = str(EV / ('environment-' + name + '.png'))
    bpy.ops.render.render(write_still=True)
    for obj in group:
        obj.hide_render = True

for filename, objects in [('EnvironmentForest.glb', forest), ('EnvironmentQuarry.glb', quarry)]:
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.hide_render = False
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.gltf(filepath=str(OUT / filename), export_format='GLB', use_selection=True,
          export_apply=True, export_yup=True, export_texcoords=True, export_normals=True, export_materials='EXPORT')
    for obj in objects:
        obj.hide_render = True
# Delete only the imported, unused copies from the disposable review document.
# Editable output source and baseline comparison copies remain present.
for obj in world_objects + pine_objects:
    bpy.data.objects.remove(obj, do_unlink=True)
for obj in forest + quarry:
    obj.hide_render = False
bpy.context.preferences.filepaths.save_version = 0
bpy.ops.wm.save_as_mainfile(filepath=str(EV / 'EnvironmentFinish.blend'))
assert {key:digest(path) for key,path in sources.items()} == source_hashes
write('environment-finish-result.json', {
    'source_sha': os.environ.get('GITHUB_SHA'), 'source_sha256': source_hashes,
    'sources_preserved': True, 'qwen_model': payload['model'], 'transport': transport,
    'actual_memory_sha256': context['actual_memory_sha256'], 'plan': plan,
    'plan_sha256': digest(EV/'environment-finish-plan.json'),
    'glb_sha256': {p.name:digest(p) for p in OUT.glob('*.glb')},
    'blend_sha256': digest(EV/'EnvironmentFinish.blend'),
    'retained_pine_faces': len(faces),
    'triangles': sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in forest+quarry),
    'art_approved': False, 'unity_capture_pending': True,
})
print('VALORIA_ENVIRONMENT_AUTHOR_COMPLETE')
