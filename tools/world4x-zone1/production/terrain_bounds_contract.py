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
    """Qwen chooses original XY path, radius and Z *displacement*.

    In the 2026-10-14 three-attempt failure Qwen confused center elevation
    with -14m/+12m displacement. Never ask the model to invent source Z.
    That noncreative topological lookup comes from the original .blend.
    """
    coords={
        axis.lower():{"type":"number","minimum":bounds[0],"maximum":bounds[1]}
        for axis,bounds in zip(AXES[:2],limits[:2])
    }
    return {
        "type":"object", "additionalProperties":False,
        "properties":{
            "center_xy":{"type":"object","additionalProperties":False,
                         "properties":coords,"required":["x","y"]},
            "vertical_displacement_m":{"type":"number","minimum":-14,"maximum":12},
            "radius":{"type":"number","minimum":18,"maximum":30},
        },
        "required":["center_xy","vertical_displacement_m","radius"]
    }


def actual_surface_height(surface_context,x,y,max_distance_m=3.0):
    """Find actual frozen Blender source elevation at model-picked XY.

    No artistic value is generated or changed. We choose the nearest genuine
    pre-existing source vertex ONLY to anchor the path to its physical terrain,
    rejecting targets far from all original vertices. This is a sampled
    source lookup, not synthetic terrain height.
    """
    if not isinstance(surface_context,dict) or not surface_context.get("source_unchanged"):
        raise ValueError("Read-only source height provenance missing")
    vertices=surface_context.get("vertex_positions")
    if not isinstance(vertices,list) or not vertices:
        raise ValueError("Original Blender terrain height inventory missing")
    if not all(type(v) in (int,float) and math.isfinite(v) for v in (x,y)):
        raise ValueError("Artist XY coordinates must be finite numeric values")
    nearest=None
    for vertex in vertices:
        if not isinstance(vertex,list) or len(vertex)!=4 or type(vertex[0]) is not int:
            raise ValueError("Source terrain inventory has malformed vertices")
        _,vx,vy,vz=vertex
        if not all(type(v) in (int,float) and math.isfinite(v) for v in (vx,vy,vz)):
            raise ValueError("Source terrain inventory has invalid coordinates")
        distance_sq=(vx-x)**2+(vy-y)**2
        if nearest is None or distance_sq<nearest[0] or (distance_sq==nearest[0] and vertex[0]<nearest[1]):
            nearest=(distance_sq,vertex[0],vz)
    if nearest is None or nearest[0] > max_distance_m**2:
        raise ValueError("Artist XY knot has no nearby physical source terrain vertices")
    return nearest[2]


def normalize_artist_authored_knots(raw_document,surface_context):
    """Keep Qwen's XY and signed displacement EXACT.

    Only compute base terrain Z from an immutable, hashed read-only Blender
    inventory. Raw Qwen JSON is preserved separately; no mesh is authored,
    no coordinates are clamped, and invalid plans are still rejected.
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
            if not isinstance(knot,dict) or set(knot)!={"center_xy","vertical_displacement_m","radius"}:
                raise ValueError("Artist knot needs ONLY center_xy, vertical_displacement_m and radius")
            pos=knot.pop("center_xy")
            dz=knot.pop("vertical_displacement_m")
            if not isinstance(pos,dict) or set(pos)!={"x","y"}:
                raise ValueError("Artist horizontal center must contain exactly source-local x,y")
            if not all(type(v) in (int,float) and math.isfinite(v) for v in (pos["x"],pos["y"],dz)):
                raise ValueError("Artist plan has invalid numeric spatial components")
            original_z=actual_surface_height(surface_context,pos["x"],pos["y"])
            knot["center"]=[pos["x"],pos["y"],original_z]
            knot["delta"]=[0,0,dz]
    return doc
