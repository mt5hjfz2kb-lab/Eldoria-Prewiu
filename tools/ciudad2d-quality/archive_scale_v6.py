#!/usr/bin/env python3
"""Archive source-checked original Unity screenshots for V3/V6 comparison.
The two native ZIPs are fetched by GitHub Actions, never generated or altered here.
"""
from pathlib import Path
import hashlib
import json
import zipfile
from io import BytesIO
from xml.etree import ElementTree as ET
from PIL import Image, ImageDraw, ImageChops, ImageStat

REPO=Path(__file__).resolve().parents[2]
CITY=REPO/"Unity/Assets/Experimentos/Ciudad2D"
QA=CITY/"QA"
REQ=Path(__file__).parent/"functional_v6_archive_request.json"
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
        if version=="v6":
            assert m["source_scene"]=="Experimento_Ciudad2D_PremiumV6.unity"
            assert abs(float(m["cuartel_visual_scale_factor"])-1.14)<.0001
            assert not m.get("functional_training_targets_qa_only",False)
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
    assert req["v6_run"]>0 and req["v3_artifact"]==14344216728
    old,orig,sha_old=load("Ciudad2DPremiumPlayableV3Evidence","v3")
    new,changed,sha_new=load("Ciudad2DPremiumPlayableV6Evidence","v6")
    assert set(orig)==set(changed)
    native_dir=QA/"NativeV6Scale"
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
        draw.text((402,14),f"BASTION {stage} V6 TARGETS - UNITY REAL",fill=(240,211,173))
        file=QA/f"ArtQualityV6Scale_Bastion{stage}_V3_V6_AB.jpg"
        assert not file.exists(),"Refuse to overwrite existing visual evidence"
        out.save(file,quality=95)
        derivatives.append({"file":str(file.relative_to(REPO)),"sha256":digest(file.read_bytes()),
                            "not_native_capture":True})
    # Preserve the actual 234K-byte Unity scene as an independent historical
    # archive. The active QA scene path remains free so scene generators V2/V3/V6
    # can be rerun without overwriting previous candidates.
    v6zip=Path("/tmp/ciudad2d-functional-v6.zip")
    with zipfile.ZipFile(v6zip) as archive:
        scenes=[n for n in archive.namelist()
                if n.endswith("/QA/Experimento_Ciudad2D_PremiumV6.unity")]
        assert len(scenes)==1,"Real Unity V6 scene was not archived"
        scene=archive.read(scenes[0])
        assert scene.startswith(b"%YAML 1.1")
        assert b"QA_V4_CuartelArcheryTargets" not in scene
        assert b"QA_V5_CuartelArcheryTargets" not in scene
        assert b"QA_V6_CuartelArcheryTargets" not in scene
        archived=QA/"ArchivedScenes"
        archived.mkdir(exist_ok=True)
        target=archived/"Experimento_Ciudad2D_PremiumV6_44133747143.unity"
        assert not target.exists()
        target.write_bytes(scene)
        uid="ec837f59b040445da407785c6f670ecd"
        target.with_suffix(".unity.meta").write_text(
            "fileFormatVersion: 2\\nguid: "+uid+
            "\\nDefaultImporter:\\n  externalObjects: {}\\n  userData:\\n"
            "  assetBundleName:\\n  assetBundleVariant:\\n",encoding="utf-8")
    archived_scene={"file":str(target.relative_to(REPO)),
                    "sha256":digest(scene),"Unity_saved_scene":True,
                    "generated_from_native_run":req["v6_run"]}
    # Preserve the raw NUnit XML, not merely a CI green label.
    with zipfile.ZipFile(v6zip) as archive:
        tests=[n for n in archive.namelist()
               if n.endswith(".xml") and "premium-v6-playmode" in n.lower()]
        assert len(tests)==1,("Expected original Unity test XML",tests)
        xmlraw=archive.read(tests[0])
        testroot=ET.fromstring(xmlraw.decode("utf-8-sig"))
        assert testroot.attrib.get("total")=="1" and testroot.attrib.get("passed")=="1"
        assert testroot.attrib.get("failed")=="0"
        cases=testroot.findall(".//test-case")
        assert len(cases)>=1
        assert any("PremiumV6Scene_GenuinePlayMode_StagesSpritesShadowsPointer"
                   in c.attrib.get("fullname","")+c.attrib.get("name","")
                   and c.attrib.get("result")=="Passed" for c in cases)
        junit=native_dir/"PremiumV6_NUnit_PlayMode.xml"
        junit.write_bytes(xmlraw)
    archived_xml={"file":str(junit.relative_to(REPO)),"sha256":digest(xmlraw),
                  "PlayMode":"1 passed 0 failed","tested_on":"Unity Editor, not iPhone"}

    proof={
        "schema":1,"v3_source_run":req["v3_run"],"v3_artifact":req["v3_artifact"],
        "v6_source_run":req["v6_run"],"v6_artifact":req["v6_artifact"],
        "source_commit":req["v6_sha"],
        "engine":"Unity","mode":"Editor_RenderTexture",
        "native_frame_count_v3":11,"native_frame_count_v6":11,
        "full_v3_capture_sha256":old["captures"],
        "full_v6_capture_sha256":new["captures"],
        "v3_artifact_zip_sha256":sha_old,"v6_artifact_zip_sha256":sha_new,
        "original_v6_frames":keep,"derived_pairwise_reviews":derivatives,
        "archived_separate_Unity_scene":archived_scene,
        "verified_Unity_PlayMode_XML":archived_xml,
        "difference_metrics_are_diagnostics_not_art_scores":diffs,
        "canon_quarry_stage5":"Available, not Built",
        "post_built_quarry_fixture_only":True,
        "originals_preserved":True,"no_main_or_gameplay_changes":True,
        "art_approved":False,"physical_iPhone":"NOT_TESTED",
    }
    file=QA/"ArtQualityV6Scale_NATIVE_EVIDENCE.json"
    assert not file.exists()
    file.write_text(json.dumps(proof,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("CIUDAD2D_V6_22_SOURCE_SHA_NATIVE_UNITY_A_B_ARCHIVED_NOT_ART_APPROVED",diffs)
if __name__=="__main__":main()
