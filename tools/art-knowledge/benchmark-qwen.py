#!/usr/bin/env python3
"""Bounded genuine two-arm Qwen diagnostic, plus a deterministic no-Qwen baseline.
No Blender, no fake visual rating, no Tripo, and no modification of Eldoria.
"""
import base64, hashlib, io, json, os, pathlib, time, urllib.request, urllib.error
from PIL import Image

MODEL='qwen3-vl:2b-instruct'
ENDPOINT='http://127.0.0.1:11434/api/chat'
SRC_REF=pathlib.Path('ArtSource/World4X/Spawnling_v1/Finished/finished_face.png')
SRC_GAME=pathlib.Path('docs/evidence/spawnling-bastion2/after-local-900x1600.png')
KNOWLEDGE=pathlib.Path('pipeline/art-knowledge/catalog-v1.json')
REQUEST=pathlib.Path('pipeline/art-production-requests/eldoria-tripo-art-brain-phase2-20261021.json')
def digest(raw): return hashlib.sha256(raw).hexdigest()
def image(p, out):
    raw=p.read_bytes()
    with Image.open(io.BytesIO(raw)) as img:
        img=img.convert('RGB')
        img.thumbnail((400,400))
        data=io.BytesIO();img.save(data,format='JPEG',quality=80)
    compressed=data.getvalue();out.write_bytes(compressed)
    return {'source':str(p),'source_sha256':digest(raw),'generated_jpeg_sha256':digest(compressed),'source_bytes':len(raw),'jpeg_bytes':len(compressed)}
def generate(messages,maxsec=410):
    schema={'type':'object','properties':{
      'problem':{'type':'string','maxLength':150},
      'candidate_operation':{'type':'string','enum':['inspect','sculpt_region','assign_material','no_edit']},
      'spatial_safety':{'type':'string','maxLength':150},
      'visual_limit':{'type':'string','maxLength':150}
     },'required':['problem','candidate_operation','spatial_safety','visual_limit'],
     'additionalProperties':False}
    body={'model':MODEL,'stream':False,'think':False,'format':schema,
     'options':{'num_ctx':4096,'num_predict':620,'temperature':0},
     'messages':messages}
    started=time.monotonic()
    try:
        req=urllib.request.Request(ENDPOINT,data=json.dumps(body).encode(),headers={'Content-Type':'application/json'})
        with urllib.request.urlopen(req,timeout=maxsec) as res:reply=json.load(res)
        txt=reply.get('message',{}).get('content','')
        try: response=json.loads(txt)
        except json.JSONDecodeError as ex:
            return {'status':'REAL_QWEN_RETURNED_INVALID_JSON','seconds':round(time.monotonic()-started,3),
                    'error':str(ex)[:220],'raw':txt[:5000],
                    'eval_count':reply.get('eval_count'),'prompt_eval_count':reply.get('prompt_eval_count'),
                    'response_sha256':digest(txt.encode())}
        if not isinstance(response,dict) or not {'problem','candidate_operation','spatial_safety','visual_limit'}.issubset(response):
            return {'status':'REAL_QWEN_RETURNED_INCOMPLETE_SCHEMA',
                    'seconds':round(time.monotonic()-started,3),'raw':txt[:5000],
                    'response_sha256':digest(txt.encode())}
        return {'status':'INFERRED','seconds':round(time.monotonic()-started,3),
                'raw':txt[:5000],'json':response,'eval_count':reply.get('eval_count'),
                'prompt_eval_count':reply.get('prompt_eval_count'),
                'response_sha256':digest(txt.encode())}
    except (OSError,ValueError,TimeoutError,urllib.error.URLError) as ex:
        return {'status':'QWEN_FAILED_NO_CLAIM','seconds':round(time.monotonic()-started,3),
                'error':type(ex).__name__+': '+str(ex)[:250]}
def score(payload):
    if payload.get('status')!='INFERRED':return {'valid_json':False,'meets_minimum_source_contract':False}
    z=payload['json'];txt=json.dumps(z,ensure_ascii=False).lower()
    return {'valid_json':isinstance(z,dict),
      'acknowledged_ground_truth':any(a in txt for a in ['+x','cabeza','head','front']),
      'rejects_global_shift':any(a in txt for a in ['no mover','global','desplazamiento','translate','whole body','entire body','no translate']),
      'avoids_art_approval':not any(a in txt for a in ['art_approved: true','aprobado 10/10','definitely approved']),
      'demonstrates_actual_geometry_improvement':False}
