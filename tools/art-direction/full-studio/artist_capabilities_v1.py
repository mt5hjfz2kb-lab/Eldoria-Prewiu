"""Evidence-based artist capability adapter for the EXISTING Eldoria Blender studio.

This is NOT a new mesh generator or an art approver. It exposes tested, typed
Blender operations to the model as architectural skills while retaining all
model-chosen artistic values and the original validation/execution path.
No Blender/Ollama import, subprocess, network, or file mutation at import time.
"""
from __future__ import annotations

import hashlib
import json
from pathlib import Path

from contract import OPS, RejectedPlan, validate_plan

# Semantic intentions, not geometry macros or hard-coded mesh recipes.
# Every allowed operation below already exists in contract.OPS and
# blender_toolbox.run_action; Qwen determines target, location and dimensions.
ARCHITECTURAL_SKILLS = {
    "timber_bracing_and_joinery": {
        "purpose": "For verified timber buildings: roof-edge fascia, slender bracing, beam joints and real contact with existing wooden structures.",
        "operations": ("beam_between_faces", "mesh", "primitive", "extrude", "bevel", "boolean", "modifier", "material"),
    },
    "timber_roof_and_eaves": {
        "purpose": "For verified timber roofs: thin sloped fascia, eaves, rafter supports and coherent shingles without disturbing original UV atlases.",
        "operations": ("beam_between_faces", "mesh", "primitive", "extrude", "bevel", "modifier", "material"),
    },
    "masonry_and_parapet": {
        "purpose": "Improve the inherited wall rhythm, masonry depth and parapet.",
        "operations": ("extrude", "inset", "bevel", "sculpt", "modifier", "material"),
    },
    "buttress_and_support": {
        "purpose": "Create architecturally justified buttress depth/contact.",
        "operations": ("extrude", "mesh", "primitive", "boolean", "modifier", "material"),
    },
    "openings_and_recesses": {
        "purpose": "Improve real window/arrow-loop recession and wall thickness.",
        "operations": ("inset", "extrude", "boolean", "bevel", "modifier"),
    },
    "stone_weathering": {
        "purpose": "Author localized stone wear, broken edges and coherent response.",
        "operations": ("sculpt", "sculpt_anchor", "bevel", "material", "uv", "shader_graph"),
    },
    "materials_and_surface": {
        "purpose": "Develop PBR/material hierarchy without replacing protected UV atlases.",
        "operations": ("material", "texture", "uv", "shader_graph", "geometry_nodes"),
    },
    "reuse_authored_assets": {
        "purpose": "Reuse source meshes or licensed verified assets, preserving source identity.",
        "operations": ("duplicate", "transform", "import_asset", "join", "modifier"),
    },
}

# The artist has additional elementary operations beyond the named skills.
# These are exposed separately, never silently converted into a made-up op.
def capability_catalog(*, existing_source=True):
    missing = {op for skill in ARCHITECTURAL_SKILLS.values()
               for op in skill["operations"] if op not in OPS}
    if missing:
        raise RuntimeError("SKILL_REFERENCES_UNSUPPORTED_BLENDER_OPS: "
                           + ",".join(sorted(missing)))
    if "beam_between_faces" not in OPS:
        raise RuntimeError("MISSING_GROUNDED_BEAM_DISPATCH")
    if "geometry" in OPS:
        raise RuntimeError("AMBIGUOUS_GEOMETRY_ALIAS_NOT_ALLOWED")
    skills = {}
    for name, skill in ARCHITECTURAL_SKILLS.items():
        ops = [op for op in skill["operations"]
               if existing_source or op not in
               {"extrude", "inset", "bevel", "sculpt", "sculpt_anchor", "boolean"}]
        if ops:
            skills[name] = {"purpose": skill["purpose"], "allowed_ops": ops}
    return {"schema_version": 1, "existing_source": bool(existing_source),
            "skills": skills, "atomic_blender_ops": sorted(OPS),
            "unsafe_arbitrary_python": False, "model_selects_values": True,
            "art_approved": False}


