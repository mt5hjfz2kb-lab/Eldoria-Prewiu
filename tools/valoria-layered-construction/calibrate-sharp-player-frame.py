#!/usr/bin/env python3
"""Conservative 3D legacy-layout -> real SHARP camera rigid calibration.
No scene edits. Fits one 6DOF rigid transform in native metres; refuses false camera PASS.
Ground data: approved 22-footprint certified-legacy-skeleton.json, exact
SHARP semantic anchors from approved Unity evidence, and 44.42281 FOV.
"""
import json,math,hashlib,argparse,pathlib
import numpy as np
from scipy.spatial.transform import Rotation
from scipy.optimize import least_squares

ap=argparse.ArgumentParser()
ap.add_argument("--out",required=True)
args=ap.parse_args()
out=pathlib.Path(args.out);out.mkdir(parents=True,exist_ok=True)
layout=json.loads(pathlib.Path("Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Layout/certified-legacy-skeleton.json").read_text())
sharp=json.loads(pathlib.Path("docs/evidence/valoria-visual-authority-reset-v1/production-slice-evidence.json").read_text())
review=json.loads(pathlib.Path("docs/evidence/valoria-visual-authority-reset-v1/review.json").read_text())
variant=json.loads(pathlib.Path("docs/evidence/valoria-layered-construction-20261016/bridge-v1/BridgeLevelAccessV1-manifest.json").read_text())
ref="references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
refsha=hashlib.sha256(pathlib.Path(ref).read_bytes()).hexdigest()
assert refsha=="8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689"
assert layout["classification"]=="LEGACY_QA_CERTIFIED_TOPOLOGY_NOT_CANONICAL_SHARP_GEOMETRY"
assert sharp["camera"]["fov"]==review["camera"]["fov"]==44.42281
assert sharp["camera"]["width"]==1230 and sharp["camera"]["height"]==845
assert sharp["semantic_raycast_pass"]==sharp["semantic_raycast_required"]==45
assert variant["bridge_z_limits"]==[-19.65,-9.35]
def fp(name):
    return next(item for item in layout["footprints"] if item["id"]==name)["center"]
st=layout["stair"]
stairs=[st["first_center"][0],st["first_center"][1]+(st["count"]-1)*st["rise"]/2,
        st["first_center"][2]+(st["count"]-1)*st["run"]/2]
bridge=[0.,variant["deck_walkway_y"],sum(variant["bridge_z_limits"])/2]
# Every coordinate comes from a source footprint, authored bridge manifest or certified stair.
anchors={
    "Bridge":bridge,
    "LowerGate":fp("Puerta · ir al mundo"),
    "MainRoad":fp("VPD · L0 main street"),
    "CentralStair":stairs,
    "Bastion":layout["bastion_hero"]["center"],
    "LeftCabinParcel":fp("VPD · L0 west plot"),
    "RightCampParcel":fp("VPD · L0 east plot")
}
# Other two SHARP semantics have no homologous *certified centre* and are excluded:
# the UpperWalls and TerrainCliffSupport screen anchors do NOT correspond
# to 'master upper civic link' or arbitrarily guessed terrain XYZ.
semantic={row["id"]:row for row in sharp["home_anchors"]}
assert set(anchors).issubset(semantic.keys())
names=list(anchors)
pts=np.array([anchors[k] for k in names],float)
targets=np.array([[semantic[k]["x"],semantic[k]["y"]] for k in names],float)
w,h=sharp["camera"]["width"],sharp["camera"]["height"]
fov=sharp["camera"]["fov"]
f=1/(2*math.tan(math.radians(fov)/2))
aspect=w/h
def projected(parameters):
    coords=Rotation.from_rotvec(parameters[:3]).apply(pts)+parameters[3:6]
    if np.any(coords[:,2]<1.0):return None
    return np.stack([.5+f/aspect*coords[:,0]/coords[:,2],
                     .5+f*coords[:,1]/coords[:,2]],axis=1)
def solve(indices):
    def residual(p):
        screen=projected(p)
        if screen is None:return np.ones(len(indices)*2)*1e3
        return (screen[indices]-targets[indices]).ravel()
    result=None
    for degrees in [[-35,20,0],[-60,25,0],[-25,-15,0]]:
        init=np.r_[Rotation.from_euler("xyz",degrees,degrees=True).as_rotvec(),[0.,1.5,25.]]
        opt=least_squares(residual,init,max_nfev=1500,
             bounds=([-3.14159]*3+[-200.,-200.,5.],[3.14159]*3+[200.,200.,200.]),
             ftol=1e-12,xtol=1e-12,gtol=1e-12)
        if result is None or np.linalg.norm(residual(opt.x))<np.linalg.norm(residual(result.x)):
            result=opt
    return result.x
def table(fitted):
    px=projected(fitted)
    assert px is not None
    return [{"id":name,"legacy_world_xyz":[float(v) for v in pts[i]],
             "sharp_approved_normalized":[float(v) for v in targets[i]],
             "fitted_viewport":[float(v) for v in px[i]],
             "residual_px":round(float(np.linalg.norm((px[i]-targets[i])*[w,h])),3)}
            for i,name in enumerate(names)]
