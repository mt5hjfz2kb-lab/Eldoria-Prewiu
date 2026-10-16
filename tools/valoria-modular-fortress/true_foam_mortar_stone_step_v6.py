"""Eldoria three weakest category-specific modules. No fortress composition.
mortar_joint_recess = volumetric recessed aggregate with uneven texture,
river_foam_line = branching swirling translucent foam crest, not three wires,
stair_step = 3 real individual hand-dressed limestone stones + nosing.
Native Blender 3 GLBs/6 fixed-camera images, no fake ART_APPROVED.
"""
import bpy,sys,math,random,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector,noise
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('Require original63 GLB, real CC0 textures, output')
src,cc0,out=map(Path,args)
names=['mortar_joint_recess','river_foam_line','stair_step']
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=950
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Neutral Valoria masonry and water studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.81;s.world=w
def area(name,pos,power,color):
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.color=color;data.size=10;data.shape='DISK'
 ob=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('warm limestone light',(-7,-9,11),1950,(1,.84,.71))
area('blue evening skylight',(8,2,11),1550,(.72,.85,1))
area('soft curved surface light',(-2,10,10),900,(.97,.98,.95))
camera_data=bpy.data.cameras.new('locked single module before-after camera')
camera=bpy.data.objects.new('locked single module before-after camera',camera_data)
bpy.context.collection.objects.link(camera);s.camera=camera;camera_data.type='ORTHO'
rig=set(bpy.data.objects)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def bbox(objs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in objs if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camset(objs):
 lo,hi=bbox(objs);center=(lo+hi)*.5;size=hi-lo
 camera.location=center+Vector((.8,-1.55,1.18)).normalized()*max(size.length,1)*3
 camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
 camera_data.ortho_scale=max(size.x,size.y,size.z,.23)*1.68
def render(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def material(name,diff,normal,tint,rough=.87):
 mat=bpy.data.materials.new(name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
 im=bpy.data.images.load(str(cc0/diff),check_existing=True)
 node=nd.new('ShaderNodeTexImage');node.image=im;node.extension='REPEAT'
 mul=nd.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=.42
 mul.inputs[2].default_value=(*tint,1)
 lk.new(node.outputs['Color'],mul.inputs[1])
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=rough
 lk.new(mul.outputs['Color'],bs.inputs['Base Color'])
 p=cc0/normal
 if p.is_file():
  nm=bpy.data.images.load(str(p),check_existing=True)
  nm.colorspace_settings.name='Non-Color'
  ntex=nd.new('ShaderNodeTexImage');ntex.image=nm
  nn=nd.new('ShaderNodeNormalMap');nn.inputs['Strength'].default_value=.28
  lk.new(ntex.outputs['Color'],nn.inputs['Color']);lk.new(nn.outputs['Normal'],bs.inputs['Normal'])
 outn=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],outn.inputs['Surface'])
 return mat
limestone=material('CC0_real_chiselled_ashlar','rock_3_diffuse.jpg','rock_3_nor_gl.jpg',(.9,.82,.72))
darkstone=material('CC0_real_dirty_grout','quarry_wall_diffuse.jpg','quarry_wall_nor_gl.jpg',(.41,.39,.36),.98)
foam_m=bpy.data.materials.new('Eldoria_white_blue_microfoam');foam_m.use_nodes=True
bs=foam_m.node_tree.nodes.get('Principled BSDF')
bs.inputs['Base Color'].default_value=(.81,.91,.96,1)
bs.inputs['Roughness'].default_value=.72
bs.inputs['Alpha'].default_value=.73
foam_m.surface_render_method='DITHERED'
lightfoam=bpy.data.materials.new('Fresh_crested_river_foam');lightfoam.use_nodes=True
l=lightfoam.node_tree.nodes.get('Principled BSDF')
l.inputs['Base Color'].default_value=(.96,.97,.94,1);l.inputs['Roughness'].default_value=.78;l.inputs['Alpha'].default_value=.82
lightfoam.surface_render_method='DITHERED'
def planar_uv(o):
 uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 for poly in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(poly.normal[i]))
  a,b=([0,2] if axis==1 else [0,1] if axis==2 else [1,2])
  for li in poly.loop_indices:
   co=o.data.vertices[o.data.loops[li].vertex_index].co
   uv.data[li].uv=(co[a]*.65,co[b]*.65)
