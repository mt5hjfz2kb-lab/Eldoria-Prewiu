"""Merge a verified AI-authored Blender sculpt into the existing full Valoria scene.

Inputs are immutable. This tool changes only the named terrain mesh, preserving
canonical materials, UVs, architecture, gameplay parcel anchors, and other assets.
This is not artistic approval: requires subsequent real Unity captures + WebGL QA.
"""
import argparse,hashlib,json,math,pathlib,sys
import bpy
from mathutils import Vector
p=argparse.ArgumentParser()
p.add_argument('--source',required=True,type=pathlib.Path)
p.add_argument('--artist',required=True,type=pathlib.Path)
p.add_argument('--plan',required=True,type=pathlib.Path)
p.add_argument('--out',required=True,type=pathlib.Path)
p.add_argument('--target',default='Valoria natural playable hilltop and rock scarps')
p.add_argument('--source-sha256',required=True)
p.add_argument('--artist-sha256',required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for source,digest in ((a.source,a.source_sha256),(a.artist,a.artist_sha256)):
 if source.suffix!='.blend' or not source.exists() or sha(source)!=digest:raise ValueError('Source SHA mismatch: '+str(source))
if not a.plan.is_file():raise ValueError('Artist authoring plan missing')
plan=json.loads(a.plan.read_text())
if not isinstance(plan,list) or len(plan)!=1 or plan[0].get('type')!='sculpt_region' or plan[0].get('target')!=a.target:raise ValueError('Artist-only terrain sculpt required')
spec=plan[0].get('parameters') or {}
center=spec.get('center');delta=spec.get('delta');radius=spec.get('radius')
if not isinstance(center,list) or len(center)!=3 or not isinstance(delta,list) or len(delta)!=3:raise ValueError('Authored spatial values absent')
if not all(isinstance(v,(int,float)) and math.isfinite(v) for v in center+delta) or not isinstance(radius,(int,float)):raise ValueError('Unsafe artist coordinates')
magnitude=math.sqrt(sum(float(v)**2 for v in delta))
if not .50<=magnitude<=2 or not 10<=radius<=24 or abs(center[0])<27 or abs(center[0])>52:raise ValueError('Qwen planned sculpt outside authorized cliff shell')
if a.out.resolve() in (a.source.parent.resolve(),a.artist.parent.resolve()):raise ValueError('Source folder must remain immutable')
a.out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(a.source.resolve()))
original=bpy.data.objects.get(a.target)
if original is None or original.type!='MESH':raise ValueError('Canonical terrain absent')
old=original.data
old_world=original.matrix_world.copy()
old_count=len(old.vertices);old_faces=len(old.polygons)
old_mats=list(old.materials)
with bpy.data.libraries.load(str(a.artist.resolve()),link=False) as (src,dst):
 if a.target not in src.objects:raise ValueError('Authored terrain absent')
 dst.objects=[a.target]
external=dst.objects[0]
if external is None or external.type!='MESH':raise ValueError('Replacement is not a Blender mesh')
mesh=external.data.copy()
mesh.transform(old_world.inverted()@external.matrix_world)
if not (.65*old_count<=len(mesh.vertices)<=1.35*old_count):raise ValueError('Terrain geometry cardinality unexpectedly changed')
if len(mesh.uv_layers)==0:raise ValueError('Sculpt destroyed texture UV mapping')
if len(mesh.polygons)!=old_faces:raise ValueError('Sculpt unexpectedly changed mesh faces')
if any(not math.isfinite(c) for v in mesh.vertices for c in v.co):raise ValueError('Nonfinite vertex coordinates')
# Verify the reused mesh is actually displaced without reshaping protected
# top-of-plateau cells, where roads, parcels, and building anchors live.
if len(mesh.vertices)==old_count:
 changed=0;protected=0;highest=0.
 for before,after in zip(old.vertices,mesh.vertices):
  d=(before.co-after.co).length
  if d>.001:changed+=1
  highest=max(highest,d)
  if abs(before.co.x)<22 and -34<before.co.y<36 and before.co.z>-1 and d>.035:protected+=1
 if changed<20 or not .05<=highest<=2.05:raise ValueError('Artist edit too small or too strong')
 if protected:raise ValueError('Qwen terrain sculpt intrudes on '+str(protected)+' protected playable cells')
else:
 raise ValueError('Artist sculpt must retain exactly indexed source vertices')
saved_material_indices=[poly.material_index for poly in mesh.polygons]
mesh.materials.clear()
for material in old_mats:mesh.materials.append(material)
for poly,slot in zip(mesh.polygons,saved_material_indices):poly.material_index=slot
original.data=mesh
mesh.update()
bpy.context.view_layer.update()
source_out=a.out/'Bastion1QwenScene.blend'
glb_out=a.out/'Bastion1QwenScene.glb'
bpy.ops.wm.save_as_mainfile(filepath=str(source_out.resolve()),compress=True)
bpy.ops.export_scene.gltf(filepath=str(glb_out.resolve()),export_format='GLB',export_cameras=False,export_lights=False)
proof={'source_blend_sha256':sha(a.source),'artist_blend_sha256':sha(a.artist),'plan_sha256':sha(a.plan),'edited_blend_sha256':sha(source_out),'edited_glb_sha256':sha(glb_out),'target_object':a.target,'changed_vertices':changed,'max_vertex_delta_m':round(highest,5),'protected_cell_changes':protected,'total_vertices':len(mesh.vertices),'full_scene_nonterrain_objects_preserved':True,'gameplay_qa':False,'visual_art_approved':False}
(a.out/'integration-proof.json').write_text(json.dumps(proof,indent=2)+'\n')
print('BASTION1_GENUINE_QWEN_MESH_REUSED_IN_FULL_SCENE',json.dumps(proof))
