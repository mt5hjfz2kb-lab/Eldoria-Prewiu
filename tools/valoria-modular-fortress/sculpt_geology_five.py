"""Real geologic silhouettes: four Quaternius boulder donors and custom stratified cliff face.
Exactly 5 independent GLBs, same-camera 10 frames. Zero commercial assets.
"""
import bpy,sys,math,json,random,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
base=Path(arg[0]);donor=Path(arg[1]);maps=Path(arg[2]);out=Path(arg[3])
for p in ('glb','before','after','materials','reports'):(out/p).mkdir(parents=True,exist_ok=True)
names=['boulder_1','boulder_2','boulder_3','cliff_stone_chip','cliff_rock_column']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=950;s.render.resolution_y=950;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Neutral geologic lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.75;s.world=w
def area(n,power,loc,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=loc
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('cool key',1950,(-8,-10,13),(.78,.88,1))
area('warm fill',900,(7,2,10),(1,.82,.69))
area('rim',1100,(0,11,12),(.88,.93,1))
c=bpy.data.cameras.new('individual locked frame');cam=bpy.data.objects.new('individual locked frame',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bb(objects):
 pts=[o.matrix_world@Vector(v) for o in objects for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def cam_for(objs):
 lo,hi=bb(objs);m=(lo+hi)/2;extent=hi-lo
 cam.location=m+Vector((.9,-1.5,1.1)).normalized()*max(extent.length,1)*2.8
 cam.rotation_euler=(m-cam.location).to_track_quat('-Z','Y').to_euler()
 c.ortho_scale=max(extent.x,extent.y,extent.z,.5)*1.77
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def rock_material(name,base_color):
 p=maps/('quarry_wall_diffuse.jpg' if 'cliff' in name else 'rock_3_diffuse.jpg')
 im=bpy.data.images.load(str(p),check_existing=False);im.scale(768,768)
 pixels=np.empty(768*768*4,dtype=np.float32);im.pixels.foreach_get(pixels)
 rgb=pixels.reshape((768,768,4))[:,:,:3]
 intensity=np.dot(rgb,np.array([.3,.59,.11],dtype=np.float32))
 delta=np.clip((intensity-intensity.mean())/(intensity.std()+.03)*.16,-.25,.27)
 rgb[:]=np.clip(np.array(base_color,dtype=np.float32)[None,None,:]*(1+delta[:,:,None]),.02,.9)
 outimage=bpy.data.images.new('Eldoria_'+name+'_rock_Cc0',width=768,height=768,alpha=True)
 outimage.pixels.foreach_set(pixels);outimage.update()
 png=out/'materials'/(name+'.png');outimage.filepath_raw=str(png);outimage.file_format='PNG';outimage.save()
 bpy.data.images.remove(im)
 mat=bpy.data.materials.new('Dark_Valoria_Rock_'+name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Metallic'].default_value=0;bs.inputs['Roughness'].default_value=.88
 diffuse=nd.new('ShaderNodeTexImage');diffuse.image=outimage;lk.new(diffuse.outputs['Color'],bs.inputs['Base Color'])
 norm=maps/('quarry_wall_nor_gl.jpg' if 'cliff' in name else 'rock_3_nor_gl.jpg')
 if norm.is_file():
  imN=bpy.data.images.load(str(norm),check_existing=True);imN.colorspace_settings.name='Non-Color'
  tn=nd.new('ShaderNodeTexImage');tn.image=imN
  normalnode=nd.new('ShaderNodeNormalMap');normalnode.inputs['Strength'].default_value=.32
  lk.new(tn.outputs['Color'],normalnode.inputs['Color']);lk.new(normalnode.outputs['Normal'],bs.inputs['Normal'])
 output=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs[0],output.inputs['Surface'])
 return mat
def uv(o):
 if o.data.uv_layers:return
 l=o.data.uv_layers.new(name='UVMap')
 for p in o.data.polygons:
  ax=max(range(3),key=lambda i:abs(p.normal[i]))
  uvaxes=[i for i in range(3) if i!=ax]
  for k in p.loop_indices:
   co=o.data.vertices[o.data.loops[k].vertex_index].co
   l.data[k].uv=(co[uvaxes[0]]*.55,co[uvaxes[1]]*.55)
def cliff_body(mat,dark):
 objs=[]
 # Staggered separate rocky plates plus a dark backing, as one repeatable rock-face kit unit.
 rng=random.Random(446)
 for j in range(6):
  for i in range(2):
   xx=(-.72 if i==0 else .72)+rng.uniform(-.13,.13)
   z=j*1.02+rng.uniform(-.12,.10)
   width=rng.uniform(1.65,2.12)
   height=rng.uniform(1.02,1.4)
   yfront=rng.uniform(-.75,-1.13)
   slope=rng.uniform(-.22,.23)
   # Slab front has four corner faces and an angular midpoint for planar natural rock breaks.
   v=[(xx-width*.52,yfront,z),(xx+width*.45,yfront+slope,z+.06),
       (xx+width*.47,yfront+slope,height+z),(xx-width*.5,yfront,z+height),
       (xx,yfront-rng.uniform(.08,.28),z+height*.53)]
   back=[(x,y+.92,zz) for x,y,zz in v[:4]]
   vertices=v+back
   faces=[(0,1,4),(1,2,4),(2,3,4),(3,0,4),
          (7,6,5,8),(5,6,1,0),(6,7,2,1),(7,8,3,2),(8,5,0,3)]
   mesh=bpy.data.meshes.new('strata_'+str(j)+'_'+str(i))
   mesh.from_pydata(vertices,[],faces);mesh.update()
   ob=bpy.data.objects.new('rock_plate_'+str(j)+'_'+str(i),mesh)
   bpy.context.collection.objects.link(ob)
   ob.data.materials.append(mat if (i+j)%4 else dark);uv(ob);objs.append(ob)
 bpy.ops.mesh.primitive_cube_add(size=1,location=(0,.35,3.05))
 backing=bpy.context.object;backing.name='rock_strata_shadow_backing';backing.dimensions=(3.2,.75,6.25)
 backing.data.materials.append(dark);objs.append(backing)
 return objs
def dark_material():
 m=bpy.data.materials.new('Valoria_deep_joint_rock');m.diffuse_color=(.10,.11,.14,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.10,.11,.14,1)
 bs.inputs['Roughness'].default_value=.92
 return m
records=[]
for index,n in enumerate(names,1):
 baseline=set(bpy.data.objects);r={'asset':n,'visual_status':'PENDING_USER_AND_DIRECTOR','target':8,'art_approved':False}
 try:
  bpy.ops.import_scene.gltf(filepath=str(base/(n+'.glb')))
  old=[o for o in bpy.data.objects if o not in baseline and o.type=='MESH']
  if not old:raise RuntimeError('Missing original')
  cam_for(old);shoot(out/'before'/(n+'.png'))
  for o in old:bpy.data.objects.remove(o,do_unlink=True)
  src=donor/(n+'.glb') if n!='cliff_rock_column' else None
  color=(.36,.38,.42) if n.startswith('boulder') else ((.31,.34,.39) if n=='cliff_stone_chip' else (.28,.30,.37))
  mat=rock_material(n,color)
  if n=='cliff_rock_column':
   new=cliff_body(mat,dark_material())
  else:
   prev=set(bpy.data.objects)
   bpy.ops.import_scene.gltf(filepath=str(src))
   new=[o for o in bpy.data.objects if o not in prev and o.type=='MESH']
   if not new:raise RuntimeError('Empty CC0 donor')
   for o in new:o.data.materials.clear();o.data.materials.append(mat);uv(o)
  # Keep same locked camera to show size differences honestly;
  # rock columns explicitly built at architectural-module scale.
  shoot(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dest=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<10000:raise RuntimeError('Invalid output GLB')
  r.update({'technical':'PASS_PENDING_ART','mesh_objects':len(new),
    'source_sha':sha(base/(n+'.glb')),'candidate_sha':sha(dest),
    'origin':'Quaternius CC0 plus Poly Haven CC0' if n!='cliff_rock_column' else 'Blender stratified authored cliff with Poly Haven CC0'})
  print('ROCK_INDEPENDENT',index,'/5',n,len(new),flush=True)
 except Exception as e:
  r['technical']='FAILED';r['error']=str(e);traceback.print_exc()
 finally:
  records.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0:bpy.data.images.remove(im)
  (out/'reports'/'five_rocks.json').write_text(json.dumps({'no_assembly':True,'all_approved':False,
  'baseline_run':38813966671,'source_donors':38815831008,'cc0_PBR':38823501643,'assets':records},indent=2))
print('ROCK_FIVE_COMPLETE',sum(i.get('technical')=='PASS_PENDING_ART' for i in records),flush=True)
if len(records)!=5 or any(r['technical']!='PASS_PENDING_ART' for r in records):raise RuntimeError('Rock batch failed')
