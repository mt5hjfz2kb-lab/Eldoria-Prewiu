#!/usr/bin/env python3
"""Geometry-first disambiguation of click targets vs object centre.
This is a precursor to, not replacement for, real Unity physics and gameplay tests.
There must be no metric spatial changes until source targets have been characterized.
"""
import json,hashlib,math,pathlib,sys
import numpy as np
from scipy.spatial.transform import Rotation
base=pathlib.Path("docs/evidence/valoria-layered-construction-20261016/sharp-camera-mapping")
source=json.loads((base/"sharp-camera-rigid-calibration.json").read_text())
plan=json.loads(pathlib.Path("Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Layout/certified-legacy-skeleton.json").read_text())
ref=pathlib.Path("references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg")
assert hashlib.sha256(ref.read_bytes()).hexdigest()==source["reference_sha256"]
fit=source["global_fit"];rot=Rotation.from_quat(fit["quaternion_xyzw"]);translation=np.array(fit["translation_xyz"])
fov=source["camera_fov_degrees"];width=1230;height=845
f=1/(2*math.tan(math.radians(fov)/2));aspect=width/height
def fp(id):return next(v for v in plan["footprints"] if v["id"]==id)
def relay(label,centre,size,cls,notes):
    return dict(id=label,centre=centre,size=size,classification=cls,notes=notes)
variants={
 "Bridge":relay("Bridge",[0,.24,-14.5],[4.5,.28,10.3],"COLLIDER","existing continuous deck; walking top .38"),
 "LowerGate":relay("LowerGate",fp("Puerta · ir al mundo")["center"],fp("Puerta · ir al mundo")["size"],"HOTSPOT_TRIGGER","existing real gameplay semantic trigger"),
 "MainRoad":relay("MainRoad",fp("VPD · L0 main street")["center"],fp("VPD · L0 main street")["size"],"COLLIDER","certified route centre"),
 "CentralStair":relay("CentralStair",None,None,"12_SEPARATE_COLLIDERS","individual original stairs, not inflated aggregate cube"),
 "Bastion":relay("Bastion",[0,4.48,7.25],[5.5,3.5,4.2],"GREYBOX_NOT_INTERACTION","exact visible placeholder; click uses canonical separate data"),
 "LeftCabinParcel":relay("LeftCabinParcel",fp("VPD · L0 west plot")["center"],fp("VPD · L0 west plot")["size"],"PLOT_TRIGGER","historical plot envelope"),
 "RightCampParcel":relay("RightCampParcel",fp("VPD · L0 east plot")["center"],fp("VPD · L0 east plot")["size"],"PLOT_TRIGGER","historical plot envelope")
}
w=plan["stair"]
stairs=[]
for i in range(w["count"]):
 stairs.append(([w["first_center"][0],w["first_center"][1]+w["rise"]*i,w["first_center"][2]+w["run"]*i],
     [w["approx_width"],w["step_height"],w["step_depth"]],f"certified-tread-{i+1}"))
variants["CentralStair"]["colliders"]=stairs
def ray(uv):
    direction=np.array([(uv[0]-.5)*aspect/f,(uv[1]-.5)/f,1])
    direction/=np.linalg.norm(direction)
    return rot.inv().apply(-translation),rot.inv().apply(direction)
def projected_cube(c,s):
    from itertools import product
    pts=np.array(c)+np.array(list(product(*[[-x/2,x/2] for x in s])))
    cp=rot.apply(pts)+translation
    assert np.all(cp[:,2]>0)
    return np.column_stack((.5+f/aspect*cp[:,0]/cp[:,2],.5+f*cp[:,1]/cp[:,2]))
def contact(c,s,uv):
    o,d=ray(uv)
    lo=np.array(c)-np.array(s)/2;hi=np.array(c)+np.array(s)/2
    enter=-float('inf');leave=float('inf')
    for axis in range(3):
        if abs(d[axis])<1e-9:
            if o[axis]<lo[axis] or o[axis]>hi[axis]:return False,None
            continue
        aa=(lo[axis]-o[axis])/d[axis]
        bb=(hi[axis]-o[axis])/d[axis]
        enter=max(enter,min(aa,bb));leave=min(leave,max(aa,bb))
    return bool(leave>=max(enter,0)),float(enter)
results=[]
for src in fit["anchors"]:
 id=src["id"];v=variants[id]
 uv=np.array(src["sharp_approved_normalized"])
 shapes=v.get("colliders",[(v["centre"],v["size"],v["classification"])])
 intersects=[]
 total=[]
 for centre,size,label in shapes:
    world=projected_cube(centre,size);lo=world.min(axis=0);hi=world.max(axis=0)
    inside_bbox=bool(np.all(uv>=lo) and np.all(uv<=hi))
    ok,t=contact(centre,size,uv)
    total.append(dict(id=label,inside_screen_aabb=inside_bbox,ray_hit_actual_rotated_box=ok,
           enter_distance_m=round(t,4) if t is not None else None,
           screen_bounds=[lo.round(6).tolist(),hi.round(6).tolist()]))
    if ok:intersects.append(label)
 results.append(dict(id=id,
   original_centre_difference_px=src["residual_px"],
   visual_anchor_inside_any_projected_bound=any(z["inside_screen_aabb"] for z in total),
   source_semantic_collider_hit=bool(intersects),hit_parts=intersects,
   collider_kind=v["classification"],details=total,
   attention=("SELECTOR_MISMATCH_REQUIRES_NATIVE_DECISION" if not intersects else
              "CANDIDATE_NO_REPOSITION_REQUIRED_BEFORE_UNITY_CHECK")))
# IMPORTANT: projected AABB containment alone is insufficient evidence of ray/mesh contact.
pass_contacts=[z for z in results if z["source_semantic_collider_hit"]]
fail_contacts=[z for z in results if not z["source_semantic_collider_hit"]]
data={
 "reference_sha256":source["reference_sha256"],
 "canonical_player_camera_fov":fov,"same_locked_rigid_candidate":fit["quaternion_xyzw"],
 "method":"Exact ray vs historical source OBB per individual collider. NO new physics colliders.",
 "reported_error_81px_is_centre_to_visual_not_edge_or_contact":True,
 "all7_screen_bbox_inside":all(z["visual_anchor_inside_any_projected_bound"] for z in results),
 "source_semantic_ray_contacts":len(pass_contacts),"source_semantic_ray_failures":len(fail_contacts),
 "geometry_not_moved":True,"canonical_not_modified":True,
 "next":"Native Unity raycasts/occlusion; do NOT inflate collider silently. Inspect hotspot/mesh semantic offset.",
 "anchor_results":results,
 "tech_screen_correspondence_approved":False,
 "scene_art_approved":False
}
output=base/"semantic-geometry-contacts.json"
output.write_text(json.dumps(data,indent=2,ensure_ascii=False)+"\n")
print("VALORIA_SEMANTIC_ANCHOR_DISAMBIGUATION",json.dumps({
 "bounding_box_inside_all":data["all7_screen_bbox_inside"],"ray_contacts":len(pass_contacts),
 "ray_misses":[x["id"] for x in fail_contacts], "automatic_promotion":False},sort_keys=True))
