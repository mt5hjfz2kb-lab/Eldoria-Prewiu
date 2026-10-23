#!/usr/bin/env python3
"""Package unmodified Unity PNGs and original references; refuse mismatched A/B cameras."""
import argparse,base64,hashlib,html,json
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--before',type=Path,required=True);p.add_argument('--after',type=Path,required=True);p.add_argument('--references',type=Path,required=True);p.add_argument('--out',type=Path,required=True);p.add_argument('--before-sha',required=True);p.add_argument('--after-sha',required=True);a=p.parse_args()
if a.out.exists():raise SystemExit('Refuse to overwrite frozen review')
def data(path):return 'data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
keys=['sourceScene','version','width','height','zoom','aspect','cameraPosition','cameraRotation','worldTarget']
views=[];audit=[]
for stage,reference in [('cercana','01_vista_cercana.png'),('regional','02_vista_regional.png'),('panoramica','03_vista_panoramica.png')]:
 for orientation in ['horizontal','vertical']:
  name=f'{stage}-{orientation}'; metas=[]
  for directory in [a.before,a.after]:
   m=json.loads((directory/f'{name}-source.json').read_text());b=(directory/f'{name}-unity.png').read_bytes()
   assert m['actualUnityCamera'] and m['playMode'] and hashlib.sha256(b).hexdigest()==m['sha256'],name
   metas.append(m)
  mismatch=[k for k in keys if metas[0][k]!=metas[1][k]]
  if mismatch:raise SystemExit(f'{name}: incompatible A/B cameras: {mismatch}')
  audit.append(dict(view=name,camera_identical=True,before=metas[0],after=metas[1]))
  block=a.after/f'{stage}-blockout-{orientation}-unity.png'
  views.append(dict(name=name,before=data(a.before/f'{name}-unity.png'),after=data(a.after/f'{name}-unity.png'),reference=data(a.references/reference),blockout=data(block) if block.exists() else None))
page='''<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Eldoria — revisión de la reconstrucción</title><style>
body{margin:0;background:#101a20;color:#eef1e8;font:16px system-ui}main{max-width:1400px;margin:auto;padding:24px}h1{font:32px Georgia;margin:0 0 10px}p{max-width:1000px;line-height:1.5;color:#c5cec8}button,select{background:#263a43;color:white;border:1px solid #607c86;padding:12px;border-radius:8px;margin:5px}button.active{background:#657844}figure{margin:0}figcaption{padding:10px;background:#23333a}img{display:block;width:100%;height:auto}.grid{display:grid;grid-template-columns:1fr 1fr;gap:12px}.single{max-width:1000px;margin:auto}.ref{max-width:410px;margin:auto}.source{font:12px monospace;overflow-wrap:anywhere}details{margin:20px 0}summary{cursor:pointer}a{color:#b3d0de}@media(max-width:700px){main{padding:12px}.grid{grid-template-columns:1fr}h1{font-size:25px}} </style><main>
<h1>Eldoria · reconstrucción desde una base limpia</h1><p>Capturas auténticas de la escena jugable en Unity. Las seis parejas antes/después tienen exactamente la misma cámara. Las referencias originales se muestran aparte para juzgar composición y estilo: no existe equivalencia píxel a píxel entre ilustración y escena 3D.</p>
<p class="source">Antes: BEFORE_SHA<br>Candidato: AFTER_SHA</p>
<label>Vista <select id="view"></select></label><nav><button data-mode="pair">Antes / después</button><button data-mode="after">Candidato</button><button data-mode="blockout">Blockout</button><button data-mode="reference">Referencia original</button></nav><section id="panel"></section><details><summary>Identidad de cámaras y evidencia</summary><pre id="audit" style="white-space:pre-wrap;font-size:12px"></pre></details><p>El blockout es el candidato sin follaje ni acabado superficial; no es una captura histórica. Pruebas técnicas y comparación visual son controles distintos. Sin certificación de móvil físico ni aprobación final del propietario.</p></main><script>
const views=VIEWS,audit=AUDIT;let mode='pair';const select=document.querySelector('#view');views.forEach((v,i)=>select.add(new Option(v.name,i)));function fig(src,label,cls=''){return `<figure class="${cls}"><figcaption>${label}</figcaption><img src="${src}" alt="${label}"></figure>`}function render(){const v=views[select.value||0];document.querySelectorAll('button').forEach(b=>b.classList.toggle('active',b.dataset.mode===mode));document.querySelector('#panel').innerHTML=mode==='pair'?`<div class="grid">${fig(v.before,'Antes · Unity')}${fig(v.after,'Reconstrucción · Unity')}</div>`:mode==='reference'?fig(v.reference,'PNG original aprobado','ref'):mode==='blockout'?(v.blockout?fig(v.blockout,'Blockout del candidato · Unity','single'):'<p>Blockout disponible en regional y panorámica.</p>'):fig(v.after,'Candidato · Unity','single');document.querySelector('#audit').textContent=JSON.stringify(audit[select.value||0],null,2)}select.onchange=render;document.querySelectorAll('button').forEach(b=>b.onclick=()=>{mode=b.dataset.mode;render()});render();</script></html>'''
page=page.replace('BEFORE_SHA',html.escape(a.before_sha)).replace('AFTER_SHA',html.escape(a.after_sha)).replace('VIEWS',json.dumps(views)).replace('AUDIT',json.dumps(audit))
a.out.parent.mkdir(parents=True,exist_ok=True);a.out.write_text(page);a.out.with_suffix('.camera-audit.json').write_text(json.dumps(audit,indent=2));print(f'Wrote {a.out}; 6 identical-camera pairs; PNG hashes verified')
