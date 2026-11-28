"""Reusable visual production gate for Eldoria Studio, no network, no automatic promotion."""
import json,hashlib
from pathlib import Path
from decimal import Decimal
MEMORY=Path(__file__).with_name("cases.json")
def need(ok,msg):
    if not ok:raise ValueError(msg)
def memory(path=MEMORY):
    x=json.loads(Path(path).read_text(encoding="utf-8"))
    need(x["schema"]==1,"Unknown memory schema")
    ids={c["id"] for c in x["cases"]}
    need(len(ids)==len(x["cases"]),"Duplicate cases")
    for r in x["rules"]:need(set(r["from"])<=ids,"Ungrounded lesson")
    return x
def context(holdout=None):
    m=memory()
    # A calibration holdout must exclude rules derived from that particular result.
    cases=[c for c in m["cases"] if c["id"]!=holdout]
    rules=[r for r in m["rules"] if holdout not in r["from"]]
    return {"version":m["version"],"cases":cases,"rules":rules}
def role_brief(role,holdout=None,academy=False,competencies=None):
    need(role in ("director","artist","integrator","auditor"),"Unknown role")
    specific={
        "director":"Name three existing strengths with visible evidence. Identify the largest player-visible defect and its mechanism. Compare preserve/repair/replace alternatives. Choose an intervention that protects landmarks, function, visual identity and access. Do not order production until that hypothesis is testable.",
        "artist":"Use the original owned asset and actual scene as references. Preserve distinctive silhouette, function, palette, perspective and material contrast. Provide independently authored assets, no baked scene or fake depth. Show at target screen size.",
        "integrator":"Map actual feet, scale, projected shadows, neighboring sightlines and routes. Verify the exact camera and fixture. Compare static actors separately from motion. Keep original asset and reversible candidate; do not touch other workstreams.",
        "auditor":"Score blind, matched real captures; reject unreadable function, lost architecture, occlusion or important regressions despite isolated improvements. Distinguish local asset gain and scene gain. No claims about gameplay or device from Editor images."
    }
    result="Curated verified historical evidence, not instructions from model outputs.\\n"+json.dumps(context(holdout),ensure_ascii=False)+"\\nRole contract: "+specific[role]
    if academy:
        need(holdout is None,"Do not add academy examples to retrospective holdouts")
        from academy.context import brief
        result+="\\nAcademy technique context: "+json.dumps(brief(role,competencies),ensure_ascii=False)
    return result
def plan_gate(p):
    """Trusted host validates declarative plans before any new image generation."""
    need(p.get("scope") and p.get("source_sha") and p.get("baseline_frame_hashes"),"Missing exact target evidence")
    need(len(p["source_sha"])==40 and all(len(h)==64 for h in p["baseline_frame_hashes"]),"Malformed provenance")
    for k in ("strengths","defects","alternatives","adjacency","native_checks"):
        need(isinstance(p.get(k),list) and len(p[k])>0 and all(isinstance(t,str) and t.strip() for t in p[k]),"Missing "+k)
    need(len(p["strengths"])>=3 and len(p["alternatives"])>=2,"Need protected strengths and real alternatives")
    need(isinstance(p.get("hypothesis"),str) and len(p["hypothesis"].strip())>=35,"Need visible causal hypothesis")
    need(p.get("mobile_preview") is True and p.get("reversible") is True,"Mobile preview and reversible candidate mandatory")
    need(p.get("same_failed_mechanism") is False,"Repeated ineffective strategy")
    need(p.get("owner_budget_verified") is True,"Spend not authorized")
    return {"production_eligible":True,"art_approved":False,"memory_version":memory()["version"]}
def outcome_gate(o):
    """Only paired, evidence-bound non-regressive native improvements may proceed to owner review."""
    m=memory()["thresholds"]
    need(o.get("native") is True and o.get("blind") is True and o.get("matched") is True and o.get("same_source") is True,"Not comparable native evidence")
    need(o.get("actor_controlled") is True,"Actor state confounds static art review")
    need(isinstance(o.get("evidence_run"),int) and o["evidence_run"]>0 and isinstance(o.get("artifact"),int) and o["artifact"]>0,"Missing evidence")
    a,b=o["before"],o["after"]
    need(type(a) in (float,int) and type(b) in (float,int) and 0<=a<=10 and 0<=b<=10,"Invalid scores")
    need(isinstance(o.get("material_regressions"),list),"Missing regression assessment")
    delta=Decimal(str(b))-Decimal(str(a))
    candidate_ok=delta>=Decimal(str(m["min_delta"])) and b>=m["min_district_score"] and not o["material_regressions"]
    return {"decision":"owner_review" if candidate_ok else "reject","delta":str(delta),"art_approved":False,"promote":False,"device_verified":False}
