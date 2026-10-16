"""11 independent under-six Valoria tower/wall stone modules. REAL Poly Haven
medieval_blocks_03 CC0 scanned limestone grain cropped into individual masonry
stone faces with per-face planar UV; preserve original tower/buttress/coping silhouette.
Two studio PNG per GLB, no fortress composition or auto score.
"""
import bpy,sys,json,random,math,hashlib,traceback
from pathlib import Path
from mathutils import Vector
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,cc0,out=map(Path,A)
N=['arch_bridge_support','merlon_low','merlon_tall','parapet_walk_4m',
 'wall_buttress','wall_corner_90','wall_course_4m','wall_panel_crenellated_4m',
 'tower_square_tall','tower_square_short','wall_panel_ruined_4m']
for x in ('before','after','glb','reports'):(out/x).mkdir(exist_ok=True,parents=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=850;s.render.resolution_y=850
s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Warm stone architectural objects only');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.26,.34,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;s.world=w
def light(n,p,e,c):
 d=bpy.data.lights.new(n,'AREA');d.color=c;d.energy=e;d.shape='DISK';d.size=10
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('golden architectural modelling',(-9,-10,15),1900,(1,.85,.7))
light('northern blue fill',(8,3,13),1600,(.7,.83,1))
light('rim highlight',(-3,9,10),1050,(1,.96,.86))
cd=bpy.data.cameras.new('frozen 1:1 before after')
cam=bpy.data.objects.new('frozen 1:1 before after',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bbox(objs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objs for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def studio(objs):
 lo,hi=bbox(objs);mid=(lo+hi)/2;e=hi-lo
 cam.location=mid+Vector((.85,-1.55,1.17)).normalized()*max(e.length,1)*3.0
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(e.x,e.y,e.z,.5)*1.73
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
col=cc0/'medieval_blocks_03_diffuse.jpg'
norm=cc0/'medieval_blocks_03_nor_gl.jpg'
if not col.is_file() or not norm.is_file():raise RuntimeError('Source CC0 photo PBR missing')
im=bpy.data.images.load(str(col),check_existing=True)
ni=bpy.data.images.load(str(norm),check_existing=True);ni.colorspace_settings.name='Non-Color'
stones=[]
for material,gray in [('light',(.85,.81,.74)),('mid',(.77,.73,.68)),('dark',(.61,.61,.59))]:
 m=bpy.data.materials.new('Valoria_Photo_Hand_Dressed_CC0_'+material);m.use_nodes=True
 nd=m.node_tree.nodes;nd.clear();lk=m.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=im;tex.extension='EXTEND'
 mix=nd.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.35
 mix.inputs[2].default_value=(*gray,1);lk.new(tex.outputs['Color'],mix.inputs[1])
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.9
 lk.new(mix.outputs['Color'],bs.inputs['Base Color'])
 texn=nd.new('ShaderNodeTexImage');texn.image=ni;texn.extension='EXTEND'
 normalnode=nd.new('ShaderNodeNormalMap');normalnode.inputs['Strength'].default_value=.25
 lk.new(texn.outputs['Color'],normalnode.inputs['Color']);lk.new(normalnode.outputs['Normal'],bs.inputs['Normal'])
 op=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs[0],op.inputs['Surface'])
 stones.append(m)
# Cropped faces from masonry photograph, avoiding photographic mortar lines.
patches=[(160,25,420,170),(505,21,760,166),(55,225,265,382),
 (322,230,500,371),(701,225,840,370),(38,432,315,594),
 (400,433,563,590),(620,443,911,592),(28,636,285,786),
 (325,654,742,790),(215,854,495,985),(695,847,900,984)]
def uv_map(o,patch):
 a,b,c,d=patch;u0=a/1024.;u1=c/1024.;v0=1-d/1024.;v1=1-b/1024.
 uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 co=[v.co for v in o.data.vertices]
 low=[min(v[i] for v in co) for i in range(3)]
 dims=[max(v[i] for v in co)-low[i] for i in range(3)]
 for p in o.data.polygons:
  ax=max(range(3),key=lambda i:abs(p.normal[i]))
  indices=([0,2] if ax==1 else [0,1] if ax==2 else [1,2])
  for idx in p.loop_indices:
   v=o.data.vertices[o.data.loops[idx].vertex_index].co
   uu=(v[indices[0]]-low[indices[0]])/max(dims[indices[0]],.00001)
   vv=(v[indices[1]]-low[indices[1]])/max(dims[indices[1]],.00001)
   uv.data[idx].uv=(u0+uu*(u1-u0),v0+vv*(v1-v0))
def retouch(name,objs):
 rng=random.Random(sum(ord(c) for c in name))
 slots=0;stone_objects=0
 for ob in objs:
  if ob.type!='MESH' or not ob.data.vertices:continue
  # Never change real wooden scaffolding or iron if present.
  mats=[m.name.lower() if m else '' for m in ob.data.materials]
  if any('iron' in k or 'wood' in k or 'metal' in k for k in mats):continue
  if not mats:ob.data.materials.append(stones[rng.choices([0,1,2],[4,5,1])[0]]);slots+=1
  else:
   for i,m in enumerate(ob.data.materials):
    if m and any(x in m.name.lower() for x in ('wood','metal','iron')):continue
    ob.data.materials[i]=stones[rng.choices([0,1,2],[4,5,1])[0]];slots+=1
  # bevel only an actual stone with space for a meaningful dressed arris.
  minimum=min(ob.dimensions)
  if len(ob.data.vertices)>4 and minimum>.14:
   b=ob.modifiers.new('artisan_honed_edges','BEVEL')
   b.width=min(.018,minimum*.08);b.segments=2;b.limit_method='ANGLE'
   bpy.ops.object.select_all(action='DESELECT')
   ob.select_set(True);bpy.context.view_layer.objects.active=ob
   bpy.ops.object.modifier_apply(modifier=b.name)
  uv_map(ob,rng.choice(patches));stone_objects+=1
 return slots,stone_objects
rows=[]
for i,name in enumerate(N,1):
 start=set(bpy.data.objects)
 report={'asset':name,'art_approved':False,'target_visual_floor':6,'no_composition':True}
 try:
  srcfile=src/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(srcfile))
  models=[o for o in bpy.data.objects if o not in start and o.type=='MESH']
  if not models:raise RuntimeError('No original mesh')
  studio(models);shoot(out/'before'/(name+'.png'))
  slots,meshes=retouch(name,models)
  if not slots:raise RuntimeError('No actual stone photo material slots modified')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in models:ob.select_set(True)
  bpy.context.view_layer.objects.active=models[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<10000:raise RuntimeError('Bad glb')
  report.update({'technical':'PASS_PENDING_ART','stone_mesh_count':meshes,
    'new_CC0_slots':slots,'source_sha':sha(srcfile),'candidate_sha':sha(dest)})
  print('TRUE_STONE_FACE_PER_MODULE',i,'/',len(N),name,meshes,slots,flush=True)
 except Exception as e:
  report.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  rows.append(report)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'eleven_stone_modules.json').write_text(json.dumps({
   'scope':'11 separate assets not an assembled fortress',
   'base_run':38813966671,'CC0_source_run':38860976002,
   'all_art_approved':False,'assets':rows},indent=2))
if len(rows)!=len(N) or any(x['technical']!='PASS_PENDING_ART' for x in rows):
 raise RuntimeError('Some individual architecture modules failed')
print('ELEVEN_ARCHITECTURAL_PHOTO_STONE_CANDIDATES_READY',flush=True)
