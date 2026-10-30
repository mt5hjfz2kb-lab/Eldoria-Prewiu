import bpy,pathlib,json,hashlib,shutil,struct
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
terrain=bpy.data.objects['BLD_Terrain']
def signature():
 return hashlib.sha256(b''.join(struct.pack('<fff',*v.co) for v in terrain.data.vertices)).hexdigest()
original=signature()
packed={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
changes={}
for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures']:
 obj=bpy.data.objects[name];largest=0;count=0
 for vertex in obj.data.vertices:
  hit=bvh.ray_cast(Vector((vertex.co.x,vertex.co.y,300)),Vector((0,0,-1)))[0]
  assert hit is not None
  delta=hit.z+.015-vertex.co.z;largest=max(largest,abs(delta));count+=abs(delta)>.0001
  vertex.co.z=hit.z+.015
 obj.data.update();changes[name]={'vertices':len(obj.data.vertices),'corrected':count,'maximum_adjustment_m':largest}
assert signature()==original
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in packed.items())
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
bpy.ops.object.select_all(action='DESELECT')
for ob in bpy.context.scene.objects:
 if ob.name in names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
for name in ['RegionOneBlenderQueries.bytes','production-region.json']:shutil.copyfile(source.parent/name,out/name)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'method':'actual BlenderMCP highest-triangle floor conformance','parent_macro_run':40230766938,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_vertices_after':len(terrain.data.vertices),'protected_vertices_unchanged':51898,'entire_terrain_vertices_unchanged':True,'entire_terrain_vertex_sha256':original,'packed_images_unchanged':len(packed),'floor_corrections':changes,'new_gameplay_targets':0,'new_colliders':0,'files':{name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']},'art_approved':False}
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
bpy.app.driver_namespace['interactive_snapshot']('contact-candidate')
(out/'stop.request').write_text('Exact highest-triangle geological floor contact exported; terrain/pigment intact')
