"""Read-only proof of new Qwen art touching actual inherited Blender mesh SURFACES.

Prior AABB-only preflight accepted Qwen 8B roof rods floating beside the
source (run 38694405904). This geometric test measures closest points on
real inherited evaluated triangle surfaces, not only world bounding boxes.
It MUST NOT create, translate or sculpt ANY part.
"""
from __future__ import annotations
import math


def actual_inherited_surface_contact(original_names, *, max_samples=300,
                                     tolerance_m=.07):
    import bpy
    from mathutils.bvhtree import BVHTree

    dg=bpy.context.evaluated_depsgraph_get()
    all_objs=bpy.data.objects
    original=[all_objs.get(n) for n in original_names]
    original=[o for o in original if o and o.type=="MESH"]
    children=[o for o in all_objs if o.type=="MESH" and
              o.name not in original_names and
              not o.name.startswith("_EldoriaReview")]
    if not original:
        return {"status":"NO_REAL_INHERITED_SURFACE_DATA",
                "art_approved":False,"verified":False,"objects":[]}
    bvh=[]
    for o in original:
        # Blender BVHTree is in object's mesh-local space.
        obj_eval=o.evaluated_get(dg)
        mesh_bvh=BVHTree.FromObject(obj_eval,dg)
        if mesh_bvh is not None:
            bvh.append((o,mesh_bvh,o.matrix_world.inverted()))
    if not bvh:
        return {"status":"NO_REAL_INHERITED_SURFACE_DATA",
                "art_approved":False,"verified":False,"objects":[]}

    observations=[]
    for candidate in children:
        points=[]
        for vert in candidate.data.vertices:
            points.append(candidate.matrix_world @ vert.co)
        for poly in candidate.data.polygons:
            # Face center sampling catches mid-face contact of original wood.
            points.append(candidate.matrix_world @ poly.center)
        # Mesh edges can penetrate original source between vertices.
        for e in candidate.data.edges:
            if len(points)>=max_samples*2:
                break
            a,b=(candidate.data.vertices[int(j)].co for j in e.vertices)
            points.append(candidate.matrix_world @ ((a+b)*.5))
        if len(points)>max_samples:
            stride=math.ceil(len(points)/max_samples)
            points=points[::stride][:max_samples]
        nearest=math.inf
        which=None
        nearest_pair=None
        for point in points:
            for old,bvh_tree,inverse in bvh:
                coord=inverse @ point
                hit=bvh_tree.find_nearest(coord)
                if hit[0] is None:
                    continue
                world_surface=old.matrix_world @ hit[0]
                dist=float((point-world_surface).length)
                if dist<nearest:
                    nearest=dist
                    which=old.name
                    nearest_pair=[list(round(float(c),5) for c in point),
                                  list(round(float(c),5) for c in world_surface)]
        touching=bool(math.isfinite(nearest) and nearest<=tolerance_m)
        observations.append({
            "name":candidate.name,
            "source_surface_nearest_object":which,
            "min_real_mesh_surface_distance_m":round(nearest,5) if math.isfinite(nearest) else None,
            "contact_tolerance_m":tolerance_m,
            "surface_contact_candidate":touching,
            "sampled_actual_vertices_face_centers_edges":len(points),
            "nearest_world_points":nearest_pair
        })
    return {"status":"BLENDER_EVALUATED_MESH_CONTACT_MEASURED",
            "verified":True,"no_geometry_modified":True,"art_approved":False,
            "actual_surface_contact_all_new_components":
                bool(observations) and all(o["surface_contact_candidate"] for o in observations),
            "objects":observations}
