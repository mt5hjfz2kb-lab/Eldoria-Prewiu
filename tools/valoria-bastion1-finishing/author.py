"""Genuine local vision decision; retain raw replies before any Blender mutation."""
import argparse,base64,hashlib,json,pathlib,urllib.request,time
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('--root',type=pathlib.Path,required=True);a=p.parse_args();r=a.root
images=[]
for name in ('reference','unity'):
 source=r/(name+'.png');im=Image.open(source).convert('RGB');im.thumbnail((576,432));dst=r/(name+'-vision.jpg');im.save(dst,quality=88)
 images.append(base64.b64encode(dst.read_bytes()).decode())
brief='''You are the genuine 3D environment artist for Eldoria Bastion I. Image 1 is the approved reference, image 2 is the real current Unity scene. Independently diagnose the cliff, grass and palette. Author one finish_existing_terrain tool call, not generic advice. The existing mesh has 12601 vertices, 23296 faces, a broad cliff from z=-21 to -1 and a playable plateau above -1 (Blender z is height). All architecture, road, water, trees, UVs, face material assignments and plateau vertices are immutable. The tool is a controlled rock-fracture brush: displace only stone-face vertices below z=-1 along their horizontal outward normal with a triangle-profile vertical flute, gently modulated by rock layers; optionally use flat stone-face normals. You decide exact artistic parameters BEFORE Blender, grounded in the images. This is a deliberately stronger shape/material correction, not a tiny hidden corner edit. Existing cliff/grass are noisy photographs: you can author RGB multipliers and UV tiling, preserving packed CC0 images. Tint components are linear factors (never baked reference pixels). Grass is too yellow; make plausible olive/green turf without neon saturation. Cliff should read as broken dark gray stone with broad ridges, not moss noise. Do not change source architecture or claim approval. Return ONLY one JSON object with fields diagnosis, rationale, operation="finish_existing_terrain", parameters. Required parameters: fracture_depth_m (0.4..2.8), flute_pitch_m (2..9), layer_pitch_m (2..10), layer_strength (0..0.6), phase_radians (0..6.283), flat_rock_faces (boolean), grass_rgb (3 numbers 0.2..1), cliff_rgb (same), grass_uv_scale (0.25..2), cliff_uv_scale (0.25..2). Every value must be YOUR specific artistic choice, not copied from a canned plan. The new result must undergo real Unity A/B and gameplay QA; you cannot approve yourself.'''
for attempt in range(2):
 payload={'model':'qwen3-vl:8b-instruct','stream':False,'format':'json','keep_alive':'5m','messages':[{'role':'user','content':brief,'images':images}],'options':{'num_ctx':8192,'num_predict':850,'temperature':0.2}}
 start=time.monotonic();req=urllib.request.Request('http://127.0.0.1:11434/api/chat',json.dumps(payload).encode(),{'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=540) as res: raw=res.read()
 (r/f'raw-attempt-{attempt+1}.json').write_bytes(raw)
 result=json.loads(raw);text=result['message']['content']
 try:
  plan=json.loads(text);assert plan['operation']=='finish_existing_terrain'
  assert len(plan['diagnosis'])>30 and len(plan['rationale'])>40
  q=plan['parameters'];bounds={'fracture_depth_m':(.4,2.8),'flute_pitch_m':(2,9),'layer_pitch_m':(2,10),'layer_strength':(0,.6),'phase_radians':(0,6.283),'grass_uv_scale':(.25,2),'cliff_uv_scale':(.25,2)}
  assert set(q)==set(bounds)|{'flat_rock_faces','grass_rgb','cliff_rgb'}
  for k,(lo,hi) in bounds.items():assert type(q[k]) in (int,float) and lo<=q[k]<=hi,k
  for k in ('grass_rgb','cliff_rgb'):assert len(q[k])==3 and all(type(x) in (int,float) and .2<=x<=1 for x in q[k]),k
  assert type(q['flat_rock_faces']) is bool
  (r/'artist-plan.json').write_text(text+'\n')
  proof={'model':'qwen3-vl:8b-instruct','raw_sha256':hashlib.sha256(raw).hexdigest(),'plan_sha256':hashlib.sha256((r/'artist-plan.json').read_bytes()).hexdigest(),'elapsed_seconds':time.monotonic()-start,'attempt':attempt+1,'parameters_modified':False,'art_approved':False}
  (r/'inference-proof.json').write_text(json.dumps(proof,indent=2));print('GENUINE_QWEN_FINISH_PLAN',json.dumps(plan));break
 except (AssertionError,KeyError,ValueError) as e:
  (r/f'reject-attempt-{attempt+1}.txt').write_text(str(e));brief+=' Your prior output failed validation: '+str(e)+'. Correct the JSON schema; retain independently chosen values inside the documented bounds.'
else:raise RuntimeError('Both genuine artist replies failed; no fabricated fallback')
