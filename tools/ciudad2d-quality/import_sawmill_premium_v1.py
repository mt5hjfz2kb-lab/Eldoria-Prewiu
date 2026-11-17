#!/usr/bin/env python3
"""Preserve the existing isolated premium Valoria sawmill as three immutable PNGs.
No changes to original images, gameplay, main, or construction rules. All source
bytes checked; the alpha was prepared locally with a roof-only enclosed-hole fill
to preserve blue dark slate that initially keyed out against the charcoal matte.
The art is a CANDIDATE; code success is never visual or hardware acceptance.
"""
from pathlib import Path
from urllib.request import Request,urlopen
from urllib.parse import urlsplit
from io import BytesIO
import hashlib,json,re
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
CITY=ROOT/'Unity/Assets/Experimentos/Ciudad2D'
CAND=CITY/'Sprites/Candidates'
QA=CITY/'QA'
INPUT=Path(__file__).parent/'pending_sawmill_premium.json'
ART={
    'source':{
        'dataset':'/datasets/eda3cd16-b593-4053-bf42-31de4aba2b4f.png',
        'sha':'4bbbb369b6bf873f801ddc14366cb1231eedafc3414546f1642024845338533a',
        'size':(1536,1024),'mode':'RGB','filename':'Aserradero_Premium_SourceV1.png'},
    'rgba':{
        'dataset':'/datasets/5c1dbe53-fc26-4147-acbb-8cee4572b15a.png',
        'sha':'4fa333a21db0150179b6dd1158b559623d52765de90c82cc8b807ea30eaffa9e',
        'size':(1507,1507),'mode':'RGBA','filename':'Aserradero_Premium_V1.png'},
    'shadow':{
        'dataset':'/datasets/b74c34cf-fcd8-4ce0-b407-528a7a4d6b43.png',
        'sha':'900eeab27836e647671820287a38fefb0050cea64bf991f53ad01bcc27a4ae7c',
        'size':(1507,1507),'mode':'RGBA','filename':'Aserradero_Premium_ShadowV1.png'}
}
def digest(data):return hashlib.sha256(data).hexdigest()
def download(url,identity):
    info=urlsplit(url)
    if info.scheme!='https' or info.hostname!='d2jqrm6oza8nb6.cloudfront.net' or info.path!=identity:
        raise ValueError('Unexpected asset download endpoint')
    with urlopen(Request(url,headers={'User-Agent':'Eldoria-Valoria-ArtArchive/1.0'}),timeout=85) as reply:
        return reply.read(9_000_001)
def meta(path):
    original=(CITY/'Sprites/Cuartel.png.meta').read_text(encoding='utf-8')
    guid=hashlib.md5(str(path.relative_to(ROOT)).encode('utf-8')).hexdigest()
    original=re.sub(r'guid: [0-9a-f]{32}','guid: '+guid,original,count=1)
    original=re.sub(r'spriteAlignment: \d+','spriteAlignment: 7',original)
    original=re.sub(r'textureCompression: \d+','textureCompression: 0',original)
    path.with_suffix('.png.meta').write_text(original)
def main():
    req=json.loads(INPUT.read_text())
    assert req['schema']==1 and set(req['urls'])==set(ART)
    CAND.mkdir(parents=True,exist_ok=True);QA.mkdir(parents=True,exist_ok=True)
    result={}
    for key,cfg in ART.items():
        raw=download(req['urls'][key],cfg['dataset'])
        if len(raw)>9_000_000 or digest(raw)!=cfg['sha']:raise ValueError('Original source changed: '+key)
        image=Image.open(BytesIO(raw));image.load()
        if image.format!='PNG' or image.mode!=cfg['mode'] or image.size!=cfg['size']:
            raise ValueError('Invalid source image type/dimensions: '+key)
        path=CAND/cfg['filename'];path.write_bytes(raw)
        meta(path)
        result[key]={'file':str(path.relative_to(ROOT)),
            'sha256':cfg['sha'],'dimensions':list(image.size),'mode':image.mode,
            'hosted_source_asset_id':{'source':'74beb27e-1a19-44fa-afa4-6671db682fb7',
                                      'rgba':'86081b14-8699-4969-8f26-c1f33c20ea62',
                                      'shadow':'ccc3da73-f055-4802-97df-2b41cb388ea0'}[key]}
    rgba=Image.open(CAND/ART['rgba']['filename'])
    shadow=Image.open(CAND/ART['shadow']['filename'])
    a=rgba.getchannel('A')
    if a.getextrema()!=(0,255) or max(a.getpixel(xy) for xy in
       ((0,0),(1506,0),(0,1506),(1506,1506)))>8:
        raise ValueError('PNG has opaque matte border or no real alpha')
    if shadow.getchannel('A').getextrema()[0]!=0 or \
       shadow.getchannel('A').getextrema()[1]>110:
        raise ValueError('Shadow is not a semi-transparent contact layer')
    for name,c in [('green',(79,102,70,255)),('grey',(106,109,111,255))]:
        board=Image.new('RGBA',(1507,1507),c)
        board.alpha_composite(shadow);board.alpha_composite(rgba)
        board.thumbnail((760,760),Image.Resampling.LANCZOS)
        p=QA/('AserraderoPremiumV1_'+name+'_OFFLINE_NOT_UNITY.jpg')
        board.convert('RGB').save(p,quality=94)
        result[name+'_review_sha256']=digest(p.read_bytes())
    record={
        'schema':1,'status':'SOURCE_AND_ALPHA_VERIFIED_ART_NOT_APPROVED',
        'origin_generation':'93593bfc-8521-46eb-a402-4a71a563ae6f',
        'generator':'first_party_image_generation',
        'direct_master_reference_conditioning_proven':False,
        'visual_reference':'ArtBible/Masters/Bastion_V_MASTER.png and Cuartel Premium V2',
        'original_workflow':'source 1536x1024 neutral charcoal illustration; use 15 RGB max-channel background distance; largest 8-connected component; roof-only enclosed hole fill y<530; 9 px close; alpha Gaussian 0.6 px; bbox margin 38, bottom center; separate alpha-footprint-based shadow',
        'calibrated_matte_rgb':[14,14,20],'fill_dark_roof_holes_px':30542,
        'origin_original_preserved':True,'early_partial_cutout_not_promoted':True,
        'source_photos_and_RGBA':result,
        'reproducible_specification':True,
        'exact_bytes_source_sha_fixed':True,
        'sawmill_function':'Open mechanical saw, wood wheel, logs, sawdust, weathered oak and deep blue slate',
        'original_City2D_buildings_untouched':True,
        'existing_art_ForgV3_CuartelV2_untouched':True,
        'not_a_proof_of_uniform_3_buildings':True,
        'in_Unity':'PENDING',
        'art_approved':False,
        'physical_iPhone':'PENDING'}
    (QA/'AserraderoPremiumV1_Manifest.json').write_text(
        json.dumps(record,ensure_ascii=False,indent=2)+'\n')
    print('ASERRADERO_PREMIUM_V1_SOURCE_ALPHA_SHADOW_SHA_VERIFIED_NOT_ART_APPROVED')
if __name__=='__main__':main()
