#!/usr/bin/env python3
from pathlib import Path
import json, math
import numpy as np
import cv2
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/"references"/"VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
EVID=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-projection-v1"
depth=np.load(EVID/"depth.npy").astype(np.float32)
img=np.asarray(Image.open(SRC).convert("RGB"))
H,W=depth.shape
src=cv2.resize(img,(W,H),interpolation=cv2.INTER_LANCZOS4)

finite=np.isfinite(depth)
lo,hi=np.percentile(depth[finite],[2,98])
dn=np.clip((depth-lo)/max(hi-lo,1e-6),0,1)

# Same bounded receiver mapping as mesh builder.
z_near,z_far=6.0,18.0
Z=z_near+dn*(z_far-z_near)
fov=math.radians(50.0)
fx=(W*0.5)/math.tan(fov*0.5)
fy=fx
cx=(W-1)*0.5
cy=(H-1)*0.5
yy,xx=np.mgrid[0:H,0:W]
X=(xx-cx)*Z/fx
Y=(cy-yy)*Z/fy

def render(name, cam_x):
    # Camera translates on X, retains canonical orientation.
    Xc=X-cam_x
    Zc=Z
    u=fx*(Xc/Zc)+cx
    v=cy-fy*(Y/Zc)
    ui=np.rint(u).astype(np.int32)
    vi=np.rint(v).astype(np.int32)

    valid=(ui>=0)&(ui<W)&(vi>=0)&(vi<H)&np.isfinite(Zc)
    ids=np.flatnonzero(valid)
    dest=(vi.flat[ids]*W+ui.flat[ids]).astype(np.int64)
    zvals=Zc.flat[ids]

    # Far-to-near forward splat, so near samples overwrite far.
    order=np.argsort(zvals)[::-1]
    dest=dest[order]
    ids=ids[order]

    out=np.zeros((H*W,3),dtype=np.uint8)
    filled=np.zeros(H*W,dtype=np.uint8)
    out[dest]=src.reshape(-1,3)[ids]
    filled[dest]=1

    # Fill tiny sampling holes only; preserve meaningful disocclusions.
    im=out.reshape(H,W,3)
    mask=(1-filled.reshape(H,W))*255
    tiny=cv2.morphologyEx(mask,cv2.MORPH_OPEN,np.ones((2,2),np.uint8))
    inpaint_mask=(mask-tiny).clip(0,255).astype(np.uint8)
    if np.count_nonzero(inpaint_mask):
        im=cv2.inpaint(im,inpaint_mask,2,cv2.INPAINT_TELEA)

    # Upscale to canonical dimensions.
    full=cv2.resize(im,(img.shape[1],img.shape[0]),interpolation=cv2.INTER_LANCZOS4)
    Image.fromarray(full).save(EVID/name)

    hole_rate=float(1.0-filled.mean())
    return hole_rate

metrics={}
for name,off in [
    ("cpu-projection-base.png",0.0),
    ("cpu-projection-left-small.png",-0.20),
    ("cpu-projection-right-small.png",0.20),
    ("cpu-projection-left-medium.png",-0.45),
    ("cpu-projection-right-medium.png",0.45),
]:
    metrics[name]=render(name,off)

# Base-view pixel comparison against canonical, after both are exact same dimensions.
base=np.asarray(Image.open(EVID/"cpu-projection-base.png").convert("RGB")).astype(np.float32)
ref=img.astype(np.float32)
mae=float(np.mean(np.abs(base-ref)))
rmse=float(np.sqrt(np.mean((base-ref)**2)))
report={
  "method":"deterministic CPU forward projection from DA3 depth receiver",
  "depth_model":"depth-anything/DA3-BASE",
  "camera_offsets_world_units":{"small":0.20,"medium":0.45},
  "hole_rate":metrics,
  "base_pixel_mae_0_255":mae,
  "base_pixel_rmse_0_255":rmse,
  "vendor_credits":0,
  "unity_touched":False
}
(EVID/"cpu-projection-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
