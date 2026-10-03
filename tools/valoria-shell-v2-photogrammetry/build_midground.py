"""Author a full-width fixed-camera Valoria midground from CC0 photogrammetric cliffs.
No generated terrain, no heightfield, no voxel/remesh substrate. The source models keep their
own scan-derived geometry and PBR materials; Blender only composes/scales them into the frame.
Poly Haven assets are CC0. API usage: Powered by Poly Haven — https://polyhaven.com
"""
import bpy, os, sys, json, math, urllib.request, hashlib
from pathlib import Path
from mathutils import Vector

ASSETS=[
    {"id":"mountainside","name":"rear mountainside","anchor":(0.0,-3.1,9.5),"span":48.0,"height":11.5,"yaw":180.0},
    {"id":"rock_face_01","name":"front west cliff","anchor":(-12.0,-3.25,-1.3),"span":27.5,"height":7.0,"yaw":18.0},
    {"id":"rock_face_02","name":"front east cliff","anchor":(12.0,-3.15,-.9),"span":27.5,"height":7.2,"yaw":-18.0},
]
UA={"User-Agent":"Eldoria-ShellV2/1.0 (+https://polyhaven.com)"}

ROOT=Path(os.path.abspath(os.path.join(os.path.dirname(__file__),"..","..")))
CACHE=Path(os.environ.get("ELDORIA_PH_CACHE",str(ROOT/".tmp_polyhaven")))
CACHE.mkdir(parents=True,exist_ok=True)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

def unity_to_blender(p):
    x,y,z=p
    return Vector((x,-z,y))

def fetch_json(url):
    req=urllib.request.Request(url,headers=UA)
    with urllib.request.urlopen(req,timeout=120) as r:
        return json.load(r)

def download(url,path,expected_md5=None):
    path=Path(path);path.parent.mkdir(parents=True,exist_ok=True)
    if not path.exists() or path.stat().st_size==0:
        req=urllib.request.Request(url,headers=UA)
        with urllib.request.urlopen(req,timeout=240) as src, open(path,"wb") as dst:
            while True:
                chunk=src.read(1024*1024)
                if not chunk:break
                dst.write(chunk)
    if expected_md5:
        h=hashlib.md5(path.read_bytes()).hexdigest()
        if h.lower()!=expected_md5.lower():
            raise RuntimeError(f"MD5 mismatch for {path}: {h} != {expected_md5}")
    return path

def acquire_gltf(asset_id):
    files=fetch_json("https://api.polyhaven.com/files/"+asset_id)
    try:
        entry=files["gltf"]["1k"]["gltf"]
    except Exception:
        # Fallback to the smallest available glTF resolution.
        resolutions=files.get("gltf",{})
        if not resolutions:raise RuntimeError("No glTF published for "+asset_id)
        key=sorted(resolutions.keys(),key=lambda x:int(x[:-1]) if x.endswith("k") and x[:-1].isdigit() else 999)[0]
        entry=resolutions[key]["gltf"]
    dest=CACHE/asset_id
    main=download(entry["url"],dest/Path(urllib.parse.urlparse(entry["url"]).path).name,entry.get("md5"))
    for rel,inc in (entry.get("include") or {}).items():
        download(inc["url"],dest/rel,inc.get("md5"))
    # glTF references includes relatively; main file must sit at package root.
    if main.parent!=dest:
        target=dest/main.name
        target.write_bytes(main.read_bytes());main=target
    return main,entry

def mesh_objects(objs):
    return [o for o in objs if o.type=="MESH"]

def bounds_world(objs):
    pts=[]
    for o in mesh_objects(objs):
        pts.extend([o.matrix_world @ Vector(c) for c in o.bound_box])
    if not pts:raise RuntimeError("No mesh bounds")
    lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return lo,hi

def import_and_place(spec):
    gltf,entry=acquire_gltf(spec["id"])
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=str(gltf))
    imported=[o for o in bpy.context.scene.objects if o not in before]
    meshes=mesh_objects(imported)
    if not meshes:raise RuntimeError("No meshes imported: "+spec["id"])
    root=bpy.data.objects.new("PH · "+spec["name"],None)
    bpy.context.scene.collection.objects.link(root)
    tops=[o for o in imported if o.parent is None]
    for o in tops:o.parent=root
    root.rotation_euler[2]=math.radians(-spec["yaw"])
    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    size=hi-lo
    scale=min(spec["span"]/max(size.x,size.y,.001),spec["height"]/max(size.z,.001))
    root.scale=(scale,scale,scale)
    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    center=(lo+hi)*.5
    target=unity_to_blender(spec["anchor"])
    root.location += Vector((target.x-center.x,target.y-center.y,target.z-lo.z))
    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    tris=0
    for o in meshes:
        for p in o.data.polygons:tris+=max(0,len(p.vertices)-2)
    return {
        "id":spec["id"],"name":spec["name"],"tris":tris,
        "bounds":{"min":[round(v,3) for v in lo],"max":[round(v,3) for v in hi]},
        "download_url":entry["url"]
    }

report={"method":"full_width_cc0_photogrammetry_midground","powered_by":"Poly Haven","license":"CC0","assets":[]}
for spec in ASSETS:
    report["assets"].append(import_and_place(spec))

# Keep the scan materials/textures. No generated material replacement.
for o in bpy.context.scene.objects:
    if o.type=="MESH":
        o.select_set(True)
        for poly in o.data.polygons:poly.use_smooth=True

idx=sys.argv.index("--") if "--" in sys.argv else -1
out=Path(sys.argv[idx+1] if idx>=0 else "ValoriaPhotogrammetryMidground.glb").resolve()
report_path=Path(sys.argv[idx+2] if idx>=0 and len(sys.argv)>idx+2 else str(out.with_suffix(".json"))).resolve()
out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",export_apply=True,export_materials="EXPORT",export_yup=True)
report["output_bytes"]=out.stat().st_size
report["total_source_tris"]=sum(a["tris"] for a in report["assets"])
report_path.write_text(json.dumps(report,indent=2),encoding="utf-8")
print("VALORIA_PHOTOGRAMMETRY_MIDGROUND_BUILT",json.dumps(report))
