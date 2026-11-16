#!/usr/bin/env python3
"""Reproducible visual-regression guard for genuine Blender scene captures.

Catches the two previous false-positive "art finished" cases:
(1) vivid magenta missing textures; (2) large snow-white missing terrain shader.
This is a MINIMUM sanity check, never ART_APPROVED or equivalent.
"""
from PIL import Image, ImageChops, ImageStat
from pathlib import Path
import argparse
import json
p=argparse.ArgumentParser()
p.add_argument("--images",type=Path,required=True)
p.add_argument("--report",type=Path)
args=p.parse_args()
folder=args.images
names={
 "stage1":"DirectorV2_BastionI_1600x900.png",
 "stage5":"DirectorV2_BastionV_1600x900.png",
 "portrait":"DirectorV2_BastionV_Portrait_720x1200.png"}
frames={k:Image.open(folder/v).convert("RGB") for k,v in names.items()}
assert frames["stage1"].size==(1600,900)
assert frames["stage5"].size==(1600,900)
assert frames["portrait"].size==(720,1200)
review={}
for name,im in frames.items():
    small=im.resize((800,450)) if name!="portrait" else im.resize((360,600))
    px=list(small.getdata())
    n=len(px)
    magenta=sum(r>145 and b>125 and r>g*1.55 and b>g*1.55
                for r,g,b in px)/n
    snow=sum(r>180 and g>180 and b>176 and abs(r-g)<18 and abs(g-b)<22
             for r,g,b in px)/n
    means=ImageStat.Stat(small).mean
    review[name]={"magenta_ratio":round(magenta,5),
                  "near_white_neutral_ratio":round(snow,5),
                  "mean_rgb":[round(v,2) for v in means]}
    # Fail obvious missing asset / material bugs, without assuming beauty from numbers.
    assert magenta<.006,(name,"missing-texture-magenta",magenta)
    assert snow<.22,(name,"missing-terrain-white",snow)
    assert min(means)>20,(name,"possible-unlit-blank",means)
a=frames["stage1"].crop((450,60,1100,680))
b=frames["stage5"].crop((450,60,1100,680))
diff=ImageStat.Stat(ImageChops.difference(a,b)).mean
mean_diff=sum(diff)/len(diff)
review["castle_progression_diff"]=round(mean_diff,3)
assert mean_diff>8,("No meaningful Bastion I-V image difference",mean_diff)
review["art_approved"]=False
review["mobile_tested"]=False
review["summary"]="Basic regression checks pass; aesthetic approval requires expert and user inspection."
if args.report:
    args.report.parent.mkdir(parents=True,exist_ok=True)
    args.report.write_text(json.dumps(review,ensure_ascii=False,indent=2)+"\n")
print("ELDORIA_MISSING_TEXTURE_AND_TERRAIN_QA_PASS",json.dumps(review))
