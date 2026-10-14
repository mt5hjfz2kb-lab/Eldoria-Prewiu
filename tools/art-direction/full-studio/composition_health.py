"""Cheap geometry sanity gates for AI-authored, NEW game-asset silhouettes.

This never creates/changes Blender geometry and never judges art quality.
Its job is to stop genuine Qwen mistakes (buried roofs, identical overlapping
cubes, impossible scales, invisible 3D camera silhouettes) before paying for a
full Blender render. Rejected plans go back to Qwen for reauthoring.

The current source of these guardrails is the real Qwen run 38568103614:
four distinct named meshes were authored but all centers were at z=0.5 and
the roof was offset sideways; visual review was below source-art quality.
"""
from __future__ import annotations
import math

class CompositionRejected(ValueError):
    pass

def reject(errors):
    if errors:raise CompositionRejected("; ".join(errors))

def bounds(action):
    p=action["params"]
    c=p.get("location",[0,0,0]);s=p.get("scale",[1,1,1])
    shape=p["shape"]
    # The supported Blender primitives are centered at the origin, and the
    # axis-aligned bounds here are conservative for rotated assets.
    radius=[abs(x) for x in s]
    return tuple(c[i]-radius[i] for i in range(3)),tuple(c[i]+radius[i] for i in range(3))

def intersection_fraction(a,b):
    al,ah=bounds(a);bl,bh=bounds(b)
    overlap=1.0
    volume_a=1.0;volume_b=1.0
    for i in range(3):
        overlap*=max(0.0,min(ah[i],bh[i])-max(al[i],bl[i]))
        volume_a*=max(0.0001,ah[i]-al[i])
        volume_b*=max(0.0001,bh[i]-bl[i])
    return overlap/min(volume_a,volume_b)

def check_new_silhouette(plan,asset_category="architecture"):
    actions=[a for a in plan.get("actions",[])
             if a.get("op")=="primitive"]
    errors=[]
    if len(actions)<2:
        errors.append("Create at least two distinct primary AI-designed shapes")
    if len(actions)>12:
        errors.append("Stage the new asset in 12 or fewer actual mesh operations")
    if len({a.get("target") for a in actions})!=len(actions):
        errors.append("Blender objects must have different names")
    if len({a.get("params",{}).get("shape") for a in actions})<2 and asset_category=="architecture":
        errors.append("Different architectural parts need distinct mesh shapes")
    positions=[a["params"].get("location",[0,0,0]) for a in actions]
    if positions and max(p[2] for p in positions)-min(p[2] for p in positions)<0.5 and asset_category=="architecture":
        errors.append("Building pieces all share the same height; roof/wall/trim must occupy actual different vertical tiers")
    shapes=[a["params"].get("shape") for a in actions]
    for a in actions:
        name=a["target"].lower()
        lo,hi=bounds(a)
        if asset_category in ("architecture","prop"):
            if lo[2]<-0.45:
                errors.append(a["target"]+" is substantially buried below the ground plane; adjust its own Z placement")
            if hi[2]>45:
                errors.append(a["target"]+" exceeds a plausible reusable mobile asset bounding height")
        if a["params"].get("shape") in ("cylinder","cone") and asset_category=="architecture":
            scale=a["params"]["scale"]
            if min(scale[0],scale[1])/max(scale[0],scale[1])<0.37:
                errors.append(a["target"]+" has a severely crushed circular cross-section; adjust the artistic mass")
    for i in range(len(actions)):
        for j in range(i+1,len(actions)):
            if intersection_fraction(actions[i],actions[j])>=.90:
                errors.append(actions[i]["target"]+" and "+actions[j]["target"]+
                              " are nearly identical overlapping volumes; create meaningful distinct pieces")
    if asset_category=="architecture":
        roof=[a for a in actions if any(w in a["target"].lower() for w in ("roof","techo","cubierta","spire","chapitel"))]
        supports=[a for a in actions if a not in roof]
        if roof and supports:
            body_top=max(bounds(a)[1][2] for a in supports)
            highest_roof=max(bounds(a)[1][2] for a in roof)
            if highest_roof<body_top+0.15:
                errors.append("AI roof does not sit above the actual supporting architecture")
            for r in roof:
                rpos=r["params"]["location"]
                nearest=min(math.hypot(rpos[0]-a["params"]["location"][0],
                                       rpos[1]-a["params"]["location"][1]) for a in supports)
                rx,ry=r["params"]["scale"][:2]
                if nearest>max(rx,ry)*.55:
                    errors.append("AI roof is spatially detached from its supporting building")
    reject(errors)
    return {
      "schema_version":1,"scope":"TECHNICAL_AI_SILHOUETTE_PREFLIGHT_ONLY",
      "asset_category":asset_category,"new_object_count":len(actions),
      "distinct_mesh_shapes":len(set(shapes)),"art_approved":False,
      "no_template_geometry_inserted":True
    }
