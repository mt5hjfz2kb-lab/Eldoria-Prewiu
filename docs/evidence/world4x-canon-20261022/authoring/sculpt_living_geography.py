"""Artist-composed landforms, river terraces and stratified geological wound.
Real BlenderMCP edit of the retained world. No gameplay targets/colliders.
"""
import bpy,bmesh,numpy as np,math,json,gzip,struct,pathlib,hashlib
from mathutils import Vector,noise
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
terrain=bpy.data.objects['BLD_Terrain'];scene=bpy.context.scene
raw_glb=(source.parent/'RegionOneBlenderWorld.glb').read_bytes();export_names={n.get('name') for n in json.loads(raw_glb[20:20+struct.unpack_from('<I',raw_glb,12)[0]])['nodes']}
raw=gzip.decompress((source.parent/'RegionOneBlenderQueries.bytes').read_bytes());magic,nx,nz,width,depth=struct.unpack_from('<iiiff',raw);assert magic==0x31424c45 and (width,depth)==(1820,3600)
count=nx*nz;old_height=np.frombuffer(raw,dtype='<f4',count=count,offset=20).reshape(nz,nx).copy();masks=np.frombuffer(raw,dtype=np.uint8,count=count*4,offset=20+count*4).reshape(nz,nx,4).copy();old_masks=masks.copy()
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'));reserves=layout['city_reserves'];faults=layout['faults']
images_before={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
def smooth(v):
 v=np.clip(v,0,1);return v*v*(3-2*v)
def sample(a,x,y):
 h,w=a.shape[:2];u=np.clip((np.asarray(x)+910)/1820*(w-1),0,w-1);v=np.clip((np.asarray(y)+1800)/3600*(h-1),0,h-1);ix=u.astype(int);iy=v.astype(int);jx=np.minimum(w-1,ix+1);jy=np.minimum(h-1,iy+1);tx=u-ix;ty=v-iy
 return (a[iy,ix]*(1-tx)+a[iy,jx]*tx)*(1-ty)+(a[jy,ix]*(1-tx)+a[jy,jx]*tx)*ty
def protection(x,y,edge=180):
 d=np.hypot(np.maximum(0,np.abs(x)-96),np.maximum(0,np.maximum(-78-y,y-270)));w=smooth(d/85)*smooth(np.minimum(910-np.abs(x),1800-np.abs(y))/edge)
 for cx,cy,r in reserves:w*=smooth((np.hypot(x-cx,y-cy)-r-9)/38)
 return w
# Blender's coherent noise is sampled once, then interpolated at physical metres.
noise_fields=[]
for seed,scale in [(1.7,185),(6.3,62),(12.1,330)]:
 grid=np.array([[noise.noise(Vector((x/scale,y/scale,seed))) for x in np.linspace(-910,910,128)] for y in np.linspace(-1800,1800,256)],np.float32);noise_fields.append(grid)
def field(i,x,y):return sample(noise_fields[i],x,y)
def closest_lines(x,y,segments):
 x,y=np.broadcast_arrays(np.asarray(x),np.asarray(y));best=np.full(x.shape,1e10);tangent_x=np.zeros(x.shape);tangent_y=np.ones(x.shape);px=np.zeros(x.shape);py=np.zeros(x.shape);value=np.zeros(x.shape);radius=np.zeros(x.shape)
 for ax,ay,bx,by,va,vb,ra,rb in segments:
  dx=bx-ax;dy=by-ay;den=dx*dx+dy*dy;t=np.clip(((x-ax)*dx+(y-ay)*dy)/den,0,1);qx=ax+t*dx;qy=ay+t*dy;d=np.hypot(x-qx,y-qy);replace=d<best
  best=np.minimum(best,d);tangent_x=np.where(replace,dx/math.sqrt(den),tangent_x);tangent_y=np.where(replace,dy/math.sqrt(den),tangent_y);px=np.where(replace,qx,px);py=np.where(replace,qy,py);value=np.where(replace,va*(1-t)+vb*t,value);radius=np.where(replace,ra*(1-t)+rb*t,radius)
 return best,tangent_x,tangent_y,px,py,value,radius
streams=[]
for name in ['BLD_AuthoredTributary_0','BLD_AuthoredTributary_1']:
 m=bpy.data.objects[name].data;p=[(m.vertices[i].co+m.vertices[i+1].co)*.5 for i in range(0,len(m.vertices),2)];r=[(m.vertices[i].co-m.vertices[i+1].co).length*.5 for i in range(0,len(m.vertices),2)]
 # Every fourth source station is sufficient for the broad terrace field.
 # Water itself and its detailed collider exclusion remain the original mesh.
 indexes=list(range(0,len(p)-1,4))+[len(p)-1]
 streams.extend((p[a].x,p[a].y,p[b].x,p[b].y,p[a].z,p[b].z,r[a],r[b]) for a,b in zip(indexes,indexes[1:]))
fault_segments=[(f['ax'],f['az'],f['bx'],f['bz'],0,0,0,0) for f in faults]
coords=np.empty(len(terrain.data.vertices)*3,np.float32);terrain.data.vertices.foreach_get('co',coords);coords=coords.reshape(-1,3)
protected=(np.abs(coords[:,0])<=96.001)&(coords[:,1]>=-78.001)&(coords[:,1]<=270.001);protected_before=coords[protected].copy();old_min=float(coords[:,2].min());old_max=float(coords[:,2].max())
# Original normals are immutable in the protected AF/AG kernel.
original_normals={}
for p in terrain.data.polygons:
 if all(protected[i] for i in p.vertices):
  signature=tuple(sorted(tuple(terrain.data.vertices[i].co) for i in p.vertices))
  for loop in p.loop_indices:original_normals[(tuple(terrain.data.vertices[terrain.data.loops[loop].vertex_index].co),signature)]=tuple(terrain.data.corner_normals[loop].vector)
bridge_vertices=set()
for p in terrain.data.polygons:
 if 'Bridge' in terrain.data.materials[p.material_index].name:bridge_vertices.update(p.vertices)
# Triangulate new land only. This removes ambiguous non-planar ngons before sculpting.
bm=bmesh.new();bm.from_mesh(terrain.data);bm.verts.ensure_lookup_table();bm.faces.ensure_lookup_table()
faces=[p for p in bm.faces if not any(protected[v.index] or v.index in bridge_vertices for v in p.verts)]
bmesh.ops.triangulate(bm,faces=faces,quad_method='BEAUTY',ngon_method='BEAUTY');bm.to_mesh(terrain.data);bm.free();terrain.data.update()
coords=np.array([tuple(v.co) for v in terrain.data.vertices],np.float32);x,y,h=coords.T;h=h.copy();weight=protection(x,y);bridge_mask=np.zeros(len(coords),bool)
# Re-identify bridge vertices after topology conversion by their unaltered materials.
for p in terrain.data.polygons:
 if 'Bridge' in terrain.data.materials[p.material_index].name:bridge_mask[list(p.vertices)]=True
weight[bridge_mask]=0
rd,rtx,rty,rpx,rpy,water,radius=closest_lines(x,y,streams)
# Four deliberate low upland spines frame broad river plains, rather than random bumps.
ridges=[([(-730,510),(-610,780),(-685,1040),(-500,1300)],33,145),([(530,740),(650,1010),(510,1290),(625,1550)],27,145),([(-590,-520),(-685,-800),(-475,-1120),(-560,-1460)],31,160),([(430,-490),(545,-790),(350,-1080),(490,-1400)],21,180)]
upland=np.zeros(len(coords));warpx=x+field(0,x,y)*27;warpy=y+field(2,x,y)*24
for path,amplitude,spread in ridges:
 seg=[(*a,*b,0,0,0,0) for a,b in zip(path,path[1:])];d,*_=closest_lines(warpx,warpy,seg);upland+=amplitude*np.exp(-(d/spread)**2)*(0.85+field(1,x,y)*.22)
upland*=smooth((rd-radius-45)/100)
rolling=(field(0,x,y)*3.8+field(1,x,y)*1.8)*smooth((rd-radius-22)/90)
# Fill inherited metre-wide road trenches before painting the relocated trail.
# Broad landforms, bridges and the protected original remain unaffected.
blur=old_height.copy()
for unused in range(2):
 padded=np.pad(blur,1,mode='edge');blur=(padded[:-2,:-2]+2*padded[:-2,1:-1]+padded[:-2,2:]+2*padded[1:-1,:-2]+4*padded[1:-1,1:-1]+2*padded[1:-1,2:]+padded[2:,:-2]+2*padded[2:,1:-1]+padded[2:,2:])/16
old_road=sample(old_masks[:,:,2].astype(float)/255,x,y)*weight*smooth((rd-radius-20)/25)
newh=h*(1-old_road*.88)+sample(blur,x,y)*old_road*.88+(upland+rolling)*weight
# Moist fluvial terraces sit above the actual water; no perched painted river.
terrace=water+.5+6*np.power(np.clip((rd-radius)/135,0,1),1.35)
valley=(1-smooth((rd-radius-24)/135))*smooth((rd-radius-2)/13)*weight
newh=newh*(1-valley)+terrace*valley
# Re-open the existing excluded wound with variable natural width and a true V profile.
fd,ftx,fty,fpx,fpy,_,_=closest_lines(x,y,fault_segments);nxv=-fty;nyv=ftx
banks=np.maximum.reduce([sample(old_height,x+nxv*r,y+nyv*r) for r in [-38,-25,25,38]])
rough=field(1,x*1.1,y*1.1);signed=(x-fpx)*nxv+(y-fpy)*nyv;distance=np.abs(signed-rough*2.1)
central=np.exp(-((x+260)**2+(y-150)**2)/(82**2));bottom=1.4+(rough+.6)*.6;top=9.5+field(0,x,y)*2.6
profile=banks-(12+6*central)*(1-smooth((distance-bottom)/(top-bottom)))
fracture=(1-smooth((fd-16)/12))*weight
newh=newh*(1-fracture)+profile*fracture
newh=np.clip(newh,old_min,min(old_max-.05,47.9));newh[weight==0]=h[weight==0];newh[np.argmin(h)]=old_min;newh[np.argmax(h)]=old_max
coords[:,2]=newh;terrain.data.vertices.foreach_set('co',coords.ravel());terrain.data.update();bpy.context.view_layer.update()
assert np.array_equal(coords[protected],protected_before)
terrain.data.normals_split_custom_set([(0.,0.,0.)]*len(terrain.data.loops));terrain.data.update()
normals=[tuple(n.vector) for n in terrain.data.corner_normals]
for p in terrain.data.polygons:
 if all(protected[i] for i in p.vertices):
  signature=tuple(sorted(tuple(terrain.data.vertices[i].co) for i in p.vertices))
  for loop in p.loop_indices:
   key=(tuple(terrain.data.vertices[terrain.data.loops[loop].vertex_index].co),signature)
   if key in original_normals:normals[loop]=original_normals[key]
 else:p.use_smooth=True
terrain.data.normals_split_custom_set(normals);terrain.data.update();bpy.context.view_layer.update()
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def soil(x,y):
 hit=bvh.ray_cast(Vector((float(x),float(y),400)),Vector((0,0,-1)))[0]
 if hit is None:
  # The retained main river deliberately has open water instead of soil.
  # Preserve its certified query; never invent a new surface over that hole.
  assert float(sample(old_masks[:,:,1],x,y))>64 or float(protection(x,y))==0,(x,y)
  return float(sample(old_height,x,y))
 return hit.z
# Carry the retained decorative stones, understory and trees with the ground.
# Geometry in the protected kernel remains byte-for-byte untouched.
old_bvh=BVHTree.FromPolygons([(float(a),float(b),float(c)) for a,b,c in zip(x,y,h)], [list(p.vertices) for p in terrain.data.polygons],all_triangles=False)
for ob in scene.objects:
 if ob.name not in export_names or ob.type!='MESH' or ob.name.startswith(('BLD_','Far_')):continue
 if ob.name.startswith(('Pilot_Pine','Bank forest stone','P ridge exposed stone')):
  a,b=ob.location.x,ob.location.y
  if float(protection(a,b))==0:continue
  hit=old_bvh.ray_cast(Vector((a,b,400)),Vector((0,0,-1)))[0]
  if hit:ob.location.z+=soil(a,b)-hit.z
  far=bpy.data.objects.get('Far_'+ob.name)
  if far and hit:far.location.z+=soil(a,b)-hit.z
# Warped trails preserve the exact bridge and settlement connections.
QX,QY=np.meshgrid(np.linspace(-910,910,nx),np.linspace(-1800,1800,nz));qw=protection(QX,QY);qrd,_,_,_,_,qwater,qr=closest_lines(QX,QY,streams)
warp=qw*smooth((qrd-qr-28)/55);dx=field(0,QX,QY)*15*warp;dy=field(1,QX,QY)*13*warp
masks[:,:,2]=(np.clip(sample(old_masks[:,:,2].astype(float)/255,QX+dx,QY+dy),0,1)*255).astype(np.uint8)
# Reuse existing forest instances: break rows and connect selected masses to valleys.
near_trees=[o for o in scene.objects if o.name in export_names and o.name.startswith('BLD_ForestMass_')]
occupied={};moved=0;transplanted=0;positions=[]
def identity(name):
 h=2166136261
 for c in name:h=((h^ord(c))*16777619)&0xffffffff
 return h
def clear(q):
 a,b=q
 if abs(a)>865 or abs(b)>1750:return False
 if any(math.hypot(a-cx,b-cy)<r+14 for cx,cy,r in reserves):return False
 if float(sample(qrd-qr,a,b))<12 or float(sample(old_masks[:,:,1],a,b))>64:return False
 if sample(masks[:,:,2].astype(float)/255,np.array([a]),np.array([b]))[0]>.25:return False
 d,*_=closest_lines(np.array([a]),np.array([b]),fault_segments)
 return d[0]>17
for near in sorted(near_trees,key=lambda o:o.name):
 old=np.array([near.location.x,near.location.y]);q=old.copy();w=float(protection(*old));seed=identity(near.name)
 if w>.85:
  q+=np.array([float(field(1,old[0]+75,old[1])),float(field(0,old[0],old[1]+120))])*14
  if seed%7==0:
   d,tx,ty,px,py,_,r=closest_lines(old[:1],old[1:],streams);side=1 if (old[0]-px[0])*(-ty[0])+(old[1]-py[0])*tx[0]>0 else -1
   candidate=np.array([px[0],py[0]])+side*np.array([-ty[0],tx[0]])*(r[0]+23+seed%62)+np.array([tx[0],ty[0]])*((seed//101)%35-17)
   if clear(candidate):q=candidate;transplanted+=1
  if not clear(q):q=old
 cell=tuple(np.floor(q/7).astype(int))
 if cell in occupied:q=old;cell=tuple(np.floor(q/7).astype(int))
 occupied[cell]=True
 oldz=near.location.z;near.location.x=float(q[0]);near.location.y=float(q[1]);near.location.z=soil(*q) if w>0 else oldz
 far=bpy.data.objects.get(near.name.replace('BLD_ForestMass_','BLD_ForestFar_',1))
 if far:far.location.x=near.location.x;far.location.y=near.location.y;far.location.z+=near.location.z-oldz
 moved+=float(np.linalg.norm(q-old))>.1;positions.append(tuple(q))
# Forest masks follow actual anchors; original kernel is unchanged.
forest=np.zeros((nz,nx),np.float32)
for a,b in positions:
 ix=int((a+910)/1820*(nx-1));iy=int((b+1800)/3600*(nz-1));x0=max(0,ix-6);x1=min(nx,ix+7);y0=max(0,iy-6);y1=min(nz,iy+7)
 patch=np.exp(-((QX[y0:y1,x0:x1]-a)**2+(QY[y0:y1,x0:x1]-b)**2)/(12**2));forest[y0:y1,x0:x1]=np.maximum(forest[y0:y1,x0:x1],patch)
masks[:,:,3]=(old_masks[:,:,3]*(1-qw)+forest*255*qw).astype(np.uint8)
# Query heights are rebuilt from the actual sculpted, explicitly triangulated mesh.
heights=old_height.copy()
for j,i in np.argwhere(qw>0):heights[j,i]=soil(QX[j,i],QY[j,i])
# Paint a continuous material landscape from relief, slope, drainage and woodland.
material=next(m for m in terrain.data.materials if m.name=='Region1 open meadows')
pigment=next(n.image for n in material.node_tree.nodes if n.type=='TEX_IMAGE' and n.image)
W,H=pigment.size;pixels=np.empty(len(pigment.pixels),np.float32);pigment.pixels.foreach_get(pixels);pixels=pixels.reshape(H,W,4);base=pixels[:,:,:3].copy()
X,Y=np.meshgrid(np.linspace(-910,910,W),np.linspace(-1800,1800,H));pw=protection(X,Y,220)
height=sample(heights,X,Y);gy,gx=np.gradient(heights,3600/(nz-1),1820/(nx-1));slope=sample(np.hypot(gx,gy),X,Y);wet=np.exp(-((sample(qrd-qr,X,Y))/105)**2);woods=sample(forest,X,Y)
coarse=field(0,X,Y);fine=field(1,X,Y);high=smooth((height-11)/24);stone=smooth((slope-.18)/.32)*smooth((height-9)/19)
colour=np.zeros_like(base)+np.array((.46,.405,.17));grass=np.clip(.25+coarse*.5+wet*.45,0,1);colour=colour*(1-grass[:,:,None]) +np.array((.325,.38,.14))*grass[:,:,None]
colour=colour*(1-high[:,:,None]*.35)+np.array((.49,.425,.235))*high[:,:,None]*.35
colour=colour*(1-wet[:,:,None]*.34)+np.array((.245,.335,.155))*wet[:,:,None]*.34
colour=colour*(1-woods[:,:,None]*.25)+np.array((.245,.285,.125))*woods[:,:,None]*.25
colour=colour*(1-stone[:,:,None]*.7)+np.array((.36,.345,.29))*stone[:,:,None]*.7
colour*=1+fine[:,:,None]*.055
road=sample(masks[:,:,2].astype(float)/255,X,Y);colour=colour*(1-road[:,:,None]*.88)+np.array((.49,.405,.245))*road[:,:,None]*.88
bfd,*_=closest_lines(X,Y,fault_segments);ash=(1-smooth((bfd-14)/25))*pw;colour=colour*(1-ash[:,:,None]*.82)+np.array((.26,.245,.225))*ash[:,:,None]*.82
pixels[:,:,:3]=base*(1-pw[:,:,None])+colour*pw[:,:,None]
image=bpy.data.images.new('Sculpted landform soil wet terrace woodland and rock',W,H,alpha=True);image.pixels.foreach_set(pixels.ravel());image.filepath_raw=str(out/'LivingGeographyPigment.png');image.file_format='PNG';image.save();packed=bpy.data.images.load(image.filepath_raw,check_existing=False);packed.pack()
for m in bpy.data.materials:
 if m.use_nodes:
  for node in m.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image==pigment:node.image=packed
# Apply unified metre UV only to NEW ground; protected faces and bridges retain original data.
uv=terrain.data.uv_layers.active
for p in terrain.data.polygons:
 if any(protected[i] for i in p.vertices) or 'Bridge' in terrain.data.materials[p.material_index].name:continue
 p.material_index=0
 for loop in p.loop_indices:
  q=terrain.data.vertices[terrain.data.loops[loop].vertex_index].co;uv.data[loop].uv=((q.x+910)/1820,(q.y+1800)/3600)
# Existing licensed stone material family becomes horizontal geological bands.
old_stone=list(bpy.data.objects['BLD_BreachBrokenBasaltPlates'].data.materials)[:4]
assert len(old_stone)==4
for i,m in enumerate(old_stone):
 if m not in list(terrain.data.materials):terrain.data.materials.append(m)
rock_slots=[list(terrain.data.materials).index(m) for m in old_stone]
face_centers=np.array([tuple(p.center) for p in terrain.data.polygons],np.float32)
face_distance,*_=closest_lines(face_centers[:,0],face_centers[:,1],fault_segments)
for index in np.flatnonzero(face_distance<25):
 p=terrain.data.polygons[int(index)];q=p.center
 if any(protected[i] for i in p.vertices):continue
 if p.normal.z<.95:p.material_index=rock_slots[int(math.floor((q.z+float(field(1,q.x,q.y))*.55)/2.2))%4];p.use_smooth=False
terrain.data.update();bpy.context.view_layer.update()
# Mineral overlays are copied from the very same ground triangles, not piled slabs.
old_masks_2d={}
for name in ['BLD_BreachFloor','BLD_BreachOutlyingFractures']:
 ob=bpy.data.objects[name];old_masks_2d[name]=BVHTree.FromPolygons([(v.co.x,v.co.y,0) for v in ob.data.vertices],[list(p.vertices) for p in ob.data.polygons],all_triangles=False)
def install_copy(name,chosen,offset=.015):
 vertices=[];faces=[];polys=[]
 for p in chosen:
  face=[]
  for i in p.vertices:
   q=terrain.data.vertices[i].co;face.append(len(vertices));vertices.append((q.x,q.y,soil(q.x,q.y)+offset))
  faces.append(face);polys.append(p)
 assert len(faces)>10,name
 mesh=bpy.data.meshes.new(name+' sculpted geological surface');mesh.from_pydata(vertices,[],faces);mesh.update()
 for m in terrain.data.materials:mesh.materials.append(m)
 layer=mesh.uv_layers.new(name='Actual geological source UV')
 for p,source_p in zip(mesh.polygons,polys):
  p.material_index=source_p.material_index;p.use_smooth=source_p.use_smooth
  for a,b in zip(p.loop_indices,source_p.loop_indices):layer.data[a].uv=terrain.data.uv_layers.active.data[b].uv
 ob=bpy.data.objects[name];ob.data=mesh
candidate_faces=[terrain.data.polygons[int(index)] for index in np.flatnonzero(face_distance<42)]
for name,mask in old_masks_2d.items():
 selected=[p for p in candidate_faces if mask.ray_cast(Vector((p.center.x,p.center.y,30)),Vector((0,0,-1)),60)[0] is not None];install_copy(name,selected)
classified=[[],[],[]]
for index in np.flatnonzero(face_distance<27):
 p=terrain.data.polygons[int(index)]
 q=p.center
 if not(-430<q.x<-110 and 15<q.y<290):continue
 d=float(face_distance[index])
 if d<9 and p.normal.z<.92:classified[0].append(p)
 elif 9<=d<17 and p.normal.z<.96:classified[1].append(p)
 elif 17<=d<27 and float(field(1,q.x,q.y))>-.2:classified[2].append(p)
for name,polys in zip(['BLD_BreachBrokenBasaltPlates','BLD_BreachTerracedStrata','BLD_BreachWeatheredRockfall'],classified):install_copy(name,polys,.026)
# Three small exposed vents replace the long painted violet strokes.
core=bpy.data.objects['BLD_BreachMineralHeart'];core_material=bpy.data.materials.new('Breach naturally graded exposed mineral rare vents');core_material.diffuse_color=(.18,.015,.32,1);core_material.use_nodes=True
nodes=core_material.node_tree.nodes;nodes.clear();bsdf=nodes.new('ShaderNodeBsdfPrincipled');output=nodes.new('ShaderNodeOutputMaterial');core_material.node_tree.links.new(bsdf.outputs['BSDF'],output.inputs['Surface'])
bsdf.inputs['Base Color'].default_value=(.18,.015,.32,1);bsdf.inputs['Emission Color'].default_value=(.32,.012,.68,1);bsdf.inputs['Emission Strength'].default_value=.8
vv=[];ff=[];vents=[(-262,151),(-278,166),(-238,142)]
for i,(a,b) in enumerate(vents):
 start=len(vv)
 for k in range(9):
  angle=math.tau*k/9;r=1.0+.28*math.sin(k*2.1+i);qx=a+math.cos(angle)*r*1.8;qy=b+math.sin(angle)*r;vv.append((qx,qy,soil(qx,qy)+.034))
 vv.append((a,b,soil(a,b)+.034))
 for k in range(9):ff.append((start+k,start+(k+1)%9,start+9))
 anchor=bpy.data.objects.get('BLD_BreachInteriorLightAnchor_'+str(i))
 if not anchor:anchor=bpy.data.objects.new('BLD_BreachInteriorLightAnchor_'+str(i),None);scene.collection.objects.link(anchor)
 anchor.location=(a,b,soil(a,b)+1.4);export_names.add(anchor.name)
 lamp_data=bpy.data.lights.new('Artist Breach interior violet '+str(i),'POINT');lamp_data.energy=650;lamp_data.color=(.48,.07,1);lamp_data.shadow_soft_size=2.2;lamp=bpy.data.objects.new('ArtistBreachInterior_'+str(i),lamp_data);scene.collection.objects.link(lamp);lamp.location=anchor.location
mesh=bpy.data.meshes.new('Rare mineral vents in actual canyon floor');mesh.from_pydata(vv,[],ff);mesh.materials.append(core_material);mesh.update();core.data=mesh
# All actual terrain query vertices are exported; state and coordinates stay authoritative.
coords=np.array([tuple(v.co) for v in terrain.data.vertices],np.float32);assert np.array_equal(coords[protected],protected_before)
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as file:
 file.write(struct.pack('<iiiff',magic,nx,nz,width,depth));file.write(heights.astype('<f4').tobytes());file.write(masks.tobytes());file.write(struct.pack('<ii',0x32564c45,len(coords)));file.write(coords[:,[0,2,1]].astype('<f4').tobytes())
layout['terrain_vertices']=len(coords);layout['living_geography']={'upland_spines':4,'true_fluvial_terraces':True,'warped_existing_trails':True,'forest_instances_transplanted':transplanted,'interior_breach_light_anchors':3,'protected_AF_AG_vertices_unchanged':int(protected.sum())};(out/'production-region.json').write_text(json.dumps(layout,indent=2))
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==value for name,value in images_before.items())
bpy.ops.object.select_all(action='DESELECT')
for ob in scene.objects:
 if ob.name in export_names and ob.type in {'MESH','EMPTY'}:ob.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True);bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'method':'actual BlenderMCP sculpted connected low uplands, fluvial terraces and stratified canyon','source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_vertices_after':len(coords),'protected_vertices_unchanged':int(protected.sum()),'original_packed_images_unchanged':len(images_before),'upland_spines':4,'forest_instances_jittered':moved,'forest_instances_transplanted_to_ecotones':transplanted,'canyon_wall_faces':[len(p) for p in classified],'interior_light_anchors':3,'height_bounds_preserved':[old_min,old_max],'new_gameplay_targets':0,'new_colliders':0,'files':{name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']},'art_approved':False};(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
bpy.app.driver_namespace['artist_keep_interior_lights']=True
def identity(name):
 h=2166136261
 for c in name:h=((h^ord(c))*16777619)&0xffffffff
 return h
def bounds(ob):
 q=ob.rotation_quaternion if ob.rotation_mode=='QUATERNION' else ob.rotation_euler.to_quaternion();matrix=Matrix.LocRotScale(ob.location,q,ob.scale)
 if ob.parent:matrix=ob.parent.matrix_world@ob.matrix_parent_inverse@matrix
 p=[matrix@Vector(v) for v in ob.bound_box];return tuple(max(v[i] for v in p)-min(v[i] for v in p) for i in range(3)),min(v.z for v in p)
bpy.context.view_layer.update()
previous={ob:(ob.hide_render,ob.location.copy(),ob.scale.copy()) for ob in bpy.context.scene.objects if ob.type=='MESH'}
for near in list(bpy.context.scene.objects):
 if not near.name.startswith(('BLD_ForestMass_','Pilot_Pine')):continue
 retained=near.name.startswith('Pilot_Pine');far=bpy.data.objects.get('Far_'+near.name if retained else near.name.replace('BLD_ForestMass_','BLD_ForestFar_',1))
 if not far:continue
 h=identity(near.name);show=h%5==0 if retained else True
 if not retained:
  far.scale.x*=2.1;far.scale.y*=2.1;far.scale.z*=2.6;size,base=bounds(far);foot=max(size[:2])
  if foot<12:show=(h%10000)/10000.<foot*foot/(22*22);crown=22;high=34
  else:crown=20+h%7;high=34+h%8
 else:crown=20+(h//5)%5;high=34+h%7
 if show:
  size,base=bounds(far);foot=max(size[:2]);far.scale.x*=crown/max(.01,foot);far.scale.y*=crown/max(.01,foot);far.scale.z*=high/max(.01,size[2]);far.location.z+=bounds(near)[1]-bounds(far)[1]
 near.hide_render=True;far.hide_render=not show
bpy.context.view_layer.update()
bpy.app.driver_namespace['artist_preview_use_far']=True
bpy.app.driver_namespace['interactive_snapshot']('living-geography-candidate')
bpy.app.driver_namespace['artist_preview_use_far']=False
for ob,(hide,location,scale) in previous.items():ob.hide_render=hide;ob.location=location;ob.scale=scale

(out/'stop.request').write_text('Living relief, forest ecotones, trails and stratified wound source saved; review before Unity')
