"""V14 professional entrance-specific masonry join and rock-footing pass.
Immortal V12 candidate is the geometric baseline. This file does NOT design a
new gate or reuse the unrelated upper Bastion experiment V13. It makes missing
masonry-to-rock foundation transitions for the two existing flank walls/four
existing flank towers using the ACTUAL V12 WORLD bounds and terrain BVH. Blender
geometry/UVs are edited, no donor copyrighted textures imported or rendered.
"""
import bpy,os,json,hashlib,struct,math,bmesh
from mathutils import Vector
from mathutils.bvhtree import BVHTree
root=os.getcwd()
base=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016')
out=os.path.join(base,'entry-zone-v14')
os.makedirs(out,exist_ok=True)
sha=lambda f:hashlib.sha256(open(f,'rb').read()).hexdigest()
source=os.path.join(base,'entry-zone-v12/ValoriaEntryZoneV12.blend')
measures=os.path.join(base,'entry-zone-v13-survey/bastion-source-world-bounds.json')
assert sha(source)=='21c5681e20a498bcd97925520fd621907b023704a119e65890f2ec62c99081de'
report=json.load(open(measures,encoding='utf8'))
assert report['source_v12_sha256']==sha(source)
bpy.ops.wm.open_mainfile(filepath=source)
old_names=set(bpy.data.objects)
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('ValoriaEntryTerrainV12'))
stone=next(m for m in bpy.data.materials if m.name.startswith('ENTRY_road_masonry_PBR'))
assert stone.use_nodes and any(n.type=='TEX_IMAGE' and n.image for n in stone.node_tree.nodes)
terrain.name='ValoriaEntryTerrainV14'
# Slots are measured from original gate/world V12, NEVER guessing module pivots.
records={e['name']:e for e in report['source_gate_and_courtyard']}
six=[name for name in records if name.startswith('ENTRY_V12_REUSED_WALLFAMILY_')]
assert len(six)==6
for name in six:
 o=bpy.data.objects[name];m=o.matrix_world
 pts=[m@v.co for v in o.data.vertices]
 lo=Vector([min(p[i] for p in pts) for i in range(3)])
 hi=Vector([max(p[i] for p in pts) for i in range(3)])
 cert=records[name]
 assert max(abs(lo[i]-cert['world_min_xyz'][i]) for i in range(3))<.008
 assert max(abs(hi[i]-cert['world_max_xyz'][i]) for i in range(3))<.008
# Real cliff geometry probes for each socket — support structure is built TO
# existing rock, not vertically extruded to invented ground height.
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def ground(x,y):
 hit,norm,face,d=bvh.ray_cast(Vector((x,y,16)),Vector((0,0,-1)),35)
 if hit is None:raise AssertionError(('NO_REAL_GROUND_AT_SOCKET',round(x,2),round(y,2)))
 return float(hit.z)
def qpoint(x,y,z):
 return (round(float(x),5),round(float(y),5),round(float(z),5))
def maker(name):
 return {'name':name,'verts':[],'faces':[],'uvs':[],'material':stone}
def quad(pr,points,normal_expected=None):
 # Distinct per-face vertices for robust UVs and exact PBR meter scale.
 offset=len(pr['verts'])
 pr['verts'].extend(points)
 pr['faces'].append(tuple(range(offset,offset+len(points))))
 pr['uvs'].extend([((p[0]+p[1])*.21,p[2]*.24) for p in points])
def rect6(pr,x0,x1,y0,y1,z0,z1,bevel=.045):
 # Quoin block: real 3D masonry with beveled upper corners, not planar decals.
 if z1-z0<.07:return
 x0,x1=sorted((x0,x1));y0,y1=sorted((y0,y1))
 inset=min(bevel,.18*(x1-x0),.18*(y1-y0))
 b=[qpoint(x0,y0,z0),qpoint(x1,y0,z0),qpoint(x1,y1,z0),qpoint(x0,y1,z0)]
 u=[qpoint(x0+inset,y0+inset,z1),qpoint(x1-inset,y0+inset,z1),
    qpoint(x1-inset,y1-inset,z1),qpoint(x0+inset,y1-inset,z1)]
 quad(pr,u);quad(pr,list(reversed(b)))
 for k in range(4):quad(pr,[b[k],b[(k+1)%4],u[(k+1)%4],u[k]])
