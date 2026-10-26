"""Erode the physical sawtooth bank surface revealed by native40250944622.
Use the connected soil as a continuous broad bank; retain the deep rock incision.
"""
import bpy,bmesh,numpy as np,pathlib,json,hashlib,struct,gzip,math
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source']);scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain'];mesh=terrain.data;old=np.array([tuple(v.co) for v in mesh.vertices],np.float32);protected=(abs(old[:,0])<=96.001)&(old[:,1]>=-78.001)&(old[:,1]<=270.001)
images={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
raw=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();names={n.get('name') for n in json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])['nodes']}
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'));reserves=[r for r in layout['city_reserves'] if -540<r[0]<-30 and -90<r[1]<390]
xs=np.arange(-494.,-73.,2.);ys=np.arange(-44.,345.,2.);values=np.zeros((len(ys),len(xs)),float);weights=np.zeros_like(values)
for j,y in enumerate(ys):
 for i,x in enumerate(xs):
  hit=bvh.ray_cast(Vector((x,y,400)),Vector((0,0,-1)))
  if hit[0] is None:continue
  index=hit[2];rock=mesh.materials[mesh.polygons[index].material_index].name.startswith('Breach fractured basalt')
  if not rock:values[j,i]=hit[0].z;weights[j,i]=1
def blur(a):
 p=np.pad(a,1,mode='edge')
 return (p[:-2,:-2]+2*p[:-2,1:-1]+p[:-2,2:]+2*p[1:-1,:-2]+4*p[1:-1,1:-1]+2*p[1:-1,2:]+p[2:,:-2]+2*p[2:,1:-1]+p[2:,2:])/16
numerator=values.copy();denominator=weights.copy()
for unused in range(100):numerator=blur(numerator);denominator=blur(denominator)
assert denominator.min()>.00001
surface=numerator/denominator
assert surface.min()>-8 and surface.max()<49
def sample(a,x,y):
 u=max(0,min(a.shape[1]-1,(x-xs[0])/2));v=max(0,min(a.shape[0]-1,(y-ys[0])/2));i=int(u);j=int(v);ii=min(a.shape[1]-1,i+1);jj=min(a.shape[0]-1,j+1);tx=u-i;ty=v-j
 return (a[j,i]*(1-tx)+a[j,ii]*tx)*(1-ty)+(a[jj,i]*(1-tx)+a[jj,ii]*tx)*ty
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
water_trees=[]
for name in ['BLD_RiverExtension_1','BLD_RiverExtension_-1','Pilot_River']:
 ob=bpy.data.objects.get(name)
 if not ob:continue
 transform=terrain.matrix_world.inverted()@ob.matrix_world
 points=[transform@v.co for v in ob.data.vertices]
 water_trees.append(BVHTree.FromPolygons([(v.x,v.y,0) for v in points],[list(p.vertices) for p in ob.data.polygons]))
assert len(water_trees)==3
soil_ids=set()
for face in mesh.polygons:
 if mesh.materials[face.material_index].name=='Region1 open meadows':
  soil_ids.update(i for i in face.vertices if -470<old[i,0]<-98 and -20<old[i,1]<320)
changed=0;maxdelta=0
for i,v in enumerate(mesh.vertices):
 x,y,z=old[i]
 if not(-470<x<-98 and -20<y<320):continue
 fade=smooth(min(x+470,-98-x,y+20,320-y)/24)
 for cx,cy,radius in reserves:fade*=smooth((math.hypot(x-cx,y-cy)-radius-4)/20)
 distance_to_water=min(tree.find_nearest(Vector((float(x),float(y),0)))[3] for tree in water_trees)
 fade*=smooth((distance_to_water-12)/22)
 top=sample(surface,x,y);target=top if i in soil_ids else min(float(z),top)
 newz=float(z)+(target-float(z))*fade
 maxdelta=max(maxdelta,abs(newz-float(z)));changed+=abs(newz-float(z))>.0001;v.co.z=newz
assert maxdelta>1 and changed>1000
# Smooth actual shared terrain normals locally; the preserved source normals stay.
normals=[tuple(n.vector) for n in mesh.corner_normals]
for face in mesh.polygons:
 if all(-470.001<mesh.vertices[i].co.x<-97.999 and -20.001<mesh.vertices[i].co.y<320.001 for i in face.vertices):
  face.use_smooth=True
  for li in face.loop_indices:normals[li]=(0,0,0)
mesh.update();mesh.normals_split_custom_set(normals);mesh.update();bpy.context.view_layer.update()
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def soil(x,y):
 q=bvh.ray_cast(Vector((float(x),float(y),400)),Vector((0,0,-1)))[0];assert q is not None,(x,y);return q.z
