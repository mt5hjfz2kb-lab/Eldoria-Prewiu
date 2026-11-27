"""Independent native visual audit of actual Issue200 camera frames only."""
import os,json,base64,hashlib
from decimal import Decimal as D
from pathlib import Path
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);native=root/"native";concepts=root/"concepts";refined=root/"refined";out=root/"audit";out.mkdir(exist_ok=True)
s=json.loads((native/"evidence-summary.json").read_text())
assert s["native_unity"] is True and s["matched_frames"]==96 and s["source_sha"]=="91b8de9874aed87219d733efa6e8e0aa56207f1e"
prior=json.loads((root/"prior-audit"/"summary.json").read_text())
assert prior["actual_unity_native"] is True and prior["target_met"] is False
cost=D(prior["total_estimated_usd"]);assert cost==D("0.6889613") and cost+D(".45")<=D("5")
def mosaic(names,path,maxw,maxh):
    items=[]
    for name,p in names:
        im=Image.open(p).convert("RGB");im.thumbnail((maxw,maxh));items.append((name,im))
    canvas=Image.new("RGB",(maxw*len(items),maxh+34),"#151515");draw=ImageDraw.Draw(canvas)
    for i,(name,im) in enumerate(items):
        canvas.paste(im,(i*maxw+(maxw-im.width)//2,34+(maxh-im.height)//2));draw.text((i*maxw+8,10),name,fill="white")
    canvas.save(path,quality=94)
mosaic([("Same camera home",native/"home-comparison.png"),("Same closer entry",native/"registered-entry-comparison.png")],out/"composition.jpg",780,900)
mosaic([("Keep native A B",native/"keep-close-comparison.png"),("Mill native A B",native/"mill-close-comparison.png"),("Approved conceptual B",concepts/"concept-B.jpg"),("Approved conceptual C",concepts/"concept-C.jpg")],out/"detail.jpg",600,600)
brief="Auditoria visual independiente de Bastión I Valoria. Las dos imágenes contienen capturas REALES de Unity en la misma cámara y mismo estado, baseline izquierda y candidato B+C V2 derecha. Este es un segundo intento de integración: juzga lo visible sin conocer notas anteriores, y las referencias conceptuales B y C al final de la segunda. Evalúa solo escena sin HUD: no confundir con juego publicado ni certificar dispositivo. Conserva el espacio de expansión, solo edificios canónicos; no penalizar por falta de edificios futuros. Puntúa cada versión por recursos, integración espacial, coherencia, composición, iluminación y materiales, legibilidad móvil, pulido, 0..10. Rúbrica pesos 20/20/15/15/10/10/10. Identifica concretamente si se siente como collage, acceso y contacto, si el aserradero reproduce C y el Bastión B, textura de suelo y escalas. No inflar notas por arte conceptual ni afirmar prueba comercial comparada. Devuelve exclusivamente JSON: {baseline:{resources,integration,coherence,composition,materials,readability,polish},candidate:{same},findings:[strings],recommendation:string}. Español."
content=[{"type":"input_text","text":brief}]
for p in (out/"composition.jpg",out/"detail.jpg"):content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(p.read_bytes()).decode(),"detail":"high"})
payload={"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":2400,"store":False}
(out/"reservation.json").write_text(json.dumps({"reserved_usd":"0.45","prior_estimated_usd":str(cost),"state":"in_flight"}))
raw=studio.request_api(payload,os.environ["OPENAI_API_KEY"]);u=raw["usage"];model=raw.get("model") or payload["model"];new=economics.estimate(model,u,raw.get("service_tier") or "standard")
(out/"receipt.json").write_text(json.dumps({"model":model,"usage":u,"estimated_usd":str(new),"total_estimated_usd":str(cost+new),"billing_verified":False}))
assert new<=D(".45") and raw.get("status")=="completed"
response="\n".join(c["text"] for i in raw.get("output",[]) if i.get("type")=="message" for c in i.get("content",[]) if c.get("type")=="output_text").strip()
(out/"audit.txt").write_text(response)
if response.startswith("```"):response=response.split("\n",1)[1].rsplit("```",1)[0]
audit=json.loads(response)
weights={"resources":20,"integration":20,"coherence":15,"composition":15,"materials":10,"readability":10,"polish":10}
scores={}
for k in ("baseline","candidate"):
    values=audit[k];assert set(values)==set(weights) and all(type(v) in (int,float) and 0<=v<=10 for v in values.values())
    scores[k]=round(sum(values[v]*weights[v] for v in weights)/100,3)
result={"source_sha":s["source_sha"],"actual_unity_native":True,"weighted_scores":scores,"target_met":scores["candidate"]>=8 and scores["candidate"]>scores["baseline"],"audit":audit,"total_estimated_usd":str(cost+new),"art_approved":False,"physical_device":False,"playmode":False}
(out/"summary.json").write_text(json.dumps(result,indent=2,ensure_ascii=False));print("ISSUE200_NATIVE_AUDIT "+json.dumps(result,ensure_ascii=False))
