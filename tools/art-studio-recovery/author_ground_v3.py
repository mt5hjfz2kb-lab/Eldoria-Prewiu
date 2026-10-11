"""Bake a real CC0 ground photograph into Eldoria's four grass families.

Reuses the V2MCP cliff and terrain, keeps camera/lighting unchanged, and exports
direct PBR image nodes so the look survives glTF rather than an unbaked MixRGB.
"""
import bpy, hashlib, json, os, shutil, struct
import numpy as np
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
V2=ROOT/'art-source/valoria/production/studio-rocks-v2-mcp'
SOURCE=Path(os.environ.get('ELDORIA_V2_SOURCE',str(V2/'ValoriaRockStudyV2MCP.blend')))
OUT=ROOT/'art-source/valoria/production/studio-rocks-v3-ground'
OUT.mkdir(parents=True,exist_ok=True)
DIFF=Path(os.environ.get('ELDORIA_GROUND_DIFFUSE','/tmp/eldoria-leafy-diff.jpg'))
NORM=Path(os.environ.get('ELDORIA_GROUND_NORMAL','/tmp/eldoria-leafy-nor.jpg'))
def digest(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
assert digest(DIFF)=='cfa40bc9d9417d1852db8753a8d5917f110c40101179f63543c382e39bc05e4a'
assert digest(NORM)=='832328216adc0a7e1f70a31d5ee48c9ab7f2152d83816122736cf42ac4b2ebd6'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
for name in ('overview','cliff','mobile'):
 shutil.copy2(V2/f'after-{name}.png',OUT/f'before-{name}.png')

photo=bpy.data.images.load(str(DIFF));normal=bpy.data.images.load(str(NORM));normal.colorspace_settings.name='Non-Color'
shutil.copy2(NORM,OUT/'leafy-grass-normal.jpg')
pixels=np.empty(photo.size[0]*photo.size[1]*4,dtype=np.float32)
photo.pixels.foreach_get(pixels)
rgba=pixels.reshape((-1,4))
families=[]
for mat in bpy.data.materials:
 if not mat.name.startswith('Grass lichen moss '):continue
 bs=mat.node_tree.nodes.get('Principled BSDF');assert bs
 base=np.array(bs.inputs['Base Color'].default_value[:],dtype=np.float32)
 baked=rgba*.42+base*.58
 baked[:,3]=1
 image=bpy.data.images.new('Eldoria CC0 earthy grass baked '+mat.name,photo.size[0],photo.size[1],alpha=True)
 image.pixels.foreach_set(baked.reshape(-1))
 image.filepath_raw=str(OUT/('grass-pbr-'+mat.name.rsplit(' ',1)[-1]+'.png'))
 image.file_format='PNG';image.save();image.pack()
 nodes=mat.node_tree.nodes;links=mat.node_tree.links
 tex=nodes.new('ShaderNodeTexImage');tex.image=image;tex.extension='REPEAT'
 links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 ntex=nodes.new('ShaderNodeTexImage');ntex.image=normal;ntex.extension='REPEAT'
 bump=nodes.new('ShaderNodeNormalMap');bump.inputs['Strength'].default_value=.28
 links.new(ntex.outputs['Color'],bump.inputs['Color']);links.new(bump.outputs['Normal'],bs.inputs['Normal'])
 families.append(mat.name)
assert len(families)==4,families
normal.pack()

scene=bpy.context.scene;cam=scene.camera
views={'overview':((50,-70,49),(0,4,0),108,1280,800),'cliff':((52,-64,18),(15,-24,-2),44,1280,800),'mobile':((50,-70,49),(0,4,0),108,640,400)}
for name,(pos,target,scale,rx,ry) in views.items():
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
 scene.render.resolution_x=rx;scene.render.resolution_y=ry;scene.render.filepath=str(OUT/f'after-{name}.png')
 bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaRockAndGroundV3.blend'))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
rocks=[o for o in meshes if o.name.startswith('CC0 ') or o.name.startswith('Buried continuous')]
terrain=[o for o in meshes if o.name.startswith('Valoria Highlands')]
assert len(terrain)==1
bpy.ops.object.select_all(action='DESELECT')
for o in rocks:o.select_set(True)
bpy.context.view_layer.objects.active=rocks[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaRockAndGroundV3-CLIFF.glb'),export_format='GLB',use_selection=True,export_apply=True)
terrain[0].select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaRockAndGroundV3-ENV.glb'),export_format='GLB',use_selection=True,export_apply=True)
raw=(OUT/'ValoriaRockAndGroundV3-ENV.glb').read_bytes();length=struct.unpack_from('<I',raw,12)[0];gltf=json.loads(raw[20:20+length])
grass=[m for m in gltf['materials'] if m['name'].startswith('Grass lichen moss ')]
assert len(grass)==4 and all(m['pbrMetallicRoughness'].get('baseColorTexture') and m.get('normalTexture') for m in grass)
(OUT/'production-report.json').write_text(json.dumps({
 'method':'CC0 photographed leafy ground baked to four Eldoria PBR grass families',
 'source_scene_sha256':digest(SOURCE),'diffuse_sha256':digest(DIFF),'normal_sha256':digest(NORM),
 'source_url':'https://polyhaven.com/a/leafy_grass','license':'CC0',
 'replaced_materials':families,'source_geology_and_terrain_meshes_unchanged':True,
 'fixed_cameras_and_lights':True,'direct_exportable_albedo_and_normal_maps':True,
 'glb_grass_materials_with_albedo_and_normal':len(grass),'glb_images':len(gltf.get('images',[])),
 'visual_pass':False,'unity_tested':False,'paid_services_used':False,
 'exports':{p.name:{'bytes':p.stat().st_size,'sha256':digest(p)} for p in OUT.glob('*.glb')}
},indent=2))
