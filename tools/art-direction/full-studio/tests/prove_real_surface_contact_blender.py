"""Native Blender proof: AABB overlap does NOT imply actual source surface contact.

One inherited mesh has two separated cuboid islands in the SAME object's
overall bounding box. A new object inside the AABB gap must be rejected,
while an original touching object must be treated as geometrically attached.
"""
import pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
import bpy
from blender_surface_contact import actual_inherited_surface_contact

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.mesh.primitive_cube_add(size=1,location=(-2,0,0))
a=bpy.context.object;a.name="Source"
bpy.ops.mesh.primitive_cube_add(size=1,location=(2,0,0))
b=bpy.context.object
bpy.ops.object.select_all(action="DESELECT")
a.select_set(True);b.select_set(True)
bpy.context.view_layer.objects.active=a
bpy.ops.object.join()
a.name="Source"
bpy.context.view_layer.update()
original=[tuple(v.co) for v in a.data.vertices]
# The new beam is inside the COMBINED bounding box but a long way from BOTH
# inherited real mesh island surfaces.
bpy.ops.mesh.primitive_cube_add(size=.2,location=(0,0,0))
floating=bpy.context.object;floating.name="QwenFloating"
bpy.context.view_layer.update()
bad=actual_inherited_surface_contact({"Source"})
assert bad["verified"],bad
assert not bad["actual_surface_contact_all_new_components"],bad
distance=bad["objects"][0]["min_real_mesh_surface_distance_m"]
assert distance >1.2,(distance,bad)
bpy.data.objects.remove(floating,do_unlink=True)
bpy.ops.mesh.primitive_cube_add(size=.2,location=(-2,0,.58))
touching=bpy.context.object;touching.name="QwenActualTouching"
bpy.context.view_layer.update()
good=actual_inherited_surface_contact({"Source"})
assert good["actual_surface_contact_all_new_components"],good
assert good["objects"][0]["min_real_mesh_surface_distance_m"]<=.07,good
assert [tuple(v.co) for v in a.data.vertices]==original
assert not good["art_approved"] and good["no_geometry_modified"],good
print("ELDORIA_REAL_BLENDER_BVH_SOURCE_CONTACT_PASS",distance,
      good["objects"][0]["min_real_mesh_surface_distance_m"])
