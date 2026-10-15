"""Independent physical gates for Qwen-authored multi-part architectural artwork.

Allows the model to choose MULTIPLE original Blender primitives/custom meshes
forming a compositional assembly, instead of forcing one isolated box.
No geometry or placement is authored by this validator. Contact is WORLD XYZ
AABB preflight, NOT independent final physical/artistic certification.
"""
import json
import re
from contract import RejectedPlan
from architectural_component_guard import verify_volume_and_complexity
from architectural_world_placement_gate import reject_obviously_bad_component_placement

def verify_qwen_composition(doc, source_inventory, cycle_index):
    actions=doc.get("actions",[])
    if not 2<=len(actions)<=4:
        raise RejectedPlan("Multi-part original Qwen composition requires 2..4 actions")
    namespace=rf"^Eldoria_Added_C{cycle_index:02d}_[A-Za-z][A-Za-z0-9_]{{3,35}}$"
    created=set();proofs=[]
    for i,act in enumerate(actions,1):
        kind=act.get("op"); name=act.get("target","")
        if kind in ("primitive","mesh"):
            if not re.fullmatch(namespace,name) or name in created:
                raise RejectedPlan(
                    f"Qwen component {i}: unique ORIGINAL C{cycle_index:02d} name required")
            single={"actions":[act]}
            verify_volume_and_complexity(single,source_inventory)
            proof=reject_obviously_bad_component_placement(single,source_inventory)
            created.add(name)
            proofs.append({"target":name,"op":kind,
                           "contact":proof,"world_source_unchanged":True})
        elif kind in ("material","uv","modifier"):
            if name not in created:
                raise RejectedPlan(
                    f"Qwen finish action {i}: target {name!r} is not an actual "
                    "earlier component in THIS composition; preserve original tower")
        else:
            raise RejectedPlan(
                "Multi-part architecture Qwen may only choose native new "
                "primitive/mesh plus safe finishing on newly created objects")
    if len(created)<2:
        raise RejectedPlan(
            "Single primitive is not an original multi-part timber assembly. "
            "Author at least two physically contacting distinct real components.")
    return {"status":"SOURCE_AABB_CONTACT_CANDIDATES_ONLY",
            "original_component_count":len(created),
            "components":proofs,"art_approved":False,"actual_surface_contact_verified":False,
            "authoring_values_modified":False}
