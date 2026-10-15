"""Finish 12 still-underquality individual limestone reusable modules with authentic CC0
limestone normal/albedo, hue variation, restrained bevel, and genuine minor arch wedges.
Every asset keeps its original GLB and gets true Blender paired visual A/B. NO scene.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,cc0,out=map(Path,args[:3])
for folder in ('glb','before','after','reports'):(out/folder).mkdir(parents=True,exist_ok=True)
names=['arch_bridge_support','arch_complete_major','arch_complete_minor','arch_keystone_minor',
 'arch_voussoir_minor','stone_ashlar_04','stone_half_block','stone_corner_quoin','stone_slab_cap',
 'paver_flagstone','paver_rect_long','paver_rect_short']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=780;s.render.resolution_y=780
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Classic limestone professional lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.23,.29,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def area(n,pos,energy,c):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=energy;ld.color=c;ld.size=9;ld.shape='DISK'
 ob=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('Warm edge sculpt',(-7,-9,12),1600,(1,.8,.64))
area('Blue soft fill',(8,5,11),1530,(.70,.83,1))
area('Stone rim',(-4,7,8),900,(.96,.96,1))
ca=bpy.data.cameras.new('Individual study camera');cam=bpy.data.objects.new('Individual study camera',ca)
bpy.context.collection.objects.link(cam);s.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
p=cc0/'rock_3_diffuse.jpg';np=cc0/'rock_3_nor_gl.jpg'
if not p.is_file() or not np.is_file():raise RuntimeError('Missing CC0 maps')
im=bpy.data.images.load(str(p),check_existing=True)
normal=bpy.data.images.load(str(np),check_existing=True)
normal.colorspace_settings.name='Non-Color'
variants={}
for name,tint in [('stone',(.65,.58,.49,1)),('light',(.77,.69,.57,1)),('dark',(.41,.39,.37,1))]:
 mat=bpy.data.materials.new('ELDORIA_PBR_AGED_CC0_'+name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=im;tex.location=(-510,140)
 mix=nd.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.82
 mix.inputs[2].default_value=tint;lk.new(tex.outputs['Color'],mix.inputs[1])
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.86
 lk.new(mix.outputs['Color'],bs.inputs['Base Color'])
 normtex=nd.new('ShaderNodeTexImage');normtex.image=normal;normtex.location=(-520,-100)
 mapn=nd.new('ShaderNodeNormalMap');mapn.inputs['Strength'].default_value=.22
 lk.new(normtex.outputs['Color'],mapn.inputs['Color']);lk.new(mapn.outputs['Normal'],bs.inputs['Normal'])
 outn=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs[0],outn.inputs['Surface'])
 variants[name]=mat
def bbox(objects):
 pts=[obj.matrix_world@Vector(v) for obj in objects for v in obj.bound_box]
 return (Vector([min(p[i] for p in pts) for i in range(3)]),
         Vector([max(p[i] for p in pts) for i in range(3)]))
def camera(objects):
 lo,hi=bbox(objects);center=(lo+hi)*.5;dim=hi-lo
 cam.location=center+Vector((.88,-1.65,1.18)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 ca.ortho_scale=max(dim.x,dim.y,dim.z,.35)*1.75
def render(pth):s.render.filepath=str(pth);bpy.ops.render.render(write_still=True)
def sha(pth):return hashlib.sha256(pth.read_bytes()).hexdigest()
def uv_project(obj):
 uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap')
 for f in obj.data.polygons:
  axis=max(range(3),key=lambda i:abs(f.normal[i]));others=[i for i in range(3) if i!=axis]
  for i in f.loop_indices:
   v=obj.data.vertices[obj.data.loops[i].vertex_index].co
   uv.data[i].uv=(v[others[0]]*.57,v[others[1]]*.57)
def work(name,objects):
 updates=0
 for obj in objects:
  if obj.type!='MESH':continue
  for i,m in enumerate(obj.data.materials):
   if not m:continue
   old=m.name.lower()
   if 'iron' in old or 'wood' in old or 'gold' in old:continue
   kind='dark' if old.endswith('dark') or '_dark' in old else ('light' if old.endswith('light') or '_light' in old else 'stone')
   obj.data.materials[i]=variants[kind];updates+=1
  if name.startswith(('arch_keystone_minor','arch_voussoir_minor')):
   coords=[v.co for v in obj.data.vertices]
   zlo=min(p.z for p in coords);zhi=max(p.z for p in coords)
   cen=(min(p.x for p in coords)+max(p.x for p in coords))*.5
   for v in obj.data.vertices:
    ratio=(v.co.z-zlo)/max(zhi-zlo,.001)
    v.co.x=cen+(v.co.x-cen)*(.75+.27*ratio)
   obj.data.update()
  uv_project(obj)
  if len(obj.data.polygons)>3:
   mod=obj.modifiers.new('hand dressed masonry soft edges','BEVEL');mod.width=.009;mod.segments=2
   mod.limit_method='ANGLE'
 return updates
rows=[]
for i,n in enumerate(names,1):
 before=set(bpy.data.objects);r={'asset':n,'art_approved':False,'target':8}
 try:
  old=src/(n+'.glb');bpy.ops.import_scene.gltf(filepath=str(old))
  meshes=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
  if not meshes:raise RuntimeError('No mesh')
  camera(meshes);render(out/'before'/(n+'.png'))
  slots=work(n,meshes)
  if slots==0:raise RuntimeError('No limestone material recognized')
  render(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in meshes:o.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0]
  glb=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
  r.update({'technical':'PASS_PENDING_VISUAL','slot_count':slots,'original_sha':sha(old),'candidate_sha':sha(glb)})
  print('INDIVIDUAL_ARCH_STONE_PBR',i,'/12',n,slots,flush=True)
 except Exception as e:
  r.update({'technical':'FAIL','error':str(e)});traceback.print_exc()
 finally:
  rows.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'twelve_limestone_quality.json').write_text(json.dumps({'only_12_individuals':True,'art_approved':False,
    'base_run':38813966671,'donor_pbr_run':38823501643,'items':rows},indent=2))
if len(rows)!=12 or any(r['technical']!='PASS_PENDING_VISUAL' for r in rows):raise RuntimeError('Incomplete')
print('TWELVE_LIMESTONE_ASSETS_REAL_BLENDER_DONE',flush=True)
