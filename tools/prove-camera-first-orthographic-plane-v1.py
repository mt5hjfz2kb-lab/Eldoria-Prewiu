#!/usr/bin/env python3
from pathlib import Path
import json, math
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/"references"/"VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
REQ=ROOT/"pipeline"/"valoria-production-art-reset-run-request.json"
OUT=ROOT/"docs"/"evidence"/"valoria-golden-lookdev-slice-v1"/"camera-first-orthographic-plane-v1"
OUT.mkdir(parents=True,exist_ok=True)

ref=Image.open(SRC).convert("RGB")
W,H=ref.size
request=json.loads(REQ.read_text())

# Baseline authority: official 16:9 view, same yaw/pitch and span as canonical full-frame target.
base=next(v for v in request["views"] if v["name"]=="UNITY-16x9")
if base["perspective"]:
    raise RuntimeError("Orthographic proof requires perspective=false")
yaw=math.radians(base["yaw"])
pitch=math.radians(base["pitch"])

# Matches PreproductionSceneCapture: camera = center + vector*distance, LookAt(center).
center_to_camera=np.array([
    math.sin(yaw)*math.cos(pitch),
    math.sin(pitch),
    -math.cos(yaw)*math.cos(pitch)
],dtype=np.float64)
forward=-center_to_camera
world_up=np.array([0.0,1.0,0.0])
right=np.cross(world_up,forward); right/=np.linalg.norm(right)
up=np.cross(forward,right); up/=np.linalg.norm(up)

base_span=float(base["span"])
base_aspect=float(base["width"])/float(base["height"])
base_world_width=base_span*base_aspect
px_per_world_x=W/base_world_width
px_per_world_y=H/base_span

results=[]
for v in request["views"]:
    if v["perspective"] or abs(v["yaw"]-base["yaw"])>1e-6 or abs(v["pitch"]-base["pitch"])>1e-6:
        results.append({"name":v["name"],"compatible":False,"reason":"camera orientation/projection differs"})
        continue

    delta=np.array([v["center"]["x"],v["center"]["y"],v["center"]["z"]],dtype=np.float64)
    screen_x=float(np.dot(delta,right))
    screen_y=float(np.dot(delta,up))

    cx=W*0.5 + screen_x*px_per_world_x
    cy=H*0.5 - screen_y*px_per_world_y
    crop_h=float(v["span"])*px_per_world_y
    crop_w=float(v["span"])*(float(v["width"])/float(v["height"]))*px_per_world_x

    x0,y0,x1,y1=cx-crop_w*0.5,cy-crop_h*0.5,cx+crop_w*0.5,cy+crop_h*0.5
    overflow={
      "left_px":max(0.0,-x0),
      "top_px":max(0.0,-y0),
      "right_px":max(0.0,x1-W),
      "bottom_px":max(0.0,y1-H)
    }
    inside_fraction=max(0.0,min(W,x1)-max(0.0,x0))*max(0.0,min(H,y1)-max(0.0,y0))/max(crop_w*crop_h,1e-9)

    # PIL crop intentionally leaves uncovered areas black: the proof must expose missing source coverage,
    # not hide it with invented pixels.
    crop=ref.crop((int(round(x0)),int(round(y0)),int(round(x1)),int(round(y1))))
    crop=crop.resize((int(v["width"]),int(v["height"])),Image.Resampling.LANCZOS)
    filename=f'{v["name"]}.png'
    crop.save(OUT/filename)

    results.append({
      "name":v["name"],
      "compatible":True,
      "perspective":False,
      "yaw":v["yaw"],
      "pitch":v["pitch"],
      "span":v["span"],
      "center":v["center"],
      "screen_plane_center_world":[screen_x,screen_y],
      "crop_box_reference_px":[x0,y0,x1,y1],
      "source_coverage_fraction":float(inside_fraction),
      "overflow":overflow,
      "output":filename
    })

report={
  "method":"exact canonical reference mapped to locked orthographic yaw20/pitch35 camera plane",
  "source":"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg",
  "source_dimensions":[W,H],
  "baseline_view":base["name"],
  "baseline_span":base_span,
  "baseline_aspect":base_aspect,
  "camera_axes":{"right":right.tolist(),"up":up.tolist(),"forward":forward.tolist()},
  "views":results,
  "interpretation":{
    "full_coverage":"source_coverage_fraction == 1 means no novel-view reconstruction is required for that official view",
    "overflow":"overflow identifies only the exact pixels that require source extension; hero content inside source remains untouched"
  },
  "vendor_credits":0,
  "unity_touched":False
}
(OUT/"orthographic-plane-report.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps(report,indent=2))
