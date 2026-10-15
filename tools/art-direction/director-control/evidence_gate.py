"""Eldoria artist brain: fail-closed, evidence-based director gate.

This is an independent safety/verifiability layer, NOT artistic judgment.
In diagnostic mode it cannot authorize ANY mesh edit or ART_APPROVED state.
"""
import argparse, hashlib, json, pathlib, sys

EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
ALLOWED_GEOMETRY={"bounding_box.world_dimensions_m","geometry.upper_inclined_surfaces_exist",
    "geometry.zones_world_z.upper.inclined_triangles","geometry.mesh_object_count",
    "connected_island_count","mesh_provenance","all_cameras_same_exact_source_sha256"}
PROTECTED={"door","entrance","escalera","stair","rail","barandilla","uv","atlas"}
class RejectedArtDirection(ValueError):pass
def must(condition,reason):
    if not condition:raise RejectedArtDirection(reason)
def sha(b):return hashlib.sha256(b).hexdigest()

def validate(proposal,grounding,original_observation):
    must(proposal.get("schema_version")==1,"SCHEMA_VERSION")
    must(proposal.get("original_sha256")==EXPECTED,"SOURCE_SHA_MISMATCH")
    must(grounding.get("source_sha256")==EXPECTED,"SPATIAL_SHA_MISMATCH")
    must(original_observation.get("source_sha256")==EXPECTED,"CAMERA_SHA_MISMATCH")
    must(grounding.get("immutable_source_verified") is True,"IMMUTABLE_SOURCE_NOT_PROVEN")
    must(grounding.get("all_cameras_same_exact_source_sha256") is True,"PERSPECTIVE_PROVENANCE_MISSING")
    must(proposal.get("mode")=="DIAGNOSTIC_ONLY","NO_PRODUCTION_AUTHORITY")
    must(proposal.get("art_approved") is False,"ART_APPROVAL_FORBIDDEN")
    must(proposal.get("editor_actions")==[],"UNAUTHORIZED_BLENDER_ACTIONS")
    must(proposal.get("proposals") is not None and isinstance(proposal["proposals"],list)
         and len(proposal["proposals"])<=3,"EXCESS_PROPOSALS")
    cameras={f["view"]:f["sha256"] for f in original_observation["frames"]}
    must(len(cameras)==3,"THREE_ACTUAL_VIEWS_REQUIRED")
    proven=0
    for item in proposal["proposals"]:
        must(isinstance(item,dict),"INVALID_PROPOSAL")
        must(item.get("decision") in ("INSPECT_ONLY","NO_EDIT_INSUFFICIENT_EVIDENCE"),
             "ARTISTIC_ACTION_NOT_AUTHORIZED")
        must(item.get("requires_manual_visual_review") is True,"INDEPENDENT_REVIEW_REQUIRED")
        must(item.get("contact_verified") is False,"CONTACT_NOT_VERIFIED_BY_THIS_GATE")
        must(item.get("protected_changes")==[],"PROTECTED_ASSET_CHANGES")
        must(isinstance(item.get("evidence"),list) and item["evidence"],
             "CAMERA_EVIDENCE_REQUIRED")
        for source in item["evidence"]:
            must(isinstance(source,dict) and source.get("camera_id") in cameras,
                 "UNKNOWN_CAMERA")
            must(source.get("png_sha256")==cameras.get(source["camera_id"]),
                 "CAMERA_IMAGE_PROVENANCE_MISMATCH")
        refs=item.get("geometry_refs",[])
        must(isinstance(refs,list) and all(x in ALLOWED_GEOMETRY for x in refs),
             "GEOMETRY_REF_NOT_IN_GROUNDED_CATALOG")
        must(isinstance(item.get("uncertainty"),str) and len(item["uncertainty"])>=20,
             "UNCERTAINTY_NOT_STATED")
        proven+=1
    return {"status":"EVIDENCE_REFERENCES_VALID_DIAGNOSTIC_ONLY",
            "checked_proposals":proven,"can_edit_blender":False,"art_approved":False,
            "independent_artistic_verdict":"NOT_ISSUED"}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--proposal",required=True)
    p.add_argument("--grounding",required=True)
    p.add_argument("--observation",required=True)
    p.add_argument("--out",required=True)
    a=p.parse_args()
    proposal=json.loads(pathlib.Path(a.proposal).read_text(encoding="utf-8"))
    grounding=json.loads(pathlib.Path(a.grounding).read_text(encoding="utf-8"))
    obs=json.loads(pathlib.Path(a.observation).read_text(encoding="utf-8"))
    try:
        result=validate(proposal,grounding,obs)
        result["proposal_sha256"]=sha(pathlib.Path(a.proposal).read_bytes())
        result["grounding_sha256"]=sha(pathlib.Path(a.grounding).read_bytes())
        result["observation_sha256"]=sha(pathlib.Path(a.observation).read_bytes())
    except RejectedArtDirection as exc:
        result={"status":"REJECTED_NO_EDIT","reason":str(exc),"can_edit_blender":False,
                "art_approved":False}
    pathlib.Path(a.out).write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_DIRECTOR_SAFE_GATE",result["status"])
    if result["status"]=="REJECTED_NO_EDIT":raise SystemExit(1)

if __name__=="__main__":main()
