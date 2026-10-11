"""Existing local art brain: vision -> multi-operation authored design -> Blender.
No binary-choice scalar deformation. Inference inputs and raw replies are retained.
"""
import base64, hashlib, io, json, os, time, urllib.request
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/asset-family-v2'
GATE=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3'
TARGET=Path(os.environ.get('ELDORIA_FAMILY_TARGET',str(ROOT/'references/VALORIA_FAMILY_OWNER_TARGET_20261011.jpg')))
BRIEFS={
 'wall_straight':'8m wall. Real bonded courses, segmented coping, parapet walkway, tapered buttresses. Arrow loops cut into masonry, not painted over it.',
 'watchtower':'Restore EXACT right V3 tower .blend with structural core, foot, machicolations, coping and crenels. Add coherent secondary detail. No global deformation or baked-coordinate sinusoid.',
 'wall_corner':'Two 4m arms at true 90 degrees. Bond stones around corner, continuous walkway, no overlapping ends. Same stone size/crown as wall.',
 'wall_connections':'2m wall socket with dressed end masonry and tapered pier. Contacts x=+-1m, same wall heights/stone sizes.',
 'stone_path_platform':'4m x 8m continuous road: closely fitted varied rectangular flagstones, buried earth support. No circular plinth, stepping-stone islands or floating paving. Road z=0; relief under 0.025m.',
 'cliff_rocks':'12m linear modular escarpment, not round mesa. Broad leaning angular fault planes, vertical fissures, large/medium/small hierarchy, rock talus and earth/grass crest. Crown z=0, exposed face 3.5m, closed base. Geometry and portable UV/textures carry geology.',
 'defensive_props':'Complete V3 blue banner WITH gold embroidery/bar and brazier WITH bowl, pedestal, cap/flames. Separate modular mounts, wooden braces/iron fastenings. Real physical attachments.'}
