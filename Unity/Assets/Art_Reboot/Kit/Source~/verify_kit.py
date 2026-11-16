"""Genuine Blender FBX reimport gate for Phase2 kit. No preview substitutes for Unity."""
import bpy, json, hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=root/"Source~";models=root/"Models"
m=json.loads((source/"kit_manifest.json").read_text())
phase1=root.parent/"Phase1"
atlas=(phase1/"Textures/Hero_Atlas_256.png").read_bytes()
assert m["atlas_size"]==[256,256]
assert hashlib.sha256(atlas).hexdigest()==m["atlas_sha256"],"Approved atlas differs"
assert m["material_count"]<=3 and m["material_count"]==len(m["materials"])
assert len(m["pieces"])==31 and m["grid_m"]==4
assert len(set(x["id"] for x in m["pieces"]))==31
count=0
for item in m["pieces"]:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    path=models/(item["id"]+".fbx")
    assert path.exists() and path.stat().st_size>1000,("Missing binary",path)
    bpy.ops.import_scene.fbx(filepath=str(path),use_anim=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    assert len(meshes)==1,(item["id"],"Expected exactly one pivoted mesh asset",len(meshes))
    ob=meshes[0];mesh=ob.data
    tris=sum(len(p.vertices)-2 for p in mesh.polygons)
    assert tris==item["triangles"],(item["id"],tris,item["triangles"])
    assert len(mesh.uv_layers)>0,(item["id"],"UV missing")
    uv_values=[(float(u.uv.x),float(u.uv.y)) for u in mesh.uv_layers[0].data]
    unique=set((round(x,5),round(y,5)) for x,y in uv_values)
    assert len(unique)>=4,(item["id"],"Atlas UV cells lost after FBX export",sorted(unique)[:12])
    assert any(.01<x<.99 and .01<y<.99 for x,y in unique),(item["id"],"No valid fractional atlas UV",sorted(unique)[:12])
    assert all(-.002<=x<=1.002 and -.002<=y<=1.002 for x,y in unique),(item["id"],"UVs outside atlas")
    if item["id"] in ("Roof_Gable","Wall_Straight_Intact"):
        print("KIT_REIMPORTED_UV_SAMPLES",item["id"],sorted(unique)[:15],flush=True)

    assert len(mesh.color_attributes)>0,(item["id"],"vertex colors missing")
    assert len(mesh.materials)<=3,(item["id"],"material budget exceeded")
    assert len(mesh.polygons)>0 and not mesh.validate(clean_customdata=False),(item["id"],"mesh invalid")
    # Blender FBX axis conversion may reside in object transform. Measure in world
    # instead of assuming that a raw local vertex is already oriented.
    bounds=[ob.matrix_world @ v.co for v in mesh.vertices]
    lo=min(x.z for x in bounds)
    assert -0.14<=lo<=.16,(item["id"],"base/pivot shifted",lo)
    assert ob.matrix_world.translation.length<.005,(item["id"],"pivot moved",tuple(ob.location))
    assert item["pivot_m"]==[0,0,0] and item["local_min_m"][2]>=-.015
    assert all(item["local_max_m"][i]>item["local_min_m"][i] for i in range(3)),item["id"]
    count+=tris
assert count==m["total_triangles"],(count,m["total_triangles"])
print("KIT_FBX_REIMPORT_PASS",len(m["pieces"]),"pieces",count,"triangles",m["atlas_sha256"])
