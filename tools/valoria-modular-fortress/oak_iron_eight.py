"""Individually finish 8 oak/forged-iron fortress modules using CC0 PBR.
No castle composition, no Unity. Matched before/after Blender renders.
"""
import bpy,sys,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
import numpy as np
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src=Path(arg[0]);maps=Path(arg[1]);out=Path(arg[2])
for d in ('before','after','glb','reports','materials'):(out/d).mkdir(parents=True,exist_ok=True)
names=['reinforced_oak_gate','portcullis_large','portcullis_small',
 'timber_beam_1','timber_beam_2','timber_brace_X','scaffold_small','scaffold_watch_platform']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=900;s.render.resolution_y=900
s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('isolated oak and iron dusk');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.22,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.72;s.world=w
def area(n,p,e,c):
 l=bpy.data.lights.new(n,'AREA');l.energy=e;l.color=c;l.shape='DISK';l.size=9
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=p
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('warm key',(-7,-9,13),1650,(1,.84,.68));area('blue fill',(7,4,12),1580,(.63,.78,1));area('rim',(4,10,10),900,(1,1,1))
cd=bpy.data.cameras.new('unchanged per module lens');cam=bpy.data.objects.new('unchanged per module lens',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(objs):
 p=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 return Vector([min(v[i] for v in p) for i in range(3)]),Vector([max(v[i] for v in p) for i in range(3)])
def set_camera(objs):
 lo,hi=bounds(objs);mid=(lo+hi)/2;ext=hi-lo
 cam.location=mid+Vector((.92,-1.7,1.24)).normalized()*max(ext.length,1)*3.2
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(ext.x,ext.y,ext.z,.3)*1.73
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def photo_material(kind):
 prefix='oak_veneer_05' if kind=='wood' else 'rusty_metal_05'
 p=maps/(prefix+'_diffuse.jpg')
 if not p.exists():raise RuntimeError('No real CC0 '+str(p))
 im=bpy.data.images.load(str(p),check_existing=False);im.scale(768,768)
 pixels=np.empty(768*768*4,dtype=np.float32);im.pixels.foreach_get(pixels)
 rgb=pixels.reshape(768,768,4)[:,:,:3];luma=rgb@np.array([.30,.59,.11],dtype=np.float32)
 delta=np.clip((luma-luma.mean())/(luma.std()+.03)*(.19 if kind=='wood' else .12),-.29,.30)
 base=np.array((.29,.18,.095) if kind=='wood' else (.13,.14,.145),dtype=np.float32)
 rgb[:]=np.clip(base[None,None,:]*(1+delta[:,:,None]),.025,.92)
 dest=bpy.data.images.new('ELDORIA_'+kind+'_CC0_photo_atlas',width=768,height=768,alpha=True)
 dest.pixels.foreach_set(pixels);dest.update()
 out_file=out/'materials'/(kind+'_photo_cc0.png');dest.filepath_raw=str(out_file);dest.file_format='PNG';dest.save()
 bpy.data.images.remove(im)
 mat=bpy.data.materials.new('Valoria_Tested_CC0_'+kind);mat.use_nodes=True;nodes=mat.node_tree.nodes;nodes.clear();link=mat.node_tree.links
 tex=nodes.new('ShaderNodeTexImage');tex.image=dest;tex.location=(-410,85)
 bs=nodes.new('ShaderNodeBsdfPrincipled')
 bs.inputs['Roughness'].default_value=.80 if kind=='wood' else .52
 bs.inputs['Metallic'].default_value=0 if kind=='wood' else .76
 link.new(tex.outputs['Color'],bs.inputs['Base Color'])
 npth=maps/(prefix+'_nor_gl.jpg')
 if npth.is_file():
  source=bpy.data.images.load(str(npth),check_existing=True)
  source.colorspace_settings.name='Non-Color'
  ntex=nodes.new('ShaderNodeTexImage');ntex.image=source
  nor=nodes.new('ShaderNodeNormalMap');nor.inputs['Strength'].default_value=.19 if kind=='wood' else .14
  link.new(ntex.outputs['Color'],nor.inputs['Color'])
  link.new(nor.outputs['Normal'],bs.inputs['Normal'])
 o=nodes.new('ShaderNodeOutputMaterial');link.new(bs.outputs[0],o.inputs['Surface'])
 return mat
wood=photo_material('wood');iron=photo_material('iron')
def wood_uv(ob):
 mesh=ob.data
 if not mesh.uv_layers:mesh.uv_layers.new(name='UVMap')
 arr=[v.co for v in mesh.vertices]
 lo=[min(v[i] for v in arr) for i in range(3)]
 hi=[max(v[i] for v in arr) for i in range(3)]
 dims=[hi[i]-lo[i] for i in range(3)]
 along=max(range(3),key=lambda i:dims[i])
 other=0 if along!=0 else 1
 layer=mesh.uv_layers.active
 for p in mesh.polygons:
  for idx in p.loop_indices:
   pos=mesh.vertices[mesh.loops[idx].vertex_index].co
   layer.data[idx].uv=(pos[along]*.6,pos[other]*.6)
def details(n,meshes):
 added=[];added_objects=0
 if n.startswith('portcullis_'):
  lo,hi=bounds(meshes)
  width=hi.x-lo.x
  # Simulate artisan rivets on intersecting bars: 8-by-4 metal heads.
  for i in range(8):
   x=lo.x+(i+.5)*width/8
   for j in range(4):
    z=lo.z+(j+1)*(hi.z-lo.z)/5
    bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=.032 if 'small' in n else .046,location=(x,lo.y-.055,z))
    ob=bpy.context.object;ob.name=n+'_forged_rivet';ob.data.materials.append(iron);added.append(ob)
 elif n=='reinforced_oak_gate':
  woodobjs=[m for m in meshes if any(k and 'wood' in k.name.lower() for k in m.data.materials)]
  if woodobjs:
   wl,wh=bounds(woodobjs);gwidth=wh.x-wl.x
   for o in meshes:
    if any(m and 'iron' in m.name.lower() for m in o.data.materials):
     if o.dimensions.x>gwidth*1.05:o.dimensions.x=gwidth*.92
   for x in (wl.x+.14*gwidth,wh.x-.14*gwidth):
    for z in (wl.z+.22*(wh.z-wl.z),wl.z+.77*(wh.z-wl.z)):
     bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=.043,location=(x,wl.y-.12,z))
     o=bpy.context.object;o.name=n+'_oak_gate_pin';o.data.materials.append(iron);added.append(o)
 elif n=='timber_brace_X':
  lo,hi=bounds(meshes);mid=(lo+hi)*.5
  bpy.ops.mesh.primitive_cube_add(size=1,location=(mid.x,lo.y-.075,mid.z))
  o=bpy.context.object;o.name='oak_X_joint_iron_clasp';o.dimensions=(.28,.08,.34);o.data.materials.append(iron);added.append(o)
 return added
