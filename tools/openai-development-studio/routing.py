"""Deterministic model selection; no automatic escalations or paid retries."""
import json
from pathlib import Path
from decimal import Decimal
from economics import RATES
POLICY={
 "standard":{"director":("gpt-6.1-terra","none"),"programmer":("gpt-6.2-sol","low"),"auditor":("gpt-6.1-terra","none"),"coordinator":("gpt-6.2-sol","low")},
 "economy":{"director":("gpt-6.1-terra","none"),"programmer":("gpt-6.2-sol","low"),"auditor":("gpt-6.1-terra","none"),"coordinator":("gpt-6-luna","low")},
 "deep":{"director":("gpt-6.1-terra","medium"),"programmer":("gpt-6.2-sol","medium"),"auditor":("gpt-6.1-terra","medium"),"coordinator":("gpt-6.2-sol","medium")},
}
def plan(mode,profile="standard"):
    if mode not in ("review","art_candidate") or profile not in POLICY: raise ValueError("Invalid routing profile")
    if profile=="economy" and mode!="review": raise ValueError("Economy only for bounded read-only reviews")
    roles=["director","programmer","auditor","coordinator"]
    if mode=="art_candidate":roles.insert(1,"artist")
    return [{"role":r,"model":POLICY[profile][r][0],"reasoning":POLICY[profile][r][1]} if r!="artist"
            else {"role":r,"model":"gpt-image-2.6-flare","reasoning":None} for r in roles]
def role(mode,profile,slot):
    return next(r for r in plan(mode,profile) if r["role"]==slot)
def lessons(target, mode, path=None):
    p=Path(path) if path else Path(__file__).with_name("lessons.json")
    data=json.loads(p.read_text())
    # Controlled human-reviewed evidence only; no agent response becomes an instruction.
    matches=[x for x in data["lessons"] if x["status"]=="verified" and
             ("all" in x["targets"] or target in x["targets"] or mode in x["targets"])]
    return "\n".join(x["id"]+": "+x["rule"] for x in matches)[:2000]
