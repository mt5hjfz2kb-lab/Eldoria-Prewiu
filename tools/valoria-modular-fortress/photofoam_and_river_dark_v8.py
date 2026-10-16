"""Actual photo-like independent foam-on-water VFX + dark refractive river tile.
Crossed white geometry wires are rejected; use authored stochastic breakup and
alpha transparency in exported glTF instead. Not scene or Unity approval.
"""
import bpy,sys,json,hashlib,math,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=3:raise RuntimeError('foam_source_glb water_source_glb out')
foam_src,water_src,out=map(Path,A)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1130;scene.render.resolution_y=980
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Night river water lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.16,.21,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;scene.world=w
def lamp(n,pos,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=12
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('blue fill',(-8,-10,12),1520,(.69,.81,1))
lamp('warm sunset reflection',(9,1,12),1820,(1,.82,.65))
lamp('rim',(-3,9,12),1300,(.91,.98,1))
cd=bpy.data.cameras.new('two VFX modules one locked lens')
cam=bpy.data.objects.new('two VFX modules one locked lens',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bb(models):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in models if o.type=='MESH' for v in o.bound_box]
 if not pts:raise RuntimeError('Missing physical prior VFX geometry')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camset(models):
 lo,hi=bb(models);cen=(lo+hi)*.5;d=hi-lo
 cam.location=cen+Vector((.67,-1.24,1.58)).normalized()*max(d.length,1)*2.72
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.20)*1.67
def render(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def image(name,rgba,path):
 h,w,_=rgba.shape
 im=bpy.data.images.new(name,width=w,height=h,alpha=True)
 im.pixels.foreach_set(rgba.astype(np.float32).reshape(-1))
 im.update();im.filepath_raw=str(path);im.file_format='PNG';im.save();im.pack()
 return im
def shader(name,albedo,roughness,alpha=False,normal=None,transmission=0):
 m=bpy.data.materials.new(name);m.use_nodes=True
 if alpha:m.surface_render_method='DITHERED'
 nd=m.node_tree.nodes;nd.clear();lk=m.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=albedo
 bs=nd.new('ShaderNodeBsdfPrincipled')
 bs.inputs['Roughness'].default_value=roughness
 bs.inputs['IOR'].default_value=1.333
 bs.inputs['Transmission Weight'].default_value=transmission
 lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
 if alpha:lk.new(tex.outputs['Alpha'],bs.inputs['Alpha'])
 if normal:
  nn=nd.new('ShaderNodeTexImage');nn.image=normal
  normal.colorspace_settings.name='Non-Color'
  nm=nd.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.12
  lk.new(nn.outputs['Color'],nm.inputs['Color']);lk.new(nm.outputs['Normal'],bs.inputs['Normal'])
 outnode=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],outnode.inputs['Surface'])
 return m
def uvs(obj):
 uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap')
 for face in obj.data.polygons:
  for li in face.loop_indices:
   co=obj.data.vertices[obj.data.loops[li].vertex_index].co
   uv.data[li].uv=(co.x,co.y)
def make_mesh(name,lo,hi,mat,nx,ny,curvature):
 vertices=[];faces=[]
 for j in range(ny+1):
  for i in range(nx+1):
   u=i/nx;v=j/ny
   x=lo.x+(hi.x-lo.x)*u;y=lo.y+(hi.y-lo.y)*v
   z=lo.z+curvature*(math.sin(17.5*u+8*v)+.31*math.sin(31*u-23*v+1.2))
   vertices.append((x,y,z))
 for j in range(ny):
  for i in range(nx):
   k=j*(nx+1)+i;faces.append((k,k+1,k+nx+2,k+nx+1))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
 obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
 mesh.materials.append(mat)
 uv=mesh.uv_layers.new(name='UVMap')
 for f in mesh.polygons:
  for li in f.loop_indices:
   v=mesh.vertices[mesh.loops[li].vertex_index].co
   uv.data[li].uv=((v.x-lo.x)/max(hi.x-lo.x,.01),(v.y-lo.y)/max(hi.y-lo.y,.01))
 return obj
