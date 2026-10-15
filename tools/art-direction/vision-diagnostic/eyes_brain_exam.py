"""Real Qwen vision/reasoning exam on frozen original Valoria; NO modeling API calls."""
import argparse, base64, hashlib, json, os, pathlib, time, urllib.request, urllib.error
from PIL import Image,ImageOps
import io

p=argparse.ArgumentParser()
p.add_argument("--views",required=True)
p.add_argument("--reference",required=True)
p.add_argument("--out",required=True)
p.add_argument("--model",action="append",default=[])
a=p.parse_args()
root=pathlib.Path(a.views).resolve()
out=pathlib.Path(a.out).resolve();out.mkdir(parents=True,exist_ok=True)
obs=json.loads((root/"vision-observation.json").read_text(encoding="utf-8"))
assert obs["original_unchanged"] and len(obs["frames"])==3
assert len({f["view"] for f in obs["frames"]})==3
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if obs["source_sha256"]!=EXPECTED: raise RuntimeError("Not the frozen Valoria source")
ref=pathlib.Path(a.reference).resolve()
if not ref.is_file():raise RuntimeError("Approved reference image missing")
images=[];sources=[]
for frame in obs["frames"]:
    path=root/frame["png"]
    data=path.read_bytes()
    if hashlib.sha256(data).hexdigest()!=frame["sha256"]:raise RuntimeError("Camera PNG hash mismatch")
    images.append(path);sources.append({"view":frame["view"],"sha256":frame["sha256"]})
images.append(ref);sources.append({"view":"approved_style_reference","sha256":hashlib.sha256(ref.read_bytes()).hexdigest()})
def encoded_image(path):
    with Image.open(path) as im:
        im=ImageOps.exif_transpose(im).convert("RGB")
        im.thumbnail((640,640))
        tmp=io.BytesIO()
        im.save(tmp,format="JPEG",quality=88,subsampling=0)
        return base64.b64encode(tmp.getvalue()).decode()
system=(
 "You are being TESTED as a senior 3D medieval game artist and independent scene observer. "
 "Use ONLY the four real supplied images (first three are one unmodified Valoria tower "
 "viewed from different angles; last is APPROVED ELDORIA ART STYLE REFERENCE), and the "
 "true geometric metadata. WORLD XYZ, Z is up. Do not invent unseen materials or "
 "geometry, and distinguish visually confirmed defects from hypotheses. "
 "NO tools, scripts, Blender actions, or scene mutations may be executed. "
 "Work as a thoughtful professional art director: prioritize silhouette, architectural "
 "structural plausibility and contact, materials, identity, protection and player scale. "
 "Your answer must be JSON, not markdown, in Spanish. Do not assert you have achieved "
 "any visual improvement, learned model weights, or approval."
)
bounds=f"Tower ORIGINAL world min={obs['world_bounds_min']} max={obs['world_bounds_max']}."
task=(
 "EXAMEN DE OJOS Y CEREBRO, SIN MODELAR. Esta es la misma torre original congelada "
 "en vistas front_three_quarter, reverse_three_quarter y elevated_roof, "
 "seguida por referencia artística aprobada. "+bounds+
 " A. Describe seis observaciones concretas visibles, con vista que las acredita "
 "y una incertidumbre relevante. B. Prioriza exactamente tres defectos artísticos "
 "demostrables (o señala incertidumbre si no se pueden demostrar) con diagnóstico causal, "
 "solución geométrica ORIGINAL detallada, anclaje espacial o contacto físicamente plausible, "
 "y el cambio que vería el jugador. C. Mantén puerta, escalera, UV/atlas y volumen original. "
 "D. Advierte errores evitables y di qué vistas/datos faltan. "
 "No propongas adornos flotantes, cilindros aislados ni simple recoloración como reparación. "
 "Estructura JSON obligatoria: {\"observations\":[{\"view\":\"...\",\"visible_fact\":\"...\","
 "\"confidence\":0.0}],\"prioritized_defects\":[{\"rank\":1,\"name\":\"...\","
 "\"visual_evidence\":\"...\",\"source_views\":[\"...\"],\"cause_hypothesis\":\"...\","
 "\"specific_artistic_fix\":\"...\",\"world_space_contact_reasoning\":\"...\","
 "\"expected_mobile_camera_effect\":\"...\",\"preserve\":\"...\",\"confidence\":0.0}],"
 "\"uncertainties\":[\"...\"],\"what_to_inspect_next\":\"...\","
 "\"no_blender_executed\":true}. "
 "Devuelve exactamente tres prioritized_defects; no inventes observaciones."
)
prompt_bytes=(system+"\n"+task).encode("utf-8")
endpoint="http://127.0.0.1:11434"
with urllib.request.urlopen(endpoint+"/api/tags",timeout=15) as res:
    installed={m["name"] for m in json.load(res)["models"]}
