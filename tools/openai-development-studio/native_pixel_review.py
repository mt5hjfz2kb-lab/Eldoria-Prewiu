"""Audit archived native A/B evidence without repeating Unity or spending API credits."""
import hashlib
import json
import sys
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw

root, out = map(Path, sys.argv[1:])
out.mkdir(exist_ok=False)
manifest = json.loads((root / "native-evidence.json").read_text(encoding="utf-8-sig"))
provenance = json.loads((root / "provenance.json").read_text(encoding="utf-8-sig"))
cleanup = json.loads((root / "cleanup.json").read_text(encoding="utf-8-sig"))
assert manifest["sourceFrames"] == len(manifest["frames"]) == 48
assert manifest["engineVersion"] == "6000.3.23f1" and manifest["nativeWorldRender"]
assert manifest["sourceSha"] == provenance["frozen_sha"] == "5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
assert provenance["reviewed_shader_sha256"] == "4e582b25a420383798e3aa4895d1b71b33158f18de0087165b8055adef068785"
assert cleanup["native_pass"] is True
for f in manifest["frames"]:
    assert hashlib.sha256((root / f["file"]).read_bytes()).hexdigest() == f["sha256"]
rows = []
for stage in (1, 5):
    for view in ("home", "mill", "mill-hd", *(f"travel-{i}" for i in range(5))):
        a,b,r = [root / f"b{stage}-{v}-{view}.png" for v in ("A","B","R")]
        assert a.read_bytes() == r.read_bytes(), "reversal mismatch"
        ia,ib = [Image.open(p).convert("RGB") for p in (a,b)]
        assert ia.size == ib.size
        diff = ImageChops.difference(ia, ib)
        bbox = diff.getbbox()
        count = diff.convert("L").point(lambda x: 255 if x > 2 else 0).histogram()[255]
        rows.append(dict(stage=stage,view=view,size=ia.size,changed_pixels_luma_gt_2=count,
                         changed_fraction=count/(ia.width*ia.height),bbox=bbox,reversal_exact=True))
        if view == "mill-hd":
            assert bbox is not None
            pair = Image.new("RGB", (ia.width*2,ia.height+40), (24,24,24))
            pair.paste(ia,(0,40));pair.paste(ib,(ia.width,40))
            d=ImageDraw.Draw(pair);d.text((10,10),"A - original",fill="white");d.text((ia.width+10,10),"B - candidate",fill="white")
            pair.save(out/f"b{stage}-mill-hd-AB.png")
result = dict(source_run=45675919389,verified_frames=48,exact_reversals=16,rows=rows,
              native_pass=True,original_cleanup_pending=not cleanup["scratch_removed"],
              art_approved=False,gameplay_test=False)
(out/"pixel-proof.json").write_text(json.dumps(result,indent=2))
print("STUDIO_NATIVE_PIXEL_PROOF_PASS",json.dumps(result))