reports=[]
for index,n in enumerate(names,1):
 begin=set(bpy.data.objects);r={'asset':n,'minimum':8,'art_approved':False}
 try:
  p=src/(n+'.glb');bpy.ops.import_scene.gltf(filepath=str(p))
  models=[o for o in bpy.data.objects if o not in begin and o.type=='MESH']
  if not models:raise RuntimeError('Zero meshes')
  set_camera(models);shoot(out/'before'/(n+'.png'))
  original_mats={o.name:[m.name if m else 'none' for m in o.data.materials] for o in models}
  changed=0
  added=details(n,models)
  for o in models:
   for j,m in enumerate(o.data.materials):
    if m is None:continue
    kind=m.name.lower().split('.')[0]
    if '_wood' in kind:
     o.data.materials[j]=wood;wood_uv(o);changed+=1
    elif '_iron' in kind:
     o.data.materials[j]=iron;changed+=1
  if changed==0:raise RuntimeError('No timber or iron material changes recognized; model names '+str(original_mats)[:800])
  shoot(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in models+added:o.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  dest=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<12000:raise RuntimeError('Unexpected empty output')
  r.update({'technical':'PASS_PENDING_ART','changed_material_slots':changed,'added_iron_details':len(added),
    'source_sha':sha(p),'candidate_sha':sha(dest),'source_mats':original_mats})
  print('OAK_IRON_INDIVIDUAL',index,'/',len(names),n,changed,'details',len(added),flush=True)
 except Exception as e:
  r.update({'technical':'FAILED','error':str(e)});traceback.print_exc()
 finally:
  reports.append(r)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0:bpy.data.images.remove(im)
  (out/'reports'/'oak_iron_8_status.json').write_text(json.dumps({'no_composition':True,
    'status':'CANDIDATE_ONLY','source_run':38813966671,'cc0_source':38823501643,
    'art_approved':False,'assets':reports},indent=2))
if len(reports)!=8 or any(r['technical']!='PASS_PENDING_ART' for r in reports):raise RuntimeError('Incomplete 8 module batch')
print('OAK_IRON_8_TECH_PASS',flush=True)
