"""Native Blender bridge: import only verified sprite as camera-facing textured plane."""
import bpy, pathlib, sys, json, math
from mathutils import Vector
p=pathlib.Path(sys.argv[sys.argv.index("--")+1]).resolve()
src=p/"candidate-trimmed.png"
if not src.is_file(): raise RuntimeError("No validated candidate")
bpy.ops.wm.read_factory_settings(use_empty=True)
image=bpy.data.images.load(str(src)); image.pack()
w,h=image.size
if not w or not h: raise RuntimeError("Invalid image")
bpy.ops.mesh.primitive_plane_add(size=2)
plane=bpy.context.object;plane.name="Eldoria_Lantern_Candidate_Not_Approved"
plane.scale=(w/h,1,1)
mat=bpy.data.materials.new("Eldoria_Candidate_RGBA");mat.use_nodes=True
nodes=mat.node_tree.nodes;links=mat.node_tree.links
nodes.clear(); tex=nodes.new("ShaderNodeTexImage");tex.image=image
em=nodes.new("ShaderNodeEmission");trans=nodes.new("ShaderNodeBsdfTransparent")
mix=nodes.new("ShaderNodeMixShader");output=nodes.new("ShaderNodeOutputMaterial")
links.new(tex.outputs["Color"],em.inputs["Color"]);links.new(tex.outputs["Alpha"],mix.inputs[0])
links.new(trans.outputs[0],mix.inputs[1]);links.new(em.outputs[0],mix.inputs[2]);links.new(mix.outputs[0],output.inputs["Surface"])
plane.data.materials.append(mat)
bpy.ops.wm.save_as_mainfile(filepath=str(p/"candidate.blend"))
assert pathlib.Path(p/"candidate.blend").stat().st_size>1000 and image.packed_file
(p/"blender-proof.json").write_text(json.dumps({"blender_version":bpy.app.version_string,"mesh":plane.name,"packed_texture":True,"image_size":[w,h],"native_saved":True,"art_approved":False,"unity_imported":False},indent=2))
print("BLENDER_NATIVE_CANDIDATE_SAVED",bpy.app.version_string)
