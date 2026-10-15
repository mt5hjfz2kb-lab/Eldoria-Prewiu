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

def single_tower_image_probe(model):
    """Separate single-image real tower identification from 4-image art reasoning."""
    picture=images[0]
    prompt=("Observa esta única fotografía. Describe EN TRES FRASES qué construcción "
            "se representa, la forma de su parte superior y dos elementos concretos "
            "que estén visibles en uno de sus laterales. No inventes elementos ocultos.")
    request_data={"model":model,"stream":False,"think":False,
         "messages":[{"role":"user","content":prompt,"images":[img_b64(picture)]}],
         "options":{"temperature":0.0,"num_predict":370,"num_ctx":8192}}
    req=urllib.request.Request(endpoint+"/api/chat",
            data=json.dumps(request_data).encode("utf-8"),
            headers={"Content-Type":"application/json"})
    with urllib.request.urlopen(req,timeout=300) as response:
        body=json.load(response)
    answer=body.get("message",{}).get("content","").strip()
    content=answer.casefold()
    is_refusal=any(s in content for s in ["no puedo ver", "no tengo acceso a la visualiz", "sin las imágenes", "sin las imagenes"])
    specific=any(w in content for w in ["tejado","techo","cubierta","torre","cabaña","caseta"]) and any(
       w in content for w in ["escalera","barandilla","madera","tronco","tablón","pared","escalones"])
    return {"image_sha256":sources[0]["sha256"],"raw_response":answer,
        "raw_sha256":hashlib.sha256(answer.encode()).hexdigest(),
        "characters":len(answer),"source_image_recognition_plausible":bool(
          len(answer)>75 and not is_refusal and specific),
        "refusal":is_refusal,"done_reason":body.get("done_reason")}

def ask_model(model,question,image64=None,max_tokens=900):
    # First look at ONE actual image. Ask higher-level art questions later,
    # using captions the vision model genuinely created itself.
    message={"role":"user","content":question}
    if image64 is not None:message["images"]=[image64]
    payload={"model":model,"stream":False,"think":False,"messages":[message],
             "options":{"temperature":0.20,"num_ctx":8192,"num_predict":max_tokens}}
    request=urllib.request.Request(endpoint+"/api/chat",
        data=json.dumps(payload).encode("utf-8"),headers={"Content-Type":"application/json"})
    started=time.monotonic()
    with urllib.request.urlopen(request,timeout=600) as response:
        data=json.load(response)
    answer=data.get("message",{}).get("content","").strip()
    return {"text":answer,"complete":bool(data.get("done")),
            "seconds":round(time.monotonic()-started,2),
            "prompt_eval_count":data.get("prompt_eval_count"),
            "eval_count":data.get("eval_count"),
            "sha256":hashlib.sha256(answer.encode("utf-8")).hexdigest()}

camera_questions=[
 ("front_three_quarter",
  "Describe solamente lo visible en esta imagen de una construcción medieval 3D. "
  "¿Qué forma tienen sus paredes, qué forma tiene el tejado y qué elementos aparecen "
  "al lado derecho? Da cuatro observaciones concretas, sin comentar tus capacidades."),
 ("reverse_three_quarter",
  "Observa esta imagen. Describe la forma y estructura del edificio, el tejado, "
  "la escalera y las barandillas si las ves, con sus posiciones y conexiones. "
  "Responde con cuatro frases concretas sobre la imagen."),
 ("elevated_roof",
  "Esta imagen está tomada desde arriba de un edificio medieval 3D. "
  "Describe solo qué ves en el tejado: forma, tablones, cumbrera y encuentros "
  "con otros elementos visibles. Describe cuatro detalles concretos."),
 ("approved_style_reference",
  "Describe el estilo artístico, los materiales aparentes y la paleta de esta imagen "
  "de referencia para un videojuego de fantasía medieval. Nombra cuatro rasgos visuales "
  "concretos; no inventes nada fuera de la imagen.")
]
def usable_text(x):
    txt=x["text"].lower()
    refusals=("no puedo ver","no tengo acceso a las im","no puedo visualizar",
              "no puedo observar","no tengo capacidad para ver")
    return x["complete"] and len(txt)>=110 and not any(z in txt for z in refusals)

