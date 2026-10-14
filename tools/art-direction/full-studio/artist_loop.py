"""Autonomous local Qwen -> full Blender studio -> real render -> critique loop.

This is production authoring, NOT a scripted model generator or art approval.
Blender executes only a validated typed plan; Qwen supplies every asset choice.
No paid services, cloud inference, arbitrary Python, or new game scene changes.
"""
from __future__ import annotations
import argparse, base64, hashlib, json, pathlib, subprocess, sys, time, urllib.error, urllib.request
from contract import validate_plan, RejectedPlan, safe_action_id
from learning_context import transferable_lessons
from screen_impact import screen_impact
from composition_health import check_new_silhouette,CompositionRejected

HERE=pathlib.Path(__file__).resolve().parent
REPO=HERE.parents[2]
ROLE=REPO/"tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md"
JOURNAL=REPO/"tools/art-direction/ARTIST_LEARNING_JOURNAL.md"
CASEBOOK=HERE/"learning_cases.json"

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

def schema_for_plan(max_actions=6, source_exists=False, phase='full', category='architecture', known_objects=None):
    # Let a small local visual model choose actual geometry/materials in
    # separate valid Blender MICRO-OPERATIONS. The schema, not a script,
    # prevents incompatible parameter keys from being accepted.
    max_location=180 if category=="terrain" else 35 if category=="vegetation" else 15
    max_scale=120 if category=="terrain" else 25 if category=="vegetation" else 8
    pos={"type":"number","minimum":-max_location,"maximum":max_location}
    scale_dim={"type":"number","minimum":0.05,"maximum":max_scale}
    unit={"type":"number","minimum":0,"maximum":1}
    point={"type":"array","items":pos,"minItems":3,"maxItems":3}
    scale={"type":"array","items":scale_dim,"minItems":3,"maxItems":3}
    rgba={"type":"array","items":unit,"minItems":4,"maxItems":4}
    angle={"type":"array","items":{"type":"number","minimum":-3.1416,"maximum":3.1416},"minItems":3,"maxItems":3}
    all_ops=["primitive","mesh","duplicate","transform","sculpt","extrude",
        "inset","subdivide","bevel","modifier","boolean","material","uv",
        "join","delete","light","camera","render_settings","texture",
        "asset_info","native_operator","shader_graph","geometry_nodes","import_asset"]
    if not source_exists:
        action_op={"type":"string","const":"primitive"}
        params={"type":"object","properties":{
          "shape":{"type":"string","enum":["cube","cylinder","cone","uv_sphere","ico_sphere","plane"]},
          "location":point,"rotation":angle,"scale":scale},
          "required":["shape","location","rotation","scale"],
          "additionalProperties":False}
    elif phase=="geometry":
        action_op={"type":"string","const":"transform"}
        params={"type":"object","properties":{"location":point,"scale":scale},
                "required":["location","scale"],"additionalProperties":False}
    elif phase=="mesh_edit":
        # A LOCAL visual model chooses a physically EXISTING mesh landmark and
        # sculpt direction/strength; Blender resolves the actual vertex.
        # This avoids 2B JSON always emitting zero-vector brushes without
        # substituting scripted artistic choices.
        action_op={"type":"string","const":"sculpt_anchor"}
        params={"type":"object","properties":{
          "anchor":{"type":"string","enum":["x_min","x_max","y_min","y_max","z_min","z_max"]},
          "direction":{"type":"string","enum":["+X","-X","+Y","-Y","+Z","-Z"]},
          "distance":{"type":"number","minimum":0.65,"maximum":1.8},
          "radius":{"type":"number","minimum":1.5,"maximum":8}},
          "required":["anchor","direction","distance","radius"],"additionalProperties":False}
    elif phase=="surface":
        action_op={"type":"string","const":"material"}
        params={"type":"object","properties":{
          "base_color":rgba,
          "roughness":{"type":"number","minimum":0.45,"maximum":1},
          "metallic":{"type":"number","minimum":0,"maximum":0.35}},
          "required":["base_color","roughness","metallic"],
          "additionalProperties":False}
    elif phase=="detail":
        action_op={"type":"string","const":"bevel"}
        params={"type":"object","properties":{
          "width":{"type":"number","minimum":0.01,"maximum":0.22},
          "segments":{"type":"integer","minimum":1,"maximum":4},
          "apply":{"type":"boolean"}},
          "required":["width","segments","apply"],"additionalProperties":False}
    else:
        action_op={"type":"string","enum":all_ops}
        params={"type":"object"}
    target_schema={"type":"string","minLength":2,"maxLength":70}
    if source_exists and known_objects and phase in ("geometry","mesh_edit","surface","detail"):
        target_schema={"type":"string","enum":known_objects}
    action={"type":"object","properties":{
        "id":{"type":"string","minLength":2,"maxLength":60},
        "op":action_op,
        "target":target_schema,
        "reason":{"type":"string","minLength":15,"maxLength":170},
        "params":params},
        "required":["id","op","target","reason","params"],
        "additionalProperties":False}
    if source_exists and phase=="full" and known_objects:
        action=full_source_tool_variants(known_objects,max_location)
    return {"type":"object","properties":{
        "schema_version":{"type":"integer","const":2},
        "asset_id":{"type":"string","minLength":2,"maxLength":70},
        "observed_defect":{"type":"string","minLength":30,"maxLength":180},
        "design_intent":{"type":"string","minLength":30,"maxLength":200},
        "asset_decision":{"type":"string","enum":["MODIFY","REUSE"] if source_exists else ["NEW"]},
        "expected_visual_difference":{"type":"string","minLength":30,"maxLength":200},
        "mobile_risks":{"type":"string","maxLength":180},
        "actions":{"type":"array","minItems":2 if not source_exists else 1,
                   "maxItems":min(max_actions,8),"items":action}},
        "required":["schema_version","asset_id","observed_defect","design_intent",
                    "asset_decision","expected_visual_difference","mobile_risks","actions"],
        "additionalProperties":False}

