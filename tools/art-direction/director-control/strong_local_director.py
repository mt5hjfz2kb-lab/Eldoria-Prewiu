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
def ask(prompt,limit=350):
    request=urllib.request.Request(endpoint+"/api/chat",
       data=json.dumps({"model":a.model,"stream":False,"think":False,
          "messages":[{"role":"system","content":"Eres un director de arte 3D senior especializado en geometría y pruebas. Distingue hechos de imágenes, hipótesis y mediciones. Nada de Blender, ni autorizar ediciones."},
                      {"role":"user","content":prompt}],
          "options":{"temperature":0.1,"num_ctx":12288,"num_predict":limit}}).encode(),
       headers={"Content-Type":"application/json"})
    start=time.monotonic()
    with urllib.request.urlopen(request,timeout=900) as res:
        data=json.load(res)
    t=data.get("message",{}).get("content","").strip()
    return {"text":t,"sha256":hashlib.sha256(t.encode()).hexdigest(),
            "seconds":round(time.monotonic()-start,2),
            "complete":bool(data.get("done"))}
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
