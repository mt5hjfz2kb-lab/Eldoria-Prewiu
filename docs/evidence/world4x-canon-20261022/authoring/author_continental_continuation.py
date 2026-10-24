"""Physical neighbouring land, executed by build_region_from_ai before joining AI.

The playable Region 1, query grid and camera contract stay unchanged. This is
editable Blender ground joined at the actual regional perimeter, not a backdrop
image, an opaque fog card, or an invented gameplay destination.
"""
context_extent=(1710.,2600.)
perimeter=[(x,-1800.) for x in xs]
perimeter += [(910.,y) for y in ys[1:]]
perimeter += [(x,1800.) for x in reversed(xs[:-1])]
perimeter += [(-910.,y) for y in reversed(ys[1:-1])]
assert len(set(perimeter))==len(perimeter)
# Preserve every original contact vertex, then reduce sampling outside Region 1.
# Zipper triangulation connects unequal loops without T-junctions or cracks.
coarse_x=sorted(set(np.arange(-910,911,80).tolist()+[-910.,910.]))
coarse_y=sorted(set(np.arange(-1800,1801,80).tolist()+list(range(-150,151,4))+[-1800.,1800.]))
coarse=[(x,-1800.) for x in coarse_x]+[(910.,y) for y in coarse_y[1:]]
coarse += [(x,1800.) for x in reversed(coarse_x[:-1])]+[(-910.,y) for y in reversed(coarse_y[1:-1])]
def perimeter_t(point):
 x,y=point
 if y==-1800:return x+910
 if x==910:return 1820+y+1800
 if y==1800:return 5420+910-x
 return 7240+1800-y
context_vertices=[];context_faces=[];count=len(perimeter);previous=None;previous_start=0
for ring,offset in enumerate(range(0,801,10)):
 loop=perimeter if ring==0 else list(coarse)
 if ring:
  # Track the actual coastal cross-section at every 10 m of neighbouring
  # north/south land. Wide angular cells elsewhere do not cut sawtooth shores.
  for sign in (-1,1):
   shore=float(coast(0,sign*(1800+offset)))
   for distance in (-25.,-5.,0.,5.,20.,40.):
    base_x=(shore+distance)*910/(910+offset)
    if -910<base_x<910:loop.append((base_x,sign*1800.))
  loop=sorted(set(loop),key=perimeter_t)
 start=len(context_vertices)
 for x,y in loop:
  px=x*(910+offset)/910;py=y*(1800+offset)/1800
  context_vertices.append((px,py,height(px,py)))
 if previous is not None:
  a=b=0;na=len(previous);nb=len(loop)
  ta=[perimeter_t(q) for q in previous]+[10840.]
  tb=[perimeter_t(q) for q in loop]+[10840.]
  while a<na or b<nb:
   next_a=ta[a+1] if a<na else float('inf')
   next_b=tb[b+1] if b<nb else float('inf')
   ia=previous_start+a%na;ib=start+b%nb
   if abs(next_a-next_b)<.0001:
    context_faces.append((ia,ib,start+(b+1)%nb,previous_start+(a+1)%na));a+=1;b+=1
   elif next_a<next_b:
    context_faces.append((ia,ib,previous_start+(a+1)%na));a+=1
   else:
    context_faces.append((ia,ib,start+(b+1)%nb));b+=1
 previous=loop;previous_start=start
context_mesh=bpy.data.meshes.new('Continuous neighbouring continental ground')
context_mesh.from_pydata(context_vertices,[],context_faces);context_mesh.update()
context_ground=bpy.data.objects.new('BLD_ContinentalContinuation',context_mesh)
scene.collection.objects.link(context_ground)
for face in context_mesh.polygons:face.use_smooth=True
context_uv=context_mesh.uv_layers.new(name='Continental continuation pigment')
for loop in context_mesh.loops:
 q=context_mesh.vertices[loop.vertex_index].co
 context_uv.data[loop.index].uv=((q.x+1710)/3420,(q.y+2600)/5200)
edge_error=max(abs(context_vertices[i][2]-height(x,y)) for i,(x,y) in enumerate(perimeter))
assert edge_error<.00001,('Continental continuation edge discontinuity',edge_error)
context_proof={'method':'actual Blender ground rings from every Region1 perimeter vertex',
 'shared_edge_vertices':count,'maximum_edge_height_difference_m':edge_error,
 'outer_extent_m':[-1710,-2600,1710,2600],
 'playable_region_unchanged_m':[-910,-1800,910,1800],
 'context_vertices':len(context_vertices),'context_triangles':sum(len(face)-2 for face in context_faces),
 'new_gameplay_targets':0,'art_approved':False}
print('REGION1_PHYSICAL_CONTINENTAL_CONTINUATION',json.dumps(context_proof))

