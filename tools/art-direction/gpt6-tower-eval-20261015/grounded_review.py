"""Fail-closed, source-grounded critic protocol for existing Eldoria Blender A/B.

Never authorises artistic approval: validates identification, attribution and
geometry facts separately from the model's subjective visual judgement.
"""
from __future__ import annotations
import hashlib, json, math, pathlib, re

class UnreliableCritic(ValueError):
    pass

def sha(path):
    return hashlib.sha256(pathlib.Path(path).read_bytes()).hexdigest()

def manifest(plan, catalog, contacts):
    if catalog.get("artist_plan_sha256") is None:
        raise UnreliableCritic("MISSING_PLAN_HASH")
    author = {}
    for action in plan.get("actions", []):
        if action.get("op") in ("primitive", "mesh", "import_asset"):
            name = action.get("target")
            if not isinstance(name, str) or name in author:
                raise UnreliableCritic("DUPLICATE_OR_MISSING_TARGET")
            author[name] = {
                "id": name, "operation": action["op"],
                "shape": action.get("params", {}).get("shape", "custom"),
                "location_m": action.get("params", {}).get("location"),
                "scale_m": action.get("params", {}).get("scale"),
                "rotation_rad": action.get("params", {}).get("rotation", [0,0,0]),
            }
    # A real Blender workshop can author joined members directly rather than
    # using legacy primitive actions; reuse identical provenance controls.
    if not author and isinstance(plan.get("members"),list):
        for m in plan["members"]:
            name=m.get("id")
            if not isinstance(name,str) or name in author:
                raise UnreliableCritic("DUPLICATE_OR_MISSING_TARGET")
            author[name]={"id":name,"operation":"real_boolean_joinery",
                "shape":"bevelled_timber_with_mortise_tenon",
                "location_m":m.get("center_m"),"scale_m":m.get("size_m"),
                "rotation_rad":[0,0,0]}
    actual = set(catalog.get("new_components", []))
    if set(author) != actual or not author:
        raise UnreliableCritic("PLAN_OUTPUT_COMPONENT_SET_MISMATCH")
    measured = {c["name"]: c for c in contacts.get("objects", [])}
    if set(measured) != actual:
        raise UnreliableCritic("CONTACT_COMPONENT_SET_MISMATCH")
    components = {c["name"]: c for c in catalog.get("components", [])
                  if c.get("derived_from_source_object") is False}
    if set(components) != actual:
        raise UnreliableCritic("GEOMETRY_COMPONENT_SET_MISMATCH")
    for name, obj in author.items():
        obj["mesh_vertices"] = components[name]["vertices"]
        obj["dimensions_m"] = components[name]["dimensions_m"]
        obj["surface_distance_m"] = measured[name]["min_real_mesh_surface_distance_m"]
        obj["contact_tolerance_m"] = measured[name]["contact_tolerance_m"]
        obj["surface_near_original"] = measured[name]["surface_contact_candidate"]
        # Geometric proximity does NOT prove beautiful joining or load bearing.
        if not isinstance(obj["surface_distance_m"], (int,float)):
            raise UnreliableCritic("UNMEASURED_CONTACT")
    inherited = [c["name"] for c in catalog["components"]
                 if c.get("derived_from_source_object") is True]
    if not inherited:
        raise UnreliableCritic("ORIGINAL_SOURCE_MISSING")
    return {"schema_version": 2, "plan_asset_id": plan["asset_id"],
            "new_components": [author[k] for k in sorted(author)],
            "protected_inherited_meshes": sorted(inherited),
            "counts": {"new": len(author), "inherited": len(inherited)},
            "art_approved": False}

def forbidden_unobserved_terms(inventory):
    """Reject stale first-attempt 'fascia/diagonal' when no named/rotated parts.

    This safeguard supplements (not substitutes for) object-level claims.
    Descriptive references to original roof/stairs are allowed.
    """
    parts = inventory["new_components"]
    ids = " ".join(x["id"].lower() for x in parts)
    rotated = any(any(abs(float(v)) > 0.12 for v in (x.get("rotation_rad") or [0,0,0]))
                  for x in parts)
    result = []
    if not rotated and not any("diagonal" in x["id"].lower() or "brace" in x["id"].lower() for x in parts):
        result += [r"\bdiagonal(?:s)?\b", r"\bdiagonales?\b"]
    if "fascia" not in ids:
        result += [r"\bfascia\b", r"\bfascias\b"]
    return result

