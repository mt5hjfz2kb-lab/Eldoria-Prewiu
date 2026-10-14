"""Autonomous local Qwen -> full Blender studio -> real render -> critique loop.

This is production authoring, NOT a scripted model generator or art approval.
Blender executes only a validated typed plan; Qwen supplies every asset choice.
No paid services, cloud inference, arbitrary Python, or new game scene changes.
"""
from __future__ import annotations
import argparse, base64, hashlib, json, pathlib, subprocess, sys, time, urllib.error, urllib.request
from contract import validate_plan, RejectedPlan

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

def schema_for_plan():
    # Dynamic JSON schema prevents an AI-coded Python payload while allowing
    # broad asset design and multi-step Blender geometry authoring.
    opnames=["primitive","mesh","duplicate","transform","sculpt","extrude","inset",
             "subdivide","bevel","modifier","boolean","material","uv","join","delete",
             "light","camera","render_settings","texture","asset_info",
             "native_operator","shader_graph","geometry_nodes"]
    action={"type":"object","properties":{
      "id":{"type":"string"},"op":{"type":"string","enum":opnames},
      "target":{"type":"string"},"reason":{"type":"string"},
      "params":{"type":"object"}},
      "required":["id","op","target","reason","params"]}
    return {"type":"object","properties":{
      "schema_version":{"type":"integer","const":2},
      "asset_id":{"type":"string"},"observed_defect":{"type":"string"},
      "design_intent":{"type":"string"},
      "asset_decision":{"type":"string","enum":["NEW","MODIFY","REUSE"]},
      "expected_visual_difference":{"type":"string"},
      "mobile_risks":{"type":"string"},
      "actions":{"type":"array","minItems":1,"maxItems":160,"items":action}},
      "required":["schema_version","asset_id","observed_defect","design_intent",
       "asset_decision","expected_visual_difference","mobile_risks","actions"]}

def contract_brief(max_actions):
    return (
      "You are the REAL 3D asset ARTIST. Your choices directly change Blender geometry; "
      "no engineer will invent building geometry on your behalf. Return strict JSON. "
      "Use schema_version=2, asset_id, observed_defect>=30 chars, "
      "design_intent>=30 chars, asset_decision NEW/MODIFY/REUSE, "
      "expected_visual_difference>=30 chars and actions with IDs, reasons>=15 chars, "
      "targets and params. Choose a cohesive asset silhouette and build it with multiple "
      "REAL operations, not just a single cube or a tint. You have up to "+str(max_actions)+
      " operations. Available:\n"
      "primitive: {shape:cube|cylinder|cone|plane|uv_sphere|ico_sphere|torus,"
      " location:[x,y,z], rotation:[rad,rad,rad], scale:[sx,sy,sz]};\n"
      "mesh: {vertices:[[x,y,z]...],faces:[[vertex-index,...]...]};\n"
      "duplicate: {source:name,location:[],rotation:[],scale:[]};\n"
      "transform: {location:[],rotation:[],scale:[],apply:bool};\n"
      "sculpt: {center:[x,y,z],radius:meters,delta:[x,y,z]};\n"
      "extrude: {faces:[faceIndex],offset:[x,y,z]};\n"
      "inset: {faces:[faceIndex],thickness:meters,depth:meters};\n"
      "subdivide: {faces:[faceIndex],cuts:integer}; bevel: {width:meters,segments:int};\n"
      "modifier: {kind:BEVEL|SUBSURF|SOLIDIFY|DECIMATE|REMESH|MIRROR|ARRAY|"
      "WEIGHTED_NORMAL|TRIANGULATE|SIMPLE_DEFORM|DISPLACE,options:{},apply:bool};\n"
      "boolean: {operation:DIFFERENCE|UNION|INTERSECT,operand:name};\n"
      "material: {base_color:[r,g,b,a],roughness:0..1,metallic:0..1,"
      "texture_image:generatedName OR texture_path:approvedRelativePath};\n"
      "texture: {method:NOISE|CHECKER,color_a:[r,g,b,a],color_b:[r,g,b,a],"
      "resolution:256,scale:meters}; uv:{method:smart|cube|sphere|cylinder};\n"
      "native_operator: {operator:safe native mesh/uv/object edit,kwargs:{},selection:ALL|FACE|EDGE|VERT,indices:[]}; "
      "shader_graph / geometry_nodes: {nodes:[{id,type,inputs:{}}],links:[{from,output,to,input}]}; "
      "join: {sources:[name,...]}; light:{kind:SUN|AREA|POINT|SPOT,energy:float,"
      "location:[x,y,z]}; camera:{location:[x,y,z],rotation:[radians],"
      "ortho_scale:meters}; render_settings:{engine:CYCLES,samples:16}; "
      "Only finite numeric parameters, no executable Python, no code strings, "
      "no external URLs or files, no self-approval. Build reusable medieval "
      "fantasy game-ready art with coherent materials and realistic contact, "
      "respecting target reference. All dimensions in meters and angles in radians. "
      "Objects previously created by an action can be targeted by later actions. "
      "Do not assume an unlisted source object exists.\n")

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

