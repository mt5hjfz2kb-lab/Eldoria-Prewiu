"""Autonomous local Qwen -> full Blender studio -> real render -> critique loop.

This is production authoring, NOT a scripted model generator or art approval.
Blender executes only a validated typed plan; Qwen supplies every asset choice.
No paid services, cloud inference, arbitrary Python, or new game scene changes.
"""
from __future__ import annotations
import argparse, base64, hashlib, json, os, pathlib, subprocess, sys, time, urllib.error, urllib.request
from contract import validate_plan, RejectedPlan, safe_action_id
from learning_context import transferable_lessons, evidence_case_lessons, stage_pending_art_evidence
from artistic_quality_gate import (stage_visual_request,validate_independent_review,
                                 append_accepted,status as visual_calibration_status)
from screen_impact import screen_impact
from composition_health import check_new_silhouette,CompositionRejected
from source_geometry_gate import verify_source_plan,relative_triangle_limit
from sculpt_scale_contract import conservative_schema_bounds
from professional_source_schema import source_artist_action_schema, NEW_COMPONENT_PATTERN
from architectural_component_guard import verify_volume_and_complexity
from architectural_world_placement_gate import reject_obviously_bad_component_placement
from protected_camera_regions import inspect_protected_regions
import copy, re
from triangle_budget import check_triangle_growth

HERE=pathlib.Path(__file__).resolve().parent
REPO=HERE.parents[2]
ROLE=REPO/"tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md"
JOURNAL=REPO/"tools/art-direction/ARTIST_LEARNING_JOURNAL.md"
CASEBOOK=HERE/"learning_cases.json"

def sha(path):
    return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()

def image_data(file, max_edge=1024):
    """Real, camera-preserving evidence frames; never synthesize reference pixels."""
    from PIL import Image, ImageOps
    import io
    if max_edge not in (640,768,1024,1280):
        raise ValueError("Unsupported bounded vision resolution")
    with Image.open(file) as source_image:
        im=ImageOps.exif_transpose(source_image).convert("RGB")
    im.thumbnail((max_edge,max_edge))
    buf=io.BytesIO();im.save(buf,format="JPEG",quality=88,subsampling=0)
    return base64.b64encode(buf.getvalue()).decode("ascii")

def request_ollama(endpoint, model, messages, max_tokens, timeout=600, schema=None):
    if endpoint != "http://127.0.0.1:11434/api/chat":
        raise ValueError("Only local zero-cost Ollama supported")
    # Preserve the previous 16k default. The benchmark may select a smaller
    # local memory budget explicitly without changing creative decisions.
    num_ctx=int(os.environ.get("ELDORIA_ARTIST_NUM_CTX","16384"))
    if num_ctx not in (4096,8192,16384):
        raise ValueError("Only tested bounded local context sizes are supported")
    payload={"model":model,"stream":True,"think":False,
             "messages":messages,"options":{"temperature":0.35,"num_ctx":num_ctx,"num_predict":max_tokens}}
    if schema is not None:payload["format"]=schema
    call=urllib.request.Request(endpoint,headers={"Content-Type":"application/json"},
                                data=json.dumps(payload).encode("utf-8"))
    pieces=[];done=False
    try:
        with urllib.request.urlopen(call,timeout=timeout) as response:
            for line in response:
                if not line.strip():continue
                obj=json.loads(line)
                if obj.get("error"):raise RuntimeError(obj["error"])
                pieces.append(obj.get("message",{}).get("content",""))
                if obj.get("done"):done=True;break
    except urllib.error.HTTPError as error:
        # Ollama can reject a JSON Schema before any model inference.
        # Make the bounded backend reason visible: do not reinterpret this as
        # a Qwen artistic choice or silently invent replacement geometry.
        detail=error.read(2400).decode("utf-8","replace")
        raise RuntimeError(f"OLLAMA_SCHEMA_OR_REQUEST_REJECTED HTTP {error.code}: {detail}") from error
    if not done:raise RuntimeError("Incomplete Qwen response; no plan accepted")
    return "".join(pieces)

def executed_action_signature(doc):
    """Hash Qwen's exact artistic ops/values, not IDs or prose, so retries can't cheat."""
    actions=[{"op":a["op"],"target":a["target"],"params":a["params"]}
             for a in doc.get("actions",[])]
    return hashlib.sha256(json.dumps(actions,sort_keys=True,separators=(",",":"),
                                     ensure_ascii=False).encode("utf-8")).hexdigest()


def rejected_plan_family(raw_file):
    """Identify an invalid Qwen-authored tool, never repair its geometry."""
    if not pathlib.Path(raw_file).is_file():
        return None
    try:
        d=json.loads(pathlib.Path(raw_file).read_text(encoding="utf-8"))
        actions=d.get("actions",[])
        return actions[0].get("op") if isinstance(actions,list) and actions and isinstance(actions[0],dict) else None
    except (json.JSONDecodeError,UnicodeError,TypeError,AttributeError):
        return None


def track_rejected_strategy(raw_file, counts, blocked, threshold=2):
    """Two bounded plan rejections of same family require different tool."""
    family=rejected_plan_family(raw_file)
    if family:
        counts[family]=counts.get(family,0)+1
        if counts[family]>=threshold:
            blocked.add(family)
    return family


def should_reject_repeated_invisible_plan(document, prior_invisible_signatures):
    """Refuse an already observed no-pixel-change Blender proposal unchanged."""
    if executed_action_signature(document) in (prior_invisible_signatures or set()):
        raise RejectedPlan(
            "ZERO_PIXEL_CHANGE_REPLAY_REJECTED: This exact Qwen geometry, "
            "target and numeric action was already executed in Blender and "
            "produced no meaningful camera effect. Select a genuinely NEW "
            "operation/face/direction/value, not the same plan with a new name.")
    return True


