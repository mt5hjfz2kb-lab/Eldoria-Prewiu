"""Adapt existing Eldoria Ollama image comparison to actual decisions and rejection.
No commercial endpoint, no free-form executable code, no model-only art approval.
"""
import base64,json,urllib.request,os,sys,hashlib,io
from PIL import Image
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'art-source/valoria/production/studio-rocks-v1'
config=json.loads((ROOT/'pipeline/art-studio-recovery-v1.json').read_text())
reference=ROOT/config['reference'];assert hashlib.sha256(reference.read_bytes()).hexdigest()==config['reference_sha256']
images=[OUT/'before-overview.png',OUT/'after-overview.png',reference,OUT/'before-cliff.png',OUT/'after-cliff.png']
prompt='You are the Eldoria environmental art director. Image 1 is real BEFORE Blender render; image 2 is real AFTER, with identical camera and lighting; image 3 is owner reference; images 4 and 5 are matched before/after cliff closeups. Judge ONLY rock terrain, ignoring unchanged castle and missing vegetation. Be skeptical. Do not equate more detail with better art. Determine whether the scanned stone assembly genuinely improves geology, eliminates triangle artifacts, and makes natural contact with grass. Identify repetition, gaps and mismatched style. You may REJECT the candidate. Return JSON: {"decision":"retain_candidate" or "reject_candidate", "reference_gap":"string", "defects":["strings"], "next_action":"keep_for_unity_test" or "bury_seams" or "change_donor" or "retain_baseline", "reason":"string"}. No other text.'
endpoint=os.environ.get('ELDORIA_OLLAMA_URL','http://127.0.0.1:11434');assert endpoint.startswith('http://127.0.0.1:') or endpoint.startswith('http://localhost:')
def encoded_review_frame(path):
 image=Image.open(path).convert('RGB');image.thumbnail((640,640));buffer=io.BytesIO();image.save(buffer,format='JPEG',quality=92)
 return base64.b64encode(buffer.getvalue()).decode()
payload={'model':config['model'],'stream':False,'format':'json','messages':[{'role':'user','content':prompt,'images':[encoded_review_frame(p) for p in images]}],'options':{'temperature':0,'num_predict':280,'num_ctx':8192}}
req=urllib.request.Request(endpoint+'/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
try:
 with urllib.request.urlopen(req,timeout=480) as response:raw=json.load(response)
 text=raw['message']['content'];(OUT/'qwen-raw-response.json').write_text(json.dumps(raw,indent=2));result=json.loads(text)
 assert result['decision'] in ('retain_candidate','reject_candidate')
 assert result['next_action'] in ('keep_for_unity_test','bury_seams','change_donor','retain_baseline')
 result.update(model=config['model'],actual_images=[str(p.relative_to(ROOT)) for p in images],visual_pass=False,unity_tested=False,paid_services_used=False)
 (OUT/'qwen-art-decision.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
except Exception as e:
 (OUT/'qwen-art-decision.json').write_text(json.dumps({'status':'unavailable_or_invalid','error':str(e),'visual_pass':False,'model':config['model']},indent=2));raise
