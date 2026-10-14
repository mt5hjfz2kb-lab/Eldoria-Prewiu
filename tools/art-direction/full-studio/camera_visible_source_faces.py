"""Read-only Blender raycast: identify truly camera-visible inherited face IDs.

This is SOURCE EVIDENCE, not procedural authoring. Qwen chooses which visible
face to edit and all artistic values; a raycast never moves a vertex.
"""
import bpy
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view


def camera_visible_face_landmarks(mesh_object, max_faces=20):
    if mesh_object.type!="MESH" or not 1<=max_faces<=32:
        raise ValueError("Camera face probe requires bounded source mesh")
    scene=bpy.context.scene
    camera=scene.camera
    if camera is None:
        raise RuntimeError("Matched real Blender review camera required")
    # Original face ID cannot be safely mapped across topology-changing
    # evaluated modifiers. Fail closed instead of inventing face indices.
    if any(m.type in {"SUBSURF","REMESH","BOOLEAN","DECIMATE","MULTIRES"}
           and m.show_render for m in mesh_object.modifiers):
        return []
    bpy.context.view_layer.update()
    depsgraph=bpy.context.evaluated_depsgraph_get()
    camera_pos=camera.matrix_world.translation.copy()
    forward=camera.matrix_world.to_quaternion() @ Vector((0,0,-1))
    selected={}
    for poly in mesh_object.data.polygons:
        world_center=mesh_object.matrix_world @ poly.center
        ndc=world_to_camera_view(scene,camera,world_center)
        if not (0.03<=ndc.x<=0.97 and 0.03<=ndc.y<=0.97 and ndc.z>0):
            continue
        if camera.data.type=="ORTHO":
            depth=(world_center-camera_pos).dot(forward)
            if depth<=0:
                continue
            origin=world_center-forward*(depth+0.05)
            direction=forward
            reach=depth+0.08
        else:
            vector=world_center-camera_pos
            reach=vector.length+0.02
            if vector.length<0.01:
                continue
            origin=camera_pos
            direction=vector.normalized()
        hit,location,normal,face_index,hit_object,matrix=scene.ray_cast(
            depsgraph,origin,direction,distance=reach)
        if not hit or hit_object is None or hit_object.original!=mesh_object:
            continue
        if face_index!=poly.index:
            continue
        cell=(min(4,int(ndc.x*5)),min(3,int(ndc.y*4)))
        # Prefer the larger actually frontmost surface in each screen cell.
        score=poly.area
        entry={"face_index":int(poly.index),
               "screen_xy":[round(float(ndc.x),3),round(float(ndc.y),3)],
               "world_center_m":[round(float(v),4) for v in world_center],
               "local_center_m":[round(float(v),4) for v in poly.center],
               "local_area_m2":round(float(poly.area),6)}
        if cell not in selected or selected[cell][0]<score:
            selected[cell]=(score,entry)
    items=[entry for _,entry in selected.values()]
    items.sort(key=lambda e:(e["screen_xy"][1],e["screen_xy"][0]))
    return items[:max_faces]