def validate_critique(payload, inventory, image_hashes):
    """Validate what a critic says it saw; fail closed even if JSON parses."""
    errors=[]
    if not isinstance(payload, dict):
        return {"status":"REVIEW_UNGROUNDED","errors":["NOT_JSON_OBJECT"],"art_approved":False}
    ids={c["id"] for c in inventory["new_components"]}
    if payload.get("image_sha256") != image_hashes:
        errors.append("IMAGE_PROVENANCE_NOT_ECHOED")
    observed=payload.get("observations")
    if not isinstance(observed,list) or {o.get("component_id") for o in observed if isinstance(o,dict)} != ids or len(observed)!=len(ids):
        errors.append("INVENTORY_IDENTIFICATION_MISMATCH")
    else:
        facts={c["id"]:c for c in inventory["new_components"]}
        for o in observed:
            if not isinstance(o,dict):
                errors.append("MALFORMED_OBSERVATION");continue
            if o.get("visibility") not in {"visible","not_visible","uncertain"}:
                errors.append("MISSING_VISIBILITY")
            if o.get("visual_joinery") not in {"convincing","poor","uncertain","not_applicable"}:
                errors.append("MISSING_JOINERY")
            if o.get("measured_contact") != ("NEAR_SOURCE" if facts[o["component_id"]]["surface_near_original"] else "NOT_NEAR_SOURCE"):
                errors.append("PHYSICAL_CONTACT_CONTRADICTED")
            if not isinstance(o.get("visual_reason"),str) or len(o["visual_reason"].strip()) < 8:
                errors.append("UNSUPPORTED_VISUAL_REASON")
    if payload.get("decision") not in ("IMPROVED","REJECTED","INCONCLUSIVE"):
        errors.append("MISSING_DECISION")
    sc=payload.get("scores")
    if not isinstance(sc,dict) or set(sc)!= {"silhouette","architectural_contact","materials","style","game_camera"} or any(
            type(v) not in (int,float) or not math.isfinite(v) or v < 0 or v > 10 for v in sc.values()):
        errors.append("BAD_RUBRIC")
    for key in ("reasons","uncertainties"):
        if not isinstance(payload.get(key), list) or any(not isinstance(x,str) for x in payload[key]):
            errors.append("BAD_"+key.upper())
    full=" ".join([json.dumps(payload.get("observations",[]),ensure_ascii=False),
                   json.dumps(payload.get("reasons",[]),ensure_ascii=False),
                   json.dumps(payload.get("uncertainties",[]),ensure_ascii=False)]).lower()
    for term in forbidden_unobserved_terms(inventory):
        if re.search(term,full):
            errors.append("UNOBSERVED_FEATURE_"+term)
    return {"status":"REVIEW_UNGROUNDED" if errors else "PROVISIONAL_REVIEW_GROUNDED",
            "errors": sorted(set(errors)),
            "critic_decision":payload.get("decision") if not errors else "WITHHELD",
            "art_approved":False, "calibration_validated":False}

def critic_messages(inventory, hashes):
    # No previous-artifact reasons, negative hypotheses, or editor commentary.
    # Critic first inventories actual objects, THEN separately judges visual art.
    objective=[{"component_id": c["id"],"shape":c["shape"],
                "dimensions_m":c["dimensions_m"],
                "rotation_rad":c["rotation_rad"],
                "surface_contact_m":c["surface_distance_m"],
                "measured_contact":"NEAR_SOURCE" if c["surface_near_original"] else "NOT_NEAR_SOURCE"}
               for c in inventory["new_components"]]
    instructions = ("You are an independent Blender screenshot critic. Inspect only the 3 supplied genuine images "
        "(approved visual language reference, BEFORE, AFTER). An inventory derived from actual exported Blender meshes "
        "is authoritative about which NEW parts exist. DO NOT remember or discuss previous candidates. "
        "Do not invent ANY new components, construction details or identifiers absent from the inventory. "
        "Geometric NEAR_SOURCE measures sampled proximity only: it does not guarantee structurally welded or visually "
        "convincing joinery. Discuss visual joinery SEPARATELY. Original roof and steps may be discussed. "
        "For each inventory component, return exactly one observation; no missing or extra component. "
        "Uncertain observations are welcome. Strict JSON object fields: image_sha256 (exact provided object), "
        "observations (list of {component_id,visibility:visible|not_visible|uncertain, "
        "visual_joinery:convincing|poor|uncertain|not_applicable, "
        "measured_contact:NEAR_SOURCE|NOT_NEAR_SOURCE,visual_reason}), "
        "decision:IMPROVED|REJECTED|INCONCLUSIVE, "
        "scores:{silhouette,architectural_contact,materials,style,game_camera} with numbers 0..10, "
        "reasons (list of strings), uncertainties (list of strings). JSON only. "
        "This review is provisional and never constitutes ART_APPROVED.")
    user = "ACTUAL NEW COMPONENTS (not suggestions): "+json.dumps(objective,ensure_ascii=False,separators=(",",":"))+"\nIMAGE HASHES: "+json.dumps(hashes,sort_keys=True)
    return [{"role":"system","content":instructions},{"role":"user","content":user}]

def extract_json(s):
    if not isinstance(s,str) or not s.strip():
        raise UnreliableCritic("EMPTY_RESPONSE")
    cleaned=s.strip()
    if cleaned.startswith("".join(["`","`","`"])):
        cleaned=re.sub(r"^\x60{3}(?:json)?\s*", "", cleaned)
        cleaned=re.sub(r"\s*\x60{3}$", "", cleaned)
    try:
        parsed=json.loads(cleaned)
    except json.JSONDecodeError as e:
        raise UnreliableCritic("INVALID_CRITIC_JSON") from e
    if not isinstance(parsed,dict):
        raise UnreliableCritic("NON_OBJECT_RESPONSE")
    return parsed
