"""Adapt the existing local Qwen capture-review method; auxiliary, never an art gate."""
import bpy,base64,hashlib,json,os,pathlib,tempfile,time,urllib.request
root=pathlib.Path.cwd();ev=root/'docs/evidence/valoria-reference-clean'
result={'role':'AUXILIARY_CRITIQUE_OF_ACTUAL_UNITY_CAPTURE','art_approved':False,'new_model_downloads':0,'source_sha':os.environ.get('GITHUB_SHA')}

# The geography trial has its own exact A/B evidence; do not review old
# roof/world PNGs and incorrectly transfer their score to a new candidate.
request_config=json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf-8-sig'))
if request_config.get('review_scope')=='geography_v1':
 import sys
 result.update(role='GEOGRAPHY_INDEPENDENT_VISUAL_CRITIQUE',
               status='ART_REVIEW_REQUIRED',reviews=[],art_approved=False)
 folder=ev/'geography-v1'
 try:
  meta=json.loads((folder/'trial.json').read_text('utf-8'))
  assert meta['source_scene_sha256']==json.loads((folder/'survey.json').read_text('utf-8'))['source_scene_sha256']
  assert meta.get('native_unity_ab') and not meta.get('art_approved')
  if len(meta.get('images',[]))!=15 or len(meta.get('image_sha256',[]))!=15:
   raise ValueError('Expected 15 exact native geography A/B output images')
  for name,expected_hash in zip(meta['images'],meta['image_sha256']):
   candidate=folder/name
   if not candidate.is_file() or hashlib.sha256(candidate.read_bytes()).hexdigest()!=expected_hash:
    raise ValueError('Native geography image does not match reviewed candidate: '+name)
  for kind in ['city-I-wide','city-I-mobile','city-II-wide','city-II-mobile']:
   b=json.loads((folder/('before-'+kind+'-camera.json')).read_text('utf-8'))
   a=json.loads((folder/('after-'+kind+'-camera.json')).read_text('utf-8'))
   if b.get('stamp')!=a.get('stamp') or b.get('width')!=a.get('width') or b.get('height')!=a.get('height'):
    raise ValueError('Native city camera A/B not identical: '+kind)
  tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
  installed=[x['name'] for x in tags.get('models',[])]
  result['installed_models']=installed
  model=next(m for m in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if m in installed)
  group=[
   ('city',
    [root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg', folder/'before-city-II-wide.png',
     folder/'after-city-II-wide.png',folder/'after-city-II-mobile.png'],
    'Approved medieval Valoria reference, followed by REAL Unity Bastion II BEFORE, AFTER (same fixed camera), and AFTER mobile portrait. Four patches/plots visible in reference are expressly REMOVED and MUST NOT be recreated. Compare ONLY retained compact bastion, bridge, blue river, road shoulder wear, meadow topography/ground contact, vegetation, visual cohesion and legibility. Do not invent construction or change camera.'),
   ('world_overview',
    [folder/'before-world-REGION1-ENTIRE-2048x1152.png', folder/'after-world-REGION1-ENTIRE-2048x1152.png',
     folder/'before-world-forest-valoria-wide.png',folder/'after-world-forest-valoria-wide.png'],
    'REAL Unity Region1 entirety BEFORE/AFTER using same orthographic audit pose, then forest BEFORE/AFTER. The sector is a mobile 4X strategic map, not open-world exploration. Evaluate visual continuity and macrogeographic relief, actual forest masses and clearings, intact roads/cities/resources/POIs and mobile readability. Do not rate based on triangle counts or changed-pixel percentage.'),
   ('world_contacts',
    [folder/'before-world-quarry-valoria-wide.png',folder/'after-world-quarry-valoria-wide.png',
     folder/'before-world-corrupt-scout-wide.png',folder/'after-world-corrupt-scout-wide.png'],
    'REAL Unity strategic quarry BEFORE/AFTER followed by corrupt zone BEFORE/AFTER at matched cameras. Check quarry-rock-ground geological continuity, path alignment, soil transitions, unnatural dark planar halos, visual medieval dark fantasy coherence and actual visible improvement.')
  ]
  result['source_scene_sha256']=meta['source_scene_sha256']
  result['candidate_tool_sha256']=meta['tool_sha256']
  result['candidate_method']=meta.get('method')
  for scope,files,context in group:
   identities=[{'file':f.relative_to(root).as_posix(),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in files]
   if scope=='city' and identities[0]['sha256']!='86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d':
    raise ValueError('Canonical reference JPEG changed')
   images=[]
   with tempfile.TemporaryDirectory(prefix='eldoria-geography-review-') as scratch:
    for ix,path in enumerate(files):
     im=bpy.data.images.load(str(path),check_existing=False)
     w,h=im.size;scale=680/max(w,h)
     im.scale(max(1,round(w*scale)),max(1,round(h*scale)))
     f=pathlib.Path(scratch)/(str(ix)+'.jpg')
     im.filepath_raw=str(f);im.file_format='JPEG';im.save()
     bpy.data.images.remove(im);images.append(base64.b64encode(f.read_bytes()).decode())
   prompt=(context+' You are a reviewer who did NOT author these changes. Cite visual evidence only, and distinguish genuine quality gain from mere perceptibility. '
      'Provide separate rubric scores 0-10 for composition, surfaces/geography, materials, contacts/cohesion, lighting/depth, artistic identity, mobile legibility and commercial impression, a weighted overall BEFORE and AFTER estimate, and whether improvement is meaningfully visible at full screen. '
      'Identify three most serious remaining defects with concrete production fixes, any regressions, and uncertainty. Do not mark ART_APPROVED or performance certified, do not invent absent details. '
      'Return JSON object with keys: visual_observations, scores_before, scores_after, improved_full_screen, regressions, three_remaining_defects, uncertainty.')
   payload={'model':model,'stream':False,'format':'json',
            'messages':[{'role':'user','content':prompt,'images':images}],
            'options':{'temperature':.1,'num_ctx':8192,'num_predict':1700},'keep_alive':'0s'}
   started=time.monotonic()
   raw=json.load(urllib.request.urlopen(urllib.request.Request(
       'http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),
       headers={'Content-Type':'application/json'}),timeout=240))
   result['reviews'].append({'scope':scope,'model':model,'images':identities,
       'duration_seconds':round(time.monotonic()-started,2),
       'advisory_review':json.loads(raw['message']['content'])})
  result['status']='GEOGRAPHY_INDEPENDENT_VISUAL_CRITIQUE_RECORDED'
 except Exception as ex:
  result.update(status='AUXILIARY_UNAVAILABLE_NOT_AN_ART_PASS',error=str(ex))
 (folder/'independent-review.json').write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
 print('ELDORIA_GEOGRAPHY_INDEPENDENT_QWEN_REVIEW',result['status'])
 sys.exit(0)

# Final production review uses current integrated evidence, never legacy plot captures.
if json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf-8-sig')).get('review_scope') in ('production_final','production_final_city'):
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
  city_only=json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf-8-sig')).get('review_scope')=='production_final_city'
  if city_only:
   batches=batches[:1]
   batches[0]=(batches[0][0],[pathlib.Path(str(p).replace('roof-junction226','city-surface235')) for p in batches[0][1]],batches[0][2])
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
 (ev/('production-final-city-review.json' if json.loads((root/'tools/valoria-reference-clean/request.json').read_text('utf-8-sig')).get('review_scope')=='production_final_city' else 'production-final-independent-review.json')).write_text(json.dumps(result,indent=2),encoding='utf-8')
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

