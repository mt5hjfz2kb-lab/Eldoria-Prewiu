"""GLB roundtrip and matched before/after renders. Run after final source export."""
import bpy,json,math,hashlib,os
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[3]
OUT=Path(os.environ.get('ELDORIA_HERO_VERIFY_OUTPUT',ROOT/'art-source/valoria/production/first-hero-gate-v1'))
bpy.ops.wm.open_mainfile(filepath=str(OUT/'ValoriaHeroGate.blend'))
scene=bpy.context.scene
def capture(name):
    cam=scene.camera;cam.location=(13,-19,13);cam.rotation_euler=(Vector((0,0,3.5))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=15.5
    scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.filepath=str(OUT/(name+'.png'));bpy.ops.render.render(write_still=True)
capture('after-editable')
for o in list(bpy.data.collections['VALORIA_HERO_GATE'].objects):bpy.data.objects.remove(o,do_unlink=True)
prior=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(OUT/'ValoriaHeroGate.glb'))
imported=[o for o in set(bpy.data.objects)-prior if o.type=='MESH']
assert imported and all(o.data.uv_layers for o in imported)
assert all(o.data.materials for o in imported)
capture('after-glb-roundtrip')
# Actual lower-detail library exports, retaining UV/materials. Not device certification.
lods=[]
originals={o:o.data.copy() for o in imported}
for level,angle,ratio in [(1,.075,.92)]:
    for o in imported:
        o.data=originals[o].copy();o.modifiers.clear()
        material=o.data.materials[0].name
        if any(k in material for k in ('Cloth','Brass','flame','Iron','Oak')):continue
        # Edge-collapse across disconnected ashlars destroyed the silhouette.
        # Planar dissolve keeps separate dressed stones and cloth intact.
        m=o.modifiers.new('Library LOD planar dissolve','DECIMATE');m.decimate_type='DISSOLVE';m.angle_limit=angle
        m.delimit={'MATERIAL','SEAM','SHARP','UV'}
        collapse=o.modifiers.new('Bounded LOD reduction','DECIMATE');collapse.ratio=ratio
        collapse.delimit={'NORMAL','MATERIAL','SEAM','SHARP','UV'}
        normal=o.modifiers.new('LOD weighted normals','WEIGHTED_NORMAL');normal.keep_sharp=True
    bpy.ops.object.select_all(action='DESELECT')
    for o in imported:o.select_set(True)
    bpy.context.view_layer.objects.active=imported[0]
    p=OUT/('ValoriaHeroGate_LOD'+str(level)+'.glb')
    bpy.ops.export_scene.gltf(filepath=str(p),export_format='GLB',use_selection=True,export_apply=True)
    graph=bpy.context.evaluated_depsgraph_get();tris=0
    for o in imported:
        d=o.evaluated_get(graph).to_mesh();tris+=sum(len(f.vertices)-2 for f in d.polygons);o.evaluated_get(graph).to_mesh_clear()
    lods.append({'level':level,'angle_radians':angle,'collapse_ratio':ratio,'triangles':tris,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    capture('after-lod1')
for o in imported:bpy.data.objects.remove(o,do_unlink=True)
if not os.environ.get('ELDORIA_HERO_VERIFY_OUTPUT'):
    for o in list(bpy.data.objects):
        if o.type=='EMPTY':bpy.data.objects.remove(o,do_unlink=True)
    bpy.ops.import_scene.gltf(filepath=str(ROOT/'art-source/valoria/production/gatehouse-v5-quality-calibration/valoria_gate_reference.glb'))
    baseline=[o for o in bpy.data.objects if o.type=='MESH' and o.name!='RENDER_CONTEXT_ONLY']
    points=[o.matrix_world@Vector(c) for o in baseline for c in o.bound_box]
    lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
    scale=11.68/(hi.x-lo.x)
    # Preserve hierarchy by applying normalization to import roots only.
    roots=[o for o in bpy.data.objects if o.parent is None and (o.type=='EMPTY' or o in baseline)]
    for o in roots:
        o.scale*=scale;o.location=(o.location-Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z)))*scale
    capture('before-v5-matched')
(OUT/'export-verification.json').write_text(json.dumps({'roundtrip_import':True,'uv_present':True,'materials_present':True,'matched_camera_lighting':True,'lods':lods,'physical_mobile_performance':False,'unity_tested':False},indent=2))
print('ELDORIA_HERO_GLTF_ROUNDTRIP_PASS')
