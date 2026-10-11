"""Create true V3 + seven V2 integration assembly, separately from source assets."""
import bpy, math, sys, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/asset-family-v2'
OUT.mkdir(parents=True,exist_ok=True)
gate=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb'
assert gate.is_file(),gate
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def imported(path,name,xyz,scale=1):
 prior=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(path))
 new=[o for o in bpy.data.objects if o not in prior]
 assert sum(o.type=='MESH' for o in new)>0
 pivot=bpy.data.objects.new('assembly '+name,None);bpy.context.scene.collection.objects.link(pivot)
 pivot.location=xyz;pivot.scale=scale if isinstance(scale,tuple) else (scale,scale,scale)
 for ob in new:
  if ob.parent is None:ob.parent=pivot
  ob.name=name+' / '+ob.name
 return sum(o.type=='MESH' for o in new)
meshes={}
meshes['V3_Gate_FROZEN']=imported(gate,'V3 gate frozen',(0,0,0))
pieces={
 'wall_straight':[(-10.2,0,0),(10.2,0,0)],
 'watchtower':[(-15,0,0),(15,0,0)],
 'wall_corner':[(-18,7,0),(18,7,0)],
 'wall_connections':[(-6,0,0),(6,0,0)],
 'stone_path_platform':[(0,-11,.15)],
 'cliff_rocks':[(0,0,-.25)],
 'defensive_props':[(0,-4,0)]}
for asset,locs in pieces.items():
 file=OUT/asset/(asset+('-v2-2.glb' if asset=='watchtower' else '-v2-1.glb'))
 assert file.is_file(),file
 meshes[asset]=sum(imported(file,asset,xyz,1.0 if asset!='cliff_rocks' else (4.0,4.0,1.35)) for xyz in locs)
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=24;sc.cycles.use_denoising=False
sc.view_layers[0].cycles.use_denoising=False
sc.render.resolution_x=1280;sc.render.resolution_y=800;sc.render.resolution_percentage=100
sc.render.film_transparent=False
world=bpy.data.worlds.new('Valoria overcast studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.23,.27,.33,1)
light=bpy.data.lights.new('key','AREA');o=bpy.data.objects.new('key',light);sc.collection.objects.link(o)
o.location=(-13,-19,28);light.energy=4500;light.size=12
cam=bpy.data.cameras.new('hero ortho');o=bpy.data.objects.new('hero ortho',cam);sc.collection.objects.link(o)
cam.type='ORTHO';sc.camera=o
for label,where,target,scale in [('hero',(27,-39,27),(0,0,1),42),('strategic',(34,-48,39),(0,0,0),57)]:
 o.location=where;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
 cam.ortho_scale=scale
 sc.render.filepath=str(OUT/('assembled-v2-2-'+label+'.png'))
 bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'assembled-v2-2.blend'))
(OUT/'assembled-v2-2-report.json').write_text(json.dumps({'included':meshes,'hero_gate_source':str(gate.relative_to(ROOT)),'gate_v3_modified':False,'art_pass':False,'unity_pass':False},indent=2))
