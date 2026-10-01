import argparse, json, os, sys
import bpy

def parse():
    argv=sys.argv
    argv=argv[argv.index("--")+1:] if "--" in argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--input",required=True)
    p.add_argument("--output",required=True)
    p.add_argument("--report",required=True)
    p.add_argument("--ratio",type=float,default=.35)
    p.add_argument("--size",type=int,default=1024)
    return p.parse_args(argv)

def mesh_objects():
    return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]

def tri_count(o):
    o.data.calc_loop_triangles()
    return len(o.data.loop_triangles)

def join_objects(objs,name):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.object.join()
    objs[0].name=name
    return objs[0]

def metrics(o):
    return {
      "vertices":len(o.data.vertices),
      "triangles":tri_count(o),
      "uv":bool(o.data.uv_layers),
      "materials":len([m for m in o.data.materials if m])
    }

a=parse()
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=a.input)
srcs=mesh_objects()
if not srcs: raise RuntimeError("No mesh imported")
source=join_objects(srcs,"BakeSourceHigh")
source_metrics=metrics(source)

high=source.copy(); high.data=source.data.copy(); bpy.context.collection.objects.link(high); high.name="BakeHigh"
low=source.copy(); low.data=source.data.copy(); bpy.context.collection.objects.link(low); low.name="BakeLow"
bpy.data.objects.remove(source,do_unlink=True)

# Simplify target.
bpy.context.view_layer.objects.active=low
low.select_set(True)
mod=low.modifiers.new("Eldoria_BakeLow_Decimate","DECIMATE")
mod.decimate_type="COLLAPSE"
mod.ratio=max(.02,min(1.0,a.ratio))
mod.use_collapse_triangulate=True
bpy.ops.object.modifier_apply(modifier=mod.name)
low.select_set(False)

# Fresh UVs make the proof deterministic and avoid overlapping islands from imported assets.
bpy.context.view_layer.objects.active=low
low.select_set(True)
bpy.ops.object.mode_set(mode="EDIT")
bpy.ops.mesh.select_all(action="SELECT")
if not low.data.uv_layers: low.data.uv_layers.new(name="UVMap")
bpy.ops.uv.smart_project(angle_limit=1.15192,island_margin=.03)
bpy.ops.object.mode_set(mode="OBJECT")
low.select_set(False)

# Neutral test material with baked tangent-space normal.
mat=bpy.data.materials.new("Eldoria_BakeProof")
mat.use_nodes=True
nodes=mat.node_tree.nodes
links=mat.node_tree.links
for n in list(nodes): nodes.remove(n)
out=nodes.new("ShaderNodeOutputMaterial")
bsdf=nodes.new("ShaderNodeBsdfPrincipled")
bsdf.inputs["Base Color"].default_value=(.45,.43,.39,1)
bsdf.inputs["Roughness"].default_value=.78
tex=nodes.new("ShaderNodeTexImage")
img=bpy.data.images.new("Eldoria_BakedNormal",width=a.size,height=a.size,alpha=False,float_buffer=False)
img.colorspace_settings.name="Non-Color"
tex.image=img
normal=nodes.new("ShaderNodeNormalMap")
normal.inputs["Strength"].default_value=1.0
links.new(tex.outputs["Color"],normal.inputs["Color"])
links.new(normal.outputs["Normal"],bsdf.inputs["Normal"])
links.new(bsdf.outputs["BSDF"],out.inputs["Surface"])
low.data.materials.clear(); low.data.materials.append(mat)

# Bake source -> low.
bpy.context.scene.render.engine="BLENDER_EEVEE_NEXT"
bpy.ops.object.select_all(action="DESELECT")
high.select_set(True); low.select_set(True)
bpy.context.view_layer.objects.active=low
for n in mat.node_tree.nodes: n.select=False
tex.select=True; mat.node_tree.nodes.active=tex
bpy.context.scene.render.bake.use_selected_to_active=True
bpy.context.scene.render.bake.cage_extrusion=.08
bpy.context.scene.render.bake.max_ray_distance=.16
bpy.context.scene.render.bake.normal_space="TANGENT"
bpy.ops.object.bake(type="NORMAL",use_clear=True,margin=8)

# Pack baked texture and remove high before export.
img.pack()
bpy.data.objects.remove(high,do_unlink=True)
os.makedirs(os.path.dirname(a.output),exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
low.select_set(True); bpy.context.view_layer.objects.active=low
bpy.ops.export_scene.gltf(filepath=a.output,export_format="GLB",use_selection=True,export_apply=True)

report={
  "source":a.input,
  "source_metrics":source_metrics,
  "low_metrics":metrics(low),
  "ratio_requested":a.ratio,
  "texture_size":a.size,
  "baked_normal_packed":bool(img.packed_file),
  "output_bytes":os.path.getsize(a.output),
  "tripo_credits":0
}
with open(a.report,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
