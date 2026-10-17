"""Real Qwen 8B independent comparison of preserved V7 and native Unity V9.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v9'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V9_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V9','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v9/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v9/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('Four TRUE images: approved artistic reference; saved Unity V7 with the approved AFTER light; '
 'V7 immediately before V9 import from the same scene/camera; V9 in Unity with the SAME light, lens and viewport. '
 'Judge whether V9 terrain-native diffuse blending creates connected organic roads over the hidden beige source road renderers (source colliders preserved); '
 'whether earth-based side tracks and the lower apron are visible, connected and do not look like painted stripes; color and shape of river; protected bridge, tower, paved stairs and terrain. '
 'Look for new road texture alignment, inverted terrain faces, clipping at original routes, tile-like procedural repetition and unrelated improvements. '
 'Original bridge geometry and colliders were intentionally not moved; never assert physical collision PASS from images. '
 'Score V7 and V9 on each weighted dimension: geometry/silhouette 25%, architectural proportion/contact 25%, '
 'materials/detail 20%, Valoria style 20%, gameplay-camera readability 10%, with 0-10 scores and image-grounded observations. '
 'Return strict JSON with verdict (LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT, ART_REJECTED), '
 'scores_v7, scores_v9, positives, regressions, outstanding_defects, next_visual_edit. '
 'Do not grade or falsely approve the large GREYBOX upper bastion: it has NOT changed. '
 'No numeric claims about invisible colliders or physical mobile FPS. No automatic ART_APPROVED. '
 'Known lessons: V6 inverted terrain normal was missed by earlier model; V7 solved it. '
 'Source path and camera are identical across Unity screenshots. Memory: '+memory)
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
print('VALORIA_V9_REVIEW_COMPLETE',json.dumps(result))
