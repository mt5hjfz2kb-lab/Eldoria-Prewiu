import bpy,pathlib,json,hashlib,struct,shutil,math
from mathutils import Vector,Matrix
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
def vertex_hash(ob):return hashlib.sha256(b''.join(struct.pack('<fff',*v.co) for v in ob.data.vertices)).hexdigest()
terrain=bpy.data.objects['BLD_Terrain'];fixed={name:vertex_hash(bpy.data.objects[name]) for name in ['BLD_Terrain','BLD_BreachFloor','BLD_BreachOutlyingFractures','BLD_BreachBrokenBasaltPlates']}
images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
water=[]
for name in ['BLD_AuthoredTributary_0','BLD_AuthoredTributary_1']:
 ob=bpy.data.objects[name];mesh=ob.data;points=[(mesh.vertices[i].co+mesh.vertices[i+1].co)*.5 for i in range(0,len(mesh.vertices),2)];lengths=[0.]
 for a,b in zip(points,points[1:]):lengths.append(lengths[-1]+(b-a).length)
 uv=mesh.uv_layers.active
 for loop in mesh.loops:
  index=loop.vertex_index;uv.data[loop.index].uv=(lengths[index//2]/220.,float(index%2))
 mesh.update();water.append({'name':name,'metres':lengths[-1],'mapping':'longitudinal current, cross-channel depth; no world-grid repetition'})
# Reuse the exact two approved close donors; build a shared strategic mesh at
# about 240 triangles rather than the previous 67-triangle collapsed crowns.
meshes={};replaced=0
for far in list(bpy.context.scene.objects):
 if not far.name.startswith('BLD_ForestFar_'):continue
 near=bpy.data.objects.get(far.name.replace('BLD_ForestFar_','BLD_ForestMass_',1));assert near
 key=near.data.as_pointer()
 if key not in meshes:
  data=near.data.copy();data.name=near.data.name+' strategic branch volume';temp=bpy.data.objects.new('Temporary strategic donor authoring',data);bpy.context.scene.collection.objects.link(temp)
  bpy.ops.object.select_all(action='DESELECT');temp.select_set(True);bpy.context.view_layer.objects.active=temp
  mod=temp.modifiers.new('Shared strategic branch silhouette','DECIMATE');mod.ratio=.23;mod.use_collapse_triangulate=True
  bpy.ops.object.modifier_apply(modifier=mod.name);data=temp.data
  meshes[key]=data;bpy.data.objects.remove(temp,do_unlink=True)
 far.data.use_fake_user=True;far.data=meshes[key];replaced+=1
assert len(meshes)==2 and replaced>4000
assert all(vertex_hash(bpy.data.objects[name])==value for name,value in fixed.items())
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in images.items())
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
bpy.ops.object.select_all(action='DESELECT')
for ob in bpy.context.scene.objects:
 if ob.name in names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
for name in ['RegionOneBlenderQueries.bytes','production-region.json']:shutil.copyfile(source.parent/name,out/name)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'method':'actual BlenderMCP organic strategic silhouettes and flow UV authoring','parent_source_run':40233611270,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_vertices_after':len(terrain.data.vertices),'protected_vertices_unchanged':51898,'terrain_and_geology_vertex_hashes_unchanged':fixed,'packed_images_unchanged':len(images),'shared_strategic_meshes':len(meshes),'strategic_trees_reusing_two_donors':replaced,'strategic_triangles_per_mesh':[sum(len(p.vertices)-2 for p in m.polygons) for m in meshes.values()],'tributary_uvs':water,'new_gameplay_targets':0,'new_colliders':0,'files':{name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']},'art_approved':False}
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
# Workshop now previews the strategic source meshes with the same 34-41m
# envelopes as Unity. This is review-only and is applied AFTER saving/export.
def identity(name):
 h=2166136261
 for c in name:h=((h^ord(c))*16777619)&0xffffffff
 return h
def bounds(ob):
 q=ob.rotation_quaternion if ob.rotation_mode=='QUATERNION' else ob.rotation_euler.to_quaternion();matrix=Matrix.LocRotScale(ob.location,q,ob.scale)
 if ob.parent:matrix=ob.parent.matrix_world@ob.matrix_parent_inverse@matrix
 p=[matrix@Vector(v) for v in ob.bound_box];return tuple(max(v[i] for v in p)-min(v[i] for v in p) for i in range(3)),min(v.z for v in p)
bpy.context.view_layer.update()
previous={ob:(ob.hide_render,ob.location.copy(),ob.scale.copy()) for ob in bpy.context.scene.objects if ob.type=='MESH'}
for near in list(bpy.context.scene.objects):
 if not near.name.startswith(('BLD_ForestMass_','Pilot_Pine')):continue
 retained=near.name.startswith('Pilot_Pine');far=bpy.data.objects.get('Far_'+near.name if retained else near.name.replace('BLD_ForestMass_','BLD_ForestFar_',1))
 if not far:continue
 h=identity(near.name);show=h%5==0 if retained else True
 if not retained:
  far.scale.x*=2.1;far.scale.y*=2.1;far.scale.z*=2.6;size,base=bounds(far);foot=max(size[:2])
  if foot<12:show=(h%10000)/10000.<foot*foot/(22*22);crown=22;high=34
  else:crown=20+h%7;high=34+h%8
 else:crown=20+(h//5)%5;high=34+h%7
 if show:
  size,base=bounds(far);foot=max(size[:2]);far.scale.x*=crown/max(.01,foot);far.scale.y*=crown/max(.01,foot);far.scale.z*=high/max(.01,size[2]);far.location.z+=bounds(near)[1]-bounds(far)[1]
 near.hide_render=True;far.hide_render=not show
bpy.context.view_layer.update()
bpy.app.driver_namespace['artist_preview_use_far']=True
bpy.app.driver_namespace['interactive_snapshot']('organic-strategic-candidate')
bpy.app.driver_namespace['artist_preview_use_far']=False
for ob,(hide,location,scale) in previous.items():ob.hide_render=hide;ob.location=location;ob.scale=scale
(out/'stop.request').write_text('Source flow mapping and shared strategic tree volumes saved; source terrain untouched')
