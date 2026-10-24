import bpy,json,pathlib,math
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
report=[]
objects=[o for o in bpy.context.scene.objects if o.type=='MESH' and not o.hide_render and (o.name=='BLD_Terrain' or 'Cutbank' in o.name or 'River' in o.name)]
for y in [-32.92237,-27.92237,-22.92237,-18.92237,-16.92237,-15.92237,-14.92237,-12.92237,-7.92237,0.07763]:
 row={'x':3.3,'y':y,'hits':[]}
 for obj in objects:
  inv=obj.matrix_world.inverted();hit,p,n,face=obj.ray_cast(inv@Vector((3.3,y,150)),inv.to_3x3()@Vector((0,0,-1)),distance=400)
  if hit:row['hits'].append({'object':obj.name,'height':(obj.matrix_world@p).z})
 report.append(row)
(out/'bridge-support-surface-diagnostic.json').write_text(json.dumps({'source':'40069960813','probe':report,'surface_names':[o.name for o in objects]},indent=2))
print('SURFACE_DIAGNOSIS',json.dumps(report))
