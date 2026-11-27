"""Second intervention: actual placement and road mesh hierarchy, not more subtle recoloring."""
import os,json,base64
from decimal import Decimal as D
from pathlib import Path
import studio,economics
root=Path(os.environ["RUNNER_TEMP"]);src=root/"previous";out=root/"cycle-production";out.mkdir(exist_ok=True)
a=json.loads((src/"summary.json").read_text());prior=D(a["total_estimated_usd"])
assert prior==D("1.1670638") and a["mean_before"]==a["mean_after"]==8.21
assert prior+D(".70")+D("1.75")<=D(5)
bounds={"peso_dx":(-.70,-.25),"peso_dy":(.12,.45),"weavers_dx":(.18,.45),"weavers_dy":(.10,.30),"fence_dx":(.10,.35),"fence_dy":(-.35,-.10),"gate_width":(1.45,1.75),"mill_width":(.85,1.10),"freight_width":(1.20,1.45),"branch_width":(.82,1.04),"postern_width":(.78,.96),"core_ink":(.80,.98)}
ledger=[]
def dump(n,x):(out/n).write_text(json.dumps(x,indent=2,ensure_ascii=False))
def call(role,keys,brief,names):
    row={"role":role,"reserved_usd":".35","status":"in_flight"};ledger.append(row);dump("ledger.json",ledger)
    content=[{"type":"input_text","text":brief+"\nReturn JSON {values:{"+",".join(keys)+"},rationale:{same keys:Spanish explanation},risks:[strings]}. Numeric bounds: "+json.dumps({k:bounds[k] for k in keys})+". No new resources, no code. Values are actual world-space units or alpha ink."}]
    for n in names:
        p=src/n;assert p.is_file() and p.stat().st_size>10000
        content.append({"type":"input_image","image_url":"data:image/jpeg;base64,"+base64.b64encode(p.read_bytes()).decode(),"detail":"high"})
    r=studio.request_api({"model":"gpt-6.2-sol","input":[{"role":"user","content":content}],"reasoning":{"effort":"low"},"max_output_tokens":2000,"store":False},os.environ["OPENAI_API_KEY"])
    cost=economics.estimate(r.get("model") or "gpt-6.2-sol",r["usage"],r.get("service_tier") or "standard");row.update(status="complete",estimated_usd=str(cost),usage=r["usage"]);dump("ledger.json",ledger)
    assert r["status"]=="completed" and cost<=D(".35")
    s="\n".join(c["text"] for i in r["output"] if i.get("type")=="message" for c in i["content"] if c.get("type")=="output_text").strip()
    if s.startswith("```"):s=s.split("\n",1)[1].rsplit("```",1)[0]
    j=json.loads(s);assert set(j["values"])==set(keys)
    for k,v in j["values"].items():assert type(v) in (int,float) and bounds[k][0]<=v<=bounds[k][1]
    dump(role+".json",j);return j["values"]
context="The previous 9 shader parameters achieved ZERO mean improvement in 5 blind native A/B audits: 8.21 -> 8.21. Do not repeat the subtle approach. Actual images A=original B=first candidate, same camera. Only environment composition, no changing actual buildings, pivots, canonical gameplay routes, expansion reservations or approved sawmill C. "
v={}
v.update(call("spatial-artist",["peso_dx","peso_dy","weavers_dx","weavers_dy","fence_dx","fence_dy"],context+"Choose small but perceptible whole-sprite foot offsets: CasasDelPeso toward well (left/up) to separate granary silo from entrance, IV FrenteTejedores closer to street (right/up), VallaLenador away from cart and worker (right/down). Existing anchors relative to MarketRow (0,0), Junction (-2.15,-1.57), Mill.Foot (2.03,-1.34); worker (1.34,-.86), cart (1.66,-.68). Keep road axes unchanged; preserve projected shadows and depth based on foot. Reject any overlap risks in rationale.",["stage-5-urban-details.jpg","stage-4-contact-details.jpg"]))
v.update(call("street-artist",["gate_width","mill_width","freight_width","branch_width","postern_width","core_ink"],context+"Choose physically distinct ribbon mesh widths. Original gate and barracks 1.32, mill/market/quarry .92, freight 1.15, borough 1.04 and Peso 1.18 and Postern .66. Candidate widens main approach and freight, narrows side branch where appropriate, opens passage under arch; stronger center coverage (core_ink) while shoulders fade organically. Keep exact path centers, no new roads, gate/civic street reads as hierarchy, avoid opaque paving at crossings. The existing ground underlay and authored textures stay. ",["stage-5-urban-details.jpg","stage-3-comparison.jpg"]))
total=prior+sum(D(r["estimated_usd"]) for r in ledger)
assert set(v)==set(bounds) and total<=D(5)
dump("parameters.json",{"source_head":"f27f4774e32efc5ad046478a07747eead4256c3e","values":v,"estimated_prior_usd":str(prior),"estimated_new_usd":str(total-prior),"estimated_total_usd":str(total),"billing_verified":False,"calls":2})
print("SPATIAL_ARTIST_PARAMETERS "+json.dumps(v),flush=True)
