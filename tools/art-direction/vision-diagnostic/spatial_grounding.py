"""Blender read-only spatial facts for the exact immutable Valoria tower.
A camera view is NOT a different mesh; WORLD XYZ and topology are authoritative.
No model, scene, or asset is ever saved or exported.
"""
import argparse, hashlib, json, math, pathlib, sys
from collections import Counter,defaultdict
import bpy
from mathutils import Vector

argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
p=argparse.ArgumentParser()
p.add_argument("--source",required=True)
p.add_argument("--observation",required=True)
p.add_argument("--out",required=True)
a=p.parse_args(argv)
source=pathlib.Path(a.source).resolve()
observation=pathlib.Path(a.observation).resolve()
output=pathlib.Path(a.out).resolve()
output.parent.mkdir(parents=True,exist_ok=True)
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
before=hashlib.sha256(source.read_bytes()).hexdigest()
if before!=EXPECTED or source.suffix.lower()!=".glb":
    raise RuntimeError("FROZEN_TOWER_SOURCE_MISMATCH")
obs=json.loads(observation.read_text(encoding="utf-8"))
if obs["source_sha256"]!=before or len(obs["frames"])!=3:
    raise RuntimeError("CAMERA_PROVENANCE_ORIGINAL_MISMATCH")
if len(set(f["view"] for f in obs["frames"]))!=3:
    raise RuntimeError("NEED_THREE_DISTINCT_CAMERA_VIEWS")
for frame in obs["frames"]:
    f=observation.parent/frame["png"]
    if hashlib.sha256(f.read_bytes()).hexdigest()!=frame["sha256"]:
        raise RuntimeError("CAMERA_IMAGE_HASH_MISMATCH")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[obj for obj in bpy.context.scene.objects if obj.type=="MESH" and obj.data.vertices]
if not meshes:raise RuntimeError("TOWER_CONTAINS_NO_MESH")
world_pts=[o.matrix_world @ v.co for o in meshes for v in o.data.vertices]
lo=[min(p[i] for p in world_pts) for i in range(3)]
hi=[max(p[i] for p in world_pts) for i in range(3)]
dims=[hi[i]-lo[i] for i in range(3)]
if not(2.0<dims[0]<3.5 and 2.0<dims[1]<3.5 and 2.0<dims[2]<4.5):
    raise RuntimeError("UNEXPECTED_ORIGINAL_TOWER_WORLD_BOUNDS")
zmin,zmax=lo[2],hi[2];height=zmax-zmin
zones=defaultdict(lambda:{"triangles":0,"world_area_m2":0.,"flat_horizontal_triangles":0,
                           "inclined_triangles":0,"vertical_triangles":0,
                           "upward_normal_triangles":0})
mesh_stats=[]
components=[]
total_triangles=0
for ob in meshes:
    me=ob.data
    # Local axis may not be world axis. Derive every fact after matrix_world.
    matrix=ob.matrix_world
    verts=[matrix @ v.co for v in me.vertices]
    if not verts:continue
    parent=list(range(len(verts)))
    def find(i):
        while parent[i]!=i:
            parent[i]=parent[parent[i]]
            i=parent[i]
        return i
    def merge(i,j):
        a,b=find(i),find(j)
        if a!=b:parent[b]=a
    for poly in me.polygons:
        ix=list(poly.vertices)
        for j in ix[1:]:merge(ix[0],j)
        # Fan-triangulate polygons for geometric evidence, WITHOUT editing mesh.
        for k in range(1,len(ix)-1):
            p0,p1,p2=verts[ix[0]],verts[ix[k]],verts[ix[k+1]]
            cross=(p1-p0).cross(p2-p0); area=cross.length*.5
            if area<1e-10:continue
            n=cross.normalized()
            center=(p0+p1+p2)/3
            zone=("low" if center.z<zmin+.35*height
                  else "middle" if center.z<zmin+.70*height else "upper")
            bucket=zones[zone];bucket["triangles"]+=1
            bucket["world_area_m2"]+=area
            if abs(n.z)>.94:bucket["flat_horizontal_triangles"]+=1
            elif abs(n.z)<.12:bucket["vertical_triangles"]+=1
            else:bucket["inclined_triangles"]+=1
            if n.z>.20:bucket["upward_normal_triangles"]+=1
            total_triangles+=1
    comp=defaultdict(list)
    for i,v in enumerate(verts):comp[find(i)].append(v)
    component_records=[]
    for vlist in comp.values():
        cl=[min(v[i] for v in vlist) for i in range(3)]
        ch=[max(v[i] for v in vlist) for i in range(3)]
        component_records.append({"vertices":len(vlist),
            "world_bounds_min_m":[round(v,5) for v in cl],
            "world_bounds_max_m":[round(v,5) for v in ch],
            "height_fraction":[round((cl[2]-zmin)/height,4),round((ch[2]-zmin)/height,4)]})
    components.extend(component_records)
    uv=sum(len(layer.data) for layer in me.uv_layers)
    mesh_stats.append({"name":ob.name,"vertices":len(me.vertices),
        "faces":len(me.polygons),"connected_islands":len(component_records),
        "uv_layer_count":len(me.uv_layers),"uv_coordinates":uv,
        "materials":[m.name if m else None for m in me.materials],
        "mesh_world_transform":[[round(float(v),7) for v in row] for row in matrix]})