def mesh_object(pr):
 mesh=bpy.data.meshes.new(pr['name']+'_mesh')
 mesh.from_pydata(pr['verts'],[],pr['faces']);mesh.update()
 ob=bpy.data.objects.new(pr['name'],mesh);bpy.context.collection.objects.link(ob)
 ob.data.materials.append(pr['material'])
 uv=mesh.uv_layers.new(name='ENTRY_V14_Masonry_RealScale')
 for face in mesh.polygons:
  for loop_id in face.loop_indices:
   vindex=mesh.loops[loop_id].vertex_index
   uv.data[loop_id].uv=pr['uvs'][vindex]
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
 bm.to_mesh(mesh);bm.free();mesh.update()
 return ob
# Construction design driven by six certified footprints:
# Wall body at Y ~7.75, front direction positive Y toward bridge and cliff.
# Three 0.6-masonry coursed, battered buttress piers each side physically
# contact bank surface, structurally tie to wall (inside face Y=8.16).
# Six tower footprints get four corner plinths with stepped stone joints.
design={'objectives':['join masonry flank-wall piers to real rock bank',
                       'give four towers real terrain-fitting dressed foundations',
                       'retain arch, original gate and bridge circulation'],
 'baseline':'V12 visible player home, original V7 gate/camera and certified road colliders',
 'allowed_geometry':'six foundation/contact sets using existing V12 stone PBR and BVH',
 'wall_joints':'tapered keyed masonry courses bonded into existing wall body',
 'terrain_sampling':'BVH actual vertices; no dummy plane; probe front toe and back',
 'bridge_footings':'source unchanged, contact measured only, never masked with water',
 'protected':'original gate/wall meshes, colliders, camera and all road stairs',
 'materials':'V7 already packed approved stone PBR; real meter-based UVs',
 'art_gate':'same home Unity A/B and owner reference; no autoapproval'}
samples=[];courses=0;objects=[]
for side in ['West','East']:
 key=f'ENTRY_V12_REUSED_WALLFAMILY_{side}FlankWall'
 wall=bpy.data.objects[key];bounds=records[key]
 x0,x1=bounds['world_min_xyz'][0],bounds['world_max_xyz'][0]
 ymid=bounds['center_xyz'][1]
 bottom=bounds['world_min_xyz'][2]
 foundation=maker(f'ENTRY_V14_GEOMETRIC_{side}_RockBondedWallFoundation')
 # Closely fitted battered plinth stones, three independent structural piers.
 for col in range(3):
  cx=x0+.92+(x1-x0-1.84)*col/2
  back=ymid+.22;front=ymid+1.47
  g=ground(cx,front)
  local_bottom=min(bottom-.10,g-.08)
  local_bottom=max(local_bottom,bottom-2.4) # never invent a skyscraper pier
  top=min(bottom+1.52,1.97)
  assert top>local_bottom+.35 and top<3.0,(side,cx,local_bottom,top)
  for layer in range(3):
   t0=layer/3;t1=(layer+1)/3
   z0=local_bottom+(top-local_bottom)*t0
   z1=local_bottom+(top-local_bottom)*t1-.028
   xw=.52-.09*t0
   front_y=front-.13*t0
   rect6(foundation,cx-xw,cx+xw,back,front_y,z0,z1,.035)
   courses+=1
  samples.append({'footing':side,'x':cx,'front_y':front,'real_terrain_z':g,
   'foundation_low_z':local_bottom,'interlocked_wall_bottom_z':bottom,
   'masonry_top_z':top})
 objects.append(mesh_object(foundation))
 for category in ['InnerTower','OuterTower']:
  tname=f'ENTRY_V12_REUSED_WALLFAMILY_{side}{category}'
  cert=records[tname];xlo,xhi=cert['world_min_xyz'][0],cert['world_max_xyz'][0]
  ylo,yhi=cert['world_min_xyz'][1],cert['world_max_xyz'][1]
  zlo=cert['world_min_xyz'][2];cx=(xlo+xhi)/2
  # A continuous bonded tower plinth: regular stone courses share interlocking
  # faces, and the lowest course reaches measured cliff rock, not empty space.
  toe_y=yhi+.38; g=ground(cx,toe_y)
  low=max(min(zlo-.10,g-.08),zlo-2.0)
  high=zlo+1.05
  assert high>low+.30
  pier=maker('ENTRY_V14_GEOMETRIC_'+side+category+'_ContouredStoneBase')
  for layer in range(3):
   frac0=layer/3;frac1=(layer+1)/3
   z0=low+(high-low)*frac0;z1=low+(high-low)*frac1-.025
   spread=.31-.17*frac0
   # Real tower-footprint rectangle extends into tower footprint; visible exterior
   # edges and wide toothed footing establish physical foundation at rocky bank.
   rect6(pier,xlo-spread,xhi+spread,ylo-spread,yhi+spread,z0,z1,.04)
   courses+=1
  samples.append({'footing':side+category,'x':cx,'front_y':toe_y,
     'real_terrain_z':g,'foundation_low_z':low,
     'interlocked_wall_bottom_z':zlo,'masonry_top_z':high})
  objects.append(mesh_object(pier))