# Two independent, measured diagnostics: 4-structure central route and 7 inclusive plots.
spine_ids=[names.index(n) for n in ["Bridge","LowerGate","MainRoad","CentralStair"]]
fit_spine=solve(spine_ids)
fit_all=solve(list(range(len(names))))
spine=table(fit_spine);full=table(fit_all)
camera_gate_px=12.0
# A native metre rigid transform must match *all* game-critical plots: never force a pass by
# fitting only the central road and ignoring left/right districts.
full_max=max(x["residual_px"] for x in full)
spine_holdout_max=max(x["residual_px"] for x in spine if x["id"] not in {"Bridge","LowerGate","MainRoad","CentralStair"})
passed=(full_max<=camera_gate_px)
rot=Rotation.from_rotvec(fit_all[:3])
result={
 "schema_version":1,
 "reference_sha256":refsha,
 "camera_authority":"ValoriaParcelPresentation.Home(), SHARP perspective",
 "camera_fov_degrees":fov,
 "camera_identity_pose":True,
 "aspect_ratio":aspect,
 "source_data":["certified-legacy-skeleton.json","BridgeLevelAccessV1-manifest.json","production-slice-evidence.json"],
 "excluded_unmatched_semantics":["UpperWalls","TerrainCliffSupport"],
 "world_layout_modification":False,"rigid_transform_scale":1,
 "global_fit":{
   "quaternion_xyzw":[float(q) for q in rot.as_quat()],
   "translation_xyz":[float(v) for v in fit_all[3:6]],
   "world_canonical_coordinate_method":"R(world)*1 + T, Unity +Z perspective, FOV 44.42281",
   "max_residual_px":full_max,
   "rms_residual_px":round(math.sqrt(sum(x["residual_px"]**2 for x in full)/len(full)),3),
   "anchors":full,
 },
 "spine_fit":{
   "max_holdout_error_px":spine_holdout_max,
   "anchors":spine,
 },
 "camera_to_world_mapping_pass":passed,
 "threshold_all_anchors_px":camera_gate_px,
 "validation_scope":"DIAGNOSTIC_ONLY_NOT_GAMEPLAY_CERTIFIED",
 "recommended_next_step":("Implement native Unity player camera perspective plus semantic plot tests; "
                          "if failed, use an isolated independently authored 3D layout compatible with certified topology, "
                          "not unverified arbitrary point shifts")
}
(out/"sharp-camera-rigid-calibration.json").write_text(json.dumps(result,indent=2,ensure_ascii=False)+"\n")
# Spatially truthful viewport A/B: the target points and the actual best rigid projections.
svg=['<svg xmlns="http://www.w3.org/2000/svg" width="1230" height="845" viewBox="0 0 1230 845">',
'<rect width="1230" height="845" fill="#e5edf1"/>',
'<text x="30" y="34" font-family="sans-serif" font-size="23">VALORIA · SHARP perspectiva FOV 44.42281° · RIGID FIT DIAGNOSTIC</text>',
'<text x="30" y="62" font-family="sans-serif" font-size="17">Círculos = anclas canónicas · cruces = posiciones 3D tras única transformación rígida (no validadas)</text>']
for row in full:
    a=row["sharp_approved_normalized"];b=row["fitted_viewport"]
    ax,ay=a[0]*w,(1-a[1])*h
    bx,by=b[0]*w,(1-b[1])*h
    label=row["id"]
    svg += [f'<path d="M {ax:.1f},{ay:.1f} L {bx:.1f},{by:.1f}" stroke="#aa3322" stroke-width="2"/>',
            f'<circle cx="{ax:.1f}" cy="{ay:.1f}" r="9" fill="#146d71"/>',
            f'<path d="M {bx-8:.1f},{by-8:.1f} L {bx+8:.1f},{by+8:.1f} M {bx-8:.1f},{by+8:.1f} L {bx+8:.1f},{by-8:.1f}" stroke="#b22b21" stroke-width="3"/>',
            f'<text x="{ax+13:.1f}" y="{ay+3:.1f}" font-family="sans-serif" font-size="18">{label}: {row["residual_px"]:.1f}px</text>']
svg += [f'<text x="30" y="816" font-family="sans-serif" font-size="18">MAX ERROR {full_max:.1f}px · THRESHOLD {camera_gate_px}px · {"PASS" if passed else "FAIL — NO RUNTIME PROMOTION"}</text>',
        '</svg>']
(out/"sharp-rigid-reprojection-A-B.svg").write_text("\n".join(svg)+"\n")
print("VALORIA_SHARP_RIGID_CAMERA_FIT",json.dumps({
    "all7_max_error_px":full_max,"rms_px":result["global_fit"]["rms_residual_px"],
    "spine_holdout_max_px":spine_holdout_max,"mapping_pass":passed},sort_keys=True))