def cube(name,loc,size,mat,bevel=0.0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 if bevel:
  mod=o.modifiers.new('worn_rounded_limestone_arris','BEVEL')
  mod.width=min(bevel,min(size)*.2);mod.segments=3;mod.limit_method='ANGLE'
  bpy.context.view_layer.objects.active=o
  bpy.ops.object.modifier_apply(modifier=mod.name)
 planar_uv(o);return o
def mortar_asset(lo,hi):
 # Detailed recessed mineral-filled masonry seam: 1.8m long x .4m tall,
 # only ~5cm thick so still fits between separately repeated modular stones.
 W=float(hi.x-lo.x);H=float(hi.z-lo.z);Y=(lo.y+hi.y)*.5
 cx=(lo.x+hi.x)*.5;cz=(lo.z+hi.z)*.5;objs=[]
 objs.append(cube('irregular_real_mortar_joint_back',(cx,Y,cz),(W,.051,H),darkstone,.003))
 rng=random.Random(8626)
 for i in range(3):
  z=float(lo.z)+H*(i+.3)/3
  points=[]
  for j in range(18):
   x=float(lo.x)+W*j/17
   y=float(lo.y)-.008+rng.uniform(-.004,.004)
   zz=z+.012*math.sin(j*.56+i)+rng.uniform(-.012,.012)
   points.append((x,y,zz))
  cv=bpy.data.curves.new('stone_recess_rough_fissure','CURVE');cv.dimensions='3D'
  spline=cv.splines.new('POLY');spline.points.add(len(points)-1)
  for k,p in enumerate(points):spline.points[k].co=(*p,1)
  cv.bevel_depth=.003+ i*.0005;cv.bevel_resolution=2
  ob=bpy.data.objects.new('broken_mortar_fissure_'+str(i),cv);bpy.context.collection.objects.link(ob)
  ob.data.materials.append(darkstone);objs.append(ob)
 for i in range(37):
  x=float(lo.x)+rng.random()*W;z=float(lo.z)+rng.random()*H
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rng.uniform(.005,.014),location=(x,float(lo.y)-.013,z))
  ob=bpy.context.object;ob.name='embedded_angular_mortar_aggregate_'+str(i)
  ob.scale=(1.2,.6,.65);ob.data.materials.append(limestone if i%5==0 else darkstone)
  objs.append(ob)
 return objs
def foam_asset(lo,hi):
 rng=random.Random(721);objs=[]
 W=float(hi.x-lo.x);cx=(lo.x+hi.x)*.5;cy=(lo.y+hi.y)*.5;z=hi.z
 # Physically finite billowy drift, not 3 parallel straight wires.
 for bank in range(7):
  center=bank-3
  vertices=[];faces=[]
  n=45
  for i in range(n+1):
   u=i/n
   x=float(lo.x)+W*u
   base=cy+center*.025+.037*math.sin(u*10.6+bank*.95)+.012*math.sin(u*31+bank)
   wid=.014+.023*(1+math.sin(u*17.5+bank*1.3))*.5
   wid*=.58+.42*math.sin(math.pi*u)
   for sign in (-1,1):
    y=base+sign*wid
    vertices.append((x,y,z+.007*math.sin(u*19+bank)))
   if i<n:
    k=i*2
    faces.append((k,k+1,k+3,k+2))
  me=bpy.data.meshes.new('broken_wavy_river_froth_'+str(bank))
  me.from_pydata(vertices,[],faces);me.update()
  obj=bpy.data.objects.new('white_water_billow_ribbon_'+str(bank),me)
  bpy.context.collection.objects.link(obj)
  obj.data.materials.append(foam_m if bank%3 else lightfoam)
  uv=me.uv_layers.new(name='UVMap')
  for p in me.polygons:
   for li in p.loop_indices:
    idx=me.loops[li].vertex_index
    uv.data[li].uv=(idx//2/n,idx%2)
  objs.append(obj)
 # Many tiny blue-white beads bonded to froth above the rippled stream surface.
 for i in range(75):
  x=float(lo.x)+W*rng.random()
  y=cy+rng.gauss(0,.065)
  rr=rng.uniform(.007,.022)
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rr,location=(x,y,z+.009))
  ob=bpy.context.object;ob.name='whitewater_microbubble_'+str(i)
  ob.scale=(rng.uniform(.9,1.7),rng.uniform(.7,1.1),.18)
  ob.data.materials.append(foam_m if i%3 else lightfoam);objs.append(ob)
 return objs
