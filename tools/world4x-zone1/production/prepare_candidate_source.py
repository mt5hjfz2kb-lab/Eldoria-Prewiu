"""Derive an editable source from the exact recovered GLB; never invent original .blend hashes."""
import argparse,hashlib,json,pathlib,sys
import bpy
from mathutils import Vector
p=argparse.ArgumentParser();p.add_argument('--out',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);out=a.out.resolve()
root=pathlib.Path(__file__).resolve().parents[3]
model=root/'Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb'
proof=json.loads((root/'docs/world4x-zone1/evidence/local-cycle-20261014/finishing-execution.json').read_text())
assert hashlib.sha256(model.read_bytes()).hexdigest()==proof['model_sha256']
if out.exists() and any(out.iterdir()):raise ValueError('Use a fresh evidence directory')
out.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(model))
terrain=bpy.data.objects.get('ZoneOne continuous prairie source')
if not terrain or terrain.type!='MESH':raise ValueError('Actual terrain mesh not retained')
hidden=[]
for obj in bpy.data.objects:
 if obj.type=='MESH' and obj!=terrain:
  obj.hide_render=True;hidden.append(obj.name)
world=bpy.context.scene.world or bpy.data.worlds.new('ZoneOne sky');bpy.context.scene.world=world
world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.52,.66,.84,1)
world.node_tree.nodes['Background'].inputs[1].default_value=.45
data=bpy.data.lights.new('Source sun','SUN');data.energy=2.4;data.angle=.12
sun=bpy.data.objects.new('Source sun',data);bpy.context.collection.objects.link(sun)
sun.rotation_euler=(.471239,-.401426,-.610865)
data=bpy.data.cameras.new('Frozen source camera');camera=bpy.data.objects.new('Frozen source camera',data)
bpy.context.collection.objects.link(camera);bpy.context.scene.camera=camera
focus=Vector((10,15,0));camera.location=focus+Vector((0,-130,145))
camera.rotation_euler=(focus-camera.location).to_track_quat('-Z','Y').to_euler()
data.type='ORTHO';data.ortho_scale=160
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=False
scene.render.resolution_x=1000;scene.render.resolution_y=800;scene.render.resolution_percentage=100
for image in bpy.data.images:
 if image.source=='FILE' and not image.packed_file:image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(out/'derived-source.blend'))
scene.render.filepath=str(out/'blender-before.png');bpy.ops.render.render(write_still=True)
(out/'derived-source-proof.json').write_text(json.dumps({'origin_glb_sha256':proof['model_sha256'],
 'derived_blend_sha256':hashlib.sha256((out/'derived-source.blend').read_bytes()).hexdigest(),
 'image_type':'BLENDER_NOT_UNITY','original_missing_blend_recreated':False,
 'terrain':terrain.name,'vertices':len(terrain.data.vertices),'polygons':len(terrain.data.polygons),
 'camera_matrix':[list(row) for row in camera.matrix_world],'hidden_source_meshes':hidden,
 'scope':'ground-only reference patch; props retained, not authored or approved',
 'art_approved':False},indent=2)+'\n')
