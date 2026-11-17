#!/usr/bin/env python3
"""Archive source-checked original Unity screenshots for V3/V4 comparison.
The two native ZIPs are fetched by GitHub Actions, never generated or altered here.
"""
from pathlib import Path
import hashlib
import json
import zipfile
from io import BytesIO
from PIL import Image, ImageDraw, ImageChops, ImageStat

REPO=Path(__file__).resolve().parents[2]
CITY=REPO/"Unity/Assets/Experimentos/Ciudad2D"
QA=CITY/"QA"
REQ=Path(__file__).parent/"functional_v4_archive_request.json"
FRAMES=(
    "premium_bastion1_390x844.png",
    "premium_bastion3_390x844.png",
    "premium_bastion5_390x844.png",
    "premium_bastion5_1170x2532.png",
    "premium_bastion5_844x390.png",
    "premium_bastion5_1170x2532_cuartel_close.png",
    "premium_bastion5_quarry_built_390x844.png",
)
def digest(raw):return hashlib.sha256(raw).hexdigest()
def load(prefix,version):
    archive=Path("/tmp/ciudad2d-functional-"+version+".zip")
    assert archive.is_file() and archive.stat().st_size>5_000_000
    with zipfile.ZipFile(archive) as z:
        found=[n for n in z.namelist() if n.endswith(prefix+"/evidence.json")]
        assert len(found)==1, ("Expected one native evidence.json",found)
        root=found[0].rsplit("/",1)[0]+"/"
        m=json.loads(z.read(found[0]).decode("utf-8-sig"))
        assert m["engine"]=="Unity" and m["mode"]=="Editor_RenderTexture"
        assert len(m["captures"])==11 and len(m["four_premium_candidates"])==4
        assert m["gameplay_modified"] is False and m["main_modified"] is False
        assert m["art_approved"] is False and m["iPhone_physical_test"]=="PENDING"
        assert m["quarry_unbuilt_at_default_stage5"] and m["quarry_built_qa_fixture_only"]
        if version=="v4":
            assert m["source_scene"]=="Experimento_Ciudad2D_PremiumV4.unity"
            assert m["functional_training_targets_qa_only"] and m["functional_target_noninteractive"]
        all_frames={}
        for c in m["captures"]:
            raw=z.read(root+c["file"])
            assert digest(raw)==c["sha256"], ("Original Unity PNG SHA mismatch",c["file"])
            im=Image.open(BytesIO(raw))
            assert im.format=="PNG" and im.size==(c["width"],c["height"])
            all_frames[c["file"]]=raw
        return m,all_frames,digest(archive.read_bytes())
def main():
    req=json.loads(REQ.read_text(encoding="utf-8"))
    assert req["schema"]==1 and req["v3_run"]==44116723739
    assert req["v4_run"]>0 and req["v3_artifact"]==14344216728
    old,orig,sha_old=load("Ciudad2DPremiumPlayableV3Evidence","v3")
    new,changed,sha_new=load("Ciudad2DPremiumPlayableV4Evidence","v4")
    assert set(orig)==set(changed)
    native_dir=QA/"NativeV4Functional"
    native_dir.mkdir(exist_ok=False)
    keep=[]
    for name in FRAMES:
        raw=changed[name]
        dest=native_dir/name
        dest.write_bytes(raw)
        keep.append({"file":str(dest.relative_to(REPO)),"sha256":digest(raw)})
    diffs={}
    images={}
    for stage in [1,3,5]:
        name=f"premium_bastion{stage}_390x844.png"
        a=Image.open(BytesIO(orig[name])).convert("RGB")
        c=Image.open(BytesIO(changed[name])).convert("RGB")
        delta=ImageChops.difference(a,c)
        focus=delta.crop((20,170,170,302))
        other=delta.copy()
        other.paste((0,0,0),(20,170,170,302))
        diffs[str(stage)]={
            "whole_mean_rgb":round(sum(ImageStat.Stat(delta).mean)/3,4),
            "cuartel_roi_mean_rgb":round(sum(ImageStat.Stat(focus).mean)/3,4),
            "other_areas_mean_rgb":round(sum(ImageStat.Stat(other).mean)/3,4),
            "difference_bbox":delta.getbbox(),
        }
        images[stage]=(a,c)
    # Stage I: Cuartel is locked. No art effect may be visible.
    assert diffs["1"]["cuartel_roi_mean_rgb"] < .1
    # Stage III/V: Cuartel is finished; the new training range must be real.
    assert diffs["3"]["cuartel_roi_mean_rgb"] > .10
    assert diffs["5"]["cuartel_roi_mean_rgb"] > .10
    derivatives=[]
    for stage, pair in images.items():
        out=Image.new("RGB",(390*2,884),(21,30,41))
        out.paste(pair[0],(0,40));out.paste(pair[1],(390,40))
        draw=ImageDraw.Draw(out)
        draw.text((12,14),f"BASTION {stage} V3 - UNITY REAL",fill=(240,211,173))
        draw.text((402,14),f"BASTION {stage} V4 TARGETS - UNITY REAL",fill=(240,211,173))
        file=QA/f"ArtQualityV4Functional_Bastion{stage}_V3_V4_AB.jpg"
        assert not file.exists(),"Refuse to overwrite existing visual evidence"
        out.save(file,quality=95)
        derivatives.append({"file":str(file.relative_to(REPO)),"sha256":digest(file.read_bytes()),
                            "not_native_capture":True})
    proof={
        "schema":1,"v3_source_run":req["v3_run"],"v3_artifact":req["v3_artifact"],
        "v4_source_run":req["v4_run"],"v4_artifact":req["v4_artifact"],
        "source_commit":req["v4_sha"],
        "engine":"Unity","mode":"Editor_RenderTexture",
        "native_frame_count_v3":11,"native_frame_count_v4":11,
        "full_v3_capture_sha256":old["captures"],
        "full_v4_capture_sha256":new["captures"],
        "v3_artifact_zip_sha256":sha_old,"v4_artifact_zip_sha256":sha_new,
        "original_v4_frames":keep,"derived_pairwise_reviews":derivatives,
        "difference_metrics_are_diagnostics_not_art_scores":diffs,
        "canon_quarry_stage5":"Available, not Built",
        "post_built_quarry_fixture_only":True,
        "originals_preserved":True,"no_main_or_gameplay_changes":True,
        "art_approved":False,"physical_iPhone":"NOT_TESTED",
    }
    file=QA/"ArtQualityV4Functional_NATIVE_EVIDENCE.json"
    assert not file.exists()
    file.write_text(json.dumps(proof,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("CIUDAD2D_V4_22_SOURCE_SHA_NATIVE_UNITY_A_B_ARCHIVED_NOT_ART_APPROVED",diffs)
if __name__=="__main__":main()
