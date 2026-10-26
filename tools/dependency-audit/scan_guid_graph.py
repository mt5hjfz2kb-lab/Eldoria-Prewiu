#!/usr/bin/env python3
"""Read-only static Unity GUID audit for Eldoria. Never approves deletion."""
import os,sys,re,json,subprocess,collections
from pathlib import Path
from datetime import datetime,timezone

SCENE="Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity"
META=re.compile(r"(?m)^guid:\s*([a-fA-F0-9]{32})\s*$")
REF=re.compile(r"\bguid:\s*([a-fA-F0-9]{32})\b")
GROUPS={
 "world_native_sources":"docs/evidence/world4x-canon-20261022/sources/native-",
 "world_authoring":"docs/evidence/world4x-canon-20261022/authoring/",
 "layered_generated":"Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/GeneratedEntry",
 "integration_generated":"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Generated-",
 "layered_all":"Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/",
 "reference_all":"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/",
 "world_runtime":"Unity/Assets/Eldoria/World4XCanon/",
 "valoria_resources":"Unity/Assets/Eldoria/Resources/Valoria/",
 "reference_evidence":"docs/evidence/valoria-reference-clean/"
}
TEXT_SUFFIXES=(".unity",".prefab",".mat",".asset",".anim",".controller",".overrideController",".shader",".shadergraph",".terrainlayer",".uxml",".uss",".lighting")
MAX_FILE=6_291_456
def git(*args):
    return subprocess.run(("git",)+args,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE).stdout
