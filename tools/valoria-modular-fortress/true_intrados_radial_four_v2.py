"""Four keystone/voussoir modules; visible radial geometry MUST actually taper
at arch intrados. Preserve source photo PBR CC0, UV, modular bounds and original.
Fixed single Blender camera A/B; no assembled arch or Unity.
"""
import bpy,sys,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise SystemExit('source four GLB path, output')
src,out=map(Path,args)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=960;s.render.resolution_y=960;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria stone voussoir gallery');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.31,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=w
def light(n,power,pos,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=10
 ob=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('Golden masonry key',1800,(-7,-8,11),(1,.82,.69))
light('Cool overcast fill',1500,(7,3,11),(.68,.81,1))
light('edge',850,(-2,9,9),(.99,.95,.82))
camd=bpy.data.cameras.new('identical wedge gallery lens');cam=bpy.data.objects.new('identical wedge gallery lens',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objects if o.type=='MESH' for v in o.bound_box]
 return Vector([min(q[i] for q in pts) for i in range(3)]),Vector([max(q[i] for q in pts) for i in range(3)])
def setcam(models):
 lo,hi=bounds(models);mid=(lo+hi)*.5;dim=hi-lo
 cam.location=mid+Vector((.85,-1.55,1.21)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(dim.x,dim.y,dim.z,.3)*1.8
def photo(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
record=[]
for n in names:
 pre=set(bpy.data.objects);r={'asset':n,'art_approved':False}
 try:
  original=src/(n+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  meshes=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if len(meshes)!=1:raise RuntimeError('Radial stone should remain one repeatable mesh')
  setcam(meshes);photo(out/'before'/(n+'.png'))
  ob=meshes[0]
  m=ob.data
  coords=[v.co for v in m.vertices]
  xlo,xhi=min(v.x for v in coords),max(v.x for v in coords)
  zlo,zhi=min(v.z for v in coords),max(v.z for v in coords)
  # Blender imports glTF Y-up; z is truly up here after importer.
  height=zhi-zlo;mid=(xlo+xhi)/2
  if height<.05 or xhi-xlo<.1:raise RuntimeError('Stone local vertical or width collapsed')
  for vert in m.vertices:
   radial=(vert.co.z-zlo)/height
   # Real width wedge: intrados 60%, extrados 100%.
   # The older 4-piece "radial" trial had only minimal taper.
   factor=.60+.40*radial
   vert.co.x=mid+(vert.co.x-mid)*factor
  m.update()
  # Only micro bevel to maintain clean physically hand dressed arris.
  mod=ob.modifiers.new('edge transition','BEVEL');mod.width=.008;mod.segments=2;mod.limit_method='ANGLE'
  photo(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
  bpy.context.view_layer.objects.active=ob
  target=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<20000:raise RuntimeError('Stone PBR lost')
  r.update({'technical':'PASS_PENDING_VISUAL','source_sha':sha(original),'candidate_sha':sha(target),
    'intrados_width_ratio':.60,'extrados_width_ratio':1,'geometry_vertices':len(m.vertices),
    'source_run':38872720758})
  print('ARCH_STONE_TRUE_RADIAL_TAPER',n,round(xhi-xlo,3),round(height,3),flush=True)
 except Exception as ex:
  r.update({'technical':'FAILED','error':repr(ex)});traceback.print_exc()
 finally:
  record.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'four_deep_radial_wedges.json').write_text(json.dumps({'no_assembled_arch':True,
    'art_approved':False,'results':record},indent=2))
if any(x['technical']!='PASS_PENDING_VISUAL' for x in record):raise RuntimeError('Four wedge A/B incomplete')
print('FOUR_TRUE_RADIAL_GEOMETRY_CANDIDATES',flush=True)
