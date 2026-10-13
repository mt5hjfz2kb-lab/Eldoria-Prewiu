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
cx,cy=-20.4,-11.3
if abs(cx)<12 or not -16<cy<-9:raise RuntimeError('Invalid site')
cz=z_at(cx,cy)
wall_cz=z_at(-21.0,-8.0)
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
scale=min(5.15/size.x,3.20/size.y,3.00/size.z,1.80)
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
sx=min(1,5.9/max(.01,ps.x));sy=min(1,4.5/max(.01,ps.y));sz=min(1,.28/max(.01,ps.z))
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

# A genuine roofed wall-side guard bay, rather than the visually rejected tiny
# freestanding sign. The Qwen plan selected a western guard + supply anchor.
# These specific meshes are deterministic Blender implementation, NOT AI sculpt.
new_parts=[]
def beam(label,position,dimensions,tilt=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=position)
 o=bpy.context.object;o.name=label;o.dimensions=dimensions
 if tilt:o.rotation_euler.x=tilt
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(wood)
 new_parts.append(o)
 return o
# Front edge faces the camera/bridge. Rear eave nearly meets the pre-existing
# detached wall at y=-8; no additional wall/gate/buildable parcel is inserted.
for dx in (-2.45,2.45):
 for dy in (-1.8,1.8):
  beam('Bastion I timber guard bay upright',(cx+dx,cy+dy,cz+1.4),(.22,.22,2.8))
for dy in (-1.8,1.8):
 beam('Bastion I guard bay horizontal wall plate',(cx,cy+dy,cz+2.80),(5.20,.23,.21))
beam('Bastion I guard bay roof ridge',(cx,cy,cz+3.54),(5.9,.20,.18))
# Two pitched planes; roof materials are copied from existing selected wooden
# props, not injected screen-aligned decals or a newly sourced paid asset.
roof=wood.copy();roof.name='Bastion I weathered timber guard shelter roof'
if roof.use_nodes and roof.node_tree.nodes.get('Principled BSDF'):
 bs=roof.node_tree.nodes.get('Principled BSDF')
 color=bs.inputs['Base Color'].default_value
 bs.inputs['Base Color'].default_value=(min(1,color[0]*.82),min(1,color[1]*.76),min(1,color[2]*.68),1)
 bs.inputs['Roughness'].default_value=.90
else:roof.diffuse_color=(.23,.17,.11,1)
for side in (-1,1):
 panel=beam('Bastion I pitched guard shelter roof',(cx,cy+side*.95,cz+3.16),(5.85,2.13,.13),tilt=-side*.385)
 panel.data.materials.clear();panel.data.materials.append(roof)
# Visible structural knee braces, deliberately grouped with roof/posts.
from mathutils import Vector as BVec
def diagonal(label,a,b,thick=.13):
 start,end=BVec(a),BVec(b)
 direction=end-start
 obj=beam(label,(start+end)/2,(thick,thick,direction.length))
 obj.rotation_euler=direction.to_track_quat('Z','Y').to_euler()
 return obj
for dx in (-2.45,2.45):
 for dy in (-1.8,1.8):
  diagonal('Bastion I guard post timber knee brace',
   (cx+dx,cy+dy,cz+2.15),(cx+dx*.67,cy+dy,cz+2.80))
# Structural masonry enclosure, deliberately integrated with the west wall,
# rather than a second roof or a scatter of standalone decorative props.
# Wall material is borrowed from the actual authored wall asset. The geometry
# below is deterministic and must NOT be represented as Qwen-authored vertices.
wall_meshes=group('LeftDetachedRemnant')
stone=next((m for o in wall_meshes for m in o.data.materials if m),None)
if stone is None:raise RuntimeError('West wall has no reusable stone material')
stone_parts=[]
def masonry(label,position,dimensions):
 bpy.ops.mesh.primitive_cube_add(size=1,location=position)
 obj=bpy.context.object;obj.name=label;obj.dimensions=dimensions
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 obj.data.materials.append(stone)
 stone_parts.append(obj)
 return obj
# A broad wall-side base ties the shelter footprint to the old west remnant.
# Preserve the open front toward the playable courtyard. The rear return
# reaches y=-8.25, immediately beside the existing wall at y=-8.
rear_y=-8.75
masonry('Bastion I integrated west wall rear masonry return',
 (cx,rear_y,cz+1.02),(8.0,.72,2.05))
