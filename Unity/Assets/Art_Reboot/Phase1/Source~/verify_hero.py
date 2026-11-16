import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(root/'Models/Reboot_Hero.fbx'))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert len(meshes)==1
m=meshes[0]
assert not m.data.validate(clean_customdata=False)
tri=sum(len(p.vertices)-2 for p in m.data.polygons)
expected=json.loads((root/'Source~/HeroReview/geometry.json').read_text())['triangles']
assert tri==expected,(tri,expected)
assert len(m.data.uv_layers)>0 and len(m.data.color_attributes)>0
assert 7<m.dimensions.z<10,tuple(m.dimensions)
report={'fbx_triangles':tri,'dimensions':list(m.dimensions),'uv_layers':len(m.data.uv_layers),'color_attributes':len(m.data.color_attributes),'unity_verified':False}
(root/'Source~/HeroReview/fbx-validation.json').write_text(json.dumps(report,indent=2))
print('HERO_FBX_PASS',report)
