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
    # The reference's artistic quality lives in small, independent regions
    # as much as in the full scene. The full-screen A/B above is preserved,
    # and this additional paired sheet reveals the actual GPU typography,
    # sculpted nav surfaces, resource chips, mission and burgundy CTA.
    # Each pair is cropped at IDENTICAL pixel coordinates from the 9:16
    # owner image and the exact-SHA-verified Unity image. This is a diagnostic
    # enlargement, not a reconstructed screen or a subjective art score.
    focus=(
        ("01  PERFIL Y PODER", (0,0,385,135)),
        ("02  RECURSOS", (445,0,900,135)),
        ("03  MISION Y LA BRECHA", (0,112,610,275)),
        ("04  CASTILLO REAL", (205,305,820,810)),
        ("05  FICHA DEL BASTION Y BOTON BURDEOS", (500,760,900,1365)),
        ("06  MEDALLONES DE NAVEGACION", (90,1370,805,1590)),
    )
    rows=[]
    colwidth=520
    for label,rect in focus:
        crops=(reference.crop(rect),actual.crop(rect))
        tw=min(colwidth,round((rect[2]-rect[0])*2.1))
        th=round((rect[3]-rect[1])*tw/(rect[2]-rect[0]))
        row=Image.new("RGB",(colwidth*2+58,th+94),bg)
        rd=ImageDraw.Draw(row)
        rd.text((20,8),label,font=font(24),fill=bronze)
        rd.text((20,43),"A · REFERENCIA",font=font(16),fill=paper)
        rd.text((colwidth+40,43),"B · UNITY REAL",font=font(16),fill=paper)
        for i,frame in enumerate(crops):
            thumb=frame.resize((tw,th),Image.Resampling.LANCZOS)
            row.paste(thumb,(20+i*(colwidth+20),82))
        rows.append(row)
    detail=Image.new("RGB",(colwidth*2+58,sum(row.height for row in rows)+
            (len(rows)+1)*20+85),bg)
    dp=ImageDraw.Draw(detail)
    dp.text((20,12),"VIGILIA · 6 ZONAS ARTISTICAS · A/B",
            font=font(27),fill=bronze)
    dp.text((20,55),"Recortes de imagen aprobada y Unity verificado; no es aprobacion.",
            font=font(16),fill=paper)
    dy=105
    for row in rows:
        detail.paste(row,(0,dy))
        dy+=row.height+20
    detail_name="vigilia_OWNER_details_vs_REAL_Unity.jpg"
    detail.save(out/detail_name,quality=92,subsampling=0,optimize=True)
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
        "focus_region_count":len(focus),
        "detail_sheet":detail_name,
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