# Side wing walls frame the original shelter and form a legible medieval bay.
# No central route or future building parcel is occupied.
for dx in (-3.88,3.88):
 masonry('Bastion I guard compound stone wing',
 (cx+dx,-10.65,cz+.77),(.72,4.5,1.55))
 masonry('Bastion I stone wing cap',
 (cx+dx,-10.65,cz+1.59),(.90,4.62,.18))
# The masonry threshold makes the courtyard-side entrance read as a single
# architectural compound, not an object floating over the grass.
masonry('Bastion I guard compound stone threshold',
 (cx,-13.15,cz+.12),(8.1,.72,.24))
# Wall-to-roof bridging lintel and stepped parapet anchor the timber roof to
# the masonry enclosure. Their height is restrained relative to fortification.
masonry('Bastion I west wall shelter connection lintel',
 (cx,-9.18,cz+2.88),(6.25,.50,.42))
for dx in (-3.1,-1.55,0,1.55,3.1):
 masonry('Bastion I west wall low merlon',
 (cx+dx,rear_y,cz+2.34),(0.88,.77,.64))
# An outer contact skirt visually seats the structure into the slope without
# replacing terrain or inserting a new navigable raised building parcel.
masonry('Bastion I guard compound embedded foundation',
 (cx,-11.08,cz-.08),(8.35,5.8,.27))
# Authentic modular wall_connections stone mesh anchors the new return to the
# western remnant. It is a scaled masonry buttress, NOT a separate watchtower.
pier_kit=group('LeftDetachedPier')
pier_corners=[o.matrix_world @ Vector(v) for o in pier_kit for v in o.bound_box]
pier_lo=Vector(tuple(min(v[k] for v in pier_corners) for k in range(3)))
pier_hi=Vector(tuple(max(v[k] for v in pier_corners) for k in range(3)))
pier_size=pier_hi-pier_lo
if min(pier_size.x,pier_size.y,pier_size.z)<.01:raise RuntimeError('Source wall-connection pier has degenerate bounds')
pier_scale=min(2.45/pier_size.x,2.2/pier_size.y,3.9/pier_size.z)
px,py=-17.95,-8.70
pier_anchor=(Matrix.Translation(Vector((px,py,z_at(px,py)-.015)))
 @ Matrix.Scale(pier_scale,4)
 @ Matrix.Translation(Vector((-(pier_lo.x+pier_hi.x)/2,-(pier_lo.y+pier_hi.y)/2,-pier_lo.z))))
reused_wall_connection_pier_meshes=0
for old in pier_kit:
 obj=old.copy();obj.data=old.data;obj.parent=None
 obj.matrix_world=pier_anchor @ old.matrix_world
 obj.name='Bastion I Qwen-selected integrated west-wall buttressed corner %02d'%reused_wall_connection_pier_meshes
 bpy.context.collection.objects.link(obj);reused_wall_connection_pier_meshes+=1
# A second real source-mesh corner is physically founded on the existing
# left stone wing at the roof's foreground edge. This is the front termination
# of the SAME wall-integrated compound, not a detached tower/building.
front_pier_scale=min(2.0/pier_size.x,1.9/pier_size.y,2.9/pier_size.z)
front_px,front_py=-24.27,-12.27
front_pier_anchor=(Matrix.Translation(Vector((front_px,front_py,z_at(front_px,front_py)-.025)))
 @ Matrix.Scale(front_pier_scale,4)
 @ Matrix.Translation(Vector((-(pier_lo.x+pier_hi.x)/2,-(pier_lo.y+pier_hi.y)/2,-pier_lo.z))))
reused_front_wall_corner_meshes=0
for old in pier_kit:
 obj=old.copy();obj.data=old.data;obj.parent=None
 obj.matrix_world=front_pier_anchor @ old.matrix_world
 obj.name='Bastion I original west wing front buttress %02d'%reused_front_wall_corner_meshes
 bpy.context.collection.objects.link(obj);reused_front_wall_corner_meshes+=1
