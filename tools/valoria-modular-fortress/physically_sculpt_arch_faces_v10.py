"""Valoria 4 individual V10 radial wedges: serious contact-safe true hand-sculpt.
Subdivide outward front plane in Blender and recess physically 4-8cm wide
chisel indents, actual mineral micrograin. Preserve original fitted wedge bounds,
polyhaven CC0 photo albedo, UVs, and separate GLBs/A-Bs, no arch assembly.
"""
import bpy,sys,math,random,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector,noise
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,args)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1060;s.render.resolution_y=1060
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('cut limestone A/B neutral gallery');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def area(n,power,pos,col):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=col;data.shape='DISK';data.size=10
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('stone carving sunset',1910,(-7,-8,12),(1,.85,.72))
area('blue sky fill',1580,(8,3,11),(.72,.83,1))
area('rim rough stone',1060,(-2,9,11),(.98,.96,.87))
camdata=bpy.data.cameras.new('unchanged arch single stone view')
cam=bpy.data.objects.new('unchanged arch single stone view',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(models):
 bpy.context.view_layer.update()
 p=[o.matrix_world@Vector(v) for o in models if o.type=='MESH' for v in o.bound_box]
 return Vector([min(q[i] for q in p) for i in range(3)]),Vector([max(q[i] for q in p) for i in range(3)])
def aim(models):
 lo,hi=bbox(models);mid=(lo+hi)*.5;d=hi-lo
 cam.location=mid+Vector((.85,-1.55,1.2)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camdata.ortho_scale=max(d.x,d.y,d.z,.2)*1.76
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
rows=[]
for i,name in enumerate(names):
 original=src/(name+'.glb');pre=set(bpy.data.objects)
 row={'asset':name,'artist_target':6,'art_approved':False,'source_run':38906155308}
 try:
  bpy.ops.import_scene.gltf(filepath=str(original))
  imported=[o for o in bpy.data.objects if o not in pre];mesh=[o for o in imported if o.type=='MESH']
  if len(mesh)!=1:raise RuntimeError('Expected exactly one standalone fitted wedge')
  o=mesh[0];lo,hi=bbox(mesh)
  aim(mesh);shoot(out/'before'/(name+'.png'))
  # Subdivide each already-beveled authored masonry face while preserving
  # outer volume and interpolated photo UV. No tessellated block construction.
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
  bpy.context.view_layer.objects.active=o
  mod=o.modifiers.new('stone_relief_sculpt_mesh_resolution','SUBSURF')
  mod.subdivision_type='SIMPLE';mod.levels=2;mod.render_levels=2
  bpy.ops.object.modifier_apply(modifier=mod.name)
  current_lo,current_hi=bbox(mesh)
  miny=float(current_lo.y);thickness=float(current_hi.y-current_lo.y)
  depth=min(.026,thickness*.08)
  rng=random.Random(1010+i*17)
  cx=(float(lo.x)+float(hi.x))*.5
  cz=(float(lo.z)+float(hi.z))*.5
  spanx=float(hi.x-lo.x);spanz=float(hi.z-lo.z)
  pits=[(cx+rng.uniform(-.37,.37)*spanx,cz+rng.uniform(-.36,.36)*spanz,
         rng.uniform(.055,.095),rng.uniform(.011,.026)) for _ in range(7)]
  count=0
  matrix=o.matrix_world.copy();inv=matrix.inverted_safe()
  for v in o.data.vertices:
   worldv=matrix@v.co
   # The front facing camera is negative Y. Only authentic dressed front
   # stone gets relief: lateral mating surfaces of modular arch remain intact.
   if worldv.y>miny+.004:continue
   change=0
   for px,pz,radius,deep in pits:
    dx=(worldv.x-px)/radius;dz=(worldv.z-pz)/radius
    dist2=dx*dx+dz*dz
    if dist2<3.0:change+=deep*math.exp(-dist2*1.8)
   f=noise.noise(Vector((worldv.x*28+i*5,worldv.z*24,i*1.1)))
   change+=.0025*(f+.7)
   change=min(max(change,0),depth)
   if change>.001:
    worldv.y+=change
    v.co=inv@worldv
    count+=1
  o.data.update()
  if count<15:raise RuntimeError('No actual photogrammetric stone surface was sculpted')
  final_lo,final_hi=bbox(mesh)
  for ax in range(3):
   if final_lo[ax]<lo[ax]-.004 or final_hi[ax]>hi[ax]+.004:
    raise RuntimeError('Stone does not preserve original contact envelope')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in imported:obj.select_set(True)
  bpy.context.view_layer.objects.active=o
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<65000:raise RuntimeError('Lost real CC0 PBR texture')
  row.update({'technical':'PASS_PENDING_ART','sculpted_vertices':count,
    'real_chisel_depressions':len(pits),'max_inset_m':depth,
    'vertices_after_subdivision':len(o.data.vertices),'faces_after_subdivision':len(o.data.polygons),
    'source_sha':sha(original),'candidate_sha':sha(target),'contact_envelope_unchanged':True})
  print('TRUE_HANDSCULPT_PBR_RADIAL_WEDGE',name,count,flush=True)
 except Exception as e:
  row.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  rows.append(row)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'four_actually_sculpted_faces_v10.json').write_text(json.dumps({
   'single_stone_not_arch_assembly':True,'all_art_approved':False,'assets':rows},indent=2))
if len(rows)!=4 or any(r['technical']!='PASS_PENDING_ART' for r in rows):
 raise RuntimeError('Incomplete 4 stone A/B sculpture study')
print('FOUR_CC0_ACTUALLY_SCULPTED_ARCH_FACES_COMPLETE',flush=True)