def full_source_tool_variants(existing_objects,max_location):
    """A true per-tool JSON signature instead of generic unconstrained params.

    The AI still selects every operation, target and ARTISTIC NUMBER. Typing
    only rejects fake Blender operators, made-up mesh targets and wrong options.
    It does not synthesize vertices, adjust materials or substitute designs.
    """
    from extended_ops import SAFE_SHADER_NODES, SAFE_GEOMETRY_NODES, SAFE_NATIVE_OPS
    existing={"type":"string","enum":list(existing_objects)}
    free_name={"type":"string","minLength":2,"maxLength":70}
    numerical={"type":"number","minimum":-max_location,"maximum":max_location}
    xyz={"type":"array","minItems":3,"maxItems":3,"items":numerical}
    size={"type":"array","minItems":3,"maxItems":3,
          "items":{"type":"number","minimum":0.0001,"maximum":120}}
    unit={"type":"number","minimum":0,"maximum":1}
    rgba={"type":"array","minItems":4,"maxItems":4,"items":unit}
    positive={"type":"number","exclusiveMinimum":0,"maximum":100}
    indexes={"type":"array","minItems":1,"maxItems":60,
             "items":{"type":"integer","minimum":0,"maximum":200000}}
    def obj(props,required):
        return {"type":"object","additionalProperties":False,
                "properties":props,"required":required}
    def action(op,params,required,new_object=False):
        return obj({
          "id":{"type":"string","minLength":2,"maxLength":50},
          "op":{"const":op},
          "target":free_name if new_object else existing,
          "reason":{"type":"string","minLength":15,"maxLength":170},
          "params":obj(params,required),
        },["id","op","target","reason","params"])
    entries=[]
    entries.append(action("material",{
        "base_color":rgba,"roughness":unit,"metallic":unit
    },["base_color","roughness","metallic"]))
    entries.append(action("sculpt_anchor",{
        "anchor":{"type":"string","enum":["x_min","x_max","y_min","y_max","z_min","z_max"]},
        "direction":{"type":"string","enum":["+X","-X","+Y","-Y","+Z","-Z"]},
        "distance":{"type":"number","minimum":0.65,"maximum":1.8},
        "radius":{"type":"number","minimum":1.5,"maximum":8},
    },["anchor","direction","distance","radius"]))
    entries.append(action("sculpt",{
        "center":xyz,"delta":xyz,"radius":positive,
    },["center","delta","radius"]))
    entries.append(action("bevel",{
        "width":positive,"segments":{"type":"integer","minimum":1,"maximum":6},
        "apply":{"type":"boolean"},
    },["width","segments","apply"]))
    entries.append(action("uv",{
        "method":{"type":"string","enum":["smart","cube","sphere","cylinder"]},
    },["method"]))
    entries.append(action("modifier",{
        "kind":{"type":"string","enum":["BEVEL","SUBSURF","SOLIDIFY","DECIMATE",
                   "REMESH","MIRROR","ARRAY","WEIGHTED_NORMAL","TRIANGULATE",
                   "SIMPLE_DEFORM","DISPLACE"]},
        "options":{"type":"object","additionalProperties":False,"properties":{
            "width":positive,"segments":{"type":"integer","minimum":1,"maximum":6},
            "levels":{"type":"integer","minimum":0,"maximum":3},
            "render_levels":{"type":"integer","minimum":0,"maximum":3},
            "thickness":numerical,"ratio":unit,"voxel_size":positive,
            "count":{"type":"integer","minimum":2,"maximum":64},
            "offset":xyz,"axis":{"type":"string","enum":["X","Y","Z"]},
            "angle":numerical,"strength":numerical,"texture_scale":positive
        }},
        "apply":{"type":"boolean"},
    },["kind","options","apply"]))
    entries.append(action("native_operator",{
        "operator":{"type":"string","enum":sorted(SAFE_NATIVE_OPS)},
        "kwargs":{"type":"object","additionalProperties":False},
        "selection":{"const":"ALL"},
    },["operator","kwargs","selection"]))
    for op,params,required in [
        ("extrude",{"faces":indexes,"offset":xyz},["faces","offset"]),
        ("inset",{"faces":indexes,"thickness":positive},["faces","thickness"]),
        ("subdivide",{"faces":indexes,"cuts":{"type":"integer","minimum":1,"maximum":4}},["faces","cuts"]),
        ("boolean",{"operand":existing,
                    "operation":{"type":"string","enum":["DIFFERENCE","UNION","INTERSECT"]},
                    "delete_operand":{"type":"boolean"}},["operand","operation"]),
        ("transform",{"location":xyz,"scale":size},["location","scale"]),
    ]:
        entries.append(action(op,params,required))
    entries.append(action("primitive",{
        "shape":{"type":"string","enum":["cube","cylinder","cone","plane","uv_sphere","ico_sphere","torus"]},
        "location":xyz,"rotation":xyz,"scale":size
    },["shape","location","rotation","scale"],new_object=True))
    entries.append(action("duplicate",{
        "source":existing,"location":xyz,"rotation":xyz,"scale":size
    },["source","location","rotation","scale"],new_object=True))
    nodes_base={"type":"object","additionalProperties":False,"properties":{
        "id":{"type":"string","minLength":1,"maxLength":70},
        "type":{"type":"string"},
        "inputs":{"type":"object"},
    },"required":["id","type"]}
    links={"type":"array","minItems":0,"maxItems":60,"items":obj({
        "from":{"type":"string"},"to":{"type":"string"},
        "output":{"type":"string"},"input":{"type":"string"}
    },["from","to","output","input"])}
    for op,allowed in [("shader_graph",SAFE_SHADER_NODES),
                       ("geometry_nodes",SAFE_GEOMETRY_NODES)]:
        node_schema=dict(nodes_base)
        node_schema["properties"]=dict(nodes_base["properties"])
        node_schema["properties"]["type"]={"type":"string","enum":sorted(allowed)}
        entries.append(action(op,{
           "nodes":{"type":"array","minItems":2,"maxItems":18,"items":node_schema},
           "links":links,
        },["nodes","links"]))
    return {"oneOf":entries}