def artist_skill_prompt(*, existing_source=True, phase="full", category="architecture"):
    if category != "architecture" or phase != "full":
        return ""
    catalog = capability_catalog(existing_source=existing_source)
    return (
        "\nEXISTING REAL BLENDER CAPABILITIES (architectural design vocabulary, "
        "NOT procedural templates): " + json.dumps(catalog["skills"], sort_keys=True)
        + ". Each skill is a design goal. Choose the exact supported operation(s), "
        "actual inspected source object(s), parameters and composition yourself. "
        "Never use skill names or invented 'geometry' as executable op. "
        "Before choosing an architectural skill, identify the ACTUAL "
        "source material from genuine source-camera images. A wooden log or "
        "timber tower is NOT stone masonry: do not invent limestone buttresses "
        "or stone weathering if no stone exists. The owner STYLE reference is "
        "not the same as the inherited source's material. For timber fascia "
        "or roof braces choose your OWN elongated slender connected geometry "
        "rather than a floating cubic block. New objects use Blender WORLD "
        "metres and WORLD +Z is up; a local GLB axis is not WORLD XYZ. "
        "FIRST author exactly the structural action count required by the "
        "live schema. An op=sculpt with params.delta=[0,0,0] is not sculpting: "
        "it changes ZERO inherited vertices and is rejected by the actual "
        "Blender contract. Choose a NONZERO deliberate displacement with "
        "measured source-space support, or choose extrude, inset or bevel "
        "on a verified, real source face/edge instead. Never attach a second "
        "operation that relies on a topology edit before reinspection. "
        "Do not repack the protected inherited source UV atlas with a generic "
        "smart UV action. Inspect the resulting mesh and camera after each "
        "structural stage; do not mistake changed pixels for art quality."
    )


