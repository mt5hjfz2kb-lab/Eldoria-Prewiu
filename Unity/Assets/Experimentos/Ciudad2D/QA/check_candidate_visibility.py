#!/usr/bin/env python3
"""Conservative optical gate for a genuine Unity candidate A/B pair.

Exits nonzero if the candidate building appears to have disappeared. This is
NOT a substitute for human art approval, and is only calibrated to the fixed
Cuartel parcel and 1170x2532 native evidence framing.
"""
import argparse
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROI = (60, 485, 320, 740)  # x1,y1,x2,y2 in top-left 1170x2532 image
MIN_GRADIENT_RATIO = 0.72

def edge_energy(image, bounds):
    x1,y1,x2,y2=bounds
    rgb=np.asarray(image.convert("RGB"),dtype=np.float32)[y1:y2,x1:x2]
    grey=rgb[...,0]*0.2126+rgb[...,1]*0.7152+rgb[...,2]*0.0722
    return float(np.abs(np.diff(grey,axis=0)).mean()+np.abs(np.diff(grey,axis=1)).mean())

def check(before, candidate):
    with Image.open(before) as ref,Image.open(candidate) as aft:
        if ref.size!=(1170,2532) or aft.size!=ref.size:
            raise AssertionError("Native A/B frames must both be exactly 1170x2532")
        ref_score=edge_energy(ref,ROI)
        candidate_score=edge_energy(aft,ROI)
    ratio=candidate_score/ref_score if ref_score else 0.0
    result={"source":"Unity native A/B RGB frames, not Pillow composites",
            "cuartel_roi":list(ROI),
            "reference_gradient":round(ref_score,3),
            "candidate_gradient":round(candidate_score,3),
            "ratio":round(ratio,4),
            "threshold":MIN_GRADIENT_RATIO,
            "visible_candidate_gate_pass":ratio>=MIN_GRADIENT_RATIO,
            "art_approved":False}
    return result

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("reference_png",type=Path)
    ap.add_argument("candidate_png",type=Path)
    opts=ap.parse_args()
    result=check(opts.reference_png,opts.candidate_png)
    print(json.dumps(result,ensure_ascii=False,indent=2))
    if not result["visible_candidate_gate_pass"]:
        raise SystemExit("CANDIDATE_VISIBILITY_FAIL: probable vanished/blank Cuartel despite PNG and CI success")

if __name__=="__main__":main()
