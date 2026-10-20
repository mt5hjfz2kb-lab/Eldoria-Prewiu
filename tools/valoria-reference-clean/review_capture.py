"""Adapt the existing local Qwen capture-review method; auxiliary, never an art gate."""
import bpy,base64,hashlib,json,os,pathlib,tempfile,time,urllib.request
root=pathlib.Path.cwd();ev=root/'docs/evidence/valoria-reference-clean'
result={'role':'AUXILIARY_CRITIQUE_OF_ACTUAL_UNITY_CAPTURE','art_approved':False,'new_model_downloads':0,'source_sha':os.environ.get('GITHUB_SHA')}

# Final production review uses current integrated evidence, never legacy plot captures.
if json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf-8-sig')).get('review_scope')=='production_final':
 import sys
 result['role']='INDEPENDENT_LOCAL_VISUAL_CRITIQUE_NOT_AUTHOR_OR_ACCEPTANCE_GATE'
 result['reviews']=[]
 try:
  tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
  installed=[x['name'] for x in tags.get('models',[])];result['installed_models']=installed
  model=next(x for x in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if x in installed)
  batches=[
   ('city',[root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',ev/'roof-junction226/after-I-landscape.png',ev/'roof-junction226/after-II-landscape.png',ev/'roof-junction226/after-I-portrait.png'],
    'Image1 is the approved artistic reference. Images2,3,4 are authentic Unity captures of the current city: Bastion I landscape, Bastion II landscape and Bastion I portrait. The four empty plots in the reference MUST stay HIDDEN in the game. Do not request their reconstruction. Judge the compact keep, gate, bridge, river, meadow, paths and vegetation. The existing game and progression are fixed; no new districts, camps, monumental mountains, cliffs or Bastion III.'),
   ('world',[ev/'world-contact222/after-forest-valoria-32.png',ev/'world-contact222/after-quarry-valoria-32.png',ev/'world-contact222/after-corrupt-scout-14.png',ev/'world-contact222/after-engendro-valoria-14.png'],
    'All four images are authentic Unity current-world captures: forest regional view, quarry regional view, Corruptos close view and Engendro close view. Judge visible resource identity, natural silhouettes, excavation, ground contact, visual clarity and cohesion. Threats must be represented through restrained Breach corruption, NEVER enemy tents or settlements. The camera framing and gameplay are fixed. Do not invent missing unseen content.')
  ]
  for scope,paths,context in batches:
   identities=[{'path':p.relative_to(root).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths]
   if scope=='city':
    assert identities[0]['sha256']=='86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d'
   images=[]
   with tempfile.TemporaryDirectory(prefix='eldoria-final-review-') as scratch:
    for index,path in enumerate(paths):
     im=bpy.data.images.load(str(path),check_existing=False);w,h=im.size;scale=640/max(w,h);im.scale(max(1,round(w*scale)),max(1,round(h*scale)))
     out=pathlib.Path(scratch)/(str(index)+'.jpg');im.filepath_raw=str(out);im.file_format='JPEG';im.save();bpy.data.images.remove(im);images.append(base64.b64encode(out.read_bytes()).decode())
   prompt=context+' You did not create these assets. Critique the actual visible result independently. Do not assume success because code or tests passed. Give three specific visible strengths and the three highest-impact remaining defects, with a concrete correction for each. Clearly distinguish visible observations from uncertainty. Do not claim performance or mechanics are proven by images. Do not grant ART_APPROVED, certification, or a forced target score. Return valid JSON with keys visible_strengths, highest_impact_defects (observation/correction), visual_quality_estimate_0_to_10, uncertainty. Scores are advisory only.'
   request={'model':model,'stream':False,'format':'json','messages':[{'role':'user','content':prompt,'images':images}],'options':{'temperature':.1,'num_ctx':8192,'num_predict':1100},'keep_alive':'0s'}
   start=time.monotonic();raw=json.load(urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(request).encode(),headers={'Content-Type':'application/json'}),timeout=240))
   result['reviews'].append({'scope':scope,'images':identities,'model':model,'elapsed_seconds':round(time.monotonic()-start,2),'review':json.loads(raw['message']['content'])})
  result['status']='REAL_LOCAL_INDEPENDENT_CRITIQUE_RECORDED'
 except Exception as exc:result.update(status='AUXILIARY_UNAVAILABLE_NOT_AN_ART_PASS',error=str(exc))
 (ev/'production-final-independent-review.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
 print('VALORIA_PRODUCTION_FINAL_QWEN_AUXILIARY',result['status'])
 sys.exit(0)

try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags.get('models',[])];result['installed_models']=installed
 model=next(x for x in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if x in installed)
 request_config=json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf8'));shore_review=request_config.get('review_scope')=='shore';lighting_review=request_config.get('review_scope')=='lighting'
 paths=[ev/'reference-target.jpeg',ev/'playmode-home.png',ev/'shore-zone-before-detail.png',ev/'shore-zone-after-detail.png'] if shore_review else [ev/'reference-target.jpeg',ev/'playmode-home.png',ev/'detail-bastion.png',ev/'detail-gate-bridge.png'];paths=[ev/'reference-target.jpeg',ev/'playmode-home.png',ev/'lighting-before-bastion.png',ev/'lighting-after-bastion.png'] if lighting_review else paths;result['original_images']=[{'path':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths]
 images=[]
 # Small inspection copies avoid the known historical image-context overflow.
 with tempfile.TemporaryDirectory(prefix='valoria-native-review-') as scratch:
  for index,path in enumerate(paths):
   im=bpy.data.images.load(str(path),check_existing=False);im.scale(640,480);out=pathlib.Path(scratch)/(str(index)+'.jpg');im.filepath_raw=str(out);im.file_format='JPEG';im.save();bpy.data.images.remove(im);images.append(base64.b64encode(out.read_bytes()).decode())
 facts=json.loads((ev/'unity-art.json').read_text('utf8'));result['native_scene_facts']={k:facts.get(k) for k in ['source_sha','directional_lights','native_terrain_resolution','triangles','renderers']};source_facts=json.loads((ev/'source-art.json').read_text('utf8'));result['native_scene_facts']['source_roof_triangles']=source_facts.get('blue_slate_triangles');result['native_scene_facts']['source_bridge_geometry']=source_facts.get('bridge_geometry')
 prompt=('Verified native scene facts: '+json.dumps(result['native_scene_facts'])+'. The roof has physically authored individual slate geometry. Do not falsely claim there is no directional light, no native terrain, or no modeled roof tiles; you may criticize their visible appearance and readability. '
 'Image 1 is the owner target. Image 2 is the ACTUAL CURRENT UNITY PLAYER CAMERA. Images 3 and 4 are genuine Unity detail-camera views of that same keep and bridge, not concept art. Critique player-camera legibility, but do not claim geometry is absent when the detail cameras show it. '
 'Review the second image against the first. Its layout must contain one compact main keep, one distinct lower gate, '
 'a real arched river bridge, four empty building plots and sparse broken walls. '
 'Give the THREE highest-impact remaining visual defects and a concrete Blender or Unity correction for each. '
 'Judge composition, architecture proportions, material separation, terrain/shore integration, natural vegetation and lighting. '
 'Do not invent hidden geometry or judge mobile performance from an image. Do not call a technical export an artistic success. '
 'Do not recommend adding more towers or permanent buildings to the empty plots. '
 'Return valid JSON with keys visible_strengths, highest_impact_defects (three items with observation and correction), '
 'reference_similarity_estimate_0_to_10, uncertainty. This estimate is advisory and cannot issue ART_APPROVED or MOBILE_PASS.')
 if shore_review:prompt=('Image1 is the owner reference; image2 is the ACTUAL CURRENT UNITY player view. Images3 and4 are actual same-camera, same-light Unity shore BEFORE and AFTER views, original bank geometry and physically reseated candidate. Image3 is before, image4 after; judge only visible bank contact, slope, exposed soil, rocks and water. Do not claim changes in castle or camera in this pair. The owner requires a clearly visible artistic improvement, not more details or higher pixel counts. Identify any floating foliage/rocks or exposed cuts; uncertainty is allowed. '+prompt.replace('Images 3 and 4 are genuine Unity detail-camera views of that same keep and bridge, not concept art.','Images3 and4 show the same fixed shore camera before/after.'))
 if lighting_review:prompt=('Image1 is owner reference. Image2 actual current Unity player view. Image3 is actual Unity BEFORE lighting/GI at the Bastion; image4 is AFTER, IDENTICAL camera and geometry, intentionally improved lighting. Judge visible depth, stone separation, sun shadow softness and natural daylight versus torch brightness. Do not claim geometry changed or buildings were moved. Return JSON with visible_strengths, highest_impact_defects (3 observations and concrete corrections), reference_similarity_estimate_0_to_10, uncertainty. This is advisory only, never ART_APPROVED. Ground truth: actual baked GI1 atlas,264 light probes,4 baked reflections. No realtime GI or paid services.')
 request={'model':model,'stream':False,'format':'json','messages':[{'role':'user','content':prompt,'images':images}],
 'options':{'temperature':.1,'num_ctx':8192,'num_predict':1100},'keep_alive':'0s'}
 start=time.monotonic();raw=json.load(urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(request).encode(),headers={'Content-Type':'application/json'}),timeout=240))
 (ev/'qwen-review-raw.json').write_text(json.dumps(raw,indent=2),encoding='utf8')
 result.update(status='REAL_LOCAL_CRITIQUE_RECORDED',model=model,elapsed_seconds=round(time.monotonic()-start,2),review=json.loads(raw['message']['content']))
except Exception as exc:result.update(status='AUXILIARY_UNAVAILABLE_NOT_AN_ART_PASS',error=str(exc))
(ev/'qwen-review.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8')
print('VALORIA_REFERENCE_QWEN_AUXILIARY',result['status'])

