"""Valoria V10 isolated WATER MATERIAL study. Physically transparent, actual
stochastic ripple mesh from V9, glTF transmission/IOR, neutral studio bottom
ONLY to demonstrate refraction. The riverbed is not exported. Paired same
lens/source A-B, no Unity scene or art auto-promotion.
"""
import bpy,sys,json,hashlib
from pathlib import Path
from mathutils import Vector
import numpy as np
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError('Usage: <v9-water-glb-dir> <result-dir>')
src,out=map(Path,args)
for d in ('glb','before','after','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
old=src/'river_water_tile.glb'
if not old.is_file():raise RuntimeError('Require accepted stochastic V9 water geometry')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1100;s.render.resolution_y=1020
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria open sky water lookdev')
world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.30,.36,.43,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.9;s.world=world
def area(name,pos,power,color,size):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.size=size;l.shape='DISK'
 o=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('large cool sky reflection',(-6,-6,13),3400,(.73,.86,1),6)
area('warm shoreline',(8,3,11),1800,(1,.89,.68),6)
area('white oblique highlight',(-2,9,8),1250,(1,1,1),3)
data=bpy.data.cameras.new('fixed water 3d quality camera')
cam=bpy.data.objects.new('fixed water 3d quality camera',data)
bpy.context.collection.objects.link(cam);s.camera=cam;data.type='ORTHO'
rig=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(old))
mesh=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
if len(mesh)!=1:raise RuntimeError('Not one V9 repeatable water mesh')
obj=mesh[0]
pts=[obj.matrix_world@Vector(p) for p in obj.bound_box]
lo=Vector([min(p[i] for p in pts) for i in range(3)])
hi=Vector([max(p[i] for p in pts) for i in range(3)])
cen=(lo+hi)*.5;dim=hi-lo
cam.location=cen+Vector((.7,-1.3,1.45)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
data.ortho_scale=max(dim.x,dim.y,.1)*1.62
# Temporary controlled matte river floor, not part of the actual asset.
bpy.ops.mesh.primitive_plane_add(size=1,location=(cen.x,cen.y,float(lo.z)-.17))
floor=bpy.context.object;floor.name='STUDIO_RIVERBED_EXCLUDED_FROM_GLB'
floor.dimensions=(float(dim.x)*1.06,float(dim.y)*1.06,0)
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bed=bpy.data.materials.new('Test-only dark underwater gravel')
bed.use_nodes=True
bs=bed.node_tree.nodes.get('Principled BSDF')
bs.inputs['Base Color'].default_value=(.135,.125,.116,1)
bs.inputs['Roughness'].default_value=.96
noise=bpy.data.textures.new('studio rocky bottom nonexported','CLOUDS')
noise.noise_scale=.2
mod=floor.modifiers.new('studio slight geology roughness','DISPLACE')
mod.texture=noise;mod.strength=.005
floor.data.materials.append(bed)
rig.add(floor)
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
render(out/'before'/'river_water_tile.png')
srcmat=obj.data.materials[0]
if not srcmat or not srcmat.use_nodes:raise RuntimeError('PBR water material not found')
texes=[n for n in srcmat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
colors=[n for n in texes if n.image.colorspace_settings.name!='Non-Color']
norms=[n for n in texes if n.image.colorspace_settings.name=='Non-Color']
if not colors or not norms:raise RuntimeError('Baked stochastic albedo and normal absent')
sourceim=colors[0].image
W,H=sourceim.size
pixels=np.empty(W*H*4,np.float32);sourceim.pixels.foreach_get(pixels)
r=pixels.reshape((H,W,4))
l=r[:,:,0]*.3+r[:,:,1]*.58+r[:,:,2]*.12
level=np.clip((l-float(l.mean()))/(max(float(l.std()),.001)),-2,2)
base=np.array([.155,.285,.34],dtype=np.float32)
r[:,:,:3]=np.clip(base[None,None,:]*(1+.018*level[:,:,None]),.055,.65)
r[:,:,3]=.83
albedo=bpy.data.images.new('Valoria physically clear river PBR',W,H,alpha=True)
albedo.pixels.foreach_set(r.reshape(-1));albedo.update()
atlas=out/'atlas'/'valoria_water_blue_clear_CC0_compatible.png'
albedo.filepath_raw=str(atlas);albedo.file_format='PNG';albedo.save();albedo.pack()
mat=bpy.data.materials.new('VALORIA_water_transmission_true_IOR_1_333')
mat.use_nodes=True;mat.surface_render_method='DITHERED'
nd=mat.node_tree.nodes;nd.clear();links=mat.node_tree.links
col=nd.new('ShaderNodeTexImage');col.image=albedo
normtex=nd.new('ShaderNodeTexImage');normtex.image=norms[0].image
bump=nd.new('ShaderNodeNormalMap');bump.inputs['Strength'].default_value=.20
bs=nd.new('ShaderNodeBsdfPrincipled')
bs.inputs['Roughness'].default_value=.17
bs.inputs['Metallic'].default_value=0
bs.inputs['IOR'].default_value=1.333
bs.inputs['Transmission Weight'].default_value=.70
bs.inputs['Alpha'].default_value=.83
links.new(col.outputs['Color'],bs.inputs['Base Color'])
links.new(col.outputs['Alpha'],bs.inputs['Alpha'])
links.new(normtex.outputs['Color'],bump.inputs['Color'])
links.new(bump.outputs['Normal'],bs.inputs['Normal'])
output=nd.new('ShaderNodeOutputMaterial')
links.new(bs.outputs['BSDF'],output.inputs['Surface'])
obj.data.materials.clear();obj.data.materials.append(mat)
render(out/'after'/'river_water_tile.png')
bpy.ops.object.select_all(action='DESELECT')
obj.select_set(True);bpy.context.view_layer.objects.active=obj
glb=out/'glb'/'river_water_tile.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<100000:raise RuntimeError('Water GLB missing textures')
report={'asset':'river_water_tile','parent_run':38897442996,
 'technical':'PASS_PENDING_VISUAL','art_approved':False,'unity_approved':False,
 'transmission':.70,'index_refraction':1.333,'alpha':.83,'roughness':.17,
 'texture_sha':sha(atlas),'glb_sha':sha(glb),
 'test_floor_only':True,'glb_excludes_studio_floor':True,
 'warning':'Confirm Khronos glTF transmission and isolated Unity URP material before any 6/10 promotion'}
(out/'reports'/'one_clear_water_v10.json').write_text(json.dumps(report,indent=2))
print('WATER_V10_TRUE_TRANSMISSION_SINGLE_ASSET_PENDING_ART',glb.stat().st_size,flush=True)
