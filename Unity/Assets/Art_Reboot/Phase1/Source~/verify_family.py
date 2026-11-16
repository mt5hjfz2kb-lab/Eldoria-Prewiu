import bpy,json,hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1]
reports=[]
for name in ['ruin','corrupt','environment']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(root/('Models/Reboot_'+name.capitalize()+'.fbx')))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 assert meshes
 tri=sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons)
 source=json.loads((root/('Source~/Family/'+name+'.json')).read_text())
 assert tri==source['triangles']
 assert all(len(o.data.uv_layers)>0 and len(o.data.color_attributes)>0 and not o.data.validate(clean_customdata=False) for o in meshes)
 assert source['approved_hero_fbx_sha256']==hashlib.sha256((root/'Models/Reboot_Hero.fbx').read_bytes()).hexdigest()
 reports.append(dict(mode=name,triangles=tri,uv_color_valid=True))
(root/'Source~/Family/fbx-validation.json').write_text(json.dumps(reports,indent=2))
print('FAMILY_FBX_PASS',reports)
