"""Read-only inspection and genuine preview of an existing reusable Blender asset.

No authored geometry or source file changes. Qwen receives real object names,
dimensions, materials and mesh provenance instead of imaginary topology.
"""
import argparse,hashlib,json,pathlib,sys
import bpy

HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
from blender_toolbox import camera_override,render_setup,ensure_review_lighting,snapshot_meshes

def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--source",type=pathlib.Path,required=True)
    parser.add_argument("--out",type=pathlib.Path,required=True)
    parser.add_argument("--review",type=pathlib.Path)
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])
    if not args.source.is_file():raise FileNotFoundError("Frozen source not found")
    if args.out.exists() and any(args.out.iterdir()):raise ValueError("Fresh inspection folder required")
    args.out.mkdir(parents=True,exist_ok=True)
    source_hash=digest(args.source)
    if args.source.suffix.lower()==".blend":
        bpy.ops.wm.open_mainfile(filepath=str(args.source.resolve()))
    elif args.source.suffix.lower() in (".glb",".gltf"):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.gltf(filepath=str(args.source.resolve()))
    else:raise ValueError("Only original .blend/.glb/.gltf assets supported")
    review=json.loads(args.review.read_text()) if args.review else {}
    render_setup(review.get("render",{"samples":8,"width":960,"height":720}))
    camera_override(review.get("camera",{"location":[11,-18,14],
        "rotation":[1.012291,0,.541052],"ortho_scale":22}))
    ensure_review_lighting()
    meshes=snapshot_meshes()
    inventory=[]
    for o in sorted((o for o in bpy.data.objects if o.type=="MESH"),key=lambda x:x.name):
        verts=o.data.vertices
        lo=[round(min((float(v.co[i]) for v in verts),default=0),5) for i in range(3)]
        hi=[round(max((float(v.co[i]) for v in verts),default=0),5) for i in range(3)]
        inventory.append({
            "name":o.name,"vertices":len(verts),"faces":len(o.data.polygons),
            "local_bounds_min_m":lo,"local_bounds_max_m":hi,
            "world_location_m":[round(float(v),5) for v in o.location],
            "world_dimensions_m":[round(float(v),5) for v in o.dimensions],
            "materials":[m.name if m else None for m in o.data.materials],
            "uv_layers":[l.name for l in o.data.uv_layers],
            "geometry_sha256":meshes[o.name]})
    image=args.out/"source-preview-blender.png"
    bpy.context.scene.render.filepath=str(image.resolve())
    bpy.ops.render.render(write_still=True)
    if digest(args.source)!=source_hash:raise RuntimeError("Original source was modified")
    proof={"schema_version":1,"source_sha256":source_hash,
           "source_unchanged":True,"image_kind":"REAL_BLENDER_NOT_UNITY",
           "source_preview_sha256":digest(image),"asset_count":len(inventory),
           "objects":inventory,"art_approved":False}
    (args.out/"source-inspection.json").write_text(json.dumps(proof,indent=2)+"\n")
    print("ELDORIA_FULL_STUDIO_REAL_SOURCE_INSPECTED",len(inventory),source_hash)
if __name__=="__main__":main()
