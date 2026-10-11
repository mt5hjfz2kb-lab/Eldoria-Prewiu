"""Manifest-driven SHARP scenes: replace masked records only, one global sorter."""
import argparse, hashlib, json, re
from pathlib import Path
import numpy as np
from scipy.spatial import cKDTree

def sha(data): return hashlib.sha256(data).hexdigest()
def read(path, budget):
    data=Path(path).read_bytes();offset=data.index(b'end_header\n')+11;header=data[:offset]
    count=int(re.search(rb'element vertex (\d+)',header)[1]);end=offset+count*56
    assert end<=len(data),'Truncated SHARP PLY'
    step=max(1,int(np.ceil(count/budget)))
    v=np.frombuffer(data,dtype='<f4',count=count*14,offset=offset).reshape(count,14)[::step].copy()
    invalid=~np.isfinite(v).all(axis=1);v[~np.isfinite(v)]=0;v[invalid,6]=-20
    meta=data[end:];intrinsic=np.frombuffer(meta,dtype='<f4',count=9,offset=64)
    dim=np.frombuffer(meta,dtype='<u4',count=2,offset=100).astype(float)
    f,cx,cy=float(intrinsic[0]),float(intrinsic[2]),float(intrinsic[5])
    with np.errstate(divide='ignore',invalid='ignore'):uv=np.column_stack((v[:,0]/v[:,2]*f+cx,v[:,1]/v[:,2]*f+cy))
    return data,header,v,meta,uv,(f,cx,cy),dim

def main():
    ap=argparse.ArgumentParser();ap.add_argument('source');ap.add_argument('ground');ap.add_argument('output');ap.add_argument('--manifest',default='pipeline/valoria-parcel-regions-v1.json');a=ap.parse_args()
    manifest=json.loads(Path(a.manifest).read_text());budget=manifest['working_source_max_splats']
    data,header,v,meta,uv,camera,dim=read(a.source,budget)
    gd,_,gv,_,guv,_,gdim=read(a.ground,budget)
    assert sha(data)==manifest['source_sha256'],'Locked source SHA mismatch'
    assert sha(gd)==manifest['ground']['ply_sha256'],'Local ground SHA mismatch'
    assert np.array_equal(dim,[manifest['coordinate_space']['width'],manifest['coordinate_space']['height']]),'Coordinate-space mismatch'
    bits=[p['variant_bit'] for p in manifest['parcels']]
    assert sorted(bits)==[1<<i for i in range(len(bits))],'Invalid variant bits'
    masks={};radii={};union=np.zeros(len(v),dtype=bool)
    for p in manifest['parcels']:
        x,y,rx,ry=p['mask']['ellipse'];radius=np.linalg.norm((uv-[x,y])/[rx,ry],axis=1);mask=radius<1
        for x0,y0,x1,y1 in p['mask'].get('rectangles',[]):mask|=(uv[:,0]>x0)&(uv[:,0]<x1)&(uv[:,1]>y0)&(uv[:,1]<y1)
        assert np.any(mask) and not np.any(union&mask),'Empty/overlapping mask'
        masks[p['id']]=mask;radii[p['id']]=radius;union|=mask
    guv*=dim/gdim;valid=np.isfinite(gv).all(axis=1)&np.isfinite(guv).all(axis=1)&(gv[:,2]>0)
    gv=gv[valid];guv=guv[valid];tree=cKDTree(guv);replacements={};calibration={};f,cx,cy=camera
    for p in manifest['parcels']:
        mask=masks[p['id']];q=uv[mask];assert np.isfinite(q).all()
        distance,ids=tree.query(q);ground=gv[ids].copy();radius=radii[p['id']];ring=p['calibration_ring']
        border=(radius>ring['inner'])&(radius<ring['outer'])&~union&np.isfinite(uv).all(axis=1)&(v[:,2]>0)
        assert border.sum()>100,'Insufficient boundary samples'
        border_ids=tree.query(uv[border])[1];ratio=float(np.median(v[border,2]/gv[border_ids,2]))
        assert np.isfinite(ratio) and ratio>0
        depth=ground[:,2]*ratio;ground[:,0]=(q[:,0]-cx)*depth/f;ground[:,1]=(q[:,1]-cy)*depth/f;ground[:,2]=depth
        ground[:,7:10]+=np.log(ratio*float(dim[0])/float(gdim[0]));assert np.isfinite(ground).all()
        replacements[p['id']]=ground;calibration[p['id']]={'depth_ratio':ratio,'records':int(mask.sum()),'max_nearest_pixel_distance':float(distance.max())}
    out=Path(a.output);out.mkdir(parents=True,exist_ok=True)
    h=re.sub(rb'element vertex \d+',b'element vertex '+str(len(v)).encode(),header,count=1);baseline=h+v.astype('<f4').tobytes()+meta;records={}
    for state in range(1<<len(bits)):
        points=v.copy()
        for p in manifest['parcels']:
            mask=masks[p['id']]
            if not state&p['variant_bit']:points[mask]=replacements[p['id']]
            else:assert points[mask].tobytes()==v[mask].tobytes()
        assert points[~union].tobytes()==v[~union].tobytes(),'Out-of-mask mutation'
        payload=h+points.astype('<f4').tobytes()+meta
        if state==(1<<len(bits))-1:assert payload==baseline
        (out/('state-'+str(state)+'.ply')).write_bytes(payload);records[str(state)]={'count':len(points),'sha256':sha(payload)}
    (out/'parcel-regions.json').write_text(json.dumps(manifest,indent=2)+'\n')
    evidence={'source_sha256':sha(data),'ground_source_sha256':sha(gd),'manifest_sha256':sha(Path(a.manifest).read_bytes()),'splat_count':len(v),'outside_parcels_byte_identical':True,'built_built_records_byte_identical':True,'baseline_sha256':sha(baseline),'method':'one globally sorted SHARP scene; manifest-driven parcel variants','calibration':calibration,'states':records}
    (out/'parcel-source.json').write_text(json.dumps(evidence,indent=2)+'\n');print(json.dumps(evidence))
if __name__=='__main__':main()
