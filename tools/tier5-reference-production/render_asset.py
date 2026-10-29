import bpy, sys, math, json
from pathlib import Path
from mathutils import Vector

args=sys.argv[sys.argv.index('--')+1:]
source,out,kind=args[:3]
out=Path(out); out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=source)
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
points=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box]
lo=Vector(tuple(min(p[i] for p in points) for i in range(3)))
hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
center=(lo+hi)/2; extent=max(hi-lo); scale=3/extent
root=bpy.data.objects.new('Review normalization only',None); bpy.context.collection.objects.link(root)
for o in list(bpy.context.scene.objects):
    if o!=root and o.parent is None: o.parent=root
root.location=-center*scale; root.scale=(scale,)*3
scene=bpy.context.scene
scene.render.engine='CYCLES'; scene.cycles.samples=32
scene.cycles.use_denoising=False; scene.render.threads_mode='FIXED'; scene.render.threads=4
scene.render.resolution_x=900; scene.render.resolution_y=900; scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Neutral studio'); scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.32,.32,.32,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65
scene.view_settings.view_transform='Standard'
scene.view_settings.exposure=0; scene.view_settings.gamma=1
for name,loc,power,size in [('Key',(4,-5,6),600,5),('Fill',(-4,-2,3),350,5),('Rim',(1,4,5),450,4)]:
    data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.shape='DISK';data.size=size
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.location=loc
    ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('Review camera'); cam=bpy.data.objects.new('Review camera',camdata);scene.collection.objects.link(cam)
scene.camera=cam;camdata.type='ORTHO';camdata.ortho_scale=3.65
angles=[0,45,90,135,180,225,270,315]
elev=math.radians(8 if kind=='character' else 30)
for degrees in angles:
    a=math.radians(degrees)
    cam.location=(7*math.cos(a)*math.cos(elev),7*math.sin(a)*math.cos(elev),7*math.sin(elev))
    cam.rotation_euler=(-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(out/('view_%03d.png'%degrees));bpy.ops.render.render(write_still=True)
metrics={'source':Path(source).name,'meshes':len(objects),'triangles':0,'uv_all':all(bool(o.data.uv_layers) for o in objects),'source_bounds':list(hi-lo),'images':[{'name':i.name,'size':list(i.size)} for i in bpy.data.images if i.type=='IMAGE']}
for o in objects:o.data.calc_loop_triangles();metrics['triangles']+=len(o.data.loop_triangles)
(out/'render-metrics.json').write_text(json.dumps(metrics,indent=2))
