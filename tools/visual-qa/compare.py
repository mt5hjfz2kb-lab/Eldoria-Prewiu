#!/usr/bin/env python3
"""Review exact-head Unity screenshot artifacts, never infer artistic quality from pixels."""
import argparse, hashlib, html, io, json, os, re, sys, zipfile
from pathlib import Path
from urllib.request import Request, urlopen
from urllib.parse import urlencode
from PIL import Image, ImageChops, ImageStat

MAX_ARCHIVE = 160 * 1024 * 1024
MAX_UNPACKED = 500 * 1024 * 1024
MAX_IMAGE = 25 * 1024 * 1024
SHA = re.compile(r'^[0-9a-f]{40}$')
Image.MAX_IMAGE_PIXELS = 22_000_000

def api(repo, token, path, binary=False):
    req = Request('https://api.github.com/repos/' + repo + path,
                  headers={'Authorization':'Bearer '+token,'Accept':'application/vnd.github+json',
                           'X-GitHub-Api-Version':'2022-11-28','User-Agent':'eldoria-qa'})
    with urlopen(req,timeout=75) as res:
        data = res.read(MAX_ARCHIVE + 1)
    if len(data)>MAX_ARCHIVE: raise ValueError('Archive/API response too large')
    return data if binary else json.loads(data)

def source_pair(run_id):
    token=os.environ['GITHUB_TOKEN']; repo=os.environ['GITHUB_REPOSITORY']
    current=api(repo,token,f'/actions/runs/{run_id}')
    if current.get('conclusion')!='success' or current.get('status')!='completed':
        raise ValueError('The selected Unity certification is not complete and successful')
    if current.get('head_repository',{}).get('full_name')!=repo:
        raise ValueError('Forked/untrusted repository source')
    if current.get('head_branch') not in ('main','fix/player-feedback-context-ui-20261030'):
        raise ValueError('Branch not approved for this pilot')
    if not SHA.fullmatch(current.get('head_sha') or ''):
        raise ValueError('Invalid exact commit SHA')
    def artifact(run):
        artifacts=api(repo,token,f"/actions/runs/{run['id']}/artifacts?per_page=100")['artifacts']
        return next((a for a in artifacts if a['name']==f"eldoria-v41-native-{run['id']}"
                     and not a.get('expired') and 0<a.get('size_in_bytes',0)<=MAX_ARCHIVE),None)
    after=artifact(current)
    if not after: raise ValueError('Current run lacks exact run-ID V4.1 screenshot artifact')
    query=urlencode({'branch':current['head_branch'],'per_page':100,'status':'completed'})
    history=api(repo,token,f"/actions/workflows/{current['workflow_id']}/runs?{query}")['workflow_runs']
    older=sorted((x for x in history if x['id']!=current['id'] and x.get('workflow_id')==current['workflow_id']
                  and x.get('head_repository',{}).get('full_name')==repo
                  and x.get('status')=='completed' and x.get('conclusion')=='success'
                  and x.get('head_branch')==current['head_branch']
                  and SHA.fullmatch(x.get('head_sha') or '') and x['head_sha']!=current['head_sha']
                  and x.get('created_at','')<current.get('created_at','')),
                 key=lambda x:(x.get('created_at',''),x['id']),reverse=True)
    for before in older:
        old=artifact(before)
        if old:
            return (api(repo,token,f"/actions/artifacts/{old['id']}/zip",True),
                    api(repo,token,f"/actions/artifacts/{after['id']}/zip",True),
                    {'before_run':before['id'],'after_run':current['id'],'before_sha':before['head_sha'],
                     'after_sha':current['head_sha'],'branch':current['head_branch'],
                     'verified_github_metadata':True})
    raise ValueError('No earlier certified run on the same branch with screenshots')

def pngs(data):
    result={}; total=0
    with zipfile.ZipFile(io.BytesIO(data)) as z:
        for member in z.infolist():
            total+=member.file_size
            if total>MAX_UNPACKED or member.file_size>MAX_IMAGE:
                raise ValueError('Unsafe screenshot archive size')
            if not member.filename.lower().endswith('.png'): continue
            name=member.filename
            if name!=Path(name).name or '\\' in name or '..' in name or name in result:
                raise ValueError('Unsafe or repeated screenshot path')
            raw=z.read(member)
            with Image.open(io.BytesIO(raw)) as im: im.verify()
            result[name]=raw
            if len(result)>150:raise ValueError('Too many screenshots')
    return result

