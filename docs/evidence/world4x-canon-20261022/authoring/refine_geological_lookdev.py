"""Reviewed look-development on the exact preserved Blender candidate.
No terrain positions, routes, protected AF/AG images or gameplay models change.
"""
import bpy,sys,pathlib,json,hashlib,math,struct,zlib,gzip
import numpy as np
from mathutils import Vector
source,out=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:])
input_sha=hashlib.sha256(source.read_bytes()).hexdigest()
bpy.ops.wm.open_mainfile(filepath=str(source))
bpy.context.preferences.filepaths.save_version=0
terrain=bpy.data.objects['BLD_Terrain']
def terrain_digest():
 v=np.array([tuple(p.co) for p in terrain.data.vertices],dtype=np.float32)
 return hashlib.sha256(v.tobytes()).hexdigest()
terrain_before=terrain_digest()
# Derive two spatial bands from the SAME reviewed original AF clear-grass patch.
af=bpy.data.images['AF broad meadow flows']
values=np.empty(len(af.pixels),np.float32);af.pixels.foreach_get(values)
pixels=values.reshape(af.size[1],af.size[0],4)
patch=pixels[int(af.size[1]*.90):int(af.size[1]*.98),int(af.size[0]*.02):int(af.size[0]*.075),:3].copy()
linear=np.where(patch<=.04045,patch/12.92,((patch+.055)/1.055)**2.4)
mean=linear.mean(axis=(0,1))
fy=np.fft.fftfreq(patch.shape[0])[:,None];fx=np.fft.fftfreq(patch.shape[1])[None,:]
gaussian=np.exp(-2*math.pi**2*24.0**2*(fx*fx+fy*fy))
low=np.stack([np.fft.ifft2(np.fft.fft2(linear[:,:,c])*gaussian).real for c in range(3)],axis=2)
meso=((linear-low)/np.maximum(mean,.001)).mean(axis=2)
# Remove separable illumination drift that formed the rejected mirrored weave.
meso=meso-meso.mean(axis=0)[None,:]-meso.mean(axis=1)[:,None]+meso.mean()
meso*=.18/max(.18,float(np.abs(meso).max()))
alpha=.5+meso*.5
alpha=np.concatenate([alpha,alpha[:,::-1]],axis=1)
alpha=np.concatenate([alpha,alpha[::-1]],axis=0)
# Preserve the ACTUAL native PNG RGB bytes, rather than round-tripping
# through Blender colour-managed image buffers when authoring a data channel.
def read_rgba_png(path):
 raw=path.read_bytes();assert raw[:8]==bytes((137,80,78,71,13,10,26,10))
 offset=8;compressed=bytearray()
 while offset<len(raw):
  count=struct.unpack('>I',raw[offset:offset+4])[0];kind=raw[offset+4:offset+8];payload=raw[offset+8:offset+8+count];offset+=12+count
  if kind==b'IHDR':
   w,h,depth,colour,compression,filtering,interlace=struct.unpack('>IIBBBBB',payload)
   assert depth==8 and colour in (2,6) and interlace==0
  if kind==b'IDAT':compressed.extend(payload)
 bpp=4 if colour==6 else 3;stride=w*bpp
 decoded=zlib.decompress(bytes(compressed));result=[];previous=bytearray(stride);cursor=0
 for row in range(h):
  mode=decoded[cursor];cursor+=1;scan=bytearray(decoded[cursor:cursor+stride]);cursor+=stride
  for k in range(stride):
   left=scan[k-bpp] if k>=bpp else 0;up=previous[k];corner=previous[k-bpp] if k>=bpp else 0
   predictor=0
   if mode==1:predictor=left
   elif mode==2:predictor=up
   elif mode==3:predictor=(left+up)//2
   elif mode==4:
    p=left+up-corner;aa=abs(p-left);bb=abs(p-up);cc=abs(p-corner)
    predictor=left if aa<=bb and aa<=cc else up if bb<=cc else corner
   else:assert mode==0
   scan[k]=(scan[k]+predictor)&255
  result.append(bytes(scan));previous=scan
 values=np.frombuffer(b''.join(result),dtype=np.uint8).reshape(h,w,bpp)
 if bpp==3:values=np.concatenate([values,np.full((h,w,1),255,np.uint8)],axis=2)
 return values.copy()
def write_rgba_png(path,values):
 h,w,_=values.shape
 def chunk(kind,payload):
  return struct.pack('>I',len(payload))+kind+payload+struct.pack('>I',zlib.crc32(kind+payload)&0xffffffff)
 scan=b''.join(bytes(1)+row.tobytes() for row in values)
 path.write_bytes(bytes((137,80,78,71,13,10,26,10))+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(scan,9))+chunk(b'IEND',b''))
