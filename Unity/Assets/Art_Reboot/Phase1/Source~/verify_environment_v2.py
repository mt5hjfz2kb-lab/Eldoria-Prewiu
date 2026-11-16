import bpy,json,hashlib
from pathlib import Path
r=Path(__file__).resolve().parents[1]
s=json.loads((r/'Source~/EnvironmentReview/environmentv2.json').read_text())
for n,h in s['approved_buildings_sha256'].items():assert hashlib.sha256((r/('Models/Reboot_'+n+'.fbx')).read_bytes()).hexdigest()==h
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(r/'Models/Reboot_EnvironmentV2.fbx'))
mm=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert sum(len(p.vertices)-2 for o in mm for p in o.data.polygons)==s['triangles']
assert all(len(o.data.uv_layers) and len(o.data.color_attributes) and not o.data.validate(clean_customdata=False) for o in mm)
print('ENVIRONMENT_FBX_PASS',s['triangles'],s['approved_buildings_sha256'])
