"""Camera-locked Blender BEFORE/AFTER visibility measurement.

Only tests screen-space impact under a matched render, never aesthetic quality,
Unity import, or mobile device performance. This is a guard against Qwen's
previous technically valid but virtually invisible sculpting operations.
"""
from __future__ import annotations
from pathlib import Path
from PIL import Image,ImageChops

def screen_impact(before,after,min_fraction=.006,channel_threshold=10):
    b=Image.open(before).convert("RGB")
    a=Image.open(after).convert("RGB")
    if b.size!=a.size:
        raise ValueError("Unmatched render sizes")
    if not 0<=min_fraction<=1 or not 0<=channel_threshold<=255:
        raise ValueError("Invalid visibility contract")
    diff=ImageChops.difference(a,b)
    # The L channel accounts for changes in both chroma and luminance.
    pixels=diff.convert("L").point(lambda p:255 if p>channel_threshold else 0,mode="L")
    hist=pixels.histogram()
    changed=hist[255]
    total=b.width*b.height
    fraction=changed/total
    bounds=pixels.getbbox()
    return {
        "scope":"BLENDER_RENDER_SCREEN_IMPACT_NOT_UNITY_ART",
        "before":str(Path(before).name),"after":str(Path(after).name),
        "camera_same":True,"resolution":[b.width,b.height],
        "total_pixels":total,"changed_pixels":changed,
        "changed_fraction":round(fraction,7),
        "bounding_rectangle":list(bounds) if bounds else None,
        "minimum_fraction":min_fraction,
        "threshold_0_255":channel_threshold,
        "visually_present":fraction>=min_fraction and bounds is not None,
        "art_approved":False,"unity_approved":False,
    }
