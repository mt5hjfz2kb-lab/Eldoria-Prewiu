#!/usr/bin/env python3
"""Three-way Unity visual QA using original approved source SHA. Never grants art approval."""
import argparse, hashlib, html, io, json, re, zipfile
from pathlib import Path
from PIL import Image, ImageChops, ImageOps, ImageStat, ImageDraw
APPROVED={
 'march':{'name':'marcha_3_heroes_porcentajes_APROBADA.png','sha256':'9afd178a57783f83bd0ccb5bbfb78c50643eb75ae74d01cbed63375d7dcd091c','size':(941,1672),'capture':'march-reference-ratio.png'},
 'training':{'name':'entrenamiento_recorte_lamina_APROBADO.png','sha256':'a110ba0ffd2a08fa4976c488f87e624fa5601fe3f36b223ee82d19f306adbadc','size':(554,542),'capture':'training-reference-ratio.png'}}
Image.MAX_IMAGE_PIXELS=22000000
def item(zipfileobj,basename):
    found=[n for n in zipfileobj.namelist() if n==basename or n.endswith('/'+basename)]
    if len(found)!=1:raise ValueError('Expected exactly one '+basename)
    entry=zipfileobj.getinfo(found[0])
    if not 0<entry.file_size<140000000:raise ValueError('Invalid image size')
    return zipfileobj.read(entry)
def image(data):
    with Image.open(io.BytesIO(data)) as im:
        if im.format!='PNG':raise ValueError('Expected PNG')
        return im.convert('RGB')
def compare(a,b):
    if a.size!=b.size:raise ValueError('Unity capture dimensions differ')
    if a.tobytes()==b.tobytes():return {'identical_pixels':True,'changed_pixels_pct':0.0,'mean_rgb_delta':0.0}
    a=ImageOps.contain(a,(512,512));b=ImageOps.contain(b,(512,512))
    d=ImageChops.difference(a,b);h=d.convert('L').histogram()
    return {'identical_pixels':False,'changed_pixels_pct':round(100*sum(h[16:])/(a.width*a.height),2),
            'mean_rgb_delta':round(sum(ImageStat.Stat(d).mean)/3,2)}
def run(before,after,reference,output,sha_before,sha_after):
    if not re.fullmatch('[0-9a-f]{40}',sha_before) or not re.fullmatch('[0-9a-f]{40}',sha_after) or sha_before==sha_after:raise ValueError('Exact distinct 40-char SHAs required')
    output.mkdir(parents=True,exist_ok=True)
    result={'source_before':sha_before,'source_after':sha_after,'art_approved':False,'artistic_quality_automatically_scored':False,'screens':{}}
    with zipfile.ZipFile(before) as z0, zipfile.ZipFile(after) as z1, zipfile.ZipFile(reference) as zr:
        for key,policy in APPROVED.items():
            raw=item(zr,policy['name'])
            if hashlib.sha256(raw).hexdigest()!=policy['sha256']:raise ValueError('Tampered approved original: '+key)
            approved=image(raw)
            if approved.size!=policy['size']:raise ValueError('Approved size mismatch')
            old=image(item(z0,policy['capture']));new=image(item(z1,policy['capture']))
            stats=compare(old,new)
            result['screens'][key]={**stats,'real_unity_dimensions':old.size,'approved_reference_sha256':policy['sha256'],'artistic_verdict':'REQUIRES_INDEPENDENT_REVIEW'}
            cw,ch=420,580;canvas=Image.new('RGB',(3*cw,ch+70),'#121c28');d=ImageDraw.Draw(canvas)
            for i,(im,title) in enumerate(((approved,'REFERENCIA APROBADA'),(old,'UNITY ANTERIOR'),(new,'UNITY POSTERIOR'))):
                im=ImageOps.contain(im,(cw-14,ch-18),Image.Resampling.LANCZOS)
                canvas.paste(im,(i*cw+(cw-im.width)//2,56+(ch-im.height)//2))
                d.text((i*cw+14,14),title,fill='#f4f3ed')
            d.text((14,ch+36),key+' : cambio '+str(stats['changed_pixels_pct'])+' %; arte no aprobado',fill='#f0ca7d')
            canvas.save(output/(key+'-reference-before-after.jpg'),quality=90)
    (output/'result.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf-8')
    rows=''.join('<tr><td>'+html.escape(k)+'</td><td>'+str(v['changed_pixels_pct'])+'%</td><td>'+('Sí' if v['identical_pixels'] else 'No')+'</td><td>Sin evaluación artística automática</td></tr>' for k,v in result['screens'].items())
    imgs=''.join('<h2>'+k+'</h2><img loading="lazy" style="width:100%" src="'+k+'-reference-before-after.jpg">' for k in result['screens'])
    page='<html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style>body{background:#101c2b;color:#eee;font:16px system-ui;max-width:1260px;margin:auto;padding:16px}td,th{padding:12px;border-bottom:1px solid #52606a;text-align:left}</style><h1>Eldoria: referencia auténtica vs Unity</h1><p>Los píxeles modificados NO certifican mejora artística. No se simulan habilidades, unidades o mecánicas de la referencia.</p><table><tr><th>Pantalla</th><th>Cambio visual</th><th>Idéntica</th><th>Arte</th></tr>'+rows+'</table>'+imgs+'</html>'
    (output/'index.html').write_text(page,encoding='utf-8')
    return result
def main():
    p=argparse.ArgumentParser()
    for name in ('before-zip','after-zip','reference-zip'):p.add_argument('--'+name,type=Path,required=True)
    for name in ('before-sha','after-sha'):p.add_argument('--'+name,required=True)
    p.add_argument('--output',type=Path,required=True)
    a=p.parse_args()
    r=run(a.before_zip,a.after_zip,a.reference_zip,a.output,a.before_sha,a.after_sha)
    print(json.dumps({'screens':r['screens'],'art_approved':False},ensure_ascii=False))
if __name__=='__main__':main()