def contract_brief(max_actions,source_exists=False,phase="full",category="architecture"):
    base=("You are the sole 3D ARTIST. Every operation, target object and "
      "numeric value is chosen by you and physically executed in Blender. "
      "Return strict schema_version=2 JSON; each unique action needs op, "
      "target, reason and params. Your current asset brief/reference and real "
      "source inventory are authoritative, NOT historical cliff scripts. "
      "Keep plans short: at most "+str(max_actions)+" actions. "
      "No Python code, placeholders, model approval or artificial confirmations. ")
    if not source_exists and category=="terrain":
        return base+"Create a real contiguous terrain PATCH with varied elevations, cohesive silhouette and deliberate base. Every op=primitive; params={shape,location:[x,y,z],rotation:[0,0,0],scale:[x,y,z]}. Your later cycles will sculpt and shade it. You decide all original dimensions."
    if not source_exists and category=="vegetation":
        return base+"CREATE actual aligned trunk and canopy meshes from scratch. Every op=primitive with shape,location,rotation,scale. YOU select physical proportions. Later stages add materials."
    if not source_exists and category=="prop":
        return base+"CREATE a recognizable multi-part medieval prop with two or more meshes. Every op=primitive with shape,location,rotation,scale; all creative choices are YOURS."
    if not source_exists and category=="character":
        return base+"CREATE static humanoid blockout geometry only, proportions chosen by YOU. Do not pretend there is rigging/animation. Every op=primitive with shape,location,rotation,scale."
    if not source_exists:
        return base+"EMPTY scene: asset_decision=NEW, every op=primitive. Each has params EXACTLY {shape:cylinder|cone|cube|uv_sphere|ico_sphere,location:[x,y,z],rotation:[0,0,0],scale:[sx,sy,sz]}. Model a separate, vertically aligned masonry body and believable roof. Do not bury objects, offset roof or overlap identical solids. All parts must be original and camera-visible."
    if phase=="mesh_edit":
        return base+"REAL SOURCE MESH EDIT ONLY: every op=sculpt_anchor, target MUST match the literal existing object name (including a middle dot, if any). For EACH action params EXACTLY {anchor:x_min|x_max|y_min|y_max|z_min|z_max,direction:+X|-X|+Y|-Y|+Z|-Z,distance:0.65..1.8,radius:1.5..8}. INHERITED VERTEX EDIT: Read inspected local_bounds_min_m / local_bounds_max_m and LOCAL EXTREMA and the owner reference: decide which SOURCE extremum is artistically defective and which direction, distance and radius would improve its silhouette. The executor will resolve the chosen extremal vertex in actual Blender geometry and deform nearby inherited vertices. NO zero delta, NO new primitives, NO fake object transforms. Make a substantial but controlled visible silhouette edit at a landmark consistent with the reference. Preserve recognisable architecture; choose direction and impact deliberately rather than minimum values. Every action must make a visible source-mesh change; do not repeat identical actions."
    if phase=="geometry":
        return base+"THIS STAGE ONLY FIXES GEOMETRY of the real existing Blender meshes. For EACH action op=transform; choose one EXACTLY NAMED source object from verified inventory. params must contain BOTH location:[x,y,z] and scale:[sx,sy,sz], no extra keys. FIX the formerly overlapping and horizontally misaligned roof and tower by placing objects at DIFFERENT HEIGHTS. Y coordinates must not be copied blindly. Blender operation is absolute position and absolute scale. NEVER wrap values in a transform sub-object."
    if phase=="surface":
        return base+"THIS STAGE ONLY AUTHORS REAL PBR MATERIALS on named existing Blender meshes. For EACH action op=material; choose exact source object name, params EXACTLY {base_color:[r,g,b,a],roughness:0.45..1,metallic:0..0.35}. Use plausible medieval limestone, terracotta, oak and aged iron according to each component; different real objects must receive coherent but distinct artist-picked colours. NEVER assign material twice to the SAME inherited mesh: a later material overwrites the previous one. If the source has ONE mesh, choose ONE deliberate material pass; for genuine variation choose shader_graph in full mode or source-grounded multiple distinct meshes. Do not use transform or invent object IDs."
    if phase=="detail":
        return base+"THIS STAGE ONLY ADDS REAL MESH EDGE DETAILS. For EACH action op=bevel; choose existing source mesh from verified inventory, params EXACTLY {width:0.01..0.22,segments:1..4,apply:true}. Avoid beveling huge terrain unnecessarily. You determine which edges/details merit finishing."
    return base+(
        "FULL PROFESSIONAL ARTIST MODE on verified source: choose which REAL Blender "
        "tools to use based on the observed visual defect, NOT a fixed primitive recipe. "
        "Every target must be an inspected existing mesh or an object created by an "
        "earlier action in this plan. Tools may be combined in sequence. "
        "Exact params contract examples (numbers below only illustrate TYPES, "
        "they are NOT proposed artistic values): "
        "material={base_color:[r,g,b,a],roughness:0..1,metallic:0..1}; "
        "sculpt={center:[x,y,z],delta:[dx,dy,dz],radius:positive}; "
        "bevel={width:positive,segments:1..4,apply:true}; "
        "uv={method:smart|cube|sphere|cylinder}; "
        "extrude={faces:[verified_face_indices],offset:[dx,dy,dz]}; "
        "inset={faces:[verified_face_indices],thickness:positive}; "
        "modifier={kind:BEVEL|SUBSURF|SOLIDIFY|DECIMATE|REMESH|MIRROR|ARRAY|"
        "WEIGHTED_NORMAL|TRIANGULATE|SIMPLE_DEFORM|DISPLACE,options:{},apply:false}; "
        "native_operator={operator:object.shade_smooth|object.shade_flat|"
        "mesh.poke|mesh.subdivide|mesh.bevel|uv.smart_project,kwargs:{},selection:ALL}; "
        "shader_graph and geometry_nodes each require nodes:[{id,type,inputs:{}}] "
        "and links:[{from,to,output,input}] with a corresponding Output node; "
        "choose actual supported Blender node types, never Python snippets. "
        "Use mesh with authored vertices/faces, boolean with operand/operation, "
        "duplicate, texture and verified import_asset when useful. "
        "Select the operation mix that addresses SILHOUETTE, structure, depth, "
        "PBR and meaningful camera-visible detail; do not claim unsupported "
        "rigging/animation or substitute an unrelated building. "
        "If this is the second cycle after an empty scene, the existing primitives "
        "were ONLY a blockout: improve actual forms/materials with genuine art decisions."
    )

