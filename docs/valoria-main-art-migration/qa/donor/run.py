"""Produce or verify private region and Corona evidence on the frozen experimental source."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import socket
import struct
import subprocess
import sys
import time
import urllib.request
import xml.etree.ElementTree as ET

SHA="6057c2604de84ae27f47ede44eafa294c96bcd24"
SCENE="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity"
TEST="Eldoria.Tests.RegionOneCanonIntegratedAcceptanceTests.CoronaRegionRendersAndRetainsGeographicRoutes"
SCRIPTS="tools/valoria-modular25d/"
INPUTS={
 "tools/bastion-iii/author-frontier.py":"cd142878444cd7abe2fea47585fb10c02a9c6b77",
 SCRIPTS+"journey-browser.py":"54ca4c666937c4350ef024fa60ef41682b1928d6",
 SCRIPTS+"journey-serve.py":"5ef9f4354c385b34be39222e712547aa75d9764e",
 SCRIPTS+"reduce-legacy-images.ps1":"d1f49d9db6416eaa5f7ab3197036f7cbbd741c8d",
 "Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DInterface.JourneyProbe.cs":"a8a825fa463b8c162fc0ec6687f78b3d0f0f26e6",
 SCRIPTS+"pack-hidden-geography.py":"92ef54dbf54c5a48597a9c705ad42835aebef092",
 "Unity/"+SCENE:"24c68b8029686c89c6a72f06f3d5151b62e2edf9",
 "Unity/Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity":"23d78dc89b3dd182945c7f1ffab61f2adf3b8c55",
 "Unity/Assets/Eldoria/Tests/PlayMode/RegionOneCanonIntegratedAcceptanceTests.cs":"a179d4d3b8212d903e1cf719661de92c10165c3e",
 "Unity/Assets/Eldoria/Tests/EditMode/CoronaPresentationContractTests.cs":"fa56f687d0580f7f828595b4d04997f2ea773cf5",
 "Unity/Assets/Eldoria/World4XCanon/Editor/RegionOneCanonicalCapture.cs":"1ae9f58cfc05ee66c9e0601c18e96faec36d87b4",
 "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/BastionIIIArtBake.cs":"06e3f9a0424b18d4bf6b88d9f0d3a537d807513a",
}
ROOMS=("training","heroes","daily","mail","march","vigilants","scouts","bag")
COMMON=("home","wood","stone","region","breach","panorama","woodland","river","breach-panorama")


def need(value,message):
    if not value: raise ValueError(message)


def digest(path):
    sha=hashlib.sha256()
    with path.open("rb") as stream:
        for part in iter(lambda:stream.read(4<<20),b""): sha.update(part)
    return sha.hexdigest()


def inventory(root):
    return {p.relative_to(root).as_posix():digest(p) for p in sorted(root.rglob("*")) if p.is_file()}


def git(root,*args):
    return subprocess.check_output(["git","-C",root,*args],text=True).strip()


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def save(path,value):
    path.write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding="utf-8")


def png_size(path):
    with path.open("rb") as f: head=f.read(24)
    need(head[:8]==b"\x89PNG\r\n\x1a\n" and head[12:16]==b"IHDR","Invalid image "+str(path))
    return struct.unpack(">II",head[16:24])


def run(cmd,root,env,log=None):
    if log:
        with log.open("w",encoding="utf-8") as f: subprocess.run(cmd,cwd=root,env=env,check=True,stdout=f,stderr=subprocess.STDOUT)
    else: subprocess.run(cmd,cwd=root,env=env,check=True)


def verify_region(folder,phase):
    expected=set(COMMON)
    if phase=="after":
        expected.update(("city-at-alternate-position","breach-near","stone-retina"))
        expected.update(f"camera/{i:03d}" for i in range(60))
        expected.update(f"idle-{kind}/{i:03d}" for kind in ("Boar","Spawn") for i in range(32))
    seen={}
    for p in folder.rglob("*.png"):
        label=p.relative_to(folder).with_suffix("").as_posix()
        info=load(p.with_suffix(".json")); width,height=png_size(p)
        need(info["playMode"] and info["actualUnityCamera"] and info["version"]=="6000.3.23f1"
             and info["sourceScene"]==SCENE and info["sha256"]==digest(p) and
             (info["width"],info["height"])==(width,height) and (width,height)==
             ((1170,2532) if label=="stone-retina" else (390,844)),"Mismatched Unity frame "+label)
        seen[label]=info["sha256"]
    need(expected<=seen.keys(),"Missing region views "+str(sorted(expected-seen.keys())))
    return seen


def native(a,root,out,report):
    need(not git(root,"status","--porcelain","--untracked-files=no"),"Disposable donor checkout is dirty.")
    project=root/"Unity"; env=dict(os.environ,GITHUB_SHA=SHA)
    unity=a.unity.resolve(); blender=a.blender.resolve()
    need(unity.is_file() and blender.is_file(),"Local Unity and Blender required.")
    def editor(label,options,extra=None):
        run([unity,"-batchmode","-force-d3d11","-projectPath",project,*options,"-logFile",out/(label+".log")],
            root,dict(env,**(extra or {})))
    def tests(label,platform,test_filter,extra=None):
        xml=out/(label+".xml")
        editor(label,["-runTests","-testPlatform",platform,"-testFilter",test_filter,"-testResults",xml],extra)
        rows=ET.parse(xml).getroot().findall(".//test-case")
        need(rows and all(x.get("result")=="Passed" and test_filter in x.get("fullname","") for x in rows),
             "Filtered Unity tests incomplete: "+label)
        if platform=="PlayMode": need(len(rows)==1,"Unexpected number of region test cases.")
        else: need(len(rows)>=8,"Missing original Corona contract cases.")
        report.setdefault("nunit",{})[label]={"cases":len(rows),"sha256":digest(xml)}
    tests("corona-contract","EditMode","Eldoria.Tests.CoronaPresentationContractTests")
    run([blender,"--background","--python-exit-code","1","--python",root/"tools/bastion-iii/author-frontier.py"],root,env,out/"blender.log")
    editor("bake",["-executeMethod","Eldoria.ReferenceClean.Editor.BastionIIIArtBake.Build"])
    for phase in ("before","after"):
        stage=out/phase;stage.mkdir()
        baseline=phase=="before"
        tests(phase,"PlayMode",TEST,{"VALORIA_CONTENT_PROBE":"1","VALORIA_GAMEPLAY_PROBE":"1","ELDORIA_REGION1_CAPTURE_OUT":str(stage),"ELDORIA_WORLD_ART_BASELINE":str(int(baseline)),
              "ELDORIA_BAKE_TERRAIN_QUERIES":str(int(baseline)),"ELDORIA_VALIDATE_TERRAIN_RAYCAST":str(int(not baseline))})
        report.setdefault("frames",{})[phase]=verify_region(stage,phase)
        if baseline:
            tape=project/"Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RuntimeTerrainQueries.bytes"
            shutil.copyfile(stage/"terrain.bytes",tape)
    for name in ("grid.bin","routes.txt"):
        a_hash=digest(out/"before"/name)
        need(a_hash==digest(out/"after"/name),"Donor's two region phases disagree: "+name)
        report[name]=a_hash
    # Follow the dedicated two-scene mobile packager in the original disposable donor workflow.
    src=project/"Assets/Eldoria/Resources/Valoria";dest=project/"Assets/ValoriaBuildInputs/LegacyModels"
    need(not dest.exists(),"Legacy model staging already exists.")
    for file in sorted(src.rglob("*.glb")):
        target=dest/file.relative_to(src);target.parent.mkdir(parents=True,exist_ok=True)
        shutil.move(str(file),str(target));shutil.move(str(file)+".meta",str(target)+".meta")
    run([sys.executable,root/(SCRIPTS+"pack-hidden-geography.py"),project,"--combined-player"],root,env,out/"mobile-packaging.log")
    player=out/"private-player"
    editor("webgl",["-quit","-executeMethod","Eldoria.World4XCanon.Editor.RegionOneCanonicalCapture.BuildModularWorld"],
           {"ELDORIA_WORLD_PLAYER_OUT":str(player)})
    need((player/"index.html").is_file() and list(player.rglob("*.wasm*")) and list(player.rglob("*.data*")),"Incomplete combined WebGL player.")
    report["player"]=inventory(player)


def browser(a,root,out,report):
    native_dir=a.native.resolve()
    source=load(native_dir/"manifest.json")
    need(source["source"]==SHA and source["mode"]=="native" and source["status"]=="COMPLETE" and
         source["player"]==inventory(native_dir/"private-player"),"The exact private donor player is required.")
    import importlib.metadata
    report["playwright"]=importlib.metadata.version("playwright")
    report["nativeManifestSha256"]=digest(native_dir/"manifest.json")
    code=(root/(SCRIPTS+"journey-browser.py")).read_text(encoding="utf-8")
    # Observe existing actions. These two states already occur between real player touches.
    anchor="            await tap('Toggle left activities',False);assert state['drawer']"
    anchor2="            await tap('Current real construction objective',False);assert state['council']"
    for token,label in ((anchor,"activities"),(anchor2,"council")):
        need(code.count(token)==1,"Updated browser fixture")
        code=code.replace(token,token+"\n            await page.screenshot(path=OUT/'"+label+".png')")
    for suffix,orientation in (("", "portrait"),("-horizontal","landscape")):
        token="                    await page.screenshot(path=OUT/('sistema-'+name+'"+suffix+".png'))"
        need(code.count(token)==1,"Updated donor room capture")
        contents = "{'room':state['room'],'level':state['level'],'taps':state['taps'],'viewport':page.viewport_size}"
        code=code.replace(token,token+"\n                    (OUT/('controls-'+name+'-"+orientation+".json')).write_text(json.dumps("+contents+"),encoding='utf-8')")
    if a.chrome:
        chrome=str(a.chrome.resolve()); need(Path(chrome).is_file(),"Chrome executable missing.")
        token="shutil.which('google-chrome') or shutil.which('chromium')"
        need(code.count(token)==1,"Updated Chromium locator")
        code=code.replace(token,repr(chrome))
    driver=out/"observed-journey.py";driver.write_text(code,encoding="utf-8")
    report["observedBrowserDriverSha256"]=digest(driver)
    with socket.socket() as s:
        s.bind(("127.0.0.1",0));port=s.getsockname()[1]
    url=f"http://127.0.0.1:{port}/index.html"
    server=subprocess.Popen([sys.executable,root/(SCRIPTS+"journey-serve.py"),native_dir/"private-player",str(port)])
    try:
        for _ in range(60):
            need(server.poll() is None,"Local-only journey server terminated.")
            try:
                with urllib.request.urlopen(url,timeout=2) as result:
                    if result.status==200: break
            except OSError: time.sleep(.2)
        else: raise RuntimeError("Private player did not start serving.")
        shots=out/"screens"
        run([sys.executable,driver,url+"?journey-qa"],root,dict(os.environ,VALORIA_DPR=str(a.dpr),
            VALORIA_TEST_REGION="1",VALORIA_EVIDENCE=str(shots)),out/"journey.log")
    finally:
        server.terminate()
        server.wait(timeout=15)
    records=load(shots/"journey.json")
    visited={x.get("room_traversed") for x in records["log"]}
    landscape={x.get("room_landscape") for x in records["log"] if x.get("exit_available")}
    need(set(ROOMS)<=visited and set(ROOMS)<=landscape and records["last"]["level"]==5,"Incomplete real room traversal.")
    frames={}
    for f in sorted(shots.glob("*.png")):
        frames[f.name]={"sha256":digest(f),"size":png_size(f)}
    wanted={"sistema-"+room+suffix+".png" for room in ROOMS for suffix in ("","-horizontal")}
    wanted.update(["activities.png","council.png","nombre-inicial.png","guia-iniciar-obra.png","temporizador-en-parcela.png",
                   "ciudad-a-region-5.png","regreso-a-valoria-5.png","region-1-pinch-real-bastion-5.png","region-1-pan-real-bastion-5.png"])
    for name in wanted:
        size=(844,390) if "-horizontal." in name else (390,844)
        need(name in frames and frames[name]["size"]==tuple(x*a.dpr for x in size),"Missing state or incorrect viewport: "+name)
    observed_controls={}
    for room in ROOMS:
        for orientation,size in (("portrait",(390,844)),("landscape",(844,390))):
            path=shots/("controls-"+room+"-"+orientation+".json")
            record=load(path)
            need(record["room"]==room and record["level"]==5 and record["taps"] and
                 (record["viewport"]["width"],record["viewport"]["height"])==size, "Missing traced controls: "+room)
            observed_controls[path.name]=digest(path)
    report.update(frames=frames,controls=observed_controls,journeySha256=digest(shots/"journey.json"),browser=records["browser"],dpr=a.dpr,
        video={str(f.relative_to(shots)):digest(f) for f in shots.rglob("*.webm")},
        rewardCeremony=[x for x in records["log"] if "daily_claim_ceremony" in x])
    need(report["video"],"Missing native WebGL journey recording.")


def main():
    p=argparse.ArgumentParser(description=__doc__);sub=p.add_subparsers(dest="mode",required=True)
    for mode in ("native","browser"):
        s=sub.add_parser(mode)
        for field in ("checkout","output"):
            s.add_argument("--"+field,required=True,type=Path)
        if mode=="native":
            for field in ("unity","blender"):
                s.add_argument("--"+field,required=True,type=Path)
        else:
            s.add_argument("--native",required=True,type=Path)
            s.add_argument("--chrome",type=Path)
            s.add_argument("--dpr",choices=(1,3),type=int,default=1)
    a=p.parse_args();root=a.checkout.resolve();out=a.output.resolve()
    need(out!=root and root not in out.parents,"Use fresh output outside the disposable checkout.")
    out.mkdir(parents=True,exist_ok=False)
    report={"source":SHA,"mode":a.mode,"status":"INCOMPLETE","runnerSha256":digest(Path(__file__)),"physicalIphone":False}
    try:
        need(git(root,"rev-parse","HEAD")==SHA,"Unexpected donor SHA.")
        for path,blob in INPUTS.items():
            if a.mode=="browser" and not path.startswith(SCRIPTS): continue
            need(git(root,"hash-object","--no-filters",path)==blob,"Frozen donor input mismatch: "+path)
        report["sourceBlobs"]=INPUTS if a.mode=="native" else {k:v for k,v in INPUTS.items() if k.startswith(SCRIPTS)}
        (native if a.mode=="native" else browser)(a,root,out,report)
        report["status"]="COMPLETE"
    except Exception as error:
        report["error"]=str(error);print(str(error),file=sys.stderr)
    finally: save(out/"manifest.json",report)
    return 0 if report["status"]=="COMPLETE" else 1

if __name__=="__main__": sys.exit(main())
