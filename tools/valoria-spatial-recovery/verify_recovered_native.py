#!/usr/bin/env python3
"""Recover ONLY genuine archived Unity 6.3 Editor frames for comparison video.

This validator operates on already-existing frames and NUnit XML. It does NOT
create substitute gameplay images, operate Unity, certify 8/10 art, or write saves.
"""
from __future__ import annotations

import hashlib
import json
from pathlib import Path
import subprocess
import argparse
import xml.etree.ElementTree as ET

from PIL import Image, ImageChops

RUN_ID = 44285505085
ORIGINAL_ARTIFACT = 14419596041
SOURCE_SHA256 = "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06"
SOURCE_BRANCH_SHA = "4c4b88047a06e2d94ff722e6ea31678fc30981c0"
TRACKS = ("pan", "approach", "retreat")
DIMENSIONS = ((390, 844), (1170, 2532))


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate(folder: Path) -> dict:
    if folder.name != "Ciudad2DDepthPilotEvidence":
        raise RuntimeError("Expected unmodified Unity output directory")
    manifest = json.loads((folder / "evidence.json").read_text(encoding="utf-8"))
    assert manifest["engine"] == "Unity" and manifest["mode"] == "Editor_RenderTexture"
    assert manifest["baseline_scene_sha256"] == SOURCE_SHA256
    assert manifest["source_authority"].endswith("@" + "10dbc197d48ea8517c6a3dd6c22f1a2abf75d508")
    assert manifest["mobile_device_test"] == "NOT_PERFORMED"
    assert manifest["webgl_test"] == "NOT_PERFORMED"
    assert manifest["art_approved"] == "NO"
    assert manifest["gameplay_changed"] == "NO"
    assert manifest["fps"] == 12 and manifest["frames_per_trajectory"] == 18
    entries = manifest["frames"]
    assert len(entries) == 114, len(entries)

    by_key = {}
    for row in entries:
        assert row["variant"] in ("A", "B")
        assert row["trajectory"] in TRACKS
        assert (row["width"],row["height"]) in DIMENSIONS
        assert (row["frame"] in range(18))
        assert row["width"] == 390 or row["frame"] == 9
        key = (row["variant"], row["trajectory"], row["frame"], row["width"],row["height"])
        assert key not in by_key, ("duplicated frame", key)
        filename = row["file"]
        assert "/" not in filename and "\\" not in filename and filename.endswith(".png")
        path = folder / filename
        assert path.is_file() and path.stat().st_size > 8000
        assert sha(path) == row["sha256"], "Native PNG hash mismatch"
        with Image.open(path) as im:
            assert im.size == (row["width"],row["height"])
        by_key[key] = row
    assert len(by_key) == 114
    pairs = 0
    changed = 0
    large_changed = 0
    motion = {}
    for track in TRACKS:
        for i in range(18):
            for w,h in DIMENSIONS:
                if w == 1170 and i != 9:
                    continue
                a = by_key[("A",track,i,w,h)]
                b = by_key[("B",track,i,w,h)]
                for k in ("zoom","focusX","focusY","seconds",
                          "actualCameraX","actualCameraY","actualOrthographicSize"):
                    assert abs(a[k]-b[k]) < 1e-4,(track,i,k)
                pairs += 1
                if w==390:
                    with Image.open(folder/a["file"]) as ia, Image.open(folder/b["file"]) as ib:
                        if ImageChops.difference(ia.convert("RGB"),ib.convert("RGB")).getbbox():
                            changed += 1
        for variant in ("A","B"):
            frames=[by_key[(variant,track,i,390,844)] for i in range(18)]
            shifts={
                "camera_x":round(max(x["actualCameraX"] for x in frames)-min(x["actualCameraX"] for x in frames),4),
                "camera_y":round(max(x["actualCameraY"] for x in frames)-min(x["actualCameraY"] for x in frames),4),
                "orthographic_size":round(max(x["actualOrthographicSize"] for x in frames)-min(x["actualOrthographicSize"] for x in frames),4)
            }
            assert sum(shifts.values()) > .05, ("Unity camera is static",track,variant)
            with Image.open(folder/frames[0]["file"]) as first, Image.open(folder/frames[-1]["file"]) as last:
                assert ImageChops.difference(first.convert("RGB"),last.convert("RGB")).getbbox(), "Still frame playback"
            motion[variant+"_"+track]=shifts
    assert pairs == 57 and changed > 0
    xml_path=folder/"PlayMode.xml"
    assert xml_path.is_file(), "Missing authentic Unity NUnit report"
    xml=ET.parse(xml_path).getroot()
    assert xml.get("result")=="Passed" and xml.get("passed")=="1" and xml.get("failed")=="0"
    case=list(xml.iter("test-case"))
    assert len(case)==1 and case[0].get("result")=="Passed"
    result={
      "evidence_type":"Recovered actual Unity Editor RenderTexture and PlayMode log",
      "source_run":RUN_ID, "source_artifact":ORIGINAL_ARTIFACT,
      "source_branch":"art/valoria-25d-profundidad-piloto",
      "native_source_sha":SOURCE_BRANCH_SHA,
      "baseline_sha256":SOURCE_SHA256,
      "sha256_verified_png":len(entries),
      "A_B_identical_camera_pairs":pairs,
      "native_comparisons_that_differ":changed,
      "NUnit_PlayMode_EnterPlayMode":"Passed: 1/1",
      "real_camera_motion":motion,
      "visual_quality":"NOT_APPROVED",
      "iphone":"NOT_TESTED","WebGL":"NOT_TESTED",
      "not_our_other_spatial_pilot":True
    }
    return result


