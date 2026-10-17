"""Real Qwen 8B independent comparison of preserved V7 and native Unity V13.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v13'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V13_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V13','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v13/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v13/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('Four genuine images: approved Eldoria Valoria reference, frozen V7 in Unity, before V13 source V7 in exactly the same Unity camera, after V13 in exact same player camera and daylight. '
 'V13 reconstructs the previously greybox UPPER BASTION using nine geometrically real source meshes from BastionFamilyV1 preserving their relative forms; source WallFamily lower gate flanks from V12 are also included. '
 'The original lower bridge, lower gate, steps, terrain, source colliders and camera must remain visually and physically intact. '
 'Judge whether the upper fortification is actually present and integrated, visible from the home camera, and improves depth, silhouette and masonry. '
 'Distinguish new upper architecture from the baseline grey blockout, look for obvious floating foundations, inappropriate bright/shaded bands, missing roofs, flat source textures, clipping and scale mismatches. '
 'Do not say imagery is identical if the upper bastion visibly changed. Do not guess collider details from images; gameplay tests are separate. '
 'Scores on five dimensions 0–10 numeric each: geometry/silhouette weight 25%, architectural contact 25%, materials/detail 20%, approved Valoria style 20%, gameplay readability 10%. '
 'Return JSON with verdict LOCAL_ART_IMPROVEMENT or NO_VISIBLE_IMPROVEMENT or ART_REJECTED, scores_v7, scores_v13, positives, regressions, outstanding_defects, next_visual_edit, uncertainty. '
 'No automatic full_frame_art_pass or 8/10 approval without complete independent technical and artistic review. Memory: '+memory)
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
print('VALORIA_V13_REVIEW_COMPLETE',json.dumps(result))
