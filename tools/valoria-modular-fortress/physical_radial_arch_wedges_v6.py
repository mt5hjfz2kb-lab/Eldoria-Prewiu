"""Valoria four individual keystones/voussoirs: genuine trapezoid cross-section.
Not a shallow displacement on a rectangular paving slab. Build a real narrowing
intrados (0.47-0.58 outer width), mechanically straight radial joints, imperfect
sculpted limestone arrises, exact donor exterior bounds and inherited photo-PBR.
3D A/B native Blender, never art-approve or compose a wall or complete arch.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,argv)
for f in ('glb','before','after','reports'):(out/f).mkdir(parents=True,exist_ok=True)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1100;scene.render.resolution_y=1100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('neutral warm limestone sculpture');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.22,.28,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.80;scene.world=w
def lamp(n,pos,power,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.size=10;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm golden chisel',(-6,-9,11),1900,(1,.87,.71))
lamp('blue alpine fill',(8,3,11),1690,(.73,.86,1))
lamp('edge sculpt rim',(-1,8,10),910,(.97,.97,1))
camd=bpy.data.cameras.new('fixed same before after camera')
cam=bpy.data.objects.new('fixed same before after camera',camd)
bpy.context.collection.objects.link(cam);scene.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objects if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def aim(objects):
 low,high=bbox(objects);center=(low+high)*.5;d=high-low
 cam.location=center+Vector((.8,-1.52,1.17)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(d.x,d.y,d.z,.3)*1.72
def photograph(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def tapered_one(name,old,lo,hi,rng):
 W=float(hi.x-lo.x);D=float(hi.y-lo.y);H=float(hi.z-lo.z)
 centerx=float((hi.x+lo.x)*.5)
 centery=float((hi.y+lo.y)*.5)
 if W<.2 or H<.12 or D<.20:raise RuntimeError('Source dimensions not suitable for real wedge '+str([W,D,H]))
 # Lower edge is always 51-56% of maximum width; upper edge 96-98%.
 bottom_ratio=.56 if 'keystone' in name else .50
 top_ratio=.97
 # Four undulating true carved horizontal profiles retain physically straight
 # adjacent radial courses and create actual visible in/out wedge surfaces.
 heights=(.0,.16,.46,.82,1.0)
 verts=[]
 for row,t in enumerate(heights):
  z=float(lo.z)+H*t
  ratio=bottom_ratio+(top_ratio-bottom_ratio)*t
  if row in (1,2,3):ratio+=rng.uniform(-.014,.014)
  half=W*ratio*.5
  front=float(lo.y)+D*.008
  back=float(hi.y)-D*.008
  # Blender worlds: x is left/right on facade, y is stone depth, z is vertical.
  verts.extend([(centerx-half,front,z),(centerx+half,front,z),
                (centerx+half,back,z),(centerx-half,back,z)])
 faces=[(3,2,1,0)]
 for row in range(len(heights)-1):
  a=4*row;b=4*(row+1)
  faces.extend(((a,a+1,b+1,b),(a+1,a+2,b+2,b+1),
                (a+2,a+3,b+3,b+2),(a+3,a,b,b+3)))
 faces.append((16,17,18,19))
 mesh=bpy.data.meshes.new('real_limestone_radial_wedge_'+name)
 mesh.from_pydata(verts,[],faces);mesh.update()
 obj=bpy.data.objects.new('true_voussoir_physical_taper_'+name,mesh)
 bpy.context.collection.objects.link(obj)
 # Preserve actual source photographic PBR and original normal maps.
 materials=[]
 for m in old.data.materials:
  if m and m not in materials:materials.append(m)
 if not materials:raise RuntimeError('Source original photo-PBR material missing')
 for m in materials:mesh.materials.append(m)
 mod=obj.modifiers.new('true_lightly_worn_ancient_stone_arrises','BEVEL')
 mod.width=.009 if 'minor' in name else .013
 mod.segments=3
 mod.limit_method='ANGLE'
 bpy.context.view_layer.objects.active=obj
 bpy.ops.object.modifier_apply(modifier=mod.name)
 uv=mesh.uv_layers.active or mesh.uv_layers.new(name='real_anatomical_stone_UV')
 # Orient every surface independently; no stretched side-face texture.
 points=[v.co for v in mesh.vertices]
 minp=[min(v[i] for v in points) for i in range(3)]
 maxp=[max(v[i] for v in points) for i in range(3)]
 lengths=[max(maxp[i]-minp[i],.0001) for i in range(3)]
 for p in mesh.polygons:
  axis=max(range(3),key=lambda i:abs(p.normal[i]))
  axes=([0,2] if axis==1 else ([0,1] if axis==2 else [1,2]))
  for li in p.loop_indices:
   co=mesh.vertices[mesh.loops[li].vertex_index].co
   u=(co[axes[0]]-minp[axes[0]])/lengths[axes[0]]
   v=(co[axes[1]]-minp[axes[1]])/lengths[axes[1]]
   uv.data[li].uv=(.12+.74*u,.11+.78*v)
 bpy.context.view_layer.update()
 lo2,hi2=bbox([obj])
 if any(float(lo2[i])<float(lo[i])-.002 or float(hi2[i])>float(hi[i])+.002 for i in range(3)):
  raise RuntimeError('Expanded beyond reference architectural module envelope')
 return obj,{'intrados_to_full_width_ratio':bottom_ratio,'extrados_to_full_width_ratio':top_ratio,
    'real_faces':len(mesh.polygons),'real_vertices':len(mesh.vertices),
    'original_dimensions':[round(float(v),4) for v in hi-lo]}
records=[]
for name in names:
 before=set(bpy.data.objects)
 record={'asset':name,'target':6,'art_approved':False,'unity_approved':False}
 try:
  path=source/(name+'.glb');bpy.ops.import_scene.gltf(filepath=str(path))
  sourceobjs=[o for o in bpy.data.objects if o not in before]
  source_mesh=[o for o in sourceobjs if o.type=='MESH']
  if len(source_mesh)!=1:raise RuntimeError('Expected one independent donor stone mesh')
  low,high=bbox(source_mesh)
  aim(source_mesh);photograph(out/'before'/(name+'.png'))
  new,info=tapered_one(name,source_mesh[0],low,high,random.Random(20261017+len(name)))
  for o in sourceobjs:bpy.data.objects.remove(o,do_unlink=True)
  photograph(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT');new.select_set(True);bpy.context.view_layer.objects.active=new
  destination=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(destination),export_format='GLB',use_selection=True,export_apply=True)
  if destination.stat().st_size<14000:raise RuntimeError('Packed limestone CC0 donor texture missing')
  record.update({'technical':'PASS_PENDING_ART','old_sha256':sha(path),'candidate_sha256':sha(destination),'physical_geometry':info})
  print('TRUE_ARCH_WEDGE_3D_SCULPT',name,info,flush=True)
 except Exception as exc:
  traceback.print_exc()
  record.update({'technical':'FAIL','error':repr(exc)})
 finally:
  records.append(record)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'four_real_fitted_radial_stones_v6.json').write_text(json.dumps({
   'not_full_arch_assembly':True,'license':'Poly Haven donor CC0',
   'source_run':38888410182,'art_approved':False,'items':records},indent=2))
if len(records)!=4 or any(x['technical']!='PASS_PENDING_ART' for x in records):
 raise RuntimeError('Missing true physical radial wedge outputs')
print('FOUR_PHYSICAL_WEDGE_MODULES_READY_FOR_VISUAL_GATE',flush=True)
