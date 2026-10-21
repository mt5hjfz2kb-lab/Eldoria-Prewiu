#!/usr/bin/env python3
"""Read-only, source-backed creature anatomy search region for existing Qwen artist.
The +X head direction was verified in Spawnling_B2 author evidence; these are
actual mesh vertices, NOT certified semantic part labels or model-generated art.
Runs inside the existing Blender executable, never touches the .blend on disk.
"""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

import bpy

parser = argparse.ArgumentParser()
parser.add_argument("--source", required=True)
parser.add_argument("--original-context", required=True)
parser.add_argument("--target", required=True)
parser.add_argument("--author-evidence", required=True)
parser.add_argument("--out", required=True)
a = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
source = Path(a.source)
original = Path(a.original_context)
dest = Path(a.out)
if not source.is_file() or not original.is_file() or dest.exists():
    raise RuntimeError("Missing frozen source/mesh manifest or attempted overwrite")
digest = hashlib.sha256(source.read_bytes()).hexdigest()
ctx = json.loads(original.read_text("utf8"))
assert ctx["schema_version"] == 1 and ctx["source_sha256"] == digest and ctx["target"] == a.target
author = json.loads(Path(a.author_evidence).read_text("utf8"))
assert author["blend_sha256"] == digest
assert author["head_axis_convention"] == "corrected to +X after real captured art review"

bpy.ops.wm.open_mainfile(filepath=str(source.resolve()), load_ui=False)
obj = bpy.data.objects.get(a.target)
if obj is None or obj.type != "MESH" or len(obj.data.vertices) < 100:
    raise RuntimeError("Missing real source Blender mesh")
vertices = [(i, tuple(float(v) for v in vert.co)) for i, vert in enumerate(obj.data.vertices)]
lo = ctx["local_aabb"]["min"]
hi = ctx["local_aabb"]["max"]
dx,dy,dz = [hi[i] - lo[i] for i in range(3)]
ymid = (hi[1]+lo[1])/2
# +X and upper-Z face search area is backed by source head-axis evidence.
# Distinguish face candidate from claws/feet by vertical position and Y center.
selection = [(i,v) for i,v in vertices if
             v[0] > lo[0]+0.72*dx and v[2] > lo[2]+0.53*dz
             and abs(v[1]-ymid) < .35*dy]
if len(selection) < 20:
    raise RuntimeError("FRONTAL_REGION_UNSUPPORTED_BY_SOURCE: do not invent head vertices")
centroid = [sum(v[k] for _,v in selection)/len(selection) for k in range(3)]
tip = max(selection, key=lambda item: item[1][0])
median = min(selection, key=lambda item: math.dist(item[1], centroid))
highest = max(selection, key=lambda item: item[1][2])
frontal = {
    "role": "FRONTAL_FACE_SEARCH_ONLY_NOT_ANATOMICALLY_CERTIFIED",
    "head_axis_source": "Finished/author-evidence.json says +X",
    "source_blend_sha256": digest,
    "region_rule": "source-local x > 72% of X bounds; z > 53% of Z bounds; centered Y within 35% Y span",
    "mesh_vertices_in_search": len(selection),
    "source_mesh_total_vertices": len(vertices),
    "candidate_face_centroid_local": [round(x,6) for x in centroid],
    "actual_source_surface_anchors": [
        {"vertex_index": i, "position_local": [round(x,6) for x in v], "role":name}
        for name,(i,v) in [
            ("frontmost_upper_point",tip),
            ("region_center_nearest_surface_vertex",median),
            ("highest_frontal_point",highest)
        ]
    ],
    "allowed_face_sculpt_center_aabb": {
        "min": [round(min(v[k] for _,v in selection),6) for k in range(3)],
        "max": [round(max(v[k] for _,v in selection),6) for k in range(3)]
    },
    "art_director_requirement": "View the real face photo. Independently choose a facial center from real source points, a local radius, and a significant NONZERO displacement. These candidate points alone do NOT establish facial anatomy or approval.",
    "not_certified_for": ["claws","tail","limbs","full_body","rigging"]
}
# Qwen reads only a bounded prefix of mesh context, so place authentic face context
# before historical terrain-named vertices and topology loops; never fake vertex IDs.
enhanced = {"schema_version":1,"source_sha256":digest,"target":a.target,
            "creature_anatomy":frontal,
            **{k:v for k,v in ctx.items() if k not in ("schema_version","source_sha256","target")}}
output = json.dumps(enhanced, ensure_ascii=False, indent=2) + "\n"
if len(output.encode("utf8")) > 25000:
    raise RuntimeError("Exceeded existing Qwen mesh-context size contract")
dest.parent.mkdir(parents=True, exist_ok=True)
dest.write_text(output,"utf8")
print("SOURCE_BACKED_FRONTAL_FACE_ANCHORS",json.dumps({
    "blend_sha256":digest,"vertices":len(selection),"centroid":frontal["candidate_face_centroid_local"],
    "source_unchanged":True,"qwen_inference":False
}))
