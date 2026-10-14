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


RADIUS_CHOICES = ("18", "20", "22", "24", "26", "28", "30")


def model_knot_schema(limits):
    """The model DIRECTLY chooses XY, vertical displacement and radius.

    Base terrain Z is not an artistic choice: derive it read-only from the
    exact original Blender mesh at the artist-picked source-local XY.
    Previous Qwen plans wrongly copied their -14m sculpt depth into center Z.
    """
    axes={
        axis.lower(): {"type":"number","minimum":bounds[0],"maximum":bounds[1]}
        for axis,bounds in zip(AXES[:2],limits[:2])
    }
    return {
        "type":"object","additionalProperties":False,
        "properties":{
            "center_xy":{"type":"object","additionalProperties":False,
                         "properties":axes,"required":["x","y"]},
            "vertical_displacement_m":{"type":"number","minimum":-14,"maximum":12},
            "radius_m_choice":{"type":"string","enum":list(RADIUS_CHOICES)},
        },
        "required":["center_xy","vertical_displacement_m","radius_m_choice"]
    }


def artist_xy_positions(original_plan,limits):
    """Validate literal AI XY selections BEFORE requesting real Blender Z.

    Does not snap, interpolate, clamp, invent or repair any artist choice.
    """
    if not isinstance(original_plan,dict):
        raise ValueError("Original AI plan must be a JSON object")
    actions=original_plan.get("planned_actions")
    if not isinstance(actions,list) or len(actions)!=2:
        raise ValueError("Exactly two original Qwen sculpt paths required")
    positions=[]
    for act in actions:
        knots=act.get("parameters",{}).get("knots") if isinstance(act,dict) else None
        if not isinstance(knots,list) or not 3<=len(knots)<=4:
            raise ValueError("Each original sculpt path needs 3..4 artist-chosen knots")
        for knot in knots:
            if not isinstance(knot,dict) or set(knot)!={"center_xy","vertical_displacement_m","radius_m_choice"}:
                raise ValueError("AI knot fields must be center_xy, vertical_displacement_m, radius_m_choice")
            xy=knot.get("center_xy")
            if not isinstance(xy,dict) or set(xy)!={"x","y"}:
                raise ValueError("Original center_xy must contain only x and y")
            x,y=xy["x"],xy["y"]
            if not all(type(v) in (float,int) and math.isfinite(v) for v in (x,y)):
                raise ValueError("Invalid original artist XY values")
            if not(limits[0][0]<=x<=limits[0][1] and limits[1][0]<=y<=limits[1][1]):
                raise ValueError("Original XY exceeds real source-terrain safe limits")
            dz=knot["vertical_displacement_m"]
            if type(dz) not in (float,int) or not math.isfinite(dz) or not -14<=dz<=12:
                raise ValueError("Invalid artist-chosen vertical displacement")
            if knot["radius_m_choice"] not in RADIUS_CHOICES:
                raise ValueError("Invalid artist-chosen radius from allowed source-safe values")
            positions.append([x,y])
    return positions


def validate_artist_path_curvature(original_plan, limits, minimum_double_area=130.0):
    """Reject geometrically degenerate original AI paths *before* starting Blender.

    This is a pure QA gate. It never edits, interpolates, straightens or authors
    any AI-selected XY values; a rejected plan must go back to Qwen.
    The double signed triangle area equals the two-dimensional cross product.
    """
    artist_xy_positions(original_plan, limits)
    errors = []
    for index, action in enumerate(original_plan["planned_actions"], 1):
        xy = [(k["center_xy"]["x"], k["center_xy"]["y"])
              for k in action["parameters"]["knots"]]
        if len(set(xy)) < 3:
            errors.append(f"Path {index}: needs at least three DISTINCT XY knots")
        cross = max(
            abs((xy[i+1][0]-xy[i][0]) * (xy[i+2][1]-xy[i][1]) -
                (xy[i+1][1]-xy[i][1]) * (xy[i+2][0]-xy[i][0]))
            for i in range(len(xy)-2)
        )
        if cross < minimum_double_area:
            errors.append(
                f"Path {index}: original XY knots form an insufficient bend "
                f"(cross-product area={cross:g} m2, minimum={minimum_double_area:g} m2); "
                "choose NEW original non-collinear XY coordinates"
            )
    return errors


def normalize_artist_authored_knots(raw_document,source_heights):
    """Type-only mapping of model-picked XY/delta/radius plus ACTUAL source Z.

    Source elevations must already have been measured independently in
    Blender, tied to an immutable source SHA and the model's exact XY values.
    No artistic decisions are repaired or generated by this function.
    """
    import copy
    doc=copy.deepcopy(raw_document)
    actions=doc.get("planned_actions") if isinstance(doc,dict) else None
    if not isinstance(actions,list):
        raise ValueError("Original planned_actions missing")
    if not isinstance(source_heights,list):
        raise ValueError("Read-only original source Z measurements required")
    n=sum(len(a.get("parameters",{}).get("knots",[])) for a in actions if isinstance(a,dict))
    if len(source_heights)!=n:
        raise ValueError("Source Z sample count differs from original AI knot count")
    index=0
    for action in actions:
        knots=action.get("parameters",{}).get("knots") if isinstance(action,dict) else None
        if not isinstance(knots,list):
            raise ValueError("Artist path knot array missing")
        for knot in knots:
            if not isinstance(knot,dict) or set(knot)!={"center_xy","vertical_displacement_m","radius_m_choice"}:
                raise ValueError("Artist knot structure inconsistent with typed model schema")
            xy=knot.pop("center_xy")
            dz=knot.pop("vertical_displacement_m")
            radius=knot.pop("radius_m_choice")
            z=source_heights[index]
            index+=1
            if not isinstance(xy,dict) or set(xy)!={"x","y"} or radius not in RADIUS_CHOICES:
                raise ValueError("Malformed original model XY or radius")
            if not all(type(v) in (int,float) and math.isfinite(v) for v in (xy["x"],xy["y"],dz,z)):
                raise ValueError("Original model coordinates or verified source Z invalid")
            knot["center"]=[xy["x"],xy["y"],z]
            knot["delta"]=[0,0,dz]
            knot["radius"]=int(radius)
    return doc