def run_ffmpeg(command: list[str]):
    subprocess.run(command,check=True,timeout=180)


def make_movies(folder: Path,out:Path) -> list[Path]:
    out.mkdir(parents=True,exist_ok=True)
    outputs = []
    for route in TRACKS:
        sources={}
        for variant in ("A","B"):
            p=out/(route+"_"+variant+"_Unity.mp4")
            run_ffmpeg([
                "ffmpeg","-hide_banner","-loglevel","error","-y",
                "-framerate","12","-i",str(folder/(variant+"_"+route+"_%02d_390x844.png")),
                "-frames:v","18","-c:v","libx264","-crf","20","-pix_fmt","yuv420p",
                "-movflags","+faststart",str(p)])
            sources[variant]=p
        comparison=out/(route+"_AB_Unity_real.mp4")
        run_ffmpeg([
            "ffmpeg","-hide_banner","-loglevel","error","-y",
            "-i",str(sources["A"]),"-i",str(sources["B"]),
            "-filter_complex","[0:v][1:v]hstack=inputs=2[v]",
            "-map","[v]","-c:v","libx264","-crf","19",
            "-pix_fmt","yuv420p","-movflags","+faststart",str(comparison)])
        outputs.append(comparison)
    concat = out/"paths.txt"
    concat.write_text("".join("file '"+str(p.resolve())+"'\n" for p in outputs))
    movie=out/"Valoria_Unity_Real_AB_Pan_Zoom.mp4"
    run_ffmpeg(["ffmpeg","-hide_banner","-loglevel","error","-y",
               "-f","concat","-safe","0","-i",str(concat),"-c","copy",
               "-movflags","+faststart",str(movie)])
    assert movie.stat().st_size > 100000
    return outputs+[movie]


def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--source",required=True,type=Path)
    ap.add_argument("--out",required=True,type=Path)
    args=ap.parse_args()
    data=validate(args.source)
    outputs=make_movies(args.source,args.out)
    data["movies"]=[{"file":p.name,"sha256":sha(p),"bytes":p.stat().st_size} for p in outputs]
    (args.out/"recovered_unity_verified.json").write_text(json.dumps(data,indent=2,ensure_ascii=False)+"\n")
    print(json.dumps(data,indent=2,ensure_ascii=False))


if __name__=="__main__":
    main()
