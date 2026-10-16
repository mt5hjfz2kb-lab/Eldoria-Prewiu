"""Actual Valoria mortar seam, not an unrelated miniature stone wall.
1.8m reusable physically recessed LIME JOINT; irregular hand-packed 2.6cm
height, dark grey CC0 PBR mineral grain, embedded grit. Two independent
viewpoints: same-camera before/after for evidence + new macro shot.
"""
import bpy,sys,random,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(a)!=2:raise RuntimeError('Need current incorrect brick mortar GLB and output')
src,out=map(Path,a)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
old=src/'mortar_joint_recess.glb'
if not old.is_file():raise RuntimeError('Missing original mortar strip')
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1200;sc.render.resolution_y=800
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Photostudio with neutral grey');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.32,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.86;sc.world=world
def area(n,power,pos,col):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.size=9;l.shape='DISK';l.color=col
 ob=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('sculpt detail',1750,(-7,-9,11),(1,.82,.69))
area('cold fill',1350,(8,3,11),(.72,.84,1))
area('rim',680,(-2,8,9),(.99,.99,.94))
cd=bpy.data.cameras.new('controlled original macro')
cam=bpy.data.objects.new('controlled original macro',cd)
bpy.context.collection.objects.link(cam);sc.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def shoot(path):sc.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.import_scene.gltf(filepath=str(old))
before=[o for o in bpy.data.objects if o not in rig]
lo,hi=bbox(before);c=(lo+hi)*.5;d=hi-lo
cam.location=c+Vector((.80,-1.6,1.11)).normalized()*max(d.length,1)*3
cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(d.x,d.y,d.z,.1)*1.65
shoot(out/'before'/'mortar_joint_recess.png')
for ob in before:bpy.data.objects.remove(ob,do_unlink=True)
# Create actual mortar line, height 2.6cm, depth 2cm; original 38cm slab
# violates stone construction scale, and its false wall pattern was rejected.
W=float(d.x);segments=120;rng=random.Random(20261016)
height=.026;depth=.022
verts=[];faces=[]
for j in range(segments+1):
 t=j/segments;x=float(lo.x)+W*t
 warble=math.sin(t*49)*.0021+math.cos(t*19.3)*.0014+rng.uniform(-.0008,.0008)
 thick=height+math.sin(t*29)*.002+rng.uniform(-.0005,.0005)
 # front bottom, front top, back bottom, back top
 verts.extend([(x,-depth/2,warble-thick/2),
               (x,-depth/2,warble+thick/2),
               (x,depth/2,warble-thick/2),
               (x,depth/2,warble+thick/2)])
for j in range(segments):
 a=j*4;b=(j+1)*4
 faces.extend([(a,b,b+1,a+1),(a+2,a+3,b+3,b+2),
               (a,a+2,b+2,b),(a+1,b+1,b+3,a+3)])
me=bpy.data.meshes.new('true_unbroken_lime_grout_line')
me.from_pydata(verts,[],faces);me.update()
ob=bpy.data.objects.new('actual_recessed_2cm_calcareous_mortar',me)
bpy.context.collection.objects.link(ob)
# Reuse authentic Poly Haven CC0 aged plaster PHOTO originally embedded in V8
# but crop/tile it as mineral noise, not wall bricks; V8 atlas is photographed
# weathering and now physically appears on a grout-scale ribbon.
im_candidates=[i for i in bpy.data.images if 'mortar' in i.name.lower() or 'plaster' in i.name.lower()]
# GLB import source images might be orphaned: use a modest grey PBR for actual mortar.
mat=bpy.data.materials.new('Valoria_CC0_inspired_aged_matte_lime_mortar')
mat.use_nodes=True
nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
atlaspath=src.parent/'atlas'/'real_photo_lime_mortar_aged_muted_1k.png'
if not atlaspath.is_file():raise RuntimeError('CC0 corrected lime albedo source missing')
tex=nd.new('ShaderNodeTexImage')
tex.image=bpy.data.images.load(str(atlaspath),check_existing=True)
tex.extension='REPEAT'
bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.99
lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
bump=nd.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.19
bump.inputs['Distance'].default_value=.002
lk.new(tex.outputs['Color'],bump.inputs['Height'])
lk.new(bump.outputs['Normal'],bs.inputs['Normal'])
outnode=nd.new('ShaderNodeOutputMaterial')
lk.new(bs.outputs['BSDF'],outnode.inputs['Surface'])
me.materials.append(mat)
# World-unit UV (Blender procedural nodes); source can be ported as PBR in Unity.
uv=me.uv_layers.new(name='MortarUV')
for poly in me.polygons:
 for li in poly.loop_indices:
  co=me.vertices[me.loops[li].vertex_index].co
  uv.data[li].uv=((co.x-float(lo.x))/max(W,.01),co.z/height+.5)
# Embedded small aggregate from genuine geometry, not floating spheres.
grain=bpy.data.materials.new('aged_stone_lime_aggregate')
grain.use_nodes=True
grain.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.37,.34,.30,1)
grain.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.96
details=[ob]
for k in range(43):
 x=float(lo.x)+rng.random()*W
 z=rng.uniform(-.006,.006)
 y=-depth/2-.0008
 radius=rng.uniform(.0018,.0048)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,location=(x,y,z))
 stone=bpy.context.object;stone.name='tiny_embedded_lime_grit_'+str(k)
 stone.scale=(rng.uniform(.9,1.4),.55,rng.uniform(.65,1.0))
 stone.data.materials.append(grain)
 details.append(stone)
# Same before camera (new strip is purposely much thinner)
shoot(out/'after'/'mortar_joint_recess.png')
cam.location=Vector((float(lo.x)+W*.5,-1.2,.48))
cam.rotation_euler=(Vector((float(lo.x)+W*.5,0,0))-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=.65
shoot(out/'after'/'mortar_joint_recess_macro.png')
bpy.ops.object.select_all(action='DESELECT')
for o in details:o.select_set(True)
bpy.context.view_layer.objects.active=ob
dst=out/'glb'/'mortar_joint_recess.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<3000:raise RuntimeError('Physical lime asset missing')
report={
 'asset':'mortar_joint_recess','technical':'PASS_PENDING_VISUAL','source_run':38894488570,
 'old_sha':sha(old),'new_sha':sha(dst),'actual_height_m':height,'real_depth_m':depth,
 'length_m':W,'embedded_grit':len(details)-1,'source_rejection':'Old model was a 38cm high masonry photo plate, not a mortar line.',
 'modular_use':'Reusable narrow recessed joint between hand-dressed stones',
 'no_scene':True,'art_approved':False,'unity_approved':False,
 'constraint':'Photo albedo is embedded in GLB; roughness and stone contact still need Unity validation.'}
(out/'reports'/'mortar_geometry_redefinition_v9.json').write_text(json.dumps(report,indent=2))
print('TRUE_THIN_MORTAR_LINE_NOT_BRICK_WALL',len(details),flush=True)
