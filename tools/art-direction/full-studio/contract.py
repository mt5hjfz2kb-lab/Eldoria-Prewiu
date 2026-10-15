"""Strict, extendable contracts for the Eldoria general-purpose Blender artist.

The AI selects operations and values. This module never writes or fixes a
creative decision. Unsafe or malformed decisions are rejected and sent back.
"""
from __future__ import annotations
import json
import math
from pathlib import Path

OPS = {
 "primitive", "mesh", "beam_between_faces", "duplicate", "transform", "sculpt", "sculpt_anchor", "extrude",
 "inset", "subdivide", "bevel", "modifier", "boolean", "material",
 "uv", "join", "delete", "light", "camera", "render_settings",
 "texture", "asset_info", "import_asset", "native_operator", "shader_graph", "geometry_nodes",
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
        c.isalnum() or c in " _-.·" for c in name) and not name.startswith(".")

def safe_action_id(value):
    # Action IDs are log identifiers, NEVER Blender object names or file paths.
    # Local 2B Qwen sometimes emits "/0"; that is not an unsafe command.
    return isinstance(value,str) and 1 <= len(value) <= 60 and all(
        c.isascii() and (c.isalnum() or c in "_-./") for c in value)

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
        require(safe_action_id(a.get("id")) and a["id"] not in ids, f"action {index}: duplicate/invalid id")
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
        elif op == "beam_between_faces":
            # An actual Qwen-authored timber/stone support between two Blender
            # camera-verified source-face anchors. Only the interpolation is
            # deterministic; Qwen owns the endpoints, profile and artistic intent.
            require(safe_name(p.get("source")) and p["source"] != name,
                    "Beam requires distinct real source object")
            indices=(p.get("start_face"),p.get("end_face"))
            require(all(type(x) is int and x >= 0 for x in indices)
                    and indices[0]!=indices[1],
                    "Beam requires two different source face IDs")
            require(finite_number(p.get("width_m"),0.06,0.45)
                    and finite_number(p.get("depth_m"),0.06,0.45),
                    "Beam profile must be tangible and physically bounded")
            require(name not in created,"Beam name shadows another authored mesh")
            created.add(name);geometric+=1
        elif op == "import_asset":
            file=p.get("relative_path")
            require(isinstance(file,str) and 4<len(file)<260 and
                    not Path(file).is_absolute() and
                    ".." not in Path(file).parts and
                    Path(file).suffix.lower() in (".glb",".gltf"),
                    "Imported source must be a relative, approved GLB/glTF")
            sha=p.get("sha256")
            require(isinstance(sha,str) and len(sha)==64 and
                    all(c in "0123456789abcdef" for c in sha.lower()),
                    "Imported source needs exact SHA256")
            require(vector(p.get("location",[0,0,0])) and
                    vector(p.get("rotation",[0,0,0])) and
                    vector(p.get("scale",[1,1,1]),low=0.0001,high=500),
                    "Imported source transform invalid")
            require(name not in created,"Repeated import asset name")
            created.add(name);geometric+=1
        elif op == "mesh":
            vs, fs = p.get("vertices"), p.get("faces")
            require(isinstance(vs,list) and 3 <= len(vs) <= MAX_MESH_VERTICES and all(vector(v) for v in vs), "Invalid authored vertices")
            require(isinstance(fs,list) and 1 <= len(fs) <= MAX_MESH_FACES and all(
                isinstance(f,list) and 3 <= len(f) <= 12 and
                len(set(f)) == len(f) and
                all(type(i) is int and 0 <= i < len(vs) for i in f) for f in fs), "Invalid authored faces")
            # A 3D model cannot be valid when every supposedly distinct
            # vertex shares the same coordinates or its faces have zero area.
            # Qwen MUST re-author bad artistic geometry; never infer or fix it.
            require(len({tuple(v) for v in vs})==len(vs),
                    "Custom mesh has DUPLICATE 3D vertices; choose ORIGINAL distinct [x,y,z] positions")
            def real_face_area(face):
                a=vs[face[0]]
                total=0.0
                for q in range(1,len(face)-1):
                    b=vs[face[q]];c=vs[face[q+1]]
                    u=[b[i]-a[i] for i in range(3)]
                    v=[c[i]-a[i] for i in range(3)]
                    cross=[u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]]
                    total+=math.sqrt(sum(t*t for t in cross))*0.5
                return total
            require(all(real_face_area(face)>1e-5 for face in fs),
                    "Custom mesh has ZERO-AREA faces; author real noncollinear vertex positions")
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
        elif op == "sculpt_anchor":
            require(p.get("anchor") in ("x_min","x_max","y_min","y_max","z_min","z_max"),
                    "Real inherited mesh anchor required")
            require(p.get("direction") in ("+X","-X","+Y","-Y","+Z","-Z"),
                    "Original AI direction required")
            require(finite_number(p.get("distance"),0.01,1.8) and
                    finite_number(p.get("radius"),0.1,8),
                    "Nonzero AI sculpt distance and bounded radius required")
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
        elif op in ("native_operator","shader_graph","geometry_nodes"):
            from extended_ops import validate_extended
            validate_extended(a,require)
            if op!="shader_graph":geometric+=1
        elif op == "render_settings":
            require(p.get("engine","CYCLES") in ("CYCLES","BLENDER_EEVEE_NEXT","BLENDER_EEVEE"), "Render engine invalid")
            require(type(p.get("samples",32)) is int and 4<=p.get("samples",32)<=256, "Samples budget invalid")
            require(type(p.get("width",960)) is int and 320<=p.get("width",960)<=1920, "Width budget invalid")
            require(type(p.get("height",720)) is int and 240<=p.get("height",720)<=1200, "Height budget invalid")
    # An actual PBR surface-only pass on an immutable existing BLEND is a
    # permitted artistic modification. New assets still require real geometry.
    material_edit = doc.get("asset_decision") in ("MODIFY","REUSE") and any(
        a.get("op") in ("material","shader_graph") for a in actions)
    require(geometric > 0 or material_edit, "No real authored geometry or existing-source material edit")
    return {"asset_id":doc["asset_id"],"action_count":len(actions),"geometric_actions":geometric}

def load_plan(filename):
    return validate_and_return(json.loads(Path(filename).read_text(encoding="utf-8")))

def validate_and_return(doc):
    validate_plan(doc)
    return doc