def run_blender(executable,plan,out,source,texture_root,review,asset_catalog=None,transport='direct',mcp_addon=None,mcp_sha256=None):
    cmd=[executable,"-b","--python-exit-code","1","--python",
         str(HERE/"blender_toolbox.py"),"--",
         "--plan",str(plan),"--out",str(out),"--texture-root",str(texture_root)]
    if source:cmd+=["--source",str(source)]
    if review:cmd+=["--review",str(review)]
    if asset_catalog:cmd+=["--asset-catalog",str(asset_catalog)]
    cmd+=["--transport",transport]
    if transport=="blendermcp":
        if not mcp_addon or not mcp_sha256:raise ValueError("Pinned BlenderMCP addon and SHA-256 required")
        cmd+=["--mcp-addon",str(mcp_addon),"--mcp-sha256",mcp_sha256]
    result=subprocess.run(cmd,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=1200)
    (out.parent/(out.name+"-blender-log.txt")).write_text(result.stdout,encoding="utf-8")
    if result.returncode:raise RuntimeError("Real Blender execution failed; see output log: "+result.stdout[-1700:])

def repair_noncreative_action_ids(doc):
    """Correct only non-artistic log IDs; NEVER repair geometric decisions.

    Qwen occasionally returns duplicate, empty or unsafe tracking IDs. These
    are provenance labels, not Blender object names or artist decisions.
    The verbatim model response must still be saved separately for audit.
    """
    if not isinstance(doc, dict) or not isinstance(doc.get("actions"), list):
        return doc, []
    used = set()
    repairs = []
    for index, action in enumerate(doc["actions"]):
        if not isinstance(action, dict):
            continue  # semantic validation will reject malformed actions
        original = action.get("id")
        valid = safe_action_id(original) and original not in used
        if valid:
            used.add(original)
            continue
        candidate = f"step_{index + 1:03d}"
        while candidate in used or any(
            isinstance(a, dict) and a.get("id") == candidate
            for a in doc["actions"][index+1:]
        ):
            candidate += "_"
        action["id"] = candidate
        used.add(candidate)
        repairs.append({"index": index, "original": original, "repaired": candidate,
                        "scope": "NON_ARTISTIC_LOG_IDENTIFIER_ONLY"})
    return doc, repairs

