"""Bounded Qwen-authored edit of the existing saved river-bank mesh, via existing MCP."""
import array, ast, hashlib, json, math, os, pathlib, subprocess, sys, time, urllib.request
import bpy
from mathutils import Vector

ROOT = pathlib.Path.cwd()
EV = ROOT / 'docs/evidence/valoria-reference-clean'
OUT = ROOT / 'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources'
SOURCE = EV / 'Valoria_Reference_Clean.blend'
def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
def write(name, data):
    (EV / name).write_text(json.dumps(data, indent=2), encoding='utf-8')

# Load only the existing reviewed transport definition; never execute the scene builder.
tree = ast.parse((ROOT / 'tools/valoria-reference-clean/author_scene.py').read_text())
transport = next(n for n in tree.body if isinstance(n, ast.FunctionDef) and n.name == 'execute_via_blendermcp')
RejectedPlan = RuntimeError
exec(compile(ast.Module(body=[transport], type_ignores=[]), '<existing reviewed MCP transport>', 'exec'))
expected = json.loads((EV / 'source-art.json').read_text())['blend_sha256']
assert digest(SOURCE) == expected, 'Saved Blender source changed'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE),use_scripts=False)
obj = bpy.data.objects.get('River_bank_rocks')
assert obj and obj.type == 'MESH', 'Existing bank mesh missing'
mesh = obj.data
def mesh_stamp(item):
    coords=array.array('f',[0])*(len(item.data.vertices)*3)
    item.data.vertices.foreach_get('co',coords)
    return (tuple(v for row in item.matrix_world for v in row),hashlib.sha256(coords.tobytes()).hexdigest())
protected={o.name:mesh_stamp(o) for o in bpy.context.scene.objects if o.type=='MESH' and o!=obj}
vertices = [obj.matrix_world @ v.co for v in mesh.vertices]
adj = [set() for _ in vertices]
for edge in mesh.edges:
    a, b = edge.vertices
    adj[a].add(b); adj[b].add(a)
remaining = set(range(len(vertices))); components = []
while remaining:
    seed = min(remaining); remaining.remove(seed); stack = [seed]; ids = []
    while stack:
        i = stack.pop(); ids.append(i)
        for j in adj[i]:
            if j in remaining:
                remaining.remove(j); stack.append(j)
    components.append(sorted(ids))
def bounds(ids):
    pts = [vertices[i] for i in ids]
    lo = [min(p[k] for p in pts) for k in range(3)]
    hi = [max(p[k] for p in pts) for k in range(3)]
    return lo, hi
targets = []
for index, ids in enumerate(components):
    lo, hi = bounds(ids); center = [(a+b)/2 for a,b in zip(lo,hi)]
    # Blender Y inland; protected bridge/route are well west of this right-bank zone.
    if 8 <= center[0] <= 28 and -21 < center[1] < -10 and hi[2] > -1:
        targets.append(dict(component=index, bbox_min=lo, bbox_max=hi, vertices=len(ids)))
assert len(targets) >= 3, ('Insufficient actual boulders in representative zone', targets)
context = dict(source_blend_sha256=expected, target=obj.name, coordinate_system='Blender WORLD X east, Y inland, Z up',
    water_z=-2.05, actual_components=targets,
    visual_diagnosis='Native54/58 actual Unity main image: tall separate rocks form a regular standing row along the river. Reference has low broad embedded fractured rock shelves. Existing small-pebble rounds44/45 were visible but insufficient; do not add more pebbles. Ground57 blurred/dark patch rejected; ground58 minor improvement not retained. Preserve scene composition, terrain, materials, camera and all other meshes.',
    task='Author 3 to 7 edits of these REAL connected components into lower broader shelves with varied crowns; change only this representative north bank zone. Do not invent component IDs. Base Z stays fixed; do not lower every crown to the same height. Top must remain above water_z+0.15. Preserve closed topology/UVs. Choose actual values per boulder.',
    schema={'rationale':'short art reason', 'actions':[{'component':'integer from actual_components', 'scale_x':'0.85..1.2', 'scale_y':'1.05..1.45', 'scale_z':'0.45..0.80', 'shift_inland':'0..0.6'}]})
write('bank-patch-mesh-context.json', context)
for item in context['actual_components']:
    low,high=item['bbox_min'][2],item['bbox_max'][2]
    item['minimum_scale_z_above_water']=max(.45, round((-1.90-low)/(high-low)+.005,4))
context['task'] += ' Select at most SEVEN components, not all nine. Each scale_z must exceed its minimum_scale_z_above_water.'
context_text=json.dumps(context)
response_schema={'type':'object','required':['rationale','actions'],'additionalProperties':False,'properties':{'rationale':{'type':'string'},'actions':{'type':'array','minItems':3,'maxItems':7,'items':{'type':'object','required':['component','scale_x','scale_y','scale_z','shift_inland'],'additionalProperties':False,'properties':{'component':{'type':'integer','enum':[t['component'] for t in targets]},'scale_x':{'type':'number','minimum':.85,'maximum':1.2},'scale_y':{'type':'number','minimum':1.05,'maximum':1.45},'scale_z':{'type':'number','minimum':.45,'maximum':.80},'shift_inland':{'type':'number','minimum':0,'maximum':.6}}}}}}
payload=dict(model='qwen3:14b', stream=False, format=response_schema, think=False,keep_alive=0,
    messages=[{'role':'system','content':'You are the Eldoria 3D art director. Author a bounded geometric improvement from actual world-space mesh data and the supplied independently observed visual diagnosis. Output only JSON in the requested schema. No Python, no tools, no invented geometry. Never claim art approval.'},
              {'role':'user','content':context_text}],
    options={'temperature':0.25,'num_ctx':8192,'num_predict':1800})