# An independent editable pigment, with exact broad colour at the existing
# perimeter. The same source-derived linear grass detail is applied in Unity.
CS=1536;CX,CY=np.meshgrid(np.linspace(-1710,1710,CS),np.linspace(-2600,2600,CS))
outside=np.hypot(np.maximum(0,np.abs(CX)-910),np.maximum(0,np.abs(CY)-1800))
context_groves=[(-600,1990,180,220),(120,2020,220,170),(530,1990,150,180),
 (-450,-2000,230,180),(320,-1990,190,230),(-1110,450,180,260),
 (-1120,-620,200,220),(-1110,1430,170,240),(-1130,-1450,200,240)]
def context_forest(x,y):
 f=np.zeros(np.shape(x))
 for n,(cx,cy,rx,ry) in enumerate(context_groves):
  u=(x-cx+19*np.sin((y-cy)/43+n))/rx
  v=(y-cy+17*np.sin((x-cx)/37-n))/ry
  angle=np.arctan2(v,u)
  f=np.maximum(f,np.clip((1+.16*np.sin(angle*3+n)+.09*np.cos(angle*5)-u*u-v*v)*2,0,1))
 return f*smooth(np.hypot(np.maximum(0,np.abs(x)-910),np.maximum(0,np.abs(y)-1800))/65)
context_pixels=np.ones((CS,CS,4),np.float32)
flow=.004*np.sin(CX/93+CY/123)+.003*np.sin(CY/47-CX/61)+.002*np.sin(CX/19+CY/37)
context_pixels[:,:,:3]=meadow_mean+flow[:,:,None]
paint_canonical_macro_pigment(context_pixels,CX,CY,(-1710,-2600,1710,2600),meadow_mean)
context_litter=context_forest(CX,CY)*.69
context_pixels[:,:,:3]=context_pixels[:,:,:3]*(1-context_litter[:,:,None])+np.array([.25,.34,.19])*context_litter[:,:,None]
shore=smooth((26-coast(CX,CY))/24)*.45
context_pixels[:,:,:3]=context_pixels[:,:,:3]*(1-shore[:,:,None])+np.array([.59,.54,.37])*shore[:,:,None]
ix=np.clip((CX+910)/1820*(S-1),0,S-1);iy=np.clip((CY+1800)/3600*(S-1),0,S-1)
x0=ix.astype(int);y0=iy.astype(int);x1=np.minimum(x0+1,S-1);y1=np.minimum(y0+1,S-1)
tx=(ix-x0)[:,:,None];ty=(iy-y0)[:,:,None]
boundary_rgb=(rgba[y0,x0,:3]*(1-tx)+rgba[y0,x1,:3]*tx)*(1-ty)+(rgba[y1,x0,:3]*(1-tx)+rgba[y1,x1,:3]*tx)*ty
boundary_weight=(1-smooth(outside/110))[:,:,None]
context_pixels[:,:,:3]=boundary_rgb*boundary_weight+context_pixels[:,:,:3]*(1-boundary_weight)
context_image=bpy.data.images.new('Continuous neighbouring land pigment',CS,CS)
context_image.pixels.foreach_set(context_pixels.ravel());context_image.pack()
context_material=bpy.data.materials.new('Region1 open meadows continuity');context_material.use_nodes=True
context_bs=next(n for n in context_material.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
context_bs.inputs['Roughness'].default_value=.9
context_tex=context_material.node_tree.nodes.new('ShaderNodeTexImage');context_tex.image=context_image
context_material.node_tree.links.new(context_tex.outputs['Color'],context_bs.inputs['Base Color'])
context_mesh.materials.append(context_material)

# Root coordinates are computed now while the original height source is alive.
# They become instances of the same two editable continental fir meshes later.
context_tree_roots=[]
for index,(cx,cy,rx,ry) in enumerate(context_groves):
 random=np.random.default_rng(82114+index);placed=[]
 for attempt in range(1600):
  x=cx+random.uniform(-rx*1.2,rx*1.2);y=cy+random.uniform(-ry*1.2,ry*1.2)
  if max(abs(x)-910,abs(y)-1800)<55:continue
  if abs(x)>1630 or abs(y)>2520 or coast(x,y)<24:continue
  if float(context_forest(np.array(x),np.array(y)))<random.uniform(.15,.65):continue
  if abs(y-rivercentre(x))<riverwidth(x)+18:continue
  if any((x-a)**2+(y-b)**2<12**2 for a,b in placed):continue
  placed.append((x,y));context_tree_roots.append((x,y,height(x,y),index,len(placed)))
  if len(placed)>=90:break
context_proof['context_tree_instances']=len(context_tree_roots)