def reject_overwriting_material_actions(actions):
    """Reject cosmetic pseudo-complexity: subsequent materials replace previous ones."""
    seen=set()
    for action in actions:
        if action.get("op") != "material":
            continue
        target=action.get("target")
        if target in seen:
            raise RejectedPlan(
                "Repeated material assignment to SAME real mesh "+
                str(target)+" only overwrites the previous material. "+
                "Choose ONE artist-authored PBR material per mesh or a deliberate "+
                "source-grounded shader_graph for actual regional variation."
            )
        seen.add(target)


def ask_plan(args,reference,current_image,history,role,memory,trial,attempt_number,
             source_exists=False,known_objects=None):
    msg=(contract_brief(args.max_actions,source_exists,args.phase,args.asset_category)
      +"\nCurrent asset requested by owner: "+args.brief[:500]
      +"\nVerified existing geometry or actual PREVIOUS cycle feedback: "+history[-4000:]
      +"\nNon-copyable empirical lessons: "+memory[:600]
      +"\nUse only YOUR NEW reasoning about the supplied owner reference. "
      "The examples in previous failures are warnings, not templates.")
    if source_exists and args.require_inherited_geometry_edit:
        msg+=("\\nOWNER REQUIREMENT: produce a REAL MESH DATA CHANGE to the "
              "verified inherited source. Include at least ONE genuine "
              "sculpt_anchor/sculpt/extrude/inset/subdivide/boolean action with "
              "your OWN source-grounded parameters. Two bevel modifiers, UV and "
              "PBR recolours alone are NOT sufficient. For modifiers, BEVEL "
              "supports only options.width and options.segments; ARRAY alone "
              "supports options.count and options.offset. NEVER use kind=INSET "
              "or kind=EXTRUDE: inset and extrude are DISTINCT ops. "
              "Any unsupported option or imaginary target is rejected before Blender.")
    pictures=[image_data(reference)]
    if current_image:pictures.append(image_data(current_image))
    text=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
      {"role":"user","content":msg,"images":pictures}],
      args.max_tokens,schema=schema_for_plan(args.max_actions,source_exists,args.phase,args.asset_category,known_objects),timeout=540)
    (trial/("qwen-raw-attempt-"+str(attempt_number)+".json")).write_text(text,encoding="utf-8")
    (trial/"qwen-raw.json").write_text(text,encoding="utf-8")
    doc=json.loads(text)
    doc, id_repairs = repair_noncreative_action_ids(doc)
    if id_repairs:
        (trial/("metadata-repairs-attempt-"+str(attempt_number)+".json")).write_text(
            json.dumps({"raw_response_sha256":hashlib.sha256(text.encode("utf-8")).hexdigest(),
                        "scope":"NON_ARTISTIC_LOG_IDENTIFIERS_ONLY",
                        "repairs":id_repairs},indent=2)+"\n",encoding="utf-8")
    validate_plan(doc)
    reject_overwriting_material_actions(doc["actions"])
    if source_exists and args.require_inherited_geometry_edit:
        structural={"sculpt_anchor","sculpt","extrude","inset","subdivide","boolean"}
        if not any(x["op"] in structural for x in doc["actions"]):
            raise RejectedPlan(
                "Owner requested REAL inherited source-mesh redesign; BEVEL, MATERIAL, "
                "UV, object transform or unapplied modifiers alone do NOT count. "
                "Select an artist-chosen sculpt_anchor, sculpt, extrude, inset, "
                "subdivide or boolean operation on an inspected existing object."
            )
    if not source_exists:
        check_new_silhouette(doc,args.asset_category)
    if len(doc["actions"])>args.max_actions:
        raise RejectedPlan("Too many Qwen actions in one bounded cycle")
    if source_exists and args.phase=="mesh_edit":
        if not known_objects or any(a["op"]!="sculpt_anchor" or a["target"] not in known_objects
                                    for a in doc["actions"]):
            raise RejectedPlan("Inherited mesh edit must address inspected source objects")
        signatures=[(a["target"],a["params"]["anchor"],a["params"]["direction"])
                    for a in doc["actions"]]
        if len(set(signatures))!=len(signatures):
            raise RejectedPlan("Duplicate source sculpt decisions; choose independent features")
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
        critique_schema={"type":"object","properties":{
          "critique":{"type":"string"},"geometry_next":{"type":"string"},
          "material_next":{"type":"string"},"clearly_failed":{"type":"boolean"}},
          "required":["critique","geometry_next","material_next","clearly_failed"],
          "additionalProperties":False}
        txt=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
         {"role":"user","content":instruction,"images":[image_data(reference),
                       image_data(before),image_data(after)]}],350,timeout=420,schema=critique_schema)
        return json.loads(txt)
    except Exception as ex:return {"critique":"Visual critique unavailable: "+str(ex),"clearly_failed":True}

