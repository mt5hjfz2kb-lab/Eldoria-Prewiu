"""Repair four still-sub-6 arc voussoir / keystone MODULES: genuine solid radial
tapered trapezoids, six dressed faces, correct CC0 limestone UV on all surfaces.
Blender native A/B against best existing GLBs. No construction/composition of arc.
"""
import bpy,sys,random,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=4:raise SystemExit('best major GLB folder, best minor GLB folder, CC0 maps, out')
majors,minors,maps,out=map(Path,A)
names=['arch_keystone_major','arch_voussoir_major','arch_keystone_minor','arch_voussoir_minor']
for sub in ('glb','before','after','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE';sc.render.resolution_x=1050;sc.render.resolution_y=1050
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('single radial dressed stone studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.77;sc.world=w
def light(n,power,pos,c):
 l=bpy.data.lights.new(n,'AREA');l.color=c;l.energy=power;l.size=9;l.shape='DISK'
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('Amber stone raking',1700,(-7,-8,12),(1,.83,.67))
light('Blue skylight',1620,(7,4,12),(.7,.82,1))
light('edge',930,(-2,9,11),(.99,.97,.91))
ca=bpy.data.cameras.new('identical A-B camera');cam=bpy.data.objects.new('identical A-B camera',ca)
bpy.context.collection.objects.link(cam);sc.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def box(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objects if o.type=='MESH' for v in o.bound_box]
 return Vector([min(x[i] for x in pts) for i in range(3)]),Vector([max(x[i] for x in pts) for i in range(3)])
def view(objects):
 lo,hi=box(objects);c=(lo+hi)/2;e=hi-lo
 cam.location=c+Vector((.95,-1.68,1.2)).normalized()*max(e.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 ca.ortho_scale=max(e.x,e.y,e.z,.4)*1.67
def shoot(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
col=maps/'medieval_blocks_03_diffuse.jpg';nrm=maps/'medieval_blocks_03_nor_gl.jpg'
if not col.is_file() or not nrm.is_file():raise RuntimeError('Official CC0 photo source missing')
im=bpy.data.images.load(str(col),check_existing=True)
ni=bpy.data.images.load(str(nrm),check_existing=True);ni.colorspace_settings.name='Non-Color'
stone=bpy.data.materials.new('Valoria_CC0_photo_limestone_hewn_wedges');stone.use_nodes=True
nodes=stone.node_tree.nodes;nodes.clear();links=stone.node_tree.links
tex=nodes.new('ShaderNodeTexImage');tex.image=im;tex.extension='EXTEND'
mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.24
mix.inputs[2].default_value=(.83,.77,.68,1)
links.new(tex.outputs['Color'],mix.inputs[1])
bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.89
links.new(mix.outputs['Color'],bs.inputs['Base Color'])
tn=nodes.new('ShaderNodeTexImage');tn.image=ni;tn.extension='EXTEND'
norm=nodes.new('ShaderNodeNormalMap');norm.inputs['Strength'].default_value=.29
links.new(tn.outputs['Color'],norm.inputs['Color']);links.new(norm.outputs['Normal'],bs.inputs['Normal'])
output=nodes.new('ShaderNodeOutputMaterial');links.new(bs.outputs[0],output.inputs['Surface'])
patches=[(160,25,420,170),(505,21,760,166),(55,225,265,382),
         (322,230,500,371),(701,225,840,370),(38,432,315,594),
         (400,433,563,590),(620,443,911,592),(28,636,285,786),
         (325,654,742,790),(215,854,495,985),(695,847,900,984)]
def texture_on_faces(ob,seed):
 rng=random.Random(seed)
 # Use UV crop within individual real limestone block photographic interiors,
 # with each face projected in its own plane. No dark photographic mortar seams.
 uv=ob.data.uv_layers.active or ob.data.uv_layers.new(name='UVMap')
 verts=[v.co for v in ob.data.vertices]
 mins=[min(x[i] for x in verts) for i in range(3)]
 ext=[max(x[i] for x in verts)-mins[i] for i in range(3)]
 for p in ob.data.polygons:
  x1,y1,x2,y2=rng.choice(patches)
  u0,u1=x1/1024.,x2/1024.;v0,v1=1-y2/1024.,1-y1/1024.
  axis=max(range(3),key=lambda i:abs(p.normal[i]))
  a,b=([0,2] if axis==1 else [0,1] if axis==2 else [1,2])
  for j in p.loop_indices:
   v=ob.data.vertices[ob.data.loops[j].vertex_index].co
   uu=(v[a]-mins[a])/max(ext[a],.000001);vv=(v[b]-mins[b])/max(ext[b],.000001)
   uv.data[j].uv=(u0+uu*(u1-u0),v0+vv*(v1-v0))
def rebuild(name,lo,hi):
 x=(lo.x+hi.x)/2;yy=(lo.y+hi.y)/2;zlo=lo.z;zhi=hi.z
 full=float(hi.x-lo.x)
 a=full*(.36 if 'keystone' in name else .29)
 b=full*(.49 if 'keystone' in name else .46)
 # Nonrectangular tapered radial solid; top face physically wider than bottom.
 # Equal depth front and back; this has true angled intrados side planes.
 verts=[(x-a,lo.y,zlo),(x+a,lo.y,zlo),(x+b,lo.y,zhi),(x-b,lo.y,zhi),
        (x-a,hi.y,zlo),(x+a,hi.y,zlo),(x+b,hi.y,zhi),(x-b,hi.y,zhi)]
 faces=[(3,2,1,0),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
 m=bpy.data.meshes.new('TRUE_ANGLED_DRESSED_RADIAL_STONE_'+name)
 m.from_pydata(verts,[],faces);m.update()
 ob=bpy.data.objects.new(name+'_independent_hand_carved_wedge',m)
 bpy.context.collection.objects.link(ob);m.materials.append(stone)
 bevel=ob.modifiers.new('historical hand dressed arrises','BEVEL')
 bevel.width=.014;bevel.segments=3;bevel.limit_method='ANGLE'
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.modifier_apply(modifier=bevel.name)
 texture_on_faces(ob,199+sum(ord(c) for c in name))
 return [ob]
results=[]
for i,name in enumerate(names,1):
 prev=set(bpy.data.objects);p=(minors if 'minor' in name else majors)/(name+'.glb')
 report={'asset':name,'target':6,'art_approved':False,'no_composition':True}
 try:
  bpy.ops.import_scene.gltf(filepath=str(p))
  old=[o for o in bpy.data.objects if o not in prev]
  meshes=[o for o in old if o.type=='MESH']
  if not meshes:raise RuntimeError('Old GLB no meshes')
  lo,hi=box(meshes);view(meshes);shoot(out/'before'/(name+'.png'))
  for o in old:bpy.data.objects.remove(o,do_unlink=True)
  cand=rebuild(name,lo,hi)
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in cand:ob.select_set(True)
  bpy.context.view_layer.objects.active=cand[0]
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<20000:raise RuntimeError('Tiny wedge export')
  report.update({'technical':'PASS_PENDING_VISUAL','original_sha':sha(p),
    'candidate_sha':sha(target),'true_wedge_shape':True,
    'triangle_count':len(cand[0].data.polygons)})
  print('ART_TRUE_RADIAL_WEDGE_A_B',i,'/',len(names),name,flush=True)
 except Exception as e:
  report.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(report)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'four_true_radial_arch_wedges.json').write_text(json.dumps({
   'major_source':38822723551,'minor_source':38825364463,'PBR_cc0':38860976002,
   'all_art_approved':False,'units':results},indent=2))
if len(results)!=4 or any(x['technical']!='PASS_PENDING_VISUAL' for x in results):
 raise RuntimeError('Arch wedge batch failure')
print('FOUR_TRUE_INDIVIDUAL_STONE_WEDGES_DONE',flush=True)