def analyze(before_zip,after_zip,meta,out):
    if not SHA.fullmatch(meta['before_sha']) or not SHA.fullmatch(meta['after_sha']) or meta['before_sha']==meta['after_sha']:
        raise ValueError('Distinct 40-character exact source SHAs required')
    out.mkdir(parents=True,exist_ok=True)
    old=pngs(before_zip);new=pngs(after_zip)
    common=sorted(set(old)&set(new));rows=[];bad=[]
    if not common:raise ValueError('No matching screenshot filenames')
    for name in common:
        with Image.open(io.BytesIO(old[name])) as a,Image.open(io.BytesIO(new[name])) as b:
            if a.size!=b.size:
                bad.append(name);continue
            a=a.convert('RGB');b=b.convert('RGB');same=old[name]==new[name]
            if a.width>600:
                h=max(1,round(a.height*600/a.width));a=a.resize((600,h));b=b.resize((600,h))
            diff=ImageChops.difference(a,b)
            mean=round(sum(ImageStat.Stat(diff).mean)/3,2)
            changed=round(sum(diff.convert('L').histogram()[16:])/(a.width*a.height)*100,2)
            rows.append({'name':name,'mean_rgb_delta':mean,'changed_luma_over15_pct':changed,
                         'identical_file':same,'before_sha256':hashlib.sha256(old[name]).hexdigest(),
                         'after_sha256':hashlib.sha256(new[name]).hexdigest(),
                         'status':'sin cambios' if same else 'cambio detectado',
                         'artistic_verdict':'NO EVALUADO'})
    report={'provenance':meta,'screens_compared':len(rows),'resolution_mismatches':bad,
            'screens_missing':sorted(set(old)-set(new)),'screens_added':sorted(set(new)-set(old)),
            'technical_status':'INCOMPLETE' if bad else 'COMPARED',
            'art_approved':False,'artistic_progress_machine_decided':False,
            'disclaimer':'Cambios de píxel no significan mejora artística. Revisión independiente obligatoria.',
            'rows':rows}
    (out/'report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    e=lambda x:html.escape(str(x),quote=True)
    table=''.join('<tr><td>'+e(x['name'])+'</td><td>'+e(x['status'])+'</td><td>'+e(x['changed_luma_over15_pct'])+' %</td><td>Sin valoración artística</td></tr>' for x in rows)
    page=('<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">'
          '<title>Eldoria: auditoría visual</title><style>body{font:16px system-ui;background:#101a23;color:#f2f4ee;margin:25px auto;max-width:1100px;padding:0 12px}'
          'table{border-collapse:collapse;width:100%;font-size:14px}th,td{padding:9px;border-bottom:1px solid #3c505e;text-align:left}th{color:#f6ca82}p{line-height:1.5}</style>'
          '<h1>Auditoría de capturas de Eldoria</h1><p>GitHub '+e(meta['before_sha'][:8])+' → '+e(meta['after_sha'][:8])+' · '+e(meta['branch'])+'</p>'
          '<p><b>'+str(len(rows))+' capturas comparadas.</b> Las métricas NO valoran la calidad ni aprueban arte.</p>'
          '<table><thead><tr><th>Captura</th><th>Cambio</th><th>Píxeles (luma)</th><th>Arte</th></tr></thead><tbody>'+table+'</tbody></table></html>')
    (out/'index.html').write_text(page,encoding='utf-8')
    return report

def main():
    p=argparse.ArgumentParser();p.add_argument('--run-id',type=int);p.add_argument('--before-zip',type=Path);p.add_argument('--after-zip',type=Path)
    p.add_argument('--before-sha');p.add_argument('--after-sha');p.add_argument('--output',type=Path,default=Path('visual-report'))
    a=p.parse_args()
    if a.run_id:
        try:
            before,after,meta=source_pair(a.run_id)
        except ValueError as exc:
            if str(exc) not in ('Current run lacks exact run-ID V4.1 screenshot artifact',
                                 'No earlier certified run on the same branch with screenshots'):
                raise
            a.output.mkdir(parents=True, exist_ok=True)
            result={'technical_status':'NO_COMPARABLE_EVIDENCE','art_approved':False,
                    'artistic_progress_machine_decided':False,'reason':str(exc),
                    'run_id':a.run_id,'manual_review_required':True}
            (a.output/'report.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
            (a.output/'index.html').write_text('<!doctype html><html lang="es"><meta charset="utf-8">'
               '<h1>Eldoria: comparación pendiente</h1><p>'+html.escape(str(exc))+'</p>'
               '<p>No se ha concedido aprobación artística.</p>',encoding='utf-8')
            print(json.dumps(result))
            return
    else:
        if not all((a.before_zip,a.after_zip,a.before_sha,a.after_sha)):
            p.error('offline mode requires both ZIPs and both SHAs')
        before=a.before_zip.read_bytes();after=a.after_zip.read_bytes()
        meta={'before_sha':a.before_sha,'after_sha':a.after_sha,'branch':'offline',
              'verified_github_metadata':False}
    report=analyze(before,after,meta,a.output)
    print(json.dumps({'status':report['technical_status'],'count':report['screens_compared'],
                     'mismatches':len(report['resolution_mismatches']),'art_approved':False},ensure_ascii=False))
    if report['technical_status']!='COMPARED':sys.exit(2)

if __name__=='__main__':main()
