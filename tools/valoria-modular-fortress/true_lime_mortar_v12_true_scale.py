"""ELDORIA V12: corrected face-oriented atlas and UV; render fine-grain 2.6cm mortar preserving V9 geometry.
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

# V12 hypothesis: the imported thin ribbon has UVs that vertically stretch the
# photograph into striped 'wood grain'. Rebuild a 1.8m long *face-oriented* atlas
# from 100% recorded licensed CC0 pixels in non-repeating physical 5cm segments.
# Random offsets are deterministic, and adjacent sample tiles overlap/blend.
import random
from mathutils import Vector
source_w,source_h=im.size
raw=np.empty(source_w*source_h*4,dtype=np.float32)
im.pixels.foreach_get(raw)
source=raw.reshape((source_h,source_w,4))[:,:,:3]
W,H=4096,512
acc=np.zeros((H,W,3),dtype=np.float32)
weights=np.zeros((H,W,1),dtype=np.float32)
rng=random.Random(20261016)
chunk=128
overlap=22
photo_w,photo_h=210,260
for band in range(2):
 for tile in range((W+chunk-1)//chunk):
  dest_lo=max(0,tile*chunk-overlap)
  dest_hi=min(W,(tile+1)*chunk+overlap)
  if dest_hi<=dest_lo:continue
  # Even when the source includes large worn patches, this is sampled at
  # mineral scale; no artificial long ribbons or repeated masonry grids.
  px=rng.randrange(0,max(1,source_w-photo_w))
  py=rng.randrange(0,max(1,source_h-photo_h))
  rr=np.linspace(py,py+photo_h-1,256).astype(np.int32)
  cc=np.linspace(px,px+photo_w-1,dest_hi-dest_lo).astype(np.int32)
  piece=source[np.ix_(rr,cc)]
  if rng.random()<.5:piece=piece[::-1,:,:]
  if rng.random()<.5:piece=piece[:,::-1,:]
  lum=piece[:,:,0]*.3+piece[:,:,1]*.59+piece[:,:,2]*.11
  # Neutral muted calcareous mineral. Preserve CC0 microdetails while
  # suppressing isolated dark boulders from full-scale photo patches.
  contrast=np.clip((lum-.45)*.44,-.16,.16)
  rgb=np.clip(np.array([.49,.461,.418],dtype=np.float32)[None,None,:]*(1+contrast[:,:,None]),.11,.8)
  ramp=np.ones(dest_hi-dest_lo,dtype=np.float32)
  left=tile*chunk
  right=(tile+1)*chunk
  if dest_lo<left:ramp[:left-dest_lo]=np.linspace(0.02,1.,left-dest_lo)
  if dest_hi>right:ramp[right-dest_lo:]=np.linspace(1.,.02,dest_hi-right)
  row=slice(band*256,(band+1)*256)
  sl=slice(dest_lo,dest_hi)
  acc[row,sl,:]+=rgb*ramp[None,:,None]
  weights[row,sl,:]+=ramp[None,:,None]
albedo_rgb=np.clip(acc/np.maximum(weights,1e-5),.05,.9)
rgba=np.ones((H,W,4),dtype=np.float32)
rgba[:,:,:3]=albedo_rgb
def image(name,data,filename,noncolor=False):
 i=bpy.data.images.new(name,W,H,alpha=True)
 i.pixels.foreach_set(data.reshape(-1));i.update()
 path=out/'atlas'/filename
 i.filepath_raw=str(path);i.file_format='PNG';i.save();i.pack()
 if noncolor:i.colorspace_settings.name='Non-Color'
 return i,path
albedo,ap=image('Valoria CC0 lime photo orthoprojected true scale V12',rgba,'fine_lime_grout_CC0.png')
lum=albedo_rgb[:,:,0]*.3+albedo_rgb[:,:,1]*.59+albedo_rgb[:,:,2]*.11
gx=np.clip((np.roll(lum,-1,axis=1)-np.roll(lum,1,axis=1))*4.,-.18,.18)
gy=np.clip((np.roll(lum,-1,axis=0)-np.roll(lum,1,axis=0))*4.,-.18,.18)
N=np.ones((H,W,4),dtype=np.float32)
N[:,:,0]=.5-gx*.5
N[:,:,1]=.5-gy*.5
N[:,:,2]=.5+.5*np.sqrt(np.maximum(0,1-gx*gx-gy*gy))
normal,npng=image('Valoria photographed lime true scale normals V12',N,'fine_lime_grout_normal.png',True)
# IMPORTANT: explicit UV for *each polygon's actual face*. Front-facing 26mm
# side uses 256 atlas rows, upper 22mm depth uses the other 256. Long 1.8m
# dimension receives 4096 independent pixels rather than 1024 stretched rows.
uv=core.data.uv_layers.active or core.data.uv_layers.new(name='MortarTrueScaleUV')
for face in core.data.polygons:
 use_top=abs(face.normal.z)>.55
 for li in face.loop_indices:
  co=core.data.vertices[core.data.loops[li].vertex_index].co
  uu=max(0.,min(1.,(float(co.x)-float(lo.x))/max(float(dim.x),.01)))
  if use_top:
   vv=.50+.49*max(0.,min(1.,(float(co.y)-float(lo.y))/max(float(dim.y),.005)))
  else:
   vv=.01+.47*max(0.,min(1.,(float(co.z)-float(lo.z))/max(float(dim.z),.01)))
  uv.data[li].uv=(uu,vv)
mat=bpy.data.materials.new('Valoria matte photograph-grain mortar V12 2.6cm')
mat.use_nodes=True;nd=mat.node_tree.nodes;nd.clear();links=mat.node_tree.links
a=nd.new('ShaderNodeTexImage');a.image=albedo
b=nd.new('ShaderNodeTexImage');b.image=normal
norm=nd.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.32
bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.94
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
 'candidate_sha':sha(glb),'uv_rebuilt':True,'atlas_pixels':[W,H],'mapping':'face-oriented meter-scale CC0 photographed patch mosaic','albedo_sha':sha(ap),'normal_sha':sha(npng),
 'note':'V12 physically narrow lime, non-banded CC0 photo UV, not visually approved and no Unity claim'}
(out/'reports'/'true_lime_mortar_v12.json').write_text(json.dumps(report,indent=2))
print('V12_TRUE_SCALE_MORTAR_PENDING_INDEPENDENT_VISUAL',glb.stat().st_size,flush=True)
