#!/usr/bin/env python3
"""Source-only regression for real Unity raster A/B helper. No fake ART_APPROVED."""
from pathlib import Path
from tempfile import TemporaryDirectory
import json
import hashlib
from prepare_native_art_review import run as prepare_review
from PIL import Image, ImageDraw
from compare_native_art import compare, SIZES, SCENES


def verify():
    with TemporaryDirectory(prefix="vigilia-ab-source-test-") as scratch:
        root=Path(scratch)
        before=root/"before";after=root/"after";out=root/"out"
        before.mkdir();after.mkdir()
        for scene in SCENES:
            for w,h in SIZES:
                name=f"real-unity-{scene}-{w}x{h}.png"
                first=Image.new("RGB",(w,h),(52,47,39))
                changed=first.copy()
                if scene=="bastion":
                    ink=ImageDraw.Draw(changed)
                    # An exact 20x20 color change inside the canonical
                    # card crop must be detected. No game image is invented:
                    # these are controlled unit-test fixtures only.
                    ink.rectangle((w-45,int(h*.68),w-26,int(h*.68)+19),
                                  fill=(217,142,69))
                first.save(before/name)
                changed.save(after/name)
        records=compare(before,after,out,threshold=12)
        assert len(records)==len(SIZES)*len(SCENES)
        assert all(r["changed_pixels"]==0 and r["bbox"] is None
                   for r in records if "-city-" in r["file"])
        assert all(r["changed_pixels"]==400 and r["bbox"] is not None
                   for r in records if "-bastion-" in r["file"])
        report=__import__("json").loads((out/"vigilia_native_art_diff.json").read_text())
        assert report["art_approved"] is False
        assert len(report["records"])==8
        with Image.open(out/"vigilia_native_art_ab.jpg") as proof:
            assert proof.width>780 and proof.height>800
        # Fail-closed provenance test: these generated pixels are UNIT TEST
        # fixtures only. They are NEVER shown or labeled as Unity captures.
        def make_evidence(folder:Path,sha:str):
            shots=[]
            for scene in SCENES:
                for w,h in SIZES:
                    name=f"real-unity-{scene}-{w}x{h}.png"
                    shots.append({
                        "file":name,"width":w,"height":h,
                        "selected":"bastion" if scene=="bastion" else "",
                        "sha256":hashlib.sha256((folder/name).read_bytes()).hexdigest(),
                    })
            (folder/"BreachWatchNativeEvidence.json").write_text(
                json.dumps({
                    "source":sha,"screenshotCount":8,"shots":shots,
                    "status":"UNITY_NATIVE_INDEPENDENT_CAPTURED_ART_REVIEW_PENDING",
                    "artworkApproved":False,"saveUnchanged":True,
                    "oldCanvasVisible":0,"oldHudGraphicsReused":False,
                    "functioningNewNavigationClicks":6,
                }),encoding="utf-8")
        make_evidence(before,"a"*40)
        make_evidence(after,"b"*40)
        certified=prepare_review(before,after,root/"certified","a"*40,"b"*40)
        assert certified["captured_screens"]==8 and not certified["art_approved"]
        assert (root/"certified/vigilia_native_art_ab.jpg").is_file()
        # Intentionally corrupt a baseline pixel AFTER signing the manifest:
        # it must be rejected rather than silently becoming user evidence.
        tampered=before/"real-unity-bastion-390x844.png"
        with Image.open(tampered) as png:
            invalid=png.convert("RGB")
        invalid.putpixel((5,5),(14,15,16))
        invalid.save(tampered)
        try:
            prepare_review(before,after,root/"must-reject","a"*40,"b"*40)
        except ValueError as exc:
            assert "digest" in str(exc).lower()
        else:
            raise AssertionError("Stale or tampered screenshot was accepted")
        print("VIGILIA_AUTHENTIC_AB_COMPARATOR_SOURCE_TEST_PASS; real screenshot SHA gate enforced; NO ART_APPROVED")


if __name__=="__main__":
    verify()
