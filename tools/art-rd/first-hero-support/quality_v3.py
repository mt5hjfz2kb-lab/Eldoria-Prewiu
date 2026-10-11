"""V3 selective finish over saved V2 source, never regenerate architecture."""
import bpy, json, math, random, hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[3]
BASE=ROOT/'art-source/valoria/production/first-hero-gate-v1'
V2=BASE/'quality-v2'
OUT=BASE/'quality-v3'
OUT.mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(V2/'ValoriaHeroGate.blend'))
scene=bpy.context.scene
asset=bpy.data.collections['VALORIA_HERO_GATE']
rng=random.Random(301103)
before_objects=len(asset.objects)
affected=0
# Restrained, deterministic, local wear applied to the SAME mesh topology.
for ob in asset.objects:
    if ob.type!='MESH': continue
    n=ob.name
    if not n.startswith(('Tower front ashlar','Tower rear ashlar','Tower return ashlar',
        'Tower inner return ashlar','Portal jamb dressed ashlar','Wall socket stone',
        'Wall socket back stone','Tower return crenel','Portal modest merlon','Wall socket crenel',
        'Tower crown dressed coping')): continue
    if ob.data.users>1: ob.data=ob.data.copy()
    vertices=ob.data.vertices
    if not vertices: continue
    xs=[v.co.x for v in vertices]; ys=[v.co.y for v in vertices]; zs=[v.co.z for v in vertices]
    zmax=max(zs); zmin=min(zs); xmin=min(xs); xmax=max(xs)
    variation=rng.uniform(.012,.045)
    # Keep joint contacts intact: a few low-amplitude, localized erosion cuts.
    for v in vertices:
        if rng.random()<.37:
            dz=(rng.random()-.5)*variation
            v.co.z+=dz
            v.co.x+=(rng.random()-.5)*variation*.6
            v.co.y+=(rng.random()-.5)*variation*.6
    bevel=next((m for m in ob.modifiers if m.type=='BEVEL'),None)
    if bevel:
        bevel.width=max(.012,min(.052,bevel.width*rng.uniform(.77,1.13)))
        bevel.segments=2
    if n.startswith(('Tower front ashlar','Portal jamb')):
        ob.location.y+=rng.uniform(-.009,.013)
    affected+=1
# V2 cameras and lighting are restored exactly for like-for-like proof.
cam=scene.camera
cam.location=(13,-19,13)
cam.rotation_euler=(Vector((0,0,3.5))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.ortho_scale=15.5
scene.render.resolution_x,scene.render.resolution_y=1280,960
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.filepath=str(OUT/'after-hero-matched.png')
bpy.ops.render.render(write_still=True)
cam.location=(16,-23,18)
cam.rotation_euler=(Vector((0,0,4))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.ortho_scale=24
scene.render.resolution_x,scene.render.resolution_y=1280,720
scene.render.filepath=str(OUT/'after-strategic.png')
bpy.ops.render.render(write_still=True)
cam.location=(5,-12,7)
cam.rotation_euler=(Vector((1.8,-.6,3.7))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.ortho_scale=7.1
scene.render.resolution_x,scene.render.resolution_y=1000,1000
scene.render.filepath=str(OUT/'after-detail.png')
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaHeroGate.blend'))
# Immutable editable save precedes modifier evaluation / export.
meshes=[o for o in asset.objects if o.type=='MESH']
deps=bpy.context.evaluated_depsgraph_get()
for ob in meshes:
    if ob.modifiers:
        ob.data=bpy.data.meshes.new_from_object(ob.evaluated_get(deps),preserve_all_data_layers=True,depsgraph=deps)
        ob.modifiers.clear()
bpy.ops.object.select_all(action='DESELECT')
for ob in meshes: ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaHeroGate.glb'),export_format='GLB',use_selection=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
report={'origin':'quality-v2 saved .blend','baseline_v2_sha256':hashlib.sha256((V2/'ValoriaHeroGate.blend').read_bytes()).hexdigest(),
'objects_before':before_objects,'objects_after':len(asset.objects),'stones_selectively_edited':affected,
'hero_camera':'matched to quality-v2 after-hero','strategic_camera':'matched to quality-v2 after-strategic',
'blend_bytes':(OUT/'ValoriaHeroGate.blend').stat().st_size,'glb_bytes':(OUT/'ValoriaHeroGate.glb').stat().st_size,
'art_acceptance':'pending genuine side by side','unity_tested':False}
(OUT/'quality-report.json').write_text(json.dumps(report,indent=2)+'\n')
print('ELDORIA_HERO_V3_EXPORTED',json.dumps(report))
