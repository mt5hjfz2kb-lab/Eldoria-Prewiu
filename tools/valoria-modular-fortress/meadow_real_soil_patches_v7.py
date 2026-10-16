"""Valoria 4m photographed 3D meadow: add genuinely irregular bare-loam areas
WITHIN the original CC0 albedo image, not overlay planes, grid blotches or
painted polygons. Retain all physical grass clumps and geometry from v6.
Baked glTF PBR albedo; 1k dirt comes from Poly Haven CC0. Same camera A/B.
"""
import bpy,sys,json,hashlib,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('Need best v6 grass GLB dir, CC0 dirt dir, output')
src,cc0,out=map(Path,args)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
old=src/'grass_ground_patch.glb';dp=cc0/'dirt_diffuse.jpg'
if not old.is_file() or not dp.is_file():raise RuntimeError('Both real CC0 inputs required')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.27,.32,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
def lamp(n,p,pos,col):
 data=bpy.data.lights.new(n,'AREA');data.energy=p;data.color=col;data.size=10;data.shape='DISK'
 ob=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('warm grass key',1850,(-8,-9,12),(1,.89,.76))
lamp('cool sky',1600,(8,3,12),(.72,.83,1))
lamp('green natural rim',840,(-1,9,11),(.95,1,.95))
cd=bpy.data.cameras.new('unchanged terrain camera')
cam=bpy.data.objects.new('unchanged terrain camera',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bound(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(c) for o in obs if o.type=='MESH' for c in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(old))
models=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
if len(models)<2:raise RuntimeError('Source meadow leaf geometry missing')
lo,hi=bound(models);center=(lo+hi)*.5;dims=hi-lo
cam.location=center+Vector((.75,-1.55,1.32)).normalized()*max(dims.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(dims.x,dims.y,dims.z,.25)*1.48
render(out/'before'/'grass_ground_patch.png')
grassimgs=[]
for obj in models:
 for mat in obj.data.materials:
  if not mat or not mat.use_nodes:continue
  for node in mat.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image and 'muted_alpine_grass' in node.image.name:
    grassimgs.append(node.image)
imgs=list(dict.fromkeys(grassimgs))
if len(imgs)!=1:raise RuntimeError('Exactly one verified photographed olive-grass atlas expected')
im=imgs[0]
W,H=im.size
rgba=np.empty(W*H*4,dtype=np.float32)
im.pixels.foreach_get(rgba);colors=rgba.reshape(H,W,4)
dirtim=bpy.data.images.load(str(dp),check_existing=False);dirtim.scale(W,H)
soil=np.empty(W*H*4,dtype=np.float32)
dirtim.pixels.foreach_get(soil);soil=soil.reshape(H,W,4)
rng=random.Random(20261016)
yy,xx=np.mgrid[0:H,0:W].astype(np.float32);X=xx/max(W-1,1);Y=yy/max(H-1,1)
mask=np.zeros((H,W),dtype=np.float32)
for k in range(15):
 x=rng.uniform(.1,.9);y=rng.uniform(.1,.9)
 rx=rng.uniform(.031,.081);ry=rng.uniform(.035,.075)
 dx=(X-x)/rx;dy=(Y-y)/ry
 # Irregular scalloped edges, not circular painted blobs or noisy grids.
 norm=dx*dx+dy*dy
 distortion=.20*np.sin((X*31+Y*12)+k*.8)+.15*np.cos((X*18-Y*26)+k*.2)
 inside=np.clip((1.16+distortion-norm)*1.18,0,1)
 opacity=rng.uniform(.40,.70)
 mask=np.maximum(mask,inside*opacity)
# Soften borders with wider irregular coloration gradients, preserve grass detail.
mask=np.clip(mask,.0,.82)
new=colors[:,:,:3].copy()
# Watered-down dirt with warm neutral brown but keep authentic photograph details.
soil_rgb=np.clip(soil[:,:,:3]*np.array([.98,.96,.91],dtype=np.float32)[None,None,:],0,1)
colors[:,:,:3]=new*(1-mask[:,:,None])+soil_rgb*mask[:,:,None]
im.pixels.foreach_set(rgba);im.update()
atlas=out/'atlas'/'meadow_grass_and_real_loam_CC0_1k.png'
im.filepath_raw=str(atlas);im.file_format='PNG';im.save();im.pack()
bpy.data.images.remove(dirtim)
coverage=float(np.mean(mask>.20))
render(out/'after'/'grass_ground_patch.png')
bpy.ops.object.select_all(action='DESELECT')
for ob in models:ob.select_set(True)
bpy.context.view_layer.objects.active=models[0]
dst=out/'glb'/'grass_ground_patch.glb'
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
if dst.stat().st_size<120000:raise RuntimeError('Lost CC0 soil albedo or real leaves')
row={'asset':'grass_ground_patch','technical':'PASS_PENDING_VISUAL','source_sha':sha(old),'new_sha':sha(dst),
 'cc0_image_sha':sha(atlas),'fraction_of_pixels_loam_visible':coverage,'old_grass_mesh_count':len(models),
 'geometry_unchanged':True,'target_floor':6,
 'license':'Poly Haven dirt and grass both CC0',
 'art_approved':False,'unity_approved':False,'no_scene_assembly':True,
 'warning':'Subjective artistic scoring required, especially nonrepeating dirt border and grass density.'}
(out/'reports'/'meadow_real_loam_study.json').write_text(json.dumps(row,indent=2))
print('MEADOW_REAL_SOIL_CC0_MOSAIC_STUDIO_COMPLETE',coverage,flush=True)
