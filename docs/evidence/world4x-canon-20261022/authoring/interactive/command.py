import bpy,pathlib,json
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
objects=[]
for ob in bpy.context.scene.objects:
 if ob.type=='MESH' and not ob.name.startswith(('BLD_Forest','Pilot_Pine','Far_Pilot')):
  objects.append({'name':ob.name,'vertices':len(ob.data.vertices),'polygons':len(ob.data.polygons),'location':list(ob.location),'dimensions':list(ob.dimensions),'materials':[m.name if m else '' for m in ob.data.materials]})
(out/'inspected-artwork.json').write_text(json.dumps({'objects':objects,'images':[{'name':im.name,'size':list(im.size),'packed':bool(im.packed_file)} for im in bpy.data.images]},indent=2))
hidden={o:o.hide_render for o in bpy.context.scene.objects}
for o in bpy.context.scene.objects:
 if o.name.startswith(('BLD_ForestFar_','Far_Pilot_Pine')):o.hide_render=True
bpy.app.driver_namespace['interactive_snapshot']('inspection')
for o,v in hidden.items():o.hide_render=v
(out/'stop.request').write_text('Inspection and real Blender previews saved')
print('ARTIST_SCENE_INSPECTION_SAVED',len(objects))
