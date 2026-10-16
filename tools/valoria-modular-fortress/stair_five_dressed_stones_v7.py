"""ELDORIA stone stair_step V7: 5 independent dressed-stone pavers, 4m exact module.
No new library; preserve both original models, GLB material and fixed-angle A/B. """
import bpy,sys,json,hashlib,math
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:]
if len(args)!=3:raise RuntimeError('stair_step.glb paver_flagstone.glb out_dir')
src,donor,out=map(Path,args)
for name in ('before','after','glb','reports'):(out/name).mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1220;scene.render.resolution_y=850
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria sculptor neutral');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.42,.47,.51,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;scene.world=w
for n,p,loc,col in [('warm main',1550,(-7,-9,9),(1,.90,.79)),('cool fill',1150,(6,7,11),(.75,.85,1))]:
  light=bpy.data.lights.new(n,'AREA');light.energy=p;light.size=9;light.color=col
  o=bpy.data.objects.new(n,light);bpy.context.collection.objects.link(o);o.location=loc
  o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
cd=bpy.data.cameras.new('fixed A B real model');cam=bpy.data.objects.new('fixed A B real model',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
def meshes():return [o for o in scene.objects if o.type=='MESH']
def bounds(xs):
  bpy.context.view_layer.update()
  vs=[o.matrix_world@Vector(v) for o in xs for v in o.bound_box]
  return Vector(tuple(min(v[i] for v in vs) for i in range(3))),Vector(tuple(max(v[i] for v in vs) for i in range(3)))
def shoot(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(src))
original=meshes()
if not original:raise RuntimeError('Original stair_step asset missing geometry')
lo,hi=bounds(original);span=hi-lo;center=(lo+hi)*.5
if not (3.9<span.x<4.1 and .20<span.y<.27 and .60<span.z<.70):raise RuntimeError('Do not replace a different modular scale')
cam.location=center+Vector((1.1,-1.4,1)).normalized()*span.length*2.3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=span.length*1.15
shoot(out/'before'/'stair_step.png')
for o in original:bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(donor))
templates=meshes();assert templates
dl,dh=bounds(templates);dd=dh-dl
if len(templates)!=1:raise RuntimeError('Donor should be single dressed-stone paver with original texture')
template=templates[0]
# Bake source transform onto geometry. Preserve its UV and material map as exported.
bpy.ops.object.select_all(action='DESELECT')
template.select_set(True);bpy.context.view_layer.objects.active=template
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
dl,dh=bounds([template]);dd=dh-dl
slot=float(span.x)/5.
gap=.012
target_x=slot-gap
scale=(target_x/max(dd.x,.001),span.y/max(dd.y,.001),span.z/max(dd.z,.001))
for k in range(5):
 ob=template if k==0 else template.copy()
 if k>0:ob.data=template.data.copy();bpy.context.collection.objects.link(ob)
 ob.name=f'stair_step_true_joint_stone_{k+1}'
 # Each actual module is an original physically modeled flagstone, not photo panels.
 # Swap orientation in two pieces to break the photograph's repeated directional grain.
 ob.scale=scale
 if k in (1,3):ob.rotation_euler[1]=math.pi
 targetcenter=Vector((float(lo.x)+(k+.5)*slot,float((lo.y+hi.y)/2),float((lo.z+hi.z)/2)))
 bpy.context.view_layer.update()
 low,high=bounds([ob]);ob.location+=targetcenter-(low+high)*.5
bpy.context.view_layer.update()
pieces=[o for o in meshes() if o.name.startswith('stair_step_true_joint_stone_')]
if len(pieces)!=5:raise RuntimeError('5 independent physical stone blocks required')
newlo,newhi=bounds(pieces);dif=newhi-newlo
if max(abs(dif.y-span.y),abs(dif.z-span.z))>.01:raise RuntimeError('Bad module contact dimensions')
if abs(dif.x-(span.x-gap))>.02:raise RuntimeError('Incorrect stair width')
shoot(out/'after'/'stair_step.png')
save_matrix=cam.matrix_world.copy();save_scale=cd.ortho_scale
cd.ortho_scale=2.6;cam.location=center+Vector((.9,-1.4,1)).normalized()*3.4
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
shoot(out/'after'/'stair_step_macro.png')
cam.matrix_world=save_matrix;cd.ortho_scale=save_scale
bpy.ops.object.select_all(action='DESELECT')
for o in pieces:o.select_set(True)
bpy.context.view_layer.objects.active=pieces[0]
glb=out/'glb'/'stair_step.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<20000:raise RuntimeError('No real dressed-stone GLB')
report={'asset':'stair_step','old_sha':sha(src),'donor_sha':sha(donor),'new_sha':sha(glb),
 'donor_asset':'paver_flagstone','stone_count':5,'independent_stones':True,
 'source_4m_dimensions_m':list(span),'result_dimensions_m':list(dif),
 'fits_source_envelope':True,'same_camera':True,'technical':'PASS_PENDING_VISUAL',
 'art_approved':False,'unity_validated':False,'license':'MATERIAL LICENSE PENDING DONOR VERIFICATION'}
(out/'reports'/'stair_five_dressed_stones_v7.json').write_text(json.dumps(report,indent=2))
print('STAIR_FIVE_REAL_DRESSED_STONES_PENDING_VISUAL',sha(glb))
