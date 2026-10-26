"""Remove inherited flat corner normals from the locally rebuilt geological patch.
Terrain positions, winding, UVs, images and gameplay query payload stay exact.
"""
import bpy,pathlib,json,hashlib,struct,shutil
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source']);scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain'];positions=[tuple(v.co) for v in terrain.data.vertices]
images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
changes={}
for ob in [terrain]+[bpy.data.objects[name] for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures','BLD_BreachBrokenBasaltPlates','BLD_BreachTerracedStrata','BLD_BreachWeatheredRockfall']]:
 mesh=ob.data;normals=[tuple(n.vector) for n in mesh.corner_normals];count=0
 for face in mesh.polygons:
  if ob==terrain and not all(-470.001<mesh.vertices[i].co.x<-97.999 and -20.001<mesh.vertices[i].co.y<320.001 for i in face.vertices):continue
  face.use_smooth=True;count+=1
  for li in face.loop_indices:normals[li]=(0,0,0)
 mesh.update();mesh.normals_split_custom_set(normals);mesh.update();changes[ob.name]=count
assert positions==[tuple(v.co) for v in terrain.data.vertices]
assert all(hashlib.sha256(bpy.data.images[k].packed_file.data).hexdigest()==v for k,v in images.items())
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
for name in ['RegionOneBlenderQueries.bytes','production-region.json']:shutil.copyfile(source.parent/name,out/name)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report=json.loads((source.parent/'macro-art-operations.json').read_text(encoding='utf-8-sig'));report['smooth_geology_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();report['inherited_flat_corner_normals_removed']=changes
report['files']={name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']}
report['blend_bytes']=(out/'Eldoria_Region1_Production.blend').stat().st_size
assert report['blend_bytes']<100*1024*1024
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
(out/'stop.request').write_text('Smoothed inherited flat geological corner normals; original terrain positions and query payload unchanged.')
