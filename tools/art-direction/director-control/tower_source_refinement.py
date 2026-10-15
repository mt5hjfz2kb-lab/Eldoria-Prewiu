"""Eldoria: bounded original-Qwen mesh-refinement CALIBRATION on frozen copy.

NOT an asset promotion. No new model/tool definitions. Reuses real Blender studio,
its source inspector, contracts, brush bounds, live-image A/B and independent
visual reviewer. A completed technical run never marks artistic quality approved.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import pathlib
import shutil
import subprocess
import sys
import time
import urllib.request

HERE=pathlib.Path(__file__).resolve().parent
from selective_memory import recall_context
from tower_copy_art_trial import ORIGINAL_SHA, digest, local_json

API="http://127.0.0.1:11434/api/chat"

def save(path,obj):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+"\n",encoding="utf8")

def main():
    p=argparse.ArgumentParser()
    for option in ("original","reference","spatial","vision","studio","blender","out","review"):
        p.add_argument("--"+option,required=True)
    a=p.parse_args()
    work=pathlib.Path(a.out).resolve()
    work.mkdir(parents=True,exist_ok=True)
    original=pathlib.Path(a.original).resolve()
    if original.suffix.lower()!=".glb" or digest(original.read_bytes())!=ORIGINAL_SHA:
        raise RuntimeError("FROZEN_TOWER_SHA256_MISMATCH")
    spatial=local_json(a.spatial)
    vision=local_json(a.vision)
    if spatial["source_sha256"]!=ORIGINAL_SHA or not spatial["immutable_source_verified"]:
        raise RuntimeError("GROUNDING_NOT_FROM_FROZEN_TOWER")
    if vision["original_source_sha256"]!=ORIGINAL_SHA:
        raise RuntimeError("VISION_NOT_FROM_FROZEN_TOWER")
    frozen_copy=work/"tower-immutable-copy.glb"
    shutil.copy2(original,frozen_copy)
    if digest(frozen_copy.read_bytes())!=ORIGINAL_SHA:
        raise RuntimeError("FROZEN_COPY_SHA256_MISMATCH")
    studio=pathlib.Path(a.studio).resolve()/"tools/art-direction/full-studio"
    sys.path.insert(0,str(studio))
    from contract import validate_plan, RejectedPlan
    from source_geometry_gate import verify_source_plan
    from sculpt_scale_contract import source_brush_bounds, validate_source_anchor_brush
    from artist_loop import run_blender, critique, repair_noncreative_action_ids
    from screen_impact import screen_impact
    from protected_camera_regions import inspect_protected_regions

    inspect=work/"source-inspection"
    command=[str(pathlib.Path(a.blender).resolve()),"-b","--python-exit-code","1",
             "--python",str(studio/"inspect_source.py"),"--",
             "--source",str(frozen_copy),"--out",str(inspect),
             "--review",str(pathlib.Path(a.review).resolve())]
    scan=subprocess.run(command,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,
                        text=True,timeout=900,check=False)
    (work/"source-blender-inspection-log.txt").write_text(scan.stdout,encoding="utf8")
    if scan.returncode:
        raise RuntimeError("NATIVE_BLENDER_SOURCE_INSPECTOR_REJECTED")
    facts=local_json(inspect/"source-inspection.json")
    if not facts.get("source_unchanged") or facts.get("source_sha256")!=ORIGINAL_SHA:
        raise RuntimeError("SOURCE_INSPECTION_DID_NOT_VERIFY_FROZEN_IDENTITY")
    inventory=facts["objects"]
    if len(inventory)!=1:
        raise RuntimeError("EXPECTED_EXACTLY_ONE_IMPORTED_FROZEN_MESH")
    obj=inventory[0]
    # Blender projects actual imported source vertices into fixed review camera,
    # preserving original source. Qwen must select from these exact facts.
    visible_file=work/"projected-safe-source-landmarks.json"
    projection=subprocess.run([
       str(pathlib.Path(a.blender).resolve()),"-b","--python-exit-code","1",
       "--python",str(HERE/"visible_source_landmarks.py"),"--",
       "--source",str(frozen_copy),"--review",
       str(pathlib.Path(a.review).resolve()),"--out",str(visible_file)],
       stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,
       timeout=900,check=False)
    (work/"projected-visible-vertices-blender-log.txt").write_text(
        projection.stdout,encoding="utf8")
    if projection.returncode:
        raise RuntimeError("READ_ONLY_WORLD_TO_HERO_PIXEL_PROJECTION_FAILED")
    projected=local_json(visible_file)
    if projected.get("source_sha256")!=ORIGINAL_SHA or len(
            projected.get("selected_balanced_landmarks",[]))<10:
        raise RuntimeError("VERTEX_PROJECTION_SOURCE_NOT_VERIFIED")
    selected_vertices=projected["selected_balanced_landmarks"]
    inv=dict(obj)
    inv["vertex_landmarks"]=list(obj["vertex_landmarks"])+[
        {"vertex_index":p["vertex_index"],
         "local_position_m":p["local_center"]}
        for p in selected_vertices]
    working_inventory=[inv]
    bounds=source_brush_bounds(obj)
    if not bounds:
        raise RuntimeError("VERIFIED_SOURCE_BRUSH_LIMITS_MISSING")
    lesson=recall_context(
        "¿Una cámara o vista diferente de un GLB prueba que sus vértices "
        "cambiaron de verdad? ¿Permite modificar UV, la entrada o la "
        "escalera protegida para mejorar la calidad de un asset?")
    if lesson["selection_count"]>2:
        raise RuntimeError("RECALL_SELECTOR_EXCEEDED_TWO_LESSONS")

    source_data={
        "source_sha256":ORIGINAL_SHA,"mesh_name":obj["name"],
        "world_up":"+Z",
        "measured_world_bounds_min_m":obj["world_bounds_min_m"],
        "measured_world_bounds_max_m":obj["world_bounds_max_m"],
        "verified_local_extrema":obj["local_extrema"],
        "verified_world_bounds_dimensions_m":obj["world_bounds_dimensions_m"],
        "source_materials":obj["materials"],
        "source_uv_layers":obj["uv_layers"],
        "max_world_brush_limits":bounds,
        "camera_protected_regions":local_json(a.review)["protected_screen_regions"],
        "vision_hypotheses_not_geometry":vision.get("single_image_observations",[])[:4],
        "original_source_immutable":True,
        "source_verified_camera_projected_safe_vertices":selected_vertices,
        "projected_safe_landmark_proof_sha256":digest(visible_file.read_bytes())
    }
    encoded=json.dumps(source_data,ensure_ascii=False,separators=(",",":"))
    preview=inspect/"source-preview-blender.png"
    save(work/"refinement-source-context.json",{
        "model":"qwen3:14b","source":source_data,
        "grounding_sha256":digest(pathlib.Path(a.spatial).read_bytes()),
        "source_preview_sha256":digest(preview.read_bytes()),
        "selected_lessons":lesson,"original_sha256":ORIGINAL_SHA})

    schema={
      "type":"object",
      "properties":{
        "schema_version":{"type":"integer","enum":[2]},
        "asset_id":{"type":"string"},
        "observed_defect":{"type":"string"},
        "design_intent":{"type":"string"},
        "expected_visual_difference":{"type":"string"},
        "mobile_risks":{"type":"string"},
        "asset_decision":{"type":"string","enum":["MODIFY"]},
        "art_approved":{"type":"boolean","enum":[False]},
        "actions":{"type":"array","minItems":1,"maxItems":1,"items":{
           "type":"object","properties":{
             "id":{"type":"string"},
             "op":{"type":"string","enum":["sculpt"]},
             "target":{"type":"string","enum":[obj["name"]]},
             "reason":{"type":"string"},
             "params":{"type":"object","properties":{
                "center":{"type":"array","minItems":3,"maxItems":3,
                          "items":{"type":"number"}},
                "delta":{"type":"array","minItems":3,"maxItems":3,
                         "items":{"type":"number","minimum":-0.48,"maximum":0.48}},
                "radius":{"type":"number","minimum":0.7,"maximum":1.6}
             },"required":["center","delta","radius"],
               "additionalProperties":False}
            },"required":["id","op","target","reason","params"],
              "additionalProperties":False}}
      },
      "required":["schema_version","asset_id","observed_defect",
                  "design_intent","expected_visual_difference",
                  "mobile_risks","asset_decision","art_approved","actions"],
      "additionalProperties":False
    }
    # This is a source *vertex edit* so we intentionally do not author
    # standalone geometric boxes. A source copy may change; source GLB never.
    prompt=(
      "You are Qwen3 14B, 3D artist for an isolated calibration on a "
      "FROZEN COPY. Two prior sculpt_anchor attempts selected the exact "
      "same x_min,+Z,distance 0.0392m,radius 0.229m, moving actual vertices "
      "but modifying only 0.031% of camera pixels. Those were REJECTED. "
      "A Qwen-created giant rectangular prism also failed artistic review; "
      "do NOT add objects. NEW STRATEGY: use professional Blender 'sculpt' "
      "on an ACTUAL SAFE SCREEN-VISIBLE SOURCE VERTEX. "
      "Blender mapped original LOCAL vertex coordinates into fixed camera "
      "pixels. params.center MUST equal ONE of "
      "source_verified_camera_projected_safe_vertices[*].local_center "
      "triples, without interpolation, not WORLD coords. "
      "The params.delta and params.radius are in imported mesh LOCAL units "
      "(distinct from WORLD XYZ). Choose a clearly perceptible but not "
      "destructive sculpt. Radius 0.7..1.6 LOCAL, and delta vector length "
      "0.15..0.48 LOCAL. UV coordinates, source GLB, doorway, staircase, "
      "railings and roof shape remain protected. A screen-space projected "
      "vertex is not proof of a timber joint; explain uncertainty. "
      "Return exactly ONE original typed 'sculpt' action, no others; "
      "do NOT self-approve. Original source evidence:\n"+encoded+
      "\nSELECTED MEMORY:\n"+lesson["prompt_fragment"]
    )
    results=[]
    record={"schema_version":1,"scope":"FROZEN_COPY_ONLY_INHERITED_MESH_EDIT",
            "original_sha256":ORIGINAL_SHA,
            "expected_original_immutable":True,
            "model":"qwen3:14b",
            "review_model":"qwen3-vl:8b-instruct-q4_K_M",
            "selected_memory":lesson,
            "art_approved":False,
            "unity_tested":False,
            "calibration_validated":False,
            "attempts":results}
    save(work/"source-refinement-evidence.json",record)
    for attempt in range(1,3):
        trial=work/("attempt-"+str(attempt))
        trial.mkdir(parents=True,exist_ok=True)
        prior=json.dumps([{"attempt":r["attempt"],"reason":r.get("reject")}
                          for r in results],ensure_ascii=False)
        body=prompt+"\nREJECTED PREVIOUS PLAN (if any):\n"+prior
        req={
          "model":"qwen3:14b","stream":False,"think":False,"format":schema,
          "messages":[{"role":"system","content":
           "Senior Blender asset editor. JSON typed ORIGINAL parameters only. "
           "Stay within inherited-mesh brush limits. No art approval."},
          {"role":"user","content":body}],
          "options":{"temperature":0.24,"seed":20261027+attempt,
                     "num_ctx":12288,"num_predict":950},
          "keep_alive":0
        }
        wire=json.dumps(req,ensure_ascii=False).encode("utf8")
        start=time.monotonic()
        entry={"attempt":attempt,"prompt_sha256":digest(wire),
               "input_source_sha256":ORIGINAL_SHA}
        results.append(entry)
        save(work/"source-refinement-evidence.json",record)
        try:
            call=urllib.request.Request(API,data=wire,headers={"Content-Type":"application/json"})
            with urllib.request.urlopen(call,timeout=720) as reply:
                raw=reply.read()
            (trial/"qwen14b-raw-api.json").write_bytes(raw)
            response=json.loads(raw)
            if response.get("done_reason")!="stop":
                raise RuntimeError("QWEN14B_MESH_EDIT_INFERENCE_TRUNCATED")
            answer=response.get("message",{}).get("content","")
            entry["response_sha256"]=digest(answer)
            proposal=json.loads(answer)
            proposal,ids=repair_noncreative_action_ids(proposal)
            validate_plan(proposal)
            if len(proposal["actions"])!=1:
                raise RejectedPlan("EXACTLY_ONE_LOCAL_SCULPT_REQUIRED")
            action=proposal["actions"][0]
            if action["op"]!="sculpt" or action["target"]!=obj["name"]:
                raise RejectedPlan("ONLY_SOURCE_GROUNDED_ORIGINAL_SCULPT_ALLOWED")
            xyz=action["params"]["center"]
            if not any(max(abs(x-y) for x,y in zip(xyz,p["local_center"]))<0.0002
                       for p in selected_vertices):
                raise RejectedPlan("SCULPT_CENTER_NOT_FROM_FROZEN_BLENDER_CAMERA_LANDMARK")
            delta=action["params"]["delta"]
            magnitude=sum(float(x)*float(x) for x in delta)**0.5
            if not 0.15<=magnitude<=0.48:
                raise RejectedPlan("SCULPT_DELTA_LOCAL_UNITS_TOO_SMALL_OR_LARGE")
            verify_source_plan(proposal,working_inventory)
            entry["qwen_artist_values_unchanged"]=True
            entry["noncreative_id_repairs"]=ids
            plan=trial/"qwen14b-art-plan.json"
            save(plan,proposal)
            output=trial/"output"
            run_blender(str(pathlib.Path(a.blender).resolve()),
                        plan,output,frozen_copy,pathlib.Path(a.studio).resolve(),
                        pathlib.Path(a.review).resolve())
            proof=local_json(output/"execution-proof.json")
            save(trial/"source-mesh-proof.json",proof)
            actually_edited=any(
                x.get("geometry_changed") and x.get("originals_changed")
                for x in proof.get("execution",[]))
            if not actually_edited:
                raise RuntimeError("BLENDER_EXECUTED_BUT_DID_NOT_EDIT_INHERITED_MESH")
            before=output/"before-blender.png"
            after=output/"after-blender.png"
            screen=screen_impact(before,after)
            protected=inspect_protected_regions(
                before,after,pathlib.Path(a.review))
            save(trial/"screen-impact.json",screen)
            save(trial/"protected-review.json",protected)
            if not protected["allowed"]:
                raise RuntimeError("SOURCE_SCULPT_AFFECTED_PROTECTED_CAMERA_REGION")
            if not screen["visually_present"]:
                raise RuntimeError("SOURCE_SCULPT_NOT_VISIBLE_FROM_REVIEW_CAMERA")
            import argparse as _ap
            visual=critique(
                _ap.Namespace(endpoint=API,model="qwen3-vl:8b-instruct-q4_K_M",
                              vision_max_edge=768),
                pathlib.Path(a.reference).resolve(),before,after,
                "Independent different vision model. Compare genuine matching "
                "camera renders and reject damage, distortions, or subtle "
                "changes that fail to improve the architecture. Never self approve.")
            save(trial/"independent-visual-review.json",visual)
            entry.update({"status":"BLENDER_AB_REVIEW_REQUIRED",
                 "real_original_mesh_edited_on_copy":True,
                 "real_camera_matched":bool(proof.get("matched_review_camera")),
                 "screen_changed_fraction":screen.get("changed_fraction"),
                 "independent_vision_review":visual,
                 "art_approved":False})
            save(work/"source-refinement-evidence.json",record)
            if digest(original.read_bytes())!=ORIGINAL_SHA or digest(frozen_copy.read_bytes())!=ORIGINAL_SHA:
                raise RuntimeError("FROZEN_SOURCE_SHA_CHANGED")
            print("ELDORIA_REAL_MESH_EDIT_TRIAL",json.dumps(entry,ensure_ascii=False),flush=True)
            return
        except Exception as exc:
            entry["reject"]=repr(exc)[:700]
            entry["status"]="FAIL_CLOSED"
            save(work/"source-refinement-evidence.json",record)
            print("ELDORIA_REJECTED_MESH_EDIT_TRIAL",attempt,entry["reject"],flush=True)
    raise RuntimeError("ALL_SOURCE_MESH_SCULPT_ATTEMPTS_FAILED_REVIEW_EVIDENCE")
if __name__=="__main__":
    main()
