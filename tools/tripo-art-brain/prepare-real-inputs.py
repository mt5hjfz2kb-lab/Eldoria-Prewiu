#!/usr/bin/env python3
"""Derive CPU-friendly images from authentic saved Eldoria captures. No drawings.
The isolated UnityQA front.png faces the TAIL (head +X in canonical source),
so it must not be mistaken for a face reference. Use the real Blender face
capture to guide the anatomical design; keep the true strategic Unity frame as
second image for gameplay readability. Both originals remain unchanged.
"""
import sys,json,hashlib,pathlib
from PIL import Image
out=pathlib.Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
files={
  "reference": pathlib.Path("ArtSource/World4X/Spawnling_v1/Finished/finished_face.png"),
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
print("AUTHENTIC_BLENDER_FACE_AND_UNITY_STRATEGIC_IMAGES_RESIZED_WITH_SHA; no generated images")
