"""No-edit, matched-input vision exam for locally installed Qwen models.

This benchmark deliberately DOES NOT force Ollama 'format=json'. On 2026-10-15
both 4B and 8B returned identical empty JSON skeletons despite valid real images.
A valid transport response is not a visual diagnosis.
"""
import argparse, base64, hashlib, io, json, pathlib, time, urllib.request
from PIL import Image, ImageOps

parser=argparse.ArgumentParser()
parser.add_argument("--views",required=True)
parser.add_argument("--reference",required=True)
parser.add_argument("--out",required=True)
parser.add_argument("--model",action="append",default=[])
a=parser.parse_args()
root=pathlib.Path(a.views).resolve()
out=pathlib.Path(a.out).resolve();out.mkdir(parents=True,exist_ok=True)
obs=json.loads((root/"vision-observation.json").read_text(encoding="utf-8"))
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if obs.get("source_sha256")!=EXPECTED or not obs.get("original_unchanged"):
    raise RuntimeError("Frozen original Valoria source validation failed")
if len(obs["frames"])!=3 or len(set(f["view"] for f in obs["frames"]))!=3:
    raise RuntimeError("Three different original views are mandatory")
reference=pathlib.Path(a.reference).resolve()
if not reference.is_file():raise RuntimeError("Approved Eldoria reference unavailable")
sources=[];images=[]
for f in obs["frames"]:
    img=root/f["png"]
    if not img.is_file():raise RuntimeError("Original camera PNG missing")
    raw=img.read_bytes()
    if not raw.startswith(b"\x89PNG\r\n\x1a\n") or hashlib.sha256(raw).hexdigest()!=f["sha256"]:
        raise RuntimeError("Original camera PNG hash mismatch")
    sources.append({"view":f["view"],"sha256":f["sha256"]})
    images.append(img)
sources.append({"view":"approved_style_reference","sha256":hashlib.sha256(reference.read_bytes()).hexdigest()})
images.append(reference)
def img_b64(p):
    with Image.open(p) as im:
        im=ImageOps.exif_transpose(im).convert("RGB")
        im.thumbnail((768,768))
        buff=io.BytesIO()
        im.save(buff,format="JPEG",quality=88,subsampling=0)
        return base64.b64encode(buff.getvalue()).decode()
encoded=[img_b64(i) for i in images]

system=("Eres un director artistico 3D medieval senior en una prueba de observacion de imagenes, "
"no de Blender. La torre original de Valoria se muestra con tres camaras distintas, "
"seguida por una referencia de estilo aprobada (cuarta imagen). Describe SOLO detalles "
"que realmente puedas observar; expresa incertidumbres. WORLD XYZ, Z es vertical. "
"No escribas codigo, planes JSON ni comandos; no digas que has modelado o aprobado nada.")

task=(
"EXAMEN VISUAL INDEPENDIENTE: la primera imagen es frontal oblicua de la torre original, "
"la segunda oblicua inversa, la tercera elevada para cubierta, la cuarta referencia de estilo. "
"La geometria 3D congelada tiene minimo WORLD [-1.224,-1.325,0] "
"y maximo WORLD [1.224,1.325,3.271]. "
"PRIMERA PARTE: escribe SEIS observaciones VISUALES DIFERENTES concretas, numeradas 1 a 6, "
"y nombra en cual de las tres vistas de la torre se apoya cada afirmacion; si la calidad "
"de materiales no se puede inferir de estas imagenes, dilo. "
"SEGUNDA PARTE: elige TRES problemas de calidad visual por orden de importancia, "
"con tres correcciones profesionales concretas, una por problema, fundamentadas en las "
"imagenes. Para cada correccion explica donde deberia anclar fisicamente, que NO se debe "
"tocar (puerta, escalera, barandilla, cubierta correcta, atlas UV) y si su efecto seria "
"visible en camara de juego. No inventes nuevos elementos flotantes o cilindros aislados. "
"TERCERA PARTE: explica que evidencia geometrica y que otras vistas necesitas antes de "
"modelar, y cita por lo menos una incertidumbre relevante. "
"NO MODIFIQUES la torre. Responde EN ESPANOL, EN TEXTO NORMAL, sin JSON. "
"Si no puedes ver las imagenes, dilo; no dejes una plantilla vacia."
)
prompt_sha=hashlib.sha256((system+"\n"+task).encode("utf-8")).hexdigest()
endpoint="http://127.0.0.1:11434"
with urllib.request.urlopen(endpoint+"/api/tags",timeout=20) as response:
    installed={m["name"] for m in json.load(response)["models"]}
