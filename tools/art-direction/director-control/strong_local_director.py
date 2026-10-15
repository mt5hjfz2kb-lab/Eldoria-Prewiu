"""Compare a larger local text reasoning director with grounded 8B-VL visual evidence.
NO visual capability is claimed for the text director. NO Blender editing is performed.
"""
import argparse,hashlib,json,pathlib,time,urllib.request,re
p=argparse.ArgumentParser()
p.add_argument("--grounding",required=True)
p.add_argument("--prior-vision",required=True)
p.add_argument("--prior-director",required=True)
p.add_argument("--out",required=True)
p.add_argument("--model",default="qwen3:14b")
p.add_argument("--selective-freeform-diagnostic",action="store_true",help="Read-only paired free artistic reasoning with selectively recalled verified lessons; original behavior unchanged otherwise")
p.add_argument("--provenance-calibration",action="store_true",help="Focused UV/material reasoning plus read-only Blender inspection planning; no edits")
a=p.parse_args()
out=pathlib.Path(a.out);out.mkdir(parents=True,exist_ok=True)
g_bytes=pathlib.Path(a.grounding).read_bytes()
geo=json.loads(g_bytes)
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if geo["source_sha256"]!=EXPECTED or not geo["all_cameras_same_exact_source_sha256"]:
    raise RuntimeError("STRONG_DIRECTOR_SPATIAL_SOURCE_MISMATCH")
def read_prior(path):
    s=pathlib.Path(path).read_text(encoding="utf-8-sig").strip()
    if s.endswith("\\n"):s=s[:-2]
    return json.loads(s)
previous=read_prior(a.prior_vision)
weak=read_prior(a.prior_director)
if previous.get("original_source_sha256")!=EXPECTED or weak.get("source_sha256")!=EXPECTED:
    raise RuntimeError("PRIOR_DIAGNOSTIC_HAS_DIFFERENT_TOWER")
caps=previous.get("single_image_observations",[])
if len(caps)<4:raise RuntimeError("PRIOR_AUTHENTIC_CAMERA_CAPTIONS_MISSING")
ground={
  "source":"ONE original immutable GLB across all camera angles",
  "sha256":EXPECTED,
  "world_up":"+Z",
  "world_dimensions_m":geo["bounding_box"]["world_dimensions_m"],
  "upper_sloped_triangles":geo["geometry"]["zones_world_z"]["upper"]["inclined_triangles"],
  "triangle_slope_does_not_reveal_exact_roof_form":True,
  "one_imported_mesh":geo["geometry"]["mesh_object_count"]==1,
  "distinct_topological_islands":geo["connected_island_count"],
  "mesh_provenance":geo["mesh_provenance"],
  "protected_elements":["entrance","exterior staircase","railings","UV atlas"],
  "prohibitions":[
   "camera differences never imply geometric mutations of an unchanged source",
   "rectangular footprints do not require a flat or monopitch roof",
   "AABB overlap cannot certify physical surface contact",
   "neutral Blender debugging lighting does NOT certify game materials",
   "descriptions of the art-style reference are NOT facts about the actual tower"
  ],
  "limitations":geo["evidence_limitations"]
}
facts=json.dumps(ground,ensure_ascii=False)
cases=[
 ("roof_requires_flat","Toda torre de planta rectangular necesita cubierta plana.","RECHAZAR"),
 ("different_camera_means_mesh_changes","Las tres vistas muestran geometría distinta porque cambian las descripciones.","RECHAZAR"),
 ("uv_protection","Está permitido sustituir las UV actuales antes de obtener aprobación artística.","RECHAZAR"),
 ("same_original","Las tres cámaras muestran el mismo archivo GLB original sin alteraciones.","ACEPTAR"),
 ("contact_test","La intersección de cajas de dos vigas demuestra unión física real.","RECHAZAR"),
 ("sloped_exact","893 triángulos inclinados arriba prueban que el tejado es de un solo plano.","RECHAZAR"),
 ("style_not_scene","La iluminación de antorchas de la referencia puede no existir en los renders neutros de la torre.","ACEPTAR"),
 ("protected_stair","La escalera exterior no debe trasladarse durante esta fase diagnóstica.","ACEPTAR")
]
endpoint="http://127.0.0.1:11434"
with urllib.request.urlopen(endpoint+"/api/tags",timeout=15) as res:
    models={m["name"] for m in json.load(res)["models"]}
