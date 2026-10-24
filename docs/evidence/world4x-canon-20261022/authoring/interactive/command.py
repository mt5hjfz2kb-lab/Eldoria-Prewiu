# Read-only baseline observation. No source asset or gameplay is modified.
import bpy,json,pathlib,hashlib
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
scene=bpy.context.scene
cameras={'panoramica-vertical': {'sourceScene': 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity', 'version': '6000.3.23f1', 'label': 'panoramica-vertical', 'sha256': 'b7af4f826683688041272bb5f83a9a4c631f3688bbb6f3ac1dce31a80c735d9d', 'context': 'Actual integrated Valoria World PlayMode / disposable genuine Gateway progression', 'targetId': '', 'actualUnityCamera': True, 'playMode': True, 'physicalIphone': False, 'width': 390, 'height': 844, 'zoom': 2048.107177734375, 'aspect': 0.4620853066444397, 'cameraPosition': {'x': 1017.9999389648438, 'y': 4602.06640625, 'z': -2214.57763671875}, 'worldTarget': {'x': 1017.9999389648438, 'y': 0.0, 'z': 30.0}, 'cameraRotation': {'x': 0.5299192667007446, 'y': 0.0, 'z': 0.0, 'w': 0.8480480909347534}}, 'regional-horizontal': {'sourceScene': 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity', 'version': '6000.3.23f1', 'label': 'regional-horizontal', 'sha256': 'e0aa8525361b083fe81b21a0f905682a161ececb0d10837997b5063e274a8596', 'context': 'Actual integrated Valoria World PlayMode / disposable genuine Gateway progression', 'targetId': '', 'actualUnityCamera': True, 'playMode': True, 'physicalIphone': False, 'width': 1280, 'height': 720, 'zoom': 280.0, 'aspect': 1.7777777910232544, 'cameraPosition': {'x': 1018.0, 'y': 629.1558227539062, 'z': -276.8597717285156}, 'worldTarget': {'x': 1018.0, 'y': 0.0, 'z': 30.000001907348633}, 'cameraRotation': {'x': 0.5299192667007446, 'y': 0.0, 'z': 0.0, 'w': 0.8480480909347534}}}
camera_records={}
# Render one representation of each tree, not overlapping near/far duplicates.
for o in scene.objects:
 if o.name.startswith(('BLD_ForestFar_','Far_Pilot_Pine')):o.hide_render=True
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
bpy.app.driver_namespace['interactive_snapshot']('00-continental-observe')
print(json.dumps({'observed':True,'edited_geometry':False,'blender_mcp':True,'terrain_vertices':len(bpy.data.objects['BLD_Terrain'].data.vertices),'art_approved':False}))
