#!/usr/bin/env python3
"""Archive actual source-verifiable Unity captures for the six-option premium city.

A original (32 props), B quiet (23), C Cuartel, D + Aserradero, E + Granero,
F + Cantera. Native captures and review JPGs are explicitly distinguished.
No fabricated render, score, device result or visual acceptance is recorded.
"""
from pathlib import Path
from PIL import Image,ImageDraw
from io import BytesIO
import hashlib,json,zipfile
import numpy as np

REPO=Path(__file__).resolve().parents[2]
CITY=REPO/'Unity/Assets/Experimentos/Ciudad2D'
QA=CITY/'QA'
REQ=Path(__file__).parent/'quality_v5_archive_request.json'
ZIP=Path('/tmp/ciudad2d-v5-source-unity.zip')
MODES=('A_32_original','B_23_quiet','C_23_premium',
       'D_23_cuartel_sawmill','E_23_cuartel_sawmill_granary',
       'F_23_all_four_premium')
SHA_CANTERA='dd6a4d7acd67d2de2e8ae53dc8d433e4e03bb136e7130d13130087de3e3f649b'
SHA_SHADOW='e42096dd255f96c0e3e979ae9321d3bf6ba20ccea52c53426c4f845e875fc8de'
def sha(data):return hashlib.sha256(data).hexdigest()

