import json,base64,urllib.request
from pathlib import Path
root=Path(__file__).resolve().parents[3]
base=root/'art-source/valoria/production/first-hero-gate-v1'
out=base/'quality-v3'
images=[root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg',base/'quality-v2/after-hero-matched.png',out/'after-hero-matched.png']
assert all(p.exists() for p in images)
body={'model':'qwen2.5vl:3b','stream':False,'think':False,'options':{'num_predict':350,'num_ctx':4096},'messages':[{'role':'user','content':'Images in order: approved visual reference, current gate V2, same gate V3. Compare stone realism, crenels, details, composition and material quality. Identify concrete visible improvements, regressions and changes not visible at gameplay distance. Do not infer anything outside the images.','images':[base64.b64encode(p.read_bytes()).decode() for p in images]}]}
request=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(body).encode(),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(request,timeout=600) as response: reply=json.load(response)
answer=reply.get('message',{}).get('content','')
assert answer.strip(),reply
report={'runtime':'ollama local GitHub Actions','model':body['model'],'actual_image_files':[str(p.relative_to(root)) for p in images],'advisory':answer,'art_acceptance':'pending human visual review'}
(out/'qwen-art-review.json').write_text(json.dumps(report,indent=2)+'\n')
print('ELDORIA_QWEN_V3_IMAGE_REVIEW',answer)
