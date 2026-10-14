"""Read-only exact existing Blender terrain height inventory for Qwen XY paths.

This is NOT a terrain generator, creative decision maker, mesh transformation,
or a replacement for the artist. It reads all SOURCE vertices so a model-picked
horizontal knot can be anchored to the actual inherited terrain surface.
"""
import argparse
import hashlib
import json
import pathlib
import sys
import bpy

p=argparse.ArgumentParser()
p.add_argument("--source",type=pathlib.Path,required=True)
p.add_argument("--target",required=True)
p.add_argument("--out",type=pathlib.Path,required=True)
args=p.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])

if not args.source.is_file() or args.source.suffix.lower()!=".blend":
    raise ValueError("Immutable original Blender .blend required")
source_sha=hashlib.sha256(args.source.read_bytes()).hexdigest()
bpy.ops.wm.open_mainfile(filepath=str(args.source.resolve()))
obj=bpy.data.objects.get(args.target)
if obj is None or obj.type!="MESH" or len(obj.data.vertices)<1000:
    raise ValueError("Real inherited source terrain mesh unavailable")
coords=[[v.index,float(v.co.x),float(v.co.y),float(v.co.z)]
        for v in obj.data.vertices]
if len({(v[1],v[2]) for v in coords})<1000:
    raise ValueError("Cannot safely anchor XY on a non-terrain mesh")
if hashlib.sha256(args.source.read_bytes()).hexdigest()!=source_sha:
    raise ValueError("Frozen source mutated during read-only source inspection")
payload={
    "schema_version":1,
    "source_sha256":source_sha,
    "target":args.target,
    "vertex_count":len(coords),
    "coordinates":"ACTUAL_BLENDER_OBJECT_LOCAL_XY_AND_Z",
    "vertex_positions":coords,
    "source_unchanged":True,
    "art_approved":False,
}
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps(payload,separators=(",",":"))+"\n",encoding="utf-8")
print("ELDORIA_READ_ONLY_TERRAIN_HEIGHT_INVENTORY",len(coords),source_sha,flush=True)
