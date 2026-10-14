"""Autonomous local Qwen -> full Blender studio -> real render -> critique loop.

This is production authoring, NOT a scripted model generator or art approval.
Blender executes only a validated typed plan; Qwen supplies every asset choice.
No paid services, cloud inference, arbitrary Python, or new game scene changes.
"""
from __future__ import annotations
import argparse, base64, hashlib, json, pathlib, subprocess, sys, time, urllib.error, urllib.request
from contract import validate_plan, RejectedPlan
from learning_context import transferable_lessons
from screen_impact import screen_impact

HERE=pathlib.Path(__file__).resolve().parent
REPO=HERE.parents[2]
ROLE=REPO/"tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md"
JOURNAL=REPO/"tools/art-direction/ARTIST_LEARNING_JOURNAL.md"

def sha(path):
    return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()

def image_data(file):
    from PIL import Image
    import io
    im=Image.open(file).convert("RGB")
    im.thumbnail((640,640))
    buf=io.BytesIO();im.save(buf,format="JPEG",quality=82)
    return base64.b64encode(buf.getvalue()).decode("ascii")

def request_ollama(endpoint, model, messages, max_tokens, timeout=600, schema=None):
    if endpoint != "http://127.0.0.1:11434/api/chat":
        raise ValueError("Only local zero-cost Ollama supported")
    payload={"model":model,"stream":True,"think":False,
             "messages":messages,"options":{"temperature":0.35,"num_ctx":16384,"num_predict":max_tokens}}
    if schema is not None:payload["format"]=schema
    call=urllib.request.Request(endpoint,headers={"Content-Type":"application/json"},
                                data=json.dumps(payload).encode("utf-8"))
    pieces=[];done=False
    with urllib.request.urlopen(call,timeout=timeout) as response:
        for line in response:
            if not line.strip():continue
            obj=json.loads(line)
            if obj.get("error"):raise RuntimeError(obj["error"])
            pieces.append(obj.get("message",{}).get("content",""))
            if obj.get("done"):done=True;break
    if not done:raise RuntimeError("Incomplete Qwen response; no plan accepted")
    return "".join(pieces)

def schema_for_plan(max_actions=6, source_exists=False):
    # A small local 2B visual model cannot simultaneously design 20 Blender
    # operations and correctly invent arbitrary face indices. Author from
    # scratch in bounded stages: geometry first, finishing on REAL next source.
    number={"type":"number","minimum":-15,"maximum":15}
    angle={"type":"number","minimum":-3.1416,"maximum":3.1416}
    dimension={"type":"number","minimum":0.15,"maximum":8}
    point={"type":"array","minItems":3,"maxItems":3,"items":number}
    scale={"type":"array","minItems":3,"maxItems":3,"items":dimension}
    orientation={"type":"array","minItems":3,"maxItems":3,"items":angle}
    actions=["primitive","mesh","duplicate","transform","sculpt","extrude","inset",
             "subdivide","bevel","modifier","boolean","material","uv","join","delete",
             "light","camera","render_settings","texture","asset_info",
             "native_operator","shader_graph","geometry_nodes"]
    if not source_exists:
        # Each action must create a real, bounded Blender mesh. Materials,
        # reuse, UV and Geometry Nodes are available in later source cycles.
        primitive_params={"type":"object","properties":{
           "shape":{"type":"string","enum":["cube","cylinder","cone","uv_sphere","ico_sphere"]},
           "location":point,"rotation":orientation,"scale":scale},
           "required":["shape","location","rotation","scale"],
           "additionalProperties":False}
        op={"type":"string","const":"primitive"}
        params=primitive_params
    else:
        op={"type":"string","enum":actions}
        params={"type":"object"}
    action={"type":"object","properties":{
       "id":{"type":"string","minLength":2,"maxLength":60},
       "op":op,"target":{"type":"string","minLength":2,"maxLength":70},
       "reason":{"type":"string","minLength":15,"maxLength":160},
       "params":params},
       "required":["id","op","target","reason","params"],
       "additionalProperties":False}
    return {"type":"object","properties":{
       "schema_version":{"type":"integer","const":2},
       "asset_id":{"type":"string","minLength":2,"maxLength":70},
       "observed_defect":{"type":"string","minLength":30,"maxLength":160},
       "design_intent":{"type":"string","minLength":30,"maxLength":200},
       "asset_decision":{"type":"string","enum":["MODIFY","REUSE"] if source_exists else ["NEW"]},
       "expected_visual_difference":{"type":"string","minLength":30,"maxLength":160},
       "mobile_risks":{"type":"string","maxLength":180},
       "actions":{"type":"array","minItems":2 if not source_exists else 1,
                  "maxItems":min(max_actions,8),"items":action}},
       "required":["schema_version","asset_id","observed_defect","design_intent",
         "asset_decision","expected_visual_difference","mobile_risks","actions"],
       "additionalProperties":False}

