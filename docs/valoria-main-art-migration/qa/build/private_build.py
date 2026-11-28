"""Build a private, identifiable Valoria player in an exclusively owned disposable checkout."""
import argparse
import contextlib
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import time
import zipfile

SCENE = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity"
EVIDENCE = Path("docs/evidence/valoria-reference-clean")
GENERATED = Path(SCENE).parent / "Resources/BastionIII"
NS = "Eldoria.ReferenceClean.Editor."
CERT = "f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8"
HERE = Path(__file__).resolve().parent
BUILDERS = {
    "WebGL": ("ValoriaIntegratedWebBuild.Build", "web-build.json",
              "Builds/ValoriaWebPlaytest", "WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED"),
    "Android": ("ValoriaReferenceCleanAndroidBuild.Build", "android-build.json",
                "Builds/ValoriaReferenceClean/Valoria-Reference-Clean.apk", "APK_COMPILED_NOT_DEVICE_VALIDATED"),
}


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write(path, obj):
    path.write_text(json.dumps(obj, indent=2, ensure_ascii=False), encoding="utf-8")


def digest(path):
    sha = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(4 << 20), b""):
            sha.update(chunk)
    return sha.hexdigest()


def git(root, *args):
    return subprocess.check_output(["git","-C",str(root),*args], text=True).strip()


def need(condition, reason):
    if not condition:
        raise ValueError(reason)


def run_command(command, root, env, log=None):
    with (log.open("w", encoding="utf-8") if log else contextlib.nullcontext()) as stream:
        subprocess.run([str(x) for x in command], check=True, cwd=root, env=env,
                       stdout=stream, stderr=subprocess.STDOUT if stream else None)


@contextlib.contextmanager
def android_inputs(root):
    # Same exclusions as the existing family packager. Preserve the source and metas.
    project = root / "Unity"
    request = root / "tools/valoria-reference-clean/request.json"
    backup = request.read_bytes()
    cfg = read(request)
    cfg.update(content_updates=False, publish_content=False)
    sources = sorted((project/"Assets/Eldoria/Resources/Valoria").rglob("*.glb"))
    sources += [project/"Assets/StreamingAssets/ValoriaFamilyV3"]
    with tempfile.TemporaryDirectory(dir=root.parent, prefix="valoria-private-inputs-") as tmp:
        moved = []
        try:
            write(request,cfg)
            for item in sources:
                for path in (item, item.with_name(item.name+".meta")):
                    if path.exists():
                        target = Path(tmp)/path.relative_to(project)
                        target.parent.mkdir(parents=True, exist_ok=True)
                        shutil.move(str(path),str(target))
                        moved.append((path,target))
            yield
        finally:
            for path,target in reversed(moved):
                need(not path.exists(),"Concurrent change while restoring " + str(path))
                shutil.move(str(target),str(path))
            request.write_bytes(backup)


@contextlib.contextmanager
def fresh_receipts(paths):
    # Versioned historical evidence must never satisfy the current run.
    originals = {path:path.read_bytes() for path in paths if path.is_file()}
    try:
        for path in originals:
            path.unlink()
        yield
    finally:
        for path in paths:
            if path in originals:
                path.write_bytes(originals[path])
            else:
                path.unlink(missing_ok=True)


def measure(player):
    files = [player] if player.is_file() else sorted(p for p in player.rglob("*") if p.is_file())
    rows = [{"name": p.name if player.is_file() else p.relative_to(player).as_posix(),
             "bytes": p.stat().st_size, "sha256": digest(p)} for p in files]
    return {"files": rows, "packageBytes": sum(r["bytes"] for r in rows)}


