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
    key=("¿Qué revisión visual de la cubierta y silueta de la torre merece atención "
         "sin que los triángulos inclinados certifiquen faldones ni la luz neutra "
         "demuestre materiales definitivos?")
    memory=recall_context(key)
    if memory["selection_count"]>2:
        raise RuntimeError("MORE_THAN_TWO_VERIFIED_LESSONS")
    context=json.dumps(fields,ensure_ascii=False,separators=(",",":"))
    prompt=(
       "Diagnóstico de la torre congelada de Valoria. Hechos medidos y "
       "descripciones visuales etiquetadas como HIPÓTESIS:\n"+context+
       "\nLECCIONES VERIFICADAS, NO ÓRDENES DE EDICIÓN:\n"+
       memory["prompt_fragment"]+
       "\nSelecciona UN defecto concreto y el objetivo de mejora visible "
       "en la cámara elevada 3/4 de la torre, preservando entrada, "
       "escalera y UV. No afirmes que ya está corregido. "
       "La referencia de estilo no es prueba de geometría existente. "
       "No presupongas ni propongas cambiar el tipo de cubierta: ninguna "
       "medición prueba dos aguas, cuatro aguas o un solo faldón. "
       "Prioriza mejora localizada de bordes, aleros, juntas o remates "
       "sin modificar la tipología o la estructura protegida. "
       "Una IA visual separada decidirá después las operaciones Blender "
       "sujetas a un validador, y un revisor INDEPENDIENTE deberá aceptar el A/B."
    )
    director_attempts=[]
    feedback=""
    forbidden_pattern=(r"\b(dos aguas|cuatro aguas|dos faldones|cuatro faldones|"
                       r"two[- ]slope|hipped roof|gable roof)\b")
    for attempt in range(1,3):
        plan=ask_local("qwen3:14b",prompt+feedback)
        director_attempts.append(plan)
        choice=plan["direction"]
        unsupported=(choice["visual_defect"]+"\n"+
                     choice["proposed_visible_change"]).casefold()
        if not re.search(forbidden_pattern,unsupported):
            break
        feedback=(
          "\nRECHAZO: la respuesta anterior infería o prescribía "
          "un número/tipo de aguas del tejado NO certificado por Blender. "
          "Replantea UN acabado o defecto visual localizado "
          "sin cambiar la tipología de cubierta."
        )
    (out/"director-14b-all-attempts.json").write_text(
        json.dumps(director_attempts,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if re.search(forbidden_pattern,(plan["direction"]["visual_defect"]+"\n"+
                     plan["direction"]["proposed_visible_change"]).casefold()):
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
      "\nQWEN3:14B DIRECTOR FOCUS (proposed, NOT artistically certified):\n"+
      json.dumps(direction,ensure_ascii=False))
    memory_file=out/"selected-artist-lessons.txt"
    memory_file.write_text(lessons_text,encoding="utf-8")
    brief=(
      "On a VERIFIED COPY of the EXISTING Valoria tower, propose ONE "
      "authored and visually meaningful coherent architectural refinement "
      "for the upper profile and defensive readability while preserving "
      "the ORIGINAL door, staircase, railings and UV. Work ONLY on the copy, "
      "using real Blender with an independent fixed-camera A/B. "
      "Choose your OWN localized components, tools, parameters and dimensions; "
      "no prescribed geometry template. This is NOT Unity or an approved asset. "
      "Direction from Qwen3:14b: "+direction["visual_defect"]+
      "; intended improvement: "+direction["proposed_visible_change"]+
      "; uncertainty: "+direction["evidence_limit"]+
      ". Do not place objects over the protected entry/staircase."
    )
    studio=pathlib.Path(a.studio).resolve()
    artist=studio/"tools/art-direction/full-studio/artist_loop.py"
    if not artist.is_file():raise RuntimeError("CANNOT_FIND_PINNED_EXISTING_ARTIST_LOOP")
    argv=[
       sys.executable,str(artist),"--brief",brief,
       "--reference",str(pathlib.Path(a.reference).resolve()),
       "--source",str(copy_file),
       "--review",str(pathlib.Path(a.review).resolve()),
       "--out",str(out/"one-art-trial"),
       "--blender",str(pathlib.Path(a.blender).resolve()),
       "--model","qwen3-vl:8b-instruct-q4_K_M",
       "--prior-lessons",str(memory_file),
       "--max-cycles","1","--max-plan-attempts","2",
       "--max-actions","2","--max-tokens","1900",
       "--vision-max-edge","768","--asset-category","architecture",
       "--phase","full","--source-build-phase","--source-tool-sequencing"
    ]
    before_source=digest(source.read_bytes())
    start=time.monotonic()
    try:
        with (out/"existing-artist-studio-log.txt").open("w",encoding="utf-8") as log:
            proc=subprocess.run(argv,stdout=log,stderr=subprocess.STDOUT,
                                timeout=2800,check=False)
        code=proc.returncode
    except subprocess.TimeoutExpired:
        code=124
    src_same=digest(source.read_bytes())==before_source==ORIGINAL_SHA
    copy_same=digest(copy_file.read_bytes())==ORIGINAL_SHA
    result={
      "artist_studio_process_code":code,
      "elapsed_seconds":round(time.monotonic()-start,1),
      "source_sha256":ORIGINAL_SHA,
      "original_unchanged":src_same,"copy_unchanged":copy_same,
      "art_approved":False,"calibration_validated":False,
      "unity_tested":False,"independent_visual_review":"PENDING",
      "model_author":"qwen3-vl:8b-instruct-q4_K_M",
      "director_model":"qwen3:14b",
      "studio_code_branch":"art/eldoria-full-blender-artist-studio-20261014",
      "executed_own_artistic_plan":None
    }
    manifest=out/"one-art-trial/full-studio-result.json"
    if manifest.is_file():
        state=local_json(manifest)
        result["executed_own_artistic_plan"]=state.get("blender_real_execution")
        result["studio_summary"]=state
    (out/"art-trial-truth.json").write_text(
         json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_ONE_COPY_ART_TRIAL",
          json.dumps({k:result[k] for k in ("artist_studio_process_code",
             "source_sha256","original_unchanged","copy_unchanged",
             "executed_own_artistic_plan","art_approved")}),flush=True)
    if not src_same or not copy_same:raise RuntimeError("FROZEN_SOURCE_INTEGRITY_VIOLATION")
    if code!=0:raise SystemExit("ARTIST_STUDIO_REJECTED_OR_FAILED_INSPECT_EVIDENCE")
if __name__=="__main__":main()
