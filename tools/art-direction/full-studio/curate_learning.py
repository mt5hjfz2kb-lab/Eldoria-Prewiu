"""Promote independently reviewed A/B evidence into a *candidate* artist casebook.

This utility does not train Qwen, self-approve visual quality, edit game assets,
or push changes. The reviewed candidate requires explicit integration.
"""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
from learning_context import BLOCK

ART_RESULTS = {"VISUAL_REJECTED", "ART_SOURCE_PASS"}
IMAGE_SIG = b"\x89PNG\r\n\x1a\n"


def sha_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def read_json(path: Path) -> dict:
    data = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(data, dict):
        raise ValueError("JSON object expected")
    return data


def curate_case(pending_path: Path, review_path: Path, casebook_path: Path) -> dict:
    pending_path, review_path, casebook_path = map(Path,(pending_path, review_path, casebook_path))
    pending_bytes = pending_path.read_bytes()
    pending = json.loads(pending_bytes)
    review = read_json(review_path)
    casebook = read_json(casebook_path)
    if pending.get("schema_version") != 1 or pending.get("status") != "PENDING_INDEPENDENT_VERIFICATION":
        raise ValueError("Only pending genuine studio A/B evidence can be reviewed")
    if pending.get("art_approved") or pending.get("independent_visual_review"):
        raise ValueError("Untrusted pending record already claims approval")
    if review.get("pending_sha256") != sha_bytes(pending_bytes):
        raise ValueError("Review does not refer to exact pending proof")
    if review.get("result") not in ART_RESULTS:
        raise ValueError("Only reviewed source-pass or visual rejection can be learned")
    if review.get("reviewer_role") != "INDEPENDENT_ART_REVIEW":
        raise ValueError("The model cannot approve its own proposal")
    lesson = review.get("safe_lesson", "")
    if not isinstance(lesson, str) or not 45 <= len(lesson) <= 380 or BLOCK.search(lesson):
        raise ValueError("Specific transferable visual lesson required")
    if not isinstance(review.get("diagnosis"), str) or len(review["diagnosis"]) < 45:
        raise ValueError("Independent artistic diagnosis required")
    if type(review.get("run_id")) is not int or review["run_id"] <= 0:
        raise ValueError("Real run ID required")
    if type(review.get("artifact_id")) is not int or review["artifact_id"] <= 0:
        raise ValueError("Real artifact provenance required")
    if casebook.get("schema_version") != 1:
        raise ValueError("Invalid casebook schema")
    evidence = pending.get("evidence",{})
    mapped={"plan":"artist-plan.json",
            "before_image":"output/before-blender.png",
            "after_image":"output/after-blender.png",
            "model_self_critique":"qwen-critique.json",
            "screen_comparison":"camera-screen-impact.json",
            "blend":"output/artist-asset.blend",
            "glb":"output/artist-asset.glb"}
    trial_root = pending_path.parent
    for kind, rel in mapped.items():
        path = trial_root / rel
        recorded = evidence.get(kind, {})
        if not path.is_file() or sha_bytes(path.read_bytes()) != recorded.get("sha256"):
            raise ValueError("Source evidence missing or changed: "+kind)
    for key in ("before_image","after_image"):
        if not (trial_root / mapped[key]).read_bytes().startswith(IMAGE_SIG):
            raise ValueError("Original A/B PNG evidence required")
    if not (trial_root / mapped["blend"]).read_bytes().startswith(b"BLENDER"):
        raise ValueError("BLEND signature absent")
    if not (trial_root / mapped["glb"]).read_bytes().startswith(b"glTF"):
        raise ValueError("GLB signature absent")
    if (review.get("before_sha256") != evidence["before_image"]["sha256"] or
            review.get("after_sha256") != evidence["after_image"]["sha256"]):
        raise ValueError("Reviewer images differ from originating A/B")
    case_id = "independent-"+str(review["run_id"])+"-"+str(review["artifact_id"])
    if any(c.get("id")==case_id for c in casebook.get("cases",[])):
        raise ValueError("No duplicate review case IDs")
    record = {
        "id":case_id,
        "run_id":review["run_id"],"evidence_artifact_id":review["artifact_id"],
        "raw_response_sha256":evidence["plan"]["sha256"],
        "result":review["result"],
        "observed":review["diagnosis"],
        "safe_lesson":lesson,
        "review_provenance":{
            "pending_sha256":sha_bytes(pending_bytes),
            "review_sha256":sha_bytes(review_path.read_bytes()),
            "source_sha256":pending["original_source_sha256"],
            "before_sha256":evidence["before_image"]["sha256"],
            "after_sha256":evidence["after_image"]["sha256"],
            "independent_reviewer_declared":True,
            "unity_verified":False,
            "no_model_weight_training":True}
    }
    return {**casebook,"cases":[*casebook.get("cases",[]),record]}


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("--pending",type=Path,required=True)
    p.add_argument("--review",type=Path,required=True)
    p.add_argument("--casebook",type=Path,required=True)
    p.add_argument("--out",type=Path,required=True)
    args=p.parse_args()
    if args.out.resolve()==args.casebook.resolve():
        raise ValueError("Never overwrite canonical casebook in a test or on the runner")
    if args.out.exists():
        raise FileExistsError("Refuse to overwrite existing evidence")
    candidate=curate_case(args.pending,args.review,args.casebook)
    args.out.write_text(json.dumps(candidate,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_REVIEWED_LEARNING_CANDIDATE",len(candidate["cases"]),
          "PENDING_REPOSITORY_REVIEW_NOT_WEIGHT_TRAINING")


if __name__=="__main__":
    main()
