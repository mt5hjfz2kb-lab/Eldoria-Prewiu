"""Source-grounded, noncreative coordinate limits for Zone One Qwen plans.

This module NEVER designs a terrain shape, invents knot positions, clamps
a model output, or approves art. It merely constrains the allowed JSON
coordinate ranges to the *inspected* source mesh local AABB before inference.
"""
import math

DEFAULT_ALLOWED = ((-72.0, 72.0), (32.0, 86.0), (-5.0, 7.0))
AXES = ("X", "Y", "Z")


def verified_knot_ranges(mesh_context, allowed=DEFAULT_ALLOWED, margin=0.01):
    aabb = mesh_context.get("local_aabb") if isinstance(mesh_context, dict) else None
    if not isinstance(aabb, dict):
        raise ValueError("Source mesh local_aabb missing; cannot direct safe terrain sculpt")
    low, high = aabb.get("min"), aabb.get("max")
    if (not isinstance(low, list) or not isinstance(high, list)
            or len(low) != 3 or len(high) != 3):
        raise ValueError("Source local mesh AABB must contain three minimum/maximum coordinates")
    out = []
    for index, (request_low, request_high) in enumerate(allowed):
        source_low, source_high = low[index], high[index]
        if (not all(type(v) in (float, int) and math.isfinite(v)
                    for v in (source_low, source_high, request_low, request_high))
                or source_low >= source_high):
            raise ValueError(f"Invalid source {AXES[index]}-axis local AABB")
        lower = max(float(request_low), float(source_low) + margin)
        upper = min(float(request_high), float(source_high) - margin)
        if lower >= upper:
            raise ValueError(
                f"Safe intended terrain area does not overlap real source {AXES[index]} bounds")
        out.append((round(lower, 5), round(upper, 5)))
    return tuple(out)


def coordinate_schema(limits):
    return {"type": "array", "minItems": 3, "maxItems": 3,
            "prefixItems": [
                {"type": "number", "minimum": low, "maximum": high}
                for low, high in limits]}


def limits_label(limits):
    return " ".join(
        f"{axis}={bounds[0]:g}..{bounds[1]:g}"
        for axis, bounds in zip(AXES, limits))


def inside_limits(center, limits):
    return (isinstance(center, list) and len(center) == 3 and
            all(type(v) in (int, float) and math.isfinite(v) and lo <= v <= hi
                for v, (lo, hi) in zip(center, limits)))


def model_knot_schema(limits):
    """Use named/scalar coordinates because small local model JSON constraints may
    not honor prefixItems on different numeric dimensions of a vector.
    The model authors x/y/z and vertical change; conversion is type-only.
    """
    coords = {
        axis.lower(): {"type":"number","minimum":bounds[0],"maximum":bounds[1]}
        for axis, bounds in zip(AXES,limits)
    }
    return {
        "type":"object", "additionalProperties":False,
        "properties":{
            "center_xyz":{"type":"object","additionalProperties":False,
                          "properties":coords,"required":["x","y","z"]},
            "vertical_displacement_m":{"type":"number","minimum":-14,"maximum":12},
            "radius":{"type":"number","minimum":18,"maximum":30},
        },
        "required":["center_xyz","vertical_displacement_m","radius"]
    }


def normalize_artist_authored_knots(raw_document):
    """Lossless typed serialization of LLM-selected spatial numbers.

    Never clamp, repair, snap to mesh, interpolate or change any AI-authored
    coordinate or displacement; reject malformed fields before art execution.
    The raw response remains the authoritative original inference evidence.
    """
    import copy
    doc=copy.deepcopy(raw_document)
    actions=doc.get("planned_actions") if isinstance(doc,dict) else None
    if not isinstance(actions,list):
        raise ValueError("No artist planned_actions array")
    for action in actions:
        knots=action.get("parameters",{}).get("knots") if isinstance(action,dict) else None
        if not isinstance(knots,list):
            raise ValueError("Artist path has no knots")
        for knot in knots:
            if not isinstance(knot,dict) or set(knot)!={"center_xyz","vertical_displacement_m","radius"}:
                raise ValueError("Artist knot requires center_xyz, vertical_displacement_m and radius")
            pos=knot.pop("center_xyz")
            dz=knot.pop("vertical_displacement_m")
            if not isinstance(pos,dict) or set(pos)!={"x","y","z"}:
                raise ValueError("Artist named source-local center must contain exactly x,y,z")
            if not all(type(v) in (int,float) and math.isfinite(v) for v in (pos["x"],pos["y"],pos["z"],dz)):
                raise ValueError("Artist knot contains invalid numeric components")
            knot["center"]=[pos["x"],pos["y"],pos["z"]]
            knot["delta"]=[0,0,dz]
    return doc
