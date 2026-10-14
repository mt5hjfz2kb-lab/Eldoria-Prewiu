"""Scoped source-vision adapter: real Qwen relief revision through the existing typed studio API."""
import argparse, base64, hashlib, json, pathlib, sys, time, urllib.request
from PIL import Image
ROOT = pathlib.Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'tools/art-direction/ai-authoring'))
from artist_plan_validation import inspect_artist_plan
from author_brief import cpu_artist_schema

parser = argparse.ArgumentParser()
parser.add_argument('--work-dir', type=pathlib.Path, required=True)
parser.add_argument('--before-frame', type=pathlib.Path, required=True)
args = parser.parse_args()
c = args.work_dir
target = 'ZoneOne continuous prairie source'
topology = json.loads((c / 'context/mesh-context.json').read_text())
role = (ROOT / 'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md').read_bytes()
memory = (ROOT / 'docs/world4x-zone1/GROUND_ART_REVIEW_20261014.json').read_text()
images = []
inputs = []
for index, path in enumerate([ROOT / 'docs/world4x-zone1/evidence/local-cycle-20261014/references/local-reference.png', args.before_frame]):
    image = Image.open(path).convert('RGB'); image.thumbnail((384, 384))
    out = c / ('revision-vision-' + str(index) + '.jpg'); image.save(out, quality=88)
    images.append(base64.b64encode(out.read_bytes()).decode())
    inputs.append({'source': str(path), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                   'vision_sha256': hashlib.sha256(out.read_bytes()).hexdigest()})
schema = cpu_artist_schema(target, 'existing', 2)
center = {'type': 'array', 'minItems': 3, 'maxItems': 3, 'prefixItems': [
    {'type': 'number', 'minimum': -72, 'maximum': 72},
    {'type': 'number', 'minimum': 30, 'maximum': 82},
    {'type': 'number', 'minimum': topology['local_aabb']['min'][2], 'maximum': topology['local_aabb']['max'][2]}]}
delta = {'type': 'array', 'minItems': 3, 'maxItems': 3, 'prefixItems': [{'const': 0}, {'const': 0},
    {'anyOf': [{'type': 'number', 'minimum': -6, 'maximum': -1.5}, {'type': 'number', 'minimum': 2, 'maximum': 7}]}]}
knot = {'type': 'object', 'properties': {'center': center, 'delta': delta,
    'radius': {'type': 'number', 'minimum': 16, 'maximum': 32}},
    'required': ['center', 'delta', 'radius'], 'additionalProperties': False}
variants = []
for kind, parameters in [('sculpt_region', knot), ('sculpt_path', {'type': 'object', 'properties': {
    'knots': {'type': 'array', 'minItems': 3, 'maxItems': 4, 'items': knot}}, 'required': ['knots'], 'additionalProperties': False})]:
    variants.append({'type': 'object', 'properties': {'id': {'type': 'string'}, 'type': {'const': kind},
        'target': {'const': target}, 'rationale': {'type': 'string', 'minLength': 15, 'maxLength': 180},
        'parameters': parameters}, 'required': ['id', 'type', 'target', 'rationale', 'parameters'], 'additionalProperties': False})
schema['properties']['planned_actions']['items'] = {'oneOf': variants}
prompt = ('Zone One GEOGRAPHY SOURCE REVISION, not Valoria architecture. Two images: owner destination, actual current Blender ground-only candidate, NOT UNITY. '
    'The prior AI raised a straight long east-west strip at Y=30 and a round hill; the actual wide camera still reads as lawn. '
    'Preserve the new prairie material, ALL other meshes, city pad and sector boundary. Author connected asymmetric rolling relief and a curving shallow valley, '
    'with broad open traversable plains. Do not create an isolated circular mound, straight ridge, terraced platform, canyon, mountain, paths or violet paint. '
    'Choose your own 1..2 typed actions. At least ONE sculpt_path must have 3..4 non-collinear knots: bend a natural landform; never just a straight line. '
    'A VALLEY requires NEGATIVE Z displacement; positive displacement creates a ridge. Correct the prior geometry rather than piling decorations over it. '
    'Working mask: every center and every knot X=-72..72, Y=30..82; each radius 16..32 meters. These limits preserve city and all seams. '
    'Every delta must be [0,0,dz], with dz=-6..-1.5 for depressions OR +2..+7 for elevations. '
    'Keep buildable flat areas between forms. Think about the actual camera and broad gentle slopes, not number of moved vertices. '
    'Return original design and executable JSON. Do not claim approval or execution. ACTUAL TOPOLOGY: ' + json.dumps(topology)
    + '\nRETRIEVED ACTUAL EXPERIENCE, NOT TRAINING: ' + memory)