write('bank-patch-qwen-request.json',payload)
server=None;log=None
def api(path, data=None, timeout=3):
    request=urllib.request.Request('http://127.0.0.1:11434'+path, data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return json.load(response)
try:
    try: api('/api/version')
    except Exception:
        users=pathlib.Path(os.environ.get('SystemDrive','C:')+'/Users')
        installs=list(users.glob('*/AppData/Local/Programs/Ollama/ollama.exe'))
        assert installs, 'Previously installed Ollama missing'
        env=os.environ.copy();env['OLLAMA_HOST']='127.0.0.1:11434'
        models=list(users.glob('*/.ollama/models'))
        if models: env['OLLAMA_MODELS']=str(models[0])
        log=open(EV/'bank-patch-ollama.log','w')
        server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
        for attempt in range(40):
            try: api('/api/version');break
            except Exception: time.sleep(1)
        else: raise RuntimeError('Installed Ollama did not become ready')
    tags=api('/api/tags')
    assert any(m['name']=='qwen3:14b' for m in tags['models']), 'Installed model missing; no download allowed'
    start=time.monotonic();response=api('/api/chat',payload,240)
    write('bank-patch-qwen-response.json',response)
    plan=json.loads(response['message']['content']);actions=plan['actions']
    assert 3 <= len(actions) <= min(7,len(targets))
    allowed={t['component']:t for t in targets};seen=set()
    for action in actions:
        i=action['component'];assert isinstance(i,int) and i in allowed and i not in seen;seen.add(i)
        assert set(action)=={'component','scale_x','scale_y','scale_z','shift_inland'}
        for key,low,high in [('scale_x',.85,1.2),('scale_y',1.05,1.45),('scale_z',.45,.80),('shift_inland',0,.6)]:
            assert isinstance(action[key],(int,float)) and math.isfinite(action[key]) and low<=action[key]<=high, action
        lo,hi=bounds(components[i]);assert lo[2]+(hi[2]-lo[2])*action['scale_z']>-1.90, 'Crown submerged'
    write('bank-patch-validated-plan.json',dict(plan=plan,context_sha256=hashlib.sha256(context_text.encode()).hexdigest(),inference_seconds=time.monotonic()-start,art_approved=False))
finally:
    if server:
        subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],check=False,capture_output=True)
        server.wait(timeout=20)
    if log:log.close()

original=[v.co.copy() for v in mesh.vertices]
operation={'id':'valoria-existing-bank-shelves-qwen-v1','op':'reshape_existing_bank_components','actions':actions}
changed=set();inverse=obj.matrix_world.inverted()
def perform(action):
    assert action==operation and not changed
    for a in action['actions']:
        ids=components[a['component']];lo,hi=bounds(ids);cx=(lo[0]+hi[0])/2;cy=(lo[1]+hi[1])/2
        for i in ids:
            p=vertices[i]
            q=Vector((cx+(p.x-cx)*a['scale_x'],cy+(p.y-cy)*a['scale_y']+a['shift_inland'],lo[2]+(p.z-lo[2])*a['scale_z']))
            mesh.vertices[i].co=inverse@q;changed.add(i)
    mesh.update()
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None)
transport_status='ACTUAL_EXISTING_BLENDERMCP_TYPED_ACTION';transport_error=None
try:
    assert addon is not None, 'Existing BlenderMCP addon not found'
    execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
except Exception as error:
    transport_error=str(error)
    for i,p in enumerate(original):mesh.vertices[i].co=p
    changed.clear();perform(operation)
    transport_status='DIRECT_BLENDER_TYPED_FALLBACK_AFTER_MCP_UNAVAILABLE'
assert changed and all(mesh.vertices[i].co==original[i] for i in range(len(original)) if i not in changed)
assert all(mesh_stamp(bpy.data.objects[name])==stamp for name,stamp in protected.items()), 'Protected mesh changed'
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(EV/'RiverBankPatch.blend'))
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=str(OUT/'RiverBankPatch.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected, 'Original saved source changed'
write('bank-patch-result.json',dict(source_sha=os.environ.get('GITHUB_SHA'),source_blend_sha256=expected,
    source_preserved=True,transport=transport_status,transport_error=transport_error,qwen_model='qwen3:14b',
    edited_components=sorted(seen),changed_vertices=len(changed),topology_unchanged=True,other_vertices_unchanged=True,
    glb_sha256=digest(OUT/'RiverBankPatch.glb'),blend_sha256=digest(EV/'RiverBankPatch.blend'),art_approved=False))
print('VALORIA_EXISTING_BANK_PATCH_COMPLETE')
