#!/usr/bin/env python3
"""Search every curated, current and archived Eldoria 3D source before authoring."""
import argparse,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
C=ROOT/"pipeline/asset-vault/master-catalog-v1.json"
CURRENT=ROOT/"pipeline/asset-vault/current-full-index-v1.json"
HISTORY=ROOT/"pipeline/asset-vault/historical-full-index-v1.json"
STOP={"para","with","from","sobre","model","modelo","asset","nuevo","new","the","and","una","uno","los","las","del","glb","blend"}
ALIASES={
    "cantera":{"stone","quarry","rock","extraction","piedra","mine","mina"},
    "madera":{"wood","timber","log","forest","aserradero","lumber","sawmill"},
    "jabalí":{"boar","rockboar","prey"},"jabali":{"boar","rockboar","prey"},
    "engendro":{"spawnling","rift","enemy"},
    "brecha":{"breach","rift","corruption","terrain","fisura"},
    "muralla":{"wall","stone","masonry","castle","defensive"},
    "bastión":{"bastion","fortress","hero"},
    "ciudad":{"city","valoria","building","houses"},
    "puente":{"bridge","street","landing"},
    "roca":{"rock","stone","cliff","terrain"},
    "torre":{"tower","turret","keep"},
    "mina":{"mine","mineral","quarry","stone"},
    "arbol":{"tree","forest","conifer","pine"},
    "árbol":{"tree","forest","conifer","pine"}
}
RANK={"USE_PRODUCTION":45,"INTEGRATED_REUSE_WITH_LIMITS":35,
      "REUSE_CONDITIONAL":25,"REUSE_SOURCE_PRIORITY":22,
      "SELECTIVE_REUSE":18,"LANDMARK_ONLY":5,
      "WORLD_CANDIDATE_ONLY":12,"TECH_CANDIDATE_NOT_ART_APPROVED":12,
      "HISTORICAL_HOLD_FOR_DIRECT_REVIEW":4,"HISTORICAL_REVIEW":4,
      "EXPERIMENT_REVIEW":0,"THIRD_PARTY_LICENSE_UNVERIFIED":-10,
      "REJECT_CURRENT_VISUAL":-60,"REJECT_FOR_CURRENT_CANON":-100}
def words(s):
    return {w for w in re.findall(r"[a-záéíóúüñ0-9]+",str(s).lower())
            if len(w)>2 and w not in STOP}
def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig")) if path.exists() else None
def search(query,limit=20,include_rejected=False):
    request=words(query);expanded=set(request)
    for w in request:expanded.update(ALIASES.get(w,()))
    cur=load(C);paths=set();results=[]
    def add(name,path,ref,verdict,family,source,description,sha=None):
        text=" ".join(map(str,[name,path,description,family]))
        matched=expanded & words(text)
        if not matched:return
        if verdict.startswith("REJECT") and not include_rejected:return
        key=(path or name,ref)
        if key in paths:return
        paths.add(key)
        results.append({"name":name,"path":path,"ref":ref,"verdict":verdict,
                        "family":family,"source":source,
                        "git_blob_sha1":sha,"matched":sorted(matched),
                        "score":len(matched)*15+RANK.get(verdict,2),
                        "note":description[:220]})
    for a in (cur or {}).get("records",[]):
        add(a.get("name"),a.get("path"),a.get("ref"),a.get("verdict"),
            a.get("family"),"CURATED",str(a.get("role",""))+" "+str(a.get("limitations","")))
    now=load(CURRENT)
    for x in (now or {}).get("records",[]):
        if x.get("status")=="CURATED_ENTRY":continue
        add(Path(x["path"]).stem,x["path"],"CURRENT_MAIN_OR_PR",
            x.get("status","UNCLASSIFIED_REVIEW"),x.get("family",""),
            "CURRENT_TREE",x.get("reason",""),x.get("git_blob_sha1"))
    past=load(HISTORY)
    for x in (past or {}).get("candidate_paths",[]):
        if not x.get("versions"):continue
        v=x["versions"][0]
        ref=v["example_refs"][0]["pinned_commit"] if v.get("example_refs") else "ARCHIVED_REF"
        add(Path(x["path"]).stem,x["path"],ref,
            "HISTORICAL_REVIEW",x.get("category",""),
            "HISTORICAL_TREE",x.get("category",""),v.get("git_blob_sha1"))
    results.sort(key=lambda x:(-x["score"],x["source"],x["name"]))
    return {"query":query,"total_found":len(results),"top":results[:max(1,limit)],
            "catalog_loaded":bool(cur),"current_loaded":bool(now),
            "historical_loaded":bool(past),
            "warning":"Search discovers candidates, not approval. Verify original identity, licensing, current owner, zoom and rendered quality before modifying the game."}
def main():
    p=argparse.ArgumentParser();p.add_argument("query",nargs="+")
    p.add_argument("--json",action="store_true");p.add_argument("--limit",type=int,default=15)
    p.add_argument("--include-rejected",action="store_true");a=p.parse_args()
    o=search(" ".join(a.query),a.limit,a.include_rejected)
    if a.json:print(json.dumps(o,ensure_ascii=False,indent=2))
    else:
        print("Found",o["total_found"],"candidate(s) across curated/current/historical indexes")
        for item in o["top"]:
            print(" - [{}] {} @ {} : {}".format(item["verdict"],item["name"],item["ref"],item["path"]))
        if not o["historical_loaded"] or not o["current_loaded"]:
            print("NOTE: Full indexes missing; search coverage is incomplete.")
if __name__=="__main__":main()
