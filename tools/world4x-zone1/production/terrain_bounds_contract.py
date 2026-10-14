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


RADIUS_CHOICES = ("18","20","22","24","26","28","30")


def model_knot_schema(limits):
    """Artist chooses actual XY, displacement and one exact tool radius.

    Z is NOT an art parameter: physical contact height is independently
    measured from the real frozen Blender terrain at the chosen XY.
    """
    xy = {
        axis.lower(): {"type":"number","minimum":limits[index][0],
                       "maximum":limits[index][1]}
        for index,axis in enumerate(AXES[:2])
    }
    return {
        "type":"object", "additionalProperties":False,
        "properties":{
            "center_xy":{"type":"object","additionalProperties":False,
                         "properties":xy,"required":["x","y"]},
            "vertical_displacement_m":{"type":"number","minimum":-14,"maximum":12},
            "radius_m_choice":{"type":"string","enum":list(RADIUS_CHOICES)},
        },
        "required":["center_xy","vertical_displacement_m","radius_m_choice"]
    }


def artist_xy_positions(raw_document, xy_limits):
    if not isinstance(raw_document,dict) or not isinstance(raw_document.get("planned_actions"),list):
        raise ValueError("Original model document lacks actions")
    positions=[]
    for action in raw_document["planned_actions"]:
        knots=action.get("parameters",{}).get("knots") if isinstance(action,dict) else None
        if not isinstance(knots,list) or not 3<=len(knots)<=4:
            raise ValueError("Expected 3..4 model-authored knots per path")
        for knot in knots:
            if not isinstance(knot,dict) or set(knot)!={"center_xy","vertical_displacement_m","radius_m_choice"}:
                raise ValueError("Malformed original model knot fields")
            pos=knot["center_xy"]
            if not isinstance(pos,dict) or set(pos)!={"x","y"}:
                raise ValueError("Missing exact model-authored x/y")
            x,y=pos["x"],pos["y"]
            if not all(type(q) in (int,float) and math.isfinite(q) for q in (x,y)):
                raise ValueError("Artist supplied unsafe XY")
            if not(xy_limits[0][0]<=x<=xy_limits[0][1] and xy_limits[1][0]<=y<=xy_limits[1][1]):
                raise ValueError("AI terrain XY lies outside source-authorized region")
            if knot["radius_m_choice"] not in RADIUS_CHOICES:
                raise ValueError("Invalid artist-chosen radius enum; NEVER coerce/clamp")
            if type(knot["vertical_displacement_m"]) not in (int,float) or not math.isfinite(knot["vertical_displacement_m"]):
                raise ValueError("Invalid model-chosen signed relief")
            positions.append([x,y])
    if not 1<=len(positions)<=8:
        raise ValueError("Unexpected number of original Qwen XY knots")
    return positions


def normalize_artist_authored_knots(raw_document, source_heights):
    """Preserve every Qwen-authored value. Convert only data representation.

    source_heights are measured, not invented, from the immutable source
    terrain using inspect_ground_heights.py at each Qwen-selected XY.
    """
    import copy
    doc=copy.deepcopy(raw_document)
    actions=doc.get("planned_actions") if isinstance(doc,dict) else None
    if not isinstance(actions,list) or not isinstance(source_heights,list):
        raise ValueError("Missing model actions or genuine Blender source heights")
    index=0
    for action in actions:
        knots=action.get("parameters",{}).get("knots") if isinstance(action,dict) else None
        if not isinstance(knots,list):
            raise ValueError("Original model path has no knots")
        for knot in knots:
            if index>=len(source_heights):
                raise ValueError("Insufficient measured original terrain heights")
            pos=knot.pop("center_xy")
            dz=knot.pop("vertical_displacement_m")
            radius_choice=knot.pop("radius_m_choice")
            height=source_heights[index]
            if type(height) not in (float,int) or not math.isfinite(height):
                raise ValueError("Blender returned non-finite terrain surface height")
            if radius_choice not in RADIUS_CHOICES:
                raise ValueError("Model-selected radius not an approved tool value")
            knot["center"]=[pos["x"],pos["y"],height]
            knot["delta"]=[0,0,dz]
            knot["radius"]=float(radius_choice)
            index+=1
    if index!=len(source_heights):
        raise ValueError("Source height count does not match model knot count")
    return doc
