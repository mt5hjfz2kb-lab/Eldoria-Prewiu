"""Neutral Blender WORLD-space bounding facts for AI artistic decisions.

All actual positioning is authored by the local model, never by this module.
Imported GLB mesh vertices may have local +Y aligned to global +Z.
"""
from math import isfinite

def summarize_world_points(points):
    values=list(points)
    if not values or any(len(p)!=3 or any(not isfinite(float(n)) for n in p) for p in values):
        raise ValueError("Finite original WORLD xyz mesh vertices are required")
    lo=[round(min(float(p[i]) for p in values),5) for i in range(3)]
    hi=[round(max(float(p[i]) for p in values),5) for i in range(3)]
    return {"world_bounds_min_m":lo,
            "world_bounds_max_m":hi,
            "world_bounds_center_m":[round((lo[i]+hi[i])*0.5,5) for i in range(3)],
            "world_bounds_dimensions_m":[round(hi[i]-lo[i],5) for i in range(3)],
            "world_up_axis":"+Z"}
