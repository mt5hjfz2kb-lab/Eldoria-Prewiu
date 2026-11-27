"""Fit authored independent RGBA sprites into the existing Unity canvas and preserve pivot/PPU.
No city compositing. Nothing in the source checkout is modified.
"""
import sys,json,hashlib,re
from pathlib import Path
from PIL import Image
import numpy as np
assets,source,out=map(Path,sys.argv[1:4])
out.mkdir(exist_ok=False)
manifest=json.loads((assets/"asset-manifest.json").read_text())
assert set(manifest)=={"bastion","sawmill","meadow"}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bbox(im):
    a=im.getchannel("A")
    b=a.point(lambda v:255 if v>128 else 0).getbbox()
    assert b and a.getextrema()[0]==0 and a.getextrema()[1]>=240
    return b
report={}
for name in ("bastion","sawmill"):
    p=assets/(name+".png");assert sha(p)==manifest[name]["sha256"]
    donor=source/(name+"_i_hd.png");meta=source/(name+"_i_hd.png.meta")
    before=Image.open(donor).convert("RGBA");new=Image.open(p).convert("RGBA")
    original_box=bbox(before);subject_box=bbox(new)
    W,H=before.size
    left,top,right,bottom=original_box
    # Preserve occupied width and floor; never stretch or introduce a cropped silhouette.
    subject=new.crop(subject_box)
    scale=min((right-left)/subject.width,(bottom-2)/subject.height)
    w,h=max(1,round(subject.width*scale)),max(1,round(subject.height*scale))
    resized=subject.resize((w,h),Image.Resampling.LANCZOS)
    x=round((left+right-w)/2);y=bottom-h
    assert x>=0 and y>=0 and x+w<=W and y+h<=H
    canvas=Image.new("RGBA",(W,H));canvas.paste(resized,(x,y))
    dest=out/(name+"-fit.png");canvas.save(dest)
    text=meta.read_text()
    ppu=float(re.search(r"spritePixelsToUnits:\s*([\d.]+)",text).group(1))
    pivot=re.search(r"spritePivot:\s*\{x:\s*([\d.]+),\s*y:\s*([\d.]+)\}",text)
    report[name]={"source_sha256":sha(donor),"asset_sha256":sha(p),"fitted_sha256":sha(dest),"canvas":[W,H],"original_bbox":original_box,"candidate_bbox":bbox(canvas),"pixels_per_unit":ppu,"pivot":[float(pivot.group(1)),float(pivot.group(2))],"uniform_scale":scale,"native_unity_verified":False}
meadow=assets/"meadow.png";assert sha(meadow)==manifest["meadow"]["sha256"]
im=Image.open(meadow).convert("RGB");a=np.array(im)
assert im.size==(1024,1024) and np.array_equal(a[0],a[-1]) and np.array_equal(a[:,0],a[:,-1])
im.save(out/"meadow.png")
report["meadow"]={"source_sha256":sha(meadow),"fitted_sha256":sha(out/"meadow.png"),"edge_equal":True,"native_unity_verified":False}
(out/"fit-manifest.json").write_text(json.dumps(report,indent=2))
print(json.dumps(report))
