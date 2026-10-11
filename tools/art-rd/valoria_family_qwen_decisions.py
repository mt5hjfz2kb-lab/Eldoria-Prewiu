"""Reliable visual-art planning with EXISTING local Ollama/Qwen brain.
Uses real V3 reference and actual individual/strategic Blender renders.
Retains each raw model reply; no fabricated plan, no automatic ART PASS.
"""
import json,base64,io,urllib.request,time
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/first-asset-family-v1'
REF=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3/after-hero-matched.png'
assert REF.exists(),'Preserved V3 rendered reference required'
ALLOWED={
 'wall_straight':['break_ashlar_cadence','soften_buttress_arrises'],
 'watchtower':['taper_crown_silhouette','weather_radial_ashlar'],
 'wall_corner':['dress_miter_join','soften_corner_coping'],
 'wall_connections':['taper_socket_contacts','chip_dressed_edges'],
 'stone_path_platform':['sculpt_stone_treads','disrupt_paving_repetition'],
 'cliff_rocks':['sculpt_geological_shelves','carve_erosion_channels','break_cliff_silhouette'],
 'defensive_props':['relax_banner_silhouette','weather_defensive_stakes'],
 'assembled_family':['none']}
def encoded(path):
 with Image.open(path) as pic:
  pic=pic.convert('RGB');pic.thumbnail((448,448))
  buf=io.BytesIO();pic.save(buf,format='JPEG',quality=74)
  return base64.b64encode(buf.getvalue()).decode()
def parse(reply):
 # raw_decode tolerates incidental suffix without inventing data.
 st=reply.find('{')
 if st<0:raise ValueError('No JSON object')
 return json.JSONDecoder().raw_decode(reply[st:])[0]
def ask(prompt,images,choices):
 schema={'type':'object','properties':{
  'observation':{'type':'string'},
  'geometry_decision':{'type':'string'},
  'art_operation':{'type':'string','enum':choices},
  'masonry_irregularity':{'type':'number','minimum':.008,'maximum':.07}},
  'required':['observation','geometry_decision','art_operation','masonry_irregularity'],
  'additionalProperties':False}
 payload={'model':'qwen2.5vl:3b','stream':False,'think':False,
  'format':schema,'options':{'num_predict':310,'num_ctx':8192,'temperature':0.0},
  'messages':[{'role':'user','content':prompt,'images':images}]}
 req=urllib.request.Request('http://127.0.0.1:11434/api/chat',
  data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=550) as response:
  data=json.load(response)
 raw=data.get('message',{}).get('content','')
 if not raw.strip():raise ValueError('Empty Qwen response')
 return raw,parse(raw)
def valid(plan,choices):
 if not isinstance(plan,dict):return False
 if plan.get('art_operation') not in choices:return False
 if not str(plan.get('observation','')).strip():return False
 if not str(plan.get('geometry_decision','')).strip():return False
 try:return .008<=float(plan['masonry_irregularity'])<=.07
 except (KeyError,ValueError,TypeError):return False
base64ref=encoded(REF)
decisions={}
traces={}
for name,choices in ALLOWED.items():
 img=[base64ref,encoded(OUT/name/'before-hero.png'),encoded(OUT/name/'before-strategic.png')]
 prompt=(
  'You are the ACTING visual 3D art director for medieval mobile game Eldoria. '
  'Image 1 is the frozen Hero V3 reference: quality MINIMUM, never edit. '
  'Image 2 is the '+name+' candidate close-up, image 3 strategic camera. '
  'Compare architecture, worn edges, stone realism, geological credibility, and mobile readability. '
  'The candidate was explicitly rejected as prototype quality. '
  'Choose ONE concrete mesh authoring intervention from '+', '.join(choices)+'. '
  'Explain a visibly grounded flaw and the corrective Blender operation, not just color/noise. '
  'Return JSON with observation, geometry_decision, art_operation, masonry_irregularity '
  '(0.008 to 0.07). No ART PASS or claim that a change has already been made.'
 )
 traces[name]=[]
 plan=None
 for attempt in range(2):
  try:
   raw,p=ask(prompt+(' Your previous output did not validate. Follow JSON keys/types exactly.' if attempt else ''),img,choices)
   traces[name].append({'attempt':attempt+1,'raw_model_response':raw,'parsed':p})
   if valid(p,choices):plan=p;break
  except Exception as exc:
   traces[name].append({'attempt':attempt+1,'error':str(exc)})
 if plan is None:
  (OUT/'qwen-inference-trace.json').write_text(json.dumps(traces,indent=2))
  raise RuntimeError('No valid Qwen architectural authoring decision for '+name+' after two attempts; raw evidence retained')
 decisions[name]=plan
 (OUT/'agent-decisions.json').write_text(json.dumps(decisions,indent=2))
 (OUT/'qwen-inference-trace.json').write_text(json.dumps(traces,indent=2))
 print('ELDORIA_QWEN_ARTIST_DECISION',name,json.dumps(plan),flush=True)
assert len(decisions)==len(ALLOWED)
print('ELDORIA_QWEN_EIGHT_ART_DECISIONS_VERIFIED')
