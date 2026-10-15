"""Read-only physical coherence gate for AI-authored additions to an existing scene.

Never moves, rotates or creates objects. A rejected Qwen plan must be reauthored
by Qwen using the measured world-space source evidence. This is a narrow
preflight for obviously implausible positions, not artistic self-approval.
"""
import math
from contract import RejectedPlan

def _aabb_from_original_art(action):
    params=action["params"]
    if action["op"]=="mesh":
        verts=params["vertices"]
        return [min(v[i] for v in verts) for i in range(3)],[
            max(v[i] for v in verts) for i in range(3)]
    if action["op"]!="primitive":
        raise RejectedPlan("Only ORIGINAL physical primitives/mesh can be placed")
    loc=params["location"];size=params["scale"];angles=params["rotation"]
    if len(loc)!=3 or len(size)!=3 or len(angles)!=3:
        raise RejectedPlan("Original world XYZ location, scale and rotation required")
    # Blender Euler XYZ: rotation matrix Rz @ Ry @ Rx; absolute transformed
    # unit-cube half-extents are a CONSERVATIVE bbox for cone/cylinder/sphere.
    cx,cy,cz=(math.cos(a) for a in angles)
    sx,sy,sz=(math.sin(a) for a in angles)
    rotation=[
       [cz*cy,cz*sy*sx-sz*cx,cz*sy*cx+sz*sx],
       [sz*cy,sz*sy*sx+cz*cx,sz*sy*cx-cz*sx],
       [-sy,cy*sx,cy*cx]]
    half=[sum(abs(rotation[i][j])*size[j] for j in range(3)) for i in range(3)]
    return [loc[i]-half[i] for i in range(3)],[loc[i]+half[i] for i in range(3)]

