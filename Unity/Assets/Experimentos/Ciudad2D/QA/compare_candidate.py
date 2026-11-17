#!/usr/bin/env python3
"""Compare original/candidate building from one genuine Unity artifact (no resizing).

Run: python QA/compare_candidate.py --artifact archive.zip --out /tmp/city2d-ab
Both images MUST originate from one matching SHA-verified evidence.json.
Metrics are descriptive, not an automatic art approval.
"""
from pathlib import Path
from PIL import Image,ImageDraw
import argparse,hashlib,io,json,zipfile
import numpy as np

ORIGINAL="bastion-5-1170x2532.png"
CANDIDATE="bastion-5-1170x2532-cuartel-candidato.png"

def verified_pair(path):
    with zipfile.ZipFile(path) as z:
        refs=[x for x in z.namelist() if x.endswith("evidence.json")]
        assert len(refs)==1,"Missing unique evidence"
        doc=json.loads(z.read(refs[0]).decode("utf-8-sig"))
        assert doc["engine"]=="Unity" and doc["mode"]=="Editor_RenderTexture_no_PlayMode"
        assert doc["candidate_cuartel_comparison_only"] is True
        files={x["file"]:x for x in doc["captures"]}
        images=[]
        for name in [ORIGINAL,CANDIDATE]:
            entries=[p for p in z.namelist() if p.endswith("/"+name)]
            assert len(entries)==1,"Missing unique capture "+name
            raw=z.read(entries[0])
            assert hashlib.sha256(raw).hexdigest()==files[name]["sha256"],name
            im=Image.open(io.BytesIO(raw)).convert("RGB")
            assert im.size==(1170,2532)
            images.append(im)
        return images,files

def run(path,out):
    images,source=verified_pair(path)
    a,b=images
    # Cuartel occupies upper-left city district at iPhone physical pixels.
    crop=(0,315,490,900)
    x=a.crop(crop);y=b.crop(crop)
    board=Image.new("RGB",(x.width*2+20,x.height+36),(24,30,38))
    board.paste(x,(0,36));board.paste(y,(x.width+20,36))
    d=ImageDraw.Draw(board)
    d.text((10,12),"UNITY ACTUAL",fill="#f3d99c")
    d.text((x.width+30,12),"UNITY CANDIDATO",fill="#f3d99c")
    diff=np.abs(np.array(x,dtype=np.float32)-np.array(y,dtype=np.float32))
    result={"status":"NATIVE_AB_SHA_VERIFIED_UNAPPROVED_ART",
            "original_sha256":source[ORIGINAL]["sha256"],
            "candidate_sha256":source[CANDIDATE]["sha256"],
            "crop_without_rescaling":[*crop],
            "rgb_mae_cuartel_area":round(float(diff.mean()),3),
            "limitations":["A stronger edge contrast is not a commercial-quality score",
                           "Single candidate does not guarantee consistency with other buildings",
                           "Mobile PlayMode, memory and production rendering are not certified"]}
    dest=Path(out);dest.mkdir(parents=True,exist_ok=True)
    board.save(dest/"Cuartel_AB_Unity_1to1.png",optimize=True)
    (dest/"Cuartel_AB_result.json").write_text(json.dumps(result,indent=2)+"\n")
    print(json.dumps(result,indent=2))

if __name__=="__main__":
    parser=argparse.ArgumentParser()
    parser.add_argument("--artifact",required=True)
    parser.add_argument("--out",required=True)
    args=parser.parse_args()
    run(args.artifact,args.out)