def main():
    root=pathlib.Path(os.environ.get('BENCH_OUT','/tmp/eldoria-memory-ab'))
    root.mkdir(parents=True,exist_ok=True)
    canon=json.loads(KNOWLEDGE.read_text('utf8'));req=json.loads(REQUEST.read_text('utf8'))
    assert req['allow_tripo'] is False and not req['credit_authorization']['approved']
    assert canon['safe_defaults']['may_modify_unity'] is False
    left=image(SRC_REF,root/'reference.jpg');right=image(SRC_GAME,root/'game.jpg')
    imgs=[base64.b64encode((root/'reference.jpg').read_bytes()).decode(),
          base64.b64encode((root/'game.jpg').read_bytes()).decode()]
    # Freeze everything except the extra historical-case context.
    base=('Study the SAME REAL two photographs of Eldoria Spawnling. First is a close-up '
      'of the existing sculpt, second the strategic in-game view. This creature has head '
      'faces oriented +X according to measured source; do not invent vertex coordinates. '
      'Identify ONE safe artistic next operation; preserve two legs, hands, contacts, '
      'existing gameplay/rig and original 3D source. Return JSON with keys '
      'problem, candidate_operation, spatial_safety, source_constraints, visual_limit. '
      'Do NOT claim to have edited Blender or improved graphics.')
    remembered=[x for x in canon['records'] if x['id'] in ('K13','K14','K19','K20','K38')]
    lessons=' ELDORIA VERIFIED-STATUS MEMORY: '+' '.join(
      x['id']+': '+x['observed_result'][:220]+'; AVOID: '+x['avoid'][:180]
      for x in remembered)
    # Control: no-Qwen uses the existing policy and makes no artistic decision.
    no_qwen={'status':'NO_QWEN_RULE_BASELINE','seconds':0,'model_calls':0,
      'output':'Inspect unchanged model, no automatic sculpt; refer to existing Blender/Unity art review.',
      'art_approved':False,'artistic_improvement_verified':False}
    def messages(extra):
        return [{'role':'system','content':'You are an advisory art analyst, never an art-approval authority.'},
                {'role':'user','content':base+extra,'images':imgs}]
    arms={}
    # Same model, options, source PNG derivatives and base prompt. Fixed order is disclosed.
    for label,extra in [('qwen_without_memory',''),('qwen_with_memory',lessons)]:
        prompt=messages(extra)
        arms[label]=generate(prompt)
        arms[label]['prompt_sha256']=digest(json.dumps(prompt,sort_keys=True).encode())
        arms[label]['judgment']=score(arms[label])
        (root/(label+'.json')).write_text(json.dumps(arms[label],indent=2,ensure_ascii=False)+'\n')
        print('ELDORIA_AB_ARM',label,arms[label]['status'],'seconds',arms[label]['seconds'],flush=True)
    report={'schema_version':1,'kind':'genuine_qwen_memory_ab_diagnostic',
      'source_reference':left,'source_unity':right,'model':MODEL,
      'base_prompt_sha256':digest(base.encode()),'memory_case_ids':[x['id'] for x in remembered],
      'memory_text_sha256':digest(lessons.encode()),
      'controls':'same image bytes/model/JSON format/context limit/sampling temperature; different memory text; fixed order (not blinded)',
      'baseline_no_qwen':no_qwen,'arms':arms,
      'quality_impact':'UNDETERMINED_NO_BLENDER_OR_UNITY_AB',
      'cost_claim':'NO_PAID_TRIPO_OR_CLOUD_MODEL_INVOCATIONS',
      'limitations':['CPU latency is noisy; one sample per Qwen condition',
       'Independent artistic value cannot be inferred from JSON responses',
       'Do not claim time savings until repeated task-matched 3D output trials',
       'Controlled Qwen-memory effect only, NOT actual quality of a new game asset',
       'Qwen-free baseline has no inferred tokens; it is a safe route, not matched finished art'],
      'art_approved':False,'game_mutated':False}
    (root/'report.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
    print('ELDORIA_REAL_AB_RECORDED',json.dumps({k:v['status'] for k,v in arms.items()}),flush=True)
    # A valid comparison report exists even if CPU Qwen fails; never invent quality success.
if __name__=='__main__':main()
