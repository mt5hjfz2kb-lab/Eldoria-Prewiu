"""V12 genuine modular flank extension to existing V7 LowerGate.
Preserves its meshes, licensed photogrammetry, bridge, road, twelve stairs, camera
and gameplay colliders. Uses the existing authored WallFamilyV1, never a new kit.
No creative remodelling or anisotropic stretching. Not automatic art approval.
"""
import bpy,os,json,hashlib,math,struct,bmesh
from mathutils import Vector,Matrix
root=os.getcwd()
base=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016')
out=os.path.join(base,'entry-zone-v12')
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(base,'entry-zone-v7','ValoriaEntryZoneV7.blend')
wall=os.path.join(root,'art-source/valoria/production/wall-family-v1/WallFamilyV1.blend')
bounds_report=os.path.join(base,'entry-zone-v11-inspection','wall-source-world-bounds.json')
assert sha(source)=='24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381'
assert sha(wall)=='0ac5ef48b55946d0820b8b064662f8d077060332044b077f2993d9444595f430'
report=json.load(open(bounds_report,encoding='utf-8'))
assert report['wall_family_sha256']==sha(wall)
bpy.ops.wm.open_mainfile(filepath=source)
source_scene_names=set(bpy.data.objects.keys())
v7_terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('ValoriaEntryTerrainV7'))
roadstone=next(m for m in bpy.data.materials if m.name.startswith('ENTRY_road_masonry_PBR'))
# Genuine PBR maps already packed in immutable V7; no new external textures.
assert any(n.type=='TEX_IMAGE' and n.image for n in roadstone.node_tree.nodes)
v7_terrain.name='ValoriaEntryTerrainV12'
donors=['WestPartialWall','WestWallTowerA','WestWallTowerB','EastWallTowerA','EastWallTowerB','EastPartialWall']
with bpy.data.libraries.load(wall,link=False) as (s,d):
 d.objects=[n for n in s.objects if n in donors]
pieces={o.name:o for o in d.objects if o is not None and o.type=='MESH'}
assert len(pieces)==6,(list(pieces),donors)
for o in pieces.values():
 bpy.context.scene.collection.objects.link(o)
bpy.context.view_layer.update()
# Compare evaluated source transforms to the previously measured real WORLD boxes;
# no assumption of identity/origin-zero and no moving parented source objects.
source_geometry={}
for name,obj in pieces.items():
 mat=obj.matrix_world.copy()
 world=[mat@v.co for v in obj.data.vertices]
 mins=Vector([min(p[i] for p in world) for i in range(3)])
 maxs=Vector([max(p[i] for p in world) for i in range(3)])
 cert=report['wall_world_bounds'][name]
 assert max(abs(mins[i]-cert['world_min_xyz'][i]) for i in range(3))<.005,(name,mins,cert['world_min_xyz'])
 assert max(abs(maxs[i]-cert['world_max_xyz'][i]) for i in range(3))<.005,(name,maxs,cert['world_max_xyz'])
 source_geometry[name]=(world,mins,maxs,obj)
# Verify only license-checked or procedurally authored materials are used.
# Replaced image-bearing donor mats are not imported into the candidate.
donor_images={}
for name,obj in pieces.items():
 names=[]
 for m in obj.data.materials:
  if m is None:continue
  for node in m.node_tree.nodes if m.use_nodes else []:
   if node.type=='TEX_IMAGE' and node.image:
    names.append(node.image.name)
 donor_images[name]=names
# Source donor texture provenance is not certified. Explicitly DISCARD every
# imported donor material, retaining only vertex/face geometry and UVs.
# Replace every slot with license-checked original V7 PBR/trim references.
# This never exports donor textures or modifies the original donor .blend.
v7rock=next(m for m in bpy.data.materials if m.name.startswith('ENTRY_existing_rock_PBR'))
v7trim=next((m for m in bpy.data.materials if m.name=='Eldoria_trim'),roadstone)

