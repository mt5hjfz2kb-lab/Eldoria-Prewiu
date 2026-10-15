"""Check upper sloped surface ORIENTATIONS in true Blender WORLD coordinates.
This is geometric evidence, not a semantic roof-type certification.
"""
import argparse,hashlib,json,math,pathlib,sys
from collections import defaultdict
import bpy
from mathutils import Vector
a=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
p=argparse.ArgumentParser()
p.add_argument("--source",required=True)
p.add_argument("--out",required=True)
x=p.parse_args(a)
path=pathlib.Path(x.source).resolve()
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if hashlib.sha256(path.read_bytes()).hexdigest()!=EXPECTED:raise RuntimeError("ORIGINAL_TOWER_SHA_MISMATCH")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(path))
objects=[o for o in bpy.context.scene.objects if o.type=="MESH"]
allv=[o.matrix_world @ v.co for o in objects for v in o.data.vertices]
if not allv:raise RuntimeError("NO_ORIGINAL_GEOMETRY")
zmin=min(v.z for v in allv);zmax=max(v.z for v in allv)
threshold=zmin+.70*(zmax-zmin)
buckets=defaultdict(lambda:{"triangles":0,"area_m2":0.0,"average_nz_area":0.0})
for ob in objects:
    w=[ob.matrix_world @ v.co for v in ob.data.vertices]
    for face in ob.data.polygons:
        ix=list(face.vertices)
        for i in range(1,len(ix)-1):
            v0,v1,v2=(w[ix[0]],w[ix[i]],w[ix[i+1]])
            center=(v0+v1+v2)/3
            if center.z<threshold:continue
            cross=(v1-v0).cross(v2-v0)
            area=cross.length*.5
            if area<.0001:continue
            n=cross.normalized()
            if not (.20<abs(n.z)<.96):continue
            axis=("X" if abs(n.x)>=abs(n.y) else "Y")
            sign=("+" if (n.x if axis=="X" else n.y)>=0 else "-")
            code=axis+sign
            b=buckets[code]
            b["triangles"]+=1
            b["area_m2"]+=area
            b["average_nz_area"]+=abs(n.z)*area
report={}
for axis in ("X+","X-","Y+","Y-"):
    bucket=buckets[axis]
    area=bucket["area_m2"]
    report[axis]={"triangles":bucket["triangles"],"world_area_m2":round(area,5),
                  "average_abs_vertical_normal":round(bucket["average_nz_area"]/area,4) if area else None}
large=[axis for axis,v in report.items() if v["world_area_m2"]>=.30]
conclusion=("MULTIPLE_UPPER_SURFACE_SLOPE_DIRECTIONS"
    if len(large)>=3 else "ONLY_"+str(len(large))+"_MATERIAL_SLOPE_DIRECTIONS_CONFIRMED")
payload={"schema_version":1,"source_sha256":EXPECTED,
    "world_up_axis":"+Z","upper_zone_zmin":round(threshold,5),
    "upper_surface_inclined_normal_area_by_world_direction":report,
    "substantial_directions_threshold_area_m2":0.30,
    "substantial_directions":large,"geometric_direction_signal":conclusion,
    "limitations":[
      "Surface normals may include roof tiles, trims, walls and undersides; this is not semantic roof segmentation.",
      "A normal-direction histogram does not certify exact roof type, ridges, number of true roof planes or contacts.",
      "A model must not replace the roof merely because a visual caption calls it gabled or hipped.",
      "All camera viewpoints display the same original GLB source."
    ],
    "source_mutated":False,"art_approved":False,"blender_edited":False}
if hashlib.sha256(path.read_bytes()).hexdigest()!=EXPECTED:raise RuntimeError("MUTATED_SOURCE")
o=pathlib.Path(x.out)
o.parent.mkdir(parents=True,exist_ok=True)
o.write_text(json.dumps(payload,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_ROOF_DIRECTION_SPATIAL_PROBE",json.dumps(report),conclusion)
