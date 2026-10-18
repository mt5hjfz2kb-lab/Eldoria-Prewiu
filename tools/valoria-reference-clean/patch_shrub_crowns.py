"""Bounded Qwen-authored edit of the existing saved river-bank mesh, via existing MCP."""
import bmesh, array, ast, hashlib, json, math, os, pathlib, subprocess, sys, time, urllib.request
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
obj = bpy.data.objects.get('Shrub_group')
assert obj and obj.type == 'MESH', 'Existing shrub group missing'
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
targets=[]
for index,ids in enumerate(components):
    lo,hi=bounds(ids);center=[(a+b)/2 for a,b in zip(lo,hi)];extent=[b-a for a,b in zip(lo,hi)]
    if -15<center[0]<15 and 28<center[1]<42 and min(extent[0],extent[1])>.65 and extent[2]>.5:
        targets.append(dict(component=index,bbox_min=lo,bbox_max=hi,vertices=len(ids),extent=extent))
targets=sorted(targets,key=lambda t:t['extent'][0]*t['extent'][1],reverse=True)[:2]
assert targets, 'No measured foundation donor crowns found'
context=dict(target=obj.name,source_blend_sha256=expected,coordinate_system='Blender WORLD X east,Y inland,Z up',
 actual_components=targets,visual_diagnosis='Actual Unity63 Bastion detail: donor shrubs read as large faceted green rocks; reference has many smaller coherent rounded leaf clusters. Preserve the existing footprint, ground contact and all architecture. Native63 SSAO-only change was too weak and not retained; do not use light or material tint as substitute for crown geometry.',
 task='Replace each listed existing single crown with an authored arrangement of 7 to 11 overlapping small leaf lobes. Give normalized centers x/y/z in [0,1] of the measured crown bbox and normalized radii rx/ry/rz in [0.16,0.34]. Every lobe must fit inside original bounds: radius <= center <= 1-radius. Keep bottom contact (lowest z-radius <=0.04) and top profile (highest z+radius >=0.9); preserve clump silhouette overall, with notches. Author specific asymmetric overlapping centers, not new separate plants. Return exactly one action per listed component. Existing crown will be removed, never overlaid.')
write('shrub-crown-mesh-context.json',context);context_text=json.dumps(context)
lobe_props={k:{'type':'number'} for k in ['x','y','z','rx','ry','rz']}
response_schema={'type':'object','required':['rationale','actions'],'additionalProperties':False,'properties':{'rationale':{'type':'string'},'actions':{'type':'array','minItems':len(targets),'maxItems':len(targets),'items':{'type':'object','required':['component','lobes'],'additionalProperties':False,'properties':{'component':{'type':'integer','enum':[t['component'] for t in targets]},'lobes':{'type':'array','minItems':7,'maxItems':11,'items':{'type':'object','required':list(lobe_props),'additionalProperties':False,'properties':lobe_props}}}}}}}
payload=dict(model='qwen3:14b',stream=False,format=response_schema,think=False,keep_alive=0,
 messages=[{'role':'system','content':'You are the Eldoria 3D vegetation artist. Author an actual coherent leaf crown using the measured original component bounds. Return only the typed JSON plan, never Python. Do not claim quality approval.'},{'role':'user','content':context_text}],
 options={'temperature':.25,'num_ctx':8192,'num_predict':2800})
