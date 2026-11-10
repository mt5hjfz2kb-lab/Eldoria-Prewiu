#!/usr/bin/env python3
"""Real A/B: owner-supplied approved art vs 8-shot SHA-verified Unity capture.

No generated mock scene, no game HUD composite, no automatic ART_APPROVED.
Keep owner image private/local, only the comparison is an optional artifact.
"""
from __future__ import annotations
import argparse, hashlib, json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
from prepare_native_art_review import collect

SIZE=(900,1600)
SHOT="real-unity-bastion-900x1600.png"

def digest(path:Path)->str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

def font(size:int):
    for name in ("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                 "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf"):
        try:return ImageFont.truetype(name,size)
        except OSError:pass
    return ImageFont.load_default()

def make(owner:Path,evidence:Path,out:Path,candidate_sha:str):
    if len(candidate_sha)!=40 or any(x not in '0123456789abcdef' for x in candidate_sha.lower()):
        raise ValueError("Exact full commit SHA required")
    out.mkdir(parents=True,exist_ok=True)
    report=collect(evidence,out/"verified_native",candidate_sha)
    actual_path=out/"verified_native"/SHOT
    with Image.open(actual_path) as im:
        im.load()
        if im.size!=SIZE:raise ValueError("Unity screenshot not 900x1600")
        actual=im.convert("RGB")
    with Image.open(owner) as img:
        img.load()
        if img.width<600 or img.height<1000 or abs(img.width/img.height-.5625)>.02:
            raise ValueError("Approved reference must be real 9:16 portrait")
        reference=img.convert("RGB").resize(SIZE,Image.Resampling.LANCZOS)
    bg=(22,21,20);paper=(232,223,205);bronze=(215,160,91)
    left=20;right=960;top=78
    canvas=Image.new("RGB",(1880,2110),bg)
    pen=ImageDraw.Draw(canvas)
    pen.text((left+10,18),"A  REFERENCIA APROBADA",font=font(32),fill=bronze)
    pen.text((right+10,18),"B  CAPTURA REAL DE UNITY",font=font(32),fill=bronze)
    canvas.paste(reference,(left,top))
    canvas.paste(actual,(right,top))
    # Same location in the original pixels: crop for inspection only.
    # Neither image is used as a game asset or rendered over the other.
    region=(505,760,900,1360)
    bottom=1700
    for image,x,title in ((reference,left,"A - ficha aprobada"),
                          (actual,right,"B - ficha Unity")):
        crop=image.crop(region)
        crop.thumbnail((470,310),Image.Resampling.LANCZOS)
        canvas.paste(crop,(x,bottom+36))
        pen.text((x+5,bottom),title,font=font(22),fill=paper)
    pen.text((left,2070),"A: imagen aprobada por el propietario",
             font=font(17),fill=paper)
    pen.text((right,2070),"B: Unity SHA verificado "+candidate_sha[:12],
             font=font(17),fill=paper)
    output=out/"vigilia_OWNER_reference_vs_REAL_Unity.jpg"
    canvas.save(output,quality=91,subsampling=0,optimize=True)
    manifest={
        "reference_sha256":digest(owner),
        "candidate_source_sha":candidate_sha,
        "unity_png_sha256":digest(actual_path),
        "native_status":report["status"],
        "native_save_unchanged":report["saveUnchanged"],
        "native_pointer_interactions":report["functioningNewNavigationClicks"],
        "reference_is_user_provided":True,
        "candidate_is_verified_unity_screenshot":True,
        "visual_score":None,
        "art_approved":False,
        "output":output.name
    }
    (out/"vigilia_OWNER_reference_vs_Unity_provenance.json").write_text(
        json.dumps(manifest,indent=2,ensure_ascii=False),encoding="utf-8")
    print("OWNER_REFERENCE_VS_REAL_UNITY_AB_CREATED",output)
    return manifest

if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--owner",type=Path,required=True)
    parser.add_argument("--native",type=Path,required=True)
    parser.add_argument("--out",type=Path,required=True)
    parser.add_argument("--sha",required=True)
    args=parser.parse_args()
    make(args.owner,args.native,args.out,args.sha)
