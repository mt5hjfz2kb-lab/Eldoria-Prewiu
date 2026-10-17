# Reuse the existing local artist brief as auxiliary advice only; never an art gate.
import os,sys,json,runpy,pathlib,urllib.request,hashlib
out=pathlib.Path('docs/evidence/valoria-layered-construction-20261016/entry-zone-v1')
status={'role':'AUXILIARY_EXISTING_ARTIST_BRIEF_NOT_SOLE_AUTHORITY','new_model_downloads':False,'executed_geometry':False}
try:
 tags=json.load(urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=15))
 (out/'ollama-installed-models.json').write_text(json.dumps(tags,indent=2),'utf8')
 installed=[m['name'] for m in tags.get('models',[])]
 model=next((m for m in ['qwen3-vl:4b-instruct','qwen3-vl:8b-instruct-q4_K_M','hf.co/Qwen/Qwen3-VL-2B-Instruct-GGUF:Q4_K_M'] if m in installed),None)
 if not model:raise RuntimeError('No historically known vision model currently installed; no download authorized')
 memory=list(out.glob('memory-*.txt'))
 inventory=('Current scene native Unity XYZ: bridge ends z=-19.65/-9.35 at y=.38; gate x=0,z=-9.25; '
 'stairs x=-.9789,z=28.13, 12 original treads; left plot -22.45,.31,19.20; right plot 20.10,.31,16.93. '
 'Blender is X,-UnityZ,UnityY. The camera is on POSITIVE Blender Y, unlike old tower studies. '
 'This is a new isolated entry zone using the existing V3 terrain reprofile and original LowerGateFamilyV1 .blend at uniform .75. '
 'No FOV changes; source V1 full frame is still greybox. Review the attached reference and actual Unity image, advise a bounded improvement only. '
 'Do not propose changes within 4m of bridge endpoints or the protected path/stair corridor. '
 'Prior evidence: 250 whole-city translations failed simultaneous macro proportion + old source rays; spatial reflow is now authorized. '
 + ('\nVerified historical memory: '+memory[-1].read_text('utf8') if memory else '\nNo new memory brief found.'))
 ip=out/'qwen-context-inventory.txt';ip.write_text(inventory,'utf8')
 sys.path.insert(0,str(pathlib.Path('tools/art-direction/ai-authoring').resolve()))
 sys.argv=['author_brief.py','--reference','references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',
 '--unity-before','docs/evidence/valoria-layered-construction-20261016/spatial-v1/spatial-AFTER-home.png',
 '--target-object','ValoriaEntryTerrainV1','--mesh-context',str(out/'inspect/mesh-context.json'),
 '--inventory',str(ip),'--out',str(out/'qwen-auxiliary'),'--model',model,'--num-ctx','8192','--num-predict','1100',
 '--max-actions','2','--repair-attempts','0','--request-timeout','240']
 runpy.run_path('tools/art-direction/ai-authoring/author_brief.py',run_name='__main__')
 status.update(model=model,status='REAL_AUXILIARY_PLAN_RECORDED_NOT_EXECUTED')
except (Exception,SystemExit) as exc:
 status.update(status='AUXILIARY_UNAVAILABLE_OR_REJECTED_NO_ART_APPROVAL',error=str(exc))
(out/'qwen-auxiliary-status.json').write_text(json.dumps(status,indent=2),'utf8')
print('VALORIA_QWEN_AUXILIARY',json.dumps(status))
