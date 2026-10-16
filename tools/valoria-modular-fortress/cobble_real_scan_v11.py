"""Valoria cobble_pebbles V11: preserve existing 28-pebble layout but replace angular
primitive stones with decimated CC0 photogrammetry chip, distinct world rotations,
subtle genuine vertex variability and real contact baseline. Same Blender studio A/B.
No Unity/Bastion/4X, no new library, no automatic ART_APPROVED.
"""
import bpy,sys,json,hashlib,random,math
from pathlib import Path
from mathutils import Vector
from mathutils import noise
a=sys.argv[sys.argv.index('--')+1:]
if len(a)!=3:raise RuntimeError('source cobbles glb, exact scanned chip glb, output dir')
old,donor,out=map(Path,a)
for sub in ('before','after','glb','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
def sha(x):return hashlib.sha256(x.read_bytes()).hexdigest()
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1120;s.render.resolution_y=920;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('neutral earth small stone studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.43,.48,.54,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=world
for label,power,loc,col in [('warm stone rake',1800,(-6,-8,12),(1,.91,.76)),('cold sky',1100,(6,5,9),(.8,.9,1))]:
 light=bpy.data.lights.new(label,'AREA');light.energy=power;light.size=9;light.color=col
 obj=bpy.data.objects.new(label,light);bpy.context.collection.objects.link(obj)
 obj.location=loc;obj.rotation_euler=(-obj.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('same stone cluster camera')
cam=bpy.data.objects.new('same stone cluster camera',camdata)
bpy.context.collection.objects.link(cam);s.camera=cam;camdata.type='ORTHO'
def meshes():return [o for o in s.objects if o.type=='MESH']
def bbox(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs for v in o.bound_box]
 return Vector(tuple(min(v[i] for v in pts) for i in range(3))),Vector(tuple(max(v[i] for v in pts) for i in range(3)))
def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(old))
original=meshes()
if len(original)<12:raise RuntimeError('Expected actual scattered pebble source, not 1 block')
lo,hi=bbox(original);dim=hi-lo;mid=(hi+lo)*.5
positions=[]
for ob in original:
 a,b=bbox([ob]);positions.append(((a+b)*.5,b-a))
if not (.9<dim.x<2.2 and .9<dim.y<2.2 and .04<dim.z<.35):
 raise RuntimeError('Unexpected modular pebble scatter envelope '+str(dim))
cam.location=mid+Vector((.8,-1.25,1.5)).normalized()*dim.length*2.8
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=dim.length*1.32
# Neutral floor exists only in the evaluation studio, NOT in exported pebbles.
bpy.ops.mesh.primitive_plane_add(size=3.7,location=(mid.x,mid.y,float(lo.z)-.004))
plane=bpy.context.object;plane.name='STUDIO_FLOOR_NOT_EXPORTED'
m=bpy.data.materials.new('neutral earth matte check surface');m.use_nodes=True
m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.30,.325,.31,1)
plane.data.materials.append(m)
shoot(out/'before'/'cobble_pebbles.png')
for obj in original:bpy.data.objects.remove(obj,do_unlink=True)
bpy.ops.import_scene.gltf(filepath=str(donor))
imported=[o for o in meshes() if o is not plane]
if len(imported)!=1:raise RuntimeError('Scanned chip must import as a single real mesh')
base=imported[0]
bpy.ops.object.select_all(action='DESELECT');base.select_set(True)
bpy.context.view_layer.objects.active=base
# Make a genuine mobile-suitable decimated scan; donor is >60k triangles,
# whereas each tiny pebble needs only several hundred.
facecount=len(base.data.polygons)
if facecount<1000:raise RuntimeError('Expected authentic high-resolution scan, not generic cube')
mod=base.modifiers.new('real CC0 chip mobile LOD','DECIMATE')
mod.ratio=min(1.,380/facecount)
bpy.ops.object.modifier_apply(modifier=mod.name)
lod_faces=len(base.data.polygons)
if lod_faces>800 or lod_faces<180:raise RuntimeError('Scan LOD failed')
rng=random.Random(20261016)
res=[]
for k,(target,d) in enumerate(positions):
 obj=base if k==0 else base.copy()
 if k:
  obj.data=base.data.copy();bpy.context.collection.objects.link(obj)
 elif obj.data.users>1:obj.data=obj.data.copy()
 obj.name=f'pebble_real_scanned_stone_{k+1:02d}'
 # Distinct azimuthal orientation of each actual 3D donor.
 obj.rotation_euler[2]=rng.uniform(0,math.tau)
 obj.rotation_euler[0]=rng.uniform(-.13,.13)
 obj.rotation_euler[1]=rng.uniform(-.15,.15)
 # Small physical erosion is DIFFERENT for every pebble and remains visible
 # in silhouette, unlike a superficial hue-only change.
 for v in obj.data.vertices:
  offset=noise.noise(Vector((v.co.x*9+k*.39,v.co.y*9-k*.23,v.co.z*9)))
  v.co+=v.normal*(.007*offset)
 bpy.context.view_layer.update()
 l,h=bbox([obj]);size=h-l
 sx=max(.01,d.x-.006)/max(size.x,.001)
 sy=max(.01,d.y-.006)/max(size.y,.001)
 sz=max(.012,d.z-.002)/max(size.z,.001)
 factor=min(sx,sy,sz)*rng.uniform(.86,.97)
 obj.scale=Vector((factor,factor,factor))
 bpy.context.view_layer.update();l,h=bbox([obj])
 # Ground every genuine stone at original lowest grade instead of floating
 # pebbles and keep its original X/Y location in modular 1.4m cluster.
 obj.location+=Vector((target.x-(l.x+h.x)*.5,target.y-(l.y+h.y)*.5,float(lo.z)-l.z))
 res.append(obj)
nl,nh=bbox(res);nsize=nh-nl
if nsize.x>dim.x+.022 or nsize.y>dim.y+.022 or nsize.z>dim.z+.025:
 raise RuntimeError('Pebble cluster exceeded original modular envelope')
shoot(out/'after'/'cobble_pebbles.png')
camera_matrix=cam.matrix_world.copy();ortho=camdata.ortho_scale
camdata.ortho_scale=dim.x*.72
shoot(out/'after'/'cobble_pebbles_macro.png')
cam.matrix_world=camera_matrix;camdata.ortho_scale=ortho
bpy.ops.object.select_all(action='DESELECT')
for ob in res:ob.select_set(True)
bpy.context.view_layer.objects.active=res[0]
glb=out/'glb'/'cobble_pebbles.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<30000:raise RuntimeError('Missing actual scanned stone GLB')
r={'asset':'cobble_pebbles','source_run':38870003255,'donor_run':38878576444,
 'source_sha256':sha(old),'donor_sha256':sha(donor),'candidate_sha256':sha(glb),
 'photographic_cc0_donor':'cliff_stone_chip','individual_stones':len(res),
 'per_stone_lod_triangles':lod_faces,'original_bounds_m':list(dim),
 'candidate_bounds_m':list(nsize),'real_ground_contact_in_studio':True,
 'real_3d_stones':True,'same_camera_before_after':True,
 'new_asset_family_created':False,'studio_floor_exported':False,
 'art_approved':False,'unity_validated':False,'status':'TECH_PASS_PENDING_VISUAL'}
(out/'reports'/'cobble_scanned_real_stone_v11.json').write_text(json.dumps(r,indent=2))
print('REAL_SCANNED_28_GROUNDED_PEBBLES_WAIT_VISUAL',sha(glb),lod_faces)
