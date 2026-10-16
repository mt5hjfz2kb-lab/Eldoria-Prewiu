"""Eldoria forge V2: preserve exact approved candidate gate arch geometry;
bake high-quality actual Poly Haven rusty_metal_05 CC0 photo relief as slate-black
wrought iron albedo. No copper/orange iron; no scene; fixed-camera A/B 2 gates.
"""
import bpy,sys,json,math,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise SystemExit('source-gates-glb-folder CC0-maps output')
source,maps,out=map(Path,arg)
for f in ('glb','before','after','atlas','reports'):(out/f).mkdir(parents=True,exist_ok=True)
names=['portcullis_large','portcullis_small']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=1100
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Alpine dark iron lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.26,.34,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def lamp(n,power,pos,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=10
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm metal edge',2100,(-7,-9,12),(1,.82,.66))
lamp('cool sky',1800,(8,4,11),(.69,.83,1))
lamp('rim',1300,(1,10,9),(.89,.96,1))
camdata=bpy.data.cameras.new('locked individual gate camera')
cam=bpy.data.objects.new('locked individual gate camera',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(meshes):
 corners=[o.matrix_world@Vector(p) for o in meshes for p in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in corners) for i in range(3)]),Vector([max(p[i] for p in corners) for i in range(3)])
def camera(meshes):
 lo,hi=bbox(meshes);cen=(lo+hi)*.5;dims=hi-lo
 cam.location=cen+Vector((.9,-1.62,1.14)).normalized()*max(dims.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 camdata.ortho_scale=max(dims.x,dims.y,dims.z)*1.68
def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
photomap=maps/'rusty_metal_05_diffuse.jpg'
normalmap=maps/'rusty_metal_05_nor_gl.jpg'
if not photomap.is_file():raise RuntimeError('Lost CC0 iron diffuse')
original=bpy.data.images.load(str(photomap),check_existing=False)
original.scale(768,768)
pixels=np.empty(768*768*4,dtype=np.float32);original.pixels.foreach_get(pixels)
rgba=pixels.reshape(768,768,4)
rgb=rgba[:,:,:3];lum=rgb[:,:,0]*.32+rgb[:,:,1]*.55+rgb[:,:,2]*.13
mid=float(lum.mean());sigma=float(lum.std())+.015
rel=np.clip((lum-mid)/sigma,-2,2)
# Keep the microscopic weathering field and real photo grain, but remove orange rust
# that gave previous source a bronze color incompatible with Valoria.
tone=np.clip(.85+rel*.17,.58,1.18)
rgb[:]=np.clip(np.array([.12,.132,.15],dtype=np.float32)[None,None,:]*tone[:,:,None],.018,.23)
dest_img=bpy.data.images.new('VALORIA_CC0_forged_black_iron',768,768,alpha=True)
dest_img.pixels.foreach_set(pixels);dest_img.update()
atlas=out/'atlas'/'valoria_dark_wrought_iron_CC0.png'
dest_img.filepath_raw=str(atlas);dest_img.file_format='PNG';dest_img.save()
bpy.data.images.remove(original)
mat=bpy.data.materials.new('Eldoria_dark_forged_iron_CC0_PBR');mat.use_nodes=True
ns=mat.node_tree.nodes;ns.clear();l=mat.node_tree.links
tex=ns.new('ShaderNodeTexImage');tex.image=dest_img
bs=ns.new('ShaderNodeBsdfPrincipled')
bs.inputs['Metallic'].default_value=.61;bs.inputs['Roughness'].default_value=.65
l.new(tex.outputs['Color'],bs.inputs['Base Color'])
if normalmap.is_file():
 nim=bpy.data.images.load(str(normalmap),check_existing=True)
 nim.colorspace_settings.name='Non-Color'
 tn=ns.new('ShaderNodeTexImage');tn.image=nim
 nm=ns.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.20
 l.new(tn.outputs['Color'],nm.inputs['Color']);l.new(nm.outputs['Normal'],bs.inputs['Normal'])
op=ns.new('ShaderNodeOutputMaterial');l.new(bs.outputs[0],op.inputs['Surface'])
records=[]
for name in names:
 pre=set(bpy.data.objects);r={'asset':name,'art_approved':False,'target_score':8}
 try:
  p=source/(name+'.glb');bpy.ops.import_scene.gltf(filepath=str(p))
  models=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if not models:raise RuntimeError('Empty current forged geometry')
  polys=sum(len(o.data.polygons) for o in models)
  camera(models);shoot(out/'before'/(name+'.png'))
  replaced=0
  for o in models:
   if not o.data.materials:o.data.materials.append(mat);replaced+=1
   else:
    for idx in range(len(o.data.materials)):
     o.data.materials[idx]=mat;replaced+=1
  if replaced<len(models):raise RuntimeError('Some bars were not treated')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in models:o.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<15000:raise RuntimeError('Candidate export empty')
  r.update({'technical':'PASS_PENDING_VISUAL','mesh_count':len(models),'polygon_count':polys,
   'material_slots_replaced':replaced,'original_sha':sha(p),'candidate_sha':sha(target),
   'photo_source':'Poly Haven rusty_metal_05 CC0 photo in black slate albedo + original normal'})
  print('BLACK_WROUGHT_IRON_CASTLE_GATE',name,len(models),flush=True)
 except Exception as e:r.update({'technical':'FAILED','error':str(e)});traceback.print_exc()
 finally:
  records.append(r)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'two_gothic_iron_gates_black_cc0.json').write_text(json.dumps({
   'source_run':38859885125,'no_assembly':True,'all_art_approved':False,'items':records},indent=2))
if len(records)!=2 or any(r['technical']!='PASS_PENDING_VISUAL' for r in records):raise RuntimeError('Iron color pilot failed')
print('BLACK_WROUGHT_IRON_TWO_TECH_COMPLETE',flush=True)
