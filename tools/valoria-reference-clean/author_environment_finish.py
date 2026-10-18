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
memory_full = memory_path.read_text(encoding='utf8')
# The generated memory is short today. If it grows, retain whole provenance and
# lesson lines within a fixed budget; never slice a sentence or expand 8k blindly.
memory_lines, memory_chars = [], 0
for line in memory_full.splitlines():
    if memory_chars + len(line) + 1 <= 1800:
        memory_lines.append(line)
        memory_chars += len(line) + 1
memory = '\n'.join(memory_lines)
system_path = ROOT / 'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md'
system_prompt = system_path.read_text(encoding='utf8')
system_hash = digest(system_path)
local_lo, local_hi = lo - origin, hi - origin
previous_failure = {
    'run_id': '39144291626', 'geometry_executed': False,
    'cause': 'The model reused source-world pine coordinates as destination-local coordinates. '
             'JSON schema alone did not communicate numeric bounds semantically.',
    'observed': {'forest_x': [30.5, 36.8], 'forest_y': [57.5, 63.0], 'forest_scale_max': 1.05,
                 'quarry_depth': 20, 'quarry_height': 10, 'quarry_rim_x': [29, 35],
                 'quarry_rim_y': [56, 62], 'city_path_width': 5,
                 'shore_width': 3, 'shore_scale': .7},
    'correction': 'Pass real mesh bounds already transformed to their local frame; '
                  'state all bounds in user text; validate every field and request one guided correction.'
}
context = {
    'source_sha256': source_hashes,
    'target': {'name': 'EnvironmentFinish', 'mode': 'NEW',
               'scope': 'Two new destination assemblies; preserve imported source meshes and Bastion. '
                        'The retained pine is already centred at local X/Y zero with its base at Z zero.'},
    'retained_pine_LOCAL_bounds': [list(local_lo), list(local_hi)],
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
    'coordinate_and_parameter_contract': (
        'This is a subordinate typed tool of the existing artist. Return ONE canonical '
        'planned_action: type construct_mesh, target EnvironmentFinish, whose parameters '
        'are the following bounded composition decisions. Target mode is NEW; explain why '
        'a quarry extraction face is needed and which existing pine/stone sources are reused. '
        'Never output source-world coordinates for a destination. Forest and quarry each '
        'have their OWN LOCAL origin (0,0,0), Z up, X right, Y away from the front entrance. '
        'ALL NUMERIC LIMITS ARE INCLUSIVE. forest_trees: choose 8-11 entries, '
        'each x -10..10 metres, y 0..10 metres, scale 0.60..0.90; use staggered canopy groups '
        'with a visible entrance in front of Y=0. log_yard_side: left or right. '
        'quarry_width: 15..20 metres; quarry_depth: 10..14 metres; quarry_height: 4.5..6.5 metres; '
        'quarry_benches: integer 2 or 3. quarry_rim: exactly seven LEFT-TO-RIGHT distinct '
        'control points in NORMALIZED coordinates, x -1..1, y 0..1, height 0.65..1; '
        'first-to-last x span must exceed 1.4. These are fractions of the quarry dimensions, '
        'not metres or coordinates from the city. The rear rim bows back in its middle. '
        'derrick_side: left or right. city_path_width: 1.5..2.2 metres. shore_gaps: '
        'exactly three stretches using CITY-WORLD X only, each x -62..-18 OR 18..62 metres, '
        'width 5..9 metres, scale 0.35..0.60; keep the WHOLE stretch outside bridge X -18..18, '
        'include stretches on BOTH sides, and avoid overlap. No asset is approved by this plan. '
        'Previous attempt 39144291626 was rejected before editing because it used city-world '
        'coordinates for the forest/rim and exceeded the explicit size bounds. Use these '
        'LOCAL frames and ranges exactly; choose a coherent original composition within them.'),
}

def number(lo, hi):
    return {'type': 'number', 'minimum': lo, 'maximum': hi}

def record(fields):
    return {'type': 'object', 'additionalProperties': False,
            'required': list(fields), 'properties': fields}

