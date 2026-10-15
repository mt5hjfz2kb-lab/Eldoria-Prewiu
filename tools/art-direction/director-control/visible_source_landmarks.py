"""Read-only proof: actual Blender vertex -> locked review camera pixel coordinates.

This does not design geometry, select edits, move vertices or bypass the
professional asset studio. A human-inspected safe lateral wall region guides
Qwen toward source-observed candidate locations, not invented coordinates.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import pathlib
import sys
import bpy
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view

def digest(p):
    return hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()

def main():
    after=sys.argv[sys.argv.index("--")+1:]
    p=argparse.ArgumentParser()
    p.add_argument("--source",type=pathlib.Path,required=True)
    p.add_argument("--review",type=pathlib.Path,required=True)
    p.add_argument("--out",type=pathlib.Path,required=True)
    a=p.parse_args(after)
    source_sha=digest(a.source)
    expected="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
    if source_sha!=expected:
        raise RuntimeError("SOURCE_VERTEX_PROJECTION_HASH_NOT_FROZEN")
    review=json.loads(a.review.read_text(encoding="utf-8-sig"))
    render=review["render"]
    W,H=render["width"],render["height"]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(a.source))
    meshes=[o for o in bpy.data.objects if o.type=="MESH"]
    if len(meshes)!=1:raise RuntimeError("PROJECTION_EXPECTS_ONE_IMPORT_MESH")
    ob=meshes[0]
    camera=bpy.data.cameras.new("ELDORIA_READONLY_CAMERA")
    cam=bpy.data.objects.new("ELDORIA_READONLY_CAMERA",camera)
    bpy.context.scene.collection.objects.link(cam)
    spec=review["camera"]
    cam.location=Vector(spec["location"])
    target=Vector(spec["target"])
    cam.rotation_euler=(target-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.type="ORTHO"
    cam.data.ortho_scale=float(spec["ortho_scale"])
    bpy.context.scene.camera=cam
    bpy.context.scene.render.resolution_x=W
    bpy.context.scene.render.resolution_y=H
    bpy.context.scene.render.resolution_percentage=100
    bpy.context.view_layer.update()
    # The safe ROI is grounded in actual screenshot of Valoria's visible
    # log wall, not generic invented geometry or creative positioning.
    safe_box=[305,265,481,465]
    safe=[]
    for v in ob.data.vertices:
        w=ob.matrix_world @ v.co
        if not 0.43<=w.z<=2.18:continue
        projected=world_to_camera_view(bpy.context.scene,cam,w)
        if projected.z<=0:continue
        x=projected.x*W
        y=(1-projected.y)*H
        if not (safe_box[0]<=x<=safe_box[2] and safe_box[1]<=y<=safe_box[3]):
            continue
        if any(reg["box_px"][0]-15<=x<=reg["box_px"][2]+15 and
               reg["box_px"][1]-15<=y<=reg["box_px"][3]+15
               for reg in review["protected_screen_regions"]):
            continue
        safe.append({"vertex_index":v.index,
                     "local_center":[round(float(t),5) for t in v.co],
                     "world_center":[round(float(t),5) for t in w],
                     "screen_pixel":[round(x,2),round(y,2)],
                     "source_observed":True})
    # Spatial coverage of the unprotected wall rather than arbitrary first N
    # vertices. This picks only factual source sample points; Qwen chooses edit.
    candidates=[]
    used=set()
    for yi in range(4):
        for xi in range(4):
            aimx=safe_box[0]+(xi+.5)*(safe_box[2]-safe_box[0])/4
            aimy=safe_box[1]+(yi+.5)*(safe_box[3]-safe_box[1])/4
            ordered=sorted(safe,key=lambda pt:
                           (pt["screen_pixel"][0]-aimx)**2+
                           (pt["screen_pixel"][1]-aimy)**2)
            for item in ordered:
                if item["vertex_index"] not in used:
                    candidates.append(item)
                    used.add(item["vertex_index"])
                    break
    if len(candidates)<10:
        raise RuntimeError("INSUFFICIENT_REAL_SAFE_VISIBLE_MESH_ANCHORS")
    result={
      "schema_version":1,
      "source_sha256":source_sha,"original_untouched":True,
      "camera":"ACTUAL_LOCKED_HERO_CAMERA",
      "camera_review_sha256":digest(a.review),
      "review_resolution":[W,H],
      "human_inspected_safe_side_wall_roi":safe_box,
      "protected_region_boxes":[x["box_px"] for x in review["protected_screen_regions"]],
      "source_candidate_count":len(safe),
      "selected_balanced_landmarks":candidates,
      "camera_visibility_note":"Projection only; not proof of ray-visible or semantic timber joints",
      "author_values_rewritten":False,
      "art_approved":False
    }
    a.out.parent.mkdir(parents=True,exist_ok=True)
    a.out.write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf8")
    if digest(a.source)!=source_sha:
        raise RuntimeError("SOURCE_CHANGED_DURING_READONLY_PROJECTION")
    print("ELDORIA_VERIFIED_SOURCE_PROJECTION",len(safe),len(candidates))
if __name__=="__main__":
    main()
