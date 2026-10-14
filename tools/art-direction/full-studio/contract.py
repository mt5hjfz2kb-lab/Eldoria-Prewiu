"""Strict, extendable contracts for the Eldoria general-purpose Blender artist.

The AI selects operations and values. This module never writes or fixes a
creative decision. Unsafe or malformed decisions are rejected and sent back.
"""
from __future__ import annotations
import json
import math
from pathlib import Path

OPS = {
 "primitive", "mesh", "duplicate", "transform", "sculpt", "extrude",
 "inset", "subdivide", "bevel", "modifier", "boolean", "material",
 "uv", "join", "delete", "light", "camera", "render_settings",
 "texture", "asset_info",
}
SHAPES = {"cube", "cylinder", "cone", "plane", "uv_sphere", "ico_sphere", "torus"}
MODIFIERS = {
 "BEVEL", "SUBSURF", "SOLIDIFY", "DECIMATE", "REMESH", "MIRROR",
 "ARRAY", "WEIGHTED_NORMAL", "TRIANGULATE", "SIMPLE_DEFORM", "DISPLACE",
}
UV_METHODS = {"smart", "cube", "sphere", "cylinder"}
BOOLEANS = {"DIFFERENCE", "UNION", "INTERSECT"}
MAX_ACTIONS = 160
MAX_MESH_VERTICES = 80000
MAX_MESH_FACES = 90000
MAX_OUTPUT_TRIANGLES = 220000
MAX_LIGHTS = 12

class RejectedPlan(ValueError):
    pass

def finite_number(x, low=-1e5, high=1e5):
    return type(x) in (int, float) and math.isfinite(x) and low <= x <= high

def vector(x, size=3, low=-1e5, high=1e5):
    return isinstance(x, list) and len(x) == size and all(finite_number(v, low, high) for v in x)

def safe_name(name):
    return isinstance(name, str) and 1 <= len(name) <= 90 and all(
        c.isalnum() or c in " _-." for c in name) and not name.startswith(".")

def require(condition, why):
    if not condition:
        raise RejectedPlan(why)