def should_continue_after_critique(cycle_number, max_cycles, started_empty, evaluation):
    """One visible empty-scene blockout does not equal completed artistic authoring.

    Existing asset work may stop when Qwen judges it adequate, subject to the
    separate independent review gate. New assets with >1 cycle MUST receive
    at least one source-grounded finishing pass, irrespective of Qwen self praise.
    """
    return cycle_number < max_cycles and (
        evaluation.get("clearly_failed", True) or
        (started_empty and cycle_number == 1)
    )


def main():
    p=argparse.ArgumentParser()
    p.add_argument("--brief",required=True)
    p.add_argument("--reference",type=pathlib.Path,required=True)
    p.add_argument("--source",type=pathlib.Path)
    p.add_argument("--current-frame",type=pathlib.Path)
    p.add_argument("--review",type=pathlib.Path)
    p.add_argument("--texture-root",type=pathlib.Path,default=REPO)
    p.add_argument("--asset-catalog",type=pathlib.Path)
    p.add_argument("--out",type=pathlib.Path,required=True)
    p.add_argument("--model",default="qwen3-vl:4b-instruct")
    p.add_argument("--endpoint",default="http://127.0.0.1:11434/api/chat")
    p.add_argument("--blender",default="blender")
    p.add_argument("--transport",choices=("direct","blendermcp"),default="direct")
    p.add_argument("--mcp-addon",type=pathlib.Path)
    p.add_argument("--mcp-sha256")
    p.add_argument("--require-inherited-geometry-edit",action="store_true",
                   help="Require physically changed original mesh data, not modifiers/material-only gestures")
    p.add_argument("--max-cycles",type=int,default=2)
    p.add_argument("--asset-category",choices=("architecture","terrain","vegetation","prop","character"),default="architecture")
    p.add_argument("--phase",choices=("full","geometry","mesh_edit","surface","detail"),default="full")
    p.add_argument("--max-actions",type=int,default=32)
    p.add_argument("--max-tokens",type=int,default=1250)
    p.add_argument("--prior-lessons",type=pathlib.Path)
    args=p.parse_args()
    if not 1<=args.max_cycles<=5 or not 1<=args.max_actions<=160:raise ValueError("Cycle/action limits")
    if not args.reference.is_file():raise FileNotFoundError("Real owner reference required")
    if args.source and not args.source.is_file():raise FileNotFoundError("Source .blend/GLB missing")
    if args.require_inherited_geometry_edit and not args.source:
        raise ValueError("Inherited-source edit requirement needs an existing BLEND/GLB")
    if args.out.exists() and any(args.out.iterdir()):raise ValueError("Fresh output required")
    args.out.mkdir(parents=True,exist_ok=True)
    canonical_role=ROLE.read_text(encoding="utf-8")
    role=(canonical_role[:530]+"\nThis task is a NEW or UPDATED complete asset, NOT the historical cliff task. Make independent geometry, original reusable subcomponents, materials, and verifiable actual Blender edits. Historical numbers are never your current plan.")
    # Genuine evidence-backed journal retrieval, sanitized of old rejected
    # cliff coordinates/IDs. The first 2B run replayed them and overflowed.
    lesson_proof=transferable_lessons(JOURNAL.read_text(encoding="utf-8"),args.brief,limit=600)
    (args.out/"journal-context-proof.json").write_text(json.dumps(lesson_proof,indent=2)+"\n",encoding="utf-8")
    casebook=json.loads(CASEBOOK.read_text(encoding="utf-8"))
    if casebook.get("schema_version")!=1:raise ValueError("Corrupt Qwen artist casebook")
    recent=[case["safe_lesson"] for case in casebook["cases"][-4:] if isinstance(case.get("safe_lesson"),str)]
    memory=(" | ".join(recent)+" | "+lesson_proof["prompt_text"])[:600]
    (args.out/"learning-casebook-proof.json").write_text(json.dumps({
        "casebook_sha256":sha(CASEBOOK),
        "evidence_run_ids":[case.get("run_id") for case in casebook["cases"]],
        "model_weight_training":False,
        "verbatim_historical_actions_sent":False
    },indent=2)+"\n",encoding="utf-8")
    if args.prior_lessons:memory+="\n"+args.prior_lessons.read_text(encoding="utf-8")[:6000]
    current=args.current_frame
    source=args.source
    started_empty=args.source is None
    original_source_sha=sha(args.source) if args.source else None
    history=""
    known_objects=[]
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
                          "local_bounds_min_m":x["local_bounds_min_m"],
                          "local_bounds_max_m":x["local_bounds_max_m"],
                          "local_extrema":x["local_extrema"],
                          "materials":x["materials"],"uv_layers":x["uv_layers"]}
                         for x in actual["objects"][:80]]
        history+="\nVERIFIED FROZEN SOURCE BLENDER MESH INVENTORY: "+json.dumps(verified_objects)[:12000]
        known_objects=[o["name"] for o in verified_objects]
    if args.asset_catalog:
        inventory=json.loads(args.asset_catalog.read_text(encoding="utf-8"))
        allowed=[{"path":i["relative_path"],"sha256":i["sha256"],"license":i["license"]}
                 for i in inventory.get("assets",[]) if i.get("reuse_approved") is True]
        history+="\\nOWNER-APPROVED REAL ART SOURCE CATALOG (use exact SHA): "+json.dumps(allowed[:25])[:6000]
    records=[]
    for cycle in range(1,args.max_cycles+1):
        trial=args.out/("cycle-"+str(cycle));trial.mkdir()
        for attempt in range(1,4):
            try:
                doc=ask_plan(args,args.reference,current,history,role,memory,trial,attempt,source_exists=bool(source),known_objects=known_objects)
                break
            except (RejectedPlan,CompositionRejected,json.JSONDecodeError) as e:
                history+="\nA non-executable Qwen proposal was REJECTED: "+str(e)
                if attempt==3:raise
        candidate=trial/"artist-plan.json";candidate.write_text(json.dumps(doc,indent=2)+"\n")
        stage=trial/"output"
        try:
            run_blender(args.blender,candidate,stage,source,args.texture_root,args.review,args.asset_catalog,args.transport,args.mcp_addon,args.mcp_sha256)
        except Exception as ex:
            records.append({"cycle":cycle,"status":"BLENDER_REJECTED","error":str(ex)[-800:]})
            history+="\nACTUAL BLENDER REJECTION: "+str(ex)[-1100:]
            continue
        result=json.loads((stage/"execution-proof.json").read_text())
        if (args.phase=="mesh_edit" or args.require_inherited_geometry_edit) and source:
            edits=[step for step in result["execution"] if step["op"] in (
                       "sculpt_anchor","sculpt","extrude","inset","subdivide","boolean")
                   and step["geometry_changed"] and step["originals_changed"]]
            if not edits or not result.get("inherited_mesh_data_changed"):
                records.append({"cycle":cycle,"status":"INHERITED_MESH_UNCHANGED",
                                "plan_sha256":sha(candidate)})
                history+="\nREJECTED: Blender changed no original vertex or polygon data of the inspected inherited source. Material, UV, transforms and unapplied modifiers do not satisfy the owner's real source-mesh edit requirement. Select a physical mesh edit using Blender with artist-authored actual spatial decisions."
                continue
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
        known_objects=[o["name"] for o in catalog["components"][:80]]
        if not should_continue_after_critique(cycle,args.max_cycles,started_empty,evaluation):break
        if started_empty and cycle==1:
            history+="\\nFIRST CYCLE WAS BLOCKOUT ONLY: source geometry now exists. For this second cycle choose actual higher-level Blender finishing operations (mesh editing, material, UV, bevel, surface/node work) guided by the real render and reference. No preset values."
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
