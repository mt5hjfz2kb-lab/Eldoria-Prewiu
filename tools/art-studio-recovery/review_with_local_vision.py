"""Adapt existing Eldoria Ollama image comparison to actual decisions and rejection.
No commercial endpoint, no free-form executable code, no model-only art approval.
"""
import base64,json,urllib.request,os,sys,hashlib,io
from PIL import Image,ImageDraw
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'art-source/valoria/production/studio-rocks-v1'
post='--post-mcp' in sys.argv
focused='--focused' in sys.argv
report_name='qwen-post-mcp-decision' if post else 'qwen-art-decision'
raw_name='qwen-post-mcp-raw-response' if post else 'qwen-raw-response'
if focused and post:report_name='qwen-focused-post-decision';raw_name='qwen-focused-post-raw-response'
config=json.loads((ROOT/'pipeline/art-studio-recovery-v1.json').read_text())
reference=ROOT/config['reference'];assert hashlib.sha256(reference.read_bytes()).hexdigest()==config['reference_sha256']
images=[OUT/'before-overview.png',OUT/'after-overview.png',reference,OUT/'before-cliff.png',OUT/'after-cliff.png']
prompt='You are the Eldoria environmental art director. Image 1 is real BEFORE Blender render; image 2 is real AFTER, with identical camera and lighting; image 3 is owner reference; images 4 and 5 are matched before/after cliff closeups. Judge ONLY rock terrain, ignoring unchanged castle and missing vegetation. Be skeptical. Do not equate more detail with better art. Determine whether the scanned stone assembly genuinely improves geology, eliminates triangle artifacts, and makes natural contact with grass. Identify repetition, gaps and mismatched style. You may REJECT the candidate. Return JSON: {"decision":"retain_candidate" or "reject_candidate", "reference_gap":"string", "defects":["strings"], "next_action":"keep_for_unity_test" or "bury_seams" or "change_donor" or "retain_baseline", "reason":"string"}. No other text.'
if focused:
 board=Image.new('RGB',(1024,545),'#18202b');draw=ImageDraw.Draw(board)
 for i,name in enumerate(['before-cliff.png','after-cliff.png']):
  frame=Image.open(OUT/name).convert('RGB').crop((0,300,1280,800)).resize((512,200))
  board.paste(frame,(i*512,25));draw.text((i*512+10,8),'BEFORE' if i==0 else 'AFTER',fill='white')
 ref=Image.open(reference).convert('RGB').crop((1000,760,1513,1039)).resize((512,279))
 board.paste(ref,(256,258));draw.text((266,240),'OWNER REFERENCE - ROCK CROP',fill='white')
 board_path=OUT/('focused-post-review.png' if post else 'focused-pre-review.png');board.save(board_path)
 images=[board_path]
 prompt='Evaluate ONLY CLIFF ROCK quality in this actual-render comparison board. Top LEFT is BEFORE; top RIGHT is AFTER; bottom is owner reference rock crop. Ignore buildings, roads, trees and whole-scene composition: they are outside these crops. Compare geological shape, visible texture repetition, and contact with grass. Does AFTER improve on BEFORE, and does it reach the reference quality? These are different views of the same rock region. Do not approve merely because more texture exists. Return JSON with decision retain_candidate or reject_candidate; rock_change improved or unchanged or worse; reference_quality_met boolean; defects array of at most 3 rock-only flaws; next_action keep_for_unity_test or bury_seams or change_donor or retain_baseline; reason one short sentence. No other text.'
endpoint=os.environ.get('ELDORIA_OLLAMA_URL','http://127.0.0.1:11434');assert endpoint.startswith('http://127.0.0.1:') or endpoint.startswith('http://localhost:')
def encoded_review_frame(path):
 image=Image.open(path).convert('RGB');image.thumbnail((1024,1024) if focused else (640,640));buffer=io.BytesIO();image.save(buffer,format='JPEG',quality=92)
 return base64.b64encode(buffer.getvalue()).decode()
payload={'model':config['model'],'stream':False,'format':'json','messages':[{'role':'user','content':prompt,'images':[encoded_review_frame(p) for p in images]}],'options':{'temperature':0,'num_predict':280,'num_ctx':4096 if focused else 8192}}
req=urllib.request.Request(endpoint+'/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
try:
 with urllib.request.urlopen(req,timeout=480) as response:raw=json.load(response)
 text=raw['message']['content'];(OUT/(raw_name+'.json')).write_text(json.dumps(raw,indent=2));result=json.loads(text)
 assert result['decision'] in ('retain_candidate','reject_candidate')
 if focused:
  assert result['rock_change'] in ('improved','unchanged','worse')
  assert isinstance(result['reference_quality_met'],bool)
  assert isinstance(result['defects'],list) and len(result['defects'])<=3
  assert not any(word in ' '.join(result['defects']).lower() for word in ('castle','building','path','road'))
 assert result['next_action'] in ('keep_for_unity_test','bury_seams','change_donor','retain_baseline')
 result.update(focused_rock_comparison=focused,review_phase='post_mcp_correction' if post else 'pre_mcp_correction',model=config['model'],actual_images=[str(p.relative_to(ROOT)) for p in images],visual_pass=False,unity_tested=False,paid_services_used=False)
 (OUT/(report_name+'.json')).write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
except Exception as e:
 (OUT/(report_name+'.json')).write_text(json.dumps({'status':'unavailable_or_invalid','error':str(e),'visual_pass':False,'model':config['model']},indent=2));raise
