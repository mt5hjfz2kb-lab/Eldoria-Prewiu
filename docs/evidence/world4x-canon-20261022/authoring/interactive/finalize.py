import bpy,json,pathlib,hashlib,os
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
assert (out/'03-cycle.blend').exists()
names=['panoramica-horizontal','panoramica-vertical'];current={n:bpy.data.objects[n] for n in names}
for obj in list(bpy.data.objects):
 if obj.users==0 and any(obj.name.startswith(n+'.') for n in names):bpy.data.objects.remove(obj)
with bpy.data.libraries.load(str(out/'00-observe.blend'),link=False) as (src,dst):dst.objects=list(names)
for obj in dst.objects:bpy.context.scene.collection.objects.link(obj)
bpy.context.view_layer.update()
audit={}
for name,original in zip(names,dst.objects):
 obj=current[name];error=max(abs(obj.matrix_world[i][j]-original.matrix_world[i][j]) for i in range(4) for j in range(4))
 assert error<1e-6 and obj.data.ortho_scale==original.data.ortho_scale and obj.data.type==original.data.type, str({'camera':name,'matrix_error':error,'current':list(obj.location),'original':list(original.location),'current_rotation':list(obj.rotation_euler),'original_rotation':list(original.rotation_euler)})
 audit[name]={'same_as_initial_observation':True,'matrix_error':error,'ortho_scale':obj.data.ortho_scale,'matrix_world':[list(r) for r in obj.matrix_world]}
 bpy.data.objects.remove(original,do_unlink=True)
terrain=bpy.data.objects['BLD_Terrain'];bvh=BVHTree.FromPolygons([v.co for v in terrain.data.vertices],[tuple(p.vertices) for p in terrain.data.polygons],all_triangles=True)
probes=[]
for x,y,z in [(96.8059,-398.115,-5.4),(113.8059,-406.115,-8.4),(128.8059,-420.115,-17.6)]:
 h=bvh.ray_cast(Vector((x,y,200)),Vector((0,0,-1)))[0];probes.append({'point':[x,y,z],'terrain_height':h.z if h else None,'water_buried':h is not None and z<h.z})
for request in sorted(out.glob('request-*.json')):
 data=json.loads(request.read_text(encoding='utf-8-sig'));code=data.get('params',{}).get('code')
 if isinstance(code,str):bpy.data.texts.new('MCP_'+request.stem).write(code)
bpy.context.scene.camera=current['panoramica-vertical'];bpy.context.scene.render.resolution_x=390;bpy.context.scene.render.resolution_y=844
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Interactive_Trial_NOT_APPROVED.blend'))
report={'pid':os.getpid(),'three_cycles_completed':True,'art_approved':False,'unity_changed':False,'cameras':audit,'river_mouth_probes':probes,'final_blend_sha256':hashlib.sha256((out/'Eldoria_Interactive_Trial_NOT_APPROVED.blend').read_bytes()).hexdigest()}
(out/'final-verification.json').write_text(json.dumps(report,indent=2))
(out/'stop.request').write_text('Three cycles reviewed. Save and close this owned session; do not touch any other process.')
print('FINAL_VERIFICATION',json.dumps(report))
