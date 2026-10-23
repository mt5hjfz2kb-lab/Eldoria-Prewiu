import bpy,math,json,pathlib,hashlib
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
assert (out/'00-observe.blend').exists() and not (out/'01-cycle.blend').exists()
terrain=bpy.data.objects['BLD_Terrain'];before=hashlib.sha256(b''.join(bytes(v.co) if False else repr(tuple(v.co)).encode() for v in terrain.data.vertices)).hexdigest()
changes=[]
# Five objects identified in the observed image: west canopies unnaturally stretched by prior lattice.
for name,sx,sy,angle,dx,dy in [('BLD_ForestMass_04',.48,1.15,-.22,30,-8),('BLD_ForestMass_09',.58,1.35,.35,45,0),('BLD_ForestMass_14',.55,1.55,-.38,70,0),('BLD_ForestMass_19',.55,1.30,.30,35,-6),('BLD_ForestMass_28',.50,1.20,-.20,55,16)]:
 obj=bpy.data.objects[name];bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 old={'dimensions':list(obj.dimensions),'matrix':[list(r) for r in obj.matrix_world]}
 bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY',center='BOUNDS')
 bpy.ops.transform.resize(value=(sx,sy,1),orient_type='GLOBAL')
 bpy.ops.transform.rotate(value=angle,orient_axis='Z',orient_type='GLOBAL')
 bpy.ops.transform.translate(value=(dx,dy,0),orient_type='GLOBAL')
 bpy.context.view_layer.update()
 changes.append({'object':name,'before':old,'after':{'dimensions':list(obj.dimensions),'matrix':[list(r) for r in obj.matrix_world]},'tools':['origin bounds','resize','rotate','translate']})
assert before==hashlib.sha256(b''.join(repr(tuple(v.co)).encode() for v in terrain.data.vertices)).hexdigest()
(out/'01-decision-and-operations.json').write_text(json.dumps({'observed':'00-observe-panoramica-horizontal.png','reason':'Five western canopies form exaggerated horizontal bands; restore compact asymmetric grouping and clearings before adding any detail.','changes':changes,'terrain_unchanged':True},indent=2))
bpy.app.driver_namespace['interactive_snapshot']('01-cycle')
print('CYCLE_1_COMPLETE',json.dumps(changes))
