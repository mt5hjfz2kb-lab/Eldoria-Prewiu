"""Player-view correction directed from the original three references.
Reuse the inspected retained world. Change the canyon's actual wall silhouette,
retain geography, gameplay exclusions, source images, city datums and UVs.
"""
import bpy, pathlib, json, hashlib, struct, gzip, shutil, math
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
assert hashlib.sha256(source.read_bytes()).hexdigest()=='fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce'
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
exec(compile((helpers/'artist_preview.py').read_text(),'artist_preview.py','exec'))
terrain=bpy.data.objects['BLD_Terrain'];scene=bpy.context.scene
raw_glb=(source.parent/'RegionOneBlenderWorld.glb').read_bytes()
export_names={n.get('name') for n in json.loads(raw_glb[20:20+struct.unpack_from('<I',raw_glb,12)[0]])['nodes']}
raw=gzip.decompress((source.parent/'RegionOneBlenderQueries.bytes').read_bytes())
magic,nx,nz,_,_=struct.unpack_from('<iiiff',raw);assert magic==0x31424c45
count=nx*nz;heights=np.frombuffer(raw,dtype='<f4',count=count,offset=20).reshape(nz,nx).copy()
masks=raw[20+count*4:20+count*8]
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'))
original_images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
coords=np.array([tuple(v.co) for v in terrain.data.vertices],np.float32);before=coords.copy()
# The core includes the settlement and original banks; it is never sculpted.
protected=(abs(coords[:,0])<=96.001)&(coords[:,1]>=-78.001)&(coords[:,1]<=270.001)
wall_vertices=set()
for p in terrain.data.polygons:
 if 'Breach fractured basalt' in terrain.data.materials[p.material_index].name:wall_vertices.update(p.vertices)
assert len(wall_vertices)>1000
bridge_vertices=set()
for face in terrain.data.polygons:
 if 'Bridge' in terrain.data.materials[face.material_index].name:
  bridge_vertices.update(i for i in face.vertices if -1<float(coords[i,0])<8)
assert len(bridge_vertices)>50
# The original meadow samples remain frozen. The owner explicitly requests
# correction of the bridge; its joined deck vertices are the narrow exception.
protected[list(bridge_vertices)]=False
normals=[tuple(n.vector) for n in terrain.data.corner_normals]
# Macro correction: the reference is a shattered depression, not a uniform
# motorway-like Y trench. Keep its existing excluded axis but taper depth away
# from the centre and fracture the central ground into irregular fitted plates.
old_grid=heights.copy()
def sample_old(x,y):
 u=np.clip((x+910)/1820*(nx-1),0,nx-1);v=np.clip((y+1800)/3600*(nz-1),0,nz-1)
 i=int(u);j=int(v);k=min(nx-1,i+1);l=min(nz-1,j+1);tx=u-i;ty=v-j
 return float((old_grid[j,i]*(1-tx)+old_grid[j,k]*tx)*(1-ty)+(old_grid[l,i]*(1-tx)+old_grid[l,k]*tx)*ty)
segments=layout['faults']
def nearest_fault(x,y):
 result=None
 for seg in segments:
  ax,ay,bx,by=seg['ax'],seg['az'],seg['bx'],seg['bz'];dx=bx-ax;dy=by-ay;length=math.hypot(dx,dy)
  t=max(0,min(1,((x-ax)*dx+(y-ay)*dy)/(length*length)));px=ax+t*dx;py=ay+t*dy;distance=math.hypot(x-px,y-py)
  if result is None or distance<result[0]:result=(distance,px,py,-dy/length,dx/length)
 return result
# Hand-directed ring of broken geological plates; deterministic irregular cells.
seeds=[]
for row in range(-6,7):
 for col in range(-6,7):
  if row*row+col*col>44:continue
  seeds.append((-260+col*20+5*math.sin(row*4.3+col*1.7),150+row*20+5*math.sin(col*3.7-row*2.1)))