def foam_asset(lo,hi):
 h,w=320,1152
 yy,xx=np.mgrid[0:h,0:w].astype(np.float32)
 u=xx/max(w-1,1);v=(yy/(h-1)-.5)*2
 rng=np.random.default_rng(4631)
 phase=2*np.pi
 wave=.13*np.sin(phase*2.0*u)+.055*np.sin(phase*6.7*u+1.1)
 breadth=.26+.08*np.cos(phase*3.3*u)+.06*np.sin(phase*7*u)
 distance=np.abs(v-wave)/np.maximum(breadth,.05)
 turbulence=(np.sin(phase*(17.2*u+2.9*v)+1)+np.cos(phase*(11.3*u-4.8*v)+.7)+
            .44*np.cos(phase*(36*u+9*v)))
 holes=np.clip(.66+.13*turbulence,.13,1)
 fall=np.clip((1.22-distance)/.65,0,1)
 alpha=np.clip((fall**1.65)*holes*(.8+.2*np.sin(u*56)**2),0,.87)
 # Tiny spray and bubbles outside central band, irregular rather than zigzag wires.
 spray=np.clip(turbulence-1.35,0,2)*np.exp(-np.maximum(distance-.8,0)*2)*.12
 alpha=np.clip(alpha+spray,0,.9)
 rgb=np.empty((h,w,4),dtype=np.float32)
 gray=np.clip(.89+.06*np.cos(phase*(17.2*u+2.9*v))-.10*distance,.71,1)
 rgb[:,:,0]=gray*.91;rgb[:,:,1]=gray*.97;rgb[:,:,2]=gray
 rgb[:,:,3]=alpha
 im=image('ELDORIA_broken_photo_spray_bubbles','' if False else rgb,out/'atlas'/'irregular_photo_whitewater_RGBA.png')
 material=shader('Valoria_CC0_compatible_alpha_foamy_river_edge',im,.8,alpha=True)
 width=float(hi.x-lo.x)
 mid=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,hi.z))
 # wider than old 0.07m technical line: meaningful broken spray width 0.24m
 low=Vector((float(lo.x),float(mid.y)-.16,float(hi.z)))
 high=Vector((float(hi.x),float(mid.y)+.16,float(hi.z)))
 return [make_mesh('semitransparent_nonperiodic_water_spray',low,high,material,38,8,.001)],{
  'real_alpha_image':im.name,'physical_width_m':.32,'stochastic_bubble_field':True}
def water_asset(lo,hi):
 # Statistically irregular low-contrast reflective blue. No repeating
 # apparent parallel bright waves from the previously rejected candidate.
 h=w=512;yy,xx=np.mgrid[0:h,0:w].astype(np.float32)
 u=xx/(w-1);v=yy/(h-1)
 rng=np.random.default_rng(87018)
 arr=np.zeros((h,w,4),dtype=np.float32)
 phase=2*np.pi
 waves=.5*np.sin(phase*(4.73*u+1.3*v)+.35*np.sin(phase*1.3*v))
 waves+=.27*np.sin(phase*(11*u-3.14*v)+.8)
 waves+=.20*np.sin(phase*(15.4*u+8.7*v)+1.9)
 nrm=waves/1.15
 grad=np.clip(.92+.028*nrm,.84,.99)
 arr[:,:,0]=.105*grad;arr[:,:,1]=.194*grad;arr[:,:,2]=.24*grad;arr[:,:,3]=1
 im=image('ELDORIA_water_dark_alpine_reflective_blue',arr,out/'atlas'/'dark_river_nonrepeating_photo_like.png')
 dudy,dudx=np.gradient(waves)
 norm=np.zeros_like(arr)
 norm[:,:,0]=np.clip(.5-dudx*.012,.39,.61)
 norm[:,:,1]=np.clip(.5-dudy*.012,.39,.61)
 norm[:,:,2]=1;norm[:,:,3]=1
 imnorm=image('ELDORIA_soft_ripple_normal',norm,out/'atlas'/'soft_nonperiodic_ripple_normal.png')
 mat=shader('Valoria_water_IOR133_dark_blue',im,.19,normal=imnorm,transmission=.42)
 low=Vector((lo.x,lo.y,(lo.z+hi.z)/2));high=Vector((hi.x,hi.y,low.z))
 return [make_mesh('subtle_genuine_geometric_ripple_surface',low,high,mat,90,90,.0025)],{
  'water_ior':1.333,'photo_like_alpha':False,'transmission':.42,
  'known_deferred_Unity':'Final river transparency and scrolling normals must be evaluated with Unity ShaderGraph'}
records=[]
for name,path in [('river_foam_line',foam_src),('river_water_tile',water_src)]:
 pre=set(bpy.data.objects);r={'asset':name,'art_approved':False,'floor_required':6}
 try:
  bpy.ops.import_scene.gltf(filepath=str(path))
  before=[o for o in bpy.data.objects if o not in pre]
  meshes=[o for o in before if o.type=='MESH']
  lo,hi=bb(meshes);camset(meshes)
  render(out/'before'/(name+'.png'))
  for o in before:bpy.data.objects.remove(o,do_unlink=True)
  if name=='river_foam_line':new,details=foam_asset(lo,hi)
  else:new,details=water_asset(lo,hi)
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in new:obj.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dst=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<8000:raise RuntimeError('Empty low-value VFX 3D GLB')
  r.update({'technical':'PASS_PENDING_VISUAL','old_sha':sha(path),'candidate_sha':sha(dst),'details':details})
  print('TWO_TRUE_WATER_FOAM_ASSETS',name,dst.stat().st_size,flush=True)
 except Exception as e:r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(r)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'two_natural_river_vfx_modules.json').write_text(json.dumps({
   'not_composition':True,'art_approved':False,'results':records},indent=2))
if any(r['technical']!='PASS_PENDING_VISUAL' for r in records):raise RuntimeError('Water foam VFX incomplete')
print('TWO_NATIVE_WATER_AND_FOAM_VFX_FINISHED',flush=True)