# Every pigment exposure follows the same actual highest connected surface.
for name,offset in [('BLD_BreachFloor',.015),('BLD_BreachOutlyingFractures',.015),('BLD_BreachBrokenBasaltPlates',.026),('BLD_BreachTerracedStrata',.026),('BLD_BreachWeatheredRockfall',.026)]:
 m=bpy.data.objects[name].data
 for v in m.vertices:v.co.z=soil(v.co.x,v.co.y)+offset
 # The masks were exported with separate vertices per polygon. Weld coincident
 # pigment vertices so smooth normals genuinely interpolate between triangles.
 bm=bmesh.new();bm.from_mesh(m);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00005);bm.normal_update();bm.to_mesh(m);bm.free()
 for face in m.polygons:face.use_smooth=True
 m.update();m.normals_split_custom_set([(0,0,0)]*len(m.loops));m.update()
for v in bpy.data.objects['BLD_BreachMineralHeart'].data.vertices:v.co.z=soil(v.co.x,v.co.y)+.034
for i in range(3):
 a=bpy.data.objects['BLD_BreachInteriorLightAnchor_'+str(i)];a.location.z=soil(a.location.x,a.location.y)+1.4
 lamp=bpy.data.objects.get('ArtistBreachInterior_'+str(i))
 if lamp:lamp.location=a.location
grounded=0
for ob in list(scene.objects):
 if ob.name not in names or not ob.name.startswith('BLD_ForestMass_'):continue
 x,y=ob.location.x,ob.location.y
 if not(-470<x<-98 and -20<y<320):continue
 delta=soil(x,y)-ob.location.z;ob.location.z+=delta
 far=bpy.data.objects.get(ob.name.replace('BLD_ForestMass_','BLD_ForestFar_',1))
 if far:far.location.z+=delta
 grounded+=1
coords=np.array([tuple(v.co) for v in mesh.vertices],np.float32);assert np.array_equal(coords[protected],old[protected]);assert np.array_equal(coords[:,:2],old[:,:2])
assert abs(coords[:,2].min()-old[:,2].min())<.00001 and abs(coords[:,2].max()-old[:,2].max())<.00001
q=gzip.decompress((source.parent/'RegionOneBlenderQueries.bytes').read_bytes());magic,nx,nz,width,depth=struct.unpack_from('<iiiff',q);assert width==1820 and depth==3600
N=nx*nz;heights=np.frombuffer(q,dtype='<f4',count=N,offset=20).reshape(nz,nx).copy();masks=q[20+N*4:20+N*8]
for j in range(nz):
 y=-1800+j*3600/(nz-1)
 if not(-20<y<320):continue
 for i in range(nx):
  x=-910+i*1820/(nx-1)
  if -470<x<-98:heights[j,i]=soil(x,y)
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as f:
 f.write(struct.pack('<iiiff',magic,nx,nz,width,depth));f.write(heights.astype('<f4').tobytes());f.write(masks);f.write(struct.pack('<ii',0x32564c45,len(coords)));f.write(coords[:,[0,2,1]].astype('<f4').tobytes())
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'));layout['living_geography']['physically_eroded_continuous_canyon_banks']=True
(out/'production-region.json').write_text(json.dumps(layout,indent=2))
assert all(hashlib.sha256(bpy.data.images[k].packed_file.data).hexdigest()==v for k,v in images.items())
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True);bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report=json.loads((source.parent/'macro-art-operations.json').read_text(encoding='utf-8-sig'));report['eroded_banks_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();report['physical_sawtooth_bank_correction']={'changed_unprotected_vertices':changed,'max_vertical_correction_m':maxdelta,'crown_pairs_regrounded':grounded,'protected_positions_exact':True,'height_bounds_exact':True,'soil_mask_coincident_vertices_welded':True,'rejected_normals_only_candidate_native':40250944622}
report['files']={name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']};report['blend_bytes']=(out/'Eldoria_Region1_Production.blend').stat().st_size
assert report['blend_bytes']<100*1024*1024
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers']);exec(compile((helpers/'artist_preview.py').read_text(),str(helpers/'artist_preview.py'),'exec'))
text=(helpers.parent/'sculpt_living_geography.py').read_text();preview=text.split("bpy.app.driver_namespace['artist_keep_interior_lights']=True",1)[1];bpy.app.driver_namespace['artist_keep_interior_lights']=True
exec(compile(preview.replace('living-geography-candidate','eroded-continuous-banks'),str(helpers.parent/'sculpt_living_geography.py')+'-preview','exec'))
(out/'stop.request').write_text('Physical bank correction saved; quick Blender visual review required before native certification.')
