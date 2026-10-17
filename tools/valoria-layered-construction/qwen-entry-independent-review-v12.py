"""Real Qwen 8B independent comparison of preserved V7 and native Unity V12.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v12'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V12_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V12','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v12/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v12/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('Four genuine images: Eldoria approved art target, frozen V7 in Unity, before V12 in exactly the same Unity camera, after V12 in same camera, viewport, light. '
 'V12 adds ONLY six actual source project WallFamily elements: two lateral wall extensions and four smaller flank towers near the existing gateway at x +/- 10-17. '
 'The original lower gate, its towers, bridge/footings, stairs, terrain, river, vegetation and original camera are immutable; do not credit V12 with improvements to those components. '
 'Judge whether the source-derived masonry and tower silhouettes meaningfully improve the ENTRANCE architectural composition and materials and visibly join original wings. '
 'Critique fortress feeling, contact with ground, mismatched roof heights, texture scale, inaccurate proportions, floating/overlapping wall joints, and visual legibility from player home viewport. '
 'Never assert source collider or navigation changes from images. Do not score the huge unchanged distant grey fortress as V12 production art. '
 'Give actual numeric 0-10 scores (NOT 0-25 percentage weight) on geometry/silhouette 25%, architecture/contact 25%, materials/detail 20%, Valoria style 20%, readability 10% '
 'for both V7 and V12 with concrete visible image evidence. Return strict JSON with verdict (LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT, ART_REJECTED), scores_v7, scores_v12, '
 'positives, regressions, outstanding_defects, next_visual_edit, uncertainty. No automatic ART_APPROVED; mobile FPS not tested. '
 'V8,V9,V10 were visibly rejected on their roads; V11 CC0 bank details were below visible quality threshold. Memory: '+memory)
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
print('VALORIA_V12_REVIEW_COMPLETE',json.dumps(result))