# Geometry only from the actual modular source. Foundation alignments keyed to
# the frozen LowerGate V7 WORLD bounds (on this Blender Z-up landscape).
gate=report['gate_world_bounds']
assert gate['ENTRY_LowerGate_WestWing']['world_max_xyz'][0]>10.4
assert gate['ENTRY_LowerGate_EastWing']['world_min_xyz'][0]<-9.6
# West wall continues V7 tower/wing from x10.45 to16.52; east is its
# explicitly mirrored COPY, with new normals, not an uncontrolled glTF mirror.
# East donor wall is angled ~9m in Y and would distort this entrance in place;
# it is inspected but not selected. Four real towers keep original proportions.
plan=[
 ('WestPartialWall','WestFlankWall',+13.50,7.75,.375,False),
 ('WestPartialWall','EastFlankWall',-13.50,7.75,.375,True),
 ('WestWallTowerA','WestOuterTower',+16.24,7.78,.38,False),
 ('WestWallTowerB','WestInnerTower',+10.73,7.78,.38,False),
 ('EastWallTowerA','EastOuterTower',-16.24,7.78,.38,False),
 ('EastWallTowerB','EastInnerTower',-10.73,7.78,.38,False),
]
# Preserve the original wall-family trim/cap shading, use actual V7 PBR stone
# only for broad wall faces/base. Material assignment is original provenance
# plus already archive-approved V7 PBR; no flat fake stone fills.
new_objects=[]
placements=[]
for original_name,label,cx,cy,zmin,mirror in plan:
 world,lo,hi,donor=source_geometry[original_name]
 center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
 obj=donor.copy();obj.data=donor.data.copy()
 bpy.context.scene.collection.objects.link(obj)
 obj.name='ENTRY_V12_REUSED_WALLFAMILY_'+label
 newmesh=[]
 for p in world:
  q=p-center
  if mirror:q.x=-q.x
  newmesh.append((q.x+cx,q.y+cy,q.z+zmin))
 for v,vec in zip(obj.data.vertices,newmesh):v.co=Vector(vec)
 obj.matrix_world=Matrix.Identity(4)
 obj.parent=None
 obj.hide_render=False;obj.hide_set(False)
 bm=bmesh.new();bm.from_mesh(obj.data)
 if mirror:bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 bm.to_mesh(obj.data);bm.free();obj.data.update()
 # Replace ALL unverified donor material slots. The only image-bearing
 # maps exported are the already packed, licensed V7 photographic materials.
 for idx,m in enumerate(obj.data.materials):
  label=m.name if m else ''
  if label.endswith('wall_cap'):obj.data.materials[idx]=v7trim
  elif label.endswith('wall_shadow'):obj.data.materials[idx]=v7rock
  else:obj.data.materials[idx]=roadstone
 assert all(m in (roadstone,v7trim,v7rock) for m in obj.data.materials)
 b=[v.co for v in obj.data.vertices]
 actual_min=Vector([min(p[i] for p in b) for i in range(3)])
 actual_max=Vector([max(p[i] for p in b) for i in range(3)])
 assert abs(actual_min.z-zmin)<.005
 assert min(abs(actual_min.x),abs(actual_max.x))>9.45,'Protected gate central aperture invaded'
 assert actual_max.z<7.2,'Real gate silhouette dominated by donor wall'
 placements.append({'donor':original_name,'name':obj.name,'mirrored_symmetric_copy':mirror,
  'world_min_xyz':list(actual_min),'world_max_xyz':list(actual_max),
  'triangles':sum(len(p.vertices)-2 for p in obj.data.polygons)})
 new_objects.append(obj)
# Do not export inspection-only original donors; they never enter the visible scene.
for o in pieces.values():bpy.data.objects.remove(o,do_unlink=True)
assert set(bpy.data.objects.keys())-source_scene_names==set(o.name for o in new_objects)
for image in bpy.data.images:
 if image.source=='FILE' and not image.packed_file:
  if os.path.isfile(bpy.path.abspath(image.filepath)):image.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=v7_terrain
blend=os.path.join(out,'ValoriaEntryZoneV12.blend')
glb=os.path.join(out,'ValoriaEntryZoneV12.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read();gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for node in gltf['nodes'] if 'mesh' in node for p in gltf['meshes'][node['mesh']]['primitives'])
assert triangles>342006 and triangles<370000,triangles
result={'stage':'V12 genuine selective existing WallFamily architectural extension',
 'art_status':'CANDIDATE_NOT_APPROVED','source_v7_blend_sha256':sha(source),
 'original_wall_family_sha256':sha(wall),'source_inspection':bounds_report,
 'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'source_triangles':342006,'donor_texture_nodes':donor_images,
 'reused_wall_meshes':placements,'existing_models_reused':True,
 'EastPartialWall_not_used':'Original source diagonal world bounds would not join the gate without unacceptable anisotropic deformation',
 'existing_v7_road_PBR_reused':True,'uncertified_donor_materials_discarded':True,
 'original_source_geometry_unchanged':True,'gameplay_colliders_unchanged':True,
 'license_scope':'Existing project-authored WallFamily GEOMETRY only; every imported donor texture/material discarded; packed verified V7 PBR and gate trim only',
 'payment_eur':0,'physical_mobile_tested':False,'full_frame_art_pass':False}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(result,indent=2)+'\n')
print('VALORIA_ENTRY_V12_SOURCE_COMPLETE',json.dumps(result))
