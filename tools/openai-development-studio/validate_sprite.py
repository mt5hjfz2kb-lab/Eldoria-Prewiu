"""Deterministic local asset acceptance and Blender interchange; no game promotion."""
import hashlib,json,pathlib,sys
from PIL import Image
root=pathlib.Path(sys.argv[1])
p=root/"candidate.png"
raw=p.read_bytes()
with Image.open(p) as im:
    im.verify()
with Image.open(p) as im:
    rgba=im.convert("RGBA")
    if rgba.size!=(1024,1024): raise ValueError("Unexpected image dimensions")
    alpha=rgba.getchannel("A")
    histogram=alpha.histogram()
    pixels=rgba.width*rgba.height
    clear=histogram[0]/pixels
    # Generated alpha can peak at 254. Measure near-opaque coverage, preserve alpha.
    opaque=sum(histogram[250:])/pixels
    if not (0.03<=clear<=0.95 and opaque>=0.01):
        raise ValueError("Transparency gate: candidate unsuitable for isolated sprite")
    bbox=alpha.point(lambda x: 255 if x>5 else 0).getbbox()
    if bbox is None: raise ValueError("Empty sprite")
    crop=rgba.crop(bbox)
    crop.save(root/"candidate-trimmed.png")
    info={"width":rgba.width,"height":rgba.height,"transparent_fraction":clear,"near_opaque_fraction_alpha_250_255":opaque,"bounds":bbox,
          "original_sha256":hashlib.sha256(raw).hexdigest(),"trimmed_sha256":hashlib.sha256((root/"candidate-trimmed.png").read_bytes()).hexdigest(),
          "unity_candidate_only":True,"art_approved":False}
    (root/"sprite-validation.json").write_text(json.dumps(info,indent=2))
    print("SPRITE_TECHNICAL_PASS",json.dumps(info))
