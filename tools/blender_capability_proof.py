import argparse, json, os, sys, shutil
import bpy

def args():
    argv=sys.argv
    argv=argv[argv.index("--")+1:] if "--" in argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output-dir", required=True)
    p.add_argument("--report", required=True)
    p.add_argument("--ratios", default="1.0,0.5,0.25")
    return p.parse_args(argv)

def clean():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes,bpy.data.materials,bpy.data.images,bpy.data.cameras,bpy.data.lights):
        pass

def meshes():
    return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]

def tris(o):
    o.data.calc_loop_triangles()
    return len(o.data.loop_triangles)

def metrics():
    ms=meshes()
    mats=set()
    uv=True
    verts=0
    tri=0
    for o in ms:
        verts+=len(o.data.vertices)
        tri+=tris(o)
        uv=uv and bool(o.data.uv_layers)
        for m in o.data.materials:
            if m: mats.add(m.name)
    return {"meshes":len(ms),"vertices":verts,"triangles":tri,"materials":len(mats),"uv_all_meshes":uv}

def import_glb(path):
    bpy.ops.import_scene.gltf(filepath=path)

def export_glb(path):
    os.makedirs(os.path.dirname(path),exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=False,export_apply=True)

def apply_ratio(ratio):
    if ratio >= 0.999999:
        return
    for o in meshes():
        before=tris(o)
        if before < 64: continue
        mod=o.modifiers.new(name="Eldoria_LOD_Decimate",type="DECIMATE")
        mod.decimate_type="COLLAPSE"
        mod.ratio=max(0.01,min(1.0,ratio))
        mod.use_collapse_triangulate=True
        bpy.context.view_layer.objects.active=o
        o.select_set(True)
        bpy.ops.object.modifier_apply(modifier=mod.name)
        o.select_set(False)

def consolidate_duplicate_material_slots():
    changes=[]
    for o in meshes():
        seen={}
        remap={}
        for i,m in enumerate(o.data.materials):
            if not m: continue
            key=m.name
            if key in seen: remap[i]=seen[key]
            else: seen[key]=i
        if remap:
            for poly in o.data.polygons:
                if poly.material_index in remap:
                    poly.material_index=remap[poly.material_index]
            for idx in sorted(remap.keys(), reverse=True):
                o.data.materials.pop(index=idx)
            changes.append({"object":o.name,"removed_duplicate_slots":len(remap)})
    return changes

a=args()
ratios=[float(x.strip()) for x in a.ratios.split(",") if x.strip()]
if not ratios: raise RuntimeError("No LOD ratios supplied")

clean()
import_glb(a.input)
baseline=metrics()
material_changes=consolidate_duplicate_material_slots()

tmp=os.path.join(a.output_dir,"_baseline.blend")
os.makedirs(a.output_dir,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=tmp)

results=[]
for idx,ratio in enumerate(ratios):
    bpy.ops.wm.open_mainfile(filepath=tmp)
    apply_ratio(ratio)
    after=metrics()
    out=os.path.join(a.output_dir,f"lod{idx}.glb")
    export_glb(out)
    results.append({
        "lod":idx,
        "ratio_requested":ratio,
        "metrics":after,
        "path":out,
        "bytes":os.path.getsize(out)
    })

try: os.remove(tmp)
except OSError: pass

report={
    "source":a.input,
    "baseline":baseline,
    "material_slot_consolidation":material_changes,
    "lods":results,
    "method":"deterministic Blender Decimate COLLAPSE; source duplicated from saved baseline before each ratio",
    "tripo_credits":0
}
with open(a.report,"w",encoding="utf-8") as f:
    json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
