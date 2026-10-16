"""Two authentic Quaternius CC0 individual grass tufts with extra physically
CURVED blade ribbons and lighter varied Alpine olive foliage.
Rebuild from healthy sparse source, NOT black rigid failed V5.
Fixed camera Blender A/B, preserve original texture and reusable GLB origins.
"""
import bpy,sys,math,json,hashlib,random,traceback
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,arg)
names=['grass_tuft_1','grass_tuft_2']
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=980;s.render.resolution_y=980
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Alpine individual grass lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
def light(n,power,where,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.size=9;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=where
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('warm meadow light',1800,(-7,-8,12),(1,.89,.7))
light('sky cool diffuse',1520,(8,3,11),(.73,.84,1))
light('rim',850,(-2,9,10),(.95,.97,.91))
cd=bpy.data.cameras.new('locked real CC0 grass comparison')
cam=bpy.data.objects.new('locked real CC0 grass comparison',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bbox(models):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in models if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera(meshes):
 lo,hi=bbox(meshes);center=(lo+hi)*.5;d=hi-lo
 cam.location=center+Vector((.86,-1.55,1.19)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.2)*1.88
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def leaf_mat(name,color):
 m=bpy.data.materials.new(name);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(*color,1)
 bs.inputs['Roughness'].default_value=.94
 return m
mats=[
 leaf_mat('Valoria_living_grass_sage',(.25,.36,.12)),
 leaf_mat('Valoria_living_grass_moss',(.18,.29,.09)),
 leaf_mat('Valoria_living_grass_sunlit',(.35,.43,.17)),
 leaf_mat('Valoria_living_grass_shadow',(.13,.24,.10)),
]
rows=[]
for idx,n in enumerate(names):
 pre=set(bpy.data.objects);r={'asset':n,'art_approved':False,'target':6}
 try:
  path=source/(n+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(path))
  old=[ob for ob in bpy.data.objects if ob not in pre]
  mesh=[o for o in old if o.type=='MESH']
  if not mesh:raise RuntimeError('Missing original Quaternius CC0 blades')
  camera(mesh);shoot(out/'before'/(n+'.png'))
  lo,hi=bbox(mesh);center=(lo+hi)*.5;d=hi-lo
  rng=random.Random(543+idx*18)
  blades=46 if idx==0 else 55
  vertices=[];faces=[];material_idx=[]
  for k in range(blades):
   angle=rng.uniform(0,2*math.pi)
   rbase=rng.uniform(0,min(float(d.x),float(d.y))*.28)
   x=float(center.x)+math.cos(angle)*rbase
   y=float(center.y)+math.sin(angle)*rbase
   z=float(lo.z)
   length=float(d.z)*rng.uniform(.55,.98)
   tilt=rng.uniform(.20,.48)*length
   w=rng.uniform(.014,.031)
   direction=angle+rng.uniform(-.6,.6)
   origin=len(vertices)
   segments=6
   for t in range(segments+1):
    u=t/segments
    tip=tilt*(u**1.6)
    dx=math.cos(direction)*tip;dy=math.sin(direction)*tip
    ww=w*max(.04,1-u**1.35)
    vertices.extend([(x+dx-ww*math.sin(direction),y+dy+ww*math.cos(direction),z+length*u),
                     (x+dx+ww*math.sin(direction),y+dy-ww*math.cos(direction),z+length*u)])
   for t in range(segments):
    a=origin+2*t
    faces.append((a,a+1,a+3,a+2))
    material_idx.append(rng.randrange(len(mats)) if t==0 else material_idx[-1])
  m=bpy.data.meshes.new('real_bending_leaf_ribbons')
  m.from_pydata(vertices,[],faces);m.update()
  o=bpy.data.objects.new('curved_living_grass_leaves_'+str(blades),m)
  bpy.context.collection.objects.link(o)
  for mat in mats:m.materials.append(mat)
  for face,mi in zip(m.polygons,material_idx):face.material_index=mi
  shoot(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in old+[o]:ob.select_set(True)
  bpy.context.view_layer.objects.active=mesh[0]
  dest=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<40000:raise RuntimeError('CC0 leaves lost on export')
  r.update({'technical':'PASS_PENDING_ART','physical_curved_blades':blades,
    'source_sha':sha(path),'candidate_sha':sha(dest),
    'original_model_preserved':True,'material_palettes':len(mats),
    'source':'Quaternius CC0 sparse individual grass donor'})
  print('VALORIA_CURVED_REAL_GRASS_FLOOR6',n,blades,flush=True)
 except Exception as ex:
  r.update({'technical':'FAILED','error':repr(ex)});traceback.print_exc()
 finally:
  rows.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'two_curved_living_grass_tufts.json').write_text(json.dumps({
   'source_run':38819524063,'rejected_dark_needle_trial':38873081731,
   'both_art_approved':False,'no_scene':True,'items':rows},indent=2))
if len(rows)!=2 or any(r['technical']!='PASS_PENDING_ART' for r in rows):
 raise RuntimeError('Two genuinely curved grass tuft models incomplete')
print('TWO_ALPINE_GRASS_TUFTS_COMPLETED_TECHNICALLY',flush=True)
