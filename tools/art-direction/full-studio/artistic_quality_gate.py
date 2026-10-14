"""Eldoria permanent visual quality contract — independent review handoff.

This is NOT an automated aesthetic judge. Produces SHA-linked review requests;
only an independent, evidence-bound review may promote a Blender candidate.
Final ART_APPROVED/CALIBRATION_VALIDATED still require main's JS verifier
with real Unity A/B, independent judgment and three accepted source links.
"""
from __future__ import annotations
import hashlib
import json
import math
from pathlib import Path

WEIGHTS={"geometry":.25,"architecture":.25,"materials":.20,
         "style":.20,"camera":.10}
HEX=set("0123456789abcdef")

def digest(path):
    p=Path(path)
    return hashlib.sha256(p.read_bytes()).hexdigest()

def sha_ok(value):
    return isinstance(value,str) and len(value)==64 and set(value.lower())<=HEX

def weighted(rubric):
    if not isinstance(rubric,dict):
        raise ValueError("Five independent rubric entries required")
    total=0
    for name,weight in WEIGHTS.items():
        v=rubric.get(name)
        if type(v) not in (int,float) or not math.isfinite(v) or not 0<=v<=10:
            raise ValueError("Missing independent score 0..10 for "+name)
        total+=v*weight
    return total

