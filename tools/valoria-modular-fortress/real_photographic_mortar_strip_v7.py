"""Actual CC0 historic mortar joint single modular component.
Physically rough recessed thin 1.8m strip, fractured plaster grains,
photographed PBR from Poly Haven plaster_stone_wall_02 (NOT stone texture).
A/B Blender 5.2, same camera, no wall composition, no false 8/10.
"""
import bpy,sys,json,hashlib,math,random,traceback
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
original,photo,out=map(Path,a)
for p in ('before','after','glb','reports'):(out/p).mkdir(parents=True,exist_ok=True)
if not original.is_file():raise RuntimeError('Missing 63 original joint')
if not (photo/'plaster_stone_wall_02_diffuse.jpg').is_file():raise RuntimeError('Missing real CC0 mortar albedo')
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1100;sc.render.resolution_y=850
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria historic mortar single seam photo studio')
w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.26,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;sc.world=w
def area(n,pos,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=10
 obj=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(obj);obj.location=pos
 obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
area('warm raking texture',(-7,-9,11),2050,(1,.82,.70))
area('sky grazing texture',(8,3,9),1300,(.72,.85,1))
ca=bpy.data.cameras.new('mortar same before after')
cam=bpy.data.objects.new('mortar same before after',ca);bpy.context.collection.objects.link(cam);sc.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in objects if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def frame(obs):
 lo,hi=bbox(obs);center=(lo+hi)*.5;d=hi-lo
 cam.location=center+Vector((.24,-1.50,1.12)).normalized()*max(d.length,1)*2.9
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 ca.ortho_scale=max(d.x,d.y,d.z,.20)*1.54
def snap(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.import_scene.gltf(filepath=str(original))
olds=[o for o in bpy.data.objects if o not in rig]
meshes=[o for o in olds if o.type=='MESH']
lo,hi=bbox(meshes);frame(meshes)
dims=hi-lo
if not 1.6<dims.x<2.1:raise RuntimeError('Unexpected original modular joint length')
snap(out/'before'/'mortar_joint_recess.png')
for ob in olds:bpy.data.objects.remove(ob,do_unlink=True)
material=bpy.data.materials.new('PolyHaven_CC0_plaster_stone_wall_02_granular_mortar')
material.use_nodes=True
nodes=material.node_tree.nodes;nodes.clear();link=material.node_tree.links
diff=bpy.data.images.load(str(photo/'plaster_stone_wall_02_diffuse.jpg'),check_existing=True)
normal=bpy.data.images.load(str(photo/'plaster_stone_wall_02_nor_gl.jpg'),check_existing=True)
normal.colorspace_settings.name='Non-Color'
rough=bpy.data.images.load(str(photo/'plaster_stone_wall_02_rough.jpg'),check_existing=True)
rough.colorspace_settings.name='Non-Color'
albedo=nodes.new('ShaderNodeTexImage');albedo.image=diff
norm=nodes.new('ShaderNodeTexImage');norm.image=normal
rr=nodes.new('ShaderNodeTexImage');rr.image=rough
bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.95
link.new(albedo.outputs['Color'],bs.inputs['Base Color'])
bump=nodes.new('ShaderNodeNormalMap');bump.inputs['Strength'].default_value=.67
link.new(norm.outputs['Color'],bump.inputs['Color']);link.new(bump.outputs['Normal'],bs.inputs['Normal'])
# Real roughness photo adds irregular stone lime sheen, avoid mathematical gray.
link.new(rr.outputs['Color'],bs.inputs['Roughness'])
outnode=nodes.new('ShaderNodeOutputMaterial');link.new(bs.outputs[0],outnode.inputs['Surface'])
side=bpy.data.materials.new('physical_dark_recessed_aged_mortar_edge')
side.use_nodes=True
side.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.19,.18,.17,1)
side.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.97
W=float(dims.x);H=float(dims.z);D=max(.04,float(dims.y))
cx=(lo.x+hi.x)*.5;cy=(lo.y+hi.y)*.5;cz=(lo.z+hi.z)*.5
bpy.ops.mesh.primitive_cube_add(size=1,location=(cx,cy,cz))
core=bpy.context.object;core.name='shaded_5cm_deep_recess_core';core.dimensions=(W-.002,D,H-.005)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
core.data.materials.append(side)
front=float(lo.y)-.003
rng=random.Random(20261016)
n=110;m=24
verts=[];faces=[]
for j in range(m+1):
 for i in range(n+1):
  u=i/n;v=j/m
  x=float(lo.x)+W*u
  z=float(lo.z)+H*v
  noise=.004*math.sin(u*69+v*25)+.003*math.sin(u*107-v*44)+rng.uniform(-.003,.003)
  y=front+noise
  verts.append((x,y,z))
for j in range(m):
 for i in range(n):
  k=j*(n+1)+i;faces.append((k,k+1,k+n+2,k+n+1))
me=bpy.data.meshes.new('actually_gritty_photographic_lime_mortar')
me.from_pydata(verts,[],faces);me.update()
joints=bpy.data.objects.new('real_aged_lime_mortar_rough_surface',me)
bpy.context.collection.objects.link(joints);me.materials.append(material)
uv=me.uv_layers.new(name='UVMap')
for poly in me.polygons:
 for li in poly.loop_indices:
  p=me.vertices[me.loops[li].vertex_index].co
  uv.data[li].uv=((p.x-lo.x)/W,(p.z-lo.z)/H)
# Exposed stones embedded flush within the recessed mortar and irregular
# aggregate; no floating decorative gravel beyond the modular envelope.
objs=[core,joints]
for k in range(82):
 x=float(lo.x)+rng.uniform(.015,W-.015)
 z=float(lo.z)+rng.uniform(.012,H-.012)
 r=rng.uniform(.003,.010)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=r,location=(x,front-.001,z))
 o=bpy.context.object;o.name='authentic_embedded_lime_aggregate_'+str(k)
 o.scale=(rng.uniform(1.1,1.8),rng.uniform(.5,.7),rng.uniform(.6,1.2))
 o.data.materials.append(material);objs.append(o)
snap(out/'after'/'mortar_joint_recess.png')
bpy.ops.object.select_all(action='DESELECT')
for o in objs:o.select_set(True)
bpy.context.view_layer.objects.active=joints
dest=out/'glb'/'mortar_joint_recess.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<250000:raise RuntimeError('Historical mortar texture or normal missing from GLB')
report={'asset':'mortar_joint_recess','technical':'PASS_PENDING_ART',
 'photo_source':'Poly Haven plaster_stone_wall_02 CC0 (1k diffuse, roughness, normal)',
 'source_sha':sha(original),'new_sha':sha(dest),'true_lime_mortar_surface_faces':len(me.polygons),
 'embedded_lime_aggregate':82,'modular_width_m':W,
 'unrealized_integration':'Physical seam 1.8m x about 0.38m wall contact; test interlocking with separate masonry in Unity later',
 'art_approved':False,'unity_approved':False,'no_assembly':True}
(out/'reports'/'single_real_mortar_CC0_pbr.json').write_text(json.dumps(report,indent=2))
print('ONE_REAL_HISTORIC_MORTAR_PBR_MODULE',dest.stat().st_size,flush=True)
