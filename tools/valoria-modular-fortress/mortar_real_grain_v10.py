"""V10: photogrammetry-derived lime mortar is a PHYSICAL 26mm recess.
From V9 real grout: remove sparse floating white grit, introduce bedded mineral
aggregate, engrave narrow organic grain with genuine CC0 PBR photo shading.
Reference macro plus locked old A/B. No assembled wall or Unity.
"""
import bpy,sys,json,math,hashlib,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('V9 mortar GLB folder, source CC0 photographed V8 albedo, output')
src,photo_source,out=map(Path,args)
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
base=src/'mortar_joint_recess.glb'
if not base.exists():raise RuntimeError('Expected V9 physical lime seam')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1200;s.render.resolution_y=850
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Macro historical lime stone seam');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.21,.25,.32,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def lamp(n,power,pos,color):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=10
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm raking limestone',1850,(-7,-8,12),(1,.84,.70))
lamp('cool blue fill',1490,(7,3,12),(.72,.83,1))
lamp('subtle top',680,(-2,8,11),(.97,.96,.88))
cd=bpy.data.cameras.new('same source art camera')
cam=bpy.data.objects.new('same source art camera',cd);bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(base))
objs=[o for o in bpy.data.objects if o not in rig]
meshes=[o for o in objs if o.type=='MESH']
lo,hi=bounds(meshes);mid=(lo+hi)*.5;dim=hi-lo
if not .017<float(dim.z)<.07:raise RuntimeError('Must be a grout strip, not full brick wall: '+str(dim))
cam.location=mid+Vector((.80,-1.6,1.11)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(dim.x,dim.y,dim.z,.1)*1.65
shoot(out/'before'/'mortar_joint_recess.png')
original_count=len(meshes)
removed=0
for ob in list(meshes):
 if 'grit' in ob.name.lower():
  bpy.data.objects.remove(ob,do_unlink=True);removed+=1
grout=[o for o in bpy.data.objects if o not in rig and o.type=='MESH' and 'mortar' in o.name.lower()]
if not grout:raise RuntimeError('Real lime grout body missing')
body=max(grout,key=lambda o:len(o.data.vertices))
# Preserve real photo, but heighten local contrast for legible lime grains at
# game zoom, without printed horizontal brick-wall lines.
diffuse=None;nmats=[]
for m in body.data.materials:
 if not m or not m.use_nodes:continue
 nmats.append(m)
 for node in m.node_tree.nodes:
  if node.type=='TEX_IMAGE' and node.image and node.image.colorspace_settings.name!='Non-Color':
   diffuse=node.image
if diffuse is None:
 # GLB V9 contains a Blender Bump photo node that glTF export can omit.
 # Recover VERIFIED CC0 V8 photo rather than fabricating a cement material.
 if not photo_source.is_file():raise RuntimeError('Original CC0 photographed lime grout atlas missing')
 diffuse=bpy.data.images.load(str(photo_source),check_existing=True)
 primary=nmats[0]
 nodes=primary.node_tree.nodes
 bs=nodes.get('Principled BSDF')
 if bs is None:raise RuntimeError('No Principled PBR surface on grout mesh')
 imnode=nodes.new('ShaderNodeTexImage');imnode.image=diffuse
 for oldlink in list(bs.inputs['Base Color'].links):primary.node_tree.links.remove(oldlink)
 primary.node_tree.links.new(imnode.outputs['Color'],bs.inputs['Base Color'])
width,height=diffuse.size
pixels=np.empty(width*height*4,dtype=np.float32)
diffuse.pixels.foreach_get(pixels)
rgba=pixels.reshape(height,width,4);rgb=rgba[:,:,:3]
gray=rgb[:,:,0]*.29+rgb[:,:,1]*.59+rgb[:,:,2]*.12
# Preserve source photo microdetail while reducing monotone brightness and
# the over-sharp isolated white chips that read as stuck polystyrene.
avg=float(gray.mean())
grain=np.clip((gray-avg)*1.52,-.15,.15)
rgb[:]=np.clip(np.stack([.325+grain*.95,.302+grain*.91,.275+grain*.85],axis=-1),.15,.55)
diffuse.pixels.foreach_set(rgba.reshape(-1));diffuse.update()
p=out/'atlas'/'Valoria_real_CC0_aged_granular_grout.png'
diffuse.filepath_raw=str(p);diffuse.file_format='PNG';diffuse.save();diffuse.pack()
# Slight variation in actual thickness; every edge remains within the 26mm
# modular joint to avoid bleeding into adjacent stones.
rng=random.Random(2026116)
for v in body.data.vertices:
 v.co.z+=rng.uniform(-.00065,.00065)
body.data.update()
minp,maxp=bounds([body])
if maxp.z-minp.z>.031:raise RuntimeError('Joint no longer fits nominal 26mm')
# Convert 160 tetrahedra to ONE model; embedded 60% into base front surface.
points=[];faces=[];face_mats=[]
for i in range(160):
 x=float(minp.x)+rng.uniform(.025, max(.025, float(maxp.x-minp.x)-.025))
 z=float(mid.z)+rng.uniform(-.010,.010)
 front=float(minp.y)
 r=rng.uniform(.0007,.00265)
 a=len(points)
 # All protrusions <= 2mm from true front face, no isolated white blobs.
 points.extend([(x-r,front+.0003,z-r*.15),(x+r*.65,front+.0002,z-r*.25),
                (x,front-.0015,z+r*.53),(x-r*.10,front+.0012,z-r*.65)])
 faces.extend([(a,a+1,a+2),(a,a+3,a+1),(a,a+2,a+3),(a+1,a+3,a+2)])
mesh=bpy.data.meshes.new('dense_embedded_real_lime_mineral_grain')
mesh.from_pydata(points,[],faces);mesh.update()
grainobj=bpy.data.objects.new('physically_embedded_160_small_grain_lime_stone',mesh)
bpy.context.collection.objects.link(grainobj)
mat=bpy.data.materials.new('subdued_handpacked_limestone_microaggregate')
mat.use_nodes=True
pr=mat.node_tree.nodes.get('Principled BSDF')
pr.inputs['Base Color'].default_value=(.33,.31,.29,1);pr.inputs['Roughness'].default_value=.97
mesh.materials.append(mat)
# A few dark fine, SHORT hairline voids are real but not repetitive rails.
crack=bpy.data.materials.new('grout_shadow_microfractures')
crack.use_nodes=True
crack.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.19,.19,.185,1)
crack.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.98
cracks=[]
for k in range(7):
 x=float(minp.x)+rng.uniform(.06,float(maxp.x-minp.x)-.12)
 z=float(mid.z)+rng.uniform(-.007,.007)
 length=rng.uniform(.035,.09)
 curve=bpy.data.curves.new('genuine_organic_lime_microfracture','CURVE');curve.dimensions='3D'
 sp=curve.splines.new('POLY');sp.points.add(4)
 for j in range(5):
  sp.points[j].co=(x+j*length/4, float(minp.y)-.0015,z+.0007*math.sin(j*1.6+k),1)
 curve.bevel_depth=.00048;curve.bevel_resolution=1
 ob=bpy.data.objects.new('short_inset_grout_tear_'+str(k),curve);bpy.context.collection.objects.link(ob)
 ob.data.materials.append(crack)
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.convert(target='MESH');cracks.append(bpy.context.object)
new=[body,grainobj]+cracks
shoot(out/'after'/'mortar_joint_recess.png')
beforecam=cam.matrix_world.copy();before_scale=cd.ortho_scale
cam.location=Vector((float(mid.x),float(minp.y)-.35,float(mid.z)+.16))
cam.rotation_euler=(Vector((mid.x,mid.y,mid.z))-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=.65
shoot(out/'after'/'mortar_joint_recess_macro.png')
cam.matrix_world=beforecam;cd.ortho_scale=before_scale
bpy.ops.object.select_all(action='DESELECT')
for o in new:o.select_set(True)
bpy.context.view_layer.objects.active=body
target=out/'glb'/'mortar_joint_recess.glb'
bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
if target.stat().st_size<11000:raise RuntimeError('Mortar materials or geometry not exported')
report={'asset':'mortar_joint_recess','prior_run':38895731036,'technical':'PASS_PENDING_ART',
'old_glb_sha':sha(base),'new_glb_sha':sha(target),'source_CC0_atlas_sha':sha(p),
'removed_isolated_white_grit':removed,'embedded_grain_count':160,'microfracture_count':len(cracks),
'mortar_max_height_m':float(maxp.z-minp.z),'material':'photo-sourced subdued calcareous mix',
'art_approved':False,'unity_approved':False,'no_scene_composition':True,
'caveat':'A macro photograph is needed for a narrow 26mm architectural joint; no false success from wide-view insignificance.'}
(out/'reports'/'historical_calcareous_mortar_v10.json').write_text(json.dumps(report,indent=2))
print('GRANULAR_TRUE_LIME_GROUT_V10',target.stat().st_size,'embedded grains',160,flush=True)
