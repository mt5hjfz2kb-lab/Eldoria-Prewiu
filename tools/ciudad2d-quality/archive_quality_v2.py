#!/usr/bin/env python3
"""Permanently archive exact source-verified real Unity quality A/B/C.
Never claim a montage is a new Unity screenshot, hardware test or art approval.
"""
import hashlib,io,json,zipfile
from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/Assets/Experimentos/Ciudad2D/QA'
CFG=Path(__file__).parent/'quality_v2_archive_request.json'
SOURCE=Path('/tmp/city2d-quality-v2-native.zip')
def sha(binary):return hashlib.sha256(binary).hexdigest()
def main():
    req=json.loads(CFG.read_text())
    assert req['schema']==1 and len(req['commit_sha'])==40
    assert req['run_id']>0 and req['artifact_id']>0
    assert SOURCE.is_file() and SOURCE.stat().st_size>15_000_000
    with zipfile.ZipFile(SOURCE) as z:
        matches=[n for n in z.namelist() if n.endswith('Ciudad2DQualityV2Evidence/evidence.json')]
        if len(matches)!=1:raise ValueError('No unique genuine Unity quality V2 report')
        root=matches[0].rsplit('/',1)[0]+'/'
        report=json.loads(z.read(matches[0]).decode('utf-8-sig'))
        assert report['engine']=='Unity' and report['mode']=='Editor_RenderTexture'
        assert report['modes']==['A_32_original','B_23_quiet','C_23_premium']
        assert report['shadow_separate'] is True
        assert report['removed_duplicate_background_props']==9
        assert report['quieter_props']==23
        assert report['legacy_original_props']==32
        assert report['main_modified']==report['gameplay_modified']==report['art_approved']==False
        assert report['physical_iphone_test']=='PENDING'
        assert len(report['captures'])==18
        photos={}
        for rec in report['captures']:
            path=root+rec['file']
            data=z.read(path)
            if sha(data)!=rec['sha256']:raise ValueError('Native Unity image hash mismatch: '+path)
            im=Image.open(io.BytesIO(data));im.load()
            assert im.format=='PNG' and im.size==(rec['width'],rec['height'])
            photos[rec['file']]=im.convert('RGB')
    assert len(photos)==18
    modes=report['modes']
    names={3:'ArtQualityV2_BastionIII_A_B_C.jpg',
           5:'ArtQualityV2_BastionV_A_B_C.jpg'}
    deriv=[]
    changed={}
    for stage,path in names.items():
        src=[photos[f'{mode}_bastion{stage}_390x844_standard.png'] for mode in modes]
        sheet=Image.new('RGB',(1170,890),(24,34,43))
        draw=ImageDraw.Draw(sheet)
        for i,(name,frame) in enumerate(zip(['A / 32 props','B / 23 props','C / 23 + Cuartel premium'],src)):
            sheet.paste(frame,(390*i,46))
            draw.text((390*i+12,15),f'Bastion {stage} - '+name,fill=(233,213,174))
        out=OUT/path
        if out.exists():raise ValueError('No overwrite of existing art review')
        sheet.save(out,quality=95,subsampling=0)
        deriv.append({'file':str(out.relative_to(ROOT)),'sha256':sha(out.read_bytes())})
        arr=[np.asarray(im,dtype=np.int16) for im in src]
        changed[str(stage)]={
            'pixel_MAE_A_to_B':round(float(np.abs(arr[0]-arr[1]).mean()),5),
            'pixel_MAE_B_to_C':round(float(np.abs(arr[1]-arr[2]).mean()),5)}
        if changed[str(stage)]['pixel_MAE_B_to_C']<.01:raise ValueError('Premium Cuartel failed to render')
    frames=[photos[f'{mode}_bastion5_1170x2532_cuartel_close.png'] for mode in modes]
    close=Image.new('RGB',(1170,890),(24,34,43))
    dd=ImageDraw.Draw(close)
    for i,frame in enumerate(frames):
        close.paste(frame.resize((390,844),Image.Resampling.LANCZOS),(i*390,46))
        dd.text((i*390+12,15),['A / Original','B / Fondo menos denso','C / Cuartel nuevo'][i],fill=(233,213,174))
    closefile=OUT/'ArtQualityV2_CuartelDetail_A_B_C.jpg'
    close.save(closefile,quality=95)
    deriv.append({'file':str(closefile.relative_to(ROOT)),'sha256':sha(closefile.read_bytes())})
    result={
       'schema':1,'engine':'Unity','capture_mode':'Editor_RenderTexture',
       'source_run_id':req['run_id'],'source_artifact_id':req['artifact_id'],
       'source_branch_head_sha':req['commit_sha'],'source_zip_sha256':sha(SOURCE.read_bytes()),
       'source_original_pngs':report['captures'],'number_authentic_pngs':18,
       'derived_review_jpgs_not_native_captures':deriv,
       'density_test':{'original_props':32,'quiet_props':23,'deleted_duplicate_plants':9,
                       'differences':changed},
       'source_cuartel_sha256':report['premium_cuartel_original_sha256'],
       'source_shadow_sha256':report['shadow_source_sha256'],
       'no_iPhone_physical_claim':True,'candidate_not_playmode_touch_tested':True,
       'source_untouched':True,'art_approved':False,
       'limitations':'Aserradero, Granero and Cantera remain original sprites with geometric plinths; premium source consistency for these is still pending.'}
    (OUT/'ArtQualityV2_NATIVE_EVIDENCE.json').write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n')
    print('REAL_UNITY_18_MATCHED_SHA_A_B_C_REVIEW_ARCHIVED_ART_NOT_APPROVED',changed)
if __name__=='__main__':main()
