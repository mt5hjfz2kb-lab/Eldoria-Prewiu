#!/usr/bin/env python3
"""Independent visibility gate for real Unity city/treasure A/B pairs.

Only native pixel evidence can pass. Passing is *not* artistic approval or
proof of live PR173, saves, progression, audio or physical devices.
"""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

SIZES = ((1600,900),(900,1600),(844,390),(390,844))
MIN_VISIBLE_LUMINANCE = 0.27

def require(ok, reason):
    if not ok:
        raise ValueError('CITY_ART_VISIBILITY_GATE: ' + reason)

def visibility(baseline, calendar):
    """Compare pixels OUTSIDE the solid slate modal, from paired same-camera shots."""
    require(baseline.size == calendar.size, 'unmatched native camera viewports')
    w,h = baseline.size
    a = baseline.convert('RGB')
    b = calendar.convert('RGB')
    pa,pb = a.load(),b.load()
    bright,baseline_luma,calendar_luma = 0,0.,0.
    # Inner dialog may be opaque by design. The 4% perimeter is safely outside
    # its authored frame even at 390 px width, and still contains real city.
    for y in range(0,h,5):
        for x in range(0,w,5):
            if .04*w <= x < .96*w and .04*h <= y < .96*h:
                continue
            ra,ga,ba=pa[x,y]
            la=ra*.2126+ga*.7152+ba*.0722
            if la <= 40:  # exclude black water/night and unlit/empty patches
                continue
            rb,gb,bb=pb[x,y]
            lb=rb*.2126+gb*.7152+bb*.0722
            bright += 1
            baseline_luma += la
            calendar_luma += lb
    require(bright >= 150, f'city baseline too sparse for visibility: {w}x{h} / {bright}')
    ratio=calendar_luma/baseline_luma
    require(ratio >= MIN_VISIBLE_LUMINANCE,
        f'city almost disappears behind calendar: {w}x{h} visible={ratio:.3f} (<{MIN_VISIBLE_LUMINANCE:.2f})')
    return {'size':f'{w}x{h}','samples':bright,'visible_luminance_ratio':round(ratio,4)}

def audit(folder,source_sha,output):
    root=Path(folder)
    city=root/'city'
    report=json.loads((city/'city-art-report.json').read_text(encoding='utf-8-sig'))
    require(report.get('source')==source_sha,'city report does not match exact PR181 HEAD')
    require(report.get('status')=='CITY_ART_CONTEXT_RENDERED_NOT_GAMEPLAY_CERTIFIED',
            'native canonical city art test did not complete')
    for flag in ('artApproved','fullSceneVerified','pr173Integrated','iosVerified'):
        require(report.get(flag) is False, f'false certification: {flag}')
    require(report.get('realGatewayClaims')==0,'art-only scene paid a reward')
    require(report.get('citySceneOpened') is True and report.get('cityCameraUsed') is True,
            'non-canonical scene or camera')
    require(report.get('cityRenderers',0)>=30 and report.get('pngCount')==12,
            'city graphics incomplete')
    shots=report.get('shots',[])
    require(len(shots)==12,'native report must have twelve genuine city/context PNG')
    names={s.get('file'):s for s in shots}
    require(len(names)==12,'duplicate city image in report')
    results=[]
    for w,h in SIZES:
        pair=[]
        for variant in ('actual-city-baseline','authored-calendar-context',
                        'authored-bastion-visual-only'):
            name=f'real-city-daily-{variant}-{w}x{h}.png'
            shot=names.get(name)
            require(shot is not None and shot.get('variant')==variant,
                    f'missing actual Unity city image {name}')
            path=city/name
            require(path.is_file() and path.stat().st_size==shot.get('bytes'),
                    f'actual Unity image absent or wrong byte size: {name}')
            require(hashlib.sha256(path.read_bytes()).hexdigest()==shot.get('sha256'),
                    f'native city image hash mismatch: {name}')
            im=Image.open(path)
            require(im.size==(w,h) and shot.get('width')==w and shot.get('height')==h,
                    f'native camera crop mismatch: {name}')
            pair.append(im.copy())
            im.close()
        baseline, calendar, bastion = pair
        daily_result = visibility(baseline, calendar)
        daily_result['variant'] = 'calendar'
        bastion_result = visibility(baseline, bastion)
        bastion_result['variant'] = 'bastion_visual_only'
        results.extend((daily_result, bastion_result))
    require(set(names)=={f'real-city-daily-{v}-{w}x{h}.png'
        for w,h in SIZES for v in ('actual-city-baseline','authored-calendar-context',
            'authored-bastion-visual-only')},
        'extra unexplained city artifact')
    out=Path(output)
    out.mkdir(parents=True,exist_ok=True)
    (out/'city-visibility.json').write_text(json.dumps({
        'source_sha':source_sha,'native_city_pairs':8,
        'visual_only_bastion_studies':4,
        'artApproved':False,'fullSceneVerified':False,'pr173Integrated':False,
        'minimum_relative_luminance':MIN_VISIBLE_LUMINANCE,
        'results':results},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('PASS genuine-city visibility (technical minimum, NOT artistic approval):',results)

def selftest():
    w,h=256,128
    baseline=Image.new('RGB',(w,h),(180,143,85))
    corrected=Image.new('RGB',(w,h),(64,50,30))
    old=Image.new('RGB',(w,h),(25,20,12))
    require(visibility(baseline,corrected)['visible_luminance_ratio']>MIN_VISIBLE_LUMINANCE,
            'expected improved A/B synthetic to pass')
    try:
        visibility(baseline,old)
    except ValueError as exc:
        require('almost disappears' in str(exc),'old dark synthetic rejected for wrong reason')
    else:
        raise AssertionError('old near-black screen must fail native visibility gate')
    print('PASS city visibility selftest: deliberately dark overlay rejected')

if __name__=='__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--selftest',action='store_true')
    p.add_argument('--input',default='native-originals')
    p.add_argument('--source-sha')
    p.add_argument('--output',default='native-art-qa')
    opt=p.parse_args()
    if opt.selftest:
        selftest()
    else:
        require(bool(opt.source_sha),'exact source SHA required')
        audit(opt.input,opt.source_sha,opt.output)
