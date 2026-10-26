import bpy,pathlib,json,hashlib
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
assert hashlib.sha256(source.read_bytes()).hexdigest()=='fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce'
objects=[]
for ob in bpy.context.scene.objects:
 if ob.type!='MESH' or ob.name.startswith(('BLD_Forest','Far_','Pilot_Pine','Bank forest stone')):continue
 coords=[ob.matrix_world@Vector(v) for v in ob.bound_box]
 objects.append({'name':ob.name,'vertices':len(ob.data.vertices),'material_faces':{m.name:sum(p.material_index==i for p in ob.data.polygons) for i,m in enumerate(ob.data.materials) if m},'bounds_min':[min(v[i] for v in coords) for i in range(3)],'bounds_max':[max(v[i] for v in coords) for i in range(3)],'materials':[m.name if m else None for m in ob.data.materials]})
materials=[]
for m in bpy.data.materials:
 if not m.use_nodes:continue
 images=[{'name':n.image.name,'size':list(n.image.size)} for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
 materials.append({'name':m.name,'colour':list(m.diffuse_color),'images':images})
(out/'player-art-source-inspection.json').write_text(json.dumps({'objects':objects,'materials':materials,'source':str(source),'sha256':hashlib.sha256(source.read_bytes()).hexdigest()},indent=2))
print('PLAYER_ART_SOURCE_INSPECTION_COMPLETE',len(objects),len(materials))
(out/'stop.request').write_text('Inspection completed, source unchanged; release editor before authoring batch.')
