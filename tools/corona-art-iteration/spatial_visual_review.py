"""Independent evaluation of authentic spatial A/B frames; no promotion or auto retry."""
import argparse, hashlib, json, os, re, subprocess, sys
from decimal import Decimal as D
from pathlib import Path
from PIL import Image, ImageDraw

CITY = ['bastion-1','bastion-2','bastion-3','bastion-4','bastion-5','bastion-5-panorama']
WORLD = ['home','wood','stone','region','breach','panorama','woodland','river','breach-panorama']
BASE = '5ef2aee67cf8fde57ba2fc4b3d929b517ee67944'
CORRECTED_UI = ['march','bag']
def read(p):
    return json.loads(p.read_text(encoding='utf-8'))
def write(p,x):
    p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()
def gh(path):
    return json.loads(subprocess.check_output(['gh','api',path],timeout=60))
def checked_image(p):
    with Image.open(p) as im:
        if im.size != (390,844): raise ValueError('Unexpected native frame dimensions')
        return im.convert('RGB')
def city_pairs(a):
    m=read(a.spatial/'manifest.json')
    if m['baseline_sha']!=BASE or m['candidate_sha']!=os.environ['GITHUB_SHA'] or m['missing']:
        raise ValueError('Wrong or incomplete city source')
    rows={r['role']:r for r in m['pairs']}
    for k in CITY:
        r=rows[k]
        b=a.baseline/(k+'.png' if k!='bastion-5' else 'bastion-5-completo.png')
        c=a.candidate/b.name
        if sha(b)!=r['baseline_sha256'] or sha(c)!=r['candidate_sha256']:raise ValueError('City image hash mismatch '+k)
        yield k,checked_image(b),checked_image(c)
def corrected_ui_pairs(a):
    m=read(a.ui/'manifest.json')
    if m['baseline_sha']!=BASE or m['candidate_sha']!=os.environ['GITHUB_SHA'] or m['missing'] or m['dpr']!=1:
        raise ValueError('Wrong corrected UI source')
    rows={r['role']:r for r in m['pairs']}
    for k in CORRECTED_UI:
        r=rows[k]
        b=a.baseline/('sistema-'+k+'.png')
        c=a.candidate/b.name
        if sha(b)!=r['baseline_sha256'] or sha(c)!=r['candidate_sha256']:
            raise ValueError('Corrected UI image hash mismatch '+k)
        yield k,checked_image(b),checked_image(c)
def world_pairs(a):
    for k in WORLD:
        files=[a.native/'after'/('colour-'+v+'-'+k+'.png') for v in ('original','candidate')]
        metadata=[read(p.with_suffix('.json')) for p in files]
        for p,m in zip(files,metadata):
            if sha(p)!=m['sha256'] or not m['actualUnityCamera'] or not m['playMode'] or (m['width'],m['height'])!=(390,844):
                raise ValueError('Unverified native frame '+k)
        for f in ('cameraPosition','cameraRotation','worldTarget','zoom','aspect'):
            if metadata[0][f]!=metadata[1][f]:raise ValueError('Unmatched native camera '+k)
        yield k,checked_image(files[0]),checked_image(files[1])
