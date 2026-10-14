"""Read-only ACTUAL Blender review-camera visible-face evidence.

The historical landmark sampler picked faces by arbitrary local XYZ percentiles;
these could be entirely hidden from the camera. This uses the SAME active
review camera as the original image, screen projection, and actual scene ray casts.
It creates no artwork and never edits the source or invents an index.
"""
from __future__ import annotations

def camera_visible_face_landmarks(scene, obj, max_faces=24):
    from mathutils import Vector
    from bpy_extras.object_utils import world_to_camera_view

    cam=scene.camera
    if cam is None or obj.type!="MESH":
        return []
    scene.view_layers[0].update()
    dg=scene.evaluated_depsgraph_get()
    width=max(1,scene.render.resolution_x * scene.render.resolution_percentage // 100)
    height=max(1,scene.render.resolution_y * scene.render.resolution_percentage // 100)
    frame=cam.data.view_frame(scene=scene)
    fmin=[min(p[i] for p in frame) for i in (0,1)]
    fmax=[max(p[i] for p in frame) for i in (0,1)]
    cam_world=cam.matrix_world
    local_forward=Vector((0,0,-1))
    forward=(cam_world.to_3x3() @ local_forward).normalized()
    world_verts={v.index:cam_world.inverted() @ (obj.matrix_world @ v.co) for v in obj.data.vertices}
    # Build actual camera-space projections for every source vertex ONCE.
    px={}
    for v in obj.data.vertices:
        w=obj.matrix_world @ v.co
        px[v.index]=world_to_camera_view(scene,cam,w)
    source_z=[(obj.matrix_world @ v.co).z for v in obj.data.vertices]
    bottom=min(source_z,default=0);span=max(.001,max(source_z,default=1)-bottom)
    candidates=[]
    for poly in obj.data.polygons:
        xy=[px[int(i)] for i in poly.vertices]
        if not xy or any(q.z<=0 for q in xy):continue
        xlo=max(0,min(q.x for q in xy));xhi=min(1,max(q.x for q in xy))
        ylo=max(0,min(q.y for q in xy));yhi=min(1,max(q.y for q in xy))
        area=max(0,xhi-xlo)*width * max(0,yhi-ylo)*height
        if area<3:continue
        center=obj.matrix_world @ poly.center
        uv=world_to_camera_view(scene,cam,center)
        if uv.z<=0 or not (.015<=uv.x<=.985 and .015<=uv.y<=.985):continue
        height_fraction=max(0,min(1,(center.z-bottom)/span))
        band=min(2,int(height_fraction*3))
        candidates.append((area,band,poly,center,uv))
    # Respect roof, wall and lower access rather than only the largest polygons.
    selected=[];cell_count={};band_count={0:0,1:0,2:0}
    for area,band,poly,center,uv in sorted(candidates,
                         key=lambda item:(-item[0],-item[3].z,item[2].index))[:600]:
        if len(selected)>=max_faces:break
        if band_count[band]>=max(6,(max_faces+1)//3):continue
        cell=(int(uv.x*5),int(uv.y*5),band)
        if cell_count.get(cell,0)>=3:continue
        if cam.data.type=="ORTHO":
            origin_local=Vector((fmin[0]+uv.x*(fmax[0]-fmin[0]),
                                 fmin[1]+uv.y*(fmax[1]-fmin[1]),0))
            origin=cam_world @ origin_local
            direction=forward
        else:
            origin=cam_world.translation
            direction=(center-origin).normalized()
        distance=(center-origin).length +.1
        hit,_,_,face_index,hit_object,_=scene.ray_cast(
            dg,origin,direction,distance=max(0.1,distance))
        # The proof requires the actual target polygon to be the first visible
        # ray hit. Merely projecting a hidden rear face is not visibility.
        if not hit or hit_object is None or hit_object.name!=obj.name or face_index!=poly.index:
            continue
        record={
            "face_index":int(poly.index),
            "camera_visible":True,
            "camera_ray_hit_original_face":True,
            "camera_screen_uv":[round(float(uv.x),4),round(float(uv.y),4)],
            "world_center_m":[round(float(t),4) for t in center],
            "world_height_fraction":round(float((center.z-bottom)/span),4),
            "projected_bbox_area_px":round(float(area),1),
            "local_center_m":[round(float(v),4) for v in poly.center]}
        selected.append(record)
        cell_count[cell]=cell_count.get(cell,0)+1
        band_count[band]+=1
    return selected
