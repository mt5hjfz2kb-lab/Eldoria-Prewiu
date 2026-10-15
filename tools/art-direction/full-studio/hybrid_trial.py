"""One adapter over the existing artist studio and verified joinery workshop.

No paid APIs; external director JSON and local director share exact schema,
baseline, cameras, source observations and constructive executor. Never approves
its own work. Explicit review is required before chaining a candidate.
"""
from __future__ import annotations
import argparse, hashlib, json, pathlib, subprocess, sys, time
from types import SimpleNamespace
HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE));sys.path.insert(0,str(HERE.parent/'gpt6-tower-eval-20261015'))
sys.path.insert(0,str(HERE.parent/'director-control'))
from artist_loop import request_ollama, observe_source_with_vl, image_data
from joinery_contract import validate
from selective_memory import recall_context

BRIEF=('Improve the architectural craftsmanship and readable front framing of '
 'this existing timber watchtower in the fixed camera, toward the owner Valoria '
 'reference. Preserve original geometry, UV, roof, doorway and stairs. Choose '
 'a coherent complete timber assembly, its proportions, positions, material '
 'and real joints. Avoid a tiny ridge strip, unsupported decoration, or claiming '
 'hidden joinery visible. Metres WORLD XYZ, +Z up. No replacement tower.')

def sha(p):return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
def put(p,d):p.write_text(json.dumps(d,indent=2,ensure_ascii=False)+'\n',encoding='utf8')
def blender(exe,script,args,out):
    t=time.monotonic()
    r=subprocess.run([exe,'-b','-t','4','--python-exit-code','1','--python',str(script),'--']+list(map(str,args)),
                     stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,timeout=900)
    out.write_text(r.stdout,encoding='utf8')
    if r.returncode:raise RuntimeError('BLENDER_FAILED '+r.stdout[-900:])
    return round(time.monotonic()-t,3)

def schema_like(value):
    # A type schema, never artistic parameters copied from the example.
    if type(value) is bool:return {'type':'boolean','const':False}
    if isinstance(value,str):return {'type':'string'}
    if type(value) is int:return {'type':'integer'}
    if type(value) is float:return {'type':'number'}
    if isinstance(value,list):
        return {'type':'array','items':schema_like(value[0]),
                **({'minItems':len(value),'maxItems':len(value)} if value and type(value[0]) in (int,float) else {})}
    return {'type':'object','properties':{k:schema_like(v) for k,v in value.items()},
            'required':list(value),'additionalProperties':False}

def common_schema():
    example=json.loads((HERE.parent/'gpt6-tower-eval-20261015/gpt6-joinery-plan-r3.json').read_text())
    s=schema_like(example);props=s['properties']
    props['schema_version']={'type':'integer','const':1}
    props['members'].update(minItems=2,maxItems=12)
    props['joints'].update(minItems=1,maxItems=16)
    jp=props['joints']['items']['properties']
    jp['kind']={'type':'string','const':'mortise_tenon'};jp['axis']={'type':'string','const':'Z'}
    return s

def prepare(a):
    a.out.mkdir(parents=True,exist_ok=False)
    if sha(a.source)!='e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869':
        raise ValueError('FROZEN_SOURCE_SHA_MISMATCH')
    blender(a.blender,HERE/'hybrid_source.py',['--source',a.source,'--atlas',a.atlas,
       '--review',a.review,'--out',a.out/'source'],a.out/'normalization.log')
    source=a.out/'source/normalized-source.glb'
    blender(a.blender,HERE/'inspect_source.py',['--source',source,'--review',a.review,
      '--out',a.out/'inspection'],a.out/'inspection.log')
    inspection=a.out/'inspection';inv=json.loads((inspection/'source-inspection.json').read_text())
    args=SimpleNamespace(vision_model=a.vision_model,model=a.model,endpoint=a.endpoint)
    obs=observe_source_with_vl(args,a.reference,inspection/'source-preview-blender.png',
          [inspection/'source-top-blender.png',inspection/'source-side-blender.png'],'',a.out)
    # Existing selective recall, not model training. Literal source + chosen entries.
    memory=recall_context('Comprobar contacto AABB de la geometría modificada y presencia final del material y textura contra referencia')
    put(a.out/'selective-memory.json',memory)
    objects=[]
    for obj in inv['objects']:
        objects.append({k:obj[k] for k in ('name','world_bounds_min_m','world_bounds_max_m',
          'camera_visible_face_landmarks','materials','uv_layers') if k in obj})
    context={'brief':BRIEF,'source_sha256':sha(source),'original_source_sha256':sha(a.source),
      'reference_sha256':sha(a.reference),'review_sha256':sha(a.review),
      'observation':obs['visual_observation'],'geometry':objects,'memory':memory['prompt_fragment'],
      'capabilities':'Parametric timber members, real mortise/tenon joints, pegs, bevel, UV, embedded wood texture; direct Blender executor shared by both directors.',
      'limits':'This controlled test uses axis-aligned timber/joints. Whole studio remains available for other hypotheses. No superiority claim outside this task.',
      'schema':common_schema()}
    put(a.out/'common-context.json',context)
    (a.out/'common-context.sha256').write_text(sha(a.out/'common-context.json')+'\n')

