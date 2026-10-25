#!/usr/bin/env python3
"""Find Eldoria assets to reuse BEFORE creating a new model. No write operations."""
import argparse
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "pipeline/asset-vault/master-catalog-v1.json"
STOP = {"para", "with", "from", "sobre", "model", "modelo", "asset", "nuevo", "new", "the", "and", "una", "uno", "los", "las", "del"}
ALIASES = {
    "cantera": {"stone", "quarry", "rock", "extraction", "piedra"},
    "madera": {"wood", "timber", "log", "forest", "aserradero"},
    "jabalí": {"boar", "rockboar", "prey"},
    "jabali": {"boar", "rockboar", "prey"},
    "engendro": {"spawnling", "rift", "enemy"},
    "brecha": {"breach", "rift", "corruption", "terrain"},
    "muralla": {"wall", "stone", "masonry"},
    "bastión": {"bastion", "fortress", "hero"},
    "ciudad": {"city", "valoria", "building"},
    "puente": {"bridge", "street", "landing"},
}
RANK = {"USE_PRODUCTION": 30, "INTEGRATED_REUSE_WITH_LIMITS": 25,
        "REUSE_CONDITIONAL": 15, "LANDMARK_ONLY": 8,
        "WORLD_CANDIDATE_ONLY": 7, "TECH_CANDIDATE_NOT_ART_APPROVED": 6,
        "HISTORICAL_HOLD_FOR_DIRECT_REVIEW": 3,
        "REJECT_CURRENT_VISUAL": -50, "REJECT_FOR_CURRENT_CANON": -100}

def words(s):
    return {w for w in re.findall(r"[a-záéíóúüñ0-9]+", s.lower()) if len(w)>2 and w not in STOP}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("query", nargs="+", help="Description of desired asset")
    p.add_argument("--json", action="store_true")
    p.add_argument("--limit", type=int, default=12)
    args=p.parse_args()
    q=words(" ".join(args.query))
    expanded=set(q)
    for token in q:
        expanded.update(ALIASES.get(token, set()))
    c=json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    hits=[]
    for a in c["records"]:
        content=" ".join(str(a.get(k) or "") for k in ("name", "role", "path", "family", "limitations"))
        tokens=words(content)
        matches=expanded & tokens
        if not matches:
            continue
        score=10*len(matches)+RANK.get(a["verdict"],0)
        hits.append({"score":score,"id":a["id"],"name":a["name"],
                     "verdict":a["verdict"],"path":a.get("path"),
                     "ref":a.get("ref"),"matches":sorted(matches),
                     "limitations":a.get("limitations","")[:280]})
    hits.sort(key=lambda a:(-a["score"],a["name"]))
    result={"query":" ".join(args.query),"candidate_count":len(hits),
            "top":hits[:max(1,args.limit)],
            "rule":"Always inspect source, current canon, provenance, licensing and real-camera quality before using. Do not generate duplicate by default."}
    if args.json:
        print(json.dumps(result,ensure_ascii=False,indent=2))
    else:
        print("Candidates:",len(hits),"for",result["query"])
        for a in result["top"]:
            print(f"- [{a['verdict']}] {a['name']} @ {a['ref']} : {a['path']} ({', '.join(a['matches'])})")
        if not hits:
            print("No matching CURATED entry; also consult automatic candidate issue and historical inventory before new production.")
    return 0

if __name__=="__main__":
    raise SystemExit(main())
