"""Zero-spend premium square-metre source capability proof.
Creates one deliberately authored ~2m x 2.2m stone/buttress fragment only.
This is a bounded capability test, not a reusable building generator and not a Unity asset.
"""
import bpy, json, math, os
from pathlib import Path
from mathutils import Vector

ROOT = Path(os.environ.get('GITHUB_WORKSPACE', os.getcwd()))
REQ = json.loads((ROOT/'pipeline/valoria-production-art-starter-run-request.json').read_text())
SPEC = json.loads((ROOT/REQ['mesh_spec']).read_text())
if SPEC.get('source_method') != 'sculpt_detail_authored':
    raise RuntimeError('premium square metre author called for wrong source_method')
SRC = ROOT/REQ['source_dir']; EVID = ROOT/REQ['evidence_dir']
SRC.mkdir(parents=True, exist_ok=True); EVID.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

# Neutral source-review material only. No texture/material rescue in this proof.
mat = bpy.data.materials.new('Valoria_Clay'); mat.diffuse_color=(0.47,0.43,0.38,1)

def bevel_noise(obj, bevel=0.025, noise=0.0, seed=0):
    bpy.context.view_layer.objects.active=obj; obj.select_set(True)
    if bevel:
        m=obj.modifiers.new('hand-shaped edge roll','BEVEL'); m.width=bevel; m.segments=3
        bpy.ops.object.modifier_apply(modifier=m.name)
    if noise:
        s=obj.modifiers.new('micro surface subdivision','SUBSURF'); s.levels=2; s.render_levels=2
        bpy.ops.object.modifier_apply(modifier=s.name)
        tex=bpy.data.textures.new(f'stone_variation_{seed}', type='CLOUDS'); tex.noise_scale=0.17+0.013*(seed%5); tex.noise_depth=2
        d=obj.modifiers.new('authored stone surface','DISPLACE'); d.texture=tex; d.strength=noise; d.texture_coords='GLOBAL'; d.mid_level=0.52
        bpy.ops.object.modifier_apply(modifier=d.name)
    obj.data.materials.append(mat); obj.select_set(False)

def add_block(name, loc, scale, rot=(0,0,0), bevel=0.022, noise=0.014, seed=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o=bpy.context.object; o.name=name; o.scale=scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel_noise(o, bevel, noise, seed)
    o['source_group']='StoneMass'; o['authoring_source']='bounded_sculpt_detail_proof'
    return o

def add_wedge(name, verts, faces, group='ButtressProfile', bevel=0.03, noise=0.012, seed=50):
    me=bpy.data.meshes.new(name+'Mesh'); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(o)
    o.data.materials.append(mat); o['source_group']=group; o['authoring_source']='bounded_sculpt_detail_proof'
    bpy.context.view_layer.objects.active=o; o.select_set(True)
    b=o.modifiers.new('hand-shaped edge roll','BEVEL'); b.width=bevel; b.segments=3
    bpy.ops.object.modifier_apply(modifier=b.name)
    s=o.modifiers.new('micro surface subdivision','SUBSURF'); s.levels=2; s.render_levels=2
    bpy.ops.object.modifier_apply(modifier=s.name)
    tex=bpy.data.textures.new(f'wedge_variation_{seed}', type='CLOUDS'); tex.noise_scale=.19; tex.noise_depth=2
    d=o.modifiers.new('authored stone surface','DISPLACE'); d.texture=tex; d.strength=noise; d.texture_coords='GLOBAL'; d.mid_level=.52
    bpy.ops.object.modifier_apply(modifier=d.name); o.select_set(False)
    return o

# Backing masonry: explicit staggered courses with deliberate asymmetry and readable joint depth.
# Dimensions are authored to fit a 2.0 x 2.2 m review patch, not generated from a grammar.
rows=[
    [(-.77,.31),(-.35,.42),(.10,.48),(.56,.39),(.84,.18)],
    [(-.86,.25),(-.54,.35),(-.12,.50),(.38,.47),(.79,.31)],
    [(-.78,.33),(-.36,.42),(.08,.45),(.50,.39),(.83,.24)],
    [(-.86,.27),(-.54,.34),(-.15,.46),(.30,.43),(.72,.39)],
    [(-.76,.34),(-.34,.42),(.11,.47),(.56,.41),(.86,.20)],
]
row_h=[.22,.215,.225,.205,.215]
z=-.98
objects=[]; seed=1
for ri,row in enumerate(rows):
    z += row_h[ri]
    for ci,(cx,w) in enumerate(row):
        # preserve small mortar gaps; irregular face depth/rotation produces real light-catching relief.
        h=row_h[ri]-.028
        dep=.20 + .018*math.sin(seed*1.91)
        y=.05 + .012*math.cos(seed*2.37)
        rot=(math.radians(.6*math.sin(seed)), math.radians(.7*math.cos(seed*1.7)), math.radians(1.15*math.sin(seed*2.1)))
        objects.append(add_block(f'Stone_{ri}_{ci}',(cx,y,z),(w/2-.014,dep/2,h/2),rot,0.018+0.004*(seed%3),0.011+0.002*(seed%4),seed))
        seed+=1

# Buttress: tapered primary mass broken into stacked authored stones. Projection depth is intentionally strong.
# Central pier is slightly off-axis to avoid a sterile kit read.
for i,(zz,ww,hh,dd,dx) in enumerate([
    (-.79,.66,.38,.62,-.03),(-.42,.62,.34,.57,.01),(-.09,.58,.31,.51,-.015),(.22,.54,.30,.45,.015),(.51,.48,.28,.39,-.005)
]):
    rot=(math.radians(.5*math.sin(i+2)),math.radians(.8*math.cos(i+1)),math.radians(.7*math.sin(i*1.3)))
    objects.append(add_block(f'ButtressStone_{i}',(dx,-.18,zz),(ww/2,dd/2,hh/2),rot,.028,.015,70+i))
    objects[-1]['source_group']='ButtressProfile'

# Sloped cap stone gives the buttress a silhouette-changing terminal instead of a box-stack ending.
v=[(-.28,-.49,.61),(.27,-.49,.61),(.23,.02,.61),(-.24,.02,.61),(-.22,-.42,.82),(.21,-.42,.82),(.18,.00,.78),(-.19,.00,.78)]
f=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(4,0,3,7)]
objects.append(add_wedge('ButtressCap',v,f,'ButtressProfile',.025,.010,91))

