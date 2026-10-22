"""Freeze actual Unity mesh for the existing typed Qwen/Blender author.
This script copies measured geometry; it makes NO artistic deformation.
One Blender unit is ten Unity metres, retaining all vertex indices.
"""
import bpy, json, pathlib, sys, hashlib
from mathutils import Vector

args=sys.argv[sys.argv.index('--')+1:]
source=pathlib.Path(args[0]); output=pathlib.Path(args[1])
output.mkdir(parents=True,exist_ok=True)
data=json.loads(source.read_text('utf8'))
origin=data['origin']
vertices=[((v['x']-origin['x'])/10, (v['z']-origin['z'])/10, v['y']/10)
          for v in data['vertices']]
# Unity Y-up to Blender Z-up reflects handedness; reverse face winding.
indices=data['triangles']
faces=[tuple(reversed(indices[i:i+3])) for i in range(0,len(indices),3)]
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
mesh=bpy.data.meshes.new('MeasuredRegionTerrain');mesh.from_pydata(vertices,[],faces);mesh.update()
obj=bpy.data.objects.new('RegionOneCanonicalTerrain',mesh)
bpy.context.collection.objects.link(obj)
bpy.context.view_layer.objects.active=obj;obj.select_set(True)
uv=mesh.uv_layers.new(name='UnityAtlas')
for polygon in mesh.polygons:
    for loop in polygon.loop_indices:
        v=data['uv'][mesh.loops[loop].vertex_index]
        uv.data[loop].uv=(v['x'],v['y'])
mat=bpy.data.materials.new('Neutral clay for matched geometry review')
mat.diffuse_color=(.50,.53,.39,1);obj.data.materials.append(mat)
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE_NEXT' if bpy.app.version<(4,2,0) else 'CYCLES'
scene.cycles.samples=8
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100
camera_data=bpy.data.cameras.new('FixedStrategicReview');camera=bpy.data.objects.new('FixedStrategicReview',camera_data)
bpy.context.collection.objects.link(camera);scene.camera=camera
camera.location=(0,-24,52);direction=Vector((0,0,0))-camera.location
camera.rotation_euler=direction.to_track_quat('-Z','Y').to_euler();camera_data.type='ORTHO';camera_data.ortho_scale=87
sun_data=bpy.data.lights.new('FixedReviewSun','SUN');sun_data.energy=2
sun=bpy.data.objects.new('FixedReviewSun',sun_data);bpy.context.collection.objects.link(sun)
sun.rotation_euler=(.45,-.6,-.5)
scene.world.color=(.25,.25,.25)
dest=output/'terrain-source.blend'
if dest.exists():raise RuntimeError('Refuse to replace frozen Blender source')
bpy.ops.wm.save_as_mainfile(filepath=str(dest.resolve()))
scene.render.filepath=str((output/'terrain-clay-before.png').resolve())
bpy.ops.render.render(write_still=True)
(output/'source-provenance.json').write_text(json.dumps({
    'unity_mesh_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),
    'blend_sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),
    'vertices':len(vertices),'triangles':len(faces),'unity_units_per_blender_unit':10,
    'artistic_operations':0,'gameplay_modified':False,
    'render_scope':'Blender terrain-only clay; NOT full Unity gameplay or art approval'
},indent=2))
