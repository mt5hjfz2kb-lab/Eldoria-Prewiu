import bpy, json, os
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
src=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","HeroBastionGenerated","Valoria_HeroBastion_v1.glb")
out=os.environ.get("BASTION_AUDIT_OUT",os.path.join(ROOT,"hero-bastion-geometry-audit.json"))
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)
def wb(o):
    pts=[o.matrix_world @ Vector(c) for c in o.bound_box]
    return {"min":[min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)],
            "max":[max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)]}
meshes=[]
for o in bpy.context.scene.objects:
    if o.type!="MESH": continue
    o.data.calc_loop_triangles()
    meshes.append({"name":o.name,"vertices":len(o.data.vertices),"triangles":len(o.data.loop_triangles),
                   "materials":[m.name if m else None for m in o.data.materials],"bounds_blender":wb(o)})
payload={"source":src,"mesh_count":len(meshes),"materials":sorted({m.name for m in bpy.data.materials}),"meshes":meshes}
with open(out,"w",encoding="utf-8") as f: json.dump(payload,f,indent=2)
print(json.dumps(payload,indent=2))