def step_asset(lo,hi):
 rng=random.Random(980);W=float(hi.x-lo.x);D=float(hi.y-lo.y);H=float(hi.z-lo.z)
 z=(lo.z+hi.z)*.5;ym=(lo.y+hi.y)*.5
 objs=[]
 # 3 unambiguously real stone pavers in 1 repeatable ~4m tread module.
 # Physical 14mm grout, chamfered edges, worn variations.
 n=3;unit=W/n;gap=.017
 for i in range(n):
  a=lo.x+unit*(i+.5)
  ob=cube('one_step_dressed_ashlar_paver_'+str(i),
    (a,ym+rng.uniform(-.005,.005),z+rng.uniform(-.003,.005)),
    (unit-gap,D-.01,H-.009),limestone,.027)
  ob.rotation_euler.z=rng.uniform(-.006,.006)
  objs.append(ob)
 # Folded actual limestone stone lip on the toe line; dark recess below.
 objs.append(cube('step_physical_carved_nosing',(float((lo.x+hi.x)*.5),float(lo.y)+.045,float(hi.z)-.029),
                   (W-.022,.095,.047),limestone,.014))
 objs.append(cube('embedded_stone_step_shaded_riser',(float((lo.x+hi.x)*.5),float(lo.y)+.014,float(lo.z)+.054),
                  (W-.07,.026,.050),darkstone,.008))
 return objs
builders={'mortar_joint_recess':mortar_asset,'river_foam_line':foam_asset,'stair_step':step_asset}
results=[]
for name in names:
 existing=set(bpy.data.objects);r={'asset':name,'visual_floor':6,'art_approved':False}
 try:
  original=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  old=[o for o in bpy.data.objects if o not in existing]
  meshes=[o for o in old if o.type=='MESH']
  if not meshes:raise RuntimeError('Missing previous individual geometry')
  lo,hi=bbox(meshes);camset(meshes);render(out/'before'/(name+'.png'))
  for obj in old:bpy.data.objects.remove(obj,do_unlink=True)
  new=builders[name](lo,hi)
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dst=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<5000:raise RuntimeError('Missing replacement geometry')
  r.update({'technical':'BLENDER_PASS_NEEDS_ART_REVIEW','old_sha':sha(original),
    'new_sha':sha(dst),'mesh_objects':len(new),'source':'Poly Haven CC0 photo limestone plus authored geometric details'})
  print('REAL_LOWEST_THREE_ASSETS',name,len(new),flush=True)
 except Exception as e:r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(r)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'three_weakest_craft_modules.json').write_text(json.dumps({
    'no_assembly':True,'no_art_approved':True,'individuals':results},indent=2))
if any(r['technical']!='BLENDER_PASS_NEEDS_ART_REVIEW' for r in results):
 raise RuntimeError('Three independent low-score studies incomplete')
print('THREE_LOWEST_ASSETS_TEST_COMPLETE',flush=True)
