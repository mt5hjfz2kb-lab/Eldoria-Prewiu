"""Reimport exact portable exports; assembly demonstrates real contact and reuse.
Frozen V3 stays intact. This is family QA, not a published-game screenshot.
"""
import bpy,hashlib,json,math,os
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'art-source/valoria/production/asset-family-v2/quality-v3'
GATE=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb'
gate_hash=hashlib.sha256(GATE.read_bytes()).hexdigest()
bpy.ops.wm.read_factory_settings(use_empty=True)
records=[]
def imported(path,label,xyz=(0,0,0),angle=0):
 prior=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(path));new=[o for o in bpy.data.objects if o not in prior]
 root=bpy.data.objects.new(label,None);bpy.context.scene.collection.objects.link(root);root.location=xyz;root.rotation_euler.z=angle
 for ob in new:
  if ob.parent is None:ob.parent=root
  ob.name=label+' / '+ob.name
 meshes=[o for o in new if o.type=='MESH']
 assert meshes
 records.append({'asset':label,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'meshes':len(meshes),'position':xyz,'angle':angle})
 return root,meshes
_,gate_mesh=imported(GATE,'Frozen Hero V3')
bpy.context.view_layer.update()
gate_edge=max((ob.matrix_world@Vector(c)).x for ob in gate_mesh for c in ob.bound_box)
def part(name,loc,angle=0):return imported(BASE/name/(name+'.glb'),name,loc,angle)
# Connect sockets to the actual frozen gate extent, then full walls.
for side in (-1,1):
 angle=0 if side==1 else math.pi
 part('wall_connections',(side*(gate_edge+.85),0,0),angle)
 part('wall_straight',(side*(gate_edge+5.8),0,0),angle)
 # Corner arm extends from x=-4 to 0, joins outer wall end without overlap.
 part('wall_corner',(side*(gate_edge+13.8),0,0),0 if side==1 else math.pi)
 part('watchtower',(side*(gate_edge+13.8),4.9,0))
# Repeat original-width modules without anisotropic geometry scaling.
for i in range(5):part('cliff_rocks',(-24+i*12,-6,-.06))
for i in range(2):part('stone_path_platform',(0,-4+i*8,.005))
part('defensive_props',(0,7.5,0))
# QA-only backing ground supports the path and fortifications behind the cliff.
# It is explicitly context, not an eighth family asset or a game terrain claim.
soil=next(m for m in bpy.data.materials if m.name.startswith('Valoria family soil portable'))
verts=[(-30,-4,-.5),(30,-4,-.5),(30,12,-.5),(-30,12,-.5),(-30,-4,.005),(30,-4,.005),(30,12,.005),(-30,12,.005)]
data=bpy.data.meshes.new('QA ground context');data.from_pydata(verts,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]);data.update();data.materials.append(soil)
ground=bpy.data.objects.new('QA_CONTEXT_ONLY - ground support, excluded from asset delivery',data);sc=bpy.context.scene;sc.collection.objects.link(ground)
uv=data.uv_layers.new(name='UVMap')
for poly in data.polygons:
 for li in poly.loop_indices:
  v=data.vertices[data.loops[li].vertex_index].co;uv.data[li].uv=(v.x*.3,v.y*.3)
# Identical embedded pixels remain identical, with one packed image per hash.
# This keeps the editable QA assembly compact without changing any export.
canonical_images={};deduplicated=0
for mat in bpy.data.materials:
 if not mat.use_nodes:continue
 for node in mat.node_tree.nodes:
  if node.type!='TEX_IMAGE' or node.image is None or not node.image.packed_file:continue
  im=node.image;key=(hashlib.sha256(im.packed_file.data).hexdigest(),im.colorspace_settings.name)
  if key in canonical_images:
   node.image=canonical_images[key];deduplicated+=1
  else:canonical_images[key]=im
for im in list(bpy.data.images):
 if im.users==0:bpy.data.images.remove(im)
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=int(os.environ.get('ELDORIA_FAMILY_SAMPLES','48'));sc.cycles.use_denoising=True
sc.render.resolution_x=1600;sc.render.resolution_y=1000;sc.render.resolution_percentage=100;sc.render.image_settings.file_format='PNG'
sc.view_settings.view_transform='AgX';sc.view_settings.look='None'
world=bpy.data.worlds.new('Family dusk studio');world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.17,.22,.31,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.5;sc.world=world
for name,pos,power,color,size in [('Warm sunset',(-18,-24,32),7000,(1,.77,.53),18),('Sky',(18,-2,27),5000,(.49,.64,1),15),('Rim',(-16,20,28),6000,(1,.71,.48),18)]:
 light=bpy.data.lights.new(name,'AREA');ob=bpy.data.objects.new(name,light);sc.collection.objects.link(ob);ob.location=pos;ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler();light.energy=power;light.color=color;light.size=size
sun=bpy.data.lights.new('Scale-independent warm daylight','SUN');sun.energy=1.8;sun.angle=.22;sun.color=(1,.83,.66)
sun_ob=bpy.data.objects.new('Scale-independent warm daylight',sun);sc.collection.objects.link(sun_ob);sun_ob.rotation_euler=(Vector((0,0,0))-Vector((-18,-24,32))).to_track_quat('-Z','Y').to_euler()
cam=bpy.data.cameras.new('Export roundtrip camera');ob=bpy.data.objects.new('Export roundtrip camera',cam);sc.collection.objects.link(ob);sc.camera=ob;cam.type='ORTHO'
for view,pos,target,scale in [('hero',(32,-47,30),(0,0,1),57),('strategic',(35,-50,43),(0,0,0),67),('gate-contact',(14,-20,13),(3,0,3),20)]:
 ob.location=pos;ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler();cam.ortho_scale=scale;sc.render.filepath=str(BASE/('assembly-'+view+'.png'));bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BASE/'family-assembly.blend'))
assert hashlib.sha256(GATE.read_bytes()).hexdigest()==gate_hash
(BASE/'assembly-evidence.json').write_text(json.dumps({'frozen_gate_sha256':gate_hash,'exact_glb_reimports':records,'no_anisotropic_scaling':True,'identical_packed_image_references_deduplicated':deduplicated,'ground_support':'explicit QA-only context, not a delivered family member','art_pass':False,'unity_pass':False,'scope':'offline modular family QA, not game integration'},indent=2))
