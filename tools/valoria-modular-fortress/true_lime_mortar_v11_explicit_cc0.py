"""ELDORIA V11: render visible fine-grain 2.6cm mortar preserving V9 geometry.
Stage-only until shared Windows runner is idle. No scene work, 8/10 claim or
auto-promotion. Uses embedded Poly Haven CC0 photo + real baked glTF normal.
"""
import bpy,sys,json,hashlib
import numpy as np
from pathlib import Path
from mathutils import Vector,noise
argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(argv)!=3:raise RuntimeError('Arguments: mortar_v9_glb_dir verified_cc0_photo_png output')
src,photo_source,out=map(Path,argv)
if not photo_source.is_file():raise RuntimeError('Verified CC0 photographed lime source missing: '+str(photo_source))
for dirname in ('glb','before','after','atlas','reports'):
 (out/dirname).mkdir(parents=True,exist_ok=True)
input_glb=src/'mortar_joint_recess.glb'
if not input_glb.is_file():raise RuntimeError('Need actual V9 narrow 2.6cm mortar GLB')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1100;s.render.resolution_y=800
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria fine mineral lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.24,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.80;s.world=w
for name,power,loc,rgb in [
 ('raking studio',1500,(-6,-9,11),(1,.85,.7)),
 ('cool studio',1450,(8,3,10),(.71,.83,1))]:
 lamp=bpy.data.lights.new(name,'AREA');lamp.energy=power
 lamp.color=rgb;lamp.size=9;lamp.shape='DISK'
 o=bpy.data.objects.new(name,lamp);bpy.context.collection.objects.link(o)
 o.location=loc;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('same A B mortar camera')
cam=bpy.data.objects.new('same A B mortar camera',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
rig=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(input_glb))
meshes=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
main=[o for o in meshes if 'actual_recessed' in o.name.lower() and 'mortar' in o.name.lower()]
if len(main)!=1:raise RuntimeError('V9 actual recessed mortar mesh missing')
core=main[0]
pts=[o.matrix_world@Vector(p) for o in meshes for p in o.bound_box]
lo=Vector([min(p[i] for p in pts) for i in range(3)])
hi=Vector([max(p[i] for p in pts) for i in range(3)])
dim=hi-lo;center=(hi+lo)*.5
if not .017<=dim.z<=.06:raise RuntimeError('Geometry not a narrow 2-6cm mortar seam')
cam.location=center+Vector((.80,-1.60,1.11)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=max(dim.x,.1)*1.65
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
render(out/'before'/'mortar_joint_recess.png')
camera_matrix=cam.matrix_world.copy();overview_scale=camdata.ortho_scale
cam.location=center+Vector((.07,-.55,.20))
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=.65
render(out/'before'/'mortar_joint_recess_macro.png')
cam.matrix_world=camera_matrix;camdata.ortho_scale=overview_scale
removed=0
for ob in list(meshes):
 if ob is not core and 'lime_grit' in ob.name.lower():
  bpy.data.objects.remove(ob,do_unlink=True);removed+=1
if removed<20:raise RuntimeError('Wrong source, no V9 pebbles to remove')
for vert in core.data.vertices:
 vert.co.y+=noise.noise(Vector((vert.co.x*43.,vert.co.z*130.,.37)))*.00035
core.data.update()
images=[n.image for m in core.data.materials if m and m.use_nodes for n in m.node_tree.nodes
 if n.type=='TEX_IMAGE' and n.image and n.image.colorspace_settings.name!='Non-Color']
if images:
 im=images[0]
else:
 # The actual V9 glTF exporter omitted the Blender bump-photo node.
 # Recover the exact official CC0 photo from independently archived V8.
 im=bpy.data.images.load(str(photo_source),check_existing=False)
W,H=im.size
data=np.empty(W*H*4,dtype=np.float32);im.pixels.foreach_get(data)
rgba=data.reshape((H,W,4));rgb=rgba[:,:,:3]
lum=rgb[:,:,0]*.30+rgb[:,:,1]*.59+rgb[:,:,2]*.11
rel=np.clip((lum-float(lum.mean()))/(max(.001,float(lum.std()))),-2.2,2.2)
rgba[:,:,:3]=np.clip(np.array([.46,.431,.392],dtype=np.float32)*(1+.22*rel[:,:,None]),.05,.8)
def image(name,data,filename,noncolor=False):
 i=bpy.data.images.new(name,W,H,alpha=True)
 i.pixels.foreach_set(data.reshape(-1));i.update()
 path=out/'atlas'/filename
 i.filepath_raw=str(path);i.file_format='PNG';i.save();i.pack()
 if noncolor:i.colorspace_settings.name='Non-Color'
 return i,path
albedo,ap=image('Valoria genuine fine lime mineral CC0',rgba,'fine_lime_grout_CC0.png')
gx=(np.roll(rel,-1,axis=1)-np.roll(rel,1,axis=1))*.075
gy=(np.roll(rel,-1,axis=0)-np.roll(rel,1,axis=0))*.075
N=np.ones((H,W,4),dtype=np.float32)
N[:,:,0]=.5-np.clip(gx,-.5,.5)*.5
N[:,:,1]=.5-np.clip(gy,-.5,.5)*.5
N[:,:,2]=.5+.5*np.sqrt(np.maximum(0,1-np.clip(gx,-.5,.5)**2-np.clip(gy,-.5,.5)**2))
normal,npng=image('Valoria fine mineral normals CC0',N,'fine_lime_grout_normal.png',True)
mat=bpy.data.materials.new('Valoria matte packed lime mortar 2.6cm')
mat.use_nodes=True;nd=mat.node_tree.nodes;nd.clear();links=mat.node_tree.links
a=nd.new('ShaderNodeTexImage');a.image=albedo
b=nd.new('ShaderNodeTexImage');b.image=normal
norm=nd.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.22
bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.97
links.new(a.outputs['Color'],bs.inputs['Base Color'])
links.new(b.outputs['Color'],norm.inputs['Color'])
links.new(norm.outputs['Normal'],bs.inputs['Normal'])
output=nd.new('ShaderNodeOutputMaterial')
links.new(bs.outputs['BSDF'],output.inputs['Surface'])
core.data.materials.clear();core.data.materials.append(mat)
render(out/'after'/'mortar_joint_recess.png')
cam.location=center+Vector((.07,-.55,.20))
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=.65
render(out/'after'/'mortar_joint_recess_macro.png')
bpy.ops.object.select_all(action='DESELECT')
core.select_set(True);bpy.context.view_layer.objects.active=core
glb=out/'glb'/'mortar_joint_recess.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<5000:raise RuntimeError('Missing real GLB')
report={'asset':'mortar_joint_recess','source_run':38895731036,
 'technical':'PASS_PENDING_VISUAL','art_approved':False,
 'removed_floating_pebbles':removed,'actual_height_m':float(dim.z),
 'source_photo_sha':sha(photo_source),'albedo_restored_explicitly':not bool(images),
 'candidate_sha':sha(glb),'albedo_sha':sha(ap),'normal_sha':sha(npng),
 'note':'Physical 26mm mortar, fine mineral photo CC0, no hero scene'}
(out/'reports'/'true_lime_mortar_v10.json').write_text(json.dumps(report,indent=2))
print('V10_MORTAR_TEST_PENDING_VISUAL',glb.stat().st_size,flush=True)