def ask_plan(args,reference,current_image,history,role,memory,trial):
    msg=(contract_brief(args.max_actions)
      +"\nASSET BRIEF: "+args.brief
      +"\nVerified old failure/lesson memory (not model weight training): "+memory[:9000]
      +"\nPrevious cycle or typed-tool feedback: "+history[-10000:]
      +"\nGenerate up to "+str(args.max_actions)+" concrete AI-chosen actions, not a static procedural script.")
    pictures=[image_data(reference)]
    if current_image:pictures.append(image_data(current_image))
    text=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
      {"role":"user","content":msg,"images":pictures}],args.max_tokens,schema=schema_for_plan())
    filename=trial/"qwen-raw.json";filename.write_text(text,encoding="utf-8")
    doc=json.loads(text);validate_plan(doc)
    if len(doc["actions"])>args.max_actions:raise RejectedPlan("AI operation budget exceeded")
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
    p.add_argument("--max-tokens",type=int,default=4500)
    p.add_argument("--prior-lessons",type=pathlib.Path)
    args=p.parse_args()
    if not 1<=args.max_cycles<=5 or not 1<=args.max_actions<=160:raise ValueError("Cycle/action limits")
    if not args.reference.is_file():raise FileNotFoundError("Real owner reference required")
    if args.source and not args.source.is_file():raise FileNotFoundError("Source .blend/GLB missing")
    if args.out.exists() and any(args.out.iterdir()):raise ValueError("Fresh output required")
    args.out.mkdir(parents=True,exist_ok=True)
    role=ROLE.read_text(encoding="utf-8")
    memory=JOURNAL.read_text(encoding="utf-8")[-6000:]
    if args.prior_lessons:memory+="\n"+args.prior_lessons.read_text(encoding="utf-8")[:6000]
    current=args.current_frame
    source=args.source
    history=""
    records=[]
    for cycle in range(1,args.max_cycles+1):
        trial=args.out/("cycle-"+str(cycle));trial.mkdir()
        for attempt in range(1,4):
            try:
                doc=ask_plan(args,args.reference,current,history,role,memory,trial)
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
        evaluation=critique(args,args.reference,stage/"before-blender.png",stage/"after-blender.png",role)
        (trial/"qwen-critique.json").write_text(json.dumps(evaluation,indent=2)+"\n")
        records.append({"cycle":cycle,"status":"ART_REVIEW_REQUIRED",
                        "plan_sha256":sha(candidate),
                        "output_blend_sha256":result["blend_sha256"],
                        "output_glb_sha256":result["glb_sha256"],
                        "matched_review_camera":result["matched_review_camera"],
                        "critique":evaluation})
        history+="\nRECORDED genuine Blender visual critique: "+json.dumps(evaluation)[:3500]
        source=stage/"artist-asset.blend";current=stage/"after-blender.png"
        if not evaluation.get("clearly_failed",True):break
    final={"state":"SOURCE_CANDIDATE_UNITY_AND_INDEPENDENT_ART_REVIEW_REQUIRED",
           "ai_authored_action_plans":len([r for r in records if r["status"]=="ART_REVIEW_REQUIRED"]),
           "source_authority_unchanged":True,"blender_real_execution":any(r["status"]=="ART_REVIEW_REQUIRED" for r in records),
           "art_approved":False,"unity_art_verified":False,"physical_mobile_validated":False,
           "model":args.model,"system_profile_sha256":hashlib.sha256(role.encode()).hexdigest(),
           "journal_sha256":sha(JOURNAL),"cycles":records}
    (args.out/"full-studio-result.json").write_text(json.dumps(final,indent=2)+"\n")
    print("ELDORIA_FULL_STUDIO_CYCLE",json.dumps({k:final[k] for k in
        ("state","ai_authored_action_plans","blender_real_execution","art_approved")}))
    if not final["blender_real_execution"]:raise RuntimeError("No executable AI-authored Blender asset; see cycle evidence")
if __name__=="__main__":main()
