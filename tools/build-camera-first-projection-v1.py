#!/usr/bin/env python3
import json, math, os, sys
from pathlib import Path

import cv2
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "references" / "VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
OUT = ROOT / "docs" / "evidence" / "valoria-golden-lookdev-slice-v1" / "camera-first-projection-v1"
MESH_DIR = ROOT / "art-source" / "valoria" / "lookdev" / "golden-slice-v1" / "camera-first-projection-v1"
OUT.mkdir(parents=True, exist_ok=True)
MESH_DIR.mkdir(parents=True, exist_ok=True)

# Downscale only for depth inference/receiver geometry; exact canonical image remains texture authority.
img = Image.open(SRC).convert("RGB")
orig_w, orig_h = img.size
infer_w = min(1024, orig_w)
infer_h = max(1, round(orig_h * infer_w / orig_w))
infer = img.resize((infer_w, infer_h), Image.Resampling.LANCZOS)
infer_path = OUT / "canonical-reference-depth-input.jpg"
infer.save(infer_path, quality=95)

from depth_anything_3.api import DepthAnything3
import torch

device = torch.device("cpu")
model = DepthAnything3.from_pretrained("depth-anything/DA3-BASE", revision="f4a6c9b3c95e41c82048423d3493a81ec3fa810e")
model = model.to(device=device)
model.eval()

prediction = model.inference([str(infer_path)])
depth = np.asarray(prediction.depth[0], dtype=np.float32)
conf = np.asarray(prediction.conf[0], dtype=np.float32) if prediction.conf is not None else np.ones_like(depth)

# Save raw arrays.
np.save(OUT / "depth.npy", depth)
np.save(OUT / "confidence.npy", conf)

# Robust normalization only for mesh construction and previews.
finite = np.isfinite(depth)
vals = depth[finite]
lo, hi = np.percentile(vals, [2.0, 98.0])
dn = np.clip((depth - lo) / max(hi - lo, 1e-6), 0.0, 1.0)

# Depth visualization: near = warm/bright, far = dark.
vis = (255.0 * (1.0 - dn)).astype(np.uint8)
vis = cv2.applyColorMap(vis, cv2.COLORMAP_TURBO)
cv2.imwrite(str(OUT / "depth-preview.png"), vis)

# Build a reduced receiver mesh in camera space.
# Use roughly 320 samples horizontally for proof quality/perf balance.
mesh_w = min(320, infer_w)
mesh_h = max(2, round(infer_h * mesh_w / infer_w))
dn_m = cv2.resize(dn, (mesh_w, mesh_h), interpolation=cv2.INTER_AREA)
conf_m = cv2.resize(conf, (mesh_w, mesh_h), interpolation=cv2.INTER_AREA)

# Perspective receiver. Relative depth is remapped to a bounded physical interval.
# Near structures around 6 units, distant image content around 18.
z_near, z_far = 6.0, 18.0
Z = z_near + dn_m * (z_far - z_near)
fov_deg = 50.0
focal_px = (mesh_w * 0.5) / math.tan(math.radians(fov_deg * 0.5))
cx = (mesh_w - 1) * 0.5
cy = (mesh_h - 1) * 0.5

verts=[]
uvs=[]
for y in range(mesh_h):
    for x in range(mesh_w):
        z=float(Z[y,x])
        X=(x-cx)*z/focal_px
        Y=(cy-y)*z/focal_px
        verts.append((X,Y,-z))
        u=x/(mesh_w-1)
        v=1.0-y/(mesh_h-1)
        uvs.append((u,v))

def idx(x,y): return y*mesh_w+x

faces=[]
threshold=0.075
min_conf=float(np.percentile(conf_m[np.isfinite(conf_m)],5.0)) if np.isfinite(conf_m).any() else -1e9
for y in range(mesh_h-1):
    for x in range(mesh_w-1):
        ids=[idx(x,y),idx(x+1,y),idx(x,y+1),idx(x+1,y+1)]
        ds=[dn_m[y,x],dn_m[y,x+1],dn_m[y+1,x],dn_m[y+1,x+1]]
        cs=[conf_m[y,x],conf_m[y,x+1],conf_m[y+1,x],conf_m[y+1,x+1]]
        # Avoid stretching across sharp depth discontinuities.
        if max(ds)-min(ds) > threshold or min(cs) < min_conf:
            continue
        a,b,c,d=ids
        faces.append((a,c,b))
        faces.append((b,c,d))

obj=MESH_DIR/"projection_receiver.obj"
mtl=MESH_DIR/"projection_receiver.mtl"
tex=MESH_DIR/"VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
img.save(tex, quality=95)
with mtl.open("w",encoding="utf8") as f:
    f.write("newmtl CanonicalProjection\n")
    f.write("Ka 1 1 1\nKd 1 1 1\nKs 0 0 0\nillum 1\n")
    f.write("map_Kd VALORIA_APPROVED_VISUAL_REFERENCE.jpg\n")
with obj.open("w",encoding="utf8") as f:
    f.write("mtllib projection_receiver.mtl\n")
    for v in verts: f.write("v %.7f %.7f %.7f\n"%v)
    for uv in uvs: f.write("vt %.7f %.7f\n"%uv)
    f.write("usemtl CanonicalProjection\n")
    for a,b,c in faces:
        # OBJ 1-based; vertex and uv indices match.
        f.write(f"f {a+1}/{a+1} {b+1}/{b+1} {c+1}/{c+1}\n")

report={
    "source":str(SRC.relative_to(ROOT)),
    "source_dimensions":[orig_w,orig_h],
    "depth_model":"depth-anything/DA3-BASE",
    "depth_model_license":"Apache-2.0",
    "inference_dimensions":[infer_w,infer_h],
    "mesh_grid":[mesh_w,mesh_h],
    "vertices":len(verts),
    "triangles":len(faces),
    "depth_percentiles":{"p02":float(lo),"p98":float(hi)},
    "receiver_depth_range":[z_near,z_far],
    "fov_deg":fov_deg,
    "triangle_depth_discontinuity_threshold":threshold,
    "vendor_credits":0,
    "unity_touched":False
}
(OUT/"source-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
