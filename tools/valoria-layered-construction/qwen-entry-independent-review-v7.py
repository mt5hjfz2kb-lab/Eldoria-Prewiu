"""One bounded read-only review through the already installed local Ollama endpoint.
No model downloads, agents, geometry execution or automatic art approval.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd();ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7';ev.mkdir(exist_ok=True,parents=True)
sha=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'EXTERNAL_MODEL_REVIEW_OF_GPT_AUTHORED_GEOMETRY_NOT_SOLE_ART_AUTHORITY','new_models_downloaded':False,'automatic_art_approval':False}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10));installed=[m['name'] for m in tags['models']]
 model=next(m for m in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if m in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg','docs/evidence/valoria-layered-construction-20261016/entry-zone-v5/entry-AFTER-home.png','docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/entry-AFTER-home.png','docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2500]
 prompt=('Review four real images in order: approved reference, best prior Unity V5, V7 geometry with SAME old sun, V7 with ONLY the sun direction corrected. '
 'You did not author this geometry: GPT authored it. Be a critical independent reviewer. '
 'Evaluate only what is actually visible: bank shapes, silhouette, material detail, bridge/gate contact, road and stair readability. '
 'No numeric ratings, no inferred gameplay or performance certification. Compare A/B and reference. '
 'Return JSON with fields verdict (ART_REJECTED or LOCAL_ART_IMPROVEMENT or NO_VISIBLE_IMPROVEMENT), visible_improvements, remaining_defects, protected_regressions, next_bounded_edit. '
 'A green workflow is not art approval. Prior Qwen failed to notice V6 terrain normals pointed down (-0.933 mean): do not repeat that mistaken local approval. V7 source asserts upward normals. Bridge footings were detached due to unevaluated Blender matrices; V7 evaluates transforms and checks real bounds. Gate arch/tower source geometry and textures are unchanged; do not invent improvements in those protected meshes. Compare lighting separately from geometry. Do not prescribe a workflow or new module; identify a visible defect and concrete bounded edit only. '
 'Unchanged upper Bastion is still a greybox, do not mistake it for final architecture. '
 'Existing error memory follows: '+memory)
 (ev/'independent-review-input.json').write_text(json.dumps({'model':model,'prompt':prompt,'memory_sha256':sha(memory.encode()),'images':[{ 'path':p,'sha256':sha(raw)} for p,raw in zip(paths,images)]},indent=2),'utf8')
 payload={'model':model,'stream':False,'format':'json','messages':[{'role':'system','content':'You are an independent visual critic. Ground every claim in the attached images. Do not invent scores or certify untested properties.'},{'role':'user','content':prompt,'images':[base64.b64encode(raw).decode() for raw in images]}],'options':{'temperature':.1,'num_ctx':8192,'num_predict':700},'keep_alive':'0s'}
 start=time.monotonic();req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 response=json.load(urllib.request.urlopen(req,timeout=480));(ev/'independent-review-raw.json').write_text(json.dumps(response,indent=2),'utf8')
 result.update(model=model,status='REAL_LOCAL_MODEL_CRITIQUE_RECORDED',elapsed_seconds=round(time.monotonic()-start,3),review=response['message']['content'])
except Exception as e:result.update(status='REVIEW_UNAVAILABLE_NOT_ART_PASS',error=str(e))
(ev/'independent-review-status.json').write_text(json.dumps(result,indent=2),'utf8');print(json.dumps(result))
