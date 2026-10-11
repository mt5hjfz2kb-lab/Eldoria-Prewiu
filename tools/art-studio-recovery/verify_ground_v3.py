"""Round-trip the GLB into the source lighting and keep a matched review board."""
import bpy
from pathlib import Path
from mathutils import Vector

root = Path(__file__).resolve().parents[2]
out = root / 'art-source/valoria/production/studio-rocks-v3-ground'
bpy.ops.wm.open_mainfile(filepath=str(out / 'ValoriaRockAndGroundV3.blend'))
for obj in list(bpy.data.objects):
    if obj.type == 'MESH' and (obj.name.startswith('CC0 ') or obj.name.startswith('Buried continuous') or obj.name.startswith('Valoria Highlands')):
        bpy.data.objects.remove(obj, do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(out / 'ValoriaRockAndGroundV3-ENV.glb'))
imported = set(bpy.context.selected_objects)
grass = list({m for obj in imported for m in obj.data.materials if m and m.name.startswith('Grass lichen moss ')})
assert len(grass) == 4, [m.name for m in grass]
assert all(m.node_tree and any(n.type == 'TEX_IMAGE' and n.image for n in m.node_tree.nodes) for m in grass)
scene = bpy.context.scene
cam = scene.camera
cam.location = (50, -70, 49)
cam.rotation_euler = (Vector((0, 4, 0)) - cam.location).to_track_quat('-Z', 'Y').to_euler()
cam.data.ortho_scale = 108
scene.render.resolution_x = 1280
scene.render.resolution_y = 800
scene.render.filepath = str(out / 'roundtrip-overview.png')
bpy.ops.render.render(write_still=True)
print('GLB round-trip grass materials:', len(grass))
