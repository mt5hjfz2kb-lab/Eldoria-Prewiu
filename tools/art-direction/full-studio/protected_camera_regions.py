"""Camera-space preservation gate for explicitly art-directed functional zones.

Pure QA: never moves vertices, authors shapes, changes materials, or approves art.
Configured zones come from a human-reviewed real Blender screenshot.
"""
import json
from pathlib import Path
from PIL import Image

def inspect_protected_regions(before,after,review_file,channel_threshold=12):
    review=json.loads(Path(review_file).read_text(encoding="utf-8"))
    regions=review.get("protected_screen_regions",[])
    if not regions:
        return {"enabled":False,"allowed":True,"zones":[],"art_approved":False}
    a=Image.open(before).convert("RGB")
    b=Image.open(after).convert("RGB")
    if a.size!=b.size:
        raise ValueError("Camera A/B protected-region QA requires identical image dimensions")
    w,h=a.size
    pixels_a=a.load();pixels_b=b.load()
    evidence=[];allowed=True
    for region in regions:
        name=region["name"]
        x1,y1,x2,y2=region["box_px"]
        maximum=float(region["max_changed_fraction"])
        if not(0<=x1<x2<=w and 0<=y1<y2<=h and 0<=maximum<=1):
            raise ValueError("Invalid actual camera-region coordinates or limit")
        total=(x2-x1)*(y2-y1)
        changed=sum(
            1 for y in range(y1,y2) for x in range(x1,x2)
            if max(abs(pixels_a[x,y][c]-pixels_b[x,y][c]) for c in range(3))>channel_threshold)
        fraction=changed/total
        ok=fraction<=maximum
        allowed=allowed and ok
        evidence.append({"name":name,"box_px":[x1,y1,x2,y2],
                         "changed_fraction":round(fraction,6),
                         "max_changed_fraction":maximum,"allowed":ok})
    return {"enabled":True,"allowed":allowed,"zones":evidence,
            "camera_resolution_px":[w,h],"art_approved":False,
            "no_artist_parameters_modified":True}
