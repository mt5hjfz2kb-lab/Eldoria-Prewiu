import argparse, json, os, sys
import bpy

def args():
    a=sys.argv
    a=a[a.index("--")+1:] if "--" in a else []
    p=argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--report", required=True)
    p.add_argument("--min-component-ratio", type=float, default=0.08)
    p.add_argument("--min-component-triangles", type=int, default=120)
    return p.parse_args(a)

def tris(o):
    o.data.calc_loop_triangles()
    return len(o.data.loop_triangles)

def import_glb(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    objs=[o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]
    if not objs: raise RuntimeError("No mesh in input")
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    if len(objs)>1: bpy.ops.object.join()
    return bpy.context.view_layer.objects.active

def split_loose(obj):
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.separate(type="LOOSE")
    bpy.ops.object.mode_set(mode="OBJECT")
    return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]

def main():
    a=args()
    obj=import_glb(a.input)
    before=tris(obj)
    parts=split_loose(obj)
    rows=sorted([{"obj":o,"tris":tris(o)} for o in parts], key=lambda x:x["tris"], reverse=True)
    largest=rows[0]["tris"]
    threshold=max(a.min_component_triangles, int(round(largest*a.min_component_ratio)))
    kept=[]; removed=[]
    for r in rows:
        item={"name":r["obj"].name,"triangles":r["tris"]}
        if r["tris"]>=threshold:
            kept.append(r); item["action"]="keep"
        else:
            removed.append(r); item["action"]="remove"
        item["threshold"]=threshold
    if not kept: raise RuntimeError("Salvage removed every component")
    for r in removed: bpy.data.objects.remove(r["obj"], do_unlink=True)
    bpy.ops.object.select_all(action="DESELECT")
    for r in kept: r["obj"].select_set(True)
    bpy.context.view_layer.objects.active=kept[0]["obj"]
    if len(kept)>1: bpy.ops.object.join()
    out=bpy.context.view_layer.objects.active
    out.name="Eldoria_Salvaged_Module"
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    after=tris(out)
    os.makedirs(os.path.dirname(a.output),exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=a.output,export_format="GLB",export_apply=True,export_materials="EXPORT",export_yup=True,use_selection=True)
    report={
      "schema_version":1,"input":os.path.basename(a.input),"triangles_before":before,
      "triangles_after":after,"component_count_before":len(rows),"kept_components":len(kept),
      "removed_components":len(removed),"threshold_triangles":threshold,
      "removed_triangles":sum(r["tris"] for r in removed),
      "components":[{"triangles":r["tris"],"action":("keep" if r in kept else "remove")} for r in rows],
      "policy":"remove disconnected components below max(min_component_triangles, largest_component*min_component_ratio); never modify connected dominant geometry"
    }
    with open(a.report,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
    print(json.dumps(report,indent=2))

if __name__=="__main__": main()
