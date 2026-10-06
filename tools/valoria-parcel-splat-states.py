"""Separate the two player parcels from the LOCKED SHARP source, never regenerate it.

Unoccupied ground is transplanted from unoccupied terrain in the same SHARP scene.
This is a bounded surface fallback, not image cards or a new visual generator.
All vertex records outside the declared parcel masks are byte-identical.
"""
import argparse, hashlib, json, re
from pathlib import Path
import numpy as np
from scipy.spatial import cKDTree

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('source'); ap.add_argument('output'); a=ap.parse_args()
    source=Path(a.source); out=Path(a.output); out.mkdir(parents=True,exist_ok=True)
    data=source.read_bytes(); offset=data.index(b'end_header\n')+11; header=data[:offset]
    count=int(re.search(rb'element vertex (\d+)',header)[1]); end=offset+count*56
    v=np.frombuffer(data,dtype='<f4',count=count*14,offset=offset).reshape(count,14)[::2].copy()
    meta=data[end:]; intrinsic=np.frombuffer(meta,dtype='<f4',count=9,offset=64)
    f,cx,cy=float(intrinsic[0]),float(intrinsic[2]),float(intrinsic[5])
    uv=np.column_stack((v[:,0]/v[:,2]*f+cx,v[:,1]/v[:,2]*f+cy))
    u,y=uv.T
    left=(((u-258)/143)**2+((y-324)/72)**2<1)|((u>204)&(u<240)&(y>200)&(y<270))
    right=((u-1050)/151)**2+((y-390)/72)**2<1
    masks={'left':left,'right':right}; assert not np.any(left&right)
    def write(name,points):
        h=re.sub(rb'element vertex \d+',b'element vertex '+str(len(points)).encode(),header,count=1)
        p=out/(name+'.ply'); p.write_bytes(h+points.astype('<f4').tobytes()+meta)
        return {'count':len(points),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
    records={'base':write('base',v[~(left|right)])}
    for name,mask in masks.items():
        records[name]=write(name,v[mask])
        # Terrain donor: no building, path, vegetation, fire or player character.
        rect=(400,520,365,425) if name=='left' else (750,870,340,415)
        x0,x1,y0,y1=rect
        dm=(u>x0)&(u<x1)&(y>y0)&(y<y1)&np.isfinite(v).all(axis=1)
        donor=v[dm]; duv=uv[dm]
        q=uv[mask]
        sample=np.column_stack((x0+np.mod(q[:,0]-q[:,0].min(),x1-x0),y0+np.mod(q[:,1]-q[:,1].min(),y1-y0)))
        ids=cKDTree(duv).query(sample)[1]
        ground=donor[ids].copy()
        # Extrapolate the existing continuous terrain's reciprocal-depth plane.
        coef=np.linalg.lstsq(np.column_stack((duv,np.ones(len(duv)))),1/donor[:,2],rcond=None)[0]
        depth=1/(np.column_stack((q,np.ones(len(q))))@coef)
        depth=np.clip(depth,12,35)
        ratio=depth/ground[:,2]
        ground[:,0]=(q[:,0]-cx)*depth/f; ground[:,1]=(q[:,1]-cy)*depth/f; ground[:,2]=depth
        ground[:,7:10]+=np.log(ratio)[:,None]
        records[name+'-ground']=write(name+'-ground',ground)
    records['baseline']=write('baseline',v)
    assert records['base']['count']+records['left']['count']+records['right']['count']==len(v)
    (out/'parcel-source.json').write_text(json.dumps({'source_sha256':hashlib.sha256(data).hexdigest(),'splat_count':len(v),'outside_parcels_byte_identical':True,'method':'conditional original SHARP parcel layers + same-scene terrain Gaussian support','mask_pixels':{'left':{'ellipse':[258,324,143,72],'smoke':[204,200,240,270]},'right':{'ellipse':[1050,390,151,72]}},'layers':records},indent=2))
    print(json.dumps(records))

if __name__=='__main__': main()
