"""Advisory local image comparisons with bounded Qwen image payloads."""
import json,base64,urllib.request,io
from PIL import Image
from pathlib import Path
root=Path(__file__).resolve().parents[3]
base=root/'art-source/valoria/production/first-hero-gate-v1'
out=base/'quality-v3'
ref=root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg'
v2=base/'quality-v2/after-hero-matched.png'
v3=out/'after-hero-matched.png'
s2=base/'quality-v2/after-strategic.png'
s3=out/'after-strategic.png'
assert all(p.exists() for p in (ref,v2,v3,s2,s3))
def encoded(path):
    with Image.open(path) as im:
        im=im.convert('RGB')
        im.thumbnail((448,448))
        buff=io.BytesIO();im.save(buff,format='JPEG',quality=80)
        return base64.b64encode(buff.getvalue()).decode('ascii')
def ask(label,a,b):
    payload={'model':'qwen2.5vl:3b','stream':False,'think':False,
        'options':{'num_predict':250,'num_ctx':4096,'temperature':0.1},
        'messages':[{'role':'user','content':(
            'You are an art director reviewing two renders of a medieval game gate. '+
            label+' Describe visible stone realism, repetition, craftsmanship and readability. '+
            'Identify concrete differences without speculation. Is the improvement noticeable? Reply in concise English.'),
          'images':[encoded(a),encoded(b)]}]}
    request=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
    try:
        with urllib.request.urlopen(request,timeout=600) as response: data=json.load(response)
    except urllib.error.HTTPError as exc:
        raise RuntimeError(f'Ollama HTTP {exc.code}: '+exc.read().decode(errors='replace')[:1600])
    answer=data.get('message',{}).get('content','')
    if not answer.strip(): raise RuntimeError('No final text from model')
    return {'inputs':[str(a.relative_to(root)),str(b.relative_to(root))], 'advisory':answer}
reviews=[ask('First V2 hero gate, then V3 hero gate.',v2,v3),
         ask('First V2 strategic camera, then V3 strategic camera.',s2,s3),
         ask('First approved target reference, then actual V3 hero.',ref,v3)]
result={'runtime':'local Ollama on GitHub Actions','model':'qwen2.5vl:3b','resizing':'preserve aspect JPEG 448 max','real_image_review':reviews,'art_acceptance':'human comparison required'}
(out/'qwen-art-review.json').write_text(json.dumps(result,indent=2)+'\n')
print('ELDORIA_QWEN_V3_REAL_ART_REVIEW',json.dumps(result,ensure_ascii=False))
