#!/usr/bin/env python3
"""Independent visibility gate for real Unity city/treasure A/B pairs.

Only native pixel evidence can pass. Passing is *not* artistic approval or
proof of live PR173, saves, progression, audio or physical devices.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
from PIL import Image

SIZES = ((1600,900),(900,1600),(844,390),(390,844))
MIN_VISIBLE_LUMINANCE = 0.27
MIN_BASTION_STRUCTURE_CORRELATION = 0.68

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

def masonry_visibility(image, aperture):
    """Native studio subject check; never discover a convenient bright region."""
    ox,oy,aw,ah=aperture
    w,h=image.size
    require(all(isinstance(v,(int,float)) and math.isfinite(v) for v in aperture)
            and aw>100 and ah>100 and ox>=0 and oy>=0 and ox+aw<=w+1 and oy+ah<=h+1,
            'actual projected masonry aperture is invalid')
    p=image.convert('RGB').load()
    step=max(1,int(min(aw,ah)//55))
    pixels=[p[x,y] for y in range(max(0,round(oy)),min(h,round(oy+ah)),step)
            for x in range(max(0,round(ox)),min(w,round(ox+aw)),step)]
    lit=sum(sum(c)>=48 for c in pixels)
    bins=len({tuple(v>>4 for v in c) for c in pixels})
    require(len(pixels)>=150 and lit>len(pixels)/6 and bins>=36,
            f'actual studio masonry absent, dark or flat: {lit}/{len(pixels)} lit, {bins} bins')
    return {'native_masonry_samples':len(pixels),'native_masonry_lit_fraction':round(lit/len(pixels),4),
            'native_masonry_color_bins':bins}

def masonry_luminance_retention(baseline, ceremony, aperture):
    require(baseline.size==ceremony.size,'studio A/B viewport mismatch')
    ox,oy,aw,ah=aperture
    a,b=baseline.convert('RGB').load(),ceremony.convert('RGB').load()
    samples=[]
    step=max(1,int(min(aw,ah)//55))
    for y in range(round(oy),round(oy+ah),step):
        for x in range(round(ox),round(ox+aw),step):
            r,g,blue=a[x,y];old=r*.2126+g*.7152+blue*.0722
            if old<=40:continue
            r,g,blue=b[x,y];new=r*.2126+g*.7152+blue*.0722
            samples.append((old,new))
    require(len(samples)>=150,'studio has too few actual baseline masonry pixels')
    ratio=sum(n for _,n in samples)/sum(o for o,_ in samples)
    require(ratio>=MIN_VISIBLE_LUMINANCE,'studio masonry concealed by ceremony')
    return {'masonry_visible_luminance_ratio':round(ratio,4)}

def bastion_structure_retention(baseline, ceremony, aperture=None):
    """Require the ACTUAL focused Bastion masonry to survive foreground painting.

    Run 43005395878 had a valid camera, 20 PNGs and enough perimeter light,
    yet its flattened arch obscured the building in the image center. A
    fixed dark/bronze translucent veil preserves the original masonry's
    spatial variation (high correlation); a pasted arch does not. This is
    measured on the real Unity A/B pixels, never a fabricated mockup.
    """
    require(baseline.size == ceremony.size, 'Bastion camera A/B viewport differs')
    w,h = baseline.size
    old,new = baseline.convert('RGB').load(),ceremony.convert('RGB').load()
    step = max(1,min(w,h)//130)
    values = []
    # The picture may occupy a reserved left column. Both frames must use
    # its actual measured Unity rectangle and IDENTICAL projection/texture.
    ox,oy,aw,ah = aperture if aperture is not None else (0,0,w,h)
    require(aw>0 and ah>0 and ox>=0 and oy>=0 and ox+aw<=w+1 and oy+ah<=h+1,
            'Bastion measured aperture lies outside actual viewport')
    for y in range(round(oy+ah*.37),round(oy+ah*.63),step):
        for x in range(round(ox+aw*.38),round(ox+aw*.62),step):
            r,g,b = old[x,y]
            a = r*.2126+g*.7152+b*.0722
            if a < 45:
                continue
            r,g,b = new[x,y]
            c = r*.2126+g*.7152+b*.0722
            values.append((a,c))
    n = len(values)
    require(n >= 200,f'no usable real Bastion masonry samples: {w}x{h}, {n}')
    mx = sum(a for a,_ in values)/n
    my = sum(b for _,b in values)/n
    vx = sum((a-mx)**2 for a,_ in values)
    vy = sum((b-my)**2 for _,b in values)
    cov = sum((a-mx)*(b-my) for a,b in values)
    correlation = cov/math.sqrt(vx*vy) if vx>1e-9 and vy>1e-9 else 0.
    require(correlation >= MIN_BASTION_STRUCTURE_CORRELATION,
            f'actual Bastion architecture concealed by stage/ink: {w}x{h}, '
            f'central native-pixel correlation={correlation:.3f} '
            f'(<{MIN_BASTION_STRUCTURE_CORRELATION:.2f}). '
            'Move/crop painted trim aside; do not invent a new building.')
    return {'focused_bastion_pixels':n,
            'focused_real_building_correlation':round(correlation,4)}

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
    require(report.get('cityRenderers',0)>=30 and report.get('pngCount')==20,
            'city graphics incomplete')
    shots=report.get('shots',[])
    require(len(shots)==20,'native report must have twenty genuine city/context PNG')
    names={s.get('file'):s for s in shots}
    require(len(names)==20,'duplicate city image in report')
    results=[]
    for w,h in SIZES:
        pair=[]
        for variant in ('actual-city-baseline','authored-calendar-context',
                        'authored-bastion-visual-only',
                        'real-bastion-camera-baseline','real-bastion-camera-ceremony'):
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
        baseline, calendar, bastion, focused_baseline, focused_bastion = pair
        require(hashlib.sha256(baseline.tobytes()).digest() !=
                hashlib.sha256(focused_baseline.tobytes()).digest(),
                f'genuine Bastion camera did not move: {w}x{h}')
        daily_result = visibility(baseline, calendar)
        daily_result['variant'] = 'calendar'
        require(report.get('bastionPresentationMethod')=='ACTUAL_BUILT_CITY_CINEMATIC_CONTEXT'
                and report.get('actualBuiltContextProofs')==8 and report.get('actualCityContextPixelProofs')==8 and report.get('sceneEnvironmentRestored') is True,
                'studio requires eight restored actual-built source captures')
        metadata=names[f'real-city-daily-authored-bastion-visual-only-{w}x{h}.png']
        subject=tuple(metadata.get(k,0) for k in ('subjectX','subjectY','subjectW','subjectH'))
        bastion_result = {'size':f'{w}x{h}',**masonry_visibility(bastion,subject)}
        bastion_result['variant'] = 'bastion_visual_only'
        hero_result = {'size':f'{w}x{h}'}
        hero_result['variant'] = 'canonical_bastion_camera_closeup'
        aligned=[]
        for variant in ('real-bastion-camera-baseline','real-bastion-camera-ceremony'):
            metadata=names[f'real-city-daily-{variant}-{w}x{h}.png']
            rect=tuple(metadata.get(k,0) for k in ('subjectX','subjectY','subjectW','subjectH'))
            require(all(isinstance(v,(int,float)) and math.isfinite(v) for v in rect)
                    and rect[2]>100 and rect[3]>100,
                    f'actual Unity Bastion aperture metadata missing: {w}x{h}')
            aligned.append(rect)
        require(all(abs(a-b)<.1 for a,b in zip(*aligned)),
                f'Bastion A/B picture aperture moved: {w}x{h}')
        masonry_visibility(focused_baseline,aligned[0])
        hero_result.update(masonry_visibility(focused_bastion,aligned[0]))
        hero_result.update(masonry_luminance_retention(focused_baseline,focused_bastion,aligned[0]))
        hero_result.update(bastion_structure_retention(
            focused_baseline,focused_bastion,aligned[0]))
        hero_result['actual_unity_aperture']=aligned[0]
        results.extend((daily_result, bastion_result, hero_result))
    require(set(names)=={f'real-city-daily-{v}-{w}x{h}.png'
        for w,h in SIZES for v in ('actual-city-baseline','authored-calendar-context',
            'authored-bastion-visual-only', 'real-bastion-camera-baseline',
            'real-bastion-camera-ceremony')},
        'extra unexplained city artifact')
    out=Path(output)
    out.mkdir(parents=True,exist_ok=True)
    (out/'city-visibility.json').write_text(json.dumps({
        'source_sha':source_sha,'native_city_pairs':12,
        'visual_only_bastion_studies':8,
        'artApproved':False,'fullSceneVerified':False,'pr173Integrated':False,
        'minimum_relative_luminance':MIN_VISIBLE_LUMINANCE,
        'minimum_real_bastion_correlation':MIN_BASTION_STRUCTURE_CORRELATION,
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
    # Regress the precise native failure: an arch can leave the perimeter
    # bright while erasing the castle inside the focused camera aperture.
    masonry=Image.new('RGB',(256,128))
    preserved=Image.new('RGB',(256,128))
    for yy in range(128):
        for xx in range(256):
            pattern=(xx*17+yy*13)%83
            col=(85+pattern,72+pattern,68+pattern)
            masonry.putpixel((xx,yy),col)
            preserved.putpixel((xx,yy),tuple(round(v*.46) for v in col))
    require(bastion_structure_retention(masonry,preserved)
            ['focused_real_building_correlation']>.95,
            'a transparent consistent bronze veil preserves real masonry')
    replaced=preserved.copy()
    for yy in range(round(128*.37),round(128*.63)):
        for xx in range(round(256*.38),round(256*.62)):
            replaced.putpixel((xx,yy),(31,26,21))
    try:
        bastion_structure_retention(masonry,replaced)
    except ValueError as exc:
        require('architecture concealed' in str(exc),
                'obscured Bastion must fail because its structure is lost')
    else:
        raise AssertionError('painted arch must not replace real Bastion pixels')
    for color in ((0,0,0),(190,140,90)):
        try:
            masonry_visibility(Image.new('RGB',(256,256),color),(10,10,220,220))
        except ValueError:pass
        else:raise AssertionError('blank or flat studio must fail')
    studio=Image.new('RGB',(256,256),(0,0,0))
    for y in range(20,240):
        for x in range(20,240):
            studio.putpixel((x,y),(48+(x%13)*16,40+(y%13)*16,32+((x+y)%13)*16))
    require(masonry_visibility(studio,(20,20,220,220))['native_masonry_color_bins']>=36,
            'actual masonry ROI cannot be rejected for intentional surrounding negative space')
    dark=Image.new('RGB',(256,256),(0,0,0))
    try:masonry_luminance_retention(studio,dark,(20,20,220,220))
    except ValueError:pass
    else:raise AssertionError('ceremony concealing masonry must fail')
    print('PASS city visibility selftest: dark overlay AND fake arch rejected')

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

