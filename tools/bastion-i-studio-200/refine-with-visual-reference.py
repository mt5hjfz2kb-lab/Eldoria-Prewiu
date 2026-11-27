"""Reference-conditioned second pass: no city-sheet extraction; independent re-authored PNGs.
Uses the official image edit endpoint, with a crop of the approved concept as visual input only.
"""
from pathlib import Path
import os,json,base64,hashlib,shutil
from decimal import Decimal as D
from PIL import Image,ImageDraw
import numpy as np
from openai import OpenAI
import economics,studio
root=Path(os.environ["RUNNER_TEMP"]);src=root/"concepts";previous=root/"previous";out=root/"refined"
out.mkdir(exist_ok=True)
prior=json.loads((previous/"summary.json").read_text()); assert D(prior["total_estimated_usd"])==D("0.5485698")
assert all(x["status"]=="complete" for x in json.loads((previous/"ledger.json").read_text()))
client=OpenAI(max_retries=0,timeout=180)
ledger=[]
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False))
def begin(n,c):
    assert len(ledger)<3 and all(x["status"]=="complete" for x in ledger)
    assert D(prior["total_estimated_usd"])+sum(D(x["reserved"]) for x in ledger)+D(c)<=D("5")
    x={"slot":n,"reserved":c,"status":"in_flight"};ledger.append(x);dump("ledger.json",ledger);print("START "+n,flush=True);return x
def finish(x,u,model,tier="standard"):
    cost=economics.estimate(model,u,tier);x.update(status="complete",estimated_usd=str(cost),usage=u,model=model);dump("ledger.json",ledger)
    assert cost<=D(x["reserved"]);print("DONE "+x["slot"]+" "+str(cost),flush=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
manifest={}
for name,letter,coords,brief in [
 ("bastion","B",(400,145,875,545),"Exact round dominant tower at the upper-left of castle, conical roof, round front flanking towers, front-facing timber gate at center; preserve visible facade proportions and view angle. Do not substitute square towers or rotate the gate. Remove ONLY surrounding nature, dirt, lumber piles, characters. Re-author high resolution architectural sprite with all hidden masonry bases reconstructed; no cast shadow or ground, no extraneous decor."),
 ("sawmill","C",(20,425,386,700),"Exact open-framed sawmill at left, low slate roofs, exposed braced posts and partly roofless frame on left, canvas awning at right, attached saw bench and log piles. Preserve the reference proportions, orientation and silhouette. Remove the separate rear shed and other buildings, people and ground. Re-author high resolution building only; retain the open spaces in the timber frame, no base slab or cast shadow.")]:
    original=Image.open(src/("concept-"+letter+".png")).convert("RGB")
    assert original.size==(1024,1024)
    ref=out/(name+"-reference-only.png");original.crop(coords).save(ref)
    row=begin(name,"1.15")
    with ref.open("rb") as im:
        rsp=client.images.edit(model="gpt-image-2.6-flare",image=im,prompt="Create a new original high-detail production game sprite from the supplied approved visual reference. "+brief+" Match fixed orthographic 2.5D parallel edges and upper-left warm lighting. Completely transparent PNG background, no matte fringe; complete silhouette with generous safe margins. No words, labels, watermark, terrain or other buildings. Keep same visible building design, not a reinterpretation.",size="1024x1024",quality="high",background="transparent",output_format="png",n=1)
    raw=rsp.model_dump(exclude_none=True);finish(row,raw["usage"],"gpt-image-2.6-flare")
    assert len(raw["data"])==1
    data=base64.b64decode(raw["data"][0]["b64_json"],validate=True)
    orig=out/(name+"-original.png");orig.write_bytes(data)
    with Image.open(orig) as im:
        assert im.mode=="RGBA","Explicit transparent output absent"
        alpha=im.getchannel("A");b=alpha.point(lambda x:255 if x>128 else 0).getbbox()
        assert b and b[0]>3 and b[1]>3 and b[2]<im.width-3 and b[3]<im.height-3 and alpha.getextrema()[0]==0 and alpha.getextrema()[1]>=240
        subject=im.crop(b);subject.save(out/(name+".png"))
    manifest[name]={"sha256":sha(out/(name+".png")),"original_sha256":sha(orig),"reference_sha256":sha(ref),"width":subject.width,"height":subject.height,"native_alpha_preserved":True,"art_approved":False}
    dump("asset-manifest.json",manifest)
shutil.copyfile(previous/"meadow.png",out/"meadow.png")
manifest["meadow"]=json.loads((previous/"asset-manifest.json").read_text())["meadow"]
assert manifest["meadow"]["sha256"]==sha(out/"meadow.png")
dump("asset-manifest.json",manifest)
sheet=Image.new("RGB",(1100,820),(38,43,47));d=ImageDraw.Draw(sheet)
for i,name in enumerate(("bastion","sawmill")):
    for j,p in enumerate([out/(name+"-reference-only.png"),out/(name+".png")]):
        with Image.open(p) as im:
            im=im.convert("RGBA");im.thumbnail((530,385));sheet.paste(im,(i*550+(550-im.width)//2,j*410+20),im)
        d.text((i*550+5,j*410+4),name+" "+("reference" if j==0 else "new independent sprite"),fill="white")
sheet.save(out/"asset-reference-comparison.jpg",quality=94)
row=begin("independent-audit","0.45")
brief="Evalúa en español la fidelidad del Bastión (izquierda) y aserradero (derecha). Fila superior = recorte de referencia aprobado B/C, solo referencia; fila inferior = nuevos sprites independientes de producción. Comprueba perspectiva, forma, proporción, acceso, sombra y ausencia de terreno. Valora si ameritan prueba A/B real Unity, no afirmar que el juego ha mejorado ni asignar nota final. Devuelve JSON con bastion y sawmill cada uno {score:0..10,merits_native_ab:boolean,defects:string} y recomendacion. No rechaces por no mostrar la escena completa: estos son sprites y no capturas de juego."
payload={"model":"gpt-6.2-sol","input":[{"role":"user","content":[{"type":"input_text","text":brief},{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode((out/"asset-reference-comparison.jpg").read_bytes()).decode(),"detail":"high"}]}],"reasoning":{"effort":"low"},"max_output_tokens":1600,"store":False}
raw=studio.request_api(payload,os.environ["OPENAI_API_KEY"]);finish(row,raw["usage"],raw.get("model") or payload["model"],raw.get("service_tier") or "standard")
assert raw.get("status")=="completed"
s="\n".join(c["text"] for i in raw.get("output",[]) if i.get("type")=="message" for c in i.get("content",[]) if c.get("type")=="output_text").strip()
(out/"independent-audit.txt").write_text(s)
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
audit=json.loads(s)
gate=all(audit[n]["merits_native_ab"] is True for n in ("bastion","sawmill"))
dump("summary.json",{"total_estimated_usd":str(D(prior["total_estimated_usd"])+sum(D(x["estimated_usd"]) for x in ledger)),"prior_estimated_usd":prior["total_estimated_usd"],"calls":len(ledger),"asset_gate_for_native_ab":gate,"audit":audit,"native_unity":False,"art_approved":False,"billing_verified":False})
