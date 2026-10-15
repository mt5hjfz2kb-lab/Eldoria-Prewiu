"""Prospectively balanced, read-only, binary epistemic paired test of verified memory.
Designed after tri-state labels proved ambiguous; original trial remains immutable evidence.
"""
import argparse,hashlib,json,pathlib,random,time,urllib.request
SHA="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
# Gold labels fixed BEFORE this workflow executes. 8 verifiable facts, 8 false/unproven claims.
CASES=[
 ("three_cameras_same_source","Los tres renders medidos proceden del mismo GLB original inalterado.",True),
 ("source_locked","La geometría fuente original se declara inmutable en la evidencia.",True),
 ("dimensions","Las dimensiones WORLD de la torre son aproximadamente 2.45, 2.65 y 3.27 metros.",True),
 ("sloped_faces","Blender contabilizó 893 triángulos inclinados en la zona superior.",True),
 ("many_islands","La malla original tiene 3029 islas topológicas conectadas distintas.",True),
 ("one_mesh","Blender importó un único objeto de malla de la torre.",True),
 ("world_z","La vertical del diagnóstico corresponde al eje +Z en WORLD.",True),
 ("slopes_present","La inspección geométrica confirmó la existencia de superficies superiores inclinadas.",True),
 ("roof_one","La cantidad de triángulos superiores permite certificar que hay un único faldón.",False),
 ("roof_four","Las normales agrupadas certifican que la cubierta tiene exactamente cuatro aguas.",False),
 ("contact_aabb","El solapamiento de dos cajas AABB certifica el contacto físico de sus superficies.",False),
 ("camera_geometry","Las descripciones distintas entre cámaras prueban modificaciones de vértices.",False),
 ("uv_replace","La misión diagnóstica autoriza sustituir las UV existentes de la torre.",False),
 ("neutral_material","Un render con iluminación neutra demuestra el material final del juego.",False),
 ("style_torches","Las antorchas dibujadas en una referencia de estilo prueban su presencia en el GLB.",False),
 ("green_is_art","Un PASS técnico y una autocrítica positiva certifican calidad artística 8/10.",False)]
MEMORY=[
 "Lección confirmada: separar observación visual de geometría WORLD medida.",
 "Lección confirmada: las 3 cámaras ven el mismo archivo SHA; no se deducen mutaciones.",
 "Lección confirmada: contar superficies inclinadas NO segmenta faldones.",
 "Lección confirmada: AABB solapadas no demuestran contacto geométrico.",
 "Lección confirmada: referencias de estilo y luz neutra no son medidas del modelo.",
 "Lección confirmada: están protegidos UV, entrada, escalera y barandillas; edición prohibida en diagnóstico.",
 "Lección confirmada: éxito CI y autocrítica no equivalen a aprobación estética."]
def digest(t):return hashlib.sha256(t.encode()).hexdigest()
def question(endpoint,model,context,claim,memory,ctx=4096):
    prompt="HECHOS BLENDER + CONTRATO INMUTABLE: "+context
    if memory:prompt+="\nMEMORIA VERIFICADA RECUPERADA: "+json.dumps(MEMORY,ensure_ascii=False)
    prompt+="\nAFIRMACIÓN A EVALUAR: "+claim+"\nDecide PROBADO solo si se deduce inequívocamente del contexto medido; cualquier conclusión falsa o no demostrada debe ser NO_PROBADO. Devuelve JSON {decision,reason}. No hagas modificaciones."
    req={"model":model,"stream":False,"think":False,"format":"json","messages":[
      {"role":"system","content":"Verificador epistémico de arte 3D, solo diagnóstico. JSON exclusivamente con decision PROBADO o NO_PROBADO y reason breve."},
      {"role":"user","content":prompt}],
      "options":{"temperature":0,"seed":221015,"num_ctx":ctx,"num_predict":200}}
    start=time.monotonic()
    with urllib.request.urlopen(urllib.request.Request(endpoint+"/api/chat",data=json.dumps(req).encode(),headers={"Content-Type":"application/json"}),timeout=650) as res:response=json.load(res)
    raw=response.get("message",{}).get("content","").strip()
    decision=json.loads(raw).get("decision")
    if decision not in ("PROBADO","NO_PROBADO") or not response.get("done"):raise ValueError("MALFORMED_OR_INCOMPLETE_DECISION")
    e=response.get("eval_duration",0)
    return {"decision":decision,"seconds":round(time.monotonic()-start,2),
        "prompt_sha256":digest(prompt),"raw_response_sha256":digest(raw),
        "prompt_eval_count":response.get("prompt_eval_count"),"eval_count":response.get("eval_count"),
        "total_duration_ns":response.get("total_duration"),
        "tokens_per_second":round(response.get("eval_count",0)*1e9/e,2) if e else None,
        "reason":str(json.loads(raw).get("reason",""))[:350]}