def validate_plan(doc):
    """Reject ambiguous, self-approving, unsafe, or nonexistent actions."""
    require(isinstance(doc, dict), "Expected JSON object")
    require(doc.get("schema_version") == 2, "schema_version must be 2")
    for field in ("asset_id", "observed_defect", "design_intent", "expected_visual_difference"):
        require(isinstance(doc.get(field), str) and len(doc[field]) >= (30 if field != "asset_id" else 2), "Missing meaningful "+field)
    require(doc.get("asset_decision") in ("NEW", "MODIFY", "REUSE"), "asset_decision invalid")
    require(doc.get("art_approved") is not True, "AI cannot approve its own art")
    actions = doc.get("actions")
    require(isinstance(actions, list) and 1 <= len(actions) <= MAX_ACTIONS, "Expected 1..160 AI-authored actions")
    ids, created, lights = set(), set(), 0
    geometric = 0
    for index, a in enumerate(actions):
        require(isinstance(a, dict), f"action {index}: expected object")
        op, p = a.get("op"), a.get("params")
        require(op in OPS, f"action {index}: unsupported Blender operation {op}")
        require(safe_name(a.get("id")) and a["id"] not in ids, f"action {index}: duplicate/invalid id")
        ids.add(a["id"])
        require(isinstance(a.get("reason"), str) and len(a["reason"]) >= 15,
                f"action {index}: missing artistic rationale")
        require(isinstance(p, dict), f"action {index}: params object required")
        name = a.get("target")
        if op not in ("render_settings", "asset_info"):
            require(safe_name(name), f"action {index}: invalid object name")
        if op == "primitive":
            require(p.get("shape") in SHAPES, "Unknown primitive shape")
            require(vector(p.get("location", [0,0,0])) and vector(p.get("rotation", [0,0,0])),
                    "Primitive transform invalid")
            require(vector(p.get("scale", [1,1,1]), low=0.0001, high=500),
                    "Primitive scale invalid")
            require(name not in created, "Creation name repeated")
            created.add(name); geometric += 1
        elif op == "mesh":
            vs, fs = p.get("vertices"), p.get("faces")
            require(isinstance(vs,list) and 3 <= len(vs) <= MAX_MESH_VERTICES and all(vector(v) for v in vs), "Invalid authored vertices")
            require(isinstance(fs,list) and 1 <= len(fs) <= MAX_MESH_FACES and all(
                isinstance(f,list) and 3 <= len(f) <= 12 and
                len(set(f)) == len(f) and
                all(type(i) is int and 0 <= i < len(vs) for i in f) for f in fs), "Invalid authored faces")
            require(name not in created, "Creation name repeated")
            created.add(name); geometric += 1
        elif op in ("duplicate", "light", "camera"):
            require(name not in created, "Creation name repeated")
            if op == "duplicate":
                require(safe_name(p.get("source")), "Duplicate needs source name")
                geometric += 1
            if op == "light":
                lights += 1
                require(lights <= MAX_LIGHTS and p.get("kind", "AREA") in ("SUN", "AREA", "POINT", "SPOT"), "Invalid lighting setup")
            if op == "camera":
                require(vector(p.get("location")) and vector(p.get("rotation")), "Camera location/rotation required")
            created.add(name)
        elif op == "transform":
            require(any(k in p for k in ("location","rotation","scale")), "Empty transform")
            for key in ("location","rotation"):
                if key in p: require(vector(p[key]), key+" invalid")
            if "scale" in p: require(vector(p["scale"],low=0.0001,high=500), "scale invalid")
            geometric += 1
        elif op == "sculpt":
            require(vector(p.get("center")) and vector(p.get("delta")) and
                    any(abs(v)>1e-8 for v in p["delta"]) and
                    finite_number(p.get("radius"),0.001,1000), "Invalid AI sculpt brush")
            geometric += 1
        elif op in ("extrude", "inset", "subdivide", "bevel"):
            require(isinstance(p.get("faces" if op in ("extrude","inset","subdivide") else "edges", []),list), "Indices must be a list")
            if op == "extrude": require(vector(p.get("offset")), "Extrude requires original displacement")
            if op == "inset": require(finite_number(p.get("thickness"),0.0001,100), "Inset thickness invalid")
            if op == "bevel": require(finite_number(p.get("width"),0.0001,100), "Bevel width invalid")
            geometric += 1
        elif op == "modifier":
            require(p.get("kind") in MODIFIERS, "Modifier not supported")
            require(isinstance(p.get("options", {}),dict), "Modifier options invalid")
            require(set(p.get("options",{})) <= {"width","segments","levels","render_levels","thickness","ratio","voxel_size","count","offset","axis","angle","strength","texture_scale","use_clamp_overlap"}, "Unsafe modifier options")
            geometric += 1
        elif op == "boolean":
            require(p.get("operation") in BOOLEANS and safe_name(p.get("operand")), "Invalid boolean")
            geometric += 1
        elif op == "material":
            require(vector(p.get("base_color"),size=4,low=0,high=1) and
                    finite_number(p.get("roughness"),0,1) and
                    finite_number(p.get("metallic",0),0,1), "Invalid PBR material")
            if "texture_path" in p:
                require(isinstance(p["texture_path"],str) and len(p["texture_path"])<260 and not Path(p["texture_path"]).is_absolute() and ".." not in Path(p["texture_path"]).parts, "Texture path must be relative to approved source")
        elif op == "uv":
            require(p.get("method") in UV_METHODS, "Unknown UV method")
        elif op == "join":
            require(isinstance(p.get("sources"),list) and len(p["sources"])>=2 and
                    all(safe_name(n) for n in p["sources"]), "Join needs multiple object names")
            geometric += 1
        elif op == "texture":
            require(p.get("method") in ("NOISE","CHECKER"), "Texture method invalid")
            require(vector(p.get("color_a"),4,0,1) and vector(p.get("color_b"),4,0,1), "Texture colors invalid")
            require(type(p.get("resolution",512)) is int and 64 <= p.get("resolution",512) <= 1024, "Texture resolution invalid")
            require(finite_number(p.get("scale"),0.01,1000), "Texture scale invalid")
        elif op == "render_settings":
            require(p.get("engine","CYCLES") in ("CYCLES","BLENDER_EEVEE_NEXT","BLENDER_EEVEE"), "Render engine invalid")
            require(type(p.get("samples",32)) is int and 4<=p.get("samples",32)<=256, "Samples budget invalid")
            require(type(p.get("width",960)) is int and 320<=p.get("width",960)<=1920, "Width budget invalid")
            require(type(p.get("height",720)) is int and 240<=p.get("height",720)<=1200, "Height budget invalid")
    require(geometric > 0, "No substantial authored geometry action")
    return {"asset_id":doc["asset_id"],"action_count":len(actions),"geometric_actions":geometric}

def load_plan(filename):
    return validate_and_return(json.loads(Path(filename).read_text(encoding="utf-8")))

def validate_and_return(doc):
    validate_plan(doc)
    return doc
