# Blender-only zero-credit reduction of the existing optimized Tripo boar.
# Call: blender -b --python make_boar_far_lod.py -- INPUT.glb OUT.glb SOURCE.blend
import bpy,sys,pathlib,hashlib,json
args=sys.argv[sys.argv.index('--')+1:]
if len(args)!=3:raise RuntimeError('Usage: original near source, actual far GLB, editable blend')
source,target,editable=map(pathlib.Path,args)
if not source.is_file():raise FileNotFoundError(source)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(source))
mesh_objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
if not mesh_objects:raise RuntimeError('Original Tripo boar contains no mesh')
initial=sum(len(obj.data.polygons) for obj in mesh_objects)
for obj in mesh_objects:
 bpy.context.view_layer.objects.active=obj
 bpy.ops.object.select_all(action='DESELECT')
 obj.select_set(True)
 modifier=obj.modifiers.new(name='Mobile far LOD from approved Tripo source',type='DECIMATE')
 modifier.ratio=.10
 bpy.ops.object.modifier_apply(modifier=modifier.name)
final=sum(len(obj.data.polygons) for obj in mesh_objects)
if final>max(1,int(initial*.18)) or final<100:raise RuntimeError('Tripo LOD polygon guard failed')
target.parent.mkdir(parents=True,exist_ok=True)
editable.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(editable),compress=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,
    export_texcoords=True,export_normals=True,export_materials='EXPORT')
if target.stat().st_size<100000:raise RuntimeError('Boar far model GLB unexpectedly small')
print('ORIGINAL_TRIPO_BOAR_FAR_LOD',json.dumps({'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'original_triangles':initial,'far_triangles':final,'original_untouched':True,'tripo_credits_used':0,'editable_source':str(editable)},sort_keys=True))