write('shrub-crown-qwen-request.json',payload)
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
        log=open(EV/'shrub-crown-ollama.log','w')
        server=subprocess.Popen([str(installs[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
        for attempt in range(40):
            try: api('/api/version');break
            except Exception: time.sleep(1)
        else: raise RuntimeError('Installed Ollama did not become ready')
    tags=api('/api/tags')
    assert any(m['name']=='qwen3:14b' for m in tags['models']), 'Installed model missing; no download allowed'
    start=time.monotonic();response=api('/api/chat',payload,240)
    write('shrub-crown-qwen-response.json',response)
    plan=json.loads(response['message']['content']);actions=plan['actions'];allowed={t['component']:t for t in targets};seen=set()
    def validate(candidate):
        assert len(candidate['actions'])==len(targets), 'One action required per measured crown'
        checked=set()
        for action in candidate['actions']:
            i=action['component'];assert isinstance(i,int) and i in allowed and i not in checked;checked.add(i)
            assert 7<=len(action['lobes'])<=11
            for lobe in action['lobes']:
                assert set(lobe)==set(lobe_props)
                for k in lobe_props:assert isinstance(lobe[k],(int,float)) and math.isfinite(lobe[k])
                for axis in 'xyz':
                    radius=lobe['r'+axis];center=lobe[axis]
                    assert .16<=radius<=.34 and radius-.001<=center<=1-radius+.001, 'Lobe outside preserved crown envelope'
            assert min(l['z']-l['rz'] for l in action['lobes'])<=.04, 'Floating crown'
            assert max(l['z']+l['rz'] for l in action['lobes'])>=.9, 'Crown top lost'
        return checked
    try:seen=validate(plan)
    except AssertionError as error:
        write('shrub-crown-first-rejected-plan.json',dict(plan=plan,error=str(error)))
        payload['messages'] += [{'role':'assistant','content':response['message']['content']},{'role':'user','content':'Repair this geometric validation error without changing the measured envelope: '+str(error)+'. Each axis must satisfy radius <= center <= 1-radius; base <=0.04 and top >=0.90. Return complete corrected JSON.'}]
        write('shrub-crown-correction-request.json',payload)
        response=api('/api/chat',payload,240);write('shrub-crown-correction-response.json',response)
        plan=json.loads(response['message']['content']);seen=validate(plan);actions=plan['actions']
    write('shrub-crown-validated-plan.json',dict(plan=plan,context_sha256=hashlib.sha256(context_text.encode()).hexdigest(),inference_seconds=time.monotonic()-start,art_approved=False))
finally:
    if server:
        subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],check=False,capture_output=True)
        server.wait(timeout=20)
    if log:log.close()

selected={i for action in actions for i in components[action['component']]}
before_triangles=sum(len(p.vertices)-2 for p in mesh.polygons)
materials={}
for action in actions:
    ids=set(components[action['component']]);polys=[p for p in mesh.polygons if any(i in ids for i in p.vertices)]
    assert all(all(i in ids for i in p.vertices) for p in polys)
    material=max(set(p.material_index for p in polys),key=lambda m:sum(1 for p in polys if p.material_index==m))
    mat=mesh.materials[material]
    assert not any(n.type=='TEX_IMAGE' and n.image for n in mat.node_tree.nodes), 'Textured crown needs authored UV mapping; do not destroy it'
    materials[action['component']]=material
original_mesh=mesh.copy()
operation={'id':'valoria-existing-shrub-crowns-qwen-v1','op':'replace_existing_crowns_with_leaf_lobes','actions':actions}
performed=False
def perform(action):
    global performed
    assert action==operation and not performed
    bm=bmesh.new();bm.from_mesh(mesh);bm.verts.ensure_lookup_table()
    bmesh.ops.delete(bm,geom=[bm.verts[i] for i in sorted(selected)],context='VERTS')
    inv=obj.matrix_world.inverted()
    for a in actions:
        lo,hi=bounds(components[a['component']]);extent=[hi[k]-lo[k] for k in range(3)]
        for l in a['lobes']:
            created=bmesh.ops.create_icosphere(bm,subdivisions=2,radius=1)['verts'];created=set(created)
            for v in created:
                unit=v.co.copy();v.co=inv@Vector([lo[k]+extent[k]*(l[axis]+unit[k]*l['r'+axis]) for k,axis in enumerate('xyz')])
            for f in {f for v in created for f in v.link_faces}:
                f.material_index=materials[a['component']];f.smooth=True
    bm.normal_update();bm.to_mesh(mesh);bm.free();mesh.update();performed=True
import addon_utils
addon=next((m for m in addon_utils.modules() if m.__name__=='blender_mcp'),None)
transport_status='ACTUAL_EXISTING_BLENDERMCP_TYPED_ACTION';transport_error=None
try:
    assert addon is not None, 'Existing BlenderMCP addon not found'
    execute_via_blendermcp([operation],perform,pathlib.Path(addon.__file__),'eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5')
except Exception as error:
    transport_error=str(error);obj.data=original_mesh.copy();mesh=obj.data;performed=False;perform(operation)
    transport_status='DIRECT_BLENDER_TYPED_FALLBACK_AFTER_MCP_UNAVAILABLE'
assert performed and all(mesh_stamp(bpy.data.objects[name])==stamp for name,stamp in protected.items()), 'Protected mesh changed'
def face_key(data,p):
    return (p.material_index,tuple(sorted(tuple(round(v,5) for v in data.vertices[i].co) for i in p.vertices)))
retained={face_key(original_mesh,p) for p in original_mesh.polygons if not any(i in selected for i in p.vertices)}
actual={face_key(mesh,p) for p in mesh.polygons}
assert retained.issubset(actual), 'Unselected donor geometry changed'
after_triangles=sum(len(p.vertices)-2 for p in mesh.polygons)
assert after_triangles-before_triangles<5000, 'Unnecessary crown topology growth'
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(EV/'ShrubCrownPatch.blend'))
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=str(OUT/'ShrubCrownPatch.glb'),export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
assert digest(SOURCE)==expected
write('shrub-crown-result.json',dict(source_sha=os.environ.get('GITHUB_SHA'),source_blend_sha256=expected,source_preserved=True,transport=transport_status,transport_error=transport_error,qwen_model='qwen3:14b',edited_components=sorted(seen),before_triangles=before_triangles,after_triangles=after_triangles,other_faces_unchanged=True,glb_sha256=digest(OUT/'ShrubCrownPatch.glb'),blend_sha256=digest(EV/'ShrubCrownPatch.blend'),art_approved=False))
print('VALORIA_EXISTING_SHRUB_CROWN_COMPLETE')
