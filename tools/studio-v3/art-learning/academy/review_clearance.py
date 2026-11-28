"""Independent review of measured-clearance repair with original lab control.
Adaptive same-case exercise, not generalization or Unity certification."""
import os,json,hashlib,base64,secrets
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);out=root/"clearance-results";out.mkdir(exist_ok=True)
p=json.loads((out/"summary.json").read_text());prior=D(p["estimated_total_usd"]);assert prior==D("1.8619108") and prior+D(".20")<=D("5")
original=out/"control-courtyard.png";candidate=out/"corrected-courtyard.png";assert hashlib.sha256(original.read_bytes()).hexdigest()==p["rendered_images_sha256"][original.name]
m=json.loads((out/"clearance-metrics.json").read_text());assert m["camera_unchanged"]
mapping={"X":"corrected","Y":"original"} if secrets.randbelow(2) else {"X":"original","Y":"corrected"}
images={"original":original,"corrected":candidate}
def sheet(name,labels,order):
    canv=Image.new("RGB",(1320,900),(31,33,35));d=ImageDraw.Draw(canv)
    for i,k in enumerate(order):
        with Image.open(images[k]) as im:
            im=im.convert("RGB");assert im.size==(660,660)
            canv.paste(im,(i*660,30));im.thumbnail((190,190));canv.paste(im,(i*660+235,700))
        d.text((i*660+10,10),labels[i],fill="white")
    canv.save(out/name,quality=94)
sheet("clearance-blind.jpg",["X","Y"],[mapping["X"],mapping["Y"]])
sheet("clearance-comparison.jpg",["Original","Measured correction"],["original","corrected"])
prompt="Blind review of two actual renders of the same Blender courtyard exercise. Only positioning of well and cart differs; not a game or Unity asset. Compare visual separation, legibility of well and cart at reduced size, uncluttered path, main building hierarchy, natural placement and overall composition. Choose original if extra spacing harms cohesion. Rate each 0..10 using same criteria. JSON {X:number,Y:number,preferred:'X'|'Y'|'tie',evidence:string,remaining_defects:[strings]}. Spanish."
ledger=out/"clearance-ledger.json";ledger.write_text(json.dumps({"status":"in_flight","reserved_usd":".20"}))
r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":[{"type":"input_text","text":prompt},{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode((out/"clearance-blind.jpg").read_bytes()).decode(),"detail":"high"}]}],"reasoning":{"effort":"low"},"max_output_tokens":1200,"store":False},os.environ["OPENAI_API_KEY"])
cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard")
ledger.write_text(json.dumps({"status":"complete","estimated_usd":str(cost),"usage":r["usage"]}))
assert cost<=D(".20") and r["status"]=="completed"
s="\n".join(x["text"] for y in r["output"] if y.get("type")=="message" for x in y["content"] if x.get("type")=="output_text").strip()
if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
j=json.loads(s);assert all(type(j[k]) in (int,float) and 0<=j[k]<=10 for k in ("X","Y")) and j["preferred"] in ("X","Y","tie")
scores={mapping[k]:j[k] for k in mapping}
summary={"scores":scores,"delta":round(scores["corrected"]-scores["original"],2),"preferred":mapping.get(j["preferred"],"tie"),"audit":j,"mapping":mapping,"metrics":m,"candidate_sha256":hashlib.sha256(candidate.read_bytes()).hexdigest(),"estimated_new_usd":str(cost),"estimated_total_usd":str(prior+cost),"adaptive_same_case":True,"game_modified":False,"unity_native":False,"billing_verified":False}
(out/"clearance-summary.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2))
print("CLEARANCE_REVIEW_FINAL",json.dumps(summary,ensure_ascii=False),flush=True)
