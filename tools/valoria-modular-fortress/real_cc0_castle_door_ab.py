"""ELDORIA: actual single-piece A/B of large_castle_door CC0 (Poly Haven).
No fortress composition, no Unity writes, no invented ART_APPROVED.
Preserve source doors/hinge pivot, frame, PBR textures, UVs and scale.
"""
import bpy,json,sys,math,hashlib,traceback
from pathlib import Path
from mathutils import Vector,Matrix
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise RuntimeError('Need previous-door-GLB, CC0-source-dir, output-dir')
original,src,out=Path(arg[0]),Path(arg[1]),Path(arg[2])
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
asset='reinforced_oak_gate'
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene
s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1050;s.render.resolution_y=1050;s.render.resolution_percentage=100
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria isolated real-gate lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def light(n,power,pos,color):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=power;ld.color=color;ld.shape='DISK';ld.size=9
 o=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('old-gold raking key',2000,(-7,-9,13),(1,.83,.68))
light('cool overcast fill',1500,(8,3,12),(.70,.82,1))
light('iron edge highlight',970,(2,10,9),(.98,.97,.88))
cd=bpy.data.cameras.new('unaltered paired camera');cam=bpy.data.objects.new('unaltered paired camera',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for part in iter(lambda:f.read(1024*1024),b''):h.update(part)
 return h.hexdigest()
def load(p):
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(p))
 loaded=[o for o in bpy.data.objects if o not in before]
 mesh=[o for o in loaded if o.type=='MESH']
 if not mesh:raise RuntimeError('No actual 3D mesh imported: '+str(p))
 return loaded,mesh
def box(mesh):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in mesh for v in o.bound_box]
 return (Vector([min(v[i] for v in pts) for i in range(3)]),
         Vector([max(v[i] for v in pts) for i in range(3)]))
def render(p):
 s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
if not original.is_file():raise RuntimeError('Previous gate GLB missing')
oldall,oldmesh=load(original)
lo,hi=box(oldmesh);center=(lo+hi)*.5;dim=hi-lo
cam.location=center+Vector((.96,-1.65,1.18)).normalized()*max(dim.length,1)*3.3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(dim.x,dim.y,dim.z,.5)*1.9
render(out/'before'/(asset+'.png'))
prior={'mesh_count':len(oldmesh),'face_count':sum(len(m.data.polygons) for m in oldmesh),
 'world_dimensions':[round(float(x),4) for x in dim],'original_sha':sha(original)}
for o in oldall:bpy.data.objects.remove(o,do_unlink=True)
source=src/'large_castle_door_1k.gltf'
if not source.is_file():raise RuntimeError('Missing verified official CC0 1k GLTF')
gltf=json.loads(source.read_text(encoding='utf-8'))
# Artifact deliberately kept exact source bytes. Local import path fix is temporary.
for el in gltf.get('buffers',[])+gltf.get('images',[]):
 if 'uri' in el and not el['uri'].startswith('data:'):
  el['uri']=Path(el['uri']).name
temp=src/'large_castle_door_locally_importable.gltf'
temp.write_text(json.dumps(gltf),encoding='utf-8')
imported,models=load(temp)
source_mesh_names=[o.name for o in models]
source_scene_names=[o.name for o in imported]
source_materials=sorted({m.name for o in models for m in o.data.materials if m})
firstlo,firsthi=box(models);scandim=firsthi-firstlo
if scandim.z < .01:raise RuntimeError('Source door is non-vertical or collapsed')
target_height=float(dim.z)
scale=target_height/max(float(scandim.z),.01)
# Change each root matrix, retaining child door hinge transforms and original UVs.
for o in imported:
 if o.parent is None:o.matrix_world=Matrix.Scale(scale,4) @ o.matrix_world
bpy.context.view_layer.update()
belo,behi=box(models)
offset=Vector((center.x-(belo.x+behi.x)*.5, center.y-(belo.y+behi.y)*.5,lo.z-belo.z))
for o in imported:
 if o.parent is None:o.matrix_world=Matrix.Translation(offset) @ o.matrix_world
bpy.context.view_layer.update()
adjusted_lo,adjusted_hi=box(models);adjusted=adjusted_hi-adjusted_lo
if abs(adjusted.z-dim.z)>0.02:raise RuntimeError('Donor scaling exceeded door height tolerance')
# Preserve all photographed CC0 diffuse, normal and metallic/roughness nodes.
texture_images=sorted({n.image.name for o in models for m in o.data.materials if m and m.use_nodes
                        for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image})
if not texture_images:raise RuntimeError('Imported door has lost CC0 PBR photo-textures')
for o in models:
 for m in o.data.materials:
  if m is None or not m.use_nodes:continue
  for node in m.node_tree.nodes:
   if node.type=='BSDF_PRINCIPLED':
    # Protect original grain/iron detail: neutral style direction only.
    node.inputs['Roughness'].default_value=.77
    node.inputs['Metallic'].default_value=.08
    if 'Coat Weight' in node.inputs:node.inputs['Coat Weight'].default_value=.025
render(out/'after'/(asset+'.png'))
original_loc=cam.location.copy();original_rot=cam.rotation_euler.copy()
cam.location=center+Vector((-.96,1.65,1.18)).normalized()*max(dim.length,1)*3.3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
render(out/'after'/(asset+'_rear.png'))
cam.location=original_loc;cam.rotation_euler=original_rot
bpy.ops.object.select_all(action='DESELECT')
for ob in imported:ob.select_set(True)
bpy.context.view_layer.objects.active=models[0]
dest=out/'glb'/(asset+'.glb')
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<100000:raise RuntimeError('Photogrammetric original door mesh stripped/collapsed')
report={'asset':asset,'technical':'PASS_PENDING_VISUAL',
 'asset_source':'Poly Haven large_castle_door 1K, CC0','source_glTF_sha':sha(source),
 'baseline':prior,'donor_mesh_names':source_mesh_names,'donor_node_names':source_scene_names,
 'donor_materials':source_materials,'donor_texture_images':texture_images,
 'candidate_world_dimensions':[round(float(x),4) for x in adjusted],
 'size_relative_to_original':[round(float(adjusted[i]/max(dim[i],.0001)),3) for i in range(3)],
 'out_bytes':dest.stat().st_size,'candidate_sha':sha(dest),
 'art_approved':False,'unity_approved':False,
 'limitations':'Standalone door-frame-and-paired-leaves component. Hinges/pivots and stone/iron visual integration must be checked by human and in Unity; never auto-promote.',
 'no_composition':True}
(out/'reports'/'independent_authentic_castle_gate.json').write_text(json.dumps(report,indent=2))
print('AUTHENTIC_CASTLE_DOOR_SINGLE_ASSET_READY',json.dumps({
 'mesh_count':len(models),'poly_count':sum(len(o.data.polygons) for o in models),
 'images':len(texture_images),'bytes':dest.stat().st_size}),flush=True)