results=[]
models=a.model or ["qwen3-vl:4b-instruct","qwen3-vl:8b-instruct-q4_K_M"]
for model in models:
    label="4b" if "4b" in model else "8b"
    fp=out/("diagnosis-"+label+".json")
    record={"requested_model":model,"image_sources":sources,
        "original_source_sha256":obs["source_sha256"],
        "prompt_sha256":hashlib.sha256(prompt_bytes).hexdigest(),
        "no_blender_executed":True,"art_approved":False,"independent_review":"PENDING",
        "model_weights_updated":False}
    if model not in installed:
        record.update(status="NOT_INSTALLED_NO_DOWNLOAD",raw_response="",json_valid=False)
    else:
        payload={
            "model":model,"stream":True,"think":False,
            "messages":[{"role":"system","content":system},
                        {"role":"user","content":task,"images":[encoded_image(i) for i in images]}],
            "options":{"temperature":0.2,"num_ctx":16384,"num_predict":2400},
            "format":"json"
        }
        body=json.dumps(payload).encode("utf-8")
        start=time.monotonic()
        raw=[]
        try:
            req=urllib.request.Request(endpoint+"/api/chat",body,{"Content-Type":"application/json"})
            done=False
            with urllib.request.urlopen(req,timeout=540) as response:
                for line in response:
                    if not line.strip():continue
                    part=json.loads(line)
                    if part.get("error"):raise RuntimeError(str(part["error"])[:400])
                    raw.append(part.get("message",{}).get("content",""))
                    if part.get("done"):done=True;break
            content="".join(raw)
            record.update(raw_response=content,raw_sha256=hashlib.sha256(content.encode()).hexdigest(),
                          seconds=round(time.monotonic()-start,2),response_complete=done)
            try:
                decoded=json.loads(content)
                record["parsed_answer"]=decoded
                record["json_valid"]=isinstance(decoded,dict)
                record["three_defects"]=isinstance(decoded,dict) and len(decoded.get("prioritized_defects",[]))==3
                record["six_observations"]=isinstance(decoded,dict) and len(decoded.get("observations",[]))>=6
                record["status"]="AWAITING_INDEPENDENT_VISUAL_REVIEW" if done else "INCOMPLETE"
            except (ValueError,TypeError) as e:
                record.update(status="INVALID_RESPONSE_JSON",json_valid=False,parse_error=str(e)[:200])
        except Exception as e:
            record.update(status="MODEL_CALL_ERROR",error=str(e)[:500],
                          raw_response="".join(raw),seconds=round(time.monotonic()-start,2))
    fp.write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    results.append({"model":model,"status":record["status"],"record":fp.name,
                    "elapsed_seconds":record.get("seconds"),"raw_sha256":record.get("raw_sha256"),
                    "three_defects":record.get("three_defects")})
    print("ELDORIA_EYES_BRAIN_MODEL",model,record["status"],flush=True)
summary={"schema_version":1,"state":"DIAGNOSTIC_ONLY_INDEPENDENT_REVIEW_PENDING",
         "tower_sha256":obs["source_sha256"],"images":sources,"model_results":results,
         "artist_edit_executed":False,"art_approved":False,"unity_verified":False,"model_downloads":0}
(out/"diagnostic-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
if not any(r["status"]=="AWAITING_INDEPENDENT_VISUAL_REVIEW" for r in results):
    raise SystemExit("No complete local-model vision diagnosis; artifact preserved")
print("ELDORIA_REAL_VISION_DIAGNOSTIC_READY_FOR_INDEPENDENT_REVIEW")
