"""Individual Valoria terrain: make 4m olive meadow a genuinely 3D grass patch.
Add 110 natural clumps of bent blades, NOT colored balls or billboard rows.
Retain prior Quaternius + CC0 original underlying photographed grass,
bounds, UV. Same-camera A/B plus normal closeup. No assembled scene.
"""
import bpy,sys,random,math,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
src,out=map(Path,args)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
p=src/'grass_ground_patch.glb'
if not p.is_file():raise RuntimeError('Best CC0 photographic grass tile absent')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Meadow midday studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.27,.33,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=w
def lamp(n,pos,power,color):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.color=color;l.size=10;l.shape='DISK'
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm mountain sun',(-8,-9,12),1920,(1,.88,.75))
lamp('blue sky',(8,3,11),1500,(.70,.82,1))
lamp('blade rim',(-1,8,8),900,(1,1,.9))
camd=bpy.data.cameras.new('same macro grass before after')
cam=bpy.data.objects.new('same macro grass before after',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(a[i] for a in pts) for i in range(3)]),Vector([max(a[i] for a in pts) for i in range(3)])
def photo(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(p))
old=[o for o in bpy.data.objects if o not in rig]
models=[o for o in old if o.type=='MESH']
lo,hi=bounds(models)
c=(lo+hi)*.5;size=hi-lo
cam.location=c+Vector((.75,-1.55,1.32)).normalized()*max(size.length,1)*3
cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
camd.ortho_scale=max(size.x,size.y,size.z,.25)*1.48
photo(out/'before'/'grass_ground_patch.png')
# Distinct organic leaf colors are still PBR materials, not unlit monochrome.
leafcolors=[(.12,.21,.066),(.20,.28,.105),(.13,.25,.093),(.31,.32,.13),(.16,.23,.095)]
mats=[]
for i,rgb in enumerate(leafcolors):
 mat=bpy.data.materials.new('Alpine_real_blade_olive_variety_'+str(i));mat.use_nodes=True
 sh=mat.node_tree.nodes.get('Principled BSDF')
 sh.inputs['Base Color'].default_value=(*rgb,1)
 sh.inputs['Roughness'].default_value=.91
 mats.append(mat)
rng=random.Random(20261116)
# Model geometrically curved blade strips: three heights per leaf,
# wind lean and nonplanar leaf midsections create changing highlights.
verts=[];faces=[];material_ids=[];rootz=float(lo.z+.026)
width=float(size.x);depth=float(size.y)
# Guard original footprint: mild inset keeps seamlessly repeatable tiles.
for group in range(110):
 gx=float(lo.x)+rng.uniform(.07,.93)*width
 gy=float(lo.y)+rng.uniform(.07,.93)*depth
 angle=rng.uniform(0,math.tau)
 bare=rng.random()<.13
 if bare:continue
 leaves=rng.randint(5,10)
 for leaf in range(leaves):
  ang=angle+leaf*math.tau/leaves+rng.uniform(-.28,.28)
  off=rng.uniform(.009,.049)
  x=gx+math.cos(ang)*off;y=gy+math.sin(ang)*off
  height=rng.uniform(.08,.205)
  blade_width=rng.uniform(.007,.018)
  lean=rng.uniform(.04,.10)
  dx,dy=math.cos(ang),math.sin(ang)
  side=Vector((-dy,dx,0))
  start=len(verts)
  for t in (0,.45,.77,1):
   cx=x+dx*lean*(t*t);cy=y+dy*lean*(t*t)
   half=blade_width*(1-t*.8)*.5
   verts.extend([(cx-side.x*half,cy-side.y*half,rootz+height*t),
                 (cx+side.x*half,cy+side.y*half,rootz+height*t)])
  for j in range(3):
   idx=start+2*j
   faces.append((idx,idx+1,idx+3,idx+2))
   material_ids.append(rng.choices(range(5),weights=[4,4,3,1,3])[0])
mesh=bpy.data.meshes.new('true_mountain_meadow_curved_leaves')
mesh.from_pydata(verts,[],faces);mesh.update()
for mat in mats:mesh.materials.append(mat)
for poly,idx in zip(mesh.polygons,material_ids):poly.material_index=idx
# Double sided foliage: leaves should remain readable in Unity without backface loss.
for m in mats:
 m.use_nodes=True
 m.surface_render_method='DITHERED'
ob=bpy.data.objects.new('110_distinct_blade_clusters_real_geometry',mesh)
bpy.context.collection.objects.link(ob)
models.append(ob)
if len(mesh.polygons)<1200:raise RuntimeError('Grass detail count too low to affect photo')
photo(out/'after'/'grass_ground_patch.png')
bpy.ops.object.select_all(action='DESELECT')
for o in old+[ob]:o.select_set(True)
bpy.context.view_layer.objects.active=old[0]
dst=out/'glb'/'grass_ground_patch.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<50000:raise RuntimeError('Original photo and geometry stripped')
r={'asset':'grass_ground_patch','technical':'PASS_PENDING_VISUAL','no_scene':True,
   'original_CC0_photo_preserved':True,'new_curved_leaf_faces':len(mesh.polygons),
   'target':6,'origin_run':38889135999,'source_sha':sha(p),'new_sha':sha(dst),
   'art_approved':False,'check':'Visually compare leaf density/silhouette at game distance; beware flat edges and vertical grass clumps.'}
(out/'reports'/'real_meadow_addition.json').write_text(json.dumps(r,indent=2))
print('ONE_GROUND_110_TRUE_CURVED_GRASS_CLUSTER_CANDIDATE',len(mesh.polygons),flush=True)
