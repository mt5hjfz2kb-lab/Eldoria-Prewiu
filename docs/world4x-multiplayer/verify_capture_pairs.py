"""Verify native capture identity before visual review; never grants art approval."""
import argparse, hashlib, json, pathlib

LABELS=[f'{view}-{orientation}' for view in ('cercana','regional','panoramica') for orientation in ('horizontal','vertical')]
FIELDS=('sourceScene','version','width','height','zoom','aspect','cameraPosition','cameraRotation','worldTarget')

def same(a,b):
    if isinstance(a,dict):return a.keys()==b.keys() and all(same(a[k],b[k]) for k in a)
    if isinstance(a,(int,float)) and isinstance(b,(int,float)):return abs(a-b)<1e-5
    return a==b

def verify(before,after):
    result=[]
    for label in LABELS:
        items=[]
        for root in (before,after):
            p=root/(label+'-source.json');m=json.loads(p.read_text(encoding='utf-8-sig'))
            png=root/(label+'-unity.png');digest=hashlib.sha256(png.read_bytes()).hexdigest()
            assert digest==m['sha256'],f'Capture bytes disagree with native metadata: {png}'
            assert m['actualUnityCamera'] and m['playMode'],f'Not a native player capture: {p}'
            items.append((m,digest))
        for key in FIELDS:assert same(items[0][0][key],items[1][0][key]),f'Mismatched {label}/{key}'
        result.append({'label':label,'camera':{k:items[0][0][k] for k in FIELDS},'before_sha256':items[0][1],'after_sha256':items[1][1],'image_changed':items[0][1]!=items[1][1]})
    return {'same_cameras':True,'native_metadata_hashes_verified':True,'art_approved':False,'pairs':result}

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('before',type=pathlib.Path);p.add_argument('after',type=pathlib.Path);p.add_argument('--out',type=pathlib.Path,required=True);a=p.parse_args()
    a.out.write_text(json.dumps(verify(a.before,a.after),indent=2)+'\n')
    print('SIX_NATIVE_CAMERA_PAIRS_VERIFIED; independent visual judgment still required')
