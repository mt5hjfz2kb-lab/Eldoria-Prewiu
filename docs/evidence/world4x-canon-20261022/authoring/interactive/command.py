import bpy,bmesh,json,pathlib,hashlib
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
assert (out/'02-cycle.blend').exists() and not (out/'03-cycle.blend').exists()
names=['BLD_ForestMass_04','BLD_ForestMass_09','BLD_ForestMass_14','BLD_ForestMass_19','BLD_ForestMass_28']
# Reject folded silhouettes observed in cycle 2; recover only these objects, not the scene.
for obj in list(bpy.data.objects):
 if obj.users==0 and any(obj.name.startswith(n+'.') for n in names):bpy.data.objects.remove(obj)
with bpy.data.libraries.load(str(out/'01-cycle.blend'),link=False) as (src,dst):dst.objects=list(names)
for name,recovered in zip(names,dst.objects):
 old=bpy.data.objects[name];collections=list(old.users_collection);bpy.data.objects.remove(old,do_unlink=True);recovered.name=name
 for collection in collections:collection.objects.link(recovered)
river=bpy.data.objects['BLD_River_00'];terrain=bpy.data.objects['BLD_Terrain']
original=[v.co.copy() for v in terrain.data.vertices]
bm=bmesh.new();bm.from_mesh(river.data)
edge=min((e for e in bm.edges if e.is_boundary),key=lambda e:sum(v.co.y for v in e.verts)/2)
start=sum((v.co for v in edge.verts),Vector())/2
assert -400<start.y<-370 and 70<start.x<110,tuple(start)
extrusions=[]
for delta,widen in [((12,-12,-.5),1.05),((17,-8,-3),1.15),((15,-14,-9.2),1.25)]:
 result=bmesh.ops.extrude_edge_only(bm,edges=[edge]);new=[g for g in result['geom'] if isinstance(g,bmesh.types.BMVert)]
 assert len(new)==2
 bmesh.ops.translate(bm,verts=new,vec=Vector(delta));centre=(new[0].co+new[1].co)/2
 for v in new:v.co.x=centre.x+(v.co.x-centre.x)*widen;v.co.y=centre.y+(v.co.y-centre.y)*widen
 edge=next(e for e in bm.edges if all(v in new for v in e.verts))
 extrusions.append({'centre':list(centre),'width':(new[0].co-new[1].co).length})
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(river.data);bm.free();river.data.update()
# Shape the physical mouth with local native proportional editing of the existing terrain.
bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
strokes=[]
for x,y,target_z,radius in [(103,-399,-6.7,16),(119,-409,-9.5,18),(137,-420,-15,16)]:
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_mode(type='VERT');bpy.ops.mesh.select_all(action='DESELECT');bpy.ops.object.mode_set(mode='OBJECT')
 vertex=min(terrain.data.vertices,key=lambda v:(v.co.x-x)**2+(v.co.y-y)**2)
 dz=min(0,target_z-vertex.co.z);vertex.select=True
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.transform.translate(value=(0,0,dz),orient_type='GLOBAL',use_proportional_edit=True,proportional_edit_falloff='SMOOTH',proportional_size=radius);bpy.ops.object.mode_set(mode='OBJECT');strokes.append({'at':[x,y],'delta_z':dz,'radius':radius})
changed=[v.index for v,p in zip(terrain.data.vertices,original) if (v.co-p).length>.001]
assert changed and all(terrain.data.vertices[i].co.y< -350 for i in changed),'Editing escaped local river mouth'
(out/'03-decision-and-operations.json').write_text(json.dumps({'observed':'02-cycle-panoramica-horizontal.png','rejected_cycle2':'Notched canopy reads as folded sheets, not woodland. Restore exact cycle1 objects.','decision':'Repair the visible disconnected main river mouth by native edge extrusion and local terrain editing. No full generator run.','river_start':list(start),'extrusions':extrusions,'terrain_strokes':strokes,'terrain_changed_vertices':len(changed),'changed_indices':changed,'core_preserved':True},indent=2))
bpy.app.driver_namespace['interactive_snapshot']('03-cycle')
# Preserve exact camera matrices and operation history alongside the final editable file.
(out/'camera-audit.json').write_text(json.dumps({name:{'matrix_world':[list(r) for r in bpy.data.objects[name].matrix_world],'type':bpy.data.objects[name].data.type,'ortho_scale':bpy.data.objects[name].data.ortho_scale} for name in ['panoramica-horizontal','panoramica-vertical']},indent=2))
print('CYCLE_3_COMPLETE',json.dumps({'river_extrusions':extrusions,'terrain_changed_vertices':len(changed)}))
