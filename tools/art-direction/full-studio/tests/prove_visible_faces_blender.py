"""Real Blender proof that camera face samples are ray-visible (not random indexes)."""
import pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
import bpy
from blender_toolbox import camera_override,render_setup
from camera_visible_faces import camera_visible_face_landmarks

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.mesh.primitive_cube_add(size=2,location=(0,0,1))
cube=bpy.context.object;cube.name="Camera-test source cube"
render_setup({"samples":1,"width":960,"height":720})
camera_override({"location":[6,-7,5],"target":[0,0,1],"ortho_scale":6,
                 "projection":"ORTHO"})
bpy.context.view_layer.update()
before=[tuple(v.co) for v in cube.data.vertices]
visible=camera_visible_face_landmarks(bpy.context.scene,cube,max_faces=6)
assert visible, "Actual source cube has at least one review-camera-visible polygon"
assert all(x["camera_visible"] and x["camera_ray_hit_original_face"] for x in visible)
assert all(0<=x["face_index"]<len(cube.data.polygons) for x in visible)
assert len(set(x["face_index"] for x in visible))==len(visible)
assert all(0<=x["camera_screen_uv"][i]<=1 for x in visible for i in (0,1))
assert [tuple(v.co) for v in cube.data.vertices]==before
print("ELDORIA_REAL_BLENDER_CAMERA_FACE_RAYCAST_PASS",len(visible),
      [x["face_index"] for x in visible])
