"""Source-phase adapter to the existing artist API. Blender images are NEVER Unity evidence."""
import argparse,base64,hashlib,json,pathlib,sys,time,urllib.request
ROOT=pathlib.Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'tools/art-direction/ai-authoring'))
from artist_plan_validation import inspect_artist_plan,bounded_repair_feedback,camera_footprint_errors
from author_brief import cpu_artist_schema
def sha(b):return hashlib.sha256(b).hexdigest()
p=argparse.ArgumentParser();p.add_argument('--cycle',type=pathlib.Path,required=True)
p.add_argument('--model',default='qwen3-vl:8b-instruct');a=p.parse_args();c=a.cycle
target='ZoneOne continuous prairie source'
top=json.loads((c/'context/mesh-context.json').read_text())
role=(ROOT/'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md').read_bytes()
lessons=(ROOT/'docs/world4x-zone1/evidence/local-cycle-20261014/independent-final-source-review.json').read_text()
reference=ROOT/'docs/world4x-zone1/evidence/local-cycle-20261014/references/local-reference.png'
images=[reference,c/'source/blender-before.png'];encoded=[];provenance=[]
from PIL import Image
for i,path in enumerate(images):
 im=Image.open(path).convert('RGB');im.thumbnail((640,640));dest=c/f'vision-{i}.jpg';im.save(dest,quality=88)
 encoded.append(base64.b64encode(dest.read_bytes()).decode());provenance.append({'original':str(path),'sha256':sha(path.read_bytes()),'derived_sha256':sha(dest.read_bytes())})
prompt=('Current task is ZONA ONE ground-only source authoring, not Valoria architecture. '
 'Image 1 is owner destination. Image 2 is an ACTUAL BLENDER SOURCE RENDER, NOT UNITY; there is no Unity capture yet. '
 'User requires ground quality before props or scaling. Diagnose natural prairie relief rather than decorating the scene. '
 'Keep all other assets untouched and hidden. Design 2..4 broad, asymmetrical gentle landforms, connected valleys and open buildable prairie. '
 'Avoid isolated circular mounds, rigid waves, terraces and mountains. No purple paint. '
 'Choose your own exact local coordinates from topology, not indices. Use ONLY sculpt_region or sculpt_path. '
 'Only vertical delta [0,0,dz], magnitude 2..9m, radius 16..35m. Preserve outer boundary x/y +/-120m: centers and path corridors must stay more than radius+2m inside it. '
 'Protect the existing city ground rectangle x=-40..4,y=-52..-8: do not let sculpt footprints touch it. '
 'Use slopes suitable for armies, wide flat areas between hills, not a noisy bumpy surface. '
 'At least one broad camera-visible change. No approval or claimed execution. '
 'REAL MESH TOPOLOGY: '+json.dumps(top)+'\nREAL PRIOR REVIEW / RETRIEVED EXPERIENCE, NOT MODEL TRAINING: '+lessons)
schema=cpu_artist_schema(target,'existing',4);schema['properties']['planned_actions']['items']['properties']['type']['enum']=['sculpt_region','sculpt_path']
payload={'model':a.model,'stream':False,'think':False,'format':schema,'options':{'num_ctx':8192,'num_predict':1800,'temperature':.3},'messages':[{'role':'system','content':role.decode()},{'role':'user','content':prompt,'images':encoded}]}
(c/'actual-request.json').write_text(json.dumps(payload,indent=2))
attempts=[]
for attempt in range(3):
 start=time.monotonic()
 req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=540) as response:reply=json.load(response)
 raw=reply['message']['content'];(c/f'raw-qwen-{attempt+1}.json').write_text(json.dumps(reply,indent=2))
 errors=[]
 try:
  design=json.loads(raw);errors=inspect_artist_plan(design,target,'existing',4,top,True,True)+camera_footprint_errors(design,16,2)
  for op in design.get('planned_actions',[]):
   pr=op['parameters'];knots=pr.get('knots',[]) if op['type']=='sculpt_path' else [pr]
   if op['type'] not in ('sculpt_region','sculpt_path'):errors.append('Only local vertical terrain sculpt permitted')
   for k in knots:
    xyz=k.get('center',[]);delta=k.get('delta',[]);r=k.get('radius',0)
    if len(xyz)!=3 or len(delta)!=3:continue
    if delta[:2]!=[0,0] or not 2<=abs(delta[2])<=9 or not 16<=r<=35:errors.append('Respect vertical delta 2..9m and radius16..35m')
    x,y=xyz[:2]
    if abs(x)+r>=118 or abs(y)+r>=118:errors.append('Scaffold boundary must stay unchanged')
    if -40-r<x<4+r and -52-r<y<-8+r:errors.append('Footprint overlaps protected city pad')
 except (ValueError,KeyError,TypeError) as e:errors.append(str(e));design=None
 attempts.append({'attempt':attempt+1,'seconds':round(time.monotonic()-start,2),'errors':errors,'response_sha256':sha(raw.encode()),'eval_count':reply.get('eval_count')})
 (c/'artist-attempts.json').write_text(json.dumps(attempts,indent=2))
 print('REAL_QWEN_GROUND_ATTEMPT',attempt+1,errors,flush=True)
 if not errors:
  (c/'artist-plan.json').write_text(json.dumps(design['planned_actions'],indent=2))
  (c/'artist-inference.json').write_text(json.dumps({'author':'QWEN_LOCAL','model':a.model,'image_type':'BLENDER_SOURCE_NOT_UNITY','system_prompt_sha256':sha(role),'images':provenance,'direction':design,'attempts':attempts,'model_weights_trained':False,'self_approval':False},indent=2));break
 if attempt==2:raise RuntimeError('Three authentic Qwen decisions rejected; no substitute geometry executed')
 payload['messages']+=[{'role':'assistant','content':raw},{'role':'user','content':bounded_repair_feedback(errors)}]