def source_contact_facts(objects, limit_per_mesh=12):
    """Provide genuine WORLD ray-hit anchors across LOWER/MIDDLE/ROOF bands.

    Select up to four strongest measured points per height band, preserving
    useful roof anchors and bounded Qwen context. No inferred coordinates.
    Qwen remains free to choose its own architecture and proportions.
    """
    import math
    result=[]
    for item in objects:
        groups={"roof":[],"middle":[],"lower":[]}
        for hit in item.get("camera_visible_face_landmarks",[])[:24]:
            xyz=hit.get("world_center_m")
            screen=hit.get("camera_screen_uv")
            height=hit.get("world_height_fraction")
            if hit.get("camera_visible") is not True or hit.get("camera_ray_hit_original_face") is not True:
                continue
            if type(hit.get("face_index")) is not int:
                continue
            if not (isinstance(xyz,list) and len(xyz)==3
                    and all(type(v) in (int,float) and math.isfinite(v) for v in xyz)):
                continue
            if not (isinstance(screen,list) and len(screen)==2
                    and all(type(v) in (int,float) and math.isfinite(v) for v in screen)):
                continue
            if type(height) not in (int,float) or not math.isfinite(height):
                continue
            zone="roof" if height>=0.67 else ("middle" if height>=0.35 else "lower")
            groups[zone].append({"id":hit["face_index"],"screen_uv":screen,
                                 "world_xyz_m":xyz,"height":height})
        # Present physically elevated camera rays early, without omitting
        # wall/foundation anchors. Remaining hits never become fake guesses.
        hits=[]
        each=max(1,limit_per_mesh//3)
        for zone in ("roof","middle","lower"):
            hits.extend(groups[zone][:each])
        if hits:
            result.append({"name":item["name"],"faces":hits[:limit_per_mesh]})
    return result


def strict_preflight_plan(plan):
    """Cheap source-side validation BEFORE launching Blender.
    
    Blender still performs its own authoritative topology/material checks.
    No repair of creative parameters or missing required keys is performed.
    """
    validate_plan(plan)
    invalid = [a["op"] for a in plan["actions"] if a["op"] not in OPS]
    if invalid:
        raise RejectedPlan("UNSUPPORTED_EXECUTOR_OP: " + ",".join(invalid))
    return {"valid": True, "action_count": len(plan["actions"]),
            "op_families": [a["op"] for a in plan["actions"]],
            "art_approved": False}


def check_source_environment(repo_root):
    """Cheap deterministic preflight; no Qwen, GPU, or Blender startup."""
    root = Path(repo_root)
    deps = {
        "contract": root / "tools/art-direction/full-studio/contract.py",
        "toolbox": root / "tools/art-direction/full-studio/blender_toolbox.py",
        "loop": root / "tools/art-direction/full-studio/artist_loop.py",
    }
    absent = [name for name, path in deps.items() if not path.is_file()]
    if absent:
        raise FileNotFoundError("STUDIO_MODULES_MISSING: " + ",".join(absent))
    src = deps["toolbox"].read_text(encoding="utf-8")
    if "def run_action(" not in src or "execute_via_blendermcp(" not in src:
        raise RuntimeError("STUDIO_DISPATCH_ENTRYPOINTS_MISSING")
    # Proves the known dispatch routes are present, not that Blender ran.
    known = {
        "primitive", "mesh", "beam_between_faces", "duplicate", "transform", "sculpt", "sculpt_anchor",
        "extrude", "inset", "subdivide", "bevel", "modifier", "boolean",
        "material", "texture", "uv", "join", "delete", "light", "camera",
        "render_settings", "native_operator", "shader_graph", "geometry_nodes",
        "asset_info", "import_asset",
    }
    if OPS != known:
        raise RuntimeError("OPS_CONTRACT_DRIFT: " + str(sorted(OPS ^ known)))
    return {"preflight": "STATIC_PASS", "ops_count": len(OPS),
            "supports_direct": True, "supports_blendermcp": True,
            "blender_executed": False, "art_approved": False}


def visual_intent_payload(*, original_reference, before_png, after_png, plan):
    """Cryptographically bind real image inputs to the model-authored intention.

    A consumer must pass the actual image bytes to Qwen-VL. Hashes never
    substitute for visual perception or camera-matching evidence.
    """
    strict_preflight_plan(plan)
    from PIL import Image
    images = {}
    for role, path in (("reference", original_reference),
                       ("before", before_png), ("after", after_png)):
        p = Path(path)
        if not p.is_file():
            raise FileNotFoundError("GENUINE_" + role.upper() + "_IMAGE_MISSING")
        content = p.read_bytes()
        if content[:8] != bytes.fromhex("89504e470d0a1a0a") and content[:3] != bytes.fromhex("ffd8ff"):
            raise ValueError("NOT_A_REAL_PNG_OR_JPEG: " + role)
        with Image.open(p) as decoded:
            size = list(decoded.size)
            decoded.verify()
        images[role] = {"sha256": hashlib.sha256(content).hexdigest(),
                        "path": str(p), "bytes": len(content), "size": size}
    if images["before"]["size"] != images["after"]["size"]:
        raise ValueError("BEFORE_AFTER_RESOLUTION_MISMATCH")
    if images["before"]["sha256"] == images["after"]["sha256"]:
        raise ValueError("BEFORE_AFTER_IMAGES_IDENTICAL")
    return {"images": images, "intent": {
                "design_intent": plan["design_intent"],
                "expected_visual_difference": plan["expected_visual_difference"],
                "operations": [a["op"] for a in plan["actions"]],
                "reasons": [a["reason"] for a in plan["actions"]]},
            "source": "REAL_IMAGE_BYTES", "independent_art_approval": False}


def is_semantically_reviewable(critique):
    """Require alignment with the authored intention before human art review.

    Returns a HOLD decision, not an independent aesthetic certification.
    Legacy reviews without these fields fail closed if this gate is enabled.
    """
    if not isinstance(critique, dict):
        return False
    return (critique.get("intent_alignment") == "MATCH"
            and critique.get("clearly_failed") is False
            and bool(str(critique.get("visible_change_observed", "")).strip())
            and bool(str(critique.get("plan_vs_geometry_reason", "")).strip()))


def failure_category(error):
    """Failure attribution; never count technical failures as artistic rejects."""
    s = str(error).lower()
    if any(t in s for t in ("importerror", "modulenotfound", "syntaxerror",
                             "jsondecodeerror", "source modules missing")):
        return "INTEGRATION"
    if any(t in s for t in ("unsupported blender operation", "unknown operator",
                             "schema", "invalid json", "unsupported_executor_op",
                             "rejected_plan", "unsupported blender op",
                             "invalid ai sculpt brush", "zero delta",
                             "zero displacement", "zero sculpt")):
        return "SCHEMA_OR_PLAN"
    if any(t in s for t in ("not_visible", "screen", "invisible", "0.031%",
                             "below pixel")):
        return "VISIBILITY"
    if any(t in s for t in ("protected", "overlap", "buried", "world bounds",
                             "source contact")):
        return "SPATIAL_SAFETY"
    if any(t in s for t in ("art_rejected", "clearly_failed", "intent_mismatch",
                             "aesthetic")):
        return "ART_QUALITY"
    if any(t in s for t in ("socket", "transport", "blender failed",
                             "server unavailable")):
        return "EXECUTION"
    return "UNCLASSIFIED"
