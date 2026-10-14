"""Noncreative physical-quality gate for FIRST new architectural component.

Reject flat placemats, tiny slivers, invented source-colliding IDs. The model
chooses every shape, position, dimension, face and material. No repairs here.
"""
import math
from contract import RejectedPlan

def verify_volume_and_complexity(doc):
    actions=doc.get("actions",[])
    if not actions or actions[0].get("op") not in ("primitive","mesh"):
        raise RejectedPlan("Architectural addition must start with a Qwen-authored NEW physical component")
    action=actions[0]
    p=action["params"]
    if action["op"]=="primitive":
        if p.get("shape")=="plane":
            raise RejectedPlan("An isolated flat PLANE is not the required 3D architectural component. Choose YOUR own volumetric architectural form.")
        dims=[2*float(s) for s in p["scale"]]
        if min(dims)<.12 or max(dims)<.5 or math.prod(dims)<.025:
            raise RejectedPlan("New component too thin or tiny to establish a readable 3D architectural addition; choose new original proportions")
    else:
        vertices=p.get("vertices",[])
        faces=p.get("faces",[])
        if len(vertices)<6 or len(faces)<4:
            raise RejectedPlan("New architecture needs an independently authored volumetric 3D mesh, at least six vertices and four actual faces; flat polygons are insufficient")
        low=[min(v[i] for v in vertices) for i in range(3)]
        high=[max(v[i] for v in vertices) for i in range(3)]
        ext=[high[i]-low[i] for i in range(3)]
        if min(ext)<.12 or max(ext)<.5 or math.prod(ext)<.025:
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
    return True
