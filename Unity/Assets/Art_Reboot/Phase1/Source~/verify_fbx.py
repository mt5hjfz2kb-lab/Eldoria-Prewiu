"""Read-only Blender FBX reimport. Does not save or rewrite source art."""
from pathlib import Path
import bpy, json, math
root=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(root/'Models/Reboot_Pilot.fbx'))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
expected={'Terrain','01_Reclaimed_Hall','02_Roofless_Ruin','03_Corrupted_Watchtower'}
assert {o.name for o in objects}==expected
triangles=0
for obj in objects:
 mesh=obj.data
 assert len(mesh.uv_layers)>0 and len(mesh.color_attributes)>0
 assert all(math.isfinite(c) for v in mesh.vertices for c in v.co)
 for uv in mesh.uv_layers.active.data:
  u,v=uv.uv
  assert .023-1e-5 <= u <= .977+1e-5 and .023-1e-5 <= v <= .977+1e-5
 # Every face must stay inside one atlas swatch with padding, never sample the whole sheet.
 for face in mesh.polygons:
  assert len(face.vertices)==3, (obj.name, face.index, "FBX must be pre-triangulated")
  tiles={(int(mesh.uv_layers.active.data[j].uv.x*4),int(mesh.uv_layers.active.data[j].uv.y*4)) for j in face.loop_indices}
  assert len(tiles)==1, (obj.name,face.index,tiles)
  triangles+=len(face.vertices)-2
expected_triangles=json.loads((root/'Evidence/geometry.json').read_text())['triangles']
assert triangles==expected_triangles,(triangles,expected_triangles)
materials={m.name for o in objects for m in o.data.materials}
assert materials=={'Reboot_Atlas','Reboot_Light','Reboot_Corruption'},materials
print(json.dumps({'status':'BLENDER_FBX_REIMPORT_PASS','meshes':len(objects),'triangles':triangles,'uv_face_in_single_atlas_tile':True,'vertex_colors':True,'unity_tested':False},indent=2))