if a.model not in models:raise RuntimeError("STRONG_MODEL_NOT_INSTALLED_NO_PULL_IN_PYTHON")
def ask(prompt,limit=350,*,json_mode=False,temperature=0.1):
    request=urllib.request.Request(endpoint+"/api/chat",
       data=json.dumps({"model":a.model,"stream":False,"think":False,
          **({"format":"json"} if json_mode else {}),
          "messages":[{"role":"system","content":"Eres un director de arte 3D senior especializado en geometría y pruebas. Distingue hechos de imágenes, hipótesis y mediciones. Nada de Blender, ni autorizar ediciones."},
                      {"role":"user","content":prompt}],
          "options":{"temperature":temperature,"num_ctx":12288,"num_predict":limit,
                     **({"seed":20261015} if json_mode else {})}}).encode(),
       headers={"Content-Type":"application/json"})
    start=time.monotonic()
    with urllib.request.urlopen(request,timeout=900) as res:
        data=json.load(res)
    t=data.get("message",{}).get("content","").strip()
    return {"text":t,"sha256":hashlib.sha256(t.encode()).hexdigest(),
            "seconds":round(time.monotonic()-start,2),
            "complete":bool(data.get("done")),
            "done_reason":data.get("done_reason"),
            "prompt_eval_count":data.get("prompt_eval_count"),
            "eval_count":data.get("eval_count"),
            "total_duration_ns":data.get("total_duration"),
            "raw_api_response":data }
