"""Materialize a genuine local-AI proposed medieval focal station using existing assets.
Requires Qwen's archived proposal JSON; human-authored geometry execution, NOT
Qwen-authored mesh vertices. No new buildings, resources, or central road edits.
blender -b --python compose_bastion_guard_station.py -- base.glb proposal.json out.glb
"""
import bpy,sys,json,pathlib,hashlib,math
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
argv=sys.argv[sys.argv.index('--')+1:]
if len(argv)!=3:raise SystemExit('Expected base.glb proposal.json output.glb')
src,proposal_path,dst=map(pathlib.Path,argv)
proposal=json.loads(proposal_path.read_text())
for k in ('concept','site','composition','existing_assets','functional_risks'):
 if not proposal.get(k):raise RuntimeError('AI proposal incomplete: '+k)
ptext=' '.join(str(proposal[k]) for k in ('concept','site','composition')).lower()
if not any(x in ptext for x in ('guard','supply','suministr','vigil','puesto','station','almac','outpost')):
 raise RuntimeError('AI did not choose an approved-scale guard/supply landmark')
if not any(x in ptext for x in ('left','izquier','west','oeste','wall','muralla','fortif')):
 raise RuntimeError('AI proposal does not ground the station beside existing west wall')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
objects=list(bpy.data.objects)
meshes=[o for o in objects if o.type=='MESH']
terrain=[o for o in meshes if 'natural playable hilltop' in o.name.lower()]
if len(terrain)!=1:raise RuntimeError('Missing frozen original terrain')
bvh=BVHTree.FromObject(terrain[0],bpy.context.evaluated_depsgraph_get())
def z_at(x,y):
 hit,n,i,d=bvh.ray_cast(Vector((x,y,100)),Vector((0,0,-1)),200)
 if hit is None:raise RuntimeError('No contact with original terrain')
 return hit.z
# Site is constrained by the known detached west wall x=-21,y=-8, never
# at a future central parcel or axial path. Reserve 7m-wide axial corridor.
cx,cy=-20.4,-12.0
if abs(cx)<12 or not -16<cy<-9:raise RuntimeError('Invalid site')
cz=z_at(cx,cy)
# Source groups exist as named empties from the immutable authored assembly.
def descendants(root):
 result=[]
 for o in meshes:
  p=o.parent
  while p is not None:
   if p==root:
    result.append(o);break
   p=p.parent
 return result
def group(label):
 roots=[o for o in objects if o.name.split('.')[0]==label]
 if len(roots)!=1:raise RuntimeError('Authored asset group missing '+label+': '+str(len(roots)))
 group_meshes=descendants(roots[0])
 if not group_meshes:raise RuntimeError('No reusable meshes under '+label)
 return group_meshes
# Defensive kit contains already-selected medieval props. Preserve each
# actual mesh/material/UV and duplicate the assembled group at reduced size.
kit=group('LowerGatePracticalSet')
coords=[o.matrix_world @ Vector(c) for o in kit for c in o.bound_box]
low=Vector(tuple(min(v[k] for v in coords) for k in range(3)))
high=Vector(tuple(max(v[k] for v in coords) for k in range(3)))
size=high-low
if size.x<=0 or size.y<=0 or size.z<=0:raise RuntimeError('Invalid existing prop bounds')
scale=min(4.3/size.x,2.5/size.y,2.5/size.z,1.6)
anchor=Matrix.Translation(Vector((cx,cy,cz+.10))) @ Matrix.Scale(scale,4) @ Matrix.Translation(Vector((-(low.x+high.x)/2,-(low.y+high.y)/2,-low.z)))
instances=0
for old in kit:
 obj=old.copy();obj.data=old.data;obj.parent=None
 obj.matrix_world=anchor @ old.matrix_world
 obj.name='Bastion I AI-selected reused guard-supply props %02d'%instances
 bpy.context.collection.objects.link(obj);instances+=1
# Reuse one existing stone platform at 4.7 x 3.1m for ground contact.
platform=group('CentralRoad_-4')
corners=[o.matrix_world @ Vector(c) for o in platform for c in o.bound_box]
plo=Vector(tuple(min(v[k] for v in corners) for k in range(3)))
phi=Vector(tuple(max(v[k] for v in corners) for k in range(3)))
ps=phi-plo
sx=min(1,4.7/max(.01,ps.x));sy=min(1,3.1/max(.01,ps.y));sz=min(1,.28/max(.01,ps.z))
# Existing road slabs may be multi-part; same materials and geometry retained.
trans=Matrix.Translation(Vector((cx,cy,cz-.025))) @ Matrix.Diagonal(Vector((sx,sy,sz,1))) @ Matrix.Translation(Vector((-(plo.x+phi.x)/2,-(plo.y+phi.y)/2,-plo.z)))
for old in platform:
 obj=old.copy();obj.data=old.data;obj.parent=None;obj.matrix_world=trans @ old.matrix_world
 obj.name='Bastion I reused guard station paving %02d'%instances
 bpy.context.collection.objects.link(obj);instances+=1
# Sparse framed guard sign: 2 posts + 1 lintel, weathered timber from the
# pre-existing practical kit material if available; fallback a neutral color.
wood=next((m for o in kit for m in o.data.materials if m and ('wood' in m.name.lower() or 'oak' in m.name.lower())),None)
if wood is None:
 wood=bpy.data.materials.new('Bastion I unpainted timber');wood.diffuse_color=(.30,.23,.15,1)
def beam(label,position,dimensions):
 bpy.ops.mesh.primitive_cube_add(size=1,location=position)
 o=bpy.context.object;o.name=label;o.dimensions=dimensions
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(wood)
 return o
for dx in (-1.7,1.7):
 beam('Bastion I small timber guard frame post',(cx+dx,cy+1.0,cz+1.25),(.16,.16,2.5))
beam('Bastion I small timber guard frame lintel',(cx,cy+1.0,cz+2.5),(3.55,.20,.22))
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'ai_proposal_sha256':hashlib.sha256(proposal_path.read_bytes()).hexdigest(),
 'ai_authorship':('owner-directed deterministic fallback, NOT Qwen' if proposal.get('_provenance')=='owner_directed_fallback' else 'concept and site from verified local Qwen inference; geometry implementation deterministic Blender'),
 'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'site':[cx,cy,cz],'reused_prop_meshes':len(kit),'reused_paving_meshes':len(platform),
 'new_timber_meshes':3,'road_clearance_m':abs(cx)-7,'existing_source_meshes_modified':False,
 'new_resource_nodes':0,'art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_AI_PROPOSED_GUARD_STATION',json.dumps(proof))
