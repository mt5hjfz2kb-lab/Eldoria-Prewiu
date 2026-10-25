"""Artist-directed valleys, riparian composition and fractured Breach on certified source.
Executed through the existing, verified BlenderMCP session. Never edits AF/AG source faces.
"""
import bpy, pathlib, json, hashlib, math, struct, gzip
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree

out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain']
gltf_bytes=(source.parent/'RegionOneBlenderWorld.glb').read_bytes()
json_size=struct.unpack_from('<I',gltf_bytes,12)[0]
names={node.get('name') for node in json.loads(gltf_bytes[20:20+json_size])['nodes']}
exported=[ob for ob in scene.objects if ob.name in names and ob.type in {'MESH','EMPTY'}]
assert terrain in exported
coords=np.empty(len(terrain.data.vertices)*3,np.float32);terrain.data.vertices.foreach_get('co',coords)
coords=coords.reshape(-1,3)
protected=(np.abs(coords[:,0])<=96.001)&(coords[:,1]>=-78.001)&(coords[:,1]<=270.001)
protected_before=coords[protected].copy()
images_before={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
query_path=source.parent/'RegionOneBlenderQueries.bytes'
layout_path=source.parent/'production-region.json'
assert query_path.exists() and layout_path.exists(),str(source.parent)
raw=gzip.decompress(query_path.read_bytes())
magic,nx,nz,width,depth=struct.unpack_from('<iiiff',raw)
assert magic==0x31424c45
count=nx*nz
oldheight=np.frombuffer(raw,dtype='<f4',count=count,offset=20).reshape(nz,nx).copy()
masks=np.frombuffer(raw,dtype=np.uint8,count=count*4,offset=20+count*4).reshape(nz,nx,4).copy()
layout=json.loads(layout_path.read_text(encoding='utf-8-sig'))
reserves=layout['city_reserves']
def smooth(x):
 x=np.clip(x,0,1);return x*x*(3-2*x)
def sample(field,x,y):
 xx=np.clip((np.asarray(x)+910)/1820*(nx-1),0,nx-1);yy=np.clip((np.asarray(y)+1800)/3600*(nz-1),0,nz-1)
 x0=xx.astype(int);y0=yy.astype(int);x1=np.minimum(nx-1,x0+1);y1=np.minimum(nz-1,y0+1)
 tx=xx-x0;ty=yy-y0
 return (field[y0,x0]*(1-tx)+field[y0,x1]*tx)*(1-ty)+(field[y1,x0]*(1-tx)+field[y1,x1]*tx)*ty
def main_water(x):
 i=int(round((x+910)/1820*(nx-1)));ys=np.linspace(-1800,1800,nz)
 candidates=ys[(masks[:,i,1]>128)&(np.abs(ys)<130)]
 assert len(candidates)>0
 return float(np.mean(candidates))
def curve(points,spacing=9):
 result=[]
 for i in range(len(points)-1):
  p0=np.array(points[max(0,i-1)],float);p1=np.array(points[i],float);p2=np.array(points[i+1],float);p3=np.array(points[min(len(points)-1,i+2)],float)
  steps=max(3,int(np.linalg.norm(p2-p1)/spacing))
  for t in np.linspace(0,1,steps,endpoint=False):
   result.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t))
 result.append(np.array(points[-1],float));return np.array(result)
# Two deliberate S-shaped drainage valleys, with broad small oxbows and dry
# gravel fords where EXISTING roads cross. Existing city reserves stay dry.
north=[(240,1630),(105,1450),(230,1260),(65,1060),(220,905),(40,715),(145,565),(265,390),(300,200),(300,main_water(300))]
south=[(-100,-1640),(-210,-1450),(-110,-1250),(-275,-1050),(-145,-860),(-290,-650),(-210,-470),(-420,-305),(-375,-140),(-300,-65),(-250,main_water(-250))]
streams=[]
for index,path in enumerate([north,south]):
 points=curve(path)
 for cx,cy,rr in reserves:
  delta=points-np.array([cx,cy]);distance=np.linalg.norm(delta,axis=1);shift=np.maximum(0,rr+55-distance)
  points+=delta/np.maximum(distance[:,None],.001)*shift[:,None]
 lengths=np.r_[0,np.cumsum(np.linalg.norm(np.diff(points,axis=0),axis=1))]
 t=lengths/lengths[-1]
 downstream=float(sample(oldheight,*points[-1]))+.45
 upstream=max(downstream+2.0,float(sample(oldheight,*points[0]))-.65)
 water=upstream*(1-t)+downstream*t
 radius=5.0+2.2*np.sin(np.pi*t)**.6
 radius+=(25 if index==0 else 18)*np.exp(-((t-(.23 if index==0 else .13))/.038)**2)
 road=sample(masks[:,:,2].astype(float)/255,points[:,0],points[:,1])>.10
 ford=np.zeros(len(points),bool)
 for k in np.where(road)[0]:ford|=np.abs(lengths-lengths[k])<24
 streams.append(dict(points=points,lengths=lengths,water=water,radius=radius,ford=ford))