# Real Qwen decision excerpts (runs 38479509741 and 38480277880):
# stepped buttressed masonry transition + cliff-rock footing + existing bush.
# Meshes, proportions and placements remain deterministic Blender work.
for bx,by in ((cx-3.76,-10.25),(cx+3.76,-10.25)):
 masonry('Bastion I stepped masonry wall connection buttress lower',
         (bx,by,cz+.57),(.95,1.35,1.16))
 masonry('Bastion I stepped masonry wall connection buttress upper',
         (bx,by+.30,cz+1.55),(.68,.86,.90))
# Reuse only an authored rock module; rescale as a small, planted wall-footing
# rock, not as a pasted decal or an independent new playable obstruction.
rockkit=group('FrontEscarpment_-23')
rockcorners=[o.matrix_world @ Vector(c) for o in rockkit for c in o.bound_box]
rocklo=Vector(tuple(min(v[k] for v in rockcorners) for k in range(3)))
rockhi=Vector(tuple(max(v[k] for v in rockcorners) for k in range(3)))
rocksize=rockhi-rocklo
if min(rocksize.x,rocksize.y,rocksize.z)<.01:raise RuntimeError('Rock module missing real geometry')
rockscale=min(1.7/rocksize.x,1.3/rocksize.y,.82/rocksize.z)
rx,ry=-24.8,-11.45
rock_anchor=(Matrix.Translation(Vector((rx,ry,z_at(rx,ry)-.05)))
 @ Matrix.Scale(rockscale,4)
 @ Matrix.Translation(Vector((-(rocklo.x+rockhi.x)/2,-(rocklo.y+rockhi.y)/2,-rocklo.z))))
reused_rock_meshes=0
for old in rockkit:
 obj=old.copy();obj.data=old.data;obj.parent=None
 obj.matrix_world=rock_anchor @ old.matrix_world
 obj.name='Bastion I Qwen-selected original masonry junction rock %02d'%reused_rock_meshes
 bpy.context.collection.objects.link(obj);reused_rock_meshes+=1
# Exactly one duplicated original bush; no synthetic flora or resource nodes.
bush_sources=[o for o in meshes if o.name.startswith('EA03_Nature_Bush_01a_LOD0')]
if not bush_sources:raise RuntimeError('Qwen-specified original EA03 bush missing')
bush=bush_sources[0]
bush_corners=[bush.matrix_world @ Vector(v) for v in bush.bound_box]
bush_base=Vector((sum(v.x for v in bush_corners)/8,
                  sum(v.y for v in bush_corners)/8,
                  min(v.z for v in bush_corners)))
bush_x,bush_y=-24.95,-10.85
bush_clone=bush.copy();bush_clone.data=bush.data;bush_clone.parent=None
bush_clone.matrix_world=(Matrix.Translation(Vector((bush_x,bush_y,z_at(bush_x,bush_y)+.01)))
 @ Matrix.Scale(1.62,4) @ Matrix.Translation(-bush_base) @ bush.matrix_world)
bush_clone.name='Bastion I Qwen-selected reused wall-junction bush'
bpy.context.collection.objects.link(bush_clone)
# Keeping the central road, playable court and state-driven lots empty is
# explicit. No colliders or resource nodes are included in the exported GLB.
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'ai_proposal_sha256':hashlib.sha256(proposal_path.read_bytes()).hexdigest(),
 'ai_authorship':('curated excerpts from two genuine but incomplete local Qwen responses; every mesh implementation deterministic Blender' if proposal.get('_provenance',{}).get('mode')=='curated_excerpts_from_two_incomplete_real_qwen_inferences' else 'concept/site from original Qwen inference; geometry implementation deterministic Blender'),
 'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'site':[cx,cy,cz],'reused_prop_meshes':len(kit),'reused_paving_meshes':len(platform),
 'new_timber_meshes':len(new_parts),'new_masonry_meshes':len(stone_parts),'reused_wall_connection_pier_meshes':reused_wall_connection_pier_meshes,'reused_front_wall_corner_meshes':reused_front_wall_corner_meshes,'reused_junction_rock_meshes':reused_rock_meshes,'reused_junction_bush_meshes':1,'roof_panels':2,'road_clearance_m':abs(cx)-4.25-7,'existing_source_meshes_modified':False,
 'new_resource_nodes':0,'architectural_integration':'western masonry return and wings, original wall_connections masonry rear and front supporting buttresses, stepped ground buttresses, Qwen-selected original cliff rock and bush; deterministic execution','art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_AI_PROPOSED_GUARD_STATION',json.dumps(proof))
