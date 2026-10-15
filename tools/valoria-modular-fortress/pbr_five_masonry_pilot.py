"""Isolated 5-module visual A/B: CC0 Poly Haven stone texture + real wedge geometry.
No composition, no Unity, no automatic approval. Blender 5.2.
"""
import bpy,sys,math,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)<3:raise SystemExit('source63 textureCC0 outputDir')
src,tex_root,out=map(Path,args[:3])
for d in ('before','after','glb','metadata'):(out/d).mkdir(parents=True,exist_ok=True)
names=['stone_ashlar_01','stone_ashlar_02','stone_ashlar_03','arch_keystone_major','arch_voussoir_major']
palette={'stone_ashlar_01':(.65,.585,.50),'stone_ashlar_02':(.62,.565,.49),
         'stone_ashlar_03':(.54,.525,.48),'arch_keystone_major':(.66,.59,.52),
         'arch_voussoir_major':(.625,.575,.51)}
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene
sc.render.engine='BLENDER_EEVEE';sc.render.resolution_x=900;sc.render.resolution_y=900
sc.render.resolution_percentage=100;sc.render.image_settings.file_format='PNG'
sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria neutral independent asset studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.30,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.75;sc.world=world
def area(n,p,power,rgb):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=rgb;data.shape='DISK';data.size=8
 ob=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(ob);ob.location=p
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('warm key',(-6,-8,12),1750,(1,.82,.69))
area('cool fill',(8,4,10),1400,(.67,.77,1))
area('pale rim',(-2,8,9),980,(1,1,1))
d=bpy.data.cameras.new('locked A/B camera');cam=bpy.data.objects.new('locked A/B camera',d)
bpy.context.collection.objects.link(cam);sc.camera=cam;d.type='ORTHO'
rig=set(bpy.data.objects)
def render(path):sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def bbox(meshes):
 pts=[ob.matrix_world@Vector(c) for ob in meshes for c in ob.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera(meshes):
 bpy.context.view_layer.update();lo,hi=bbox(meshes);center=(lo+hi)/2;size=hi-lo
 cam.location=center+Vector((.84,-1.5,1.1)).normalized()*max(size.length,1)*3.25
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 d.ortho_scale=max(size.x,size.y,size.z,.4)*1.70
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
rock=tex_root/'rock_3_diffuse.jpg'
normal_path=tex_root/'rock_3_nor_gl.jpg'
rough_path=tex_root/'rock_3_rough.jpg'
for p in (rock,normal_path,rough_path):
 if not p.is_file() or p.stat().st_size<10000:raise RuntimeError('Missing verified CC0 source '+str(p))
def color_material(name):
 img=bpy.data.images.load(str(rock),check_existing=False);img.scale(768,768)
 pix=np.empty(768*768*4,dtype=np.float32);img.pixels.foreach_get(pix)
 arr=pix.reshape((768,768,4))
 rgb=arr[:,:,:3];brightness=np.dot(rgb,np.array([.30,.59,.11],dtype=np.float32))
 mu=float(brightness.mean());sigma=float(brightness.std())+.015
 factor=np.clip(1+.105*(brightness-mu)/sigma,.79,1.23)
 base=np.asarray(palette[name],dtype=np.float32)
 rgb[:]=np.clip(base[None,None,:]*factor[:,:,None],.02,.98)
 arr[:,:,3]=1
 out_img=bpy.data.images.new(name+'_CalibratedCC0Limestone',width=768,height=768,alpha=True)
 out_img.pixels.foreach_set(pix);out_img.update()
 file=out/'metadata'/(name+'_limestone_albedo.png')
 out_img.filepath_raw=str(file);out_img.file_format='PNG';out_img.save()
 bpy.data.images.remove(img)
 mat=bpy.data.materials.new('Valoria_aged_limestone_'+name);mat.use_nodes=True
 nodes=mat.node_tree.nodes;nodes.clear();links=mat.node_tree.links
 tex=nodes.new('ShaderNodeTexImage');tex.image=out_img;tex.interpolation='Linear';tex.location=(-520,130)
 bs=nodes.new('ShaderNodeBsdfPrincipled');bs.location=(20,80)
 bs.inputs['Metallic'].default_value=0;bs.inputs['Roughness'].default_value=.88
 links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 # The donor normal is noisy; soften strength to preserve stylized readability.
 nimg=bpy.data.images.load(str(normal_path),check_existing=True);nimg.colorspace_settings.name='Non-Color'
 ntex=nodes.new('ShaderNodeTexImage');ntex.image=nimg;ntex.location=(-540,-160)
 nor=nodes.new('ShaderNodeNormalMap');nor.inputs['Strength'].default_value=.20;nor.location=(-220,-110)
 links.new(ntex.outputs['Color'],nor.inputs['Color']);links.new(nor.outputs['Normal'],bs.inputs['Normal'])
 outnode=nodes.new('ShaderNodeOutputMaterial');outnode.location=(350,60)
 links.new(bs.outputs[0],outnode.inputs['Surface'])
 return mat
def make_wedge(mesh):
 # Replace 2D rectangular voussoir signature with genuine tapered radial wedge.
 # Keep lateral depth and local origin unchanged for modular assembly.
 coords=np.array([tuple(v.co) for v in mesh.vertices],dtype=np.float64)
 lo=coords.min(axis=0);hi=coords.max(axis=0)
 for v in mesh.vertices:
  f=(v.co.z-lo[2])/max(hi[2]-lo[2],.001)
  v.co.x=(v.co.x-(lo[0]+hi[0])/2)*(.74+.30*f)+(lo[0]+hi[0])/2
 mesh.update()
 return {'changed_vertices':len(mesh.vertices),'upper_width_ratio':1.04,'lower_width_ratio':.74}
def add_bevel(ob):
 # Geometry rounded contact edges at sub-centimetre scale, not faux noise displacement.
 mod=ob.modifiers.new('hand-worn edges for limestone','BEVEL')
 mod.width=.016;mod.segments=3;mod.limit_method='ANGLE'
 mod2=ob.modifiers.new('large flat dressed-stone faces','WEIGHTED_NORMAL')
 mod2.keep_sharp=True
def uv_fallback(mesh):
 if mesh.uv_layers:return
 layer=mesh.uv_layers.new(name='UVMap')
 for p in mesh.polygons:
  axes=sorted(range(3),key=lambda k:abs(p.normal[k]))[:2]
  for i in p.loop_indices:
   v=mesh.vertices[mesh.loops[i].vertex_index].co
   layer.data[i].uv=(v[axes[0]],v[axes[1]])
records=[]
for i,n in enumerate(names,1):
 before=set(bpy.data.objects);report={'asset':n,'art_approved':False,'reference_target':8}
 try:
  p=src/(n+'.glb');bpy.ops.import_scene.gltf(filepath=str(p))
  objects=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
  if not objects:raise RuntimeError('No source mesh')
  camera(objects);render(out/'before'/(n+'.png'))
  mat=color_material(n)
  wedge=None
  for ob in objects:
   if n.startswith('arch_'):wedge=make_wedge(ob.data)
   add_bevel(ob);uv_fallback(ob.data)
   ob.data.materials.clear();ob.data.materials.append(mat)
  # Camera remains IDENTICAL to source for valid A/B
  render(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in objects:ob.select_set(True)
  bpy.context.view_layer.objects.active=objects[0]
  p2=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(p2),export_format='GLB',use_selection=True,export_apply=True)
  if p2.stat().st_size<10000:raise RuntimeError('Export empty')
  report.update({'technical':'PASS_PENDING_ART','source_sha':digest(p),'candidate_sha':digest(p2),
    'albedo_sha':digest(out/'metadata'/(n+'_limestone_albedo.png')),
    'source_texture':'Poly Haven rock_3 CC0 1K albedo normal donor',
    'wedge_geometry':wedge,'before_sha':digest(out/'before'/(n+'.png')),
    'after_sha':digest(out/'after'/(n+'.png'))})
  print('MASONRY_A_B',i,'/',len(names),n,report['technical'],flush=True)
 except Exception as e:
  report.update({'technical':'FAILED','error':str(e)})
  traceback.print_exc()
 finally:
  records.append(report)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for img in list(bpy.data.images):
   if img.users==0:bpy.data.images.remove(img)
  (out/'metadata'/'manifest.json').write_text(json.dumps({'scope':'five independent modules only',
   'blender_generation':'single A/B per piece','no_composition':True,
   'art_approved':False,'source_run':38813966671,'tex_run':38822341704,'assets':records},indent=2))
if any(v['technical']!='PASS_PENDING_ART' for v in records):raise RuntimeError('Partial failure')
print('FIVE_CC0_PBR_A_B_SUCCESS',flush=True)
