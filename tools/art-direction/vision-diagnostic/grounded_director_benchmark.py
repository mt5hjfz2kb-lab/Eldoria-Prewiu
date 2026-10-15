"""Read-only director benchmark: compare unsupported claims against verified Blender facts.
Never send AI output to a Blender executor. No downloads, no weights training, no approvals.
"""
import argparse,hashlib,json,pathlib,time,urllib.request,re
p=argparse.ArgumentParser()
p.add_argument("--grounding",required=True)
p.add_argument("--prior",required=True)
p.add_argument("--out",required=True)
args=p.parse_args()
ground_path=pathlib.Path(args.grounding); prior=pathlib.Path(args.prior)
out=pathlib.Path(args.out);out.mkdir(parents=True,exist_ok=True)
facts=json.loads(ground_path.read_text(encoding="utf-8"))
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if facts["source_sha256"]!=EXPECTED or facts["artist_edits_executed"]:
    raise RuntimeError("DIRECTOR_REQUIRES_IMMUTABLE_SPATIAL_TRUTH")
if not facts["geometry"]["upper_inclined_surfaces_exist"]:
    raise RuntimeError("ORIGINAL_UPPER_SLOPED_SURFACES_NOT_VERIFIED")
source_sha=hashlib.sha256(ground_path.read_bytes()).hexdigest()
context={
    "asset":"Valoria original; one identical source in three camera views",
    "sha256":EXPECTED,"world_up":"+Z",
    "world_dimensions_m":facts["bounding_box"]["world_dimensions_m"],
    "upper_inclined_triangles":facts["geometry"]["zones_world_z"]["upper"]["inclined_triangles"],
    "mesh_count":facts["geometry"]["mesh_object_count"],
    "connected_island_count":facts["connected_island_count"],
    "cameras_same_original_geometry":facts["all_cameras_same_exact_source_sha256"],
    "protected":["exterior staircase","original doorway","railings","UVs"],
    "verified_rules":facts["nonnegotiable_facts"],
    "limitations":facts["evidence_limitations"]
}
# Expected classifications are deliberately stored outside the model prompt.
# The test measures use of explicit grounding, NOT autonomous professional expertise.
cases=[
 ("roof_footprint","Dado que las paredes tienen planta aproximadamente rectangular, cualquier cubierta debe ser plana.","RECHAZAR"),
 ("camera_perception","Si el modelo describe un tejado distinto desde dos angulos, esas son necesariamente geometrías diferentes.","RECHAZAR"),
 ("staircase_protection","Mover la escalera exterior hacia el interior es una corrección autorizada en esta fase.","RECHAZAR"),
 ("uv_preservation","La escalera, la puerta y los UV originales se deben preservar en esta fase.","ACEPTAR"),
 ("source_same","Las tres cámaras observan una sola fuente GLB congelada e idéntica.","ACEPTAR"),
 ("contact_bbox","La intersección de dos cajas delimitadoras demuestra contacto auténtico de superficies.","RECHAZAR"),
 ("upper_slope_exact_roof","La existencia de caras inclinadas arriba demuestra por sí sola un tejado exacto a dos aguas.","RECHAZAR")
]
endpoint="http://127.0.0.1:11434"
with urllib.request.urlopen(endpoint+"/api/tags",timeout=15) as resp:
    available={x["name"] for x in json.load(resp).get("models",[])}
def complete(model,prompt,max_predict=500):
    req=urllib.request.Request(endpoint+"/api/chat",data=json.dumps({
       "model":model,"stream":False,"think":False,
       "messages":[{"role":"system","content":"Eres un director técnico-artístico 3D. Las mediciones verificadas tienen prioridad sobre inferencias. No ejecutes Blender. Responde en español."},
                   {"role":"user","content":prompt}],
       "options":{"temperature":0.1,"num_predict":max_predict,"num_ctx":12288}
    }).encode(),headers={"Content-Type":"application/json"})
    begun=time.monotonic()
    with urllib.request.urlopen(req,timeout=420) as res:
        obj=json.load(res)
    result=obj.get("message",{}).get("content","").strip()
    return {"text":result,"seconds":round(time.monotonic()-begun,2),
            "raw_sha256":hashlib.sha256(result.encode()).hexdigest(),
            "done":obj.get("done",False)}
def previous_record(name):
    path=prior/("diagnosis-"+name+".json")
    if not path.is_file():raise RuntimeError("MISSING_PREVIOUS_REAL_VISION_RECORD")
    raw=path.read_text(encoding="utf-8").strip()
    # Legacy diagnostic JSON accidentally wrote a literal \\n suffix; normalize representation only.
    if raw.endswith("\\n"):raw=raw[:-2]
    doc=json.loads(raw)
    if doc.get("original_source_sha256")!=EXPECTED:raise RuntimeError("PRIOR_SOURCE_SHA_MISMATCH")
    return doc
