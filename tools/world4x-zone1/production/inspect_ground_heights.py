"""Read-only Blender source-surface height sampling for LLM-specified XY knots.

Qwen authors the path XY, radius and vertical displacement. The real original
terrain supplies Z, which is geometry metadata, not an artistic choice.
Never edit the source or use synthetic terrain heights.
"""
import argparse, hashlib, json, math, pathlib, sys
import bpy

argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
p=argparse.ArgumentParser()
p.add_argument("--source",type=pathlib.Path,required=True)
p.add_argument("--positions",type=pathlib.Path,required=True)
p.add_argument("--out",type=pathlib.Path,required=True)
a=p.parse_args(argv)
if a.source.suffix.lower()!=".blend" or not a.source.is_file():
    raise ValueError("Actual frozen Blender terrain source required")
source_sha=hashlib.sha256(a.source.read_bytes()).hexdigest()
requests=json.loads(a.positions.read_text())
coords=requests.get("positions") if isinstance(requests,dict) else None
if not isinstance(coords,list) or not 1<=len(coords)<=8:
    raise ValueError("Expected 1..8 source-local XY terrain samples")
for point in coords:
    if not isinstance(point,list) or len(point)!=2 or any(
            type(x) not in (int,float) or not math.isfinite(x) or abs(x)>120 for x in point):
        raise ValueError("Invalid AI selected XY knot coordinate")

bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
obj=bpy.data.objects.get("ZoneOne continuous prairie source")
if not obj or obj.type!="MESH" or len(obj.data.vertices)<14000:
    raise ValueError("Frozen continuous terrain mesh not found")
verts=list(obj.data.vertices)
samples=[]
for x,y in coords:
    vertex=min(verts,key=lambda v:((v.co.x-x)**2+(v.co.y-y)**2,v.index))
    xy_distance=((vertex.co.x-x)**2+(vertex.co.y-y)**2)**.5
    if xy_distance>4.0:
        raise ValueError("No real source terrain vertex near Qwen's XY anchor")
    samples.append({
        "ai_selected_xy":[x,y],
        "source_sample_z":float(vertex.co.z),
        "source_vertex_index":int(vertex.index),
        "nearest_source_xy":[float(vertex.co.x),float(vertex.co.y)],
        "nearest_xy_distance_m":xy_distance
    })
if hashlib.sha256(a.source.read_bytes()).hexdigest()!=source_sha:
    raise RuntimeError("Frozen source mutated during read-only height query")
a.out.parent.mkdir(parents=True,exist_ok=True)
a.out.write_text(json.dumps({
    "source_sha256":source_sha, "source_mesh":obj.name,
    "original_vertex_count":len(verts),
    "samples":samples, "source_unchanged":True,
    "art_approved":False, "artist_numeric_decisions_rewritten":False,
    "source_height_measured_not_model_authored":True
},indent=2)+"\n")
print("ELDORIA_REAL_SOURCE_KNOT_HEIGHTS",len(samples),flush=True)
