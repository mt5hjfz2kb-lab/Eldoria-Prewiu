import bpy,json,pathlib
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
terrain=bpy.data.objects['BLD_Terrain'];mesh=terrain.data
faces=[tuple(p.vertices) for p in mesh.polygons if 'Bridge' not in mesh.materials[p.material_index].name]
bv=BVHTree.FromPolygons([terrain.matrix_world@v.co for v in mesh.vertices],faces)
supports=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith('Pilot_Cutbank_')]
river=bpy.data.objects['Pilot_River'];rows=[]
for i in range(41):
 x=3.3;y=-32.922370433807373+i
 ground=bv.ray_cast(Vector((x,y,150)),Vector((0,0,-1)))[0]
 hitnames=['BLD_Terrain_without_bridge'] if ground is not None else []
 for obj in supports+[river]:
  inv=obj.matrix_world.inverted();hit,p,n,face=obj.ray_cast(inv@Vector((x,y,150)),inv.to_3x3()@Vector((0,0,-1)),distance=400)
  if hit:hitnames.append(obj.name)
 rows.append({'y':y,'hits':hitnames})
assert all(r['hits'] for r in rows),json.dumps([r for r in rows if not r['hits']])
(out/'bridge-independent-surface-oracle.json').write_text(json.dumps({'old_bridge_faces_excluded':True,'samples':rows,'all_supported':True},indent=2))
print('BRIDGE_ORACLE_REAL_BANK_AND_WATER_COVERAGE_PASS',len(rows))
import bpy,pathlib,hashlib
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
snapshot=bpy.app.driver_namespace['interactive_snapshot']
source=out.parent/'executive-e5c63ba475cd-20261024'/'Eldoria_Macro_Review_Candidate_NOT_APPROVED.blend'
assert hashlib.sha256(source.read_bytes()).hexdigest()=='40caf5805b1d21595f637f329ea66a57135e1c1825b3b0cc4d5addc7bf272d0d'
bpy.ops.wm.open_mainfile(filepath=str(source))
bpy.app.driver_namespace['interactive_output']=str(out);bpy.app.driver_namespace['interactive_snapshot']=snapshot
# Read-only baseline observation. No source asset or gameplay is modified.
import bpy,json,pathlib,hashlib
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
scene=bpy.context.scene
cameras={'panoramica-vertical': {'sourceScene': 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity', 'version': '6000.3.23f1', 'label': 'panoramica-vertical', 'sha256': 'b7af4f826683688041272bb5f83a9a4c631f3688bbb6f3ac1dce31a80c735d9d', 'context': 'Actual integrated Valoria World PlayMode / disposable genuine Gateway progression', 'targetId': '', 'actualUnityCamera': True, 'playMode': True, 'physicalIphone': False, 'width': 390, 'height': 844, 'zoom': 2048.107177734375, 'aspect': 0.4620853066444397, 'cameraPosition': {'x': 1017.9999389648438, 'y': 4602.06640625, 'z': -2214.57763671875}, 'worldTarget': {'x': 1017.9999389648438, 'y': 0.0, 'z': 30.0}, 'cameraRotation': {'x': 0.5299192667007446, 'y': 0.0, 'z': 0.0, 'w': 0.8480480909347534}}, 'regional-horizontal': {'sourceScene': 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity', 'version': '6000.3.23f1', 'label': 'regional-horizontal', 'sha256': 'e0aa8525361b083fe81b21a0f905682a161ececb0d10837997b5063e274a8596', 'context': 'Actual integrated Valoria World PlayMode / disposable genuine Gateway progression', 'targetId': '', 'actualUnityCamera': True, 'playMode': True, 'physicalIphone': False, 'width': 1280, 'height': 720, 'zoom': 280.0, 'aspect': 1.7777777910232544, 'cameraPosition': {'x': 1018.0, 'y': 629.1558227539062, 'z': -276.8597717285156}, 'worldTarget': {'x': 1018.0, 'y': 0.0, 'z': 30.000001907348633}, 'cameraRotation': {'x': 0.5299192667007446, 'y': 0.0, 'z': 0.0, 'w': 0.8480480909347534}}}
camera_records={}
# Match continental far-LOD geometry and its existing Unity scale for these distant proxies.
for o in scene.objects:
 if o.name.startswith('BLD_ForestMass_'):
  far=bpy.data.objects.get(o.name.replace('BLD_ForestMass_','BLD_ForestFar_'))
  if far and not o.hide_render:
   o.hide_render=True;far.hide_render=False
   far.scale.x*=1.5;far.scale.y*=1.5;far.scale.z*=1.85
 if o.name.startswith('Far_Pilot_Pine'):o.hide_render=True
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=8;scene.cycles.use_denoising=True
scene.view_settings.view_transform='Standard'
for label,m in cameras.items():
 o=bpy.data.objects.get(label)
 if o is None:
  o=bpy.data.objects.new(label,bpy.data.cameras.new(label));scene.collection.objects.link(o)
 cp=m['cameraPosition'];home=m['worldTarget'];o.location=(cp['x']-1018,cp['z']-30,cp['y'])
 target=Vector((home['x']-1018,home['z']-30,home['y']))
 o.rotation_euler=(target-o.location).to_track_quat('-Z','Y').to_euler()
 o.data.type='ORTHO';o.data.clip_end=20000;o.data.ortho_scale=1;scene.camera=o
 scene.render.resolution_x=m['width'];scene.render.resolution_y=m['height'];scene.render.resolution_percentage=100
 bpy.context.view_layer.update()
 right=o.matrix_world.to_3x3() @ Vector((1,0,0))
 a=world_to_camera_view(scene,o,target);b=world_to_camera_view(scene,o,target+right)
 o.data.ortho_scale=abs(b.x-a.x)*2*m['zoom']*m['aspect']
 camera_records[label]={'unity_metadata':m,'blender_matrix':[list(row) for row in o.matrix_world],'ortho_scale':o.data.ortho_scale,'render_is_unity':False}