components.sort(key=lambda x:x["vertices"],reverse=True)
zone_dict={}
for name in ("low","middle","upper"):
    z=zones[name]
    zone_dict[name]={key:(round(val,5) if isinstance(val,float) else val)
                     for key,val in z.items()}
upper=zones["upper"]
has_upper_slope=upper["inclined_triangles"]>5 and upper["world_area_m2"]>.1
result={"schema_version":1,"status":"SOURCE_GROUNDED_READ_ONLY_NOT_ART_APPROVAL",
    "source_sha256":before,"immutable_source_verified":True,
    "source_file":source.name,"world_up":"+Z",
    "camera_views":[{"id":f["view"],"png":f["png"],"sha256":f["sha256"],
         "camera_location_xyz":f["camera_world_location"],
         "camera_rotation_euler_xyz":f["camera_rotation"]} for f in obs["frames"]],
    "all_cameras_same_exact_source_sha256":True,
    "bounding_box": {"world_min_m":[round(v,5) for v in lo],
        "world_max_m":[round(v,5) for v in hi],
        "world_dimensions_m":[round(v,5) for v in dims],
        "bounding_box_footprint_rectangular":True,
        "footprint_does_not_constrain_roof_to_flat":True},
    "geometry":{"mesh_object_count":len(meshes),
        "triangle_count_approx":total_triangles,
        "zones_world_z":zone_dict,
        "upper_inclined_surfaces_exist":has_upper_slope,
        "note":"Upper inclined triangles indicate sloped surface geometry. Do not infer roof type, material, exact contacts, or errors solely from normals."},
    "connected_island_count":len(components),
    "largest_connected_islands":components[:30],
    "mesh_provenance":mesh_stats,
    "nonnegotiable_facts":[
      "All three images show the SAME immutable imported GLB; perspective changes are not mesh mutations.",
      "A rectangular/cubic wall footprint can architecturally support pitched or hipped roof surfaces.",
      "The asset's exterior staircase, entrance, railings and existing UVs are protected: no move/remove/redesign without new owner authorization.",
      "Axis conversion matters: original mesh LOCAL axes must not be treated as Blender WORLD XYZ.",
      "Bounding-box overlap is NOT proof of physical surface contact."
    ],
    "evidence_limitations":[
      "No automatic semantic labeling of roofs/stairs/doors inside original aggregated mesh.",
      "No structural proof from upper inclined face counts alone.",
      "No guaranteed exact surface contacts for individual roof elements.",
      "Camera renders and this spatial context are not Unity gameplay views or artistic approval."
    ],
    "artist_edits_executed":False,"unity_verified":False,"art_approved":False}
if hashlib.sha256(source.read_bytes()).hexdigest()!=before:
    raise RuntimeError("FROZEN_TOWER_WAS_MODIFIED")
output.write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_IMMUTABLE_SPATIAL_GROUNDING_PASS",before,
      "world_dim",result["bounding_box"]["world_dimensions_m"],
      "upper_inclined",has_upper_slope,"components",len(components))
