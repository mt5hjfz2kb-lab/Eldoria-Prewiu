"""Real Qwen 8B independent comparison of preserved V7 and native Unity V16.
Its opinion is evidence, not auto-approval; do not conflate technical PASS with ART.
"""
import base64,hashlib,json,pathlib,time,urllib.request
root=pathlib.Path.cwd()
ev=root/'docs/evidence/valoria-layered-construction-20261016/entry-zone-v16'
ev.mkdir(parents=True,exist_ok=True)
digest=lambda raw:hashlib.sha256(raw).hexdigest()
result={'role':'INDEPENDENT_QWEN_EVALUATION_OF_GPT_V16_AUTHORING','automatic_art_approval':False,'new_models_downloaded':False,'candidate':'V16','baseline':'V7'}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags['models']]
 model=next(name for name in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if name in installed)
 paths=['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/light-AFTER-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v16/entry-BEFORE-home.png',
 'docs/evidence/valoria-layered-construction-20261016/entry-zone-v16/light-AFTER-home.png']
 images=[(root/p).read_bytes() for p in paths]
 memory=(root/'tools/art-direction/ARTIST_LEARNING_JOURNAL.md').read_text('utf8')[:2600]
 prompt=('Matched ORIGINAL V12 BEFORE vs V16 AFTER real Unity captures from the native player camera and approved Eldoria Valoria reference. '
 'Only V16 material finishing changed, no objects, camera, lighting, geometric forms or terrain sculpt: licensed CC0 Poly Haven photo-scanned diffuse/normal/rough texture '
 'on original gate stone meshes and existing side fortification, with world-meter-projected UVs. Original heraldry, metal and trims preserved. '
 'Judge if stone appears more realistic, consistent with rock cliffs and original reference, not overly noisy, dark, tiled, flat, grey or plastic. '
 'Inspect material seam continuity at gate-wing/flank-wall sockets, color between tower and wall, normal map scale, depth, and home camera readability. '
 'Do not credit geometry or physical footing contacts; those have NOT changed. If color or texture worsens, say ART_REJECTED. '
 'Provide 0-10 scores for geometry 25%, architecture/contact 25%, material/detail 20%, style 20%, readability 10% for both V12 and V16, '
 'with specific visual evidence and weighted improvement >0.25 requirement. '
 'Return strict JSON verdict LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT or ART_REJECTED; scores_v12,scores_v16,positives,regressions,outstanding_defects,next_visual_edit,uncertainty. '
 'No automatic ART_APPROVED, not mobile QA. Memory: '+memory)
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
print('VALORIA_V16_REVIEW_COMPLETE',json.dumps(result))
