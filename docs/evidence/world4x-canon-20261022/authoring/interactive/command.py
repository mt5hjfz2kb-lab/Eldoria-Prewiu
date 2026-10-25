import bpy,json,pathlib,math
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE_NEXT' if bpy.app.version<(5,0,0) else 'BLENDER_EEVEE'
if scene.render.engine=='CYCLES':scene.cycles.samples=8
for o in scene.objects:
 if o.name.startswith(('BLD_ForestFar_','Far_Pilot')):o.hide_render=True
scene.world.color=(.45,.45,.45)
if not any(o.type=='LIGHT' for o in scene.objects):
 light=bpy.data.lights.new('Review daylight','SUN');light.energy=2
 ob=bpy.data.objects.new('Review daylight',light);scene.collection.objects.link(ob);ob.rotation_euler=(.65,-.4,-.6)
for label,position,scale,res in [('regional',(0,-307,629),560,(1280,720)),('panorama',(0,-2000,3900),4100,(720,1280))]:
 data=bpy.data.cameras.new(label);camera=bpy.data.objects.new(label,data);scene.collection.objects.link(camera)
 camera.location=position;camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler()
 data.type='ORTHO';data.ortho_scale=scale;scene.camera=camera
 scene.render.resolution_x,scene.render.resolution_y=res;scene.render.resolution_percentage=100
 scene.render.filepath=str(out/(label+'-baseline-proxy.png'));bpy.ops.render.render(write_still=True)
objects=[{'name':o.name,'vertices':len(o.data.vertices),'dimensions':list(o.dimensions)} for o in scene.objects if o.type=='MESH' and ('Breach' in o.name or o.name=='BLD_Terrain')]
(out/'macro-inspection.json').write_text(json.dumps({'objects':objects,'mcp_executed':True,'preview_not_Unity':True},indent=2))
(out/'stop.request').write_text('Read-only source inspection complete; close owned session.')
print('MACRO_SOURCE_OBSERVED',json.dumps(objects))