def schema_for_plan(max_actions=6, source_exists=False, phase='full', category='architecture', known_objects=None, source_inventory=None, sequenced_source=False, assembly_source=False, assembly_cycle=0, safe_primitive_fallback=False, excluded_source_ops=None):
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
        # Scale edit limits from real Blender WORLD dimensions when available.
        # Never alter actual model-chosen directions or distances.
        brush=conservative_schema_bounds(source_inventory)
        distance=(brush["distance"] if brush else {"minimum":0.65,"maximum":1.8})
        radius=(brush["radius"] if brush else {"minimum":1.5,"maximum":8})
        # A LOCAL visual model chooses a physically EXISTING mesh landmark and
        # sculpt direction/strength; Blender resolves the actual vertex.
        # This avoids 2B JSON always emitting zero-vector brushes without
        # substituting scripted artistic choices.
        action_op={"type":"string","const":"sculpt_anchor"}
        params={"type":"object","properties":{
          "anchor":{"type":"string","enum":["x_min","x_max","y_min","y_max","z_min","z_max"]},
          "direction":{"type":"string","enum":["+X","-X","+Y","-Y","+Z","-Z"]},
          "distance":{"type":"number",**distance},
          "radius":{"type":"number",**radius}},
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
    if source_exists and phase=="full" and category=="architecture" and source_inventory:
        # Keep Qwen's creative tool selection; make signatures exact. A source
        # that only has inherited meshes needs no invented new object IDs.
        action=source_artist_action_schema(source_inventory)
    actions_schema={"type":"array","minItems":2 if not source_exists else 1,
                    "maxItems":min(max_actions,8),"items":action}
    if sequenced_source and source_exists and phase=="full" and category=="architecture" and source_inventory:
        # The artist still freely selects the actual structural tool, coordinates,
        # target face/edge and material values. Sequence is a safety dependency:
        # finish the mesh before any operations relying on its changed topology.
        tools=action["oneOf"]
        structural=[v for v in tools if v["properties"]["op"]["const"]
                    in (("primitive","mesh") if assembly_source else
                        ("sculpt_anchor","sculpt","extrude","inset","bevel","primitive","mesh"))]
        if not assembly_source and excluded_source_ops:
            structural=[v for v in structural
                        if v["properties"]["op"]["const"] not in excluded_source_ops]
        finishing=[v for v in tools if v["properties"]["op"]["const"]
                   in ("material","uv")]
        optional=[v for v in tools if v["properties"]["op"]["const"]
                  in ("material","uv","modifier")]
        if not structural or not finishing:raise ValueError("No typed source tools")
        if assembly_source:
            # Volumetric geometry is required for the new architecture proof.
            # The model still freely designs shape, positions and proportions.
            structural=copy.deepcopy(structural)
            if safe_primitive_fallback:
                # One rejected topology-authoring proposal can temporarily
                # limit the executable tool, NEVER fix/generate model geometry.
                structural=[v for v in structural if v["properties"]["op"]["const"]=="primitive"]
            stage_pattern=(rf"^Eldoria_Added_C{assembly_cycle:02d}_[A-Za-z][A-Za-z0-9_]{{3,35}}$"
                           if assembly_cycle else NEW_COMPONENT_PATTERN)
            for variant in structural:
                op=variant["properties"]["op"]["const"]
                variant["properties"]["target"]["pattern"]=stage_pattern
                prm=variant["properties"]["params"]["properties"]
                if op=="primitive":
                    prm["shape"]["enum"]=[x for x in prm["shape"]["enum"] if x!="plane"]
                elif op=="mesh":
                    prm["vertices"]["minItems"]=6
                    prm["faces"]["minItems"]=4
            # The first component name is selected by Qwen, under a reserved
            # namespace that CAN NEVER collide with inherited object names.
            # Later surface operations may reference the SAME chosen name;
            # the runtime verifies the binding rather than inventing a name.
            finishing=copy.deepcopy(finishing)
            optional=copy.deepcopy(optional)
            namespace={"type":"string","minLength":18,"maxLength":60,
                       "pattern":stage_pattern}
            for v in finishing+optional:
                v["properties"]["target"]=copy.deepcopy(namespace)
        inherited_atlas_only=(not assembly_source and
                              all(obj.get("faces",0)>2500 for obj in source_inventory))
        if inherited_atlas_only:
            # No forced material+UV finishing on the dense imported atlas.
            # ONE Qwen-chosen structural op, then genuine Blender reinspection
            # before a separate later cycle can texture a newly made object.
            actions_schema={"type":"array","minItems":1,"maxItems":1,
                            "prefixItems":[{"oneOf":structural}]}
        else:
            actions_schema={"type":"array","minItems":2,"maxItems":3,
                            "prefixItems":[{"oneOf":structural},
                                           {"oneOf":finishing},
                                           {"oneOf":optional}]}
    return {"type":"object","properties":{
        "schema_version":{"type":"integer","const":2},
        "asset_id":{"type":"string","minLength":2,"maxLength":70},
        "observed_defect":{"type":"string","minLength":30,"maxLength":180},
        "design_intent":{"type":"string","minLength":30,"maxLength":200},
        "asset_decision":{"type":"string","enum":["MODIFY","REUSE"] if source_exists else ["NEW"]},
        "expected_visual_difference":{"type":"string","minLength":30,"maxLength":200},
        "mobile_risks":{"type":"string","maxLength":180},
        "actions":actions_schema},
        "required":["schema_version","asset_id","observed_defect","design_intent",
                    "asset_decision","expected_visual_difference","mobile_risks","actions"],
        "additionalProperties":False}

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
        return base+"REAL SOURCE MESH EDIT ONLY: every op=sculpt_anchor, target MUST match the literal existing object name (including a middle dot, if any). For EACH action params EXACTLY {anchor:x_min|x_max|y_min|y_max|z_min|z_max,direction:+X|-X|+Y|-Y|+Z|-Z,distance:small positive METERS relative to measured WORLD dimensions,radius:small LOCAL brush in METERS according to actual source size}. WORLD-SPACE BRUSH: Blender WORLD +Z means physically upward even if imported GLB LOCAL +Y is aligned with WORLD +Z. The brush direction, distance and radius are in WORLD axes and physical metres; only the anchor vertex is chosen from LOCAL EXTREMA. INHERITED VERTEX EDIT: Read inspected local_bounds_min_m / local_bounds_max_m and LOCAL EXTREMA and the owner reference: decide which SOURCE extremum is artistically defective and which direction, distance and radius would improve its silhouette. The executor will resolve the chosen extremal vertex in actual Blender geometry and deform nearby inherited vertices. NO zero delta, NO new primitives, NO fake object transforms. Make a substantial but controlled visible silhouette edit at a landmark consistent with the reference. Preserve recognisable architecture; choose direction and impact deliberately rather than minimum values. Every action must make a visible source-mesh change; do not repeat identical actions."
    if phase=="geometry":
        return base+"THIS STAGE ONLY FIXES GEOMETRY of the real existing Blender meshes. For EACH action op=transform; choose one EXACTLY NAMED source object from verified inventory. params must contain BOTH location:[x,y,z] and scale:[sx,sy,sz], no extra keys. FIX the formerly overlapping and horizontally misaligned roof and tower by placing objects at DIFFERENT HEIGHTS. Y coordinates must not be copied blindly. Blender operation is absolute position and absolute scale. NEVER wrap values in a transform sub-object."
    if phase=="surface":
        return base+"THIS STAGE ONLY AUTHORS REAL PBR MATERIALS on named existing Blender meshes. For EACH action op=material; choose exact source object name, params EXACTLY {base_color:[r,g,b,a],roughness:0.45..1,metallic:0..0.35}. Use plausible medieval limestone, terracotta, oak and aged iron according to each component; different real objects must receive coherent but distinct artist-picked colours. Do not use transform or invent object IDs."
    if phase=="detail":
        return base+"THIS STAGE ONLY ADDS REAL MESH EDGE DETAILS. For EACH action op=bevel; choose existing source mesh from verified inventory, params EXACTLY {width:0.01..0.22,segments:1..4,apply:true}. Avoid beveling huge terrain unnecessarily. You determine which edges/details merit finishing."
    return base+(
        "Choose YOUR OWN tools and numeric values from NINE real, validated Blender operation families (sculpt_anchor, sculpt, extrude, inset, bevel, modifier, material, uv, native_operator). "
        "These are NOT preset designs; when architectural quality needs a new part, choose a NEW primitive or an explicitly authored custom vertices/faces mesh. "
        "New objects REQUIRE a DISTINCT name not in the observed source inventory. "
        "Every custom mesh vertex coordinate MUST be unique and EVERY face must have nonzero area; never repeat [0,0,0] to fill a vertex array. "
        "When uncertain of valid custom vertices/faces, choose a PRIMITIVE and set original shape/location/rotation/scale. "
        "Do not choose the original tower name for a created object. "
        "The original inherited tower MUST remain recognizable; added components should attach to it and improve visible architecture. "
        "Do not output mesh:operation pseudo-parameters. "
        "Only select face IDs 0..faces-1 from the INSPECTED SOURCE; local_extrema vertex_index is NEVER a face index. "
        "For sculpt, pick your OWN center within brush radius of real local extrema/vertex landmarks. For sculpt_anchor, choose a LOCAL anchor but WORLD direction (+Z really rises); distance and radius are physical WORLD metres and bounded by the actual source dimensions. "
        "For extrude or inset, choose REAL observed face_landmarks.face_index according to their local_center_m/local_normal. "
        "For bevel on detailed imported geometry, choose selected edge_landmarks.edge_index (NOT full-object bevel), with edges:[ids]; NEVER copy vertex_index to face/edge ID. "
        "Plan at most ONE face/edge topology changing operator each cycle, then inspect the new mesh before more selected faces/edges. "
        "Model decisions are NEVER automatically clamped, moved or fixed; bad original choices are rejected before Blender. "
        "FULL PROFESSIONAL ARTIST MODE on verified source: choose which REAL Blender "
        "tools to use based on the observed visual defect, NOT a fixed primitive recipe. "
        "Every target must be an inspected existing mesh or an object created by an "
        "earlier action in this plan. Tools may be combined in sequence. "
        "Exact params contract examples (numbers below only illustrate TYPES, "
        "they are NOT proposed artistic values): "
        "material={base_color:[r,g,b,a],roughness:0..1,metallic:0..1}; "
        "sculpt={center:[x,y,z],delta:[dx,dy,dz],radius:positive}; "
        "bevel={edges:[real_edge_indices],width:positive,segments:1..4,apply:true}; "
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

def invalid_mesh_requires_tool_fallback(rejection_reasons):
    """After an invalid original mesh, Qwen may reauthor via a volumetric Blender primitive."""
    labels=("OPEN or NONMANIFOLD","ZERO-AREA","DUPLICATE 3D vertices",
            "COPLANAR","insufficient X/Y/Z volume","noncollinear")
    return any(any(label in reason for label in labels) for reason in rejection_reasons)


def ask_plan(args,reference,current_image,history,role,memory,trial,attempt_number,
             source_exists=False,known_objects=None,source_inventory=None,spatial_views=None,
             prior_invisible_signatures=None,excluded_source_ops=None):
    # Preserve older offline test callers without weakening runtime constraints.
    source_build=bool(getattr(args,"source_build_phase",False))
    tool_sequence=bool(getattr(args,"source_tool_sequencing",False))
    if not hasattr(args,"source_build_phase"):args.source_build_phase=source_build
    if not hasattr(args,"source_tool_sequencing"):args.source_tool_sequencing=tool_sequence
    msg=(contract_brief(args.max_actions,source_exists,args.phase,args.asset_category)
      +"\nCurrent asset requested by owner: "+args.brief[:500]
      +"\nVerified existing geometry or actual PREVIOUS cycle feedback: "+history[-11800:]
      +"\nEvidence-ranked verified lessons (not geometry instructions): "+memory[:3500]
      +"\nUse only YOUR NEW reasoning about the supplied owner reference. "
      "The examples in previous failures are warnings, not templates.")
    if args.source_build_phase and not (args.source_tool_sequencing and source_exists and args.phase=="full"):
        raise ValueError("Architecture build phase requires a frozen inherited source and typed sequence")
    if excluded_source_ops:
        msg+=("\\nDO NOT REPEAT PROVEN-INVISIBLE BLENDER ACTION FAMILIES: "+
              ", ".join(sorted(excluded_source_ops))+
              ". The original fixed-camera A/B changed 0 pixels; choose "
              "a different, physically useful original Blender operation. ")
    if args.source_build_phase:
        msg+=("\\nBLENDER SPATIAL FACTS, NOT ARTISTIC PRESETS: Blender WORLD +Z is UP. "
              "For any NEW object, location/rotation/scale are in Blender WORLD xyz. "
              "An imported GLB mesh may have LOCAL +Y aligned with WORLD +Z. "
              "Never put a chimney 'up' using WORLD Y; height is WORLD Z. "
              "Use real world_bounds_min_m/world_bounds_max_m/world_bounds_center_m from "
              "the latest inspected asset for all architectural placement. "
              "Choose your OWN scale, pose and material after examining the reference "
              "and preserve the existing door, stairs and player circulation.")

    inherited_atlas_only=(source_exists and
        not args.source_build_phase and source_inventory and
        all(item.get("faces",0)>2500 for item in source_inventory))
    if args.source_tool_sequencing and args.phase=="full" and inherited_atlas_only:
        msg+=("\nPROTECT ORIGINAL TEXTURE ATLAS: the imported inherited tower "
              "is a single dense mesh. Choose EXACTLY ONE precise STRUCTURAL "
              "Blender operation in this cycle (selected face/edge extrude, "
              "sculpt, bevel, or an original attached NEW component). "
              "Do NOT repaint or UV unwrap the source: whole-source MATERIAL "
              "and UV would destroy the existing timber textures and protected "
              "door/stairs appearance. Let Blender re-inspect after structural "
              "edits; only independent newly authored objects can later "
              "receive safe local material/UV finishing. "
              "All structural values remain chosen by YOU, not a recipe.")
    if args.source_tool_sequencing and source_exists and args.phase=="full" and not inherited_atlas_only:
        msg+=("\nThis is a staged mesh-edit session on a dense original asset: "
          "the FIRST operation must be ONE geometric operation chosen by YOU "
          + ("(choose a NEW primitive or handcrafted mesh and its dimensions yourself). "
             "Invent your own unique object name STARTING Eldoria_Added_ followed by a letter and 3-42 alphanumeric/underscore characters. "
             "Create genuinely VOLUMETRIC 3D architecture with readable depth in the player's fixed camera: NOT a single floor plane or two triangles. "
             "ROOF EAVE / FASCIA / OVERHANG means a REAL physically supported timber feature near the roof's outer WORLD X/Y perimeter, BELOW the highest roof apex. "
             "A primitive at the CENTER of the highest apex is NOT a roof eave; a flat decorative cylinder there is not architectural progress. "
             "Blender SOLIDIFY only thickens existing geometry; it does NOT attach disconnected objects to the original roof. "
             "Choose a credible geometry and position grounded in the visible timber edges, roof slope and real source size. "
             "If choosing primitive, design meaningful original X/Y/Z scale and placement; a flat plane is NOT valid. "
             "If choosing your own vertices/faces, design a CLOSED physical architectural solid with at least SIX non-coplanar vertices and FOUR faces: each mesh edge must belong to exactly TWO faces, no open triangular flaps. "
             "In each next art cycle, add a DIFFERENT complementary 3D part to the same architectural feature, connecting it naturally to your previous visible addition. "
             "Consult CURRENT actual Blender source inventory and prior real render, choosing a NEW different Eldoria_Added_ name in each cycle. "
             "NEVER reuse any inherited mesh name or object mentioned in the verified source inventory. "
             "Your SECOND and optional THIRD actions MUST use the identical NEW object name you chose FIRST, "
             "to shade/finish YOUR newly created component, not repaint the original tower. "
             if args.source_build_phase
             else "(sculpt, sculpt_anchor, extrude, inset, selected-edge bevel, NEW primitive or authored mesh). ")
          + "The SECOND is YOUR material or UV operation. "
          "An optional THIRD is material, UV or a safe non-global modifier. "
          "This ordering prevents use of stale mesh topology. "
          "All geometry and artistic values are chosen by YOU, never a script.")
    cycle_index=int(trial.name.removeprefix("cycle-")) if re.fullmatch(r"cycle-[1-9][0-9]*",trial.name) else 1
    previous_rejections=[]
    for previous_attempt in range(1,attempt_number):
        note=trial/("rejected-plan-attempt-"+str(previous_attempt)+".json")
        if note.is_file():
            previous_rejections.append(json.loads(note.read_text(encoding="utf-8")).get("rejection",""))
    # Model failures are rejected exactly as authored. A proven INVALID
    # handcrafted mesh allows Qwen to choose a real Blender primitive on the
    # next retry; it still authors every dimension, placement and material.
    topology_fallback=(args.source_build_phase and
                       invalid_mesh_requires_tool_fallback(previous_rejections))
    if args.source_build_phase:
        ns=f"Eldoria_Added_C{cycle_index:02d}_"
        msg+=("\\nMandatory per-cycle new-object namespace: your chosen new name begins "+
              ns+" followed by YOUR original descriptive identifier. "
              "Invent a DIFFERENT name from any verified existing mesh. "
              "Your design must be integrated with the medieval architecture without obstructing "
              "the entry door, wooden stairs, or circulation. Do not create a freestanding oversized "
              "box that swallows the building; compare local dimensions and player camera.")
    if topology_fallback:
        msg+=("\\nYour original handwritten mesh attempt did not form a closed solid. "
              "For THIS RETRY, use a REAL VOLUMETRIC Blender primitive. "
              "YOU still choose which primitive, placement, orientation, dimensions, material and intent. "
              "No script chooses your new structure.")
    if source_exists and source_inventory:
        # Only true Blender camera ray hits are called visible. These IDs
        # are real observed source faces, never scripted artistic actions.
        camera_facts=[
            {"name":obj["name"],
             "faces":[{"id":i["face_index"],
                       "screen_uv":i["camera_screen_uv"],
                       "height":i["world_height_fraction"]}
                      for i in obj.get("camera_visible_face_landmarks",[])[:24]]}
            for obj in source_inventory
            if obj.get("camera_visible_face_landmarks")]
        if camera_facts:
            msg+=("\nACTUAL CAMERA-RAY-VISIBLE SOURCE FACE INDICES: "+
                  json.dumps(camera_facts,ensure_ascii=False)[:5000]+
                  "\nFor extrude/inset choose face ID with the desired on-screen "
                  "location from these verified camera hits. Hidden faces "
                  "from old local percentile sampling cannot improve this view.")
    edge=getattr(args,"vision_max_edge",1024)
    if source_exists and source_inventory:
        # Replace a giant truncated inventory with SHORT trustworthy local
        # geometric facts immediately before the images. Never prescribe
        # actual new component coordinates.
        entry=max(source_inventory,key=lambda item:item.get("evaluated_triangles",item.get("faces",0)))
        minimum=entry.get("world_bounds_min_m")
        maximum=entry.get("world_bounds_max_m")
        if minimum and maximum:
            msg+=("\nIMPORTANT BOUNDARY FACTS measured in actual Blender WORLD metres: "+
                  "building x=[%.2f, %.2f], y=[%.2f, %.2f], z=[%.2f, %.2f]. "%(
                      minimum[0],maximum[0],minimum[1],maximum[1],minimum[2],maximum[2])+
                  "Blender WORLD Z is HEIGHT. Imported GLB mesh local Y may be up "+
                  "but new component WORLD Y is horizontal depth, NOT height. "+
                  "Choose original modest rooftop detail physically near actual x/y surface. "+
                  "The staircase/door region must remain open.")
        if args.phase=="mesh_edit":
            from sculpt_scale_contract import conservative_schema_bounds
            limits=conservative_schema_bounds(source_inventory)
            if limits:
                msg+=("\nSOURCE-SIZED WORLD BRUSH BOUNDS (METRES; do NOT invent other values): "
                      "distance [%.4f,%.4f], radius [%.4f,%.4f]. "%(
                          limits["distance"]["minimum"],limits["distance"]["maximum"],
                          limits["radius"]["minimum"],limits["radius"]["maximum"]))
    pictures=[image_data(reference,edge)]
    if current_image:pictures.append(image_data(current_image,edge))
    if getattr(args,"focus_image",None):pictures.append(image_data(args.focus_image,edge))
    if spatial_views:
        msg+=("\\nMODEL VISUAL SOURCES ORDER: (1) owner reference, (2) CURRENT Blender hero view, "
              "(3) actual Blender WORLD TOP orthographic from +Z looking down, "
              "(4) actual Blender WORLD SIDE orthographic from +X. "
              "Use these multiple camera angles and measured world bounds together "
              "to place a physically attached, complementary medieval part. "
              "The views are read-only diagnostic renders, not design presets.")
        for frame in spatial_views:
            if not frame.is_file():raise FileNotFoundError("Real Blender spatial camera image missing: "+str(frame))
            pictures.append(image_data(frame,edge))
    actual_messages=[{"role":"system","content":role},
                     {"role":"user","content":msg,"images":pictures}]
    actual_schema=schema_for_plan(args.max_actions,source_exists,args.phase,args.asset_category,
                    known_objects,source_inventory,args.source_tool_sequencing,
                    args.source_build_phase,cycle_index if args.source_build_phase else 0,
                    topology_fallback,excluded_source_ops)
    submitted=json.dumps({"messages":actual_messages,"schema":actual_schema,
                          "model":args.model},sort_keys=True,separators=(",",":")).encode("utf-8")
    (trial/("actual-qwen-context-attempt-"+str(attempt_number)+".json")).write_text(
        json.dumps({"context_sha256":hashlib.sha256(submitted).hexdigest(),
                    "model":args.model,"includes_original_owner_reference":True,
                    "includes_real_inspected_blender_images":bool(current_image),
                    "journal_and_cases_in_message":bool(memory),
                    "source_history_characters":len(history),
                    "note":"Exact submitted content+images+schema hashed in memory, no image bytes forged"},indent=2)+"\n")
    text=request_ollama(args.endpoint,args.model,actual_messages,args.max_tokens,
                        schema=actual_schema,timeout=540)
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
    should_reject_repeated_invisible_plan(doc,prior_invisible_signatures)
    if source_exists and args.phase in ('full','mesh_edit') and source_inventory:
        verify_source_plan(doc,source_inventory)
        if args.source_build_phase:
            actions=doc["actions"]
            if len(actions)<2 or actions[0]["op"] not in ("primitive","mesh"):
                raise RejectedPlan("First creation action must be an artist-selected new primitive or custom mesh")
            created_name=actions[0]["target"]
            ns_pattern=rf"^Eldoria_Added_C{cycle_index:02d}_[A-Za-z][A-Za-z0-9_]{{3,35}}$"
            if not re.fullmatch(ns_pattern,created_name):
                raise RejectedPlan("New Qwen object must use original descriptive suffix with CURRENT CYCLE namespace Eldoria_Added_C%02d_ and must never reuse any inherited source name" % cycle_index)
            for action in actions[1:]:
                if action["target"] != created_name:
                    raise RejectedPlan("New assembly: all subsequent surface/modifier operations must target the SAME new component named by Qwen's first action")
            verify_volume_and_complexity(doc,source_inventory)
            spatial_proof=reject_obviously_bad_component_placement(doc,source_inventory)
            (trial/("world-placement-proof-attempt-"+str(attempt_number)+".json")).write_text(
                json.dumps(spatial_proof,indent=2)+"\n",encoding="utf-8")
        elif doc.get("actions") and doc["actions"][0]["op"] in ("primitive","mesh"):
            # Full professional source work still protects the existing real
            # building; only the forced assembly phase used these QA gates
            # previously, allowing unconstrained disconnected source addons.
            verify_volume_and_complexity(doc,source_inventory)
            spatial_proof=reject_obviously_bad_component_placement(doc,source_inventory)
            (trial/("world-placement-proof-attempt-"+str(attempt_number)+".json")).write_text(
                json.dumps(spatial_proof,indent=2)+"\n",encoding="utf-8")
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
    # The accepted authored plan is bound to the EXACT payload delivered
    # to Ollama, including raw reference pixels, camera views and model schema.
    attempted=trial/("actual-qwen-context-attempt-"+str(attempt_number)+".json")
    (trial/"actual-qwen-context-executed.json").write_bytes(attempted.read_bytes())
    return doc

def critique(args,reference,before,after,role):
    instruction=("Inspect these REAL renders, in order: target, before, after. "
      "Describe three defects ONLY if supported by pixels in the actual "
      "AFTER image, whether its timber silhouette and materials visibly improved "
      "versus BEFORE, and one concrete artistic approach if not. "
      "These are neutral Blender previews, NOT Unity: never claim Unity screenshots, "
      "Hero Gate comparisons, a historical cliff or any unshown object. "
      "If improvement is too subtle, misplaced at the roof apex, or visually "
      "incoherent, set clearly_failed=true. Do not invent Unity evidence. "
      "Return concise JSON with strings critique, geometry_next, material_next "
      "and bool clearly_failed. This critique never certifies art.")
    try:
        critique_schema={"type":"object","properties":{
          "critique":{"type":"string"},"geometry_next":{"type":"string"},
          "material_next":{"type":"string"},"clearly_failed":{"type":"boolean"}},
          "required":["critique","geometry_next","material_next","clearly_failed"],
          "additionalProperties":False}
        txt=request_ollama(args.endpoint,args.model,[{"role":"system","content":role},
         {"role":"user","content":instruction,"images":[image_data(reference,getattr(args,"vision_max_edge",1024)),
                       image_data(before,getattr(args,"vision_max_edge",1024)),
                       image_data(after,getattr(args,"vision_max_edge",1024))]}],350,timeout=420,schema=critique_schema)
        return json.loads(txt)
    except Exception as ex:return {"critique":"Visual critique unavailable: "+str(ex),"clearly_failed":True}

def summarize_real_artist_cycles(records):
    """Separate physically executed Blender from safety-approved review candidates.

    Never equate an executed-but-rejected dangerous edit with artistic success.
    """
    executed={"ART_REVIEW_REQUIRED", "SCREENSPACE_REJECTED",
              "PROTECTED_ACCESS_OCCLUSION_REJECTED",
              "BLENDER_VISUAL_ACCEPTED","ART_REJECTED"}
    return {
        "real_blender_execution_count":sum(rec.get("status") in executed for rec in records),
        "qa_review_candidate_count":sum(rec.get("status") in
          ("ART_REVIEW_REQUIRED","BLENDER_VISUAL_ACCEPTED","ART_REJECTED") for rec in records),
        "protected_access_rejection_count":sum(
            rec.get("status")=="PROTECTED_ACCESS_OCCLUSION_REJECTED" for rec in records),
        "screen_impact_rejection_count":sum(
            rec.get("status")=="SCREENSPACE_REJECTED" for rec in records),
        "model_self_critique_failed_count":sum(
            rec.get("critique",{}).get("clearly_failed",False) for rec in records)
    }


def should_continue_after_critique(cycle_number, max_cycles, started_empty, evaluation,
                                  min_successful_cycles=1, successful_count=1):
    """One visible empty-scene blockout does not equal completed artistic authoring.

    Existing asset work may stop when Qwen judges it adequate, subject to the
    separate independent review gate. New assets with >1 cycle MUST receive
    at least one source-grounded finishing pass, irrespective of Qwen self praise.
    """
    return cycle_number < max_cycles and (
        evaluation.get("clearly_failed", True) or
        (started_empty and cycle_number == 1) or
        successful_count < min_successful_cycles
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
    p.add_argument("--max-cycles",type=int,default=2)
    p.add_argument("--source-build-phase",action="store_true",
                   help="Qwen chooses a new primitive or custom mesh for attached architecture; no prescribed shape")
    p.add_argument("--source-tool-sequencing",action="store_true",
                   help="One original Qwen structural choice followed by surface finishing per source-mesh cycle")
    p.add_argument("--max-plan-attempts",type=int,default=5,
                   help="Bounded original Qwen plan revisions; no artistic value repair")
    p.add_argument("--min-successful-cycles",type=int,default=1,
                   help="Require this many real Blender/Qwen cycles before considering a self-critique stop")
    p.add_argument("--asset-category",choices=("architecture","terrain","vegetation","prop","character"),default="architecture")
    p.add_argument("--phase",choices=("full","geometry","mesh_edit","surface","detail"),default="full")
    p.add_argument("--max-actions",type=int,default=32)
    p.add_argument("--max-tokens",type=int,default=2048)
    p.add_argument("--vision-max-edge",type=int,default=1024,choices=(640,768,1024,1280))
    p.add_argument("--focus-image",type=pathlib.Path,
                   help="Optional genuine source detail image, never a fabricated preview")
    p.add_argument("--prior-lessons",type=pathlib.Path)
    p.add_argument("--independent-reviews-dir",type=pathlib.Path,
                   help="Exact per-cycle independent reviewer JSON files; absent => hold for review")
    p.add_argument("--accepted-chain",type=pathlib.Path,
                   help="Previously independently accepted SHA-linked Blender-only chain")
    p.add_argument("--frozen-original-asset-sha256",
                   help="Original immutable tower SHA, necessary when continuing accepted outputs")
    args=p.parse_args()
    if not 1<=args.max_cycles<=5 or not 1<=args.max_plan_attempts<=6 or not 1<=args.max_actions<=160 or not 1<=args.min_successful_cycles<=args.max_cycles:
        raise ValueError("Cycle/action/minimum-successful-cycle limits")
    if not args.reference.is_file():raise FileNotFoundError("Real owner reference required")
    if args.focus_image and not args.focus_image.is_file():
        raise FileNotFoundError("Genuine source detail image missing")
    if args.source and not args.source.is_file():raise FileNotFoundError("Source .blend/GLB missing")
    if args.out.exists() and any(args.out.iterdir()):raise ValueError("Fresh output required")
    args.out.mkdir(parents=True,exist_ok=True)
    canonical_role=ROLE.read_text(encoding="utf-8")
    role=(canonical_role+"\nCURRENT BRIEF AND LIVE SOURCE OVERRIDE ALL HISTORICAL ART SAMPLES. "
          "Do not replay old geometry/IDs or infer quality without real A/B and independent review.")
    # Genuine evidence-backed journal retrieval, sanitized of old rejected
    # cliff coordinates/IDs. The first 2B run replayed them and overflowed.
    lesson_proof=transferable_lessons(JOURNAL.read_text(encoding="utf-8"),args.brief,limit=1400)
    (args.out/"journal-context-proof.json").write_text(json.dumps(lesson_proof,indent=2)+"\n",encoding="utf-8")
    casebook=json.loads(CASEBOOK.read_text(encoding="utf-8"))
    if casebook.get("schema_version")!=1:raise ValueError("Corrupt Qwen artist casebook")
    case_context=evidence_case_lessons(casebook,args.brief,limit=1400)
    memory=("VERIFIED JOURNAL LESSONS - NOT NEW ACTION PLANS:\n"+
            lesson_proof["prompt_text"]+
            "\nHISTORICAL EVIDENCE-LINKED VISUAL FAILURES/SUCCESSES:\n"+
            case_context["prompt_text"])
    (args.out/"learning-casebook-proof.json").write_text(json.dumps({
        **case_context,
        "casebook_sha256":sha(CASEBOOK),
        "model_weight_training":False,
        "model_self_review_not_independent":True,
        "verbatim_historical_actions_sent":False
    },indent=2)+"\n",encoding="utf-8")
    if args.prior_lessons:memory+="\n"+args.prior_lessons.read_text(encoding="utf-8")[:6000]
    current=args.current_frame
    source=args.source
    started_empty=args.source is None
    original_source_sha=sha(args.source) if args.source else None
    frozen_original_sha=args.frozen_original_asset_sha256 or original_source_sha
    accepted_chain=json.loads(args.accepted_chain.read_text(encoding="utf-8")) if args.accepted_chain else []
    if accepted_chain:
        if not args.source or accepted_chain[-1]["output_asset_sha256"]!=original_source_sha:
            raise ValueError("Next Qwen input must be last INDEPENDENTLY ACCEPTED output SHA")
        if accepted_chain[0]["original_asset_sha256"]!=frozen_original_sha:
            raise ValueError("Frozen original asset does not match certified history")
    elif args.frozen_original_asset_sha256 and args.frozen_original_asset_sha256!=original_source_sha:
        raise ValueError("Cannot start a non-original source without an independently accepted chain")
    history=""
    known_objects=[]
    verified_objects=[]
    spatial_views=[]
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
        spatial_views=[inspected/"source-top-blender.png",inspected/"source-side-blender.png"]
        verified_objects=[{"name":x["name"],"vertices":x["vertices"],"faces":x["faces"],
                          "evaluated_triangles":x["evaluated_triangles"],
                          "world_location_m":x["world_location_m"],
                          "world_dimensions_m":x["world_dimensions_m"],
                          "world_bounds_min_m":x["world_bounds_min_m"],
                          "world_bounds_max_m":x["world_bounds_max_m"],
                          "world_bounds_center_m":x["world_bounds_center_m"],
                          "world_bounds_dimensions_m":x["world_bounds_dimensions_m"],
                          "world_up_axis":x["world_up_axis"],
                          "mesh_local_axis_aligned_with_world_up":x["mesh_local_axis_aligned_with_world_up"],
                          "local_bounds_min_m":x["local_bounds_min_m"],
                          "local_bounds_max_m":x["local_bounds_max_m"],
                          "local_extrema":x["local_extrema"],
                          "vertex_landmarks":x.get("vertex_landmarks",[]),
                          "face_landmarks":x.get("face_landmarks",[]),
                          "camera_visible_face_landmarks":x.get("camera_visible_face_landmarks",[]),
                          "edges":x.get("edges",0),
                          "edge_landmarks":x.get("edge_landmarks",[]),
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
    invisible_plan_signatures=set()
    invisible_source_ops=set()
    rejected_invisible_family_count={}
    rejected_plan_family_counts={}
    for cycle in range(1,args.max_cycles+1):
        trial=args.out/("cycle-"+str(cycle));trial.mkdir()
        doc=None
        for attempt in range(1,args.max_plan_attempts+1):
            try:
                doc=ask_plan(args,args.reference,current,history,role,memory,trial,attempt,source_exists=bool(source),known_objects=known_objects,source_inventory=verified_objects,spatial_views=spatial_views,prior_invisible_signatures=invisible_plan_signatures,excluded_source_ops=invisible_source_ops)
                break
            except (RejectedPlan,CompositionRejected,json.JSONDecodeError) as e:
                # Keep every exact rejected proposal and the actionable validator
                # reason: never clamp geometry or silently rewrite model values.
                raw_attempt=trial/("qwen-raw-attempt-"+str(attempt)+".json")
                (trial/("rejected-plan-attempt-"+str(attempt)+".json")).write_text(
                    json.dumps({"attempt":attempt,"rejection":str(e),
                      "raw_model_sha256":sha(raw_attempt) if raw_attempt.is_file() else None,
                      "no_artistic_values_rewritten":True},indent=2)+"\n",encoding="utf-8")
                family=track_rejected_strategy(
                    raw_attempt,rejected_plan_family_counts,invisible_source_ops)
                history+="\nA non-executable Qwen proposal was REJECTED: "+str(e)
                if family and family in invisible_source_ops:
                    history+=("\nOWNER CONTRACT V1: "+family+
                              " failed at least twice; it is now excluded "
                              "from the future Qwen schema. Choose a "
                              "DIFFERENT geometric or artistic hypothesis.")
                if attempt==args.max_plan_attempts:
                    diagnostic={"cycle":cycle,"status":"PLAN_STRATEGY_EXHAUSTED",
                        "failure_kind":"FIVE_ORIGINAL_QWEN_PLANS_REJECTED_BEFORE_BLENDER",
                        "blocked_tool_families":sorted(invisible_source_ops),
                        "last_rejection":str(e),
                        "original_source_sha256":frozen_original_sha,
                        "model_weight_training":False,"art_approved":False}
                    records.append(diagnostic)
                    (trial/"five-attempt-strategy-diagnosis.json").write_text(
                        json.dumps(diagnostic,indent=2)+"\n",encoding="utf-8")
                    history+=("\nFIVE invalid plan attempts; do not repeat "
                              "this artistic/geometry hypothesis. Move to "
                              "the next distinct safe tool family if available.")
        if doc is None:
            continue
        candidate=trial/"artist-plan.json";candidate.write_text(json.dumps(doc,indent=2)+"\n")
        stage=trial/"output"
        try:
            run_blender(args.blender,candidate,stage,source,args.texture_root,args.review,args.asset_catalog,args.transport,args.mcp_addon,args.mcp_sha256)
        except Exception as ex:
            records.append({"cycle":cycle,"status":"BLENDER_REJECTED","error":str(ex)[-800:]})
            history+="\nACTUAL BLENDER REJECTION: "+str(ex)[-1100:]
            continue
        result=json.loads((stage/"execution-proof.json").read_text())
        if source and args.phase=="full" and verified_objects:
            baseline=result.get("baseline_evaluated_triangles")
            after=result.get("evaluated_triangles")
            # Both are Blender evaluated triangles on the same source and
            # neutral review stage. Face counts were never comparable.
            guard=check_triangle_growth(baseline,after)
            (trial/"evaluated-triangle-comparison.json").write_text(
                json.dumps(guard,indent=2)+"\\n",encoding="utf-8")
            if not guard["allowed"]:
                records.append({"cycle":cycle,"status":"MOBILE_MESH_DENSITY_REJECTED",
                                "baseline_evaluated_triangles":baseline,
                                "triangles":after,"budget":guard["triangle_limit"],
                                "plan_sha256":sha(candidate)})
                history+=f"\\nREJECTED: actual Blender triangle growth {baseline} -> {after} exceeds safe limit {guard['triangle_limit']}. Use localized changes, not global subdivision."
                continue
        if args.phase=="mesh_edit" and source:
            edits=[step for step in result["execution"] if step["op"]=="sculpt_anchor"
                   and step["geometry_changed"] and step["originals_changed"]]
            if not edits:
                records.append({"cycle":cycle,"status":"INHERITED_MESH_UNCHANGED",
                                "plan_sha256":sha(candidate)})
                history+="\nREJECTED: Blender changed no vertex data of an inherited mesh; select a brush that reaches real source vertices, not an object transform."
                continue
        screen=screen_impact(stage/"before-blender.png",stage/"after-blender.png")
        (trial/"camera-screen-impact.json").write_text(json.dumps(screen,indent=2)+"\n")
        if not result["matched_review_camera"] or not screen["visually_present"]:
            # The permanent contract disallows two rejected retries with the
            # SAME geometric hypothesis, even if tiny shadow differences are
            # nonzero. Visibility threshold is rejection, not a quality score.
            invisible_plan_signatures.add(executed_action_signature(doc))
            if (source and args.source_tool_sequencing and
                    not args.source_build_phase and doc.get("actions") and
                    doc["actions"][0]["op"] in
                    ("extrude","inset","sculpt_anchor","sculpt","bevel")):
                family=doc["actions"][0]["op"]
                rejected_invisible_family_count[family]=(
                    rejected_invisible_family_count.get(family,0)+1)
                if rejected_invisible_family_count[family]>=2:
                    invisible_source_ops.add(family)
                    history+=("\nOWNER CONTRACT V1: two visually rejected attempts "
                              "using "+family+" => CHANGE GEOMETRIC HYPOTHESIS. "
                              "This source tool family is disabled for the "
                              "remaining cycles; select a genuinely different "
                              "original Blender tool, not a minor numeric variant.")
            stage_pending_art_evidence(
                trial/"pending-learning-case.json",model=args.model,
                source_hash=original_source_sha,plan_file=candidate,
                raw_response_file=trial/"qwen-raw.json",
                before_image=stage/"before-blender.png",
                after_image=stage/"after-blender.png",critique_file=None,
                comparison_file=trial/"camera-screen-impact.json",
                output_blend=stage/"artist-asset.blend",
                output_glb=stage/"artist-asset.glb",
                screen_gate="SCREENSPACE_REJECTED")
            records.append({"cycle":cycle,"status":"SCREENSPACE_REJECTED","screen_impact":screen})
            history+=("\nVISUAL CAMERA FAILURE: actual Blender A/B changed "+
                      str(screen["changed_fraction"])+
                      " of the screen, too little for the player camera. "
                      "The next Qwen plan must change the core silhouette and not simply tweak materials.")
            continue
        if args.review and args.source and args.phase=="full":
            # The render is real and the camera fixed. Qwen must not cover
            # art-directed player entry/stairs. Reject, never reposition art.
            region_proof=inspect_protected_regions(
                stage/"before-blender.png",stage/"after-blender.png",args.review)
            (trial/"protected-camera-regions.json").write_text(
                json.dumps(region_proof,indent=2)+"\n",encoding="utf-8")
            if not region_proof["allowed"]:
                offenders=[f"{z['name']} changed {z['changed_fraction']:.1%}"
                           for z in region_proof["zones"] if not z["allowed"]]
                stage_pending_art_evidence(
                    trial/"pending-learning-case.json",model=args.model,
                    source_hash=original_source_sha,plan_file=candidate,
                    raw_response_file=trial/"qwen-raw.json",
                    before_image=stage/"before-blender.png",
                    after_image=stage/"after-blender.png",critique_file=None,
                    comparison_file=trial/"camera-screen-impact.json",
                    protected_regions_file=trial/"protected-camera-regions.json",
                    output_blend=stage/"artist-asset.blend",
                    output_glb=stage/"artist-asset.glb",
                    screen_gate="PROTECTED_ACCESS_OCCLUSION_REJECTED")
                records.append({"cycle":cycle,"status":"PROTECTED_ACCESS_OCCLUSION_REJECTED",
                                "regions":region_proof})
                history+=("\nVISUAL QA REJECTED: new architecture obscures original access: "+
                          "; ".join(offenders)+
                          ". Qwen choose ORIGINAL geometry elsewhere without blocking door or stairs.")
                continue
        evaluation=critique(args,args.reference,stage/"before-blender.png",stage/"after-blender.png",role)
        (trial/"qwen-critique.json").write_text(json.dumps(evaluation,indent=2)+"\n")
        stage_pending_art_evidence(
            trial/"pending-learning-case.json",model=args.model,
            source_hash=original_source_sha,plan_file=candidate,
            raw_response_file=trial/"qwen-raw.json",
            before_image=stage/"before-blender.png",
            after_image=stage/"after-blender.png",
            critique_file=trial/"qwen-critique.json",
            comparison_file=trial/"camera-screen-impact.json",
            output_blend=stage/"artist-asset.blend",
            output_glb=stage/"artist-asset.glb")
        request=stage_visual_request(
            output=trial/"art-review-request.json",reference=args.reference,
            original_source_sha=frozen_original_sha,input_source=source if source else args.source,
            raw_plan=candidate,execution_proof=stage/"execution-proof.json",
            before=stage/"before-blender.png",after=stage/"after-blender.png",
            model=args.model,memory_proof=trial/"actual-qwen-context-executed.json",
            review_camera_file=args.review,
            run_id=int(os.environ.get("GITHUB_RUN_ID","0")) or None)
        records.append({"cycle":cycle,"status":"ART_REVIEW_REQUIRED",
                        "review_request_sha256":sha(trial/"art-review-request.json"),
                        "plan_sha256":sha(candidate),
                        "output_blend_sha256":result["blend_sha256"],
                        "output_glb_sha256":result["glb_sha256"],
                        "matched_review_camera":result["matched_review_camera"],
                        "critique":evaluation})
        # Qwen's critique is not independent; a visible output is a review
        # REQUEST, never an accepted source. Do not chain unreviewed cycles.
        path=(args.independent_reviews_dir/("cycle-"+str(cycle)+".json")
              if args.independent_reviews_dir else None)
        if path is None or not path.is_file():
            history+="\nOWNER QUALITY CONTRACT: candidate held for independent visual review; do not promote as next tower input."
            break
        try:
            review_decision=validate_independent_review(
                request,path,prior=accepted_chain[-1] if accepted_chain else None)
            accepted_chain=append_accepted(accepted_chain,review_decision,
                                          frozen_original=frozen_original_sha)
        except ValueError as rejection:
            records[-1]["status"]="ART_REJECTED"
            records[-1]["independent_visual_rejection"]=str(rejection)
            history+="\nINDEPENDENT ART REJECTED: "+str(rejection)
            break
        records[-1]["status"]="BLENDER_VISUAL_ACCEPTED"
        records[-1]["independent_review_sha256"]=sha(path)
        (args.out/"independently-accepted-chain.json").write_text(
            json.dumps(accepted_chain,indent=2)+"\n",encoding="utf-8")
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
        if should_continue_after_critique(cycle,args.max_cycles,args.source is None,evaluation,
               args.min_successful_cycles,len([r for r in records if r["status"]=="ART_REVIEW_REQUIRED"])):
            # Topology-changing actions invalidate original face IDs. Inspect
            # the actual generated BLEND before giving Qwen the next face map.
            next_inspection=trial/"reinspection"
            command=[args.blender,"-b","--python-exit-code","1","--python",
                     str(HERE/"inspect_source.py"),"--","--source",str(source),"--out",str(next_inspection)]
            if args.review:command+=["--review",str(args.review)]
            check=subprocess.run(command,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=900)
            (trial/"reinspection-blender-log.txt").write_text(check.stdout,encoding="utf-8")
            if check.returncode:raise RuntimeError("Blender reinspection failed: "+check.stdout[-1000:])
            inspected_again=json.loads((next_inspection/"source-inspection.json").read_text())
            spatial_views=[next_inspection/"source-top-blender.png",
                           next_inspection/"source-side-blender.png"]
            if not inspected_again["source_unchanged"] or inspected_again["source_sha256"]!=sha(source):
                raise RuntimeError("Reinspection changed candidate source")
            verified_objects=[{k:x[k] for k in ("name","vertices","faces","local_extrema",
                                                "local_bounds_min_m","local_bounds_max_m",
                                                "vertex_landmarks","face_landmarks","camera_visible_face_landmarks",
                                                "edges","edge_landmarks",
                                                "evaluated_triangles",
                                                "world_bounds_min_m","world_bounds_max_m",
                                                "world_bounds_center_m","world_bounds_dimensions_m",
                                                "world_up_axis","mesh_local_axis_aligned_with_world_up",
                                                "materials","uv_layers")}
                              for x in inspected_again["objects"][:80]]
            known_objects=[x["name"] for x in verified_objects]
            history+="\\nUP-TO-DATE ACTUAL BLENDER FACE/EXTREMA INVENTORY: "+json.dumps(verified_objects)[:12000]
        if not should_continue_after_critique(cycle,args.max_cycles,started_empty,evaluation,
               args.min_successful_cycles,len([r for r in records if r["status"]=="ART_REVIEW_REQUIRED"])):break
        if started_empty and cycle==1:
            history+="\\nFIRST CYCLE WAS BLOCKOUT ONLY: source geometry now exists. For this second cycle choose actual higher-level Blender finishing operations (mesh editing, material, UV, bevel, surface/node work) guided by the real render and reference. No preset values."
    rejected_count=sum(rec.get("status") in (
            "SCREENSPACE_REJECTED","PROTECTED_ACCESS_OCCLUSION_REJECTED",
            "MOBILE_MESH_DENSITY_REJECTED","BLENDER_REJECTED","ART_REJECTED")
                       for rec in records)
    if rejected_count>=5 and not accepted_chain:
        (args.out/"owner-strategy-diagnosis.json").write_text(
            json.dumps({"status":"STRATEGY_REVIEW_REQUIRED",
                        "rejected_cycles":rejected_count,
                        "source_sha256":original_source_sha,
                        "excluded_failed_tool_families":sorted(invisible_source_ops),
                        "accepted_interventions":0,
                        "reason":"Five failed complete cycles; review actual "
                        "camera, mesh face choice, artistic hypothesis and "
                        "model constraints before any repeated run.",
                        "art_approved":False},indent=2)+"\n")
    if args.source and sha(args.source)!=original_source_sha:raise RuntimeError("Original frozen source mutated")
    evidence_summary=summarize_real_artist_cycles(records)
    calibration=visual_calibration_status(accepted_chain)
    final={"state":calibration["status"],"owner_art_quality_contract":"V1",
           "formal_verifier":"tools/art-visual-acceptance/verify.mjs",
           "blender_visual_accepted_count":calibration["blender_visual_accepted_count"],
           "required_accepted_count":3,
           "last_independent_visual_score":calibration["last_visual_score"],
           "frozen_original_asset_sha256":frozen_original_sha,
           "formal_art_approval_manifest_present":False,
           "ai_authored_action_plans":evidence_summary["qa_review_candidate_count"],
           **evidence_summary,
           "source_authority_unchanged":True,"frozen_source_sha256":original_source_sha,
           "blender_real_execution":evidence_summary["real_blender_execution_count"]>0,
           "art_approved":False,"unity_art_verified":False,"physical_mobile_validated":False,
           "independent_artistic_improvement_verified":False,
           "model":args.model,"num_ctx":int(os.environ.get("ELDORIA_ARTIST_NUM_CTX","16384")),
           "vision_max_edge":args.vision_max_edge,
           "system_profile_sha256":hashlib.sha256(role.encode()).hexdigest(),
           "journal_sha256":sha(JOURNAL),"cycles":records}
    (args.out/"full-studio-result.json").write_text(json.dumps(final,indent=2)+"\n")
    print("ELDORIA_FULL_STUDIO_CYCLE",json.dumps({k:final[k] for k in
        ("state","ai_authored_action_plans","blender_real_execution","art_approved")}))
    if not final["blender_real_execution"]:
        raise RuntimeError("No actual Blender execution; inspect raw Qwen and validator failures")
    if not final["ai_authored_action_plans"]:
        raise RuntimeError("Blender executed Qwen plans, but every candidate failed safety or screen QA; nothing suitable for visual review")
if __name__=="__main__":main()
