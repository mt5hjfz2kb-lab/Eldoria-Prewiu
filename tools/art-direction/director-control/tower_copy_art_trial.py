"""One isolated owner-authorized art CALIBRATION attempt, never a production promotion.

Use existing professional studio as-is from its pinned owner branch.
Qwen3 14B chooses a grounded creative focus, Qwen-VL chooses typed Blender
operations, original tower is checksum-frozen, and all outputs stay in TEMP.
Any Blender A/B is a candidate for INDEPENDENT review, never ART_APPROVED.
"""
import argparse
import hashlib
import json
import re
import os
import pathlib
import shutil
import subprocess
import sys
import time
import urllib.request

HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
from selective_memory import recall_context

ORIGINAL_SHA="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
LOCAL_API="http://127.0.0.1:11434"

def digest(data):
    return hashlib.sha256(data if isinstance(data,bytes) else data.encode("utf-8")).hexdigest()

def local_json(path):
    # Historical Qwen-VL artifact 38699407600 appends literal ASCII backslash+n
    # AFTER the final JSON brace. Preserve its raw bytes for SHA, decode only.
    raw=pathlib.Path(path).read_text(encoding="utf-8-sig").strip()
    if raw.endswith("\\n"):
        raw=raw[:-2].rstrip()
    return json.loads(raw)

def ask_local(model,prompt):
    payload={
        "model":model,"stream":False,"think":False,"format":"json",
        "messages":[
          {"role":"system","content":
           "Eres el director artístico de Eldoria. La fuente Blender y sus SHA "
           "tienen prioridad sobre las hipótesis de cámara. Analiza el DEFECTO "
           "VISUAL más importante, no edites, no inventes un tejado ni "
           "contactos físicos. Eres un director, NO el revisor independiente. "
           "Devuelve JSON con 'visual_defect', 'proposed_visible_change', "
           "'evidence_limit', 'protected_parts_risk' y 'intended_camera_view'."},
          {"role":"user","content":prompt}],
        "options":{"temperature":0.15,"seed":20261015,"num_ctx":8192,
                   "num_predict":650},"keep_alive":0
    }
    wire=json.dumps(payload,ensure_ascii=False).encode("utf-8")
    req=urllib.request.Request(LOCAL_API+"/api/chat",data=wire,
                             headers={"Content-Type":"application/json"})
    t=time.monotonic()
    with urllib.request.urlopen(req,timeout=720) as reply:
        raw=reply.read()
    parsed=json.loads(raw)
    txt=parsed.get("message",{}).get("content","")
    if not parsed.get("done") or parsed.get("done_reason")!="stop":
        raise RuntimeError("14B_ART_DIRECTION_INCOMPLETE_OR_TRUNCATED")
    doc=json.loads(txt)
    for k in ("visual_defect","proposed_visible_change","evidence_limit",
              "protected_parts_risk","intended_camera_view"):
        if not isinstance(doc.get(k),str) or len(doc[k].strip())<12:
            raise RuntimeError("14B_ART_DIRECTION_NOT_SPECIFIC:"+k)
    return {"raw_request_sha256":digest(wire),"raw_response_sha256":digest(raw),
            "raw_response":parsed,"direction":doc,
            "seconds":round(time.monotonic()-t,2)}