detail_path=out/'RegionOneGrassDetail.png'
rgba=read_rgba_png(detail_path);original_rgb=rgba[:,:,:3].copy()
assert rgba.shape[:2]==alpha.shape
rgba[:,:,3]=np.rint(np.clip(alpha[::-1],0,1)*255).astype(np.uint8)
write_rgba_png(detail_path,rgba)
verified=read_rgba_png(detail_path)
assert np.array_equal(verified[:,:,:3],original_rgb),'Native PNG RGB bytes changed during alpha authoring'
fresh=bpy.data.images.load(str(detail_path),check_existing=False)
fresh.name='AF reviewed micro RGB and regional meso alpha';fresh.colorspace_settings.name='Non-Color';fresh.alpha_mode='CHANNEL_PACKED';fresh.use_fake_user=True;fresh.pack()
assert hashlib.sha256(fresh.packed_file.data).hexdigest()==hashlib.sha256(detail_path.read_bytes()).hexdigest()
# Replace the rejected three flat-colour mineral plates with a continuous
# exposure following the same actual ground triangles and existing fault.
heart=bpy.data.objects['BLD_BreachMineralHeart']
width=256;X,Y=np.meshgrid(np.linspace(-315,-205,width),np.linspace(95,205,width))
ns={}
recipe=pathlib.Path(__file__).with_name('interactive')/'macro_fracture_recipe.py'
exec(compile(recipe.read_text(encoding='utf-8'),str(recipe),'exec'),ns)
distance=ns['stroke_distance'](X,Y,ns['AUTHOR_FAULTS'])
dx=X+260;dy=Y-150;r=np.hypot(dx,dy);angle=np.arctan2(dy,dx)
radius=29+9*np.sin(angle*5+.7)+5*np.cos(angle*3-.2)
def smooth(a):
 a=np.clip(a,0,1);return a*a*(3-2*a)
edge=r/radius
opacity=1-smooth((edge-.72)/.35)
exposure=1-smooth((edge-.20)/.65)
pigment=bpy.data.images['Region1 authored geological pigment']
data=np.empty(len(pigment.pixels),np.float32);pigment.pixels.foreach_get(data)
data=data.reshape(pigment.size[1],pigment.size[0],4)
# Strengthen the already-authored continental washes using the ACTUAL
# exported ground height, never noise or a new terrain surface.
query=gzip.decompress((out/'RegionOneBlenderQueries.bytes').read_bytes())
magic,nx,nz,world_width,world_height=struct.unpack_from('<iiiff',query)
assert magic==0x31424c45
count=nx*nz
heights=np.frombuffer(query,dtype='<f4',count=count,offset=20).reshape(nz,nx)
masks=np.frombuffer(query,dtype=np.uint8,count=count*4,offset=20+count*4).reshape(nz,nx,4)
W,H=pigment.size
xc=np.linspace(-910,910,W,dtype=np.float32);yc=np.linspace(-1800,1800,H,dtype=np.float32)
xx=np.linspace(0,nx-1,W,dtype=np.float32);yy=np.linspace(0,nz-1,H,dtype=np.float32)
x0=xx.astype(int);x1=np.minimum(x0+1,nx-1);wx=xx-x0
y0=yy.astype(int);y1=np.minimum(y0+1,nz-1);wy=(yy-y0)[:,None]
def sampled(field):
 bottom=field[y0[:,None],x0[None,:]]*(1-wx)[None,:]+field[y0[:,None],x1[None,:]]*wx[None,:]
 top=field[y1[:,None],x0[None,:]]*(1-wx)[None,:]+field[y1[:,None],x1[None,:]]*wx[None,:]
 return bottom*(1-wy)+top*wy
height=sampled(heights)
land=np.clip((sampled(masks[:,:,0].astype(np.float32))/255-.50)/.25,0,1)
water=sampled(masks[:,:,1].astype(np.float32))/255
road=sampled(masks[:,:,2].astype(np.float32))/255
core_distance=np.hypot(np.maximum(0,np.abs(xc)-96)[None,:],np.maximum(0,np.maximum(-78-yc,yc-270))[:,None])
edge_distance=np.minimum((910-np.abs(xc))[None,:],(1800-np.abs(yc))[:,None])
# The neighbouring context retains its original pigment. Meet it with zero
# correction and zero derivative, rather than exposing the exported UV rectangle.
peripheral_blend=smooth(edge_distance/240)
weight=smooth(core_distance/260)*land*(1-water)*(1-road)**2*peripheral_blend
# Preserve the existing geological palette and original protected core.
cx=np.where((xc>=-720)&(xc<=-20))[0];cy=np.where((yc>=-100)&(yc<=680))[0]
GX,GY=np.meshgrid(xc[cx],yc[cy])
fault=ns['stroke_distance'](GX,GY,ns['AUTHOR_FAULTS'])
weight[np.ix_(cy,cx)]*=smooth((fault-55)/55)
palette_mean=patch.mean(axis=(0,1))
graded=palette_mean+(data[:,:,:3]-palette_mean)*1.35
ridge=smooth((height-5)/12)
graded[:,:,0]*=1+.12*ridge;graded[:,:,1]*=1+.04*ridge;graded[:,:,2]*=1-.06*ridge
data[:,:,:3]=data[:,:,:3]*(1-weight[:,:,None])+np.clip(graded,.025,.95)*weight[:,:,None]
pigment.pixels.foreach_set(data.ravel())
pigment.filepath_raw=str(out/'Region1ContinentalReviewedWashes.png');pigment.file_format='PNG';pigment.save()
country=bpy.data.images.load(str(out/'Region1ContinentalReviewedWashes.png'),check_existing=False);country.name='Region1 reviewed original-flow continental pigment';country.pack()
assert hashlib.sha256(country.packed_file.data).hexdigest()==hashlib.sha256((out/'Region1ContinentalReviewedWashes.png').read_bytes()).hexdigest()
links=0
for material in bpy.data.materials:
 if material.use_nodes:
  for item in material.node_tree.nodes:
   if item.type=='TEX_IMAGE' and item.image==pigment:item.image=country;links+=1
