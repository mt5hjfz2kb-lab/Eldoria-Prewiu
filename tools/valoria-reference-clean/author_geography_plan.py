"""Bounded terrain decisions by the existing local artist from pinned native268 pixels.
No API credits, generated code, production scene writes, or self-approval.
"""
import argparse, base64, hashlib, json, math, os, pathlib, subprocess, time, urllib.request, urllib.error

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p, value): p.write_text(json.dumps(value, indent=2), encoding="utf8")
def api(path, data=None, timeout=5):
    req=urllib.request.Request("http://127.0.0.1:11434"+path,
        data=None if data is None else json.dumps(data).encode(),
        headers={"Content-Type":"application/json"})
    try:
        with urllib.request.urlopen(req, timeout=timeout) as response: return json.load(response)
    except urllib.error.HTTPError as error:
        detail=error.read().decode("utf8",errors="replace")[:4000]
        raise RuntimeError("Local artist HTTP "+str(error.code)+": "+detail) from error
def record(fields):
    return {"type":"object","additionalProperties":False,"required":list(fields),"properties":fields}
def number(lo,hi): return {"type":"number","minimum":lo,"maximum":hi}
def validate(plan):
    errors=[]
    for key in ("observed_defect","design_intent"):
        if len(plan.get(key,""))<35: errors.append(key+" must explain the visible cause")
    actions=plan.get("planned_actions",[])
    if not 4<=len(actions)<=8: errors.append("4-8 actions required")
    for target in ("city","world"):
        selected=[a for a in actions if a.get("target")==target]
        if not (3 if target=="world" else 2)<=len(selected)<=4: errors.append(target+" has "+str(len(selected))+" actions; requires "+("3-4" if target=="world" else "2-4"))
    for a in actions:
        if a.get("type")!="sculpt_region" or a.get("target") not in ("city","world"):
            errors.append("Only existing city/world sculpt_region actions"); continue
        bounds={"city":{"x":(-80,80),"z":(-75,100),"radius_x":(15,40),"radius_z":(18,55),"height":(1.5,4)},
                "world":{"x":(895,1105),"z":(-105,105),"radius_x":(22,65),"radius_z":(25,75),"height":(5,16)}}[a["target"]]
        for key,(lo,hi) in bounds.items():
            value=a.get("parameters",{}).get(key)
            if not isinstance(value,(int,float)) or not math.isfinite(value) or not lo<=value<=hi:
                errors.append(a["target"]+"."+key+" outside "+str((lo,hi)))
    for a in actions:
        if a.get("target")=="world":
            q=a.get("parameters",{})
            try:
                if q["height"]/min(q["radius_x"],q["radius_z"])>.24:
                    errors.append("World hill too steep and isolated: height/min(radius_x,radius_z) must be <=0.24. Use broad low connected shoulders.")
            except (KeyError,ZeroDivisionError,TypeError):pass
    # Landforms must not peak around immutable targets and create artificial craters.
    spots=[(933,-23),(926,-27),(1065,42),(997,-34),(1064.5,-32.9),(1018,30)]
    for x,z in spots:
        height=0
        for a in actions:
            if a.get("target")!="world":continue
            q=a.get("parameters",{})
            try:
                r2=((x-q["x"])/q["radius_x"])**2+((z-q["z"])/q["radius_z"])**2
                height+=q["height"]*max(0,1-r2)**3
            except (KeyError,ZeroDivisionError,TypeError):continue
        if height>0.6:errors.append("Landform overlaps protected POI "+str((x,z))+"; move centre into empty north/west/south landscape, not onto targets.")
    return errors

