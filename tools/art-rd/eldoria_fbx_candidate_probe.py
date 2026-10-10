import bpy, json, os
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
out=Path(os.environ.get('ELDORIA_ASSET_MAP','/tmp/eldoria-asset-map.json'))
out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
source=root/'Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb'
bpy.ops.import_scene.gltf(filepath=str(source))
def describe(ob):
 if ob.type!='MESH':return None
 bbox=[ob.matrix_world @ Vector(v) for v in ob.bound_box]
 return {'name':ob.name,'triangles':sum(len(p.vertices)-2 for p in ob.data.polygons),
 'materials':[m.name for m in ob.data.materials if m],
 'bounds':[[round(min(v[i] for v in bbox),2),round(max(v[i] for v in bbox),2)] for i in range(3)]}
world=[describe(x) for x in bpy.data.objects if x.type=='MESH']
available=[]
mesh_dir=root/'Unity/Assets/EmaceArt/Slavic World Free/Meshes'
names=['EA03_Town_Building_Administrative _01a.fbx','EA03_Village_OutBuilding_Shed_01a.fbx','EA03_Village_OutBuilding_Shed_03b.fbx','EA03_Village_OutBuilding_Cubby_01d.fbx']
for name in names:
 path=mesh_dir/name
 if not path.exists():
  available.append({'source':name,'error':'missing'})
  continue
 before=set(bpy.data.objects)
 try:
  bpy.ops.import_scene.fbx(filepath=str(path))
  meshes=[describe(x) for x in bpy.data.objects if x not in before and x.type=='MESH']
  available.append({'source':name,'meshes':meshes})
 except Exception as ex: available.append({'source':name,'error':str(ex)})
report={'source_world_meshes':world,'candidate_details':available,'import_only':True,'production_pass':False}
out.write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf8')
print('ELDORIA_CURATED_ASSET_PROBE_PASS world_meshes=',len(world),'candidates=',len(available))