def valley_field(x,y):
 x,y=np.broadcast_arrays(np.asarray(x),np.asarray(y));best=np.full(x.shape,1e9);level=np.zeros(x.shape);radius=np.zeros(x.shape);ford=np.zeros(x.shape,bool)
 for stream in streams:
  p=stream['points']
  for k,(a,b) in enumerate(zip(p,p[1:])):
   r=max(stream['radius'][k:k+2])+40
   chosen=(x>=min(a[0],b[0])-r)&(x<=max(a[0],b[0])+r)&(y>=min(a[1],b[1])-r)&(y<=max(a[1],b[1])+r)
   if not np.any(chosen):continue
   dx,dy=b-a;den=dx*dx+dy*dy
   t=np.clip(((x[chosen]-a[0])*dx+(y[chosen]-a[1])*dy)/den,0,1)
   dist=np.hypot(x[chosen]-a[0]-t*dx,y[chosen]-a[1]-t*dy)
   prior=best[chosen];replace=dist<prior
   prior[replace]=dist[replace];best[chosen]=prior
   w=level[chosen];w[replace]=(stream['water'][k]*(1-t)+stream['water'][k+1]*t)[replace];level[chosen]=w
   w=radius[chosen];w[replace]=(stream['radius'][k]*(1-t)+stream['radius'][k+1]*t)[replace];radius[chosen]=w
   w=ford[chosen];w[replace]=bool(stream['ford'][k] or stream['ford'][k+1]);ford[chosen]=w
 return best,level,radius,ford
def protect_weight(x,y):
 d=np.hypot(np.maximum(0,np.abs(x)-96),np.maximum(0,np.maximum(-78-y,y-270)))
 weight=smooth(d/45)*smooth(np.minimum(910-np.abs(x),1800-np.abs(y))/40)
 for cx,cy,r in reserves:weight*=smooth((np.hypot(x-cx,y-cy)-r-8)/16)
 return weight

