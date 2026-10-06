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
ref=np.asarray(Image.open(SRC).convert("RGB"))
H,W=depth.shape
img=cv2.resize(ref,(W,H),interpolation=cv2.INTER_LANCZOS4)

finite=np.isfinite(depth)
lo,hi=np.percentile(depth[finite],[2.0,98.0])
dn=np.clip((depth-lo)/max(hi-lo,1e-6),0.0,1.0)

fov=math.radians(50.0)
fx=(W*0.5)/math.tan(fov*0.5)

# Depth-derived multiplane split. These thresholds are intentionally fixed and reproducible:
# near lower foreground, lower/mid city, upper city/hill, background.
thresholds=[0.05,0.10,0.15]
lows=[0.0]+thresholds
highs=thresholds+[1.01]

masks=[]
for lo_t,hi_t in zip(lows,highs):
    m=((dn>=lo_t)&(dn<hi_t)).astype(np.uint8)*255
    m=cv2.morphologyEx(m,cv2.MORPH_CLOSE,np.ones((3,3),np.uint8))
    masks.append(m)

zmap=6.0+dn*12.0
plates=[]
extension_radius=30
ext_kernel=np.ones((extension_radius*2+1,extension_radius*2+1),np.uint8)

for i,(lo_t,hi_t,m) in enumerate(zip(lows,highs,masks)):
    if i==len(masks)-1:
        # Farthest plate becomes a complete reconstructed backdrop.
        holes=(dn<lo_t).astype(np.uint8)*255
        bgr=cv2.cvtColor(img,cv2.COLOR_RGB2BGR)
        plate=cv2.inpaint(bgr,holes,7,cv2.INPAINT_TELEA)
        plate=cv2.cvtColor(plate,cv2.COLOR_BGR2RGB)
        alpha=np.ones((H,W),np.uint8)*255
    else:
        # Reconstruct only the hidden nearer-side neighborhood needed for disocclusion.
        holes=(dn<lo_t).astype(np.uint8)*255 if lo_t>0 else np.zeros((H,W),np.uint8)
        bgr=cv2.cvtColor(img,cv2.COLOR_RGB2BGR)
        if np.count_nonzero(holes):
            bgr=cv2.inpaint(bgr,holes,5,cv2.INPAINT_TELEA)
        plate=cv2.cvtColor(bgr,cv2.COLOR_BGR2RGB)
        dil=cv2.dilate(m,ext_kernel)
        nearer=(dn<lo_t).astype(np.uint8)*255 if lo_t>0 else np.zeros_like(m)
        alpha=np.maximum(m,cv2.bitwise_and(dil,nearer))
        alpha=cv2.GaussianBlur(alpha,(0,0),0.8)

    vals=zmap[m>0]
    zmed=float(np.median(vals)) if vals.size else 12.0
    plates.append((plate,alpha,zmed))

def shift_layer(plate,alpha,dx):
    M=np.float32([[1,0,dx],[0,1,0]])
    pim=cv2.warpAffine(plate,M,(W,H),flags=cv2.INTER_LINEAR,borderMode=cv2.BORDER_REFLECT_101)
    pam=cv2.warpAffine(alpha,M,(W,H),flags=cv2.INTER_LINEAR,borderMode=cv2.BORDER_CONSTANT,borderValue=0)
    return pim,pam

def render(name,cam_x):
    canvas=np.zeros((H,W,3),np.float32)
    # Far -> near compositing.
    for plate,alpha,zmed in reversed(plates):
        dx=-fx*cam_x/zmed
        pim,pam=shift_layer(plate,alpha,dx)
        a=(pam.astype(np.float32)/255.0)[...,None]
        canvas=pim.astype(np.float32)*a + canvas*(1.0-a)

    out=np.clip(canvas,0,255).astype(np.uint8)

    # Small deterministic overscan crop removes only viewport-edge exposure.
    mx=int(W*0.015)
    my=int(H*0.005)
    out=out[my:H-my,mx:W-mx]
    full=cv2.resize(out,(ref.shape[1],ref.shape[0]),interpolation=cv2.INTER_LANCZOS4)
    Image.fromarray(full).save(EVID/name)
    return full

outputs={}
for name,off in [
    ("layered-projection-base.png",0.0),
    ("layered-projection-left-small.png",-0.20),
    ("layered-projection-right-small.png",0.20),
    ("layered-projection-left-medium.png",-0.45),
    ("layered-projection-right-medium.png",0.45),
]:
    outputs[name]=render(name,off)

base=outputs["layered-projection-base.png"].astype(np.float32)
ref_f=ref.astype(np.float32)
mae=float(np.mean(np.abs(base-ref_f)))
rmse=float(np.sqrt(np.mean((base-ref_f)**2)))

report={
  "method":"depth-derived multiplane projection with deterministic disocclusion reconstruction",
  "depth_model":"depth-anything/DA3-BASE",
  "depth_thresholds":thresholds,
  "representative_layer_depths":[float(x[2]) for x in plates],
  "camera_offsets_world_units":{"small":0.20,"medium":0.45},
  "overscan_crop_fraction":{"x":0.015,"y":0.005},
  "base_pixel_mae_0_255":mae,
  "base_pixel_rmse_0_255":rmse,
  "vendor_credits":0,
  "unity_touched":False,
  "decision_rule":"Judge small/medium views visually against canonical target. No Unity authorization implied."
}
(EVID/"layered-projection-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
