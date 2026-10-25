import bpy,pathlib,math
from mathutils import Vector
def artist_snapshot(label):
 out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);scene=bpy.context.scene
 camera_before=scene.camera;hidden={o:o.hide_render for o in scene.objects}
 for ob in scene.objects:
  if ob.type=='LIGHT' or (ob.name.startswith(('BLD_ForestFar_','Far_Pilot_Pine')) and not bpy.app.driver_namespace.get('artist_preview_use_far',False)):ob.hide_render=True
 data=bpy.data.cameras.new('Artist composition review');camera=bpy.data.objects.new('Artist composition review',data);scene.collection.objects.link(camera);data.type='ORTHO';data.clip_end=15000;scene.camera=camera
 light_data=bpy.data.lights.new('Artist matched workshop sun','SUN');sun=bpy.data.objects.new('Artist matched workshop sun',light_data);scene.collection.objects.link(sun);light_data.energy=2.2
 sun.rotation_euler=Vector((.384,-.548,.743)).to_track_quat('Z','Y').to_euler()
 scene.render.engine='BLENDER_EEVEE';scene.render.image_settings.file_format='PNG';scene.render.resolution_percentage=100
 scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
 try:
  for name,focus,offset,scale,width,height in [
   ('near-breach',(-260,150,-3),(0,-100,140),145,1280,720),
   ('regional',(-40,150,4),(0,-800,1700),1050,1280,720),
   ('panorama',(0,0,0),(0,-2300,5200),4200,390,844)]:
   target=Vector(focus);camera.location=target+Vector(offset);camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();data.ortho_scale=scale
   scene.render.resolution_x=width;scene.render.resolution_y=height;scene.render.filepath=str(out/(label+'-'+name+'.png'));bpy.ops.render.render(write_still=True)
 finally:
  scene.camera=camera_before;bpy.data.objects.remove(camera,do_unlink=True);bpy.data.objects.remove(sun,do_unlink=True)
  for ob,state in hidden.items():ob.hide_render=state
bpy.app.driver_namespace['interactive_snapshot']=artist_snapshot
