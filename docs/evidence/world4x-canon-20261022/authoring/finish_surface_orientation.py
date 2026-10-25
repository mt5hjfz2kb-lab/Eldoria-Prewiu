import bpy,pathlib,json,hashlib,struct,shutil
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source']);scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain'];fixed_positions=[tuple(v.co) for v in terrain.data.vertices];images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
changes={}
for ob in [terrain]+[bpy.data.objects[name] for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures','BLD_BreachBrokenBasaltPlates','BLD_BreachTerracedStrata','BLD_BreachWeatheredRockfall']]:
 mesh=ob.data;normals=[tuple(n.vector) for n in mesh.corner_normals];faces=[p for p in mesh.polygons if ob!=terrain or all(-470.001<mesh.vertices[i].co.x<-97.999 and -20.001<mesh.vertices[i].co.y<320.001 for i in p.vertices)]
 horizontal=[p for p in faces if abs(p.normal.z)>.6];assert horizontal,(ob.name,len(faces));up=sum(p.normal.z*p.area for p in horizontal);assert up!=0
 count=0
 for p in faces:
  if p.normal.z<-.000001:
   p.flip();count+=1
   for li in p.loop_indices:
    normals[li]=(0,0,0)
    if mesh.uv_layers.active:
     q=mesh.vertices[mesh.loops[li].vertex_index].co;mesh.uv_layers.active.data[li].uv=((q.x+910)/1820,(q.y+1800)/3600)
 mesh.update();mesh.normals_split_custom_set(normals);mesh.update()
 assert sum(p.normal.z*p.area for p in faces if abs(p.normal.z)>.6)>0,ob.name
 changes[ob.name]={'faces':len(faces),'flipped':count,'area_weighted_up_before':up}
# Export a real supported glTF pigment; Unity supplies the physical strata.
rock=bpy.data.materials['Breach fractured basalt continuous geological wall'];bsdf=next(n for n in rock.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
for link in list(bsdf.inputs['Base Color'].links):rock.node_tree.links.remove(link)
bsdf.inputs['Base Color'].default_value=(.22,.235,.25,1);rock.diffuse_color=(.22,.235,.25,1)
assert [tuple(v.co) for v in terrain.data.vertices]==fixed_positions
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in images.items())
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
for name in ['RegionOneBlenderQueries.bytes','production-region.json']:shutil.copyfile(source.parent/name,out/name)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True);bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report=json.loads((source.parent/'macro-art-operations.json').read_text(encoding='utf-8-sig'));report['orientation_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();report['corrected_outward_surface_orientation']=changes;report['terrain_vertex_positions_unchanged']=True;report['exported_rock_base_colour']=[.22,.235,.25,1];report['files']={name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']};report['blend_bytes']=(out/'Eldoria_Region1_Production.blend').stat().st_size;assert report['blend_bytes']<100*1024*1024;(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2));(out/'stop.request').write_text('Corrected physical face orientation and supported glTF pigment; vertices/images unchanged')
