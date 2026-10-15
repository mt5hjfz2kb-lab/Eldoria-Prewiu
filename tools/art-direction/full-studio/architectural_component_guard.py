"""Noncreative physical-quality gate for FIRST new architectural component.

Reject flat placemats, tiny slivers, invented source-colliding IDs. The model
chooses every shape, position, dimension, face and material. No repairs here.
"""
import math
from contract import RejectedPlan

def verify_volume_and_complexity(doc, source_inventory=None):
    actions=doc.get("actions",[])
    if not actions or actions[0].get("op") not in ("primitive","mesh","beam_between_faces"):
        raise RejectedPlan("Architectural addition must start with a Qwen-authored NEW physical component")
    action=actions[0]
    p=action["params"]
    # An attached detail on a small existing building need not be a standalone
    # half-metre tower. A/B visibility and source exterior gates remain mandatory.
    measured=[x for x in (source_inventory or []) if
              isinstance(x.get("world_bounds_min_m"),list) and
              isinstance(x.get("world_bounds_max_m"),list)]
    small_inherited=any(0.5 < max(
        x["world_bounds_max_m"][i]-x["world_bounds_min_m"][i] for i in range(3)
        ) <= 5 for x in measured)
    min_side=.08 if small_inherited else .12
    min_max=.28 if small_inherited else .5
    min_volume=.006 if small_inherited else .025
    if action["op"]=="beam_between_faces":
        actual=next((o for o in (source_inventory or []) if o["name"]==p["source"]),None)
        if actual is None:raise RejectedPlan("Beam requires inspected original geometry")
        actual_hits={h["face_index"]:h["world_center_m"] for h in
            actual.get("camera_visible_face_landmarks",[]) if
            h.get("camera_visible") is True and
            h.get("camera_ray_hit_original_face") is True and
            isinstance(h.get("world_center_m"),list)}
        a,b=p["start_face"],p["end_face"]
        if a not in actual_hits or b not in actual_hits or a==b:
            raise RejectedPlan("Beam must use two different REAL source-contact faces")
        length=math.dist(actual_hits[a],actual_hits[b])
        width,depth=float(p["width_m"]),float(p["depth_m"])
        if (length<min_max or min(width,depth)<min_side or
                length*width*depth<min_volume):
            raise RejectedPlan("Real beam between verified faces too small for credible mobile architecture")
        return True
    if action["op"]=="primitive":
        if p.get("shape")=="plane":
            raise RejectedPlan("An isolated flat PLANE is not the required 3D architectural component. Choose YOUR own volumetric architectural form.")
        dims=[2*float(s) for s in p["scale"]]
        if min(dims)<min_side or max(dims)<min_max or math.prod(dims)<min_volume:
            raise RejectedPlan("New component too thin or tiny to establish a readable 3D architectural addition; choose new original proportions")
    else:
        vertices=p.get("vertices",[])
        faces=p.get("faces",[])
        if len(vertices)<6 or len(faces)<4:
            raise RejectedPlan("New architecture needs an independently authored volumetric 3D mesh, at least six vertices and four actual faces; flat polygons are insufficient")
        low=[min(v[i] for v in vertices) for i in range(3)]
        high=[max(v[i] for v in vertices) for i in range(3)]
        ext=[high[i]-low[i] for i in range(3)]
        if min(ext)<min_side or max(ext)<min_max or math.prod(ext)<min_volume:
            raise RejectedPlan("New component has insufficient X/Y/Z volume; design an ORIGINAL spatially developed 3D structure, not a ground decal")
        # Check 4 noncoplanar points without deciding or modifying geometry.
        a=vertices[0]
        vectors=[[v[j]-a[j] for j in range(3)] for v in vertices[1:]]
        def det(u,v,w):
            return u[0]*(v[1]*w[2]-v[2]*w[1])-u[1]*(v[0]*w[2]-v[2]*w[0])+u[2]*(v[0]*w[1]-v[1]*w[0])
        nonflat=any(abs(det(vectors[i],vectors[j],vectors[k]))>.001
                    for i in range(len(vectors)) for j in range(i+1,len(vectors))
                    for k in range(j+1,len(vectors)))
        if not nonflat:
            raise RejectedPlan("All chosen new component vertices are COPLANAR; Qwen must author a genuine 3D volume")
        # Professional reusable architecture needs closed boundaries, not a
        # floating one-sided triangle awning. Qwen owns EVERY mesh face.
        edge_counts={}
        for face in faces:
            for i,v in enumerate(face):
                a,b=sorted((v,face[(i+1)%len(face)]))
                edge_counts[(a,b)]=edge_counts.get((a,b),0)+1
        if any(n!=2 for n in edge_counts.values()) or len(set(k for e in edge_counts for k in e))!=len(vertices):
            raise RejectedPlan("Model-authored new 3D mesh is OPEN or NONMANIFOLD: every edge must belong to exactly 2 polygon faces and every vertex must be used; design YOUR OWN closed solid or choose a volumetric Blender primitive")
    return True
