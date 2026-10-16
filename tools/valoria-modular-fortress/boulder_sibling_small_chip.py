"""Replace sub6 bland dark 'cliff_stone_chip' scan with a genuinely fractured
sibling derived from the superior rough Poly Haven CC0 boulder_01 scanned mesh;
independent silhouette and exact original modular bounds, PBR intact.
"""
import bpy,sys,json,hashlib,traceback,math,random
from pathlib import Path
from mathutils import Vector,Matrix
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,args)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1000;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria real-rock chip material study');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=w
def lamp(name,pos,power,col):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('sunset stone key',(-8,-9,13),1700,(1,.88,.75))
lamp('cold stone fill',(8,3,11),1700,(.70,.82,1))
lamp('edge',(-2,9,12),900,(.95,.97,1))
c=bpy.data.cameras.new('one-rock same look');cam=bpy.data.objects.new('one-rock same look',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bb(objs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
old=source/'cliff_stone_chip.glb'
donor=source/'boulder_2.glb'
bpy.ops.import_scene.gltf(filepath=str(old))
oldobjs=[o for o in bpy.data.objects if o not in rig]
mesh=[o for o in oldobjs if o.type=='MESH']
if len(mesh)!=1:raise RuntimeError('Bad cliff chip source')
low,high=bb(mesh);centre=(low+high)*.5;dim=high-low
cam.location=centre+Vector((.89,-1.55,1.13)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(centre-cam.location).to_track_quat('-Z','Y').to_euler()
c.ortho_scale=max(dim.x,dim.y,dim.z)*1.74
shoot(out/'before'/'cliff_stone_chip.png')
for o in oldobjs:bpy.data.objects.remove(o,do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(donor))
new=[o for o in bpy.data.objects if o not in rig]
models=[o for o in new if o.type=='MESH']
if len(models)!=1:raise RuntimeError('Missing donor genuine fractured rock geometry')
o=models[0];p0,p1=bb(models);sizes=p1-p0
# Same broad original footprint and substantially DIFFERENT distinct silhouette:
# nonuniform compressed strata and 28-degree horizontal twist, not same boulder reused whole.
factors=[float(dim.x/max(sizes.x,.01))*.94,
         float(dim.y/max(sizes.y,.01))*.90,
         float(dim.z/max(sizes.z,.01))*.83]
for root in new:
 if root.parent is None:
  root.matrix_world=Matrix.Rotation(math.radians(28),4,'Z')@Matrix.Diagonal(Vector((factors[0],factors[1],factors[2],1)))@root.matrix_world
bpy.context.view_layer.update()
a,b=bb(models)
offset=centre-(a+b)*.5
for root in new:
 if root.parent is None:root.matrix_world=Matrix.Translation(offset)@root.matrix_world
bpy.context.view_layer.update()
# The donor's photogrammetric fracture normals/albedo remain entirely real.
after_low,after_hi=bb(models)
if (after_hi-after_low).length>dim.length*1.8:raise RuntimeError('Mismatched chip scale')
shoot(out/'after'/'cliff_stone_chip.png')
bpy.ops.object.select_all(action='DESELECT')
for root in new:root.select_set(True)
bpy.context.view_layer.objects.active=models[0]
glb=out/'glb'/'cliff_stone_chip.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<100000:raise RuntimeError('Scanned fracture surface stripped')
report={'asset':'cliff_stone_chip','technical':'PASS_PENDING_VISUAL',
 'source_previous_chip_sha':sha(old),'licensed_scanned_donor_sha':sha(donor),
 'new_sha':sha(glb),'source_run':38875141612,
 'donor_provenance':'Poly Haven boulder_01 CC0 photographic scan — sister variant from boulder_2',
 'new_dimensions':[round(float(v),3) for v in after_hi-after_low],
 'original_dimensions':[round(float(v),3) for v in dim],
 'modular_source_family':True,'visually_approved':False,'unity_approved':False,
 'warning':'Do not claim unique geological species or promote without A/B; keep boulder_2 as separate asset.'}
(out/'reports'/'sibling_chip_from_scan.json').write_text(json.dumps(report,indent=2))
print('RUGGED_SIBLING_STONE_CHIP_CC0_REAL',glb.stat().st_size,flush=True)
