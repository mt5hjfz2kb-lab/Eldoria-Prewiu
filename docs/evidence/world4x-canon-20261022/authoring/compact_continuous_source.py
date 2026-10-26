import bpy,pathlib,hashlib,json,shutil
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
source_images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
for image in bpy.data.images:
 if image.packed_file:image.use_fake_user=True
removed=[]
for mesh in list(bpy.data.meshes):
 if not any(ob.type=='MESH' and ob.data==mesh for ob in bpy.data.objects):removed.append(mesh.name);bpy.data.meshes.remove(mesh)
for name in ['RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']:shutil.copyfile(source.parent/name,out/name)
report=json.loads((source.parent/'macro-art-operations.json').read_text(encoding='utf-8-sig'))
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in source_images.items())
report['compacted_unused_modelling_meshes']=removed;report['compaction_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();report['files']={name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']};report['blend_bytes']=(out/'Eldoria_Region1_Production.blend').stat().st_size;(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
assert report['blend_bytes']<100*1024*1024,report['blend_bytes']
(out/'stop.request').write_text('Exact reviewed geometry preserved; unused modelling buffers removed from editable source')
