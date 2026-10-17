"""Real Qwen 8B independent comparison of preserved V7 and native Unity V15.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v15'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V15_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V15','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v15/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v15/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('You are given real Unity captures of the SAME completed V12 fortification BEFORE and V15 AFTER, matched player camera and daylight, plus approved Eldoria reference. '
 'V15 only sculpts a CONTINUOUS real topographic mesh beneath the existing six wall/tower footprints, raising depressed East-bank grassy rock to meet the real masonry bases '
 'without any extra stones, props, extra tower variants, and without changing architecture, bridge, camera, textures, or the twelve stairs. '
 'Look for changed physical footing contact and cliff shoulder profile, not building count. Does the right/east side wall now sit visibly more convincingly on the natural ground? '
 'Any unwanted artificial raised grass bulges, sawtooth contour, extreme terrain blending, landscape tilt, shader seams, stone-green gap or loss of original rocky cliff? '
 'Do not invent extra geometry or change in camera if images show identical architecture. Do not confuse a subtle edit with an 8/10 full art pass. '
 'Score 0–10 for geometry 25%, architectural contact 25%, materials/UV 20%, Eldoria style 20%, player readability 10%, for BOTH real V12 and V15, '
 'and give concrete references from specific pixels/regions of screenshots. Output JSON verdict LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT or ART_REJECTED; '
 'scores_v12, scores_v15, positives, regressions, outstanding_defects, next_visual_edit, uncertainty. '
 'Automatic art approval is strictly forbidden; gameplay/mobile validation is separate. Memory: '+memory)
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
print('VALORIA_V15_REVIEW_COMPLETE',json.dumps(result))