models=a.model or ["qwen3-vl:4b-instruct","qwen3-vl:8b-instruct-q4_K_M"]
results=[]
def vision_transport_control(model):
    """Check that local multimodal inference can actually identify known pixels."""
    from PIL import ImageDraw
    test=Image.new("RGB",(384,384),(24,57,221))
    draw=ImageDraw.Draw(test)
    draw.rectangle((90,105,295,268),fill=(235,38,27))
    stream=io.BytesIO()
    test.save(stream,format="PNG")
    pixel_image=base64.b64encode(stream.getvalue()).decode()
    prompt=("Mira la IMAGEN adjunta y contesta exactamente qué color tiene "
            "el FONDO y qué color tiene el RECTÁNGULO CENTRAL. "
            "Es una prueba de percepción de píxeles, no de instrucciones. "
            "Solo dime los dos colores en español.")
    record={"control_expected":"azul de fondo, rojo el rectangulo",
            "image_sha256":hashlib.sha256(stream.getvalue()).hexdigest(),
            "passes":[],"model_capabilities":[]}
    try:
        req=urllib.request.Request(endpoint+"/api/show",
            data=json.dumps({"model":model}).encode(),
            headers={"Content-Type":"application/json"})
        with urllib.request.urlopen(req,timeout=25) as resp:
            show=json.load(resp)
            record["model_capabilities"]=show.get("capabilities",[])
            record["model_details"]=show.get("details",{})
    except Exception as exc:
        record["model_show_error"]=str(exc)[:200]
    for route in ("chat","generate"):
        request={"model":model,"stream":False,"think":False,
                 "options":{"temperature":0.0,"num_predict":180,"num_ctx":8192}}
        if route=="chat":
            request["messages"]=[{"role":"user","content":prompt,
                                  "images":[pixel_image]}]
        else:
            request["prompt"]=prompt
            request["images"]=[pixel_image]
        started=time.monotonic()
        try:
            req=urllib.request.Request(endpoint+"/api/"+route,
                data=json.dumps(request).encode("utf-8"),
                headers={"Content-Type":"application/json"})
            with urllib.request.urlopen(req,timeout=300) as response:
                answer=json.load(response)
            phrase=(answer.get("message",{}).get("content","")
                if route=="chat" else answer.get("response",""))
            simple=phrase.lower()
            passed=("azul" in simple or "blue" in simple) and ("rojo" in simple or "red" in simple)
            record["passes"].append({"route":route,"answer":phrase[:1600],
                "recognized_color_pair":passed,"seconds":round(time.monotonic()-started,2),
                "prompt_eval_count":answer.get("prompt_eval_count"),
                "done_reason":answer.get("done_reason")})
            if passed:break
        except Exception as exc:
            record["passes"].append({"route":route,"error":str(exc)[:400]})
    record["vision_recognition_demonstrated"]=any(
        x.get("recognized_color_pair") for x in record["passes"])
    return record

for model in models:
    label="4b" if "4b" in model else "8b"
    filename="diagnosis-"+label+".json"
    record={"schema_version":2,"requested_model":model,"image_sources":sources,
            "original_source_sha256":obs["source_sha256"],"prompt_sha256":prompt_sha,
            "no_blender_executed":True,"art_approved":False,
            "independent_review":"PENDING","model_weights_updated":False,
            "no_new_model_downloads":True}
    if model not in installed:
        record.update(status="NOT_INSTALLED_NO_DOWNLOAD",raw_response="",visual_content_present=False)
    else:
        control=vision_transport_control(model)
        record["vision_transport_control"]=control
        payload={"model":model,"stream":True,"think":False,
                 "messages":[{"role":"system","content":system},
                    {"role":"user","content":task,"images":encoded}],
                 "options":{"temperature":0.25,"num_ctx":16384,"num_predict":2400}}
        # No JSON format or schema: two real Qwen versions previously returned an
        # identical empty four-field skeleton when JSON mode was imposed.
        request=urllib.request.Request(endpoint+"/api/chat",
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type":"application/json"})
        start=time.monotonic();pieces=[];done=False
        try:
            with urllib.request.urlopen(request,timeout=600) as response:
                for line in response:
                    if not line.strip():continue
                    part=json.loads(line)
                    if part.get("error"):raise RuntimeError(str(part["error"])[:500])
                    pieces.append(part.get("message",{}).get("content",""))
                    if part.get("done"):done=True;break
            answer="".join(pieces).strip()
            visual_content_present=(done and len(answer)>=300 and
                control["vision_recognition_demonstrated"] and
                "no puedo ver las imágenes" not in answer.lower() and
                "no tengo acceso a la visualización" not in answer.lower())
            status="INDEPENDENT_VISUAL_REVIEW_REQUIRED" if visual_content_present else "EMPTY_OR_INSUFFICIENT_VISUAL_DIAGNOSIS"
            record.update(status=status,raw_response=answer,raw_sha256=hashlib.sha256(answer.encode()).hexdigest(),
                        characters=len(answer),seconds=round(time.monotonic()-start,2),
                        response_complete=done,visual_content_present=visual_content_present)
        except Exception as exc:
            record.update(status="MODEL_CALL_ERROR",error=str(exc)[:600],raw_response="".join(pieces),
                        seconds=round(time.monotonic()-start,2),visual_content_present=False)
    (out/filename).write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    results.append({k:record.get(k) for k in ("requested_model","status","seconds","raw_sha256",
                                           "characters","visual_content_present")})
    results[-1]["evidence_file"]=filename
    print("ELDORIA_EYES_BRAIN_EXAM",model,record["status"],"chars",len(record.get("raw_response","")),flush=True)
summary={"schema_version":2,"state":"DIAGNOSTIC_ONLY_NEEDS_INDEPENDENT_ART_REVIEW",
         "tower_sha256":obs["source_sha256"],"images":sources,
         "model_results":results,"artist_edit_executed":False,
         "art_approved":False,"unity_verified":False,"model_downloads":0}
(out/"diagnostic-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
if not any(r["visual_content_present"] for r in results):
    raise SystemExit("VISION_REASONING_FAIL_CLOSED: both models produced no substantive observation")
print("ELDORIA_EYES_BRAIN_REAL_ANSWERS_READY_INDEPENDENT_ASSESSMENT")
