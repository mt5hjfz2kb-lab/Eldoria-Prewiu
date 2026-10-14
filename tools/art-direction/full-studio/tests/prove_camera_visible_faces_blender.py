"""Native Blender proof: raycast identifies screen-visible faces only."""
import math,pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
import bpy
from camera_visible_source_faces import camera_visible_face_landmarks
from blender_toolbox import camera_override

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.mesh.primitive_cube_add(size=2)
cube=bpy.context.object
cube.name="Imported_Local_Y_Up"
cube.rotation_euler=(math.pi/2,0,0)
bpy.context.view_layer.update()
camera_override({"location":[6,-8,6],"target":[0,0,0],"ortho_scale":6,"projection":"ORTHO"})
actual=camera_visible_face_landmarks(cube,max_faces=20)
indices={p["face_index"] for p in actual}
assert actual, "Real fixed review camera must identify at least one face"
assert len(indices)<=3, "Backside faces may not be offered as visible"
assert all(0<=f["screen_xy"][0]<=1 and 0<=f["screen_xy"][1]<=1 for f in actual)
assert all(0<=f["face_index"]<len(cube.data.polygons) for f in actual)
print("ELDORIA_CAMERA_VISIBLE_FACE_RAYCAST_PASS",sorted(indices))
