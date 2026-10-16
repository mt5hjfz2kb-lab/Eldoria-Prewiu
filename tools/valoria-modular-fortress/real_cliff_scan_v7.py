"""ELDORIA 2026-10-16: replace only faceted cliff_rock_column with exact CC0 photogrammetry.
Standalone 1 GLB/2 matched Blender screenshots; Unity/Bastion/4X untouched."""
import bpy,sys,json,hashlib
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:]
if len(argv)!=3:raise RuntimeError('current_cliff donor_rock_face02 output')
old,donor,out=Path(argv[0]),Path(argv[1]),Path(argv[2])
for x in ('before','after','glb','reports'):(out/x).mkdir(parents=True,exist_ok=True)
def sha(x):return hashlib.sha256(x.read_bytes()).hexdigest()
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1180;s.render.resolution_y=900;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria neutral cliff photostudio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.42,.47,.51,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
for name,power,loc,rgb in [('warm light',1650,(-7,-9,12),(1,.88,.76)),('cool fill',1250,(8,6,11),(.77,.86,1))]:
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.size=9;l.color=rgb
 obj=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(obj)
 obj.location=loc;obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
c=bpy.data.cameras.new('locked studio A B');cam=bpy.data.objects.new('locked studio A B',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
def meshes():return [o for o in bpy.context.scene.objects if o.type=='MESH']
def bounds(obs):
 bpy.context.view_layer.update()
 q=[o.matrix_world@Vector(v) for o in obs for v in o.bound_box]
 return Vector(tuple(min(z[i] for z in q) for i in range(3))),Vector(tuple(max(z[i] for z in q) for i in range(3)))
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(old))
previous=meshes();assert previous
low,high=bounds(previous);target=high-low;center=(low+high)*.5
cam.location=center+Vector((1,-1.7,1.2)).normalized()*target.length*2.6
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
c.ortho_scale=max(target.length*1.20,6)
render(out/'before'/'cliff_rock_column.png')
for ob in previous:bpy.data.objects.remove(ob,do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(donor))
current=meshes();assert current
low,high=bounds(current);dim=high-low
factor=Vector(tuple(target[i]/max(float(dim[i]),.01) for i in range(3)))
if max(factor)/min(factor)>4.8:raise RuntimeError('Excessive nonuniform scan normalization '+str(factor))
for ob in current:
 ob.scale.x*=factor.x;ob.scale.y*=factor.y;ob.scale.z*=factor.z
low,high=bounds(current);delta=center-(high+low)*.5
for ob in current:ob.location+=delta
low,high=bounds(current);actual=high-low
maxerr=max(abs(target[i]-actual[i]) for i in range(3))
if maxerr>.03:raise RuntimeError('Source modular envelope incompatible '+str(maxerr))
render(out/'after'/'cliff_rock_column.png')
c.ortho_scale*=.80
render(out/'after'/'cliff_rock_column_macro.png')
bpy.ops.object.select_all(action='DESELECT')
for ob in current:ob.select_set(True)
bpy.context.view_layer.objects.active=current[0]
glb=out/'glb'/'cliff_rock_column.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<100000:raise RuntimeError('CC0 scan glb absent')
report={'asset':'cliff_rock_column','source_run':38825223115,'donor_run':38825975637,
 'donor':'Poly Haven rock_face_02 real photogrammetry CC0',
 'original_sha':sha(old),'source_donor_sha':sha(donor),'candidate_sha':sha(glb),
 'target_dimensions_m':list(target),'candidate_dimensions_m':list(actual),
 'fit_error_m':float(maxerr),'relative_axis_scale':list(factor),
 'same_camera':True,'real_blender_glb':True,'art_approved':False,'unity_validated':False,
 'status':'TECH_PASS_PENDING_INDEPENDENT_VISUAL'}
(out/'reports'/'cliff_real_scan_v7.json').write_text(json.dumps(report,indent=2))
print('REAL_CLIFF_CC0_WAIT_VISUAL',sha(glb),maxerr,flush=True)