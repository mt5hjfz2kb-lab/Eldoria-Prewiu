#!/usr/bin/env python3
"""Create REAL scene-aware Qwen artistic direction using local Ollama and versioned system role.
Requires two genuine images; it does not execute Blender or certify art by itself.
"""
import argparse,base64,hashlib,json,pathlib,urllib.request
HERE=pathlib.Path(__file__).resolve().parent
ALLOWED={'sculpt_region','reprofile_crest','construct_mesh','assign_material','place_existing_asset'}
def digest(data):return hashlib.sha256(data).hexdigest()
def main():
 p=argparse.ArgumentParser()
 p.add_argument('--reference',type=pathlib.Path,required=True)
 p.add_argument('--unity-before',type=pathlib.Path,required=True)
 p.add_argument('--target-object',required=True)
 p.add_argument('--inventory',type=pathlib.Path)
 p.add_argument('--out',required=True,type=pathlib.Path)
 p.add_argument('--model',default='qwen3-vl:4b-instruct')
 p.add_argument('--endpoint',default='http://127.0.0.1:11434/api/chat')
 a=p.parse_args()
 if not (a.endpoint.startswith('http://localhost:') or a.endpoint.startswith('http://127.0.0.1:')):raise ValueError('Only free local Ollama is permitted')
 inputs=[];hashes={}
 for key,image in [('reference',a.reference),('unity_before',a.unity_before)]:
  if not image.is_file() or image.suffix.lower() not in ('.png','.jpg','.jpeg'):raise ValueError('Genuine image missing: '+str(image))
  raw=image.read_bytes()
  if len(raw)>10_000_000:raise ValueError('Input image too large')
  hashes[key]=digest(raw);inputs.append(base64.b64encode(raw).decode())
 role=(HERE/'ARTIST_SYSTEM.md').read_bytes()
 inventory=a.inventory.read_text('utf8')[:6000] if a.inventory else 'Inspect original eight sources, Hero V3 and existing licensed vegetation.'
 prompt=('Compare left REFERENCE and right GENUINE UNITY screenshot. Diagnose and DIRECT NEW 3D geometry, '
  'not just noise parameters. Valid action parameter formats: '
  'sculpt_region center [x,y,z], radius, delta [x,y,z]; '
  'reprofile_crest axis x|y, height_axis y|z, control_points [[pos,height],...], band_min, band_max; '
  'construct_mesh vertices [[x,y,z]...], faces [[indices]...], material_color [r,g,b]; '
  'assign_material rgb [r,g,b], roughness; '
  'place_existing_asset source_object, location [x,y,z], rotation [x,y,z], scale [x,y,z]. '
  f'Only target these actual objects: {a.target_object}. Current inventory: {inventory}. '
  'Output strictly JSON; model must choose artist-directed geometric edit if structural deficit.')
 payload={'model':a.model,'stream':False,'think':False,'format':'json',
  'options':{'temperature':0.25,'num_predict':1800,'num_ctx':4096},
  'messages':[{'role':'system','content':role.decode('utf8')},
              {'role':'user','content':prompt,'images':inputs}]}
 request=urllib.request.Request(a.endpoint,data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(request,timeout=540) as response:reply=json.load(response)
 design=json.loads(reply['message']['content'])
 if len(design.get('observed_defect',''))<35 or len(design.get('design_intent',''))<35:raise ValueError('Insufficient artist diagnosis')
 if design.get('asset_decision') not in ['REUSE','MODIFY','NEW']:raise ValueError('Reuse-vs-new decision missing')
 ops=design.get('planned_actions')
 if not isinstance(ops,list) or not 1<=len(ops)<=8:raise ValueError('1-8 authoring operations required')
 ids=set()
 for op in ops:
  if op.get('type') not in ALLOWED or not op.get('id') or op['id'] in ids:raise ValueError('Invalid/duplicate artist action')
  if op.get('target')!=a.target_object:raise ValueError('Unauthorized Blender target')
  if not isinstance(op.get('parameters'),dict) or len(op.get('rationale',''))<15:raise ValueError('Unspecified modelling intent')
  ids.add(op['id'])
 a.out.mkdir(parents=True,exist_ok=True)
 evidence={'model':a.model,'system_role':'system','system_prompt_sha256':digest(role),'reference_sha256':hashes['reference'],
  'unity_before_sha256':hashes['unity_before'],'target_object':a.target_object,'direction':design,'self_approval':False}
 (a.out/'artist-inference.json').write_text(json.dumps(evidence,indent=2)+'\n','utf8')
 (a.out/'artist-plan.json').write_text(json.dumps(ops,indent=2)+'\n','utf8')
 print('REAL_LOCAL_ARTIST_DIRECTION_RECORDED',len(ops),'actions')
if __name__=='__main__':main()
