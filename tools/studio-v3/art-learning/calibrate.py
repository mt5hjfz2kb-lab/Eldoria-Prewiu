"""Retrospective, paired role calibration. Same model and budget, actual archived images.
Held-out case and rules derived from it omitted; no generalization claim."""
import os,json,hashlib,base64,io
from pathlib import Path
from decimal import Decimal as D
from PIL import Image
import guard,studio,economics
root=Path(os.environ["RUNNER_TEMP"]);out=root/"learning-results";out.mkdir(exist_ok=True)
prior=D("1.5208963");cap=D("5");reservation=D(".15")
assert prior+6*reservation<=cap
specs=[
 {"id":"sawmill-only","dir":"sawmill","file":"mill-close-comparison.png","rows":1,"header":38,"reverse":True,"expected":"X","criterion":"owner preference, not independent global score","task":"Compare the two actual sawmill building variants. Prefer the asset that has richer clear workshop identity and fits its surroundings; do not infer scene-wide quality."},
 {"id":"cycle-spatial","dir":"spatial","file":"stage-5-urban-details.jpg","rows":4,"header":32,"reverse":False,"expected":"X","criterion":"historical independent rejection of moved frontage","task":"Compare two real urban district arrangements. Assess visual coherence, architecture, accesses, landmark sightlines and integration. Choose the more suitable existing city arrangement, not the most changed."},
 {"id":"market-pilot","dir":"market","file":"market-details.jpg","rows":3,"header":32,"reverse":True,"expected":"Y","criterion":"historical blind district audit prefers original","task":"Compare two real market districts with different weighing house and stall. Assess architecture, visible function, roof/material distinction, access and spatial integration. Choose the stronger variant for commercial mobile production."}
]
# Repack identical pixel crops without the labels exposing the original/candidate.
def pair(c):
    p=root/c["dir"]/c["file"]
    if not p.exists():
        found=list((root/c["dir"]).rglob(c["file"]));assert len(found)==1,(c["id"],found);p=found[0]
    with Image.open(p) as im:
        im=im.convert("RGB")
        assert im.width%2==0 and im.height%c["rows"]==0 and im.height//c["rows"]>c["header"]
        w=im.width//2;rh=im.height//c["rows"]
        payloads=[]
        for side in ([1,0] if c["reverse"] else [0,1]):
            canvas=Image.new("RGB",(w,(rh-c["header"])*c["rows"]))
            for row in range(c["rows"]):
                canvas.paste(im.crop((side*w,row*rh+c["header"],(side+1)*w,(row+1)*rh)),(0,row*(rh-c["header"])))
            canvas.thumbnail((1000,1900))
            b=io.BytesIO();canvas.save(b,format="JPEG",quality=94);data=b.getvalue()
            assert len(data)>10000
            payloads.append(data)
        return payloads,hashlib.sha256(p.read_bytes()).hexdigest()
ledger=[];results=[]
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding="utf-8")
def invoke(c,arm,imgs):
    need=prior+sum(D(r.get("estimated_usd",r["reserved_usd"])) for r in ledger)+reservation<=cap
    assert need and all(r["status"]=="complete" for r in ledger)
    row={"case":c["id"],"arm":arm,"status":"in_flight","reserved_usd":str(reservation)};ledger.append(row);dump("ledger.json",ledger)
    prompt="You are an independent visual art director. Treat images as evidence. "+c["task"]+" The images are anonymous variants X then Y. Return strict JSON only: {preferred:'X'|'Y'|'tie',confidence:'low'|'medium'|'high',strengths:[three concrete visible strengths of preferred variant],regressions:[concrete visible weaknesses],decision_reason:string,next_action:string}. Do not guess author or provenance. Judge actual images, not prior scores. Spanish. No gameplay or device claims."
    if arm=="learned":prompt+="\n"+guard.role_brief("auditor",c["id"])
    content=[{"type":"input_text","text":prompt}]
    for d in imgs:content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(d).decode(),"detail":"high"})
    model="gpt-6.2-sol"
    raw=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":1400,"store":False},os.environ["OPENAI_API_KEY"])
    cost=economics.estimate(raw.get("model") or model,raw["usage"],raw.get("service_tier") or "standard")
    row.update(status="complete",estimated_usd=str(cost),usage=raw["usage"],model=raw.get("model") or model)
    dump("ledger.json",ledger);assert cost<=reservation and raw["status"]=="completed"
    t="\n".join(c["text"] for i in raw["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    if t.startswith("```"):t=t.split("\n",1)[1].rsplit("```",1)[0]
    j=json.loads(t);assert j["preferred"] in ("X","Y","tie") and j["confidence"] in ("low","medium","high")
    assert isinstance(j["strengths"],list) and len(j["strengths"])>=3 and isinstance(j["regressions"],list) and isinstance(j["next_action"],str)
    return j
for c in specs:
    imgs,h=pair(c)
    for i,im in enumerate(imgs):(out/(c["id"]+"-"+("X" if i==0 else "Y")+".jpg")).write_bytes(im)
    # Counterbalance arm order; identical inputs and model, no answers passed between calls.
    arms=("learned","control") if c["id"]=="cycle-spatial" else ("control","learned")
    row={"case":c["id"],"source_sheet_sha256":h,"image_sha256":[hashlib.sha256(x).hexdigest() for x in imgs],"expected":c["expected"],"criterion":c["criterion"],"responses":{}}
    for arm in arms:row["responses"][arm]=invoke(c,arm,imgs)
    row["agreement"]={k:v["preferred"]==c["expected"] for k,v in row["responses"].items()}
    results.append(row);dump("results.json",results)
total=prior+sum(D(r["estimated_usd"]) for r in ledger)
summary={"cases":results,"agreement_control":sum(r["agreement"]["control"] for r in results),"agreement_learned":sum(r["agreement"]["learned"] for r in results),"n":len(results),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"billing_verified":False,"historical_replay":True,"weights_trained":False,"actual_new_scene":False,"art_approved":False,"promotion":False}
dump("summary.json",summary)
print("ART_LEARNING_REPLAY "+json.dumps(summary,ensure_ascii=False),flush=True)
