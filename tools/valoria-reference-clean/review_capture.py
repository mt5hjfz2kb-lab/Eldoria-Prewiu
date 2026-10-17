"""Adapt the existing local Qwen capture-review method; auxiliary, never an art gate."""
import bpy,base64,hashlib,json,os,pathlib,tempfile,time,urllib.request
root=pathlib.Path.cwd();ev=root/'docs/evidence/valoria-reference-clean'
result={'role':'AUXILIARY_CRITIQUE_OF_ACTUAL_UNITY_CAPTURE','art_approved':False,'new_model_downloads':0,'source_sha':os.environ.get('GITHUB_SHA')}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=10))
 installed=[x['name'] for x in tags.get('models',[])];result['installed_models']=installed
 model=next(x for x in ['qwen3-vl:8b-instruct-q4_K_M','qwen3-vl:4b-instruct'] if x in installed)
 paths=[ev/'reference-target.jpeg',ev/'playmode-home.png'];result['original_images']=[{'path':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths]
 images=[]
 # Small inspection copies avoid the known historical image-context overflow.
 with tempfile.TemporaryDirectory(prefix='valoria-native-review-') as scratch:
  for index,path in enumerate(paths):
   im=bpy.data.images.load(str(path),check_existing=False);im.scale(640,480);out=pathlib.Path(scratch)/(str(index)+'.jpg');im.filepath_raw=str(out);im.file_format='JPEG';im.save();bpy.data.images.remove(im);images.append(base64.b64encode(out.read_bytes()).decode())
 prompt=('Image 1 is the owner target for Valoria. Image 2 is the ACTUAL CURRENT UNITY CAMERA, not generated concept art. '
 'Review the second image against the first. Its layout must contain one compact main keep, one distinct lower gate, '
 'a real arched river bridge, four empty building plots and sparse broken walls. '
 'Give the THREE highest-impact remaining visual defects and a concrete Blender or Unity correction for each. '
 'Judge composition, architecture proportions, material separation, terrain/shore integration, natural vegetation and lighting. '
 'Do not invent hidden geometry or judge mobile performance from an image. Do not call a technical export an artistic success. '
 'Do not recommend adding more towers or permanent buildings to the empty plots. '
 'Return valid JSON with keys visible_strengths, highest_impact_defects (three items with observation and correction), '
 'reference_similarity_estimate_0_to_10, uncertainty. This estimate is advisory and cannot issue ART_APPROVED or MOBILE_PASS.')
 request={'model':model,'stream':False,'format':'json','messages':[{'role':'user','content':prompt,'images':images}],
 'options':{'temperature':.1,'num_ctx':8192,'num_predict':1100},'keep_alive':'0s'}
 start=time.monotonic();raw=json.load(urllib.request.urlopen(urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(request).encode(),headers={'Content-Type':'application/json'}),timeout=240))
 (ev/'qwen-review-raw.json').write_text(json.dumps(raw,indent=2),encoding='utf8')
 result.update(status='REAL_LOCAL_CRITIQUE_RECORDED',model=model,elapsed_seconds=round(time.monotonic()-start,2),review=json.loads(raw['message']['content']))
except Exception as exc:result.update(status='AUXILIARY_UNAVAILABLE_NOT_AN_ART_PASS',error=str(exc))
(ev/'qwen-review.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf8')
print('VALORIA_REFERENCE_QWEN_AUXILIARY',result['status'])
