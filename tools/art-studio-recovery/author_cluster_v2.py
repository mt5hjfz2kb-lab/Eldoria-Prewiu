"""Test a second CC0 geological donor in the existing Valoria Blender study.

The source scene, camera, world, grass surface and both gatehouses are retained.
This is a bounded artist-directed donor substitution, not a procedural terrain reset.
"""
import bpy, bmesh, hashlib, json, math, os, random, shutil
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
V1=ROOT/'art-source/valoria/production/studio-rocks-v1'
OUT=ROOT/'art-source/valoria/production/studio-rocks-v2'
OUT.mkdir(parents=True,exist_ok=True)
DONOR=Path(os.environ.get('ELDORIA_CLUSTER_SOURCE','/tmp/eldoria-cluster-source'))
obj=next(DONOR.rglob('ClusterRock_LOD4-4k.obj'))
archive=Path(os.environ.get('ELDORIA_CLUSTER_ARCHIVE','/tmp/eldoria-cluster.7z'))
def digest(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()

bpy.ops.wm.open_mainfile(filepath=str(V1/'ValoriaRockStudy.blend'))
old=[o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('CC0 directed cliff course')]
assert len(old)==52
old_mesh=old[0].data
old_material=old_mesh.materials[0]
assert old_material and len(old_mesh.polygons)>1000
for name in ('overview','cliff','mobile'):
 shutil.copy2(V1/f'after-{name}.png',OUT/f'before-{name}.png')

existing=set(bpy.data.objects)
bpy.ops.wm.obj_import(filepath=str(obj))
new=[o for o in bpy.data.objects if o not in existing and o.type=='MESH']
assert len(new)==1,new
master=new[0]
world=[master.matrix_world @ v.co for v in master.data.vertices]
low=Vector(tuple(min(v[i] for v in world) for i in range(3)))
high=Vector(tuple(max(v[i] for v in world) for i in range(3)))
size=high-low
assert min(size)>0
master.matrix_world.identity()
for v,p in zip(master.data.vertices,world):
 v.co=((p.x-(low.x+high.x)/2)/size.x,(p.y-(low.y+high.y)/2)/size.y,(p.z-low.z)/size.z)

# Cut the rock crown below its tallest lobes; the unseen top embeds into the
# retained grass rather than forming a line of narrow spikes above the lawn.
bm=bmesh.new();bm.from_mesh(master.data)
bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),plane_co=(0,0,.73),plane_no=(0,0,1),clear_outer=True,clear_inner=False)
cap=bmesh.ops.holes_fill(bm,edges=[e for e in bm.edges if e.is_boundary],sides=0)['faces']
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(master.data);bm.free();master.data.update()
for p in master.data.polygons:p.use_smooth=True

mat=bpy.data.materials.new('CC0 scanned cluster - cool granite palette')
mat.use_nodes=True
nodes=mat.node_tree.nodes;links=mat.node_tree.links
bs=nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.88
for kind,filename in [('albedo','ClusterRock_LOD4_diffuse.tga'),('normal','ClusterRock_LOD4_normal.tga')]:
 image=bpy.data.images.load(str(obj.parent/filename));image.scale(1024,1024)
 image.filepath_raw=str(OUT/f'cluster-{kind}.png');image.file_format='PNG';image.save();image.pack()
 tex=nodes.new('ShaderNodeTexImage');tex.image=image
 if kind=='albedo':
  tint=nodes.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=1
  tint.inputs[2].default_value=(.80,.85,.87,1)
  links.new(tex.outputs['Color'],tint.inputs[1]);links.new(tint.outputs['Color'],bs.inputs['Base Color'])
 else:
  image.colorspace_settings.name='Non-Color'
  norm=nodes.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.55
  links.new(tex.outputs['Color'],norm.inputs['Color']);links.new(norm.outputs['Normal'],bs.inputs['Normal'])
master.data.materials.clear();master.data.materials.append(mat)
new_triangles=sum(len(p.vertices)-2 for p in master.data.polygons)

# The two scanned donors alternate in selected geological courses. Large
# cluster groups break the repeated rounded crest without moving the footprint.
selected=[]
for o in old:
 i=int(o.name.rsplit(' ',1)[-1])
 if i%5 not in (1,3):continue
 o.data=master.data
 o.name=f'CC0 cluster directed cliff course {i:02d}'
 # Keep the established crown elevation, vary orientation and profile through
 # the actual independent scan, not additive mesh noise.
 o.rotation_euler.z+=(-.18 if i%2 else .21)
 o.scale.x*=1.07 if i%2 else .97
 o.scale.y*=.98 if i%2 else 1.07
 o.location.z-=.25
 selected.append(i)
assert len(selected)>=20
bpy.data.objects.remove(master,do_unlink=True)

s=bpy.context.scene;cam=s.camera
assert cam and cam.name.startswith('Matched strategic study')
views={'overview':((50,-70,49),(0,4,0),108,1280,800),'cliff':((52,-64,18),(15,-24,-2),44,1280,800),'mobile':((50,-70,49),(0,4,0),108,640,400)}
for name,(pos,target,scale,rx,ry) in views.items():
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
 s.render.resolution_x=rx;s.render.resolution_y=ry;s.render.filepath=str(OUT/f'after-{name}.png')
 bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaRockStudyV2.blend'))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
rocks=[o for o in meshes if o.name.startswith('CC0 ') or o.name.startswith('Buried continuous')]
terrain=[o for o in meshes if o.name.startswith('Valoria Highlands')]
assert len(terrain)==1 and len(rocks)>=69
bpy.ops.object.select_all(action='DESELECT')
for o in rocks:o.select_set(True)
bpy.context.view_layer.objects.active=rocks[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaScannedCliffV2.glb'),export_format='GLB',use_selection=True,export_apply=True)
terrain[0].select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaEnvironmentV2.glb'),export_format='GLB',use_selection=True,export_apply=True)
(OUT/'production-report.json').write_text(json.dumps({
 'method':'artist_selected_second_CC0_scan_in_existing_MCP_repaired_rock_course',
 'source_scene_sha256':digest(V1/'ValoriaRockStudy.blend'),'owner_reference_sha256':digest(V1/'owner-reference.jpeg'),
 'cluster_source_url':'https://opengameart.org/content/free-photogrammetric-rock-cluster',
 'cluster_source_archive_sha256':digest(archive),'cluster_obj_sha256':digest(obj),
 'new_cluster_indices':selected,'cluster_instance_count':len(selected),'retained_old_course_count':52-len(selected),
 'new_cluster_triangles':new_triangles,
 'cut_cap_faces':len(cap),'old_donor_geometry_retained':True,'grass_terrain_and_gates_retained':True,
 'same_camera_and_lighting':True,'paid_services_used':False,'qwen_art_gate_used':False,
 'blender_mcp_v1_input_retained':True,'visual_pass':False,'unity_tested':False,
 'exports':{p.name:{'bytes':p.stat().st_size,'sha256':digest(p)} for p in OUT.glob('*.glb')}
},indent=2))
