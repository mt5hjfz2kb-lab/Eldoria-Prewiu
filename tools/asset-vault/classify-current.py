#!/usr/bin/env python3
"""Classify every current Git-tracked 3D asset path by origin, not by artistic approval."""
import argparse,json,subprocess
from pathlib import Path
from collections import Counter,defaultdict
ROOT=Path(__file__).resolve().parents[2]
EXT=(".glb",".blend",".gltf",".fbx",".obj",".stl")
PACKS=[("Unity/Assets/Mega Fantasy Props Pack/","Mega Fantasy Props Pack"),
("Unity/Assets/EmaceArt/","Slavic World Free"),
("Unity/Assets/PolyOne/","PolyOne modular terrain"),
("Unity/Assets/Tree_Packs/","URP Tree Pack"),
("Unity/Assets/Bublik/","Bublik castle"),
("Unity/Assets/Holotna/","Holotna mountain"),
("Unity/Assets/WorldPackCompare/","Quaternius comparative"),
("Unity/Assets/NatureStarterKit2/","NatureStarterKit2")]
def assess(path):
    for prefix,family in PACKS:
        if path.startswith(prefix):return family,"THIRD_PARTY_LICENSE_UNVERIFIED","Review license, style and current-camera quality."
    if path.startswith("Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/"):
        return "Valoria alternate building families","SELECTIVE_REUSE","Compare with approved present-day Valoria."
    if path.startswith("Unity/Assets/Eldoria/ArtTests/"):
        return "Eldoria art tests","HISTORICAL_REVIEW","Experiment, not automatically current art."
    if path.startswith("art-source/valoria/production/"):
        return "Valoria editable production sources","REUSE_SOURCE_PRIORITY","Potential editable 3D, compare version and evidence."
    if path.startswith("art-source/valoria/lookdev/"):
        return "Valoria lookdev variants","HISTORICAL_REVIEW","Do not restore rejected compositions."
    if path.startswith("docs/evidence/valoria-layered-construction-"):
        return "Valoria layered construction versions","HISTORICAL_REVIEW","Many variants: choose by artistic proof not number."
    if path.startswith("docs/evidence/"):
        return "Other original 3D evidence","HISTORICAL_REVIEW","Archival proof is not current production approval."
    if path.startswith("Unity/Assets/Eldoria/Resources/"):
        return "Other Eldoria resources","SELECTIVE_REUSE","Review scene usage and current version."
    if path.startswith(("art-source/","ArtSource/")):
        return "Other editable Eldoria sources","REUSE_SOURCE_PRIORITY","Reuse original when suitable, preserve provenance."
    if "/candidates/" in path or "/variants/" in path:
        return "Other 3D experiments","HISTORICAL_REVIEW","Never automatically restore old variants."
    return "Other tracked 3D","UNCLASSIFIED_REVIEW","Inspect source, license and visual role."
def main():
    p=argparse.ArgumentParser();p.add_argument("--output",required=True,type=Path);a=p.parse_args()
    catalog=json.loads((ROOT/"pipeline/asset-vault/master-catalog-v1.json").read_text(encoding="utf-8-sig"))
    known=set()
    for record in catalog["records"]:
        for key in ("path","editable"):
            if record.get(key):known.add(record[key])
        for collection in ("sources","integration"):
            for v in record.get(collection,{}).values():
                if isinstance(v,dict) and v.get("path"):known.add(v["path"])
    tree=subprocess.check_output(["git","-C",str(ROOT),"ls-tree","-r","-z","HEAD"])
    records=[];families=Counter();statuses=Counter();duplicates=defaultdict(list)
    for raw in tree.decode("utf-8","replace").split("\0"):
        if not raw or "\t" not in raw:continue
        header,path=raw.split("\t",1)
        if not path.lower().endswith(EXT):continue
        sha=header.split()[2]
        family,status,reason=assess(path)
        if path in known:status="CURATED_ENTRY"
        records.append({"path":path,"git_blob_sha1":sha,"family":family,
                        "status":status,"reason":reason,
                        "art_approved_by_scan":False,"license_verified_by_scan":False})
        duplicates[sha].append(path);families[family]+=1;statuses[status]+=1
    output={"schema_version":1,"kind":"complete_current_3d_git_tree_index",
        "read_only":True,"total":len(records),"unique_git_blobs":len(duplicates),
        "duplicate_groups":[{"git_blob_sha1":oid,"paths":paths} for oid,paths in sorted(duplicates.items()) if len(paths)>1],
        "family_counts":dict(families),"status_counts":dict(statuses),
        "records":sorted(records,key=lambda x:x["path"]),
        "warning":"No binary-body SHA-256, artistic approval, license, performance or automatic promotion."}
    a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(output,ensure_ascii=False,separators=(",",":"))+"\n",encoding="utf-8")
    print("CURRENT_TREE_FAMILY_AUDIT",json.dumps({k:v for k,v in output.items() if k not in ("records","duplicate_groups")},ensure_ascii=False))
if __name__=="__main__":main()
