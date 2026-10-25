import bpy,json,pathlib,collections
result=[]
for ob in bpy.context.scene.objects:
 if ob.name.startswith('Temporary'):
  result.append({'name':ob.name,'vertices':len(ob.data.vertices) if ob.type=='MESH' else 0,'faces':len(ob.data.polygons) if ob.type=='MESH' else 0,'materials':[m.name for m in ob.data.materials] if ob.type=='MESH' else [],'material_counts':dict(collections.Counter(p.material_index for p in ob.data.polygons)) if ob.type=='MESH' else {},'bounds':[list(v) for v in ob.bound_box]})
p=pathlib.Path(bpy.app.driver_namespace['interactive_output'])/'cut-diagnostic.json';p.write_text(json.dumps(result,indent=2));print(json.dumps(result))