seeds=np.asarray(seeds)
changed=set()
for i,(x,y,z) in enumerate(before):
 x,y,z=float(x),float(y),float(z)
 if protected[i] or i in bridge_vertices:continue
 r=math.hypot(x+260,y-150)
 if not(-430<x<-110 and 15<y<290):continue
 distance,px,py,normal_x,normal_y=nearest_fault(x,y)
 if distance>24 and r>125:continue
 bank=max(sample_old(px+normal_x*28,py+normal_y*28),sample_old(px-normal_x*28,py-normal_y*28))
 if i in wall_vertices:
  depth=max(0,bank-z);factor=.16+.84*math.exp(-(r/59)**2)
  z=bank-depth*factor
 if r<125:
  ds=np.sort(np.linalg.norm(seeds-np.asarray((x,y)),axis=1))
  edge=ds[1]-ds[0]
  radial=max(0,min(1,(125-r)/35))
  # Plate tops remain the same ground; fractures are actual shallow cuts.
  crack=max(0,1-edge/2.7)**1.4
  z-=crack*2.2*radial
  z+=.22*math.sin(x*.21+y*.17)*radial*(1-crack)
  # One central wound, tapering to its connected branches.
  z=min(z,bank-14*math.exp(-(r/18)**2))
 coords[i,2]=z
 if abs(z-float(before[i,2]))>.001:changed.add(i)
assert len(changed)>1000
# Short crossing between the measured real banks (-29 and -8), rather than
# 34 metres of timber reaching all the way into the city. Keep the same axis.
land_faces=[list(p.vertices) for p in terrain.data.polygons if 'Bridge' not in terrain.data.materials[p.material_index].name]
land_bvh=BVHTree.FromPolygons(coords.tolist(),land_faces,all_triangles=False)
def land(x,y):
 hit=land_bvh.ray_cast(Vector((float(x),float(y),300)),Vector((0,0,-1)))[0]
 assert hit is not None,('Bridge requires real bank support',x,y)
 return hit.z
start_y,end_y=-29.,-8.;old_centre=-18.922370433807373;old_length=34.
start_z=max(land(x,start_y) for x in (-.7,3.3,7.3))+.18
end_z=max(land(x,end_y) for x in (-.7,3.3,7.3))+.18
def deck(y):
 t=max(0,min(1,(y-start_y)/(end_y-start_y)))
 return start_z*(1-t)+end_z*t+.25*math.sin(t*math.pi)
def mapped_y(y):return (y-old_centre)/old_length*(end_y-start_y)+(start_y+end_y)/2
old_deck={round(float(before[i,1]),4):float(before[i,2]) for i in bridge_vertices}
def old_height_at(y):
 keys=sorted(old_deck);return float(np.interp(y,keys,[old_deck[k] for k in keys]))
for i in bridge_vertices:
 coords[i,1]=mapped_y(float(before[i,1]));coords[i,2]=deck(float(coords[i,1]))
rails=bpy.data.objects['BLD_CrossingRails_0.0']
for vertex in rails.data.vertices:
 old_y=float(vertex.co.y);relative_z=max(0,float(vertex.co.z)-old_height_at(old_y))
 vertex.co.y=mapped_y(old_y);vertex.co.z=deck(vertex.co.y)+relative_z
rails.data.update()
for bridge in layout['bridges']:
 if abs(bridge['x']-3.3)<.01:bridge['z']=(start_y+end_y)/2;bridge['length']=end_y-start_y
terrain.data.vertices.foreach_set('co',coords.ravel());terrain.data.update()
# Preserve protected custom normals; update only the sculpted wall corners.
for p in terrain.data.polygons:
 if any(i in changed for i in p.vertices):
  for loop in p.loop_indices:normals[loop]=tuple(p.normal)
rock_slot=next(i for i,m in enumerate(terrain.data.materials) if 'Breach fractured basalt' in m.name)
for face in terrain.data.polygons:
 if any(protected[i] for i in face.vertices):continue
 centre=face.center;r=math.hypot(centre.x+260,centre.y-150)
 angle=math.atan2(centre.y-150,centre.x+260)
 if r<92+10*math.sin(angle*3)+7*math.sin(angle*7) and any(i in changed for i in face.vertices):face.material_index=rock_slot
terrain.data.normals_split_custom_set(normals);terrain.data.update();bpy.context.view_layer.update()
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def ground(x,y):
 hit=bvh.ray_cast(Vector((float(x),float(y),300)),Vector((0,0,-1)))[0]
 assert hit is not None,(x,y)
 return hit.z