(out/'matched-camera-proxy.json').write_text(json.dumps(camera_records,indent=2))
bpy.app.driver_namespace['interactive_snapshot']('00-forest-lod-observe')
print(json.dumps({'observed':True,'edited_geometry':False,'blender_mcp':True,'terrain_vertices':len(bpy.data.objects['BLD_Terrain'].data.vertices),'art_approved':False}))

# Pending source-workshop experiment, NOT applied to production.
# Art diagnosis: projected fir crowns are squat at the real 62 degree view.
# Keep every tree anchor, species topology, UV and material. Do not add objects.
import bpy,math,json,pathlib

def apply_canopy_proportions():
 changes=[]
 for tree in bpy.context.scene.objects:
  if not tree.name.startswith(('BLD_ForestMass_','BLD_ForestFar_')):continue
  if tree.hide_get():continue
  x,y=tree.location.x,tree.location.y
  # Preserve exact retained AF/AG representation; gradual change outside its edge.
  distance=math.hypot(max(0,abs(x)-96),max(0,-78-y,y-270))
  weight=max(0,min(1,(distance-35)/180));weight=weight*weight*(3-2*weight)
  if weight==0:continue
  before=list(tree.scale)
  tree.scale.x*=1-.15*weight;tree.scale.y*=1-.15*weight;tree.scale.z*=1+.65*weight
  changes.append({'name':tree.name,'anchor':list(tree.location),'before':before,'after':list(tree.scale)})
 bpy.context.view_layer.update()
 return changes

changes=apply_canopy_proportions()
(out/'canopy-proportion-operations.json').write_text(json.dumps({'art_approved':False,'unity_integrated':False,'changes':changes},indent=2))
bpy.app.driver_namespace['interactive_snapshot']('01-slender-canopy-review')
print('CANOPY_REVIEW_ONLY',len(changes))
