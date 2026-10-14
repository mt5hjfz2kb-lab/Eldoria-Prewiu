"""Director material trial on actual Qwen sculpt. No new AI decision or Unity approval claimed."""
import argparse, hashlib, json, math, pathlib, sys
import bpy
p=argparse.ArgumentParser();p.add_argument('--source',type=pathlib.Path,required=True);p.add_argument('--out',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);a.out.mkdir(parents=True,exist_ok=False)
root=pathlib.Path(__file__).resolve().parents[3]
bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
terrain=bpy.data.objects['ZoneOne continuous prairie source'];mesh=terrain.data
positions=[list(v.co) for v in mesh.vertices]
plan=root/'docs/world4x-zone1/evidence/green-material-rejected-20261014/artist-plan.json'
recipe=json.loads(plan.read_text())[0]['parameters'];tile_m=recipe['detail_tile_m']
# Reuse the artist's selected 4m detail scale, preserving RGB donor rather than
# losing it in a 240m-wide baked atlas. Other rejected palette choices are not adopted.
donor=root/'Unity/Assets/Mega Fantasy Props Pack/Textures/grass_1_fixed_d.png'
uv=mesh.uv_layers.active
for loop in mesh.loops:
 v=mesh.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(v.x/tile_m,v.y/tile_m)
material=bpy.data.materials.new('ZoneOne donor tiled ground director trial');material.use_nodes=True
nodes=material.node_tree.nodes;nodes.clear();links=material.node_tree.links
texture=nodes.new('ShaderNodeTexImage');texture.image=bpy.data.images.load(str(donor.resolve()),check_existing=True)
texture.extension='REPEAT';texture.image.pack()
shader=nodes.new('ShaderNodeBsdfPrincipled');shader.inputs['Roughness'].default_value=.88
links.new(texture.outputs['Color'],shader.inputs['Base Color'])
output=nodes.new('ShaderNodeOutputMaterial');links.new(shader.outputs[0],output.inputs['Surface'])
mesh.materials.clear();mesh.materials.append(material)
assert positions==[list(v.co) for v in mesh.vertices]
bpy.ops.wm.save_as_mainfile(filepath=str((a.out/'tiled-ground.blend').resolve()))
bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
bpy.ops.export_scene.gltf(filepath=str((a.out/'tiled-ground.glb').resolve()),export_format='GLB',use_selection=True)
(a.out/'material-trial.json').write_text(json.dumps({
 'authorship':'Director technical material-route trial on genuine Qwen-authored sculpt',
 'source_sha256':hashlib.sha256(a.source.read_bytes()).hexdigest(),
 'recovered_artist_plan_sha256':hashlib.sha256(plan.read_bytes()).hexdigest(),
 'reused_artist_parameter':{'detail_tile_m':tile_m},
 'rejected_palette_not_adopted':True,'new_inference':False,
 'geometry_coordinates_unchanged':True,'uv_changed_for_actual_repeating_detail':True,
 'donor_sha256':hashlib.sha256(donor.read_bytes()).hexdigest(),
 'model_sha256':hashlib.sha256((a.out/'tiled-ground.glb').read_bytes()).hexdigest(),
 'art_approved':False,'unity_executed':False},indent=2))
