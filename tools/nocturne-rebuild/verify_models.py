#!/usr/bin/env python3
"""Independent GLB glTF2 validation for mobile art. No Unity installation required.

Counts real triangles & unique material-surface primitives, rather than using
GLB size as a proxy for visual quality/performance. Logs all metrics.
"""
from pathlib import Path
import json,struct,sys,hashlib
ROOT=Path(__file__).resolve().parents[2]
DIR=ROOT/"Unity/Assets/Eldoria/Experiments/NocturneRebuild/Models"
NAMES=[f"Bastion_{i:02d}" for i in range(1,6)]+[
"Sawmill","Barracks","Granary","Stoneworks",
"Pine","Rock","CityGround","WorldGround","ConstructionPlot"
]
def glb(path):
    data=path.read_bytes()
    if len(data)<1000 or data[:4]!=b"glTF" or len(data)!=struct.unpack("<I",data[8:12])[0]:
        raise ValueError(f"Invalid GLB2 {path} ({len(data)} bytes)")
    if struct.unpack("<I",data[4:8])[0]!=2:raise ValueError(f"GLTF version not 2: {path}")
    n,typ=struct.unpack("<II",data[12:20])
    if typ!=0x4e4f534a:raise ValueError(f"First GLB chunk not JSON: {path}")
    return json.loads(data[20:20+n])
def inspect(name):
    path=DIR/(name+".glb")
    j=glb(path)
    meshes=j.get("meshes",[])
    primitives=[x for m in meshes for x in m.get("primitives",[])]
    if len(primitives)==0:raise AssertionError(f"No 3D mesh: {name}")
    access=j.get("accessors",[])
    tris=0
    for pr in primitives:
        if pr.get("mode",4)!=4:raise AssertionError(f"Non-triangle geometry: {name}")
        if "indices" in pr:vertices=access[pr["indices"]]["count"]
        else:vertices=access[pr["attributes"]["POSITION"]]["count"]
        tris+=vertices//3
    materials=len(j.get("materials",[]))
    images=len(j.get("images",[]))
    if tris<12:raise AssertionError(f"Missing 3D detail {name}: {tris}")
    if len(primitives)>24:
        raise AssertionError(f"Mobile draw surface overload: {name} has {len(primitives)} primitives")
    if not j.get("scenes") or not j.get("nodes"):raise AssertionError(f"No importable scene: {name}")
    row={
        "name":name,"triangles":tris,"surface_batches":len(primitives),
        "materials":materials,"textures":images,
        "bytes":path.stat().st_size,
        "sha256":hashlib.sha256(path.read_bytes()).hexdigest()
    }
    print(f"{name:18s} {tris:7d} tris | {len(primitives):2d} surfaces | "
          f"{materials:2d} materials | {images:2d} images | {row['bytes']:8d} bytes")
    return row
if __name__=="__main__":
    rows=[inspect(n) for n in NAMES]
    sizes=[r["triangles"] for r in rows[:5]]
    if sizes!=sorted(sizes) or len(set(sizes))!=5:
        raise AssertionError(f"Progression meshes not visibly differentiated in topology: {sizes}")
    if len(set(r["sha256"] for r in rows[:5]))!=5:
        raise AssertionError("All five levels cannot be the same geometry")
    out=ROOT/"ArtSource/NocturneRebuild"
    out.mkdir(parents=True,exist_ok=True)
    report=out/"mobile-art-mesh-audit.json"
    report.write_text(json.dumps({"status":"STRUCTURE_ONLY_NOT_VISUAL_APPROVAL",
       "mobile_surface_gate":24,"levels":rows[:5],"other_assets":rows[5:]},
       ensure_ascii=False,indent=2),encoding="utf-8")
    print("NOCTURNE_GEOMETRY_QA_PASS",len(rows),report)
