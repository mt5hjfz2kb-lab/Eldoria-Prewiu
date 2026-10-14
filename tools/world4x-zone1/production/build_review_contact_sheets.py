"""Create honest read-only reduced JPEG A/B sheets for independent human/agent vision."""
import base64
import hashlib
import json
import pathlib
from PIL import Image, ImageDraw
source=pathlib.Path("docs/world4x-zone1/evidence/real-multizoom-38583442987")
dest=pathlib.Path("/tmp/zoneone-contact")
dest.mkdir(parents=True,exist_ok=False)
evidence=json.loads((source/"zoom-review.json").read_text())
rows=[]
for view in ("global","regional","local","mobile"):
    pair=[]
    for name in ("before","after"):
        f=source/(name+"-"+view+"-blender.png")
        assert f.exists()
        record=next(x for x in evidence["records"] if x["view"]==view and x["version"]==name)
        assert hashlib.sha256(f.read_bytes()).hexdigest()==record["image_sha256"]
        img=Image.open(f).convert("RGB")
        assert img.size==(record["width"],record["height"])
        target=(390,510) if view=="mobile" else (480,390)
        img.thumbnail(target,Image.Resampling.LANCZOS)
        pair.append(img)
    gap=14
    top=34
    w=max(pair[0].width,pair[1].width)
    h=max(pair[0].height,pair[1].height)
    sheet=Image.new("RGB",(w*2+gap,h+top),(237,237,237))
    dr=ImageDraw.Draw(sheet)
    dr.text((10,10),"BEFORE - "+view.upper(),fill=(20,20,20))
    dr.text((w+gap+10,10),"AFTER - "+view.upper(),fill=(20,20,20))
    sheet.paste(pair[0],(0,top))
    sheet.paste(pair[1],(w+gap,top))
    picture=dest/(view+"-true-AB.jpg")
    sheet.save(picture,quality=80,optimize=True,subsampling=2)
    (dest/(view+"-true-AB.b64")).write_text(base64.b64encode(picture.read_bytes()).decode("ascii"),encoding="ascii")
    rows.append({"view":view,"sheet_sha256":hashlib.sha256(picture.read_bytes()).hexdigest(),
        "base64_sha256":hashlib.sha256((dest/(view+"-true-AB.b64")).read_bytes()).hexdigest(),
        "source_images":[x["image_sha256"] for x in evidence["records"] if x["view"]==view],
        "type":"REAL RESIZED BLENDER CAPTURE, NOT ILLUSTRATION","size":sheet.size})
(dest/"provenance.json").write_text(json.dumps({"source_run":38583442987,"artist_run":38580519176,
    "art_approved":False,"native_unity":False,"sheets":rows},indent=2)+"\n")