# Focused calibration is an opt-in read-only diagnostic through this SAME 14B
# art director, not a replacement agent and not an authoring/production pipeline.
if a.provenance_calibration:
    if a.selective_freeform_diagnostic:
        raise RuntimeError("DIRECTOR_DIAGNOSTIC_MODES_ARE_EXCLUSIVE")
    import random
    from selective_memory import recall_context, ORIGINAL_SHA256
    from evidence_gate import validate_provenance_diagnostic, RejectedArtDirection
    if EXPECTED != ORIGINAL_SHA256 or not geo.get("immutable_source_verified"):
        raise RuntimeError("SOURCE_NOT_FROZEN_FOR_CALIBRATION")
    if not geo.get("all_cameras_same_exact_source_sha256"):
        raise RuntimeError("CAMERA_SOURCE_NOT_VERIFIED")
    inventory=geo.get("mesh_provenance",[])
    if len(inventory)!=1 or inventory[0].get("uv_layer_count",0)<1 or not inventory[0].get("materials"):
        raise RuntimeError("INVENTORY_MATERIAL_UV_SOURCE_MISSING")
    # Unaltered measured fields, no fabricated material certification.
    grounded={
        "source_sha256":EXPECTED,
        "same_immutable_glb_all_cameras":True,
        "world_up":geo["world_up"],
        "bounding_box":geo["bounding_box"],
        "geometry":geo["geometry"],
        "connected_island_count":geo["connected_island_count"],
        "mesh_provenance":geo["mesh_provenance"],
        "evidence_limitations":geo["evidence_limitations"],
        "protected_elements":ground["protected_elements"],
        "observations_from_8b_uncertain":[
           {"camera":str(x["view"]), "hypothesis":str(x["text"])[:220]} for x in caps
        ]
    }
    grounded_json=json.dumps(grounded,ensure_ascii=False,separators=(",",":"))
    questions=[
       ("source_uv",
        "Audita los datos REALES de UV y material asignado del GLB. Enumera hechos medidos con cita de campo; separa inventario de fidelidad en Unity."),
       ("neutral_materials",
        "Se ve una torre en luz neutra y una referencia con antorchas. ¿Qué prueba el inventario sobre UV/material de origen y qué NO demuestra sobre material definitivo ni antorchas?"),
       ("islands_and_uv",
        "Hay una sola malla importada y muchas islas topológicas. ¿Cómo afecta esto a la EXISTENCIA de UV y material asignado? No deduzcas ausencia de UV por fragmentación."),
       ("preproduction_plan",
        "Eres director de arte: prepara hasta tres inspecciones profesionales de SOLO LECTURA sobre UV, material, tejado y contacto usando información Blender y observaciones visuales. No plantees ediciones ejecutables.")
    ]
    spec=(
       "DEVUELVE SOLO UN OBJETO JSON en español con EXACTAMENTE estos campos "
       "(sin Markdown): source_has_uv (boolean), uv_layer_count (integer), "
       "uv_coordinates (integer), source_has_assigned_material (boolean), "
       "assigned_source_material (string), original_mesh_object_count (integer), "
       "topological_island_count (integer), final_unity_material_certified (boolean), "
       "torches_in_source_proven (boolean), roof_type_proven (boolean), "
       "physical_contacts_proven (boolean), model_edit_authorized (boolean), "
       "art_approved (boolean), decision (string), "
       "evidence_paths (array of source field names EXACTLY like "
       "mesh_provenance[0].uv_layer_count, mesh_provenance[0].uv_coordinates, "
       "mesh_provenance[0].materials[0], geometry.mesh_object_count, "
       "connected_island_count), read_only_inspection_plan (array of 1-3 objects "
       "with keys tool,mode,goal. tool ONLY one of BLENDER_READ_ONLY, "
       "QWEN_VL_OBSERVATION, INDEPENDENT_REVIEW; mode INSPECT_ONLY), "
       "uncertainty (brief grounded reason). Verifica que los datos "
       "POSITIVOS de inventario no equivalen a certificación Unity. "
       "No afirmes que no hay material ni UV cuando sí existe un registro medido. "
       "Nunca ejecutes una herramienta ni ordenes modificar ningún asset."
    )
    runs=[(key,question,arm,repeat) for key,question in questions
          for arm in ("baseline","selective") for repeat in (1,2)]
    random.Random(20261015).shuffle(runs)
    records=[]
    result={"schema_version":1,"mode":"DIAGNOSTIC_ONLY_MATERIAL_UV_CALIBRATION",
            "model":a.model,"source_sha256":EXPECTED,
            "spatial_grounding_sha256":hashlib.sha256(g_bytes).hexdigest(),
            "prior_vision_sha256":hashlib.sha256(pathlib.Path(a.prior_vision).read_bytes()).hexdigest(),
            "prior_director_sha256":hashlib.sha256(pathlib.Path(a.prior_director).read_bytes()).hexdigest(),
            "source_context_sha256":hashlib.sha256(grounded_json.encode()).hexdigest(),
            "prompt_schema_sha256":hashlib.sha256(spec.encode()).hexdigest(),
            "material_ground_truth":inventory[0],
            "questions":[{"key":k,"question":q} for k,q in questions],
            "conditions":["baseline","selective"],"fixed_seed":20261015,
            "repetitions_per_case_arm":2,"responses":records,
            "art_approved":False,"can_edit_blender":False,
            "editor_actions":[],"model_weights_trained":False,
            "calibration_validated":False}
    target=out/"qwen3-14b-material-provenance-calibration.json"
    def save():
        target.write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    save()
    for key,question,arm,repeat in runs:
        memory=recall_context(question) if arm=="selective" else None
        prompt=("MEDICIONES DE BLENDER Y OBSERVACIONES DE CÁMARA NO CONFIRMADAS: "+
            grounded_json+"\nCUESTIÓN DEL DIRECTOR: "+question+"\n"+spec)
        if memory and memory["prompt_fragment"]:
            prompt+="\n"+memory["prompt_fragment"]
        rec={"case":key,"arm":arm,"replicate":repeat,
             "prompt":prompt,
             "prompt_sha256":hashlib.sha256(prompt.encode()).hexdigest(),
             "recall":memory,
             "selected_lesson_indices":
                [x["index"] for x in memory["selection"]] if memory else []}
        try:
            response=ask(prompt,720,json_mode=True,temperature=0)
            rec["ollama"]=response
            if response.get("done_reason")!="stop" or not response["complete"]:
                raise ValueError("TRUNCATED_OR_INCOMPLETE_OUTPUT")
            parsed=json.loads(response["text"])
            rec["parsed"]=parsed
            rec["independent_gate"]=validate_provenance_diagnostic(parsed,geo)
        except Exception as error:
            rec["error"]=repr(error)[:550]
            rec["independent_gate"]={"status":"FAIL_CLOSED","reason":repr(error)[:550],
                                      "can_edit_blender":False,"art_approved":False}
        records.append(rec)
        save()
        print("ELDORIA_UV_MATERIAL_REVIEW",key,arm,repeat,
              rec["independent_gate"]["status"],flush=True)
    result["summary"]={
         "calls":len(records),
         "valid":sum(r["independent_gate"]["status"]=="PROVENANCE_DIAGNOSTIC_TECH_PASS_NOT_ART_APPROVAL" for r in records),
         "invalid":sum(r["independent_gate"]["status"]=="FAIL_CLOSED" for r in records),
         "by_arm":{arm:{
             "valid":sum(r["arm"]==arm and r["independent_gate"]["status"]=="PROVENANCE_DIAGNOSTIC_TECH_PASS_NOT_ART_APPROVAL" for r in records),
             "total":sum(r["arm"]==arm for r in records)}
             for arm in ("baseline","selective")},
         "no_mesh_edits":True,
         "art_approved":False,
         "calibration_validated":False
    }
    save()
    print("ELDORIA_UV_MATERIAL_CALIBRATION_SUMMARY",
          json.dumps(result["summary"]),flush=True)
    if result["summary"]["invalid"]>0:
        raise SystemExit("PROVENANCE_DIAGNOSTIC_GATE_REJECTED")
    raise SystemExit(0)

