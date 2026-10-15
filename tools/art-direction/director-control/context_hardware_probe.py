"""One controlled, read-only Qwen14B full-evidence context-window probe (4K vs 12K).
Not an artistic certification, training, or Blender edit.
"""
import argparse,hashlib,json,pathlib,time,urllib.request
SHA="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
def read_json(path):
    raw=pathlib.Path(path).read_bytes()
    s=raw.decode("utf-8-sig").strip()
    if s.endswith("\\n"):s=s[:-2]
    return json.loads(s),hashlib.sha256(raw).hexdigest()
def api(endpoint,route,payload=None,timeout=30):
    req=urllib.request.Request(endpoint+route,data=None if payload is None else json.dumps(payload).encode(),headers={"Content-Type":"application/json"})
    with urllib.request.urlopen(req,timeout=timeout) as response:return json.load(response)
def main():
    ap=argparse.ArgumentParser()
    for key in ("grounding","vision","director","out"):ap.add_argument("--"+key,required=True)
    ap.add_argument("--model",default="qwen3:14b")
    ap.add_argument("--endpoint",default="http://127.0.0.1:11434")
    a=ap.parse_args()
    g,gs=read_json(a.grounding);v,vs=read_json(a.vision);d,ds=read_json(a.director)
    if g.get("source_sha256")!=SHA or g.get("immutable_source_verified") is not True or not g.get("all_cameras_same_exact_source_sha256"):raise SystemExit("SOURCE_GROUNDING_REJECTED")
    if v.get("original_source_sha256")!=SHA or d.get("source_sha256")!=SHA:raise SystemExit("MISMATCHED_AUTHENTIC_EVIDENCE")
    if g.get("connected_island_count")!=3029:raise SystemExit("UNEXPECTED_TOWER_GEOMETRY")
    obs=v.get("single_image_observations",[])
    if len(obs)<4:raise SystemExit("AUTHENTIC_CAMERA_OBSERVATIONS_MISSING")
    # Feed authentic long evidence, not invented padding or fabricated images.
    full=json.dumps({"blender_ground_truth":g,"qwen8b_camera_observations_hypothetical":v,"prior_grounded_8b_diagnosis":d},ensure_ascii=False,separators=(",",":"))
    system=("Eres director de arte 3D haciendo exclusivamente diagnóstico sin Blender. "+
            "Prioriza mediciones Blender verificadas sobre interpretaciones Qwen8B y no concluyas geometría de capturas.")
    question=("Usando el contexto original completo, devuelve ÚNICAMENTE JSON con estas claves exactas: "+
              "source_sha256 (string), connected_islands (integer), roof_form_proven (boolean), "+
              "editing_allowed (boolean), reason (una frase). NO INVENTES. "+
              "Aplica las restricciones vigentes del proyecto y usa exclusivamente las pruebas del contexto, sin inventar datos.")
    # Hold identical evidence and question fixed, vary only the actual Ollama num_ctx.
    probes=[]
    for window in (4096,12288):
        body={"model":a.model,"stream":False,"think":False,"format":"json","messages":[{"role":"system","content":system},{"role":"user","content":"CONTEXTO AUTÉNTICO:\n"+full+"\nPREGUNTA:\n"+question}],
              "options":{"temperature":0,"seed":20261015,"num_ctx":window,"num_predict":350}}
        start=time.monotonic()
        record={"num_ctx_requested":window,"request_sha256":hashlib.sha256(json.dumps(body,ensure_ascii=False,sort_keys=True).encode()).hexdigest()}
        try:
            response=api(a.endpoint,"/api/chat",body,timeout=900)
            raw=response.get("message",{}).get("content","").strip()
            parsed=json.loads(raw)
            checks={"source_sha_correct":parsed.get("source_sha256")==SHA,
                    "islands_correct":parsed.get("connected_islands")==3029,
                    "not_claiming_roof_proven":parsed.get("roof_form_proven") is False,
                    "no_edit":parsed.get("editing_allowed") is False}
            record.update({"correct_count":sum(checks.values()),"checks":checks,
               "complete":bool(response.get("done")),"answer":parsed,
               "answer_sha256":hashlib.sha256(raw.encode()).hexdigest(),
               "prompt_eval_count":response.get("prompt_eval_count"),
               "eval_count":response.get("eval_count"),
               "prompt_eval_duration_ns":response.get("prompt_eval_duration"),
               "eval_duration_ns":response.get("eval_duration"),
               "load_duration_ns":response.get("load_duration"),
               "total_duration_ns":response.get("total_duration")})
            ev=response.get("eval_duration",0)
            record["generated_tokens_per_sec"]=round(response.get("eval_count",0)*1e9/ev,2) if ev else None
        except Exception as exc:
            record["error"]=repr(exc)[:500];record["correct_count"]=0
        record["wall_seconds"]=round(time.monotonic()-start,2)
        probes.append(record)
        print("ELDORIA_CONTEXT_PROBE",window,"correct",record["correct_count"],"wall",record["wall_seconds"],flush=True)
    result={"schema_version":1,"model":a.model,"source_sha256":SHA,"context_evidence_sha256":{"geometry":gs,"vision":vs,"prior_director":ds},
        "authentic_evidence_characters":len(full),"cases":probes,"art_approved":False,"calibration_validated":False,
        "model_weights_trained":False,"blender_edits_executed":False,
        "inference_scope":"fixed pair context diagnostic; no generalized quality claim"}
    out=pathlib.Path(a.out);out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if any("error" in p or not p.get("complete") for p in probes):raise SystemExit("CONTEXT_PROBE_INFERENCE_INCOMPLETE")
if __name__=="__main__":main()
