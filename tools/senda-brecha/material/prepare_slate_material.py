"""Deterministic periodic boundary repair for one verified Studio material.
Do not use generated imagery as a full scene. Preserve interior pixels."""
import hashlib,json,math,sys
from pathlib import Path
from PIL import Image
import numpy as np
EXPECTED="81220ac1c7a76ad8481fd2f29f3d13fb8f298f4a8d1e0079b58e3bfa891af34e"
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def edge(a):return [float(np.abs(a[:,0].astype(float)-a[:,-1]).mean()),float(np.abs(a[0].astype(float)-a[-1]).mean())]
def close_axis(a,axis,width):
    b=np.swapaxes(a,axis,0).copy();ref=b.copy();n=b.shape[0]
    for k in range(width):
        w=(1+math.cos(math.pi*k/width))*.25
        b[k]=(1-w)*ref[k]+w*ref[n-1-k]
        b[n-1-k]=(1-w)*ref[n-1-k]+w*ref[k]
    return np.swapaxes(b,axis,0)
def prepare(src,out):
    assert digest(src)==EXPECTED,"Unexpected generated source"
    im=Image.open(src);assert im.size==(1024,1024)
    assert im.convert("RGBA").getchannel("A").getextrema()==(255,255)
    raw=np.asarray(im.convert("RGB"))
    repaired=np.rint(close_axis(close_axis(raw.astype(float),0,24),1,24)).clip(0,255).astype("uint8")
    assert np.array_equal(raw[24:-24,24:-24],repaired[24:-24,24:-24])
    assert edge(repaired)==[0.0,0.0]
    out.mkdir(parents=True,exist_ok=False)
    tile=out/"senda-worn-slate.png";Image.fromarray(repaired).save(tile,optimize=True)
    # Exact source, processing method and byte identity. No artistic approval.
    proof={"schema":1,"source_run":45707395248,"source_artifact":15065212591,"source_sha256":EXPECTED,
           "derived_sha256":digest(tile),"dimensions":[1024,1024],"alpha_extrema":[255,255],
           "edge_mean_abs_before":edge(raw),"edge_mean_abs_after":edge(repaired),"repair_pixels_each_side":24,
           "interior_unchanged":True,"scope":"surface albedo only","art_approved":False}
    (out/"senda-worn-slate.provenance.json").write_text(json.dumps(proof,indent=2)+"\n")
    print("SENDA_MATERIAL_PERIODIC_PASS",json.dumps(proof))
if __name__=="__main__":prepare(Path(sys.argv[1]),Path(sys.argv[2]))