output=[]
for tag,model in [("4b","qwen3-vl:4b-instruct"),("8b","qwen3-vl:8b-instruct-q4_K_M")]:
    record={"schema_version":1,"model":model,"source_sha256":EXPECTED,
            "spatial_grounding_sha256":source_sha,"model_weights_trained":False,
            "read_only":True,"art_approved":False}
    if model not in available:
        record["status"]="MODEL_NOT_INSTALLED_NO_DOWNLOAD"
    else:
        old=previous_record(tag)
        captions=old.get("single_image_observations",[])
        if len(captions)<4:raise RuntimeError("REAL_PRIOR_SINGLE_VIEW_PROOFS_ABSENT")
        context_json=json.dumps(context,ensure_ascii=False)
        snippets="\n".join(
            "HIPOTESIS DE OBSERVACION "+x["view"]+": "+x["text"][:900]
            for x in captions)
        tests=[]
        for identifier,claim,expected in cases:
            ask=("HECHOS MEDIDOS, NO OPINIONES: "+context_json+"\n"
              "Evalúa una sola afirmación. Antes de razonar, tu PRIMERA PALABRA debe ser "
              "ACEPTAR o RECHAZAR. No supongas que las observaciones anteriores son infalibles. "
              "Afirmación: "+claim+" Justifica en 1-3 frases con los hechos.")
            try:
                answer=complete(model,ask,max_predict=210)
                first=re.search(r"\b(ACEPTAR|RECHAZAR)\b",answer["text"][:70].upper())
                prediction=first.group(1) if first else "SIN_DECISION"
                tests.append({"id":identifier,"actual":prediction,
                  "expected":expected,"correct":prediction==expected,
                  "answer":answer["text"],"raw_sha256":answer["raw_sha256"]})
            except Exception as exc:
                tests.append({"id":identifier,"expected":expected,
                   "correct":False,"error":str(exc)[:300]})
            print("SPATIAL_GROUNDED_CASE",model,identifier,tests[-1]["correct"],flush=True)
        director_prompt=(
           "Debes dirigir arte 3D de Valoria, pero NO PUEDES MODIFICAR BLENDER. "
           "Este es tu contexto de geometría verificado: "+context_json+"\n"
           "Estas descripciones fueron generadas por un modelo visual en cuatro imágenes, "
           "NO son mediciones y PUEDEN CONTRADECIRSE debido a perspectiva. "
           "No asumas que un error de descripción es un error de la malla:\n"+snippets+"\n"
           "A) Identifica cuál de las antiguas propuestas era físicamente equivocada "
           "(base rectangular no admite cubierta inclinada; cambio de geometría por cámara; "
           "mover escalera protegida) y por qué; B) separa hechos geométricos verificables, "
           "observaciones visuales tentativas y asuntos que requieren más comprobaciones; "
           "C) propone SOLO hasta tres mejoras artísticas concretas de BAJO RIESGO si tienen "
           "evidencia visual en estas observaciones, y si faltan datos di NO VERIFICADO; "
           "D) razona contactos reales, jerarquía de materiales y visualización móvil; "
           "E) propón un plan de inspección sin editar la torre. "
           "La salida es un informe de DIAGNOSTICO, nunca una autorización de ejecución.")
        try:
            review=complete(model,director_prompt,max_predict=1700)
            record["director_analysis"]=review
        except Exception as exc:
            record["director_error"]=str(exc)[:300]
        n=sum(t["correct"] for t in tests)
        record["adversarial_cases"]=tests
        record["correct_count"]=n
        record["total_count"]=len(cases)
        record["provisional_grounded_rule_use"]=n>=6 and bool(record.get("director_analysis",{}).get("text"))
        record["status"]=("REVIEW_REQUIRED_NOT_ART_APPROVAL" if record["provisional_grounded_rule_use"]
               else "SPATIAL_DIRECTOR_REJECTED")
    filename="grounded-director-"+tag+".json"
    (out/filename).write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    output.append({"model":model,"status":record["status"],"correct":record.get("correct_count"),
                   "cases":record.get("total_count"),"record":filename,
                   "grounded_rule_use":record.get("provisional_grounded_rule_use",False)})
summary={"schema_version":1,"source_sha256":EXPECTED,"grounding_sha256":source_sha,
         "model_results":output,"art_approved":False,
         "independent_artist_review":"PENDING","artist_edits_executed":False,
         "cost_eur":0,"model_downloads":0,"model_weights_training":False}
(out/"grounded-director-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_GROUNDED_DIRECTOR_DIAGNOSTIC_DONE",json.dumps(output,ensure_ascii=False))