assert links>0,'Continental look-development was not linked to actual scene materials'
(out/'continental-washes-review.json').write_text(json.dumps({'source':'Original AF patch and existing authored flows; exact exported height data','protected_core_pixels_changed':False,'terrain_positions_changed':False,'new_gameplay_objects':0,'material_links':links,'contrast':1.35,'art_approved':False},indent=2))
ix=np.clip(((X+910)/1820*(pigment.size[0]-1)).astype(int),0,pigment.size[0]-1)
iy=np.clip(((Y+1800)/3600*(pigment.size[1]-1)).astype(int),0,pigment.size[1]-1)
ground=data[iy,ix,:3]
vein=np.clip((6-distance)/5,0,1)*np.exp(-(r/22)**2)
grain=.007*np.sin(X*4.7+Y*1.7)+.005*np.cos(Y*3.9-X*2.3)
rock=np.empty((*X.shape,3),np.float32)
rock[:]=np.array((.105,.055,.135));rock+=grain[:,:,None]
rock=rock*(1-vein[:,:,None]*.72)+np.array((.55,.035,.70))*vein[:,:,None]*.72
colour=ground*(1-exposure[:,:,None]*.88)+rock*exposure[:,:,None]*.88
paint=np.ones((width,width,4),np.float32);paint[:,:,:3]=colour;paint[:,:,3]=opacity
image=bpy.data.images.new('Continuous exposed central mineral',width,width,alpha=True)
image.pixels.foreach_set(paint.ravel());image.filepath_raw=str(out/'BreachCentralMineral.png');image.file_format='PNG';image.save()
packed=bpy.data.images.load(str(out/'BreachCentralMineral.png'),check_existing=False);packed.pack()
assert hashlib.sha256(packed.packed_file.data).hexdigest()==hashlib.sha256((out/'BreachCentralMineral.png').read_bytes()).hexdigest()
# Weld only duplicate vertices of this NEW exposure mesh, never original ground.
vertices=[];faces=[];lookup={}
for poly in heart.data.polygons:
 ids=[]
 for index in poly.vertices:
  q=tuple(heart.data.vertices[index].co)
  if q not in lookup:lookup[q]=len(vertices);vertices.append(q)
  ids.append(lookup[q])
 faces.append(tuple(ids))
mesh=bpy.data.meshes.new('Continuous mineral exposure on unchanged ground')
mesh.from_pydata(vertices,[],faces);mesh.update()
for face in mesh.polygons:face.use_smooth=True
uv=mesh.uv_layers.new(name='Physical mineral exposure')
for loop in mesh.loops:
 q=mesh.vertices[loop.vertex_index].co
 uv.data[loop.index].uv=((q.x+315)/110,(q.y-95)/110)
mat=bpy.data.materials.new('Breach naturally graded exposed mineral');mat.use_nodes=True
if hasattr(mat,'surface_render_method'):mat.surface_render_method='DITHERED'
bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
bs.inputs['Base Color'].default_value=(1,1,1,1);bs.inputs['Roughness'].default_value=.9
node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=packed
mat.node_tree.links.new(node.outputs['Color'],bs.inputs['Base Color'])
mat.node_tree.links.new(node.outputs['Alpha'],bs.inputs['Alpha'])
mesh.materials.append(mat);heart.data=mesh
assert terrain_digest()==terrain_before,'Protected geography changed during look development'
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
report={'input_blend_sha256':input_sha,'terrain_before_after_sha256':terrain_before,'terrain_positions_changed':False,'protected_AF_AG_images_edited':False,'near_detail_RGB_replaced':False,'regional_meso_band_in_existing_alpha':True,'heart_flat_plate_materials_replaced':True,'actual_ground_faces':len(faces),'art_approved':False}
(out/'reviewed-lookdev-operations.json').write_text(json.dumps(report,indent=2))
print('REVIEWED_LOOKDEV_EXACT_GROUND_PASS',json.dumps(report))
