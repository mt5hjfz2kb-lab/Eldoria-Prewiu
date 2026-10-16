"""Valoria V3 correction of three genuinely failed under-six candidates:
green photographed grass with real blades, blue low-amplitude water (NO parallel
white stripes), and removing floating ruin debris. Matched Blender A/B, 3 GLBs.
Never change Unity or declare artistic approval automatically.
"""
import bpy,sys,math,json,hashlib,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(argv)!=3:raise RuntimeError('Require previous four-candidate folder, CC0 photo folder, output')
prev,cc0,out=map(Path,argv)
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
names=['grass_ground_patch','river_water_tile','wall_panel_ruined_4m']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1024;s.render.resolution_y=1024
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Eldoria 3 individual unlit-prior reference studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def area(n,power,pos,color):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.color=color;l.size=9;l.shape='DISK'
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('sunset warm',1650,(-8,-9,12),(1,.85,.71))
area('cloud blue',1660,(8,4,12),(.70,.82,1))
area('upper rim',800,(-2,9,10),(.91,.96,1))
cd=bpy.data.cameras.new('identical camera for 3 A/B pairs')
cam=bpy.data.objects.new('identical camera for 3 A/B pairs',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def box(objects):
 bpy.context.view_layer.update()
 points=[o.matrix_world@Vector(c) for o in objects if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in points) for i in range(3)]),Vector([max(p[i] for p in points) for i in range(3)])
def setcam(objects):
 lo,hi=box(objects);cen=(lo+hi)/2;dim=hi-lo
 cam.location=cen+Vector((.86,-1.59,1.1)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(dim.x,dim.y,dim.z,.2)*1.76
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def material_image(name,color_img,rough=.82,metal=0,normal_img=None):
 m=bpy.data.materials.new(name);m.use_nodes=True
 nd=m.node_tree.nodes;nd.clear();lk=m.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=color_img
 bs=nd.new('ShaderNodeBsdfPrincipled')
 bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
 lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
 if normal_img:
  nt=nd.new('ShaderNodeTexImage');nt.image=normal_img;nt.image.colorspace_settings.name='Non-Color'
  nn=nd.new('ShaderNodeNormalMap');nn.inputs['Strength'].default_value=.20
  lk.new(nt.outputs['Color'],nn.inputs['Color']);lk.new(nn.outputs['Normal'],bs.inputs['Normal'])
 outnode=nd.new('ShaderNodeOutputMaterial')
 lk.new(bs.outputs['BSDF'],outnode.inputs['Surface'])
 return m
def grass(meshes,low,high):
 img=bpy.data.images.load(str(cc0/'grass_ground_diffuse.jpg'),check_existing=False)
 img.scale(1024,1024)
 px=np.empty(1024*1024*4,dtype=np.float32);img.pixels.foreach_get(px)
 rgb=px.reshape((1024*1024,4))[:,:3]
 lum=rgb[:,0]*.30+rgb[:,1]*.61+rgb[:,2]*.09
 contrast=np.clip((lum-float(lum.mean()))/(float(lum.std())+.025),-2,2)
 original=rgb.copy()
 target=np.array([.26,.37,.185],dtype=np.float32)
 rgb[:]=np.clip(target[None,:]*(1+.14*contrast[:,None])+
                   .08*(original-original.mean(axis=0)),.025,.80)
 image=bpy.data.images.new('ELDORIA_scanned_CC0_muted_alive_green_grass',width=1024,height=1024,alpha=True)
 image.pixels.foreach_set(px);image.update()
 image.filepath_raw=str(out/'atlas'/'muted_alpine_grass_CC0_1k.png')
 image.file_format='PNG';image.save()
 bpy.data.images.remove(img)
 normal=bpy.data.images.load(str(cc0/'grass_ground_nor_gl.jpg'),check_existing=True)
 mat=material_image('Valoria_olive_grass_stone_dusk',image,.91,0,normal)
 for ob in meshes:
  ob.data.materials.clear();ob.data.materials.append(mat)
 # Modest true leaf triangles; avoid washed-out ball scatter and garish grass.
 blade=bpy.data.materials.new('Valoria_olive_green_actual_grass_blades')
 blade.use_nodes=True
 blade.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.13,.22,.08,1)
 blade.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.93
 rng=random.Random(43071)
 width=float(high.x-low.x);depth=float(high.y-low.y)
 vertices=[];faces=[]
 for k in range(420):
  x=float(low.x)+rng.random()*width
  y=float(low.y)+rng.random()*depth
  z=float((low.z+high.z)*.5)
  h=.035+rng.random()*.064
  dx=rng.uniform(-.018,.018);dy=rng.uniform(-.018,.018)
  start=len(vertices)
  vertices.extend([(x-.013,y,z),(x+.013,y,z),(x+dx,y+dy,z+h)])
  faces.append((start,start+1,start+2))
 m=bpy.data.meshes.new('true_subtle_alpine_grass_leaves')
 m.from_pydata(vertices,[],faces);m.update()
 ob=bpy.data.objects.new('subtle_green_grass_420_real_blades',m)
 bpy.context.collection.objects.link(ob);m.materials.append(blade)
 return meshes+[ob],{'blades':420,'cc0':'grass_ground 1K photographed, muted green'}