def stage_visual_request(*,output,reference,original_source_sha,input_source,
                         raw_plan,execution_proof,before,after,model,
                         raw_response_file=None,
                         memory_proof=None,review_camera_file=None,run_id=None):
    """Consume actual source+model+Blender evidence; write no art-approval flag."""
    from PIL import Image
    src=Path(input_source)
    proof=json.loads(Path(execution_proof).read_text(encoding="utf-8"))
    if not proof.get("blender_execution") or not proof.get("matched_review_camera"):
        raise ValueError("Only real matched-camera Blender execution is reviewable")
    if not sha_ok(original_source_sha):
        raise ValueError("Original frozen source hash required")
    if proof.get("source_sha256")!=digest(src) or proof.get("artist_plan_sha256")!=digest(raw_plan):
        raise ValueError("Plan/source bytes differ from actual Blender execution")
    if proof.get("before_blender_sha256")!=digest(before) or proof.get("after_blender_sha256")!=digest(after):
        raise ValueError("Blender A/B hashes differ from executed source")
    if digest(before)==digest(after):
        raise ValueError("No real BEFORE/AFTER image difference")
    with Image.open(before) as b,Image.open(after) as a:
        if b.format!="PNG" or a.format!="PNG" or b.size!=a.size:
            raise ValueError("Real equal-resolution PNG A/B required")
        resolution=list(b.size)
    extra=digest(memory_proof) if memory_proof else None
    req={"schema_version":1,"kind":"ELDORIA_INDEPENDENT_ARTISTIC_REVIEW_REQUEST",
         "status":"ART_REVIEW_REQUIRED","art_approved":False,
         "calibration_validated":False,"model_weight_training":False,
         "model":model,"run_id":run_id,"original_asset_sha256":original_source_sha,
         "input_asset_sha256":digest(src),
         "output_asset_sha256":proof.get("blend_sha256"),
         "output_glb_sha256":proof.get("glb_sha256"),
         "qwen_plan_sha256":digest(raw_response_file) if raw_response_file else digest(raw_plan),
         "execution_plan_sha256":digest(raw_plan),
         "reference_sha256":digest(reference),
         "memory_context_sha256":extra,
         "camera_matched_in_blender":True,
         "review_camera_spec_sha256":digest(review_camera_file) if review_camera_file else None,
         "render_profile":"actual matched native Blender fixed-camera execution",
         "resolution":resolution,
         "before_png_sha256":digest(before),"after_png_sha256":digest(after),
         "evaluation_fields":list(WEIGHTS),
         "requirements":"Independent evaluator must see genuine PNG A/B/reference, source/protected regions and explain >=0.25 improvement and no severe regressions. No automatic acceptance."}
    Path(output).write_text(json.dumps(req,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
    return req

def validate_independent_review(request,review,*,prior=None):
    """Provisional Blender-only evidence gate, never final Unity/ART_APPROVED."""
    if isinstance(request,(str,Path)):
        request=json.loads(Path(request).read_text(encoding="utf-8"))
    if isinstance(review,(str,Path)):
        review=json.loads(Path(review).read_text(encoding="utf-8"))
    if request.get("status")!="ART_REVIEW_REQUIRED":
        raise ValueError("Only real pending A/B requests can be reviewed")
    if review.get("review_kind")!="INDEPENDENT_BLENDER_VISUAL_REVIEW" or review.get("independent") is not True:
        raise ValueError("Independent Blender visual review required")
    if (not isinstance(review.get("reviewer_id"),str) or
            len(review["reviewer_id"])<4 or
            review["reviewer_id"]==request["model"]):
        raise ValueError("Qwen cannot independently judge its own art")
    for name in ("original_asset_sha256","input_asset_sha256",
                 "output_asset_sha256","qwen_plan_sha256","execution_plan_sha256",
                 "reference_sha256","before_png_sha256","after_png_sha256",
                 "review_camera_spec_sha256"):
        if review.get(name)!=request.get(name):
            raise ValueError("Review not bound to exact original A/B "+name)
    for name in ("diagnosis","observed_improvement","protected_parts_impact"):
        if not isinstance(review.get(name),str) or len(review[name].strip())<24:
            raise ValueError("Unsubstantiated artistic evaluation: "+name)
    if review.get("severe_regressions")!=[] or review.get("protected_regions_ok") is not True:
        raise ValueError("Protected features affected or severe regressions observed")
    before=review.get("before");after=review.get("after")
    b=weighted(before);a=weighted(after)
    if prior:
        if prior.get("output_asset_sha256")!=request["input_asset_sha256"]:
            raise ValueError("Same-tower accepted-source SHA chain broken")
        if prior.get("original_asset_sha256")!=request["original_asset_sha256"]:
            raise ValueError("Frozen original mismatch")
        if abs(prior.get("after_score",-100)-b)>.011:
            raise ValueError("Independent weighted score not continuous")
    if any(after[x]<before[x]-.500001 for x in WEIGHTS):
        raise ValueError("Individual dimension regressed >0.5")
    if a-b<.25-1e-8 or not review.get("accepted") is True:
        raise ValueError("Real independent >=0.25 visual improvement not established")
    return {"status":"BLENDER_VISUAL_ACCEPTED",
            "original_asset_sha256":request["original_asset_sha256"],
            "input_asset_sha256":request["input_asset_sha256"],
            "output_asset_sha256":request["output_asset_sha256"],
            "qwen_plan_sha256":request["qwen_plan_sha256"],
            "before_png_sha256":request["before_png_sha256"],
            "after_png_sha256":request["after_png_sha256"],
            "independent_reviewer":review["reviewer_id"],
            "before_score":round(b,5),"after_score":round(a,5),
            "improvement":round(a-b,5),
            "art_approved":False,"unity_verified":False,
            "calibration_validated":False}

def append_accepted(chain,record,*,frozen_original):
    """Promote only a verified provisional record, for NEXT source input."""
    if record.get("status")!="BLENDER_VISUAL_ACCEPTED" or record.get("art_approved"):
        raise ValueError("Cannot promote non-independent or final-approved claims")
    if record["original_asset_sha256"]!=frozen_original:
        raise ValueError("Wrong frozen source")
    prior=chain[-1] if chain else None
    expected=prior["output_asset_sha256"] if prior else frozen_original
    if record["input_asset_sha256"]!=expected:
        raise ValueError("Rejected or unrelated source cannot be next cycle")
    if prior and abs(prior["after_score"]-record["before_score"])>.011:
        raise ValueError("Visual evaluation score discontinuity")
    if record["output_asset_sha256"]==record["input_asset_sha256"]:
        raise ValueError("Unchanged asset cannot advance art")
    return [*chain,record]

def status(chain):
    return {"status":"CALIBRATION_IN_PROGRESS","blender_visual_accepted_count":len(chain),
            "consecutive_needed":3,"art_approved":False,
            "calibration_validated":False,"unity_verified":False,
            "last_visual_score":chain[-1]["after_score"] if chain else None,
            "remaining":max(0,3-len(chain)),
            "next_gate":"Continue from LAST INDEPENDENTLY ACCEPTED asset; obtain 3 linked accepted Blender improvements, final >=8/10 and true Unity A/B under main verifier"}
