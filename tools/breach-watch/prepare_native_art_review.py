#!/usr/bin/env python3
"""Verify genuine Unity run artifacts and create reviewer-friendly A/B JPG.

Never use generated/mock screenshots. Exact GitHub SHA, real PNG SHA256,
8-shot inventory, original gameplay and no ART_APPROVED are required.
"""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from compare_native_art import compare, SIZES, SCENES

EXPECTED={f"real-unity-{scene}-{w}x{h}.png" for scene in SCENES for w,h in SIZES}


def collect(root:Path, out:Path, source_sha:str):
    found=list(root.rglob("BreachWatchNativeEvidence.json"))
    if len(found)!=1:
        raise ValueError(f"Expected one native evidence JSON in {root}, found {len(found)}")
    report=json.loads(found[0].read_text(encoding="utf-8"))
    if report.get("source")!=source_sha or report.get("screenshotCount")!=8:
        raise ValueError(f"Native evidence source/count mismatch: {root}")
    if report.get("status")!="UNITY_NATIVE_INDEPENDENT_CAPTURED_ART_REVIEW_PENDING":
        raise ValueError("Native Unity report not from certified live capture")
    if report.get("artworkApproved") or report.get("saveUnchanged") is not True:
        raise ValueError("Report asserts art approval or mutated the game save")
    if report.get("oldCanvasVisible")!=0 or report.get("oldHudGraphicsReused"):
        raise ValueError("Actual legacy HUD overlays remain present")
    if report.get("functioningNewNavigationClicks",0)<6:
        raise ValueError("Not enough real native pointer interactions")
    shots=report.get("shots")
    if not isinstance(shots,list) or {x.get("file") for x in shots}!=EXPECTED:
        raise ValueError("Incomplete or duplicated Unity screenshot inventory")
    out.mkdir(parents=True,exist_ok=True)
    for shot in shots:
        name=shot["file"]
        source=found[0].parent/name
        if not source.is_file():
            raise ValueError(f"Missing real Unity image: {name}")
        digest=hashlib.sha256(source.read_bytes()).hexdigest()
        if digest!=shot.get("sha256"):
            raise ValueError(f"Untrusted/stale image digest: {name}")
        expected_selection="bastion" if "-bastion-" in name else ""
        if shot.get("selected")!=expected_selection:
            raise ValueError(f"Real city/selected-Bastion evidence mismatch: {name}")
        wh=tuple(map(int,name.removesuffix(".png").split("-")[-1].split("x")))
        if (shot.get("width"),shot.get("height"))!=wh:
            raise ValueError(f"Wrong Unity output resolution: {name}")
        shutil.copyfile(source,out/name)
    return report


def run(baseline:Path,candidate:Path,out:Path,baseline_sha:str,candidate_sha:str):
    if baseline_sha==candidate_sha:
        raise ValueError("The comparison must use a different HEAD")
    before=collect(baseline,out/"original",baseline_sha)
    after=collect(candidate,out/"revised",candidate_sha)
    results=compare(out/"original",out/"revised",out)
    manifest={
        "baseline_source":baseline_sha,
        "candidate_source":candidate_sha,
        "captured_screens":len(EXPECTED),
        "candidate_native_pointer_clicks":after["functioningNewNavigationClicks"],
        "source_authenticity":"SHA256 validated against original Unity evidence JSON",
        "art_approved":False,
        "owner_visual_review_required":True,
        "diff":results,
    }
    (out/"vigilia_review_manifest.json").write_text(
        json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf-8")
    print("VIGILIA_REAL_NATIVE_JPG_AB_READY (not ART_APPROVED)")
    return manifest


if __name__=="__main__":
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument("baseline",type=Path)
    ap.add_argument("candidate",type=Path)
    ap.add_argument("output",type=Path)
    ap.add_argument("--baseline-sha",required=True)
    ap.add_argument("--candidate-sha",required=True)
    a=ap.parse_args()
    run(a.baseline,a.candidate,a.output,a.baseline_sha,a.candidate_sha)
