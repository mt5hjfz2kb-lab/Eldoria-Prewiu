"""Valoria river_water_tile candidate V8: one standalone genuine translucent
deep-water 3D tile with nonperiodic soft wave geometry, UV-baked normals and
PBR glTF alpha. Blender A/B and source SHA; no scene/Unity modification.
"""
import bpy,sys,math,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2: raise SystemExit('original63_glb_dir output_dir')
source,out=map(Path,args)
for folder in ('before','after','glb','atlas','reports'): (out/folder).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1100;s.render.resolution_y=1020;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Alpine river-water study neutral blue hour');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.11,.15,.20,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=world
def area(n,pos,energy,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=energy;d.color=col;d.shape='DISK';d.size=7
 ob=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('soft large sky reflection',(-4,-6,10),1900,(.72,.84,1))
area('warm dusk shoreline',(8,1,11),1200,(1,.86,.70))
area('blue rim',(-3,9,9),800,(.80,.91,1))
cd=bpy.data.cameras.new('locked river study')
cam=bpy.data.objects.new('locked river study',cd);bpy.context.collection.objects.link(cam)
s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(models):
 pts=[o.matrix_world@Vector(v) for o in models if o.type=='MESH' for v in o.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
original=source/'river_water_tile.glb'
if not original.is_file():raise RuntimeError('Original 63 river GLB missing')
bpy.ops.import_scene.gltf(filepath=str(original))
before=[o for o in bpy.data.objects if o not in rig];meshes=[o for o in before if o.type=='MESH']
if not meshes:raise RuntimeError('Missing original water geometry')
lo,hi=bounds(meshes);d=hi-lo;mid=(lo+hi)*.5
if d.x<.5 or d.y<.5:raise RuntimeError('Invalid water source size '+str(d))
cam.location=mid+Vector((.66,-1.31,1.35)).normalized()*max(d.length,1)*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(d.x,d.y,.3)*1.62
render(out/'before'/'river_water_tile.png')
for o in before:bpy.data.objects.remove(o,do_unlink=True)
N=88;vertices=[];faces=[]
# Non-parallel river motion: noise warp and wavelength blending with modest
# millimetric peak-to-trough height (no artificial white threads).
def height(u,v):
 wx=u+.067*math.sin(v*11.9+u*3.7)
 wy=v+.052*math.sin(u*9.1-v*2.3)
 return (.0061*math.sin(2*math.pi*(3.1*wx+.68*wy))
       +.0030*math.sin(2*math.pi*(5.3*wy-1.8*wx))
       +.0016*math.sin(2*math.pi*(10.1*wx+3.4*wy+math.sin(wx*5.2))))
for j in range(N+1):
 for i in range(N+1):
  u=i/N;v=j/N
  vertices.append((float(lo.x)+u*float(d.x),float(lo.y)+v*float(d.y),
                   float(mid.z)+height(u,v)))
for j in range(N):
 for i in range(N):
  k=j*(N+1)+i;faces.append((k,k+1,k+N+2,k+N+1))
m=bpy.data.meshes.new('organic_water_surface_not_white_stripes')
m.from_pydata(vertices,[],faces);m.update()
o=bpy.data.objects.new('river_water_tile_authored_wave',m)
bpy.context.collection.objects.link(o)
uv=m.uv_layers.new(name='UVMap')
for face in m.polygons:
 for li in face.loop_indices:
  v=m.vertices[m.loops[li].vertex_index].co
  uv.data[li].uv=((v.x-lo.x)/d.x,(v.y-lo.y)/d.y)
# Tileable low contrast water albedo and proper tangent-space normal, fully
# baked into PNG for GLB + Unity (no procedural Blender nodes that vanish).
Q=768;gy,gx=np.mgrid[:Q,:Q].astype(np.float32);u=gx/(Q-1);v=gy/(Q-1)
warp_u=u+.042*np.sin(v*12.1+u*4.2)
warp_v=v+.039*np.sin(u*8.9-v*3.5)
bands=np.sin(2*np.pi*(3.1*warp_u+.65*warp_v))
bands+=.37*np.sin(2*np.pi*(5.3*warp_v-1.83*warp_u))
bands+=.16*np.sin(2*np.pi*(10.3*warp_u+3.33*warp_v))
luma=np.clip(1+.025*bands,.92,1.09)
arr=np.zeros((Q,Q,4),dtype=np.float32)
arr[:,:,0]=.112*luma
arr[:,:,1]=.214*luma
arr[:,:,2]=.273*luma
arr[:,:,3]=.91
im=bpy.data.images.new('valoria_deep_blue_river_pbr',Q,Q,alpha=True)
im.pixels.foreach_set(arr.reshape(-1));im.update()
albedo=out/'atlas'/'river_low_frequency_blue_rgba.png'
im.filepath_raw=str(albedo);im.file_format='PNG';im.save();im.pack()
gyy,gxx=np.gradient(bands)
xx=np.clip(-gxx*.18,-.30,.30)
yy=np.clip(-gyy*.18,-.30,.30)
normal=np.zeros_like(arr)
normal[:,:,0]=np.clip(.5+.5*xx,0,1)
normal[:,:,1]=np.clip(.5+.5*yy,0,1)
normal[:,:,2]=np.clip(.5+.5*np.sqrt(np.maximum(0,1-xx*xx-yy*yy)),0,1)
normal[:,:,3]=1
ni=bpy.data.images.new('valoria_river_disturbed_normal',Q,Q,alpha=True)
ni.pixels.foreach_set(normal.reshape(-1));ni.update()
norm=out/'atlas'/'river_low_frequency_normal.png'
ni.filepath_raw=str(norm);ni.file_format='PNG';ni.save();ni.pack()
ni.colorspace_settings.name='Non-Color'
mat=bpy.data.materials.new('VALORIA_water_alpha_blue_green_evening')
mat.use_nodes=True;mat.surface_render_method='DITHERED'
nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
col=nd.new('ShaderNodeTexImage');col.image=im
ncol=nd.new('ShaderNodeTexImage');ncol.image=ni
nmap=nd.new('ShaderNodeNormalMap');nmap.inputs['Strength'].default_value=.26
bs=nd.new('ShaderNodeBsdfPrincipled')
bs.inputs['Roughness'].default_value=.21;bs.inputs['Metallic'].default_value=.03
bs.inputs['IOR'].default_value=1.333
lk.new(col.outputs['Color'],bs.inputs['Base Color'])
lk.new(col.outputs['Alpha'],bs.inputs['Alpha'])
lk.new(ncol.outputs['Color'],nmap.inputs['Color'])
lk.new(nmap.outputs['Normal'],bs.inputs['Normal'])
finish=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],finish.inputs['Surface'])
m.materials.append(mat)
render(out/'after'/'river_water_tile.png')
bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
bpy.context.view_layer.objects.active=o
dst=out/'glb'/'river_water_tile.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<120000:raise RuntimeError('Photo normal maps not embedded')
info={'asset':'river_water_tile','technical':'PASS_PENDING_ART',
 'original_sha':sha(original),'glb_sha':sha(dst),'diffuse_sha':sha(albedo),
 'normal_sha':sha(norm),'geometry_quads':N*N,'old_size_m':[float(x) for x in d],
 'height_deformation_m':'approximately +/-0.01',
 'pbr':'basecolor alpha 0.91, normal, roughness 0.21, IOR 1.333',
 'no_parallel_white_foam':True,'art_approved':False,'unity_approved':False,
 'remaining':'Water requires ShaderGraph transparency, depth and shoreline foaming when asset integration allowed. Not an 8/10 certification.'}
(out/'reports'/'one_real_river_water_tile.json').write_text(json.dumps(info,indent=2))
print('REAL_STANDALONE_RIVER_GLTF_CANDIDATE',dst.stat().st_size,flush=True)
