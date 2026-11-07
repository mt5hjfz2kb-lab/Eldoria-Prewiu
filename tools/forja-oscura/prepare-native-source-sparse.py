#!/usr/bin/env python3
"""Build a bounded sparse checkout plan from a previously certified Unity
scene dependency manifest. This creates source inputs only (NOT Unity evidence).
Run inside PR173's Ubuntu CI against exact pinned main SHA 9efc0fef.
"""
import re, sys, pathlib, subprocess, json, os
base=pathlib.Path(sys.argv[1]).resolve()
manifest=base/"docs/evidence/valoria-reference-clean/integration-input-manifest.txt"
assert manifest.is_file(), "Main Unity input dependency manifest missing"
paths=set()
for line in manifest.read_text(encoding="utf-8-sig").splitlines():
    m=re.fullmatch(r"(.+?)\s+([A-F0-9]{2}(?:-[A-F0-9]{2}){31})",line.strip())
    if not m: continue
    name=m.group(1).replace("\\","/")
    if name.startswith(("Assets/","Packages/")) and ".." not in pathlib.PurePosixPath(name).parts:
        paths.add("/Unity/"+name)
assert len(paths)>1200, ("Insufficient certified input files",len(paths))
patterns={
    "/Unity/ProjectSettings/**",
    "/Unity/Packages/**",
    "/Unity/Assets/**/*.meta",
    "/Unity/Assets/**/*.asmdef",
    "/Unity/Assets/**/*.asmref",
    "/Unity/Assets/**/*.cs",
    "/Unity/Assets/**/Resources/**",
    "/Unity/Assets/**/StreamingAssets/**",
    "/Unity/Assets/UniversalRenderPipelineGlobalSettings.asset",
    "/Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity",
}
patterns|=paths
run=subprocess.run(["git","-C",str(base),"sparse-checkout","set","--no-cone","--stdin"],
    input="\n".join(sorted(patterns))+"\n",text=True,capture_output=True,timeout=1500)
if run.returncode:
    raise SystemExit("REAL_GIT_SPARSE_CHECKOUT_FAILED: "+run.stderr[-3000:])
check={
"Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity":"24c68b8029686c89c6a72f06f3d5151b62e2edf9",
"Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs":"e79200c6f58f0dc763e017f29dee24a835070040",
"Assets/Eldoria/UI/ValoriaRoyalHUDV1/Components/RoyalHUD.cs":"4cc86572d3bd585c508954d1534f0216bdee1f0a",
"Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV2Hud.cs":"512d3d20e3c2a44bc662df7879a5f0d5fd9682d2",
}
for name,sha in check.items():
    actual=subprocess.check_output(["git","hash-object",str(base/"Unity"/name)],text=True).strip()
    assert actual==sha,(name,sha,actual)
assert "6000.3.23f1" in (base/"Unity/ProjectSettings/ProjectVersion.txt").read_text()
assert (base/"Unity/Packages/manifest.json").is_file()
total=sum(p.stat().st_size for p in (base/"Unity").rglob("*") if p.is_file())
files=sum(1 for p in (base/"Unity").rglob("*") if p.is_file())
assert total<8_000_000_000,("Refuse unexpectedly massive Unity package",total)
out={"status":"PINNED_NATIVE_SCENE_SOURCE_SPARSE_CHECKOUT_PASS_NOT_UNITY",
 "base":"9efc0fefd447585f6867ab03ce2c41084b32adf2",
 "manifest_entries":len(paths),"sparse_patterns":len(patterns),
 "files":files,"bytes":total,"kit_included":False,"unity_tested":False}
(base/"native-source-sparse-report.json").write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))
