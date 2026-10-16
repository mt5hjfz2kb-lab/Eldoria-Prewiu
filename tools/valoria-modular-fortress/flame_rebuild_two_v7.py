"""Eldoria replace ugly triangular foam-like flames with photostylized crossed
alpha-textured emissive fire tongues on two existing detailed CC0 medieval fixtures.
Native Blender. Keep stone courses, wrought basket, photo materials untouched.
"""
import bpy,sys,hashlib,json,math,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=2:raise RuntimeError('Provide current torch/brazier GLBs folder and output')
src,out=map(Path,arg)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['torch_wall','brazier_pedestal']
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1040;sc.render.resolution_y=1040
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('warm game-prop studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.17,.22,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.79;sc.world=w
def area(n,power,pos,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=8
 ob=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('amber key',1450,(-6,-8,11),(1,.83,.66))
area('blue fill',1400,(7,3,10),(.67,.80,1))
cd=bpy.data.cameras.new('paired flame visual review')
cam=bpy.data.objects.new('paired flame visual review',cd)
bpy.context.collection.objects.link(cam);sc.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obj):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obj if o.type=='MESH' for v in o.bound_box]
 if not pts:raise RuntimeError('No model')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def setcam(mesh):
 low,high=bounds(mesh);cen=(low+high)*.5;dim=high-low
 cam.location=cen+Vector((.86,-1.58,1.17)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(dim.x,dim.y,dim.z,.25)*1.62
def render(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
# Reusable palette/mask generated independently and packed for Unity compatibility.
H,W=768,384
yy,xx=np.mgrid[0:H,0:W].astype(np.float32)
y=yy/(H-1);x=(xx/(W-1)-.5)*2
center=.09*np.sin(5.4*y)+.055*np.sin(15*y+.6)+.018*np.sin(38*y)
width=.69*np.power(np.maximum(1-y,.001),.72)*(
    .86+.16*np.sin(21*y+.5)+.085*np.sin(39*y+1.2))
width=np.maximum(width,.013)
rad=np.abs((x-center)/width)
boundary=np.clip((1.07-rad)/.21,0,1)
tongue=np.clip(np.sin((1-y)*math.pi)*2.5+.15,0,1)
flicker=np.clip(.83+.17*np.sin(30*y+xx*.052)+.10*np.sin(53*y-xx*.15),.25,1.0)
alpha=(boundary**1.5)*tongue*flicker
alpha*=np.clip((1-y)*9,0,1)
alpha=np.clip(alpha,0,1)
core=np.clip(1-rad*1.8,0,1)
hot=np.clip(1-y*.87,0,1)
pixels=np.zeros((H,W,4),dtype=np.float32)
pixels[:,:,0]=np.clip(.95,0,1)
pixels[:,:,1]=np.clip(.17+.75*core+.22*hot,0,1)
pixels[:,:,2]=np.clip(.025+.18*core+.38*core*hot,0,1)
pixels[:,:,3]=alpha
im=bpy.data.images.new('Valoria_3d_flicker_fire_generated_alpha',W,H,alpha=True)
im.pixels.foreach_set(pixels.reshape(-1));im.update()
png=out/'atlas'/'valoria_real_flickering_flame_alpha.png'
im.filepath_raw=str(png);im.file_format='PNG';im.save();im.pack()
mat=bpy.data.materials.new('Valoria_emissive_fire_PBR_blend')
mat.use_nodes=True;mat.surface_render_method='DITHERED'
nodes=mat.node_tree.nodes;nodes.clear();link=mat.node_tree.links
tex=nodes.new('ShaderNodeTexImage');tex.image=im
bs=nodes.new('ShaderNodeBsdfPrincipled')
bs.inputs['Roughness'].default_value=.85
link.new(tex.outputs['Color'],bs.inputs['Base Color'])
link.new(tex.outputs['Alpha'],bs.inputs['Alpha'])
link.new(tex.outputs['Color'],bs.inputs['Emission Color'])
bs.inputs['Emission Strength'].default_value=3.5
output=nodes.new('ShaderNodeOutputMaterial');link.new(bs.outputs['BSDF'],output.inputs['Surface'])
def flame_card(name,cx,cy,z0,height,width,theta):
 # Crossed double-sided alpha planes with genuine irregular flame texture.
 vertices=[];faces=[(0,1,2,3)]
 for x,z in ((-width/2,0),(width/2,0),(width/2,height),(-width/2,height)):
  vertices.append((cx+x*math.cos(theta),cy+x*math.sin(theta),z0+z))
 mesh=bpy.data.meshes.new('real_spectral_fire_mesh');mesh.from_pydata(vertices,[],faces);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 mesh.materials.append(mat)
 uv=mesh.uv_layers.new(name='UVMap')
 for face in mesh.polygons:
  for li,u in zip(face.loop_indices,((0,0),(1,0),(1,1),(0,1))):uv.data[li].uv=u
 return ob
records=[]
for name in names:
 pre=set(bpy.data.objects);rec={'asset':name,'min_quality_requested':6,'art_approved':False}
 try:
  old=src/(name+'.glb');bpy.ops.import_scene.gltf(filepath=str(old))
  mesh=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  setcam(mesh);render(out/'before'/(name+'.png'))
  old_fire=[o for o in mesh if 'volumetric_flame' in o.name.lower()]
  if len(old_fire)<4:raise RuntimeError('Expected older orange flame geometry')
  low,high=bounds(old_fire)
  center=(low+high)*.5
  height=float(high.z-low.z)
  for obj in old_fire:bpy.data.objects.remove(obj,do_unlink=True)
  width=max(.19,height*.42)
  flame_objs=[flame_card('independent_filmic_fire_tongue_'+str(i),float(center.x),float(center.y),
    float(low.z),height*1.08,width, i*math.pi/3) for i in range(3)]
  # Preserve all existing PBR oak/stone/iron/coal meshes in their exact position.
  new=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for obj in new:obj.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dst=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<45000:raise RuntimeError('Source props disappeared')
  rec.update({'technical':'PASS_PENDING_VISUAL','original_sha':sha(old),'new_sha':sha(dst),
     'preserved_original_nonflame_parts':len(mesh)-len(old_fire),
     'replaced_old_flame_objects':len(old_fire),'new_transparent_flame_planes':len(flame_objs),
     'glTF_alpha_emissive_diffuse':'single baked RGBA texture shared across crossing cards'})
  print('FIRE_TWO_REPLACEMENT',name,len(new),flush=True)
 except Exception as e:rec.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(rec)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'torch_and_brazier_flame_repair.json').write_text(json.dumps({
   'reference':'Warm torch fires of Valoria blue hour','no_assembly':True,
   'all_art_approved':False,'items':records},indent=2))
if any(r['technical']!='PASS_PENDING_VISUAL' for r in records):raise RuntimeError('Both fire candidates required')
print('TWO_FIRE_PROPS_PBR_GLTF_FINISHED',flush=True)