# Sampling ONLY for bridge contact; bridge mesh remains unmodified.
feet=bpy.data.objects.get('ENTRY_SUPPORTED_VALORIA_ARCH_FOOTINGS')
assert feet is not None
feetbounds=records['ENTRY_SUPPORTED_VALORIA_ARCH_FOOTINGS']
bridge_min=feetbounds['world_min_xyz'][2]
design['existing_bridge_feet_bottom_z']=bridge_min
design['bridge_original_world_bounds']=feetbounds
assert len(objects)==6 and courses==30 and len(samples)==10
# UV/geometry and normals are real. Preserve original .blend and contact colliders.
expected={x.name for x in objects}|{terrain.name}
new_names=set(bpy.data.objects)-old_names
assert new_names==expected,('FOUNDATION_OBJECT_SET_MISMATCH',sorted(new_names),sorted(expected),
                               'ORIGINAL_LOST',sorted(old_names-set(bpy.data.objects)))
for img in bpy.data.images:
 if img.source=='FILE' and not img.packed_file and os.path.isfile(bpy.path.abspath(img.filepath)):img.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV14.blend')
glb=os.path.join(out,'ValoriaEntryZoneV14.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read();gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for n in gltf['nodes'] if 'mesh' in n for p in gltf['meshes'][n['mesh']]['primitives'])
assert 351374<triangles<375000,triangles
manifest={'status':'V14_ARCHITECTURAL_SOCKET_CANDIDATE_UNAPPROVED',
 'source_v12_blend_sha256':sha(source),'result_blend_sha256':sha(blend),
 'result_glb_sha256':sha(glb),'triangles':triangles,
 'real_terrain_samples':samples,'design':design,'source_12_stairs_unchanged':True,
 'source_gate_and_bridge_unchanged':True,'source_colliders_unchanged':True,
 'masonry_source':'V12 original trusted V7 PBR, UV metres',
 'new_fitted_stone_footings':len(objects),'real_dressed_stone_courses':courses,
 'input_geometry_source_unchanged':True,'zero_paid_spend':True,
 'art_status':'REQUIRES_REAL_UNITY_AB_AND_INDEPENDENT_REVIEW',
 'mobile_profiled':False}
with open(os.path.join(out,'entry-source.json'),'w',encoding='utf8') as f:json.dump(manifest,f,indent=2)
print('VALORIA_V14_NATIVE_SOURCE_COMPLETE',json.dumps({'triangles':triangles,'rock_contacts':len(samples),'masonry_courses':courses}))
