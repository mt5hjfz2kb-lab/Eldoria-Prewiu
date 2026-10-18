"""Measure historical wall assets in real Blender WORLD coordinates against frozen V7 gate.
Read-only diagnosis; never alter the V7 source, reference, bridge, original wall family,
colliders, or gameplay. All numbers are taken from linked Blender geometry.
"""
import bpy,math,json,hashlib,os
from mathutils import Vector
root=os.getcwd()
out=os.path.join(root,"docs/evidence/valoria-layered-construction-20261016/entry-zone-v11-inspection")
os.makedirs(out,exist_ok=True)
s=lambda p:hashlib.sha256(open(p,"rb").read()).hexdigest()
base=os.path.join(root,"docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/ValoriaEntryZoneV7.blend")
donor=os.path.join(root,"art-source/valoria/production/wall-family-v1/WallFamilyV1.blend")
assert s(base)=="24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381"
assert os.path.isfile(donor)
bpy.ops.wm.open_mainfile(filepath=base)
bpy.context.view_layer.update()
def bounds(o):
    if o.type!="MESH" or not o.data.vertices: return None
    verts=[o.matrix_world@v.co for v in o.data.vertices]
    mins=[min(p[i] for p in verts) for i in range(3)]
    maxs=[max(p[i] for p in verts) for i in range(3)]
    return {"world_min_xyz":[round(v,4) for v in mins],
      "world_max_xyz":[round(v,4) for v in maxs],
      "world_size_xyz":[round(b-a,4) for a,b in zip(mins,maxs)],
      "vertex_count":len(verts),"polygon_count":len(o.data.polygons),
      "matrix_world":[[round(float(o.matrix_world[i][j]),5) for j in range(4)] for i in range(4)],
      "material_names":[m.name if m else "NONE" for m in o.data.materials]}
existing=[o for o in bpy.data.objects if o.type=="MESH" and o.name.startswith("ENTRY_LowerGate")]
old={o.name:bounds(o) for o in existing}
with bpy.data.libraries.load(donor,link=False) as(src,dst):
 names=list(src.objects)
 dst.objects=[n for n in names if any(w in n for w in (
  "WestPartialWall","EastPartialWall","WestWallTowerA","WestWallTowerB",
  "EastWallTowerA","EastWallTowerB"))]
selected=[o for o in dst.objects if o and o.type=="MESH"]
for o in selected:bpy.context.scene.collection.objects.link(o)
bpy.context.view_layer.update() # mandatory before reading WORLD matrices of newly appended sources
walls={o.name:bounds(o) for o in selected}
report={"status":"REAL_NATIVE_BLENDER_WORLD_BOUNDS_ONLY",
 "source_v7_sha256":s(base),"wall_family_sha256":s(donor),
 "donor_object_names":names,"selected_object_count":len(selected),
 "wall_world_bounds":walls,"gate_world_bounds":old,
 "conversion_reminder":"Existing sources may already have transforms; glTFast reflects X when importing from RH glTF to Unity. Do not mirror or rebase blindly.",
 "license_reminder":"Must inspect individual source texture provenance before reuse.",
 "recommendation":"Do not authorize assembly until detailed WORLD bounds, site intercepts, and source texture licenses are reviewed."}
path=os.path.join(out,"wall-source-world-bounds.json")
with open(path,"w",encoding="utf8") as f:json.dump(report,f,indent=2,ensure_ascii=False)
print("VALORIA_WALL_BOUNDS_COMPLETE",json.dumps(report,ensure_ascii=False))
