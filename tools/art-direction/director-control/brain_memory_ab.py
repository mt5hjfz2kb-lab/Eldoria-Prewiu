"""Read-only Qwen3 14B paired memory test on real Blender spatial evidence."""
import argparse,hashlib,json,pathlib,random,time,urllib.request
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
CASES=[
("same_source","Tres vistas del mismo GLB SHA muestran la misma geometría fuente.","SOSTENIBLE"),
("roof_two","Las áreas de triángulos inclinados prueban exactamente dos faldones.","NO_DEMOSTRADO"),
("roof_one","893 triángulos inclinados prueban una única vertiente.","NO_DEMOSTRADO"),
("roof_four","Normales X+,X-,Y+,Y- prueban tejado a cuatro aguas.","NO_DEMOSTRADO"),
("camera_change","Cambiar de cámara modifica los vértices de un GLB intacto.","CONTRADICHO"),
("aabb","Dos AABB solapadas prueban contacto físico real.","NO_DEMOSTRADO"),
("uv","Se permite sustituir el atlas UV durante diagnóstico.","CONTRADICHO"),
("torches","Las antorchas de la referencia están necesariamente en la torre.","NO_DEMOSTRADO"),
("neutral","Iluminación neutra prueba materiales finales.","NO_DEMOSTRADO"),
("dimensions","Las dimensiones WORLD aproximadas son 2.45 x 2.65 x 3.27 m.","SOSTENIBLE"),
("islands","Una malla importada implica una única isla conectada.","CONTRADICHO"),
("quality","Gate PASS y autocorrección prueban arte de calidad 8/10.","CONTRADICHO")]
MEMORY=[
"Descripciones visuales son hipótesis, no geometría medida.",
"Un SHA original con distintas cámaras no implica malla diferente.",
"Normales agregadas no prueban el número de faldones.",
"Intersección AABB no certifica contacto de superficies.",
"Iluminación neutra no acredita materiales ni contenido de referencia.",
"Proteger entrada, escalera, barandillas y UV; no editar en diagnóstico.",
"PASS técnico y autocrítica no son evaluación estética independiente."]
def digest(b):return hashlib.sha256(b).hexdigest()
def ask(endpoint,model,prompt):
    body={"model":model,"stream":False,"think":False,"format":"json","messages":[{"role":"system","content":"Director de arte 3D. Solo razonamiento, sin editar. Separa hechos de hipótesis. Responde JSON."},{"role":"user","content":prompt}],"options":{"temperature":0,"seed":1337,"num_predict":300}}
    req=urllib.request.Request(endpoint+"/api/chat",data=json.dumps(body).encode(),headers={"Content-Type":"application/json"})
    t=time.monotonic()
    with urllib.request.urlopen(req,timeout=420) as f:r=json.load(f)
    if not r.get("done"):raise ValueError("INCOMPLETE_RESPONSE")
    raw=r.get("message",{}).get("content","").strip()
    if raw.startswith("```"):raw=raw.strip("`").removeprefix("json").strip()
    obj=json.loads(raw)
    if obj.get("decision") not in ("SOSTENIBLE","NO_DEMOSTRADO","CONTRADICHO"):raise ValueError("BAD_DECISION")
    return {"decision":obj["decision"],"explanation":str(obj.get("evidence",""))[:400],"response_sha256":digest(raw.encode()),"seconds":round(time.monotonic()-t,2)}
def main():
    p=argparse.ArgumentParser();p.add_argument("--grounding",required=True);p.add_argument("--out",required=True);p.add_argument("--model",default="qwen3:14b");p.add_argument("--endpoint",default="http://127.0.0.1:11434");a=p.parse_args()
    raw=pathlib.Path(a.grounding).read_bytes();g=json.loads(raw)
    if g.get("source_sha256")!=EXPECTED or g.get("immutable_source_verified") is not True or g.get("all_cameras_same_exact_source_sha256") is not True:raise SystemExit("SOURCE_PROVENANCE_FAILURE")
    if g.get("geometry",{}).get("mesh_object_count")!=1:raise SystemExit("MESH_GROUNDING_FAILURE")
    with urllib.request.urlopen(a.endpoint+"/api/tags",timeout=20) as f:models={m["name"] for m in json.load(f)["models"]}
    if a.model not in models:raise SystemExit("QWEN_MODEL_NOT_INSTALLED")
    context=json.dumps({"source_sha256":EXPECTED,"dimensions":g["bounding_box"]["world_dimensions_m"],"upper_inclined_triangles":g["geometry"]["zones_world_z"]["upper"]["inclined_triangles"],"islands":g.get("connected_island_count"),"note":"Normals/triangles aggregated, roof not segmented; no contact test","mode":"DIAGNOSTIC_ONLY"},ensure_ascii=False)
    order=[(k,c,e,arm) for k,c,e in CASES for arm in ("baseline","memory")];random.Random(20261015).shuffle(order)
    results=[]
    for key,claim,expected,arm in order:
        prompt="Datos medidos: "+context+("\nMemoria verificada: "+"; ".join(MEMORY) if arm=="memory" else "")+"\nAfirmación: "+claim+"\nResponde solo JSON con decision SOSTENIBLE, NO_DEMOSTRADO o CONTRADICHO y evidence explicativa. No se permite editar."
        entry={"case":key,"arm":arm,"expected":expected,"prompt_sha256":digest(prompt.encode())}
        try:entry.update(ask(a.endpoint,a.model,prompt))
        except Exception as exc:entry["error"]=str(exc)[:300]
        entry["correct"]=entry.get("decision")==expected;results.append(entry)
        print("ELDORIA_MEMORY_CASE",key,arm,entry["correct"],flush=True)
    pairs=[]
    for key,_,_ in CASES:
        b=next(x for x in results if x["case"]==key and x["arm"]=="baseline");m=next(x for x in results if x["case"]==key and x["arm"]=="memory")
        pairs.append({"id":key,"baseline_correct":b["correct"],"memory_correct":m["correct"]})
    baseline=sum(x["baseline_correct"] for x in pairs);memory=sum(x["memory_correct"] for x in pairs)
    gains=sum(not x["baseline_correct"] and x["memory_correct"] for x in pairs);regressions=sum(x["baseline_correct"] and not x["memory_correct"] for x in pairs)
    output={"source_sha256":EXPECTED,"grounding_sha256":digest(raw),"model":a.model,"case_count":len(pairs),"baseline_correct":baseline,"memory_correct":memory,"gain_count":gains,"regression_count":regressions,"pairs":pairs,"measurements":results,"art_approved":False,"any_blender_edits":False,"calibration_validated":False,"model_weights_trained":False}
    out=pathlib.Path(a.out);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(output,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("ELDORIA_MEMORY_AB_COMPLETE",baseline,memory,gains,regressions)
if __name__=="__main__":main()
