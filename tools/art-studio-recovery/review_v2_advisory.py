"""Auxiliary local Qwen review; the artistic director retains final judgement."""
import base64, hashlib, io, json, os, urllib.request
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
V1=ROOT/'art-source/valoria/production/studio-rocks-v1'
OUT=ROOT/'art-source/valoria/production/studio-rocks-v2-mcp'
REF=V1/'owner-reference.jpeg'
config=json.loads((ROOT/'pipeline/art-studio-recovery-v1.json').read_text())
assert hashlib.sha256(REF.read_bytes()).hexdigest()==config['reference_sha256']
board=Image.new('RGB',(1024,545),'#18202b');draw=ImageDraw.Draw(board)
for index,p in enumerate((V1/'after-cliff.png',OUT/'after-cliff.png')):
 frame=Image.open(p).convert('RGB').crop((0,300,1280,800)).resize((512,200))
 board.paste(frame,(index*512,25))
 draw.text((index*512+10,8),'V1 SCAN' if index==0 else 'V2 TWO CC0 SCANS + MCP',fill='white')
ref=Image.open(REF).convert('RGB').crop((1000,760,1513,1039)).resize((512,279))
board.paste(ref,(256,258));draw.text((266,240),'OWNER REFERENCE - ROCK CROP',fill='white')
board_path=OUT/'vision-advisory-board.png';board.save(board_path)
buf=io.BytesIO();board.save(buf,format='JPEG',quality=92)
payload={'model':config['model'],'stream':False,'format':'json',
 'messages':[{'role':'user','content':('Assess only cliff-rock silhouette variety and grass contacts on this single labeled image. Top left is existing V1, top right is V2, bottom is owner rock reference. Ignore castle, paths and full-scene composition. Return JSON with rock_change one of improved/mixed/regressed, reference_quality_met boolean, defects array of at most three cliff-specific strings, and suggested_action one of retain_for_study/fix_contact/change_technique. This is advisory; do not assert certification.'),'images':[base64.b64encode(buf.getvalue()).decode()]}],
 'options':{'temperature':0,'num_predict':200,'num_ctx':4096}}
url=os.environ.get('ELDORIA_OLLAMA_URL','http://127.0.0.1:11434')
assert url.startswith('http://127.0.0.1:') or url.startswith('http://localhost:')
try:
 request=urllib.request.Request(url+'/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(request,timeout=420) as response:raw=json.load(response)
 (OUT/'vision-advisory-raw.json').write_text(json.dumps(raw,indent=2))
 result=json.loads(raw['message']['content'])
 assert result['rock_change'] in ('improved','mixed','regressed')
 assert result['suggested_action'] in ('retain_for_study','fix_contact','change_technique')
 assert isinstance(result['reference_quality_met'],bool)
 result.update(status='auxiliary_only',visual_pass=False,unity_tested=False,model=config['model'],source_board_sha256=hashlib.sha256(board_path.read_bytes()).hexdigest())
 (OUT/'vision-advisory.json').write_text(json.dumps(result,indent=2))
except Exception as exc:
 (OUT/'vision-advisory.json').write_text(json.dumps({'status':'unavailable_or_invalid','error':str(exc),'visual_pass':False,'unity_tested':False,'source_board_sha256':hashlib.sha256(board_path.read_bytes()).hexdigest()},indent=2))
