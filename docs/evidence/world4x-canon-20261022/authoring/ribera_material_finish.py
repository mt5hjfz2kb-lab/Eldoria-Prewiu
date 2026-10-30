"""G: retained ribera, UV material finish from existing project surfaces, original pines."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='f335d73479614a1b8c579b7a5e4fb6eed9077b7e5ea415dfcccb65c15b7f14a0'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
# Recover inherited foliage, retaining the archived sculpt variants for inspection.
original=bpy.data.objects['Inherited pine before crown sculpt'].data
for o in scene.objects:
 if o.name.startswith('Pilot_Pine'):o.data=original
S=2048
Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
def pixels(name):
 im=bpy.data.images[name];w,h=im.size;a=np.array(im.pixels[:],np.float32).reshape(h,w,4);ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);return a[iy[:,None],ix[None,:]]
def mask(name):return pixels(name)[:,:,0]
# Reuse D's bounded brushwork and path/river masks. The rejected broad E/F pattern is not composited.
base=pixels('Ribera_pictorial_ground');rgb=base[:,:,:3].copy()
root=pathlib.Path(__file__).resolve().parents[4]/'Unity/Assets'
asset=root/'Mega Fantasy Props Pack/Textures/grass_1_fixed_d.png'
assert asset.exists(),asset
im=bpy.data.images.load(str(asset),check_existing=True);w,h=im.size;t=np.array(im.pixels[:],np.float32).reshape(h,w,4)[:,:,:3]
# Stylized material swatch: suppress chromatic photographic detail and bake a soft, limited value range.
lum=t@np.array([.2126,.7152,.0722],np.float32)
for _ in range(6):lum=(4*lum+np.roll(lum,1,0)+np.roll(lum,-1,0)+np.roll(lum,1,1)+np.roll(lum,-1,1))/8
lo,hi=np.percentile(lum,[8,92]);lum=np.clip((lum-lo)/(hi-lo),0,1);lum=.78+.34*lum
# Two world-aligned samples at different scales: material grain, not new biome topology.
u=((X+96)/9*w).astype(int)%w;v=((Y+78)/9*h).astype(int)%h
u2=((X+Y+174)/17*w).astype(int)%w;v2=((Y-X+174)/17*h).astype(int)%h
value=.65*lum[v,u]+.35*lum[v2,u2]
path=mask('07 footpath');bank=mask('04 eroded bank crown');litter=mask('03 pine litter')
strength=(1-.8*path)*(1-.55*bank)
rgb*=1+(value[:,:,None]-1)*strength[:,:,None]
# Readable, localized warm earth clearings around tree groups, with softened irregular edges.
# Keep the D openings and introduce earth only under grouped foliage, not a full-map tessellation.
trees=sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:o.name)
for i,o in enumerate(trees):
 x,y,z=o.location
 if abs(x)>85 or abs(y)>65:continue
 d=((X-x)/(2.5*o.scale.x))**2+((Y-y)/(2.0*o.scale.y))**2
 a=np.clip(1-d,0,1)*.24
 rgb=rgb*(1-a[:,:,None])+np.array([.55,.48,.25])*a[:,:,None]
rgba=np.ones((S,S,4),np.float32);rgba[:,:,:3]=np.clip(rgb,0,1)
def image(name,a):
 h,w=a.shape[:2];im=bpy.data.images.new(name,width=w,height=h);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(a.ravel());assert float(np.max(np.array(im.pixels[:])))>.1;im.filepath_raw=str(out/(name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack();return im
assert np.isfinite(rgba).all() and float(rgb.mean())>.05, ('Invalid material canvas',float(rgb.mean()))
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');n=nodes.new('ShaderNodeTexImage');n.image=image('G stylized grass and earth finish',rgba);soil.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color']);nodes.active=n
# Sculpt and place inherited rock copies at the foot of existing forest clusters, not gameplay nodes.
rocks=sorted([o for o in scene.objects if o.name.startswith('Pilot_Rock')],key=lambda o:o.name)
coll=bpy.data.collections.new('08_Ribera_environment_finish');scene.collection.children.link(coll)
terrains=[bpy.data.objects['Pilot_Terrain_'+str(i)] for i in (-1,1)]
from mathutils import Vector
from mathutils.bvhtree import BVHTree
bvh=[BVHTree.FromObject(o,bpy.context.evaluated_depsgraph_get()) for o in terrains]
def ground(x,y):
 hits=[b.ray_cast(Vector((x,y,50)),Vector((0,0,-1)))[0] for b in bvh];return next((p.z for p in hits if p is not None),None)
count=0
for i,o in enumerate(trees):
 if i%3!=0 or abs(o.location.x)>82 or abs(o.location.y)>62:continue
 for j,(dx,dy,scale) in enumerate([(-2.2,-1.1,.28),(1.4,-2.4,.17),(-.6,2.4,.12)]):
  x=o.location.x+dx*o.scale.x;y=o.location.y+dy*o.scale.y;z=ground(x,y)
  if z is None:continue
  rock=bpy.data.objects.new('Bank forest stone %03d'%count,rocks[(i+j)%len(rocks)].data);coll.objects.link(rock);rock.location=(x,y,z+.08);rock.scale=(scale*1.25,scale*.8,scale);rock.rotation_euler[2]=i*.73+j;count+=1
# Save complete editable lineage; export only visible candidate geometry.
scene['finishing_stage']='G D foliage and ground composition with stylized existing texture finish; E water retained'
scene['G_existing_texture']=str(asset.relative_to(root));scene['G_source_hash']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'G','source_F_sha256':scene['G_source_hash'],'rejected':'E/F broad camouflage and F blobby crowns','retained':'D composition and foliage; E current/depth water and bank sculpture','surface_method':'existing grass material filtered and palette constrained over D painted UV geography','environment_stones':count,'art_approved':False},indent=2))
