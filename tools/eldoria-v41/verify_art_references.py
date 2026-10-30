#!/usr/bin/env python3
"""Fail closed: verify exact approved visual references and optional Unity captures.
No generation/alteration. Static pixels may be compared; dynamic values require
manual alignment review. No automatic ART_APPROVED grant.
"""
import argparse,hashlib,json,struct,sys
from pathlib import Path

MANIFEST=Path(__file__).with_name("approved-art-manifest.json")
def png_dimensions(data):
    if data[:8]!=b"\x89PNG\r\n\x1a\n" or data[12:16]!=b"IHDR":
        raise ValueError("Not a valid PNG header")
    return struct.unpack(">II",data[16:24])
def find_one(root,name):
    hits=list(root.rglob(name))
    if len(hits)!=1:raise ValueError(f"Expected exactly one approved original {name}, found {len(hits)}")
    return hits[0]
def check(root):
    manifest=json.loads(MANIFEST.read_text("utf-8"))
    result={}
    for ref in manifest["references"]:
        file=find_one(root,ref["basename"])
        data=file.read_bytes()
        actual=hashlib.sha256(data).hexdigest()
        size=png_dimensions(data)
        if actual!=ref["sha256"] or list(size)!=[ref["width"],ref["height"]]:
            raise ValueError("Art reference changed: "+ref["basename"])
        result[ref["screen"]]={"file":str(file),"sha256":actual,"dimensions":list(size)}
    return result
def compare(reference,candidate):
    try:
        from PIL import Image,ImageChops,ImageStat
    except ImportError:
        raise RuntimeError("Optional screenshot pixel comparison requires Pillow")
    with Image.open(reference) as approved,Image.open(candidate) as rendered:
        if approved.size!=rendered.size:
            raise ValueError(f"Capture dimensions {rendered.size} differ from approved {approved.size}")
        a=approved.convert("RGB");b=rendered.convert("RGB")
        diff=ImageChops.difference(a,b)
        stats=ImageStat.Stat(diff)
        return {"pixel_identical":diff.getbbox() is None,
                "mean_absolute_channel_error":sum(stats.mean)/3,
                "warning":"Not a substitute for visual review of dynamic controls and actual Unity mobile/landscape"}
def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--reference-dir",type=Path,required=True)
    ap.add_argument("--capture",type=Path,help="optional Unity screenshot (same pixels as reference)")
    ap.add_argument("--screen",choices=["march","training","mailbox"])
    args=ap.parse_args()
    references=check(args.reference_dir)
    output={"reference_integrity":"PASS","references":references,
            "ART_APPROVED":False}
    if args.capture:
        if not args.screen:raise ValueError("--screen required with --capture")
        output["capture"]=compare(references[args.screen]["file"],args.capture)
    print(json.dumps(output,ensure_ascii=False,indent=2))
if __name__=="__main__":
    try:main()
    except Exception as exc:
        print("ELDORIA_ART_REFERENCE_FAIL "+str(exc),file=sys.stderr)
        sys.exit(1)