def main():
    root=Path.cwd()
    import urllib.request
    token=os.environ.get("GITHUB_API_TOKEN")
    repo=os.environ.get("GITHUB_REPOSITORY")
    if not token or not repo:raise RuntimeError("Missing GitHub API token/repository; fail closed")
    tree_sha=git("rev-parse","HEAD^{tree}").decode().strip()
    url="https://api.github.com/repos/"+repo+"/git/trees/"+tree_sha+"?recursive=1"
    request=urllib.request.Request(url,headers={"Authorization":"Bearer "+token,
        "Accept":"application/vnd.github+json","User-Agent":"EldoriaReadOnlyGuidAudit"})
    with urllib.request.urlopen(request,timeout=45) as handle:
        tree=json.load(handle)
    if tree.get("truncated") or not tree.get("tree"):
        raise RuntimeError("Incomplete GitHub tree; fail closed")
    files={v["path"]:{"sha":v["sha"],"size":v.get("size",0)}
           for v in tree["tree"] if v.get("type")=="blob"}
    meta_files=[p for p in files if p.startswith("Unity/Assets/") and p.endswith(".meta")]
    missing=[p for p in meta_files if not Path(p).is_file()]
    if missing:
        raise RuntimeError("FAIL_CLOSED_META_SPARSE_MISSING count="+str(len(missing))+" first="+str(missing[:3]))
    guid_to_path=collections.defaultdict(list)
    for p in meta_files:
        m=META.search(Path(p).read_text(encoding="utf-8",errors="replace"))
        if m: guid_to_path[m.group(1).lower()].append(p[:-5])
    issues={"large":[],"unreadable":[],"binary":[]}
    total_remote=0
    def read(p):
        nonlocal total_remote
        info=files.get(p)
        if not info:return None
        if info["size"]>MAX_FILE:
            issues["large"].append({"path":p,"size":info["size"]})
            return None
        if Path(p).is_file(): data=Path(p).read_bytes()
        else:
            issues["unreadable"].append({"path":p,"reason":"not_in_sparse_snapshot_deferred_to_unity"})
            return None
        if b"\0" in data[:1024]:
            issues["binary"].append(p)
            return None
        return data.decode("utf-8",errors="replace")
    bs=read("Unity/ProjectSettings/EditorBuildSettings.asset")
    if not bs:raise RuntimeError("Missing Unity build settings")
    scenes=[]
    for section in bs.split("  - enabled: ")[1:]:
        if not section.startswith("1"): continue
        match=re.search(r"(?m)^\s*path:\s*(Assets/[^\r\n]+)\s*$",section)
        if match and "Unity/"+match.group(1) in files:scenes.append("Unity/"+match.group(1))
    if SCENE not in scenes:
        raise RuntimeError("FAIL_CLOSED_CANNOT_VERIFY_REAL_BUILD_SCENE")
    resources=[p for p in files if p.startswith("Unity/Assets/") and ("/Resources/" in p or "/StreamingAssets/" in p) and not p.endswith(".meta")]
    queue=collections.deque(scenes+resources)
    seen=set(); guid_refs=set(); missing_guids=collections.Counter(); edges=0; serialized=0
    while queue:
        p=queue.popleft()
        if p in seen:continue
        seen.add(p)
        if len(seen)>14000:raise RuntimeError("FAIL_CLOSED_TOO_MANY_ASSETS")
        if not p.endswith(TEXT_SUFFIXES):continue
        body=read(p)
        if body is None:continue
        serialized+=1
        for guid in REF.findall(body):
            guid=guid.lower()
            if guid=="0"*32:continue
            guid_refs.add(guid)
            dests=guid_to_path.get(guid,[])
            if not dests: missing_guids[guid]+=1
            for dest in dests:
                edges+=1
                if dest not in seen:queue.append(dest)
    groups={}
    for name,prefix in GROUPS.items():
        matching=[(p,i) for p,i in files.items() if p.startswith(prefix) and not p.endswith(".meta")]
        guarded=[(p,i) for p,i in matching if p in seen]
        groups[name]={
            "files":len(matching),
            "logical_bytes":sum(x["size"] for _,x in matching),
            "guid_reachable_files":len(guarded),
            "guid_reachable_bytes":sum(x["size"] for _,x in guarded),
            "reachable_examples":[p for p,_ in guarded[:16]],
            "disclaimer":"Assets not reachable by this partial GUID graph are NOT certified removable."
        }
    workflows=[]
    wf_tokens={"native_sources":"docs/evidence/world4x-canon-20261022/sources/",
               "authoring":"docs/evidence/world4x-canon-20261022/authoring/",
               "arttests":"Unity/Assets/Eldoria/ArtTests/",
               "generated":"GeneratedEntry",
               "signing_proof":"android-existing-key-check.json"}
    workflow_paths=[p for p in files if p.startswith(".github/workflows/") and p.endswith((".yml",".yaml"))]
    for p in workflow_paths:
        body=read(p)
        if body:
            hits=[name for name,word in wf_tokens.items() if word in body]
            if hits:workflows.append({"path":p,"hits":hits})
    output={
       "status":"STATIC_GUID_PREFLIGHT_ONLY_NOT_UNITY_CERTIFIED",
       "snapshot":git("rev-parse","HEAD").decode().strip(),
       "at_utc":datetime.now(timezone.utc).isoformat(),
       "safe_execution":{"read_only":True,"files_moved":0,"deleted":0,"unity_editor_used":False,"windows_runner_used":False},
       "tree_file_count":len(files),"unity_meta_files":len(meta_files),
       "guid_mapping":len(guid_to_path),
       "duplicate_guids":{k:v for k,v in guid_to_path.items() if len(v)>1},
       "enabled_scenes":scenes,"resource_roots":len(resources),
       "guid_graph":{"reached_assets":len(seen),"ref_guids":len(guid_refs),"edges":edges,"serialized_files_read":serialized,
          "missing_guid_count":len(missing_guids),
          "missing_guid_samples":[{"guid":g,"count":v} for g,v in missing_guids.most_common(30)],
          "issues":{"too_large_count":len(issues["large"]),"unreadable_count":len(issues["unreadable"]),
                    "large_examples":issues["large"][:30],"unreadable_examples":issues["unreadable"][:20]},
          "small_remote_fetch_bytes":total_remote
       },
       "groups":groups,
       "workflow_refs":workflows,"workflow_files":len(workflow_paths),
       "restrictions":["GUID references do not reveal Blender source or historical workflow requirements",
                       "Some YAML assets larger than 1 MiB are intentionally not parsed",
                       "All Resources/StreamingAssets inputs are conservatively protected",
                       "Before movement run Unity AssetDatabase.GetDependencies and build regression tests",
                       "No deletion or archive promotion is authorized by this report"]
    }
    os.makedirs("../audit-output",exist_ok=True)
    with open("../audit-output/dependency-audit.json","w",encoding="utf-8") as f:json.dump(output,f,indent=2,ensure_ascii=False)
    lines=["# Eldoria: read-only GUID audit","",
       "Repository commit: "+output["snapshot"],
       "Tree files: "+str(len(files)),
       "Unity meta files: "+str(len(meta_files)),
       "Active build scene: "+SCENE,
       "Resources/StreamingAssets roots: "+str(len(resources)),
       "GUID graph reachable assets: "+str(len(seen)),
       "Scanned serialized files: "+str(serialized),
       "Unresolved GUIDs: "+str(len(missing_guids)),
       "Large YAML files deferred to Unity: "+str(len(issues["large"])),"",
       "| Group | Files | Logical GiB | GUID-reachable assets |",
       "|---|---:|---:|---:|"]
    for key,g in groups.items():
        lines.append("| "+key+" | "+str(g["files"])+" | "+format(g["logical_bytes"]/1024**3,".3f")+" | "+str(g["guid_reachable_files"])+" |")
    lines+=["","Workflows naming key areas: "+str(len(workflows))+"/"+str(len(workflow_paths)),
       "","NO FILE IS APPROVED FOR DELETION. Unity Editor inspection remains necessary."]
    summary="\n".join(lines)+"\n"
    Path("../audit-output/SUMMARY.md").write_text(summary,encoding="utf-8")
    print(summary)
if __name__=="__main__":main()
