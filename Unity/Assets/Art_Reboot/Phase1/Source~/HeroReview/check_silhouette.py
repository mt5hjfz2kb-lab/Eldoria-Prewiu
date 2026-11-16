"""Actual selected 3D silhouette at game-equivalent angle, not a Unity render."""
import bpy
from pathlib import Path
p=Path(__file__).parent
bpy.ops.wm.open_mainfile(filepath=str(p/'Reboot_Hero.blend'))
mat=bpy.data.materials.new('Solid black silhouette');mat.use_nodes=True
n=mat.node_tree.nodes;n.clear();e=n.new('ShaderNodeEmission');e.inputs['Color'].default_value=(0,0,0,1)
o=n.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(e.outputs[0],o.inputs['Surface'])
for ob in bpy.context.scene.objects:
 if ob.type=='MESH':
  for i in range(len(ob.data.materials)):ob.data.materials[i]=mat
scene=bpy.context.scene;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(1,1,1,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=1
scene.cycles.samples=1;scene.cycles.use_denoising=False;scene.view_settings.view_transform='Standard'
scene.render.resolution_x=660;scene.render.resolution_y=750
scene.render.filepath=str(p/'hero-actual-silhouette.png');bpy.ops.render.render(write_still=True)