# The actual Qwen3 14B director uses the same grounded input and camera hypotheses
# as the original experiment. This opt-in branch tests selective recall for OPEN
# artistic reasoning; it never reaches a Blender command or any production output.
if a.selective_freeform_diagnostic:
    import random
    from selective_memory import recall_context, ORIGINAL_SHA256
    if EXPECTED != ORIGINAL_SHA256 or not geo.get("immutable_source_verified"):
        raise RuntimeError("SELECTIVE_DIRECTOR_SOURCE_NOT_IMMUTABLE")
    cam_notes="\n".join(
        "HYPOTHESIS_FROM_8B_VIEW "+str(x["view"])+": "+str(x["text"])[:850]
        for x in caps)
    source_context=(
        "BLENDER WORLD XYZ MEASURED EVIDENCE (source SHA verified): "+facts+
        "\nCAMERA DESCRIPTIONS: uncertain observations rather than geometry facts:\n"+
        cam_notes)
    # Open-ended questions are NOT their expected answers. Selective retrieval
    # depends only on the question text, never model output or reviewer labels.
    investigations=[
        ("roof", "¿Los triángulos inclinados certifican el número exacto de faldones del tejado de esta torre? Describe qué forma puedes concluir, qué queda incierto y el dato que falta."),
        ("contact", "¿El solapamiento entre cajas AABB demuestra contacto físico correcto en el soporte estructural? Distingue observación de medición y propone pruebas pendientes."),
        ("camera", "¿Un cambio de cámara y descripciones visuales diferentes demuestran que se modificó el GLB original o su geometría? Contrasta vistas y procedencia."),
        ("materials", "¿La iluminación neutra y la referencia artística demuestran los materiales finales y la presencia de antorchas? Evalúa solo lo verificado."),
        ("overview", "Como director artístico independiente, identifica hasta tres prioridades de revisión visual de esta torre, fuentes y límites de evidencia, qué preservarías y cuál es la siguiente comprobación más útil.")
    ]
    conditions=[("baseline",False),("selective",True)]
    trials=[(name,question,mode,with_mem) for name,question in investigations for mode,with_mem in conditions]
    random.Random(20261015).shuffle(trials)
    evidence={
        "schema_version":1, "mode":"DIAGNOSTIC_ONLY_READ_ONLY",
        "model":a.model,"model_is_text_only":True,"vision_model":"qwen3-vl:8b-instruct-q4_K_M",
        "grounding_sha256":hashlib.sha256(g_bytes).hexdigest(),
        "original_source_sha256":EXPECTED,
        "prior_vision_sha256":hashlib.sha256(pathlib.Path(a.prior_vision).read_bytes()).hexdigest(),
        "prior_8b_director_sha256":hashlib.sha256(pathlib.Path(a.prior_director).read_bytes()).hexdigest(),
        "grounded_context_sha256":hashlib.sha256(source_context.encode("utf-8")).hexdigest(),
        "sample_scope":"five exploratory freeform questions; fixed seed order; not an artistic approval",
        "conditions":["baseline","selective"],"responses":[],
        "any_blender_edit_executed":False,"art_approved":False,
        "calibration_validated":False,"new_model_weights_trained":False,
        "editor_actions":[],"can_edit_blender":False
    }
    filename=out/"qwen3-14b-director-selective-freeform.json"
    def persist():
        filename.write_text(json.dumps(evidence,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    persist()
    for name,question,mode,use_memory in trials:
        recall=recall_context(question) if use_memory else None
        # Keep the exact same original evidence and user question between arms.
        prompt=(source_context+"\nOPEN DIRECTOR QUESTION: "+question+
             "\nExplain what is supported by actual geometry vs visual hypothesis. "
             "Give specific priority only if justified; otherwise say NO_EDIT. "
             "Never propose executable modeling commands. "
             "Respond in Spanish in at most 300 words using compact sections: verified facts, "
             "visual hypotheses, additional measurements, and NO_EDIT conclusion if insufficient.")
        if recall and recall["prompt_fragment"]:
            prompt+="\n"+recall["prompt_fragment"]
        record={"case":name,"condition":mode,"question":question,
                "prompt_sha256":hashlib.sha256(prompt.encode("utf-8")).hexdigest(),
                "prompt_exact":prompt,"recall":recall,
                "selected_lesson_indices":[x["index"] for x in recall["selection"]] if recall else [],
                "original_source_sha256":EXPECTED}
        try:
            response=ask(prompt,1200)
            record.update({"answer":response})
            if not response["complete"] or response.get("done_reason")!="stop" or not response["text"].strip():
                raise ValueError("FREEFORM_INFERENCE_INCOMPLETE_OR_TRUNCATED")
        except Exception as exc:
            record["error"]=str(exc)[:450]
        evidence["responses"].append(record)
        persist()
        print("ELDORIA_SELECTIVE_DIRECTOR_FREEFORM",name,mode,
              "memories",len(record["selected_lesson_indices"]),
              "complete",bool(record.get("answer")),flush=True)
    evidence["all_inferences_completed"]=len(evidence["responses"])==len(trials) and all("answer" in x and "error" not in x for x in evidence["responses"])
    persist()
    print("ELDORIA_REAL_DIRECTOR_SELECTIVE_DIAGNOSTIC",
          len(evidence["responses"]),"complete",evidence["all_inferences_completed"],flush=True)
    if not evidence["all_inferences_completed"]:
        raise SystemExit("OPEN_DIRECTOR_INFERENCE_FAILURE")
    raise SystemExit(0)

tests=[]
for key,claim,expected in cases:
    prompt=("Contexto VERIFICADO: "+facts+"\nDecide si esta afirmación es sostenible. "
            "Comienza EXACTAMENTE con ACEPTAR o RECHAZAR y da una explicación breve. "
            "Afirmación: "+claim)
    try:
        result=ask(prompt,180)
        match=re.search(r"\b(ACEPTAR|RECHAZAR)\b",result["text"][:70].upper())
        decision=match.group(1) if match else "SIN_DECISION"
        tests.append({"case":key,"expected":expected,"decision":decision,
                      "correct":decision==expected,"answer":result["text"],
                      "response_sha256":result["sha256"]})
    except Exception as e:
        tests.append({"case":key,"expected":expected,"correct":False,"error":str(e)[:300]})
    print("STRONG_DIRECTOR_CASE",key,tests[-1]["correct"],flush=True)
notes="\n".join("OBSERVACION DE CAMARA, NO HECHO PROBADO ["+x["view"]+"]: "+x["text"][:850] for x in caps)
free_prompt=(
  "Eres el director artístico de Eldoria. Trabajas SIN EDITAR. Contexto estructural verificado: "
  +facts+"\nObservaciones visuales de un modelo anterior; pueden contener errores: \n"+notes+
  "\nANTES de sugerir mejoras revisa y corrige explícitamente estos 4 peligros: "
  "creer que 893 triángulos inclinados equivalen a una sola vertiente; "
  "deducir que una cubierta inclinada es imposible en una planta rectangular; "
  "convertir cambios de cámara en cambios de geometría; "
  "inventar para la torre antorchas, aleros o ventanas solo por aparecer en la referencia de estilo. "
  "\nEntrega: (1) hechos comprobados, (2) errores del diagnóstico previo, "
  "(3) hasta TRES recomendaciones por prioridad, respaldadas cada una por vistas originales "
  "y por el contexto espacial, con contacto estructural aún pendiente si no medido, "
  "(4) qué preservar, (5) qué NO SABEMOS, (6) qué te impediría dar una orden de edición. "
  "Si no hay evidencia suficiente para cambiar una pieza, escribe NO_EDIT. "
  "No te inventes nombres de objetos internos, índices ni calidades de materiales por renders neutros."
)
strong=ask(free_prompt,2200)
result={"schema_version":1,"model":a.model,"model_is_text_only":True,
        "spatial_grounding_sha256":hashlib.sha256(g_bytes).hexdigest(),
        "original_source_sha256":EXPECTED,
        "vision_model":"qwen3-vl:8b-instruct-q4_K_M",
        "visual_descriptions_hypothetical":True,
        "cases":tests,"case_score":sum(x["correct"] for x in tests),
        "case_total":len(tests),"director_freeform":strong,
        "art_approved":False,"independent_review":"REQUIRED",
        "any_blender_edit_executed":False,"new_model_weights_trained":False}
(out/"qwen3-14b-grounded-director.json").write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_QWEN3_14B_ART_DIRECTION_DIAGNOSTIC_COMPLETE",
      result["case_score"],"/",len(tests),len(strong["text"]))