def main():
    req=json.loads(REQ.read_text())
    assert req['schema']==1 and req['run_id']>0 and req['artifact_id']>0
    assert len(req['source_sha'])==40 and ZIP.is_file() and ZIP.stat().st_size>20_000_000
    with zipfile.ZipFile(ZIP) as z:
        manifests=[n for n in z.namelist() if n.endswith('Ciudad2DQualityV5Evidence/evidence.json')]
        if len(manifests)!=1:raise ValueError('No uniquely identified original Unity V5 evidence')
        root=manifests[0].rsplit('/',1)[0]+'/'
        report=json.loads(z.read(manifests[0]).decode('utf-8-sig'))
        assert report['engine']=='Unity' and report['mode']=='Editor_RenderTexture'
        assert tuple(report['modes'])==MODES and len(report['captures'])==60
        assert report['quarry_candidate_sha256']==SHA_CANTERA
        assert report['quarry_shadow_sha256']==SHA_SHADOW
        assert report['bastion_I_1_to_4_V_5_unchanged'] is True
        assert report['gameplay_modified'] is False and report['main_modified'] is False
        assert report['art_approved'] is False and report['physical_iphone_test']=='PENDING'
        raw={}
        images={}
        for record in report['captures']:
            binary=z.read(root+record['file'])
            if sha(binary)!=record['sha256']:
                raise ValueError('Raw Unity SHA differs from native evidence '+record['file'])
            im=Image.open(BytesIO(binary));im.load()
            if im.format!='PNG' or im.size!=(record['width'],record['height']):
                raise ValueError('Screenshot image type or dimensions are false')
            raw[record['file']]=binary
            images[record['file']]=im.convert('RGB')
    assert len(images)==60
    QA.mkdir(parents=True,exist_ok=True)
    derivatives=[]
    differences={}
    for stage in (1,3,5):
        preview=Image.new('RGB',(390*6,893),(24,35,45))
        draw=ImageDraw.Draw(preview)
        variants=[]
        for i,mode in enumerate(MODES):
            name=f'{mode}_bastion{stage}_390x844_standard.png'
            im=images[name]
            preview.paste(im,(390*i,48))
            variants.append(np.asarray(im,dtype=np.int16))
            draw.text((390*i+8,16),f'B{stage} '+['A ORIG','B FONDO','C CUARTEL','D ASERR','E GRANERO','F CANTERA'][i],fill=(248,218,174))
        file=QA/f'ArtQualityV5_Bastion{stage}_A_to_F_REVIEW.jpg'
        if file.exists():raise ValueError('Previous art evidence must never be overwritten')
        preview.save(file,quality=94)
        derivatives.append({'file':str(file.relative_to(REPO)),'sha256':sha(file.read_bytes()),'not_a_native_capture':True})
        differences[str(stage)]={
            'original_A_vs_all_premium_F_mean_RGB':round(float(np.abs(variants[0]-variants[5]).mean()),5),
            'quarry_E_to_F_mean_RGB':round(float(np.abs(variants[4]-variants[5]).mean()),5)
        }
    # The canonical DemoStage(5,"cantera") is AVAILABLE (stoneworks=0),
    # not Built. Therefore the correct old six-mode gallery must NOT display
    # the new quarry in ANY stage. The separate V6 post-build QA suite handles
    # quality of its actual constructed appearance without changing progression.
    for stage in ('1','3','5'):
        if differences[stage]['quarry_E_to_F_mean_RGB']>.005:
            raise ValueError('Quarry art rendered on a canonical not-yet-built plot at stage '+stage)
    for name in ('cuartel','aserradero','granero','cantera'):
        overview=Image.new('RGB',(390*6,893),(24,35,45))
        draw=ImageDraw.Draw(overview)
        for i,mode in enumerate(MODES):
            im=images[f'{mode}_bastion5_1170x2532_{name}_close.png']
            overview.paste(im.resize((390,844),Image.Resampling.LANCZOS),(390*i,48))
            draw.text((390*i+8,16),f'{name.upper()} '+MODE_SHORT[i],fill=(249,215,173))
        dest=QA/f'ArtQualityV5_{name}_DETAIL_A_to_F_REVIEW.jpg'
        overview.save(dest,quality=94)
        derivatives.append({'file':str(dest.relative_to(REPO)),'sha256':sha(dest.read_bytes()),'not_a_native_capture':True})
    # Preserve nine original unmodified Unity PNGs permanently, separately
    # from the six-way review montages and the complete source artifact ZIP.
    keep=[
       'A_32_original_bastion5_390x844_standard.png',
       'F_23_all_four_premium_bastion1_390x844_standard.png',
       'F_23_all_four_premium_bastion3_390x844_standard.png',
       'F_23_all_four_premium_bastion5_390x844_standard.png',
       'F_23_all_four_premium_bastion5_1170x2532_standard.png',
       'F_23_all_four_premium_bastion5_844x390_standard.png',
       *[f'F_23_all_four_premium_bastion5_1170x2532_{name}_close.png'
         for name in ('cuartel','aserradero','granero','cantera')]
    ]
    originals=[]
    native_dir=QA/'NativeV5'
    native_dir.mkdir(parents=True,exist_ok=True)
    for name in keep:
        if name not in raw:raise ValueError('Native source missing '+name)
        dest=native_dir/name
        if dest.exists():raise ValueError('Refusing to overwrite exact native source '+name)
        dest.write_bytes(raw[name])
        originals.append({'file':str(dest.relative_to(REPO)),'sha256':sha(raw[name])})
    proof={
        'schema':1,'source_run_id':req['run_id'],'source_artifact_id':req['artifact_id'],
        'source_commit_sha':req['source_sha'],'source_zip_sha256':sha(ZIP.read_bytes()),
        'engine':'Unity','mode':'Editor_RenderTexture',
        'verified_original_frame_count':len(images),
        'all_original_capture_sha256':report['captures'],
        'archived_native_png_files':originals,
        'derived_review_jpg_files':derivatives,
        'quarry_source_sha256':SHA_CANTERA,'quarry_contact_shadow_sha256':SHA_SHADOW,
        'screen_differences_are_not_art_scores':differences,
        'original_control_A_retained':True,'no_gameplay_or_main_modifications':True,
        'canonical_stage5_quarry_status':'Available, not Built',
        'quarry_invisible_in_this_gallery_by_design':True,
        'constructed_quarry_visual_qa':'SEPARATE_V6_BUILT_FIXTURE',
        'playmode_of_new_premium_scene':'SEPARATE_TEST',
        'physical_iphone':'NOT_TESTED',
        'premium_art_approved':False
    }
    (QA/'ArtQualityV5_NATIVE_EVIDENCE.json').write_text(json.dumps(proof,indent=2,ensure_ascii=False)+'\n')
    print('CITY2D_60_SOURCE_SHA_GENUINE_UNITY_ART_V5_ARCHIVE_PASS, ART_PENDING',differences)
MODE_SHORT=['A','B','C','D','E','F']
if __name__=='__main__':main()
