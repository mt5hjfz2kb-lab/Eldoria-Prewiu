"""Targeted ART recovery from visual rejection: real river foam surface, keep pale stone step, longer 3D fire.
Four independent A/B, no assembly; source choices recorded in report.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise SystemExit('source-v3-glbs, previous-six-glbs, result')
base,prev,out=map(Path,args)
names=['river_foam_line','stair_step','torch_wall','brazier_pedestal']
for k in ('glb','before','after','reports'):(out/k).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1020;s.render.resolution_y=1020
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Independent reference review stage');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.76;s.world=w
def lighting(name,pos,power,color):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.size=9;l.shape='DISK'
 ob=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lighting('Warm raking key',(-7,-8,12),1700,(1,.84,.66))
lighting('Cool fill',(7,3,11),1450,(.69,.83,1))
lighting('rim',(2,8,9),950,(1,1,1))
cd=bpy.data.cameras.new('paired neutral');cam=bpy.data.objects.new('paired neutral',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(mesh):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in mesh if ob.type=='MESH' for v in ob.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def view(mesh):
 lo,hi=bounds(mesh);cen=(lo+hi)*.5;dims=hi-lo
 cam.location=cen+Vector((.87,-1.55,1.12)).normalized()*max(dims.length,1)*3.1
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(dims.x,dims.y,dims.z,.35)*1.72
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def mat(name,color,rough=.6,emit=0):
 m=bpy.data.materials.new(name);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=rough
 if emit>0:
  bs.inputs['Emission Color'].default_value=(*color,1);bs.inputs['Emission Strength'].default_value=emit
 return m
white=mat('soft_white_watercrest',(.72,.84,.87),.64)
white_shine=mat('light_yellowish_whitewater',(.93,.94,.89),.46)
core=mat('flame_warm_white_gold',(.98,.68,.13),.4,2.2)
outer=mat('flame_amber_orange',(.97,.24,.018),.4,1.5)
def cube(n,pos,dim,m,bev=0.016):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=n
 o.dimensions=dim;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(m)
 if bev:
  b=o.modifiers.new('genuine worn hewn edges','BEVEL');b.width=min(bev,min(dim)*.2)
  b.segments=2;b.limit_method='ANGLE'
 return o
def swath(lo,hi):
 R=random.Random(1125);out=[]
 x0,x1=float(lo.x),float(hi.x)
 # Continuous ribbon physically models a translucent broken foamy wave front.
 for layer in range(3):
  y_shift=(layer-1)*.13
  verts=[];faces=[]
  for i in range(85):
   u=i/84
   x=x0+(x1-x0)*u
   center=y_shift+.04*math.sin(u*12.8+layer*2)+.025*math.sin(u*27.5+1.0)
   width=.025+.025*abs(math.sin(u*11+layer))+.016*R.random()
   verts.extend([(x,center-width*.5,.02+.004*R.random()),
                 (x,center+width*.5,.02+.004*R.random())])
   if i<84:
    a=2*i;faces.extend([(a,a+1,a+3),(a,a+3,a+2)])
  me=bpy.data.meshes.new('foam_crest_ribbon');me.from_pydata(verts,[],faces);me.update()
  o=bpy.data.objects.new('actual_wavecrest_ribbon_'+str(layer),me)
  bpy.context.collection.objects.link(o);me.materials.append(white_shine if layer==1 else white)
  out.append(o)
 # Separate foam tongues + oxygenated pockets, NOT three white hairs.
 for j in range(17):
  u=R.random();x=x0+(x1-x0)*u
  y=R.gauss(0,.13)
  bpy.ops.mesh.primitive_uv_sphere_add(segments=10,ring_count=5,radius=R.uniform(.022,.055),location=(x,y,.026))
  o=bpy.context.object;o.name='micro_whitewater_pool_'+str(j)
  o.scale=(1.9,.68,.13);o.data.materials.append(white_shine)
  out.append(o)
 return out
def actual_flame(prefix,x,y,z,height,base_radius):
 out=[]
 R=random.Random(870 if prefix=='wall' else 716)
 # Real asymmetrical flame geometry with a four-sided changing contour at every
 # vertical height, not a flat salmon polygon with black dot-like decorations.
 for plume in range(6):
  angle=plume*math.pi*2/6
  cx=x+math.cos(angle)*base_radius*.35
  cy=y+math.sin(angle)*base_radius*.35
  vert=[];faces=[]
  for k in range(8):
   t=k/7
   rad=base_radius*(1-t)**1.15*(.60 if plume%3 else 1.0)
   bend=math.sin(t*6+plume)*base_radius*.30*t
   zz=z+t*height*R.uniform(.70,1.17)
   for j in range(6):
    a=j*2*math.pi/6
    vert.append((cx+math.cos(a)*rad+bend,cy+math.sin(a)*rad,zz))
   if k<7:
    for j in range(6):
     a=k*6+j;b=k*6+(j+1)%6;c=(k+1)*6+j;d=(k+1)*6+(j+1)%6
     faces.extend([(a,b,d),(a,d,c)])
  me=bpy.data.meshes.new(prefix+'_3D_crafted_flame')
  me.from_pydata(vert,[],faces);me.update()
  o=bpy.data.objects.new(prefix+'_real_volumetric_flame_'+str(plume),me)
  bpy.context.collection.objects.link(o);me.materials.append(core if plume>3 else outer)
  out.append(o)
 return out
def step_polish(meshes):
 lo,hi=bounds(meshes);R=random.Random(31)
 pbr=next((m for o in meshes for m in o.data.materials if m),None)
 if not pbr:raise RuntimeError('No original pale CC0 limestone to preserve')
 out=list(meshes)
 front=float(lo.y);z=float(hi.z)
 width=float(hi.x-lo.x)
 out.append(cube('REAL_stone_stair_nosing',
    ((lo.x+hi.x)*.5,front+.034,z-.022),(width-.05,.095,.045),pbr,.018))
 # Deliberately shallow hand-cut chisel grooves at contact faces, not wood strips.
 shadow=mat('deep_worn_mortar_chisel_fissure',(.34,.31,.28),.97)
 for i in range(3):
  x=lo.x+(i+1)*width/4+R.uniform(-.03,.03)
  out.append(cube('single_step_fine_ashlar_joint_'+str(i),(x,front-.0005,z-.054),
      (.008,.008,.067),shadow,.002))
 return out
def flame_upgrade(meshes,name):
 # Keep physically forged wall arm and richly textured old stone pedestal.
 root=[o for o in meshes if 'flame_tongue' not in o.name.lower()]
 removed=[o for o in meshes if o not in root]
 for o in removed:bpy.data.objects.remove(o,do_unlink=True)
 lo,hi=bounds(root)
 x=(lo.x+hi.x)*.5;y=(lo.y+hi.y)*.5
 if name=='torch_wall':z=float(lo.z)+.84;height=.81;radius=.085
 else:z=float(lo.z)+1.19;height=.79;radius=.135
 # center just above iron basket; same overall modular function.
 return root+actual_flame(name,x,y,z,height,radius)
results=[]
for name in names:
 before=set(bpy.data.objects);base_source=base/(name+'.glb') if name in ('river_foam_line','stair_step') else prev/(name+'.glb')
 row={'asset':name,'original_selected_source':str(base_source.name),
      'target_min':6,'art_approved':False,'no_scene':True}
 try:
  bpy.ops.import_scene.gltf(filepath=str(base_source))
  meshes=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
  if not meshes:raise RuntimeError('No source geometry')
  view(meshes);render(out/'before'/(name+'.png'))
  lo,hi=bounds(meshes)
  if name=='river_foam_line':
   for o in meshes:bpy.data.objects.remove(o,do_unlink=True)
   objects=swath(lo,hi)
  elif name=='stair_step':objects=step_polish(meshes)
  else:objects=flame_upgrade(meshes,name)
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in objects:o.select_set(True)
  bpy.context.view_layer.objects.active=objects[0]
  p=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(p),export_format='GLB',use_selection=True,export_apply=True)
  if p.stat().st_size<2500:raise RuntimeError('GLB missing')
  row.update({'technical':'PASS_PENDING_ART','mesh_count':len(objects),
              'source_sha':sha(base_source),'candidate_sha':sha(p)})
  print('ART_DIRECTOR_REDO_WEAK_ASSET',name,len(objects),flush=True)
 except Exception as e:
  row.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  results.append(row)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'four_art_recovery.json').write_text(json.dumps({
   'point':'Correct artistic failure, not merely successful geometry exports',
   'results':results,'not_approved':True},indent=2))
if any(x['technical']!='PASS_PENDING_ART' for x in results):raise RuntimeError('Some art correction failed')
print('FOUR_ART_RECOVERY_CANDIDATES_READY',flush=True)
