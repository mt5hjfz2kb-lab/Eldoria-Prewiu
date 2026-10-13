#!/usr/bin/env python3
"""Offline CI gate for the isolated, visual-only legacy GLB Unity catalog."""
import hashlib,json,pathlib,re,sys
ROOT=pathlib.Path(__file__).resolve().parents[2]
catalog_file=ROOT/"Unity/Assets/Eldoria/AssetLibrarySelectionV1/Resources/EldoriaSelectedAssetCatalogV1.json"
audit_file=ROOT/"pipeline/asset-library-canonicalization-audit.json"
runtime_file=ROOT/"Unity/Assets/Eldoria/AssetLibrarySelectionV1/EldoriaSelectedAssetLibraryV1.cs"
editor_file=ROOT/"Unity/Assets/Eldoria/AssetLibrarySelectionV1/Editor/EldoriaSelectedAssetLibraryBuilderV1.cs"
catalog=json.loads(catalog_file.read_text())
inventory=json.loads(audit_file.read_text())
assert catalog["schema_version"]==1 and catalog["not_a_unity_visual_pass"] is True
rows=catalog["entries"]
assert len(rows)==36 and len({r["id"] for r in rows})==36
legacy=[r for r in rows if r["origin"]=="legacy_canonical"]
frozen=[r for r in rows if r["origin"]=="hero_v3_frozen"]
supplement=[r for r in rows if r["origin"]=="historical_supplement"]
assert len(legacy)==26 and len(frozen)==8 and len(supplement)==2
assert len([r for r in legacy if r["tier"]=="SELECTED"])==10
manifest={r["path"]:r for r in inventory["assets"]}
assert len(manifest)==26
statuses={"SELECTED","CONDITIONAL","SCENERY_ONLY","MATERIAL_DIAG","WORLD_ONLY"}
legacy_validated=[]
total_bytes=0
for row in legacy:
    assert row["tier"] in statuses, row["id"]
    assert row["source_relative"] in manifest
    source=pathlib.Path(row["source_relative"])
    assert not source.is_absolute() and ".." not in source.parts
    assert source.parts[:4]==("Unity","Assets","Eldoria","Resources"),row["source_relative"]
    assert source.parts[4]==("WorldPlayerCity" if row["tier"]=="WORLD_ONLY" else "Valoria"),row["source_relative"]
    assert row["resource_key"]==source.as_posix().split("/Resources/",1)[1][:-4]
    assert row["source_relative"].endswith(".glb")
    original=manifest[row["source_relative"]]
    assert row["sha256"]==original["sha256"],row["id"]
    assert row["original_bytes"]==original["bytes"],row["id"]
    full=ROOT/source
    assert full.is_file() and (ROOT/(str(source)+".meta")).is_file(),row["id"]
    size=full.stat().st_size
    assert size==original["bytes"],row["id"]
    h=hashlib.sha256()
    with full.open("rb") as f:
        for chunk in iter(lambda:f.read(1024*1024),b""):h.update(chunk)
    assert h.hexdigest()==row["sha256"],row["id"]
    assert row["unity_verified"] is False
    if row["tier"]=="SELECTED":
        assert row["score"]>=6.5 and row["resource_key"],row["id"]
    if row["tier"] in {"MATERIAL_DIAG","SCENERY_ONLY","WORLD_ONLY"}:
        assert row["tier"]!="SELECTED"
    total_bytes+=size
    legacy_validated.append(row["id"])
for row in frozen:
    assert row["source_commit"]=="0abf13870d21725c30acf3f3e3a8c86385470073"
    assert re.fullmatch("[a-f0-9]{64}",row["sha256"]) and not row["resource_key"]
    assert row["tier"] in {"REAUTHOR","FROZEN_REFERENCE"}
assert all(s["tier"]=="HOLD_EXTERNAL" and not s["resource_key"] for s in supplement)
runtime=runtime_file.read_text()
editor=editor_file.read_text()
checks=[
 ("inert runtime", "SpawnSelectedVisual" in runtime and "GeneratedPrefabsResource" in runtime),
 ("not active in game", "OnEnable(" not in runtime and "Awake(" not in runtime),
 ("no scene mutation", "ProductionSlice" not in runtime and "Valoria.unity" not in editor),
 ("collider disable", "collider.enabled = false" in runtime),
 ("editor hash gate", "SHA256.Create()" in editor and "SHA mismatch" in editor),
 ("source scene isolated", "EditorSceneManager.NewScene" in editor),
 ("editor prefab generator", "PrefabUtility.SaveAsPrefabAsset" in editor),
 ("deterministic captures", "selected-prefabs-unity.png" in editor),
 ("editor neutral namespace", "Eldoria.AssetLibrary.Editor" in editor)
]
assert all(ok for _,ok in checks),[name for name,ok in checks if not ok]
out=ROOT/"selected-library-v1-validation"
out.mkdir(exist_ok=True)
evidence={
 "status":"SOURCE_SELECTION_STATIC_GATE_PASS",
 "unity_editor_compilation":"NOT_RUN_RUNNER_RESERVED",
 "unity_artist_approval":"NOT_ESTABLISHED",
 "can_promote_to_game":False,
 "files_selected":10,
 "legacy_verified":len(legacy_validated),
 "frozen_referenced":len(frozen),
 "supplements_referenced":len(supplement),
 "files_hashed_bytes":total_bytes,
 "selection": [r["id"] for r in legacy if r["tier"]=="SELECTED"],
 "checks":[name for name,ok in checks if ok],
 "runner_shared_consumed":False,
 "paid_credits":0,
}
(out/"validation.json").write_text(json.dumps(evidence,indent=2)+"\n")
print(json.dumps(evidence,indent=2))