def contract_brief(max_actions, source_exists=False):
    # Short, exact model contract with the full vetted toolbox available once
    # an editable source exists. This avoids copying the irrelevant cliff data
    # from historic journals or expanding incomplete 20-action JSON documents.
    base=("You are the sole creative 3D artist. Blender is your workshop. "
      "Build the owner's CURRENT requested medieval asset, not terrain cliffs "
      "from old experiments. Never copy old object IDs, numeric coordinates, "
      "run numbers or failed actions. Do not approve yourself. Use strict JSON "
      "schema_version=2. Action IDs must be UNIQUE descriptive names; targets "
      "must be real objects you create, or in the verified source inventory. "
      "Parameters are metric numbers (radians for rotations). Each reason "
      "explains YOUR own creative geometry decision. "
      "Generate between 2 and "+str(max_actions)+" CONCISE operations: "
      "start by shaping the main silhouette, then include at least one "
      "independent secondary feature. No huge face/vertex arrays. ")
    if not source_exists:
        return (base+
          "The scene is EMPTY; asset_decision is NEW. THIS STAGE IS THE "
          "INITIAL VISIBLE SILHOUETTE ONLY; finishing operations follow after "
          "Blender creates actual source geometry. For ALL actions op=primitive "
          "with params EXACTLY {shape:cube|cylinder|cone|uv_sphere|ico_sphere,"
          "location:[x,y,z],rotation:[0,0,0],scale:[sx,sy,sz]}. "
          "Every action must CREATE A NEW DISTINCTLY NAMED MESH. "
          "Real medieval tower body height roughly 3-8 meters, roof above it, "
          "stone walls, one smaller defensive feature. Location coordinates "
          "must be between -15 and +15m, scale 0.15 to 8. All numbers realistic. "
          "Use TWO distinct shape types (e.g. cylinder body and cone roof), "
          "not repeated huge cube blobs. Never use operation mesh, duplicate "
          "or material while there is no source. Never use old cliff names. "
          "Compose original camera-visible components and preserve modularity.")
    return (base+
      "The existing source objects have been INSPECTED in real Blender. "
      "Choose Qwen-authored operations within safe full toolbox: primitive, "
      "mesh, duplicate, transform, sculpt, extrude, inset, subdivide, bevel, "
      "modifier, boolean, material, uv, join, native_operator, shader_graph "
      "and geometry_nodes; no code execution. Source object names must match "
      "the verified inventory. Finish the existing asset rather than restart.")

def run_blender(executable,plan,out,source,texture_root,review,transport='direct',mcp_addon=None,mcp_sha256=None):
    cmd=[executable,"-b","--python-exit-code","1","--python",
         str(HERE/"blender_toolbox.py"),"--",
         "--plan",str(plan),"--out",str(out),"--texture-root",str(texture_root)]
    if source:cmd+=["--source",str(source)]
    if review:cmd+=["--review",str(review)]
    cmd+=["--transport",transport]
    if transport=="blendermcp":
        if not mcp_addon or not mcp_sha256:raise ValueError("Pinned BlenderMCP addon and SHA-256 required")
        cmd+=["--mcp-addon",str(mcp_addon),"--mcp-sha256",mcp_sha256]
    result=subprocess.run(cmd,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=1200)
    (out.parent/(out.name+"-blender-log.txt")).write_text(result.stdout,encoding="utf-8")
    if result.returncode:raise RuntimeError("Real Blender execution failed; see output log: "+result.stdout[-1700:])

def ask_plan(args,reference,current_image,history,role,memory,trial,attempt_number,
             source_exists=False):
    msg=(contract_brief(args.max_actions,source_exists)
      +"\nCurrent asset requested by owner: "+args.brief[:500]
      +"\nVerified existing geometry or actual PREVIOUS cycle feedback: "+history[-4000:]
      +"\nNon-copyable empirical lessons: "+memory[:600]
      +"\nUse only YOUR NEW reasoning about the supplied owner reference. "
      "The examples in previous failures are warnings, not templates.")
    pictures=[image_data(reference)]
    if current_image:pictures.append(image_data(current_image))
    text=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
      {"role":"user","content":msg,"images":pictures}],
      args.max_tokens,schema=schema_for_plan(args.max_actions,source_exists),timeout=540)
    (trial/("qwen-raw-attempt-"+str(attempt_number)+".json")).write_text(text,encoding="utf-8")
    (trial/"qwen-raw.json").write_text(text,encoding="utf-8")
    doc=json.loads(text)
    validate_plan(doc)
    if len(doc["actions"])>args.max_actions:
        raise RejectedPlan("Too many Qwen actions in one bounded cycle")
    if not source_exists:
        if doc["asset_decision"]!="NEW":
            raise RejectedPlan("Fresh EMPTY scene requires Qwen to design a NEW asset")
        if not any(a["op"] in ("primitive","mesh") for a in doc["actions"]):
            raise RejectedPlan("No real new geometry for an empty scene")
        created=set()
        for act in doc["actions"]:
            if act["op"] in ("primitive","mesh"):
                if act["target"] in created:raise RejectedPlan("Duplicate new object")
                created.add(act["target"])
            elif act["op"] in ("material","uv","bevel","modifier"):
                if act["target"] not in created:
                    raise RejectedPlan("Qwen targeted nonexistent new mesh "+act["target"])
            elif act["op"]=="duplicate":
                if act["params"].get("source") not in created:
                    raise RejectedPlan("Qwen duplicate lacks an actual prior object")
                created.add(act["target"])
    return doc

