"""Read-only Qwen3-VL independent visual review of 63 native Blender modules.
Only uses locally installed model and Blender-made PNG. Zero downloads/payment.
Outputs advisory scores, NEVER ART_APPROVED.
"""
import sys,json,base64,urllib.request,urllib.error,os,time,re,hashlib
from pathlib import Path
args=sys.argv[1:];source=Path(args[0]);output=Path(args[1]);model=args[2]
output.mkdir(parents=True,exist_ok=True)
m=json.loads((source/'individual_quality_status.json').read_text())
reference=("Valoria Eldoria reference: premium stylized medieval fortress on natural alpine "
 "rock plateau; warm weathered hand-cut limestone with softly worn corners, "
 "credible masonry and iron joinery, aged oak, deep ultramarine embroidered banners, "
 "dense northern fir/pines with layered boughs, natural grey geology, cinematic warm "
 "fire at blue dusk. A SIMPLE repeatable stone may be small but still needs crafted "
 "surface and contact quality. No studio floor or castle composite is in scope.")
headers={"Content-Type":"application/json"}
def call(method,api,data=None,timeout=120):
 body=json.dumps(data).encode() if data is not None else None
 req=urllib.request.Request('http://127.0.0.1:11434'+api,body,headers=headers,method=method)
 with urllib.request.urlopen(req,timeout=timeout) as res:return json.loads(res.read())
models=call('GET','/api/tags',timeout=10)
available=[q.get('name','') for q in models.get('models',[])]
if model not in available:
 raise SystemExit('No installed local model '+model+'; installed='+str(available))
records=[]
for i,a in enumerate(m['assets']):
 name=a['asset']
 views=[source/'previews'/(name+'_front.png'),source/'previews'/(name+'_back.png')]
 try:
  images=[base64.b64encode(p.read_bytes()).decode('ascii') for p in views]
  prompt=("Independent art-quality audit. No self-approval. Assess this SINGLE 3D asset "
   "using BOTH authentic Blender camera images. The asset name is "+name+". "
   +reference+" Give JSON only with keys score_geometry, score_contacts, "
   "score_materials, score_style, score_readability (each 0..10); "
   "weighted_score (0.25/0.25/0.20/0.20/0.10); "
   "defects (array with 1-3 SPECIFIC visible defects); "
   "visible_strength (string); decision (REJECT or PROVISIONAL_PASS). "
   "Rate below 8 unless geometry, contacts, materials, style and legibility "
   "are all at professional illustrative-game level. Do not rate implementation details "
   "not visible in images. Single pieces are judged on their own craftsmanship. "
   "A studio preview cannot prove Unity readiness.")
  request={"model":model,"prompt":prompt,"images":images,"stream":False,
   "format":"json","options":{"temperature":0.15,"num_predict":330,"num_ctx":8192}}
  response=call('POST','/api/generate',request,timeout=180)
  raw=response.get('response','')
  try:data=json.loads(raw)
  except Exception:
   text=re.search(r'\{.*\}',raw,re.S)
   data=json.loads(text.group()) if text else {}
  weights=[.25,.25,.20,.20,.10]
  values=[]
  for key in ('score_geometry','score_contacts','score_materials','score_style','score_readability'):
   val=float(data.get(key,-1));values.append(max(0,min(val,10)))
  score=sum(x*w for x,w in zip(values,weights)) if min(values)>=0 else None
  record={'asset':name,'model':model,'raw_response':raw,'scores':dict(zip(
   ('geometry','contacts','materials','style','readability'),values)),
   'weighted_score':score,'defects':data.get('defects',[]),
   'visible_strength':data.get('visible_strength',''),
   'reviewer_claim':'INDEPENDENT_QWEN_ADVISORY_ONLY',
   'visual_decision':('PROVISIONAL_PASS' if score is not None and score>=8 and min(values)>=6.5 else 'REJECT'),
   'art_approved':False,'unity_approved':False}
 except Exception as e:
  record={'asset':name,'visual_decision':'REVIEW_ERROR','error':repr(e),'art_approved':False}
 records.append(record)
 (output/'review.json').write_text(json.dumps({'source':'NATIVE_BLENDER_PER_ASSET_63',
  'model':model,'author':'GPT6_DIRECTED_BLENDER; reviewer Qwen3-VL',
  'count':len(records),'all_art_approved':False,'results':records},indent=2,ensure_ascii=False))
 print('INDEPENDENT_VISUAL',i+1,'/63',name,record.get('weighted_score'),record['visual_decision'],flush=True)
print('REVIEW_COMPLETE',len(records),sum(r.get('visual_decision')=='PROVISIONAL_PASS' for r in records),flush=True)
