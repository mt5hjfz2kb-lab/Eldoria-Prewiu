#!/usr/bin/env python3
"""Supplement Unity's native dependency graph with literal references in tracked text."""
import argparse, collections, json, os, pathlib, re, subprocess
SOURCE="d9d4e1f9ffb9f7ab29e1c963a80eb08321c35bb4"
EXT={".cs",".py",".js",".mjs",".ts",".ps1",".sh",".yml",".yaml",".json",".md",".txt",".unity",".prefab",".mat",".shader",".hlsl",".asmdef",".xml"}
ACTIVE={".cs",".py",".js",".mjs",".ts",".ps1",".sh",".yml",".yaml",".shader",".hlsl"}
def main():
    p=argparse.ArgumentParser()
    p.add_argument("--repo",required=True);p.add_argument("--audit-jsonl",required=True);p.add_argument("--out",required=True)
    p.add_argument("--scan-ref",default="HEAD")
    a=p.parse_args();root=pathlib.Path(a.repo)
    scan_commit=subprocess.run(["git","-C",str(root),"rev-parse",a.scan_ref+"^{commit}"],stdout=subprocess.PIPE,check=True).stdout.decode().strip()
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
    # Read immutable Git blobs, not a sparse/reused working directory. Missing
    # checkout files must not silently turn a reference scan into an empty result.
    tree=subprocess.run(["git","-C",str(root),"ls-tree","-r","-z",scan_commit],stdout=subprocess.PIPE,check=True).stdout
    entries=[]
    for raw in tree.split(b"\0"):
        if not raw:continue
        header,name=raw.split(b"\t",1);mode,kind,blob=header.decode().split()
        name=name.decode("utf-8");suffix=pathlib.PurePosixPath(name).suffix.lower()
        if kind!="blob" or not name.startswith(top_dirs) or suffix not in EXT:continue
        if name.startswith("tools/dependency-audit/"):continue
        entries.append((name,suffix,blob))
    checked=0;skipped=0;size=0;hits=[];target_flags=collections.defaultdict(set);dynamic=[];guid_targets=set();directory_targets=set();hit_count=0
    proc=subprocess.Popen(["git","-C",str(root),"cat-file","--batch"],stdin=subprocess.PIPE,stdout=subprocess.PIPE)
    try:
      for name,suffix,blob in entries:
        proc.stdin.write((blob+"\n").encode());proc.stdin.flush()
        header=proc.stdout.readline().decode().strip().split()
        if len(header)!=3 or header[0]!=blob or header[1]!="blob":
            raise RuntimeError("Cannot verify tracked text blob: "+name)
        length=int(header[2]);remaining=length;parts=[]
        while remaining:
            chunk=proc.stdout.read(min(remaining,1024*1024))
            if not chunk:raise RuntimeError("Truncated Git blob: "+name)
            if length<=6*1024*1024:parts.append(chunk)
            remaining-=len(chunk)
        if proc.stdout.read(1)!=b"\n":raise RuntimeError("Invalid Git batch framing")
        if length>6*1024*1024:
            skipped+=1;continue
        text=b"".join(parts).decode("utf-8",errors="ignore")
        checked+=1;size+=length
        matched=set()
        for m in look.finditer(text):
            token=m.group()
            exact=guid_to_path.get(token.lower())
            if exact: matched.add(("guid",exact))
            elif token in indicators:
                for x in dirs[token]: matched.add(("directory_mention",x))
        # Detect dynamic path construction independently from exact folder mentions.
        dynamic_path=("Generated-" in text or "GeneratedEntryV" in text or "ArtVariants/" in text)
        origin=("operational_code" if suffix in ACTIVE else
                "configuration_or_serialized" if suffix in {".unity",".prefab",".mat"} else
                "documentation" if name.startswith("docs/") or suffix==".md" else
                "configuration_or_serialized")
        if dynamic_path and origin=="operational_code":
            dynamic.append({"file":name,"generic_generation_pattern":True})
        for how,target in matched:
            target_flags[target].add(origin);hit_count+=1
            (guid_targets if how=="guid" else directory_targets).add(target)
            if len(hits)<1500:
                hits.append({"asset":target,"reference_kind":how,"source":name,"origin":origin})
    finally:
        proc.stdin.close();proc.stdout.close()
        if proc.wait()!=0:raise RuntimeError("Git reference scan process failed")
    protected=set(x for x,flags in target_flags.items() if "operational_code" in flags or "configuration_or_serialized" in flags)
    docs=set(x for x,flags in target_flags.items() if "documentation" in flags)
    report={"schema":1,"source_unity_commit":SOURCE,"text_scan_commit":scan_commit,"text_source":"immutable Git blobs, independent of sparse checkout","candidate_count":len(records),
            "scanned_text_files":checked,"scanned_text_bytes":size,
            "oversized_or_unreadable_skipped":skipped,
            "assets_mentioned_in_code_or_config":len(protected),
            "assets_mentioned_in_docs":len(docs),
            "assets_with_exact_guid_text_matches":len(guid_targets),
            "assets_with_directory_mentions":len(directory_targets),
            "dynamic_generator_count":len(dynamic),
            "saved_hit_count":len(hits),"hit_list_truncated":hit_count>len(hits),
            "classification_note":"JSON/TXT reports under docs are documentation, not operational dependencies; directory mentions do not prove direct asset use.",
            "assets_without_literal_text_matches":len(candidates-set(target_flags)),
            "dynamic_generator_files":dynamic[:100],
            "warning":"Heuristic supplement, NOT a complete inverse dependency proof. Unity GUID importer result, source-version matching, larger files and historical provenance must still be respected.",
            "top_hits":hits}
    out=pathlib.Path(a.out);out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    print("ELDORIA_STATIC_REFERENCES",json.dumps({k:v for k,v in report.items() if k not in ("top_hits","dynamic_generator_files")}),flush=True)
    print("ELDORIA_DYNAMIC_REFERENCE_COUNT",len(dynamic),flush=True)
if __name__=="__main__":main()