def main():
    p=argparse.ArgumentParser()
    for k in ("original","reference","spatial","vision","studio","blender","out","review"):
        p.add_argument("--"+k,required=True)
    a=p.parse_args()
    out=pathlib.Path(a.out).resolve()
    out.mkdir(parents=True,exist_ok=True)
    source=pathlib.Path(a.original).resolve()
    original=source.read_bytes()
    if digest(original)!=ORIGINAL_SHA or source.suffix.lower()!=".glb":
        raise RuntimeError("FROZEN_GLTF_ORIGINAL_HASH_MISMATCH")
    geo=local_json(a.spatial)
    vision=local_json(a.vision)
    if geo.get("source_sha256")!=ORIGINAL_SHA or geo.get("immutable_source_verified") is not True:
        raise RuntimeError("SPATIAL_ORIGINAL_MISMATCH")
    if not geo.get("all_cameras_same_exact_source_sha256"):
        raise RuntimeError("SPATIAL_CAMERA_IDENTITY_NOT_PROVEN")
    if vision.get("original_source_sha256")!=ORIGINAL_SHA:
        raise RuntimeError("VISION_ORIGINAL_MISMATCH")
    caps=vision.get("single_image_observations",[])
    if len(caps)<4:
        raise RuntimeError("MISSING_AUTHENTIC_CAMERA_OBSERVATIONS")
    copy_file=out/"tower-frozen-copy.glb"
    shutil.copyfile(source,copy_file)
    if digest(copy_file.read_bytes())!=ORIGINAL_SHA:
        raise RuntimeError("SOURCE_COPY_NOT_IDENTICAL")
    fields={
      "source_sha256":ORIGINAL_SHA,"world_up":geo["world_up"],
      "world_dimensions_m":geo["bounding_box"]["world_dimensions_m"],
      "measured_mesh":geo["mesh_provenance"],
      "one_imported_mesh":geo["geometry"]["mesh_object_count"]==1,
      "topological_islands":geo["connected_island_count"],
      "upper_inclined_triangle_count":geo["geometry"]["zones_world_z"]["upper"]["inclined_triangles"],
      "limitations":geo["evidence_limitations"],
      "observations_from_visual_model_are_hypotheses":[
          {"view":str(c["view"]),"hypothesis":str(c["text"])[:500]}
          for c in caps],
      "protected":"Original entrance, exterior staircase, railings, UV atlas and source mesh must remain intact"
    }
    key=("¿Las cajas AABB demuestran contacto físico real para un refuerzo "
         "de pared y la iluminación neutra prueba el material definitivo?")
    memory=recall_context(key)
    if memory["selection_count"]>2:
        raise RuntimeError("MORE_THAN_TWO_VERIFIED_LESSONS")
    context=json.dumps(fields,ensure_ascii=False,separators=(",",":"))
    prompt=(
       "TORRE VALORIA congelada. Geometría medible y observaciones visuales "
       "HIPOTÉTICAS:\n"+context+
       "\nLECCIONES VERIFICADAS RELEVANTES:\n"+memory["prompt_fragment"]+
       "\nCAMBIO DE ESTRATEGIA OBLIGATORIO: el anterior intento de alero "
       "fue RECHAZADO DOS VECES: malla con vértices duplicados y un cilindro "
       "centrado en el ápice, sin contacto geométrico exterior. No repitas "
       "el ALERO ni edites TEJADO/CUBIERTA. "
       "Escoge un solo DEFECTO de legibilidad de juntas, ensamblaje de troncos "
       "o refuerzo local sobre la pared lateral no protegida, visible desde "
       "cámara tres cuartos, sin tocar entrada, escalera, barandillas o UV. "
       "Propón un objetivo visual preciso, no código ni valores de geometría. "
       "El artista Qwen-VL decidirá la construcción original y sus medidas "
       "bajo el verificador de Blender; un revisor independiente evaluará A/B."
    )
    director_attempts=[]
    feedback=""
    forbidden_pattern=(r"\b(tejado|cubierta|aleros?|cumbrera|faldones?|"
                       r"dos aguas|cuatro aguas|eave|overhang|roof|"
                       r"two[- ]slope|hipped roof|gable roof)\b")
    for attempt in range(1,3):
        plan=ask_local("qwen3:14b",prompt+feedback)
        director_attempts.append(plan)
        choice=plan["direction"]
        unsupported=choice["proposed_visible_change"].casefold()
        if not re.search(forbidden_pattern,unsupported):
            break
        feedback=(
          "\nRECHAZO: la respuesta anterior infería o prescribía "
          "una propuesta sobre tejado/alero ya rechazada dos veces. "
          "Cambia de hipótesis: solo un ensamblaje de troncos o refuerzo "
          "visible de pared LATERAL no protegida."
        )
    (out/"director-14b-all-attempts.json").write_text(
        json.dumps(director_attempts,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if re.search(forbidden_pattern,plan["direction"]["proposed_visible_change"].casefold()):
        raise RuntimeError("DIRECTOR_REPEATED_UNVERIFIED_ROOF_TOPOLOGY")
    handoff={
      "schema_version":1,"mode":"OWNER_AUTHORIZED_ONE_TOWER_COPY_ART_TRIAL",
      "model":"qwen3:14b","model_text_only":True,
      "original_sha256":ORIGINAL_SHA,
      "blender_geometry_sha256":digest(pathlib.Path(a.spatial).read_bytes()),
      "8b_vision_sha256":digest(pathlib.Path(a.vision).read_bytes()),
      "selected_memory":memory,
      "director":plan,
      "director_attempts":len(director_attempts),
      "art_approved":False,"calibration_validated":False,
      "unity_tested":False,"original_source_edited":False
    }
    (out/"director-14b-handoff.json").write_text(
      json.dumps(handoff,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    direction=plan["direction"]
    lessons_text=(
      "OWNER-AUTHORIZED ISOLATED TOWER-COPY CALIBRATION ONLY. "
      "Verified original GLB SHA-256: "+ORIGINAL_SHA+
      ". Do NOT change the entrance, exterior stairs, railings or existing UVs. "
      "Do NOT imply a 2-slope or 4-slope roof without proof.\n"+
      memory["prompt_fragment"]+
      "\nTWO VERIFIED REJECTIONS: duplicate mesh vertices and apex-centered roof cylinder "
      "failed the safety gates; do NOT repeat eave strategy. Choose a different "
      "professional joinery hypothesis, with physical wall contact and "
      "protected door/stair/UV intact.\n"
      "QWEN3:14B DIRECTOR FOCUS (proposed, NOT artistically certified):\n"+
      json.dumps(direction,ensure_ascii=False))
    memory_file=out/"selected-artist-lessons.txt"
    memory_file.write_text(lessons_text,encoding="utf-8")
    brief=(
      "On a VERIFIED COPY of the EXISTING Valoria tower, propose ONE "
      "authored and visually meaningful coherent architectural refinement "
      "for localized lateral timber WALL joint readability (NOT roof or eave) while preserving "
      "the ORIGINAL door, staircase, railings and UV. Work ONLY on the copy, "
      "using real Blender with an independent fixed-camera A/B. "
      "Choose your OWN localized components, tools, parameters and dimensions; "
      "no prescribed geometry template. This is NOT Unity or an approved asset. "
      "Direction from Qwen3:14b: "+direction["visual_defect"]+
      "; intended improvement: "+direction["proposed_visible_change"]+
      "; uncertainty: "+direction["evidence_limit"]+
      ". Do not place objects over the protected entry/staircase."
    )
    # AUTHOR UPGRADE: after repeated Qwen-VL 8B invalid typed plans, Qwen3 14B
    # authors original typed tools/positions, and the EXISTING studio validates
    # and executes them on the frozen copy. This is not procedural geometry.
    studio=pathlib.Path(a.studio).resolve()
    workshop=studio/"tools/art-direction/full-studio"
    sys.path.insert(0,str(workshop))
    from contract import validate_plan, RejectedPlan
    from artist_loop import run_blender, repair_noncreative_action_ids, critique
    from source_geometry_gate import verify_source_plan, check_triangle_growth
    from architectural_component_guard import verify_volume_and_complexity
    from architectural_world_placement_gate import reject_obviously_bad_component_placement
    from screen_impact import screen_impact
    from protected_camera_regions import inspect_protected_regions

    inspect_root=out/"source-review"
    command=[
        str(pathlib.Path(a.blender).resolve()),"-b","--python-exit-code","1",
        "--python",str(workshop/"inspect_source.py"),"--",
        "--source",str(copy_file),"--out",str(inspect_root),
        "--review",str(pathlib.Path(a.review).resolve())]
    t=time.monotonic()
    checked=subprocess.run(command,text=True,stdout=subprocess.PIPE,
                           stderr=subprocess.STDOUT,timeout=850,check=False)
    (out/"source-inspection-blender-log.txt").write_text(checked.stdout,encoding="utf-8")
    if checked.returncode:raise RuntimeError("EXISTING_SOURCE_INSPECTOR_FAILED")
    inspected=local_json(inspect_root/"source-inspection.json")
    if inspected.get("source_unchanged") is not True or inspected.get("source_sha256")!=ORIGINAL_SHA:
        raise RuntimeError("ORIGINAL_BLENDER_SOURCE_INSPECTION_MISMATCH")
    source_inventory=inspected.get("objects",[])
    if not source_inventory:raise RuntimeError("NO_REAL_BLENDER_SOURCE_MESH_INVENTORY")
    brief=(
       "You are the 14B author of an ORIGINAL small architectural improvement "
       "on a frozen Valoria tower COPY. The previous Qwen-VL8B was unable to "
       "author valid geometry: its custom mesh had duplicate vertices and its "
       "next proposed wall addition was too thin to be seen, both REJECTED. "
       "This is a new author strategy. Choose genuine Blender tools and "
       "sizes for ONE visible attachment to an unprotected lateral log-wall "
       "joint, such as joinery or a support detail. Preserve door, staircase, "
       "railings, roof type and original UV. The building is only about "
       "2.45x2.65x3.27 metres. Use verified WORLD +Z up, NOT GLB local Y. "
       "New component must be a closed 3D volume, physically sizeable and "
       "situated at an existing exterior X/Y surface; no centrally buried "
       "filler, no hovering component, no tiny thin decoration. "
       "Do not reuse any rejected geometric coordinates or prior action. "
       "You choose original location, rotation, proportions, geometry and "
       "material. Design for one fixed 3/4 camera, pending independent review."
    )
    local_geom={
      "world_up":"+Z","source_sha256":ORIGINAL_SHA,
      "world_bounds":[{"name":m["name"],"min":m["world_bounds_min_m"],
                       "max":m["world_bounds_max_m"],"center":m["world_bounds_center_m"],
                       "dimensions":m["world_bounds_dimensions_m"],
                       "triangles":m["evaluated_triangles"]}
                      for m in source_inventory[:3]],
      "source_materials":geo["mesh_provenance"],
      "eyes_8b_visual_hypotheses":[
          {"view":str(v["view"]),"hypothesis":str(v["text"])[:360]}
          for v in caps],
      "focus_14b_from_previous_call":direction,
      "source_original_unchanged":True
    }
    # Typed action contract already exists in the professional studio:
    # schema v2, model chooses first primitive/mesh and optional material.
    # Importing its contract means no parallel Blender operator implementation.
    typed_spec=(
      "Devuelve SOLO JSON schema_version=2 con campos asset_id(string), "
      "observed_defect(string min 30 chars), design_intent(string min 30), "
      "expected_visual_difference(string min 30), mobile_risks(string), "
      "asset_decision='MODIFY', art_approved=false, "
      "actions array of EXACTLY 2 objects. "
      "Action 1: id unique, op EITHER 'primitive' OR 'mesh', "
      "target new safe name ^Eldoria_Added_C01_[A-Za-z][A-Za-z0-9_]{3,35}$, "
      "reason >=15 chars, params. For primitive: shape cube/cylinder/cone, "
      "location [x,y,z] WORLD, rotation [x,y,z] radians, scale [sx,sy,sz] "
      "are HALF dimensions and all positive. For mesh: DISTINCT vertices "
      "noncoplanar and CLOSED faces with each edge in exactly two faces. "
      "Use primitive if you cannot author closed manifold topology. "
      "Action 2: id different, op='material', target EXACT same new component, "
      "reason >=15 chars, params with base_color=[r,g,b,1] each 0..1, "
      "roughness 0.45..1, metallic 0..0.35. "
      "Never issue arbitrary Python, native commands or modify the original "
      "source mesh. Existing contact/volume/UV/protected gates independently "
      "validate and REJECT your design without editing values."
    )
    trial_root=out/"one-art-trial"/"cycle-1"
    trial_root.mkdir(parents=True,exist_ok=True)
    failures=[]
    author_result=None
    for attempt in range(1,4):
        content_prompt=(brief+"\nACTUAL MEASURED BLENDER SOURCE:\n"+
                json.dumps(local_geom,ensure_ascii=False,separators=(",",":"))+
                "\nSELECTED VERIFIED LESSONS:\n"+memory["prompt_fragment"]+
                "\nFORMAL PLAN CONTRACT:\n"+typed_spec+
                "\nREJECTIONS FROM PRIOR ATTEMPTS ON THIS COPY:\n"+
                json.dumps(failures,ensure_ascii=False))
        message=[
          {"role":"system","content":"Eres un artista técnico 3D senior. "
             "TÚ eliges operaciones y valores para Blender; NO eres juez de "
             "tu calidad artística. Entrega solo JSON tipado verificable, "
             "sin afirmaciones de éxito ni acciones fuera del contrato."},
          {"role":"user","content":content_prompt}
        ]
        req={
           "model":"qwen3:14b","stream":False,"think":False,"format":"json",
           "messages":message,
           "options":{"temperature":0.22,"seed":20261015+attempt,
                      "num_ctx":12288,"num_predict":2100},"keep_alive":0}
        wire=json.dumps(req,ensure_ascii=False).encode("utf-8")
        started=time.monotonic()
        try:
            call=urllib.request.Request(LOCAL_API+"/api/chat",data=wire,
                 headers={"Content-Type":"application/json"})
            with urllib.request.urlopen(call,timeout=850) as res:
                raw=res.read()
            api_response=json.loads(raw)
            text=api_response.get("message",{}).get("content","")
            (trial_root/f"qwen14b-plan-raw-attempt-{attempt}.json").write_bytes(raw)
            if api_response.get("done_reason")!="stop" or not text.strip():
                raise ValueError("QWEN14B_AUTHOR_INFERENCE_INCOMPLETE")
            doc=json.loads(text)
            original_doc_hash=digest(text)
            doc,identifier_repairs=repair_noncreative_action_ids(doc)
            validate_plan(doc)
            if len(doc["actions"])!=2 or doc["actions"][0]["op"] not in ("primitive","mesh") or doc["actions"][1]["op"]!="material":
                raise RejectedPlan("FIRST ORIGINAL PHYSICAL GEOMETRY SECOND NEW COMPONENT MATERIAL REQUIRED")
            if doc["actions"][0]["target"]!=doc["actions"][1]["target"]:
                raise RejectedPlan("MATERIAL_TARGET_MUST_MATCH_ORIGINAL_QWEN_COMPONENT")
            verify_source_plan(doc,source_inventory)
            verify_volume_and_complexity(doc,source_inventory)
            spatial_gate=reject_obviously_bad_component_placement(doc,source_inventory)
            (trial_root/"spatial-placement-proof.json").write_text(
                json.dumps(spatial_gate,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
            plan_file=trial_root/"artist-plan.json"
            plan_file.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
            author_result={"attempt":attempt,"original_qwen_text_sha256":original_doc_hash,
                 "request_sha256":digest(wire),"elapsed_seconds":round(time.monotonic()-started,2),
                 "noncreative_identifier_repairs":identifier_repairs,
                 "original_action_values_unmodified":True,
                 "source_sha256":ORIGINAL_SHA,
                 "model":"qwen3:14b"}
            (trial_root/"author-14b-provenance.json").write_text(
                json.dumps(author_result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
            break
        except Exception as exc:
            failed={"attempt":attempt,"reason":str(exc)[:700],
                    "no_geometry_values_rewritten":True}
            failures.append(failed)
            (trial_root/f"rejected-author-attempt-{attempt}.json").write_text(
                json.dumps(failed,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
            print("ELDORIA_QWEN14B_GEOMETRY_PROPOSAL_REJECTED",
                   attempt,failed["reason"],flush=True)
    if author_result is None:
        raise RuntimeError("THREE_TYPED_GEOMETRY_PLANS_REJECTED_DIAGNOSE_NEW_AUTHOR_STRATEGY")
    print("ELDORIA_QWEN14B_ORIGINAL_TYPED_PLAN_VALIDATED",
          author_result["attempt"],flush=True)
    output=trial_root/"output"
    output.mkdir(parents=True,exist_ok=True)
    run_blender(str(pathlib.Path(a.blender).resolve()),
                plan_file,output,copy_file,studio,
                pathlib.Path(a.review).resolve())
    proof=local_json(output/"execution-proof.json")
    if not proof.get("blender_execution") or not proof.get("matched_review_camera"):
        raise RuntimeError("REAL_BLENDER_CAMERA_OR_EXECUTION_NOT_PROVEN")
    if proof.get("source_sha256")!=ORIGINAL_SHA:
        raise RuntimeError("ACTUAL_EXECUTION_SOURCE_SHA_CHANGED")
    if proof.get("action_count")!=2:
        raise RuntimeError("BLENDER_DID_NOT_EXECUTE_EXACT_MODEL_ACTION_COUNT")
    if any(step.get("originals_changed") for step in proof["execution"]):
        raise RuntimeError("PROTECTED_SOURCE_MESH_MODIFIED")
    growth=check_triangle_growth(proof.get("baseline_evaluated_triangles"),proof.get("evaluated_triangles"))
    (trial_root/"evaluated-triangle-comparison.json").write_text(
        json.dumps(growth,indent=2)+"\n",encoding="utf-8")
    if not growth.get("allowed"):
        raise RuntimeError("MOBILE_TRIANGLE_GROWTH_REJECTED")
    screen=screen_impact(output/"before-blender.png",output/"after-blender.png")
    (trial_root/"camera-screen-impact.json").write_text(
        json.dumps(screen,indent=2)+"\n",encoding="utf-8")
    regions=inspect_protected_regions(
        output/"before-blender.png",output/"after-blender.png",pathlib.Path(a.review))
    (trial_root/"protected-camera-regions.json").write_text(
        json.dumps(regions,indent=2)+"\n",encoding="utf-8")
    if not regions.get("allowed"):
        raise RuntimeError("MODEL_ART_BLOCKS_PROTECTED_ENTRY_OR_STAIRS")
    if not screen.get("visually_present"):
        raise RuntimeError("MODEL_ART_NOT_VISIBLE_FROM_LOCKED_CAMERA")
    # An independent VISUAL model reviews, but cannot self-certify ART_APPROVED.
    try:
        import argparse as _argparse
        from artist_loop import critique as independent_visual_critique
        proxy=_argparse.Namespace(
            endpoint=LOCAL_API+"/api/chat",
            model="qwen3-vl:8b-instruct-q4_K_M",
            vision_max_edge=768)
        independent=independent_visual_critique(
            proxy,pathlib.Path(a.reference),output/"before-blender.png",
            output/"after-blender.png",
            "You are a DIFFERENT vision model than Qwen3:14b author. "
            "Compare genuine before and after objectively; reject cosmetic "
            "or disconnected pieces. Artistic approval is always independent.")
    except Exception as exc:
        independent={"critique":"INDEPENDENT_VISUAL_REVIEW_UNAVAILABLE",
                     "error":repr(exc)[:500],"clearly_failed":True}
    (trial_root/"independent-qwen-vl-review.json").write_text(
        json.dumps(independent,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    state={
        "state":"ART_REVIEW_REQUIRED_NOT_APPROVED",
        "source_sha256":ORIGINAL_SHA,
        "director_model":"qwen3:14b",
        "geometry_author_model":"qwen3:14b",
        "visual_observer_model":"qwen3-vl:8b-instruct-q4_K_M",
        "artist_plan_sha256":digest(plan_file.read_bytes()),
        "actual_blender_execution":True,
        "matched_review_camera":True,
        "inherited_geometry_preserved":True,
        "screen_impact":screen,
        "independent_ai_critique":independent,
        "art_approved":False,"calibration_validated":False,
        "unity_tested":False,"independent_human_visual_review":"REQUIRED"
    }
    (out/"art-trial-truth.json").write_text(
         json.dumps(state,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if digest(source.read_bytes())!=ORIGINAL_SHA or digest(copy_file.read_bytes())!=ORIGINAL_SHA:
        raise RuntimeError("IMMUTABLE_TOWER_ORIGINAL_OR_COPY_CHANGED")
    print("ELDORIA_QWEN14B_GENUINE_BLENDER_AB_REVIEW_REQUIRED",
          state["artist_plan_sha256"],screen["visually_present"],flush=True)

if __name__=="__main__":main()