def critique(args,reference,before,after,role):
    instruction=("Inspect these REAL renders, in order: target, before, after. "
      "Describe three exact defects in the AFTER asset, whether its silhouette "
      "and materials improved versus BEFORE, and one new concrete artistic "
      "approach if they did not. Do not invent Unity evidence. "
      "Return concise JSON with strings critique, geometry_next, material_next "
      "and bool clearly_failed. This critique never certifies art.")
    try:
        txt=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
         {"role":"user","content":instruction,"images":[image_data(reference),
                       image_data(before),image_data(after)]}],500,timeout=420)
        return json.loads(txt)
    except Exception as ex:return {"critique":"Visual critique unavailable: "+str(ex),"clearly_failed":True}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--brief",required=True)
    p.add_argument("--reference",type=pathlib.Path,required=True)
    p.add_argument("--source",type=pathlib.Path)
    p.add_argument("--current-frame",type=pathlib.Path)
    p.add_argument("--review",type=pathlib.Path)
    p.add_argument("--texture-root",type=pathlib.Path,default=REPO)
    p.add_argument("--out",type=pathlib.Path,required=True)
    p.add_argument("--model",default="qwen3-vl:4b-instruct")
    p.add_argument("--endpoint",default="http://127.0.0.1:11434/api/chat")
    p.add_argument("--blender",default="blender")
    p.add_argument("--transport",choices=("direct","blendermcp"),default="direct")
    p.add_argument("--mcp-addon",type=pathlib.Path)
    p.add_argument("--mcp-sha256")
    p.add_argument("--max-cycles",type=int,default=2)
    p.add_argument("--max-actions",type=int,default=32)
    p.add_argument("--max-tokens",type=int,default=1250)
    p.add_argument("--prior-lessons",type=pathlib.Path)
    args=p.parse_args()
    if not 1<=args.max_cycles<=5 or not 1<=args.max_actions<=160:raise ValueError("Cycle/action limits")
    if not args.reference.is_file():raise FileNotFoundError("Real owner reference required")
    if args.source and not args.source.is_file():raise FileNotFoundError("Source .blend/GLB missing")
    if args.out.exists() and any(args.out.iterdir()):raise ValueError("Fresh output required")
    args.out.mkdir(parents=True,exist_ok=True)
    canonical_role=ROLE.read_text(encoding="utf-8")
    role=(canonical_role[:530]+"\nThis task is a NEW or UPDATED complete asset, NOT the historical cliff task. Make independent geometry, original reusable subcomponents, materials, and verifiable actual Blender edits. Historical numbers are never your current plan.")
    # Genuine evidence-backed journal retrieval, sanitized of old rejected
    # cliff coordinates/IDs. The first 2B run replayed them and overflowed.
    lesson_proof=transferable_lessons(JOURNAL.read_text(encoding="utf-8"),args.brief,limit=600)
    (args.out/"journal-context-proof.json").write_text(json.dumps(lesson_proof,indent=2)+"\n",encoding="utf-8")
    memory=lesson_proof["prompt_text"]
    if args.prior_lessons:memory+="\n"+args.prior_lessons.read_text(encoding="utf-8")[:6000]
    current=args.current_frame
    source=args.source
    original_source_sha=sha(args.source) if args.source else None
    history=""
    if args.source:
        inspected=args.out/"source-inspection"
        command=[args.blender,"-b","--python-exit-code","1","--python",str(HERE/"inspect_source.py"),
                 "--","--source",str(args.source),"--out",str(inspected)]
        if args.review:command+=["--review",str(args.review)]
        check=subprocess.run(command,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=900)
        (args.out/"source-inspection-blender-log.txt").write_text(check.stdout,encoding="utf-8")
        if check.returncode:raise RuntimeError("Source inspection failed: "+check.stdout[-1000:])
        actual=json.loads((inspected/"source-inspection.json").read_text())
        if not actual["source_unchanged"] or actual["source_sha256"]!=original_source_sha:
            raise RuntimeError("Frozen original source identity changed")
        if not current:current=inspected/"source-preview-blender.png"
        verified_objects=[{"name":x["name"],"vertices":x["vertices"],"faces":x["faces"],
                          "world_location_m":x["world_location_m"],
                          "world_dimensions_m":x["world_dimensions_m"],
                          "materials":x["materials"],"uv_layers":x["uv_layers"]}
                         for x in actual["objects"][:80]]
        history+="\nVERIFIED FROZEN SOURCE BLENDER MESH INVENTORY: "+json.dumps(verified_objects)[:12000]
    records=[]
    for cycle in range(1,args.max_cycles+1):
        trial=args.out/("cycle-"+str(cycle));trial.mkdir()
        for attempt in range(1,4):
            try:
                doc=ask_plan(args,args.reference,current,history,role,memory,trial,attempt,source_exists=bool(source))
                break
            except (RejectedPlan,json.JSONDecodeError) as e:
                history+="\nA non-executable Qwen proposal was REJECTED: "+str(e)
                if attempt==3:raise
        candidate=trial/"artist-plan.json";candidate.write_text(json.dumps(doc,indent=2)+"\n")
        stage=trial/"output"
        try:
            run_blender(args.blender,candidate,stage,source,args.texture_root,args.review,args.transport,args.mcp_addon,args.mcp_sha256)
        except Exception as ex:
            records.append({"cycle":cycle,"status":"BLENDER_REJECTED","error":str(ex)[-800:]})
            history+="\nACTUAL BLENDER REJECTION: "+str(ex)[-1100:]
            continue
        result=json.loads((stage/"execution-proof.json").read_text())
        screen=screen_impact(stage/"before-blender.png",stage/"after-blender.png")
        (trial/"camera-screen-impact.json").write_text(json.dumps(screen,indent=2)+"\n")
        if not result["matched_review_camera"] or not screen["visually_present"]:
            records.append({"cycle":cycle,"status":"SCREENSPACE_REJECTED","screen_impact":screen})
            history+=("\nVISUAL CAMERA FAILURE: actual Blender A/B changed "+
                      str(screen["changed_fraction"])+
                      " of the screen, too little for the player camera. "
                      "The next Qwen plan must change the core silhouette and not simply tweak materials.")
            continue
        evaluation=critique(args,args.reference,stage/"before-blender.png",stage/"after-blender.png",role)
        (trial/"qwen-critique.json").write_text(json.dumps(evaluation,indent=2)+"\n")
        records.append({"cycle":cycle,"status":"ART_REVIEW_REQUIRED",
                        "plan_sha256":sha(candidate),
                        "output_blend_sha256":result["blend_sha256"],
                        "output_glb_sha256":result["glb_sha256"],
                        "matched_review_camera":result["matched_review_camera"],
                        "critique":evaluation})
        history+="\nRECORDED genuine Blender visual critique: "+json.dumps(evaluation)[:3500]
        # Genuine Blender inventory is the next model's source of truth, not
        # invented object names or a fixed archetype template.
        catalog=json.loads((stage/"reusable-asset-catalog.json").read_text())
        anchors=[{"name":o["name"],"faces":o["faces"],"dimensions_m":o["dimensions_m"],
                  "pivot_m":o["pivot_m"],"materials":o["materials"]}
                 for o in catalog["components"][:60]]
        history+="\nSOURCE-VERIFIED CURRENT BLENDER ASSET INVENTORY: "+json.dumps(anchors)[:7000]
        source=stage/"artist-asset.blend";current=stage/"after-blender.png"
        if not evaluation.get("clearly_failed",True):break
    if args.source and sha(args.source)!=original_source_sha:raise RuntimeError("Original frozen source mutated")
    final={"state":"SOURCE_CANDIDATE_UNITY_AND_INDEPENDENT_ART_REVIEW_REQUIRED",
           "ai_authored_action_plans":len([r for r in records if r["status"]=="ART_REVIEW_REQUIRED"]),
           "source_authority_unchanged":True,"frozen_source_sha256":original_source_sha,"blender_real_execution":any(r["status"]=="ART_REVIEW_REQUIRED" for r in records),
           "art_approved":False,"unity_art_verified":False,"physical_mobile_validated":False,
           "model":args.model,"system_profile_sha256":hashlib.sha256(role.encode()).hexdigest(),
           "journal_sha256":sha(JOURNAL),"cycles":records}
    (args.out/"full-studio-result.json").write_text(json.dumps(final,indent=2)+"\n")
    print("ELDORIA_FULL_STUDIO_CYCLE",json.dumps({k:final[k] for k in
        ("state","ai_authored_action_plans","blender_real_execution","art_approved")}))
    if not final["blender_real_execution"]:raise RuntimeError("No executable AI-authored Blender asset; see cycle evidence")
if __name__=="__main__":main()