def build(args, manifest):
    root, out = args.checkout.resolve(), args.output.resolve()
    project = root/"Unity"
    target, unity = args.target, args.unity.resolve()
    need(Path(git(root,"rev-parse","--show-toplevel")).resolve()==root, "Use a disposable repository root.")
    need(git(root,"rev-parse","HEAD")==args.source,"Checkout differs from the pinned source SHA.")
    need(not git(root,"status","--porcelain","--untracked-files=no"), "Tracked source is dirty.")
    need(not git(root,"ls-files","--others","--","Unity/Assets","Unity/Packages","Unity/ProjectSettings"),
         "Unexpected untracked Unity inputs in this checkout.")
    version = (project/"ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
    manifest["unityExpected"] = next(x.split(": ",1)[1] for x in version.splitlines() if x.startswith("m_EditorVersion: "))
    need(unity.is_file(),"Unity executable unavailable.")
    need(args.blender.is_file(), "Blender executable unavailable for the canonical Bastion III bake.")
    need((project/SCENE).is_file(), "Canonical integrated scene is missing.")
    method, receipt, location, success = BUILDERS[target]
    destination = project/location
    need(not destination.exists(),"Use a fresh build output in the disposable checkout.")
    evidence = root/EVIDENCE
    evidence.mkdir(parents=True,exist_ok=True)
    env = dict(os.environ,GITHUB_SHA=args.source)
    # Explicit -buildTarget prevents a warm Library's previous target from leaking into the proof.
    def unity_step(label, method, platform, options=None):
        started = time.monotonic()
        log = out/(label+".log")
        cmd = [unity,"-batchmode","-force-d3d11","-buildTarget",platform,"-projectPath",project,
               "-executeMethod",NS+method,"-logFile",log]
        try:
            run_command(cmd,root,dict(env,**(options or {})))
        finally:
            manifest["steps"].append({"label":label,"method":NS+method,"seconds":round(time.monotonic()-started,2)})
    if target=="Android":
        keys=read(evidence/"android-existing-key-check.json")
        need(keys["package"]=="com.eldoria.valoria.v5test" and keys["old_version_code"]==1 and
             any(k.get("matches_original") and k.get("certificate_sha256")==CERT and Path(k["path"]).is_file()
                 for k in keys["keys"]), "Existing Android signing identity unavailable on this runner.")
    platforms=unity.parent/"Data/PlaybackEngines"
    module="AndroidPlayer" if target=="Android" else "WebGLSupport"
    need((platforms/module).is_dir(), "Missing installed Unity module "+module)
    author=root/"tools/bastion-iii/author-frontier.py"
    run_command([args.blender.resolve(),"--background","--python-exit-code","1","--python",author],
                root,env,out/"blender.log")
    need((project/GENERATED/"PrivateFissure.glb").is_file(),"Blender did not create the authored fissure.")
    unity_step("bastion-iii","BastionIIIArtBake.Build","Win64")
    inputs=[project/GENERATED/"PrivateFissure.glb"]+[
        project/GENERATED/"Granary"/(n+".prefab") for n in ("Granary","StoneWorkshop","StockCrate","StockSack","StockBarrel")]
    manifest["generatedInputs"]={p.relative_to(project).as_posix():digest(p) for p in inputs}
    proof_files=[evidence/name for name in ("integration-input-signature.txt","integration-input-manifest.txt","integration-result.json")]
    with fresh_receipts([evidence/receipt]+(proof_files if target=="WebGL" else [])):
        if target=="WebGL":
            unity_step("native-proof","ValoriaIntegrationBuild.ValidateCurrent","Win64")
            for item in proof_files:
                shutil.copy2(item,out/item.name)
        stage=project/"Assets/Editor/ValoriaPrivateBuildQA"
        need(not stage.exists() and not stage.with_suffix(".meta").exists(),"Editor observation path already exists.")
        stage.mkdir(parents=True)
        observer=HERE/"ValoriaPackageContents.cs"
        manifest["observerSha256"]=digest(observer)
        packed=out/"packed-assets.json"
        try:
            shutil.copy2(observer,stage/observer.name)
            with (android_inputs(root) if target=="Android" else contextlib.nullcontext()):
                try:
                    unity_step("package",method,target,{"VALORIA_QA_PACKED_CONTENTS":str(packed)})
                finally:
                    if (evidence/receipt).exists():
                        shutil.copy2(evidence/receipt,out/receipt)
        finally:
            shutil.rmtree(stage)
            stage.with_suffix(".meta").unlink(missing_ok=True)
    status=read(out/receipt)
    need((status["status"],status["scene"],status["source_sha"])==(success,SCENE,args.source),"Invalid canonical builder receipt.")
    data=read(packed)
    need((data["source"],data["target"],data["unity"])==(args.source,target,manifest["unityExpected"])
         and data["entries"],"Packed BuildReport identity mismatch or missing asset records.")
    manifest.update(packedSha256=digest(packed),packedRows=len(data["entries"]),receiptSha256=digest(out/receipt))
    if target=="WebGL":
        need((destination/"index.html").is_file() and list(destination.rglob("*.wasm*")) and
             list(destination.rglob("*.data*")), "WebGL HTML, WASM or data payload missing.")
    else:
        need(destination.is_file() and digest(destination)==status["apk_sha256"],"APK differs from its canonical receipt.")
        with zipfile.ZipFile(destination) as apk:
            need("AndroidManifest.xml" in apk.namelist() and "lib/arm64-v8a/libil2cpp.so" in apk.namelist()
                 and apk.testzip() is None,"Android APK payload invalid or missing ARM64 IL2CPP.")
        # Verify the signature of the final APK, including the certificate, without accessing key material.
        sdk=platforms/"AndroidPlayer/SDK/build-tools"
        jars=sorted(sdk.glob("*/lib/apksigner.jar"),key=lambda p:tuple(int(x) for x in re.findall(r"\d+",p.parent.parent.name)),reverse=True)
        java=platforms/"AndroidPlayer/OpenJDK/bin/java.exe"
        need(jars and java.is_file(),"Installed APK signature verification tools unavailable.")
        signature=subprocess.check_output([java,"-jar",jars[0],"verify","--verbose","--print-certs",destination],text=True)
        need(re.search(r"certificate SHA-256 digest:\s*"+CERT+r"\b",signature,re.I),"APK signed by an unexpected certificate.")
        manifest["verifiedCertificateSha256"]=CERT
    player=out/("player.apk" if target=="Android" else "player")
    shutil.move(str(destination),str(player))
    manifest.update(measure(player))
    manifest["status"]="PRIVATE_PACKAGE_VERIFIED"


def main():
    p=argparse.ArgumentParser(description=__doc__)
    for key in ("checkout","output","unity","blender"):
        p.add_argument("--"+key,required=True,type=Path)
    p.add_argument("--source",required=True)
    p.add_argument("--target",choices=sorted(BUILDERS),required=True)
    a=p.parse_args()
    out=a.output.resolve(); root=a.checkout.resolve()
    if not re.fullmatch(r"[0-9a-f]{40}",a.source) or out==root or root in out.parents:
        p.error("Use a full source SHA and new output outside the source checkout.")
    out.mkdir(parents=True,exist_ok=False)
    manifest={"source":a.source,"target":a.target,"scene":SCENE,"status":"INCOMPLETE","steps":[],
              "launcherSha256":digest(Path(__file__)),"deviceTested":False}
    try:
        build(a,manifest)
    except Exception as error:
        manifest["error"]=str(error)
        print(str(error),file=sys.stderr)
    finally:
        write(out/"manifest.json",manifest)
    return 0 if manifest["status"]=="PRIVATE_PACKAGE_VERIFIED" else 1

if __name__=="__main__":
    sys.exit(main())
