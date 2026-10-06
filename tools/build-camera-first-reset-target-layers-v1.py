#!/usr/bin/env python3
import json
from pathlib import Path
import cv2
import numpy as np
from PIL import Image
import torch
from depth_anything_3.api import DepthAnything3

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/"docs"/"evidence"/"valoria-art-production-reset-v1"/"canonical-target.jpeg"
OUT=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-reset-target-v1"
ASSET=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-reset-target-v1"
OUT.mkdir(parents=True,exist_ok=True)
ASSET.mkdir(parents=True,exist_ok=True)

# Clean Golden crop: Lower Gate + Bridge + adjacent cliff/ground/water.
# It intentionally excludes the HUD rails/buttons from the owner reference.
crop_box=(300,420,1000,960)  # x0,y0,x1,y1 in authoritative 1536x1024 target.
img=Image.open(SRC).convert("RGB")
crop=img.crop(crop_box)
crop.save(OUT/"canonical-golden-crop.png")
crop.save(ASSET/"canonical-golden-crop.png")

model=DepthAnything3.from_pretrained(
    "depth-anything/DA3-BASE",
    revision="f4a6c9b3c95e41c82048423d3493a81ec3fa810e"
).to(torch.device("cpu"))
model.eval()
pred=model.inference([str(OUT/"canonical-golden-crop.png")])
depth=np.asarray(pred.depth[0],dtype=np.float32)
conf=np.asarray(pred.conf[0],dtype=np.float32) if pred.conf is not None else np.ones_like(depth)

# Resize back to exact crop raster if model internally changes dimensions.
w,h=crop.size
depth=cv2.resize(depth,(w,h),interpolation=cv2.INTER_CUBIC)
conf=cv2.resize(conf,(w,h),interpolation=cv2.INTER_CUBIC)
depth=cv2.bilateralFilter(depth,9,0.15,15)

finite=np.isfinite(depth)
lo,hi=np.percentile(depth[finite],[2,98])
dn=np.clip((depth-lo)/max(hi-lo,1e-6),0,1)

# Visual depth diagnostic.
vis=(255*(1-dn)).astype(np.uint8)
vis=cv2.applyColorMap(vis,cv2.COLORMAP_TURBO)
cv2.imwrite(str(OUT/"reset-crop-depth-preview.png"),vis)
np.save(OUT/"reset-crop-depth.npy",depth)

# Four deterministic depth layers. Every source pixel belongs to exactly one layer.
# Near = low depth normalization, far = high.
thresholds=[0.25,0.50,0.75]
bands=np.digitize(dn,thresholds,right=False)
rgb=np.asarray(crop,dtype=np.uint8)

layer_stats=[]
for band in range(4):
    mask=(bands==band).astype(np.uint8)*255
    rgba=np.dstack([rgb,mask])
    out=Image.fromarray(rgba,"RGBA")
    out.save(ASSET/f"layer-{band}.png")
    out.save(OUT/f"layer-{band}.png")
    layer_stats.append({
        "layer":band,
        "pixels":int((bands==band).sum()),
        "fraction":float((bands==band).mean()),
        "depth_min":float(dn[bands==band].min()) if np.any(bands==band) else None,
        "depth_max":float(dn[bands==band].max()) if np.any(bands==band) else None
    })

report={
    "authority":"docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg",
    "authority_sha256":"8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689",
    "source_dimensions":[1536,1024],
    "crop_box":list(crop_box),
    "crop_dimensions":[w,h],
    "crop_contains":["Lower Gate","Bridge","adjacent cliff","ground","water"],
    "hud_intentionally_excluded":True,
    "camera_projection":"orthographic",
    "reset_camera_pitch":35.0,
    "reset_camera_yaw":20.0,
    "reset_vertical_span":48.0,
    "depth_model":"depth-anything/DA3-BASE",
    "depth_model_license":"Apache-2.0",
    "layers":layer_stats,
    "credits":0,
    "unity_touched":False
}
(OUT/"reset-layer-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
