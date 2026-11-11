#!/usr/bin/env python3
"""Safely stage ONE Runway illustration before Unity, never as approved gameplay art.

Requires exact source URL in a temporary request, validates real transparent PNG,
writes candidate-only bytes/provenance, and never touches Unity, other branches,
main, player saves or art approval. The image still needs HUMAN visual QA.
"""
import argparse
import hashlib
import io
import json
import os
from pathlib import Path
from urllib.parse import urlparse
from urllib.request import Request, urlopen
from PIL import Image

ROOT = Path("tools/daily-treasures")
SCENES = {
    "lyra": "canonical_hero_sprite_overlay_required",
    "explorers": "canonical_world_overlay_required",
    "vigilants": "canonical_event_insignia_overlay_required",
    "bastion": "actual_city_capture_overlay_required",
}
EXPECTED_HOST = "dnznrvs05pmza.cloudfront.net"

def validate(data):
    if len(data) > 16_000_000 or not data.startswith(b"\x89PNG\r\n\x1a\n"):
        raise ValueError("The Runway result is not a genuine bounded PNG")
    with Image.open(io.BytesIO(data)) as im:
        im.load()
        if im.mode != "RGBA":
            raise ValueError("Isolated source art must have a true RGBA alpha channel")
        if im.width < 1024 or im.height < 512:
            raise ValueError("Source illustration below production spatial minimum")
        alpha = im.getchannel("A")
        hist = alpha.histogram()
        transparent = sum(hist[:16]) / (im.width*im.height)
        visible = sum(hist[160:]) / (im.width*im.height)
        if not (.08 <= transparent <= .94 and visible >= .04):
            raise ValueError("Generated poster, blank or incorrect alpha segmentation")
        return {"size":[im.width,im.height],"transparent_ratio":round(transparent,4),
                "visible_ratio":round(visible,4)}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--request",type=Path)
    p.add_argument("--selftest",action="store_true")
    a=p.parse_args()
    if a.selftest:
        x=Image.new("RGBA",(1024,576),(0,0,0,0))
        from PIL import ImageDraw
        ImageDraw.Draw(x).ellipse((130,90,850,480),fill=(147,101,53,245))
        buffer=io.BytesIO(); x.save(buffer,format="PNG")
        assert validate(buffer.getvalue())["visible_ratio"] > .04
        print("PASS: transparent candidate image admission, no Unity or art approval")
        return
    if not a.request or not a.request.is_file():
        print("NO ART CANDIDATE REQUEST; no work")
        return
    spec=json.loads(a.request.read_text())
    if spec.get("scene") not in SCENES or spec.get("stage")!="PREUNITY_CANDIDATE_ONLY" or spec.get("art_approved") is not False:
        raise ValueError("Import request not a safe non-approved Exploradores candidate")
    scene=spec["scene"]
    candidate=ROOT / "art-candidates" / (scene + "_stage.png")
    receipt=ROOT / "art-candidates" / (scene + "_stage.provenance.json")
    url=spec.get("temporary_download_url","")
    link=urlparse(url)
    if link.scheme!="https" or link.hostname != EXPECTED_HOST:
        raise ValueError("Untrusted source host")
    if candidate.exists() or receipt.exists():
        if not (candidate.exists() and receipt.exists()):
            raise ValueError("Half-imported source candidate: refuse overwrite")
        actual=hashlib.sha256(candidate.read_bytes()).hexdigest()
        expected=json.loads(receipt.read_text())["sha256"]
        if actual!=expected:
            raise ValueError("Existing candidate differs: refuse overwrite")
        print("UNCHANGED candidate exact SHA; no art promotion")
        return
    request=Request(url,headers={"User-Agent":"Eldoria-Daily-Art-Source-Importer"})
    with urlopen(request,timeout=45) as reply:
        raw=reply.read(16_000_001)
    info=validate(raw)
    digest=hashlib.sha256(raw).hexdigest()
    candidate.parent.mkdir(parents=True,exist_ok=True)
    candidate.write_bytes(raw)
    provenance={"scene":scene,"status":"PREUNITY_UNREVIEWED_CANDIDATE",
        "art_approved":False,"unity_native_verified":False,
        "identity":SCENES[scene],
        "generated_illustration_not_authentic_gameplay":True,
        "source_task_id":spec["source_task_id"],
        "segmentation_task_id":spec["segmentation_task_id"],
        "sha256":digest,**info}
    receipt.write_text(json.dumps(provenance,indent=2)+"\n")
    print(json.dumps(provenance,indent=2))

if __name__=="__main__":main()
