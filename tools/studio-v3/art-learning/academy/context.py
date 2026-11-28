"""Role-specific academy retrieval. Do not equate studied principles with demonstrated quality."""
import json
from pathlib import Path
ROOT=Path(__file__).parent
ROLES={"director":("composition","projection","light"),"artist":("light","material","projection"),"integrator":("projection","composition"),"auditor":("light","material","projection","composition")}
def load():
    c=json.loads((ROOT/"curriculum.json").read_text(encoding="utf-8"))
    t=json.loads((ROOT/"techniques.json").read_text(encoding="utf-8"))
    if t["schema"]!=1:raise ValueError("Unknown academy version")
    known={s["id"]:s for s in c["sources"]}
    direct=set(t["provenance"]["direct_text"]);curated=set(t["provenance"]["curated_only"])
    if direct&curated or direct|curated!=set(known):raise ValueError("Invalid source provenance")
    ids=set()
    for card in t["cards"]:
        if card["id"] in ids or not card["sources"] or not set(card["sources"])<=set(known):raise ValueError("Invalid card")
        ids.add(card["id"])
        if card["competency"] not in ROLES["auditor"]:raise ValueError("Invalid competency")
        if not all(isinstance(card[k],str) and card[k].strip() for k in ("principle","procedure","check","risk")):raise ValueError("Incomplete practice")
    return c,t,known
def brief(role,competencies=None):
    if role not in ROLES:raise ValueError("Unknown academy role")
    c,t,known=load();allowed=set(ROLES[role])
    chosen=allowed if competencies is None else set(competencies)
    if not chosen or not chosen<=allowed:raise ValueError("Unsupported competency for this role")
    cards=[x for x in t["cards"] if x["competency"] in chosen]
    sources={s for x in cards for s in x["sources"]}
    return {"version":t["version"],"role":role,"status":t["provenance"]["status"],
      "contract":"Apply relevant practice to an isolated candidate; show evidence and protected strengths, do not claim course completion or scene gain. Use matched native A/B before owner review.",
      "cards":cards,"sources":[{"id":s,"url":known[s]["url"],"read_status":"direct_text" if s in t["provenance"]["direct_text"] else "curated_only"} for s in sorted(sources)]}