fields = {
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
text_field = {'type': 'string', 'minLength': 35}
schema = record({
    'observed_defect': text_field, 'design_intent': text_field,
    'asset_decision': {'type': 'string', 'enum': ['NEW']},
    'reason_for_new_geometry': text_field,
    'planned_actions': {'type': 'array', 'minItems': 1, 'maxItems': 1,
       'items': record({'id': {'type': 'string'},
                        'type': {'type': 'string', 'enum': ['construct_mesh']},
                        'target': {'type': 'string', 'enum': ['EnvironmentFinish']},
                        'rationale': text_field, 'parameters': record(fields)})},
    'expected_visual_difference': text_field, 'mobile_risks': text_field,
})
payload = dict(model='qwen3-vl:8b-instruct-q4_K_M', stream=False, format=schema,
               think=False, keep_alive=0,
               messages=[{'role': 'system', 'content': system_prompt},
                         {'role': 'user', 'content': json.dumps(context),
                          'images': [base64.b64encode(p.read_bytes()).decode() for p in images]}],
               options={'temperature': .15, 'num_ctx': 8192, 'num_predict': 2100})
request_manifest = {'model': payload['model'], 'schema': schema,
      'system_prompt_path': str(system_path.relative_to(ROOT)), 'system_prompt_sha256': system_hash,
      'system_prompt': system_prompt, 'options': payload['options'],
      'context': context, 'image_sha256': {p.name: digest(p) for p in images},
      'memory_source_sha256': digest(memory_path), 'memory_selected_chars': len(memory),
      'memory_omitted_lines': len(memory_full.splitlines()) - len(memory_lines),
      'mesh_observation_source_world_bounds': [list(lo), list(hi)],
      'previous_failure': previous_failure}

def schema_errors(value, spec, path='plan'):
    """Validate the actual answer; Ollama JSON grammar does not enforce ranges."""
    errors = []
    kind = spec['type']
    valid_type = {'object': isinstance(value, dict), 'array': isinstance(value, list),
                  'string': isinstance(value, str),
                  'number': isinstance(value, (int, float)) and not isinstance(value, bool),
                  'integer': isinstance(value, int) and not isinstance(value, bool)}[kind]
    if not valid_type:
        return [f'{path}: expected {kind}, got {value!r}']
    if kind == 'object':
        for key in spec.get('required', []):
            if key not in value:
                errors.append(f'{path}.{key}: missing')
        for key, item in value.items():
            if key not in spec['properties']:
                errors.append(f'{path}.{key}: unexpected field')
            else:
                errors += schema_errors(item, spec['properties'][key], path + '.' + key)
    elif kind == 'array':
        if not spec.get('minItems', 0) <= len(value) <= spec.get('maxItems', len(value)):
            errors.append(f'{path}: count {len(value)} outside {spec.get("minItems", 0)}..{spec.get("maxItems")}')
        for i, item in enumerate(value):
            errors += schema_errors(item, spec['items'], f'{path}[{i}]')
    elif kind in ('number', 'integer'):
        if not math.isfinite(value) or not spec.get('minimum', value) <= value <= spec.get('maximum', value):
            errors.append(f'{path}: {value} outside {spec.get("minimum")}..{spec.get("maximum")}')
    elif kind == 'string' and len(value) < spec.get('minLength', 0):
        errors.append(f'{path}: needs at least {spec["minLength"]} characters')
    if 'enum' in spec and value not in spec['enum']:
        errors.append(f'{path}: {value!r} must be one of {spec["enum"]}')
    return errors

def composition_errors(candidate):
    errors = schema_errors(candidate, schema)
    if errors:
        return errors
    p = candidate['planned_actions'][0]['parameters']
    rim = p['quarry_rim']
    if any(a['x'] >= b['x'] for a, b in zip(rim, rim[1:])):
        errors.append('quarry_rim must progress left-to-right with distinct x values')
    if rim[-1]['x'] - rim[0]['x'] <= 1.4:
        errors.append('quarry_rim first-to-last x span must exceed 1.4')
    gaps = sorted(p['shore_gaps'], key=lambda g: g['x'])
    if not (gaps[0]['x'] < -18 and gaps[-1]['x'] > 18):
        errors.append('shore_gaps need stretches on both banks outside X -18..18')
    for gap in gaps:
        if abs(gap['x']) - gap['width'] / 2 <= 18:
            errors.append(f'shore_gaps centre {gap["x"]} with width {gap["width"]} overlaps bridge corridor X -18..18')
    for a, b in zip(gaps, gaps[1:]):
        if b['x'] - a['x'] < (a['width'] + b['width']) / 2:
            errors.append(f'shore_gaps at {a["x"]} and {b["x"]} overlap')
    return errors

def fixed_parameters_hash(candidate):
    p = candidate['planned_actions'][0]['parameters']
    fixed = {key: value for key, value in p.items() if key != 'quarry_rim'}
    return hashlib.sha256(json.dumps(fixed, sort_keys=True, separators=(',', ':')).encode()).hexdigest()

# Resume the retained artist's last answer after its exact cause was reviewed.
# A full-plan retry corrected one shore coordinate but needlessly redesigned a
# valid quarry profile. Only that profile is now sent back to the same artist.
resume_path = ROOT / 'tools/valoria-reference-clean/environment-finish-resume-39145677870.json'
resume, inference_schema, author_context = None, schema, context
if resume_path.is_file():
    resume = json.loads(resume_path.read_text(encoding='utf8'))
    assert resume['source_sha256'] == source_hashes, 'Resume source changed'
    assert resume['canonical_system_sha256'] == system_hash, 'Resume canonical artist identity changed'
    assert resume['image_sha256'] == {p.name: digest(p) for p in images}, 'Resume observation changed'
    assert fixed_parameters_hash(resume['artist_plan']) == resume['parameters_except_rim_sha256']
    seed_errors = composition_errors(resume['artist_plan'])
    assert seed_errors and all(error.startswith('quarry_rim') for error in seed_errors), seed_errors
    inference_schema = json.loads(json.dumps(schema))
    inference_schema['properties']['planned_actions']['items']['properties']['parameters'] = record({'quarry_rim': fields['quarry_rim']})
    fixed = resume['artist_plan']['planned_actions'][0]['parameters']
    author_context = dict(context)
    author_context['coordinate_and_parameter_contract'] = (
        'TARGETED CONTINUATION OF YOUR OWN LAST ANSWER, not a new commission. '
        'All forest positions, log yard, quarry dimensions, equipment side, city access '
        'width and riverbank stretches are frozen and already valid. Correct ONLY quarry_rim. '
        'Return canonical JSON with exactly ONE planned_action: construct_mesh, target '
        'EnvironmentFinish (mode NEW), parameters containing ONLY quarry_rim. '
        'Give exactly seven distinct points in STRICT LEFT-TO-RIGHT ORDER. Local normalized '
        'x must stay in [-1,1], y in [0,1], height in [0.65,1]. The last x minus the first x '
        'MUST EXCEED 1.4. The middle of the rear contour bows back; ends return forward. '
        'These fractions drive the real cut-face mesh, not city-world coordinates. '
        'Your earlier valid profile below already matches the same retained dimensions; '
        'you may retain it if it serves this composition. Do not redesign any other parameter. '
        'Your last profile ended x=0.6 after x=1.0 and therefore failed order/span checks. '
        'Do not score or approve anything; actual Blender and Unity review follows.')
    author_context['fixed_quarry_dimensions'] = {key: fixed[key] for key in
        ('quarry_width', 'quarry_depth', 'quarry_height', 'quarry_benches', 'derrick_side')}
    author_context['your_rejected_rim'] = fixed['quarry_rim']
    author_context['your_earlier_valid_rim_same_dimensions'] = resume['previous_valid_rim']
    author_context['frozen_remaining_parameters_sha256'] = resume['parameters_except_rim_sha256']
    author_context['fixed_forest_composition'] = fixed['forest_trees']
    payload['format'] = inference_schema
    payload['messages'][1]['content'] = json.dumps(author_context)
    request_manifest.update(schema=inference_schema, context=author_context,
        resume_source=str(resume_path.relative_to(ROOT)), resume_sha256=digest(resume_path),
        resume_response_sha256=resume['response_sha256'],
        correction_classification=resume['classification'], editable_fields=['quarry_rim'],
        fixed_parameters_sha256=resume['parameters_except_rim_sha256'])
write('environment-finish-qwen-request.json', request_manifest)

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
    base_messages = payload['messages']
    attempts = []
    for attempt in range(1, 3):
        response = api('/api/chat', payload, 420)
        write(f'environment-finish-qwen-response-attempt-{attempt}.json', response)
        write('environment-finish-qwen-response.json', response)
        try:
            received_plan = json.loads(response['message']['content'])
            errors = schema_errors(received_plan, inference_schema)
            if not errors:
                if resume:
                    artist_plan = json.loads(json.dumps(resume['artist_plan']))
                    artist_plan['planned_actions'][0]['parameters']['quarry_rim'] = received_plan['planned_actions'][0]['parameters']['quarry_rim']
                    assert fixed_parameters_hash(artist_plan) == resume['parameters_except_rim_sha256'], 'Non-rim artist decision changed'
                else:
                    artist_plan = received_plan
                errors = composition_errors(artist_plan)
        except (ValueError, KeyError, TypeError) as error:
            errors = ['Invalid canonical JSON: ' + str(error)]
        if response.get('done_reason') != 'stop':
            errors.append('Model answer was not complete: ' + str(response.get('done_reason')))
        attempts.append({'attempt': attempt, 'errors': errors,
                         'prompt_eval_count': response.get('prompt_eval_count'),
                         'eval_count': response.get('eval_count'),
                         'response_sha256': digest(EV/f'environment-finish-qwen-response-attempt-{attempt}.json')})
        write('environment-finish-plan-validation.json', {'attempts': attempts,
             'canonical_system_sha256': system_hash, 'numeric_limits_unchanged': True,
             'geometry_executed': False})
        if not errors:
            break
        if attempt == 2:
            raise RuntimeError('Artist plan rejected before geometry after one guided correction: '
                               + '; '.join(errors[:12]))
        # A fresh bounded request retains both real images, the complete brief,
        # source hashes and selected history. Do not append the invalid answer
        # to an already full chat or replace artist decisions with clamped values.
        correction = ('VALIDATOR REJECTED YOUR PREVIOUS ANSWER BEFORE ANY EDIT. '
                      'Correct these exact errors and recheck every range and coordinate frame. '
                      'Return the complete canonical JSON, with concise prose.\n'
                      + '\n'.join(errors)[:1600])
        write('environment-finish-guided-correction.json', {'errors': errors,
              'correction_sent': correction, 'canonical_system_sha256': system_hash})
        corrected_context = dict(author_context, validator_correction=correction)
        payload['messages'] = [base_messages[0], dict(base_messages[1], content=json.dumps(corrected_context))]
    if resume:
        write('environment-finish-rim-correction.json', {
             'source_run': resume['run_id'], 'source_response_sha256': resume['response_sha256'],
             'correction_response_sha256': digest(EV/'environment-finish-qwen-response.json'),
             'classification': resume['classification'], 'edited_fields': ['quarry_rim'],
             'fixed_parameters_sha256_before': resume['parameters_except_rim_sha256'],
             'fixed_parameters_sha256_after': fixed_parameters_hash(artist_plan),
             'canonical_correction': received_plan, 'all_original_gates_passed': True})
    write('environment-finish-artist-plan.json', artist_plan)
    plan = dict(artist_plan['planned_actions'][0]['parameters'])
    plan['rationale'] = artist_plan['design_intent'] + ' ' + artist_plan['planned_actions'][0]['rationale']
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
    'canonical_system_sha256': system_hash, 'memory_source_sha256': digest(memory_path),
    'actual_memory_sha256': context['actual_memory_sha256'], 'plan': plan,
    'canonical_artist_plan_sha256': digest(EV/'environment-finish-artist-plan.json'),
    'plan_sha256': digest(EV/'environment-finish-plan.json'),
    'glb_sha256': {p.name:digest(p) for p in OUT.glob('*.glb')},
    'blend_sha256': digest(EV/'EnvironmentFinish.blend'),
    'retained_pine_faces': len(faces),
    'triangles': sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in forest+quarry),
    'art_approved': False, 'unity_capture_pending': True,
})
print('VALORIA_ENVIRONMENT_AUTHOR_COMPLETE')