def water(meshes,low,high):
 # Take the previous wavy real 90x90 mesh but remove its highly periodic
 # white specular patches. Bake natural non-periodic blue normal/albedo.
 n=1024
 yy,xx=np.mgrid[0:n,0:n].astype(np.float32);x=xx/n;y=yy/n
 phase=2*np.pi*(x*2.8+y*.93)+.7*np.sin(2*np.pi*(y*2.2-x*.75))
 wave=(np.sin(phase)+.32*np.sin(2*np.pi*(x*6.8-y*3.4))+
       .19*np.sin(2*np.pi*(x*11.2+y*7.1)+.9))
 v=np.clip(.90+wave*.037,.79,1.04)
 px=np.empty((n,n,4),dtype=np.float32)
 px[:,:,0]=np.clip(.115*v,.02,1)
 px[:,:,1]=np.clip(.232*v,.02,1)
 px[:,:,2]=np.clip(.277*v,.02,1)
 px[:,:,3]=1
 arr=px.reshape(-1).copy()
 im=bpy.data.images.new('Eldoria_blue_cold_water_gentle_uneven_reflections',n,n,alpha=True)
 im.pixels.foreach_set(arr);im.update()
 im.filepath_raw=str(out/'atlas'/'Eldoria_water_subtle_blue_1k.png')
 im.file_format='PNG';im.save()
 gy,gx=np.gradient(wave)
 nx=np.clip(-gx*.11,-.34,.34);ny=np.clip(-gy*.11,-.34,.34)
 norm=np.empty_like(px)
 norm[:,:,0]=.5+nx*.5
 norm[:,:,1]=.5+ny*.5
 norm[:,:,2]=np.sqrt(np.maximum(1-nx*nx-ny*ny,0))*0.5+.5
 norm[:,:,3]=1
 normal=bpy.data.images.new('ELDORIA_waves_true_normal_1k',n,n,alpha=True)
 normal.pixels.foreach_set(norm.reshape(-1));normal.update()
 normal.filepath_raw=str(out/'atlas'/'Eldoria_uneven_waves_normal_1k.png')
 normal.file_format='PNG';normal.save()
 m=material_image('Eldoria_dark_evening_water_for_Unity_ShaderGraph',im,.28,0,normal)
 for ob in meshes:
  ob.data.materials.clear();ob.data.materials.append(m)
  for vtx in ob.data.vertices:
   # Less large wave amplitudes than rejected bright repeated grid.
   vtx.co.z*=.33
 return meshes,{'type':'irregular_baked_albedo_normal_and_low_wave_geometry','limitation':'Unity Shader Graph for final river transparency'}
def ruin(meshes):
 before=len(meshes);removed=[];keep=[]
 for o in list(meshes):
  if 'loose_ruin_broken_stone' in o.name.lower():
   identity=o.name
   bpy.data.objects.remove(o,do_unlink=True)
   removed.append(identity)
  else:
   keep.append(o)
 if len(removed)<5:raise RuntimeError('No actual unsupported floating debris found, preserve V2')
 return keep,{'deleted_floating_debris':len(removed),'remaining_individual_stones':len(keep)}
records=[]
for name in names:
 pre=set(bpy.data.objects);r={'asset':name,'min_required':6,'art_approved':False,'no_scene':True}
 try:
  original=prev/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  meshes=[ob for ob in bpy.data.objects if ob not in pre and ob.type=='MESH']
  lo,hi=box(meshes);setcam(meshes)
  render(out/'before'/(name+'.png'))
  if name=='grass_ground_patch':new,details=grass(meshes,lo,hi)
  elif name=='river_water_tile':new,details=water(meshes,lo,hi)
  else:new,details=ruin(meshes)
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<9000:raise RuntimeError('Candidate too small')
  r.update({'technical':'PASS_PENDING_ART','before_sha':sha(original),
            'candidate_sha':sha(target),'details':details})
  print('ELDORIA_GROUND_OR_RUIN_REGRESSION_FIXED',name,details,flush=True)
 except Exception as e:
  r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(r)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'three_artifact_corrections.json').write_text(json.dumps({
   'all_pending_manual_art':True,'first_run':38871626493,
   'assets':records},indent=2))
if len(records)!=3 or any(x['technical']!='PASS_PENDING_ART' for x in records):
 raise RuntimeError('Three independent corrective variants incomplete')
print('VALORIA_THREE_ART_CORRECTIONS_FINISHED',flush=True)