# Two intentionally chipped edge fragments: silhouette damage that survives neutral-clay review.
objects.append(add_block('EdgeChip_L',(-.95,-.02,.52),(.055,.15,.20),(0,math.radians(-7),math.radians(8)),.012,.010,110)); objects[-1]['source_group']='EdgeDamage'
objects.append(add_block('EdgeChip_R',(.95,-.01,-.56),(.045,.14,.16),(0,math.radians(5),math.radians(-11)),.010,.009,111)); objects[-1]['source_group']='EdgeDamage'

# Ground receiver is a separate contract object, not a fused rock pedestal.
objects.append(add_block('GroundReceiver',(0,.10,-1.08),(1.0,.34,.06),(0,0,0),.018,.006,120)); objects[-1]['source_group']='ReceiverInterface'

# UVs by cube projection for export sanity. Tangents are created on export/import path later if adopted.
for o in objects:
    bpy.context.view_layer.objects.active=o; o.select_set(True)
    if not o.data.uv_layers:
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=.02); bpy.ops.object.mode_set(mode='OBJECT')
    o.select_set(False)

# Source save + GLB export.
blend=SRC/'Valoria_PremiumSquareMetre_v1.blend'; glb=SRC/'Valoria_PremiumSquareMetre_v1.glb'
bpy.ops.wm.save_as_mainfile(filepath=str(blend))
for o in bpy.context.selected_objects:o.select_set(False)
for o in objects:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(glb), export_format='GLB', use_selection=True, export_apply=True)
for o in objects:o.select_set(False)

# Three neutral-clay evidence views. Workbench prevents materials/lighting from rescuing weak geometry.
world=bpy.context.scene.world or bpy.data.worlds.new('World'); bpy.context.scene.world=world
scene=bpy.context.scene; scene.render.engine='BLENDER_WORKBENCH'; scene.display.shading.light='STUDIO'; scene.display.shading.show_shadows=True; scene.display.shading.show_cavity=True; scene.display.shading.cavity_type='WORLD'; scene.display.shading.curvature_ridge_factor=1.7; scene.display.shading.curvature_valley_factor=1.5
scene.render.resolution_percentage=100; scene.render.image_settings.file_format='PNG'; scene.render.film_transparent=False

def render(name,loc,target,ortho,res):
    camdat=bpy.data.cameras.new(name+'Cam'); camdat.type='ORTHO'; camdat.ortho_scale=ortho
    cam=bpy.data.objects.new(name+'Cam',camdat); bpy.context.collection.objects.link(cam); cam.location=loc
    direction=Vector(target)-cam.location; cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler(); scene.camera=cam
    scene.render.resolution_x=res[0];scene.render.resolution_y=res[1];scene.render.filepath=str(EVID/(name+'.png'));bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam,do_unlink=True);bpy.data.cameras.remove(camdat)

render('neutral-clay-close',(3.4,-5.5,2.8),(0,-.05,-.05),2.8,(1024,1024))
# Approximate canonical orthographic direction: yaw20/pitch35, enough to test gameplay-scale profile.
yaw=math.radians(20); pitch=math.radians(35); dist=8
loc=(dist*math.sin(yaw)*math.cos(pitch),-dist*math.cos(yaw)*math.cos(pitch),dist*math.sin(pitch))
render('official-camera-proxy',loc,(0,0,-.05),3.3,(1280,720))
render('silhouette-three-quarter',(4.5,-6.7,3.8),(0,-.08,-.10),3.0,(1024,768))

tris=sum(len(o.data.polygons) for o in objects)*2
report={
  'family_id':SPEC['family_id'],'source_method':SPEC['source_method'],'credits':0,'unity_touched':False,
  'source':str(blend.relative_to(ROOT)),'export':str(glb.relative_to(ROOT)),
  'preview_evidence':[str((EVID/n).relative_to(ROOT)) for n in ['neutral-clay-close.png','official-camera-proxy.png','silhouette-three-quarter.png']],
  'object_count':len(objects),'approx_triangles_upper_bound':tris,
  'groups':sorted(set(o.get('source_group','') for o in objects)),
  'review_rule':'Visual gate remains human/agent review. Technical generation never implies ART SOURCE PASS.'
}
(EVID/'source-report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
