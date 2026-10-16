"""Valoria V3: color-only A/B with original proven CC0 castle door.
V2 gray hardware is ART_REJECTED: never import its mesh. Reuse only atlas.
"""
import bpy,sys,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source,atlas,out=map(Path,args)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
if not source.is_file() or not atlas.is_file():raise RuntimeError('Original geometric source or atlas absent')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1050;s.render.resolution_y=1050
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Single reference door studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def light(n,power,pos,color):
 l=bpy.data.lights.new(n,'AREA');l.energy=power;l.shape='DISK';l.size=9;l.color=color
 o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('warm',2150,(-7,-9,13),(1,.82,.63))
light('cold',1550,(8,4,12),(.71,.82,1))
light('rim',1000,(-2,10,8),(1,.95,.85))
camd=bpy.data.cameras.new('paired camera');cam=bpy.data.objects.new('paired camera',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(source))
objs=[o for o in bpy.data.objects if o not in rig]
mesh=[o for o in objs if o.type=='MESH']
if len(mesh)!=3:raise RuntimeError('Expected intact frame and two door leaves')
corners=[o.matrix_world@Vector(c) for o in mesh for c in o.bound_box]
lo=Vector([min(c[i] for c in corners) for i in range(3)])
hi=Vector([max(c[i] for c in corners) for i in range(3)])
center=(lo+hi)*.5;ext=hi-lo
cam.location=center+Vector((.85,-1.5,1.08)).normalized()*max(ext.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camd.ortho_scale=max(ext.x,ext.y,ext.z)*1.47
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
shoot(out/'before'/'reinforced_oak_gate.png')
grade=bpy.data.images.load(str(atlas),check_existing=False)
grade.pack()
modified=[]
for o in mesh:
 for mat in o.data.materials:
  if not mat or not mat.use_nodes:continue
  for node in mat.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image and 'diff' in node.image.name.lower():
    modified.append((o.name,mat.name,node.image.name))
    node.image=grade
if len(modified)!=3:raise RuntimeError('PBR diffuse not assigned to exactly three original meshes '+str(modified))
shoot(out/'after'/'reinforced_oak_gate.png')
bpy.ops.object.select_all(action='DESELECT')
for o in objs:o.select_set(True)
bpy.context.view_layer.objects.active=mesh[0]
dest=out/'glb'/'reinforced_oak_gate.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<100000:raise RuntimeError('Source geometry/material deleted')
rep={'asset':'reinforced_oak_gate','technical':'PASS_PENDING_ART',
 'base_v1_run':38858438155,'rejected_v2_run':38858928588,
 'source_sha':sha(source),'grade_sha':sha(atlas),'candidate_sha':sha(dest),
 'source_meshes':[o.name for o in mesh],
 'geometry_untouched':True,'materials_changed':modified,
 'art_approved':False,'unity_approved':False,'no_composition':True,
 'decision':'Keep V1 original geometry. Test color difference only; if A/B is not visually superior, reject V3 and revert to V1.'}
(out/'reports'/'wood_color_only_v3.json').write_text(json.dumps(rep,indent=2))
print('VALORIA_COLOR_ONLY_DOOR_V3_TECH_PASS',dest.stat().st_size,flush=True)