def reject_obviously_bad_component_placement(plan,source_objects):
    if not plan.get("actions") or not source_objects:
        raise RejectedPlan("Original authored component + real source inspection required")
    action=plan["actions"][0]
    if action["op"] not in ("mesh","primitive"):
        raise RejectedPlan("New component must have original Blender geometry")
    original=max(source_objects,key=lambda o:o.get("evaluated_triangles",o.get("faces",0)))
    low=original.get("world_bounds_min_m");high=original.get("world_bounds_max_m")
    if not low or not high or any(high[i]<=low[i] for i in range(3)):
        raise RejectedPlan("Physical Blender WORLD source bounds required before Qwen edits")
    lo,hi=_aabb_from_original_art(action)
    span=[high[i]-low[i] for i in range(3)]
    gap=[max(0,low[i]-hi[i],lo[i]-high[i]) for i in range(3)]
    # The historical permissive 0.5--1.14 metre gap incorrectly accepted a
    # cube FLOATING BESIDE the 2.65m wooden watchtower (run 38652707944).
    # Never describe a loose object as an attached support. Use a bounded
    # world-space contact tolerance for real modelling/raycast inaccuracies;
    # this does NOT create or move authored geometry. Actual surface contact
    # still requires independent review.
    tolerance=min(.06,max(.015,min(span)*.02))
    if any(g>tolerance for g in gap):
        raise RejectedPlan(
            "NO_SOURCE_CONTACT_AABB: New artist-made structure is detached "
            "from the existing real Valoria tower WORLD bounds; "
            "axis_gaps_m="+str([round(g,4) for g in gap])+
            ", allowed_tolerance_m="+str(round(tolerance,4))+
            ". It must physically overlap or nearly touch the source on "
            "ALL WORLD X/Y/Z axes; redesign original contact/geometry, "
            "do not automatically translate the AI piece.")
    center_z=(lo[2]+hi[2])*.5
    name=action.get("target","").lower()
    # A roof EAVE / FASCIA / OVERHANG is attached to an outer roof edge,
    # not a tiny disc floating at the central apex. Real source bounds (not
    # an invented shape template) establish which placements are impossible.
    if any(t in name for t in ("eave","overhang","fascia","roofbracket","roof_bracket")):
        margin_xy=[max(.10,span[i]*.12) for i in (0,1)]
        touches_perimeter=any(
            lo[i]<=low[i]+margin_xy[i] or hi[i]>=high[i]-margin_xy[i]
            for i in (0,1))
        near_apex=lo[2]>=high[2]-span[2]*.075
        if not touches_perimeter or near_apex:
            raise RejectedPlan(
                "Roof EAVE/OVERHANG geometric attachment rejected: a roof-edge "
                "element must reach an actual existing WORLD X/Y exterior roof "
                "edge and overlap the roof below its highest WORLD Z apex. "
                "This proposal is centered at or above the apex rather than "
                "attached to a timber eave. Choose an original credible "
                "edge-attached design from verified bounds and views; "
                "do not repeat the central roof-top cylinder.")
    if any(t in name for t in ("chimney","spire","column","pillar","flue")):
        dim=[hi[i]-lo[i] for i in range(3)]
        if dim[2]<max(dim[0],dim[1])*1.1 or center_z<low[2]+.5*span[2]:
            raise RejectedPlan("Vertical named architecture is incorrectly oriented in WORLD Y "
                               "or too low: Blender WORLD +Z is vertical, GLB local Y is not. "
                               "Qwen must author a genuinely upright placed element")
    # True exterior-facing architecture has to touch or reach an observable
    # original building skin, not be a tiny cube buried at the mesh center.
    # This pure geometric AABB bound uses measured Blender WORLD vertices and
    # leaves every model-authored position and dimension completely unchanged.
    boundary_margin=[max(.12,span[i]*.10) for i in range(3)]
    reaches_exterior=any(
        lo[i]<=low[i]+boundary_margin[i] or hi[i]>=high[i]-boundary_margin[i]
        for i in range(3)
    )
    if not reaches_exterior:
        raise RejectedPlan("NEW ARCHITECTURE IS COMPLETELY BURIED INSIDE source WORLD bounds; "
                           "cannot be camera-visible structural addition. Rechoose YOUR "
                           "OWN source-attached exterior location and dimensions, no automatic repositioning.")
    # A physically attached roof/side extension must not stretch far beyond
    # the whole inherited building. The earlier accepted C02 huge Y-axis
    # block projected >1.3x the original footprint past the gable.
    # This geometric plausibility check DOES NOT propose new model values.
    overhang=[max(0,low[i]-lo[i],hi[i]-high[i]) for i in range(3)]
    if any(overhang[i]>max(.8,span[i]*.8) for i in (0,1)):
        raise RejectedPlan("New component OUTSIZES the original building horizontally "
                           "and projects unrealistically far from its source; "
                           "choose YOUR OWN smaller attached world XYZ position and dimensions")
    overlap=[max(0,min(hi[i],high[i])-max(lo[i],low[i])) for i in range(3)]
    original_area=span[0]*span[1]
    ground_intersection=overlap[0]*overlap[1]/original_area
    center_z=(lo[2]+hi[2])*.5
    center_xy=[(lo[i]+hi[i])*.5 for i in (0,1)]
    centralized=all(abs(center_xy[i]-(low[i]+high[i])*.5)<span[i]*.3 for i in (0,1))
    if (center_z<low[2]+.45*span[2] and (ground_intersection>.35 or
                                      (centralized and ground_intersection>.12))):
        raise RejectedPlan("New component BLOCKS too much of existing low-level building "
                           "footprint, door/stairs/circulation at WORLD Z floor. "
                           "Qwen must choose a different ORIGINAL attachment/placement")
    return {"source_world_bounds":[low,high],"new_world_bounds":[lo,hi],
            "bbox_contact_axis_gaps_m":[round(g,4) for g in gap],
            "bbox_contact_tolerance_m":round(tolerance,4),
            "ground_footprint_overlap_fraction":round(ground_intersection,4),
            "changes_to_artist_values":0,"art_approved":False}
