#!/usr/bin/env python3
"""Review ORIGINAL approved dual-panel Heroes art vs SHA-verified real Unity.

Original 1536x1024 is two side-by-side reference screens, NOT one playable
viewport. Preserve proportions, state and provenance; never infer ART_APPROVED.
"""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

REFERENCE_SHA256="f7969ca3ba45e6dc68bf9ff2633d1995cbc1f5c6e9d030c08f3e1b13af01fb38"
CAPTURES=(
    ("BreachWatchDestinationEvidence.json","real-unity-heroes-1600x900.png",(1600,900)),
    ("BreachWatchHeroProfilesEvidence.json","real-unity-hero-aldric-resumen-900x1600.png",(900,1600)),
)

def digest(path:Path)->str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

def font(size:int):
    for path in ("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                 "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf"):
        try: return ImageFont.truetype(path,size)
        except OSError: pass
    return ImageFont.load_default()

def verify(native:Path,evidence:str,filename:str,source_sha:str,size:tuple[int,int]):
    records=list(native.rglob(evidence))
    if len(records)!=1:
        raise ValueError("Expected exactly one original native evidence "+evidence)
    record=json.loads(records[0].read_text(encoding="utf-8"))
    if record.get("source")!=source_sha or record.get("saveUnchanged") is not True or record.get("artworkApproved") is not False:
        raise ValueError("Evidence not tied to requested source or save state")
    matches=[s for s in record.get("screens",[]) if s.get("file")==filename]
    if len(matches)!=1:raise ValueError("Native screenshot not indexed exactly once: "+filename)
    shot=matches[0]
    if ((shot.get("width"),shot.get("height"))!=size or
        shot.get("oldCanvasOff") is not True or
        shot.get("newCanvasOwnsScreen") is not True):
        raise ValueError("Actual viewport/old Canvas/real screen gate failed")
    image=records[0].parent/filename
    if not image.is_file() or shot.get("sha256")!=digest(image):
        raise ValueError("Native evidence image was absent or changed")
    with Image.open(image) as source:
        if source.size!=size:raise ValueError("Native PNG dimensions do not match evidence")
    return image

def fit(image:Image.Image,max_width:int,max_height:int)->Image.Image:
    image=image.convert("RGB").copy()
    image.thumbnail((max_width,max_height),Image.Resampling.LANCZOS)
    return image

def compare(reference:Path,native:Path,out:Path,source_sha:str):
    if len(source_sha)!=40 or any(c not in "0123456789abcdef" for c in source_sha.lower()):
        raise ValueError("Exact 40-character candidate source SHA required")
    if digest(reference)!=REFERENCE_SHA256:
        raise ValueError("This is not the original owner-approved two-panel Heroes reference")
    with Image.open(reference) as loaded:
        if loaded.size!=(1536,1024):
            raise ValueError("Owner reference dimensions changed")
        original=loaded.convert("RGB")
    salon_path,profile_path=(verify(native,e,n,source_sha,size) for e,n,size in CAPTURES)
    with Image.open(salon_path) as salon_file,Image.open(profile_path) as profile_file:
        salon=salon_file.convert("RGB")
        profile=profile_file.convert("RGB")
        pairs=[
          ("SALON / GALERIA",original.crop((0,0,768,1024)),
           salon.crop((450,0,1150,900)),
           "Original: first panel 768x1024","Unity: actual 1600x900, labelled central 700px crop"),
          ("FICHA ALDRIC",original.crop((768,0,1536,1024)),profile,
           "Original: second panel 768x1024","Unity: entire true 900x1600 portrait"),
        ]
        width=1560
        row_height=1090
        canvas=Image.new("RGB",(width,row_height*2+235),(22,21,20))
        draw=ImageDraw.Draw(canvas)
        draw.text((24,15),"ELDORIA / HEROES / APPROVED OWNER ART vs REAL UNITY",font=font(30),fill=(229,179,100))
        draw.text((24,65),"The owner reference contains TWO screens, not one gameplay screenshot.",font=font(20),fill=(218,207,185))
        for i,(name,approved,real,caption_a,caption_b) in enumerate(pairs):
            y=130+i*row_height
            draw.text((24,y),name,font=font(27),fill=(238,227,204))
            for x,title,caption,frame in ((24,"A   APPROVED",caption_a,approved),
                                          (810,"B   REAL UNITY",caption_b,real)):
                draw.text((x,y+38),title,font=font(24),fill=(229,179,100))
                draw.text((x,y+75),caption,font=font(16),fill=(221,213,194))
                thumb=fit(frame,720,920)
                canvas.paste(thumb,(x+(720-thumb.width)//2,y+115))
        foot=130+row_height*2
        draw.text((24,foot),"Unity SHA, source and old-Canvas status verified; ART_APPROVED remains false.",font=font(18),fill=(223,207,164))
        draw.text((24,foot+32),"Different viewport shapes are labelled. No automatic subjective art score.",font=font(18),fill=(223,207,164))
        out.mkdir(parents=True,exist_ok=True)
        output=out/"heroes_owner_reference_vs_real_unity.jpg"
        canvas.crop((0,0,width,foot+92)).save(output,quality=91,subsampling=0,optimize=True)
    manifest={
        "approved_reference_sha256":REFERENCE_SHA256,
        "approved_sheet":"1536x1024, two reference panels 768x1024 each",
        "candidate_sha":source_sha,
        "salon_native_sha256":digest(salon_path),
        "profile_native_sha256":digest(profile_path),
        "native_save_unchanged":True,"old_canvas_inactive":True,
        "method":"reference two-panel split; no stretching; landscape real center crop explicitly labelled; entire real portrait",
        "art_approved":False,"physical_mobile_tested":False,
        "image":output.name
    }
    (out/"heroes_owner_reference_vs_Unity_manifest.json").write_text(
        json.dumps(manifest,indent=2,ensure_ascii=False),encoding="utf-8")
    return manifest

if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reference",required=True,type=Path)
    parser.add_argument("--native",required=True,type=Path)
    parser.add_argument("--out",required=True,type=Path)
    parser.add_argument("--sha",required=True)
    args=parser.parse_args()
    print(json.dumps(compare(args.reference,args.native,args.out,args.sha),indent=2))
