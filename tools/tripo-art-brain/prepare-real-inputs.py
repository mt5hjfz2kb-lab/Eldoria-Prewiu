#!/usr/bin/env python3
"""Derive CPU-friendly images from authentic saved Eldoria photos. No drawings."""
import sys,json,hashlib,pathlib
from PIL import Image
out=pathlib.Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
files={
  "reference": pathlib.Path("ArtSource/World4X/Spawnling_v1/UnityQA/front.png"),
  "before": pathlib.Path("docs/evidence/spawnling-bastion2/after-local-900x1600.png")
}
report={}
for kind,source in files.items():
    data=source.read_bytes()
    if len(data)<10000 or not data.startswith(bytes.fromhex("89504e470d0a1a0a")):
        raise ValueError("Input is not authentic archived PNG: "+str(source))
    with Image.open(source) as original:
        img=original.convert("RGB")
    dimensions=list(img.size)
    img.thumbnail((640,640),Image.Resampling.LANCZOS)
    result=out/(kind+"-cpu640.jpg")
    img.save(result,"JPEG",quality=86)
    report[kind]={"source":str(source),"raw_sha256":hashlib.sha256(data).hexdigest(),
        "raw_dimensions_px":dimensions,"derivative":str(result),
        "derivative_sha256":hashlib.sha256(result.read_bytes()).hexdigest(),
        "derivative_dimensions_px":list(img.size)}
(out/"genuine-image-provenance.json").write_text(json.dumps(report,indent=2)+"\n","utf-8")
print("AUTHENTIC_UNITY_ARCHIVE_IMAGES_RESIZED_FOR_CPU_WITH_SHA; no generated images")
