"""Lift individually under-6 dirt tile via same proven CC0+geometry method as
improved green Valoria meadow V7. Keep real dirt normal/UV, add 9% living ground
patches and nonuniform 3D meadow sprouts/grit, same-camera A/B. NO scene.
"""
import bpy,sys,math,random,json,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(a)!=3:raise RuntimeError('Need dirt GLB folder, photographed grass V7 GLB folder, output')
src,grass_source,out=map(Path,a)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
old=src/'dirt_ground_patch.glb';grass=grass_source/'grass_ground_patch.glb'
if not old.is_file() or not grass.is_file():raise RuntimeError('Verified CC0 originals required')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1100;s.render.resolution_y=1000;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Dusk rough earth studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.27,.33,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
def light(n,p,loc,c):
 d=bpy.data.lights.new(n,'AREA');d.energy=p;d.color=c;d.size=10;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=loc
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('low warm sun',1860,(-8,-9,12),(1,.86,.71))
light('blue sky',1550,(8,3,12),(.72,.83,1))
light('edge',850,(-1,9,11),(.95,1,.93))
camd=bpy.data.cameras.new('locked ground A B lens');cam=bpy.data.objects.new('locked ground A B lens',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(objs):
 bpy.context.view_layer.update()
 ps=[o.matrix_world@Vector(q) for o in objs if o.type=='MESH' for q in o.bound_box]
 return Vector([min(p[i] for p in ps) for i in range(3)]),Vector([max(p[i] for p in ps) for i in range(3)])
def sha(f):return hashlib.sha256(f.read_bytes()).hexdigest()
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(old))
model=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
lo,hi=bounds(model);center=(lo+hi)/2;size=hi-lo
cam.location=center+Vector((.73,-1.52,1.23)).normalized()*max(size.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camd.ortho_scale=max(size.x,size.y,size.z,.25)*1.55
render(out/'before'/'dirt_ground_patch.png')
dirt_imgs={}
for obj in model:
 for mat in obj.data.materials:
  if mat and mat.use_nodes:
   for node in mat.node_tree.nodes:
    if node.type=='TEX_IMAGE' and node.image and 'dirt_diffuse' in node.image.name.lower():dirt_imgs[node.image.name]=node.image
if len(dirt_imgs)!=1:raise RuntimeError('Missing exact original photographed dirt source')
photo=next(iter(dirt_imgs.values()))
W,H=photo.size
ground=np.empty(W*H*4,dtype=np.float32);photo.pixels.foreach_get(ground)
surface=ground.reshape(H,W,4)
# temporarily import the other 4m independent meadow, strictly as color reference
prior=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(grass))
meadow_objs=[o for o in bpy.data.objects if o not in prior]
green_imgs=[i for i in bpy.data.images if 'meadow_grass_and_real_loam' in i.name.lower()]
if len(green_imgs)!=1:raise RuntimeError('Other real CC0 meadow albedo not found')
g=green_imgs[0];g.scale(W,H)
green=np.empty(W*H*4,dtype=np.float32);g.pixels.foreach_get(green)
for o in meadow_objs:bpy.data.objects.remove(o,do_unlink=True)
rng=random.Random(8861)
yy,xx=np.mgrid[0:H,0:W].astype(np.float32);x=xx/max(W-1,1);y=yy/max(H-1,1)
mask=np.zeros((H,W),dtype=np.float32)
# 14 organic shrinking green islands, not repeated spots/grid.
for k in range(14):
 cx=rng.uniform(.10,.9);cy=rng.uniform(.12,.88)
 rx=rng.uniform(.026,.068);ry=rng.uniform(.027,.069)
 dist=((x-cx)/rx)**2+((y-cy)/ry)**2
 edged=.18*np.sin(x*37+y*17+k)+.13*np.cos(x*13-y*31+k*2)
 mask=np.maximum(mask,np.clip(1.25+edged-dist,0,1)*rng.uniform(.37,.64))
mask=np.clip(mask,0,.7)
rgb=surface[:,:,:3]
other=green.reshape(H,W,4)[:,:,:3]
rgb[:]=np.clip(rgb*(1-mask[:,:,None])+other*mask[:,:,None],.02,.91)
photo.pixels.foreach_set(ground);photo.update()
atlas=out/'atlas'/'dirt_and_sparse_olive_grass_CC0_1k.png'
photo.filepath_raw=str(atlas);photo.file_format='PNG';photo.save();photo.pack()
# Grass sprouts: subtle, asymmetrical, photographic background stays dominant.
blades=[];faces=[];materials=[]
palette=[(.13,.24,.08),(.24,.3,.13),(.14,.20,.08),(.29,.27,.14)]
mats=[]
for col in palette:
 m=bpy.data.materials.new('actual_earth_sprout_olive_'+str(len(mats)));m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(*col,1);bs.inputs['Roughness'].default_value=.93
 mats.append(m)
nclusters=115
for k in range(nclusters):
 x0=float(lo.x)+rng.uniform(.06,.94)*float(size.x)
 y0=float(lo.y)+rng.uniform(.06,.94)*float(size.y)
 root=float(lo.z)+.009
 for l in range(rng.randint(2,6)):
  angle=rng.uniform(0,math.tau)
  lean=rng.uniform(.02,.055)
  h=rng.uniform(.04,.13)
  w=rng.uniform(.006,.011)
  st=len(blades)
  dx=math.cos(angle);dy=math.sin(angle)
  for t in (0,.54,.82,1):
   xx=x0+dx*(lean*t*t)
   yy=y0+dy*(lean*t*t)
   half=w*.5*(1-t*.8)
   blades.extend([(xx-dy*half,yy+dx*half,root+h*t),
                  (xx+dy*half,yy-dx*half,root+h*t)])
  for j in range(3):
   idx=st+2*j;faces.append((idx,idx+1,idx+3,idx+2))
   materials.append(rng.choice(range(len(mats))))
mesh=bpy.data.meshes.new('living_sprouts_dry_earth')
mesh.from_pydata(blades,[],faces);mesh.update()
for m in mats:mesh.materials.append(m)
for face,mat in zip(mesh.polygons,materials):face.material_index=mat
clumps=bpy.data.objects.new('small_bent_olive_grass_sprouts',mesh)
bpy.context.collection.objects.link(clumps)
models=model+[clumps]
render(out/'after'/'dirt_ground_patch.png')
bpy.ops.object.select_all(action='DESELECT')
for o in models:o.select_set(True)
bpy.context.view_layer.objects.active=model[0]
dst=out/'glb'/'dirt_ground_patch.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<85000:raise RuntimeError('Missing rich PBR dirt geometry')
r={'asset':'dirt_ground_patch','tech':'PASS_PENDING_VISUAL','original_sha':sha(old),'new_sha':sha(dst),
   'photo_sha':sha(atlas),'new_3d_grass_faces':len(mesh.polygons),
   'green_areas_fraction':float((mask>.20).mean()),
   'reference':'Valoria natural bare earth with olive-green individual tufts',
   'licensed_sources':'Poly Haven CC0 original real dirt plus improved meadow',
   'human_art_grade_pending':True,'art_approved':False,'unity_approved':False,'no_scene':True}
(out/'reports'/'dry_earth_real_grass_quality.json').write_text(json.dumps(r,indent=2))
print('REAL_EARTH_CC0_WITH_LIVING_GRASS',len(mesh.polygons),flush=True)
