"""Immutable source-candidate handoff for an existing Unity artist consumer.

An owner can inspect/import a Qwen-created GLB. The studio cannot promote
it into Bastion/World4X/Unity production scenes. The active consumer owns the
integration and Windows runner.
"""
from __future__ import annotations
import argparse,hashlib,json,pathlib
from glb_mobile_preflight import analyze_glb

class NotReadyForHandoff(ValueError):
    pass

def sha(path):
    return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()

def build_handoff(folder,reference,consumer):
    root=pathlib.Path(folder).resolve()
    ref=pathlib.Path(reference).resolve()
    if consumer not in ("bastion1","world4x","asset-library"):
        raise NotReadyForHandoff("Unknown Eldoria art consumer")
    if not ref.is_file():raise NotReadyForHandoff("Source reference missing")
    result_file=root/"full-studio-result.json"
    if not result_file.is_file():
        raise NotReadyForHandoff("Genuine Qwen complete-cycle manifest missing")
    result=json.loads(result_file.read_text(encoding="utf-8"))
    if result.get("art_approved") is not False:
        raise NotReadyForHandoff("This handoff must never self-approve art")
    if not result.get("blender_real_execution"):
        raise NotReadyForHandoff("No physical Qwen-to-Blender edit execution")
    successful=[(i,x) for i,x in enumerate(result.get("cycles",[]),1)
                if x.get("status")=="ART_REVIEW_REQUIRED"]
    if not successful:raise NotReadyForHandoff("No successful AI-authored cycle")
    cycle_num,_=successful[-1]
    cycle=root/("cycle-"+str(cycle_num))
    plan=cycle/"artist-plan.json"
    raw=cycle/"qwen-raw.json"
    out=cycle/"output"
    proof_file=out/"execution-proof.json"
    paths={"plan":plan,"raw_qwen":raw,"glb":out/"artist-asset.glb",
           "blend":out/"artist-asset.blend","before":out/"before-blender.png",
           "after":out/"after-blender.png","execution_proof":proof_file}
    if any(not p.is_file() for p in paths.values()):
        raise NotReadyForHandoff("Missing required real Blender/Qwen evidence")
    if json.loads(raw.read_text(encoding="utf-8"))!=json.loads(plan.read_text(encoding="utf-8")):
        raise NotReadyForHandoff("LLM original action plan differs from executed Blender plan")
    proof=json.loads(proof_file.read_text(encoding="utf-8"))
    expected={"plan":"artist_plan_sha256","glb":"glb_sha256",
              "blend":"blend_sha256","before":"before_blender_sha256",
              "after":"after_blender_sha256"}
    for key,field in expected.items():
        if sha(paths[key])!=proof.get(field):
            raise NotReadyForHandoff("AI source evidence has an altered "+key+" SHA")
    if not proof.get("matched_review_camera") or not proof.get("blender_execution"):
        raise NotReadyForHandoff("Unverified real Blender output/camera")
    inspection=analyze_glb(paths["glb"],material_required=False)
    source_relative=lambda p:p.resolve().relative_to(root).as_posix()
    assets={key:{"relative_path":source_relative(p),"sha256":sha(p)}
            for key,p in paths.items()}
    return {"schema_version":1,
            "state":"ARTIST_AUTHORED_BLENDER_SOURCE_REVIEW_PENDING",
            "consumer":consumer,"artist_model":result.get("model"),
            "cycle":cycle_num,"qwen_original_verified":True,
            "blender_real_execution":True,"candidate_glb":assets["glb"],
            "candidate_blend":assets["blend"],"all_authentic_evidence":assets,
            "reference_file_name":ref.name,"reference_sha256":sha(ref),
            "glb_inspection":inspection,
            "source_immutable_sha256":result.get("frozen_source_sha256"),
            "source_promotion_authorized":False,
            "art_approved":False,"unity_actual_import_verified":False,
            "matched_native_unity_camera_verified":False,
            "mobile_hardware_validated":False,
            "next_gates":["independent_artistic_reference_review",
              "native_unity_import_and_matched_camera_A_B",
              "mobile_device_triangles_materials_gameplay_and_lod_qa",
              "consumer_workstream_owner_safe_handoff"],
            "active_consumer_files_modified":False}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--result",type=pathlib.Path,required=True)
    p.add_argument("--reference",type=pathlib.Path,required=True)
    p.add_argument("--consumer",choices=("bastion1","world4x","asset-library"),required=True)
    p.add_argument("--out",type=pathlib.Path,required=True)
    a=p.parse_args()
    report=build_handoff(a.result,a.reference,a.consumer)
    a.out.parent.mkdir(parents=True,exist_ok=True)
    a.out.write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_ARTIST_CANDIDATE_HANDOFF_SOURCE_ONLY",report["consumer"],
          report["cycle"],report["candidate_glb"]["sha256"])
if __name__=="__main__":main()
