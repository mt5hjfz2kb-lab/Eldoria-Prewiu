"""Prepare one original isolated Studio landmark; do not invent scenery or alter original art."""
import sys,hashlib,json
from pathlib import Path
from PIL import Image
import numpy as np
SOURCE="d19c6b26545887b9e405126af28af5ccbf02f91ee6780c81a051ad3a67afcca4"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
src,out=map(Path,sys.argv[1:]);assert sha(src)==SOURCE
im=Image.open(src).convert("RGBA");assert im.size==(1024,1024)
a=np.array(im);alpha=a[:,:,3]
assert .39<float((alpha>=128).mean())<.44
assert int(alpha[0].max())<10 and int(alpha[-1].max())<10 and int(alpha[:,0].max())<10 and int(alpha[:,-1].max())<10
# Remove only low-opacity generator speckle and expand near-opaque 252–254 to 255.
a[:,:,3]=np.where(alpha<16,0,np.rint(np.minimum(1,alpha.astype(float)/252)*255)).astype("uint8")
a[a[:,:,3]==0,:3]=0
bbox=Image.fromarray(a[:,:,3]).getbbox();assert bbox and bbox[2]-bbox[0]>950 and bbox[3]-bbox[1]>800
out.mkdir(parents=True,exist_ok=False);p=out/"senda-breach-landmark.png";Image.fromarray(a).crop(bbox).save(p,optimize=True)
report={"schema":1,"source_run":45771904456,"source_artifact":15094351746,"source_sha256":SOURCE,"texture_sha256":sha(p),"size":list(Image.open(p).size),"alpha_crop":list(bbox),"opaque_fraction":float((a[:,:,3]>=128).mean()),"scope":"one spatial horizon landmark, no sky or background overlay","art_approved":False}
(out/"senda-breach-landmark.json").write_text(json.dumps(report,indent=2)+"\n");print("SENDA_BREACH_ALPHA_PASS",report)
