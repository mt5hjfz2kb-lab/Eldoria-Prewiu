#!/usr/bin/env python3
import json
from pathlib import Path
import numpy as np
from PIL import Image
import torch
from sam2.sam2_image_predictor import SAM2ImagePredictor

ROOT=Path(__file__).resolve().parents[1]
EVID=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-reset-target-v1"
ASSET=ROOT/"art-source"/"valoria"/"lookdev"/"golden-slice-v1"/"camera-first-reset-target-v1"
SPEC=ROOT/"docs"/"evidence"/"valoria-art-production-reset-v1"/"target-spec.json"
IMG=EVID/"canonical-golden-crop.png"
DEPTH=EVID/"reset-crop-depth.npy"

spec=json.loads(SPEC.read_text())
layer_report=json.loads((EVID/"reset-layer-report.json").read_text())
x0,y0,x1,y1=layer_report["crop_box"]
rgb=np.asarray(Image.open(IMG).convert("RGB"))
h,w=rgb.shape[:2]
depth=np.load(DEPTH).astype(np.float32)
finite=np.isfinite(depth)
lo,hi=np.percentile(depth[finite],[2,98])
dn=np.clip((depth-lo)/max(hi-lo,1e-6),0.0,1.0)

predictor=SAM2ImagePredictor.from_pretrained("facebook/sam2.1-hiera-small", device="cpu")
predictor.set_image(rgb)

def local_box(name):
    bx=spec["target_landmarks"][name]["bbox"]
    return np.array([bx[0]-x0,bx[1]-y0,bx[2]-x0,bx[3]-y0],dtype=np.float32)

def predict_box(name):
    box=local_box(name)
    with torch.inference_mode():
        masks,scores,logits=predictor.predict(
            box=box,
            multimask_output=True
        )
    best=int(np.argmax(scores))
    mask=np.asarray(masks[best],dtype=bool)
    return mask,box,float(scores[best])

gate,gate_box,gate_score=predict_box("Lower gate")
bridge,bridge_box,bridge_score=predict_box("Bridge")

# Enforce a single semantic owner for every pixel.
# Gate has priority over Bridge only in the small overlap region.
bridge=np.logical_and(bridge,~gate)
occupied=np.logical_or(gate,bridge)
env=np.logical_not(occupied)

# Split remaining environment at its robust median depth.
env_vals=dn[env]
env_threshold=float(np.median(env_vals)) if env_vals.size else 0.5
env_near=np.logical_and(env,dn<=env_threshold)
env_far=np.logical_and(env,dn>env_threshold)

masks={
    "gate":gate,
    "bridge":bridge,
    "environment-near":env_near,
    "environment-far":env_far,
}
assert sum(int(m.sum()) for m in masks.values()) == w*h

def save_rgba(name,mask):
    alpha=(mask.astype(np.uint8)*255)
    rgba=np.dstack([rgb,alpha])
    Image.fromarray(rgba,"RGBA").save(ASSET/f"semantic-{name}.png")
    Image.fromarray(rgba,"RGBA").save(EVID/f"semantic-{name}.png")
    Image.fromarray(alpha,"L").save(EVID/f"semantic-{name}-mask.png")

stats=[]
for name,mask in masks.items():
    vals=dn[mask]
    stats.append({
        "name":name,
        "pixels":int(mask.sum()),
        "fraction":float(mask.mean()),
        "mean_depth_norm":float(vals.mean()) if vals.size else None,
        "median_depth_norm":float(np.median(vals)) if vals.size else None,
        "min_depth_norm":float(vals.min()) if vals.size else None,
        "max_depth_norm":float(vals.max()) if vals.size else None,
    })
    save_rgba(name,mask)

# Composite round-trip from disjoint semantic masks must reconstruct source pixels exactly.
composite=np.zeros_like(rgb)
for name,mask in masks.items():
    composite[mask]=rgb[mask]
Image.fromarray(composite,"RGB").save(EVID/"semantic-roundtrip.png")
roundtrip_mae=float(np.abs(composite.astype(np.int16)-rgb.astype(np.int16)).mean())

report={
    "authority":layer_report["authority"],
    "source_crop":layer_report["crop_box"],
    "model":"facebook/sam2.1-hiera-small",
    "model_license":"Apache-2.0",
    "sam2_code_revision":"2b90b9f5ceec907a1c18123530e92e794ad901a4",
    "prompts":{
        "Lower gate":{"box_local":gate_box.tolist(),"score":gate_score},
        "Bridge":{"box_local":bridge_box.tolist(),"score":bridge_score},
    },
    "environment_depth_split":env_threshold,
    "semantic_layers":stats,
    "pixel_partition_complete":True,
    "roundtrip_mae":roundtrip_mae,
    "credits":0,
    "unity_touched":False,
}
(EVID/"semantic-layer-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
