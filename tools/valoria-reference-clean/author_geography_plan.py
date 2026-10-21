"""Bounded terrain decisions by the existing local artist from pinned native264 pixels.
No API credits, generated code, production scene writes, or self-approval.
"""
import argparse, base64, hashlib, json, math, os, pathlib, subprocess, time, urllib.request

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p, value): p.write_text(json.dumps(value, indent=2), encoding="utf8")
def api(path, data=None, timeout=5):
    req=urllib.request.Request("http://127.0.0.1:11434"+path,
        data=None if data is None else json.dumps(data).encode(),
        headers={"Content-Type":"application/json"})
    with urllib.request.urlopen(req, timeout=timeout) as response: return json.load(response)
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
        if not 2<=len(selected)<=4: errors.append("2-4 sculpt regions for "+target)
    for a in actions:
        if a.get("type")!="sculpt_region" or a.get("target") not in ("city","world"):
            errors.append("Only existing city/world sculpt_region actions"); continue
        bounds={"city":{"x":(-80,80),"z":(-75,100),"radius_x":(15,40),"radius_z":(18,55),"height":(1.5,4)},
                "world":{"x":(895,1105),"z":(-105,105),"radius_x":(22,65),"radius_z":(25,75),"height":(5,16)}}[a["target"]]
        for key,(lo,hi) in bounds.items():
            value=a.get("parameters",{}).get(key)
            if not isinstance(value,(int,float)) or not math.isfinite(value) or not lo<=value<=hi:
                errors.append(a["target"]+"."+key+" outside "+str((lo,hi)))
    return errors

def main():
    parser=argparse.ArgumentParser();parser.add_argument("--reference",required=True);parser.add_argument("--output",required=True)
    args=parser.parse_args()
    ref=pathlib.Path(args.reference); out=pathlib.Path(args.output); out.mkdir(parents=True,exist_ok=True)
    trials=list(ref.rglob("trial.json")); assert len(trials)==1,"Pinned native264 artifact ambiguous"
    trial=trials[0]; ev=trial.parent
    assert sha(trial)=="27d23f276fd390095c28783b643fdb5a1535efb44fe542974c4430f608d7dff0"
    identity=json.loads(trial.read_text(encoding="utf-8-sig"))
    images=[ev/"before-city-II-wide.png",ev/"before-world-REGION1-ENTIRE-2048x1152.png"]
    expected=["b0452915eb773be9060d899fb1c5fbe37a2280e46b0b0fc0a017d0facdb64e54",
              "58255a0c06e92d5441c37849b4713f94a4bdd10f1ac1059b5cb4773b10bbd827"]
    assert [sha(p) for p in images]==expected,"Real source screenshots differ"
    system=pathlib.Path("tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md")
    survey=json.loads((ev/"survey.json").read_text(encoding="utf-8-sig"))
    parameters=record({"x":number(-80,1105),"z":number(-105,105),"radius_x":number(15,65),
                       "radius_z":number(18,75),"height":number(1.5,16)})
    action=record({"id":{"type":"string"},"type":{"type":"string","enum":["sculpt_region"]},
        "target":{"type":"string","enum":["city","world"]},"rationale":{"type":"string"},"parameters":parameters})
    schema=record({"observed_defect":{"type":"string"},"design_intent":{"type":"string"},
        "asset_decision":{"type":"string","enum":["MODIFY"]},
        "reason_for_new_geometry":{"type":"string"},
        "planned_actions":{"type":"array","minItems":4,"maxItems":8,"items":action},
        "expected_visual_difference":{"type":"string"},"mobile_risks":{"type":"string"}})
    brief={
        "target":{"mode":"MODIFY","name":"existing city TerrainData and Region1 original mesh"},
        "actual_survey":survey,"source_scene_sha256":identity["source_scene_sha256"],
        "native_source_run":39658026450,
        "review":"Native264 changed38548 city samples but no convincing visible city relief; world still a flat lawn board. Forest additions alone did not solve composition. Change the terrain spatial composition, not lighting, camera, buildings, fog, rectangular plots or painted spots.",
        "brief":"Images are actual unchanged cityII and ENTIRE region. Choose 2-4 connected broad sculpted meadow forms for city and 2-4 stronger regional hills for world. This is a typed Unity terrain tool of the SAME artist: X/Z horizontal,Y up. Each sculpt_region adds height*exp(-((x-centerX)/radius_x)^2-((z-centerZ)/radius_z)^2). Choose exact centres/radii/heights from the images and measured coordinates, NOT random noise. City screenshot sees castle near(0,30), main road alongX0, river nearZ-23, sawmill west and barracks east. Put visible shoulders on either side of retained architecture. World empty north/west/south regions need a coherent regional silhouette and connected geology; forest(1065,42),city(1018,30),quarry(1064,-33),corrupt enemy(933,-23). Avoid lifting under POIs or existing quarry rocks. Preserve exact UV/roads, gameplay anchors and original mesh topology. Fixed implementation keeps road/river/foundations/border safe; decorative city foliage and rocks may follow the new terrain vertically, with gameplay colliders left unchanged. World retains all prop contact with broad keepouts. No new geometry/assets/buildings, no budget cost, no self-approval. These parameters drive real native vertices, then matched Unity images decide acceptance.",
        "strict_bounds":"CITY x[-80,80],z[-75,100],radius_x[15,40],radius_z[18,55],height[1.5,4]. WORLD x[895,1105],z[-105,105],radius_x[22,65],radius_z[25,75],height[5,16]. Exactly2-4 actions per target. Height is metres positive. 4-8 actions total. Meaningful short rationale each. Do not output source-space Blender coordinates."
    }
    messages=[{"role":"system","content":system.read_text(encoding="utf8")},
              {"role":"user","content":json.dumps(brief),"images":[base64.b64encode(p.read_bytes()).decode() for p in images]}]
    payload={"model":"qwen3-vl:8b-instruct-q4_K_M","stream":False,"format":schema,"messages":messages,
             "keep_alive":0,"options":{"temperature":.15,"num_ctx":8192,"num_predict":2400}}
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
                plan=json.loads(response["message"]["content"]);errors=validate(plan)
            except (ValueError,KeyError,TypeError) as e: errors=[str(e)]
            if response.get("done_reason")!="stop": errors.append("Incomplete model response")
            write(out/"artist-validation.json",{"attempt":attempt,"errors":errors,"geometry_executed":False})
            if not errors: break
            if attempt==2: raise RuntimeError("Local artist plan rejected: "+str(errors))
            payload["messages"]=[messages[0],dict(messages[1],content=json.dumps(brief)+"\nCorrect these validation errors: "+str(errors))]
        plan["source_scene_sha256"]=identity["source_scene_sha256"]
        plan["source_run"]=39658026450
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
