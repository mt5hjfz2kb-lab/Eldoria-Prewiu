import os, json, zipfile, urllib.request
from PIL import Image
import numpy as np
ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT="/tmp/valoria-hybrid-proof"; SRC=os.path.join(OUT,"source"); EV=os.path.join(OUT,"evidence")
os.makedirs(SRC,exist_ok=True); os.makedirs(EV,exist_ok=True)
def dl(url,path):
    os.makedirs(os.path.dirname(path),exist_ok=True)
    req=urllib.request.Request(url,headers={"User-Agent":"EldoriaResearch/1.0"})
    with urllib.request.urlopen(req,timeout=180) as r, open(path,"wb") as f:
        while True:
            b=r.read(1024*1024)
            if not b: break
            f.write(b)
gatezip=os.path.join(SRC,"gatehouse.zip")
dl("https://opengameart.org/sites/default/files/76122_GateHouse2Upload_blend.zip",gatezip)
with zipfile.ZipFile(gatezip) as z: z.extractall(os.path.join(SRC,"gatehouse"))
wall=os.path.join(SRC,"wall_diff.jpg")
dl("https://dl.polyhaven.org/file/ph-assets/Models/jpg/1k/modular_fort_01/modular_fort_01_wall_diff_1k.jpg",wall)
back=Image.open(os.path.join(ROOT,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
target=Image.alpha_composite(back,front); target.save(os.path.join(EV,"target-gate.png"))
ta=np.asarray(target).astype(np.float32)/255; mask=ta[...,3]>.08; tp=ta[...,:3][mask]
lum=tp.mean(1); tp=tp[(lum>.08)&(lum<.92)]
tm=np.median(tp,axis=0); ts=np.maximum(np.percentile(tp,80,axis=0)-np.percentile(tp,20,axis=0),.08)
im=Image.open(wall).convert("RGB"); a=np.asarray(im).astype(np.float32)/255
flat=a.reshape(-1,3); sm=np.median(flat,axis=0); ss=np.maximum(np.percentile(flat,80,axis=0)-np.percentile(flat,20,axis=0),.08)
out=np.clip((a-sm)/ss*ts+tm,.02,.98)
Image.fromarray((out*255).astype(np.uint8)).save(os.path.join(SRC,"valoria-stone.jpg"),quality=95)
blend=next((os.path.join(dp,f) for dp,_,fs in os.walk(os.path.join(SRC,"gatehouse")) for f in fs if f.lower().endswith(".blend")),None)
if not blend: raise RuntimeError("No gatehouse blend")
open(os.path.join(OUT,"blend-path.txt"),"w").write(blend)
open(os.path.join(EV,"color-transfer.json"),"w").write(json.dumps({"target_median":tm.tolist(),"target_range":ts.tolist(),"source_median":sm.tolist(),"source_range":ss.tolist()},indent=2))
print("BLEND",blend)
