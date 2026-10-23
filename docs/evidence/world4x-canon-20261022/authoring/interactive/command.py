import bpy,json,pathlib,hashlib
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
assert (out/'01-cycle.blend').exists() and not (out/'02-cycle.blend').exists()
terrain=bpy.data.objects['BLD_Terrain'];terrain_hash=hashlib.sha256(b''.join(repr(tuple(v.co)).encode() for v in terrain.data.vertices)).hexdigest()
bvh=BVHTree.FromPolygons([v.co for v in terrain.data.vertices],[tuple(p.vertices) for p in terrain.data.polygons],all_triangles=True)
changes=[]
for name in ['BLD_ForestMass_04','BLD_ForestMass_09','BLD_ForestMass_14','BLD_ForestMass_19','BLD_ForestMass_28']:
 obj=bpy.data.objects[name];bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 original=[v.co.copy() for v in obj.data.vertices];lo=Vector(tuple(min(p[k] for p in original) for k in range(3)));hi=Vector(tuple(max(p[k] for p in original) for k in range(3)));w=hi.x-lo.x;d=hi.y-lo.y
 # Native proportional mesh grabs on three observed boundary regions; carve two broad bays and pull one peninsula.
 strokes=[((lo.x+w*.34,hi.y,lo.z),(0,-d*.38,0),w*.29),((lo.x+w*.73,lo.y,lo.z),(-w*.08,d*.33,0),w*.24),((lo.x,(lo.y+hi.y)*.5,lo.z),(-w*.12,-d*.12,0),w*.28)]
 for target,delta,radius in strokes:
  bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_mode(type='VERT');bpy.ops.mesh.select_all(action='DESELECT');bpy.ops.object.mode_set(mode='OBJECT')
  nearest=min(obj.data.vertices,key=lambda v:(v.co-Vector(target)).length_squared);nearest.select=True
  bpy.ops.object.mode_set(mode='EDIT');bpy.ops.transform.translate(value=delta,orient_type='GLOBAL',use_proportional_edit=True,proportional_edit_falloff='SMOOTH',proportional_size=radius);bpy.ops.object.mode_set(mode='OBJECT')
 # Move each column by the difference between old and new real ground contact, retaining crown thickness.
 for v,old in zip(obj.data.vertices,original):
  oldground=bvh.ray_cast(Vector((old.x,old.y,200)),Vector((0,0,-1)))[0]
  newground=bvh.ray_cast(Vector((v.co.x,v.co.y,200)),Vector((0,0,-1)))[0]
  if oldground is not None and newground is not None:v.co.z+=newground.z-oldground.z
 obj.data.update();changes.append({'object':name,'tools':['apply object transforms','vertex selection','three native proportional mesh grabs','ground contact from actual terrain BVH'],'changed_vertices':sum((v.co-old).length>.001 for v,old in zip(obj.data.vertices,original)),'vertex_count':len(original)})
assert terrain_hash==hashlib.sha256(b''.join(repr(tuple(v.co)).encode() for v in terrain.data.vertices)).hexdigest()
(out/'02-decision-and-operations.json').write_text(json.dumps({'observed':'01-cycle-panoramica-horizontal.png','reason':'Object transforms removed stretching but preserved oval puck silhouettes. Edit actual canopy boundaries into bays and peninsulas, retaining the existing mesh and terrain.','changes':changes,'terrain_unchanged':True},indent=2))
bpy.app.driver_namespace['interactive_snapshot']('02-cycle')
print('CYCLE_2_COMPLETE',json.dumps(changes))