payload = {'model': 'qwen3-vl:2b-instruct', 'stream': True, 'think': False, 'format': schema,
    'options': {'num_ctx': 8192, 'num_predict': 1400, 'temperature': .35},
    'messages': [{'role': 'system', 'content': role.decode()}, {'role': 'user', 'content': prompt, 'images': images}]}
for attempt in range(2):
    (c / ('revision-request-' + str(attempt+1) + '.json')).write_text(json.dumps(payload, indent=2))
    chunks = []; done = False; started = time.monotonic(); last = started
    request = urllib.request.Request('http://127.0.0.1:11434/api/chat', data=json.dumps(payload).encode(), headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(request, timeout=540) as response, (c / ('revision-stream-' + str(attempt+1) + '.jsonl')).open('w') as log:
        for line in response:
            if not line.strip(): continue
            log.write(line.decode()); log.flush(); piece = json.loads(line)
            if piece.get('error'): raise RuntimeError(piece['error'])
            chunks.append(piece.get('message', {}).get('content', ''))
            if time.monotonic()-last > 30: print('REAL_REVISION_STREAM', len(''.join(chunks)), flush=True); last=time.monotonic()
            if piece.get('done'): done = True; break
    if not done: raise RuntimeError('Incomplete artist inference')
    raw = ''.join(chunks); (c / ('revision-response-' + str(attempt+1) + '.json')).write_text(raw)
    errors = []
    try:
        design = json.loads(raw)
        errors = inspect_artist_plan(design, target, 'existing', 2, topology=topology)
        curved = False
        for operation in design['planned_actions']:
            if operation['type'] not in ['sculpt_path', 'sculpt_region']: errors.append('Only authorized terrain sculpt actions')
            knots = operation['parameters'].get('knots', [operation['parameters']])
            for k in knots:
                x, y, z = k['center']; dx, dy, dz = k['delta']
                if not (-72 <= x <= 72 and 30 <= y <= 82 and 16 <= k['radius'] <= 32): errors.append('Every center X=-72..72,Y=30..82; every radius 16..32')
                if dx != 0 or dy != 0 or not (-6 <= dz <= -1.5 or 2 <= dz <= 7): errors.append('Every delta [0,0,dz]; dz -6..-1.5 or +2..+7')
            if operation['type'] == 'sculpt_path' and len(knots) >= 3:
                a, b, d = [k['center'] for k in knots[:3]]
                curved |= abs((b[0]-a[0])*(d[1]-a[1])-(b[1]-a[1])*(d[0]-a[0])) >= 150
        if not curved: errors.append('At least one sculpt_path needs 3..4 distinct non-collinear knots with a meaningful bend; straight ridges already failed')
    except (ValueError, KeyError, TypeError) as error: errors.append(str(error))
    if not errors:
        (c / 'artist-plan.json').write_text(json.dumps(design['planned_actions'], indent=2))
        (c / 'artist-inference.json').write_text(json.dumps({'model': payload['model'], 'system_role': 'system',
            'system_prompt_sha256': hashlib.sha256(role).hexdigest(), 'input_images': inputs,
            'direction': design, 'image_type': 'BLENDER_NOT_UNITY', 'seconds': time.monotonic()-started,
            'model_weights_trained': False, 'self_approval': False}, indent=2))
        break
    if attempt == 1: raise RuntimeError('Revision rejected: ' + str(errors))
    payload['messages'].append({'role': 'user', 'content': 'Choose your own NEW geometry correcting these exact errors: ' + str(errors)})