for model in models:
    label="4b" if "4b" in model else "8b"
    filename="diagnosis-"+label+".json"
    record={"schema_version":3,"requested_model":model,"image_sources":sources,
            "original_source_sha256":obs["source_sha256"],"prompt_sha256":prompt_sha,
            "no_blender_executed":True,"art_approved":False,
            "independent_review":"PENDING","model_weights_updated":False,
            "no_new_model_downloads":True,"test_method":"ONE_IMAGE_AT_A_TIME_THEN_REASON_FROM_OWN_CAPTIONS"}
    began=time.monotonic()
    if model not in installed:
        record.update(status="NOT_INSTALLED_NO_DOWNLOAD",raw_response="",visual_content_present=False)
    else:
        try:
            control=vision_transport_control(model)
            record["vision_transport_control"]=control
            captions=[]
            for i,(view,question) in enumerate(camera_questions):
                result=ask_model(model,question,encoded[i],max_tokens=780)
                result["view"]=view
                result["visually_substantive"]=usable_text(result)
                captions.append(result)
                print("ELDORIA_SINGLE_VIEW",model,view,"chars",len(result["text"]),
                      "usable",result["visually_substantive"],flush=True)
            record["single_image_observations"]=captions
            observational_text="\\n".join(
                "VISTA "+r["view"]+" (observación tentativa): "+r["text"]
                for r in captions)
            reasoning=(
                "Eres director de arte 3D. Has observado por separado las tres vistas de "
                "la MISMA torre original de Valoria y una cuarta imagen de REFERENCIA "
                "DE ESTILO. Estos son tus propios informes de observación anteriores:\\n"
                +observational_text+"\\n"
                "Geometría original WORLD XYZ (Z arriba): "
                "min[-1.224,-1.325,0] max[1.224,1.325,3.271]. "
                "No modifiques Blender. Señala en orden los TRES defectos visuales "
                "con mayor impacto realmente sustentados en esas observaciones. "
                "Para cada uno: evidencia y vista; posible causa; corrección física "
                "con apoyo estructural real; mejora visible desde la cámara de juego. "
                "Preserva puerta, escalera, barandillas y UV. "
                "Separa cuidadosamente hechos vistos de hipótesis. "
                "Di qué no puedes asegurar sin más vistas. Responde con detalle en español."
            )
            diagnosis=ask_model(model,reasoning,max_tokens=2100)
            record["brain_reasoning"]=diagnosis
            answer=diagnosis["text"]
            num_tower_views=sum(x["visually_substantive"] for x in captions[:3])
            status_ok=(control["vision_recognition_demonstrated"] and
                num_tower_views>=2 and usable_text(diagnosis) and len(answer)>=430)
            record.update(status=("INDEPENDENT_VISUAL_REVIEW_REQUIRED" if status_ok
                    else "INSUFFICIENT_SINGLE_VIEW_OR_REASONING"),
                    raw_response=answer,raw_sha256=diagnosis["sha256"],
                    characters=len(answer),
                    seconds=round(time.monotonic()-began,2),
                    tower_views_with_substantive_description=num_tower_views,
                    response_complete=diagnosis["complete"],
                    visual_content_present=bool(status_ok))
        except Exception as exc:
            record.update(status="MODEL_CALL_ERROR",error=str(exc)[:600],
                          seconds=round(time.monotonic()-began,2),
                          visual_content_present=False)
    (out/filename).write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\\n",encoding="utf-8")
    results.append({k:record.get(k) for k in ("requested_model","status","seconds","raw_sha256",
        "characters","visual_content_present","tower_views_with_substantive_description")})
    results[-1]["evidence_file"]=filename
    print("ELDORIA_EYES_BRAIN_EXAM",model,record["status"],"chars",
          len(record.get("raw_response","")),flush=True)
summary={"schema_version":2,"state":"DIAGNOSTIC_ONLY_NEEDS_INDEPENDENT_ART_REVIEW",
         "tower_sha256":obs["source_sha256"],"images":sources,
         "model_results":results,"artist_edit_executed":False,
         "art_approved":False,"unity_verified":False,"model_downloads":0}
(out/"diagnostic-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
if not any(r["visual_content_present"] for r in results):
    raise SystemExit("VISION_REASONING_FAIL_CLOSED: both models produced no substantive observation")
print("ELDORIA_EYES_BRAIN_REAL_ANSWERS_READY_INDEPENDENT_ASSESSMENT")
