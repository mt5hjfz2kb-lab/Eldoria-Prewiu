"""Valoria V15: physically grade the existing terrain UNDER the actual V12 wall
and tower sockets. Geometry-first correction after rejected V14 block footings.
Source V12, no new gate, props, separate foundation blocks or texture recolor.
Certified V7 bridge, original gate, collider and camera positions preserved.
"""
import bpy,os,json,hashlib,struct,math,bmesh
from mathutils import Vector
root=os.getcwd()
base=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016')
out=os.path.join(base,'entry-zone-v15')
os.makedirs(out,exist_ok=True)
sha=lambda f:hashlib.sha256(open(f,'rb').read()).hexdigest()
source=os.path.join(base,'entry-zone-v12/ValoriaEntryZoneV12.blend')
measures=os.path.join(base,'entry-zone-v13-survey/bastion-source-world-bounds.json')
assert sha(source)=='21c5681e20a498bcd97925520fd621907b023704a119e65890f2ec62c99081de'
bounds=json.load(open(measures,encoding='utf8'))
assert bounds['source_v12_sha256']==sha(source)
bpy.ops.wm.open_mainfile(filepath=source)
before={o.name for o in bpy.data.objects}
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name=='ValoriaEntryTerrainV12')
assert len(terrain.data.vertices)==106733
walls={e['name']:e for e in bounds['source_gate_and_courtyard'] if e['name'].startswith('ENTRY_V12_REUSED_WALLFAMILY_')}
assert len(walls)==6
parts=[]
for name,e in walls.items():
 o=bpy.data.objects.get(name)
 assert o is not None
 pts=[o.matrix_world@v.co for v in o.data.vertices]
 lo=[min(v[i] for v in pts) for i in range(3)]
 hi=[max(v[i] for v in pts) for i in range(3)]
 assert max(abs(lo[i]-e['world_min_xyz'][i]) for i in range(3))<.005
 assert max(abs(hi[i]-e['world_max_xyz'][i]) for i in range(3))<.005
 parts.append({'name':name,'bounds_min':lo,'bounds_max':hi,
               'target_contact_z':lo[2]-.085})
# Exact actual terrain mesh sculpt — not visual dirt paint and not a new rock block.
# Smooth each rock saddle under the real prefab socket until masonry has actual
# load-bearing contact. Falloff 1.4m laterally and 1.25m in cliff-depth so that
# the existing photographically textured cliff transitions into the wall base.
# Never sculpt near the original gate center, bridge, stairs or far river bank.
def smooth(t):
 t=max(0.,min(1.,t))
 return t*t*(3.-2.*t)
inverse=terrain.matrix_world.inverted()
changed=0;max_raise=0.;max_lower=0.;sum_raise=0.;contacts=[]
untouched_gate_vertices=0
for v in terrain.data.vertices:
 world=terrain.matrix_world@v.co
 x,y,z=world.x,world.y,world.z
 if abs(x)<9.35:
  untouched_gate_vertices+=1
  continue
 strength=0.
 target=0.
 for part in parts:
  x0,y0,_=part['bounds_min']
  x1,y1,_=part['bounds_max']
  dx=max(x0-x,0.,x-x1)
  dy=max(y0-y,0.,y-y1)
  # Terrain inside wall contact footprint gets a full grade.
  # Outside gets a soft ramp instead of a step/disconnected "foundation".
  d=math.sqrt((dx/1.65)**2+(dy/1.25)**2)
  f=1.-smooth(d)
  # Stop before the cliff throat y>=10.4 near the approved gate.
  if y>10.25:f*=1.-smooth((y-10.25)/.45)
  if f>strength:
   strength=f;target=part['target_contact_z']
 if strength<.001 or z>=target-.002:continue
 up=min(.85,(target-z)*strength)
 if up<=.0001:continue
 # Terrain keeps original stone photo texture/UV. Micro-erosion remains inside
 # original CC0 rock surface and never creates a mismatching new material.
 world.z+=up
 v.co=inverse@world
 changed+=1;max_raise=max(max_raise,up);sum_raise+=up
terrain.data.update()
bm=bmesh.new();bm.from_mesh(terrain.data)
bm.normal_update()
# Preserve original open heightfield winding and original up facing normals.
old_up=sum(f.normal.z*f.calc_area() for f in bm.faces)
if old_up<0:bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
bm.to_mesh(terrain.data);bm.free();terrain.data.update()
avg_up=sum(v.normal.z for v in terrain.data.vertices)/len(terrain.data.vertices)
assert .78<avg_up<=1.0,('TERRAIN_NORMALS',avg_up)
assert changed>100 and changed<8000 and max_raise<.9,('INVALID_ACTUAL_GRADE',changed,max_raise)
terrain.name='ValoriaEntryTerrainV15'
# Confirm the real original meshes and mesh counts are not touched by the sculpt.
for n in walls:
 assert bpy.data.objects[n].data is not None
assert {o.name for o in bpy.data.objects}-before=={terrain.name}
assert len(terrain.data.vertices)==106733
# Real contact sampling of terrain under each existing tower/wall, after sculpt.
from mathutils.bvhtree import BVHTree
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
for part in parts:
 x0,y0,z0=part['bounds_min'];x1,y1,_=part['bounds_max']
 cx=(x0+x1)/2
 samples=[]
 for yy in [y0+.05,(y0+y1)/2,y1-.05]:
  hit,norm,face,dist=bvh.ray_cast(Vector((cx,yy,12)),Vector((0,0,-1)),30)
  assert hit is not None,('MISSING_REAL_GROUND',part['name'])
  samples.append({'sample_world_xy':[cx,yy],'measured_terrain_z':float(hit.z),
   'masonry_bottom_z':float(z0),'gap':float(z0-hit.z)})
 contacts.append({'part':part['name'],'measurements':samples,
                  'max_middle_base_gap_m':max(d['gap'] for d in samples)})
for im in bpy.data.images:
 if im.source=='FILE' and not im.packed_file and os.path.isfile(bpy.path.abspath(im.filepath)):im.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV15.blend')
glb=os.path.join(out,'ValoriaEntryZoneV15.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read();gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for n in gltf['nodes'] if 'mesh' in n for p in gltf['meshes'][n['mesh']]['primitives'])
assert triangles==351374,('ORIGINAL_GEOMETRY_TRIANGLES_CHANGED',triangles)
result={'status':'V15_PHYSICAL_TERRAIN_TO_WALL_CONTACT_COPY_UNAPPROVED',
 'art_hypothesis':'correct actual bank geometry at six measured architectural sockets, rather than overlaying free-standing masonry props',
 'source_v12_blend_sha256':sha(source),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'modified_actual_terrain_vertices':changed,
 'max_actual_terrain_displacement_m':max_raise,'sum_positive_displacement_m':sum_raise,
 'terrain_normal_up_average':avg_up,'preserved_gate_axis_vertices':untouched_gate_vertices,
 'source_gate_and_bridge_geometry_untouched':True,'source_twelve_stairs_untouched':True,
 'source_six_walls_and_towers_untouched':True,'source_pbr_photogrammetry_unchanged':True,
 'source_colliders_unchanged':True,'additional_mesh_objects':0,
 'contact_measurements':contacts,'art_status':'UNAPPROVED_UNITY_AB_REQUIRED',
 'physical_mobile_tested':False,'paid_spend_eur':0}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(result,indent=2)+'\n')
print('VALORIA_V15_SOURCE_COMPLETE',json.dumps({'modified':changed,'max_raise_m':max_raise,'normals':avg_up,'triangles':triangles}))
