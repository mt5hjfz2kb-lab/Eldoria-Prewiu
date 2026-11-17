#!/usr/bin/env python3
"""Extract a small, movable archery-range detail from the existing Cuartel V2.
No novel building, gameplay, scene, or master PNG edits; output is a disposable
candidate with a separate blue-violet contact shadow and exact source SHA.
"""
# The PNGs emitted here are strictly art candidates; no main/production scene import.
from pathlib import Path
import hashlib
import json
import numpy as np
import cv2
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
QA = ROOT / "QA"
SRC = ROOT / "Sprites/Candidates/Cuartel_Premium_V2.png"
DEST = ROOT / "Sprites/Candidates/Cuartel_Entrenamiento_RecorteV1.png"
SHADE = ROOT / "Sprites/Candidates/Cuartel_Entrenamiento_RecorteV1_Shadow.png"
SOURCE_SHA = "0dcb8edb8237621f13893893edce20fec51deb1248df8bcbc3a89ea3990b4062"
META_GUIDS = {
    DEST.name: "ab2bc96cfdcf495e87742ca3f68e1e75",
    SHADE.name: "b74cbe392dfd44a1875ef9b302fc41be",
}
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
def build():
    if sha(SRC) != SOURCE_SHA:
        raise ValueError("Cuartel V2 source altered; refuse to derive art")
    if DEST.exists() or SHADE.exists():
        raise ValueError("Candidate already exists; never silently overwrite")
    image = Image.open(SRC).convert("RGBA")
    rgba = np.array(image)
    R, G, B = [rgba[:, :, k].astype(np.int16) for k in range(3)]
    mask = np.zeros(rgba.shape[:2], dtype=np.uint8)
    yy, xx = np.ogrid[:image.height, :image.width]
    for cx,cy,rx,ry in [(1154,1200,60,60),(1227,1178,60,60)]:
        inside = ((xx-cx)/rx)**2 + ((yy-cy)/ry)**2 <= 1
        warm = (R > 82) & (R > G*1.12) & (R > B*1.13)
        region = np.where(inside & warm, 255, 0).astype(np.uint8)
        contours, _ = cv2.findContours(region, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        selected = np.zeros_like(mask)
        for contour in contours:
            moment = cv2.moments(contour)
            if moment["m00"] <= 100: continue
            xmid = moment["m10"] / moment["m00"]
            ymid = moment["m01"] / moment["m00"]
            if abs(xmid-cx) < 30 and abs(ymid-cy) < 30:
                cv2.drawContours(selected,[cv2.convexHull(contour)],-1,255,-1)
        selected = cv2.morphologyEx(selected,cv2.MORPH_CLOSE,np.ones((7,7),np.uint8))
        selected = cv2.dilate(selected,np.ones((3,3),np.uint8))
        mask = cv2.bitwise_or(mask,selected)
    ys,xs = np.indices(mask.shape)
    backdrop=(xs>1110)&(xs<1187)&(ys<1176)&(R<129)
    mask[backdrop] = 0
    shaped=Image.fromarray(mask)
    painter=ImageDraw.Draw(shaped)
    supports=[
        [(1134,1235),(1151,1233),(1115,1322),(1097,1325)],
        [(1171,1234),(1186,1235),(1200,1310),(1185,1315)],
        [(1204,1211),(1220,1214),(1192,1308),(1180,1310)],
        [(1241,1211),(1257,1213),(1290,1283),(1271,1288)],
    ]
    for poly in supports: painter.polygon(poly, fill=255)
    shaped=shaped.filter(ImageFilter.GaussianBlur(0.8))
    alpha=np.minimum(np.asarray(shaped),rgba[:,:,3])
    rgba[:,:,3]=alpha
    output=Image.fromarray(rgba,"RGBA")
    bbox=output.getchannel("A").getbbox()
    pad=5
    bbox=(max(0,bbox[0]-pad),max(0,bbox[1]-pad),
          min(output.width,bbox[2]+pad),min(output.height,bbox[3]+pad))
    output=output.crop(bbox)
    assert 140 < output.width < 280 and 120 < output.height < 280
    assert output.getchannel("A").getextrema() == (0,255)
    output.save(DEST,optimize=True)
    sw,sh=output.size
    contact=Image.new("L",(sw,sh),0)
    d=ImageDraw.Draw(contact)
    d.ellipse((int(sw*.20),int(sh*.89),int(sw*.83),int(sh*.99)),fill=110)
    contact=contact.filter(ImageFilter.GaussianBlur(5))
    shade=Image.new("RGBA",(sw,sh),(40,33,66,0));shade.putalpha(contact)
    shade.save(SHADE,optimize=True)
    # Unity import in exact same PPU/pivot settings as the proven source asset.
    template=(SRC.with_suffix(".png.meta")).read_text(encoding="utf-8")
    for dest in (DEST,SHADE):
        content=template.replace("guid: 193b1e9c3228acc31cc8113910996a38",
                                 "guid: "+META_GUIDS[dest.name])
        if "guid: "+META_GUIDS[dest.name] not in content: raise ValueError("Meta contract changed")
        dest.with_suffix(dest.suffix+".meta").write_text(content,encoding="utf-8")
    previews=[]
    for background,desc in [((20,26,32),"dark"),((77,80,81),"grey"),
                            ((44,68,39),"green"),((12,12,14),"black")]:
        canvas=Image.new("RGBA",output.size,background+(255,))
        canvas.alpha_composite(output)
        file=QA/("CuartelEntrenamientoV1_"+desc+"_OFFLINE.png")
        canvas.convert("RGB").save(file,optimize=True)
        previews.append({"file":file.name,"sha256":sha(file),"not_Unity":True})
    manifest={
        "schema":1, "status":"EXTRACTED_CANDIDATE_NOT_ART_APPROVED",
        "source":str(SRC.relative_to(ROOT)), "source_sha256":SOURCE_SHA,
        "functional_identity":"two archery targets with wooden supports from own Cuartel V2",
        "candidate":str(DEST.relative_to(ROOT)),"candidate_sha256":sha(DEST),
        "dimensions":list(output.size),"shadow":str(SHADE.relative_to(ROOT)),
        "shadow_sha256":sha(SHADE),"no_square_platform":True,
        "separate_alpha":True,"pivot":"BottomCenter","pixels_per_unit":100,
        "mask":"warm-color constrained convex targets, explicit braces, local alpha",
        "previews":previews,"Unity":"PENDING","PlayMode":"PENDING",
        "physical_iPhone":"PENDING","main_modified":False,"gameplay_modified":False,
    }
    (QA/"CuartelEntrenamientoV1_Manifest.json").write_text(
        json.dumps(manifest,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("CUARTEL_TARGETS_V1_SOURCE_LOCK_ALPHA_RGBA_OFFLINE_PASS",manifest["candidate_sha256"],output.size)
if __name__=="__main__": build()
