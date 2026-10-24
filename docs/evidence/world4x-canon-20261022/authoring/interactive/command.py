import bpy,json,pathlib,hashlib,os
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
assert (out/'02-mineral-hierarchy.blend').exists()
for request in sorted(out.glob('request-*.json')):
 data=json.loads(request.read_text(encoding='utf-8-sig'));code=data.get('params',{}).get('code')
 if isinstance(code,str):bpy.data.texts.new('MCP_'+request.stem).write(code)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Macro_Review_Candidate_NOT_APPROVED.blend'),compress=True)
report={'pid':os.getpid(),'baseline_observed':True,'artistic_cycles':2,'first_cycle':'VISUAL_REJECTED','second_cycle':'CANDIDATE_NATIVE_REVIEW_REQUIRED','art_approved':False,'unity_changed':False,'final_blend_sha256':hashlib.sha256((out/'Eldoria_Macro_Review_Candidate_NOT_APPROVED.blend').read_bytes()).hexdigest()}
(out/'final-verification.json').write_text(json.dumps(report,indent=2))
(out/'stop.request').write_text('Save reviewed source and close this owned session only before native certification.')
print('FINAL_VERIFICATION',json.dumps(report))
