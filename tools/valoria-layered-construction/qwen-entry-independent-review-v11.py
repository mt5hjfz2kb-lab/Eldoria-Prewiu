"""Real Qwen 8B independent comparison of preserved V7 and native Unity V11.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v11'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V11_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V11','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v11/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v11/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('Four genuine Unity/reference images: approved Eldoria reference, original immutable V7 as saved with AFTER light, '
 'before-V7 in current controlled render conditions, and CC0-photo-bank V11 in the exact same Unity scene camera and light. '
 'Judge ONLY the V11 intervention around the riverbank geology and water: real archived Poly Haven CC0 scan diffuse and displacement, '
 'physical bank sculpt outside protected 5.5-meter bridge contact throat, muted water shader and micro ripples, and removal of beige greybox road visual surfaces; '
 'do the bank rocks read less yellow and more naturally detailed? Is water less cyan/flat? Are bridge footings, gate wings, stairs, and near/far shore coherent? '
 'V8 stone scatter, V9 ground stripes and V10 painted branching roads were rejected, V11 does NOT reproduce them. '
 'Never assert normal inversion or collider failure without geometric evidence; normals have an upward quantitative gate and colliders are tested in Unity. '
 'Evaluate visual before/after only with physically comparable camera, resolution and light. '
 'Do not claim that the grey upper fortress is final; it remains an unchanged incomplete greybox. '
 'Give weighted 0-10 dimensions geometry 25%, architectural contact 25%, materials 20%, style 20%, gameplay readability 10%. '
 'Return valid JSON verdict (LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT, ART_REJECTED), scores_v7, scores_v11, positives, '
 'regressions, outstanding_defects, next_visual_edit, and uncertainty. '
 'No automatic ART_APPROVED, no invented mobile FPS. Remember earlier V6 normal failure and V7 verified correction. Memory: '+memory)
 (ev/'independent-review-input.json').write_text(json.dumps({'model':model,'prompt_sha256':digest(prompt.encode()),'images':[{'path':p,'sha256':digest(raw)} for p,raw in zip(paths,images)],'memory_sha256':digest(memory.encode())},indent=2),'utf8')
 request={'model':model,'stream':False,'format':'json','messages':[{'role':'system','content':'Act as an independent critical 3D art reviewer; ground all claims in visible images, do not approve unseen functionality.'},{'role':'user','content':prompt,'images':[base64.b64encode(x).decode() for x in images]}],'options':{'temperature':.1,'num_ctx':8192,'num_predict':1000},'keep_alive':'0s'}
 started=time.monotonic()
 raw=json.load(urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(request).encode(),headers={'Content-Type':'application/json'}),timeout=480))
 (ev/'independent-review-raw.json').write_text(json.dumps(raw,indent=2),'utf8')
 output=json.loads(raw['message']['content'])
 result.update(status='REAL_LOCAL_MODEL_CRITIQUE_RECORDED',model=model,elapsed_seconds=round(time.monotonic()-started,2),review=output)
except Exception as exc:
 result.update(status='REVIEW_UNAVAILABLE_NOT_ART_PASS',error=str(exc))
(ev/'independent-review-status.json').write_text(json.dumps(result,indent=2),'utf8')
print('VALORIA_V11_REVIEW_COMPLETE',json.dumps(result))