def main():
    parser=argparse.ArgumentParser();parser.add_argument("--reference",required=True);parser.add_argument("--output",required=True)
    args=parser.parse_args()
    ref=pathlib.Path(args.reference); out=pathlib.Path(args.output); out.mkdir(parents=True,exist_ok=True)
    trials=list(ref.rglob("trial.json")); assert len(trials)==1,"Pinned native268 artifact ambiguous"
    trial=trials[0]; ev=trial.parent
    assert sha(trial)=="c9542de8efe345231f9c6018ea092382e7e113981fe630939b315e62145a38d7"
    identity=json.loads(trial.read_text(encoding="utf-8-sig"))
    images=[ev/"before-city-II-wide.png",ev/"before-world-REGION1-ENTIRE-2048x1152.png",ev/"after-world-REGION1-ENTIRE-2048x1152.png",ev/"after-world-quarry-valoria-wide.png"]
    expected=["8a77743581fdbb537baba157ca91a58e1805efb497a681964761aac2eddb21f7",
              "b7cf13871fa1aec855b150865c207aaed0cb72c3fd2f8e1dfafe23932c8e2064",
              "dd162c40f31936d5ddf93b61e05021819140c3ff71418966334ff4b0ca1f0658",
              "e2a17adb43b4b48b146400735f4609186b067e31cbe219411a5df084824b1477"]
    assert [sha(p) for p in images]==expected,"Real source screenshots differ"
    # Preserve all four exact provenance checks, but send two images as in the proven native268 call.
    images=[images[0],images[2]]
    system=pathlib.Path("tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md")
    survey=json.loads((ev/"survey.json").read_text(encoding="utf-8-sig"))
    parameters=record({"x":number(-80,1105),"z":number(-105,105),"radius_x":number(15,65),
                       "radius_z":number(18,75),"height":number(1.5,16)})
    action=record({"id":{"type":"string"},"type":{"type":"string","enum":["sculpt_region"]},
        "target":{"type":"string","enum":["city","world"]},"rationale":{"type":"string"},"parameters":parameters})
    def target_actions(target,minimum):
        shape=json.loads(json.dumps(action))
        shape["properties"]["target"]["enum"]=[target]
        return {"type":"array","minItems":minimum,"maxItems":4,"items":shape}
    schema=record({"observed_defect":{"type":"string"},"design_intent":{"type":"string"},
        "asset_decision":{"type":"string","enum":["MODIFY"]},
        "reason_for_new_geometry":{"type":"string"},
        "city_actions":target_actions("city",2),"world_actions":target_actions("world",3),
        "expected_visual_difference":{"type":"string"},"mobile_risks":{"type":"string"}})
    brief={
        "target":{"mode":"MODIFY","name":"existing city TerrainData and Region1 original mesh"},
        "actual_survey":survey,"source_scene_sha256":identity["source_scene_sha256"],
        "native_source_run":39669656633,
        "review":"Image1 unchanged city source. Image2 REJECTED native268: central POI craters fixed, but two isolated16m peaks with30m radii still read as round artificial mounds. Small-tree/rock protection also cuts unnatural grooves. Implementation now seats complete noninteractive natural roots on exact new terrain instead of cutting holes around them; gameplay anchors, colliders and structural props remain fixed. City shoulders need stronger broad readable meadow continuity, not decorative clutter.",
        "brief":"Author a new spatial composition using COMPACT finite-footprint landforms instead of infinite Gaussian tails. Each action adds height*max(0,1-r2)^3 where r2=((x-centerX)/radius_x)^2+((z-centerZ)/radius_z)^2. Outside ellipse ZERO height change. Select hills in EMPTY regions so fixed city, quarry, ruin, enemy and forest stay on natural open valleys, NEVER at the bottom of ring-shaped bowls. Candidate areas: northwest x925..965 z65..100, north x995..1035 z85..105, southwest x910..950 z-100..-75, south x990..1020 z-105..-85; choose3-4 broad low OVERLAPPING regional shoulders and exact radii/heights from real images. Maximum height/min(radius_x,radius_z)=0.24, so a50m-radius hill can be at most12m tall; prefer5-9m heights with40-65m radii. Connect gentle contours, do not create isolated symmetrical domes. Existing POIs forest(1065,42),city(1018,30),quarry(1064.5,-32.9),enemy(933,-23),(926,-27),ruin(997,-34). Validator requires total UNMASKED added height at every POI <=0.6m; do not rely on protected holes to fix overlapping hills. Use 2-4 city shoulders visibly outside castle footprint: west/east meadows near X+-50,Z35..70; castle(0,30),roadX0,riverZ-23. Preserve roads, UV, architecture, camera, gameplay anchors, topology and saves. Complete decorative city LOD roots follow terrain once without changing LOD size. No new buildings, plots, assets or spending. Typed parameters execute real native vertices; actual captures determine acceptance.",
        "strict_bounds":"CITY x[-80,80],z[-75,100],radius_x[15,40],radius_z[18,55],height[1.5,4]. WORLD x[895,1105],z[-105,105],radius_x[22,65],radius_z[25,75],height[5,16]. CITY2-4 actions;WORLD3-4 actions, each height/min(radius_x,radius_z)<=0.24. Height is metres positive. 4-8 actions total. Meaningful short rationale each. Do not output source-space Blender coordinates."
    }
    messages=[{"role":"system","content":system.read_text(encoding="utf8")},
              {"role":"user","content":json.dumps(brief),"images":[base64.b64encode(p.read_bytes()).decode() for p in images]}]
    payload={"model":"qwen3-vl:8b-instruct-q4_K_M","stream":False,"format":schema,"messages":messages,
             "keep_alive":0,"options":{"temperature":.15,"num_ctx":12288,"num_predict":2400}}
    write(out/"artist-request.json",{"brief":brief,"schema":schema,"system_sha256":sha(system),
        "images":[{"name":p.name,"sha256":sha(p)} for p in images],"model":payload["model"]})
    server=None;log=None
    try:
        try: api("/api/version")
        except Exception:
            users=pathlib.Path(os.environ.get("SystemDrive","C:")+"/Users")
            installs=list(users.glob("*/AppData/Local/Programs/Ollama/ollama.exe"))
            assert installs,"Existing local Ollama unavailable"
            env=os.environ.copy();env["OLLAMA_HOST"]="127.0.0.1:11434"
            models=list(users.glob("*/.ollama/models"))
            if models: env["OLLAMA_MODELS"]=str(models[0])
            log=open(out/"artist-ollama.log","w")
            server=subprocess.Popen([str(installs[0]),"serve"],env=env,stdout=log,stderr=subprocess.STDOUT)
            for _ in range(40):
                try: api("/api/version");break
                except Exception: time.sleep(1)
            else: raise RuntimeError("Existing local Ollama did not start")
        assert any(m["name"]==payload["model"] for m in api("/api/tags")["models"])
        for attempt in (1,2):
            response=api("/api/chat",payload,420);write(out/("artist-response-"+str(attempt)+".json"),response)
            try:
                plan=json.loads(response["message"]["content"])
                plan["planned_actions"]=plan.pop("city_actions",[])+plan.pop("world_actions",[])
                errors=validate(plan)
            except (ValueError,KeyError,TypeError) as e: errors=[str(e)]
            if response.get("done_reason")!="stop": errors.append("Incomplete model response")
            write(out/"artist-validation.json",{"attempt":attempt,"errors":errors,"geometry_executed":False})
            if not errors: break
            if attempt==2: raise RuntimeError("Local artist plan rejected: "+str(errors))
            payload["messages"]=messages+[{"role":"assistant","content":response["message"]["content"]},
                {"role":"user","content":"Correct your previous JSON plan while retaining the two input images and original brief. Keep two separately typed arrays city_actions(2-4) and world_actions(3-4). Validation errors: "+str(errors)}]
        plan["sculpt_kernel"]="compact_cubic_v2"
        plan["source_scene_sha256"]=identity["source_scene_sha256"]
        plan["source_run"]=39669656633
        plan["qwen_artist_authored"]=True
        plan["art_approved"]=False
        write(out/"artist-plan.json",plan)
        print("VALORIA_GEOGRAPHY_LOCAL_ARTIST_PLAN_READY "+sha(out/"artist-plan.json"))
    finally:
        if server:
            server.terminate()
            try:server.wait(timeout=20)
            except subprocess.TimeoutExpired:server.kill()
        if log:log.close()
if __name__=="__main__": main()
