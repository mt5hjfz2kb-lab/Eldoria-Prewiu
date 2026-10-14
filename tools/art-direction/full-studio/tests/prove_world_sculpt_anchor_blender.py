"""Run only inside Blender: local Y-up imported source must obey WORLD +Z."""
import math,pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
import bpy
from mathutils import Vector
from blender_toolbox import sculpt_anchor_world

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.mesh.primitive_cube_add()
o=bpy.context.object
o.name="Imported local Y-up tower"
o.rotation_euler=(math.pi/2,0,0)
o.scale=(.25,.3,.25)
bpy.context.view_layer.update()
anchor=max(o.data.vertices,key=lambda v:float(v.co.x))
before=o.matrix_world@anchor.co.copy()
prior={v.index:(o.matrix_world@v.co.copy()) for v in o.data.vertices}
count=sculpt_anchor_world(o,{"anchor":"x_max","direction":"+Z","distance":.18,"radius":.6})
bpy.context.view_layer.update()
after=o.matrix_world@anchor.co
assert count>0
assert abs((after-before).z-.18)<1e-5,(before,after)
assert abs((after-before).x)<1e-5 and abs((after-before).y)<1e-5,(before,after)
assert all(abs((o.matrix_world@v.co-prior[v.index]).x)<1e-5 for v in o.data.vertices)
print("ELDORIA_ROTATED_GLTF_WORLD_Z_SCULPT_PASS",count)
