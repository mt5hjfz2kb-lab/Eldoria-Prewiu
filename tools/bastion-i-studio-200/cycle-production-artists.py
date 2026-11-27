"""Issue200 artists: choose bounded, implementable surface tuning parameters from their own audit.
Actual values are consumed by the Unity candidate patch. No arbitrary model code executed."""
import os,json,base64
from decimal import Decimal as D
from pathlib import Path
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);src=root/"audit";out=root/"cycle-production";out.mkdir(exist_ok=True)
a=json.loads((src/"summary.json").read_text());prior=D(a["estimated_total_usd"])
assert a["source_head"]=="f27f4774e32efc5ad046478a07747eead4256c3e" and prior+D("1.10")<=D(5)
assert all(x["status"]=="complete" for x in json.loads((src/"ledger.json").read_text()))
bounds={"gate":(.55,.95),"road":(.55,.90),"mill":(.60,.95),"contacts":(.65,1.0),"meadow":(.35,.75),"postern":(.60,.95),"doors":(.55,.90),"junction":(.55,.90),"freight":(.60,.95)}
ledger=[]
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False))
def call(role,keys,brief,imgs):
    slot={"role":role,"status":"in_flight","reserved_usd":"0.35"};ledger.append(slot);dump("ledger.json",ledger)
    content=[{"type":"input_text","text":brief+"\nReturn JSON {values:{"+",".join(keys)+"},rationale:{same keys:Spanish explanation},risks:[strings]}. Each value is a numeric strength within the exact range: "+json.dumps({k:bounds[k] for k in keys})+". 0 is original, 1 is the maximum proposed correction, not opacity. Choose subtle but visible strengths using actual audit. No code, no new assets."}]
    for p in imgs:content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(p.read_bytes()).decode(),"detail":"high"})
    model="gpt-6.2-sol";r=studio.request_api({"model":model,"input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":1800,"store":False},os.environ["OPENAI_API_KEY"])
    cost=economics.estimate(r.get("model") or model,r["usage"],r.get("service_tier") or "standard");slot.update(status="complete",estimated_usd=str(cost),usage=r["usage"]);dump("ledger.json",ledger)
    assert cost<=D(".35") and r["status"]=="completed"
    s="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    (out/(role+".txt")).write_text(s)
    if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
    j=json.loads(s);assert set(j["values"])==set(keys) and set(j["rationale"])==set(keys)
    for k,v in j["values"].items():assert type(v) in (float,int) and bounds[k][0]<=v<=bounds[k][1]
    dump(role+".json",j);return j["values"]
context="You are producing the next real native Unity candidate for Valoria. Existing audit: "+json.dumps(a["director"],ensure_ascii=False)[:22000]+"\nKeep geometry, approved sawmill C, empty growth areas, independent sprites. Historical II-V must be freshly captured after implementation. Existing shader uses continuous ground meshes, projected building alpha shadows, road ribbons. Proposed changes are parameterized and deterministic."
v={}
v.update(call("surface-artist",["gate","road","junction","freight"],context+"\nGate strength mixes more readable stone at existing door, road strength narrows stone core and softens shoulders, junction strength distinguishes old civic axis from lateral branches, freight strength adds earth-to-gravel taper along existing approaches.",[src/"cycle-overview.jpg",src/"stage-5-details.jpg"]))
v.update(call("environment-artist",["mill","contacts","meadow"],context+"\nMill strengthens worn soil and muted sawdust only on completed workshop, contacts strengthen existing projected alpha at the foot (no extra oval), meadow blends modest nonperiodic broad pigment.",[src/"current-mill.jpg",src/"stage-1-overview.jpg"]))
v.update(call("urban-artist",["postern","doors"],context+"\nPostern strengthens short worn lane under existing arch without changing occlusion; doors controls localized thresholds for existing facade feet, well approach, and Adarve transition. No continuous slabs or new geometry.",[src/"stage-5-details.jpg",src/"stage-4-details.jpg"]))
total=prior+sum(D(x["estimated_usd"]) for x in ledger)
assert set(v)==set(bounds) and total<=D(5)
dump("parameters.json",{"source_head":a["source_head"],"values":v,"estimated_prior_usd":str(prior),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"billing_verified":False,"calls":3})
print("ARTIST_PRODUCTION_PARAMETERS "+json.dumps(v),flush=True)
