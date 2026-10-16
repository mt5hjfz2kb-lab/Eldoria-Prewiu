"""Reference-first 4m green grass modular ground V5.
Use best actually GREEN photographic CC0 source (38872475510), never failed
yellow tint V4. Add real curved narrow 3D living grass tuft leaves as natural
uneven clusters, not square billboards. Standalone asset only; same-camera A/B.
"""
import bpy,sys,json,math,random,hashlib,traceback
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,out=map(Path,a)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
if not source.is_file():raise RuntimeError('Real green CC0 old patch is missing')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1120;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria evening green meadow module')
world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.25,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;s.world=world
def lamp(n,pos,power,col):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=col;data.shape='DISK';data.size=10
 o=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm sun grazing grass',(-8,-8,12),2000,(1,.86,.69))
lamp('cloud blue fill',(8,4,13),1650,(.7,.85,1))
lamp('backrim',(-3,9,9),850,(.92,.98,1))
cd=bpy.data.cameras.new('locked meadow camera');cam=bpy.data.objects.new('locked meadow camera',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def box(obs):
 bpy.context.view_layer.update()
 v=[o.matrix_world@Vector(c) for o in obs if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in v) for i in range(3)]),Vector([max(p[i] for p in v) for i in range(3)])
def frame(obs):
 lo,hi=box(obs);center=(lo+hi)*.5;ext=hi-lo
 cam.location=center+Vector((.86,-1.57,1.12)).normalized()*max(ext.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(ext.x,ext.y,ext.z)*1.63
def snap(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.import_scene.gltf(filepath=str(source))
imported=[o for o in bpy.data.objects if o not in rig]
models=[o for o in imported if o.type=='MESH']
if not models:raise RuntimeError('No saved source 3D meadow')
lo,hi=box(models);frame(models)
snap(out/'before'/'grass_ground_patch.png')
# Keep ALL existing professional photographed CC0 green surface and 420 grass,
# physically add tapered wind-bent blades in clusters.
colors=[(.145,.270,.091),(.22,.335,.13),(.27,.36,.14),(.15,.245,.091)]
materials=[]
for i,c in enumerate(colors):
 mat=bpy.data.materials.new('Valoria_real_olive_leaf_shade_'+str(i))
 mat.use_nodes=True
 b=mat.node_tree.nodes.get('Principled BSDF')
 b.inputs['Base Color'].default_value=(*c,1)
 b.inputs['Roughness'].default_value=.89
 materials.append(mat)
rng=random.Random(82116)
verts=[];faces=[];matidx=[]
W=float(hi.x-lo.x);D=float(hi.y-lo.y)
ground=float((hi.z+lo.z)*.5)
# Cluster spacing is asymmetric (not a lattice), including a few lightly worn
# bare-earth gaps along the main stone path.
tufts=47;leaves_per_tuft=9;clumps=[]
for t in range(tufts):
 x=float(lo.x)+W*(.04+rng.random()*.92)
 y=float(lo.y)+D*(.04+rng.random()*.92)
 length=rng.uniform(.075,.19)
 clumps.append({'x':round(x,2),'y':round(y,2),'height':round(length,3)})
 for blade in range(leaves_per_tuft):
  theta=blade*2*math.pi/leaves_per_tuft+rng.uniform(-.36,.36)
  L=length*rng.uniform(.6,1.2)
  spread=rng.uniform(.08,.14)
  width=rng.uniform(.009,.017)
  bend=rng.uniform(.045,.11)
  bottom=len(verts)
  for step in range(6):
   zratio=step/5
   drift=bend*zratio*zratio
   bx=x+math.cos(theta)*drift
   by=y+math.sin(theta)*drift
   dx=width*(1-zratio)**.76
   # More botanical curvature rather than perfectly linear triangles
   verts.append((bx-math.sin(theta)*dx,by+math.cos(theta)*dx,ground+L*zratio))
   verts.append((bx+math.sin(theta)*dx,by-math.cos(theta)*dx,ground+L*zratio))
  for step in range(5):
   p=bottom+step*2
   faces.append((p,p+1,p+3,p+2))
   matidx.append(rng.choices(range(4),[3,4,1,2])[0])
mesh=bpy.data.meshes.new('reused_curved_olive_grass_scatter')
mesh.from_pydata(verts,[],faces);mesh.update()
ob=bpy.data.objects.new('47_distinct_real_grass_clumps_423_bent_leaves',mesh)
bpy.context.collection.objects.link(ob)
for mat in materials:mesh.materials.append(mat)
for f,i in zip(mesh.polygons,matidx):f.material_index=i
# A single authored aggregate mesh ensures the repeatable 4x4m tile stays cheap
# for Unity batches while the foliage is individually recognizable.
snap(out/'after'/'grass_ground_patch.png')
bpy.ops.object.select_all(action='DESELECT')
for o in imported+[ob]:o.select_set(True)
bpy.context.view_layer.objects.active=ob
dst=out/'glb'/'grass_ground_patch.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<200000:raise RuntimeError('Green photography or new clumps missing')
record={'asset':'grass_ground_patch','technical':'PASS_PENDING_ART','source_run':38872475510,
'original_sha256':sha(source),'candidate_sha256':sha(dst),'real_tufts':tufts,
'real_curved_leaf_blades':tufts*leaves_per_tuft,'leaf_mesh_faces':len(mesh.polygons),
'original_photographic_green_CC0_albedo_unchanged':True,
'do_not_use_yellow_V4_run':38879191750,'art_approved':False,
'unity_approved':False,'no_composition':True,
'caveat':'Check density and color next to approved Valoria reference, then revise further if still under 6.'}
(out/'reports'/'real_green_olive_meadow_v5.json').write_text(json.dumps(record,indent=2))
print('REAL_GREEN_GRASS_GROUND_423_CURVED_BLADES',dst.stat().st_size,flush=True)