def main():
    p=argparse.ArgumentParser();p.add_argument("--grounding",required=True);p.add_argument("--out",required=True);p.add_argument("--model",default="qwen3:14b");a=p.parse_args()
    raw=pathlib.Path(a.grounding).read_bytes();g=json.loads(raw.decode("utf-8-sig"))
    if not(g.get("source_sha256")==SHA and g.get("immutable_source_verified") is True and g.get("all_cameras_same_exact_source_sha256") is True):raise SystemExit("SOURCE_PROVENANCE_INVALID")
    if len(CASES)!=16 or sum(label for _,_,label in CASES)!=8 or len({x[0] for x in CASES})!=16:raise SystemExit("UNBALANCED_OR_DUPLICATE_CASES")
    ctx=json.dumps({"sha256":g["source_sha256"],"immutable":g["immutable_source_verified"],
        "all_cameras_same_exact_source_sha256":g["all_cameras_same_exact_source_sha256"],
        "world_up":g["world_up"],"bounding_box":g["bounding_box"],
        "geometry":g["geometry"],"islands":g["connected_island_count"],
        "nonnegotiable_facts":g["nonnegotiable_facts"],
        "evidence_limitations":g["evidence_limitations"],
        "diagnostic_only":True,"original_unchanged":True},ensure_ascii=False,separators=(",",":"))
    trials=[(key,claim,gold,arm) for key,claim,gold in CASES for arm in ("without_memory","with_memory")]
    random.Random(221015).shuffle(trials);responses=[]
    for key,claim,gold,arm in trials:
        record={"case":key,"arm":arm,"gold":"PROBADO" if gold else "NO_PROBADO"}
        try:record.update(question("http://127.0.0.1:11434",a.model,ctx,claim,arm=="with_memory"))
        except Exception as exc:record["error"]=str(exc)[:450]
        record["correct"]=record.get("decision")==record["gold"]
        responses.append(record)
        print("ELDORIA_MEMORY_V2",key,arm,record["correct"],flush=True)
    pairs=[]
    for key,_,_ in CASES:
        x=next(v for v in responses if v["case"]==key and v["arm"]=="without_memory")
        y=next(v for v in responses if v["case"]==key and v["arm"]=="with_memory")
        pairs.append({"case":key,"without_memory_correct":x["correct"],"with_memory_correct":y["correct"]})
    base=sum(x["without_memory_correct"] for x in pairs);mem=sum(x["with_memory_correct"] for x in pairs)
    gains=sum(not x["without_memory_correct"] and x["with_memory_correct"] for x in pairs)
    regressions=sum(x["without_memory_correct"] and not x["with_memory_correct"] for x in pairs)
    result={"version":2,"method":"balanced 8 proven vs 8 unsupported; predeclared binary gold; paired randomized order","model":a.model,
        "source_sha256":SHA,"grounding_sha256":hashlib.sha256(raw).hexdigest(),
        "memory_sha256":digest(json.dumps(MEMORY,ensure_ascii=False)),
        "memory_lessons_count":len(MEMORY),"case_count":len(CASES),"baseline_correct":base,"memory_correct":mem,
        "gain_count":gains,"regression_count":regressions,"pairs":pairs,"responses":responses,
        "verdict":"MEMORY_FIXED_SET_GAIN" if gains>0 and regressions==0 else "NOT_VALIDATED",
        "art_approved":False,"calibration_validated":False,"blender_edits_executed":False,"model_weights_trained":False}
    path=pathlib.Path(a.out);path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_MEMORY_V2_RESULT",base,mem,gains,regressions,flush=True)
if __name__=="__main__":main()
