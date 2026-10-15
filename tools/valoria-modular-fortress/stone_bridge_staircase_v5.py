"""Valoria two individual bridge/stair modules. Real constructive detail; NO scene.
Use original V3 glb A/B, CC0 actual limestone photos, own geometry, native Blender 5.2.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,tex,out=map(Path,argv[:3])
for x in ('glb','before','after','reports'):(out/x).mkdir(exist_ok=True,parents=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1024;s.render.resolution_y=1024;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria native study neutral');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.29,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=w
def area(n,pos,pow,color):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=pow;ld.color=color;ld.shape='DISK';ld.size=10
 ob=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(ob)
 ob.location=pos;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('Sunset key',(-8,-9,14),2200,(1,.8,.61));area('sky fill',(9,2,12),1800,(.69,.81,1));area('rim',(0,11,10),1000,(1,1,1))
camd=bpy.data.cameras.new('Locked pair camera');cam=bpy.data.objects.new('Locked pair camera',camd);bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def pbr():
 path=tex/'rock_3_diffuse.jpg'
 if not path.is_file():raise RuntimeError('Poly Haven CC0 source missing')
 im=bpy.data.images.load(str(path),check_existing=True)
 mat=bpy.data.materials.new('Valoria_dressed_frosted_limestone_CC0');mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();l=mat.node_tree.links
 image=nd.new('ShaderNodeTexImage');image.image=im;image.location=(-650,120)
 mult=nd.new('ShaderNodeMixRGB');mult.blend_type='MULTIPLY';mult.inputs[0].default_value=.66
 mult.inputs[2].default_value=(.69,.61,.51,1);l.new(image.outputs['Color'],mult.inputs[1])
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.86
 l.new(mult.outputs['Color'],bs.inputs['Base Color'])
 npath=tex/'rock_3_nor_gl.jpg'
 if npath.is_file():
  nim=bpy.data.images.load(str(npath),check_existing=True);nim.colorspace_settings.name='Non-Color'
  imn=nd.new('ShaderNodeTexImage');imn.image=nim
  nor=nd.new('ShaderNodeNormalMap');nor.inputs['Strength'].default_value=.25
  l.new(imn.outputs['Color'],nor.inputs['Color']);l.new(nor.outputs['Normal'],bs.inputs['Normal'])
 out=nd.new('ShaderNodeOutputMaterial');l.new(bs.outputs[0],out.inputs['Surface'])
 return mat
stone=pbr()
darker=bpy.data.materials.new('Stone_aged_dark_joint');darker.diffuse_color=(.34,.31,.30,1);darker.use_nodes=True;darker.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.34,.31,.30,1)
def cube(name,loc,dim,mat=stone,bevel=.016):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=dim
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 if bevel:
  b=o.modifiers.new('small limestone light-catching worn corners','BEVEL');b.width=min(bevel,min(dim)*.2);b.segments=2;b.limit_method='ANGLE'
 # correct per-face planar UV
 uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 for p in o.data.polygons:
  ax=max(range(3),key=lambda k:abs(p.normal[k]));plane=[i for i in range(3) if i!=ax]
  for i in p.loop_indices:
   v=o.data.vertices[o.data.loops[i].vertex_index].co;uv.data[i].uv=(v[plane[0]]*.74,v[plane[1]]*.74)
 return o
def beam(name,a,b,width,mat=stone):
 a=Vector(a);b=Vector(b);mid=(a+b)*.5
 ob=cube(name,mid,(width,width,(b-a).length),mat=mat,bevel=.018)
 ob.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
 return ob
def stairs():
 R=random.Random(330)
 group=[]
 # 12 real limestone treads, each with distinct joints and chiselled nosings.
 width=4;depth=.62;rise=.238
 for i in range(12):
  y=-3.4+i*.60;z=.125+rise*(i+1)
  # 2-3 separate paver slabs per tread for more crafted texture and joints
  slices=[(-1.43,1.15),(0,1.62),(1.43,1.12)] if i%3 else [(-1.12,1.86),(1.08,1.93)]
  for j,(x,sx) in enumerate(slices):
   o=cube('limestone_tread_'+str(i)+'_'+str(j),(x,y,z),(sx-.03,.625,.22),stone,.024)
   o.rotation_euler.z=R.uniform(-.006,.006);group.append(o)
  # heavy small vertical riser behind limestone caps
  group.append(cube('riser_mortar_'+str(i),(0,y+.235,z-.18),(3.9,.11,.23),darker,.001))
 # Side abutments: true ascending course of stones, not bare flying step geometry
 for side in (-1,1):
  x=side*2.25
  for i in range(13):
   y=-3.4+i*.60;z=.15+rise*(i+1)
   h=.53+rise*(i+1)
   group.append(cube('ascending_stair_wall_course_'+str(i),(x,y,h/2),(.57,.62,h),stone,.019))
  # Tall solid posts at both landings: readable but not overly ornate
  for j,y in enumerate((-3.8,3.65)):
   z=.35 if j==0 else 3.0
   group.append(cube('tall_balustrade_pier',(x,y,z+.53),(.69,.72,1.03),stone,.025))
   group.append(cube('pier_cap',(x,y,z+1.08),(.86,.86,.18),stone,.027))
 # base footing adds mass and shadow under flight; sink 0.12 into ground
 group.append(cube('stone_footing', (0,-.15,-.18),(4.6,8.13,.27),darker,.005))
 return group
def bridge():
 R=random.Random(328)
 group=[]
 # Five metre road deck; 6 course subdivisions by travel direction y.
 group.append(cube('bridge_loadbearing_slab',(0,0,-.23),(5.0,4.5,.38),darker,.012))
 for row in range(8):
  y=-1.98+row*.56
  width=4.22
  # offset joints to avoid checkerboard
  centers=[(-1.58,.98),(-.55,1.04),(.58,1.07),(1.62,.99)] if row%2 else [(-1.85,.47),(-.9,1.05),(.13,1.04),(1.19,1.06),(2.0,.34)]
  for col,(x,dx) in enumerate(centers):
   group.append(cube('bridge_hand_laid_paver_r'+str(row)+'_c'+str(col),
    (x,y,.10+R.uniform(-.012,.014)),(dx-.025,.56-.025,.18),stone,.018))
 for side in (-1,1):
  x=side*2.42
  for i in range(9):
   y=-2.02+i*.52
   # parapet below has massive stone courses; top capped with a lighter ashlar
   for j in range(3):
    group.append(cube('bridge_parapet_stone_'+str(side)+'_'+str(i)+'_'+str(j),
     (x,y,.24+j*.38),(.54,.515,.37),stone,.018))
   if i%2==0:
    group.append(cube('bridge_crenel_post',(x,y,1.49),(.59,.53,.50),stone,.021))
  group.append(cube('bridge_coping_span',(x,0,1.34),(.69,4.76,.16),stone,.027))
  for y in (-2.15,2.15):
   group.append(cube('bridge_corner_pier',(x,y,.88),(.86,.78,1.6),stone,.029))
   group.append(cube('bridge_corner_cap',(x,y,1.76),(1.06,1.0,.2),stone,.028))
 return group
def bbox(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 lo=Vector([min(v[i] for v in pts) for i in range(3)])
 hi=Vector([max(v[i] for v in pts) for i in range(3)])
 return lo,hi
def camset(objs):
 lo,hi=bbox(objs);mid=(lo+hi)/2;dim=hi-lo
 cam.location=mid+Vector((.9,-1.6,1.17)).normalized()*max(dim.length,1)*3
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 camd.ortho_scale=max(dim.x,dim.y,dim.z)*1.80
def render(outpath):
 s.render.filepath=str(outpath);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
result=[]
for n,author in [('staircase_flight_12',stairs),('bridge_span_4m',bridge)]:
 keep=set(bpy.data.objects);row={'asset':n,'art_approved':False,'target':8}
 try:
  original=src/(n+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(original))
  old=[o for o in bpy.data.objects if o not in keep]
  mesh=[o for o in old if o.type=='MESH']
  camset(mesh);render(out/'before'/(n+'.png'))
  for o in old:bpy.data.objects.remove(o,do_unlink=True)
  new=author()
  # A/B framing must fit both different dimensions, same chosen camera with as-close-as-possible scale
  camset(new);render(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in new:ob.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  p=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(p),export_format='GLB',use_selection=True,export_apply=True)
  row.update({'technical':'COMPLETE_VISUAL_PENDING','objects':len(new),'bytes':p.stat().st_size,
     'old_sha':sha(original),'new_sha':sha(p),'note':'Independent authored 3D module, not a composed fortress'})
  print('NEW_MODULE_GEOMETRY',n,len(new),flush=True)
 except Exception as e:
  row.update({'technical':'FAIL','error':repr(e)});traceback.print_exc()
 finally:
  result.append(row)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  for im in list(bpy.data.images):
   if im.users==0 and im.name not in ('Render Result',):bpy.data.images.remove(im)
  (out/'reports'/'steps_bridge_individual.json').write_text(json.dumps({'status':'VISUAL_GATE_PENDING','items':result,'no_scene_composition':True},indent=2))
if any(x['technical']!='COMPLETE_VISUAL_PENDING' for x in result):raise RuntimeError('Module incomplete')
print('TWO_MODULAR_ARCHITECTURE_ASSETS_DONE',flush=True)
