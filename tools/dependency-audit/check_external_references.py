#!/usr/bin/env python3
"""Supplement Unity's native dependency graph with literal references in tracked text."""
import argparse, collections, json, os, pathlib, re, subprocess
SOURCE="d9d4e1f9ffb9f7ab29e1c963a80eb08321c35bb4"
EXT={".cs",".py",".js",".mjs",".ts",".ps1",".sh",".yml",".yaml",".json",".md",".txt",".unity",".prefab",".mat",".shader",".hlsl",".asmdef",".xml"}
ACTIVE={".cs",".py",".js",".mjs",".ts",".ps1",".sh",".yml",".yaml",".shader",".hlsl"}
def main():
    p=argparse.ArgumentParser()
    p.add_argument("--repo",required=True);p.add_argument("--audit-jsonl",required=True);p.add_argument("--out",required=True)
    a=p.parse_args();root=pathlib.Path(a.repo)
    records=[json.loads(l) for l in open(a.audit_jsonl,encoding="utf-8") if l.strip()]
    records=[r for r in records if r.get("category")=="REVIEW_NO_UNITY_REFERENCE"]
    guid_to_path={r["guid"].lower():r["path"] for r in records if r.get("guid")}
    dirs=collections.defaultdict(set)
    for r in records:
        x=r["path"]
        m=re.search(r"/(Generated-\d+|GeneratedEntryV\d+|ArtVariants/[^/]+)/",x)
        if m:dirs[m.group(1)].add(x)
    indicators=set(dirs)
    look=re.compile(r"(?i)\b[0-9a-f]{32}\b|Generated-\d+|GeneratedEntryV\d+|ArtVariants/[A-Za-z0-9_-]+")
    candidates={r["path"] for r in records}
    top_dirs=(".github/","pipeline/","tools/","scripts/","docs/","Unity/Assets/")
    gitlist=subprocess.run(["git","-C",str(root),"ls-files","-z"],stdout=subprocess.PIPE,check=True).stdout
    checked=0;skipped=0;size=0;hits=[];target_flags=collections.defaultdict(set);dynamic=[]
    for raw in gitlist.split(b"\0"):
        if not raw:continue
        name=raw.decode("utf-8","replace");suffix=pathlib.PurePosixPath(name).suffix.lower()
        if not name.startswith(top_dirs) or suffix not in EXT:continue
        if name.startswith("tools/dependency-audit/"):continue
        f=root/name
        try:
            st=f.stat()
            if st.st_size>6*1024*1024:
                skipped+=1;continue
            text=f.read_text(encoding="utf-8",errors="ignore")
        except (OSError,UnicodeError):
            skipped+=1;continue
        checked+=1;size+=st.st_size
        matched=set()
        for m in look.finditer(text):
            token=m.group()
            exact=guid_to_path.get(token.lower())
            if exact: matched.add(("guid",exact))
            elif token in indicators:
                for x in dirs[token]: matched.add(("directory_mention",x))
        # Detect dynamic path construction independently from exact folder mentions.
        dynamic_path=("Generated-" in text or "GeneratedEntryV" in text or "ArtVariants/" in text)
        origin="operational_code" if suffix in ACTIVE else "documentation" if suffix==".md" else "configuration_or_serialized"
        if dynamic_path and origin=="operational_code":
            dynamic.append({"file":name,"generic_generation_pattern":True})
        for how,target in matched:
            target_flags[target].add(origin)
            if len(hits)<1500:
                hits.append({"asset":target,"reference_kind":how,"source":name,"origin":origin})
    protected=set(x for x,flags in target_flags.items() if "operational_code" in flags or "configuration_or_serialized" in flags)
    docs=set(x for x,flags in target_flags.items() if "documentation" in flags)
    report={"schema":1,"source_unity_commit":SOURCE,"candidate_count":len(records),
            "scanned_text_files":checked,"scanned_text_bytes":size,
            "oversized_or_unreadable_skipped":skipped,
            "assets_mentioned_in_code_or_config":len(protected),
            "assets_mentioned_in_docs":len(docs),
            "assets_without_literal_text_matches":len(candidates-set(target_flags)),
            "dynamic_generator_files":dynamic[:100],
            "warning":"Heuristic supplement, NOT a complete inverse dependency proof. Unity GUID importer result, source-version matching, missing/larger files and historical provenance must still be respected.",
            "top_hits":hits}
    out=pathlib.Path(a.out);out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    print("ELDORIA_STATIC_REFERENCES",json.dumps({k:v for k,v in report.items() if k not in ("top_hits","dynamic_generator_files")}),flush=True)
    print("ELDORIA_DYNAMIC_REFERENCE_COUNT",len(dynamic),flush=True)
if __name__=="__main__":main()
