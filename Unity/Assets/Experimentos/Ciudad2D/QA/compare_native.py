#!/usr/bin/env python3
"""Compare authentic native Unity image captures; pixel contrast != art quality."""
import argparse,hashlib,io,json,zipfile
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw

NAME="bastion-5-1170x2532.png"

def read_native(path):
    path=Path(path)
    if path.is_file():
        with zipfile.ZipFile(path) as z:
            a=[x for x in z.namelist() if x.endswith("evidence.json")]
            assert len(a)==1,a
            record=json.loads(z.read(a[0]).decode("utf-8-sig"))
            png=[x for x in z.namelist() if x.endswith("/"+NAME)]
            assert len(png)==1,png
            raw=z.read(png[0])
    else:
        entries=list(path.rglob("evidence.json"))
        assert len(entries)==1,entries
        record=json.loads(entries[0].read_text(encoding="utf-8-sig"))
        raw=(entries[0].parent/NAME).read_bytes()
    assert record["engine"]=="Unity" and record["mode"]=="Editor_RenderTexture_no_PlayMode"
    native=next(x for x in record["captures"] if x["file"]==NAME)
    assert hashlib.sha256(raw).hexdigest()==native["sha256"]
    image=Image.open(io.BytesIO(raw)).convert("RGB")
    assert image.size==(native["width"],native["height"])==(1170,2532)
    return image,native["sha256"]

def edge_score(pixels):
    y=np.dot(pixels.astype("float64"),[.2126,.7152,.0722])
    z=4*y[1:-1,1:-1]-y[:-2,1:-1]-y[2:,1:-1]-y[1:-1,:-2]-y[1:-1,2:]
    return float(z.var())

def compare(a,b):
    data=np.array(a,dtype=np.float64)
    test=np.array(b,dtype=np.float64)
    scoreA=edge_score(data);scoreB=edge_score(test)
    return {"mean_absolute_rgb_difference":round(float(np.abs(data-test).mean()),4),
            "edge_variance_before":round(scoreA,3),
            "edge_variance_after":round(scoreB,3),
            "edge_change_percent":round(100*(scoreB/scoreA-1),2) if scoreA else None}

def run(before,after,out):
    x,shaX=read_native(before);y,shaY=read_native(after)
    areas={"full":(0,0,1170,2532),"bastion":(230,710,875,1300),
           "secondary_west":(0,480,300,1480),"secondary_east":(860,480,1170,1480)}
    stats={k:compare(x.crop(box),y.crop(box)) for k,box in areas.items()}
    dest=Path(out);dest.mkdir(parents=True,exist_ok=True)
    a=x.crop(areas["bastion"]);b=y.crop(areas["bastion"])
    board=Image.new("RGB",(a.width*2+24,a.height+35),(20,25,32))
    board.paste(a,(0,35));board.paste(b,(a.width+24,35))
    d=ImageDraw.Draw(board)
    d.text((8,10),"UNITY BEFORE",fill="white")
    d.text((a.width+32,10),"UNITY AFTER",fill="white")
    board.save(dest/"unity_native_1to1.png",optimize=True)
    result={"status":"SHA_VERIFIED_UNITY_EDITOR_NOT_ART_APPROVAL",
            "capture_before_sha256":shaX,"capture_after_sha256":shaY,
            "measurements":stats,"warning":"Increased edge variance can indicate sharpening artifacts, not new authored detail."}
    (dest/"quality_report.json").write_text(json.dumps(result,indent=2)+"\n")
    print(json.dumps(result,indent=2))

if __name__=="__main__":
    arg=argparse.ArgumentParser()
    arg.add_argument("--before",required=True);arg.add_argument("--after",required=True)
    arg.add_argument("--out",required=True)
    a=arg.parse_args()
    run(a.before,a.after,a.out)
