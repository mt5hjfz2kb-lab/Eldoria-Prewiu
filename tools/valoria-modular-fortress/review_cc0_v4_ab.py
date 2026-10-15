"""Independent local Qwen3-VL 8B visual A/B reviewer on 12 authentic Blender renders.
Read-only, verifies change quality; never certifies ART_APPROVED or Unity.
"""
import sys,os,base64,json,urllib.request,re,hashlib,time
from pathlib import Path
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else sys.argv[1:]
if len(args)<3:raise SystemExit('source-folder out-folder local-installed-model')
src=Path(args[0]);out=Path(args[1]);model=args[2]
out.mkdir(exist_ok=True,parents=True)
items=['pine_small','pine_medium','pine_tall','boulder_1','boulder_2','boulder_3','cliff_stone_chip','shrub_1','shrub_2','shrub_3','grass_tuft_1','grass_tuft_2']
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def request(route,body=None,timeout=170):
 req=urllib.request.Request('http://127.0.0.1:11434'+route,data=json.dumps(body).encode() if body else None,
 headers={'Content-Type':'application/json'},method='POST' if body else 'GET')
 with urllib.request.urlopen(req,timeout=timeout) as res:return json.loads(res.read())
available=[x['name'] for x in request('/api/tags',timeout=10).get('models',[])]
if model not in available:raise RuntimeError('No installed model '+model+'; no downloading allowed: '+str(available))
records=[]
for i,name in enumerate(items,1):
 before=src/'before'/(name+'.png');after=src/'after'/(name+'.png')
 if not before.is_file() or not after.is_file():raise RuntimeError('Missing authentic A/B for '+name)
 b64=[base64.b64encode(p.read_bytes()).decode() for p in (before,after)]
 prompt=("You are an INDEPENDENT critical art director assessing a paid-quality stylized game art asset. "
 "These are two genuine same-camera/same-light/same-scale Blender renders of the SAME individual asset. "
 "FIRST image is BEFORE and SECOND image is AFTER. Asset "+name+". "
 "Valoria Eldoria benchmark: visually excellent realistic-stylized northern medieval alpine fortress "
 "at dusk, premium aged warm grey limestone, dense detailed dark fir trees, muted mossy green shrubs, "
 "natural rocky shapes and subtle yellow-green grass; all aesthetically coherent. "
 "Evaluate visible artistic merit, not bytes, export success or how much colors changed. "
 "Never prefer AFTER automatically. Return ONE valid JSON object with keys "
 "before_geometry,before_contacts,before_materials,before_style,before_readability,"
 "after_geometry,after_contacts,after_materials,after_style,after_readability "
 "(scores 0-10), before_defects (array of 1-3 concrete defects), "
 "after_defects (array of 1-3 concrete defects), "
 "after_regressions (array), comment (string), recommended_version ('BEFORE' or 'AFTER'). "
 "Score image quality independently, do not force 8. "
 "Use weighted score 25% geometry, 25% coherent contacts, 20% materials, 20% reference style, 10% readability; "
 "do not add new details not visible. For simple modules use individual craftsmanship, not full scene impact.")
 payload={'model':model,'images':b64,'prompt':prompt,'stream':False,'format':'json',
 'options':{'temperature':.12,'num_predict':620,'num_ctx':8192}}
 try:
  response=request('/api/generate',payload,timeout=210)
  raw=response.get('response','')
  try:d=json.loads(raw)
  except Exception:
   m=re.search(r'\{.*\}',raw,re.S);d=json.loads(m.group()) if m else {}
  dims=['geometry','contacts','materials','style','readability']
  scores={}
  for state in ('before','after'):
   arr=[float(d.get(state+'_'+k,-1)) for k in dims]
   if any(a<0 or a>10 for a in arr):raise RuntimeError('Missing or invalid score '+str(arr))
   scores[state]={'dimensions':dict(zip(dims,arr)),
     'weighted':sum(a*w for a,w in zip(arr,[.25,.25,.20,.20,.10]))}
  delta=scores['after']['weighted']-scores['before']['weighted']
  regressions=[k for k in dims if scores['after']['dimensions'][k] < scores['before']['dimensions'][k]-.5]
  recommendation=d.get('recommended_version','BEFORE')
  provisional=('AFTER_POTENTIALLY_BETTER' if delta>=.25 and not regressions and recommendation=='AFTER'
   else 'REJECT_AFTER')
  record={'asset':name,'reviewer':model,'before_sha':sha(before),'after_sha':sha(after),
   'before':scores['before'],'after':scores['after'],'delta':round(delta,3),
   'regressions':regressions,'before_defects':d.get('before_defects',[]),
   'after_defects':d.get('after_defects',[]),'after_regressions':d.get('after_regressions',[]),
   'preferred':recommendation,'decision':provisional,'raw':raw,
   'art_approved':False,'unity_approved':False}
 except Exception as e:
  record={'asset':name,'error':repr(e),'decision':'REVIEW_FAILED','art_approved':False}
 records.append(record)
 (out/'independent_ab12.json').write_text(json.dumps({
  'source_blender_run':38819524063,'reviewer_model':model,'no_auto_approval':True,
  'target_minimum':8,'all_art_approved':False,'results':records},indent=2,ensure_ascii=False))
 print('QWEN_INDEPENDENT_AB',i,'/12',name,record.get('delta','error'),record['decision'],flush=True)
print('QWEN_REVIEWED',len(records),'provisional',sum(x.get('decision')=='AFTER_POTENTIALLY_BETTER' for x in records),flush=True)
if any(x.get('decision')=='REVIEW_FAILED' for x in records):raise RuntimeError('Incomplete independent A/B due to failed evaluations')
