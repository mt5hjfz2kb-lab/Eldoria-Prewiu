"""Real Qwen 8B independent comparison of preserved V7 and native Unity V14.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v14'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V14_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V14','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v14/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v14/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('You are reviewing authentic real Unity captures, NOT concept art or a Blender preview. '
 'Compare original V12 lower entry to V14 lower entry at identical camera, lighting, resolution; supplied also original approved Eldoria reference. '
 'V14 does NOT add decorative fortress models, recolor roads or claim to improve upper grey blockout. The only change is professional actual masonry geometry: '
 'three riverbank-facing tapered interlocking stone buttress piers per existing flank wall, and stepped physically BVH-rock-fitted dressed foundations beneath four actual flank towers. '
 'The original gate, bridge, 12 stairs, 7 picking targets, renderer camera and terrain are unchanged. '
 'Judge specifically whether tower footings no longer appear floating, if the buttress course and masonry foundation look physically constructed, '
 'whether silhouette benefits or new objects look like unrelated obelisks, where seams/UV scale, foundation protrusions or angular contacts remain unacceptable. '
 'Do not invent evidence about upper fortification, terrain normals, gameplay, or mobile frame rate. '
 'Provide weighted 0-to-10 NOT percentages for geometry 25%, architecture and material/rock contacts 25%, PBR material/detail 20%, Eldoria style 20%, camera readability 10%. '
 'Return JSON verdict LOCAL_ART_IMPROVEMENT or NO_VISIBLE_IMPROVEMENT or ART_REJECTED, scores_v12 and scores_v14, positives, regressions, outstanding_defects, next_visual_edit, uncertainty. '
 'A green Unity run is not visual approval. Even positive art requires independent comparison and source checks. Memory: '+memory)
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
print('VALORIA_V14_REVIEW_COMPLETE',json.dumps(result))