OPS=['bonded_masonry','segmented_coping','parapet_walkway','tapered_buttresses','recessed_arrow_loops','v3_complete_tower','v3_complete_heraldry','dressed_corner_bond','modular_socket','fitted_flagstone_road','buried_earth_support','fault_plane_escarpment','vertical_fissures','embedded_talus','earth_grass_crest','v3_complete_braziers','oak_iron_bracing']
def visual_board(paths,asset,attempt):
 # One bounded vision input avoids pathological multi-image prefill on CPU.
 # The three original images remain separately hashed in the trace.
 width,height=(504,224) if attempt==0 else (336,168)
 board=Image.new('RGB',(width,height),(40,46,54));draw=ImageDraw.Draw(board)
 for i,path in enumerate(paths):
  with Image.open(path) as im:
   im=ImageOps.contain(im.convert('RGB'),(width//3-4,height-24))
   board.paste(im,(i*width//3+(width//3-im.width)//2,24+(height-24-im.height)//2))
  draw.text((i*width//3+5,6),['1 TARGET','2 V3 MINIMUM','3 REJECTED'][i],fill=(240,240,240))
 path=OUT/(asset+'-artist-input-'+str(attempt)+'.jpg');board.save(path,quality=88)
 return base64.b64encode(path.read_bytes()).decode(),path
schema={'type':'object','properties':{
 'diagnosis':{'type':'string'},'design_intent':{'type':'string'},
 'operations':{'type':'array','items':{'type':'string','enum':OPS},'minItems':2,'maxItems':8},
 'stone_height':{'type':'number','minimum':.27,'maximum':.38},
 'stone_length':{'type':'number','minimum':.65,'maximum':1.1},
 'buttress_projection':{'type':'number','minimum':.25,'maximum':.65},
 'loop_count':{'type':'integer','minimum':1,'maximum':3},
 'road_slab_length':{'type':'number','minimum':.45,'maximum':.8},
 'fault_lean':{'type':'number','minimum':.35,'maximum':.9},
 'fracture_depth':{'type':'number','minimum':.18,'maximum':.55},
 'talus_clusters':{'type':'integer','minimum':3,'maximum':7},
 'crest_soil_depth':{'type':'number','minimum':.12,'maximum':.3},
 'material_intent':{'type':'string'},'composition_notes':{'type':'string'}},'additionalProperties':False}
schema['required']=list(schema['properties'])
OUT.mkdir(parents=True,exist_ok=True);plans={};traces={}
for asset,brief in BRIEFS.items():
 current=OUT/asset/'v2-1-hero.png'
 if asset=='watchtower' and (OUT/asset/'v2-2-hero.png').exists():current=OUT/asset/'v2-2-hero.png'
 inputs=[TARGET,GATE/'after-hero-matched.png',current]
 assert all(p.exists() for p in inputs),inputs
 prompt=('You are external 3D ARTIST of Eldoria commanding actual Blender mesh construction. Single comparison board, left target Valoria 9.5/10; middle immutable Hero V3 MINIMUM; right rejected '+asset+'. '
  'Plan a MAJOR coherent reconstruction, not small jitter. Brief: '+brief+' '
  'Give specific visible diagnosis, your authored design, MULTIPLE relevant workshop operations, numerical proportions and composition notes. Bound values are engineering limits; YOU choose values. '
  'Each text field must be one concise sentence, max 25 words. Material intent: explain belonging to V3 limestone, blue/gold heraldry, natural earth and grey angular geology. Never approve yourself. Required JSON only.')
 payload={'model':'qwen2.5vl:3b','stream':True,'think':False,'keep_alive':'10m','format':schema,'options':{'temperature':.15,'num_predict':800,'num_ctx':2048,'num_thread':2},'messages':[{'role':'user','content':prompt,'images':[]}]}
 trace={'model':payload['model'],'prompt':prompt,'images':[{'path':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in inputs],'attempts':[]}
 for attempt in range(2):
  start=time.monotonic();raw='';events=0;print('EXTERNAL_ARTIST_START',asset,attempt,flush=True)
  encoded,board=visual_board(inputs,asset,attempt);payload['messages'][0]['images']=[encoded]
  trace['vision_board']={'path':str(board),'sha256':hashlib.sha256(board.read_bytes()).hexdigest(),'size':Image.open(board).size}
  try:
   req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
   with urllib.request.urlopen(req,timeout=240) as resp:
    for line in resp:
     event=json.loads(line);events+=1
     if event.get('error'):raise RuntimeError(event['error'])
     raw+=event.get('message',{}).get('content','')
     if events%40==0:print('EXTERNAL_ARTIST_STREAM',asset,events,round(time.monotonic()-start,1),flush=True)
     if time.monotonic()-start>360:raise TimeoutError('Bounded streamed artistic generation exceeded 360s')
     if event.get('done'):break
   plan=json.JSONDecoder().raw_decode(raw[raw.index('{'):])[0]
   assert set(schema['required'])<=set(plan)
   assert len(plan['diagnosis'])>25 and len(plan['design_intent'])>25
   assert len(plan['operations'])>=2 and all(x in OPS for x in plan['operations'])
   for field,spec in schema['properties'].items():
    if spec['type'] in ('number','integer'):assert spec['minimum']<=plan[field]<=spec['maximum'],(field,plan[field])
   trace['attempts'].append({'raw_response':raw,'elapsed_seconds':time.monotonic()-start,'valid':True,'stream_events':events,'completion_metrics':{k:event.get(k) for k in ('eval_count','eval_duration','prompt_eval_count','prompt_eval_duration','done_reason')}});plans[asset]=plan;break
  except Exception as e:
   trace['attempts'].append({'error':str(e),'raw_response':locals().get('raw',''),'valid':False})
   if attempt==1:raise
   # Cancel timed-out inference before a materially smaller board retry.
   import subprocess
   subprocess.run(['ollama','stop','qwen2.5vl:3b'],check=False)
  finally:
   traces[asset]=trace;(OUT/'qwen-family-quality-traces.json').write_text(json.dumps(traces,indent=2))
 (OUT/'qwen-family-quality-directions.json').write_text(json.dumps({'plans':plans,'self_approval':False},indent=2))
 print('EXTERNAL_ARTIST_DESIGN',asset,plan['operations'],flush=True)
assert len(plans)==7
