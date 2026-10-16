"""One individual wall V8: TEST 12 larger authored angular stone chips at mortar
contacts on genuine CC0 photostone V7. Prevent rubbery spherical gouges.
Preserve 84 ashlar structure/UV/3D stone top; no unrelated composition.
"""
import bpy,sys,random,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,arg)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
if not source.is_file():raise RuntimeError('Missing V7 4m warm limestone')
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1140;sc.render.resolution_y=950
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('single reference limestone wall');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.2,.25,.32,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;sc.world=w
def lamp(n,power,pos,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=10
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm',1800,(-8,-9,13),(1,.85,.72))
lamp('cool',1600,(8,2,11),(.7,.82,1))
lamp('rim',850,(-3,10,9),(.98,.95,.88))
ca=bpy.data.cameras.new('locked A B photo');cam=bpy.data.objects.new('locked A B photo',ca)
bpy.context.collection.objects.link(cam);sc.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(source))
objects=[o for o in bpy.data.objects if o not in rig]
mesh=[o for o in objects if o.type=='MESH']
def bbox(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
lo,hi=bbox(mesh)
mid=(lo+hi)*.5;ext=hi-lo
cam.location=mid+Vector((.8,-1.56,1.11)).normalized()*max(ext.length,1)*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
ca.ortho_scale=max(ext.x,ext.y,ext.z)*1.62
def render(path):sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
render(out/'before'/'wall_panel_regular_4m.png')
# Only original FRONT ashlar modules (not the mortar core, end quoins or caps).
stones=[o for o in mesh if 'individual_ashlar_r' in o.name.lower() and 'front' in o.name.lower()]
if len(stones)<30:raise RuntimeError('Expected real front masonry, got '+str(len(stones)))
rng=random.Random(7048)
targets=sorted(stones,key=lambda o:o.name)
# Visually distributed weathering, not an even repeated pattern.
chosen=[targets[i] for i in [2,6,9,13,16,20,24,28,31,34,37,40] if i<len(targets)]
if len(chosen)<9:raise RuntimeError('Missing intended distributed weathering contacts')
record=[]
for index,ob in enumerate(chosen):
 try:
  box0,box1=bbox([ob])
  side=-1 if rng.random()<.5 else 1
  # Cutter is outside the stone, intruding on one real corner by ~7cm.
  x=float(box0.x if side<0 else box1.x)
  y=float(box0.y)
  z=float(box1.z if rng.random()<.6 else box0.z)
  radius=rng.uniform(.052,.088)
  center=(x+side*rng.uniform(-.012,.025),y-rng.uniform(-.018,.035),
          z+rng.uniform(-.006,.018))
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,location=center)
  cutter=bpy.context.object;cutter.name='TEMP_ANGULAR_HAND_DRESSED_STONE_FLAKE'
  cutter.scale=(rng.uniform(.78,1.28),rng.uniform(.85,1.12),rng.uniform(.55,.91))
  cutter.rotation_euler=(rng.uniform(-1,1),rng.uniform(-1,1),rng.uniform(-1,1))
  mod=ob.modifiers.new('nonrepeating physical chipped limestone '+str(index),'BOOLEAN')
  mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
  bpy.ops.object.select_all(action='DESELECT')
  ob.select_set(True);bpy.context.view_layer.objects.active=ob
  bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.data.objects.remove(cutter,do_unlink=True)
  record.append({'mesh':ob.name,'chip_radius_m':round(radius,4),
                 'result_polygons':len(ob.data.polygons)})
  print('STONE_TRUE_CHIP',index+1,'/',len(chosen),ob.name,len(ob.data.polygons),flush=True)
 except Exception as exc:
  traceback.print_exc();raise RuntimeError('Failed real chipped-stone Boolean '+str(index)+': '+str(exc))
render(out/'after'/'wall_panel_regular_4m.png')
bpy.ops.object.select_all(action='DESELECT')
for ob in objects:ob.select_set(True)
bpy.context.view_layer.objects.active=mesh[0]
glb=out/'glb'/'wall_panel_regular_4m.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<400000:raise RuntimeError('Incomplete chipped wall GLB')
details={
 'asset':'wall_panel_regular_4m','technical':'PASS_PENDING_ART',
 'chip_count':len(record),'chips':record,'source_sha':sha(source),
 'result_sha':sha(glb),'source_run':38865543613,
 'goal':'Worn artisan irregular stone joins, not noise/color-only changes',
 'substantive_intervention_test':True,'no_assembly':True,
 'art_approved':False,'unity_approved':False,
 'warning':'If chips are visually too imperceptible or like random holes, reject and preserve unchipped V7.'
}
(out/'reports'/'artisan_contact_chips_v8.json').write_text(json.dumps(details,indent=2))
print('WALL_V8_ACTUAL_3D_CHIPS_TEST_COMPLETE',len(record),flush=True)