def sheet(pairs,out,limit):
    pairs=list(pairs)
    cols=3; w=390; h=844; cell=2*w+12; row=h+30
    img=Image.new('RGB',(cols*cell,((len(pairs)+cols-1)//cols)*row),'#172028')
    d=ImageDraw.Draw(img)
    for i,(key,l,r) in enumerate(pairs):
        x=i%cols*cell;y=i//cols*row
        img.paste(l,(x,y+30));img.paste(r,(x+w+12,y+30))
        d.text((x+5,y+5),'ORIGINAL / '+key,fill='white');d.text((x+w+17,y+5),'CANDIDATO / '+key,fill='white')
    for q in (93,88,80,72):
        img.save(out,quality=q,subsampling=0,optimize=True)
        if out.stat().st_size<=limit:return
    raise ValueError('Montage exceeds evidence limit')
def validate(data,keys):
    if set(data)!={'screens','overall'} or set(data['screens'])!=set(keys):raise ValueError('Incomplete visual result')
    for k,r in data['screens'].items():
        if set(r)!={'before','after','reference_score','reference_kind','observation','remaining'}:raise ValueError('Invalid row')
        for x in ('before','after','reference_score'):
            if r[x] is not None and (type(r[x]) not in (int,float) or not 0<=r[x]<=10):raise ValueError('Invalid score')
        if r['reference_kind'] not in ('direct','analog','none') or (r['reference_kind']=='none' and r['reference_score'] is not None):raise ValueError('Unjustified reference')
        if not all(isinstance(r[x],str) and r[x].strip() for x in ('observation','remaining')):raise ValueError('Missing visual observations')
        r['delta']=None if r['before'] is None or r['after'] is None else round(r['after']-r['before'],2)
        r['minimum_met']=r['after'] is not None and r['after']>=8
    return data
def main(a):
    repo=os.environ['GITHUB_REPOSITORY']; run=os.environ['GITHUB_RUN_ID']; head=os.environ['GITHUB_SHA']
    if os.environ['GITHUB_RUN_ATTEMPT']!='1':raise ValueError('No paid retry')
    run_info=gh(f'repos/{repo}/actions/runs/{run}')
    if run_info['head_sha']!=head or run_info['head_branch']!='candidate/corona-world-art-completion-20261126':raise ValueError('Wrong run')
    if gh(f'repos/{repo}/git/ref/heads/candidate/corona-world-art-completion-20261126')['object']['sha']!=head:raise ValueError('Branch changed')
    if gh(f'repos/{repo}/git/ref/heads/pilot/eldoria-openai-development-studio-20261125')['object']['sha']!='1657b2caa6b1a955319538ce3a8b2906398599b3':raise ValueError('Studio source changed')
    original=gh(f'repos/{repo}/actions/runs/45824434222')
    if original['head_sha']!='3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c':raise ValueError('Wrong UI reservation')
    superseded=gh(f'repos/{repo}/actions/runs/45831764966')
    if superseded['head_sha']!='42f990c09cb43f8e5c4b0b4b2ea126086db3a047' or superseded['status']!='completed':
        raise ValueError('Superseded spatial reservation not terminal')
    old_jobs=gh(f'repos/{repo}/actions/runs/45831764966/jobs')['jobs']
    if any(j['name']=='spatial-art-audit' and j['conclusion']!='skipped' for j in old_jobs):
        raise ValueError('Prior spatial audit may have incurred spend')
    receipt=read(a.artist/'budget-transfer.json')
    if receipt['released_without_api_calls_usd']!='2.10' or receipt['remaining_usd']!='0.85' or receipt['new_reserve_usd']!='1.25':raise ValueError('Unproven budget')
    if D('2.90')+D('1.25')+D('.45')+D('.30')+D('.10')!=D('5.00'):raise ValueError('Budget mismatch')
    meta=gh(f'repos/{repo}/contents/pipeline/active-workstreams.json?ref=main')
    import base64
    registry=json.loads(base64.b64decode(gh(f"repos/{repo}/git/blobs/{meta['sha']}")['content']))
    lease=next(x for x in registry['active'] if x['id']=='eldoria-openai-development-studio-20261125')
    if lease['owner']!='chatgpt-corona-brecha-commercial-20261126' or lease['ownership_revision']!=18:raise ValueError('Studio lease changed')
    if sha(a.native/'before'/'routes.txt')!=sha(a.native/'after'/'routes.txt') or sha(a.native/'before'/'grid.bin')!=sha(a.native/'after'/'grid.bin'):raise ValueError('Geographic parity failed')
    refs=read(a.references/'reference-manifest.json')
    if not any(x['label'].startswith('WOS') for x in refs) or not any(x['label'].startswith('COD') for x in refs):raise ValueError('References absent')
    a.out.mkdir(exist_ok=False)
    write(a.out/'budget.json',{'cap':'5.00','prior_held':'2.90','material_held':'1.25','ui_held':'.45','spatial_reserved':'.30','corrected_ui_reserved':'.10','unspent':'0.00','no_retry':True,'invoice_verified':False,'source_sha':head})
    sys.path.insert(0,str(a.studio.resolve()))
    import studio,mission
    studio.MAX_OUTPUT=3200
    results={};total=D(0)
    # Prepare both complete evidence packets before any paid request.
    packets={}
    for group,keys,pairs in [('city',CITY,city_pairs(a)),('world',WORLD,world_pairs(a)),('corrected-ui',CORRECTED_UI,corrected_ui_pairs(a))]:
        folder=a.out/group;folder.mkdir()
        jpg=folder/'actual-ab.jpg';sheet(pairs,jpg,studio.MAX_IMAGE_BYTES)
        packets[group]=(folder,keys,jpg)
    for group,(folder,keys,jpg) in packets.items():
        evidence=mission.textfile(folder/'evidence.txt',json.dumps({'screens':keys,'source_sha':head,'baseline_sha':BASE,'references':refs,'mobile_result':os.environ.get('MOBILE_RESULT')},ensure_ascii=False))
        cap=D('.10') if group=='corrected-ui' else D('.15')
        mission.RESERVES['auditor']=cap
        studio.MAX_OUTPUT=1600 if group=='corrected-ui' else 3200
        ledger=mission.Ledger(folder/'ledger.json',cap)
        prompt='Auditor independiente de arte de Eldoria. Imagen 1: pares originales izquierda y candidatos derecha, capturas auténticas. Imagen 2: referencias comerciales reales WOS y COD; no son vistas idénticas. '+('Evalúa solamente la ciudad, no el HUD. ' if group=='city' else ('Evalúa terreno, agua, integración de objetos y coherencia de las vistas a cada escala, sin HUD. ' if group=='world' else 'Evalúa legibilidad del resumen de marchas y densidad, iconos y uso del espacio de la mochila. Comprueba si realmente hay una mejora; no la presupongas. '))+ 'Puntúa antes/después 0–10 con los mismos criterios: composición, profundidad, integración, color, detalle, calidad comercial. No deduzcas mejoras de las pruebas técnicas. Si no hay referencia directa usa analog o none y null si no comparable. Señala diferencias pequeñas, nulas o regresiones. JSON puro: {screens:{id:{before:number|null,after:number|null,reference_score:number|null,reference_kind:string,observation:string,remaining:string}},overall:string}. Todos los ids: '+','.join(keys)+'. No apruebes automáticamente, ni deduzcas animación, rendimiento o iPhone físico.'
        raw=mission.ask('visual','gpt-6.1-terra',evidence,(str(jpg),str(a.references/'reference.jpg')),ledger,'auditor',folder,prompt,effort='none')
        raw=raw.strip()
        if raw.startswith('\x60\x60\x60json') and raw.endswith('\x60\x60\x60'):raw=raw[7:-3].strip()
        data=validate(json.loads(raw),keys);write(folder/'scores.json',data)
        results.update(data['screens']);total+=D(ledger.rows[0]['estimated_usd'])
    report={'source_sha':head,'baseline_sha':BASE,'screens':results,'all_scored':all(r['after'] is not None for r in results.values()),'all_minimums_met':all(r['minimum_met'] for r in results.values()),'visual_approval':False,'physical_iphone':'NOT_TESTED','estimated_usd':str(total),'invoice_verified':False,'mobile_result':os.environ.get('MOBILE_RESULT')}
    write(a.out/'spatial-scorecard.json',report)
    print('CORONA_SPATIAL_SCORECARD',json.dumps(report,ensure_ascii=False),flush=True)
if __name__=='__main__':
    p=argparse.ArgumentParser()
    for key in ('spatial','ui','native','baseline','candidate','references','artist','studio','out'):p.add_argument('--'+key,type=Path,required=True)
    main(p.parse_args())