distance,water,radius,ford=valley_field(coords[:,0],coords[:,1])
valley=(1-smooth((distance-radius-1)/17))*protect_weight(coords[:,0],coords[:,1])
bed=water-np.where(ford,-.10,.60)
coords[:,2]=coords[:,2]*(1-valley)+bed*valley
# Re-cut the EXISTING excluded geological fault into narrow connected clefts.
# This modifies the new continental ground only; no original face or UV moves.
faults=[[(f['ax'],f['az']),(f['bx'],f['bz'])] for f in layout['faults']]
def fault_field(x,y):
 d=np.full(np.shape(x),1e9)
 for (ax,ay),(bx,by) in faults:
  t=np.clip(((x-ax)*(bx-ax)+(y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
  d=np.minimum(d,np.hypot(x-ax-t*(bx-ax),y-ay-t*(by-ay)))
 return d
fd=fault_field(coords[:,0],coords[:,1]);rad=np.hypot(coords[:,0]+260,coords[:,1]-150)
cut=(1-smooth(fd/8))*(1.2+2.4*np.exp(-(rad/45)**2))*protect_weight(coords[:,0],coords[:,1])
coords[:,2]-=cut
assert np.array_equal(coords[protected],protected_before)
terrain.data.vertices.foreach_set('co',coords.ravel());terrain.data.update()
bpy.context.view_layer.update()
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def soil(x,y):
 hit=bvh.ray_cast(Vector((float(x),float(y),300)),Vector((0,0,-1)))[0]
 assert hit is not None,(x,y)
 return hit.z
def mesh_object(name,vertices,faces,materials,slots=None):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
 for material in materials:mesh.materials.append(material)
 if slots:
  for p,slot in zip(mesh.polygons,slots):p.material_index=slot
 uv=mesh.uv_layers.new(name='World metre original donor pigment')
 for loop in mesh.loops:
  q=mesh.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(q.x/12,q.y/12)
 ob=bpy.data.objects.get(name)
 if ob:ob.data=mesh
 else:ob=bpy.data.objects.new(name,mesh);scene.collection.objects.link(ob);exported.append(ob)
 return ob
water_material=bpy.data.objects['Pilot_River'].data.materials[0]
for index,stream in enumerate(streams):
 vv=[];ff=[];points=stream['points']
 for k,p in enumerate(points):
  delta=points[min(k+1,len(points)-1)]-points[max(0,k-1)];normal=np.array([-delta[1],delta[0]])/np.linalg.norm(delta)
  r=stream['radius'][k]
  for side in [-1,1]:vv.append((*list(p+normal*r*side),float(stream['water'][k])))
  if k and not(stream['ford'][k] or stream['ford'][k-1]):ff.append((2*k-2,2*k-1,2*k+1,2*k))
 mesh_object('BLD_AuthoredTributary_'+str(index),vv,ff,[water_material])

# Replace the observed broad pile with rock lips of the actual connected cleft.
plates=bpy.data.objects['BLD_BreachBrokenBasaltPlates']
stone_materials=list(plates.data.materials)
vv=[];ff=[];slots=[]
rng=np.random.default_rng(251026)
for branch,path in enumerate(faults):
 a,b=map(np.array,path);delta=b-a;length=float(np.linalg.norm(delta));normal=np.array([-delta[1],delta[0]])/length;tangent=delta/length
 station=3.0
 while station<length-2:
  p=a+tangent*station
  for side in [-1,1]:
   c=p+normal*side*(5.7+rng.uniform(-.8,.8));long=rng.uniform(3.3,6.3);wide=rng.uniform(2.2,4.2)
   ring=[c-tangent*long-normal*wide*.35,c+tangent*long*.7-normal*wide*.45,c+tangent*long+normal*wide*.35,c-tangent*long*.6+normal*wide*.65]
   base=len(vv);lift=float(rng.uniform(.15,.65))
   for q in ring:vv.append((q[0],q[1],soil(*q)+lift))
   for q in ring:vv.append((q[0],q[1],soil(*q)-.18))
   ff.extend([(base,base+1,base+2),(base,base+2,base+3)]);slots.extend([int(rng.integers(0,4))]*2)
   for j in range(4):ff.append((base+j,base+(j+1)%4,base+4+(j+1)%4,base+4+j));slots.append(4)
  station+=float(rng.uniform(8,12))
mesh_object(plates.name,vv,ff,stone_materials,slots)
# Existing overlay surfaces follow the newly incised ground, rather than spanning it.
for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures']:
 ob=bpy.data.objects[name]
 for v in ob.data.vertices:v.co.z=soil(v.co.x,v.co.y)+.018
 ob.data.update()
heart=bpy.data.objects['BLD_BreachMineralHeart'];heart_materials=list(heart.data.materials)
vv=[];ff=[];slots=[]
for branch,path in enumerate(faults):
 points=curve(path,2.5)
 for k,p in enumerate(points):
  delta=points[min(k+1,len(points)-1)]-points[max(0,k-1)];normal=np.array([-delta[1],delta[0]])/np.linalg.norm(delta)
  width=(.30+.50*math.exp(-np.linalg.norm(p-np.array([-260,150]))/34))*(.8+.2*math.sin(k*1.8))
  for side in [-1,1]:
   q=p+normal*side*width;vv.append((q[0],q[1],soil(*q)+.035))
  if k:ff.append((len(vv)-4,len(vv)-3,len(vv)-1,len(vv)-2));slots.append(min(6,len(heart_materials)-1))
mesh_object(heart.name,vv,ff,heart_materials,slots)

# The same original tree donors form believable riparian clearings. Keep all
# protected near crowns; clear only NEW continental trees from the stream bed.
cleared=[];regrounded=0
trees=[ob for ob in scene.objects if ob.name.startswith('BLD_ForestMass_')]
tree_x=np.array([ob.location.x for ob in trees]);tree_y=np.array([ob.location.y for ob in trees])
tree_d,tree_w,tree_r,tree_f=valley_field(tree_x,tree_y)
for ti,ob in enumerate(trees):
 x,y=ob.location.x,ob.location.y
 if tree_d[ti]<tree_r[ti]+7:
  far=bpy.data.objects.get(ob.name.replace('BLD_ForestMass_','BLD_ForestFar_'))
  for q in [ob,far]:
   if q:
    if q in exported:exported.remove(q)
    cleared.append(q.name);bpy.data.objects.remove(q,do_unlink=True)
 elif tree_d[ti]<tree_r[ti]+24:
  z=soil(x,y);delta=z-ob.location.z;ob.location.z=z
  far=bpy.data.objects.get(ob.name.replace('BLD_ForestMass_','BLD_ForestFar_'))
  if far:far.location.z+=delta
  regrounded+=1

# Paint source-derived meadow masses using the actual new drainage and relief.
# The original packed images remain immutable. Only the new atlas material links change.
continental=[]
for material in terrain.data.materials:
 if material and 'Region1 open meadows' in material.name:
  continental.extend(n.image for n in material.node_tree.nodes if n.type=='TEX_IMAGE' and n.image)
assert continental,'Continental source pigment material missing'
pigment=continental[0];W,H=pigment.size
pixels=np.empty(len(pigment.pixels),np.float32);pigment.pixels.foreach_get(pixels);pixels=pixels.reshape(H,W,4)
X,Y=np.meshgrid(np.linspace(-910,910,W),np.linspace(-1800,1800,H))
d,w,r,f=valley_field(X,Y)
weight=protect_weight(X,Y)
roads=sample(masks[:,:,2].astype(float)/255,X,Y)
woods=sample(masks[:,:,3].astype(float)/255,X,Y)
h=sample(oldheight,X,Y)
# A connected olive valley, dry golden shoulders and soft woodland margins.
valley_wash=(1-smooth((d-r-12)/105))*weight*(1-roads)**2
dry=smooth((h-8)/18)*weight*(1-roads)**2*(1-woods*.8)
green=np.array((.39,.46,.185));gold=np.array((.57,.51,.235))
base=pixels[:,:,:3]
base[:]=base*(1-valley_wash[:,:,None]*.42)+green*valley_wash[:,:,None]*.42
base[:]=base*(1-dry[:,:,None]*.23)+gold*dry[:,:,None]*.23
shore=(1-smooth((d-r-1)/12))*smooth((d-r+1)/4)*weight*(1-roads)
base[:]=base*(1-shore[:,:,None]*.58)+np.array((.43,.39,.255))*shore[:,:,None]*.58
new=bpy.data.images.new('Artist authored connected valley and meadow composition',W,H,alpha=True)
new.pixels.foreach_set(pixels.ravel());new.filepath_raw=str(out/'WorldMacroPigment.png');new.file_format='PNG';new.save()
packed=bpy.data.images.load(new.filepath_raw,check_existing=False);packed.pack()
links=0
for material in bpy.data.materials:
 if material.use_nodes:
  for node in material.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image==pigment:node.image=packed;links+=1
assert links>0

# Rebuild only changed query samples from the ACTUAL edited triangles, including
# the exact mesh vertex tail used by Unity's imported-geometry validation.
QX,QY=np.meshgrid(np.linspace(-910,910,nx),np.linspace(-1800,1800,nz))
d,w,r,f=valley_field(QX,QY);qw=protect_weight(QX,QY)
changed=((d<r+19)|(fault_field(QX,QY)<9))&(qw>0)
heights=oldheight.copy()
for j,i in np.argwhere(changed):heights[j,i]=soil(QX[j,i],QY[j,i])
wet=(d<r)&(~f)&(qw>.99)
masks[:,:,1]=np.maximum(masks[:,:,1],wet.astype(np.uint8)*255)
masks[:,:,3][d<r+10]=0
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as file:
 file.write(struct.pack('<iiiff',magic,nx,nz,width,depth));file.write(heights.astype('<f4').tobytes());file.write(masks.tobytes());file.write(struct.pack('<ii',0x32564c45,len(coords)))
 file.write(coords[:,[0,2,1]].astype('<f4').tobytes())
layout['artistic_drainage']={'streams':2,'existing_road_gravel_fords':True,'city_reserves_preserved':True}
layout['continental_trees']=sum(o.name.startswith('BLD_ForestMass_') for o in scene.objects)
(out/'production-region.json').write_text(json.dumps(layout,indent=2))
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==sha for name,sha in images_before.items())
assert np.array_equal(coords[protected],protected_before)
bpy.ops.object.select_all(action='DESELECT')
for ob in exported:ob.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'method':'artist-directed actual BlenderMCP edit','source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'protected_vertices_unchanged':int(protected.sum()),'original_packed_images_unchanged':len(images_before),'new_connected_streams':2,'new_gameplay_targets':0,'new_colliders':0,'cleared_continental_tree_lods':len(cleared),'regrounded_trees':regrounded,'continental_material_links':links,'query_samples_rebuilt':int(changed.sum()),'art_approved':False,'files':{name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']}}
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
hidden={ob:ob.hide_render for ob in scene.objects}
for ob in scene.objects:
 if ob.name.startswith(('BLD_ForestFar_','Far_Pilot_Pine')):ob.hide_render=True
bpy.app.driver_namespace['interactive_snapshot']('macro-candidate')
for ob,state in hidden.items():ob.hide_render=state
(out/'stop.request').write_text('Coherent macro artwork exported and reviewed-source previews saved')
print('MACRO_WORLD_ART_SAVED',json.dumps(report))
