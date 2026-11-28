"""Retrospective test of typed protection gate using exact same anonymous images.
No new art generation, no Unity, no promotion."""
import os,json,hashlib,base64
from pathlib import Path
from decimal import Decimal as D
import studio,economics,guard
from decision_gate import arbitrate,PROTECTED
root=Path(os.environ["RUNNER_TEMP"]);src=root/"prior";out=root/"learning-veto";out.mkdir(exist_ok=True)
p=json.loads((src/"summary.json").read_text());prior=D(p["estimated_total_usd"])
assert prior==D("1.5841073") and prior+D(".30")<=D("5")
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False),encoding="utf-8")
ledger=[];rows=[]
# Owner selection is a protected product decision, not an accuracy label.
rows.append({"case":"sawmill-only","reference":"explicit owner selection; not model validation","gate":arbitrate({},"Y","X","X"),"expected":"X","model_called":False})
for c,baseline,candidate in [("cycle-spatial","X","Y"),("market-pilot","Y","X")]:
    old=next(r for r in p["cases"] if r["case"]==c)
    imgs=[(src/(c+"-"+n+".jpg")).read_bytes() for n in ("X","Y")]
    assert [hashlib.sha256(x).hexdigest() for x in imgs]==old["image_sha256"]
    assert prior+sum(D(x["estimated_usd"]) for x in ledger)+D(".15")<=D("5")
    row={"case":c,"status":"in_flight","reserved_usd":".15"};ledger.append(row);dump("ledger.json",ledger)
    prompt="Independent visual reviewer of actual matched Valoria Unity market images X, then Y. Do not infer which is original. Grade each protected feature individually for BOTH variants: good, minor_defect, material_defect, uncertain. Material defect means clearly lost or obscured distinctive architecture, a landmark (including the well), identifiable building function, accessible threshold/path, integration, or mobile legibility. A partial obstruction of a landmark that substantially reduces its recognizable silhouette counts as material, not merely a preference. Do not trade loss of one protected feature for another. Existing defects may affect both. Evidence must identify concrete visible differences; never infer invisible details. Return strict JSON {preferred:'X'|'Y'|'tie',protected_features:{"+",".join(PROTECTED)+"}}; every feature has exactly {X:severity,Y:severity,evidence:string}. No other keys. Spanish evidence."
    prompt+="\n"+guard.role_brief("auditor",c)
    content=[{"type":"input_text","text":prompt}]+[{"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(x).decode(),"detail":"high"} for x in imgs]
    model="gpt-6.2-sol"
    r=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":2200,"store":False},os.environ["OPENAI_API_KEY"])
    cost=economics.estimate(r.get("model") or model,r["usage"],r.get("service_tier") or "standard")
    row.update(status="complete",estimated_usd=str(cost),usage=r["usage"]);dump("ledger.json",ledger)
    assert cost<=D(".15") and r["status"]=="completed"
    t="\n".join(z["text"] for i in r["output"] if i.get("type")=="message" for z in i["content"] if z.get("type")=="output_text").strip()
    if t.startswith("```"):t=t.split("\n",1)[1].rsplit("```",1)[0]
    verdict=json.loads(t);decision=arbitrate(verdict,baseline,candidate)
    rows.append({"case":c,"expected":baseline,"model_called":True,"judgment":verdict,"gate":decision,"agrees":decision["selected"]==baseline,"prior_preferred":old["responses"]["learned"]["preferred"]})
    dump("results.json",rows)
total=prior+sum(D(x["estimated_usd"]) for x in ledger)
summary={"results":rows,"historical_replay":True,"actual_model_calls":2,"gate_agreement":sum(r["gate"]["selected"]==r["expected"] for r in rows),"n":3,"owner_lock_count":1,"model_only_cases":2,"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"billing_verified":False,"weights_trained":False,"new_scene":False,"promoted":False}
dump("summary.json",summary);print("ART_VETO_REPLAY "+json.dumps(summary,ensure_ascii=False),flush=True)
