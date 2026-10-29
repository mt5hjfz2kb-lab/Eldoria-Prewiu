"""Reuse Eldoria's canonical Blender reduction for a visually reviewed source."""
import bpy,sys,json,importlib.util
from pathlib import Path

source,dest,target,canonical=sys.argv[sys.argv.index('--')+1:]
dest=Path(dest);dest.mkdir(parents=True,exist_ok=True)
spec=importlib.util.spec_from_file_location('canonical_tripo',canonical)
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=source)
before=module.metrics('raw')
if not before['uv_present_all_meshes']:raise RuntimeError('Missing source UVs; stop rather than hide loss')
target=min(int(target),before['triangles'])
if before['triangles']>target:module.apply_decimation_once(target)
after=module.metrics('optimized')
if after['triangles']>target+500:raise RuntimeError('Reduction did not reach its provisional budget')
for ob in module.mesh_objects():
    bpy.context.view_layer.objects.active=ob
    ob.select_set(True)
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    ob.select_set(False)
resized=module.optimize_images()
bpy.ops.file.pack_all()
bpy.ops.export_scene.gltf(filepath=str(dest/'optimized.glb'),export_format='GLB',export_apply=True,export_materials='EXPORT',export_yup=True)
bpy.ops.wm.save_as_mainfile(filepath=str(dest/'editable.blend'))
(dest/'optimization.json').write_text(json.dumps({'raw':before,'optimized':after,'resized_images':resized,'method':'Reused Eldoria canonical apply_decimation_once and optimize_images; source geometry/materials preserved, no replacement art','mobile_profiled':False,'unity_validated':False,'rigged':False},indent=2))
