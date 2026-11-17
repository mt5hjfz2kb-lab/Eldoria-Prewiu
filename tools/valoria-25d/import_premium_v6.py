#!/usr/bin/env python3
"""Selective immutable art import to a NEW branch originating from main.

Never merge the artistic branch or copy its QA workflows and prototype builders.
The source commit is pinned; the source Unity scene is SHA-256 verified.
Only its direct GUID asset references and nine runtime presentation scripts are kept.
"""
import hashlib
import json
import re
import subprocess
from pathlib import Path

SOURCE_SHA = "10dbc197d48ea8517c6a3dd6c22f1a2abf75d508"
SOURCE_ROOT = "Unity/Assets/Experimentos/Ciudad2D/"
TARGET_ROOT = "Unity/Assets/Valoria25D/"
SCENE = SOURCE_ROOT+"QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity"
SCENE_SHA256 = "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06"
RUNTIME = (
    "City2DAmbientGating", "City2DAmbientMotion", "City2DCameraFit",
    "City2DPilotHud", "City2DPremiumShadowGate", "City2DProgressionPolicy",
    "City2DStateBridge", "City2DTorchPulse", "CityBuilding",
)
MANIFEST = Path("tools/valoria-25d/selected_assets_manifest.json")
def git(*args):
    return subprocess.check_output(["git", *args])
def show(path):
    return git("show", f"{SOURCE_SHA}:{path}")
def digest(raw):
    return hashlib.sha256(raw).hexdigest()
def copy_once(source,destination):
    blob=show(source)
    target=Path(destination)
    if target.exists():
        raise RuntimeError("Do not overwrite existing integration asset: "+str(target))
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_bytes(blob)
    return {"source":source,"destination":str(target),"sha256":digest(blob),"bytes":len(blob)}

def main():
    # This is a new integration branch; NEVER operate in main or the art branch.
    branch=git("symbolic-ref","--short","HEAD").decode().strip()
    assert branch=="integration/valoria-25d-playable-20261117", branch
    assert not Path(TARGET_ROOT).exists(), "Target directory must not pre-exist"
    # source is fetched by workflow, and verified against the owner's known SHA
    git("cat-file","-t",SOURCE_SHA)
    scene=show(SCENE)
    assert scene.startswith(b"%YAML 1.1")
    assert digest(scene)==SCENE_SHA256, "Native V6 scene diverges from verified Unity build"
    # Map metas strictly inside experimental city sources, excluding QA/history.
    art_files=git("ls-tree","-r","--name-only",SOURCE_SHA,SOURCE_ROOT).decode().splitlines()
    paths=[p for p in art_files if p.startswith(SOURCE_ROOT)]
    guid_map={}
    for meta in paths:
        if not meta.endswith(".meta") or not (
            "/Sprites/" in meta or "/Scripts/" in meta):
            continue
        text=show(meta).decode("utf-8-sig")
        m=re.search(r"(?m)^guid:\s*([a-f0-9]{32})",text)
        if not m: continue
        if m.group(1) in guid_map: raise RuntimeError("Duplicate source GUID: "+m.group(1))
        guid_map[m.group(1)]=meta.removesuffix(".meta")
    requested_guids=set(re.findall(rb"guid:\s*([a-f0-9]{32})",scene))
    referenced={guid.decode() for guid in requested_guids if guid.strip(b"0")}
    selected=set()
    for guid in sorted(referenced):
        item=guid_map.get(guid)
        if item:
            if "/Scripts/Editor/" in item:
                raise RuntimeError("Archived runtime scene references editor-only asset: "+item)
            selected.add(item)
    for name in RUNTIME:
        path=SOURCE_ROOT+"Scripts/"+name+".cs"
        if path not in paths:
            raise RuntimeError("Missing runtime class "+name)
        selected.add(path)
    # Ensure all four PremiumV6 candidates and shadows really belong to the scene.
    mandatory=(
       "Cuartel_Premium_V2.png", "Cuartel_Premium_ShadowV2.png",
       "Aserradero_Premium_V2.png", "Aserradero_Premium_V2_Shadow.png",
       "Granero_Premium_V1.png", "Granero_Premium_V1_Shadow.png",
       "Cantera_Premium_V1.png", "Cantera_Premium_V1_Shadow.png",
    )
    for name in mandatory:
        path=SOURCE_ROOT+"Sprites/Candidates/"+name
        if path not in selected:
            raise RuntimeError("Expected real V6 art candidate is not a serialized scene dependency: "+name)
    # Other build sprites come from exact serialized GUIDs, not all historic candidates.
    copies=[]
    def destination(src):
        relative=src.removeprefix(SOURCE_ROOT)
        if relative.startswith("Scripts/"):
            relative="Runtime/"+relative.removeprefix("Scripts/")
        return TARGET_ROOT+relative
    for path in sorted(selected):
        if not path.startswith(SOURCE_ROOT+"Sprites/") and not path.startswith(SOURCE_ROOT+"Scripts/"):
            raise RuntimeError("Unsupported dependency, inspect before import: "+path)
        if not path.endswith((".cs",".png",".asset",".mat")):
            raise RuntimeError("Asset type not reviewed: "+path)
        if path+".meta" not in paths: raise RuntimeError("Missing stable meta for "+path)
        copies.append(copy_once(path,destination(path)))
        copies.append(copy_once(path+".meta",destination(path)+".meta"))
    scene_path=TARGET_ROOT+"Scenes/Valoria25D_PremiumV6_Source.unity"
    copies.append(copy_once(SCENE,scene_path))
    source_meta=SCENE+".meta"
    if source_meta not in paths: raise RuntimeError("Original saved Unity scene meta missing")
    copies.append(copy_once(source_meta,scene_path+".meta"))
    report={
        "schema":1,"origin_main_sha":"904ff183ffb9053f68b9c0f359b2e9d3d40ce805",
        "artist_source_commit":SOURCE_SHA,"unity_source_sha256":SCENE_SHA256,
        "runtime_scripts":[x+".cs" for x in RUNTIME],
        "resolved_scene_guids":sorted(g for g in referenced if g in guid_map),
        "external_engine_or_main_guids":sorted(g for g in referenced if g not in guid_map),
        "note_external_guids":"Unity built-in or canonical main GUID; validate in real Editor, never pretend all are sourced.",
        "four_premium_candidates":list(mandatory),
        "copied_files":copies,
        "qa_assets_copied":False,
        "main_edited":False,
        "art_branch_edited":False
    }
    MANIFEST.parent.mkdir(parents=True,exist_ok=True)
    MANIFEST.write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
    print("VALORIA25D_PINNED_V6_SELECTIVE_MIGRATION_PASS",
          "scene_guids",len(referenced),"source_matched",len(referenced)-len(report["external_engine_or_main_guids"]),
          "files",len(copies),"total_bytes",sum(x["bytes"] for x in copies))
if __name__=="__main__":
    main()
