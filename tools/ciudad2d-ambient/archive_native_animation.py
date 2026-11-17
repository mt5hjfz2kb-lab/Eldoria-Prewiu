#!/usr/bin/env python3
"""Archive original Unity living-city capture evidence without fabricating frames.

Source is the exact ZIP from a lease-checked Unity Editor run; all 26 source
PNGs are hashed before deriving contact-sheet/GIF/WebP for human review.
"""
import hashlib,io,json,zipfile
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
OUT=CITY/'QA'
CONFIG=Path(__file__).parent/'archive_native_request.json'
ZIP=Path('/tmp/ciudad2d-ambient-native-source.zip')

def sha(data):return hashlib.sha256(data).hexdigest()
def main():
    req=json.loads(CONFIG.read_text())
    assert req['schema']==1 and req['run_id']>0 and req['artifact_id']>0
    assert len(req['source_sha'])==40
    assert ZIP.is_file() and ZIP.stat().st_size>8_000_000
    with zipfile.ZipFile(ZIP) as z:
        exact=[name for name in z.namelist() if name.endswith('Ciudad2DAmbientEvidence/evidence.json')]
        if len(exact)!=1:raise ValueError('No unique authentic Unity report in artifact')
        pref=exact[0].rsplit('/',1)[0]+'/'
        report=json.loads(z.read(exact[0]).decode('utf-8-sig'))
        assert report['engine']=='Unity'
        assert report['mode']=='Editor_RenderTexture_two_times'
        assert len(report['captures'])==10 and len(report['animation_frames'])==16
        assert report['no_unique_bastion_II_III_IV_required'] is True
        assert report['gameplay_or_saves_modified'] is False and report['main_modified'] is False
        assert report['visual_art_approved'] is False and report['physical_device_test']=='PENDING'
        photos={}
        for rec in report['captures']+report['animation_frames']:
            image_bytes=z.read(pref+rec['file'])
            if sha(image_bytes)!=rec['sha256']:
                raise ValueError('Original native frame SHA mismatch: '+rec['file'])
            im=Image.open(io.BytesIO(image_bytes));im.load()
            assert im.size==(rec['width'],rec['height']) and im.format=='PNG'
            photos[rec['file']]=im.convert('RGB')
    assert len(photos)==26
    OUT.mkdir(parents=True,exist_ok=True)
    if any((OUT/name).is_file() for name in ('AmbientLiving_I_III_V_NATIVE.jpg',
        'AmbientLiving_V_Motion_NATIVE.webp','AmbientLiving_V_Motion_NATIVE.gif')):
        raise ValueError('Refusing to overwrite archived visual proof')

    # Single time, genuine Unity screenshots for I/III/V side by side.
    board=Image.new('RGB',(3*390,894),(25,32,43))
    draw=ImageDraw.Draw(board)
    for idx,stage in enumerate((1,3,5)):
        title='BASTION '+str(stage)+'  |  CIUDAD 2.5D'
        draw.text((idx*390+12,15),title,fill=(230,209,164))
        im=photos[f'bastion{stage}_390x844_t1.png']
        board.paste(im,(idx*390,45))
    sheet=OUT/'AmbientLiving_I_III_V_NATIVE.jpg'
    board.save(sheet,quality=94,subsampling=0)

    # A continuous genuine 16-frame Unity camera recording, no added animation:
    names=[f'living_bastion5_390x844_f{i:02d}.png' for i in range(16)]
    frames=[photos[name] for name in names]
    distinct=len({sha(frame.tobytes()) for frame in frames})
    if distinct<12:raise ValueError('Real source frames do not show sufficient motion')
    webp=OUT/'AmbientLiving_V_Motion_NATIVE.webp'
    frames[0].save(webp,save_all=True,append_images=frames[1:],
        duration=340,loop=0,format='WEBP',quality=90,method=6)
    gif=OUT/'AmbientLiving_V_Motion_NATIVE.gif'
    # GIF has 256-color limitations. WebP is preferable for judging fidelity.
    palette=[f.quantize(colors=230,method=Image.Quantize.MEDIANCUT,
        dither=Image.Dither.NONE) for f in frames]
    palette[0].save(gif,save_all=True,append_images=palette[1:],
        duration=340,loop=0,optimize=True,disposal=2)
    changes=[]
    for i in range(1,len(frames)):
        a=np.asarray(frames[i-1],dtype=np.int16)
        b=np.asarray(frames[i],dtype=np.int16)
        changes.append(int(np.count_nonzero(np.any(a!=b,axis=2))))
    if min(changes)<20:raise ValueError('Some Unity source frames show no measurable movement')
    proof={
        'schema':1,'engine':'Unity','mode':'Editor_RenderTexture',
        'source_run_id':req['run_id'],'source_artifact_id':req['artifact_id'],
        'source_commit_sha':req['source_sha'],'source_artifact_zip_sha256':sha(ZIP.read_bytes()),
        'source_captures':report['captures'],
        'source_animation_frames':report['animation_frames'],
        'derivatives':[
            {'file':str(p.relative_to(ROOT)),'sha256':sha(p.read_bytes()),'size_bytes':p.stat().st_size}
            for p in (sheet,webp,gif)],
        'original_frame_count':26,'loop_frame_count':16,
        'min_changed_pixels_between_adjacent_real_frames':min(changes),
        'max_changed_pixels_between_adjacent_real_frames':max(changes),
        'no_synthesized_frames':True,
        'images_derived_from_exact_Unity_captures':True,
        'art_approved':False,'playmode_animation_performance_not_tested':True,
        'physical_iphone_not_tested':True,'gameplay_unchanged':True
    }
    manifest=OUT/'AmbientLiving_NATIVE_PROVENANCE.json'
    manifest.write_text(json.dumps(proof,indent=2,ensure_ascii=False)+'\n')
    print('CITY2D_REAL_UNITY_26_SHA_FRAMES_PRESERVED_16_FRAME_ANIMATION',proof['derivatives'])

if __name__=='__main__':main()
