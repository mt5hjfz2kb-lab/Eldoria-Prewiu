#!/usr/bin/env python3
"""Validate REAL Unity RenderTexture A/B frames; optionally assemble actual MP4s.

Never generates or substitutes frames. A missing/invalid Unity source fails closed.
"""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
from PIL import Image, ImageChops

EXPECTED = 2 * (10 + 42)
TIMELINE = 42
def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def load(folder: Path):
    manifest_path = folder / "evidence.json"
    if not manifest_path.is_file():
        raise RuntimeError("No genuine Unity evidence.json - refuse video synthesis")
    report = json.loads(manifest_path.read_text(encoding="utf-8"))
    assert report["engine"].startswith("Unity"), "Not a Unity Editor source"
    assert report["test_type"].startswith("Editor capture"), "Evidence provenance mismatch"
    assert report["author_approval"] is False
    assert report["physically_tested"] is False
    frames=report["frames"]
    assert len(frames)==EXPECTED,(len(frames),EXPECTED)
    keys={}
    for entry in frames:
        path=folder/entry["file"]
        assert entry["variant"] in ("A","B")
        assert path.is_file(),f"Missing genuine RenderTexture frame {path}"
        assert sha(path.read_bytes())==entry["sha256"], f"SHA mismatch {path}"
        with Image.open(path) as img:
            assert img.size==(entry["width"],entry["height"])
        assert entry["editor_render_texture"] is True
        assert entry["scene_asset_path"].endswith(".unity")
        assert entry["actual_orthographic_size"]>0
        key=(entry["camera_step"],entry["frame_number"],entry["stage"],
             entry["width"],entry["height"])
        assert entry["variant"] not in keys.setdefault(key,{}),("duplicate",key)
        keys[key][entry["variant"]]=entry
    assert len(keys)==EXPECTED//2
    for key,pair in keys.items():
        assert set(pair)=={"A","B"},("Unpaired A/B",key)
        a,b=pair["A"],pair["B"]
        for k in ("zoom","focus_x","focus_y","simulated_seconds",
                  "actual_camera_x","actual_camera_y","actual_orthographic_size"):
            assert abs(a[k]-b[k])<1e-4,("Camera mismatch",key,k,a[k],b[k])
    report_summary={"real_unity_frames":len(frames),"a_b_exact_camera_pairs":len(keys),
                    "verified_sha256":True,"motion_pass":{},"subjective_art":"UNREVIEWED",
                    "native_playmode":"NOT_CERTIFIED_BY_THIS_SCRIPT",
                    "iphone":"NOT_TESTED"}
    for variant in ("A","B"):
        rows={r["frame_number"]:r for r in frames if
              r["variant"]==variant and r["frame_number"]>=0}
        assert set(rows)==set(range(TIMELINE)),("Missing timeline frames",variant)
        horizontal=[r for r in rows.values() if r["camera_step"]=="horizontal"]
        vertical=[r for r in rows.values() if r["camera_step"]=="vertical"]
        gate=[r for r in rows.values() if r["camera_step"]=="gate_portal_occlusion"]
        assert len(horizontal)==len(vertical)==8 and len(gate)==6
        dx=max(r["actual_camera_x"] for r in horizontal)-min(r["actual_camera_x"] for r in horizontal)
        dy=max(r["actual_camera_y"] for r in vertical)-min(r["actual_camera_y"] for r in vertical)
        assert dx>.05 and dy>.05,("Camera stayed still",variant,dx,dy)
        report_summary["motion_pass"][variant]={"actual_pan_dx":dx,"actual_pan_dy":dy}
        with Image.open(folder/rows[0]["file"]) as first,Image.open(folder/rows[7]["file"]) as last:
            assert ImageChops.difference(first.convert("RGB"),last.convert("RGB")).getbbox(), "Horizontal still duplicate"
        with Image.open(folder/rows[8]["file"]) as first,Image.open(folder/rows[15]["file"]) as last:
            assert ImageChops.difference(first.convert("RGB"),last.convert("RGB")).getbbox(), "Vertical still duplicate"
        # Gate camera is deliberately FIXED. B's characters should animate
        # against it; no photo pan or interpolated slide is an acceptable proof.
        if variant=="B":
            with Image.open(folder/rows[36]["file"]) as before,Image.open(folder/rows[41]["file"]) as after:
                assert ImageChops.difference(before.convert("RGB"),after.convert("RGB")).getbbox(), "B gate worker did not move in actual render"
    # B has to visibly differ from A in at least one genuine frame.
    changed=0
    for t in (0,12,18,28,37,41):
        a,b=folder/f"timeline_A_{t:03}.png", folder/f"timeline_B_{t:03}.png"
        with Image.open(a) as pa,Image.open(b) as pb:
            if ImageChops.difference(pa.convert("RGB"),pb.convert("RGB")).getbbox():
                changed+=1
    assert changed>0,"B is a no-op (indistinguishable from A)"
    report_summary["ab_different_at_checked_steps"]=changed
    (folder/"validated-native.json").write_text(json.dumps(report_summary,indent=2)+"\n",encoding="utf-8")
    return report_summary

def movies(folder:Path):
    exe=shutil.which("ffmpeg")
    if not exe: raise RuntimeError("ffmpeg not installed. Real frames validated; MP4 creation pending")
    for variant in ("A","B"):
        command=[exe,"-hide_banner","-loglevel","error","-y",
                 "-framerate","6","-i",str(folder/f"timeline_{variant}_%03d.png"),
                 "-frames:v",str(TIMELINE),"-c:v","libx264","-crf","22",
                 "-pix_fmt","yuv420p",str(folder/f"actual_Unity_{variant}.mp4")]
        subprocess.run(command,check=True,timeout=180)
    command=[exe,"-hide_banner","-loglevel","error","-y",
             "-i",str(folder/"actual_Unity_A.mp4"),
             "-i",str(folder/"actual_Unity_B.mp4"),
             "-filter_complex","[0:v][1:v]hstack=inputs=2[v]",
             "-map","[v]","-c:v","libx264","-crf","22","-pix_fmt","yuv420p",
             str(folder/"actual_Unity_AB_side_by_side.mp4")]
    subprocess.run(command,check=True,timeout=180)
    return [str(folder/f"actual_Unity_{v}.mp4") for v in ("A","B")]+[
        str(folder/"actual_Unity_AB_side_by_side.mp4")]

if __name__=="__main__":
    parser=argparse.ArgumentParser()
    parser.add_argument("--frames",required=True,type=Path)
    parser.add_argument("--mp4",action="store_true")
    args=parser.parse_args()
    verified=load(args.frames)
    if args.mp4:verified["real_movie_outputs"]=movies(args.frames)
    print(json.dumps(verified,indent=2))
