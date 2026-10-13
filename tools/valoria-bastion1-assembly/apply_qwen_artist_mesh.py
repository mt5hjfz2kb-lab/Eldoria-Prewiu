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
# Qwen's second-stage schema may use a curved sculpt path or crest reprofile
# instead of repeating the previously rejected sculpt_region. The MODEL
# authors all coordinates; this merge stage only validates and reuses output.
GEOMETRY_TYPES={'sculpt_region','sculpt_path','reprofile_crest'}
if (not isinstance(plan,list) or not 1<=len(plan)<=2 or
    any(not isinstance(op,dict) or op.get('type') not in GEOMETRY_TYPES or
        op.get('target')!=a.target or not isinstance(op.get('parameters'),dict)
        for op in plan)):
 raise ValueError('Expected 1..2 original Qwen-authored typed TERRAIN geometry operations')
def finite_vector(value,n):
 return (isinstance(value,list) and len(value)==n and
         all(isinstance(v,(int,float)) and not isinstance(v,bool) and math.isfinite(v) for v in value))
for op in plan:
 typ=op['type'];p=op['parameters']
 if typ=='sculpt_region':
  if (not finite_vector(p.get('center'),3) or not finite_vector(p.get('delta'),3) or
      not isinstance(p.get('radius'),(int,float)) or isinstance(p.get('radius'),bool) or
      not .05<=p['radius']<=60 or not .005<math.dist(p['delta'],(0,0,0))<=20):
   raise ValueError('Invalid original Qwen sculpt-region specification')
 elif typ=='sculpt_path':
  knots=p.get('knots')
  if not isinstance(knots,list) or not 2<=len(knots)<=24:
   raise ValueError('Qwen must author two or more sculpt-path knots')
  centers=[]
  for knot in knots:
   if (not isinstance(knot,dict) or not finite_vector(knot.get('center'),3) or
       not finite_vector(knot.get('delta'),3) or
       not isinstance(knot.get('radius'),(int,float)) or isinstance(knot.get('radius'),bool) or
       not .05<=knot['radius']<=60 or not .005<math.dist(knot['delta'],(0,0,0))<=20):
    raise ValueError('Invalid Qwen-authored spatial knot; never manufacture coordinates')
   centers.append(tuple(knot['center']))
  if len(set(centers))<2:raise ValueError('Degenerate Qwen sculpt path')
 elif typ=='reprofile_crest':
  points=p.get('control_points')
  if (p.get('axis') not in ('x','y') or p.get('height_axis') not in ('y','z')
      or p['axis']==p['height_axis'] or not isinstance(points,list) or
      not 3<=len(points)<=24 or not all(finite_vector(q,2) for q in points) or
      len({float(q[0]) for q in points})!=len(points) or
      not isinstance(p.get('band_min'),(int,float)) or
      not isinstance(p.get('band_max'),(int,float)) or
      not all(math.isfinite(v) for v in (p['band_min'],p['band_max'])) or
      p['band_min']>=p['band_max']):
   raise ValueError('Invalid Qwen-authored crest profile')
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
 if changed<20 or not .05<=highest<=5.05:raise ValueError('Artist edit too small or too strong')
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
proof={'source_blend_sha256':sha(a.source),'artist_blend_sha256':sha(a.artist),'plan_sha256':sha(a.plan),'edited_blend_sha256':sha(source_out),'edited_glb_sha256':sha(glb_out),'target_object':a.target,'original_qwen_operation_types':[op['type'] for op in plan],'changed_vertices':changed,'max_vertex_delta_m':round(highest,5),'protected_cell_changes':protected,'total_vertices':len(mesh.vertices),'full_scene_nonterrain_objects_preserved':True,'gameplay_qa':False,'visual_art_approved':False}
(a.out/'integration-proof.json').write_text(json.dumps(proof,indent=2)+'\n')
print('BASTION1_GENUINE_QWEN_MESH_REUSED_IN_FULL_SCENE',json.dumps(proof))