def compare(a):
    pack=a.pack;context=json.loads((pack/'common-context.json').read_text())
    source=pack/'source/normalized-source.glb'
    if sha(source)!=context['source_sha256'] or sha(a.reference)!=context['reference_sha256'] or sha(a.review)!=context['review_sha256']:
        raise ValueError('EXACT_COMPARISON_INPUT_CHANGED')
    a.out.mkdir(parents=True,exist_ok=False)
    messages=[{'role':'system','content':'You are a timber architectural artist. Return only original JSON using the provided workshop schema. No self approval. /no_think'},
      {'role':'user','content':json.dumps({k:v for k,v in context.items() if k!='schema'},ensure_ascii=False)+
       '\nWorkshop physical rules: member IDs start Eldoria_. size each axis .025..6m; bevel .001..min(size)/4. '
       'Every post top equals joint center Z minus tenon height/2, tolerance .008m. '
       'Tenon sizes each .015.. .30m; socket is .002.. .015m larger in every axis. '
       'Mortise center enters bottom of its lintel but stops .005m below top. '
       'Every tenon/socket must physically overlap its member also in X/Y. '
       'Peg radius .005.. .04m, length .02.. .20m, peg center must cross socket.'}]
    put(a.out/'submitted-local-context.json',{'model':a.model,'messages':messages,'schema':context['schema'],
        'common_context_sha256':sha(pack/'common-context.json')})
    t=time.monotonic();rejections=[];local=None
    for attempt in range(1,3):
        raw=request_ollama(a.endpoint,a.model,messages,2800,timeout=540,schema=context['schema'])
        (a.out/f'qwen-raw-{attempt}.json').write_text(raw,encoding='utf8')
        try:
            local=json.loads(raw);validate(local)
            break
        except (ValueError,KeyError,TypeError) as e:
            local=None;rejections.append(str(e))
            messages=messages+[{'role':'assistant','content':raw},{'role':'user','content':'Plan rejected: '+str(e)+'. Revise your own plan, no guessing.'}]
    rows=[]
    for label,plan in [('external',json.loads(a.external_plan.read_text())),('local',local)]:
        if plan is None:
            rows.append({'director':label,'state':'PLAN_REJECTED','rejections':rejections});continue
        validate(plan)
        dest=a.out/label;planpath=a.out/(label+'-plan.json');put(planpath,plan)
        try:
            elapsed=blender(a.blender,HERE.parent/'gpt6-tower-eval-20261015/joinery_blender.py',
                ['--plan',planpath,'--source',source,'--review',a.review,'--out',dest],a.out/(label+'-blender.log'))
            proof=json.loads((dest/'workshop-proof.json').read_text())
            contact=json.loads((dest/'actual-mesh-contact.json').read_text())
            # Existing intent critic sees true rendered pixels; cannot promote.
            critic_schema={'type':'object','properties':{k:{'type':'string'} for k in
                ('visible_change','architectural_benefit','regressions','style_gap','uncertainty','verdict')},
                'required':['visible_change','architectural_benefit','regressions','style_gap','uncertainty','verdict']}
            raw=request_ollama(a.endpoint,a.vision_model,[{'role':'system','content':'Independent visual critic. Do not assume edits are improvements. Hidden joints cannot be seen. Never approve production.'},
               {'role':'user','content':'Images: BEFORE, AFTER, destination STYLE. Identify visible benefit or harm. Distinguish material recovery from added geometry. '+plan.get('design_intent',''),
                'images':[image_data(dest/'before-blender.png',640),image_data(dest/'after-blender.png',640),image_data(a.reference,640)]}],650,timeout=420,schema=critic_schema)
            (dest/'independent-vl-raw.json').write_text(raw,encoding='utf8')
            rows.append({'director':label,'state':'ART_REVIEW_REQUIRED','blender_seconds':elapsed,
                'proof':proof,'contact_all_new':contact.get('actual_surface_contact_all_new_components'),
                'plan_sha256':sha(planpath),'critic':json.loads(raw)})
        except Exception as e:
            rows.append({'director':label,'state':'EXECUTION_OR_REVIEW_HOLD','error':str(e)})
    put(a.out/'comparison-result.json',{'common_context_sha256':sha(pack/'common-context.json'),
       'same_source_sha256':sha(source),'same_review_sha256':sha(a.review),
       'same_workshop_schema_sha256':hashlib.sha256(json.dumps(common_schema(),sort_keys=True).encode()).hexdigest(),
       'directors':rows,'local_plan_rejections':rejections,'total_seconds':round(time.monotonic()-t,3),
       'art_approved':False,'calibration_validated':False,'unity_paused':True,
       'next':'Independent image review; do not chain or transfer an unaccepted candidate.'})

def main():
    p=argparse.ArgumentParser();p.add_argument('mode',choices=('prepare','compare'))
    for n in ('source','atlas','reference','review','out','pack','external-plan'):p.add_argument('--'+n,type=pathlib.Path)
    p.add_argument('--blender',required=True);p.add_argument('--model',default='qwen3:14b')
    p.add_argument('--vision-model',default='qwen3-vl:8b-instruct-q4_K_M')
    p.add_argument('--endpoint',default='http://127.0.0.1:11434/api/chat')
    a=p.parse_args();prepare(a) if a.mode=='prepare' else compare(a)
if __name__=='__main__':main()
