"""Real-size Blender mesh brush limits; bounds are safety, NOT artistic decisions.

Measured WORLD bounding box is a factual input; Qwen still selects every
anchor, direction, radius and distance. No scaling/repair of its authored values.
"""
from __future__ import annotations
import math


def bounds_for_dimensions(dimensions):
    if (not isinstance(dimensions, (list,tuple)) or len(dimensions)!=3
            or any(type(x) not in (int,float) or not math.isfinite(x) or x<=0 for x in dimensions)):
        raise ValueError("Actual positive finite source dimensions required")
    span=max(float(v) for v in dimensions)
    if span<0.2 or span>1000:
        raise ValueError("Implausible source object world dimensions")
    distance_min=round(max(0.01,span*0.012),4)
    distance_max=round(min(0.48,span*0.085),4)
    radius_min=round(max(0.1,span*0.07),4)
    radius_max=round(min(2.4,span*0.35),4)
    return {"distance":{"minimum":distance_min,"maximum":max(distance_min,distance_max)},
            "radius":{"minimum":radius_min,"maximum":max(radius_min,radius_max)},
            "world_max_dimension_m":span}


def source_brush_bounds(obj):
    dimensions=obj.get("world_bounds_dimensions_m") or obj.get("world_dimensions_m")
    if dimensions is None:
        bounds_lo=obj.get("world_bounds_min_m")
        bounds_hi=obj.get("world_bounds_max_m")
        if (isinstance(bounds_lo,list) and isinstance(bounds_hi,list)
                and len(bounds_lo)==3 and len(bounds_hi)==3):
            dimensions=[bounds_hi[i]-bounds_lo[i] for i in range(3)]
    if dimensions is None:
        return None
    return bounds_for_dimensions(dimensions)


def conservative_schema_bounds(inventory):
    limits=[source_brush_bounds(obj) for obj in (inventory or [])]
    limits=[x for x in limits if x]
    if not limits:
        return None
    return {"distance":{"minimum":min(x["distance"]["minimum"] for x in limits),
                        "maximum":min(x["distance"]["maximum"] for x in limits)},
            "radius":{"minimum":min(x["radius"]["minimum"] for x in limits),
                      "maximum":min(x["radius"]["maximum"] for x in limits)}}


def validate_source_anchor_brush(action,source):
    limits=source_brush_bounds(source)
    if limits is None:
        return  # Only genuine inspected world dimensions authorize size-aware refusal.
    params=action["params"]
    for key in ("distance","radius"):
        val=params.get(key)
        bound=limits[key]
        if (type(val) not in (float,int) or not math.isfinite(val)
                or not bound["minimum"]<=val<=bound["maximum"]):
            raise ValueError(
                f"SCULPT_SCALE_REJECTED: {key}={val!r} not in "
                f"[{bound['minimum']},{bound['maximum']}] metres for "
                f"measured world size {source['world_dimensions_m']}; "
                "Qwen must choose a new LOCAL brush, never auto-clamp.")
