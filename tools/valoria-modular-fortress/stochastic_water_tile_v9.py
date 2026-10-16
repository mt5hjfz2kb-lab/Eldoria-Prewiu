"""Valoria river V9 reject periodic banding of V8. Genuine multiscale
stochastic low-amplitude surface + nonperiodic baked normal and diffuse.
Preserve 4m tile envelope, alpha and glTF PBR. One isolated A/B only.
"""
import bpy,sys,json,hashlib,traceback,math
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,args)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1100;sc.render.resolution_y=1020
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Natural studio evening light')
w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.11,.15,.20,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;sc.world=w
def lamp(n,power,pos,color):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=color;data.size=7;data.shape='DISK'
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('soft blue large reflection',1900,(-4,-6,10),(.72,.84,1))
lamp('warm evening shoreline',1200,(8,1,11),(1,.86,.70))
lamp('soft rim',800,(-3,9,9),(.80,.91,1))
cd=bpy.data.cameras.new('identical alpine water A/B')
cam=bpy.data.objects.new('identical alpine water A/B',cd)
bpy.context.collection.objects.link(cam);sc.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(obs):
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def render(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
prior=src/'river_water_tile.glb'
if not prior.is_file():raise RuntimeError('Water V8 proof source missing')
bpy.ops.import_scene.gltf(filepath=str(prior))
parts=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
if len(parts)!=1:raise RuntimeError('Expected a single organic ripple mesh')
o=parts[0]
lo,hi=bounds(parts);cen=(lo+hi)*.5;dim=hi-lo
cam.location=cen+Vector((.66,-1.31,1.35)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(dim.x,dim.y,.3)*1.62
render(out/'before'/'river_water_tile.png')
# Colored natural turbulence is periodic at tile seams, but NEVER shares
# one dominant direction like the failed sinusoidal V8 banding.
N=512
rng=np.random.default_rng(225310)
freq=np.fft.fftfreq(N)*N
kx,ky=np.meshgrid(freq,freq)
K=np.sqrt(kx*kx+ky*ky)
def field(scale):
 white=rng.standard_normal((N,N))
 spectrum=np.fft.fft2(white)
 window=np.exp(-.5*(K/scale)**2)
 window[K<1]=0
 signal=np.fft.ifft2(spectrum*window).real.astype(np.float32)
 signal-=float(signal.mean())
 signal/=max(float(signal.std()),.001)
 return signal
field0=.70*field(7)+.24*field(17)+.06*field(35)
field0/=max(float(field0.std()),.0001)
# Avoid intersecting nearby stone through amplitude > 4 mm in final mesh.
field0=np.clip(field0,-2.0,2.0)
xs=[v.co.x for v in o.data.vertices];ys=[v.co.y for v in o.data.vertices]
x0=min(xs);x1=max(xs);y0=min(ys);y1=max(ys)
height0=float(np.median([v.co.z for v in o.data.vertices]))
for vert in o.data.vertices:
 u=(vert.co.x-x0)/max(x1-x0,.01)
 v=(vert.co.y-y0)/max(y1-y0,.01)
 ix=int(u*(N-1))%N;iy=int(v*(N-1))%N
 vert.co.z=height0+float(field0[iy,ix])*.004
o.data.update()
# Albedo retains fine calm water variations without regular bright white stripes.
D=np.zeros((N,N,4),dtype=np.float32)
brightness=np.clip(1.0+.023*field0,.92,1.08)
D[:,:,0]=.096*brightness
D[:,:,1]=.185*brightness
D[:,:,2]=.240*brightness
D[:,:,3]=.93
def baked(name,values,file_name):
 im=bpy.data.images.new(name,N,N,alpha=True)
 im.pixels.foreach_set(values.reshape(-1));im.update()
 p=out/'atlas'/file_name
 im.filepath_raw=str(p);im.file_format='PNG';im.save();im.pack()
 return im,p
diffuse,dp=baked('valoria_stochastic_deep_water_cc0_compatible',D,
                 'river_blue_real_stochastic_albedo.png')
gy,gx=np.gradient(field0)
xx=np.clip(-gx*.11,-.16,.16);yy=np.clip(-gy*.11,-.16,.16)
normal=np.zeros_like(D)
normal[:,:,0]=.5+xx*.5
normal[:,:,1]=.5+yy*.5
normal[:,:,2]=.5+.5*np.sqrt(np.maximum(0,1-xx*xx-yy*yy))
normal[:,:,3]=1
nm,npng=baked('valoria_stochastic_river_surface_normal',normal,
               'river_blue_real_stochastic_normal.png')
nm.colorspace_settings.name='Non-Color'
mat=bpy.data.materials.new('VALORIA_calmer_alpine_river_alpha_PBR')
mat.use_nodes=True;mat.surface_render_method='DITHERED'
nodes=mat.node_tree.nodes;nodes.clear();lk=mat.node_tree.links
tn=nodes.new('ShaderNodeTexImage');tn.image=diffuse
nn=nodes.new('ShaderNodeTexImage');nn.image=nm
normalmap=nodes.new('ShaderNodeNormalMap');normalmap.inputs['Strength'].default_value=.22
bs=nodes.new('ShaderNodeBsdfPrincipled')
bs.inputs['Roughness'].default_value=.37
bs.inputs['Metallic'].default_value=0
bs.inputs['IOR'].default_value=1.333
lk.new(tn.outputs['Color'],bs.inputs['Base Color'])
lk.new(tn.outputs['Alpha'],bs.inputs['Alpha'])
lk.new(nn.outputs['Color'],normalmap.inputs['Color'])
lk.new(normalmap.outputs['Normal'],bs.inputs['Normal'])
output=nodes.new('ShaderNodeOutputMaterial')
lk.new(bs.outputs['BSDF'],output.inputs['Surface'])
o.data.materials.clear();o.data.materials.append(mat)
render(out/'after'/'river_water_tile.png')
bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
bpy.context.view_layer.objects.active=o
target=out/'glb'/'river_water_tile.glb'
bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
if target.stat().st_size<120000:raise RuntimeError('No proper GLTF PBR emission')
report={'asset':'river_water_tile','technical':'PASS_PENDING_VISUAL',
 'parent_V8_run':38896874472,'why':'V8 had visible diagonal repeated dark specular stripes and was ART_REJECTED',
 'source_sha':sha(prior),'candidate_sha':sha(target),'albedo_sha':sha(dp),'normal_sha':sha(npng),
 'actual_waves_height_m':'+/-0.008 approx','note':'one real reusable water tile, no Unity/ShaderGraph modifications',
 'alpha':.93,'roughness':.37,'art_approved':False,'unity_approved':False}
(out/'reports'/'one_stochastic_water_v9.json').write_text(json.dumps(report,indent=2))
print('VALORIA_STOCHASTIC_BLUE_WATER_V9',target.stat().st_size,flush=True)
