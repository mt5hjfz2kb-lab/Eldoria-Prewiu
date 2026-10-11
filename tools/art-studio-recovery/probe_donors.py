"""Reuse Eldoria donor audit/render logic without mutating production assets."""
import bpy,json,sys,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/studio-rocks-v1'
OUT.mkdir(parents=True,exist_ok=True)
sources=['art-source/valoria/production/rock-terrain-family-v1/RockTerrainFamilyV1.glb','Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/SteppedRockTerrace.glb','Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/BroadRockPlatform.glb','Unity/Assets/Holotna/Mountain/Models/Rock01.fbx','Unity/Assets/PolyOne/Free Modular Terrain/Model/SM_Cliffs_01.fbx']
report=[]
for idx,path in enumerate(sources):
 bpy.ops.wm.read_factory_settings(use_empty=True)
 if path.endswith('.glb'):bpy.ops.import_scene.gltf(filepath=str(ROOT/path))
 else:bpy.ops.import_scene.fbx(filepath=str(ROOT/path))
 objects=[o for o in bpy.data.objects if o.type=='MESH']
 verts=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
 lo=Vector(tuple(min(v[k] for v in verts) for k in range(3)));hi=Vector(tuple(max(v[k] for v in verts) for k in range(3)));center=(lo+hi)/2
 size=hi-lo
 camera=bpy.data.cameras.new('donor_camera');cam=bpy.data.objects.new('donor_camera',camera);bpy.context.collection.objects.link(cam)
 cam.location=center+Vector((1.3,-1.8,1.3))*max(size);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();camera.type='ORTHO';camera.ortho_scale=max(size)*1.45
 s=bpy.context.scene;s.camera=cam;s.render.engine='CYCLES';s.cycles.samples=12;s.cycles.use_denoising=False
 s.render.resolution_x=640;s.render.resolution_y=480;s.render.resolution_percentage=100;s.view_settings.view_transform='Standard'
 world=bpy.data.worlds.new('neutral_world');s.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.25,.31,.4,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.7
 li=bpy.data.lights.new('key','SUN');ob=bpy.data.objects.new('key',li);bpy.context.collection.objects.link(ob);ob.rotation_euler=(.55,-.35,-.5);li.energy=2
 s.render.filepath=str(OUT/f'donor-{idx}.png');bpy.ops.render.render(write_still=True)
 report.append({'path':path,'meshes':[{'name':o.name,'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'materials':[m.name if m else None for m in o.data.materials]} for o in objects],'bounds':[list(lo),list(hi)],'images':[i.name for i in bpy.data.images]})
(OUT/'donor-audit.json').write_text(json.dumps(report,indent=2))