# Carry existing rooted scenery vertically with the same physical surface.
# No tree is added or relocated in plan; linked near/far donors stay shared.
old_bvh=BVHTree.FromPolygons(before.tolist(),[list(p.vertices) for p in terrain.data.polygons],all_triangles=False)
rooted_adjustments=0
for ob in scene.objects:
 if ob.name not in export_names or not ob.name.startswith(('BLD_ForestMass_','BLD_ForestFar_','Pilot_Pine','Far_Pilot_Pine','Bank forest stone')):continue
 x,y=ob.location.x,ob.location.y
 if not(-430<x<-110 and 15<y<290):continue
 old_hit=old_bvh.ray_cast(Vector((x,y,300)),Vector((0,0,-1)))[0]
 new_hit=bvh.ray_cast(Vector((x,y,300)),Vector((0,0,-1)))[0]
 if old_hit is not None and new_hit is not None:
  delta=new_hit.z-old_hit.z;ob.location.z+=delta
  rooted_adjustments+=abs(delta)>.001
# Every retained decorative overlay follows the real cut; no floating plates.
overlay_counts={}
for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures','BLD_BreachBrokenBasaltPlates','BLD_BreachTerracedStrata','BLD_BreachWeatheredRockfall','BLD_BreachMineralHeart']:
 ob=bpy.data.objects.get(name)
 if not ob:continue
 for v in ob.data.vertices:v.co.z=ground(v.co.x,v.co.y)+(.015 if name=='BLD_BreachFloor' else .002)
 ob.data.update()
 if name!='BLD_BreachMineralHeart':
  for face in ob.data.polygons:
   q=face.center;hit=bvh.ray_cast(Vector((q.x,q.y,300)),Vector((0,0,-1)))
   if hit[0] is not None:
    material=terrain.data.materials[terrain.data.polygons[hit[2]].material_index]
    if material not in list(ob.data.materials):ob.data.materials.append(material)
    face.material_index=list(ob.data.materials).index(material)
 overlay_counts[name]=len(ob.data.vertices)
for ob in scene.objects:
 if ob.name.startswith('BLD_BreachInteriorLightAnchor_'):ob.location.z=ground(ob.location.x,ob.location.y)+1.4
# Only changed canyon samples are re-queried; all gameplay masks are exact bytes.
for j,y in enumerate(np.linspace(-1800,1800,nz)):
 if not(-40<y<295):continue
 for i,x in enumerate(np.linspace(-910,910,nx)):
  if (10<y<295 and -435<x<-105) or (-40<y<0 and -.7<x<7.3):heights[j,i]=ground(x,y)
assert np.array_equal(coords[protected],before[protected])
assert all(hashlib.sha256(bpy.data.images[n].packed_file.data).hexdigest()==h for n,h in original_images.items())
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as f:
 f.write(struct.pack('<iiiff',magic,nx,nz,1820,3600));f.write(heights.astype('<f4').tobytes());f.write(masks);f.write(struct.pack('<ii',0x32564c45,len(coords)));f.write(coords[:,[0,2,1]].astype('<f4').tobytes())
layout['player_art_correction']={'bridge_length_before':34,'bridge_length_after':21,'actual_stepped_wall_vertices':len(changed),'unchanged_gameplay_masks':True,'protected_core_vertices':int(protected.sum())}
(out/'production-region.json').write_text(json.dumps(layout,indent=2))
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in export_names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_vertices_after':len(coords),'bridge_length_before':34,'bridge_length_after':21,'actual_stepped_wall_vertices':len(changed),'protected_vertices_unchanged':int(protected.sum()),'original_packed_images_unchanged':len(original_images),'overlay_vertices_projected':overlay_counts,'gameplay_masks_sha256':hashlib.sha256(masks).hexdigest(),'new_gameplay_targets':0,'new_colliders':0,'existing_rooted_scenery_fitted':rooted_adjustments,'art_approved':False,'files':{n:hashlib.sha256((out/n).read_bytes()).hexdigest() for n in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']}}
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
bpy.app.driver_namespace['interactive_snapshot']('player-art-fractured-basin-v3')
(out/'stop.request').write_text('Saved physical wall correction; actual Unity review required.')
print('PLAYER_ART_SCULPT_COMPLETE',json.dumps(report))
