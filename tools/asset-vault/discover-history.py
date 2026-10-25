#!/usr/bin/env python3
"""Full, read-only inventory of all pinned historical 3D files and exact Git blob versions.

Reads only Git trees, never downloads Blender/GLB contents, never checks out or
merges historical code. A missing commit is explicitly counted as unavailable.
"""
import argparse
import json
import subprocess
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "pipeline/historical-branches-20261018.json"
EXTENSIONS = (".blend", ".glb", ".gltf", ".fbx", ".obj", ".stl")

def git(*args):
    p = subprocess.run(["git", "-C", str(ROOT), *args],
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
    return p.returncode, p.stdout

def classify(path):
    p=path.lower()
    if "/vendor/" in p or any(x in p for x in
        ("unity/assets/emaceart/","unity/assets/mega fantasy props pack/",
         "unity/assets/bublik/","unity/assets/polyone/","unity/assets/tree_packs/",
         "unity/assets/worldpackcompare/","unity/assets/holotna/","unity/assets/naturestarterkit2/")):
        return "THIRD_PARTY_PACK_LICENSE_REVIEW"
    if "/arttests/" in p or "/experiment" in p or "/variants/" in p:
        return "HISTORICAL_EXPERIMENT_NO_AUTO_REUSE"
    if p.endswith(".blend") or p.startswith("art-source/") or p.startswith("artsource/"):
        return "HISTORICAL_EDITABLE_SOURCE"
    return "HISTORICAL_CANDIDATE"

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, help="Optional full report destination")
    parser.add_argument("--compact-output", type=Path, help="Permanent compact versioned catalog")
    args = parser.parse_args()
    refs = json.loads(SOURCE.read_text(encoding="utf-8-sig"))["branches"]
    by_path = defaultdict(lambda: defaultdict(lambda: {"ref_count":0,"examples":[]}))
    missing=[]; examined=0
    for ref in refs:
        sha,name=ref["sha"],ref["name"]
        code,blob=git("ls-tree","-r","-z",sha)
        if code:
            missing.append({"branch":name,"sha":sha,"reason":"pinned commit unavailable locally"})
            continue
        examined+=1
        for raw in blob.decode("utf-8","replace").split("\0"):
            if not raw or "\t" not in raw: continue
            header,path=raw.split("\t",1)
            if not path.lower().endswith(EXTENSIONS): continue
            parts=header.split()
            if len(parts)<3 or parts[1]!="blob": continue
            oid=parts[2]
            v=by_path[path][oid]
            v["ref_count"]+=1
            if len(v["examples"])<3:
                v["examples"].append({"branch":name,"pinned_commit":sha})
    compact=[]
    exact_versions=0
    for path,versions in sorted(by_path.items()):
        encoded=[]
        for oid,data in sorted(versions.items(),key=lambda z:(-z[1]["ref_count"],z[0])):
            encoded.append({"git_blob_sha1":oid,
                            "ref_count":data["ref_count"],
                            "example_refs":data["examples"]})
            exact_versions+=1
        compact.append({"path":path,"category":classify(path),
                        "art_approved":False,"license_reviewed":False,
                        "versions":encoded})
    payload={
        "schema_version":2,
        "kind":"pinned_historical_3d_git_tree_version_index",
        "read_only":True,
        "declared_refs":len(refs),"complete_refs":examined,
        "unavailable_refs":len(missing),
        "complete_201_tree_audit":examined==len(refs),
        "unique_3d_paths":len(compact),"exact_path_blob_versions":exact_versions,
        "candidate_paths":compact,"missing_objects":missing,
        "warning":"Git blob SHA identifies bytes in git objects (not an independent SHA-256 or a license/quality review). Reuse requires authorisation, visual proof and safe asset source."
    }
    full=json.dumps(payload,ensure_ascii=False,indent=2)+"\n"
    if args.output:
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.write_text(full,encoding="utf-8")
    if args.compact_output:
        args.compact_output.parent.mkdir(parents=True,exist_ok=True)
        args.compact_output.write_text(json.dumps(payload,ensure_ascii=False,separators=(",",":"))+"\n",encoding="utf-8")
    if not args.output: print(full)
    return 0

if __name__=="__main__":
    raise SystemExit(main())
