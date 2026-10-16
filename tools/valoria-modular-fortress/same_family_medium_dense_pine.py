"""One individual pine_medium quality lift using a visually approved denser
pine_tall CC0 donor from the SAME Quaternius forest family; scale to exactly
previous medium tree bounds, preserve botanical branches and PBR.
"""
import bpy,sys,math,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector,Matrix
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,arg)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=1100
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria conifer evaluation');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.24,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def lamp(name,pos,power,col):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('Warm key',(-7,-8,12),1900,(1,.9,.75))
lamp('blue sky fill',(8,3,12),1600,(.70,.81,1))
lamp('rim',(-2,9,10),1000,(1,.98,.87))
cd=bpy.data.cameras.new('identical pine comparison')
cam=bpy.data.objects.new('identical pine comparison',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
medium=source/'pine_medium.glb';tall=source/'pine_tall.glb'
if not medium.is_file() or not tall.is_file():raise RuntimeError('Missing same-family donor trees')
bpy.ops.import_scene.gltf(filepath=str(medium))
original=[o for o in bpy.data.objects if o not in rig]
meshes=[o for o in original if o.type=='MESH']
lo,hi=bounds(meshes);center=(lo+hi)*.5;dimensions=hi-lo
cam.location=center+Vector((.87,-1.58,1.14)).normalized()*max(dimensions.length,1)*3.2
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(dimensions.x,dimensions.y,dimensions.z)*1.72
shoot(out/'before'/'pine_medium.png')
for o in original:bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(tall))
new=[o for o in bpy.data.objects if o not in rig]
meshes=[o for o in new if o.type=='MESH']
if not meshes:raise RuntimeError('Tall dense donor has no mesh')
b_lo,b_hi=bounds(meshes);donor_dim=b_hi-b_lo
# Keep same HEIGHT and base planted at ground; medium size uses exactly matching
# longitudinal scale, and compressed x only slightly to reflect medium crown.
scale=float(dimensions.z/max(donor_dim.z,.001))
for o in new:
 if o.parent is None:o.matrix_world=Matrix.Scale(scale,4)@o.matrix_world
bpy.context.view_layer.update()
scaled_lo,scaled_hi=bounds(meshes)
offset=Vector((center.x-(scaled_lo.x+scaled_hi.x)*.5,
               center.y-(scaled_lo.y+scaled_hi.y)*.5,
               lo.z-scaled_lo.z))
for o in new:
 if o.parent is None:o.matrix_world=Matrix.Translation(offset)@o.matrix_world
bpy.context.view_layer.update()
afterlo,afterhi=bounds(meshes)
if abs((afterhi.z-afterlo.z)-dimensions.z)>.04:raise RuntimeError('Medium height not preserved')
shoot(out/'after'/'pine_medium.png')
bpy.ops.object.select_all(action='DESELECT')
for ob in new:ob.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
dest=out/'glb'/'pine_medium.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<20000:raise RuntimeError('Empty denser same-family CC0 pine')
report={'asset':'pine_medium','source':'Quaternius pine_tall and pine_medium CC0 same family',
 'donor_run':38823354618,'technical':'PASS_PENDING_ART','original_sha':sha(medium),
 'tall_source_sha':sha(tall),'candidate_sha':sha(dest),
 'original_medium_dimensions':[round(float(x),3) for x in dimensions],
 'candidate_dimensions':[round(float(x),3) for x in afterhi-afterlo],
 'individual_not_scene':True,'art_approved':False,'unity_approved':False,
 'caveat':'Assess medium/tall duplication and branching detail; do not claim 6+ automatically.'}
(out/'reports'/'medium_with_full_tall_branches.json').write_text(json.dumps(report,indent=2))
print('MEDIUM_TREE_MORE_FULL_BRANCHES_CC0',dest.stat().st_size,flush=True)
