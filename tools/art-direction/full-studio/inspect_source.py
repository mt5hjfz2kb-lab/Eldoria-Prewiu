"""Read-only inspection and genuine preview of an existing reusable Blender asset.

No authored geometry or source file changes. Qwen receives real object names,
dimensions, materials and mesh provenance instead of imaginary topology.
"""
import argparse,hashlib,json,pathlib,sys
import bpy

HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
from blender_toolbox import camera_override,render_setup,ensure_review_lighting,ensure_review_stage,snapshot_meshes

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
        "target":[0,0,2],"ortho_scale":19}))
    ensure_review_lighting()
    ensure_review_stage()
    meshes=snapshot_meshes()
    inventory=[]
    for o in sorted((o for o in bpy.data.objects if o.type=="MESH" and not o.name.startswith("_EldoriaReview")),key=lambda x:x.name):
        verts=o.data.vertices
        lo=[round(min((float(v.co[i]) for v in verts),default=0),5) for i in range(3)]
        hi=[round(max((float(v.co[i]) for v in verts),default=0),5) for i in range(3)]
        local_extrema={}
        if len(verts):
            for axis,letter in enumerate("xyz"):
                for kind,selector in (("min",min),("max",max)):
                    vertex=selector(verts,key=lambda v:float(v.co[axis]))
                    local_extrema[letter+"_"+kind]={"vertex_index":vertex.index,
                      "local_position_m":[round(float(v),5) for v in vertex.co]}
        # Neutral read-only topology sampling: source facts, NOT art instructions.
        # Spread landmarks across measured coordinates so Qwen can pick brushes
        # and face IDs without guessing numbers from a photograph.
        sampled={}
        for axis in range(3):
            ordered=sorted(verts,key=lambda v:float(v.co[axis]))
            for frac in (0.12,0.32,0.52,0.72,0.92):
                if ordered:
                    v=ordered[round((len(ordered)-1)*frac)]
                    sampled[v.index]={"vertex_index":v.index,
                        "local_position_m":[round(float(t),5) for t in v.co]}
        vertex_landmarks=list(sampled.values())
        polys=o.data.polygons
        chosen={}
        if len(polys):
            for axis in range(3):
                ordered=sorted(polys,key=lambda f:float(f.center[axis]))
                for frac in (0.1,0.5,0.9):
                    p=ordered[round((len(ordered)-1)*frac)]
                    chosen[p.index]=p
            for face in sorted(polys,key=lambda f:float(f.area),reverse=True)[:5]:
                chosen[face.index]=face
        face_landmarks=[{"face_index":p.index,
            "local_center_m":[round(float(t),5) for t in p.center],
            "local_normal":[round(float(t),4) for t in p.normal]}
            for p in chosen.values()]
        inventory.append({
            "local_extrema":local_extrema,
            "vertex_landmarks":vertex_landmarks,
            "face_landmarks":face_landmarks,
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
